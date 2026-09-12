namespace AISAM.Services.Access;

public enum AccessResourceKind { Workspace, Brand, Content, Channel, Post }
// Actor is supplied by authenticated server code, never model-bound from a body.
public sealed record AccessRequest(Guid ActorId, Guid WorkspaceId, AccessResourceKind Kind,
    Guid ResourceId, ResourcePermission Permission, Guid? ChannelId = null, Guid? MemberId = null, bool IncludeDeleted = false);
public sealed record AccessDecision(bool Allowed, int StatusCode, string? ErrorCode)
{
    public static AccessDecision Permit { get; } = new(true, 200, null);
    public static AccessDecision Hidden { get; } = new(false, 404, "RESOURCE_NOT_FOUND");
    public static AccessDecision Denied { get; } = new(false, 403, "ACCESS_DENIED");
}
public interface IAccessControlService
{
    Task<AccessDecision> CheckAsync(AccessRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid actorId, Guid workspaceId, CancellationToken ct = default);
}

// Versioned, exact keys only. Legacy free-form permissions and TeamMember.Role
// are not authoritative. T03 assignment API must validate these keys on writes.
public static class DelegatedPermissionKeys
{
    public const string ViewAllCreators = "aisam.permission.v1.content.view_all_creators";
    public const string Review = "aisam.permission.v1.approval.review";
    public const string Publish = "aisam.permission.v1.post.publish";
    public const string Billing = "billing.manage";
    public const string MemberAnalytics = "analytics.member";
    public const string TeamCreate = "aisam.permission.v1.team.create";
    public const string BrandCreate = "aisam.permission.v1.brand.create";
}
