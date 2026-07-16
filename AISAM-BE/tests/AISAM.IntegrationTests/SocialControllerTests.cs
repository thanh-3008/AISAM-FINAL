using AISAM.API.Controllers;
using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class SocialControllerTests
{
    [Fact]
    public async Task ActiveProfileMiddleware_AutoCreatesProfile_ForDevSchedulerWhenProfileHeaderMissing()
    {
        var context = CreateMiddlewareContext(Guid.NewGuid(), "/api/dev/scheduler");
        var middleware = new ActiveProfileMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, new FakeProfileRepository(), new FakeUserRepository(), new FakeWebHostEnvironment());

        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Callback_ReturnsBadRequest_WhenStateInvalid()
    {
        var controller = CreateAuthController(new FakeSocialService
        {
            LinkAccountException = new InvalidOperationException("OAuth state is invalid or expired.")
        });

        var result = await controller.HandleFacebookCallback(new SocialCallbackRequest
        {
            Code = "oauth-code",
            State = "invalid-state"
        });

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_ReturnsOnlyActiveProfilesAccounts()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeSocialService
        {
            Accounts = new[]
            {
                new SocialAccountDto
                {
                    Id = Guid.NewGuid(),
                    ProfileId = profileId,
                    Provider = "facebook",
                    ProviderUserId = "fb-user"
                }
            }
        };
        var controller = CreateAccountsController(service, profileId);

        var result = await controller.GetMyAccounts();

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
        var response = Assert.IsType<GenericResponse<IReadOnlyList<SocialAccountDto>>>(objectResult.Value);
        Assert.True(response.Success);
        Assert.Equal(profileId, service.LastProfileId);
        Assert.Single(response.Data!);
    }

    [Fact]
    public async Task LinkTargets_ReturnsNotFound_WhenAccountBelongsToAnotherProfile()
    {
        var controller = CreateAccountsController(new FakeSocialService
        {
            LinkTargetsException = new ArgumentException("Social account not found.")
        }, Guid.NewGuid());

        var result = await controller.LinkTargets(Guid.NewGuid(), new LinkSelectedTargetsRequest
        {
            BrandId = Guid.NewGuid(),
            Provider = "facebook",
            ProviderTargetIds = new List<string> { "page-1" }
        });

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    private static SocialAuthController CreateAuthController(ISocialService service, Guid? profileId = null)
    {
        return new SocialAuthController(service, new FakeProfileRepository())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateControllerContext(profileId ?? Guid.NewGuid())
            }
        };
    }

    private static SocialAccountsController CreateAccountsController(ISocialService service, Guid profileId)
    {
        return new SocialAccountsController(service, new FakeProfileRepository())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateControllerContext(profileId)
            }
        };
    }

    private static DefaultHttpContext CreateControllerContext(Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;
        return context;
    }

    private static DefaultHttpContext CreateMiddlewareContext(Guid userId, string path)
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

    private sealed class FakeSocialService : ISocialService
    {
        public Guid LastProfileId { get; private set; }
        public IReadOnlyList<SocialAccountDto> Accounts { get; set; } = Array.Empty<SocialAccountDto>();
        public Exception? LinkAccountException { get; set; }
        public Exception? LinkTargetsException { get; set; }

        public Task<AuthUrlResponse> GetAuthUrlAsync(string provider, Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(new AuthUrlResponse
            {
                AuthUrl = "https://facebook.example/auth",
                State = "state"
            });
        }

        public Task<IReadOnlyList<FacebookAdAccountData>> GetAdAccountsForSocialAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<FacebookAdAccountData>>(new List<FacebookAdAccountData>());
        
        public Task<string?> GetFacebookUserAccessTokenAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);

        public Task<SocialAccountDto> LinkAccountAsync(string provider, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            if (LinkAccountException != null)
            {
                throw LinkAccountException;
            }

            return Task.FromResult(new SocialAccountDto
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                Provider = provider,
                ProviderUserId = "fb-user"
            });
        }

        public Task<IReadOnlyList<SocialAccountDto>> GetProfileAccountsAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(Accounts);
        }

        public Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsForAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AvailableTargetDto>>(Array.Empty<AvailableTargetDto>());

        public Task<SocialAccountDto> LinkSelectedTargetsForAccountAsync(Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            if (LinkTargetsException != null)
            {
                throw LinkTargetsException;
            }

            return Task.FromResult(new SocialAccountDto
            {
                Id = socialAccountId,
                ProfileId = profileId,
                Provider = request.Provider,
                ProviderUserId = "fb-user"
            });
        }

        public Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SocialTargetDto>>(Array.Empty<SocialTargetDto>());

        public Task<bool> UnlinkAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> UnlinkTargetAsync(Guid profileId, Guid socialIntegrationId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandAsync(Guid profileId, Guid brandId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SocialIntegrationDto>>(Array.Empty<SocialIntegrationDto>());

        public Task<SocialAccountDto?> GetSocialAccountByIdAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default)
            => Task.FromResult<SocialAccountDto?>(null);
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Profile?>(null);
        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Profile?>(null);
        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Profile>());
        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Profile>());
        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<Profile>());
        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default) => Task.FromResult(profile);
        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default) => Task.FromResult(profile);
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
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
        public Task<IReadOnlyList<Workspace>> GetAllActiveAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Workspace>>(Array.Empty<Workspace>());
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
        public Task<Dictionary<DateTime, int>> GetDailyRegistrationsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
        public Task<IReadOnlyList<User>> GetAdminsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<User>>(Array.Empty<User>());
        public Task<IReadOnlyList<Session>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Session>>(Array.Empty<Session>());
        public Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<User>>(Array.Empty<User>());
    }
}
