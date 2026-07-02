using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ICreditUsageRecordRepository
{
    Task<CreditUsageRecord> AddAsync(CreditUsageRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditUsageRecord>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<PagedResult<CreditUsageRecord>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<Dictionary<DateTime, long>> GetDailySummaryAsync(Guid workspaceId, int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AISAM.Common.Models.DailyCreditUsageDto>> GetDailyUsageAsync(Guid workspaceId, int days, CancellationToken cancellationToken = default);
}
