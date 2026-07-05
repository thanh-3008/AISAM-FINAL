using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using System.Net;
using System.Security.Cryptography;

namespace AISAM.IntegrationTests;

public class ContentServicePublishTests
{
    [Fact]
    public async Task PublishAsync_SetsContentPublishedAndCreatesPost_WhenFacebookReturnsSuccess()
    {
        var profileId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandId,
            AdType = AdTypeEnum.ImageText,
            TextContent = "Publish me",
            ImageUrl = "[\"https://example.com/image-1.png\",\"https://example.com/image-2.png\"]",
            Status = ContentStatusEnum.Approved
        };
        var account = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "protected:user-token"
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandId,
            SocialAccountId = account.Id,
            SocialAccount = account,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "page-1",
            AccessToken = "protected:page-token"
        };
        var postRepository = new FakePostRepository();
        var provider = new FakeProviderService
        {
            PublishResult = new PublishResultDto
            {
                Success = true,
                ProviderPostId = "facebook-post-1",
                PostedAt = DateTime.UtcNow,
                RefreshedTargetAccessToken = "fresh-page-token"
            }
        };
        var tokenProtector = new FakeSocialTokenProtector();
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeBrandRepository(),
            new FakeProductRepository(),
            new FakeSocialIntegrationRepository(integration),
            new FakeSocialAccountRepository(account),
            postRepository,
            provider,
            tokenProtector,
            new FakeQuotaService());

        var result = await service.PublishAsync(content.Id, integration.Id, profileId);

        Assert.True(result.Success);
        Assert.Equal(ContentStatusEnum.Published, content.Status);
        var post = Assert.Single(postRepository.Added);
        Assert.Equal(content.Id, post.ContentId);
        Assert.Equal(integration.Id, post.IntegrationId);
        Assert.Equal("facebook-post-1", post.ExternalPostId);
        Assert.Equal("user-token", provider.LastPublishedAccount!.UserAccessToken);
        Assert.Equal("page-token", provider.LastPublishedIntegration!.AccessToken);
        Assert.Equal(2, provider.LastPublishedPost!.ImageUrls!.Count);
        Assert.Equal("protected:fresh-page-token", integration.AccessToken);
        Assert.Equal("fresh-page-token", tokenProtector.LastProtectedPlaintext);
    }

    [Fact]
    public async Task PublishAsync_KeepsContentStatusUnchanged_WhenProviderFails()
    {
        var profileId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandId,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Publish me",
            Status = ContentStatusEnum.Approved
        };
        var account = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "protected:user-token"
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandId,
            SocialAccountId = account.Id,
            SocialAccount = account,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "page-1",
            AccessToken = "protected:page-token"
        };
        var postRepository = new FakePostRepository();
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeBrandRepository(),
            new FakeProductRepository(),
            new FakeSocialIntegrationRepository(integration),
            new FakeSocialAccountRepository(account),
            postRepository,
            new FakeProviderService
            {
                PublishResult = new PublishResultDto
                {
                    Success = false,
                    ErrorMessage = "Facebook rejected the request."
                }
            },
            new FakeSocialTokenProtector(),
            new FakeQuotaService());

        var result = await service.PublishAsync(content.Id, integration.Id, profileId);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadGateway, result.StatusCode);
        Assert.Equal(ContentStatusEnum.Approved, content.Status);
        Assert.Empty(postRepository.Added);
    }

    [Fact]
    public async Task PublishAsync_ReturnsReconnectRequired_WhenTokenKeyIsUnavailable()
    {
        var profileId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandId,
            Status = ContentStatusEnum.Approved
        };
        var account = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Platform = SocialPlatformEnum.TikTok,
            UserAccessToken = "unreadable-token"
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandId,
            SocialAccountId = account.Id,
            SocialAccount = account,
            Platform = SocialPlatformEnum.TikTok,
            AccessToken = "unreadable-token"
        };
        var provider = new FakeProviderService { ProviderName = "tiktok" };
        var service = CreateService(
            new FakeContentRepository(content),
            socialIntegrationRepository: new FakeSocialIntegrationRepository(integration),
            socialAccountRepository: new FakeSocialAccountRepository(account),
            providerService: provider,
            tokenProtector: new FakeSocialTokenProtector { ThrowOnUnprotect = true });

        var result = await service.PublishAsync(content.Id, integration.Id, profileId);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        Assert.Equal("SOCIAL_RECONNECT_REQUIRED", result.Error?.ErrorCode);
        Assert.Contains("Disconnect and reconnect", result.Message);
        Assert.Null(provider.LastPublishedPost);
    }

    [Fact]
    public async Task PublishAsync_AllowsAlreadyPublishedContentToUseAnotherIntegration()
    {
        var profileId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = Guid.NewGuid(),
            TextContent = "Publish on another platform",
            Status = ContentStatusEnum.Published
        };
        var account = new SocialAccount
        {
            Id = Guid.NewGuid(), ProfileId = profileId, Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "protected:user-token"
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(), ProfileId = profileId, BrandId = content.BrandId,
            SocialAccountId = account.Id, SocialAccount = account,
            Platform = SocialPlatformEnum.Facebook, ExternalId = "page-1", AccessToken = "protected:page-token"
        };
        var posts = new FakePostRepository();
        var service = CreateService(
            new FakeContentRepository(content),
            socialIntegrationRepository: new FakeSocialIntegrationRepository(integration),
            socialAccountRepository: new FakeSocialAccountRepository(account),
            postRepository: posts,
            providerService: new FakeProviderService
            {
                ProviderName = "facebook",
                PublishResult = new PublishResultDto { Success = true, ProviderPostId = "post-2" }
            },
            tokenProtector: new FakeSocialTokenProtector());

        var result = await service.PublishAsync(content.Id, integration.Id, profileId);

        Assert.True(result.Success);
        Assert.Single(posts.Added);
        Assert.Equal("post-2", posts.Added[0].ExternalPostId);
    }

    [Fact]
    public async Task PublishAsync_ReturnsForbiddenWithPostQuotaError_WhenPostQuotaExceeded()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = brandId,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Publish me",
            Status = ContentStatusEnum.Approved
        };
        var account = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "protected:user-token"
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = brandId,
            SocialAccountId = account.Id,
            SocialAccount = account,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "page-1",
            AccessToken = "protected:page-token"
        };
        var postRepository = new FakePostRepository();
        var provider = new FakeProviderService
        {
            PublishResult = new PublishResultDto
            {
                Success = true,
                ProviderPostId = "facebook-post-1"
            }
        };
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeBrandRepository(),
            new FakeProductRepository(),
            new FakeSocialIntegrationRepository(integration),
            new FakeSocialAccountRepository(account),
            postRepository,
            provider,
            new FakeSocialTokenProtector(),
            new FakeQuotaService
            {
                WorkspacePostQuotaResult = GenericResponse<bool>.CreateError(
                    "Post quota has been exceeded for the current subscription.",
                    HttpStatusCode.Forbidden,
                    "POST_QUOTA_EXCEEDED")
            });

        var result = await service.PublishAsync(content.Id, integration.Id, profileId, workspaceId);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Equal("POST_QUOTA_EXCEEDED", result.Error?.ErrorCode);
        Assert.Equal(ContentStatusEnum.Approved, content.Status);
        Assert.Empty(postRepository.Added);
        Assert.Null(provider.LastPublishedPost);
    }

    [Fact]
    public async Task PublishAsync_UsesWorkspacePostQuota()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = brandId,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Publish me",
            Status = ContentStatusEnum.Approved
        };
        var account = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "protected:user-token"
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = brandId,
            SocialAccountId = account.Id,
            SocialAccount = account,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "page-1",
            AccessToken = "protected:page-token"
        };
        var quotaService = new FakeQuotaService();
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeBrandRepository(),
            new FakeProductRepository(),
            new FakeSocialIntegrationRepository(integration),
            new FakeSocialAccountRepository(account),
            new FakePostRepository(),
            new FakeProviderService(),
            new FakeSocialTokenProtector(),
            quotaService);

        await service.PublishAsync(content.Id, integration.Id, profileId, workspaceId);

        Assert.Equal(workspaceId, quotaService.LastWorkspaceId);
    }

    [Fact]
    public async Task PublishAsync_ReturnsNotFound_WhenIntegrationBelongsToAnotherProfile()
    {
        var profileId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = Guid.NewGuid(),
            TextContent = "Publish me",
            Status = ContentStatusEnum.Approved
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            BrandId = content.BrandId,
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "protected:page-token",
            SocialAccount = new SocialAccount
            {
                Id = Guid.NewGuid(),
                ProfileId = Guid.NewGuid(),
                Platform = SocialPlatformEnum.Facebook,
                UserAccessToken = "protected:user-token"
            }
        };
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeBrandRepository(),
            new FakeProductRepository(),
            new FakeSocialIntegrationRepository(integration),
            null,
            null,
            null,
            null,
            new FakeQuotaService());

        var result = await service.PublishAsync(content.Id, integration.Id, profileId);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Social integration not found or inactive.", result.Message);
    }

    private static ContentService CreateService(
        IContentRepository? contentRepository = null,
        IBrandRepository? brandRepository = null,
        IProductRepository? productRepository = null,
        ISocialIntegrationRepository? socialIntegrationRepository = null,
        ISocialAccountRepository? socialAccountRepository = null,
        IPostRepository? postRepository = null,
        IProviderService? providerService = null,
        ISocialTokenProtector? tokenProtector = null,
        IQuotaService? quotaService = null,
        IContentCalendarRepository? contentCalendarRepository = null,
        IWorkspaceRepository? workspaceRepository = null)
    {
        return new ContentService(
            contentRepository ?? new FakeContentRepository(),
            brandRepository ?? new FakeBrandRepository(),
            productRepository ?? new FakeProductRepository(),
            socialIntegrationRepository ?? new FakeSocialIntegrationRepository(),
            socialAccountRepository ?? new FakeSocialAccountRepository(),
            postRepository ?? new FakePostRepository(),
            providerService is null ? Array.Empty<IProviderService>() : new[] { providerService },
            tokenProtector ?? new FakeSocialTokenProtector(),
            quotaService ?? new FakeQuotaService(),
            contentCalendarRepository ?? new FakeContentCalendarRepository(),
            workspaceRepository ?? new FakeWorkspaceRepository());
    }

    private sealed class FakeContentCalendarRepository : IContentCalendarRepository
    {
        public Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<ContentCalendar?>(null);
        public Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<ContentCalendar>());
        public Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ContentCalendar>>([]);
        public Task<int> CountUpcomingByProfileIdAsync(Guid profileId, DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountFailedByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ContentCalendar>>([]);
        public Task<IReadOnlyList<ContentCalendar>> ClaimDueSchedulesAtomicallyAsync(DateTime utcNow, int limit, int maxAttemptCount, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ContentCalendar>>([]);
        public Task<bool> HasActiveScheduleAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task CancelActiveSchedulesForContentAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default) => Task.FromResult(schedule);
        public Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<PagedResult<ContentCalendar>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<ContentCalendar>> GetUpcomingByWorkspaceIdAsync(Guid workspaceId, int limit, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> CountUpcomingByWorkspaceIdAsync(Guid workspaceId, DateTime utcNow, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> CountFailedByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        private readonly Dictionary<Guid, Content> _contents;

        public FakeContentRepository(params Content[] contents)
        {
            _contents = contents.ToDictionary(content => content.Id);
        }

        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _contents.TryGetValue(id, out var content);
            return Task.FromResult(content is { IsDeleted: false } ? content : null);
        }

        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _contents.TryGetValue(id, out var content);
            return Task.FromResult(content);
        }

        public Task<PagedResult<Content>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default)
        {
            _contents[content.Id] = content;
            return Task.CompletedTask;
        }

        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Content>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Brand?>(null);
        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Brand?>(null);
        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Product?>(null);
        public Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Product?>(null);
        public Task<PagedResult<Product>> GetPagedAsync(PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetProductsByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetProductsByBrandIdIncludingDeletedAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeSocialIntegrationRepository : ISocialIntegrationRepository
    {
        private readonly Dictionary<Guid, SocialIntegration> _integrations;

        public FakeSocialIntegrationRepository(params SocialIntegration[] integrations)
        {
            _integrations = integrations.ToDictionary(integration => integration.Id);
        }

        public Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _integrations.TryGetValue(id, out var integration);
            return Task.FromResult(integration is { IsDeleted: false } ? integration : null);
        }

        public Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default)
        {
            _integrations[integration.Id] = integration;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSocialAccountRepository : ISocialAccountRepository
    {
        private readonly Dictionary<Guid, SocialAccount> _accounts;

        public FakeSocialAccountRepository(params SocialAccount[] accounts)
        {
            _accounts = accounts.ToDictionary(account => account.Id);
        }

        public Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _accounts.TryGetValue(id, out var account);
            return Task.FromResult(account is { IsDeleted: false } ? account : null);
        }

        public Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakePostRepository : IPostRepository
    {
        public List<Post> Added { get; } = new();

        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Post?>(null);

        public Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default)
        {
            Added.Add(post);
            return Task.FromResult(post);
        }

        public Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteAsync(Post post, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeProviderService : IProviderService
    {
        public string ProviderName { get; set; } = "facebook";
        public PublishResultDto PublishResult { get; set; } = new() { Success = true, ProviderPostId = "provider-post-id", PostedAt = DateTime.UtcNow };
        public SocialAccount? LastPublishedAccount { get; private set; }
        public SocialIntegration? LastPublishedIntegration { get; private set; }
        public PostDto? LastPublishedPost { get; private set; }

        public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
        {
            LastPublishedAccount = new SocialAccount
            {
                Id = account.Id,
                ProfileId = account.ProfileId,
                Platform = account.Platform,
                UserAccessToken = account.UserAccessToken
            };
            LastPublishedIntegration = new SocialIntegration
            {
                Id = integration.Id,
                ProfileId = integration.ProfileId,
                BrandId = integration.BrandId,
                Platform = integration.Platform,
                AccessToken = integration.AccessToken,
                ExternalId = integration.ExternalId
            };
            LastPublishedPost = new PostDto
            {
                Message = post.Message,
                ImageUrl = post.ImageUrl,
                ImageUrls = post.ImageUrls,
                VideoUrl = post.VideoUrl
            };
            return Task.FromResult(PublishResult);
        }
    }

    private sealed class FakeSocialTokenProtector : ISocialTokenProtector
    {
        public string? LastProtectedPlaintext { get; private set; }
        public bool ThrowOnUnprotect { get; set; }

        public string Protect(string plaintext)
        {
            LastProtectedPlaintext = plaintext;
            return $"protected:{plaintext}";
        }

        public string Unprotect(string ciphertext)
        {
            if (ThrowOnUnprotect)
            {
                throw new CryptographicException("Missing key.");
            }

            return ciphertext.StartsWith("protected:", StringComparison.Ordinal)
                ? ciphertext["protected:".Length..]
                : ciphertext;
        }
    }

    private sealed class FakeQuotaService : IQuotaService
    {
        public GenericResponse<bool> PromptQuotaResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<bool> PostQuotaResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<bool> WorkspacePostQuotaResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public Guid LastWorkspaceId { get; private set; }

        public Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto()));

        public Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(PromptQuotaResult);

        public Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(PostQuotaResult);

        public Task<GenericResponse<QuotaSummaryDto>> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto()));
        }

        public Task<GenericResponse<bool>> EnsureWorkspacePostQuotaAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(WorkspacePostQuotaResult);
        }
    }

    private sealed class FakeWorkspaceRepository : IWorkspaceRepository
    {
        public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Workspace?>(null);
        public Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Workspace>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
