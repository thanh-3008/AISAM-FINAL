using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyList<CreditUsageRecord>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.CreditUsageRecords
            .Include(record => record.User)
            .Where(record => record.WorkspaceId == workspaceId)
            .OrderByDescending(record => record.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
