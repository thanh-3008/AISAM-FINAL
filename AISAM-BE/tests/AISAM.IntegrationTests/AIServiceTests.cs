using AISAM.Common.Dtos;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class AIServiceTests
{
    [Fact]
    public async Task GenerateDraftAsync_ReturnsFailedGeneration_WhenGeminiConfigIsMissing()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient(new InvalidOperationException("Gemini API key is not configured.")));

        var result = await service.GenerateDraftAsync(profileId, new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Failed, result.Data!.Status);
        Assert.Equal("Gemini API key is not configured.", result.Data.ErrorMessage);
    }

    [Fact]
    public async Task GenerateDraftAsync_ReturnsCompletedGeneration_WhenGeminiReturnsText()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient("Generated ad copy"));

        var result = await service.GenerateDraftAsync(profileId, new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Completed, result.Data!.Status);
        Assert.Equal("Generated ad copy", result.Data.GeneratedText);
    }

    [Fact]
    public async Task ApproveGenerationAsync_CopiesTextAndKeepsContentDraft()
    {
        var profileId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = Guid.NewGuid(),
            TextContent = "Old text",
            Status = ContentStatusEnum.PendingApproval
        };
        var generation = new AiGeneration
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            GeneratedText = "Approved AI text",
            Status = AiStatusEnum.Completed
        };
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeAiGenerationRepository(generation),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("unused"));

        var result = await service.ApproveAsync(generation.Id, profileId);

        Assert.True(result.Success);
        Assert.Equal("Approved AI text", content.TextContent);
        Assert.Equal(ContentStatusEnum.Draft, content.Status);
    }

    [Fact]
    public async Task GetGenerationsAsync_ReturnsNotFound_ForAnotherProfileContent()
    {
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            TextContent = "Private content"
        };
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("unused"));

        var result = await service.GetGenerationsAsync(content.Id, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task ChatAsync_SavesUserAndAiMessages_WhenGeminiSucceeds()
    {
        var profileId = Guid.NewGuid();
        var conversations = new FakeConversationRepository();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("AI response"),
            conversations);

        var result = await service.ChatAsync(profileId, new ChatRequest { Message = "User message" });

        Assert.True(result.Success);
        Assert.Equal("AI response", result.Data!.Response);
        Assert.Collection(
            conversations.Messages,
            message =>
            {
                Assert.Equal(ChatSenderType.User, message.SenderType);
                Assert.Equal("User message", message.Message);
            },
            message =>
            {
                Assert.Equal(ChatSenderType.AI, message.SenderType);
                Assert.Equal("AI response", message.Message);
            });
    }

    [Fact]
    public async Task ChatAsync_ReturnsClearErrorAndStoresAiErrorMessage_WhenGeminiFails()
    {
        var conversations = new FakeConversationRepository();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient(new InvalidOperationException("Gemini API key is not configured.")),
            conversations);

        var result = await service.ChatAsync(Guid.NewGuid(), new ChatRequest { Message = "User message" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, result.StatusCode);
        Assert.Contains("AI chat is temporarily unavailable.", result.Message);
        Assert.Equal("AI chat is temporarily unavailable.", conversations.Messages.Last().Message);
    }

    [Fact]
    public async Task ChatAsync_ReturnsNotFound_WhenBrandBelongsToAnotherProfile()
    {
        var brand = CreateBrand(Guid.NewGuid());
        var conversations = new FakeConversationRepository();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient("unused"),
            conversations);

        var result = await service.ChatAsync(Guid.NewGuid(), new ChatRequest
        {
            BrandId = brand.Id,
            Message = "User message"
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.Empty(conversations.Messages);
    }

    private static AIService CreateService(
        IContentRepository contentRepository,
        IAiGenerationRepository generationRepository,
        IBrandRepository brandRepository,
        IGeminiTextClient geminiTextClient,
        IConversationRepository? conversationRepository = null,
        IProductRepository? productRepository = null,
        IQuotaService? quotaService = null)
    {
        return new AIService(
            contentRepository,
            generationRepository,
            brandRepository,
            productRepository ?? new FakeProductRepository(),
            geminiTextClient,
            conversationRepository ?? new FakeConversationRepository(),
            quotaService ?? new FakeQuotaService());
    }

    private static Brand CreateBrand(Guid profileId)
    {
        return new Brand { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Test brand" };
    }

    private sealed class FakeGeminiTextClient : IGeminiTextClient
    {
        private readonly string? _response;
        private readonly Exception? _exception;
        public FakeGeminiTextClient(string response) => _response = response;
        public FakeGeminiTextClient(Exception exception) => _exception = exception;
        public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default) =>
            _exception != null ? Task.FromException<string>(_exception) : Task.FromResult(_response!);
    }

    private sealed class FakeAiGenerationRepository : IAiGenerationRepository
    {
        private readonly Dictionary<Guid, AiGeneration> _generations;
        public FakeAiGenerationRepository(params AiGeneration[] generations) => _generations = generations.ToDictionary(generation => generation.Id);
        public Task<AiGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_generations.GetValueOrDefault(id));
        public Task<IEnumerable<AiGeneration>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.FromResult(_generations.Values.Where(generation => generation.ContentId == contentId).AsEnumerable());
        public Task<AiGeneration> AddAsync(AiGeneration generation, CancellationToken cancellationToken = default)
        {
            _generations[generation.Id] = generation;
            return Task.FromResult(generation);
        }
        public Task UpdateAsync(AiGeneration generation, CancellationToken cancellationToken = default)
        {
            _generations[generation.Id] = generation;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        private readonly Dictionary<Guid, Content> _contents;
        public FakeContentRepository(params Content[] contents) => _contents = contents.ToDictionary(content => content.Id);
        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_contents.GetValueOrDefault(id));
        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<PagedResult<Content>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default)
        {
            _contents[content.Id] = content;
            return Task.FromResult(content);
        }
        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default)
        {
            _contents[content.Id] = content;
            return Task.CompletedTask;
        }
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

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Dictionary<Guid, Conversation> _conversations = new();
        public List<ChatMessage> Messages { get; } = new();
        public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_conversations.GetValueOrDefault(id));
        public Task<PagedResult<Conversation>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Conversation?> GetActiveAsync(Guid profileId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default) =>
            Task.FromResult(_conversations.Values.FirstOrDefault(conversation =>
                conversation.ProfileId == profileId &&
                conversation.BrandId == brandId &&
                conversation.ProductId == productId &&
                conversation.AdType == adType &&
                conversation.IsActive));
        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            _conversations[conversation.Id] = conversation;
            return Task.FromResult(conversation);
        }
        public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            _conversations[conversation.Id] = conversation;
            return Task.CompletedTask;
        }
        public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeQuotaService : IQuotaService
    {
        public Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto()));

        public Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));

        public Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
    }
}
