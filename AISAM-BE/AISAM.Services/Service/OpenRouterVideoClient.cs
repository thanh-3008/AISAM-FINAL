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

    public async Task<VideoGenerationResult> StartAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenRouterApiKey))
            return VideoGenerationResult.Fail("OpenRouter Video API Key is missing.", "OpenRouter");

        var model = _settings.OpenRouterModel ?? "minimax/video-01";
        var url = _settings.OpenRouterBaseUrl ?? "https://openrouter.ai/api/v1/videos";

        object payload;
        if (url.Contains("seedance"))
        {
            payload = new
            {
                mode = "text-to-video",
                quality_tier = "standard",
                prompt = prompt,
                aspect_ratio = "16:9",
                duration = "5",
                resolution = "720p"
            };
        }
        else
        {
            payload = new
            {
                model = model,
                prompt = prompt
            };
        }

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_settings.OpenRouterApiKey}");
        request.Content = JsonContent.Create(payload);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {errorBody}", "OpenRouter");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);

            // OpenRouter may return an id and a polling_url. DeAPI returns request_id.
            var root = document.RootElement;
            string? id = null;
            if (root.TryGetProperty("id", out var idElement)) id = idElement.GetString();
            else if (root.TryGetProperty("request_id", out var reqIdElement)) id = reqIdElement.GetString();

            if (!string.IsNullOrEmpty(id))
            {
                string? pollingUrl = null;
                if (root.TryGetProperty("polling_url", out var pollElement)) pollingUrl = pollElement.GetString();

                // Prefer returning polling URL if provided, otherwise a namespaced id
                return VideoGenerationResult.Queued(pollingUrl ?? $"openrouter:{id}", "OpenRouter");
            }

            return VideoGenerationResult.Fail("Did not receive job ID from OpenRouter.", "OpenRouter");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenRouter video start exception.");
            return VideoGenerationResult.Fail(ex.Message, "OpenRouter");
        }
    }

    public async Task<VideoGenerationResult> PollAsync(string id, CancellationToken cancellationToken)
    {
        // Allow passing either a full polling URL or an id. If the id looks like a URL, use it directly.
        var baseUrl = _settings.OpenRouterBaseUrl ?? "https://openrouter.ai/api/v1/videos";
        // If the URL ends with /generations or /txt2video, we might need to strip it or replace it, 
        // but typically appending /id works or using the correct status endpoint.
        // For deapi, it is usually /request-status/{id}
        // For seedance2ai, it is /api/v1/tasks/{id}
        var requestUrl = id.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? id
            : baseUrl.Contains("deapi") ? baseUrl.Replace("txt2video", "request-status").Replace("generations", "request-status") + $"/{id}" 
            : baseUrl.Contains("seedance") ? baseUrl.Replace("video/seedance2", "tasks") + $"/{id.Replace("openrouter:", "")}"
            : $"{baseUrl}/{id}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("Authorization", $"Bearer {_settings.OpenRouterApiKey}");

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {errorBody}", "OpenRouter");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("status", out var statusElement))
            {
                var status = statusElement.GetString();
                if (status == "completed")
                {
                    if (root.TryGetProperty("video_url", out var urlElement)) 
                    {
                        return VideoGenerationResult.Done(urlElement.GetString()!, "OpenRouter");
                    }
                    if (root.TryGetProperty("output", out var outputObj) && outputObj.TryGetProperty("video_url", out var outputUrl))
                    {
                        return VideoGenerationResult.Done(outputUrl.GetString()!, "OpenRouter");
                    }
                    return VideoGenerationResult.Fail("Job completed but video URL missing.", "OpenRouter");
                }
                else if (status == "failed")
                {
                    return VideoGenerationResult.Fail("OpenRouter job failed.", "OpenRouter");
                }
            }

            return VideoGenerationResult.InProgress($"openrouter:{id}", "OpenRouter");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenRouter video poll exception.");
            return VideoGenerationResult.Fail(ex.Message, "OpenRouter");
        }
    }
}
