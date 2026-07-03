using System.Net.Http.Json;
using System.Text.Json;
using AISAM.Common.Models;
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

    public async Task<(byte[]? Bytes, string? Url, string? Error)> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenRouterApiKey))
            return (null, null, "OpenRouter API Key is missing.");

        var model = _settings.OpenRouterModel ?? "black-forest-labs/FLUX-1.1-pro";
        var url = _settings.OpenRouterBaseUrl ?? "https://openrouter.ai/api/v1/images";

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
}
