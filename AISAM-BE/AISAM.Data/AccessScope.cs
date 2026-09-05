using AISAM.Data.Enumeration;

namespace AISAM.Data;

/// <summary>Server-resolved request scope. Never populate allowed IDs from JWT/client claims.</summary>
public sealed class AccessScope
{
    public bool Enforced { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public WorkspaceMemberRoleEnum Role { get; set; }
    public bool IsWrite { get; set; }
    public long PermissionRevision { get; set; }
    public Guid? ActiveTeamId { get; set; }
    public Guid[] TeamIds { get; set; } = [];
    public Guid[] BrandIds { get; set; } = [];
    public Guid[] IntegrationIds { get; set; } = [];
    public Guid[] HistoricalContentIds { get; set; } = [];
    public Guid[] EditableContentIds { get; set; } = [];
    public Guid[] MemberIds { get; set; } = [];
    public Guid[] AnalyticsCampaignIds { get; set; } = [];
    public bool IsOwner => Role == WorkspaceMemberRoleEnum.Owner;
    public bool IsCreator => Role == WorkspaceMemberRoleEnum.ContentCreator;
    public bool CanViewAggregate => Role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager;
    public string Version => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
        System.Text.Encoding.UTF8.GetBytes($"{WorkspaceId}:{UserId}:{Role}:{ActiveTeamId}:{PermissionRevision}:" +
            string.Join(":", new[] { TeamIds, BrandIds, IntegrationIds, HistoricalContentIds, EditableContentIds, MemberIds, AnalyticsCampaignIds }
                .Select(ids => string.Join(",", ids.Order()))))));
}
