using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class AssignmentV2Tests
{
    [Fact]
    public async Task WorkspaceManagerAssignsWithoutJoiningAndRevokeCannotRestoreChannels()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var w=new Workspace();var admin=new User{Email="w@test.local"};var user=new User{Email="m@test.local"};
        var b=new Brand{WorkspaceId=w.Id};var t=new Team{WorkspaceId=w.Id};var channel=new SocialIntegration{WorkspaceId=w.Id,BrandId=b.Id};
        db.AddRange(w,admin,user,b,t,channel,
            new WorkspaceMember{WorkspaceId=w.Id,UserId=admin.Id,WorkspaceRoleV2=WorkspaceRoleV2.WorkspaceManager,Role=WorkspaceMemberRoleEnum.Viewer},
            new WorkspaceMember{WorkspaceId=w.Id,UserId=user.Id,WorkspaceRoleV2=WorkspaceRoleV2.Member},
            new TeamMember{TeamId=t.Id,UserId=user.Id,Role=TeamRoleEnum.Manager});await db.SaveChangesAsync();
        var access=new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db));var service=new AssignmentService(db,access);
        var snapshot=await service.ReadAsync(admin.Id,w.Id,b.Id);
        snapshot=await service.ChangeAsync(new(admin.Id,w.Id,b.Id,t.Id,snapshot.Revision,true));
        var previous=snapshot.Revision;
        snapshot=await service.ChangeAsync(new(admin.Id,w.Id,b.Id,t.Id,snapshot.Revision,true,channel.Id));
        Assert.NotEqual(previous,snapshot.Revision);
        Assert.True(snapshot.Channels.Single().ScopeEnabledV2);
        await Assert.ThrowsAsync<AssignmentConflictException>(()=>service.ChangeAsync(new(admin.Id,w.Id,b.Id,t.Id,previous,false,channel.Id)));
        await Assert.ThrowsAsync<AssignmentAccessException>(()=>service.ReadAsync(user.Id,w.Id,b.Id));
        snapshot=await service.ChangeAsync(new(admin.Id,w.Id,b.Id,t.Id,snapshot.Revision,false));
        Assert.False(snapshot.Channels.Single().ScopeEnabledV2);
        snapshot=await service.ChangeAsync(new(admin.Id,w.Id,b.Id,t.Id,snapshot.Revision,true));
        Assert.False(snapshot.Channels.Single().ScopeEnabledV2);
        await Assert.ThrowsAsync<ArgumentException>(()=>service.ChangeAsync(new(admin.Id,w.Id,b.Id,t.Id,snapshot.Revision,true,channel.Id,CanManage:true,CanView:true)));
    }
}
