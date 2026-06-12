using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
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

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(), new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsUnauthorized_WhenWorkspaceHeaderIsInvalid()
    {
        var context = CreateContext(Guid.NewGuid());
        context.Request.Headers["X-Workspace-Id"] = "invalid";
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(), new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenUserIsNotWorkspaceMember()
    {
        var context = CreateContext(Guid.NewGuid());
        context.Request.Headers["X-Workspace-Id"] = Guid.NewGuid().ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(), new FakeSubscriptionRepository());

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

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(membership), new FakeSubscriptionRepository());

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

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(membership), new FakeSubscriptionRepository());

        Assert.True(nextCalled);
        Assert.Equal(membership.WorkspaceId, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(context));
        Assert.Same(membership, WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(context));
    }

    [Fact]
    public async Task InvokeAsync_RequiresWorkspaceHeader_ForContentRoute()
    {
        var context = CreateContext(Guid.NewGuid(), "/api/content");
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(), new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
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

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(), new FakeSubscriptionRepository());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_RequiresWorkspaceHeader_ForPaymentWorkspaceRoute()
    {
        var context = CreateContext(Guid.NewGuid(), "/api/payment/history");
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(), new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/api/payment/callback")]
    [InlineData("/api/payment/webhook")]
    public async Task InvokeAsync_DoesNotRequireWorkspaceHeader_ForPaymentProviderRoutes(string path)
    {
        var nextCalled = false;
        var context = CreateContext(Guid.NewGuid(), path);
        context.Request.Method = HttpMethods.Post;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(), new FakeSubscriptionRepository());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenNonOwnerAccessesBillingRoute()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.Manager);
        var context = CreateContext(userId, "/api/payment/history");
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeWorkspaceMemberRepository(membership), new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenViewerPublishesContent()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.Viewer);
        var context = CreateContext(userId, $"/api/content/{Guid.NewGuid():D}/publish/{Guid.NewGuid():D}");
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository(
                new Subscription
                {
                    WorkspaceId = membership.WorkspaceId,
                    Plan = SubscriptionPlanEnum.Plus,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(30)
                }));

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenFreePlanSchedulesContent()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.ContentCreator);
        var context = CreateContext(userId, "/api/content-schedules");
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository(
                new Subscription
                {
                    WorkspaceId = membership.WorkspaceId,
                    Plan = SubscriptionPlanEnum.Free,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(7)
                }));

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenFreePlanUsesAiImageFeature()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(
            userId,
            WorkspaceStatusEnum.Active,
            WorkspaceMemberRoleEnum.ContentCreator,
            WorkspaceTypeEnum.Personal);
        var context = CreateContext(userId, "/api/ai/generate-image");
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository(
                new Subscription
                {
                    WorkspaceId = membership.WorkspaceId,
                    Plan = SubscriptionPlanEnum.Free,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(7)
                }));

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenPersonalPlusUsesWorkspaceDashboardFeature()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(
            userId,
            WorkspaceStatusEnum.Active,
            WorkspaceMemberRoleEnum.Owner,
            WorkspaceTypeEnum.Personal);
        var context = CreateContext(userId, "/api/workspace-dashboard/summary");
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository(
                new Subscription
                {
                    WorkspaceId = membership.WorkspaceId,
                    Plan = SubscriptionPlanEnum.Plus,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(30)
                }));

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AllowsBusinessPlusWorkspaceDashboardFeature()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(
            userId,
            WorkspaceStatusEnum.Active,
            WorkspaceMemberRoleEnum.Owner,
            WorkspaceTypeEnum.Business);
        var nextCalled = false;
        var context = CreateContext(userId, "/api/workspace-dashboard/summary");
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository(
                new Subscription
                {
                    WorkspaceId = membership.WorkspaceId,
                    Plan = SubscriptionPlanEnum.Plus,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(30)
                }));

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode == 0 ? StatusCodes.Status200OK : context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenLimitedWorkspaceAttemptsWriteRoute()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.ContentCreator);
        membership.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-30);
        var context = CreateContext(userId, "/api/content");
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AllowsReadRoute_WhenWorkspaceIsLimited()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.Viewer);
        membership.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-30);
        var nextCalled = false;
        var context = CreateContext(userId, "/api/workspace-members");
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AllowsBillingRoute_ForOwnerWhenWorkspaceIsLimited()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.Owner);
        membership.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-30);
        var nextCalled = false;
        var context = CreateContext(userId, "/api/payment/history");
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenArchivedWorkspaceAttemptsWriteRoute()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.ContentCreator);
        membership.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-90);
        var context = CreateContext(userId, "/api/content");
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AllowsReadRoute_WhenWorkspaceIsArchived()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.Viewer);
        membership.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-90);
        var nextCalled = false;
        var context = CreateContext(userId, "/api/workspace-members");
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AllowsBillingRoute_ForOwnerWhenWorkspaceIsArchived()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.Owner);
        membership.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-90);
        var nextCalled = false;
        var context = CreateContext(userId, "/api/payment/checkout");
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenNonOwnerAccessesBillingRouteInArchivedWorkspace()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, WorkspaceStatusEnum.Active, WorkspaceMemberRoleEnum.Manager);
        membership.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-90);
        var context = CreateContext(userId, "/api/payment/history");
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers["X-Workspace-Id"] = membership.WorkspaceId.ToString();
        var middleware = new ActiveWorkspaceMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            new FakeWorkspaceMemberRepository(membership),
            new FakeSubscriptionRepository());

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
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

    private static WorkspaceMember CreateMembership(
        Guid userId,
        WorkspaceStatusEnum status,
        WorkspaceMemberRoleEnum role = WorkspaceMemberRoleEnum.Manager,
        WorkspaceTypeEnum workspaceType = WorkspaceTypeEnum.Business)
    {
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Workspace",
            WorkspaceType = workspaceType,
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
            Role = role
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
        public Task<WorkspaceMember> TransferOwnershipAsync(Guid workspaceId, Guid currentOwnerUserId, Guid targetMemberId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeSubscriptionRepository : ISubscriptionRepository
    {
        private readonly Dictionary<Guid, Subscription> _subscriptions;

        public FakeSubscriptionRepository(params Subscription[] subscriptions)
        {
            _subscriptions = subscriptions.ToDictionary(subscription => subscription.WorkspaceId ?? Guid.NewGuid());
        }

        public Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Subscription?> GetCurrentActiveByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(_subscriptions.GetValueOrDefault(workspaceId));

        public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
