using AISAM.API.Middleware;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AISAM.IntegrationTests;
public class ResourcePermissionFilterTests
{
    [Theory]
    [InlineData("Update",ResourcePermission.ContentEdit)]
    [InlineData("Approve",ResourcePermission.ApprovalReview)]
    [InlineData("Publish",ResourcePermission.PostPublish)]
    public async Task DenialStopsActionBeforeSideEffects(string action,ResourcePermission permission)
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.PermissionScopeEnabled=true; db.PermissionWorkspaceId=Guid.NewGuid();
        var actor=Guid.NewGuid();
        var http=new DefaultHttpContext {User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,actor.ToString())],"test"))};
        http.Request.Method="POST";
        var descriptor=new ActionDescriptor {RouteValues=new Dictionary<string,string?> {{"controller","Content"},{"action",action}}};
        var actionContext=new ActionContext(http,new RouteData(),descriptor,new ModelStateDictionary());
        var content=Guid.NewGuid(); var integration=Guid.NewGuid();
        var executing=new ActionExecutingContext(actionContext,[],new Dictionary<string,object?> {{"contentId",content},{"integrationId",integration}},new object());
        var access=new Access(); var filter=new ResourcePermissionFilter(access,db);
        bool ran=false;
        await filter.OnActionExecutionAsync(executing,()=> {ran=true; return Task.FromResult(new ActionExecutedContext(actionContext,[],new object()));});
        Assert.False(ran); Assert.Equal(403,Assert.IsType<ObjectResult>(executing.Result).StatusCode);
        Assert.Equal(actor,access.Request!.ActorId); Assert.Equal(content,access.Request.ResourceId); Assert.Equal(permission,access.Request.Permission);
        if(permission==ResourcePermission.PostPublish) Assert.Equal(integration,access.Request.ChannelId);
    }

    [Fact]
    public async Task SocialAuth_Manager_AllowedThroughFilter()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = Guid.NewGuid();
        db.PermissionOwner = false;
        db.PermissionManager = true;
        var actor = Guid.NewGuid();
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, actor.ToString())], "test")) };
        http.Request.Method = "POST";
        var descriptor = new ActionDescriptor { RouteValues = new Dictionary<string, string?> { { "controller", "SocialAuth" }, { "action", "Callback" } } };
        var actionContext = new ActionContext(http, new RouteData(), descriptor, new ModelStateDictionary());
        var executing = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
        var access = new Access();
        var filter = new ResourcePermissionFilter(access, db);
        bool ran = false;
        await filter.OnActionExecutionAsync(executing, () => { ran = true; return Task.FromResult(new ActionExecutedContext(actionContext, [], new object())); });
        Assert.True(ran);
        Assert.Null(executing.Result);
    }

    [Fact]
    public async Task SocialAuth_CreatorOrViewer_BlockedByFilter()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = Guid.NewGuid();
        db.PermissionOwner = false;
        db.PermissionManager = false;
        var actor = Guid.NewGuid();
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, actor.ToString())], "test")) };
        http.Request.Method = "POST";
        var descriptor = new ActionDescriptor { RouteValues = new Dictionary<string, string?> { { "controller", "SocialAuth" }, { "action", "Callback" } } };
        var actionContext = new ActionContext(http, new RouteData(), descriptor, new ModelStateDictionary());
        var executing = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
        var access = new Access();
        var filter = new ResourcePermissionFilter(access, db);
        bool ran = false;
        await filter.OnActionExecutionAsync(executing, () => { ran = true; return Task.FromResult(new ActionExecutedContext(actionContext, [], new object())); });
        Assert.False(ran);
        Assert.Equal(403, Assert.IsType<ObjectResult>(executing.Result).StatusCode);
    }

    private sealed class Access:IAccessControlService
    {
        public AccessRequest? Request;
        public Task<AccessDecision> CheckAsync(AccessRequest r,CancellationToken ct=default) {Request=r; return Task.FromResult(AccessDecision.Denied);}
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid a,Guid w,CancellationToken ct=default)=>throw new NotSupportedException();
    }
}
