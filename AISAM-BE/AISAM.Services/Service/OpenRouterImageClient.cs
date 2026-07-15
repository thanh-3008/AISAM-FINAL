using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class OpenRouterImageClient
{
    private readonly HttpClient _httpClient;
    private readonly ImageProviderSettings _settings;
    private readonly ILogger<OpenRouterImageClient> _logger;

    public OpenRouterImageClient(
        HttpClient httpClient,
        IOptions<ImageProviderSettings> config,
        ILogger<OpenRouterImageClient> logger)
    {
        _httpClient = httpClient;
        _settings = config.Value;
        _logger = logger;
    }

    public async Task<(byte[]? Bytes, string? Url, string? Error)> GenerateAsync(string prompt, ImageGenerationOptions? options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenRouterApiKey))
            return (null, null, "OpenRouter API Key is missing.");

        var model = _settings.OpenRouterModel ?? "black-forest-labs/FLUX-1.1-pro";
        var url = _settings.OpenRouterBaseUrl ?? "https://openrouter.ai/api/v1/images";
        var referenceImageUrls = options?.ReferenceImageUrls?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList() ?? new List<string>();

        if (referenceImageUrls.Count > 0 && IsDeApiUrl(url))
        {
            return await GenerateEditAsync(prompt, referenceImageUrls, options, cancellationToken);
        }

        var payload = new Dictionary<string, object>
        {
            { "model", model },
            { "prompt", prompt }
        };

        if (url.Contains("deapi.ai"))
        {
            payload["width"] = 1024;
            payload["height"] = 1024;
            payload["steps"] = 4; // Flux-schnell requires 4 steps max
            payload["seed"] = Random.Shared.Next(1, 99999999);
            payload["negative_prompt"] = "";
        }
        else
        {
            payload["n"] = 1;
            payload["size"] = "1024x1024";
            payload["response_format"] = "b64_json";
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
                return (null, null, $"HTTP {(int)response.StatusCode}: {errorBody}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);

            string? reqId = null;
            if (document.RootElement.TryGetProperty("request_id", out var reqIdElement)) reqId = reqIdElement.GetString();
            else if (document.RootElement.TryGetProperty("data", out var dataObj) && dataObj.ValueKind == JsonValueKind.Object)
            {
                if (dataObj.TryGetProperty("request_id", out var reqIdElement2)) reqId = reqIdElement2.GetString();
            }

            if (!string.IsNullOrEmpty(reqId))
            {
                var pollUrl = $"https://api.deapi.ai/api/v2/jobs/{reqId}";
                string lastPollResponse = "No response received";
                for (int i = 0; i < 30; i++)
                {
                    await Task.Delay(2000, cancellationToken);
                    var pollRequest = new HttpRequestMessage(HttpMethod.Get, pollUrl);
                    pollRequest.Headers.Add("Authorization", $"Bearer {_settings.OpenRouterApiKey}");
                    var pollResponse = await _httpClient.SendAsync(pollRequest, cancellationToken);
                    var pollJson = await pollResponse.Content.ReadAsStringAsync(cancellationToken);
                    lastPollResponse = $"HTTP {(int)pollResponse.StatusCode}: {pollJson}";
                    
                    if (pollResponse.IsSuccessStatusCode)
                    {
                        try
                        {
                            using var pollDoc = JsonDocument.Parse(pollJson);
                            var root = pollDoc.RootElement;
                            var targetNode = root;
                            if (root.TryGetProperty("data", out var dataNode) && dataNode.ValueKind == JsonValueKind.Object)
                            {
                                targetNode = dataNode;
                            }

                            string? status = null;
                            if (root.TryGetProperty("status", out var rootStatusEl) && rootStatusEl.ValueKind == JsonValueKind.String)
                            {
                                status = rootStatusEl.GetString()?.ToLower();
                            }
                            else if (targetNode.TryGetProperty("status", out var statusEl) && statusEl.ValueKind == JsonValueKind.String)
                            {
                                status = statusEl.GetString()?.ToLower();
                            }

                            if (!string.IsNullOrEmpty(status))
                            {
                                if (status == "completed" || status == "success" || status == "done")
                                {
                                    if (targetNode.TryGetProperty("result_url", out var resultUrl)) return (null, resultUrl.GetString(), null);
                                    
                                    if (targetNode.TryGetProperty("output", out var outputEl) && outputEl.ValueKind == JsonValueKind.Object)
                                    {
                                        if (outputEl.TryGetProperty("image_url", out var imgUrlEl)) return (null, imgUrlEl.GetString(), null);
                                        if (outputEl.TryGetProperty("url", out var outputUrlEl)) return (null, outputUrlEl.GetString(), null);
                                    }
                                    if (targetNode.TryGetProperty("output", out var outputStr) && outputStr.ValueKind == JsonValueKind.String)
                                    {
                                        return (null, outputStr.GetString(), null);
                                    }
                                    if (targetNode.TryGetProperty("image_url", out var directImg)) return (null, directImg.GetString(), null);
                                    if (targetNode.TryGetProperty("url", out var pollDirectUrl)) return (null, pollDirectUrl.GetString(), null);
                                    
                                    // if output is an array
                                    if (targetNode.TryGetProperty("output", out var outArr) && outArr.ValueKind == JsonValueKind.Array && outArr.GetArrayLength() > 0)
                                    {
                                        if (outArr[0].ValueKind == JsonValueKind.String) return (null, outArr[0].GetString(), null);
                                    }

                                    return (null, null, "DeAPI completed but could not find image URL. JSON: " + pollJson);
                                }
                                else if (status == "failed" || status == "error")
                                {
                                    return (null, null, "DeAPI task failed. JSON: " + pollJson);
                                }
                                // If status is processing/pending, it will just loop again
                            }
                        }
                        catch
                        {
                            // ignore json parse error, just loop
                        }
                    }
                }
                return (null, null, $"DeAPI polling timed out. Last response: {lastPollResponse}");
            }

            if (document.RootElement.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array && dataArray.GetArrayLength() > 0)
            {
                var item = dataArray[0];
                if (item.TryGetProperty("b64_json", out var b64Element) && b64Element.ValueKind == JsonValueKind.String)
                {
                    var b64 = b64Element.GetString();
                    if (!string.IsNullOrEmpty(b64)) return (Convert.FromBase64String(b64), null, null);
                }
                if (item.TryGetProperty("url", out var urlElement)) return (null, urlElement.GetString(), null);
            }

            // DeAPI V2 response format (usually flat object or data object containing URLs)
            if (document.RootElement.TryGetProperty("url", out var directUrl)) return (null, directUrl.GetString(), null);
            if (document.RootElement.TryGetProperty("image_url", out var imgUrl)) return (null, imgUrl.GetString(), null);
            if (document.RootElement.TryGetProperty("data", out var dataObj2) && dataObj2.ValueKind == JsonValueKind.Object)
            {
                if (dataObj2.TryGetProperty("url", out var dataUrl)) return (null, dataUrl.GetString(), null);
            }
            if (document.RootElement.TryGetProperty("images", out var imagesArr) && imagesArr.ValueKind == JsonValueKind.Array && imagesArr.GetArrayLength() > 0)
            {
                var firstImage = imagesArr[0];
                if (firstImage.ValueKind == JsonValueKind.String) return (null, firstImage.GetString(), null);
                if (firstImage.TryGetProperty("url", out var firstUrl)) return (null, firstUrl.GetString(), null);
            }

            return (null, null, "Invalid response format from Image API: " + json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenRouter image generation exception.");
            return (null, null, ex.Message);
        }
    }

    private async Task<(byte[]? Bytes, string? Url, string? Error)> GenerateEditAsync(
        string prompt,
        IReadOnlyList<string> referenceImageUrls,
        ImageGenerationOptions? options,
        CancellationToken cancellationToken)
    {
        var editUrl = BuildEditUrl();
        var model = string.IsNullOrWhiteSpace(_settings.OpenRouterEditModel)
            ? "QwenImageEdit_Plus_NF4"
            : _settings.OpenRouterEditModel;
        var cleanPrompt = CleanPromptForImageEdit(prompt, referenceImageUrls);

        try
        {
            _logger.LogInformation(
                "Starting deAPI image edit. Url={Url}, Model={Model}, ReferenceCount={ReferenceCount}",
                editUrl,
                model,
                referenceImageUrls.Count);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(cleanPrompt), "prompt");
            form.Add(new StringContent(model), "model");
            form.Add(new StringContent("20"), "steps");
            form.Add(new StringContent(Random.Shared.Next(1, 99999999).ToString(System.Globalization.CultureInfo.InvariantCulture)), "seed");
            form.Add(new StringContent(""), "negative_prompt");
            form.Add(new StringContent((options?.Width ?? 1024).ToString(System.Globalization.CultureInfo.InvariantCulture)), "width");
            form.Add(new StringContent((options?.Height ?? 1024).ToString(System.Globalization.CultureInfo.InvariantCulture)), "height");

            for (var index = 0; index < referenceImageUrls.Count; index++)
            {
                var imageUrl = referenceImageUrls[index];
                var bytes = await _httpClient.GetByteArrayAsync(imageUrl, cancellationToken);
                if (bytes.Length == 0)
                {
                    continue;
                }

                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetContentType(imageUrl));
                var fieldName = referenceImageUrls.Count == 1 ? "image" : "images[]";
                form.Add(content, fieldName, $"reference-{index + 1}{GetFileExtension(imageUrl)}");
            }

            if (!form.Any(part => part.Headers.ContentDisposition?.Name?.Trim('"') is "image" or "images[]"))
            {
                return (null, null, "No reference image could be downloaded for image-to-image generation.");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, editUrl);
            request.Headers.Add("Authorization", $"Bearer {_settings.OpenRouterApiKey}");
            request.Headers.Accept.ParseAdd(MediaTypeNames.Application.Json);
            request.Content = form;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "deAPI image edit failed. Status={StatusCode}, Body={Body}",
                    (int)response.StatusCode,
                    errorBody);
                return (null, null, $"Image edit HTTP {(int)response.StatusCode}: {errorBody}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var requestId = ExtractRequestId(document.RootElement);
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return ExtractImageResult(document.RootElement, json);
            }

            return await PollDeApiJobAsync(requestId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenRouter/deAPI image edit exception.");
            return (null, null, ex.Message);
        }
    }

    private async Task<(byte[]? Bytes, string? Url, string? Error)> PollDeApiJobAsync(string requestId, CancellationToken cancellationToken)
    {
        var pollUrl = $"https://api.deapi.ai/api/v2/jobs/{requestId}";
        string lastPollResponse = "No response received";
        var pollingInterval = TimeSpan.FromSeconds(Math.Clamp(_settings.OpenRouterEditPollingIntervalSeconds, 1, 30));
        var timeoutAt = DateTimeOffset.UtcNow.AddMinutes(Math.Clamp(_settings.OpenRouterEditTimeoutMinutes, 1, 30));

        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            await Task.Delay(pollingInterval, cancellationToken);
            var pollRequest = new HttpRequestMessage(HttpMethod.Get, pollUrl);
            pollRequest.Headers.Add("Authorization", $"Bearer {_settings.OpenRouterApiKey}");
            var pollResponse = await _httpClient.SendAsync(pollRequest, cancellationToken);
            var pollJson = await pollResponse.Content.ReadAsStringAsync(cancellationToken);
            lastPollResponse = $"HTTP {(int)pollResponse.StatusCode}: {pollJson}";

            if (!pollResponse.IsSuccessStatusCode)
            {
                continue;
            }

            try
            {
                using var pollDoc = JsonDocument.Parse(pollJson);
                var root = pollDoc.RootElement;
                var targetNode = root.TryGetProperty("data", out var dataNode) && dataNode.ValueKind == JsonValueKind.Object
                    ? dataNode
                    : root;
                var immediateResult = ExtractImageResult(targetNode, pollJson);
                if (!string.IsNullOrWhiteSpace(immediateResult.Url) || immediateResult.Bytes != null)
                {
                    return immediateResult;
                }

                var status = ExtractStatus(root, targetNode);
                if (status is "completed" or "success" or "done")
                {
                    return ExtractImageResult(targetNode, pollJson);
                }

                if (status is "failed" or "error")
                {
                    return (null, null, "DeAPI task failed. JSON: " + pollJson);
                }
            }
            catch
            {
                // Ignore transient JSON parse issues while polling.
            }
        }

        _logger.LogWarning("DeAPI image edit polling timed out. Last response: {LastPollResponse}", lastPollResponse);
        return (null, null, $"DeAPI image edit is still processing after {_settings.OpenRouterEditTimeoutMinutes} minute(s). Please try again later or increase IMAGE_OPENROUTER_EDIT_TIMEOUT_MINUTES.");
    }

    private string BuildEditUrl()
    {
        if (!string.IsNullOrWhiteSpace(_settings.OpenRouterEditBaseUrl))
        {
            return _settings.OpenRouterEditBaseUrl;
        }

        var generationUrl = _settings.OpenRouterBaseUrl ?? "https://api.deapi.ai/api/v2/images/generations";
        if (generationUrl.Contains("/images/generations", StringComparison.OrdinalIgnoreCase))
        {
            return generationUrl.Replace("/images/generations", "/images/edits", StringComparison.OrdinalIgnoreCase);
        }

        return "https://api.deapi.ai/api/v2/images/edits";
    }

    private static bool IsDeApiUrl(string url) =>
        url.Contains("deapi.ai", StringComparison.OrdinalIgnoreCase);

    private static string CleanPromptForImageEdit(string prompt, IEnumerable<string> referenceImageUrls)
    {
        var cleaned = prompt;
        foreach (var referenceImageUrl in referenceImageUrls)
        {
            cleaned = cleaned.Replace(referenceImageUrl, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return cleaned
            .Replace("Use the product shown in this reference image as the exact subject.", "Use the product in the uploaded reference image as the exact subject.", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string? ExtractRequestId(JsonElement root)
    {
        if (root.TryGetProperty("request_id", out var requestIdElement) &&
            requestIdElement.ValueKind == JsonValueKind.String)
        {
            return requestIdElement.GetString();
        }

        if (root.TryGetProperty("data", out var dataElement) &&
            dataElement.ValueKind == JsonValueKind.Object &&
            dataElement.TryGetProperty("request_id", out var dataRequestIdElement) &&
            dataRequestIdElement.ValueKind == JsonValueKind.String)
        {
            return dataRequestIdElement.GetString();
        }

        return null;
    }

    private static string? ExtractStatus(JsonElement root, JsonElement targetNode)
    {
        if (root.TryGetProperty("status", out var rootStatusElement) &&
            rootStatusElement.ValueKind == JsonValueKind.String)
        {
            return rootStatusElement.GetString()?.ToLowerInvariant();
        }

        if (targetNode.TryGetProperty("status", out var statusElement) &&
            statusElement.ValueKind == JsonValueKind.String)
        {
            return statusElement.GetString()?.ToLowerInvariant();
        }

        return null;
    }

    private static (byte[]? Bytes, string? Url, string? Error) ExtractImageResult(JsonElement targetNode, string json)
    {
        if (targetNode.TryGetProperty("result_url", out var resultUrl) &&
            resultUrl.ValueKind == JsonValueKind.String)
        {
            return (null, resultUrl.GetString(), null);
        }

        if (targetNode.TryGetProperty("output", out var outputElement))
        {
            if (outputElement.ValueKind == JsonValueKind.Object)
            {
                if (outputElement.TryGetProperty("image_url", out var imageUrlElement) &&
                    imageUrlElement.ValueKind == JsonValueKind.String)
                {
                    return (null, imageUrlElement.GetString(), null);
                }

                if (outputElement.TryGetProperty("url", out var outputUrlElement) &&
                    outputUrlElement.ValueKind == JsonValueKind.String)
                {
                    return (null, outputUrlElement.GetString(), null);
                }
            }

            if (outputElement.ValueKind == JsonValueKind.String)
            {
                return (null, outputElement.GetString(), null);
            }

            if (outputElement.ValueKind == JsonValueKind.Array && outputElement.GetArrayLength() > 0)
            {
                var firstOutput = outputElement[0];
                if (firstOutput.ValueKind == JsonValueKind.String)
                {
                    return (null, firstOutput.GetString(), null);
                }

                if (firstOutput.ValueKind == JsonValueKind.Object &&
                    firstOutput.TryGetProperty("url", out var firstOutputUrl) &&
                    firstOutputUrl.ValueKind == JsonValueKind.String)
                {
                    return (null, firstOutputUrl.GetString(), null);
                }
            }
        }

        if (targetNode.TryGetProperty("image_url", out var directImageUrl) &&
            directImageUrl.ValueKind == JsonValueKind.String)
        {
            return (null, directImageUrl.GetString(), null);
        }

        if (targetNode.TryGetProperty("url", out var directUrl) &&
            directUrl.ValueKind == JsonValueKind.String)
        {
            return (null, directUrl.GetString(), null);
        }

        if (targetNode.TryGetProperty("images", out var imagesElement) &&
            imagesElement.ValueKind == JsonValueKind.Array &&
            imagesElement.GetArrayLength() > 0)
        {
            var firstImage = imagesElement[0];
            if (firstImage.ValueKind == JsonValueKind.String)
            {
                return (null, firstImage.GetString(), null);
            }

            if (firstImage.ValueKind == JsonValueKind.Object &&
                firstImage.TryGetProperty("url", out var firstImageUrl) &&
                firstImageUrl.ValueKind == JsonValueKind.String)
            {
                return (null, firstImageUrl.GetString(), null);
            }
        }

        return (null, null, "Image job completed but could not find image URL. JSON: " + json);
    }

    private static string GetContentType(string url)
    {
        var extension = GetFileExtension(url);
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    private static string GetFileExtension(string url)
    {
        try
        {
            var path = new Uri(url).AbsolutePath;
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp"
                ? extension
                : ".jpg";
        }
        catch
        {
            return ".jpg";
        }
    }
}
