using System.Net.Http.Json;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class OpenRouterVideoClient
{
    private readonly HttpClient _httpClient;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<OpenRouterVideoClient> _logger;

    public OpenRouterVideoClient(
        HttpClient httpClient,
        IOptions<VideoProviderSettings> config,
        ILogger<OpenRouterVideoClient> logger)
    {
        _httpClient = httpClient;
        _settings = config.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartAsync(string prompt, VideoGenerationOptions? options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenRouterApiKey))
            return VideoGenerationResult.Fail("Pollo API Key is missing.", "Pollo");

        var baseUrl = _settings.OpenRouterBaseUrl ?? "https://pollo.ai/api/platform";
        var url = $"{baseUrl.TrimEnd('/')}/generation/pollo/pollo-v1-5";

        var payload = new
        {
            input = new
            {
                prompt = prompt,
                aspectRatio = options?.AspectRatio ?? "9:16"
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        // Pollo uses x-api-key for authentication
        request.Headers.Add("x-api-key", _settings.OpenRouterApiKey);
        request.Content = JsonContent.Create(payload);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {errorBody}", "Pollo");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var targetNode = root;
            if (root.TryGetProperty("data", out var dataNode) && dataNode.ValueKind == JsonValueKind.Object)
            {
                targetNode = dataNode;
            }

            if (targetNode.TryGetProperty("taskId", out var taskIdElement))
            {
                var taskId = taskIdElement.GetString();
                if (!string.IsNullOrEmpty(taskId))
                {
                    // Keep the 'openrouter:' prefix so FallbackVideoProvider still forwards the poll to this client.
                    return VideoGenerationResult.Queued($"openrouter:{taskId}", "Pollo");
                }
            }

            return VideoGenerationResult.Fail("Did not receive taskId from Pollo.", "Pollo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pollo video start exception.");
            return VideoGenerationResult.Fail(ex.Message, "Pollo");
        }
    }

    public async Task<VideoGenerationResult> PollAsync(string id, CancellationToken cancellationToken)
    {
        var baseUrl = _settings.OpenRouterBaseUrl ?? "https://pollo.ai/api/platform";
        var taskId = id.Replace("openrouter:", "");
        
        var requestUrl = taskId.StartsWith("http", StringComparison.OrdinalIgnoreCase) 
            ? taskId 
            : $"{baseUrl.TrimEnd('/')}/generation/tasks/{taskId}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("x-api-key", _settings.OpenRouterApiKey);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("[Pollo Poll] TaskId={TaskId} Status={HttpStatus} Body={Body}", taskId, (int)response.StatusCode, json);
            
            if (!response.IsSuccessStatusCode)
            {
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {json}", "Pollo");
            }

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            // Navigate to data node if present
            var targetNode = root;
            if (root.TryGetProperty("data", out var dataNode) && dataNode.ValueKind == JsonValueKind.Object)
            {
                targetNode = dataNode;
            }

            if (targetNode.TryGetProperty("status", out var statusElement))
            {
                var status = statusElement.GetString()?.ToLower();
                _logger.LogInformation("[Pollo Poll] TaskId={TaskId} PolloStatus={Status}", taskId, status);
                
                if (status == "succeed" || status == "success" || status == "completed" || status == "done")
                {
                    // Try various possible URL field names Pollo might return
                    string? videoUrl = null;
                    
                    // Check direct fields
                    foreach (var field in new[] { "videoUrl", "video_url", "resultUrl", "result_url", "url", "outputUrl", "output_url" })
                    {
                        if (targetNode.TryGetProperty(field, out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                        {
                            videoUrl = urlEl.GetString();
                            if (!string.IsNullOrEmpty(videoUrl)) break;
                        }
                    }
                    
                    // Check outputs array: outputs[0].url or outputs[0].video_url
                    if (string.IsNullOrEmpty(videoUrl) && targetNode.TryGetProperty("outputs", out var outputsEl) && outputsEl.ValueKind == JsonValueKind.Array && outputsEl.GetArrayLength() > 0)
                    {
                        var first = outputsEl[0];
                        foreach (var field in new[] { "url", "videoUrl", "video_url", "resultUrl" })
                        {
                            if (first.TryGetProperty(field, out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                            {
                                videoUrl = urlEl.GetString();
                                if (!string.IsNullOrEmpty(videoUrl)) break;
                            }
                        }
                    }
                    
                    // Check result object
                    if (string.IsNullOrEmpty(videoUrl) && targetNode.TryGetProperty("result", out var resultEl) && resultEl.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var field in new[] { "url", "videoUrl", "video_url" })
                        {
                            if (resultEl.TryGetProperty(field, out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
                            {
                                videoUrl = urlEl.GetString();
                                if (!string.IsNullOrEmpty(videoUrl)) break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(videoUrl))
                    {
                        _logger.LogInformation("[Pollo Poll] TaskId={TaskId} Done. VideoUrl={VideoUrl}", taskId, videoUrl);
                        return VideoGenerationResult.Done(videoUrl, "Pollo");
                    }
                    
                    _logger.LogWarning("[Pollo Poll] TaskId={TaskId} Status=succeed but no video URL found. Full JSON: {JSON}", taskId, json);
                    return VideoGenerationResult.Fail("Job completed but video URL missing from response. JSON: " + json, "Pollo");
                }
                else if (status == "failed" || status == "error" || status == "fail")
                {
                    var errMsg = targetNode.TryGetProperty("errorMessage", out var errEl) ? errEl.GetString() : 
                                 targetNode.TryGetProperty("error_message", out var errEl2) ? errEl2.GetString() : "Pollo job failed.";
                    _logger.LogError("[Pollo Poll] TaskId={TaskId} Failed: {Error}", taskId, errMsg);
                    return VideoGenerationResult.Fail(errMsg ?? "Pollo job failed.", "Pollo");
                }
            }

            _logger.LogInformation("[Pollo Poll] TaskId={TaskId} still processing. JSON: {JSON}", taskId, json);
            return VideoGenerationResult.InProgress($"openrouter:{taskId}", "Pollo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pollo video poll exception.");
            return VideoGenerationResult.Fail(ex.Message, "Pollo");
        }
    }
}
