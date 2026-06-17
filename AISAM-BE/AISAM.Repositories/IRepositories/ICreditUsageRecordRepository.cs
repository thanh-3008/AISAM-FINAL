using AISAM.Data.Model;
using AISAM.Common.Dtos;

namespace AISAM.Repositories.IRepositories;

public interface ICreditUsageRecordRepository
{
    Task<CreditUsageRecord> AddAsync(CreditUsageRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditUsageRecord>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<PagedResult<CreditUsageRecord>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default);
}
