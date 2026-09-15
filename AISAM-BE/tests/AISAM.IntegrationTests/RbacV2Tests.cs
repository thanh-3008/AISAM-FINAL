using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AISAM.IntegrationTests;

public class RbacV2Tests
{
    [Fact]
    public async Task ConversationHistoryRequiresCreatorAndCurrentWriteTeamScope()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var workspace = Guid.NewGuid(); var actor = Guid.NewGuid(); var a = Guid.NewGuid(); var b = Guid.NewGuid(); var brand = Guid.NewGuid();
        var own = new Conversation { WorkspaceId = workspace, CreatedByUserId = actor, BrandId = brand, TeamId = a };
        db.AddRange(new Workspace { Id = workspace }, new Brand { Id = brand, WorkspaceId = workspace },
            new Team { Id = a, WorkspaceId = workspace }, new Team { Id = b, WorkspaceId = workspace });
        db.AddRange(own, new Conversation { WorkspaceId = workspace, CreatedByUserId = actor, BrandId = brand, TeamId = b },
            new Conversation { WorkspaceId = workspace, CreatedByUserId = actor, BrandId = brand },
            new Conversation { WorkspaceId = workspace, CreatedByUserId = Guid.NewGuid(), BrandId = brand, TeamId = a },
            new TeamBrand { TeamId = a, BrandId = brand }, new TeamBrand { TeamId = b, BrandId = brand });
        await db.SaveChangesAsync();
        db.PermissionScopeEnabled = db.PermissionV2Enabled = true;
        db.PermissionWorkspaceId = workspace; db.PermissionActorId = actor;
        db.PermissionWriteTeamIds = [a]; db.PermissionTeamIds = [a,b];
        Assert.Equal(new[] {own.Id}, await db.Conversations.Select(c => c.Id).ToArrayAsync());
        db.PermissionWriteTeamIds = [];
        Assert.Empty(await db.Conversations.ToListAsync());
    }
    [Theory]
    [InlineData(TeamRoleEnum.Manager,ContentStatusEnum.PendingApproval,true,false)]
    [InlineData(TeamRoleEnum.ContentCreator,ContentStatusEnum.PendingApproval,false,false)]
    [InlineData(TeamRoleEnum.Viewer,ContentStatusEnum.PendingApproval,false,false)]
    [InlineData(TeamRoleEnum.Manager,ContentStatusEnum.Approved,false,true)]
    [InlineData(TeamRoleEnum.Manager,ContentStatusEnum.Published,false,true)]
    [InlineData(TeamRoleEnum.ContentCreator,ContentStatusEnum.Approved,false,false)]
    public void ReviewAndPublishRequireRoleAndState(TeamRoleEnum role,ContentStatusEnum status,bool review,bool publish)
    {
        var f=new RbacV2Facts(WorkspaceRoleV2.Member,WorkspaceStatusEnum.Active,true,true,true,role,true,true,true,status,true,true);
        Assert.Equal(review,RbacV2Policy.Allows(f,RbacV2Action.ApprovalReview));
        Assert.Equal(publish,RbacV2Policy.Allows(f,RbacV2Action.Publish));
        Assert.False(RbacV2Policy.Allows(f,RbacV2Action.SocialManage));
        Assert.False(RbacV2Policy.Allows(f with {TeamActive=false},RbacV2Action.Publish));
        Assert.False(RbacV2Policy.Allows(f with {ChannelGranted=false},RbacV2Action.Publish));
        Assert.False(RbacV2Policy.Allows(f with {WorkspaceStatus=WorkspaceStatusEnum.Limited},RbacV2Action.Publish));
    }

    [Fact]
    public void PostgresScopedQueryTranslates()
    {
        using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql("Host=localhost;Database=unused").Options);
        var snapshot = db.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
        snapshot = db.GetService<IModelRuntimeInitializer>().Initialize(snapshot, designTime: true);
        var differences = db.GetService<IMigrationsModelDiffer>().GetDifferences(snapshot.GetRelationalModel(), db.GetService<IDesignTimeModel>().Model.GetRelationalModel());
        Assert.True(differences.Count == 0, string.Join("\n", db.GetService<IMigrationsSqlGenerator>().Generate(differences).Select(c => c.CommandText)));
        db.PermissionScopeEnabled=db.PermissionV2Enabled=true;
        var sql=db.Contents.ToQueryString();
        Assert.Contains("team_id",sql);
        Assert.Contains("team_id", db.Conversations.ToQueryString());
        Assert.Contains("status",sql);
        Assert.Contains("content_media",db.Assets.ToQueryString());
        Assert.Contains("approved_snapshot_id",db.PublishSnapshots.ToQueryString());
        Assert.Contains("team_brand",db.Posts.ToQueryString());
    }
    [Theory]
    [InlineData(WorkspaceRoleV2.Owner,true,true)]
    [InlineData(WorkspaceRoleV2.WorkspaceManager,true,false)]
    [InlineData(WorkspaceRoleV2.Member,false,false)]
    public void BillingAndBoundary(WorkspaceRoleV2 role,bool read,bool manage)
    {
        var f=new RbacV2Facts(role,WorkspaceStatusEnum.Active,true,true,true,TeamRoleEnum.Manager,true,true,true,ContentStatusEnum.Approved,true,true);
        Assert.Equal(read,RbacV2Policy.Allows(f,RbacV2Action.BillingRead));
        Assert.Equal(manage,RbacV2Policy.Allows(f,RbacV2Action.BillingManage));
        foreach(var action in Enum.GetValues<RbacV2Action>())
        {
            Assert.False(RbacV2Policy.Allows(f with {ActiveMembership=false},action));
            Assert.False(RbacV2Policy.Allows(f with {SameWorkspace=false},action));
            Assert.False(RbacV2Policy.Allows(f with {WorkspaceRole=null},action));
        }
    }

    [Fact]
    public async Task MixedTeamsChannelsAndRevocation()
    {
        var options=new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db=new AisamContext(options);
        var user=new User{Email="rbac@example.test",IsActive=true};
        var w=new Workspace(); var b=new Brand{WorkspaceId=w.Id};
        var a=new Team{WorkspaceId=w.Id}; var t=new Team{WorkspaceId=w.Id};
        var wm=new WorkspaceMember{UserId=user.Id,WorkspaceId=w.Id,Role=WorkspaceMemberRoleEnum.Owner,WorkspaceRoleV2=WorkspaceRoleV2.Member};
        var ta=new TeamBrand{TeamId=a.Id,BrandId=b.Id}; var tb=new TeamBrand{TeamId=t.Id,BrandId=b.Id};
        var ca=new Content{WorkspaceId=w.Id,BrandId=b.Id,TeamId=a.Id,Status=ContentStatusEnum.PendingApproval};
        var cb=new Content{WorkspaceId=w.Id,BrandId=b.Id,TeamId=t.Id,Status=ContentStatusEnum.Draft};
        var channel=new SocialIntegration{WorkspaceId=w.Id,BrandId=b.Id};
        var grant=new TeamChannelAccess{TeamBrandId=tb.Id,IntegrationId=channel.Id,CanView=true,ScopeEnabledV2=true};
        db.AddRange(user,w,b,a,t,wm,ta,tb,ca,cb,channel,grant,
            new TeamMember{TeamId=a.Id,UserId=user.Id,Role=TeamRoleEnum.Manager},
            new TeamMember{TeamId=t.Id,UserId=user.Id,Role=TeamRoleEnum.Viewer,Permissions=[DelegatedPermissionKeys.Review,DelegatedPermissionKeys.Publish]});
        await db.SaveChangesAsync();
        var service=new RbacV2AccessResolver(db);
        var adapter=new RbacV2AccessAdapter(db,service);
        var http=new Microsoft.AspNetCore.Http.DefaultHttpContext {
            User=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier,user.Id.ToString())],"test")) };
        http.Items[AISAM.API.Utils.WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey]=wm;
        var middleware=new AISAM.API.Middleware.PermissionScopeMiddleware(async _=> {
            Assert.False(db.PermissionManager);
            Assert.False(db.PermissionOwner); // legacy Owner must not leak into v2
            Assert.Equal(new[]{ca.Id},await db.Contents.Select(c=>c.Id).ToArrayAsync());
        });
        await middleware.InvokeAsync(http,db,adapter);
        Assert.False(db.PermissionV2Enabled);
        Assert.False(db.PermissionScopeEnabled);
        Assert.True((await adapter.CheckAsync(new(user.Id,w.Id,AccessResourceKind.Content,ca.Id,ResourcePermission.ApprovalReview))).Allowed);
        Assert.False((await adapter.CheckAsync(new(user.Id,w.Id,AccessResourceKind.Content,cb.Id,ResourcePermission.ApprovalReview))).Allowed);
        Task<AccessDecision> Check(Content c,RbacV2Action action,Guid? ch=null,Guid? team=null)=>service.CheckAsync(new(user.Id,w.Id,action,ContentId:c.Id,ChannelId:ch,TeamId:team));
                db.PermissionScopeEnabled=true;db.PermissionV2Enabled=true;db.PermissionWorkspaceId=w.Id;
        db.PermissionActorId=user.Id;db.PermissionTeamIds=[a.Id,t.Id];db.PermissionWriteTeamIds=[a.Id];
        db.PermissionManagerTeamIds=[a.Id];db.PermissionBrandIds=[b.Id];
        Assert.Equal(new[]{ca.Id},await db.Contents.Select(c=>c.Id).ToArrayAsync());
        db.PermissionScopeEnabled=false;db.PermissionV2Enabled=false;
        var revision=(await service.ContextAsync(user.Id,w.Id))!.Revision;
        Assert.True((await Check(ca,RbacV2Action.ApprovalReview)).Allowed);
        Assert.False((await Check(cb,RbacV2Action.ApprovalReview)).Allowed);
        Assert.False((await Check(cb,RbacV2Action.ContentRead,team:a.Id)).Allowed);
        Assert.Equal(new[]{ca.Id},await (await service.VisibleContentsAsync(user.Id,w.Id)).Select(c=>c.Id).ToArrayAsync());
        ca.Status=ContentStatusEnum.Approved; cb.Status=ContentStatusEnum.Approved; await db.SaveChangesAsync();
        Assert.Equal(2,await (await service.VisibleContentsAsync(user.Id,w.Id)).CountAsync());
        var oldSnapshot=new PublishSnapshot{ContentId=cb.Id,WorkspaceId=w.Id,Payload="old draft"};
        db.Add(oldSnapshot);await db.SaveChangesAsync();
        db.PermissionScopeEnabled=db.PermissionV2Enabled=true;
        Assert.DoesNotContain(oldSnapshot.Id,await db.PublishSnapshots.Select(s=>s.Id).ToArrayAsync());
        Assert.Contains(cb.ApprovedSnapshotId!.Value,await db.PublishSnapshots.Select(s=>s.Id).ToArrayAsync());
        db.PermissionScopeEnabled=db.PermissionV2Enabled=false;
        Assert.False((await Check(ca,RbacV2Action.Publish,channel.Id)).Allowed); // grant belongs to B, role to A
        Assert.False((await Check(cb,RbacV2Action.Publish,channel.Id)).Allowed); // Viewer even with legacy Publish
        var ownGrant=new TeamChannelAccess{TeamBrandId=ta.Id,IntegrationId=channel.Id,CanView=true,ScopeEnabledV2=true};
        db.Add(ownGrant); await db.SaveChangesAsync();
        var postA=new Post{ContentId=ca.Id,IntegrationId=channel.Id,Status=ContentStatusEnum.Published};
        var postB=new Post{ContentId=cb.Id,IntegrationId=channel.Id,Status=ContentStatusEnum.Published};
        db.AddRange(postA,postB);grant.ScopeEnabledV2=false;await db.SaveChangesAsync();
        db.PermissionScopeEnabled=db.PermissionV2Enabled=true;
        Assert.Equal(new[]{postA.Id},await db.Posts.Select(p=>p.Id).ToArrayAsync());
        db.PermissionScopeEnabled=db.PermissionV2Enabled=false;
        Assert.True((await Check(ca,RbacV2Action.Publish,channel.Id)).Allowed);
        ca.Status=ContentStatusEnum.Published;await db.SaveChangesAsync();
        Assert.True((await Check(ca,RbacV2Action.Publish,channel.Id)).Allowed);
        db.ExecutionSnapshotId=Guid.NewGuid();
        Assert.False((await adapter.CheckAsync(new(user.Id,w.Id,AccessResourceKind.Content,ca.Id,ResourcePermission.PostPublish,channel.Id))).Allowed);
        db.ExecutionSnapshotId=null;
        revision=(await service.ContextAsync(user.Id,w.Id))!.Revision;
        await using(var other=new AisamContext(options))
        { var g=await other.TeamChannelAccesses.SingleAsync(g=>g.Id==ownGrant.Id);g.ScopeEnabledV2=false;await other.SaveChangesAsync(); }
        Assert.False((await Check(ca,RbacV2Action.Publish,channel.Id)).Allowed);
        Assert.NotEqual(revision,(await service.ContextAsync(user.Id,w.Id))!.Revision);
        Assert.False(await VideoJobAccess.CanRunAsync(db,adapter,user.Id,w.Id,default));
        wm.WorkspaceRoleV2=WorkspaceRoleV2.Owner;await db.SaveChangesAsync();
        Assert.True(await VideoJobAccess.CanRunAsync(db,adapter,user.Id,w.Id,default));
        ca.ApprovedSnapshotId=null;await db.SaveChangesAsync();
        Assert.False((await Check(ca,RbacV2Action.Publish,channel.Id)).Allowed);
        wm.IsActive=false;await db.SaveChangesAsync();
        Assert.False(await VideoJobAccess.CanRunAsync(db,adapter,user.Id,w.Id,default));
        Assert.Empty(await (await service.VisibleContentsAsync(user.Id,w.Id)).ToArrayAsync());
    }
}
