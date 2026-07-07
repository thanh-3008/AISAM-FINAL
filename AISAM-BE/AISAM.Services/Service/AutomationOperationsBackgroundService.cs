using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class AutomationOperationsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationOperationsBackgroundService> _logger;

    public AutomationOperationsBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AutomationOperationsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AisamContext>();
                var approval = scope.ServiceProvider.GetRequiredService<IAutomationApprovalService>();

                var autoPlans = await context.AutomationPlans.AsNoTracking()
                    .Where(plan => plan.AutoApprove && plan.Status == AutomationPlanStatusEnum.AwaitingApproval && !plan.IsDeleted)
                    .Select(plan => new { plan.Id, plan.WorkspaceId, plan.ProfileId })
                    .Take(5).ToListAsync(stoppingToken);
                foreach (var plan in autoPlans)
                {
                    var userId = await context.Profiles.Where(profile => profile.Id == plan.ProfileId).Select(profile => profile.UserId).FirstOrDefaultAsync(stoppingToken);
                    if (userId != Guid.Empty) await approval.ApproveAsync(plan.WorkspaceId, plan.Id, userId, cancellationToken: stoppingToken);
                }

                var changedItems = await context.AutomationItems
                    .Include(item => item.AutomationPlan)
                    .Include(item => item.ContentCalendar)
                    .Where(item => item.ContentCalendarId != null && item.Status == AutomationItemStatusEnum.Scheduled &&
                                   item.ContentCalendar != null &&
                                   (item.ContentCalendar.Status == ScheduleStatusEnum.Completed || item.ContentCalendar.Status == ScheduleStatusEnum.Failed))
                    .Take(100).ToListAsync(stoppingToken);

                var cancelledSchedules = await context.AutomationItems
                    .Include(item => item.AutomationPlan)
                    .Include(item => item.ContentCalendar)
                    .Where(item => item.ContentCalendarId != null && item.Status == AutomationItemStatusEnum.Scheduled &&
                                   item.ContentCalendar != null && (item.ContentCalendar.IsDeleted || !item.ContentCalendar.IsActive))
                    .Take(100).ToListAsync(stoppingToken);
                foreach (var item in cancelledSchedules)
                {
                    var replacement = item.ContentId.HasValue
                        ? await context.ContentCalendars.FirstOrDefaultAsync(value => value.ContentId == item.ContentId && value.IsActive && !value.IsDeleted, stoppingToken)
                        : null;
                    if (replacement is not null)
                    {
                        item.ContentCalendarId = replacement.Id;
                        continue;
                    }
                    item.ContentCalendarId = null;
                    item.Status = AutomationItemStatusEnum.AwaitingApproval;
                    item.LastError = "The previous schedule was cancelled. Approve the item to schedule it again.";
                    item.UpdatedAt = DateTime.UtcNow;
                    item.AutomationPlan.Status = AutomationPlanStatusEnum.AwaitingApproval;
                    item.AutomationPlan.UpdatedAt = DateTime.UtcNow;
                }
                foreach (var item in changedItems)
                {
                    item.Status = item.ContentCalendar!.Status == ScheduleStatusEnum.Completed
                        ? AutomationItemStatusEnum.Published : AutomationItemStatusEnum.PublishFailed;
                    item.LastError = item.ContentCalendar.Status == ScheduleStatusEnum.Failed ? item.ContentCalendar.LastError : null;
                    item.UpdatedAt = DateTime.UtcNow;
                }
                foreach (var plan in changedItems.Select(item => item.AutomationPlan).Distinct())
                {
                    await context.Entry(plan).Collection(value => value.Items).LoadAsync(stoppingToken);
                    plan.Status = plan.Items.Any(item => item.Status is AutomationItemStatusEnum.PublishFailed or AutomationItemStatusEnum.NeedsAttention or AutomationItemStatusEnum.GenerationFailed)
                        ? AutomationPlanStatusEnum.PartiallyFailed
                        : plan.Items.All(item => item.Status == AutomationItemStatusEnum.Published)
                            ? AutomationPlanStatusEnum.Completed : plan.Status;
                    plan.FailedItems = plan.Items.Count(item => item.Status is AutomationItemStatusEnum.PublishFailed or AutomationItemStatusEnum.NeedsAttention or AutomationItemStatusEnum.GenerationFailed or AutomationItemStatusEnum.Rejected);
                    plan.UpdatedAt = DateTime.UtcNow;
                }
                if (changedItems.Count > 0 || cancelledSchedules.Count > 0) await context.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { _logger.LogError(exception, "Automation operations worker iteration failed."); }

            try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
