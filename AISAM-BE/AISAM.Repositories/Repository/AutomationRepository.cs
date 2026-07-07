using AISAM.Data.Model;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class AutomationRepository : IAutomationRepository
{
    private readonly AisamContext _context;
    public AutomationRepository(AisamContext context) => _context = context;

    public async Task AddAsync(AutomationPlan plan, CancellationToken cancellationToken = default)
    {
        _context.AutomationPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AutomationPlan?> GetByIdAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
        => await _context.AutomationPlans.AsSplitQuery()
            .Include(plan => plan.Items.OrderBy(item => item.RowIndex).ThenBy(item => item.Platform))
                .ThenInclude(item => item.Brand)
            .Include(plan => plan.Items)
                .ThenInclude(item => item.Content)
            .Include(plan => plan.Items)
                .ThenInclude(item => item.ContentCalendar)
            .FirstOrDefaultAsync(plan => plan.Id == planId && plan.WorkspaceId == workspaceId && !plan.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<AutomationPlan>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => await _context.AutomationPlans.AsNoTracking()
            .Where(plan => plan.WorkspaceId == workspaceId && !plan.IsDeleted)
            .OrderByDescending(plan => plan.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task<AutomationPerformanceDto?> GetPerformanceAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
    {
        var items = await _context.AutomationItems.AsNoTracking()
            .Where(item => item.AutomationPlanId == planId && item.AutomationPlan.WorkspaceId == workspaceId && !item.AutomationPlan.IsDeleted)
            .Select(item => new { item.Status, item.ContentId })
            .ToListAsync(cancellationToken);
        if (items.Count == 0 && !await _context.AutomationPlans.AnyAsync(plan => plan.Id == planId && plan.WorkspaceId == workspaceId && !plan.IsDeleted, cancellationToken)) return null;
        var contentIds = items.Where(item => item.ContentId.HasValue).Select(item => item.ContentId!.Value).Distinct().ToList();
        var reports = await _context.PerformanceReports.AsNoTracking()
            .Where(report => !report.IsDeleted && report.PostId.HasValue && report.Post != null && contentIds.Contains(report.Post.ContentId))
            .ToListAsync(cancellationToken);
        return new AutomationPerformanceDto
        {
            PlanId = planId,
            TotalItems = items.Count,
            ScheduledItems = items.Count(item => item.Status == AutomationItemStatusEnum.Scheduled),
            PublishedItems = items.Count(item => item.Status == AutomationItemStatusEnum.Published),
            FailedItems = items.Count(item => item.Status is AutomationItemStatusEnum.GenerationFailed or AutomationItemStatusEnum.NeedsAttention or AutomationItemStatusEnum.PublishFailed),
            Impressions = reports.Sum(report => report.Impressions), Engagement = reports.Sum(report => report.Engagement),
            AverageCtr = reports.Count == 0 ? 0 : reports.Average(report => report.Ctr),
            EstimatedRevenue = reports.Sum(report => report.EstimatedRevenue)
        };
    }
}
