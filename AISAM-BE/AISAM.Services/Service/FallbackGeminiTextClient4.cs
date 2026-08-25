using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace AISAM.Services.Service;

public sealed class FallbackGeminiTextClient4 : IGeminiTextClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<FallbackGeminiTextClient4> _logger;

    public FallbackGeminiTextClient4(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<FallbackGeminiTextClient4>? logger = null)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<FallbackGeminiTextClient4>.Instance;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.FallbackApiKey4))
        {
            throw new InvalidOperationException("Gemini Fallback API key 4 is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-3.6-flash" : _settings.Model;
        const string effectiveResponseMimeType = "text/plain";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.FallbackApiKey4}";
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
            "FallbackGemini4",
            model,
            _settings.MaxTokens,
            _settings.Temperature,
            effectiveResponseMimeType);

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Fallback 4 Gemini API returned {(int)response.StatusCode}: {GeminiTextClient.ExtractErrorMessage(errorBody)}");
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        GeminiDiagnosticLogging.LogResponseMetadata(_logger, "FallbackGemini4", model, document.RootElement);
        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }

    public async Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.FallbackApiKey4))
        {
            throw new InvalidOperationException("Gemini Fallback API key 4 is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-3.6-flash" : _settings.Model;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.FallbackApiKey4}";
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
            throw new HttpRequestException($"Fallback 4 Gemini API returned {(int)response.StatusCode}: {GeminiTextClient.ExtractErrorMessage(errorBody)}");
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
