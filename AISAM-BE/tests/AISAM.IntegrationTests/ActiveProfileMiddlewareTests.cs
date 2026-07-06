using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class ActiveProfileMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReturnsUnauthorized_WhenProfileHeaderIsMissing()
    {
        var context = CreateContext(Guid.NewGuid());
        var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeProfileRepository(), new FakeUserRepository(), CreateEnvironment());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsUnauthorized_WhenProfileHeaderIsInvalid()
    {
        var context = CreateContext(Guid.NewGuid());
        context.Request.Headers["X-Profile-Id"] = "invalid";
        var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeProfileRepository(), new FakeUserRepository(), CreateEnvironment());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenProfileBelongsToAnotherUser()
    {
        var context = CreateContext(Guid.NewGuid());
        var profile = CreateProfile(Guid.NewGuid());
        context.Request.Headers["X-Profile-Id"] = profile.Id.ToString();
        var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeProfileRepository(profile), new FakeUserRepository(), CreateEnvironment());

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_StoresActiveProfile_WhenProfileBelongsToJwtUser()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var context = CreateContext(userId);
        context.Request.Headers["X-Profile-Id"] = profile.Id.ToString();
        var nextCalled = false;
        var middleware = new ActiveProfileMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeProfileRepository(profile), new FakeUserRepository(), CreateEnvironment());

        Assert.True(nextCalled);
        Assert.Equal(profile.Id, context.Items[ProfileContextHelper.ActiveProfileItemKey]);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenProfileDoesNotBelongToActiveWorkspace()
    {
        var userId = Guid.NewGuid();
        var profile = CreateProfile(userId);
        var activeWorkspaceId = Guid.NewGuid(); // Different from profile.Id
        
        var context = CreateContext(userId);
        context.Request.Headers["X-Profile-Id"] = profile.Id.ToString();
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = activeWorkspaceId;
        
        var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeProfileRepository(profile), new FakeUserRepository(), CreateEnvironment());

        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_SkipsDevSchedulerPrefix_WhenEnvironmentIsNotDevelopment()
    {
        var nextCalled = false;
        var context = CreateContext(Guid.NewGuid(), "/api/dev/scheduler/run-now");
        var middleware = new ActiveProfileMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeProfileRepository(), new FakeUserRepository(), CreateEnvironment("Production"));

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotRequireProfileHeaderForWorkspaceDashboardSummary()
    {
        var nextCalled = false;
        var context = CreateContext(Guid.NewGuid(), "/api/dashboard/summary");
        var middleware = new ActiveProfileMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeProfileRepository(), new FakeUserRepository(), CreateEnvironment());

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData("/api/payment/checkout")]
    [InlineData("/api/payment/history")]
    [InlineData("/api/payment/subscription/current")]
    [InlineData("/api/payment/callback")]
    [InlineData("/api/payment/webhook")]
    public async Task InvokeAsync_DoesNotRequireProfileHeaderForWorkspacePaymentRoutes(string path)
    {
        var nextCalled = false;
        var context = CreateContext(Guid.NewGuid(), path);
        var middleware = new ActiveProfileMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new FakeProfileRepository(), new FakeUserRepository(), CreateEnvironment());

        Assert.True(nextCalled);
    }

    private static DefaultHttpContext CreateContext(Guid userId, string path = "/api/dev/scheduler")
    {
        return new DefaultHttpContext
        {
            Request =
            {
                Path = path
            },
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "Test"))
        };
    }

    private static FakeWebHostEnvironment CreateEnvironment(string environmentName = "Development")
    {
        return new FakeWebHostEnvironment
        {
            EnvironmentName = environmentName
        };
    }

    private static Profile CreateProfile(Guid userId)
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test profile"
        };
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly Dictionary<Guid, Profile> _profiles;

        public FakeProfileRepository(params Profile[] profiles)
        {
            _profiles = profiles.ToDictionary(profile => profile.Id);
        }

        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _profiles.TryGetValue(id, out var profile);
            return Task.FromResult(profile);
        }

        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return GetByIdAsync(id, cancellationToken);
        }

        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_profiles.Values.Where(profile => profile.UserId == userId).AsEnumerable());
        }

        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default)
        {
            return GetByUserIdAsync(userId, cancellationToken);
        }

        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default)
        {
            return GetByUserIdAsync(userId, cancellationToken);
        }

        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default)
        {
            _profiles[profile.Id] = profile;
            return Task.FromResult(profile);
        }

        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
        {
            _profiles[profile.Id] = profile;
            return Task.FromResult(profile);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_profiles.Remove(id));
        }

        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_profiles.ContainsKey(id));
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "AISAM.API";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private sealed class FakeWorkspaceRepository : IWorkspaceRepository
    {
        public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Workspace?>(null);
        public Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Workspace?>(null);
        public Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Workspace>>(Array.Empty<Workspace>());
        public Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<PagedResult<Workspace>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id) => Task.FromResult<User?>(null);
        public Task<User?> GetByEmailAsync(string email) => Task.FromResult<User?>(null);
        public Task<User> CreateAsync(User user) => throw new NotImplementedException();
        public Task<User> UpdateAsync(User user) => throw new NotImplementedException();
        public Task<User?> GetByPasswordResetTokenAsync(string token) => Task.FromResult<User?>(null);
        public Task<User?> GetByEmailVerificationTokenAsync(string token) => Task.FromResult<User?>(null);
        public Task<PagedResult<UserListDto>> GetPagedUsersAsync(PaginationRequest request) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<UserListDto>> GetPagedUsersWithRoleFilterAsync(PaginationRequest request, int? role, bool? isEmailVerified, string? search, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
