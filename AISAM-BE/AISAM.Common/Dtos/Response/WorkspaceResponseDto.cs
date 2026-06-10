using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response;

public sealed class WorkspaceResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkspaceTypeEnum WorkspaceType { get; set; }
    public WorkspaceStatusEnum Status { get; set; }
    public WorkspaceMemberRoleEnum CurrentUserRole { get; set; }
    public int ActiveMemberCount { get; set; }
    public DateTime? SubscriptionExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
