using AISAM.API.Utils;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.API.Middleware;

public sealed class PermissionScopeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http,AisamContext db,IAccessControlService access)
    {
        if(!http.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey,out var value) ||
            value is not AISAM.Data.Model.WorkspaceMember membership)
        { await next(http); return; }
        var actor=UserClaimsHelper.GetUserIdOrThrow(http.User);
        var workspace=membership.WorkspaceId;
        if(!membership.IsActive || !await db.Users.AsNoTracking().AnyAsync(u=>u.Id==actor && u.IsActive,http.RequestAborted))
        { http.Response.StatusCode=403; await http.Response.WriteAsJsonAsync(new {success=false,errorCode="ACCESS_DENIED"}); return; }
        db.PermissionWorkspaceId=workspace; db.PermissionActorId=actor;
        db.PermissionOwner=membership.Role==WorkspaceMemberRoleEnum.Owner;
        db.PermissionManager=membership.Role==WorkspaceMemberRoleEnum.Manager;
        db.PermissionCreator=membership.Role==WorkspaceMemberRoleEnum.ContentCreator;
        db.PermissionBrandIds=(await access.GetAccessibleBrandIdsAsync(actor,workspace,http.RequestAborted)).ToArray();
        var assignments=from b in db.TeamBrands.AsNoTracking()
            join t in db.Teams.AsNoTracking() on b.TeamId equals t.Id
            join member in db.TeamMembers.AsNoTracking() on t.Id equals member.TeamId
            where b.IsActive && t.WorkspaceId==workspace && !t.IsDeleted && t.Status==TeamStatusEnum.Active && member.UserId==actor && member.IsActive
            select new {b.Id,b.BrandId,b.TeamId,member.Permissions};
        var rows=await assignments.ToListAsync(http.RequestAborted);
        db.PermissionTeamIds=rows.Select(r=>r.TeamId).Distinct().ToArray();
        db.PermissionViewAllBrandIds=rows.Where(r=>r.Permissions.Contains(DelegatedPermissionKeys.ViewAllCreators)).Select(r=>r.BrandId).Distinct().ToArray();
        db.PermissionReviewBrandIds=rows.Where(r=>r.Permissions.Contains(DelegatedPermissionKeys.Review)).Select(r=>r.BrandId).Distinct().ToArray();
        var assignmentIds=rows.Select(r=>r.Id).ToArray();
        db.PermissionChannelIds=await db.TeamChannelAccesses.AsNoTracking().Where(c=>assignmentIds.Contains(c.TeamBrandId) && c.CanView).Select(c=>c.IntegrationId).Distinct().ToArrayAsync(http.RequestAborted);
        db.PermissionPlanIds=await db.AutomationPlans.AsNoTracking().Where(p=>p.WorkspaceId==workspace &&
            !db.AutomationItems.Any(i=>i.AutomationPlanId==p.Id && (!i.BrandId.HasValue || !db.PermissionBrandIds.Contains(i.BrandId.Value))))
            .Select(p=>p.Id).ToArrayAsync(http.RequestAborted);
        db.PermissionScopeEnabled=true;
        db.BeforePermissionMutation=async (entity,state,ct)=>
        {
            AccessRequest? request=entity switch
            {
                AISAM.Data.Model.Content content when state==EntityState.Added => new(actor,workspace,AccessResourceKind.Brand,content.BrandId,ResourcePermission.ContentCreate),
                AISAM.Data.Model.Content content => new(actor,workspace,AccessResourceKind.Content,content.Id,
                    db.PermissionReviewContentId==content.Id?ResourcePermission.ApprovalReview:content.IsDeleted?ResourcePermission.ContentDelete:ResourcePermission.ContentEdit,IncludeDeleted:true),
                AISAM.Data.Model.ContentCalendar calendar => new(actor,workspace,AccessResourceKind.Content,calendar.ContentId,ResourcePermission.PostPublish,calendar.IntegrationId),
                AISAM.Data.Model.Product product => new(actor,workspace,AccessResourceKind.Brand,product.BrandId,ResourcePermission.BrandManage),
                AISAM.Data.Model.SocialIntegration channel when state==EntityState.Added => new(actor,workspace,AccessResourceKind.Brand,channel.BrandId,ResourcePermission.BrandManage),
                AISAM.Data.Model.SocialIntegration channel => new(actor,workspace,AccessResourceKind.Channel,channel.Id,ResourcePermission.SocialManage),
                AISAM.Data.Model.AdCampaign campaign => new(actor,workspace,AccessResourceKind.Brand,campaign.BrandId,ResourcePermission.BrandManage),
                _=>null
            };
            if(request is not null && !(await access.CheckAsync(request,ct)).Allowed)
                throw new ResourceMutationDeniedException();
        };
        try { await next(http); }
        finally { db.PermissionScopeEnabled=false; db.PermissionReviewContentId=null; db.PermissionReviewQueue=false; db.BeforePermissionMutation=null; }
    }
}
