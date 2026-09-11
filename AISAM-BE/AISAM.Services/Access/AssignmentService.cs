using System.Data;
using System.Security.Cryptography;
using System.Text;
using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Access;

public sealed record AssignmentChange(Guid ActorId, Guid WorkspaceId, Guid BrandId, Guid TeamId,
    string ExpectedRevision, bool Active, Guid? IntegrationId = null,
    bool CanView = false, bool CanPublish = false, bool CanManage = false);
public sealed record AssignmentSnapshot(string Revision, IReadOnlyList<TeamBrand> Teams, IReadOnlyList<TeamChannelAccess> Channels);

public sealed class AssignmentService(AisamContext db, IAccessControlService access)
{
    public async Task<AssignmentSnapshot> ReadAsync(Guid actor, Guid workspace, Guid brand, CancellationToken ct = default)
    {
        var decision=await access.CheckAsync(new(actor,workspace,AccessResourceKind.Brand,brand,ResourcePermission.BrandManage),ct);
        if(!decision.Allowed) throw new AssignmentAccessException(decision);
        return await VisibleSnapshot(actor,workspace,await Snapshot(brand,ct),ct);
    }

    private async Task<AssignmentSnapshot> VisibleSnapshot(Guid actor,Guid workspace,AssignmentSnapshot snapshot,CancellationToken ct)
    {
        if(await db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().AnyAsync(m=>m.WorkspaceId==workspace && m.UserId==actor && m.IsActive && m.Role==WorkspaceMemberRoleEnum.Owner,ct)) return snapshot;
        var ids=await (from m in db.TeamMembers.IgnoreQueryFilters().AsNoTracking()
            join t in db.Teams.IgnoreQueryFilters().AsNoTracking() on m.TeamId equals t.Id
            where m.UserId==actor && m.IsActive && t.WorkspaceId==workspace && !t.IsDeleted && t.Status==TeamStatusEnum.Active select t.Id).ToListAsync(ct);
        var teams=snapshot.Teams.Where(t=>ids.Contains(t.TeamId)).ToArray();
        var assignments=teams.Select(t=>t.Id).ToArray();
        return snapshot with {Teams=teams,Channels=snapshot.Channels.Where(c=>assignments.Contains(c.TeamBrandId) && c.CanView).ToArray()};
    }

    private async Task<AssignmentSnapshot> Snapshot(Guid brand, CancellationToken ct)
    {
        var teams=await db.TeamBrands.AsNoTracking().Where(t=>t.BrandId==brand).OrderBy(t=>t.Id).ToListAsync(ct);
        var ids=teams.Select(t=>t.Id).ToArray();
        var channels=await db.TeamChannelAccesses.AsNoTracking().Where(c=>ids.Contains(c.TeamBrandId)).OrderBy(c=>c.Id).ToListAsync(ct);
        // Full canonical state; no collision-prone timestamps or client revision counters.
        var state=string.Join(";",teams.Select(t=>$"{t.Id:N}:{t.TeamId:N}:{t.IsActive}:{t.AssignedAt.Ticks}")) + "|" +
            string.Join(";",channels.Select(c=>$"{c.Id:N}:{c.TeamBrandId:N}:{c.IntegrationId:N}:{c.CanView}:{c.CanPublish}:{c.CanManage}"));
        return new(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(state))),teams,channels);
    }

    public async Task<AssignmentSnapshot> ChangeAsync(AssignmentChange request, CancellationToken ct = default)
    {
        var attempted=false;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            // A serialization retry must not replay tracked mutations or audit rows.
            // Ask the client to reload a revision instead of retrying a grant silently.
            if(attempted) throw new AssignmentConflictException();
            attempted=true;
            return await ChangeCoreAsync(request,ct);
        });
    }

    private async Task<AssignmentSnapshot> ChangeCoreAsync(AssignmentChange request, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(request.ExpectedRevision)) throw new ArgumentException("Expected revision is required.");
        if((request.CanPublish || request.CanManage) && !request.CanView) throw new ArgumentException("Publish/manage requires view.");
        // Serializable transaction includes authorization, read revision, mutation and audit.
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var current=await ReadAsync(request.ActorId,request.WorkspaceId,request.BrandId,ct);
        if(current.Revision!=request.ExpectedRevision) throw new AssignmentConflictException();
        var team=await db.Teams.AsNoTracking().SingleOrDefaultAsync(t=>t.Id==request.TeamId && t.WorkspaceId==request.WorkspaceId && !t.IsDeleted && t.Status==TeamStatusEnum.Active,ct);
        if(team is null) throw new AssignmentAccessException(AccessDecision.Hidden);
        var member=await db.WorkspaceMembers.AsNoTracking().SingleAsync(m=>m.UserId==request.ActorId && m.WorkspaceId==request.WorkspaceId && m.IsActive,ct);
        if(member.Role!=WorkspaceMemberRoleEnum.Owner && !await db.TeamMembers.AsNoTracking().AnyAsync(m=>m.TeamId==team.Id && m.UserId==request.ActorId && m.IsActive,ct))
            throw new AssignmentAccessException(AccessDecision.Denied);
        var assignment=await db.TeamBrands.SingleOrDefaultAsync(t=>t.TeamId==team.Id && t.BrandId==request.BrandId,ct);
        if(request.IntegrationId is { } channelId)
        {
            var channel=await db.SocialIntegrations.AsNoTracking().SingleOrDefaultAsync(i=>i.Id==channelId && i.WorkspaceId==request.WorkspaceId && i.BrandId==request.BrandId && !i.IsDeleted,ct);
            if(channel is null || assignment is null || !assignment.IsActive) throw new AssignmentAccessException(AccessDecision.Hidden);
            if(member.Role!=WorkspaceMemberRoleEnum.Owner)
            {
                var manage=await access.CheckAsync(new(request.ActorId,request.WorkspaceId,AccessResourceKind.Channel,channelId,ResourcePermission.SocialManage),ct);
                if(!manage.Allowed) throw new AssignmentAccessException(manage);
                // Manager may grant only rights already held across their own valid assignments.
                var own=from g in db.TeamChannelAccesses.AsNoTracking()
                    join b in db.TeamBrands.AsNoTracking() on g.TeamBrandId equals b.Id
                    join t in db.Teams.AsNoTracking() on b.TeamId equals t.Id
                    join m in db.TeamMembers.AsNoTracking() on t.Id equals m.TeamId
                    where g.IntegrationId==channelId && b.BrandId==request.BrandId && b.IsActive && t.WorkspaceId==request.WorkspaceId && !t.IsDeleted && t.Status==TeamStatusEnum.Active && m.UserId==request.ActorId && m.IsActive
                    select g;
                if(request.CanPublish && !await own.AnyAsync(g=>g.CanView && g.CanPublish,ct)) throw new AssignmentAccessException(AccessDecision.Denied);
            }
            var grant=await db.TeamChannelAccesses.SingleOrDefaultAsync(g=>g.TeamBrandId==assignment.Id && g.IntegrationId==channelId,ct);
            if(grant is null) { grant=new(){TeamBrandId=assignment.Id,IntegrationId=channelId}; db.Add(grant); }
            grant.CanView=request.Active && request.CanView;
            grant.CanPublish=request.Active && request.CanPublish;
            grant.CanManage=request.Active && request.CanManage;
        }
        else
        {
            if (request.Active)
            {
                var hasManager = await db.TeamMembers.AsNoTracking()
                    .AnyAsync(m => m.TeamId == team.Id
                                && m.Role == "Manager"
                                && m.IsActive, ct);
                if (!hasManager)
                    throw new InvalidOperationException("TEAM_REQUIRES_MANAGER");
            }
            if(assignment is null) { assignment=new(){TeamId=team.Id,BrandId=request.BrandId}; db.Add(assignment); }
            assignment.IsActive=request.Active;
            assignment.AssignedAt=DateTime.UtcNow;
            // Revocation also clears channel grants so reactivation cannot restore stale powers.
            if(!request.Active)
                foreach(var g in await db.TeamChannelAccesses.Where(g=>g.TeamBrandId==assignment.Id).ToListAsync(ct))
                    g.CanView=g.CanPublish=g.CanManage=false;
        }
        db.AuditLogs.Add(new AuditLog { ActorId=request.ActorId,WorkspaceId=request.WorkspaceId,TeamId=request.TeamId,
            ActionType=request.Active?"permission.grant":"permission.revoke",TargetTable=request.IntegrationId.HasValue?"social_integrations":"brands",
            TargetId=request.IntegrationId??request.BrandId,Result="allowed",OldValues=System.Text.Json.JsonSerializer.Serialize(new {revision=current.Revision}),
            NewValues=System.Text.Json.JsonSerializer.Serialize(new {request.Active,request.CanView,request.CanPublish,request.CanManage}) });
        await db.SaveChangesAsync(ct);
        var result=await Snapshot(request.BrandId,ct);
        await tx.CommitAsync(ct);
        return await VisibleSnapshot(request.ActorId,request.WorkspaceId,result,ct);
    }
}
public sealed class AssignmentAccessException(AccessDecision decision) : Exception("Assignment access denied") { public AccessDecision Decision {get;}=decision; }
public sealed class AssignmentConflictException : Exception { }
