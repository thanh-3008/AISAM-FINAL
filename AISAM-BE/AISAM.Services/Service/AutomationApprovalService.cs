using System.Net;
using System.Text.Json;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class AutomationApprovalService : IAutomationApprovalService
{
    private readonly AisamContext _context;
    private readonly IContentScheduleService _scheduleService;

    public AutomationApprovalService(AisamContext context, IContentScheduleService scheduleService)
    {
        _context = context;
        _scheduleService = scheduleService;
    }

    public async Task<GenericResponse<AutomationPlanDto>> ApproveAsync(Guid workspaceId, Guid planId, Guid approverUserId, Guid? itemId = null, IReadOnlyCollection<Guid>? integrationIds = null, CancellationToken cancellationToken = default)
    {
        if (integrationIds is { Count: > 0 } && !itemId.HasValue)
            return GenericResponse<AutomationPlanDto>.CreateError("Page targets can only be selected for one automation item at a time.");
        var plan = await LoadPlanAsync(workspaceId, planId, cancellationToken);
        if (plan is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        var items = plan.Items.Where(item => (!itemId.HasValue || item.Id == itemId) &&
            (item.Status == AutomationItemStatusEnum.AwaitingApproval ||
             (item.Status == AutomationItemStatusEnum.NeedsAttention && item.ContentId.HasValue && IsSchedulingAttention(item.LastError)))).ToList();
        if (items.Count == 0) return GenericResponse<AutomationPlanDto>.CreateError("No item is awaiting approval.");

        plan.Status = AutomationPlanStatusEnum.Scheduling;
        await _context.SaveChangesAsync(cancellationToken);
        foreach (var item in items)
        {
            var content = item.Content;
            if (content is null)
            {
                item.Status = AutomationItemStatusEnum.NeedsAttention;
                item.LastError = "Generated content was not found.";
                continue;
            }
            var platform = ParsePlatform(item.Platform);
            var availableIntegrations = await _context.SocialIntegrations
                .Where(value => value.WorkspaceId == workspaceId && value.BrandId == item.BrandId &&
                                value.Platform == platform && value.IsActive && !value.IsDeleted &&
                                (!value.ExpiresAt.HasValue || value.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(value => value.UpdatedAt)
                .ToListAsync(cancellationToken);
            if (availableIntegrations.Count == 0)
            {
                item.Status = AutomationItemStatusEnum.NeedsAttention;
                item.LastError = $"No active {item.Platform} integration is linked to brand '{item.Brand?.Name ?? item.BrandId.ToString()}'. Open Social Accounts and link this platform to the same brand, then retry scheduling.";
                continue;
            }
            if (integrationIds is null && availableIntegrations.Count > 1)
            {
                item.Status = AutomationItemStatusEnum.NeedsAttention;
                item.LastError = $"No active single {item.Platform} target can be selected automatically because brand '{item.Brand?.Name ?? item.BrandId.ToString()}' has multiple linked pages. Select the pages you want to publish to.";
                continue;
            }
            var selectedIntegrations = integrationIds is { Count: > 0 }
                ? availableIntegrations.Where(value => integrationIds.Contains(value.Id)).ToList()
                : [availableIntegrations[0]];
            if (selectedIntegrations.Count == 0 || (integrationIds is { Count: > 0 } && selectedIntegrations.Count != integrationIds.Distinct().Count()))
            {
                item.Status = AutomationItemStatusEnum.NeedsAttention;
                item.LastError = "One or more selected pages are inactive or do not belong to this brand and platform.";
                continue;
            }

            content.Status = ContentStatusEnum.Approved;
            item.Status = AutomationItemStatusEnum.Approved;
            item.LastError = null;
            if (!await _context.Approvals.AnyAsync(value => value.ContentId == content.Id && value.ApproverUserId == approverUserId && value.Status == ContentStatusEnum.Approved && !value.IsDeleted, cancellationToken))
            {
                _context.Approvals.Add(new Approval { ContentId = content.Id, ApproverProfileId = plan.ProfileId, ApproverUserId = approverUserId, Status = ContentStatusEnum.Approved, ApprovedAt = DateTime.UtcNow, Notes = "Approved from Automation Plan" });
            }
            await _context.SaveChangesAsync(cancellationToken);

            var scheduleErrors = new List<string>();
            foreach (var integration in selectedIntegrations)
            {
                var targetName = string.IsNullOrWhiteSpace(integration.TargetName)
                    ? integration.ExternalId ?? integration.Id.ToString()
                    : integration.TargetName;

                try
                {
                    var existing = await _context.ContentCalendars.Where(value => value.ContentId == content.Id && value.IntegrationId == integration.Id && value.IsActive && !value.IsDeleted).OrderBy(value => value.Id).FirstOrDefaultAsync(cancellationToken);
                    if (existing is not null)
                    {
                        item.ContentCalendarId ??= existing.Id;
                        continue;
                    }

                    var scheduled = await _scheduleService.CreateInWorkspaceAsync(workspaceId, plan.ProfileId,
                        new CreateContentScheduleRequest { ContentId = content.Id, IntegrationId = integration.Id, ScheduledAt = item.ScheduledAt }, cancellationToken);
                    if (scheduled.Success && scheduled.Data is not null) item.ContentCalendarId ??= scheduled.Data.Id;
                    else scheduleErrors.Add(scheduled.Message ?? $"Unable to schedule page {targetName}.");
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    scheduleErrors.Add($"Scheduling page {targetName} failed because the database connection timed out. Retry this item.");
                }
                catch (Exception ex)
                {
                    scheduleErrors.Add($"Scheduling page {targetName} failed: {ex.GetBaseException().Message}");
                }
            }
            item.Status = scheduleErrors.Count == 0 ? AutomationItemStatusEnum.Scheduled : AutomationItemStatusEnum.NeedsAttention;
            item.LastError = scheduleErrors.Count == 0 ? null : string.Join(" ", scheduleErrors);
        }
        await FinishPlanAsync(plan, cancellationToken);
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(plan), "Approval and scheduling completed.");
    }

    public async Task<GenericResponse<AutomationPlanDto>> RejectAsync(Guid workspaceId, Guid planId, Guid itemId, Guid approverUserId, string? notes = null, CancellationToken cancellationToken = default)
    {
        var plan = await LoadPlanAsync(workspaceId, planId, cancellationToken);
        var item = plan?.Items.FirstOrDefault(value => value.Id == itemId);
        if (plan is null || item is null) return GenericResponse<AutomationPlanDto>.CreateError("Automation item not found.", HttpStatusCode.NotFound);
        if (item.Status != AutomationItemStatusEnum.AwaitingApproval) return GenericResponse<AutomationPlanDto>.CreateError("Only an item awaiting approval can be rejected.");
        item.Status = AutomationItemStatusEnum.Rejected;
        item.LastError = string.IsNullOrWhiteSpace(notes) ? "Rejected by reviewer." : notes.Trim();
        if (item.Content is not null)
        {
            item.Content.Status = ContentStatusEnum.Rejected;
            _context.Approvals.Add(new Approval { ContentId = item.Content.Id, ApproverProfileId = plan.ProfileId, ApproverUserId = approverUserId, Status = ContentStatusEnum.Rejected, Notes = item.LastError });
        }
        await FinishPlanAsync(plan, cancellationToken);
        return GenericResponse<AutomationPlanDto>.CreateSuccess(Map(plan), "Automation item rejected.");
    }

    public async Task<GenericResponse<IReadOnlyList<AutomationTargetDto>>> GetTargetsAsync(Guid workspaceId, Guid planId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _context.AutomationItems.Include(value => value.AutomationPlan).Include(value => value.Brand)
            .FirstOrDefaultAsync(value => value.Id == itemId && value.AutomationPlanId == planId && value.AutomationPlan.WorkspaceId == workspaceId, cancellationToken);
        if (item is null) return GenericResponse<IReadOnlyList<AutomationTargetDto>>.CreateError("Automation item not found.", HttpStatusCode.NotFound);
        var platform = ParsePlatform(item.Platform);
        var integrations = await _context.SocialIntegrations.AsNoTracking()
            .Where(value => value.WorkspaceId == workspaceId && value.BrandId == item.BrandId && value.Platform == platform && value.IsActive && !value.IsDeleted && (!value.ExpiresAt.HasValue || value.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(value => value.UpdatedAt).ToListAsync(cancellationToken);
        var schedules = item.ContentId.HasValue
            ? await _context.ContentCalendars.AsNoTracking().Where(value => value.ContentId == item.ContentId && value.IntegrationId.HasValue && value.IsActive && !value.IsDeleted).ToListAsync(cancellationToken)
            : [];
        IReadOnlyList<AutomationTargetDto> result = integrations.Select(value =>
        {
            var schedule = schedules.FirstOrDefault(entry => entry.IntegrationId == value.Id);
            return new AutomationTargetDto { IntegrationId = value.Id, Platform = value.Platform.ToString(), ExternalId = value.ExternalId, Name = $"{value.Platform} · {value.ExternalId ?? value.Id.ToString()}", IsScheduled = schedule is not null, ScheduleId = schedule?.Id };
        }).ToList();
        return GenericResponse<IReadOnlyList<AutomationTargetDto>>.CreateSuccess(result);
    }

    private static bool IsSchedulingAttention(string? error) => error?.StartsWith("No active ") == true || error?.StartsWith("Multiple active ") == true || error?.StartsWith("One or more selected ") == true;

    private async Task<AutomationPlan?> LoadPlanAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken) =>
        await _context.AutomationPlans.Include(value => value.Items).ThenInclude(value => value.Content)
            .Include(value => value.Items).ThenInclude(value => value.Brand)
            .FirstOrDefaultAsync(value => value.Id == planId && value.WorkspaceId == workspaceId && !value.IsDeleted, cancellationToken);

    private async Task FinishPlanAsync(AutomationPlan plan, CancellationToken cancellationToken)
    {
        var unfinished = plan.Items.Any(value => value.Status is AutomationItemStatusEnum.Pending or AutomationItemStatusEnum.GeneratingText or AutomationItemStatusEnum.GeneratingMedia or AutomationItemStatusEnum.AwaitingApproval or AutomationItemStatusEnum.Approved);
        if (unfinished) plan.Status = plan.Items.Any(value => value.Status == AutomationItemStatusEnum.AwaitingApproval) ? AutomationPlanStatusEnum.AwaitingApproval : AutomationPlanStatusEnum.Scheduling;
        else if (plan.Items.All(value => value.Status is AutomationItemStatusEnum.Scheduled or AutomationItemStatusEnum.Published)) plan.Status = AutomationPlanStatusEnum.Completed;
        else plan.Status = AutomationPlanStatusEnum.PartiallyFailed;
        plan.FailedItems = plan.Items.Count(value => value.Status is AutomationItemStatusEnum.GenerationFailed or AutomationItemStatusEnum.NeedsAttention or AutomationItemStatusEnum.Rejected or AutomationItemStatusEnum.PublishFailed);
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static SocialPlatformEnum ParsePlatform(string platform) => platform.ToLowerInvariant() switch
    {
        "facebook" => SocialPlatformEnum.Facebook,
        "instagram" => SocialPlatformEnum.Instagram,
        "tiktok" => SocialPlatformEnum.TikTok,
        _ => throw new InvalidOperationException($"Unsupported platform: {platform}")
    };

    private static AutomationPlanDto Map(AutomationPlan plan) => new()
    {
        Id = plan.Id, Name = plan.Name, SourceFileName = plan.SourceFileName, Timezone = plan.Timezone, Status = plan.Status.ToString(),
        TotalItems = plan.TotalItems, ValidItems = plan.ValidItems, FailedItems = plan.FailedItems, EstimatedCredits = plan.EstimatedCredits,
        ReservedCredits = plan.ReservedCredits, UsedCredits = plan.UsedCredits, ReleasedCredits = plan.ReleasedCredits, CreatedAt = plan.CreatedAt, ConfirmedAt = plan.ConfirmedAt,
        AutoApprove = plan.AutoApprove, TemplateSourcePlanId = plan.TemplateSourcePlanId,
        Items = plan.Items.OrderBy(value => value.RowIndex).ThenBy(value => value.Platform).Select(item => new AutomationItemDto
        {
            Id = item.Id, RowIndex = item.RowIndex, Platform = item.Platform, BrandId = item.BrandId, BrandName = item.Brand?.Name ?? string.Empty,
            ProductId = item.ProductId, ContentId = item.ContentId, ContentCalendarId = item.ContentCalendarId, Topic = item.Topic, Objective = item.Objective,
            ContentType = item.RequestedContentType.ToString(), Tone = item.Tone, Cta = item.Cta, Notes = item.Notes, ScheduledAt = item.ScheduledAt,
            Status = item.Status.ToString(), EstimatedCredits = item.EstimatedCredits, UsedCredits = item.UsedCredits, GenerationAttemptCount = item.GenerationAttemptCount,
            LastError = item.LastError, GeneratedText = item.Content?.TextContent, GeneratedImageUrl = FirstImage(item.Content?.ImageUrl), GeneratedVideoUrl = item.Content?.VideoUrl,
            VideoProvider = item.VideoProvider, ValidationErrors = DeserializeErrors(item.ValidationErrors)
        }).ToList()
    };

    private static string? FirstImage(string? json) { try { return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<List<string>>(json)?.FirstOrDefault(); } catch { return null; } }
    private static IReadOnlyList<string> DeserializeErrors(string? json) { try { return string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<string>>(json) ?? []; } catch { return json is null ? [] : [json]; } }
}
