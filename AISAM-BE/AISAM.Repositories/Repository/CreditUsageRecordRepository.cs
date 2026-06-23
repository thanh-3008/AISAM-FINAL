using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
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

    public async Task<PagedResult<CreditUsageRecord>> GetPagedByWorkspaceIdAsync(
        Guid workspaceId,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _context.CreditUsageRecords
            .Include(record => record.User)
            .Where(record => record.WorkspaceId == workspaceId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(record =>
                record.User.Email.ToLower().Contains(search) ||
                (record.User.FullName != null && record.User.FullName.ToLower().Contains(search)));
        }

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "credits" => request.SortDescending
                ? query.OrderByDescending(record => record.Credits)
                : query.OrderBy(record => record.Credits),
            "action" => request.SortDescending
                ? query.OrderByDescending(record => record.Action)
                : query.OrderBy(record => record.Action),
            "status" => request.SortDescending
                ? query.OrderByDescending(record => record.Status)
                : query.OrderBy(record => record.Status),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(record => record.CreatedAt)
                : query.OrderBy(record => record.CreatedAt),
            _ => query.OrderByDescending(record => record.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CreditUsageRecord>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<DailyCreditUsageDto>> GetDailyUsageAsync(Guid workspaceId, int days, CancellationToken cancellationToken = default)
    {
        var fromDate = DateTime.UtcNow.Date.AddDays(-days);

        var raw = await _context.CreditUsageRecords
            .Where(record =>
                record.WorkspaceId == workspaceId &&
                record.Status == CreditUsageStatusEnum.Success &&
                record.Action != CreditActionEnum.SubscriptionGrant &&
                record.Action != CreditActionEnum.CreditPackGrant &&
                record.CreatedAt >= fromDate)
            .GroupBy(record => record.CreatedAt.Date)
            .Select(group => new
            {
                Date = group.Key,
                TotalCredits = group.Sum(record => record.Credits)
            })
            .ToListAsync(cancellationToken);

        var lookup = raw.ToDictionary(r => DateOnly.FromDateTime(r.Date), r => r.TotalCredits);

        var result = new List<DailyCreditUsageDto>();
        for (var i = days; i >= 0; i--)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-i));
            result.Add(new DailyCreditUsageDto
            {
                Date = date,
                TotalCredits = lookup.GetValueOrDefault(date, 0)
            });
        }

        return result;
    }
}
