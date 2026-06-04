using AISAM.Common.Dtos;
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
                payment => !payment.IsDeleted && payment.TransactionId == reference,
                cancellationToken);
    }

    public async Task<PagedResult<Payment>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query()
            .Where(payment =>
                !payment.IsDeleted &&
                payment.Subscription != null &&
                payment.Subscription.ProfileId == profileId &&
                !payment.Subscription.IsDeleted)
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

    private IQueryable<Payment> Query()
    {
        return _context.Payments
            .Include(payment => payment.Subscription)
            .Include(payment => payment.User);
    }
}
