using System.Net.Http.Json;
using AISAM.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class HuggingFaceImageClient
{
    private readonly HttpClient _httpClient;
    private readonly ImageProviderSettings _settings;
    private readonly ILogger<HuggingFaceImageClient> _logger;

    public HuggingFaceImageClient(
        HttpClient httpClient,
        IOptions<ImageProviderSettings> config,
        ILogger<HuggingFaceImageClient> logger)
    {
        _httpClient = httpClient;
        _settings = config.Value;
        _logger = logger;
    }

    public async Task<(byte[]? Bytes, string? Url, string? Error)> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        var model = string.IsNullOrWhiteSpace(_settings.HuggingFaceModel) ? "black-forest-labs/FLUX.1-schnell" : _settings.HuggingFaceModel;
        var baseUrl = string.IsNullOrWhiteSpace(_settings.HuggingFaceBaseUrl) ? "https://api-inference.huggingface.co/models/" : _settings.HuggingFaceBaseUrl;
        var url = $"{baseUrl.TrimEnd('/')}/{model}";

        var payload = new
        {
            inputs = prompt,
            parameters = new { num_inference_steps = 5 }
        };

        int maxRetries = 3;
        int delayMs = 1000;
        HttpResponseMessage? response = null;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(payload)
                };
                
                if (!string.IsNullOrWhiteSpace(_settings.HuggingFaceApiKey))
                {
                    requestMessage.Headers.Add("Authorization", $"Bearer {_settings.HuggingFaceApiKey}");
                }

                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
                
                if ((int)response.StatusCode < 500 && response.StatusCode != System.Net.HttpStatusCode.RequestTimeout && response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                {
                    // Do not retry on 4xx except timeout and rate limit
                    break;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                _logger.LogWarning("Hugging Face attempt {Attempt} failed: {Msg}. Retrying in {Delay}ms...", i + 1, ex.Message, delayMs);
                if (i == maxRetries - 1)
                {
                    break;
                }
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2;
            }
        }

        try
        {
            if (response == null)
            {
                return (null, null, "Failed to connect to Hugging Face after multiple attempts.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return (null, null, $"HTTP {(int)response.StatusCode}: {errorBody}");
            }

            // Hugging Face returns the image directly as a binary blob
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (bytes != null && bytes.Length > 0)
            {
                return (bytes, null, null);
            }

            return (null, null, "Empty response from Hugging Face.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hugging Face image generation exception.");
            return (null, null, ex.Message);
        }
    }
}
