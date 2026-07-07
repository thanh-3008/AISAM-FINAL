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
        // Sử dụng endpoint từ doc API
        var url = baseUrl.EndsWith("/client/txt2video") ? baseUrl : $"{baseUrl.TrimEnd('/')}/client/txt2video";
        var model = _settings.DeApiModel ?? "Ltxv_13B_0_9_8_Distilled_FP8";

        // Map aspect ratio to width and height. Max dimension is 768.
        // For 9:16 -> 432x768
        // For 16:9 -> 768x432
        // For 1:1 -> 768x768
        int w = 432, h = 768;
        var ratio = options?.AspectRatio ?? "9:16";
        if (ratio == "16:9") { w = 768; h = 432; }
        else if (ratio == "1:1") { w = 768; h = 768; }

        var payload = new
        {
            prompt = prompt,
            frames = options?.DurationSeconds > 0 ? options.DurationSeconds * 30 : 120, // Default 4s
            width = w,
            height = h,
            fps = 30,
            model = model,
            steps = 1,
            seed = Random.Shared.Next(1, 1000000000), // Require seed
            negative_prompt = "low quality, worst quality, deformed"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_settings.DeApiApiKey}");
        request.Headers.Add("Accept", "application/json");
        request.Content = JsonContent.Create(payload);

        _logger.LogInformation("[DeAPI.Start] Sending POST to {Url}", url);
        _logger.LogInformation("[DeAPI.Start] Payload: prompt={PromptLen}chars, model={Model}, size={W}x{H}, seed=included", prompt.Length, model, w, h);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            _logger.LogInformation("[DeAPI.Start] HTTP Status: {Status}", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("[DeAPI.Start] Error response body: {Body}", errorBody);
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {errorBody}", "DeAPI");
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

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("Authorization", $"Bearer {_settings.DeApiApiKey}");
        request.Headers.Add("Accept", "application/json");

        _logger.LogInformation("[DeAPI.Poll] Polling task: GET {Url}", requestUrl);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("[DeAPI.Poll] HTTP Status: {Status}, Body: {Body}", (int)response.StatusCode, json.Length > 500 ? json[..500] + "..." : json);
            
            if (!response.IsSuccessStatusCode)
            {
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {json}", "DeAPI");
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
