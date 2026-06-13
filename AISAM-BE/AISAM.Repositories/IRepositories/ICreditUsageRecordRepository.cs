using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ICreditUsageRecordRepository
{
    Task<CreditUsageRecord> AddAsync(CreditUsageRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditUsageRecord>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
