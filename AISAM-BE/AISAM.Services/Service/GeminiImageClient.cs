using System.Net.Http.Json;
using System.Text.Json;
using AISAM.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class GeminiImageClient
{
    private readonly HttpClient _httpClient;
    private readonly ImageProviderSettings _settings;
    private readonly ILogger<GeminiImageClient> _logger;

    public GeminiImageClient(
        HttpClient httpClient,
        IOptions<ImageProviderSettings> config,
        ILogger<GeminiImageClient> logger)
    {
        _httpClient = httpClient;
        _settings = config.Value;
        _logger = logger;
    }

    public async Task<(byte[]? Bytes, string? Error)> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.GeminiApiKey))
            return (null, "Gemini API Key is missing.");

        var model = _settings.GeminiModel ?? "imagen-3.0-generate-002";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.GeminiApiKey}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseModalities = new[] { "IMAGE" }
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
                return (null, $"HTTP {(int)response.StatusCode}: {errorBody}");
            }

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            using var document = JsonDocument.Parse(json);
            
            var base64Data = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("inlineData")
                .GetProperty("data")
                .GetString();

            if (string.IsNullOrEmpty(base64Data))
                return (null, "No image data returned from Gemini.");

            return (Convert.FromBase64String(base64Data), null);
        }
        catch (OperationCanceledException)
        {
            return (null, $"Gemini request timed out after {_settings.GeminiTimeoutSeconds}s.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini image generation exception.");
            return (null, ex.Message);
        }
    }
}
