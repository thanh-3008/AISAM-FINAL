using AISAM.Data.Model;

namespace AISAM.API.Utils;

public static class WorkspaceContextHelper
{
    public const string ActiveWorkspaceItemKey = "ActiveWorkspaceId";
    public const string ActiveWorkspaceMembershipItemKey = "ActiveWorkspaceMembership";
    public const string EffectivePermissionContextItemKey = "EffectivePermissionContext";

    public static Guid GetActiveWorkspaceIdOrThrow(HttpContext context)
    {
        if (context.Items.TryGetValue(ActiveWorkspaceItemKey, out var value) &&
            value is Guid workspaceId)
        {
            return workspaceId;
        }

        throw new InvalidOperationException("Invalid workspace context.");
    }

    public static WorkspaceMember GetActiveWorkspaceMembershipOrThrow(HttpContext context)
    {
        if (context.Items.TryGetValue(ActiveWorkspaceMembershipItemKey, out var value) &&
            value is WorkspaceMember membership)
        {
            return membership;
        }

        throw new InvalidOperationException("Invalid workspace membership context.");
    }

    public static AISAM.Services.Access.IEffectivePermissionContext? GetEffectivePermissionContext(HttpContext context)
    {
        if (context.Items.TryGetValue(EffectivePermissionContextItemKey, out var value) &&
            value is AISAM.Services.Access.IEffectivePermissionContext ctx)
        {
            return ctx;
        }

        return null;
    }
}
