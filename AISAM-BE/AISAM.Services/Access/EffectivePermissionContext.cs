using AISAM.Data.Enumeration;

namespace AISAM.Services.Access;

public sealed class EffectivePermissionContext : IEffectivePermissionContext
{
    public Guid UserId { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public WorkspaceMemberRoleEnum WorkspaceRole { get; private set; }
    public HashSet<Guid> AccessibleTeamIds { get; private set; } = [];
    public IReadOnlyDictionary<Guid, TeamRoleEnum> BrandMaxRole { get; private set; } = new Dictionary<Guid, TeamRoleEnum>();
    public bool IsInitialized { get; private set; }

    public bool IsOwner => WorkspaceRole == WorkspaceMemberRoleEnum.Owner;
    public bool IsWorkspaceManager => WorkspaceRole == WorkspaceMemberRoleEnum.WorkspaceManager;

    public EffectivePermissionContext()
    {
    }

    public EffectivePermissionContext(
        Guid userId,
        Guid workspaceId,
        WorkspaceMemberRoleEnum workspaceRole,
        HashSet<Guid> accessibleTeamIds,
        IReadOnlyDictionary<Guid, TeamRoleEnum> brandMaxRole)
    {
        Initialize(userId, workspaceId, workspaceRole, accessibleTeamIds, brandMaxRole);
    }

    public void Initialize(
        Guid userId,
        Guid workspaceId,
        WorkspaceMemberRoleEnum workspaceRole,
        HashSet<Guid> accessibleTeamIds,
        IReadOnlyDictionary<Guid, TeamRoleEnum> brandMaxRole)
    {
        UserId = userId;
        WorkspaceId = workspaceId;
        WorkspaceRole = workspaceRole;
        AccessibleTeamIds = accessibleTeamIds ?? [];
        BrandMaxRole = brandMaxRole ?? new Dictionary<Guid, TeamRoleEnum>();
        IsInitialized = true;
    }

    public TeamRoleEnum? GetEffectiveRoleForBrand(Guid brandId)
    {
        if (IsOwner || IsWorkspaceManager)
        {
            return TeamRoleEnum.Manager;
        }

        return BrandMaxRole.TryGetValue(brandId, out var role) ? role : null;
    }

    public bool HasBrandAccess(Guid brandId)
    {
        if (IsOwner || IsWorkspaceManager)
        {
            return true;
        }

        return BrandMaxRole.ContainsKey(brandId);
    }

    public bool HasTeamAccess(Guid teamId)
    {
        if (IsOwner || IsWorkspaceManager)
        {
            return true;
        }

        return AccessibleTeamIds.Contains(teamId);
    }

    public static int GetPrivilegeRank(TeamRoleEnum role) => role switch
    {
        TeamRoleEnum.Manager => 3,
        TeamRoleEnum.ContentCreator => 2,
        TeamRoleEnum.Viewer => 1,
        _ => 0
    };

    public static TeamRoleEnum ResolveMaxRole(IEnumerable<TeamRoleEnum> roles)
    {
        return roles.OrderByDescending(GetPrivilegeRank).FirstOrDefault();
    }
}
