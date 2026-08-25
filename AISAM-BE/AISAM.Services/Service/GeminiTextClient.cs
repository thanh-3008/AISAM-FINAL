using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

namespace AISAM.Services.Service;

public sealed class GeminiTextClient : IGeminiTextClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiTextClient> _logger;

    public GeminiTextClient(HttpClient httpClient, IOptions<GeminiSettings> settings, ILogger<GeminiTextClient>? logger = null)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<GeminiTextClient>.Instance;
    }

    public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        => GenerateAsync(prompt, null, cancellationToken);

    public async Task<string> GenerateAsync(string prompt, string? responseMimeType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var primaryModel = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-3.6-flash" : _settings.Model;
        var effectiveResponseMimeType = responseMimeType ?? "text/plain";
        var modelsToTry = primaryModel == "gemini-3.6-flash" 
            ? new[] { primaryModel } 
            : new[] { primaryModel, "gemini-3.6-flash" };

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

        HttpRequestException? lastException = null;

        foreach (var model in modelsToTry)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                GeminiDiagnosticLogging.LogRequestConfiguration(
                    _logger,
                    "Gemini",
                    model,
                    _settings.MaxTokens,
                    _settings.Temperature,
                    effectiveResponseMimeType);

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.ApiKey}";
                var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    lastException = new HttpRequestException($"Gemini API ({model}) returned {(int)response.StatusCode}: {ExtractErrorMessage(errorBody)}");
                    
                    if ((int)response.StatusCode >= 500 || (int)response.StatusCode == 404 || (int)response.StatusCode == 429)
                    {
                        continue;
                    }
                    throw lastException;
                }

                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                GeminiDiagnosticLogging.LogResponseMetadata(_logger, "Gemini", model, document.RootElement);
                var text = document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"Gemini API ({model}) returned an empty response.");
                }

                _logger.LogInformation(
                    "AskAI.LLM.ProviderModelAttempt ProviderName={ProviderName} AttemptOrder={AttemptOrder} DurationMs={DurationMs} Success={Success} FailureCategory={FailureCategory} Cancelled={Cancelled}",
                    "Gemini", Array.IndexOf(modelsToTry, model) + 1, timer.ElapsedMilliseconds, true, null, false);
                return text.Trim();
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "AskAI.LLM.ProviderModelAttempt ProviderName={ProviderName} AttemptOrder={AttemptOrder} DurationMs={DurationMs} Success={Success} FailureCategory={FailureCategory} Cancelled={Cancelled}",
                    "Gemini", Array.IndexOf(modelsToTry, model) + 1, timer.ElapsedMilliseconds, false, "LLM_PROVIDER_FAILURE", false);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(
                    "AskAI.LLM.ProviderModelAttempt ProviderName={ProviderName} AttemptOrder={AttemptOrder} DurationMs={DurationMs} Success={Success} FailureCategory={FailureCategory} Cancelled={Cancelled} ExceptionType={ExceptionType}",
                    "Gemini", Array.IndexOf(modelsToTry, model) + 1, timer.ElapsedMilliseconds, false, "LLM_TIMEOUT", true, ex.GetType().Name);
                throw;
            }
        }

        if (lastException != null) throw lastException;
        throw new InvalidOperationException("Failed to generate content from any Gemini model.");
    }

    public Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
        => GenerateWithVisionAsync(textPrompt, imageBytes, mimeType, null, cancellationToken);

    public async Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType, string? responseMimeType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var primaryModel = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-3.6-flash" : _settings.Model;
        var modelsToTry = primaryModel == "gemini-3.6-flash" 
            ? new[] { primaryModel } 
            : new[] { primaryModel, "gemini-3.6-flash" };

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
                responseMimeType = responseMimeType ?? "text/plain"
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
            }
        };

        HttpRequestException? lastException = null;

        foreach (var model in modelsToTry)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.ApiKey}";
                var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    lastException = new HttpRequestException($"Gemini Vision API ({model}) returned {(int)response.StatusCode}: {ExtractErrorMessage(errorBody)}");
                    
                    if ((int)response.StatusCode >= 500 || (int)response.StatusCode == 404 || (int)response.StatusCode == 429)
                    {
                        continue;
                    }
                    throw lastException;
                }

                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                var text = document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException($"Gemini Vision API ({model}) returned an empty response.");
                }

                return text.Trim();
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }
        }

        if (lastException != null) throw lastException;
        throw new InvalidOperationException("Failed to generate content from any Gemini Vision model.");
    }

    public static string ExtractErrorMessage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? "Unknown provider error.";
            }
        }
        catch (JsonException)
        {
            // Fall through to a safe generic message.
        }

        return "Unknown provider error.";
    }
}
