using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class PhaseEQuotaIntegrationTests
{
    [Fact]
    public async Task GenerateDraftAsync_ReturnsForbiddenWithPromptQuotaError_WhenPromptQuotaExceeded()
    {
        var profileId = Guid.NewGuid();
        var brand = new Brand { Id = Guid.NewGuid(), ProfileId = profileId, WorkspaceId = profileId, Name = "Brand" };
        var creditService = new FakeCreditService
        {
            AvailabilityResult = GenericResponse<bool>.CreateError(
                "Workspace does not have enough credits.",
                HttpStatusCode.BadRequest,
                "INSUFFICIENT_WORKSPACE_CREDITS")
        };
        var contentRepository = new FakeContentRepository();
        var service = CreateAiService(contentRepository, creditService, new FakeGeminiTextClient("unused"), brand);

        var result = await service.GenerateDraftAsync(profileId, profileId, Guid.NewGuid(), new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("INSUFFICIENT_WORKSPACE_CREDITS", result.Error?.ErrorCode);
        Assert.Empty(contentRepository.StoredContents);
    }

    [Fact]
    public async Task GenerateDraftAsync_DoesNotIncreaseUsage_WhenGeminiFails()
    {
        var profileId = Guid.NewGuid();
        var brand = new Brand { Id = Guid.NewGuid(), ProfileId = profileId, WorkspaceId = profileId, Name = "Brand" };
        var creditService = new FakeCreditService();
        var generationRepository = new FakeAiGenerationRepository();
        var service = CreateAiService(
            new FakeContentRepository(),
            creditService,
            new FakeGeminiTextClient(new InvalidOperationException("Gemini API key is not configured.")),
            brand,
            generationRepository);

        var result = await service.GenerateDraftAsync(profileId, profileId, Guid.NewGuid(), new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Failed, result.Data!.Status);
        Assert.Equal(1, creditService.AvailabilityCallCount);
        Assert.Equal(0, creditService.ConsumeCallCount);
        Assert.Contains(generationRepository.StoredGenerations.Values, generation => generation.Status == AiStatusEnum.Failed);
        Assert.DoesNotContain(generationRepository.StoredGenerations.Values, generation => generation.Status == AiStatusEnum.Completed);
    }

    private static AIService CreateAiService(
        FakeContentRepository contentRepository,
        FakeCreditService creditService,
        FakeGeminiTextClient geminiTextClient,
        Brand brand,
        FakeAiGenerationRepository? generationRepository = null)
    {
        return new AIService(
            contentRepository,
            generationRepository ?? new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeProductRepository(),
            geminiTextClient,
            new FakeConversationRepository(),
            creditService,
            null!,
            null!,
            null!,
            new FakePromptEnhancerService(),
            null!);
    }

    private sealed class FakePromptEnhancerService : AISAM.Services.IServices.IPromptEnhancerService
    {
        public Task<string> EnhanceImagePromptAsync(string rawPrompt, AISAM.Data.Model.Product? product, bool hasReferenceImages, CancellationToken cancellationToken = default)
            => Task.FromResult(rawPrompt);

        public Task<string> EnhanceVideoPromptAsync(string rawPrompt, AISAM.Data.Model.Product? product, int durationSeconds = 8, string? aspectRatio = null, CancellationToken cancellationToken = default)
            => Task.FromResult(rawPrompt);

        public Task<string> EnhanceVideoPromptWithScriptAsync(string rawPrompt, string? videoScript, AISAM.Data.Model.Product? product, int durationSeconds = 9, string? aspectRatio = "9:16", CancellationToken cancellationToken = default)
            => Task.FromResult(!string.IsNullOrWhiteSpace(rawPrompt) ? rawPrompt : videoScript ?? string.Empty);
    }

    private sealed class FakeCreditService : ICreditService
    {
        public int AvailabilityCallCount { get; private set; }
        public int ConsumeCallCount { get; private set; }
        public GenericResponse<bool> AvailabilityResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public Task<CreditWallet> EnsureWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<CreditWallet>> GrantSubscriptionCreditsAsync(Guid workspaceId, Guid userId, WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<CreditWallet>> GrantCreditPackCreditsAsync(Guid workspaceId, Guid userId, WorkspaceTypeEnum workspaceType, long credits, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> EnsureCreditsAvailableAsync(Guid workspaceId, Guid userId, long credits, DateTime? now = null, CancellationToken cancellationToken = default)
        {
            AvailabilityCallCount++;
            return Task.FromResult(AvailabilityResult);
        }
        public Task<GenericResponse<CreditUsageRecord>> ConsumeCreditsAsync(Guid workspaceId, Guid userId, CreditActionEnum action, long credits, Guid? aiGenerationId = null, DateTime? now = null, CancellationToken cancellationToken = default)
        {
            ConsumeCallCount++;
            return Task.FromResult(GenericResponse<CreditUsageRecord>.CreateSuccess(new CreditUsageRecord()));
        }
        public Task<GenericResponse<CreditUsageRecord>> RecordUsageAsync(Guid workspaceId, Guid userId, CreditActionEnum action, long credits, CreditUsageStatusEnum status, Guid? aiGenerationId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<CreditWallet>> AdminAdjustCreditsAsync(Guid workspaceId, Guid adminUserId, long amount, string reason, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<CreditWallet?> GetWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<DailyCreditUsageDto>> GetDailyUsageAsync(Guid workspaceId, int days, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<CreditUsageRecordDto>> GetPagedUsageAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeGeminiTextClient : IGeminiTextClient
    {
        private readonly string? _response;
        private readonly Exception? _exception;

        public FakeGeminiTextClient(string response)
        {
            _response = response;
        }

        public FakeGeminiTextClient(Exception exception)
        {
            _exception = exception;
        }

        public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
            => _exception != null ? Task.FromException<string>(_exception) : Task.FromResult(_response!);

        public Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
            => GenerateAsync(textPrompt, cancellationToken);
    }

    private sealed class FakeAiGenerationRepository : IAiGenerationRepository
    {
        public Dictionary<Guid, AiGeneration> StoredGenerations { get; } = new();

        public Task<AiGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(StoredGenerations.GetValueOrDefault(id));

        public Task<IEnumerable<AiGeneration>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default)
            => Task.FromResult(StoredGenerations.Values.Where(generation => generation.ContentId == contentId).AsEnumerable());

        public Task<AiGeneration> AddAsync(AiGeneration generation, CancellationToken cancellationToken = default)
        {
            StoredGenerations[generation.Id] = generation;
            return Task.FromResult(generation);
        }

        public Task<List<dynamic>> GetTopWorkspacesByGenerationAsync(int limit, CancellationToken cancellationToken = default) => Task.FromResult(new List<dynamic>());

        public Task UpdateAsync(AiGeneration generation, CancellationToken cancellationToken = default)
        {
            StoredGenerations[generation.Id] = generation;
            return Task.CompletedTask;
        }
        public Task<Dictionary<DateTime, int>> GetDailyGenerationCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
        public Task<int> GetTotalGenerationCountAsync(CancellationToken cancellationToken = default) => Task.FromResult(StoredGenerations.Count);
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        public Dictionary<Guid, Content> StoredContents { get; } = new();

        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(StoredContents.GetValueOrDefault(id));

        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<PagedResult<Content>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default)
        {
            StoredContents[content.Id] = content;
            return Task.FromResult(content);
        }

        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default)
        {
            StoredContents[content.Id] = content;
            return Task.CompletedTask;
        }

        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Content>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly Dictionary<Guid, Brand> _brands;

        public FakeBrandRepository(params Brand[] brands)
        {
            _brands = brands.ToDictionary(brand => brand.Id);
        }

        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_brands.GetValueOrDefault(id));

        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<bool> ExistsByNameInWorkspaceAsync(Guid workspaceId, string name, CancellationToken cancellationToken = default)
            => Task.FromResult(_brands.Values.Any(b => b.WorkspaceId == workspaceId && b.Name == name));

        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Product?>(null);

        public Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Product?>(null);

        public Task<PagedResult<Product>> GetPagedAsync(PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<Product>> GetProductsByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<Product>> GetProductsByBrandIdIncludingDeletedAsync(Guid brandId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Conversation?>(null);

        public Task<PagedResult<Conversation>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Conversation?> GetActiveAsync(Guid profileId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default)
            => Task.FromResult<Conversation?>(null);

        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
            => Task.FromResult(conversation);

        public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
