namespace AISAM.Common.Models;

public sealed class SchedulerRunResultDto
{
    public int ScannedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}
