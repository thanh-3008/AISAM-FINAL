using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories
{
    public interface IAuditLogRepository
    {
        Task<PagedResult<AuditLog>> GetPagedAsync(Common.Dtos.Request.AuditLogFilterRequest request, CancellationToken cancellationToken = default);
        Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
        Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetActiveUsersCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    }
}
