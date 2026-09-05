using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversation?> GetByIdForWorkspaceReadAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task<Conversation?> GetByIdForWorkspaceDeleteAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task<PagedResult<Conversation>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<Conversation>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task<Conversation?> GetActiveAsync(Guid profileId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default);
    Task<Conversation?> GetActiveByWorkspaceIdAsync(Guid workspaceId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, AiGeneration>> GetGenerationsByVideoJobIdsAsync(
        Guid workspaceId,
        IEnumerable<string> videoJobIds,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<string, AiGeneration>>(new Dictionary<string, AiGeneration>());
}
