using AISAM.Common;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IDashboardService
{
    Task<GenericResponse<DashboardSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default);
}
