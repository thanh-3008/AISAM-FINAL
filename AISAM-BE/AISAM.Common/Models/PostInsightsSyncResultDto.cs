namespace AISAM.Common.Models;

public sealed class PostInsightsSyncResultDto
{
    public int ProcessedCount { get; set; }
    public int SyncedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
