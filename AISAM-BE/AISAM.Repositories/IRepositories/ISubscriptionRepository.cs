using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetCurrentActiveByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<PagedResult<Subscription>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default);
    Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default);
    Task<int> CountSuccessfulPromptUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default);
    Task<int> CountSuccessfulPostUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default);
}
