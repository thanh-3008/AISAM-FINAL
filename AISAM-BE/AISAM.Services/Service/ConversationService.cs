using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;

    public ConversationService(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<GenericResponse<PagedResult<ConversationResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetPagedByProfileIdAsync(profileId, request, cancellationToken);
        return GenericResponse<PagedResult<ConversationResponseDto>>.CreateSuccess(new PagedResult<ConversationResponseDto>
        {
            Data = conversations.Data.Select(MapToResponseDto).ToList(),
            TotalCount = conversations.TotalCount,
            Page = conversations.Page,
            PageSize = conversations.PageSize
        }, "Conversations retrieved successfully.");
    }

    public async Task<GenericResponse<ConversationDetailDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id, cancellationToken);
        if (conversation == null || conversation.ProfileId != profileId)
        {
            return GenericResponse<ConversationDetailDto>.CreateError("Conversation not found.", HttpStatusCode.NotFound);
        }

        return GenericResponse<ConversationDetailDto>.CreateSuccess(MapToDetailDto(conversation), "Conversation retrieved successfully.");
    }

    public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id, cancellationToken);
        if (conversation == null || conversation.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError("Conversation not found.", HttpStatusCode.NotFound);
        }

        conversation.IsDeleted = true;
        conversation.IsActive = false;
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Conversation deleted successfully.");
    }

    public async Task<GenericResponse<PagedResult<ConversationResponseDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, cancellationToken);
        return GenericResponse<PagedResult<ConversationResponseDto>>.CreateSuccess(new PagedResult<ConversationResponseDto>
        {
            Data = conversations.Data.Select(MapToResponseDto).ToList(),
            TotalCount = conversations.TotalCount,
            Page = conversations.Page,
            PageSize = conversations.PageSize
        }, "Conversations retrieved successfully.");
    }

    public async Task<GenericResponse<ConversationDetailDto>> GetByIdInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id, cancellationToken);
        return conversation == null || conversation.WorkspaceId != workspaceId
            ? GenericResponse<ConversationDetailDto>.CreateError("Conversation not found.", HttpStatusCode.NotFound)
            : GenericResponse<ConversationDetailDto>.CreateSuccess(MapToDetailDto(conversation), "Conversation retrieved successfully.");
    }

    public async Task<GenericResponse<bool>> SoftDeleteInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id, cancellationToken);
        if (conversation == null || conversation.WorkspaceId != workspaceId)
        {
            return GenericResponse<bool>.CreateError("Conversation not found.", HttpStatusCode.NotFound);
        }
        conversation.IsDeleted = true;
        conversation.IsActive = false;
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Conversation deleted successfully.");
    }

    private static ConversationResponseDto MapToResponseDto(Conversation conversation)
    {
        var messages = conversation.ChatMessages
            .Where(message => !message.IsDeleted)
            .OrderBy(message => message.CreatedAt)
            .ToList();
        var lastMessage = messages.LastOrDefault();

        return new ConversationResponseDto
        {
            Id = conversation.Id,
            ProfileId = conversation.ProfileId,
            BrandId = conversation.BrandId,
            BrandName = conversation.Brand?.Name,
            ProductId = conversation.ProductId,
            ProductName = conversation.Product?.Name,
            AdType = conversation.AdType,
            Title = conversation.Title,
            IsActive = conversation.IsActive,
            LastMessage = lastMessage?.Message,
            LastMessageAt = lastMessage?.CreatedAt,
            MessageCount = messages.Count
        };
    }

    private static ConversationDetailDto MapToDetailDto(Conversation conversation)
    {
        var summary = MapToResponseDto(conversation);
        return new ConversationDetailDto
        {
            Id = summary.Id,
            ProfileId = summary.ProfileId,
            BrandId = summary.BrandId,
            BrandName = summary.BrandName,
            ProductId = summary.ProductId,
            ProductName = summary.ProductName,
            AdType = summary.AdType,
            Title = summary.Title,
            IsActive = summary.IsActive,
            LastMessage = summary.LastMessage,
            LastMessageAt = summary.LastMessageAt,
            MessageCount = summary.MessageCount,
            Messages = conversation.ChatMessages
                .Where(message => !message.IsDeleted)
                .OrderBy(message => message.CreatedAt)
                .Select(MapToChatMessageDto)
                .ToList()
        };
    }

    private static ChatMessageDto MapToChatMessageDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            SenderType = message.SenderType,
            Message = message.Message,
            AiGenerationId = message.AiGenerationId,
            ContentId = message.ContentId,
            CreatedAt = message.CreatedAt
        };
    }
}
