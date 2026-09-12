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
    public async Task TeamManagerDoesNotEscalateToOtherBrandsOrUnscopedRequests()
    {
        var options = new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AisamContext(options);
        var user = new User { Email = "team-mgr@example.test" };
        var w = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business };
        var membership = new WorkspaceMember { WorkspaceId = w.Id, UserId = user.Id, Workspace = w, Role = WorkspaceMemberRoleEnum.ContentCreator };
        var brandA = new Brand { WorkspaceId = w.Id };
        var brandB = new Brand { WorkspaceId = w.Id };
        var teamA = new Team { WorkspaceId = w.Id };
        var teamB = new Team { WorkspaceId = w.Id };
        var integA = new SocialIntegration { WorkspaceId = w.Id, BrandId = brandA.Id };
        var integB = new SocialIntegration { WorkspaceId = w.Id, BrandId = brandB.Id };

        db.AddRange(user, w, membership, brandA, brandB, teamA, teamB, integA, integB,
            new TeamBrand { TeamId = teamA.Id, BrandId = brandA.Id },
            new TeamMember { TeamId = teamA.Id, UserId = user.Id, Role = TeamRoleEnum.Manager },
            new TeamBrand { TeamId = teamB.Id, BrandId = brandB.Id },
            new TeamMember { TeamId = teamB.Id, UserId = user.Id, Role = TeamRoleEnum.ContentCreator });
        await db.SaveChangesAsync();

        // 1. Scoped to Brand A -> PermissionManager is true, integA is granted, integB is not
        var httpA = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "test")) };
        httpA.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = membership;
        httpA.Request.QueryString = new QueryString($"?brandId={brandA.Id}");

        var middlewareA = new PermissionScopeMiddleware(async _ =>
        {
            Assert.True(db.PermissionManager);
            Assert.Contains(integA.Id, db.PermissionChannelIds);
            Assert.DoesNotContain(integB.Id, db.PermissionChannelIds);
            await Task.CompletedTask;
        });
        await middlewareA.InvokeAsync(httpA, db, new AccessControlService(db));

        // 2. Scoped to Brand B -> PermissionManager is false, neither channel auto-granted
        var httpB = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "test")) };
        httpB.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = membership;
        httpB.Request.QueryString = new QueryString($"?brandId={brandB.Id}");

        var middlewareB = new PermissionScopeMiddleware(async _ =>
        {
            Assert.False(db.PermissionManager);
            Assert.DoesNotContain(integB.Id, db.PermissionChannelIds);
            await Task.CompletedTask;
        });
        await middlewareB.InvokeAsync(httpB, db, new AccessControlService(db));

        // 3. Unscoped request -> PermissionManager is false
        var httpUnscoped = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "test")) };
        httpUnscoped.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = membership;

        var middlewareUnscoped = new PermissionScopeMiddleware(async _ =>
        {
            Assert.False(db.PermissionManager);
            Assert.DoesNotContain(integB.Id, db.PermissionChannelIds);
            await Task.CompletedTask;
        });
        await middlewareUnscoped.InvokeAsync(httpUnscoped, db, new AccessControlService(db));
    }
}
