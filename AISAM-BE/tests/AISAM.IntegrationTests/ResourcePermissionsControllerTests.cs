using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AISAM.IntegrationTests;
public class ResourcePermissionsControllerTests
{
    [Fact]
    public async Task UsesAuthenticatedActorAndTrustedWorkspaceAndBoundsBatch()
    {
        var actor=Guid.NewGuid(); var workspace=Guid.NewGuid(); var channel=Guid.NewGuid();
        var access=new RecordingAccess();
        var http=new DefaultHttpContext { User=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,actor.ToString())],"test")) };
        http.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey]=workspace;
        var controller=new ResourcePermissionsController(access) {ControllerContext=new ControllerContext {HttpContext=http}};
        var item=new ResourcePermissionsController.CheckItem(AccessResourceKind.Content,Guid.NewGuid(),ResourcePermission.PostPublish,channel);
        Assert.IsType<OkObjectResult>(await controller.Check([item],default));
        Assert.Equal(actor,access.Request!.ActorId); Assert.Equal(workspace,access.Request.WorkspaceId); Assert.Equal(channel,access.Request.ChannelId);
        Assert.IsType<BadRequestObjectResult>(await controller.Check(Enumerable.Repeat(item,101).ToArray(),default));
        Assert.Equal(1,access.Calls);
    }
    private sealed class RecordingAccess:IAccessControlService
    {
        public AccessRequest? Request; public int Calls;
        public Task<AccessDecision> CheckAsync(AccessRequest request,CancellationToken ct=default) {Calls++; Request=request; return Task.FromResult(AccessDecision.Denied);}
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid actorId,Guid workspaceId,CancellationToken ct=default)=>throw new NotSupportedException();
    }
}
