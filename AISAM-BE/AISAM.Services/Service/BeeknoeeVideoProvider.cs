using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

/// <summary>
/// IAIVideoProvider implementation cho Beeknoee Video API.
///
/// Convention JobId: "beeknoee:{beeknoeeJobId}" — tương tự "deapi:{opName}" của DeAPI.
/// Hệ thống dùng prefix này để VideoGenerationBackgroundService route đúng provider.
///
/// Luồng:
///   StartVideoGenerationAsync → POST /v1/video/generations → trả Queued("beeknoee:{id}")
///   CheckStatusAsync("beeknoee:{id}") → GET /v1/video/generations/{id} → map status
/// </summary>
public sealed class BeeknoeeVideoProvider : IAIVideoProvider
{
    private readonly BeeknoeeVideoClient _client;
    private readonly BeeknoeeSettings _settings;
    private readonly ILogger<BeeknoeeVideoProvider> _logger;

    public string ProviderName => "Beeknoee";

    public BeeknoeeVideoProvider(
        BeeknoeeVideoClient client,
        IOptions<BeeknoeeSettings> options,
        ILogger<BeeknoeeVideoProvider> logger)
    {
        _client = client;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> StartVideoGenerationAsync(
        string prompt,
        VideoGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Map VideoGenerationOptions → BeeknoeeVideoRequest
        var aspectRatio = options?.AspectRatio ?? "16:9";
        // Lấy duration từ request (FE), nếu không có hoặc <= 0 thì lấy default
        var duration = options != null && options.DurationSeconds > 0
            ? options.DurationSeconds.ToString()
            : _settings.DefaultVideoDuration.ToString();

        // Normalize aspect ratio (FE có thể gửi "9:16", "16:9", "1:1")
        // Beeknoee chấp nhận đúng format "16:9" / "9:16"
        var request = new BeeknoeeVideoRequest
        {
            Model = _settings.DefaultVideoModel,
            Prompt = prompt,
            AspectRatio = aspectRatio,
            Duration = duration,
            Resolution = _settings.DefaultVideoResolution,
            PersonGeneration = "allow_adult",
            // Image-to-video: nếu FE truyền FirstFrameImageUrl dưới dạng base64 (data URI)
            // ta lấy phần base64 thuần; nếu là URL thì Beeknoee không hỗ trợ trực tiếp.
            Image = ExtractBase64FromFirstFrame(options?.FirstFrameImageUrl)
        };

        _logger.LogInformation(
            "[BeeknoeeVideoProvider] Starting video. Model={Model} | Duration={Duration}s | Res={Res} | AR={AR} | HasImage={HasImg}",
            request.Model, request.Duration, request.Resolution, request.AspectRatio, request.Image != null);

        var result = await _client.StartAsync(request, cancellationToken);

        if (result.IsFailed || string.IsNullOrWhiteSpace(result.JobId))
        {
            var error = result.ErrorMessage ?? "Beeknoee trả về job_id rỗng.";
            _logger.LogError("[BeeknoeeVideoProvider] Start FAILED: {Error}", error);
            return VideoGenerationResult.Fail(error, ProviderName);
        }

        // Prefix "beeknoee:" để VideoGenerationBackgroundService nhận biết
        var externalJobId = "beeknoee:" + result.JobId;
        _logger.LogInformation(
            "[BeeknoeeVideoProvider] ✅ Job queued. ExternalJobId={JobId} | EstTime={Est}s",
            externalJobId, result.EstimatedTimeSeconds);

        return VideoGenerationResult.Queued(externalJobId, $"Beeknoee/{request.Model}");
    }

    public async Task<VideoGenerationResult> CheckStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return VideoGenerationResult.Fail("JobId rỗng.", ProviderName);

        // Strip prefix "beeknoee:"
        const string prefix = "beeknoee:";
        var beeknoeeId = jobId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? jobId[prefix.Length..]
            : jobId;

        var status = await _client.GetStatusAsync(beeknoeeId, cancellationToken);

        if (status.IsSuccess)
        {
            // Ưu tiên source_url (public URL, không cần auth).
            // Nếu null, dùng full proxy URL — BeeknoeeVideoDownloadUrl để
            // VideoGenerationBackgroundService nhận biết cần dùng auth header.
            var mediaUrl = !string.IsNullOrWhiteSpace(status.SourceUrl)
                ? status.SourceUrl
                : BuildProxyUrl(beeknoeeId);

            _logger.LogInformation(
                "[BeeknoeeVideoProvider] ✅ COMPLETED. Cost={Cost}đ | MediaUrl={Url}",
                status.CostVnd, mediaUrl);

            return VideoGenerationResult.Done(mediaUrl, $"Beeknoee/{status.Model ?? _settings.DefaultVideoModel}");
        }

        if (status.IsFailed)
        {
            _logger.LogWarning(
                "[BeeknoeeVideoProvider] ❌ FAILED/TIMEOUT. Error={Error}", status.ErrorMessage);
            return VideoGenerationResult.Fail(
                status.ErrorMessage ?? "Beeknoee video generation thất bại.",
                ProviderName);
        }

        // PENDING / PROCESSING — tiếp tục poll
        _logger.LogDebug(
            "[BeeknoeeVideoProvider] Still {Status} (poll #{Poll}). Elapsed={Elapsed}s.",
            status.Status, status.PollCount ?? 0, status.ElapsedSeconds ?? 0);

        return VideoGenerationResult.InProgress("beeknoee:" + beeknoeeId, ProviderName);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Xây dựng proxy URL đầy đủ để download video (cần Authorization header).
    /// VideoGenerationBackgroundService nhận dạng URL này qua prefix base URL Beeknoee.
    /// </summary>
    private string BuildProxyUrl(string beeknoeeId) =>
        $"{_settings.BaseUrl.TrimEnd('/')}/v1/video/generations/{beeknoeeId}/download";

    /// <summary>
    /// Trích base64 thuần từ data URI (data:video/mp4;base64,XXXX → XXXX).
    /// Nếu input là HTTP URL hoặc null → trả null (Beeknoee không nhận HTTP URL cho image field).
    /// </summary>
    private static string? ExtractBase64FromFirstFrame(string? firstFrameImageUrl)
    {
        if (string.IsNullOrWhiteSpace(firstFrameImageUrl)) return null;
        if (!firstFrameImageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) return null;

        var commaIdx = firstFrameImageUrl.IndexOf(',');
        if (commaIdx < 0 || commaIdx + 1 >= firstFrameImageUrl.Length) return null;

        return firstFrameImageUrl[(commaIdx + 1)..];
    }
}
