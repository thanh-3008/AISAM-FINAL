using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly AisamContext _context;

    public PaymentRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(payment => payment.Id == id && !payment.IsDeleted, cancellationToken);
    }

    public async Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(
                payment =>
                    !payment.IsDeleted &&
                    (payment.TransactionId == reference ||
                     (payment.Subscription != null &&
                      (payment.Subscription.PayOSOrderCode == reference ||
                       payment.Subscription.PayOSPaymentLinkId == reference))),
                cancellationToken);
    }

    public async Task<PagedResult<Payment>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query()
            .Where(payment =>
                !payment.IsDeleted &&
                payment.Subscription.ProfileId == profileId &&
                !payment.Subscription.IsDeleted &&
                payment.Status == PaymentStatusEnum.Success)
            .OrderByDescending(payment => payment.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Payment>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Payment>> GetPagedByWorkspaceIdAsync(
        Guid workspaceId,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query()
            .Where(payment =>
                !payment.IsDeleted &&
                payment.WorkspaceId == workspaceId &&
                payment.Status == PaymentStatusEnum.Success)
            .OrderByDescending(payment => payment.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Payment>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(payment => !payment.IsDeleted && payment.UserId == userId)
            .OrderByDescending(payment => payment.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        payment.CreatedAt = DateTime.UtcNow;
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<Payment>> GetPagedAllAsync(PaginationRequest request, PaymentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        var query = Query().AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<Payment> { Data = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments.CountAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments.Where(p => p.Status == PaymentStatusEnum.Success).SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<Dictionary<DateTime, decimal>> GetDailyRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to && p.Status == PaymentStatusEnum.Success)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Date, x => x.Total, cancellationToken);
    }

    public async Task<Dictionary<DateTime, int>> GetDailyTransactionCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, cancellationToken);
    }

    public async Task<List<TopWorkspaceRevenueDto>> GetTopWorkspacesByRevenueAsync(int limit, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.Where(p => p.Status == PaymentStatusEnum.Success && p.WorkspaceId != null);
        if (from.HasValue) query = query.Where(p => p.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(p => p.CreatedAt <= to.Value);

        return await query
            .GroupBy(p => p.WorkspaceId)
            .Select(g => new TopWorkspaceRevenueDto
            {
                WorkspaceId = g.Key ?? Guid.Empty,
                Revenue = g.Sum(p => p.Amount)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Payment> Query()
    {
        return _context.Payments
            .Include(payment => payment.Subscription)
            .Include(payment => payment.Workspace)
            .Include(payment => payment.User);
    }
}
