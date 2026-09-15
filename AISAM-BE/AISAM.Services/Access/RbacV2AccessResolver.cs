using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Access;

public sealed record RbacV2Request(Guid ActorId, Guid WorkspaceId, RbacV2Action Action,
    Guid? BrandId=null, Guid? ContentId=null, Guid? TeamId=null, Guid? ChannelId=null, Guid? MemberId=null);
public sealed record RbacV2Scope(Guid TeamId,Guid BrandId,string? Role,Guid[] ChannelIds);
public sealed record RbacV2Team(Guid TeamId,string? Role);
public sealed record RbacV2Context(int ContractVersion,string Revision,string WorkspaceRole,RbacV2Scope[] Scopes,
    RbacV2Team[] Teams,string[] Actions);

// No legacy-role fallback and no cached decisions. Callers supply the authenticated actor.
public sealed class RbacV2AccessResolver(AisamContext db)
{
    public async Task<RbacV2Context?> ContextAsync(Guid actor,Guid workspace,CancellationToken ct=default)
    {
        var membership=await Membership(actor,workspace,ct);
        if(membership is null)return null;
        var role=membership.Value.Member.WorkspaceRoleV2!.Value;
        var admin=role is WorkspaceRoleV2.Owner or WorkspaceRoleV2.WorkspaceManager;
        var rows=await (from tb in db.TeamBrands.IgnoreQueryFilters().AsNoTracking()
            join t in db.Teams.IgnoreQueryFilters().AsNoTracking() on tb.TeamId equals t.Id
            join b in db.Brands.IgnoreQueryFilters().AsNoTracking() on tb.BrandId equals b.Id
            where tb.IsActive && t.WorkspaceId==workspace && b.WorkspaceId==workspace && !t.IsDeleted && !b.IsDeleted && t.Status==TeamStatusEnum.Active
            select new {tb.Id,tb.TeamId,tb.BrandId}).ToListAsync(ct);
        var memberships=await db.TeamMembers.IgnoreQueryFilters().AsNoTracking().Where(m=>m.UserId==actor && m.IsActive).ToListAsync(ct);
        var channels=await (from g in db.TeamChannelAccesses.IgnoreQueryFilters().AsNoTracking()
            join i in db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking() on g.IntegrationId equals i.Id
            where g.ScopeEnabledV2 && i.WorkspaceId==workspace && !i.IsDeleted && i.IsActive
            select new {g.TeamBrandId,g.IntegrationId,i.BrandId}).ToListAsync(ct);
        var scopes=new List<RbacV2Scope>();
        foreach(var row in rows.OrderBy(r=>r.TeamId).ThenBy(r=>r.BrandId))
        {
            var tm=memberships.SingleOrDefault(m=>m.TeamId==row.TeamId);
            if(!admin && (tm is null || !Enum.IsDefined(tm.Role)))continue;
            scopes.Add(new(row.TeamId,row.BrandId,tm?.Role.ToString(),channels.Where(c=>c.TeamBrandId==row.Id && c.BrandId==row.BrandId).Select(c=>c.IntegrationId).Order().ToArray()));
        }
        if(admin)
        {
            var brands=await db.Brands.IgnoreQueryFilters().AsNoTracking().Where(b=>b.WorkspaceId==workspace && !b.IsDeleted).Select(b=>b.Id).OrderBy(id=>id).ToListAsync(ct);
            foreach(var brand in brands.Where(id=>!scopes.Any(s=>s.BrandId==id)))scopes.Add(new(Guid.Empty,brand,null,[]));
        }
        var activeTeams=await db.Teams.IgnoreQueryFilters().AsNoTracking().Where(t=>t.WorkspaceId==workspace && !t.IsDeleted && t.Status==TeamStatusEnum.Active).Select(t=>t.Id).OrderBy(id=>id).ToListAsync(ct);
        var teams=activeTeams.Where(id=>admin || memberships.Any(m=>m.TeamId==id && Enum.IsDefined(m.Role)))
            .Select(id=>new RbacV2Team(id,memberships.SingleOrDefault(m=>m.TeamId==id)?.Role.ToString())).ToArray();
        // Workspace-level capabilities only. Resource actions remain conditional on target/state.
        var actions=admin ? (role==WorkspaceRoleV2.Owner ? new[]{"billing.read","billing.manage","team.manage","brand.manage","social.manage"} : new[]{"billing.read","team.manage","brand.manage","social.manage"}) : Array.Empty<string>();
        var payload=System.Text.Json.JsonSerializer.Serialize(new{actor,workspace,role,membership.Value.Workspace.Status,scopes,teams,actions});
        var revision=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload)));
        return new(2,revision,role.ToString(),scopes.ToArray(),teams,actions);
    }
    private async Task<(WorkspaceMember Member, Workspace Workspace)?> Membership(Guid actor, Guid workspace, CancellationToken ct)
    {
        if(actor==Guid.Empty || workspace==Guid.Empty || !await db.Users.IgnoreQueryFilters().AnyAsync(u=>u.Id==actor && u.IsActive,ct)) return null;
        var m=await db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(m=>m.WorkspaceId==workspace && m.UserId==actor && m.IsActive,ct);
        var w=await db.Workspaces.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(w=>w.Id==workspace,ct);
        if(m?.WorkspaceRoleV2 is not { } role || !Enum.IsDefined(role) || w is null || !Enum.IsDefined(w.Status) || w.Status==WorkspaceStatusEnum.Deleted) return null;
        WorkspaceLifecyclePolicy.SynchronizeStatus(w,DateTime.UtcNow);
        return (m,w);
    }

    public async Task<AccessDecision> CheckAsync(RbacV2Request r,CancellationToken ct=default)
    {
        var membership=await Membership(r.ActorId,r.WorkspaceId,ct);
        if(membership is null) return AccessDecision.Hidden;
        var (member,workspace)=membership.Value;
        var admin=member.WorkspaceRoleV2 is WorkspaceRoleV2.Owner or WorkspaceRoleV2.WorkspaceManager;
        var f=new RbacV2Facts(member.WorkspaceRoleV2,workspace.Status,true,true,false,null,false,false,false,null,false,r.MemberId==r.ActorId);
        if(r.Action is RbacV2Action.BillingRead or RbacV2Action.BillingManage)
            return Decide(f,r.Action);
        var content=r.ContentId is { } cid ? await db.Contents.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(c=>c.Id==cid && c.WorkspaceId==r.WorkspaceId && !c.IsDeleted,ct) : null;
        if(r.ContentId.HasValue && content is null) return AccessDecision.Hidden;
        if(r.Action==RbacV2Action.Publish && (content is null || content.ApprovedSnapshotId is null ||
            !await db.PublishSnapshots.IgnoreQueryFilters().AnyAsync(s=>s.Id==content.ApprovedSnapshotId && s.ContentId==content.Id &&
                s.WorkspaceId==r.WorkspaceId && s.Version==content.MediaVersion,ct))) return AccessDecision.Denied;
        if(r.Action is RbacV2Action.ContentRead or RbacV2Action.ContentEdit or RbacV2Action.ContentDelete or RbacV2Action.ApprovalReview or RbacV2Action.Publish && content is null) return AccessDecision.Hidden;
        var brandId=content?.BrandId ?? r.BrandId;
        if(content is not null && r.BrandId.HasValue && r.BrandId!=content.BrandId) return AccessDecision.Hidden;
        var brand=await db.Brands.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(b=>b.Id==brandId && b.WorkspaceId==r.WorkspaceId && !b.IsDeleted,ct);
        if(brand is null) return AccessDecision.Hidden;
        // Client TeamId cannot substitute a more privileged team for an existing content.
        var teamId=content is null ? r.TeamId : content.TeamId;
        if(content is not null && r.TeamId.HasValue && r.TeamId!=content.TeamId) return AccessDecision.Hidden;
        var team=await db.Teams.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(t=>t.Id==teamId && t.WorkspaceId==r.WorkspaceId && !t.IsDeleted && t.Status==TeamStatusEnum.Active,ct);
        var assignment=team is null ? null : await db.TeamBrands.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(t=>t.TeamId==team.Id && t.BrandId==brand.Id && t.IsActive,ct);
        var tm=team is null ? null : await db.TeamMembers.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(m=>m.TeamId==team.Id && m.UserId==r.ActorId && m.IsActive,ct);
        if(!admin && (assignment is null || tm is null || !Enum.IsDefined(tm.Role))) return AccessDecision.Hidden;
        SocialIntegration? channel=null;
        var granted=false;
        if(r.ChannelId is { } channelId)
        {
            channel=await db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(i=>i.Id==channelId && i.BrandId==brand.Id && i.WorkspaceId==r.WorkspaceId && !i.IsDeleted,ct);
            if(channel is null) return AccessDecision.Hidden;
            granted=assignment is not null && await db.TeamChannelAccesses.IgnoreQueryFilters().AnyAsync(g=>g.TeamBrandId==assignment.Id && g.IntegrationId==channelId && g.ScopeEnabledV2,ct);
        }
        if(r.Action is RbacV2Action.SocialRead or RbacV2Action.SocialManage or RbacV2Action.ProviderAnalytics or RbacV2Action.Publish && channel is null) return AccessDecision.Hidden;
        if(r.Action==RbacV2Action.MemberPerformance)
        {
            if(r.MemberId is not { } target || !await db.WorkspaceMembers.IgnoreQueryFilters().AnyAsync(m=>m.UserId==target && m.WorkspaceId==r.WorkspaceId && m.IsActive,ct)) return AccessDecision.Hidden;
            if(!admin && !await db.TeamMembers.IgnoreQueryFilters().AnyAsync(m=>m.TeamId==teamId && m.UserId==target && m.IsActive,ct)) return AccessDecision.Hidden;
        }
        f=f with { TeamRole=tm?.Role, TeamActive=team is not null, BrandLinked=assignment is not null,
            ChannelGranted=granted,ChannelActive=channel?.IsActive==true,ContentStatus=content?.Status,OwnContent=content?.PrimaryCreatorId==r.ActorId };
        if(content is not null && !RbacV2Policy.Allows(f,RbacV2Action.ContentRead)) return AccessDecision.Hidden;
        return Decide(f,r.Action);
    }

    private static AccessDecision Decide(RbacV2Facts f,RbacV2Action action)=>RbacV2Policy.Allows(f,action)?AccessDecision.Permit:new(false,403,"ACTION_NOT_ALLOWED");

    // Database-side predicate preserves mixed-role visibility instead of a global Manager flag.
    // Caller must compose pagination/aggregates on this query, never on an unscoped source.
    public async Task<IQueryable<Content>> VisibleContentsAsync(Guid actor,Guid workspace,CancellationToken ct=default)
    {
        var membership=await Membership(actor,workspace,ct);
        var contents=db.Contents.IgnoreQueryFilters().AsNoTracking().Where(c=>c.WorkspaceId==workspace && !c.IsDeleted);
        if(membership is null) return contents.Where(c=>false);
        if(membership.Value.Member.WorkspaceRoleV2 is WorkspaceRoleV2.Owner or WorkspaceRoleV2.WorkspaceManager) return contents;
        return contents.Where(c=>db.Brands.IgnoreQueryFilters().Any(b=>b.Id==c.BrandId && b.WorkspaceId==workspace && !b.IsDeleted) &&
            db.Teams.IgnoreQueryFilters().Any(t=>t.Id==c.TeamId && t.WorkspaceId==workspace && !t.IsDeleted && t.Status==TeamStatusEnum.Active &&
                db.TeamBrands.IgnoreQueryFilters().Any(tb=>tb.TeamId==t.Id && tb.BrandId==c.BrandId && tb.IsActive) &&
                db.TeamMembers.IgnoreQueryFilters().Any(tm=>tm.TeamId==t.Id && tm.UserId==actor && tm.IsActive &&
                    (tm.Role==TeamRoleEnum.Manager || tm.Role==TeamRoleEnum.ContentCreator || tm.Role==TeamRoleEnum.Viewer &&
                        (c.Status==ContentStatusEnum.Approved || c.Status==ContentStatusEnum.Published)))));
    }
}
