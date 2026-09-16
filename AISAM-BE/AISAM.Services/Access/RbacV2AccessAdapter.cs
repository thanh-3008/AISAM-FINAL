using AISAM.Data.Enumeration;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Access;

// Selected server-side at startup; never OR the legacy and v2 decisions.
public sealed class RbacV2AccessAdapter(AisamContext db,RbacV2AccessResolver resolver) : IAccessControlService
{
    public async Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid actorId,Guid workspaceId,CancellationToken ct=default)
        => (await resolver.ContextAsync(actorId,workspaceId,ct))?.Scopes.Select(s=>s.BrandId).Distinct().ToArray() ?? [];

    public async Task<AccessDecision> CheckAsync(AccessRequest r,CancellationToken ct=default)
    {
        if(r.ResourceId==Guid.Empty) return AccessDecision.Hidden;
        if(r.Permission==ResourcePermission.PostPublish && r.Kind==AccessResourceKind.Content && db.ExecutionSnapshotId is { } expected &&
            !await db.Contents.IgnoreQueryFilters().AsNoTracking().AnyAsync(c=>c.Id==r.ResourceId && c.WorkspaceId==r.WorkspaceId && c.ApprovedSnapshotId==expected,ct))
            return AccessDecision.Denied;
        RbacV2Action? action=r.Permission switch
        {
            ResourcePermission.BillingManage when r.Kind==AccessResourceKind.Workspace && r.ResourceId==r.WorkspaceId => RbacV2Action.BillingManage,
            ResourcePermission.BrandView when r.Kind==AccessResourceKind.Brand => RbacV2Action.BrandRead,
            ResourcePermission.BrandManage when r.Kind==AccessResourceKind.Brand => RbacV2Action.BrandManage,
            ResourcePermission.TeamManage when r.Kind==AccessResourceKind.Brand => RbacV2Action.TeamManage,
            ResourcePermission.ContentCreate when r.Kind==AccessResourceKind.Brand => RbacV2Action.ContentCreate,
            ResourcePermission.ContentView when r.Kind==AccessResourceKind.Content => RbacV2Action.ContentRead,
            ResourcePermission.ContentEdit when r.Kind==AccessResourceKind.Content => RbacV2Action.ContentEdit,
            ResourcePermission.ContentDelete when r.Kind==AccessResourceKind.Content => RbacV2Action.ContentDelete,
            ResourcePermission.ApprovalReview when r.Kind==AccessResourceKind.Content => RbacV2Action.ApprovalReview,
            ResourcePermission.ApprovalWithdraw when r.Kind==AccessResourceKind.Content => RbacV2Action.ApprovalWithdraw,
            ResourcePermission.PostPublish when r.Kind==AccessResourceKind.Content => RbacV2Action.Publish,
            ResourcePermission.PostView when r.Kind==AccessResourceKind.Post => RbacV2Action.ContentRead,
            ResourcePermission.SocialView when r.Kind==AccessResourceKind.Channel => RbacV2Action.SocialRead,
            ResourcePermission.SocialManage when r.Kind==AccessResourceKind.Channel => RbacV2Action.SocialManage,
            ResourcePermission.AnalyticsView when r.Kind==AccessResourceKind.Brand => RbacV2Action.AnalyticsRead,
            ResourcePermission.AnalyticsMember when r.Kind==AccessResourceKind.Brand => RbacV2Action.MemberPerformance,
            _=>null
        };
        if(action is null) return AccessDecision.Denied;
        Guid? brand=r.Kind==AccessResourceKind.Brand?r.ResourceId:null;
        Guid? content=r.Kind==AccessResourceKind.Content?r.ResourceId:null;
        var channel=r.ChannelId;
        if(r.Kind==AccessResourceKind.Channel)
        { var i=await db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(i=>i.Id==r.ResourceId && i.WorkspaceId==r.WorkspaceId && !i.IsDeleted,ct);if(i is null)return AccessDecision.Hidden;brand=i.BrandId;channel=i.Id; }
        if(r.Kind==AccessResourceKind.Post)
        { var p=await db.Posts.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(p=>p.Id==r.ResourceId && !p.IsDeleted,ct);if(p is null)return AccessDecision.Hidden;content=p.ContentId;channel=p.IntegrationId; }
        // A concrete content always resolves its own team. Brand reads may union eligible teams.
        if(content.HasValue || r.TeamId.HasValue || action is RbacV2Action.BillingManage)
            return await resolver.CheckAsync(new(r.ActorId,r.WorkspaceId,action.Value,brand,content,r.TeamId,channel,r.MemberId),ct);
        if(action==RbacV2Action.ContentCreate) return AccessDecision.Denied; // callers must supply the selected TeamId
        var context=await resolver.ContextAsync(r.ActorId,r.WorkspaceId,ct);
        if(context is null) return AccessDecision.Hidden;
        if(context.WorkspaceRole is "Owner" or "WorkspaceManager")
            return await resolver.CheckAsync(new(r.ActorId,r.WorkspaceId,action.Value,brand,ChannelId:channel,MemberId:r.MemberId),ct);
        foreach(var scope in context.Scopes.Where(s=>s.BrandId==brand))
        {
            var decision=await resolver.CheckAsync(new(r.ActorId,r.WorkspaceId,action.Value,brand,TeamId:scope.TeamId,ChannelId:channel,MemberId:r.MemberId),ct);
            if(decision.Allowed)return decision;
        }
        return context.Scopes.Any(s=>s.BrandId==brand)?AccessDecision.Denied:AccessDecision.Hidden;
    }
}
