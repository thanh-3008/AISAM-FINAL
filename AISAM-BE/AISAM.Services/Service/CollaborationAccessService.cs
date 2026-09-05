using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class CollaborationAccessService(AisamContext db)
{
    public async Task<bool> HasTeamAccessAsync(Guid workspaceId, Guid teamId, Guid userId, Guid contentId, Guid? integrationId, CancellationToken ct)
    {
        var content = await db.Contents.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(c => c.Id == contentId && c.WorkspaceId == workspaceId && !c.IsDeleted, ct);
        if (content == null || !await db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && m.IsActive, ct)) return false;
        var brand = await db.TeamBrands.Include(b => b.Channels).FirstOrDefaultAsync(b => b.TeamId == teamId && b.BrandId == content.BrandId && b.IsActive &&
            b.Team.WorkspaceId == workspaceId && !b.Team.IsDeleted && b.Team.TeamMembers.Any(m => m.UserId == userId && m.IsActive), ct);
        if (brand == null) return false;
        if (!integrationId.HasValue) return true;
        if (!await db.SocialIntegrations.IgnoreQueryFilters().AnyAsync(i => i.Id == integrationId && i.WorkspaceId == workspaceId && i.BrandId == content.BrandId && i.IsActive && !i.IsDeleted, ct)) return false;
        return brand.ChannelAccessMode == ChannelAccessMode.All || brand.Channels.Any(c => c.IntegrationId == integrationId);
    }

    public async Task RecordParticipationAsync(CollaborationTask task, Guid actorId, CancellationToken ct)
    {
        if (!await db.ContentParticipations.AnyAsync(p => p.ContentId == task.ContentId && p.UserId == task.AssigneeId, ct))
            db.ContentParticipations.Add(new ContentParticipation { WorkspaceId = task.WorkspaceId, ContentId = task.ContentId, UserId = task.AssigneeId, RecordedBy = actorId });
    }

    public async Task ExpireAsync(DateTime now, Guid? workspaceId, CancellationToken ct)
    {
        // Capture revisions before reading tasks/grants. Extension, reassignment or
        // a restored native grant must invalidate a stale expiry transition too.
        var revisions = await db.Workspaces.AsNoTracking().Where(w => !workspaceId.HasValue || w.Id == workspaceId)
            .ToDictionaryAsync(w => w.Id, w => w.PermissionRevision, ct);
        var tasks = await db.CollaborationTasks.IgnoreQueryFilters().Where(t => (!workspaceId.HasValue || t.WorkspaceId == workspaceId) &&
            (t.Status == CollaborationTaskStatus.Pending || t.Status == CollaborationTaskStatus.InProgress)).ToListAsync(ct);
        foreach (var task in tasks)
        {
            if (await HasTeamAccessAsync(task.WorkspaceId, task.TeamId, task.AssigneeId, task.ContentId, task.IntegrationId, ct)) continue;
            var grants = await db.TemporaryAccessGrants.Where(g => g.WorkspaceId == task.WorkspaceId && g.TaskId == task.Id && g.UserId == task.AssigneeId).ToListAsync(ct);
            if (grants.Any(g => g.RevokedAt == null && g.GrantedAt <= now && g.ExpiresAt > now)) continue;
            var expectedAssignee = task.AssigneeId;
            var expectedTeam = task.TeamId;
            db.RegisterMutationAuthorization(task.WorkspaceId, $"Expiry:{task.Id}", revisions[task.WorkspaceId], async token =>
                await db.CollaborationTasks.IgnoreQueryFilters().AsNoTracking().AnyAsync(t => t.Id == task.Id &&
                    t.WorkspaceId == task.WorkspaceId && t.AssigneeId == expectedAssignee && t.TeamId == expectedTeam, token) &&
                !await HasTeamAccessAsync(task.WorkspaceId, expectedTeam, expectedAssignee, task.ContentId, task.IntegrationId, token) &&
                !await db.TemporaryAccessGrants.IgnoreQueryFilters().AnyAsync(g => g.WorkspaceId == task.WorkspaceId &&
                    g.TaskId == task.Id && g.UserId == expectedAssignee && g.RevokedAt == null && g.GrantedAt <= now && g.ExpiresAt > now, token));
            task.Status = CollaborationTaskStatus.Blocked;
            task.BlockedReason = grants.Any(g => g.ExpiresAt <= now && g.RevokedAt == null) ? "ACCESS_EXPIRED" : "ACCESS_REVOKED";
            task.UpdatedAt = now;
            var managers = await db.TeamMembers.Where(m => m.TeamId == task.TeamId && m.IsActive && m.Role == nameof(WorkspaceMemberRoleEnum.Manager)).Select(m => m.UserId).ToListAsync(ct);
            managers.Add(task.AssignedBy);
            managers.Add(task.AssigneeId);
            foreach (var userId in managers.Distinct())
            {
                var profile = await db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
                if (profile == null)
                {
                    profile = new Profile { UserId = userId, Name = "Workspace Profile", ProfileType = ProfileTypeEnum.Free, Status = ProfileStatusEnum.Pending };
                    db.Profiles.Add(profile);
                }
                db.Notifications.Add(new Notification { WorkspaceId = task.WorkspaceId, ProfileId = profile.Id, Title = "Task access blocked", Message = task.BlockedReason, Type = NotificationTypeEnum.SystemUpdate, TargetId = task.Id, TargetType = "collaboration_task" });
            }
            db.AuditLogs.Add(new AuditLog { WorkspaceId = task.WorkspaceId, ActorId = task.AssignedBy,
                RequestedBy = task.AssignedBy, AffectedUserId = task.AssigneeId, TeamId = task.TeamId,
                ExecutedBySystem = true, ReferenceId = task.Id,
                TargetId = task.Id, TargetTable = "collaboration_tasks", ActionType = task.BlockedReason, Notes = "Task blocked because access is no longer valid." });
        }
        if (db.ChangeTracker.HasChanges()) await db.SaveChangesAsync(ct);
    }
}

public sealed class CollaborationExpiryWorker(IServiceScopeFactory scopes, ILogger<CollaborationExpiryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<CollaborationAccessService>().ExpireAsync(DateTime.UtcNow, null, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch { logger.LogError("Collaboration expiry processing failed; access checks still enforce expiration per request."); }
        }
    }
}
