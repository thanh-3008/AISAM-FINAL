using System.Net.Http.Json;
using System.Net;
using System.Collections.Concurrent;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class DeApiVideoClient
{
    private static readonly ConcurrentDictionary<string, DateTimeOffset> PollBackoffUntil = new(StringComparer.OrdinalIgnoreCase);
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

        bool isImg2Video = !string.IsNullOrWhiteSpace(options?.FirstFrameImageUrl);
        var apiRoot = GetApiRoot(isImg2Video
            ? _settings.DeApiImg2VideoBaseUrl ?? _settings.DeApiBaseUrl
            : _settings.DeApiBaseUrl);

        // Default for backward-compatibility if not provided
        var primaryModel = _settings.DeApiModel ?? "Ltx2_3_22B_Dist_INT8";
        var fallbackModel = _settings.DeApiModelFallback ?? "Ltx2_3_22B_Dist_INT8";
        
        // If image-to-video is true, we might prefer the specific setting if available
        if (isImg2Video && !string.IsNullOrWhiteSpace(_settings.DeApiImg2VideoModel))
        {
            primaryModel = _settings.DeApiImg2VideoModel;
        }

        var primaryKey = _settings.DeApiApiKey;
        var fallbackKey = _settings.DeApiApiKeyFallback ?? _settings.DeApiApiKey;

        var modelsToTry = new List<(string Model, string Key)> { (primaryModel, primaryKey) };
        if (primaryModel != fallbackModel || primaryKey != fallbackKey)
        {
            if (!string.IsNullOrEmpty(fallbackModel))
            {
                modelsToTry.Add((fallbackModel, fallbackKey));
            }
        }

        try
        {
            byte[]? imageBytes = null;
            if (isImg2Video)
            {
                _logger.LogInformation("[DeAPI.Start] Mode: img2video. Downloading first_frame_image from {Url}", options!.FirstFrameImageUrl);
                try 
                { 
                    imageBytes = await _httpClient.GetByteArrayAsync(options.FirstFrameImageUrl, cancellationToken); 
                } 
                catch (Exception ex) 
                { 
                    _logger.LogWarning(ex, "[DeAPI.Start] Failed to download first_frame_image from {Url}. Falling back to text-to-video mode.", options!.FirstFrameImageUrl); 
                    isImg2Video = false; 
                }
            }

            HttpResponseMessage? response = null;
            string? errorBody = null;

            foreach (var m in modelsToTry)
            {
                string url;
                int reqFrames;
                int reqW, reqH;
                int fps = 24;
                HttpContent requestContent;

                // DeAPI v2 request IDs must be created by a v2 endpoint before they
                // can be queried through GET /api/v2/jobs/{request_id}.
                var endpoint = isImg2Video ? "videos/animations" : "videos/generations";
                url = $"{apiRoot}/api/v2/{endpoint}";

                reqW = 576; reqH = 1024;
                var ratio = options?.AspectRatio ?? "9:16";
                if (ratio == "16:9") { reqW = 1024; reqH = 576; }
                else if (ratio == "1:1") { reqW = 768; reqH = 768; }

                reqFrames = 193; 
                if (options?.DurationSeconds > 0)
                {
                    int requestedFrames = options.DurationSeconds * fps;
                    int n = (int)Math.Round(requestedFrames / 8.0);
                    reqFrames = (n * 8) + 1;
                }

                if (isImg2Video && imageBytes != null)
                {
                    var form = new MultipartFormDataContent();
                    var imageContent = new ByteArrayContent(imageBytes);
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    form.Add(imageContent, "first_frame_image", "frame.jpg");
                    
                    form.Add(new StringContent(reqFrames.ToString()), "frames");
                    form.Add(new StringContent(reqW.ToString()), "width");
                    form.Add(new StringContent(reqH.ToString()), "height");
                    form.Add(new StringContent(fps.ToString()), "fps");
                    if (IsLtx23Model(m.Model))
                    {
                        form.Add(new StringContent("8"), "steps");
                        form.Add(new StringContent(1.0.ToString(System.Globalization.CultureInfo.InvariantCulture)), "guidance");
                    }
                    form.Add(new StringContent(Random.Shared.Next(1, int.MaxValue).ToString()), "seed");
                    form.Add(new StringContent(m.Model), "model");

                    if (!string.IsNullOrWhiteSpace(prompt))
                        form.Add(new StringContent(prompt), "prompt");

                    requestContent = form;
                }
                else
                {
                    var payload = new Dictionary<string, object?>
                    {
                        ["prompt"] = prompt ?? "",
                        ["frames"] = reqFrames,
                        ["width"] = reqW,
                        ["height"] = reqH,
                        ["fps"] = fps,
                        ["seed"] = Random.Shared.Next(1, int.MaxValue),
                        ["model"] = m.Model
                    };
                    if (IsLtx23Model(m.Model))
                    {
                        payload["steps"] = 8;
                        payload["guidance"] = 1.0;
                    }
                    requestContent = JsonContent.Create(payload);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {m.Key}");
                request.Headers.Add("Accept", "application/json");
                request.Content = requestContent;


                _logger.LogInformation(
                    "[DeAPI.Start] Event={Event} CorrelationId={CorrelationId} ContentId={ContentId} AiGenerationId={AiGenerationId} UserId={UserId} Endpoint={Endpoint} RequestType={RequestType} Model={Model}",
                    "video.provider.create", options?.CorrelationId, options?.ContentId, options?.AiGenerationId, options?.UserId,
                    url, isImg2Video ? "image-to-video" : "text-to-video", m.Model);
                
                response = await _httpClient.SendAsync(request, cancellationToken);
                _logger.LogInformation("[DeAPI.Start] HTTP Status: {Status} for model {Model}", (int)response.StatusCode, m.Model);

                if (response.IsSuccessStatusCode)
                {
                    break; // Success!
                }

                errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("[DeAPI.Start] Model {Model} failed: {Status} - {Body}", m.Model, (int)response.StatusCode, errorBody);
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = GetRetryAfter(response);
                    _logger.LogWarning(
                        "[DeAPI.Start] Event={Event} CorrelationId={CorrelationId} HttpStatus=429 RetryAfterSeconds={RetryAfterSeconds}",
                        "video.provider.rate_limited", options?.CorrelationId, retryAfter.TotalSeconds);
                    return VideoGenerationResult.Fail(
                        $"DeAPI rate limit reached. Retry after {Math.Ceiling(retryAfter.TotalSeconds)} seconds.",
                        "DeAPI");
                }
                
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
        var taskId = id.StartsWith("deapi:", StringComparison.OrdinalIgnoreCase)
            ? id["deapi:".Length..]
            : id;
        
        var requestUrl = $"{GetApiRoot(_settings.DeApiBaseUrl)}/api/v2/jobs/{Uri.EscapeDataString(taskId)}";
        try
        {
            if (PollBackoffUntil.TryGetValue(taskId, out var backoffUntil))
            {
                if (backoffUntil > DateTimeOffset.UtcNow)
                {
                    _logger.LogInformation(
                        "[DeAPI.Poll] Event={Event} VideoJobId={VideoJobId} RetryAt={RetryAt}",
                        "video.provider.poll_deferred", $"deapi:{taskId}", backoffUntil);
                    return VideoGenerationResult.InProgress($"deapi:{taskId}", "DeAPI");
                }

                PollBackoffUntil.TryRemove(taskId, out _);
            }

            var keysToTry = new List<string> { _settings.DeApiApiKey };
        if (!string.IsNullOrWhiteSpace(_settings.DeApiApiKeyFallback) && _settings.DeApiApiKeyFallback != _settings.DeApiApiKey)
        {
            keysToTry.Add(_settings.DeApiApiKeyFallback);
        }

        HttpResponseMessage? response = null;
        HttpResponseMessage? rateLimitedResponse = null;
        string json = "";

        foreach (var key in keysToTry)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {key}");
            request.Headers.Add("Accept", "application/json");

            _logger.LogInformation("[DeAPI.Poll] Event={Event} VideoJobId={VideoJobId} Endpoint={Endpoint}",
                "video.provider.poll", $"deapi:{taskId}", requestUrl);

            response = await _httpClient.SendAsync(request, cancellationToken);
            json = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("[DeAPI.Poll] Event={Event} VideoJobId={VideoJobId} HttpStatus={HttpStatus} ResponseLength={ResponseLength}",
                "video.provider.poll_response", $"deapi:{taskId}", (int)response.StatusCode, json.Length);

            if (response.IsSuccessStatusCode)
            {
                break; // Found it!
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                rateLimitedResponse = response;
            
            // If 404 or unauthorized, it might be on the other account. Loop to the next key.
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            if (rateLimitedResponse != null)
            {
                var retryAfter = GetRetryAfter(rateLimitedResponse);
                PollBackoffUntil[taskId] = DateTimeOffset.UtcNow.Add(retryAfter);
                _logger.LogWarning("[DeAPI.Poll] Event={Event} VideoJobId={VideoJobId} HttpStatus=429 RetryAfterSeconds={RetryAfterSeconds}",
                    "video.provider.rate_limited", $"deapi:{taskId}", retryAfter.TotalSeconds);
                return VideoGenerationResult.InProgress($"deapi:{taskId}", "DeAPI");
            }

            if (response?.StatusCode == HttpStatusCode.NotFound)
            {
                PollBackoffUntil.TryRemove(taskId, out _);
                _logger.LogWarning("[DeAPI.Poll] Event={Event} VideoJobId={VideoJobId} HttpStatus=404 Error={Error}",
                    "video.provider.job_not_found", $"deapi:{taskId}", Truncate(error: json, 500));
                return VideoGenerationResult.Fail(
                    $"DeAPI job '{taskId}' was not found. The remote job is invalid, expired, or was created through an incompatible endpoint.",
                    "DeAPI");
            }
            
            return VideoGenerationResult.Fail($"HTTP {(int?)response?.StatusCode}: {json}", "DeAPI");
        }

            PollBackoffUntil.TryRemove(taskId, out _);
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

    internal static string GetApiRoot(string? configuredBaseUrl)
    {
        var configured = string.IsNullOrWhiteSpace(configuredBaseUrl)
            ? "https://api.deapi.ai"
            : configuredBaseUrl.Trim();

        if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri))
            return "https://api.deapi.ai";

        return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    private static TimeSpan GetRetryAfter(HttpResponseMessage response)
    {
        var delta = response.Headers.RetryAfter?.Delta;
        if (delta.HasValue && delta.Value > TimeSpan.Zero)
            return delta.Value;

        var date = response.Headers.RetryAfter?.Date;
        if (date.HasValue && date.Value > DateTimeOffset.UtcNow)
            return date.Value - DateTimeOffset.UtcNow;

        return TimeSpan.FromSeconds(60);
    }

    private static bool IsLtx23Model(string model) =>
        model.Equals("Ltx2_3_22B_Dist_INT8", StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string error, int maxLength) =>
        error.Length <= maxLength ? error : error[..maxLength] + "...";

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
