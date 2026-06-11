using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response;

public sealed class WorkspaceInvitationResponseDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public WorkspaceMemberRoleEnum Role { get; set; }
    public MemberQuotaModeEnum QuotaMode { get; set; }
    public long? CreditLimit { get; set; }
    public Guid InvitedByUserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AcceptWorkspaceInvitationResponseDto
{
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public WorkspaceMemberRoleEnum Role { get; set; }
    public MemberQuotaModeEnum QuotaMode { get; set; }
    public long? CreditLimit { get; set; }
}
