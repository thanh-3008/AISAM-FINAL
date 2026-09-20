using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Data.Enumeration;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class PermissionQueryScopeTests
{
    [Fact]
    public async Task CreatorScopeAppliesBeforeCountAndPaginationAndPropagatesToSchedules()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var user=new User {Email="scope@example.test"}; var w=Guid.NewGuid();
        var brand=new Brand {WorkspaceId=w}; var other=new Brand {WorkspaceId=w};
        db.AddRange(user,brand,other,new WorkspaceMember {WorkspaceId=w,UserId=user.Id});
        var own=new Content {WorkspaceId=w,BrandId=brand.Id,PrimaryCreatorId=user.Id};
        var hidden=new Content {WorkspaceId=w,BrandId=brand.Id};
        var outside=new Content {WorkspaceId=w,BrandId=other.Id,PrimaryCreatorId=user.Id};
        db.AddRange(own,hidden,outside);
        db.AddRange(new ContentCalendar {WorkspaceId=w,ContentId=own.Id},new ContentCalendar {WorkspaceId=w,ContentId=hidden.Id});
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        db.PermissionScopeEnabled=true; db.PermissionWorkspaceId=w; db.PermissionActorId=user.Id;
        db.PermissionCreator=true; db.PermissionBrandIds=[brand.Id];
        Assert.Equal(1,await db.Contents.CountAsync());
        Assert.Equal(own.Id,(await db.Contents.OrderBy(x=>x.Id).Take(1).SingleAsync()).Id);
        Assert.Equal(1,await db.ContentCalendars.CountAsync());
        Assert.Null(await db.Contents.SingleOrDefaultAsync(x=>x.Id==outside.Id));
        db.PermissionViewAllBrandIds=[brand.Id];
        Assert.Equal(2,await db.Contents.CountAsync());
        db.PermissionOnlyMyContent=true;
        Assert.Equal(own.Id,(await db.Contents.SingleAsync()).Id);
        db.PermissionOnlyMyContent=false;
        db.PermissionReviewQueue=true;
        Assert.Empty(await db.Contents.ToListAsync()); // Viewing others is not a review grant.
        db.PermissionReviewQueue=false;
        db.PermissionCreator=false;
        Assert.Empty(await db.Contents.ToListAsync());
    }

    [Fact]
    public async Task OwnerStillCannotReadAnotherWorkspaceOrMismatchedPost()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var w=Guid.NewGuid(); var b=new Brand {WorkspaceId=w}; var other=new Brand {WorkspaceId=Guid.NewGuid()};
        var content=new Content {WorkspaceId=w,BrandId=b.Id};
        var channel=new SocialIntegration {WorkspaceId=other.WorkspaceId,BrandId=other.Id};
        db.AddRange(b,other,content,channel,new Post {ContentId=content.Id,IntegrationId=channel.Id});
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        db.PermissionScopeEnabled=true; db.PermissionWorkspaceId=w; db.PermissionOwner=true; db.PermissionBrandIds=[b.Id];
        Assert.Equal(1,await db.Brands.CountAsync()); Assert.Empty(await db.Posts.ToListAsync());
    }

    [Fact]
    public async Task V2AnalyticsOnlyCountsPostsFromAssignedTeamBrandAndChannel()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var workspace=Guid.NewGuid(); var team=new Team {WorkspaceId=workspace};
        var brand=new Brand {WorkspaceId=workspace}; var otherBrand=new Brand {WorkspaceId=workspace};
        var allowedChannel=new SocialIntegration {WorkspaceId=workspace,BrandId=brand.Id};
        var deniedChannel=new SocialIntegration {WorkspaceId=workspace,BrandId=brand.Id};
        var otherChannel=new SocialIntegration {WorkspaceId=workspace,BrandId=otherBrand.Id};
        var now=DateTime.UtcNow;
        var visibleContent=new Content {WorkspaceId=workspace,TeamId=team.Id,BrandId=brand.Id,Status=ContentStatusEnum.Published};
        var hiddenContent=new Content {WorkspaceId=workspace,TeamId=team.Id,BrandId=otherBrand.Id,Status=ContentStatusEnum.Published};
        var allowedPost=new Post {ContentId=visibleContent.Id,IntegrationId=allowedChannel.Id,PublishedAt=now};
        var deniedPost=new Post {ContentId=visibleContent.Id,IntegrationId=deniedChannel.Id,PublishedAt=now};
        var otherPost=new Post {ContentId=hiddenContent.Id,IntegrationId=otherChannel.Id,PublishedAt=now};
        var allowedSchedule=new ContentCalendar {WorkspaceId=workspace,ContentId=visibleContent.Id,IntegrationId=allowedChannel.Id,ScheduledDate=now};
        var deniedSchedule=new ContentCalendar {WorkspaceId=workspace,ContentId=visibleContent.Id,IntegrationId=deniedChannel.Id,ScheduledDate=now};
        db.AddRange(team,brand,otherBrand,new TeamBrand {TeamId=team.Id,BrandId=brand.Id},
            allowedChannel,deniedChannel,otherChannel,visibleContent,hiddenContent,
            allowedPost,deniedPost,otherPost,allowedSchedule,deniedSchedule,
            new PerformanceReport {PostId=allowedPost.Id,ReportDate=now.AddDays(-1),Impressions=5},
            new PerformanceReport {PostId=allowedPost.Id,ReportDate=now,Impressions=10},
            new PerformanceReport {PostId=deniedPost.Id,ReportDate=now,Impressions=20},
            new PerformanceReport {PostId=otherPost.Id,ReportDate=now,Impressions=30});
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();

        db.PermissionScopeEnabled=db.PermissionV2Enabled=true;
        db.PermissionWorkspaceId=workspace; db.PermissionTeamIds=[team.Id];
        db.PermissionWriteTeamIds=[team.Id]; db.PermissionBrandIds=[brand.Id]; db.PermissionChannelIds=[allowedChannel.Id];
        Assert.Equal([allowedPost.Id],await db.Posts.Select(p=>p.Id).ToArrayAsync());
        Assert.Equal([allowedSchedule.Id],await db.ContentCalendars.Select(s=>s.Id).ToArrayAsync());
        Assert.Equal(2,await db.PerformanceReports.CountAsync());
        var totals=await new PerformanceReportRepository(db).GetAggregatedTotalsAsync(workspace,now.AddDays(-1),now.AddDays(1));
        Assert.Equal(1,totals.PublishedPosts);
        Assert.Equal(10,totals.Impressions);

        db.PermissionWriteTeamIds=[];
        Assert.Equal(2,await db.PerformanceReports.CountAsync());
        db.PermissionChannelIds=[];
        Assert.Empty(await db.Posts.ToListAsync());
        Assert.Empty(await db.ContentCalendars.ToListAsync());
        Assert.Empty(await db.PerformanceReports.ToListAsync());

        db.PermissionOwner=true;
        Assert.Equal(4,await db.PerformanceReports.CountAsync());
    }
}
