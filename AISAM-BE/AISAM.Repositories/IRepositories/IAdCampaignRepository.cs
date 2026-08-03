using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
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
        Task SetFacebookCampaignIdAsync(Guid campaignId, string facebookCampaignId, CancellationToken cancellationToken = default);
        Task AddAdSetAsync(AdSet adSet, CancellationToken cancellationToken = default);
        Task AddAdCreativeAsync(AdCreative creative, CancellationToken cancellationToken = default);
        Task AddAdAsync(Ad ad, CancellationToken cancellationToken = default);

        // ─── Insights ───
        Task UpdateCampaignInsightsAsync(Guid campaignId, long impressions, long clicks, decimal spend, long conversions, CancellationToken cancellationToken = default);

        // ─── Deployment step tracking ───
        Task UpdateDeploymentStatusAsync(Guid campaignId, DeploymentStatusEnum status, int step, CancellationToken cancellationToken = default);
        Task UpdateDeploymentFailureAsync(Guid campaignId, int step, string message, CancellationToken cancellationToken = default);
        Task UpdateCampaignStatusAsync(Guid campaignId, CampaignStatusEnum status, CancellationToken cancellationToken = default);
        Task<AdSet?> GetAdSetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<Ad?> GetAdByAdSetIdAsync(Guid adSetId, CancellationToken cancellationToken = default);
        Task<AdCreative?> GetCreativeByIdAsync(Guid creativeId, CancellationToken cancellationToken = default);

        // ─── Cleanup ───
        Task<IReadOnlyList<AdSet>> GetAdSetsByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Ad>> GetAdsByAdSetIdAsync(Guid adSetId, CancellationToken cancellationToken = default);
        Task HardDeleteAdAsync(Guid adId, CancellationToken cancellationToken = default);
        Task HardDeleteAdCreativeAsync(Guid creativeId, CancellationToken cancellationToken = default);
        Task HardDeleteAdSetAsync(Guid adSetId, CancellationToken cancellationToken = default);
        Task ClearFacebookIdsAsync(Guid campaignId, CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, int>> UpdateExpiredCampaignsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdCampaign>> GetDeployedCampaignsForSyncAsync(int batchSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdCampaign>> GetDeployedPendingActivationAsync(int batchSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdCampaign>> GetActiveCampaignsPastEndDateAsync(CancellationToken cancellationToken = default);
    }
}
