namespace AISAM.Common.Models;

/// <summary>
/// Cấu hình Beeknoee AI Media API (provider mới, Phương án 1 — sync-first).
/// Section: "BeeknoeeSettings".
/// Tất cả giá trị nhạy cảm (ApiKey) phải đặt trong biến môi trường BEEKNOEE_*,
/// KHÔNG commit vào appsettings.json hay source code.
/// </summary>
public sealed class BeeknoeeSettings
{
    /// <summary>Base URL của Beeknoee API. Mặc định: https://platform.beeknoee.com</summary>
    public string BaseUrl { get; set; } = "https://platform.beeknoee.com";

    /// <summary>
    /// API key Beeknoee (dạng sk-bee-xxxxxx).
    /// Đọc từ biến môi trường BEEKNOEE_API_KEY.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model ảnh mặc định khi request không chỉ định model.
    /// Đọc từ BEEKNOEE_IMAGE_MODEL. Giá trị demo: "gemini-3-pro-image-preview".
    /// </summary>
    public string DefaultImageModel { get; set; } = "gemini-3-pro-image-preview";

    /// <summary>
    /// Model video mặc định khi request không chỉ định model.
    /// Đọc từ BEEKNOEE_VIDEO_MODEL.
    /// Các lựa chọn: veo-3.1-fast-generate-preview ($0.15/s, max 8s),
    ///              veo-3.1-generate-preview ($0.40/s),
    ///              sora-2 ($0.10/s, max 20s),
    ///              seedance-2-fast ($0.08/s, max 15s).
    /// </summary>
    public string DefaultVideoModel { get; set; } = "veo-3.1-fast-generate-preview";

    /// <summary>
    /// Duration mặc định (giây) cho video Beeknoee.
    /// Đọc từ BEEKNOEE_VIDEO_DURATION. Veo max 8s, Sora max 20s.
    /// </summary>
    public int DefaultVideoDuration { get; set; } = 8;

    /// <summary>
    /// Resolution mặc định cho video Beeknoee (vd: 720p, 1080p).
    /// Đọc từ BEEKNOEE_VIDEO_RESOLUTION.
    /// </summary>
    public string DefaultVideoResolution { get; set; } = "720p";

    /// <summary>
    /// Timeout (giây) cho một HTTP request tới Beeknoee.
    /// Mặc định 120 giây — model ảnh có thể mất thêm thời gian ở server side.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Timeout (giây) cho request poll trạng thái video (GET /v1/video/generations/{id}).
    /// Mặc định 30 giây.
    /// </summary>
    public int VideoTimeoutSeconds { get; set; } = 30;
}

