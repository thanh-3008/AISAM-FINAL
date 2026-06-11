using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ICreditUsageRecordRepository
{
    Task<CreditUsageRecord> AddAsync(CreditUsageRecord record, CancellationToken cancellationToken = default);
}
