using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Services.Access;

public static class WorkspaceHrV2Policy
{
    public static bool CanInvite(WorkspaceMember? actor, WorkspaceRoleV2? requested) =>
        actor?.IsActive==true && (requested==WorkspaceRoleV2.Member && actor.WorkspaceRoleV2 is WorkspaceRoleV2.Owner or WorkspaceRoleV2.WorkspaceManager ||
        requested==WorkspaceRoleV2.WorkspaceManager && actor.WorkspaceRoleV2==WorkspaceRoleV2.Owner);

    public static bool CanRemove(WorkspaceMember? actor,WorkspaceMember target) =>
        actor?.IsActive==true && target.IsActive && actor.WorkspaceId==target.WorkspaceId &&
        target.WorkspaceRoleV2 is WorkspaceRoleV2.Member or WorkspaceRoleV2.WorkspaceManager &&
        (actor.WorkspaceRoleV2==WorkspaceRoleV2.Owner || actor.WorkspaceRoleV2==WorkspaceRoleV2.WorkspaceManager && target.WorkspaceRoleV2==WorkspaceRoleV2.Member);

    public static bool CanChangeRole(WorkspaceMember? actor,WorkspaceMember target,WorkspaceRoleV2? requested) =>
        actor?.IsActive==true && actor.WorkspaceId==target.WorkspaceId && actor.WorkspaceRoleV2==WorkspaceRoleV2.Owner && target.IsActive &&
        target.WorkspaceRoleV2 is WorkspaceRoleV2.Member or WorkspaceRoleV2.WorkspaceManager && requested is WorkspaceRoleV2.Member or WorkspaceRoleV2.WorkspaceManager;
}
