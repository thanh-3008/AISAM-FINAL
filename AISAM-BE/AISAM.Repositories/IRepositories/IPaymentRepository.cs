using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Data.Enumeration;

namespace AISAM.Repositories.IRepositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task<PagedResult<Payment>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<Payment>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<Payment>> GetPagedAllAsync(PaginationRequest request, PaymentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<DateTime, decimal>> GetDailyRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<Dictionary<DateTime, int>> GetDailyTransactionCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<TopWorkspaceRevenueDto>> GetTopWorkspacesByRevenueAsync(int limit, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}

public class TopWorkspaceRevenueDto
{
    public Guid WorkspaceId { get; set; }
    public decimal Revenue { get; set; }
}
