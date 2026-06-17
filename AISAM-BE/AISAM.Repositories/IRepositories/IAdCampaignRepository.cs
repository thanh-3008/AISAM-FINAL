using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IAdCampaignRepository
{
    Task<PagedResult<AdCampaign>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdCampaign> AddAsync(AdCampaign campaign, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdCampaign campaign, CancellationToken cancellationToken = default);
}
