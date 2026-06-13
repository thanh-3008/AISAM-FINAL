using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

public class SocialServiceTests
{
    [Fact]
    public async Task GetAuthUrlAsync_CreatesStateForActiveProfile()
    {
        var provider = new FakeProviderService
        {
            AuthUrl = "https://facebook.example/auth?state=test"
        };
        var stateStore = new FakeOAuthStateStore("generated-state");
        var service = CreateService(providerService: provider, oauthStateStore: stateStore);
        var profileId = Guid.NewGuid();

        var result = await service.GetAuthUrlAsync("facebook", profileId);

        Assert.Equal("generated-state", result.State);
        Assert.Equal("https://facebook.example/auth?state=test", result.AuthUrl);
        Assert.Equal(profileId, stateStore.LastCreatedProfileId);
    }

    [Fact]
    public async Task LinkAccountAsync_UpdatesExistingAccountToken_WhenFacebookAccountAlreadyLinked()
    {
        var profileId = Guid.NewGuid();
        var existing = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Platform = SocialPlatformEnum.Facebook,
            AccountId = "fb-user",
            UserAccessToken = "old-token"
        };
        var accountRepository = new FakeSocialAccountRepository(existing);
        var provider = new FakeProviderService
        {
            ExchangeAccount = new SocialAccountDto
            {
                Provider = "facebook",
                ProviderUserId = "fb-user",
                AccessToken = "fresh-user-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            }
        };
        var tokenProtector = new FakeSocialTokenProtector();
        var service = CreateService(
            accountRepository: accountRepository,
            providerService: provider,
            tokenProtector: tokenProtector,
            oauthStateStore: new FakeOAuthStateStore("valid-state", profileId, "facebook"));

        var result = await service.LinkAccountAsync("facebook", profileId, new SocialCallbackRequest
        {
            Code = "oauth-code",
            State = "valid-state"
        });

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal("protected:fresh-user-token", existing.UserAccessToken);
        Assert.True(existing.IsActive);
        Assert.Equal("fresh-user-token", tokenProtector.LastProtectedPlaintext);
    }

    [Fact]
    public async Task LinkSelectedTargetsForAccountAsync_ReturnsBrandError_WhenBrandBelongsToAnotherProfile()
    {
        var profileId = Guid.NewGuid();
        var account = CreateAccount(profileId);
        var brand = new Brand { Id = Guid.NewGuid(), ProfileId = Guid.NewGuid(), Name = "Other brand" };
        var service = CreateService(
            accountRepository: new FakeSocialAccountRepository(account),
            brandRepository: new FakeBrandRepository(brand),
            providerService: new FakeProviderService
            {
                Targets = new[]
                {
                    new AvailableTargetDto { ProviderTargetId = "page-1", Name = "Page One", Type = "page" }
                }
            });

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.LinkSelectedTargetsForAccountAsync(
            profileId,
            account.Id,
            new LinkSelectedTargetsRequest
            {
                BrandId = brand.Id,
                Provider = "facebook",
                ProviderTargetIds = new List<string> { "page-1" }
            }));

        Assert.Equal("Brand not found.", exception.Message);
    }

    [Fact]
    public async Task LinkSelectedTargetsForAccountAsync_CreatesIntegrationWithProtectedPageToken()
    {
        var profileId = Guid.NewGuid();
        var brand = new Brand { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Brand" };
        var account = CreateAccount(profileId);
        var provider = new FakeProviderService
        {
            Targets = new[]
            {
                new AvailableTargetDto { ProviderTargetId = "page-1", Name = "Page One", Type = "page" }
            },
            TargetAccessTokens = new Dictionary<string, string>
            {
                ["page-1"] = "page-token"
            }
        };
        var tokenProtector = new FakeSocialTokenProtector();
        var integrationRepository = new FakeSocialIntegrationRepository();
        var service = CreateService(
            accountRepository: new FakeSocialAccountRepository(account),
            integrationRepository: integrationRepository,
            brandRepository: new FakeBrandRepository(brand),
            providerService: provider,
            tokenProtector: tokenProtector);

        var result = await service.LinkSelectedTargetsForAccountAsync(profileId, account.Id, new LinkSelectedTargetsRequest
        {
            BrandId = brand.Id,
            Provider = "facebook",
            ProviderTargetIds = new List<string> { "page-1" }
        });

        var integration = Assert.Single(integrationRepository.Integrations.Values);
        Assert.Equal("protected:page-token", integration.AccessToken);
        Assert.Equal(brand.Id, integration.BrandId);
        Assert.Single(result.Targets);
    }

    [Fact]
    public async Task UnlinkAccountAsync_SoftDeletesAccountAndIntegrations()
    {
        var profileId = Guid.NewGuid();
        var account = CreateAccount(profileId);
        var integration = CreateIntegration(profileId, account.Id);
        account.SocialIntegrations.Add(integration);
        var integrationRepository = new FakeSocialIntegrationRepository(integration);
        var accountRepository = new FakeSocialAccountRepository(account);
        var service = CreateService(
            accountRepository: accountRepository,
            integrationRepository: integrationRepository);

        var result = await service.UnlinkAccountAsync(profileId, account.Id);

        Assert.True(result);
        Assert.True(account.IsDeleted);
        Assert.False(account.IsActive);
        Assert.True(integration.IsDeleted);
        Assert.False(integration.IsActive);
    }

    [Fact]
    public async Task UnlinkTargetAsync_SoftDeletesOnlyRequestedIntegration()
    {
        var profileId = Guid.NewGuid();
        var account = CreateAccount(profileId);
        var target = CreateIntegration(profileId, account.Id);
        var other = CreateIntegration(profileId, account.Id);
        var integrationRepository = new FakeSocialIntegrationRepository(target, other);
        var service = CreateService(
            accountRepository: new FakeSocialAccountRepository(account),
            integrationRepository: integrationRepository);

        var result = await service.UnlinkTargetAsync(profileId, target.Id);

        Assert.True(result);
        Assert.True(target.IsDeleted);
        Assert.False(target.IsActive);
        Assert.False(other.IsDeleted);
        Assert.True(other.IsActive);
    }

    private static SocialService CreateService(
        FakeSocialAccountRepository? accountRepository = null,
        FakeSocialIntegrationRepository? integrationRepository = null,
        FakeBrandRepository? brandRepository = null,
        FakeProviderService? providerService = null,
        FakeOAuthStateStore? oauthStateStore = null,
        FakeSocialTokenProtector? tokenProtector = null)
    {
        return new SocialService(
            accountRepository ?? new FakeSocialAccountRepository(),
            integrationRepository ?? new FakeSocialIntegrationRepository(),
            brandRepository ?? new FakeBrandRepository(),
            oauthStateStore ?? new FakeOAuthStateStore("state"),
            tokenProtector ?? new FakeSocialTokenProtector(),
            Options.Create(new FacebookSettings
            {
                RedirectUri = "https://server/callback"
            }),
            new IProviderService[] { providerService ?? new FakeProviderService() });
    }

    private static SocialAccount CreateAccount(Guid profileId)
    {
        return new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Platform = SocialPlatformEnum.Facebook,
            AccountId = "fb-user",
            UserAccessToken = "protected:user-token",
            IsActive = true
        };
    }

    private static SocialIntegration CreateIntegration(Guid profileId, Guid socialAccountId)
    {
        return new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = Guid.NewGuid(),
            SocialAccountId = socialAccountId,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = Guid.NewGuid().ToString("N"),
            AccessToken = "protected:page-token",
            IsActive = true
        };
    }

    private sealed class FakeSocialAccountRepository : ISocialAccountRepository
    {
        public Dictionary<Guid, SocialAccount> Accounts { get; } = new();

        public FakeSocialAccountRepository(params SocialAccount[] accounts)
        {
            foreach (var account in accounts)
            {
                Accounts[account.Id] = account;
            }
        }

        public Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Accounts.TryGetValue(id, out var account);
            return Task.FromResult(account is { IsDeleted: false } ? account : null);
        }

        public Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Accounts.TryGetValue(id, out var account);
            return Task.FromResult(account is { IsDeleted: false } ? account : null);
        }

        public Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default)
        {
            var account = Accounts.Values.FirstOrDefault(item =>
                item.ProfileId == profileId &&
                item.Platform == platform &&
                item.AccountId == accountId &&
                !item.IsDeleted);
            return Task.FromResult(account);
        }

        public Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SocialAccount>>(Accounts.Values.Where(account => account.ProfileId == profileId && !account.IsDeleted).ToList());
        }

        public Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default)
        {
            Accounts[account.Id] = account;
            return Task.FromResult(account);
        }

        public Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default)
        {
            Accounts[account.Id] = account;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSocialIntegrationRepository : ISocialIntegrationRepository
    {
        public Dictionary<Guid, SocialIntegration> Integrations { get; } = new();

        public FakeSocialIntegrationRepository(params SocialIntegration[] integrations)
        {
            foreach (var integration in integrations)
            {
                Integrations[integration.Id] = integration;
            }
        }

        public Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Integrations.TryGetValue(id, out var integration);
            return Task.FromResult(integration is { IsDeleted: false } ? integration : null);
        }

        public Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default)
        {
            var integration = Integrations.Values.FirstOrDefault(item =>
                item.SocialAccountId == socialAccountId &&
                item.ExternalId == externalId &&
                !item.IsDeleted);
            return Task.FromResult(integration);
        }

        public Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SocialIntegration>>(Integrations.Values.Where(integration => integration.SocialAccountId == socialAccountId && !integration.IsDeleted).ToList());
        }

        public Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SocialIntegration>>(Integrations.Values.Where(integration => integration.BrandId == brandId && !integration.IsDeleted).ToList());
        }

        public Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default)
        {
            Integrations[integration.Id] = integration;
            return Task.FromResult(integration);
        }

        public Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default)
        {
            Integrations[integration.Id] = integration;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly Dictionary<Guid, Brand> _brands;

        public FakeBrandRepository(params Brand[] brands)
        {
            _brands = brands.ToDictionary(brand => brand.Id);
        }

        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_brands.GetValueOrDefault(id));
        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProviderService : IProviderService
    {
        public string ProviderName => "facebook";
        public string AuthUrl { get; set; } = "https://facebook.example/auth";
        public SocialAccountDto ExchangeAccount { get; set; } = new()
        {
            Provider = "facebook",
            ProviderUserId = "fb-user",
            AccessToken = "fresh-token"
        };
        public IEnumerable<AvailableTargetDto> Targets { get; set; } = Array.Empty<AvailableTargetDto>();
        public Dictionary<string, string> TargetAccessTokens { get; set; } = new();

        public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
            => Task.FromResult(AuthUrl);

        public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
            => Task.FromResult(ExchangeAccount);

        public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
            => Task.FromResult(Targets);

        public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default)
            => Task.FromResult(TargetAccessTokens);

        public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeOAuthStateStore : IOAuthStateStore
    {
        private readonly string _state;
        private readonly Guid? _profileId;
        private readonly string _provider;
        public Guid? LastCreatedProfileId { get; private set; }

        public FakeOAuthStateStore(string state, Guid? profileId = null, string provider = "facebook")
        {
            _state = state;
            _profileId = profileId;
            _provider = provider;
        }

        public Task<string> CreateAsync(Guid profileId, string provider, CancellationToken cancellationToken = default)
        {
            LastCreatedProfileId = profileId;
            return Task.FromResult(_state);
        }

        public Task<OAuthStatePayload?> ConsumeAsync(string state, Guid profileId, string provider, CancellationToken cancellationToken = default)
        {
            if (state != _state)
            {
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            if (_profileId.HasValue && _profileId != profileId)
            {
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            if (!string.Equals(_provider, provider, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            return Task.FromResult<OAuthStatePayload?>(new OAuthStatePayload
            {
                State = state,
                ProfileId = profileId,
                Provider = provider,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            });
        }
    }

    private sealed class FakeSocialTokenProtector : ISocialTokenProtector
    {
        public string? LastProtectedPlaintext { get; private set; }

        public string Protect(string plaintext)
        {
            LastProtectedPlaintext = plaintext;
            return $"protected:{plaintext}";
        }

        public string Unprotect(string ciphertext)
        {
            return ciphertext.StartsWith("protected:", StringComparison.Ordinal)
                ? ciphertext["protected:".Length..]
                : ciphertext;
        }
    }
}
