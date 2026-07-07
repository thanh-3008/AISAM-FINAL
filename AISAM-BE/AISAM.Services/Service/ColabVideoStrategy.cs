using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class ColabVideoStrategy : IAIVideoProvider
{
    private readonly HttpClient _httpClient;
    private readonly IGeminiTextClient _geminiTextClient;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<ColabVideoStrategy> _logger;

    public string ProviderName => "Colab(Wan2.2)";

    public ColabVideoStrategy(
        HttpClient httpClient,
        IGeminiTextClient geminiTextClient,
        IOptions<VideoProviderSettings> options,
        ILogger<ColabVideoStrategy> logger)
    {
        _httpClient = httpClient;
        _geminiTextClient = geminiTextClient;
        _settings = options.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_settings.ColabBaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_settings.ColabBaseUrl);
        }
        if (!string.IsNullOrWhiteSpace(_settings.ColabToken))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ColabToken}");
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ColabToken);
        }
        if (_settings.ColabTimeout > 0)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.ColabTimeout);
        }
    }

    public async Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Colab] Starting video generation for prompt: '{Prompt}'", prompt);
        if (string.IsNullOrWhiteSpace(_settings.ColabBaseUrl))
        {
            _logger.LogWarning("[Colab] Base URL is not configured. Aborting.");
            return VideoGenerationResult.Fail("Colab base URL is not configured.", ProviderName);
        }

        try
        {
            // 1. Health check
            _logger.LogInformation("[Colab] Performing health check at {BaseUrl}/health", _settings.ColabBaseUrl);
            var healthResponse = await _httpClient.GetAsync("/health", cancellationToken);
            if (!healthResponse.IsSuccessStatusCode)
            {
                var healthContent = await healthResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("[Colab] Health check failed with status {StatusCode}. Response: {Content}", healthResponse.StatusCode, healthContent);
                return VideoGenerationResult.Fail("Colab endpoint is unhealthy or unreachable.", ProviderName);
            }
            _logger.LogInformation("[Colab] Health check passed.");

            // 2. Split prompt using Gemini
            int segmentCount = _settings.DefaultSegmentCount > 0 ? _settings.DefaultSegmentCount : 3;
            _logger.LogInformation("[Colab] Splitting prompt into {Count} segments using Gemini...", segmentCount);
            var segments = await SplitPromptAsync(prompt, segmentCount, cancellationToken);
            if (segments.Count == 0)
            {
                _logger.LogWarning("[Colab] Failed to split prompt into segments.");
                return VideoGenerationResult.Fail("Failed to split prompt into segments.", ProviderName);
            }
            _logger.LogInformation("[Colab] Prompt successfully split into {Count} segments: {Segments}", segments.Count, JsonSerializer.Serialize(segments));

            // 3. Trigger Colab job
            var requestBody = new { segments = segments };
            _logger.LogInformation("[Colab] Sending job request to /generate-story-video...");
            var response = await _httpClient.PostAsJsonAsync("/generate-story-video", requestBody, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[Colab] Job rejected with status {StatusCode}. Response: {Content}", response.StatusCode, responseContent);
                return VideoGenerationResult.Fail($"Colab rejected job: {responseContent}", ProviderName);
            }

            var result = JsonSerializer.Deserialize<ColabJobResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result == null || string.IsNullOrWhiteSpace(result.JobId))
            {
                _logger.LogWarning("[Colab] Job accepted but returned invalid response format: {Content}", responseContent);
                return VideoGenerationResult.Fail("Colab returned invalid job response.", ProviderName);
            }

            _logger.LogInformation("[Colab] Job successfully queued with ID: {JobId}", result.JobId);
            return VideoGenerationResult.Queued(result.JobId, ProviderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Colab] Exception occurred during StartVideoGenerationAsync.");
            return VideoGenerationResult.Fail($"Error communicating with Colab: {ex.Message}", ProviderName);
        }
    }

    public async Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ColabBaseUrl))
        {
            return VideoGenerationResult.Fail("Colab base URL is not configured.", ProviderName);
        }

        try
        {
            var response = await _httpClient.GetAsync($"/job-status/{jobId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return VideoGenerationResult.Fail($"Failed to check Colab job status: {error}", ProviderName);
            }

            var status = await response.Content.ReadFromJsonAsync<ColabStatusResponse>(cancellationToken: cancellationToken);
            if (status == null)
            {
                return VideoGenerationResult.Fail("Colab returned invalid status response.", ProviderName);
            }

            if (status.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                var url = status.VideoUrl ?? string.Empty;
                if (!url.StartsWith("http") && !string.IsNullOrWhiteSpace(_settings.ColabBaseUrl))
                {
                    // If Colab returns relative URL, make it absolute
                    url = $"{_settings.ColabBaseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
                }
                return VideoGenerationResult.Done(url, ProviderName);
            }

            if (status.Status.Equals("failed", StringComparison.OrdinalIgnoreCase))
            {
                return VideoGenerationResult.Fail(status.Error ?? "Job failed on Colab.", ProviderName);
            }

            // Still processing
            return VideoGenerationResult.InProgress(jobId, ProviderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check Colab job status for JobId {JobId}", jobId);
            return VideoGenerationResult.Fail($"Error communicating with Colab: {ex.Message}", ProviderName);
        }
    }

    private async Task<List<string>> SplitPromptAsync(string prompt, int segmentCount, CancellationToken cancellationToken)
    {
        var instruction = $$"""
You are a video script assistant. The user wants to create a continuous video divided into exactly {{segmentCount}} consecutive scenes.
Translate the following prompt into English if it's in Vietnamese.
Return ONLY a valid JSON array of strings, where each string is a detailed visual description of a scene.
Make sure the scenes flow logically from one to the next to maintain continuity.
Do not include markdown blocks or any other text outside the JSON array.

Prompt:
{{prompt}}
""";
        
        try
        {
            var geminiResponse = await _geminiTextClient.GenerateAsync(instruction, cancellationToken);
            return ParseSegments(geminiResponse) ?? new List<string> { prompt }; // Fallback to single prompt
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to use Gemini to split prompt.");
            return new List<string> { prompt };
        }
    }

    private List<string>? ParseSegments(string jsonResponse)
    {
        try
        {
            var json = jsonResponse.Trim();
            if (json.StartsWith("```", StringComparison.Ordinal))
            {
                var firstNewLine = json.IndexOf('\n');
                var lastFence = json.LastIndexOf("```", StringComparison.Ordinal);
                if (firstNewLine >= 0 && lastFence > firstNewLine)
                {
                    json = json[(firstNewLine + 1)..lastFence].Trim();
                }
            }
            if (json.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                json = json[4..].Trim();
            }

            return JsonSerializer.Deserialize<List<string>>(json);
        }
        catch
        {
            return null;
        }
    }
}

public class ColabJobResponse
{
    [JsonPropertyName("job_id")]
    public string? JobId { get; set; }
}

public class ColabStatusResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("current_segment")]
    public int? CurrentSegment { get; set; }

    [JsonPropertyName("total_segments")]
    public int? TotalSegments { get; set; }
}
