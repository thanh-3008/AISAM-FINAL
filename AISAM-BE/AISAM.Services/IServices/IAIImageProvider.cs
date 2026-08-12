namespace AISAM.Services.IServices;

public class ImageGenerationOptions
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    public IReadOnlyList<string> ReferenceImageUrls { get; set; } = Array.Empty<string>();
}

public class AIMediaResult
{
    public bool Success { get; set; }
    public byte[]? MediaBytes { get; set; }
    /// <summary>URL công khai của ảnh (thay thế cho MediaBytes khi provider trả URL).</summary>
    public string? MediaUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// true khi provider trả PROCESSING/PENDING — không có ảnh ngay, cần Phương án 2 để poll.
    /// Caller phải kiểm tra field này trước khi đọc MediaBytes/MediaUrl.
    /// </summary>
    public bool RequiresAsyncHandling { get; set; }

    /// <summary>Job ID từ provider (chỉ có giá trị khi RequiresAsyncHandling = true).</summary>
    public string? JobId { get; set; }

    // ── Factory methods ────────────────────────────────────────────────────────
    public static AIMediaResult OkBytes(byte[] bytes, string providerName) =>
        new() { Success = true, MediaBytes = bytes, ProviderName = providerName };

    public static AIMediaResult OkUrl(string url, string providerName) =>
        new() { Success = true, MediaUrl = url, ProviderName = providerName };

    public static AIMediaResult Fail(string error, string providerName) =>
        new() { Success = false, ErrorMessage = error, ProviderName = providerName };

    /// <summary>
    /// Provider trả PROCESSING/PENDING — PA1 không poll, nhường lại cho PA2.
    /// </summary>
    public static AIMediaResult Async(string jobId, string providerName) =>
        new() { Success = false, RequiresAsyncHandling = true, JobId = jobId, ProviderName = providerName };
}

public interface IAIImageProvider
{
    string ProviderName { get; }
    Task<AIMediaResult> GenerateImageAsync(string prompt, ImageGenerationOptions? options = null, CancellationToken cancellationToken = default);
}
