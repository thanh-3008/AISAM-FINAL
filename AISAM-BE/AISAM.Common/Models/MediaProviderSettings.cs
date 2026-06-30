namespace AISAM.Common.Models;

/// <summary>Cấu hình AI Image Provider. Section: "ImageProviderSettings"</summary>
public sealed class ImageProviderSettings
{
    // === Fallback: Gemini Imagen ===
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "imagen-3.0-generate-002";
    /// <summary>Timeout (giây) trước khi coi Gemini thất bại.</summary>
    public int GeminiTimeoutSeconds { get; set; } = 30;

    public string OpenRouterApiKey { get; set; } = string.Empty;
    public string? OpenRouterBaseUrl { get; set; }
    // Default primary image model (OpenRouter-compatible)
    public string OpenRouterModel { get; set; } = "bytedance-seed/seedream-4.5";
}

/// <summary>Cấu hình AI Video Provider. Section: "VideoProviderSettings"</summary>
public sealed class VideoProviderSettings
{
    // === Fallback: Gemini Veo (Vertex AI) ===
    public string GeminiApiKey { get; set; } = string.Empty;
    // Use Gemini/Vertex video model
    public string GeminiModel { get; set; } = "google/veo-3.1-fast";
    /// <summary>Timeout (giây) trước khi coi Gemini thất bại.</summary>
    public int GeminiTimeoutSeconds { get; set; } = 60;

    // === Primary: OpenRouter ===
    public string OpenRouterApiKey { get; set; } = string.Empty;
    public string? OpenRouterBaseUrl { get; set; }
    public string OpenRouterModel { get; set; } = "minimax/video-01";

    /// <summary>false = tính năng video tắt, trả về "Coming Soon". Đặt true khi có key.</summary>
    public bool Enabled { get; set; } = false;
}
