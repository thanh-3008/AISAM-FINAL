using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AISAM.API.Controllers;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

public sealed class MultiDomainOAuthTests
{
    private static IConfiguration CreateConfig(params string[] allowedOrigins)
    {
        var dict = new Dictionary<string, string?>();
        for (int i = 0; i < allowedOrigins.Length; i++)
        {
            dict[$"AllowedOrigins:{i}"] = allowedOrigins[i];
        }
        dict["FrontendSettings:BaseUrl"] = "https://aisam.io.vn";

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Theory]
    [InlineData("https://aisam.io.vn", "https://aisam.io.vn")]
    [InlineData("https://aisam.ddns.net", "https://aisam.ddns.net")]
    [InlineData("http://localhost:3000", "http://localhost:3000")]
    [InlineData("https://aisam.ddns.net/", "https://aisam.ddns.net")]
    public void OriginResolver_ResolvesAllowedOrigins_Successfully(string candidate, string expected)
    {
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net", "http://localhost:3000");
        var resolver = new OriginResolver(config);

        var resolved = resolver.ResolveOrigin(candidate);

        Assert.Equal(expected, resolved);
        Assert.True(resolver.IsAllowedOrigin(candidate));
    }

    [Theory]
    [InlineData("https://evil.com")]
    [InlineData("http://attacker.ddns.net")]
    [InlineData("https://aisam.ddns.net.evil.com")]
    [InlineData("javascript:alert(1)")]
    public void OriginResolver_ThrowsException_WhenOriginNotAllowed(string maliciousOrigin)
    {
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net");
        var resolver = new OriginResolver(config);

        var ex = Assert.Throws<InvalidOperationException>(() => resolver.ResolveOrigin(maliciousOrigin));
        Assert.Contains("not allowed", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(resolver.IsAllowedOrigin(maliciousOrigin));
    }

    [Fact]
    public void OriginResolver_ResolvesOriginFromReferer_WhenOriginHeaderMissing()
    {
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net");
        var resolver = new OriginResolver(config);

        var context = new DefaultHttpContext();
        context.Request.Headers["Referer"] = "https://aisam.ddns.net/social/connect?tab=facebook";

        var resolved = resolver.ResolveOrigin(context.Request);

        Assert.Equal("https://aisam.ddns.net", resolved);
    }

    [Fact]
    public void OriginResolver_ResolvesOriginFromForwardedHost_WhenInAllowedOrigins()
    {
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net");
        var resolver = new OriginResolver(config);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-Host"] = "aisam.ddns.net";
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        var resolved = resolver.ResolveOrigin(context.Request);

        Assert.Equal("https://aisam.ddns.net", resolved);
    }

    [Fact]
    public void OriginResolver_FallsBackToDefault_WhenNoOriginHeadersPresent()
    {
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net");
        var resolver = new OriginResolver(config);

        var context = new DefaultHttpContext();

        var resolved = resolver.ResolveOrigin(context.Request);

        Assert.Equal("https://aisam.io.vn", resolved);
    }

    [Fact]
    public async Task SignedOAuthStateStore_StoresAndRecoversOriginAndRedirectUri()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);
        var profileId = Guid.NewGuid();
        var origin = "https://aisam.ddns.net";
        var redirectUri = "https://aisam.ddns.net/social-callback/facebook";

        var state = await store.CreateAsync(profileId, "facebook", origin, redirectUri);
        var payload = await store.ConsumeAsync(state, profileId, "facebook");

        Assert.NotNull(payload);
        Assert.Equal(profileId, payload!.ProfileId);
        Assert.Equal("facebook", payload.Provider);
        Assert.Equal(origin, payload.Origin);
        Assert.Equal(redirectUri, payload.RedirectUri);
    }

    [Fact]
    public async Task SignedOAuthStateStore_RejectsReplay_WhenStateConsumedTwice()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);
        var profileId = Guid.NewGuid();

        var state = await store.CreateAsync(profileId, "facebook", "https://aisam.ddns.net", "https://aisam.ddns.net/social-callback/facebook");

        // First consume succeeds
        var first = await store.ConsumeAsync(state, profileId, "facebook");
        Assert.NotNull(first);

        // Second consume (replay attack) fails
        var second = await store.ConsumeAsync(state, profileId, "facebook");
        Assert.Null(second);
    }

    [Fact]
    public async Task SignedOAuthStateStore_RejectsTamperedState()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);
        var profileId = Guid.NewGuid();

        var state = await store.CreateAsync(profileId, "facebook", "https://aisam.ddns.net", "https://aisam.ddns.net/social-callback/facebook");

        var parts = state.Split('.');
        var tampered = parts[0] + "x." + parts[1];

        var result = await store.ConsumeAsync(tampered, profileId, "facebook");
        Assert.Null(result);
    }

    [Fact]
    public async Task SignedOAuthStateStore_RejectsExpiredState()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);
        var profileId = Guid.NewGuid();

        // Construct an expired state payload manually with valid signature
        var expiredPayload = new OAuthStatePayload
        {
            State = Guid.NewGuid().ToString("N"),
            ProfileId = profileId,
            Provider = "facebook",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5)
        };
        var json = JsonSerializer.Serialize(expiredPayload);
        var payloadPart = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes("test-signing-secret-minimum-32-chars-long"));
        var sigPart = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadPart))).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var expiredState = $"{payloadPart}.{sigPart}";

        var result = await store.ConsumeAsync(expiredState, profileId, "facebook");
        Assert.Null(result);
    }

    [Fact]
    public async Task SignedOAuthStateStore_TryPeekOrigin_DoesNotConsumeState()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);
        var profileId = Guid.NewGuid();
        var origin = "https://aisam.ddns.net";

        var state = await store.CreateAsync(profileId, "facebook", origin, $"{origin}/social-callback/facebook");

        // Peek should return origin
        var peeked = store.TryPeekOrigin(state);
        Assert.Equal(origin, peeked);

        // State is NOT consumed by peek; ConsumeAsync still succeeds
        var payload = await store.ConsumeAsync(state, profileId, "facebook");
        Assert.NotNull(payload);
        Assert.Equal(origin, payload!.Origin);
    }

    [Fact]
    public async Task SocialService_GetAuthUrlAsync_BuildsRedirectUri_FromOrigin()
    {
        var fakeProvider = new TrackingFakeProvider("facebook");
        var cache = new MemoryCache(new MemoryCacheOptions());
        var stateStore = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);

        var service = new SocialService(
            new FakeSocialAccountRepository(),
            new FakeSocialIntegrationRepository(),
            new FakeBrandRepository(),
            stateStore,
            new FakeSocialTokenProtector(),
            Options.Create(new FacebookSettings { RedirectUri = "http://localhost:3000/social-callback/facebook" }),
            Options.Create(new InstagramSettings { RedirectUri = "http://localhost:3000/auth/instagram/callback" }),
            Options.Create(new TikTokSettings { RedirectUri = "http://localhost:3000/social-callback/tiktok" }),
            new[] { fakeProvider });

        var profileId = Guid.NewGuid();
        var result = await service.GetAuthUrlAsync("facebook", profileId, "https://aisam.ddns.net");

        Assert.NotNull(result);
        Assert.Equal("https://aisam.ddns.net/social-callback/facebook", fakeProvider.LastRedirectUri);
    }

    [Fact]
    public async Task SocialService_LinkAccountAsync_UsesDecodedRedirectUriFromState()
    {
        var fakeProvider = new TrackingFakeProvider("facebook")
        {
            ExchangeAccount = new SocialAccountDto
            {
                Provider = "facebook",
                ProviderUserId = "fb-user-123",
                AccessToken = "fresh-access-token",
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            }
        };
        var cache = new MemoryCache(new MemoryCacheOptions());
        var stateStore = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);

        var service = new SocialService(
            new FakeSocialAccountRepository(),
            new FakeSocialIntegrationRepository(),
            new FakeBrandRepository(),
            stateStore,
            new FakeSocialTokenProtector(),
            Options.Create(new FacebookSettings { RedirectUri = "http://localhost:3000/social-callback/facebook" }),
            Options.Create(new InstagramSettings { RedirectUri = "http://localhost:3000/auth/instagram/callback" }),
            Options.Create(new TikTokSettings { RedirectUri = "http://localhost:3000/social-callback/tiktok" }),
            new[] { fakeProvider });

        var profileId = Guid.NewGuid();
        var origin = "https://aisam.ddns.net";
        var expectedRedirectUri = "https://aisam.ddns.net/social-callback/facebook";

        var authResult = await service.GetAuthUrlAsync("facebook", profileId, origin);

        var workspaceId = Guid.NewGuid();
        var linkResult = await service.LinkAccountInWorkspaceAsync("facebook", workspaceId, profileId, new SocialCallbackRequest
        {
            Code = "valid-oauth-code",
            State = authResult.State
        });

        Assert.NotNull(linkResult);
        Assert.Equal(expectedRedirectUri, fakeProvider.LastExchangeRedirectUri);
    }

    [Fact]
    public async Task SocialAuthRelayController_RelaysToOriginFromState()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var stateStore = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net");
        var resolver = new OriginResolver(config);

        var state = await stateStore.CreateAsync(Guid.NewGuid(), "facebook", "https://aisam.ddns.net", "https://aisam.ddns.net/social-callback/facebook");

        var controller = new SocialAuthRelayController(
            Options.Create(new FrontendSettings { BaseUrl = "https://aisam.io.vn" }),
            stateStore,
            resolver)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString($"?code=oauth-code&state={state}")
                    }
                }
            }
        };

        var result = Assert.IsType<RedirectResult>(controller.RelayFacebookCallback());
        Assert.StartsWith("https://aisam.ddns.net/social-callback/facebook", result.Url);
        Assert.Contains("code=oauth-code", result.Url);
    }

    [Fact]
    public async Task FacebookProvider_GetAuthUrlAsync_Succeeds_WhenRedirectUriEmptyButRedirectPathConfigured()
    {
        var settings = Options.Create(new FacebookSettings
        {
            AppId = "test-app-id",
            AppSecret = "test-app-secret",
            RedirectUri = "",
            RedirectPath = "/social-callback/facebook"
        });
        var provider = new FacebookProvider(new HttpClient(), settings, Microsoft.Extensions.Logging.Abstractions.NullLogger<FacebookProvider>.Instance);

        var url = await provider.GetAuthUrlAsync("test-state", "https://aisam.ddns.net/social-callback/facebook");

        Assert.Contains("redirect_uri=" + Uri.EscapeDataString("https://aisam.ddns.net/social-callback/facebook"), url);
    }

    [Fact]
    public async Task TikTokProvider_GetAuthUrlAsync_Succeeds_WhenRedirectUriEmptyButRedirectPathConfigured()
    {
        var settings = Options.Create(new TikTokSettings
        {
            ClientKey = "test-client-key",
            ClientSecret = "test-client-secret",
            RedirectUri = "",
            RedirectPath = "/social-callback/tiktok"
        });
        var provider = new TikTokProvider(new HttpClient(), settings, Microsoft.Extensions.Logging.Abstractions.NullLogger<TikTokProvider>.Instance);

        var url = await provider.GetAuthUrlAsync("test-state", "https://aisam.ddns.net/social-callback/tiktok");

        Assert.Contains("redirect_uri=" + Uri.EscapeDataString("https://aisam.ddns.net/social-callback/tiktok"), url);
    }

    [Fact]
    public async Task SocialService_ResolveRedirectUri_BuildsFromRedirectPath_WhenRedirectUriEmpty()
    {
        var fakeProvider = new TrackingFakeProvider("facebook");
        var cache = new MemoryCache(new MemoryCacheOptions());
        var stateStore = new SignedOAuthStateStore("test-signing-secret-minimum-32-chars-long", cache);
        var resolver = new OriginResolver(CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net"));

        var service = new SocialService(
            new FakeSocialAccountRepository(),
            new FakeSocialIntegrationRepository(),
            new FakeBrandRepository(),
            stateStore,
            new FakeSocialTokenProtector(),
            Options.Create(new FacebookSettings { RedirectUri = "", RedirectPath = "/social-callback/facebook" }),
            Options.Create(new InstagramSettings { RedirectUri = "", RedirectPath = "/auth/instagram/callback" }),
            Options.Create(new TikTokSettings { RedirectUri = "", RedirectPath = "/social-callback/tiktok" }),
            new[] { fakeProvider },
            resolver);

        var profileId = Guid.NewGuid();
        var result = await service.GetAuthUrlAsync("facebook", profileId, "https://aisam.ddns.net");

        Assert.NotNull(result);
        Assert.Equal("https://aisam.ddns.net/social-callback/facebook", fakeProvider.LastRedirectUri);
    }

    [Fact]
    public void PayOSPaymentService_ResolvePayOsUrls_DynamicallyResolvesBothDomains()
    {
        var resolver = new OriginResolver(CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net"));
        var settings = Options.Create(new PayOSSettings
        {
            ReturnPath = "/payment/success",
            CancelPath = "/payment/cancel"
        });

        var service = new PayOSPaymentService(
            null!, null!, null!, null!, null!, null!, null!,
            settings, new HttpClient(), null!, null, null, resolver);

        var method = typeof(PayOSPaymentService).GetMethod("ResolvePayOsUrls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);

        // 1. aisam.io.vn
        var res1 = method!.Invoke(service, new object?[] { "https://aisam.io.vn/pricing", "https://aisam.io.vn/pricing?cancel=1" });
        var tuple1 = ((string returnUrl, string cancelUrl, string? error))res1!;
        Assert.Equal("https://aisam.io.vn/pricing", tuple1.returnUrl);
        Assert.Equal("https://aisam.io.vn/pricing?cancel=1", tuple1.cancelUrl);
        Assert.Null(tuple1.error);

        // 2. aisam.ddns.net
        var res2 = method.Invoke(service, new object?[] { "https://aisam.ddns.net/pricing", "https://aisam.ddns.net/pricing?cancel=1" });
        var tuple2 = ((string returnUrl, string cancelUrl, string? error))res2!;
        Assert.Equal("https://aisam.ddns.net/pricing", tuple2.returnUrl);
        Assert.Equal("https://aisam.ddns.net/pricing?cancel=1", tuple2.cancelUrl);
        Assert.Null(tuple2.error);

        // 3. Null candidate URLs -> defaults to OriginResolver default origin + configured paths
        var res3 = method.Invoke(service, new object?[] { null, null });
        var tuple3 = ((string returnUrl, string cancelUrl, string? error))res3!;
        Assert.Equal("https://aisam.io.vn/payment/success", tuple3.returnUrl);
        Assert.Equal("https://aisam.io.vn/payment/cancel", tuple3.cancelUrl);
        Assert.Null(tuple3.error);

        // 4. Malicious candidate URL -> rejected
        var res4 = method.Invoke(service, new object?[] { "https://evil.com/phish", null });
        var tuple4 = ((string returnUrl, string cancelUrl, string? error))res4!;
        Assert.NotNull(tuple4.error);
        Assert.Contains("not allowed", tuple4.error, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TrackingFakeProvider : IProviderService
    {
        public string ProviderName { get; }
        public string? LastRedirectUri { get; private set; }
        public string? LastExchangeRedirectUri { get; private set; }
        public SocialAccountDto ExchangeAccount { get; set; } = new();

        public TrackingFakeProvider(string name)
        {
            ProviderName = name;
        }

        public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
        {
            LastRedirectUri = redirectUri;
            return Task.FromResult($"https://auth.example.com/oauth?redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}");
        }

        public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
        {
            LastExchangeRedirectUri = redirectUri;
            return Task.FromResult(ExchangeAccount);
        }

        public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<AvailableTargetDto>>(Array.Empty<AvailableTargetDto>());

        public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default)
            => Task.FromResult(new Dictionary<string, string>());

        public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
            => Task.FromResult(new PublishResultDto());

        public Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<FacebookAdAccountData>>(Array.Empty<FacebookAdAccountData>());

        public Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, string? objectStoryId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => Task.FromResult<FacebookInsightData?>(null);
        public Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateCampaignNameAsync(string adAccountId, string userAccessToken, string campaignId, string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateAdSetBudgetAsync(string adAccountId, string userAccessToken, string adSetId, decimal dailyBudget, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string?> GetAdEffectiveStatusAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
        public Task<string?> GetAdSetEffectiveStatusAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
        public Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeSocialAccountRepository : ISocialAccountRepository
    {
        private readonly List<SocialAccount> _accounts = new();

        public Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_accounts.FirstOrDefault(a => a.Id == id));
        public Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_accounts.FirstOrDefault(a => a.Id == id));
        public Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SocialAccount>>(_accounts.Where(a => a.ProfileId == profileId).ToList());
        public Task<IReadOnlyList<SocialAccount>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SocialAccount>>(_accounts.Where(a => a.WorkspaceId == workspaceId).ToList());
        public Task<IReadOnlyList<SocialAccount>> GetByProfileIdsAsync(IEnumerable<Guid> profileIds, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SocialAccount>>(_accounts.Where(a => profileIds.Contains(a.ProfileId)).ToList());
        public Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default) => Task.FromResult(_accounts.FirstOrDefault(a => a.ProfileId == profileId && a.Platform == platform && a.AccountId == accountId));
        public Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default)
        {
            if (account.Id == Guid.Empty) account.Id = Guid.NewGuid();
            _accounts.Add(account);
            return Task.FromResult(account);
        }
        public Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default)
        {
            var idx = _accounts.FindIndex(a => a.Id == account.Id);
            if (idx >= 0) _accounts[idx] = account;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSocialIntegrationRepository : ISocialIntegrationRepository
    {
        public Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<SocialIntegration?>(null);
        public Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SocialIntegration>>(new List<SocialIntegration>());
        public Task<IReadOnlyList<SocialIntegration>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SocialIntegration>>(new List<SocialIntegration>());
        public Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SocialIntegration>>(new List<SocialIntegration>());
        public Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default) => Task.FromResult<SocialIntegration?>(null);
        public Task<SocialIntegration?> GetByWorkspacePlatformExternalIdAsync(Guid workspaceId, SocialPlatformEnum platform, string externalId, CancellationToken cancellationToken = default) => Task.FromResult<SocialIntegration?>(null);
        public Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => Task.FromResult(integration);
        public Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Brand?>(null);
        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Brand?>(null);
        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Brand>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Brand>>(new List<Brand>());
        public Task<IReadOnlyList<Brand>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Brand>>(new List<Brand>());
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => Task.FromResult(brand);
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> ExistsByNameInWorkspaceAsync(Guid workspaceId, string name, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<Brand>> GetByNamesAndIdsAsync(Guid workspaceId, IEnumerable<string> names, IEnumerable<Guid> ids, CancellationToken cancellationToken = default) => Task.FromResult(new List<Brand>());
    }

    private sealed class FakeSocialTokenProtector : ISocialTokenProtector
    {
        public string Protect(string plaintext) => $"enc:{plaintext}";
        public string Unprotect(string ciphertext) => ciphertext.StartsWith("enc:") ? ciphertext[4..] : ciphertext;
        public string? TryUnprotect(string ciphertext) => ciphertext.StartsWith("enc:") ? ciphertext[4..] : ciphertext;
    }
}
