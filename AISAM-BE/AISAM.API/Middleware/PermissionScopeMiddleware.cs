using AISAM.API.Utils;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.API.Middleware;

public sealed class PermissionScopeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http, AisamContext db, IAccessControlService access)
    {
        if (!http.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey, out var value) ||
            value is not AISAM.Data.Model.WorkspaceMember membership)
        {
            await next(http);
            return;
        }

        var actor = UserClaimsHelper.GetUserIdOrThrow(http.User);
        var workspace = membership.WorkspaceId;
        if (!membership.IsActive || !await db.Users.AsNoTracking().AnyAsync(u => u.Id == actor && u.IsActive, http.RequestAborted))
        {
            http.Response.StatusCode = 403;
            await http.Response.WriteAsJsonAsync(new { success = false, errorCode = "ACCESS_DENIED" });
            return;
        }

        db.PermissionWorkspaceId = workspace;
        db.PermissionActorId = actor;
        db.PermissionOwner = membership.Role == WorkspaceMemberRoleEnum.Owner;
        db.PermissionWorkspaceManager = membership.Role == WorkspaceMemberRoleEnum.WorkspaceManager;

        var assignments = from b in db.TeamBrands.AsNoTracking()
            join t in db.Teams.AsNoTracking() on b.TeamId equals t.Id
            join member in db.TeamMembers.AsNoTracking() on t.Id equals member.TeamId
            where b.IsActive && t.WorkspaceId == workspace && !t.IsDeleted && t.Status == TeamStatusEnum.Active && member.UserId == actor && member.IsActive
            select new { b.Id, b.BrandId, b.TeamId, member.Permissions, member.Role };
        var rows = await assignments.ToListAsync(http.RequestAborted);

        HashSet<Guid> accessibleTeamIds;
        Dictionary<Guid, TeamRoleEnum> brandMaxRole;

        if (membership.Role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.WorkspaceManager)
        {
            var allTeams = await db.Teams.AsNoTracking()
                .Where(t => t.WorkspaceId == workspace && !t.IsDeleted && t.Status == TeamStatusEnum.Active)
                .Select(t => t.Id).ToListAsync(http.RequestAborted);
            accessibleTeamIds = allTeams.ToHashSet();

            var allBrands = await db.Brands.AsNoTracking()
                .Where(b => b.WorkspaceId == workspace && !b.IsDeleted)
                .Select(b => b.Id).ToListAsync(http.RequestAborted);
            brandMaxRole = allBrands.ToDictionary(id => id, _ => TeamRoleEnum.Manager);
        }
        else
        {
            accessibleTeamIds = rows.Select(r => r.TeamId).ToHashSet();
            brandMaxRole = rows
                .GroupBy(r => r.BrandId)
                .ToDictionary(
                    g => g.Key,
                    g => EffectivePermissionContext.ResolveMaxRole(g.Select(x => x.Role))
                );
        }

        var effectivePermissionContext = (http.RequestServices?.GetService(typeof(EffectivePermissionContext)) as EffectivePermissionContext) ?? new EffectivePermissionContext();
        effectivePermissionContext.Initialize(actor, workspace, membership.Role, accessibleTeamIds, brandMaxRole);
        http.Items[WorkspaceContextHelper.EffectivePermissionContextItemKey] = effectivePermissionContext;

        db.PermissionBrandIds = (await access.GetAccessibleBrandIdsAsync(actor, workspace, http.RequestAborted)).ToArray();
        db.PermissionTeamIds = accessibleTeamIds.ToArray();

        var requestedBrandId = ResolveRequestedBrandId(http);
        bool isManagerForScope = false;
        bool isCreatorForScope = false;
        bool isViewerForScope = false;

        if (membership.Role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.WorkspaceManager)
        {
            isManagerForScope = true;
        }
        else if (requestedBrandId.HasValue && brandMaxRole.TryGetValue(requestedBrandId.Value, out var role))
        {
            isManagerForScope = role == TeamRoleEnum.Manager;
            isCreatorForScope = role == TeamRoleEnum.ContentCreator;
            isViewerForScope = role == TeamRoleEnum.Viewer;
        }
        else
        {
            isCreatorForScope = membership.Role == WorkspaceMemberRoleEnum.ContentCreator || brandMaxRole.Values.Any(r => r == TeamRoleEnum.ContentCreator);
            isViewerForScope = membership.Role == WorkspaceMemberRoleEnum.Viewer || brandMaxRole.Values.Any(r => r == TeamRoleEnum.Viewer);
        }

        db.PermissionManager = isManagerForScope;
        db.PermissionCreator = isCreatorForScope;
        db.PermissionViewer = isViewerForScope;

        db.PermissionViewAllBrandIds = rows.Where(r => r.Permissions.Contains(DelegatedPermissionKeys.ViewAllCreators)).Select(r => r.BrandId).Distinct().ToArray();
        db.PermissionReviewBrandIds = rows.Where(r => r.Permissions.Contains(DelegatedPermissionKeys.Review)).Select(r => r.BrandId).Distinct().ToArray();

        var assignmentIds = rows.Select(r => r.Id).ToArray();
        var grantedChannelIds = await db.TeamChannelAccesses.AsNoTracking().Where(c => assignmentIds.Contains(c.TeamBrandId) && c.CanView).Select(c => c.IntegrationId).Distinct().ToListAsync(http.RequestAborted);

        var managerBrandScope = (membership.Role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.WorkspaceManager)
            ? db.PermissionBrandIds
            : brandMaxRole.Where(kvp => kvp.Value == TeamRoleEnum.Manager).Select(kvp => kvp.Key).ToArray();

        db.PermissionManagerBrandIds = managerBrandScope;

        if (managerBrandScope.Length > 0)
        {
            var allManagedChannels = await db.SocialIntegrations
                .IgnoreQueryFilters().AsNoTracking()
                .Where(i => managerBrandScope.Contains(i.BrandId) && i.WorkspaceId == workspace && !i.IsDeleted)
                .Select(i => i.Id).ToListAsync(http.RequestAborted);
            grantedChannelIds = grantedChannelIds.Union(allManagedChannels).Distinct().ToList();
        }

        db.PermissionChannelIds = grantedChannelIds.ToArray();
        db.PermissionPlanIds = await db.AutomationPlans.AsNoTracking().Where(p => p.WorkspaceId == workspace &&
            !db.AutomationItems.Any(i => i.AutomationPlanId == p.Id && i.BrandId.HasValue && !db.PermissionBrandIds.Contains(i.BrandId.Value)))
            .Select(p => p.Id).ToArrayAsync(http.RequestAborted);

        db.PermissionScopeEnabled = true;
        db.BeforePermissionMutation = async (entity, state, ct) =>
        {
            AccessRequest? request = entity switch
            {
                AISAM.Data.Model.Content content when state == EntityState.Added => new(actor, workspace, AccessResourceKind.Brand, content.BrandId, ResourcePermission.ContentCreate),
                AISAM.Data.Model.Content content => new(actor, workspace, AccessResourceKind.Content, content.Id,
                    db.PermissionReviewContentId == content.Id ? ResourcePermission.ApprovalReview : content.IsDeleted ? ResourcePermission.ContentDelete : ResourcePermission.ContentEdit, IncludeDeleted: true),
                AISAM.Data.Model.ContentCalendar calendar => new(actor, workspace, AccessResourceKind.Content, calendar.ContentId, ResourcePermission.PostPublish, calendar.IntegrationId),
                AISAM.Data.Model.Product product => new(actor, workspace, AccessResourceKind.Brand, product.BrandId, ResourcePermission.BrandManage),
                AISAM.Data.Model.SocialIntegration channel when state == EntityState.Added => new(actor, workspace, AccessResourceKind.Brand, channel.BrandId, ResourcePermission.BrandManage),
                AISAM.Data.Model.SocialIntegration channel => new(actor, workspace, AccessResourceKind.Channel, channel.Id, ResourcePermission.SocialManage),
                AISAM.Data.Model.AdCampaign campaign => new(actor, workspace, AccessResourceKind.Brand, campaign.BrandId, ResourcePermission.BrandManage),
                _ => null
            };
            if (request is not null && !(await access.CheckAsync(request, ct)).Allowed)
                throw new ResourceMutationDeniedException();
        };

        try { await next(http); }
        finally
        {
            db.PermissionScopeEnabled = false;
            db.PermissionReviewContentId = null;
            db.PermissionReviewQueue = false;
            db.BeforePermissionMutation = null;
            db.PermissionWorkspaceManager = false;
            db.PermissionManagerBrandIds = [];
        }
    }

    private static Guid? ResolveRequestedBrandId(HttpContext http)
    {
        if (http.Request.Query.TryGetValue("brandId", out var qVal) && Guid.TryParse(qVal, out var qGuid) && qGuid != Guid.Empty)
            return qGuid;

        if (http.Request.RouteValues.TryGetValue("brandId", out var rVal) && Guid.TryParse(rVal?.ToString(), out var rGuid) && rGuid != Guid.Empty)
            return rGuid;

        var controller = http.Request.RouteValues.TryGetValue("controller", out var cVal) ? cVal?.ToString() : null;
        if (string.Equals(controller, "Brand", StringComparison.OrdinalIgnoreCase) || string.Equals(controller, "Brands", StringComparison.OrdinalIgnoreCase))
        {
            if (http.Request.RouteValues.TryGetValue("id", out var idVal) && Guid.TryParse(idVal?.ToString(), out var idGuid) && idGuid != Guid.Empty)
                return idGuid;
        }

        if (http.Request.Headers.TryGetValue("X-Brand-Id", out var hVal) && Guid.TryParse(hVal, out var hGuid) && hGuid != Guid.Empty)
            return hGuid;

        return null;
    }
}
