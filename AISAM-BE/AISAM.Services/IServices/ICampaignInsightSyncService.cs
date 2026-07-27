using AISAM.Common;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface ICampaignInsightSyncService
{
    Task<GenericResponse<AnalyticsSyncResultDto>> SyncAsync(
        Guid workspaceId,
        Guid userId,
        AnalyticsSyncRequest request,
        CancellationToken cancellationToken = default);
}
