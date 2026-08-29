using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public class OpenAIImageResult
{
    public byte[]? Bytes { get; set; }
    public string? Error { get; set; }
}

public sealed class OpenAIImageClient
{
    private readonly HttpClient _httpClient;
    private readonly ImageProviderSettings _settings;
    private readonly ILogger<OpenAIImageClient> _logger;

    public OpenAIImageClient(HttpClient httpClient, IOptions<ImageProviderSettings> options, ILogger<OpenAIImageClient> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<OpenAIImageResult> GenerateAsync(string prompt, ImageGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenAiApiKey))
        {
            return new OpenAIImageResult { Error = "OpenAI API Key is missing." };
        }

        var width = options?.Width ?? 1024;
        var height = options?.Height ?? 1024;
        var size = $"{width}x{height}"; // e.g. 1024x1024, 720x1280

        var hasReference = options?.ReferenceImageUrls != null && options.ReferenceImageUrls.Any();
        
        if (hasReference)
        {
            return await GenerateEditAsync(prompt, options!.ReferenceImageUrls!, size, cancellationToken);
        }

        return await GenerateFromScratchAsync(prompt, size, cancellationToken);
    }

    private async Task<OpenAIImageResult> GenerateFromScratchAsync(string prompt, string size, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                model = _settings.OpenAiImageModel,
                prompt = prompt,
                size = size,
                quality = _settings.OpenAiImageQuality,
                n = 1
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[OpenAIImageClient] Generations failed. Status: {Status}, Response: {Response}", response.StatusCode, responseString);
                return new OpenAIImageResult { Error = ExtractError(responseString) };
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var dataArray) && dataArray.GetArrayLength() > 0)
            {
                var first = dataArray[0];
                if (first.TryGetProperty("b64_json", out var b64Prop))
                {
                    var base64 = b64Prop.GetString();
                    if (!string.IsNullOrEmpty(base64))
                    {
                        return new OpenAIImageResult { Bytes = Convert.FromBase64String(base64) };
                    }
                }
            }
            
            return new OpenAIImageResult { Error = "No image data returned from OpenAI." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpenAIImageClient] Exception during Generations.");
            return new OpenAIImageResult { Error = ex.Message };
        }
    }

    private async Task<OpenAIImageResult> GenerateEditAsync(string prompt, IReadOnlyList<string> referenceUrls, string size, CancellationToken cancellationToken)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(_settings.OpenAiImageModel), "model");
            formData.Add(new StringContent(prompt), "prompt");
            formData.Add(new StringContent(size), "size");
            formData.Add(new StringContent(_settings.OpenAiImageQuality), "quality");

            // Download first reference image to send as reference
            var firstUrl = referenceUrls.First();
            var imageBytes = await _httpClient.GetByteArrayAsync(firstUrl, cancellationToken);
            
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png"); // Fallback assumption, OpenAI usually accepts png/jpeg
            formData.Add(imageContent, "image", "reference.png");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/edits")
            {
                Content = formData
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[OpenAIImageClient] Edits failed. Status: {Status}, Response: {Response}", response.StatusCode, responseString);
                return new OpenAIImageResult { Error = ExtractError(responseString) };
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var dataArray) && dataArray.GetArrayLength() > 0)
            {
                var first = dataArray[0];
                if (first.TryGetProperty("b64_json", out var b64Prop))
                {
                    var base64 = b64Prop.GetString();
                    if (!string.IsNullOrEmpty(base64))
                    {
                        return new OpenAIImageResult { Bytes = Convert.FromBase64String(base64) };
                    }
                }
            }

            return new OpenAIImageResult { Error = "No image data returned from OpenAI Edits." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpenAIImageClient] Exception during Edits.");
            return new OpenAIImageResult { Error = ex.Message };
        }
    }

    private string ExtractError(string responseString)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("error", out var errorObj) && errorObj.TryGetProperty("message", out var msgProp))
            {
                return msgProp.GetString() ?? "Unknown OpenAI Error";
            }
        }
        catch
        {
            // Ignore parse errors for error extraction
        }
        return "Unknown error from OpenAI. Response: " + responseString;
    }
}
