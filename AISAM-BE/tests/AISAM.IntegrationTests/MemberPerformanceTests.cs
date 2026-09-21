using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;
public class MemberPerformanceTests
{
    [Fact]
    public async Task V2MixedRolesDoNotAggregateAnotherTeamSharingTheBrand()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var from=DateTime.UtcNow.AddDays(-1);var to=DateTime.UtcNow.AddDays(1);
        var w=new Workspace();var b=new Brand{WorkspaceId=w.Id};var unassigned=new Brand{WorkspaceId=w.Id};
        var actor=new User{FullName="Mixed role"};var other=new User{FullName="Colleague"};var viewer=new User{FullName="Viewer"};
        var a=new Team{WorkspaceId=w.Id};var v=new Team{WorkspaceId=w.Id};var c=new Team{WorkspaceId=w.Id};
        var wm=new WorkspaceMember{WorkspaceId=w.Id,UserId=actor.Id,Role=WorkspaceMemberRoleEnum.Owner,WorkspaceRoleV2=WorkspaceRoleV2.Member};
        db.AddRange(w,b,unassigned,actor,other,viewer,a,v,c,wm,
            new WorkspaceMember{WorkspaceId=w.Id,UserId=other.Id,WorkspaceRoleV2=WorkspaceRoleV2.Member},
            new WorkspaceMember{WorkspaceId=w.Id,UserId=viewer.Id,WorkspaceRoleV2=WorkspaceRoleV2.Member},
            new TeamBrand{TeamId=a.Id,BrandId=b.Id},new TeamBrand{TeamId=v.Id,BrandId=b.Id},new TeamBrand{TeamId=c.Id,BrandId=b.Id},
            new TeamMember{TeamId=a.Id,UserId=actor.Id,Role=TeamRoleEnum.Manager},
            new TeamMember{TeamId=v.Id,UserId=actor.Id,Role=TeamRoleEnum.Viewer},
            new TeamMember{TeamId=c.Id,UserId=actor.Id,Role=TeamRoleEnum.ContentCreator},
            new TeamMember{TeamId=a.Id,UserId=viewer.Id,Role=TeamRoleEnum.Viewer});
        foreach(var team in new[]{a,v,c}) db.AddRange(new TeamMember{TeamId=team.Id,UserId=other.Id,Role=TeamRoleEnum.ContentCreator},
            new Content{WorkspaceId=w.Id,BrandId=b.Id,TeamId=team.Id,PrimaryCreatorId=other.Id});
        db.AddRange(new Content{WorkspaceId=w.Id,BrandId=b.Id,TeamId=c.Id,PrimaryCreatorId=actor.Id},
            new Content{WorkspaceId=w.Id,BrandId=unassigned.Id,PrimaryCreatorId=other.Id});
        await db.SaveChangesAsync();
        var service=new MemberPerformanceService(db,new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db)));
        var teamChoice=await service.GetAsync(actor.Id,w.Id,from,to,memberId:other.Id);
        Assert.True(teamChoice.TeamSelectionRequired);Assert.Empty(teamChoice.Items);Assert.False(teamChoice.CanViewAllTeams);
        var report=await service.GetAsync(actor.Id,w.Id,from,to,teamId:a.Id,memberId:other.Id);
        Assert.Equal(1,Assert.Single(report.Items).ContentsCreated);
        db.PermissionActorId=actor.Id;
        var dashboard=new AISAM.Services.Service.WorkspaceDashboardService(null!,null!,null!,null!,null!,new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db)),db);
        Assert.Equal(403,(await dashboard.GetSummaryAsync(w.Id)).StatusCode); // before wallet/member queries
        var controller=new AISAM.API.Controllers.MemberPerformanceController(service);
        var http=new Microsoft.AspNetCore.Http.DefaultHttpContext();
        http.User=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier,actor.Id.ToString())],"test"));
        http.Items[AISAM.API.Utils.WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey]=wm;
        http.Items[AISAM.API.Utils.WorkspaceContextHelper.ActiveWorkspaceItemKey]=w.Id;
        controller.ControllerContext=new Microsoft.AspNetCore.Mvc.ControllerContext{HttpContext=http};
        var export=Assert.IsType<Microsoft.AspNetCore.Mvc.FileContentResult>(await controller.Export(from,to,teamId:a.Id,memberId:other.Id));
        using(var json=System.Text.Json.JsonDocument.Parse(export.FileContents))
            Assert.Equal(1,json.RootElement.GetProperty("items")[0].GetProperty("contentsCreated").GetInt32());
        Assert.Single(report.Teams);Assert.Equal(a.Id,report.Teams[0].Id);
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(actor.Id,w.Id,from,to,teamId:v.Id))).StatusCode);
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(actor.Id,w.Id,from,to,teamId:c.Id,memberId:other.Id))).StatusCode);
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(actor.Id,w.Id,from,to,teamId:c.Id,memberId:actor.Id))).StatusCode);
        Assert.Equal(403,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(viewer.Id,w.Id,from,to))).StatusCode);
        wm.WorkspaceRoleV2=WorkspaceRoleV2.WorkspaceManager;await db.SaveChangesAsync();
        Assert.Equal(4,Assert.Single((await service.GetAsync(actor.Id,w.Id,from,to,memberId:other.Id)).Items).ContentsCreated);
        wm.IsActive=false;await db.SaveChangesAsync();
        Assert.Equal(403,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(actor.Id,w.Id,from,to))).StatusCode);
    }
    [Fact]
    public async Task AggregatesKnownFixtureWithoutSnapshotsOrRetryDoubleCountingAndProtectsScope()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var start=new DateTime(2026,9,1,0,0,0,DateTimeKind.Utc);var end=start.AddDays(7);
        var owner=new User{FullName="Owner"};var creator=new User{FullName="Creator"};var manager=new User{FullName="Manager"};var viewer=new User();
        var w=new Workspace();var brand=new Brand{WorkspaceId=w.Id};var hidden=new Brand{WorkspaceId=w.Id};var team=new Team{WorkspaceId=w.Id};
        db.AddRange(w,owner,creator,manager,viewer,brand,hidden,team,
            new WorkspaceMember{WorkspaceId=w.Id,UserId=owner.Id,Role=WorkspaceMemberRoleEnum.Owner},
            new WorkspaceMember{WorkspaceId=w.Id,UserId=creator.Id,Role=WorkspaceMemberRoleEnum.ContentCreator},
            new WorkspaceMember{WorkspaceId=w.Id,UserId=manager.Id,Role=WorkspaceMemberRoleEnum.Manager},
            new WorkspaceMember{WorkspaceId=w.Id,UserId=viewer.Id,Role=WorkspaceMemberRoleEnum.Viewer},
            new TeamBrand{TeamId=team.Id,BrandId=brand.Id},new TeamMember{TeamId=team.Id,UserId=creator.Id},new TeamMember{TeamId=team.Id,UserId=manager.Id,Role=TeamRoleEnum.Manager});
        var c=new Content{WorkspaceId=w.Id,BrandId=brand.Id,PrimaryCreatorId=creator.Id,TeamId=team.Id,CreatedAt=start};
        var other=new Content{WorkspaceId=w.Id,BrandId=hidden.Id,PrimaryCreatorId=creator.Id,CreatedAt=start};
        var outsidePeriod=new Content{WorkspaceId=w.Id,BrandId=brand.Id,PrimaryCreatorId=creator.Id,CreatedAt=end};
        var channel=new SocialIntegration{WorkspaceId=w.Id,BrandId=brand.Id};
        var snapshotId=Guid.NewGuid();
        var post=new Post{ContentId=c.Id,IntegrationId=channel.Id,PublishedAt=start.AddDays(1),ExternalPostId="same"};
        db.AddRange(c,other,outsidePeriod,channel,post,
            new PublishOperation{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,SnapshotId=snapshotId,ActorId=owner.Id,ProviderId="same",Status="Published"},
            new Content{WorkspaceId=w.Id,BrandId=brand.Id,CreatedAt=start},
            new Post{ContentId=c.Id,IntegrationId=channel.Id,PublishedAt=start.AddDays(1).AddSeconds(1),PublishedByUserId=owner.Id,ExternalPostId="same"},
            new PerformanceReport{PostId=post.Id,ReportDate=start,Engagement=10,Impressions=100,Reach=70},
            new PerformanceReport{PostId=post.Id,ReportDate=start.AddDays(2),Engagement=30,Impressions=200,Reach=120},
            new PerformanceReport{PostId=post.Id,ReportDate=start.AddDays(3),RawData="{\"trackedClicks\":1}"},
            new Approval{ContentId=c.Id,ApproverUserId=manager.Id,SubmittedAt=start,ApprovedAt=start.AddHours(2),Status=ContentStatusEnum.Approved},
            new Approval{ContentId=c.Id,ApproverUserId=manager.Id,SubmittedAt=start.AddDays(1),CreatedAt=start.AddDays(1).AddHours(4),Status=ContentStatusEnum.Rejected},
            new Approval{ContentId=c.Id,CreatedAt=start,Status=ContentStatusEnum.Approved},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledByUserId=creator.Id,ScheduledAt=start,ExecutedAt=start.AddMinutes(5),Status=ScheduleStatusEnum.Completed,AttemptCount=3},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledByUserId=creator.Id,ScheduledAt=start,ExecutedAt=start.AddMinutes(6),Status=ScheduleStatusEnum.Completed},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledByUserId=creator.Id,ScheduledAt=start,Status=ScheduleStatusEnum.Failed,AttemptCount=5},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledByUserId=creator.Id,ScheduledAt=start,Status=ScheduleStatusEnum.Pending},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledAt=start,Status=ScheduleStatusEnum.Failed,RepeatType=RepeatTypeEnum.Daily});
        await db.SaveChangesAsync();db.ChangeTracker.Clear();
        var service=new MemberPerformanceService(db,new AccessControlService(db));
        Assert.Equal(403,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(creator.Id,w.Id,start,end))).StatusCode);
        var ownerResult=await service.GetAsync(owner.Id,w.Id,start,end);
        var row=ownerResult.Items.Single(r=>r.MemberId==creator.Id);
        Assert.Equal(2,row.ContentsCreated);Assert.Equal(1,row.CreatorPublishedPosts);Assert.Equal(0,row.PublisherPublishedPosts);
        Assert.Equal(50m,row.ApprovalRate);Assert.Null(row.TurnaroundHours);Assert.Equal(50m,row.OnTimeRate);Assert.Equal(33.33m,row.FailedPublishRate);
        Assert.Equal(30,row.Engagement);Assert.Equal(200,row.Impressions);Assert.Equal(120,row.Reach);Assert.Equal(15m,row.EngagementRate);
        var managerResult=await service.GetAsync(manager.Id,w.Id,start,end,teamId:team.Id,memberId:creator.Id);
        Assert.Equal(1,Assert.Single(managerResult.Items).ContentsCreated);
        var reviewerRow=Assert.Single((await service.GetAsync(manager.Id,w.Id,start,end,teamId:team.Id,memberId:manager.Id)).Items);
        Assert.Equal(2,reviewerRow.ReviewedSubmissions);Assert.Equal(3m,reviewerRow.TurnaroundHours);
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(manager.Id,w.Id,start,end,brandId:hidden.Id))).StatusCode);
        Assert.Equal(403,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(creator.Id,w.Id,start,end,memberId:owner.Id))).StatusCode);
        Assert.Equal(403,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(viewer.Id,w.Id,start,end))).StatusCode);
        Assert.Equal(1,ownerResult.UnattributedContents);Assert.Equal(2,ownerResult.Items.Single(r=>r.MemberId==creator.Id).ContentsCreated);
        Assert.Equal(1,ownerResult.Items.Single(r=>r.MemberId==owner.Id).PublisherPublishedPosts);
        Assert.Null(ownerResult.Items.Single(r=>r.MemberId==owner.Id).EngagementRate);
        Assert.Null(MemberPerformanceService.Rate(0,0));
        await Assert.ThrowsAsync<ArgumentException>(()=>service.GetAsync(owner.Id,w.Id,end,start));
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(owner.Id,w.Id,start,end,teamId:Guid.NewGuid()))).StatusCode);
        var assignment=await db.TeamBrands.SingleAsync(tb=>tb.TeamId==team.Id && tb.BrandId==brand.Id);assignment.IsActive=false;await db.SaveChangesAsync();
        Assert.Equal(403,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(creator.Id,w.Id,start,end))).StatusCode);
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(manager.Id,w.Id,start,end,brandId:brand.Id))).StatusCode);
    }
}
