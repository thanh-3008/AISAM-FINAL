using AISAM.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace AISAM.Services.Service;

public sealed class OpenRouterTextClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<OpenRouterTextClient> _logger;

    public OpenRouterTextClient(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<OpenRouterTextClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenRouterApiKey))
        {
            throw new InvalidOperationException("OpenRouter API key is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(_settings.OpenRouterModel) ? "google/gemini-2.5-flash" : _settings.OpenRouterModel;
        var url = "https://openrouter.ai/api/v1/chat/completions";
        
        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = _settings.MaxTokens,
            temperature = _settings.Temperature
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.OpenRouterApiKey);
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"OpenRouter API returned {(int)response.StatusCode}: {errorBody}");
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        
        if (document.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var choice = choices[0];
            if (choice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
            {
                return content.GetString() ?? string.Empty;
            }
        }

        throw new InvalidOperationException("OpenRouter API returned an empty or invalid response.");
    }
}
