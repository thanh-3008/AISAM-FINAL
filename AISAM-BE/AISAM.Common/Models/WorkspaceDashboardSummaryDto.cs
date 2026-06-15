namespace AISAM.Common.Models;

public sealed class WorkspaceDashboardSummaryDto
{
    public Guid WorkspaceId { get; set; }
    public long CreditBalance { get; set; }
    public long CreditsUsed { get; set; }
    public int PublishedPostCount { get; set; }
    public int PostQuotaLimit { get; set; }
    public int PostsRemaining { get; set; }
    public int AiUsageCount { get; set; }
    public int ActiveMemberCount { get; set; }
    public IReadOnlyList<WorkspaceTopMemberDto> TopMembers { get; set; } = [];
}

public sealed class WorkspaceTopMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long CreditsUsed { get; set; }
    public int AiUsageCount { get; set; }
}
