using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AisamContext _context;

        public AuditLogRepository(AisamContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AuditLog>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.AuditLogs.AsNoTracking().Include(al => al.Actor);
            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(al => al.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            return new PagedResult<AuditLog> { Data = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
        }

        public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
            return auditLog;
        }
    }
}
