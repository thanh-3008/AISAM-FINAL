using AISAM.Common;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class AIService : IAIService
{
    private readonly IContentRepository _contentRepository;
    private readonly IAiGenerationRepository _generationRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IProductRepository _productRepository;
    private readonly IGeminiTextClient _geminiTextClient;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICreditService _creditService;
    private const long TextGenerationCredits = 1;

    public AIService(
        IContentRepository contentRepository,
        IAiGenerationRepository generationRepository,
        IBrandRepository brandRepository,
        IProductRepository productRepository,
        IGeminiTextClient geminiTextClient,
        IConversationRepository conversationRepository,
        ICreditService creditService)
    {
        _contentRepository = contentRepository;
        _generationRepository = generationRepository;
        _brandRepository = brandRepository;
        _productRepository = productRepository;
        _geminiTextClient = geminiTextClient;
        _conversationRepository = conversationRepository;
        _creditService = creditService;
    }

    public async Task<GenericResponse<AiGenerationResponse>> GenerateDraftAsync(Guid profileId, Guid workspaceId, Guid userId, CreateDraftRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateBrandAndProductInWorkspaceAsync(workspaceId, request.BrandId, request.ProductId, cancellationToken);
        if (!validation.Success)
        {
            return GenericResponse<AiGenerationResponse>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
        }

        var creditCheck = await _creditService.EnsureCreditsAvailableAsync(workspaceId, userId, TextGenerationCredits, cancellationToken: cancellationToken);
        if (!creditCheck.Success)
        {
            return GenericResponse<AiGenerationResponse>.CreateError(
                creditCheck.Message!,
                (HttpStatusCode)creditCheck.StatusCode,
                creditCheck.Error?.ErrorCode);
        }

        var content = await _contentRepository.AddAsync(new Content
        {
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = request.BrandId,
            ProductId = request.ProductId,
            AdType = request.AdType,
            Title = request.Title,
            TextContent = string.Empty,
            Status = ContentStatusEnum.Draft
        }, cancellationToken);

        var generation = await GenerateForContentAsync(content, request.Prompt, cancellationToken);
        return await ChargeSuccessfulGenerationAsync(generation, workspaceId, userId, CreditActionEnum.GenerateText, cancellationToken);
    }

    public async Task<GenericResponse<AiGenerationResponse>> ImproveAsync(Guid contentId, Guid profileId, Guid workspaceId, Guid userId, ImproveContentRequest request, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId)
        {
            return GenericResponse<AiGenerationResponse>.CreateError("Content not found.", HttpStatusCode.NotFound);
        }

        var creditCheck = await _creditService.EnsureCreditsAvailableAsync(workspaceId, userId, TextGenerationCredits, cancellationToken: cancellationToken);
        if (!creditCheck.Success)
        {
            return GenericResponse<AiGenerationResponse>.CreateError(
                creditCheck.Message!,
                (HttpStatusCode)creditCheck.StatusCode,
                creditCheck.Error?.ErrorCode);
        }

        var generation = await GenerateForContentAsync(content, request.Prompt, cancellationToken);
        return await ChargeSuccessfulGenerationAsync(generation, workspaceId, userId, CreditActionEnum.RegenerateText, cancellationToken);
    }

    public async Task<GenericResponse<ContentResponseDto>> ApproveAsync(Guid generationId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var generation = await _generationRepository.GetByIdAsync(generationId, cancellationToken);
        if (generation == null || generation.Content.ProfileId != profileId)
        {
            return GenericResponse<ContentResponseDto>.CreateError("AI generation not found.", HttpStatusCode.NotFound);
        }

        if (generation.Status != AiStatusEnum.Completed || string.IsNullOrWhiteSpace(generation.GeneratedText))
        {
            return GenericResponse<ContentResponseDto>.CreateError("AI generation is not completed.", HttpStatusCode.BadRequest);
        }

        generation.Content.TextContent = generation.GeneratedText;
        generation.Content.Status = ContentStatusEnum.Draft;
        await _contentRepository.UpdateAsync(generation.Content, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapContent(generation.Content), "AI generation approved.");
    }

    public async Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsAsync(Guid contentId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return GenericResponse<IEnumerable<AiGenerationResponse>>.CreateError("Content not found.", HttpStatusCode.NotFound);
        }

        var generations = await _generationRepository.GetByContentIdAsync(contentId, cancellationToken);
        return GenericResponse<IEnumerable<AiGenerationResponse>>.CreateSuccess(generations.Select(MapGeneration), "AI generations retrieved.");
    }

    public async Task<GenericResponse<ChatResponse>> ChatAsync(Guid profileId, ChatRequest request, CancellationToken cancellationToken = default)
        => await ChatInternalAsync(profileId, null, request, cancellationToken);

    public async Task<GenericResponse<ChatResponse>> ChatInWorkspaceAsync(Guid profileId, Guid workspaceId, ChatRequest request, CancellationToken cancellationToken = default)
        => await ChatInternalAsync(profileId, workspaceId, request, cancellationToken);

    private async Task<GenericResponse<ChatResponse>> ChatInternalAsync(Guid profileId, Guid? workspaceId, ChatRequest request, CancellationToken cancellationToken)
    {
        if (request.BrandId.HasValue)
        {
            var validation = workspaceId.HasValue
                ? await ValidateBrandAndProductInWorkspaceAsync(workspaceId.Value, request.BrandId.Value, request.ProductId, cancellationToken)
                : await ValidateBrandAndProductAsync(profileId, request.BrandId.Value, request.ProductId, cancellationToken);
            if (!validation.Success)
            {
                return GenericResponse<ChatResponse>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
            }
        }
        else if (request.ProductId.HasValue)
        {
            return GenericResponse<ChatResponse>.CreateError("Brand is required when product is selected.", HttpStatusCode.BadRequest);
        }

        Conversation? conversation;
        if (request.ConversationId.HasValue)
        {
            conversation = await _conversationRepository.GetByIdAsync(request.ConversationId.Value, cancellationToken);
            if (conversation == null || (workspaceId.HasValue ? conversation.WorkspaceId != workspaceId : conversation.ProfileId != profileId))
            {
                return GenericResponse<ChatResponse>.CreateError("Conversation not found.", HttpStatusCode.NotFound);
            }
        }
        else
        {
            conversation = workspaceId.HasValue
                ? await _conversationRepository.GetActiveByWorkspaceIdAsync(workspaceId.Value, request.BrandId, request.ProductId, request.AdType, cancellationToken)
                : await _conversationRepository.GetActiveAsync(profileId, request.BrandId, request.ProductId, request.AdType, cancellationToken);
            conversation ??= await _conversationRepository.AddAsync(new Conversation
            {
                ProfileId = profileId,
                WorkspaceId = workspaceId ?? throw new InvalidOperationException("Workspace context is required."),
                BrandId = request.BrandId,
                ProductId = request.ProductId,
                AdType = request.AdType,
                Title = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim()[..Math.Min(request.Message.Trim().Length, 255)]
            }, cancellationToken);
        }

        await _conversationRepository.AddMessageAsync(new ChatMessage
        {
            ConversationId = conversation.Id,
            SenderType = ChatSenderType.User,
            Message = request.Message
        }, cancellationToken);

        try
        {
            var responseText = await _geminiTextClient.GenerateAsync(BuildChatPrompt(conversation, request.Message), cancellationToken);
            await _conversationRepository.AddMessageAsync(new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderType = ChatSenderType.AI,
                Message = responseText
            }, cancellationToken);

            return GenericResponse<ChatResponse>.CreateSuccess(new ChatResponse
            {
                ConversationId = conversation.Id,
                Response = responseText
            });
        }
        catch (Exception)
        {
            const string errorMessage = "AI chat is temporarily unavailable.";
            await _conversationRepository.AddMessageAsync(new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderType = ChatSenderType.AI,
                Message = errorMessage
            }, cancellationToken);

            return GenericResponse<ChatResponse>.CreateError(errorMessage, HttpStatusCode.ServiceUnavailable);
        }
    }

    private async Task<AiGenerationResponse> GenerateForContentAsync(Content content, string prompt, CancellationToken cancellationToken)
    {
        var generation = await _generationRepository.AddAsync(new AiGeneration
        {
            ContentId = content.Id,
            Content = content,
            AiPrompt = prompt,
            Status = AiStatusEnum.Pending
        }, cancellationToken);

        try
        {
            generation.GeneratedText = await _geminiTextClient.GenerateAsync(prompt, cancellationToken);
            generation.Status = AiStatusEnum.Completed;
        }
        catch (Exception ex)
        {
            generation.Status = AiStatusEnum.Failed;
            generation.ErrorMessage = ex.Message;
        }

        await _generationRepository.UpdateAsync(generation, cancellationToken);
        return MapGeneration(generation);
    }

    public async Task<GenericResponse<ContentResponseDto>> ApproveInWorkspaceAsync(Guid generationId, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var generation = await _generationRepository.GetByIdAsync(generationId, cancellationToken);
        if (generation == null || generation.Content.WorkspaceId != workspaceId) return GenericResponse<ContentResponseDto>.CreateError("AI generation not found.", HttpStatusCode.NotFound);
        if (generation.Status != AiStatusEnum.Completed || string.IsNullOrWhiteSpace(generation.GeneratedText)) return GenericResponse<ContentResponseDto>.CreateError("AI generation is not completed.", HttpStatusCode.BadRequest);
        generation.Content.TextContent = generation.GeneratedText;
        generation.Content.Status = ContentStatusEnum.Draft;
        await _contentRepository.UpdateAsync(generation.Content, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapContent(generation.Content), "AI generation approved.");
    }

    public async Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsInWorkspaceAsync(Guid contentId, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId) return GenericResponse<IEnumerable<AiGenerationResponse>>.CreateError("Content not found.", HttpStatusCode.NotFound);
        return GenericResponse<IEnumerable<AiGenerationResponse>>.CreateSuccess((await _generationRepository.GetByContentIdAsync(contentId, cancellationToken)).Select(MapGeneration), "AI generations retrieved.");
    }

    private async Task<GenericResponse<AiGenerationResponse>> ChargeSuccessfulGenerationAsync(
        AiGenerationResponse generation,
        Guid workspaceId,
        Guid userId,
        CreditActionEnum action,
        CancellationToken cancellationToken)
    {
        if (generation.Status != AiStatusEnum.Completed)
        {
            return GenericResponse<AiGenerationResponse>.CreateSuccess(generation, "AI generation processed.");
        }

        var charge = await _creditService.ConsumeCreditsAsync(
            workspaceId,
            userId,
            action,
            TextGenerationCredits,
            generation.AiGenerationId,
            cancellationToken: cancellationToken);

        if (charge.Success)
        {
            return GenericResponse<AiGenerationResponse>.CreateSuccess(generation, "AI generation processed.");
        }

        var storedGeneration = await _generationRepository.GetByIdAsync(generation.AiGenerationId, cancellationToken);
        if (storedGeneration != null)
        {
            storedGeneration.Status = AiStatusEnum.Failed;
            storedGeneration.GeneratedText = null;
            storedGeneration.ErrorMessage = charge.Message;
            await _generationRepository.UpdateAsync(storedGeneration, cancellationToken);
        }

        return GenericResponse<AiGenerationResponse>.CreateError(
            charge.Message!,
            (HttpStatusCode)charge.StatusCode,
            charge.Error?.ErrorCode);
    }

    private async Task<GenericResponse<bool>> ValidateBrandAndProductAsync(Guid profileId, Guid brandId, Guid? productId, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand == null || brand.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError("Brand not found.", HttpStatusCode.NotFound);
        }

        if (productId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
            if (product == null)
            {
                return GenericResponse<bool>.CreateError("Product not found.", HttpStatusCode.NotFound);
            }

            if (product.BrandId != brandId)
            {
                return GenericResponse<bool>.CreateError("Product does not belong to the selected brand.", HttpStatusCode.BadRequest);
            }
        }

        return GenericResponse<bool>.CreateSuccess(true);
    }

    private async Task<GenericResponse<bool>> ValidateBrandAndProductInWorkspaceAsync(Guid workspaceId, Guid brandId, Guid? productId, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand == null || brand.WorkspaceId != workspaceId) return GenericResponse<bool>.CreateError("Brand not found.", HttpStatusCode.NotFound);
        if (productId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
            if (product == null) return GenericResponse<bool>.CreateError("Product not found.", HttpStatusCode.NotFound);
            if (product.BrandId != brandId) return GenericResponse<bool>.CreateError("Product does not belong to the selected brand.", HttpStatusCode.BadRequest);
        }
        return GenericResponse<bool>.CreateSuccess(true);
    }

    private static AiGenerationResponse MapGeneration(AiGeneration generation)
    {
        return new AiGenerationResponse
        {
            AiGenerationId = generation.Id,
            ContentId = generation.ContentId,
            GeneratedText = generation.GeneratedText,
            Status = generation.Status,
            ErrorMessage = generation.ErrorMessage,
            CreatedAt = generation.CreatedAt
        };
    }

    private static string BuildChatPrompt(Conversation conversation, string message)
    {
        return $"BrandId: {conversation.BrandId?.ToString() ?? "none"}\nProductId: {conversation.ProductId?.ToString() ?? "none"}\nAdType: {conversation.AdType}\nUser: {message}";
    }

    private static ContentResponseDto MapContent(Content content)
    {
        return new ContentResponseDto
        {
            Id = content.Id,
            ProfileId = content.ProfileId,
            BrandId = content.BrandId,
            BrandName = content.Brand?.Name,
            ProductId = content.ProductId,
            AdType = content.AdType,
            Title = content.Title,
            TextContent = content.TextContent,
            ImageUrl = content.ImageUrl,
            VideoUrl = content.VideoUrl,
            StyleDescription = content.StyleDescription,
            ContextDescription = content.ContextDescription,
            RepresentativeCharacter = content.RepresentativeCharacter,
            Status = content.Status,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }
}
