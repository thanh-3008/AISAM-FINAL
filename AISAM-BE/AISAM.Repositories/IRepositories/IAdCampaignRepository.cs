using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories
{
    public interface IAdCampaignRepository
    {
        Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AdCampaign?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<AdCampaign>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<AdCampaign> AddAsync(AdCampaign campaign, CancellationToken cancellationToken = default);
        Task UpdateAsync(AdCampaign campaign, CancellationToken cancellationToken = default);
    }
}
