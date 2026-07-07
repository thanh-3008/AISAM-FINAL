namespace AISAM.Services.IServices;

public class VideoGenerationOptions
{
    public int DurationSeconds { get; set; } = 4;
    public string AspectRatio { get; set; } = "9:16";
}

public enum VideoGenerationStatus
{
    Queued,
    Processing,
    Done,
    Failed
}

public class VideoGenerationResult
{
    public bool Success => Status != VideoGenerationStatus.Failed;
    public VideoGenerationStatus Status { get; set; }
    public string? JobId { get; set; }
    public string? MediaUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string ProviderName { get; set; } = string.Empty;

    public static VideoGenerationResult Queued(string jobId, string providerName) => new() { Status = VideoGenerationStatus.Queued, JobId = jobId, ProviderName = providerName };
    public static VideoGenerationResult InProgress(string jobId, string providerName) => new() { Status = VideoGenerationStatus.Processing, JobId = jobId, ProviderName = providerName };
    public static VideoGenerationResult Done(string mediaUrl, string providerName) => new() { Status = VideoGenerationStatus.Done, MediaUrl = mediaUrl, ProviderName = providerName };
    public static VideoGenerationResult Fail(string error, string providerName) => new() { Status = VideoGenerationStatus.Failed, ErrorMessage = error, ProviderName = providerName };
}

public interface IAIVideoProvider
{
    string ProviderName { get; }
    Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default);
    Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default);
}
