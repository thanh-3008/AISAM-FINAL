using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;
using System.Text.RegularExpressions;

namespace AISAM.Services.Service;

public sealed class ConversationService : IConversationService
{
    private static readonly Regex VideoJobMarkerRegex = new(@"\[VIDEO_JOB:\s*([^\]]+)\]", RegexOptions.Compiled);
    private readonly IConversationRepository _conversationRepository;
    private readonly AccessScope? _accessScope;

    public ConversationService(IConversationRepository conversationRepository, AccessScope? accessScope = null)
    {
        _conversationRepository = conversationRepository;
        _accessScope = accessScope;
    }

    public async Task<GenericResponse<PagedResult<ConversationResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetPagedByProfileIdAsync(profileId, request, cancellationToken);
        return GenericResponse<PagedResult<ConversationResponseDto>>.CreateSuccess(new PagedResult<ConversationResponseDto>
        {
            Data = conversations.Data.Select(conversation => MapToResponseDto(conversation)).ToList(),
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

        var generationLinks = await ResolveLegacyGenerationLinksAsync(conversation, cancellationToken);
        return GenericResponse<ConversationDetailDto>.CreateSuccess(MapToDetailDto(conversation, generationLinks), "Conversation retrieved successfully.");
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
            Data = conversations.Data.Select(conversation => MapToResponseDto(
                conversation,
                suppressMessageBody: _accessScope?.Enforced == true && _accessScope.Role == WorkspaceMemberRoleEnum.Viewer)).ToList(),
            TotalCount = conversations.TotalCount,
            Page = conversations.Page,
            PageSize = conversations.PageSize
        }, "Conversations retrieved successfully.");
    }

    public async Task<GenericResponse<ConversationDetailDto>> GetByIdInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        if (_accessScope?.Enforced == true && _accessScope.Role == WorkspaceMemberRoleEnum.Viewer)
        {
            return GenericResponse<ConversationDetailDto>.CreateError(
                "Viewers cannot read conversation message bodies.",
                HttpStatusCode.Forbidden,
                "RESOURCE_ACCESS_DENIED");
        }

        var conversation = await _conversationRepository.GetByIdForWorkspaceReadAsync(id, workspaceId, cancellationToken);
        if (conversation == null)
        {
            return GenericResponse<ConversationDetailDto>.CreateError("Conversation not found.", HttpStatusCode.NotFound);
        }

        var generationLinks = await ResolveLegacyGenerationLinksAsync(conversation, cancellationToken);
        return GenericResponse<ConversationDetailDto>.CreateSuccess(MapToDetailDto(conversation, generationLinks), "Conversation retrieved successfully.");
    }

    public async Task<GenericResponse<bool>> SoftDeleteInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
    {
        if (_accessScope?.Enforced == true && _accessScope.Role == WorkspaceMemberRoleEnum.Viewer)
        {
            return GenericResponse<bool>.CreateError(
                "Viewers cannot delete conversations.",
                HttpStatusCode.Forbidden,
                "RESOURCE_ACCESS_DENIED");
        }

        var conversation = await _conversationRepository.GetByIdForWorkspaceDeleteAsync(id, workspaceId, cancellationToken);
        if (conversation == null)
        {
            return GenericResponse<bool>.CreateError("Conversation not found.", HttpStatusCode.NotFound);
        }
        conversation.IsDeleted = true;
        conversation.IsActive = false;
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Conversation deleted successfully.");
    }

    private static ConversationResponseDto MapToResponseDto(Conversation conversation, bool suppressMessageBody = false)
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
            LastMessage = suppressMessageBody ? null : lastMessage?.Message,
            LastMessageAt = lastMessage?.CreatedAt,
            MessageCount = messages.Count
        };
    }

    private async Task<IReadOnlyDictionary<string, AiGeneration>> ResolveLegacyGenerationLinksAsync(
        Conversation conversation,
        CancellationToken cancellationToken)
    {
        var missingJobIds = conversation.ChatMessages
            .Where(message => !message.IsDeleted &&
                              message.SenderType == ChatSenderType.AI &&
                              message.ContentId == null)
            .Select(message => VideoJobMarkerRegex.Match(message.Message))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value.Trim())
            .Where(jobId => jobId.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return await _conversationRepository.GetGenerationsByVideoJobIdsAsync(conversation.WorkspaceId, missingJobIds, cancellationToken);
    }

    private static ConversationDetailDto MapToDetailDto(
        Conversation conversation,
        IReadOnlyDictionary<string, AiGeneration> generationLinks)
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
                .Select(message => MapToChatMessageDto(message, generationLinks))
                .ToList()
        };
    }

    private static ChatMessageDto MapToChatMessageDto(
        ChatMessage message,
        IReadOnlyDictionary<string, AiGeneration> generationLinks)
    {
        AiGeneration? legacyGeneration = null;
        if (message.ContentId == null)
        {
            var match = VideoJobMarkerRegex.Match(message.Message);
            if (match.Success)
            {
                generationLinks.TryGetValue(match.Groups[1].Value.Trim(), out legacyGeneration);
            }
        }

        return new ChatMessageDto
        {
            Id = message.Id,
            SenderType = message.SenderType,
            Message = message.Message,
            AiGenerationId = message.AiGenerationId ?? legacyGeneration?.Id,
            ContentId = message.ContentId ?? legacyGeneration?.ContentId,
            CreatedAt = message.CreatedAt
        };
    }
}
