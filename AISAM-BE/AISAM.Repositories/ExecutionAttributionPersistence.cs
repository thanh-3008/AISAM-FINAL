using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    // An attribution context is deliberately separate from AccessScope/authority.
    public ExecutionOperation? BackgroundAttribution { get; set; }

    private void ApplyBackgroundAttribution()
    {
        var operation = BackgroundAttribution;
        if (operation == null) return;
        foreach (var entry in ChangeTracker.Entries<Content>().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (entry.Entity.WorkspaceId != operation.WorkspaceId) throw new UnauthorizedAccessException("Background content workspace mismatch.");
            if (entry.State == EntityState.Added) entry.Entity.PrimaryCreatorId = operation.ActorUserId;
            else if (entry.Property(c => c.PrimaryCreatorId).IsModified) throw new UnauthorizedAccessException("Creator attribution is immutable.");
        }
        foreach (var entry in ChangeTracker.Entries<CreditUsageRecord>().Where(e => e.State == EntityState.Added))
        {
            var record = entry.Entity;
            if (record.WorkspaceId != operation.WorkspaceId || record.UserId != operation.ActorUserId)
                throw new UnauthorizedAccessException("Background credit attribution mismatch.");
            if (record.TeamId.HasValue && record.TeamId != operation.TeamId)
                throw new UnauthorizedAccessException("Background credit team mismatch.");
            record.TeamId = operation.TeamId;
            record.BrandId ??= operation.BrandId;
            record.IntegrationId ??= operation.IntegrationId;
            record.ReferenceId ??= operation.ReferenceId;
        }
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified &&
            e.Entity is Content or CreditUsageRecord or AiGeneration or VideoGenerationJob or ContentCalendar or AutomationItem).ToArray())
            AuditLogs.Add(new AuditLog { ActorId = operation.ActorUserId, RequestedBy = operation.ActorUserId,
                ApprovedBy = operation.ApprovedBy, ExecutedBySystem = true, TeamId = operation.TeamId,
                WorkspaceId = operation.WorkspaceId, ReferenceId = operation.ReferenceId,
                AffectedUserId = entry.Entity is CreditUsageRecord credit ? credit.UserId : null,
                TargetId = (Guid)entry.Property("Id").CurrentValue!, TargetTable = entry.Metadata.GetTableName()!,
                ActionType = $"BACKGROUND_{entry.State.ToString().ToUpperInvariant()}", Notes = "Background operation state change" });
    }
    private readonly HashSet<Guid> capturedExecutionIds = new();

    private async Task CaptureExecutionAttributionAsync(CancellationToken ct)
    {
        // Existing legacy rows stay unattributed. Neither profile nor current owner is a
        // reliable substitute for the requester who originally enqueued an operation.
        foreach (var entry in ChangeTracker.Entries<ExecutionOperation>().Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted))
            throw new InvalidOperationException("Execution attribution is immutable; enqueue a new operation version.");
        if (!AccessScope.Enforced) return;
        if (ChangeTracker.Entries<ExecutionOperation>().Any(e => e.State == EntityState.Added && !capturedExecutionIds.Contains(e.Entity.Id)))
            throw new UnauthorizedAccessException("Execution attribution must be captured by the server.");
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToArray())
        {
            Guid workspace; Guid resource; Guid reference; Guid? brand = null; Guid? integration = null;
            string action;
            switch (entry.Entity)
            {
                case ContentCalendar schedule:
                    workspace = schedule.WorkspaceId; resource = schedule.ContentId; reference = schedule.Id;
                    integration = schedule.IntegrationId; action = "Publish"; break;
                case AiGeneration generation:
                    workspace = AccessScope.WorkspaceId; resource = generation.ContentId; reference = generation.Id;
                    action = "AiGenerate"; break;
                case VideoGenerationJob video:
                    if (video.UserId != AccessScope.UserId) throw new UnauthorizedAccessException("Execution actor mismatch.");
                    workspace = video.WorkspaceId; resource = reference = video.Id; action = "AiGenerate"; break;
                case AutomationPlan plan:
                    workspace = plan.WorkspaceId; resource = reference = plan.Id; action = "AutomationExecute"; break;
                default: continue;
            }
            if (workspace != AccessScope.WorkspaceId) throw new UnauthorizedAccessException("Execution workspace mismatch.");
            if (entry.Entity is ContentCalendar or AiGeneration)
            {
                var content = Contents.Local.FirstOrDefault(c => c.Id == resource) ??
                    await Contents.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(c => c.Id == resource, ct);
                if (content == null || content.WorkspaceId != workspace || content.IsDeleted)
                    throw new UnauthorizedAccessException("Execution resource mismatch.");
                brand = content.BrandId;
                if (integration.HasValue && !await SocialIntegrations.IgnoreQueryFilters().AnyAsync(i => i.Id == integration &&
                    i.WorkspaceId == workspace && i.BrandId == brand && i.IsActive && !i.IsDeleted, ct))
                    throw new UnauthorizedAccessException("Execution channel mismatch.");
            }
            if (AccessScope.ActiveTeamId.HasValue && !await TeamMembers.AnyAsync(m => m.TeamId == AccessScope.ActiveTeamId &&
                m.Team.WorkspaceId == workspace && !m.Team.IsDeleted && m.UserId == AccessScope.UserId && m.IsActive, ct))
                throw new UnauthorizedAccessException("Execution team mismatch.");
            var stampKey = entry.Entity is ContentCalendar
                ? $"Content:{resource}:Schedule:{integration}"
                : entry.Entity is AiGeneration ? $"Content:{resource}:AiGenerate:" : "";
            mutationStamps.TryGetValue(stampKey, out var stamp);
            if (stamp == null && entry.Entity is AiGeneration)
                stamp = mutationStamps.GetValueOrDefault($"Content:{resource}:AiGenerateImage:") ??
                    mutationStamps.GetValueOrDefault($"Content:{resource}:AiGenerateVideo:");
            if (stamp == null && entry.Entity is AutomationPlan)
                stamp = mutationStamps.GetValueOrDefault("Workspace:AutomationManage");
            // Without an action stamp, this snapshot cannot authorize later execution.
            // Non-Owner callers cannot enqueue an operation through such a path.
            if (stamp == null && !AccessScope.IsOwner)
                throw new UnauthorizedAccessException("Enqueue action authorization is required.");
            var captured = new ExecutionOperation
            {
                WorkspaceId = workspace, ActorUserId = AccessScope.UserId, TeamId = AccessScope.ActiveTeamId,
                ResourceId = resource, ResourceType = entry.Entity.GetType().Name, ReferenceId = reference,
                BrandId = brand, IntegrationId = integration, RequestedAction = action,
                EnqueueAuthorizedAt = stamp?.AuthorizedAt
            };
            capturedExecutionIds.Add(captured.Id);
            Set<ExecutionOperation>().Add(captured);
        }
    }

    private async Task AttributeCreditEventAsync(CreditUsageRecord record, CancellationToken ct)
    {
        if (record.WorkspaceId != AccessScope.WorkspaceId) throw new UnauthorizedAccessException("Credit workspace mismatch.");
        // Do not stamp an administrator's active Team onto another user's transaction.
        if (record.UserId == AccessScope.UserId) record.TeamId ??= AccessScope.ActiveTeamId;
        if (record.TeamId.HasValue && !await TeamMembers.AnyAsync(m => m.TeamId == record.TeamId && m.Team.WorkspaceId == record.WorkspaceId &&
            m.UserId == record.UserId && m.IsActive && !m.Team.IsDeleted, ct))
            throw new UnauthorizedAccessException("Credit team attribution mismatch.");
        record.ReferenceId ??= record.AiGenerationId;
    }
}
