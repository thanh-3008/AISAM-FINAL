using AISAM.Data.Enumeration;

namespace AISAM.Services.Access;

public interface IEffectivePermissionContext
{
    Guid UserId { get; }
    Guid WorkspaceId { get; }
    WorkspaceMemberRoleEnum WorkspaceRole { get; }
    HashSet<Guid> AccessibleTeamIds { get; }
    IReadOnlyDictionary<Guid, TeamRoleEnum> BrandMaxRole { get; }

    bool IsOwner { get; }
    bool IsWorkspaceManager { get; }
    TeamRoleEnum? GetEffectiveRoleForBrand(Guid brandId);
    bool HasBrandAccess(Guid brandId);
    bool HasTeamAccess(Guid teamId);
}
