using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class ResourceAccessService(AisamContext db)
{
    public async Task<AccessScope> ResolveAsync(Guid workspaceId, Guid userId, bool write, Guid? teamId = null, CancellationToken ct = default)
    {
        // Read before permission predicates: a concurrent change during resolution must
        // invalidate the eventual mutation stamp, even if later reads see newer state.
        var workspace = await db.Workspaces.AsNoTracking().SingleOrDefaultAsync(w => w.Id == workspaceId, ct)
            ?? throw new UnauthorizedAccessException("Workspace is unavailable.");
        WorkspaceLifecyclePolicy.SynchronizeStatus(workspace, DateTime.UtcNow);
        if (workspace.Status == WorkspaceStatusEnum.Deleted || write && WorkspaceLifecyclePolicy.IsReadOnly(workspace.Status))
            throw new UnauthorizedAccessException("Workspace is unavailable for this action.");
        var revision = workspace.PermissionRevision;
        var member = await db.WorkspaceMembers.AsNoTracking().FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId && m.IsActive && m.User.IsActive, ct)
            ?? throw new UnauthorizedAccessException("Workspace membership is no longer active.");
        var scope = db.AccessScope;
        scope.Enforced = true;
        scope.WorkspaceId = workspaceId;
        scope.UserId = userId;
        scope.Role = member.Role;
        scope.IsWrite = write;
        scope.PermissionRevision = revision;
        var teams = db.Teams.Where(t => t.WorkspaceId == workspaceId && !t.IsDeleted && t.Status == TeamStatusEnum.Active);
        scope.TeamIds = member.Role == WorkspaceMemberRoleEnum.Owner
            ? await teams.Select(t => t.Id).ToArrayAsync(ct)
            : await teams.Where(t => t.TeamMembers.Any(m => m.UserId == userId && m.IsActive)).Select(t => t.Id).ToArrayAsync(ct);
        if (teamId.HasValue && !scope.TeamIds.Contains(teamId.Value)) throw new UnauthorizedAccessException("Team is not accessible.");
        scope.ActiveTeamId = teamId ?? (scope.TeamIds.Length == 1 ? scope.TeamIds[0] : null);
        scope.MemberIds = await db.TeamMembers.Where(m => m.IsActive && scope.TeamIds.Contains(m.TeamId))
            .Select(m => m.UserId).Distinct().ToArrayAsync(ct);
        var access = await db.TeamBrands.IgnoreQueryFilters().AsNoTracking().Include(b => b.Channels)
            .Where(b => b.IsActive && scope.TeamIds.Contains(b.TeamId) && b.Brand.WorkspaceId == workspaceId && !b.Brand.IsDeleted).ToListAsync(ct);
        scope.BrandIds = access.Select(b => b.BrandId).Distinct().ToArray();
        var allBrands = access.Where(a => a.ChannelAccessMode == ChannelAccessMode.All).Select(a => a.BrandId).ToArray();
        var specificIds = access.Where(a => a.ChannelAccessMode == ChannelAccessMode.Specific).SelectMany(a => a.Channels).Select(a => a.IntegrationId).Distinct().ToArray();
        scope.IntegrationIds = await db.SocialIntegrations.IgnoreQueryFilters().Where(i => i.WorkspaceId == workspaceId && !i.IsDeleted && i.IsActive &&
            scope.BrandIds.Contains(i.BrandId) && (allBrands.Contains(i.BrandId) || specificIds.Contains(i.Id))).Select(i => i.Id).ToArrayAsync(ct);
        scope.HistoricalContentIds = await db.Contents.IgnoreQueryFilters().Where(c => c.WorkspaceId == workspaceId &&
            (c.PrimaryCreatorId == userId || c.Participations.Any(p => p.WorkspaceId == workspaceId && p.UserId == userId))).Select(c => c.Id).ToArrayAsync(ct);
        scope.AnalyticsCampaignIds = await db.AdCampaigns.IgnoreQueryFilters().Where(c => c.WorkspaceId == workspaceId &&
            (scope.IsOwner || member.Role == WorkspaceMemberRoleEnum.Manager && c.IntegrationId.HasValue &&
                scope.BrandIds.Contains(c.BrandId) && scope.IntegrationIds.Contains(c.IntegrationId.Value) &&
                db.SocialIntegrations.IgnoreQueryFilters().Any(i => i.Id == c.IntegrationId && i.WorkspaceId == workspaceId && i.BrandId == c.BrandId)))
            .Select(c => c.Id).ToArrayAsync(ct);
        var now = DateTime.UtcNow;
        var assigned = await db.CollaborationTasks.IgnoreQueryFilters().Where(t => t.WorkspaceId == workspaceId && t.AssigneeId == userId &&
            (t.Status == CollaborationTaskStatus.Pending || t.Status == CollaborationTaskStatus.InProgress) && scope.TeamIds.Contains(t.TeamId) &&
            db.TeamBrands.Any(b => b.TeamId == t.TeamId && b.IsActive && b.BrandId == t.Content.BrandId &&
                b.Brand.WorkspaceId == workspaceId && (!t.IntegrationId.HasValue ||
                    db.SocialIntegrations.IgnoreQueryFilters().Any(i => i.Id == t.IntegrationId && i.WorkspaceId == workspaceId &&
                        i.BrandId == b.BrandId && i.IsActive && !i.IsDeleted &&
                        (b.ChannelAccessMode == ChannelAccessMode.All || b.Channels.Any(ch => ch.IntegrationId == i.Id))))))
            .Select(t => t.ContentId).ToArrayAsync(ct);
        var temporary = await db.TemporaryAccessGrants.IgnoreQueryFilters().Where(g => g.WorkspaceId == workspaceId && g.Task.WorkspaceId == workspaceId && g.UserId == userId && g.CanEdit &&
            g.RevokedAt == null && g.GrantedAt <= now && g.ExpiresAt > now && g.Task.AssigneeId == userId &&
            scope.TeamIds.Contains(g.Task.TeamId)).Select(g => g.Task.ContentId).ToArrayAsync(ct);
        scope.EditableContentIds = await db.Contents.IgnoreQueryFilters().Where(c => c.WorkspaceId == workspaceId &&
            (scope.BrandIds.Contains(c.BrandId) && (member.Role == WorkspaceMemberRoleEnum.Manager || c.PrimaryCreatorId == userId || assigned.Contains(c.Id)) || temporary.Contains(c.Id)))
            .Select(c => c.Id).ToArrayAsync(ct);
        // Historical content is a separate VIEW scope. It must never grant Brand,
        // Product, Campaign or social-account access through generic query filters.
        scope.Enforced = true;
        return scope;
    }
}
