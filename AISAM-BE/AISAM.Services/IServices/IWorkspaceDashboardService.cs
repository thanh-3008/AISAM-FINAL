using AISAM.Common;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IWorkspaceDashboardService
{
    Task<GenericResponse<WorkspaceDashboardSummaryDto>> GetSummaryAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
}
