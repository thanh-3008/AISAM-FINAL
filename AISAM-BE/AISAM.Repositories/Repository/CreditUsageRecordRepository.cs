using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;

namespace AISAM.Repositories.Repository;

public sealed class CreditUsageRecordRepository : ICreditUsageRecordRepository
{
    private readonly AisamContext _context;

    public CreditUsageRecordRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<CreditUsageRecord> AddAsync(CreditUsageRecord record, CancellationToken cancellationToken = default)
    {
        record.CreatedAt = DateTime.UtcNow;
        _context.CreditUsageRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
        return record;
    }
}
