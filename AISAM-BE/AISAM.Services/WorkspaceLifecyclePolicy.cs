using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Services;

public static class WorkspaceLifecyclePolicy
{
    public static bool SynchronizeStatus(Workspace workspace, DateTime utcNow)
    {
        if (workspace.Status == WorkspaceStatusEnum.Deleted ||
            workspace.WorkspaceType != WorkspaceTypeEnum.Business ||
            !workspace.SubscriptionExpiredAt.HasValue)
        {
            return false;
        }

        var expiredAt = NormalizeUtc(workspace.SubscriptionExpiredAt.Value);
        var now = NormalizeUtc(utcNow);
        var nextStatus = ResolveStatus(expiredAt, now);
        if (workspace.Status == nextStatus)
        {
            return false;
        }

        workspace.Status = nextStatus;
        workspace.ArchivedAt = nextStatus is WorkspaceStatusEnum.Archived or WorkspaceStatusEnum.EligibleForDeletion
            ? expiredAt.AddDays(90)
            : null;
        return true;
    }

    public static bool IsReadOnly(WorkspaceStatusEnum status)
    {
        return status is WorkspaceStatusEnum.Limited
            or WorkspaceStatusEnum.Archived
            or WorkspaceStatusEnum.EligibleForDeletion;
    }

    private static WorkspaceStatusEnum ResolveStatus(DateTime expiredAt, DateTime utcNow)
    {
        if (expiredAt >= utcNow)
        {
            return WorkspaceStatusEnum.Active;
        }

        if (expiredAt.AddDays(90) > utcNow)
        {
            return WorkspaceStatusEnum.Limited;
        }

        return expiredAt.AddDays(180) >= utcNow
            ? WorkspaceStatusEnum.Archived
            : WorkspaceStatusEnum.EligibleForDeletion;
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
