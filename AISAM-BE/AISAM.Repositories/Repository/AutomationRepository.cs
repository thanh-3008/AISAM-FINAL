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

    public async Task<AutomationPlan?> GetByIdForReadAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
    {
        var visibleItems = VisibleItemsForRead(workspaceId);
        var plan = await _context.AutomationPlans.AsNoTracking()
            .Where(candidate => candidate.Id == planId && candidate.WorkspaceId == workspaceId && !candidate.IsDeleted)
            .Where(candidate => visibleItems.Any(item => item.AutomationPlanId == candidate.Id))
            .FirstOrDefaultAsync(cancellationToken);
        if (plan is null) return null;

        plan.Items = await LoadVisibleItemsAsync(visibleItems.Where(item => item.AutomationPlanId == plan.Id), cancellationToken);
        return plan;
    }

    public async Task<IReadOnlyList<AutomationPlan>> GetByWorkspaceForReadAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var visibleItems = VisibleItemsForRead(workspaceId);
        var plans = await _context.AutomationPlans.AsNoTracking()
            .Where(plan => plan.WorkspaceId == workspaceId && !plan.IsDeleted)
            .Where(plan => visibleItems.Any(item => item.AutomationPlanId == plan.Id))
            .OrderByDescending(plan => plan.CreatedAt)
            .ToListAsync(cancellationToken);
        if (plans.Count == 0) return plans;

        var planIds = plans.Select(plan => plan.Id).ToArray();
        var items = await LoadVisibleItemsAsync(
            visibleItems.Where(item => planIds.Contains(item.AutomationPlanId)),
            cancellationToken);
        var itemsByPlan = items.ToLookup(item => item.AutomationPlanId);
        foreach (var plan in plans)
            plan.Items = itemsByPlan[plan.Id].ToList();
        return plans;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task<AutomationPerformanceDto?> GetPerformanceAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
    {
        var items = await _context.AutomationItemsForAnalytics(workspaceId).AsNoTracking()
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

    private IQueryable<AutomationItem> VisibleItemsForRead(Guid workspaceId)
    {
        var scope = _context.AccessScope;
        if (!scope.Enforced || scope.WorkspaceId != workspaceId)
            throw new UnauthorizedAccessException("A current workspace scope is required.");

        if (scope.IsOwner)
        {
            return _context.AutomationItems
                .Where(item => item.AutomationPlan.WorkspaceId == workspaceId && !item.AutomationPlan.IsDeleted);
        }

        if (scope.Role == WorkspaceMemberRoleEnum.Manager)
        {
            return _context.AutomationItemsForAnalytics(workspaceId)
                .Where(item => !item.AutomationPlan.IsDeleted);
        }

        if (scope.Role == WorkspaceMemberRoleEnum.ContentCreator)
        {
            return _context.AutomationItems.IgnoreQueryFilters()
                .Where(item => item.AutomationPlan.WorkspaceId == workspaceId &&
                    !item.AutomationPlan.IsDeleted &&
                    item.Content != null &&
                    item.Content.WorkspaceId == workspaceId &&
                    item.Content.PrimaryCreatorId == scope.UserId);
        }

        return _context.AutomationItems.Where(_ => false);
    }

    private static Task<List<AutomationItem>> LoadVisibleItemsAsync(
        IQueryable<AutomationItem> query,
        CancellationToken cancellationToken)
        => query.AsNoTracking()
            .Include(item => item.Brand)
            .Include(item => item.Content)
            .Include(item => item.ContentCalendar)
            .OrderBy(item => item.RowIndex)
            .ThenBy(item => item.Platform)
            .ToListAsync(cancellationToken);
}
