using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AISAM.IntegrationTests;

public class CheckoutResourceAccessTests
{
    [Theory]
    [InlineData(WorkspaceStatusEnum.Active, true, "/api/payment/checkout", true)]
    [InlineData(WorkspaceStatusEnum.Limited, true, "/api/payment/checkout", true)]
    [InlineData(WorkspaceStatusEnum.Archived, true, "/api/payment/checkout", true)]
    [InlineData(WorkspaceStatusEnum.EligibleForDeletion, true, "/api/payment/checkout", true)]
    [InlineData(WorkspaceStatusEnum.Deleted, true, "/api/payment/checkout", false)]
    [InlineData(WorkspaceStatusEnum.Limited, false, "/api/payment/checkout", false)]
    [InlineData(WorkspaceStatusEnum.Active, false, "/api/payment/checkout", false)]
    [InlineData(WorkspaceStatusEnum.Limited, true, "/api/content", false)]
    public async Task Checkout_AllowsOwnerRenewalButPreservesAccessRestrictions(
        WorkspaceStatusEnum status, bool owner, string path, bool expected)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        f.Workspace.Status = status;
        await f.Db.SaveChangesAsync();
        using var services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        context.Request.Method = "POST";
        context.Request.Path = path;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = new WorkspaceMember
        {
            WorkspaceId = f.Workspace.Id,
            UserId = owner ? f.Owner.Id : f.Manager.Id,
            Role = owner ? WorkspaceMemberRoleEnum.Owner : WorkspaceMemberRoleEnum.Manager,
        };
        var reached = false;
        var middleware = new ResourceAccessMiddleware(_ => { reached = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context, f.Resolver, f.Db);
        Assert.Equal(expected, reached);
        if (!expected) Assert.Equal(403, context.Response.StatusCode);
    }
}
