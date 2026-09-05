using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed record ExecutionDecision(bool Allowed, string Code)
{
    public static ExecutionDecision Deny(string code) => new(false, code);
}

public interface IExecutionAuthorityPolicy
{
    Task<ExecutionDecision> CanDispatchAsync(string operation, CancellationToken ct);
    Task<ExecutionDecision> EvaluateAsync(ExecutionOperation operation, CancellationToken ct);
}

// No approved operation-to-authority mapping exists yet. Changing this policy requires
// an approved OQ-008 decision, enqueue authorization and execution/revoke tests.
public sealed class UnresolvedExecutionAuthorityPolicy : IExecutionAuthorityPolicy
{
    public Task<ExecutionDecision> CanDispatchAsync(string operation, CancellationToken ct) =>
        Task.FromResult(ExecutionDecision.Deny("BLOCKED_BY_BUSINESS_DECISION"));
    public Task<ExecutionDecision> EvaluateAsync(ExecutionOperation operation, CancellationToken ct) =>
        Task.FromResult(ExecutionDecision.Deny("BLOCKED_BY_BUSINESS_DECISION"));
}

public sealed class ExecutionAuthorizationService(AisamContext db, IExecutionAuthorityPolicy policy, IBackgroundJobHealthService? health = null)
{
    public async Task<ExecutionDecision> CanDispatchAsync(string operation, CancellationToken ct = default)
    {
        var decision = await policy.CanDispatchAsync(operation, ct);
        if (!decision.Allowed) ReportBlocked(operation, decision.Code);
        return decision;
    }
    public async Task<ExecutionDecision> CheckAsync(string resourceType, Guid referenceId, string action, CancellationToken ct = default)
    {
        var decision = await CheckCoreAsync(resourceType, referenceId, action, ct);
        if (!decision.Allowed) ReportBlocked(resourceType, decision.Code);
        return decision;
    }

    private void ReportBlocked(string operation, string code) => health?.ReportFailure(operation switch
    {
        "ScheduledPublish" or "ContentCalendar" => "ScheduledPosting",
        "AiGeneration" => "VideoPolling",
        "VideoGenerationJob" => "VideoGeneration",
        "AutomationPlan" => "AutomationGeneration",
        _ => operation
    }, code);

    private async Task<ExecutionDecision> CheckCoreAsync(string resourceType, Guid referenceId, string action, CancellationToken ct)
    {
        db.BackgroundAttribution = null;
        var operation = await db.Set<ExecutionOperation>().AsNoTracking().SingleOrDefaultAsync(
            o => o.ResourceType == resourceType && o.ReferenceId == referenceId && o.RequestedAction == action, ct);
        if (operation == null) return ExecutionDecision.Deny("EXECUTION_CONTEXT_REQUIRED");
        var invariant = await ValidateAsync(operation, ct);
        if (!invariant.Allowed) return invariant;
        var decision = await policy.EvaluateAsync(operation, ct);
        if (decision.Allowed && (operation.EnqueueAuthorizedAt == null || operation.ExecutionVersion <= 0 || operation.PolicyVersion <= 0))
            return ExecutionDecision.Deny("ENQUEUE_AUTHORIZATION_REQUIRED");
        if (decision.Allowed) db.BackgroundAttribution = operation;
        return decision;
    }

    // Validates data integrity, not the as-yet-unapproved current-actor vs approved-job policy.
    public async Task<ExecutionDecision> ValidateAsync(ExecutionOperation o, CancellationToken ct = default)
    {
        if (o.WorkspaceId == Guid.Empty || o.ActorUserId == Guid.Empty || o.ReferenceId == Guid.Empty ||
            !await db.Workspaces.AnyAsync(w => w.Id == o.WorkspaceId, ct) ||
            !await db.Users.AnyAsync(u => u.Id == o.ActorUserId, ct))
            return ExecutionDecision.Deny("INVALID_EXECUTION_IDENTITY");
        if (!o.TeamId.HasValue) return ExecutionDecision.Deny("TEAM_ATTRIBUTION_REQUIRED");
        if (!await db.Teams.AnyAsync(t => t.Id == o.TeamId && t.WorkspaceId == o.WorkspaceId && !t.IsDeleted, ct))
            return ExecutionDecision.Deny("INVALID_EXECUTION_TEAM");
        if (o.BrandId.HasValue && !await db.Brands.IgnoreQueryFilters().AnyAsync(b => b.Id == o.BrandId && b.WorkspaceId == o.WorkspaceId && !b.IsDeleted, ct))
            return ExecutionDecision.Deny("INVALID_EXECUTION_BRAND");
        if (o.IntegrationId.HasValue && (!o.BrandId.HasValue || !await db.SocialIntegrations.IgnoreQueryFilters().AnyAsync(
            i => i.Id == o.IntegrationId && i.WorkspaceId == o.WorkspaceId && i.BrandId == o.BrandId && i.IsActive && !i.IsDeleted, ct)))
            return ExecutionDecision.Deny("INVALID_EXECUTION_CHANNEL");

        var valid = o.ResourceType switch
        {
            "ContentCalendar" => await db.ContentCalendars.IgnoreQueryFilters().AnyAsync(s => s.Id == o.ReferenceId && s.ContentId == o.ResourceId &&
                s.WorkspaceId == o.WorkspaceId && !s.IsDeleted && s.IsActive && s.IntegrationId == o.IntegrationId &&
                s.Content.WorkspaceId == o.WorkspaceId && s.Content.BrandId == o.BrandId && !s.Content.IsDeleted, ct),
            "AiGeneration" => await db.AiGenerations.IgnoreQueryFilters().AnyAsync(g => g.Id == o.ReferenceId && g.ContentId == o.ResourceId && !g.IsDeleted &&
                g.Content.WorkspaceId == o.WorkspaceId && g.Content.BrandId == o.BrandId && !g.Content.IsDeleted, ct),
            "VideoGenerationJob" => await db.VideoGenerationJobs.AnyAsync(j => j.Id == o.ReferenceId && j.Id == o.ResourceId &&
                j.WorkspaceId == o.WorkspaceId && j.UserId == o.ActorUserId, ct),
            "AutomationPlan" => await db.AutomationPlans.AnyAsync(p => p.Id == o.ReferenceId && p.Id == o.ResourceId && p.WorkspaceId == o.WorkspaceId && !p.IsDeleted, ct) &&
                !await db.AutomationItems.IgnoreQueryFilters().AnyAsync(i => i.AutomationPlanId == o.ReferenceId &&
                    (i.Brand == null || i.Brand.WorkspaceId != o.WorkspaceId || i.Brand.IsDeleted ||
                     i.ProductId != null && (i.Product == null || i.Product.BrandId != i.BrandId) ||
                     i.ContentId != null && (i.Content == null || i.Content.WorkspaceId != o.WorkspaceId || i.Content.BrandId != i.BrandId || i.Content.IsDeleted) ||
                     i.ContentCalendarId != null && (i.ContentCalendar == null || i.ContentCalendar.WorkspaceId != o.WorkspaceId ||
                        i.ContentCalendar.ContentId != i.ContentId || i.ContentCalendar.Integration == null ||
                        i.ContentCalendar.Integration.WorkspaceId != o.WorkspaceId || i.ContentCalendar.Integration.BrandId != i.BrandId)), ct),
            _ => false
        };
        return valid ? new(true, "INVARIANTS_VALID") : ExecutionDecision.Deny("INVALID_EXECUTION_RESOURCE");
    }
}
