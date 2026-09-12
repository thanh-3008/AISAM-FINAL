using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Access;

public sealed class AccessControlService(AisamContext db, IEffectivePermissionContext? permissionContext = null) : IAccessControlService
{
    private async Task<(Workspace Workspace, WorkspaceMember Member)?> Membership(Guid actor, Guid workspace, CancellationToken ct)
    {
        if (actor == Guid.Empty || workspace == Guid.Empty) return null;
        if (!await db.Users.IgnoreQueryFilters().AsNoTracking().AnyAsync(u => u.Id == actor && u.IsActive, ct)) return null;
        var w = await db.Workspaces.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(w => w.Id == workspace, ct);
        if (w is null || !Enum.IsDefined(w.Status) || w.Status == WorkspaceStatusEnum.Deleted) return null;
        var m = await db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(m => m.WorkspaceId == workspace && m.UserId == actor && m.IsActive, ct);
        if (m is null || !Enum.IsDefined(m.Role)) return null;
        WorkspaceLifecyclePolicy.SynchronizeStatus(w, DateTime.UtcNow); // detached; no writes
        return (w, m);
    }

    private IQueryable<TeamBrand> Assignments(Guid actor, Guid workspace) =>
        from assignment in db.TeamBrands.IgnoreQueryFilters().AsNoTracking()
        join team in db.Teams.IgnoreQueryFilters().AsNoTracking() on assignment.TeamId equals team.Id
        join member in db.TeamMembers.IgnoreQueryFilters().AsNoTracking() on team.Id equals member.TeamId
        where assignment.IsActive && team.WorkspaceId == workspace && !team.IsDeleted &&
            team.Status == TeamStatusEnum.Active && member.UserId == actor && member.IsActive
        select assignment;

    public async Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid actorId, Guid workspaceId, CancellationToken ct = default)
    {
        var member = await Membership(actorId, workspaceId, ct);
        if (member is null) return Array.Empty<Guid>();
        var brands = db.Brands.IgnoreQueryFilters().AsNoTracking().Where(b => b.WorkspaceId == workspaceId && !b.IsDeleted);
        if (member.Value.Member.Role is not WorkspaceMemberRoleEnum.Owner and not WorkspaceMemberRoleEnum.WorkspaceManager)
        {
            var ids = Assignments(actorId, workspaceId).Select(a => a.BrandId);
            brands = brands.Where(b => ids.Contains(b.Id));
        }
        return await brands.Select(b => b.Id).Distinct().OrderBy(id => id).ToListAsync(ct);
    }

    private static bool Compatible(AccessRequest r) => r.Kind switch
    {
        AccessResourceKind.Workspace => r.Permission == ResourcePermission.BillingManage,
        AccessResourceKind.Brand => r.Permission is ResourcePermission.BrandView or ResourcePermission.BrandManage or ResourcePermission.TeamManage
            or ResourcePermission.ContentCreate or ResourcePermission.ContentViewAllCreators or ResourcePermission.AnalyticsView or ResourcePermission.AnalyticsMember,
        AccessResourceKind.Content => r.Permission is ResourcePermission.ContentView or ResourcePermission.ContentEdit or ResourcePermission.ContentDelete
            or ResourcePermission.ApprovalReview or ResourcePermission.PostPublish,
        AccessResourceKind.Channel => r.Permission is ResourcePermission.SocialView or ResourcePermission.SocialManage,
        AccessResourceKind.Post => r.Permission == ResourcePermission.PostView,
        _ => false
    };

    private async Task<IEffectivePermissionContext> ResolveEffectiveContextAsync(Guid actor, Guid workspace, WorkspaceMemberRoleEnum role, CancellationToken ct)
    {
        if (role == WorkspaceMemberRoleEnum.Owner)
        {
            var allTeamIds = await db.Teams.IgnoreQueryFilters().AsNoTracking()
                .Where(t => t.WorkspaceId == workspace && !t.IsDeleted && t.Status == TeamStatusEnum.Active)
                .Select(t => t.Id).ToListAsync(ct);
            var allBrandIds = await db.Brands.IgnoreQueryFilters().AsNoTracking()
                .Where(b => b.WorkspaceId == workspace && !b.IsDeleted)
                .Select(b => b.Id).ToListAsync(ct);
            var maxRoles = allBrandIds.ToDictionary(id => id, _ => TeamRoleEnum.Manager);
            return new EffectivePermissionContext(actor, workspace, role, allTeamIds.ToHashSet(), maxRoles);
        }

        var userTeams = await (
            from tm in db.TeamMembers.IgnoreQueryFilters().AsNoTracking()
            join t in db.Teams.IgnoreQueryFilters().AsNoTracking() on tm.TeamId equals t.Id
            where tm.UserId == actor && tm.IsActive
               && t.WorkspaceId == workspace && !t.IsDeleted && t.Status == TeamStatusEnum.Active
            select new { tm.TeamId, tm.Role }
        ).ToListAsync(ct);

        var teamIds = userTeams.Select(t => t.TeamId).Distinct().ToList();
        var teamBrands = await (
            from tb in db.TeamBrands.IgnoreQueryFilters().AsNoTracking()
            where teamIds.Contains(tb.TeamId) && tb.IsActive
            select new { tb.TeamId, tb.BrandId }
        ).ToListAsync(ct);

        var teamRoleMap = userTeams.ToDictionary(t => t.TeamId, t => t.Role != 0 ? t.Role :
            role == (WorkspaceMemberRoleEnum)4 ? TeamRoleEnum.Viewer :
            role == (WorkspaceMemberRoleEnum)3 ? TeamRoleEnum.ContentCreator :
            TeamRoleEnum.Manager);

        var brandMaxRole = teamBrands
            .GroupBy(tb => tb.BrandId)
            .ToDictionary(
                g => g.Key,
                g => EffectivePermissionContext.ResolveMaxRole(g.Select(x => teamRoleMap.GetValueOrDefault(x.TeamId, TeamRoleEnum.Viewer)))
            );

        return new EffectivePermissionContext(actor, workspace, role, teamIds.ToHashSet(), brandMaxRole);
    }

    public async Task<AccessDecision> CheckAsync(AccessRequest r, CancellationToken ct = default)
    {
        if (!Compatible(r) || r.ResourceId == Guid.Empty) return AccessDecision.Hidden;
        var membership = await Membership(r.ActorId, r.WorkspaceId, ct);
        if (membership is null) return AccessDecision.Hidden;
        var (workspace, member) = membership.Value;
        bool owner = member.Role == WorkspaceMemberRoleEnum.Owner;

        if (r.Kind == AccessResourceKind.Workspace)
            return r.ResourceId != r.WorkspaceId ? AccessDecision.Hidden : owner ? AccessDecision.Permit : AccessDecision.Denied;

        Guid brandId;
        Content? content = null;
        SocialIntegration? channel = null;
        if (r.Kind is AccessResourceKind.Content or AccessResourceKind.Post)
        {
            var contentId = r.ResourceId;
            if (r.Kind == AccessResourceKind.Post)
            {
                var post = await db.Posts.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(p => p.Id == r.ResourceId && !p.IsDeleted, ct);
                if (post is null) return AccessDecision.Hidden;
                contentId = post.ContentId;
                channel = await db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(i => i.Id == post.IntegrationId && !i.IsDeleted, ct);
                if (channel is null) return AccessDecision.Hidden;
            }
            content = await db.Contents.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(c => c.Id == contentId && (!c.IsDeleted || r.IncludeDeleted), ct);
            if (content is null || content.WorkspaceId != r.WorkspaceId) return AccessDecision.Hidden;
            brandId = content.BrandId;
        }
        else if (r.Kind == AccessResourceKind.Channel)
        {
            channel = await db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(i => i.Id == r.ResourceId && !i.IsDeleted, ct);
            if (channel is null) return AccessDecision.Hidden;
            brandId = channel.BrandId;
        }
        else brandId = r.ResourceId;

        var brand = await db.Brands.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(b => b.Id == brandId && b.WorkspaceId == r.WorkspaceId && (!b.IsDeleted || r.IncludeDeleted), ct);
        if (brand is null) return AccessDecision.Hidden;

        if (r.Permission == ResourcePermission.PostPublish)
        {
            if (r.ChannelId is null) return AccessDecision.Hidden;
            channel = await db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(i => i.Id == r.ChannelId && !i.IsDeleted, ct);
            if (channel is null) return AccessDecision.Hidden;
        }
        if (channel is not null && (channel.WorkspaceId != r.WorkspaceId || channel.BrandId != brandId)) return AccessDecision.Hidden;

        IEffectivePermissionContext context;
        if (permissionContext != null &&
            permissionContext.UserId == r.ActorId &&
            permissionContext.WorkspaceId == r.WorkspaceId)
        {
            context = permissionContext;
        }
        else
        {
            context = await ResolveEffectiveContextAsync(r.ActorId, r.WorkspaceId, member.Role, ct);
        }

        var effectiveRole = context.GetEffectiveRoleForBrand(brandId);
        if (!owner && effectiveRole == null) return AccessDecision.Hidden;

        var assignments = await Assignments(r.ActorId, r.WorkspaceId).Where(a => a.BrandId == brandId).ToListAsync(ct);
        if (!owner && assignments.Count == 0) return AccessDecision.Hidden;

        var assignmentIds = assignments.Select(a => a.Id).ToArray();
        var teamIds = assignments.Select(a => a.TeamId).ToArray();
        var keys = await db.TeamMembers.IgnoreQueryFilters().AsNoTracking().Where(m => m.UserId == r.ActorId && m.IsActive && teamIds.Contains(m.TeamId))
            .Select(m => m.Permissions).ToListAsync(ct);
        bool Key(string key) => keys.Any(list => list != null && list.Contains(key, StringComparer.Ordinal));

        var grants = channel is null ? new List<TeamChannelAccess>() : await db.TeamChannelAccesses.IgnoreQueryFilters().AsNoTracking()
            .Where(g => assignmentIds.Contains(g.TeamBrandId) && g.IntegrationId == channel.Id).ToListAsync(ct);

        bool sameTeamContent = false;
        if (content != null)
        {
            if (owner)
            {
                sameTeamContent = true;
            }
            else if (content.TeamId.HasValue)
            {
                sameTeamContent = context.HasTeamAccess(content.TeamId.Value);
            }
            else
            {
                sameTeamContent = context.HasBrandAccess(content.BrandId);
            }
        }

        bool isManager = owner || effectiveRole == TeamRoleEnum.Manager;
        bool channelAccessible = grants.Count == 0 ? (owner || (isManager && assignments.Count > 0)) : grants.Any(g => g.CanView);

        var facts = new ResourcePermissionFacts(
            WorkspaceRole: member.Role,
            TeamRole: effectiveRole,
            WorkspaceStatus: workspace.Status,
            ActiveMembership: true,
            ResourceInWorkspace: true,
            BrandAccessible: owner || context.HasBrandAccess(brandId),
            OwnContent: content?.PrimaryCreatorId == r.ActorId,
            SameTeamContent: sameTeamContent,
            ChannelAccessible: channelAccessible,
            ChannelCanPublish: grants.Any(g => g.CanView && g.CanPublish) || (grants.Count == 0 && (owner || isManager)),
            ChannelCanManage: grants.Any(g => g.CanView && g.CanManage) || (grants.Count == 0 && (owner || isManager)),
            CanViewAllCreators: Key(DelegatedPermissionKeys.ViewAllCreators),
            CanReview: Key(DelegatedPermissionKeys.Review),
            CanPublish: Key(DelegatedPermissionKeys.Publish),
            OwnPerformance: r.MemberId == r.ActorId);

        var visible = r.Kind switch
        {
            AccessResourceKind.Content or AccessResourceKind.Post => ResourcePermissionPolicy.Allows(facts, ResourcePermission.ContentView)
                || r.Permission == ResourcePermission.ApprovalReview && ResourcePermissionPolicy.Allows(facts, ResourcePermission.ApprovalReview),
            AccessResourceKind.Channel => ResourcePermissionPolicy.Allows(facts, ResourcePermission.SocialView),
            _ => true
        };
        if (!visible) return AccessDecision.Hidden;
        if (r.Permission == ResourcePermission.PostPublish && !owner && !facts.ChannelAccessible)
            return AccessDecision.Hidden;
        if (r.Permission is ResourcePermission.SocialManage or ResourcePermission.PostPublish && channel?.IsActive != true)
            return AccessDecision.Denied;
        if (r.Permission == ResourcePermission.AnalyticsMember)
        {
            if (r.MemberId is not { } target ||
                !await db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().AnyAsync(m => m.WorkspaceId == r.WorkspaceId && m.UserId == target && m.IsActive, ct))
                return AccessDecision.Hidden;
            if (!owner && target != r.ActorId && !await db.TeamMembers.IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(m => m.UserId == target && m.IsActive && teamIds.Contains(m.TeamId), ct))
                return AccessDecision.Hidden;
        }

        if (ResourcePermissionPolicy.Allows(facts, r.Permission)) return AccessDecision.Permit;

        if (r.Permission is ResourcePermission.ContentEdit or ResourcePermission.ContentDelete &&
            (member.Role == WorkspaceMemberRoleEnum.ContentCreator || effectiveRole == TeamRoleEnum.ContentCreator) && !facts.OwnContent)
            return new(false, 403, "CONTENT_NOT_OWNED");
        if (r.Permission is ResourcePermission.PostPublish or ResourcePermission.SocialManage)
            return new(false, 403, "ACCESS_DENIED_CHANNEL");
        return new(false, 403, "ACCESS_DENIED_BRAND");
    }
}
