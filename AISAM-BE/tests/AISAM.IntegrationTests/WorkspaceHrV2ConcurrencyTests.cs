using AISAM.API.Filters;
using AISAM.API.Utils;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class WorkspaceHrV2ConcurrencyTests
{
    [Fact]
    public async Task GetReturnsRevisionComputedAfterControllerCompletes()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var workspace=Guid.NewGuid();
        var http=new DefaultHttpContext();http.Request.Method="GET";http.Request.Path="/api/workspace-members";
        http.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey]=workspace;
        var executing=new ActionExecutingContext(new ActionContext(http,new RouteData(),new ActionDescriptor()),new List<IFilterMetadata>(),new Dictionary<string,object?>(),new object());
        var filter=new WorkspaceHrV2ConcurrencyFilter(db,new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db)));

        await filter.OnActionExecutionAsync(executing,()=>Task.FromResult(new ActionExecutedContext(executing,executing.Filters,executing.Controller)));
        var before=http.Response.Headers["X-HR-Revision"].ToString();
        await filter.OnActionExecutionAsync(executing,async () =>
        {
            db.WorkspaceMembers.Add(new AISAM.Data.Model.WorkspaceMember
            {
                WorkspaceId=workspace,UserId=Guid.NewGuid(),Role=AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Viewer,
                WorkspaceRoleV2=AISAM.Data.Enumeration.WorkspaceRoleV2.Member,IsActive=true
            });
            await db.SaveChangesAsync();
            return new ActionExecutedContext(executing,executing.Filters,executing.Controller);
        });

        Assert.NotEqual(before,http.Response.Headers["X-HR-Revision"].ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WrappedSerializationFailureReturnsConflict(bool returnedByMvc)
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var http=new DefaultHttpContext();http.Request.Method="GET";http.Request.Path="/api/teams";
        http.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey]=Guid.NewGuid();
        var executing=new ActionExecutingContext(new ActionContext(http,new RouteData(),new ActionDescriptor()),new List<IFilterMetadata>(),new Dictionary<string,object?>(),new object());
        var filter=new WorkspaceHrV2ConcurrencyFilter(db,new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db)));
        await filter.OnActionExecutionAsync(executing,()=>Task.FromResult(new ActionExecutedContext(executing,executing.Filters,executing.Controller)));
        http.Request.Method="POST";http.Request.Headers["If-Match"]=http.Response.Headers["X-HR-Revision"];
        var error=new InvalidOperationException("wrapper",new DbUpdateException("update",new Npgsql.PostgresException("serialization","ERROR","ERROR","40001")));
        var executed=new ActionExecutedContext(executing,executing.Filters,executing.Controller){Exception=error};
        await filter.OnActionExecutionAsync(executing,()=>returnedByMvc?Task.FromResult(executed):throw error);
        Assert.Equal(409,Assert.IsType<ObjectResult>(executing.Result).StatusCode);
        if(returnedByMvc) Assert.True(executed.ExceptionHandled);
    }

    [Theory]
    [InlineData("",428)]
    [InlineData("stale",409)]
    public async Task MissingOrStaleRevisionDoesNotExecuteMutation(string revision,int expected)
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var http=new DefaultHttpContext();http.Request.Method="PUT";http.Request.Path="/api/workspace-members/member/role";
        http.Request.Headers["If-Match"]=revision;
        http.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey]=Guid.NewGuid();
        var action=new ActionContext(http,new RouteData(),new ActionDescriptor());
        var executing=new ActionExecutingContext(action,new List<IFilterMetadata>(),new Dictionary<string,object?>(),new object());
        var filter=new WorkspaceHrV2ConcurrencyFilter(db,new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db)));
        await filter.OnActionExecutionAsync(executing,()=>throw new Exception("Mutation must not run"));
        Assert.Equal(expected,Assert.IsType<ObjectResult>(executing.Result).StatusCode);
    }
}
