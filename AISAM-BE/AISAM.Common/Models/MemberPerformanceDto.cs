namespace AISAM.Common.Models;

public sealed class MemberPerformanceItemDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TotalContentCreated { get; set; }
    public int TotalContentParticipated { get; set; }
    public int TotalPosts { get; set; }
    public int DraftCount { get; set; }
    public int InReviewCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PublishedCount { get; set; }
    public int RejectedCount { get; set; }
    public long TotalImpressions { get; set; }
    public long TotalEngagement { get; set; }
    public long TotalClicks { get; set; }
    public decimal EngagementRate { get; set; }
    public DateTime? LatestActivityAt { get; set; }
}

public sealed class MemberPerformanceResponseDto
{
    public IReadOnlyList<MemberPerformanceItemDto> Members { get; set; } = [];
    public DateRangeDto DateRange { get; set; } = new();
    public int TotalMembers { get; set; }
}
