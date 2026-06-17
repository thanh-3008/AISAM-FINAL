using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IAdCampaignService
{
    Task<GenericResponse<PagedResult<AdCampaignDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdCampaignDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid campaignId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdCampaignDto>> CreateInWorkspaceAsync(Guid workspaceId, Guid profileId, CreateAdCampaignRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdCampaignDto>> UpdateInWorkspaceAsync(Guid workspaceId, Guid campaignId, UpdateAdCampaignRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> DeleteInWorkspaceAsync(Guid workspaceId, Guid campaignId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdCampaignDto>> SyncInWorkspaceAsync(Guid workspaceId, Guid campaignId, CancellationToken cancellationToken = default);
}
