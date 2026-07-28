using System.Net.Http.Json;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class DeApiVideoClient
{
    private readonly HttpClient _httpClient;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<DeApiVideoClient> _logger;

    public DeApiVideoClient(
        HttpClient httpClient,
        IOptions<VideoProviderSettings> config,
        ILogger<DeApiVideoClient> logger)
    {
        _httpClient = httpClient;
        _settings = config.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartAsync(string prompt, VideoGenerationOptions? options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.DeApiApiKey))
            return VideoGenerationResult.Fail("DeAPI API Key is missing.", "DeAPI");

        var baseUrl = _settings.DeApiBaseUrl ?? "https://api.deapi.ai/api/v1";
        bool isImg2Video = !string.IsNullOrWhiteSpace(options?.FirstFrameImageUrl);

        var endpoint = isImg2Video ? "/client/img2video" : "/client/txt2video";
        var url = baseUrl.EndsWith("/client/img2video") || baseUrl.EndsWith("/client/txt2video")
            ? baseUrl.Replace("/client/img2video", endpoint).Replace("/client/txt2video", endpoint)
            : $"{baseUrl.TrimEnd('/')}{endpoint}";

        var primaryModel = _settings.DeApiModel ?? "Ltx2_3_22B_Dist_INT8";
        var primaryKey = _settings.DeApiApiKey;
        var fallbackModel = _settings.DeApiModelFallback ?? "Ltxv_13B_0_9_8_Distilled_FP8";
        var fallbackKey = _settings.DeApiApiKeyFallback ?? primaryKey;

        var modelsToTry = primaryModel == fallbackModel 
            ? new[] { (Model: primaryModel, Key: primaryKey) } 
            : new[] { (Model: primaryModel, Key: primaryKey), (Model: fallbackModel, Key: fallbackKey) };

        // Map aspect ratio to width and height. DeAPI requires width >= 512.
        int w = 576, h = 1024;
        var ratio = options?.AspectRatio ?? "9:16";
        if (ratio == "16:9") { w = 1024; h = 576; }
        else if (ratio == "1:1") { w = 768; h = 768; }

        int fps = 24;
        // LTX-Video requires frame counts to follow (8n + 1). 
        int frames = 193; // Default to 8 seconds (24*8 = 192, 192/8 = 24, 24*8+1 = 193)
        if (options?.DurationSeconds > 0)
        {
            int requestedFrames = options.DurationSeconds * fps;
            int n = (int)Math.Round(requestedFrames / 8.0);
            frames = (n * 8) + 1;
        }

        try
        {
            byte[]? imageBytes = null;
            if (isImg2Video)
            {
                _logger.LogInformation("[DeAPI.Start] Mode: img2video. Downloading first_frame_image from {Url}", options!.FirstFrameImageUrl);
                imageBytes = await _httpClient.GetByteArrayAsync(options.FirstFrameImageUrl, cancellationToken);
            }

            HttpResponseMessage? response = null;
            string? errorBody = null;

            foreach (var m in modelsToTry)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {m.Key}");
                request.Headers.Add("Accept", "application/json");

                if (isImg2Video && imageBytes != null)
                {
                    var form = new MultipartFormDataContent();
                    var imageContent = new ByteArrayContent(imageBytes);
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    form.Add(imageContent, "first_frame_image", "frame.jpg");
                    
                    form.Add(new StringContent(frames.ToString()), "frames");
                    form.Add(new StringContent(w.ToString()), "width");
                    form.Add(new StringContent(h.ToString()), "height");
                    form.Add(new StringContent(fps.ToString()), "fps");
                    form.Add(new StringContent(Random.Shared.Next(1, int.MaxValue).ToString()), "seed");
                    form.Add(new StringContent(m.Model), "model");

                    if (!string.IsNullOrWhiteSpace(prompt))
                        form.Add(new StringContent(prompt), "prompt");

                    request.Content = form;
                }
                else
                {
                    var payload = new
                    {
                        prompt = prompt ?? "",
                        frames = frames,
                        width = w,
                        height = h,
                        fps = fps,
                        seed = Random.Shared.Next(1, int.MaxValue),
                        model = m.Model
                    };
                    request.Content = JsonContent.Create(payload);
                }

                _logger.LogInformation("[DeAPI.Start] Sending POST to {Url} with model {Model}", url, m.Model);
                
                response = await _httpClient.SendAsync(request, cancellationToken);
                _logger.LogInformation("[DeAPI.Start] HTTP Status: {Status} for model {Model}", (int)response.StatusCode, m.Model);

                if (response.IsSuccessStatusCode)
                {
                    break; // Success!
                }

                errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("[DeAPI.Start] Model {Model} failed: {Status} - {Body}", m.Model, (int)response.StatusCode, errorBody);
                
                // Keep trying the next model if it failed...
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                return VideoGenerationResult.Fail($"HTTP {(int?)response?.StatusCode}: {errorBody}", "DeAPI");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // === Format 1: {"data":{"request_id":"..."}} — async job (actual DeAPI txt2video response) ===
            if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object)
            {
                if (dataEl.TryGetProperty("request_id", out var requestIdEl))
                {
                    var requestId = requestIdEl.GetString();
                    if (!string.IsNullOrEmpty(requestId))
                    {
                        _logger.LogInformation("DeAPI video queued with request_id={RequestId}", requestId);
                        return VideoGenerationResult.Queued($"deapi:{requestId}", "DeAPI");
                    }
                }
                // data object might contain a direct URL
                if (dataEl.TryGetProperty("url", out var dataUrlEl))
                {
                    var videoUrl = dataUrlEl.GetString();
                    if (!string.IsNullOrEmpty(videoUrl))
                        return VideoGenerationResult.Done(videoUrl, "DeAPI");
                }
            }

            // === Format 2: {"data":[{"url":"..."}]} — synchronous array return ===
            if (root.TryGetProperty("data", out var dataArr2) && dataArr2.ValueKind == JsonValueKind.Array && dataArr2.GetArrayLength() > 0)
            {
                var first = dataArr2[0];
                if (first.TryGetProperty("url", out var urlEl))
                {
                    var videoUrl = urlEl.GetString();
                    if (!string.IsNullOrEmpty(videoUrl))
                        return VideoGenerationResult.Done(videoUrl, "DeAPI");
                }
            }

            // === Format 3: {"id":"..."} or {"task_id":"..."} — root-level async ===
            string? id = null;
            if (root.TryGetProperty("id", out var idEl)) id = idEl.GetString();
            else if (root.TryGetProperty("task_id", out var taskIdEl)) id = taskIdEl.GetString();
            
            if (!string.IsNullOrEmpty(id))
            {
                return VideoGenerationResult.Queued($"deapi:{id}", "DeAPI");
            }

            // === Format 4: {"url":"..."} — direct URL ===
            if (root.TryGetProperty("url", out var directUrlEl))
            {
                var directUrl = directUrlEl.GetString();
                if (!string.IsNullOrEmpty(directUrl))
                    return VideoGenerationResult.Done(directUrl, "DeAPI");
            }

            _logger.LogWarning("DeAPI unrecognized response: {Json}", json);
            return VideoGenerationResult.Fail($"Unrecognized response format from DeAPI. Response: {json}", "DeAPI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeAPI video start exception.");
            return VideoGenerationResult.Fail(ex.Message, "DeAPI");
        }
    }

    public async Task<VideoGenerationResult> PollAsync(string id, CancellationToken cancellationToken)
    {
        // DeAPI v2 polling endpoint: GET /api/v2/jobs/{request_id}
        var taskId = id.Replace("deapi:", "");
        
        // Dùng base URL gốc (api.deapi.ai) nhưng dùng endpoint v2
        var requestUrl = $"https://api.deapi.ai/api/v2/jobs/{taskId}";
        try
        {
            var keysToTry = new List<string> { _settings.DeApiApiKey };
        if (!string.IsNullOrWhiteSpace(_settings.DeApiApiKeyFallback) && _settings.DeApiApiKeyFallback != _settings.DeApiApiKey)
        {
            keysToTry.Add(_settings.DeApiApiKeyFallback);
        }

        HttpResponseMessage? response = null;
        string json = "";

        foreach (var key in keysToTry)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {key}");
            request.Headers.Add("Accept", "application/json");

            _logger.LogInformation("[DeAPI.Poll] Polling task: GET {Url} with key ending in {Key}", requestUrl, key.Length > 4 ? key[^4..] : "***");

            response = await _httpClient.SendAsync(request, cancellationToken);
            json = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("[DeAPI.Poll] HTTP Status: {Status}, Body: {Body}", (int)response.StatusCode, json.Length > 500 ? json[..500] + "..." : json);

            if (response.IsSuccessStatusCode)
            {
                break; // Found it!
            }
            
            // If 404 or unauthorized, it might be on the other account. Loop to the next key.
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            return VideoGenerationResult.Fail($"HTTP {(int?)response?.StatusCode}: {json}", "DeAPI");
        }

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            // Extract status from various possible locations
            string? status = null;
            if (root.TryGetProperty("status", out var statusElement))
                status = statusElement.GetString()?.ToLower();
            // v2 may nest status inside "data"
            if (status == null && root.TryGetProperty("data", out var dataObj) && dataObj.ValueKind == JsonValueKind.Object)
            {
                if (dataObj.TryGetProperty("status", out var dataStatusEl))
                    status = dataStatusEl.GetString()?.ToLower();
            }

            _logger.LogInformation("[DeAPI.Poll] Parsed status: {Status}", status ?? "(null)");

            if (status == "succeed" || status == "success" || status == "completed" || status == "done")
            {
                // Try extracting video URL from multiple possible locations
                var videoUrl = TryExtractVideoUrl(root);
                if (!string.IsNullOrEmpty(videoUrl))
                {
                    _logger.LogInformation("[DeAPI.Poll] ✅ Video ready: {Url}", videoUrl);
                    return VideoGenerationResult.Done(videoUrl, "DeAPI");
                }
                
                _logger.LogWarning("[DeAPI.Poll] Job completed but no video URL found in: {Json}", json);
                return VideoGenerationResult.Fail("DeAPI job completed but video URL is missing. JSON: " + json, "DeAPI");
            }
            else if (status == "failed" || status == "error" || status == "fail")
            {
                var errMsg = "DeAPI job failed.";
                if (root.TryGetProperty("error_message", out var errEl)) errMsg = errEl.GetString() ?? errMsg;
                else if (root.TryGetProperty("error", out var errEl2)) errMsg = errEl2.GetString() ?? errMsg;
                else if (root.TryGetProperty("message", out var msgEl)) errMsg = msgEl.GetString() ?? errMsg;
                else if (root.TryGetProperty("data", out var dataObjErr) && dataObjErr.ValueKind == JsonValueKind.Object)
                {
                    if (dataObjErr.TryGetProperty("error_message", out var dErr)) errMsg = dErr.GetString() ?? errMsg;
                    else if (dataObjErr.TryGetProperty("error", out var dErr2)) errMsg = dErr2.GetString() ?? errMsg;
                    else if (dataObjErr.TryGetProperty("message", out var dMsg)) errMsg = dMsg.GetString() ?? errMsg;
                }
                _logger.LogWarning("[DeAPI.Poll] ❌ Job failed: {Error}", errMsg);
                return VideoGenerationResult.Fail(errMsg, "DeAPI");
            }

            _logger.LogInformation("[DeAPI.Poll] Job still in progress. Status: {Status}", status ?? "unknown");
            return VideoGenerationResult.InProgress($"deapi:{taskId}", "DeAPI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeAPI video poll exception.");
            return VideoGenerationResult.Fail(ex.Message, "DeAPI");
        }
    }

    private static readonly string[] KnownUrlKeys =
    {
        "url", "video_url", "output_url", "result_url", "resultUrl", "videoUrl"
    };

    /// <summary>
    /// Extracts video URL from various possible JSON response structures
    /// </summary>
    internal static string? TryExtractVideoUrl(JsonElement root)
    {
        // 1. Recursive scan on known structures
        var extracted = RecursiveFindUrl(root);
        if (extracted != null)
            return extracted;

        // 2. Fallback: scan any property containing "url" with an http link
        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (property.Name.Contains("url", StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.String
                    && property.Value.GetString()?.StartsWith("http") == true)
                {
                    Console.WriteLine($"[DeAPI.Poll] Video URL found via fallback scan under unexpected key: {property.Name}");
                    return property.Value.GetString();
                }
            }
        }

        return null;
    }

    private static string? RecursiveFindUrl(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            // First check if this object has any known url keys
            foreach (var key in KnownUrlKeys)
            {
                if (element.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String)
                {
                    var val = prop.GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        return val;
                }
            }

            // Otherwise recurse into standard wrapper objects like data, result, output
            var wrappers = new[] { "data", "result", "output", "response" };
            foreach (var wrapper in wrappers)
            {
                if (element.TryGetProperty(wrapper, out var child))
                {
                    var childRes = RecursiveFindUrl(child);
                    if (childRes != null) return childRes;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0)
        {
            // Check first element if it's an array
            return RecursiveFindUrl(element[0]);
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            var val = element.GetString();
            if (val != null && val.StartsWith("http"))
                return val;
        }

        return null;
    }
}
