using AISAM.Common.Dtos.Response;
using AISAM.Common.Dtos;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISAM.IntegrationTests;

public class AIServiceTests
{
    [Fact]
    public async Task StartVideoGenerationAsync_ReturnsExistingProcessingGeneration_WithoutCreatingDuplicateProviderJob()
    {
        var workspaceId = Guid.NewGuid();
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = Guid.NewGuid(), Title = "Video" };
        var existing = new AiGeneration
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            Status = AiStatusEnum.Processing,
            VideoJobId = "deapi:existing-job",
            ProviderName = "DeAPI"
        };
        var provider = new FakeVideoProvider();
        var service = CreateService(
            new FakeContentRepository(content),
            new FakeAiGenerationRepository(existing),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("unused"),
            videoProvider: provider);

        var result = await service.StartVideoGenerationAsync(workspaceId, Guid.NewGuid(), new GenerateVideoRequest
        {
            ContentId = content.Id,
            DurationSeconds = 4,
            AspectRatio = "9:16"
        });

        Assert.True(result.Success);
        Assert.Equal(existing.Id, result.Data!.AiGenerationId);
        Assert.Equal("deapi:existing-job", result.Data.VideoJobId);
        Assert.Equal(0, provider.StartCallCount);
    }

    [Fact]
    public async Task CheckVideoStatusAsync_MarksExplicitProviderFailureFailed_AndStopsPolling()
    {
        var workspaceId = Guid.NewGuid();
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = Guid.NewGuid() };
        var generation = new AiGeneration
        {
            Id = Guid.NewGuid(), ContentId = content.Id, Content = content,
            Status = AiStatusEnum.Processing, VideoJobId = "deapi:missing-job", CreatedAt = DateTime.UtcNow
        };
        var provider = new FakeVideoProvider
        {
            PollResult = VideoGenerationResult.Fail("GPU worker failed.", "DeAPI")
        };
        var service = CreateService(
            new FakeContentRepository(content), new FakeAiGenerationRepository(generation),
            new FakeBrandRepository(), new FakeGeminiTextClient("unused"), videoProvider: provider);

        var result = await service.CheckVideoStatusAsync(generation.Id, workspaceId, Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Failed, result.Data!.Status);
        Assert.Equal("Generation failed.", result.Data.ErrorMessage);
        Assert.Equal(1, provider.CheckCallCount);
    }

    [Fact]
    public async Task CheckVideoStatusAsync_ProcessingResultPreservesGenerationAndJobId()
    {
        var workspaceId = Guid.NewGuid();
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = Guid.NewGuid() };
        var generation = new AiGeneration
        {
            Id = Guid.NewGuid(), ContentId = content.Id, Content = content,
            Status = AiStatusEnum.Processing, VideoJobId = "deapi:same-job", CreatedAt = DateTime.UtcNow
        };
        var provider = new FakeVideoProvider
        {
            PollResult = VideoGenerationResult.InProgress("deapi:same-job", "DeAPI")
        };
        var service = CreateService(
            new FakeContentRepository(content), new FakeAiGenerationRepository(generation),
            new FakeBrandRepository(), new FakeGeminiTextClient("unused"), videoProvider: provider);

        var result = await service.CheckVideoStatusAsync(generation.Id, workspaceId, Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Processing, result.Data!.Status);
        Assert.Equal("deapi:same-job", result.Data.VideoJobId);
        Assert.Null(result.Data.ErrorMessage);
    }

    [Fact]
    public async Task CheckVideoStatusAsync_CompletedResult_SavesUploadedVideoUrl()
    {
        await using var server = new OneShotHttpServer([1, 2, 3, 4]);
        var workspaceId = Guid.NewGuid();
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = Guid.NewGuid() };
        var generation = new AiGeneration
        {
            Id = Guid.NewGuid(), ContentId = content.Id, Content = content,
            Status = AiStatusEnum.Processing, VideoJobId = "deapi:completed-job", CreatedAt = DateTime.UtcNow
        };
        var mediaStorage = new FakeMediaStorageService("https://cdn.example/video.mp4");
        var provider = new FakeVideoProvider
        {
            PollResult = VideoGenerationResult.Done(server.Url, "DeAPI")
        };
        var service = CreateService(
            new FakeContentRepository(content), new FakeAiGenerationRepository(generation),
            new FakeBrandRepository(), new FakeGeminiTextClient("unused"),
            videoProvider: provider, mediaStorage: mediaStorage);

        var result = await service.CheckVideoStatusAsync(generation.Id, workspaceId, Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Completed, result.Data!.Status);
        Assert.Equal("https://cdn.example/video.mp4", result.Data.GeneratedVideoUrl);
        Assert.Equal("https://cdn.example/video.mp4", content.VideoUrl);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, mediaStorage.UploadedBytes);
        Assert.Equal("deapi:completed-job", result.Data.VideoJobId);
    }

    [Fact]
    public async Task CheckVideoStatusAsync_ConcurrentCalls_DoNotApplyConflictingTerminalTransitions()
    {
        var workspaceId = Guid.NewGuid();
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = Guid.NewGuid() };
        var generation = new AiGeneration
        {
            Id = Guid.NewGuid(), ContentId = content.Id, Content = content,
            Status = AiStatusEnum.Processing, VideoJobId = "deapi:concurrent-job", CreatedAt = DateTime.UtcNow
        };
        var provider = new FakeVideoProvider
        {
            PollResult = VideoGenerationResult.Fail("Explicit provider failure.", "DeAPI"),
            CheckDelay = TimeSpan.FromMilliseconds(50)
        };
        var service = CreateService(
            new FakeContentRepository(content), new FakeAiGenerationRepository(generation),
            new FakeBrandRepository(), new FakeGeminiTextClient("unused"), videoProvider: provider);

        await Task.WhenAll(
            service.CheckVideoStatusAsync(generation.Id, workspaceId, Guid.NewGuid()),
            service.CheckVideoStatusAsync(generation.Id, workspaceId, Guid.NewGuid()));

        Assert.Equal(AiStatusEnum.Failed, generation.Status);
        Assert.Equal("deapi:concurrent-job", generation.VideoJobId);
        Assert.Equal(1, provider.CheckCallCount);
    }

    [Fact]
    public async Task CheckVideoStatusAsync_TimesOutStaleJob_WithoutCallingProvider()
    {
        var workspaceId = Guid.NewGuid();
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = Guid.NewGuid() };
        var generation = new AiGeneration
        {
            Id = Guid.NewGuid(), ContentId = content.Id, Content = content,
            Status = AiStatusEnum.Processing, VideoJobId = "deapi:stale-job", CreatedAt = DateTime.UtcNow.AddMinutes(-31)
        };
        var provider = new FakeVideoProvider();
        var service = CreateService(
            new FakeContentRepository(content), new FakeAiGenerationRepository(generation),
            new FakeBrandRepository(), new FakeGeminiTextClient("unused"), videoProvider: provider);

        var result = await service.CheckVideoStatusAsync(generation.Id, workspaceId, Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Failed, result.Data!.Status);
        Assert.Equal("Generation failed.", result.Data.ErrorMessage);
        Assert.Equal(0, provider.CheckCallCount);
    }

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

        var result = await service.GenerateDraftAsync(profileId, profileId, Guid.NewGuid(), new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Failed, result.Data!.Status);
        Assert.Equal("Generation failed.", result.Data.ErrorMessage);
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

        var result = await service.GenerateDraftAsync(profileId, profileId, Guid.NewGuid(), new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(AiStatusEnum.Completed, result.Data!.Status);
        Assert.Equal("Generated ad copy", result.Data.GeneratedText);
    }

    [Fact]
    public async Task GenerateDraftAsync_ConsumesOneWorkspaceCredit_AfterGeminiSucceeds()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        brand.WorkspaceId = workspaceId;
        var credits = new FakeCreditService();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient("Generated ad copy"),
            creditService: credits);

        var result = await service.GenerateDraftAsync(profileId, workspaceId, userId, new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.True(result.Success);
        Assert.Equal(1, credits.ConsumeCallCount);
        Assert.Equal(workspaceId, credits.LastWorkspaceId);
        Assert.Equal(userId, credits.LastUserId);
        Assert.Equal(CreditActionEnum.GenerateText, credits.LastAction);
        Assert.Equal(1, credits.LastCredits);
    }

    [Fact]
    public async Task GenerateDraftAsync_HidesGeneratedText_WhenFinalCreditChargeFails()
    {
        var profileId = Guid.NewGuid();
        var brand = CreateBrand(profileId);
        var generations = new FakeAiGenerationRepository();
        var credits = new FakeCreditService
        {
            ConsumeResult = GenericResponse<CreditUsageRecord>.CreateError(
                "Workspace does not have enough credits.",
                HttpStatusCode.BadRequest,
                "INSUFFICIENT_WORKSPACE_CREDITS")
        };
        var service = CreateService(
            new FakeContentRepository(),
            generations,
            new FakeBrandRepository(brand),
            new FakeGeminiTextClient("Generated ad copy"),
            creditService: credits);

        var result = await service.GenerateDraftAsync(profileId, profileId, Guid.NewGuid(), new CreateDraftRequest
        {
            BrandId = brand.Id,
            Prompt = "Create an ad"
        });

        Assert.False(result.Success);
        var stored = Assert.Single(await generations.GetByContentIdAsync(generations.StoredContentId));
        Assert.Equal(AiStatusEnum.Failed, stored.Status);
        Assert.Null(stored.GeneratedText);
    }

    [Fact]
    public async Task ApproveGenerationAsync_CopiesTextAndSetsPendingApproval()
    {
        var profileId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = profileId,
            BrandId = Guid.NewGuid(),
            TextContent = "Old text",
            Status = ContentStatusEnum.Draft
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
    public async Task ImproveAsync_ReturnsForbidden_WhenPromptQuotaIsExceeded()
    {
        var profileId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = profileId,
            BrandId = Guid.NewGuid(),
            TextContent = "Draft"
        };
        var generationRepository = new FakeAiGenerationRepository();
        var service = CreateService(
            new FakeContentRepository(content),
            generationRepository,
            new FakeBrandRepository(),
            new FakeGeminiTextClient("unused"),
            creditService: new FakeCreditService
            {
                AvailabilityResult = GenericResponse<bool>.CreateError(
                    "Workspace does not have enough credits.",
                    HttpStatusCode.BadRequest,
                    "INSUFFICIENT_WORKSPACE_CREDITS")
            });

        var result = await service.ImproveAsync(content.Id, profileId, profileId, Guid.NewGuid(), new ImproveContentRequest { Prompt = "Improve" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("INSUFFICIENT_WORKSPACE_CREDITS", result.Error?.ErrorCode);
        Assert.Empty(await generationRepository.GetByContentIdAsync(content.Id));
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

        var result = await service.ChatInWorkspaceAsync(profileId, profileId, Guid.NewGuid(), new ChatRequest { Message = "User message" });

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
    public async Task ChatAsync_DoesNotCreateContent_ForConversationalResponse()
    {
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("{\"intent\":\"chat\",\"response\":\"Chao ban, minh co the noi tieng Viet.\"}"));

        var result = await service.ChatInWorkspaceAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new ChatRequest { Message = "Xin chao, noi tieng Viet duoc khong?" });

        Assert.True(result.Success);
        Assert.False(result.Data!.ShouldCreateContent);
        Assert.Equal("Chao ban, minh co the noi tieng Viet.", result.Data.Response);
    }

    [Fact]
    public async Task ChatAsync_MarksReadyPostAsContent_ForGenerationResponse()
    {
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("{\"intent\":\"content\",\"response\":\"Bai quang cao san sang dang.\"}"));

        var result = await service.ChatInWorkspaceAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new ChatRequest { Message = "Viet cho toi mot bai quang cao." });

        Assert.True(result.Success);
        Assert.True(result.Data!.ShouldCreateContent);
        Assert.Equal("Bai quang cao san sang dang.", result.Data.Response);
    }

    [Fact]
    public void ExtractGeneratedTitle_UsesSafeFallback_NotBrandName_WhenAiOutputHasNoTitle()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var title = (string)method!.Invoke(null, new object?[] { "   " })!;

        Assert.Equal("Untitled Post", title);
        Assert.NotEqual("Apple Store Vietnam", title);
    }

    // ── Regression Tests: Caption-first-sentence as title fallback ──────────

    [Fact]
    public void ExtractGeneratedTitle_UsesUppercaseCaptionHeadline_InsteadOfAiTitle()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var aiResponse = "Title: New product introduction\n\nCaption: 🔥 TẠO DẤU ẤN ĐƯỜNG PHỐ CỰC CHẤT\nCÙNG NIKE AIR MAX 95 BIG BUBBLE!\n\nCác sneakerhead đã sẵn sàng.";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        Assert.Equal("🔥 TẠO DẤU ẤN ĐƯỜNG PHỐ CỰC CHẤT CÙNG NIKE AIR MAX 95 BIG BUBBLE!", title);
    }

    [Fact]
    public void ExtractGeneratedTitle_UsesCaptionFirstSentence_InsteadOfExplicitAiTitle_WhenNoUppercaseHeadline()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var aiResponse = "Title: New product introduction\n\nCaption: Câu đầu tiên của caption. Câu thứ hai.";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        Assert.Equal("Câu đầu tiên của caption.", title);
    }

    [Fact]
    public void ExtractGeneratedTitle_UsesCaptionFirstSentence_WhenNoAiTitle()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var aiResponse = "Khám phá sức mạnh camera iPhone 16 Pro. Ghi lại mọi khoảnh khắc.";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        // Must return the first meaningful sentence, not Product Name or Brand Name
        Assert.Equal("Khám phá sức mạnh camera iPhone 16 Pro.", title);
    }

    [Fact]
    public void ExtractGeneratedTitle_UsesFirstSentence_WhenAiTitleIsWhitespace()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var aiResponse = "Title:    \n\nTận hưởng hành trình xanh cùng VinFast VF 3! Di chuyển thông minh hơn mỗi ngày.";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        Assert.Equal("Tận hưởng hành trình xanh cùng VinFast VF 3!", title);
    }

    [Fact]
    public void ExtractGeneratedTitle_SkipsLeadingEmptyLines_InCaption()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var aiResponse = "\n\nKhám phá Coca-Cola Zero ngay hôm nay. Hương vị sảng khoái.";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        Assert.Equal("Khám phá Coca-Cola Zero ngay hôm nay.", title);
    }

    [Fact]
    public void ExtractGeneratedTitle_HandlesTextWithoutPunctuation()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var aiResponse = "Khám phá trải nghiệm mới cùng sản phẩm của chúng tôi";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        Assert.Equal("Khám phá trải nghiệm mới cùng sản phẩm của chúng tôi", title);
    }

    [Fact]
    public void ExtractGeneratedTitle_ReturnsUntitledPost_WhenNeitherTitleNorCaptionExist()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var title = (string)method!.Invoke(null, new object?[] { "   " })!;

        Assert.Equal("Untitled Post", title);
    }

    [Fact]
    public void ExtractGeneratedTitle_DoesNotUseBrandOrProductName_AsContentTitleFallback()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var aiResponse = "Khám phá camera chuyên nghiệp với khả năng chụp ảnh ấn tượng. Lưu giữ mọi khoảnh khắc.";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        Assert.Equal("Khám phá camera chuyên nghiệp với khả năng chụp ảnh ấn tượng.", title);
        Assert.NotEqual("Apple Store Vietnam", title); // Brand Name must NOT be fallback
        Assert.NotEqual("iPhone 16 Pro", title);       // Product Name must NOT be fallback
    }

    [Fact]
    public void ExtractGeneratedTitle_ProductNameNotUsedAsFallback_WhenOnlyWhitespaceInput()
    {
        var method = typeof(AIService).GetMethod("ExtractGeneratedTitle", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        // Even when Product Name is contextually known, helper does NOT accept it
        var aiResponse = "   ";
        var title = (string)method!.Invoke(null, new object?[] { aiResponse })!;

        Assert.Equal("Untitled Post", title);
        Assert.NotEqual("iPhone 16 Pro", title); // Product Name is NOT a fallback
    }

    [Fact]
    public async Task ChatAsync_IncludesSelectedBrandAndProductDetailsInPrompt()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            Name = "Ao Nha",
            Description = "Thoi trang gia dinh",
            Slogan = "Mac dep moi ngay",
            Usp = "Vai mem ben",
            TargetAudience = "Gia dinh co con nho"
        };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BrandId = brand.Id,
            Brand = brand,
            Name = "Ao tre em",
            Description = "Cotton mem",
            Price = 199000,
            Stock = 25
        };
        var gemini = new FakeGeminiTextClient("{\"intent\":\"chat\",\"response\":\"Da hieu san pham.\"}");
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(brand),
            gemini,
            productRepository: new FakeProductRepository(product));

        var result = await service.ChatInWorkspaceAsync(profileId, workspaceId, Guid.NewGuid(), new ChatRequest
        {
            BrandId = brand.Id,
            ProductId = product.Id,
            Message = "Hay xem thong tin san pham hien tai."
        });

        Assert.True(result.Success);
        Assert.Contains("Ao Nha", gemini.LastPrompt);
        Assert.Contains("Thoi trang gia dinh", gemini.LastPrompt);
        Assert.Contains("Vai mem ben", gemini.LastPrompt);
        Assert.Contains("Gia dinh co con nho", gemini.LastPrompt);
        Assert.Contains("Ao tre em", gemini.LastPrompt);
        Assert.Contains("Cotton mem", gemini.LastPrompt);
        Assert.Contains("199000", gemini.LastPrompt);
        Assert.Contains("25", gemini.LastPrompt);
    }

    [Fact]
    public async Task ChatAsync_ConsumesOneWorkspaceCredit_AfterGeminiSucceeds()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var credits = new FakeCreditService();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("AI response"),
            creditService: credits);

        var result = await service.ChatInWorkspaceAsync(Guid.NewGuid(), workspaceId, userId, new ChatRequest { Message = "User message" });

        Assert.True(result.Success);
        Assert.Equal(1, credits.ConsumeCallCount);
        Assert.Equal(workspaceId, credits.LastWorkspaceId);
        Assert.Equal(userId, credits.LastUserId);
        Assert.Equal(CreditActionEnum.GenerateText, credits.LastAction);
        Assert.Equal(1, credits.LastCredits);
    }

    [Fact]
    public async Task ChatAsync_ReturnsCreditError_WhenCreditDeductionFails()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var credits = new FakeCreditService
        {
            ConsumeResult = GenericResponse<CreditUsageRecord>.CreateError(
                "Workspace does not have enough credits.",
                HttpStatusCode.BadRequest,
                "INSUFFICIENT_WORKSPACE_CREDITS")
        };
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient("AI response"),
            creditService: credits);

        var result = await service.ChatInWorkspaceAsync(Guid.NewGuid(), workspaceId, userId, new ChatRequest { Message = "User message" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("INSUFFICIENT_WORKSPACE_CREDITS", result.Error?.ErrorCode);
        Assert.Equal(1, credits.ConsumeCallCount);
    }

    [Fact]
    public async Task ChatAsync_ReturnsClearErrorAndStoresAiErrorMessage_WhenGeminiFails()
    {
        var conversations = new FakeConversationRepository();
        var credits = new FakeCreditService();
        var service = CreateService(
            new FakeContentRepository(),
            new FakeAiGenerationRepository(),
            new FakeBrandRepository(),
            new FakeGeminiTextClient(new InvalidOperationException("Gemini API key is not configured.")),
            conversations,
            creditService: credits);

        var result = await service.ChatInWorkspaceAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new ChatRequest { Message = "User message" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, result.StatusCode);
        Assert.Equal("Hệ thống AI đang bận. Vui lòng thử lại sau.", result.Message);
        Assert.Equal("(Hệ thống AI đang bảo trì hoặc quá tải, không thể phản hồi lúc này. Vui lòng thử lại sau.)", conversations.Messages.Last().Message);
        Assert.Equal(0, credits.ConsumeCallCount);
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
        IPromptEnhancerService? promptEnhancer = null,
        IAIVideoProvider? videoProvider = null,
        IMediaStorageService? mediaStorage = null)
    {
        return new AIService(
            contentRepository,
            generationRepository,
            brandRepository,
            productRepository ?? new FakeProductRepository(),
            geminiTextClient,
            conversationRepository ?? new FakeConversationRepository(),
            creditService ?? new FakeCreditService(),
            null!,
            videoProvider ?? new FakeVideoProvider(),
            mediaStorage!,
            promptEnhancer ?? new FakePromptEnhancerService(),
            NullLogger<AIService>.Instance);
    }

    private static Brand CreateBrand(Guid profileId)
    {
        return new Brand { Id = Guid.NewGuid(), ProfileId = profileId, WorkspaceId = profileId, Name = "Test brand" };
    }

    private sealed class FakeGeminiTextClient : IGeminiTextClient
    {
        private readonly string? _response;
        private readonly Exception? _exception;
        public string LastPrompt { get; private set; } = string.Empty;
        public FakeGeminiTextClient(string response) => _response = response;
        public FakeGeminiTextClient(Exception exception) => _exception = exception;
        public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            LastPrompt = prompt;
            return _exception != null ? Task.FromException<string>(_exception) : Task.FromResult(_response!);
        }
        public Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
            => GenerateAsync(textPrompt, cancellationToken);
    }

    private sealed class FakeAiGenerationRepository : IAiGenerationRepository
    {
        public Dictionary<Guid, AiGeneration> StoredGenerations { get; } = new();

        public Guid StoredContentId => StoredGenerations.Values.FirstOrDefault()?.ContentId ?? Guid.Empty;

        public FakeAiGenerationRepository(params AiGeneration[] generations)
        {
            foreach (var g in generations)
            {
                StoredGenerations[g.Id] = g;
            }
        }

        public Task<AiGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(StoredGenerations.GetValueOrDefault(id));

        public Task<AiGeneration?> GetActiveVideoByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default)
            => Task.FromResult(StoredGenerations.Values
                .Where(g => g.ContentId == contentId && g.Status == AiStatusEnum.Processing && !string.IsNullOrWhiteSpace(g.VideoJobId))
                .OrderByDescending(g => g.CreatedAt)
                .FirstOrDefault());

        public Task<IEnumerable<AiGenerationListDto>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<AiGenerationListDto>>(StoredGenerations.Values.Where(g => g.ContentId == contentId).Select(g => new AiGenerationListDto { Id = g.Id, ContentId = g.ContentId, GeneratedText = g.GeneratedText, Status = g.Status, ErrorMessage = g.ErrorMessage }).ToList());

        public Task<List<string>> GetRecentVideoPatternIdsByProductAsync(Guid productId, int limit = 3, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<string>());

        public Task<AiGeneration> AddAsync(AiGeneration generation, CancellationToken cancellationToken = default)
        {
            StoredGenerations[generation.Id] = generation;
            return Task.FromResult(generation);
        }
        public Task UpdateAsync(AiGeneration generation, CancellationToken cancellationToken = default)
        {
            StoredGenerations[generation.Id] = generation;
            return Task.CompletedTask;
        }
        public Task<Dictionary<DateTime, int>> GetDailyGenerationCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
        public Task<int> GetTotalGenerationCountAsync(CancellationToken cancellationToken = default) => Task.FromResult(StoredGenerations.Count);
        public Task<List<dynamic>> GetTopWorkspacesByGenerationAsync(int limit, CancellationToken cancellationToken = default) => Task.FromResult(new List<dynamic>());
    }

    private sealed class FakeVideoProvider : IAIVideoProvider
    {
        public string ProviderName => "Fake";
        public int StartCallCount { get; private set; }
        private int _checkCallCount;
        public int CheckCallCount => _checkCallCount;
        public TimeSpan CheckDelay { get; set; }
        public VideoGenerationResult PollResult { get; set; } = VideoGenerationResult.InProgress("deapi:processing", "Fake");

        public Task<VideoGenerationResult> StartVideoGenerationAsync(string prompt, VideoGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            StartCallCount++;
            return Task.FromResult(VideoGenerationResult.Queued("deapi:new-job", ProviderName));
        }

        public async Task<VideoGenerationResult> CheckStatusAsync(string jobId, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _checkCallCount);
            if (CheckDelay > TimeSpan.Zero)
                await Task.Delay(CheckDelay, cancellationToken);
            return PollResult;
        }
    }

    private sealed class FakeMediaStorageService(string uploadedUrl) : IMediaStorageService
    {
        public byte[]? UploadedBytes { get; private set; }

        public Task<string> UploadAsync(Microsoft.AspNetCore.Http.IFormFile file, string folder, string fileName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string> UploadBytesAsync(byte[] data, string folder, string fileName, CancellationToken cancellationToken = default)
        {
            UploadedBytes = data;
            return Task.FromResult(uploadedUrl);
        }
    }

    private sealed class OneShotHttpServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;
        private readonly Task _serverTask;

        public OneShotHttpServer(byte[] body)
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            Url = $"http://127.0.0.1:{port}/video.mp4";
            _serverTask = ServeAsync(body);
        }

        public string Url { get; }

        private async Task ServeAsync(byte[] body)
        {
            using var client = await _listener.AcceptTcpClientAsync();
            await using var stream = client.GetStream();
            var requestBuffer = new byte[4096];
            await stream.ReadAsync(requestBuffer);
            var header = System.Text.Encoding.ASCII.GetBytes(
                $"HTTP/1.1 200 OK\r\nContent-Type: video/mp4\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(header);
            await stream.WriteAsync(body);
        }

        public async ValueTask DisposeAsync()
        {
            _listener.Stop();
            await _serverTask;
        }
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        public Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        private readonly Dictionary<Guid, Content> _contents;
        public FakeContentRepository(params Content[] contents) => _contents = contents.ToDictionary(content => content.Id);
        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_contents.GetValueOrDefault(id));
        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<PagedResult<ContentListDto>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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
        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<ContentListDto>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
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
        public Task<bool> ExistsByNameInWorkspaceAsync(Guid workspaceId, string name, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<Brand>> GetByNamesAndIdsAsync(Guid workspaceId, IEnumerable<string> names, IEnumerable<Guid> ids, CancellationToken cancellationToken = default) => Task.FromResult(new List<Brand>());
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<Product?> GetBasicByIdAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
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
        public Task<Conversation?> GetActiveByWorkspaceIdAsync(Guid workspaceId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default) =>
            Task.FromResult(_conversations.Values.FirstOrDefault(conversation =>
                conversation.WorkspaceId == workspaceId &&
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

    private sealed class FakeCreditService : ICreditService
    {
        public GenericResponse<bool> AvailabilityResult { get; set; } = GenericResponse<bool>.CreateSuccess(true);
        public GenericResponse<CreditUsageRecord> ConsumeResult { get; set; } = GenericResponse<CreditUsageRecord>.CreateSuccess(new CreditUsageRecord());
        public int ConsumeCallCount { get; private set; }
        public Guid LastWorkspaceId { get; private set; }
        public Guid LastUserId { get; private set; }
        public CreditActionEnum LastAction { get; private set; }
        public long LastCredits { get; private set; }

        public Task<CreditWallet> EnsureWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(new CreditWallet { WorkspaceId = workspaceId, Balance = 100 });
        public Task<GenericResponse<CreditWallet>> GrantSubscriptionCreditsAsync(Guid workspaceId, Guid userId, WorkspaceTypeEnum workspaceType, SubscriptionPlanEnum plan, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<GenericResponse<CreditWallet>> GrantCreditPackCreditsAsync(Guid workspaceId, Guid userId, WorkspaceTypeEnum workspaceType, long credits, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<GenericResponse<bool>> EnsureCreditsAvailableAsync(Guid workspaceId, Guid userId, long credits, DateTime? now = null, CancellationToken cancellationToken = default)
            => Task.FromResult(AvailabilityResult);
        public Task<GenericResponse<CreditUsageRecord>> ConsumeCreditsAsync(Guid workspaceId, Guid userId, CreditActionEnum action, long credits, Guid? aiGenerationId = null, DateTime? now = null, CancellationToken cancellationToken = default)
        {
            ConsumeCallCount++;
            LastWorkspaceId = workspaceId;
            LastUserId = userId;
            LastAction = action;
            LastCredits = credits;
            return Task.FromResult(ConsumeResult);
        }
        public Task<GenericResponse<CreditUsageRecord>> RecordUsageAsync(Guid workspaceId, Guid userId, CreditActionEnum action, long credits, CreditUsageStatusEnum status, Guid? aiGenerationId = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<GenericResponse<CreditWallet>> AdminAdjustCreditsAsync(Guid workspaceId, Guid adminUserId, long amount, string reason, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<CreditWallet?> GetWalletAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<DailyCreditUsageDto>> GetDailyUsageAsync(Guid workspaceId, int days, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<CreditUsageRecordDto>> GetPagedUsageAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<long> GetMaximumBalanceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<CreditUsageRecordDto>> GetPagedUsageByUserAsync(Guid workspaceId, Guid userId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    /// <summary>Pass-through stub — returns the raw prompt unchanged so existing tests are not affected.</summary>
    private sealed class FakePromptEnhancerService : IPromptEnhancerService
    {
        public Task<string> EnhanceImagePromptAsync(string rawPrompt, AISAM.Data.Model.Product? product, bool hasReferenceImages, CancellationToken cancellationToken = default)
            => Task.FromResult(rawPrompt);

        public Task<(string Prompt, string? PatternId)> EnhanceVideoPromptAsync(string rawPrompt, AISAM.Data.Model.Product? product, int durationSeconds = 8, string? aspectRatio = null, List<string>? recentlyUsedPrompts = null, string? referenceImageUrl = null, CancellationToken cancellationToken = default)
            => Task.FromResult<(string Prompt, string? PatternId)>((rawPrompt, "fake_pattern"));

        public Task<string> EnhanceVideoPromptWithScriptAsync(string rawPrompt, string? videoScript, AISAM.Data.Model.Product? product, int durationSeconds = 9, string? aspectRatio = "9:16", string? referenceImageUrl = null, CancellationToken cancellationToken = default)
            => Task.FromResult(!string.IsNullOrWhiteSpace(rawPrompt) ? rawPrompt : videoScript ?? string.Empty);
    }
}








