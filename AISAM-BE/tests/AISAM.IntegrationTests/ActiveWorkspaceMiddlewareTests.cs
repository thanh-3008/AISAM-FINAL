using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class ActiveWorkspaceMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReturnsUnauthorized_WhenWorkspaceHeaderIsMissing()
    {
        var context = CreateContext(Guid.NewGuid());
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsUnauthorized_WhenWorkspaceHeaderIsInvalid()
    {
        var context = CreateContext(Guid.NewGuid());
        context.Request.Headers["X-Workspace-Id"] = "invalid";
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenUserIsNotWorkspaceMember()
    {
        var context = CreateContext(Guid.NewGuid());
        context.Request.Headers["X-Workspace-Id"] = Guid.NewGuid().ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository());

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsNotFound_WhenWorkspaceIsDeleted()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Deleted);
        var context = CreateContext(userId);
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(membership));

        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_StoresActiveWorkspaceAndMembership_WhenMembershipIsValid()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active);
        var context = CreateContext(userId);
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var nextCalled = false;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(membership));

        Assert.True(nextCalled);
        Assert.Equal(membership.WorkspaceId, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(context));
        Assert.Same(membership, WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(context));
    }

    [Fact]
    public async Task InvokeAsync_DoesNotRequireWorkspaceHeader_ForProfileBasedRoute()
    {
        var nextCalled = false;
        var context = CreateContext(Guid.NewGuid(), "/api/content");
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotRequireWorkspaceHeader_ForInvitationAcceptRoute()
    {
        var nextCalled = false;
        var context = CreateContext(Guid.NewGuid(), "/api/workspace-invitations/accept");
        context.Request.Method = HttpMethods.Post;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository());

        Assert.True(nextCalled);
    }

    private static DefaultHttpContext CreateContext(Guid userId, string path = "/api/workspace-members")
    {
        return new DefaultHttpContext
        {
            Request = { Path = path },
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "Test"))
        };
    }

    private static WorkspaceMember CreateMembership(Guid userId, WorkspaceStatusEnum status)
    {
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Workspace",
            WorkspaceType = WorkspaceTypeEnum.Business,
            Status = status
        };
        return new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = $"{userId:N}@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            },
            Role = WorkspaceMemberRoleEnum.Manager
        };
    }

    private sealed class FakeWorkspaceMemberRepository : IWorkspaceMemberRepository
    {
        private readonly Dictionary<(Guid WorkspaceId, Guid UserId), WorkspaceMember> _memberships;

        public FakeWorkspaceMemberRepository(params WorkspaceMember[] memberships)
        {
            _memberships = memberships.ToDictionary(member => (member.WorkspaceId, member.UserId));
        }

        public Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_memberships.GetValueOrDefault((workspaceId, userId)));

        public Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember> AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
