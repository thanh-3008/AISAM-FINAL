using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
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

        await middleware.InvokeAsync(context, new FakeProfileRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsUnauthorized_WhenProfileHeaderIsInvalid()
    {
        var context = CreateContext(Guid.NewGuid());
        context.Request.Headers["X-Profile-Id"] = "invalid";
        var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeProfileRepository());

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsForbidden_WhenProfileBelongsToAnotherUser()
    {
        var context = CreateContext(Guid.NewGuid());
        var profile = CreateProfile(Guid.NewGuid());
        context.Request.Headers["X-Profile-Id"] = profile.Id.ToString();
        var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeProfileRepository(profile));

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

        await middleware.InvokeAsync(context, new FakeProfileRepository(profile));

        Assert.True(nextCalled);
        Assert.Equal(profile.Id, context.Items[ProfileContextHelper.ActiveProfileItemKey]);
    }

    private static DefaultHttpContext CreateContext(Guid userId)
    {
        return new DefaultHttpContext
        {
            Request =
            {
                Path = "/api/content"
            },
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "Test"))
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
}
