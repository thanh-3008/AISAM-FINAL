using AISAM.Common;

namespace AISAM.Services.IServices
{
    public interface IAdminDashboardService
    {
        Task<GenericResponse<object>> GetSummaryAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetChartsAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetRevenueStatsAsync(Guid adminUserId, string period, CancellationToken cancellationToken = default);
    }
}
