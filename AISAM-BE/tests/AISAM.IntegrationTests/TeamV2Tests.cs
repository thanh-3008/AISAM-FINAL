using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class TeamV2Tests
{
    [Fact]
    public async Task CreateManageAndDeactivatePreservesContentAndRevokesScope()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var w=new Workspace();var owner=new User{Email="o@test.local"};var manager=new User{Email="m@test.local"};var creator=new User{Email="c@test.local"};
        db.AddRange(w,owner,manager,creator,
            new WorkspaceMember{WorkspaceId=w.Id,UserId=owner.Id,WorkspaceRoleV2=WorkspaceRoleV2.Owner},
            new WorkspaceMember{WorkspaceId=w.Id,UserId=manager.Id,WorkspaceRoleV2=WorkspaceRoleV2.Member},
            new WorkspaceMember{WorkspaceId=w.Id,UserId=creator.Id,WorkspaceRoleV2=WorkspaceRoleV2.Member});
        await db.SaveChangesAsync();
        var service=new TeamService(db,new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.CreateAsync(manager.Id,w.Id,new("Denied",null,null)));
        await Assert.ThrowsAsync<ArgumentException>(()=>service.CreateAsync(owner.Id,w.Id,new("Invalid",null,[new(Guid.NewGuid(),"Viewer")])));
        Assert.Empty(await db.Teams.ToListAsync());
        var team=await service.CreateAsync(owner.Id,w.Id,new("Team",null,[new(manager.Id,"Manager"),new(creator.Id,"ContentCreator")]));
        Assert.NotNull(await service.GetByIdAsync(owner.Id,w.Id,team.Id));
        Assert.Equal(2,team.Members.Count);
        Assert.DoesNotContain(team.Members,m=>m.UserId==owner.Id);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.UpdateAsync(manager.Id,w.Id,team.Id,new("Rename",null)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.UpdateMemberRoleAsync(manager.Id,w.Id,team.Id,creator.Id,"Manager"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.RemoveMemberAsync(manager.Id,w.Id,team.Id,manager.Id));
        Assert.Equal("Viewer",(await service.UpdateMemberRoleAsync(manager.Id,w.Id,team.Id,creator.Id,"Viewer")).Role);
        var b=new Brand{WorkspaceId=w.Id};var tb=new TeamBrand{TeamId=team.Id,BrandId=b.Id};var channel=new SocialIntegration{WorkspaceId=w.Id,BrandId=b.Id};
        var grant=new TeamChannelAccess{TeamBrandId=tb.Id,IntegrationId=channel.Id,CanView=true,ScopeEnabledV2=true};
        var content=new Content{WorkspaceId=w.Id,BrandId=b.Id,TeamId=team.Id};db.AddRange(b,tb,channel,grant,content);await db.SaveChangesAsync();
        await service.DeleteAsync(owner.Id,w.Id,team.Id);
        Assert.False(tb.IsActive);Assert.False(grant.ScopeEnabledV2);
        Assert.Equal(TeamStatusEnum.Inactive,(await db.Teams.SingleAsync()).Status);
        Assert.True(await db.Contents.AnyAsync(c=>c.Id==content.Id));
        Assert.Null(await service.GetByIdAsync(manager.Id,w.Id,team.Id));
        Assert.Empty((await service.ListAsync(manager.Id,w.Id)).Items);
    }
}
