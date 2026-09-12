using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;
public class MemberPerformanceTests
{
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
            new TeamBrand{TeamId=team.Id,BrandId=brand.Id},new TeamMember{TeamId=team.Id,UserId=creator.Id},new TeamMember{TeamId=team.Id,UserId=manager.Id});
        var c=new Content{WorkspaceId=w.Id,BrandId=brand.Id,PrimaryCreatorId=creator.Id,TeamId=team.Id,CreatedAt=start};
        var other=new Content{WorkspaceId=w.Id,BrandId=hidden.Id,PrimaryCreatorId=creator.Id,CreatedAt=start};
        var outsidePeriod=new Content{WorkspaceId=w.Id,BrandId=brand.Id,PrimaryCreatorId=creator.Id,CreatedAt=end};
        var channel=new SocialIntegration{WorkspaceId=w.Id,BrandId=brand.Id};
        var post=new Post{ContentId=c.Id,IntegrationId=channel.Id,PublishedAt=start.AddDays(1),PublishedByUserId=owner.Id,ExternalPostId="same"};
        db.AddRange(c,other,outsidePeriod,channel,post,
            new Content{WorkspaceId=w.Id,BrandId=brand.Id,CreatedAt=start},
            new Post{ContentId=c.Id,IntegrationId=channel.Id,PublishedAt=start.AddDays(1).AddSeconds(1),PublishedByUserId=owner.Id,ExternalPostId="same"},
            new PerformanceReport{PostId=post.Id,ReportDate=start,Engagement=10,Impressions=100,Reach=70},
            new PerformanceReport{PostId=post.Id,ReportDate=start.AddDays(2),Engagement=30,Impressions=200,Reach=120},
            new PerformanceReport{PostId=post.Id,ReportDate=start.AddDays(3),RawData="{\"trackedClicks\":1}"},
            new Approval{ContentId=c.Id,SubmittedAt=start,ApprovedAt=start.AddHours(2),Status=ContentStatusEnum.Approved},
            new Approval{ContentId=c.Id,SubmittedAt=start.AddDays(1),CreatedAt=start.AddDays(1).AddHours(4),Status=ContentStatusEnum.Rejected},
            new Approval{ContentId=c.Id,CreatedAt=start,Status=ContentStatusEnum.Approved},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledAt=start,ExecutedAt=start.AddMinutes(5),Status=ScheduleStatusEnum.Completed,AttemptCount=3},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledAt=start,ExecutedAt=start.AddMinutes(6),Status=ScheduleStatusEnum.Completed},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledAt=start,Status=ScheduleStatusEnum.Failed,AttemptCount=5},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledAt=start,Status=ScheduleStatusEnum.Pending},
            new ContentCalendar{WorkspaceId=w.Id,ContentId=c.Id,IntegrationId=channel.Id,ScheduledAt=start,Status=ScheduleStatusEnum.Failed,RepeatType=RepeatTypeEnum.Daily});
        await db.SaveChangesAsync();db.ChangeTracker.Clear();
        var service=new MemberPerformanceService(db,new AccessControlService(db));
        var result=await service.GetAsync(creator.Id,w.Id,start,end);
        var row=Assert.Single(result.Items);
        Assert.Equal(1,row.ContentsCreated);Assert.Equal(1,row.CreatorPublishedPosts);Assert.Equal(0,row.PublisherPublishedPosts);
        Assert.Equal(50m,row.ApprovalRate);Assert.Equal(3m,row.TurnaroundHours);Assert.Equal(50m,row.OnTimeRate);Assert.Equal(33.33m,row.FailedPublishRate);
        Assert.Equal(30,row.Engagement);Assert.Equal(200,row.Impressions);Assert.Equal(120,row.Reach);Assert.Equal(15m,row.EngagementRate);Assert.Null(result.UnattributedContents);
        var managerResult=await service.GetAsync(manager.Id,w.Id,start,end,memberId:creator.Id);
        Assert.Equal(1,Assert.Single(managerResult.Items).ContentsCreated);
        // NC-07: WorkspaceManager has workspace-wide brand scope — 'hidden' brand is still in same workspace, so must NOT be 404
        var wmHiddenResult = await service.GetAsync(manager.Id,w.Id,start,end,brandId:hidden.Id);
        Assert.NotNull(wmHiddenResult); // WM sees all brands in the workspace
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(creator.Id,w.Id,start,end,memberId:owner.Id))).StatusCode);
        Assert.Equal(403,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(viewer.Id,w.Id,start,end))).StatusCode);
        var ownerResult=await service.GetAsync(owner.Id,w.Id,start,end);
        Assert.Equal(1,ownerResult.UnattributedContents);Assert.Equal(2,ownerResult.Items.Single(r=>r.MemberId==creator.Id).ContentsCreated);
        Assert.Null(ownerResult.Items.Single(r=>r.MemberId==owner.Id).EngagementRate);
        Assert.Null(MemberPerformanceService.Rate(0,0));
        await Assert.ThrowsAsync<ArgumentException>(()=>service.GetAsync(owner.Id,w.Id,end,start));
        Assert.Equal(404,(await Assert.ThrowsAsync<PerformanceAccessException>(()=>service.GetAsync(owner.Id,w.Id,start,end,teamId:Guid.NewGuid()))).StatusCode);
        var assignment=await db.TeamBrands.SingleAsync();assignment.IsActive=false;await db.SaveChangesAsync();
        Assert.Empty((await service.GetAsync(creator.Id,w.Id,start,end)).Items);
        // NC-07: WorkspaceManager sees workspace-wide brands regardless of TeamBrand.IsActive
        var wmResult = await service.GetAsync(manager.Id,w.Id,start,end,brandId:brand.Id);
        Assert.NotNull(wmResult); // WM must NOT get 404; workspace-wide scope applies
    }
}
