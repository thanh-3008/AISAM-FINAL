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
        var workspaceId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient(new InvalidOperationException("Gemini API key is not configured.")),
            profileRepository: new FakeProfileRepository(new Profile
            {
                Id = profileId,
                UserId = Guid.NewGuid(),
                Name = "Profile",
                ProfileType = ProfileTypeEnum.Basic
            }));

        var result = await service.GenerateDraftAsync(profileId, workspaceId, new CreateDraftRequest
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
        var workspaceId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var creditService = new FakeCreditService();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient("Generated ad copy"),
            creditService: creditService,
            profileRepository: new FakeProfileRepository(new Profile
            {
                Id = profileId,
                UserId = Guid.NewGuid(),
                Name = "Profile",
                ProfileType = ProfileTypeEnum.Basic
            }));

        var result = await service.GenerateDraftAsync(profileId, workspaceId, new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Completed, result.Data!.Status);
        Assert.Equal("Generated ad copy", result.Data.GeneratedText);
        Assert.Single(creditService.ConsumeCalls);
        Assert.Equal(workspaceId, creditService.ConsumeCalls[0].WorkspaceId);
        Assert.Equal(CreditActionEnum.GenerateText, creditService.ConsumeCalls[0].Action);
        Assert.Equal(1, creditService.ConsumeCalls[0].Credits);
        Assert.Equal(result.Data.AiGenerationId, creditService.ConsumeCalls[0].AiGenerationId);
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
    public async Task ImproveAsync_ConsumesCredits_WhenGenerationSucceeds()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = Guid.NewGuid(),
            TextContent = "Draft"
        };
        var generationRepository = new FakeAiGenerationRepository();
        var creditService = new FakeCreditService();
        var service = CreateService(
            new FakeContentRepository(content),
            generationRepository,
            new FakeBrandRepository(),
            new FakeGeminiTextClient("unused"),
            creditService: creditService,
            profileRepository: new FakeProfileRepository(new Profile
            {
                Id = profileId,
                UserId = Guid.NewGuid(),
                Name = "Profile",
                ProfileType = ProfileTypeEnum.Basic
            }));

        var result = await service.ImproveAsync(content.Id, profileId, workspaceId, new ImproveContentRequest { Prompt = "Improve" });

        Assert.True(result.Success);
        Assert.Single(creditService.ConsumeCalls);
        Assert.Equal(CreditActionEnum.RegenerateText, creditService.ConsumeCalls[0].Action);
        Assert.Equal(1, creditService.ConsumeCalls[0].Credits);
        Assert.Single(await generationRepository.GetByContentIdAsync(content.Id));
    }

    [Fact]
    public async Task GenerateDraftAsync_DoesNotConsumeCredits_WhenGeminiFails()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var creditService = new FakeCreditService();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient(new InvalidOperationException("Gemini API key is not configured.")),
            creditService: creditService,
            profileRepository: new FakeProfileRepository(new Profile
            {
                Id = profileId,
                UserId = Guid.NewGuid(),
                Name = "Profile",
                ProfileType = ProfileTypeEnum.Basic
            }));

        var result = await service.GenerateDraftAsync(profileId, workspaceId, new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Failed, result.Data!.Status);
        Assert.Empty(creditService.ConsumeCalls);
    }

    [Fact]
    public async Task GenerateDraftAsync_ReturnsCreditErrorWithoutCallingProvider_WhenWorkspaceHasInsufficientCredits()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var geminiClient = new FakeGeminiTextClient("Generated ad copy");
        var creditService = new FakeCreditService
        {
            AvailabilityResult = GenericResponse<bool>.CreateError(
                "Workspace does not have enough credits.",
                HttpStatusCode.BadRequest,
                "INSUFFICIENT_WORKSPACE_CREDITS")
        };
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            geminiClient,
            creditService: creditService,
            profileRepository: new FakeProfileRepository(new Profile
            {
                Id = profileId,
                UserId = Guid.NewGuid(),
                Name = "Profile",
                ProfileType = ProfileTypeEnum.Basic
            }));

        var result = await service.GenerateDraftAsync(profileId, workspaceId, new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("INSUFFICIENT_WORKSPACE_CREDITS", result.Error?.ErrorCode);
        Assert.Equal(0, geminiClient.CallCount);
        Assert.Empty(creditService.ConsumeCalls);
    }

    [Fact]
    public async Task ChatAsync_SavesUserAndAiMessages_WhenGeminiSucceeds()
    {
        var profileId = Guid.NewGuid();
        var conversations = new FakeConversationRepository();
        var creditService = new FakeCreditService();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("AI response"),
            conversations,
            creditService: creditService);

        var result = await service.ChatAsync(profileId, new ChatRequest { Message = "User message" });

        Assert.True(result.Success);
        Assert.Equal("AI response", result.Data!.Response);
        Assert.Empty(creditService.ConsumeCalls);
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
        ICreditService? creditService = null,
        IProfileRepository? profileRepository = null)
    {
        return new AIService(
            contentRepository,
            generationRepository,
            brandRepository,
            productRepository ?? new FakeProductRepository(),
            geminiTextClient,
            conversationRepository ?? new FakeConversationRepository(),
            profileRepository ?? new FakeProfileRepository(),
            creditService ?? new FakeCreditService());
    }

    private static Brand CreateBrand(Guid profileId)
    {
        return new Brand { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Test brand" };
    }

    private sealed class FakeGeminiTextClient : IGeminiTextClient
    {
        private readonly string? _response;
        private readonly Exception? _exception;
        public int CallCount { get; private set; }
        public FakeGeminiTextClient(string response) => _response = response;
        public FakeGeminiTextClient(Exception exception) => _exception = exception;
        public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return _exception != null ? Task.FromException<string>(_exception) : Task.FromResult(_response!);
        }
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
        public Task<PagedResult<Content>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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
        public Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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
        public Task<PagedResult<Product>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly Dictionary<Guid, Profile> _profiles;

        public FakeProfileRepository(params Profile[] profiles)
        {
            _profiles = profiles.ToDictionary(profile => profile.Id);
        }

        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_profiles.GetValueOrDefault(id));

        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeCreditService : ICreditService
    {
        public List<(Guid WorkspaceId, Guid UserId, CreditActionEnum Action, long Credits, Guid? AiGenerationId)> ConsumeCalls { get; } = new();
        public GenericResponse<bool> AvailabilityResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<CreditUsageRecord> ConsumeResult { get; set; } = GenericResponse<CreditUsageRecord>.CreateSuccess(new CreditUsageRecord());

        public Task<GenericResponse<bool>> CanConsumeCreditsAsync(Guid workspaceId, Guid userId, long credits, DateTime? now = null, CancellationToken cancellationToken = default)
            => Task.FromResult(AvailabilityResult);

        public Task<GenericResponse<CreditUsageRecord>> ConsumeCreditsAsync(Guid workspaceId, Guid userId, CreditActionEnum action, long credits, Guid? aiGenerationId = null, DateTime? now = null, CancellationToken cancellationToken = default)
        {
            ConsumeCalls.Add((workspaceId, userId, action, credits, aiGenerationId));
            return Task.FromResult(ConsumeResult);
        }

        public Task<CreditWallet> EnsureWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<CreditWallet>> GrantSubscriptionCreditsAsync(Guid workspaceId, Guid userId, WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<CreditWallet>> GrantCreditPackCreditsAsync(Guid workspaceId, Guid userId, WorkspaceTypeEnum workspaceType, long credits, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<CreditUsageRecord>> RecordUsageAsync(Guid workspaceId, Guid userId, CreditActionEnum action, long credits, CreditUsageStatusEnum status, Guid? aiGenerationId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
