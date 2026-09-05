using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class ContentAuthorizationService(AisamContext db, ResourceAccessService resolver, CollaborationAccessService collaboration, TimeProvider? clock = null)
{
    public async Task EnsureWorkspaceOwnerMutationAsync(Guid workspaceId, string operation, CancellationToken ct)
    {
        var actor = db.AccessScope.UserId;
        async Task<bool> Allowed(CancellationToken token)
        {
            if (!db.AccessScope.Enforced || db.AccessScope.WorkspaceId != workspaceId || db.AccessScope.UserId != actor) return false;
            try { return (await resolver.ResolveAsync(workspaceId, actor, true, db.AccessScope.ActiveTeamId, token)).IsOwner; }
            catch (UnauthorizedAccessException) { return false; }
        }
        if (!await Allowed(ct)) throw new ResourceAccessDeniedException();
        db.RegisterMutationAuthorization(workspaceId, $"Workspace:{operation}", db.AccessScope.PermissionRevision, Allowed);
    }

    public async Task EnsureBrandActionAsync(Guid workspaceId, Guid? brandId, ContentAction action, CancellationToken ct)
    {
        var actor = db.AccessScope.UserId;
        async Task<bool> Allowed(CancellationToken token)
        {
            if (!db.AccessScope.Enforced || db.AccessScope.WorkspaceId != workspaceId || db.AccessScope.UserId != actor) return false;
            AccessScope current;
            try { current = await resolver.ResolveAsync(workspaceId, actor, true, db.AccessScope.ActiveTeamId, token); }
            catch (UnauthorizedAccessException) { return false; }
            if (brandId.HasValue && !await db.Brands.IgnoreQueryFilters().AnyAsync(b => b.Id == brandId && b.WorkspaceId == workspaceId && !b.IsDeleted, token)) return false;
            if (current.IsOwner) return true;
            // CREATE retains the current native role/Brand permission. New AI/media/chat
            // permissions remain conservative until the action policy is approved.
            return action == ContentAction.Create && brandId.HasValue && current.BrandIds.Contains(brandId.Value) &&
                current.Role is WorkspaceMemberRoleEnum.Manager or WorkspaceMemberRoleEnum.ContentCreator;
        }
        if (!await Allowed(ct)) throw new ResourceAccessDeniedException();
        db.RegisterMutationAuthorization(workspaceId, $"Brand:{brandId}:{action}", db.AccessScope.PermissionRevision, Allowed);
    }
    public Task EnsureCurrentBrandActionAsync(Guid? brandId, ContentAction action, CancellationToken ct) =>
        EnsureBrandActionAsync(db.AccessScope.WorkspaceId, brandId, action, ct);

    private sealed record ContentResource(Guid Id, Guid BrandId, Guid? PrimaryCreatorId);

    public async Task<Dictionary<string, bool>> GetActionsAsync(Guid workspaceId, Guid contentId, Guid? channelId, CancellationToken ct)
    {
        if (!db.AccessScope.Enforced || workspaceId != db.AccessScope.WorkspaceId) throw new ResourceAccessDeniedException();
        var scope = await resolver.ResolveAsync(workspaceId, db.AccessScope.UserId, false, db.AccessScope.ActiveTeamId, ct);
        var content = await ReadResourceAsync(workspaceId, contentId, channelId, ct);
        var actions = new Dictionary<string, bool>();
        foreach (var action in Enum.GetValues<ContentAction>().Where(a => a != ContentAction.Create))
            actions[action.ToString()] = content != null && await AllowsInScopeAsync(scope, content, action, channelId, ct);
        return actions;
    }

    public async Task EnsureAsync(Guid workspaceId, Guid contentId, ContentAction action, Guid? channelId, CancellationToken ct)
    {
        if (!await AllowsAsync(workspaceId, contentId, action, channelId, ct)) throw new ResourceAccessDeniedException();
        if (action is not ContentAction.View and not ContentAction.ViewAnalytics)
        {
            var actor = db.AccessScope.UserId;
            db.RegisterMutationAuthorization(workspaceId, $"Content:{contentId}:{action}:{channelId}", db.AccessScope.PermissionRevision,
                token => db.AccessScope.UserId == actor ? AllowsAsync(workspaceId, contentId, action, channelId, token) : Task.FromResult(false));
        }
    }

    public async Task<bool> AllowsAsync(Guid workspaceId, Guid contentId, ContentAction action, Guid? channelId = null, CancellationToken ct = default)
    {
        if (!db.AccessScope.Enforced || workspaceId != db.AccessScope.WorkspaceId) return false;
        AccessScope scope;
        try { scope = await resolver.ResolveAsync(workspaceId, db.AccessScope.UserId, action is not ContentAction.View and not ContentAction.ViewAnalytics, db.AccessScope.ActiveTeamId, ct); }
        catch (UnauthorizedAccessException) { return false; }
        var content = await ReadResourceAsync(workspaceId, contentId, channelId, ct);
        return content != null && await AllowsInScopeAsync(scope, content, action, channelId, ct);
    }

    private async Task<ContentResource?> ReadResourceAsync(Guid workspaceId, Guid contentId, Guid? channelId, CancellationToken ct)
    {
        // Read the minimum resource attributes with an explicit workspace boundary. General
        // resource filters are unsuitable for deciding whether an independent grant exists.
        var content = await db.Contents.IgnoreQueryFilters().AsNoTracking()
            .Where(c => c.Id == contentId && c.WorkspaceId == workspaceId)
            .Select(c => new ContentResource(c.Id, c.BrandId, c.PrimaryCreatorId)).FirstOrDefaultAsync(ct);
        if (content == null) return null;
        if (channelId.HasValue && !await db.SocialIntegrations.IgnoreQueryFilters().AnyAsync(i => i.Id == channelId &&
            i.WorkspaceId == workspaceId && i.BrandId == content.BrandId && i.IsActive && !i.IsDeleted, ct)) return null;
        return content;
    }

    private async Task<bool> AllowsInScopeAsync(AccessScope scope, ContentResource content, ContentAction action, Guid? channelId, CancellationToken ct)
    {
        var workspaceId = scope.WorkspaceId;
        var contentId = content.Id;
        if (scope.IsOwner) return true;
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer) return action == ContentAction.View && scope.BrandIds.Contains(content.BrandId);
        if (action is ContentAction.View or ContentAction.ViewAnalytics)
            return scope.IsCreator ? scope.HistoricalContentIds.Contains(contentId) :
                scope.BrandIds.Contains(content.BrandId) && (!channelId.HasValue || scope.IntegrationIds.Contains(channelId.Value));

        var hasBrand = scope.BrandIds.Contains(content.BrandId);
        var hasChannel = !channelId.HasValue || scope.IntegrationIds.Contains(channelId.Value);
        if (scope.Role == WorkspaceMemberRoleEnum.Manager)
            return hasBrand && hasChannel && action is ContentAction.Edit or ContentAction.Delete or ContentAction.Restore or
                ContentAction.Clone or ContentAction.Submit or ContentAction.Approve or ContentAction.Reject or
                ContentAction.Assign or ContentAction.Reassign or ContentAction.Schedule or ContentAction.Reschedule or
                ContentAction.Unschedule or ContentAction.Publish or ContentAction.ViewAnalytics;

        // Preserve existing Creator lifecycle permissions only with independent current
        // Team/Brand access. OWN alone never grants an action.
        if (scope.IsCreator && hasBrand && hasChannel && content.PrimaryCreatorId == scope.UserId &&
            action is ContentAction.Edit or ContentAction.Delete or ContentAction.Restore or ContentAction.Clone or ContentAction.Submit or ContentAction.Assign)
            return true;

        // The currently defined temporary grant carries CanEdit only. It is not a
        // permission to delete, clone, submit, assign, schedule, publish or share.
        if (!scope.IsCreator || action != ContentAction.Edit) return false;
        var tasks = await db.CollaborationTasks.IgnoreQueryFilters().AsNoTracking().Where(t => t.WorkspaceId == workspaceId &&
            t.ContentId == contentId && t.AssigneeId == scope.UserId).ToListAsync(ct);
        foreach (var task in tasks)
        {
            if (!scope.TeamIds.Contains(task.TeamId)) continue;
            if (task.Status is CollaborationTaskStatus.Pending or CollaborationTaskStatus.InProgress &&
                await collaboration.HasTeamAccessAsync(workspaceId, task.TeamId, scope.UserId, contentId, task.IntegrationId, ct)) return true;
            var now = clock?.GetUtcNow().UtcDateTime ?? DateTime.UtcNow;
            if (await db.TemporaryAccessGrants.AsNoTracking().AnyAsync(g => g.WorkspaceId == workspaceId && g.TaskId == task.Id &&
                g.UserId == scope.UserId && g.CanEdit && g.RevokedAt == null && g.GrantedAt <= now && g.ExpiresAt > now, ct)) return true;
        }
        return false;
    }
}
