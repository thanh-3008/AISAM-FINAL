using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    public IQueryable<WorkspaceAuditLogResponseDto> AuditLogsForRead(Guid workspaceId)
    {
        if (!AccessScope.Enforced || workspaceId != AccessScope.WorkspaceId)
            throw new UnauthorizedAccessException("A current workspace scope is required.");
        if (!AccessScope.IsOwner && AccessScope.Role != WorkspaceMemberRoleEnum.Manager)
            throw new UnauthorizedAccessException("Audit logs require Owner or Manager access.");

        return AuditLogs.AsNoTracking()
            .Where(log =>
                log.WorkspaceId == workspaceId &&
                (AccessScope.IsOwner ||
                    log.TeamId.HasValue && AccessScope.TeamIds.Contains(log.TeamId.Value)))
            .Select(log => new WorkspaceAuditLogResponseDto
            {
                Id = log.Id,
                ActorId = log.ActorId,
                TeamId = log.TeamId,
                AffectedUserId = log.AffectedUserId,
                RequestedBy = log.RequestedBy,
                ApprovedBy = log.ApprovedBy,
                ExecutedBySystem = log.ExecutedBySystem,
                ReferenceId = log.ReferenceId,
                ActionType = log.ActionType,
                TargetTable = log.TargetTable,
                TargetId = log.TargetId,
                CreatedAt = log.CreatedAt
            });
    }
}
