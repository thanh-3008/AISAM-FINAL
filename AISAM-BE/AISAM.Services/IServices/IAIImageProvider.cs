namespace AISAM.Services.IServices;

public class ImageGenerationOptions
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
}

public class AIMediaResult
{
    public bool Success { get; set; }
    public byte[]? MediaBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public string ProviderName { get; set; } = string.Empty;

    public static AIMediaResult OkBytes(byte[] bytes, string providerName) => new() { Success = true, MediaBytes = bytes, ProviderName = providerName };
    public static AIMediaResult Fail(string error, string providerName) => new() { Success = false, ErrorMessage = error, ProviderName = providerName };
}

public interface IAIImageProvider
{
    string ProviderName { get; }
    Task<AIMediaResult> GenerateImageAsync(string prompt, ImageGenerationOptions? options = null, CancellationToken cancellationToken = default);
}
