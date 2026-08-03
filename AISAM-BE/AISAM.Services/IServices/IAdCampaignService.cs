using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IAdCampaignService
    {
        Task<GenericResponse<PagedResult<AdCampaignResponseDto>>> GetPagedByWorkspaceIdAsync(Guid workspaceId, Guid userId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> CreateAsync(Guid workspaceId, Guid userId, CreateAdCampaignRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> UpdateAsync(Guid id, Guid workspaceId, Guid userId, UpdateAdCampaignRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> DeployAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> DeployToFacebookAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> ActivateAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> CleanupFailedDeploymentAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> SyncCampaignInsightsAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> DuplicateAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);

        Task<GenericResponse<BulkCampaignResultDto>> BulkCreateAsync(Guid workspaceId, Guid userId, BulkCreateAdCampaignRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<BulkCampaignResultDto>> BulkDeleteAsync(Guid workspaceId, Guid userId, BulkDeleteAdCampaignRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<BulkCampaignResultDto>> BulkDeployAsync(Guid workspaceId, Guid userId, BulkDeployAdCampaignRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<CampaignPreviewDto>> GetPreviewAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    }
}
