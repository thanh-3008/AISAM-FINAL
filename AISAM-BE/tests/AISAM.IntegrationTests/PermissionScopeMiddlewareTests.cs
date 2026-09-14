using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AISAM.IntegrationTests;
public class PermissionScopeMiddlewareTests
{
    [Fact]
    public async Task ViewAllCreatorCannotSaveEditsToOthersAndScopeIsReset()
    {
        var options=new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db=new AisamContext(options);
        var user=new User {Email="creator-scope@example.test"};
        var w=new Workspace {WorkspaceType=WorkspaceTypeEnum.Business};
        var membership=new WorkspaceMember {WorkspaceId=w.Id,UserId=user.Id,Workspace=w,Role=WorkspaceMemberRoleEnum.ContentCreator};
        var b=new Brand {WorkspaceId=w.Id}; var team=new Team {WorkspaceId=w.Id};
        var content=new Content {WorkspaceId=w.Id,BrandId=b.Id,TextContent="Original"};
        db.AddRange(user,w,membership,b,team,content,new TeamBrand {TeamId=team.Id,BrandId=b.Id},
            new TeamMember {TeamId=team.Id,UserId=user.Id,Permissions=[DelegatedPermissionKeys.ViewAllCreators]});
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var http=new DefaultHttpContext {User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())],"test"))};
        http.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey]=membership;
        var middleware=new PermissionScopeMiddleware(async _=>
        {
            var visible=await db.Contents.SingleAsync(c=>c.Id==content.Id);
            visible.TextContent="Must not persist";
            await db.SaveChangesAsync();
        });
        await Assert.ThrowsAsync<ResourceMutationDeniedException>(()=>middleware.InvokeAsync(http,db,new AccessControlService(db)));
        Assert.False(db.PermissionScopeEnabled); Assert.Null(db.BeforePermissionMutation);
        await using var verify=new AisamContext(options);
        Assert.Equal("Original",(await verify.Contents.SingleAsync()).TextContent);
        Assert.Empty(await verify.AuditLogs.ToListAsync());
    }

    [Fact]
    public async Task TeamMemberWithManagerRoleEnablesPermissionManager()
    {
        var options = new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AisamContext(options);
        var user = new User { Email = "team-mgr@example.test" };
        var w = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business };
        var membership = new WorkspaceMember { WorkspaceId = w.Id, UserId = user.Id, Workspace = w, Role = WorkspaceMemberRoleEnum.ContentCreator };
        var b = new Brand { WorkspaceId = w.Id }; var team = new Team { WorkspaceId = w.Id, Status = TeamStatusEnum.Active };
        db.AddRange(user, w, membership, b, team,
            new TeamBrand { TeamId = team.Id, BrandId = b.Id, IsActive = true },
            new TeamMember { TeamId = team.Id, UserId = user.Id, Role = TeamRoleEnum.Manager, IsActive = true, Permissions = new List<string>() });
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();

        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "test")) };
        http.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = membership;

        bool managerScopeObserved = false;
        var middleware = new PermissionScopeMiddleware(async ctx =>
        {
            managerScopeObserved = db.PermissionManager;
            await Task.CompletedTask;
        });

        await middleware.InvokeAsync(http, db, new AccessControlService(db));
        Assert.True(managerScopeObserved);
    }
}
