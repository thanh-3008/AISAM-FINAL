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

        public async Task<PagedResult<AuditLog>> GetPagedAsync(Common.Dtos.Request.AuditLogFilterRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.AuditLogs.AsNoTracking().Include(al => al.Actor).AsQueryable();

            if (!string.IsNullOrEmpty(request.ActionType))
                query = query.Where(al => al.ActionType == request.ActionType);

            if (!string.IsNullOrEmpty(request.TargetTable))
                query = query.Where(al => al.TargetTable == request.TargetTable);

            if (request.FromDate.HasValue)
                query = query.Where(al => al.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
            {
                var toDateStr = request.ToDate.Value.AddDays(1).Date;
                query = query.Where(al => al.CreatedAt < toDateStr);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(al => al.Actor.Email.ToLower().Contains(search) || al.Actor.FullName.ToLower().Contains(search));
            }

            if (request.ActorId.HasValue)
                query = query.Where(al => al.ActorId == request.ActorId.Value);

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

        public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Include(al => al.Actor)
                .FirstOrDefaultAsync(al => al.Id == id, cancellationToken);
        }

        public async Task<int> GetActiveUsersCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
                .Select(a => a.ActorId)
                .Distinct()
                .CountAsync(cancellationToken);
        }
    }
}
