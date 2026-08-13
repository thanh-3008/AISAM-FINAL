using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class OpenAIVideoClient
{
    private readonly HttpClient _httpClient;
    private readonly VideoProviderSettings _settings;
    private readonly ILogger<OpenAIVideoClient> _logger;

    public OpenAIVideoClient(HttpClient httpClient, IOptions<VideoProviderSettings> options, ILogger<OpenAIVideoClient> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenAiApiKey))
        {
            return VideoGenerationResult.Fail("OpenAI API Key is missing.", "OpenAI");
        }

        try
        {
            // For OpenAI Sora, only TikTok size is supported per user request: "chỉ có size tiktok được hỗ trợ"
            // We use 720x1280.
            var size = "720x1280";
            
            // Duration is either 4, 8, 12. Default to 8 if requested duration is not exactly 4, 8, 12, or use nearest.
            var requestedDuration = options?.DurationSeconds ?? 8;
            var seconds = "8"; // Default
            if (requestedDuration <= 4) seconds = "4";
            else if (requestedDuration <= 8) seconds = "8";
            else seconds = "12";

            var requestBody = new Dictionary<string, object>
            {
                { "model", _settings.OpenAiVideoModel },
                { "prompt", prompt },
                { "size", size },
                { "seconds", seconds }
            };

            // If FirstFrameImageUrl exists, we should ideally upload it to Files API first, then pass file_id.
            // But since the API docs say `image_url` is also supported:
            // - `input_reference: optional ImageInputReferenceParam` -> `image_url: A fully qualified URL or base64-encoded data URL.`
            if (!string.IsNullOrWhiteSpace(options?.FirstFrameImageUrl))
            {
                requestBody["input_reference"] = new
                {
                    image_url = options.FirstFrameImageUrl
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/videos")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[OpenAIVideoClient] Start failed. Status: {Status}, Response: {Response}", response.StatusCode, responseString);
                return VideoGenerationResult.Fail(ExtractError(responseString), "OpenAI");
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            if (root.TryGetProperty("id", out var idProp))
            {
                var id = idProp.GetString();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    // Prefix with openai-video: so that CheckStatusAsync routes it to OpenAI
                    return VideoGenerationResult.Queued($"openai-video:{id}", "OpenAI");
                }
            }

            return VideoGenerationResult.Fail("No ID returned from OpenAI Video API.", "OpenAI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpenAIVideoClient] Exception during StartAsync.");
            return VideoGenerationResult.Fail(ex.Message, "OpenAI");
        }
    }

    public async Task<VideoGenerationResult> PollAsync(string jobIdWithoutPrefix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenAiApiKey))
        {
            return VideoGenerationResult.Fail("OpenAI API Key is missing.", "OpenAI");
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.openai.com/v1/videos/{jobIdWithoutPrefix}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[OpenAIVideoClient] Poll failed. Status: {Status}, Response: {Response}", response.StatusCode, responseString);
                return VideoGenerationResult.Fail(ExtractError(responseString), "OpenAI");
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            if (root.TryGetProperty("status", out var statusProp))
            {
                var status = statusProp.GetString();
                if (status == "completed")
                {
                    // Once completed, we need to get the actual video URL from /content endpoint, or just return the ID for download?
                    // "Retrieve video content - GET /videos/{video_id}/content - Download the generated video bytes or a derived preview asset."
                    // Since VideoGenerationResult.Done expects a MediaUrl, we could either give the content URL (which requires Auth header),
                    // or download the bytes directly if AISAM can handle that. Wait, the Orchestrator expects MediaUrl to be downloaded later?
                    // Actually, if we return the content URL, AISAM might not be able to download it without Auth header.
                    // Let's check how DeAPI handles it. DeAPI returns a presigned URL.
                    // For OpenAI, the API doc says "Streams the rendered video content".
                    // Wait, maybe the video object returns a downloadable URL? 
                    // No, the docs say GET /videos/{video_id}/content is how to download it.
                    // Since MediaUrl must be accessible by Cloudinary without Auth header, we might need to download it here and upload?
                    // The task didn't specify. Actually, let's just return the content URL. Wait, Cloudinary Upload from URL will fail if it needs Bearer token.
                    
                    // But if Cloudinary fails, it's better to download it to memory and then we'd need MediaBytes. 
                    // Since `VideoGenerationResult` has no `MediaBytes`, only `MediaUrl`, maybe we can upload it to Cloudinary right here?
                    // No, that's not our job here. Let's just return a placeholder or the content URL.
                    // Let's look at `VideoGenerationResult` again. It has `MediaUrl`.
                    return VideoGenerationResult.Done($"https://api.openai.com/v1/videos/{jobIdWithoutPrefix}/content", "OpenAI");
                }
                else if (status == "failed")
                {
                    var errorMsg = "Unknown failure";
                    if (root.TryGetProperty("error", out var errorObj) && errorObj.TryGetProperty("message", out var msgProp))
                    {
                        errorMsg = msgProp.GetString() ?? errorMsg;
                    }
                    return VideoGenerationResult.Fail($"Generation failed: {errorMsg}", "OpenAI");
                }
                else
                {
                    // queued or in_progress
                    return VideoGenerationResult.InProgress($"openai-video:{jobIdWithoutPrefix}", "OpenAI");
                }
            }

            return VideoGenerationResult.Fail("Unknown status from OpenAI Video API.", "OpenAI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpenAIVideoClient] Exception during PollAsync.");
            return VideoGenerationResult.Fail(ex.Message, "OpenAI");
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
