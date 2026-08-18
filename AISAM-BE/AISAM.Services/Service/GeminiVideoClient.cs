using System.Net.Http.Json;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class GeminiVideoClient
{
    private readonly HttpClient _httpClient;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<GeminiVideoClient> _logger;

    public GeminiVideoClient(
        HttpClient httpClient,
        IOptions<VideoProviderSettings> config,
        ILogger<GeminiVideoClient> logger)
    {
        _httpClient = httpClient;
        _settings = config.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.GeminiApiKey))
            return VideoGenerationResult.Fail("Gemini Video API Key is missing.", "Gemini");

        var model = _settings.GeminiModel ?? "veo-2.0-generate-001";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.GeminiApiKey}";

        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { responseModalities = new[] { "VIDEO" } },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
            }
        };

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_settings.GeminiTimeoutSeconds));

            var response = await _httpClient.PostAsJsonAsync(url, payload, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cts.Token);
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {errorBody}", "Gemini");
            }

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            using var document = JsonDocument.Parse(json);
            
            // Expected response format for async job: { "name": "operations/..." }
            if (document.RootElement.TryGetProperty("name", out var nameElement))
            {
                return VideoGenerationResult.Queued($"gemini:{nameElement.GetString()}", "Gemini");
            }

            return VideoGenerationResult.Fail("Did not receive operation name from Gemini.", "Gemini");
        }
        catch (OperationCanceledException)
        {
            return VideoGenerationResult.Fail($"Request timed out.", "Gemini");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini video start exception.");
            return VideoGenerationResult.Fail(ex.Message, "Gemini");
        }
    }

    public async Task<VideoGenerationResult> PollAsync(string operationName, CancellationToken cancellationToken)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/{operationName}?key={_settings.GeminiApiKey}";
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return VideoGenerationResult.Fail($"HTTP {(int)response.StatusCode}: {errorBody}", "Gemini");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("done", out var doneElement) && doneElement.GetBoolean())
            {
                if (root.TryGetProperty("error", out var errorElement))
                {
                    return VideoGenerationResult.Fail(errorElement.GetRawText(), "Gemini");
                }

                if (root.TryGetProperty("response", out var respElement))
                {
                    // Video URL typically might not be a direct URL in Gemini, but a Base64 stream or something else.
                    // Assuming for now it returns a URI we can download, or we need to extract bytes.
                    // For the sake of this client, we assume it provides a URI in 'videoUri' or similar, or we might need to parse inlineData.
                    
                    if (respElement.TryGetProperty("videoUri", out var uriElement))
                    {
                        return VideoGenerationResult.Done(uriElement.GetString()!, "Gemini");
                    }
                    
                    if (respElement.TryGetProperty("candidates", out var cands) && cands.GetArrayLength() > 0)
                    {
                        // Some models might return it inside candidates.
                        return VideoGenerationResult.Fail("Video returned inline but not supported yet.", "Gemini");
                    }
                }
                
                return VideoGenerationResult.Fail("Job done but no video URL found.", "Gemini");
            }

            return VideoGenerationResult.InProgress($"gemini:{operationName}", "Gemini");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini video poll exception.");
            return VideoGenerationResult.Fail(ex.Message, "Gemini");
        }
    }
}
