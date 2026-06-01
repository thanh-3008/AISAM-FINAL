using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class PerformanceReportRepository : IPerformanceReportRepository
{
    private readonly AisamContext _context;

    public PerformanceReportRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<int> CountByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReports
            .Include(report => report.Post)
                .ThenInclude(post => post!.Content)
            .Where(report =>
                !report.IsDeleted &&
                report.Post != null &&
                report.Post.Content != null &&
                report.Post.Content.ProfileId == profileId)
            .CountAsync(cancellationToken);
    }
}
