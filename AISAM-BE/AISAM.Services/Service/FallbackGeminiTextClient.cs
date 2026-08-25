using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AISAM.Services.Service;

public sealed class FallbackGeminiTextClient : IGeminiTextClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<FallbackGeminiTextClient> _logger;

    public FallbackGeminiTextClient(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<FallbackGeminiTextClient>? logger = null)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<FallbackGeminiTextClient>.Instance;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.FallbackApiKey))
        {
            throw new InvalidOperationException("Fallback Gemini API key is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-3.6-flash" : _settings.Model;
        const string effectiveResponseMimeType = "text/plain";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.FallbackApiKey}";
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                maxOutputTokens = _settings.MaxTokens,
                temperature = _settings.Temperature,
                responseMimeType = effectiveResponseMimeType
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
            }
        };

        GeminiDiagnosticLogging.LogRequestConfiguration(
            _logger,
            "FallbackGemini1",
            model,
            _settings.MaxTokens,
            _settings.Temperature,
            effectiveResponseMimeType);

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Fallback Gemini API returned {(int)response.StatusCode}: {GeminiTextClient.ExtractErrorMessage(errorBody)}");
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        GeminiDiagnosticLogging.LogResponseMetadata(_logger, "FallbackGemini1", model, document.RootElement);
        
        if (document.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("finishReason", out var fr))
            {
                Console.WriteLine($"[FallbackGeminiTextClient] finishReason: {fr.GetString()}");
            }
            else
            {
                Console.WriteLine("[FallbackGeminiTextClient] finishReason: (missing)");
            }

            var text = candidate
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
                
            return text ?? string.Empty;
        }

        return string.Empty;
    }

    public async Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.FallbackApiKey))
        {
            throw new InvalidOperationException("Fallback Gemini API key is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-3.6-flash" : _settings.Model;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.FallbackApiKey}";
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = textPrompt },
                        new { inlineData = new { mimeType = mimeType, data = Convert.ToBase64String(imageBytes) } }
                    }
                }
            },
            generationConfig = new
            {
                maxOutputTokens = _settings.MaxTokens,
                temperature = _settings.Temperature,
                responseMimeType = "text/plain"
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Fallback Gemini Vision API returned {(int)response.StatusCode}: {GeminiTextClient.ExtractErrorMessage(errorBody)}");
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }
}
