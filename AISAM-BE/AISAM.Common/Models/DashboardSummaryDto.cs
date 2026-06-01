namespace AISAM.Common.Models;

public sealed class DashboardSummaryDto
{
    public int DraftContentCount { get; set; }
    public int PublishedContentCount { get; set; }
    public int PendingApprovalContentCount { get; set; }
    public int UpcomingScheduleCount { get; set; }
    public int FailedScheduleCount { get; set; }
    public int ActiveSocialIntegrationCount { get; set; }
    public int PublishedPostCount { get; set; }
    public int UnreadNotificationCount { get; set; }
}
