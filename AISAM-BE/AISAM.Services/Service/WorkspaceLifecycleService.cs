using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class WorkspaceLifecycleService : IWorkspaceLifecycleService
{
    public WorkspaceLifecycleState ResolveState(Workspace workspace, DateTime? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        if (workspace.Status == WorkspaceStatusEnum.Deleted || workspace.DeletedAt.HasValue)
        {
            return WorkspaceLifecycleState.Deleted;
        }

        if (!workspace.SubscriptionExpiredAt.HasValue)
        {
            return workspace.Status switch
            {
                WorkspaceStatusEnum.Limited => WorkspaceLifecycleState.Limited,
                WorkspaceStatusEnum.Archived => WorkspaceLifecycleState.Archived,
                WorkspaceStatusEnum.EligibleForDeletion => WorkspaceLifecycleState.EligibleForAdminDeletion,
                _ when workspace.ArchivedAt.HasValue => WorkspaceLifecycleState.Archived,
                _ => WorkspaceLifecycleState.Active
            };
        }

        var now = (utcNow ?? DateTime.UtcNow).Date;
        var expiredAt = workspace.SubscriptionExpiredAt.Value.Date;
        if (now < expiredAt)
        {
            return workspace.Status == WorkspaceStatusEnum.Archived || workspace.ArchivedAt.HasValue
                ? WorkspaceLifecycleState.Archived
                : WorkspaceLifecycleState.Active;
        }

        var elapsedDays = (now - expiredAt).Days;
        var isArchivedPersisted = workspace.Status == WorkspaceStatusEnum.Archived || workspace.ArchivedAt.HasValue;
        if (isArchivedPersisted && elapsedDays <= 180)
        {
            return WorkspaceLifecycleState.Archived;
        }

        if (elapsedDays < 90)
        {
            return WorkspaceLifecycleState.Limited;
        }

        if (elapsedDays <= 180)
        {
            return WorkspaceLifecycleState.Archived;
        }

        return WorkspaceLifecycleState.EligibleForAdminDeletion;
    }

    public bool TrySynchronizePersistenceState(Workspace workspace, DateTime? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        var runtimeState = ResolveState(workspace, utcNow);
        var changed = false;
        var now = (utcNow ?? DateTime.UtcNow).Date;

        var targetStatus = runtimeState switch
        {
            WorkspaceLifecycleState.Active => WorkspaceStatusEnum.Active,
            WorkspaceLifecycleState.Limited => WorkspaceStatusEnum.Limited,
            WorkspaceLifecycleState.Archived => WorkspaceStatusEnum.Archived,
            WorkspaceLifecycleState.EligibleForAdminDeletion => WorkspaceStatusEnum.EligibleForDeletion,
            WorkspaceLifecycleState.Deleted => WorkspaceStatusEnum.Deleted,
            _ => workspace.Status
        };

        if (workspace.Status != targetStatus)
        {
            workspace.Status = targetStatus;
            changed = true;
        }

        if (runtimeState is WorkspaceLifecycleState.Archived or WorkspaceLifecycleState.EligibleForAdminDeletion &&
            !workspace.ArchivedAt.HasValue)
        {
            workspace.ArchivedAt = now;
            changed = true;
        }

        return changed;
    }
}
