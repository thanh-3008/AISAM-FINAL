namespace AISAM.Common.Models;

/// <summary>Cấu hình AI Image Provider. Section: "ImageProviderSettings"</summary>
public sealed class ImageProviderSettings
{


    public string OpenRouterApiKey { get; set; } = string.Empty;
    public string? OpenRouterBaseUrl { get; set; }
    // Default primary image model (OpenRouter-compatible)
    public string OpenRouterModel { get; set; } = "bytedance-seed/seedream-4.5";
    public string? OpenRouterEditBaseUrl { get; set; }
    public string OpenRouterEditModel { get; set; } = "QwenImageEdit_Plus_NF4";
    public int OpenRouterEditPollingIntervalSeconds { get; set; } = 5;
    public int OpenRouterEditTimeoutMinutes { get; set; } = 10;

    // === Fallback 1: Hugging Face ===
    public string HuggingFaceApiKey { get; set; } = string.Empty;
    public string? HuggingFaceBaseUrl { get; set; } = "https://router.huggingface.co/hf-inference/models/";
    public string HuggingFaceModel { get; set; } = "black-forest-labs/FLUX.1-schnell";
}

/// <summary>Cấu hình AI Video Provider. Section: "VideoProviderSettings"</summary>
public sealed class VideoProviderSettings
{
    // === Primary: Gemini Veo ===
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "veo-2.0-generate-001";
    public int GeminiTimeoutSeconds { get; set; } = 60;

    // === Fallback 1: DeAPI ===
    public string DeApiApiKey { get; set; } = string.Empty;
    public string? DeApiBaseUrl { get; set; } = "https://api.deapi.ai/api/v1";
    public string DeApiModel { get; set; } = "Ltx2_3_22B_Dist_INT8";
    public string? DeApiApiKeyFallback { get; set; }
    public string? DeApiModelFallback { get; set; }



    // === Colab Fallback ===
    public bool EnableColabFallback { get; set; } = true;
    public int PollenTimeoutSeconds { get; set; } = 30; // Primary provider timeout
    public int DefaultSegmentCount { get; set; } = 3;
    public int PollingIntervalSeconds { get; set; } = 15;
    public int TimeoutMinutes { get; set; } = 30;
    public string? ColabBaseUrl { get; set; }
    public string? ColabToken { get; set; }
    public int ColabTimeout { get; set; } = 300;

    /// <summary>false = tính năng video tắt, trả về "Coming Soon". Đặt true khi có key.</summary>
    public bool Enabled { get; set; } = false;
}
