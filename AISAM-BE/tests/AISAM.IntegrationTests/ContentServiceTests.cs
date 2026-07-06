using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class ContentServiceTests
{
    [Fact]
    public async Task CreateAsync_UsesActiveProfile_WhenBrandBelongsToProfile()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var repository = new FakeContentRepository();
        var service = CreateService(repository, new FakeBrandRepository(brand));

        var result = await service.CreateAsync(profileId, new CreateContentRequest
        {
            BrandId = brand.Id,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Needs review"
        });

        Assert.True(result.Success);
        Assert.Equal(profileId, repository.Added.Single().ProfileId);
        Assert.Equal(ContentStatusEnum.PendingApproval, repository.Added.Single().Status);
    }

    [Fact]
    public async Task CreateAsync_UsesPendingApproval_WhenRequestedAtCreation()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var repository = new FakeContentRepository();
        var service = CreateService(repository, new FakeBrandRepository(brand));

        var result = await service.CreateAsync(profileId, new CreateContentRequest
        {
            BrandId = brand.Id,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Needs review",
            Status = ContentStatusEnum.PendingApproval
        });

        Assert.True(result.Success);
        Assert.Equal(ContentStatusEnum.PendingApproval, repository.Added.Single().Status);
    }

    [Fact]
    public async Task CreateAsync_ReturnsBadRequest_WhenLifecycleStatusIsSelectedAtCreation()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var repository = new FakeContentRepository();
        var service = CreateService(repository, new FakeBrandRepository(brand));

        var result = await service.CreateAsync(profileId, new CreateContentRequest
        {
            BrandId = brand.Id,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Published too early",
            Status = ContentStatusEnum.Published
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenBrandBelongsToAnotherProfile()
    {
        var brand = CreateBrand(Guid.NewGuid());
        var repository = new FakeContentRepository();
        var service = CreateService(repository, new FakeBrandRepository(brand));

        var result = await service.CreateAsync(Guid.NewGuid(), new CreateContentRequest
        {
            BrandId = brand.Id,
            TextContent = "Draft"
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task CreateAsync_ReturnsBadRequest_WhenProductDoesNotBelongToBrand()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var product = new Product { Id = Guid.NewGuid(), BrandId = Guid.NewGuid(), Name = "Other product" };
        var repository = new FakeContentRepository();
        var service = CreateService(repository, new FakeBrandRepository(brand), new FakeProductRepository(product));

        var result = await service.CreateAsync(profileId, new CreateContentRequest
        {
            BrandId = brand.Id,
            ProductId = product.Id,
            TextContent = "Draft"
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task CreateAsync_FormatsSingleImageUrl_ForJsonbColumn()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var repository = new FakeContentRepository();
        var service = CreateService(repository, new FakeBrandRepository(brand));

        await service.CreateAsync(profileId, new CreateContentRequest
        {
            BrandId = brand.Id,
            TextContent = "Draft",
            ImageUrl = "https://example.com/image.png"
        });

        Assert.Equal("[\"https://example.com/image.png\"]", repository.Added.Single().ImageUrl);
    }

    [Fact]
    public async Task CloneAsync_CreatesNewDraft()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var existing = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brand.Id,
            Brand = brand,
            TextContent = "Published content",
            Status = ContentStatusEnum.Published
        };
        var repository = new FakeContentRepository(existing);
        var service = CreateService(repository, new FakeBrandRepository(brand));

        var result = await service.CloneAsync(existing.Id, profileId);

        Assert.True(result.Success);
        Assert.NotEqual(existing.Id, result.Data!.Id);
        Assert.Equal(ContentStatusEnum.Draft, result.Data.Status);
        Assert.Equal("Published content", result.Data.TextContent);
    }

    [Fact]
    public async Task RestoreAsync_ResetsStatusToDraft()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brand.Id,
            Brand = brand,
            TextContent = "Deleted content",
            IsDeleted = true,
            Status = ContentStatusEnum.Published
        };
        var repository = new FakeContentRepository(content);
        var service = CreateService(repository, new FakeBrandRepository(brand));

        var result = await service.RestoreAsync(content.Id, profileId);

        Assert.True(result.Success);
        Assert.False(content.IsDeleted);
        Assert.Equal(ContentStatusEnum.Draft, content.Status);
    }

    private static ContentService CreateService(
        IContentRepository contentRepository,
        IBrandRepository brandRepository,
        IProductRepository? productRepository = null,
        IContentCalendarRepository? contentCalendarRepository = null,
        IWorkspaceRepository? workspaceRepository = null)
    {
        return new ContentService(
            contentRepository,
            brandRepository,
            productRepository ?? new FakeProductRepository(),
            new FakeSocialIntegrationRepository(),
            new FakeSocialAccountRepository(),
            new FakePostRepository(),
            Array.Empty<IProviderService>(),
            new FakeSocialTokenProtector(),
            new FakeQuotaService(),
            contentCalendarRepository ?? new FakeContentCalendarRepository(),
            workspaceRepository ?? new FakeWorkspaceRepository());
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
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
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

    private static Brand CreateBrand(Guid profileId)
    {
        return new Brand { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Test brand" };
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        private readonly Dictionary<Guid, Content> _contents;

        public FakeContentRepository(params Content[] contents)
        {
            _contents = contents.ToDictionary(content => content.Id);
        }

        public List<Content> Added { get; } = new();

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
        {
            var data = _contents.Values.Where(content => content.ProfileId == profileId).ToList();
            return Task.FromResult(new PagedResult<Content> { Data = data, TotalCount = data.Count, Page = 1, PageSize = 10 });
        }

        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default)
        {
            Added.Add(content);
            _contents[content.Id] = content;
            return Task.FromResult(content);
        }

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
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly Dictionary<Guid, Brand> _brands;
        public FakeBrandRepository(params Brand[] brands) => _brands = brands.ToDictionary(brand => brand.Id);
        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_brands.GetValueOrDefault(id));
        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _products;
        public FakeProductRepository(params Product[] products) => _products = products.ToDictionary(product => product.Id);
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_products.GetValueOrDefault(id));
        public Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<PagedResult<Product>> GetPagedAsync(PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetProductsByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetProductsByBrandIdIncludingDeletedAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeSocialIntegrationRepository : ISocialIntegrationRepository
    {
        public Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<SocialIntegration?>(null);
        public Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeSocialAccountRepository : ISocialAccountRepository
    {
        public Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<SocialAccount?>(null);
        public Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<SocialAccount?>(null);
        public Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakePostRepository : IPostRepository
    {
        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Post?>(null);
        public Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Post post, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeSocialTokenProtector : ISocialTokenProtector
    {
        public string Protect(string plaintext) => plaintext;
        public string Unprotect(string ciphertext) => ciphertext;
    }

    private sealed class FakeQuotaService : IQuotaService
    {
        public Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto()));

        public Task<GenericResponse<QuotaSummaryDto>> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto()));

        public Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));

        public Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));

        public Task<GenericResponse<bool>> EnsureWorkspacePostQuotaAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
    }
}
