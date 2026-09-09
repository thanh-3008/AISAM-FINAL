using AISAM.Data.Model;
using AISAM.Repositories;
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
}
