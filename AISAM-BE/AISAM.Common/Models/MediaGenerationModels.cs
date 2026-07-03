namespace AISAM.Common.Models;

public sealed class GenerateImageRequest
{
    public Guid ContentId { get; set; }
    /// <summary>Nếu null, hệ thống sẽ tự build prompt từ nội dung Content.</summary>
    public string? CustomPrompt { get; set; }
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
}

public sealed class GenerateVideoRequest
{
    public Guid ContentId { get; set; }
    public string? CustomPrompt { get; set; }
    public int DurationSeconds { get; set; } = 4;
    public string AspectRatio { get; set; } = "16:9";
}
