using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        ApplyBackgroundAttribution();
        await CaptureExecutionAttributionAsync(cancellationToken);
        foreach (var entry in ChangeTracker.Entries<Workspace>().Where(e => e.State == EntityState.Added).ToArray())
        {
            if (Teams.Local.Any(t => t.WorkspaceId == entry.Entity.Id)) continue;
            var team = new Team { WorkspaceId = entry.Entity.Id, Workspace = entry.Entity, Name = entry.Entity.Name };
            foreach (var member in entry.Entity.Members)
                team.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = member.UserId, Role = member.Role.ToString() });
            Teams.Add(team);
        }
        foreach (var entry in ChangeTracker.Entries<WorkspaceMember>().Where(e => e.State == EntityState.Modified).ToArray())
        {
            var member = entry.Entity;
            if (!entry.Property(m => m.Role).IsModified && !entry.Property(m => m.IsActive).IsModified) continue;
            var links = await TeamMembers.Where(t => t.UserId == member.UserId && t.Team.WorkspaceId == member.WorkspaceId).ToListAsync(cancellationToken);
            foreach (var link in links)
            {
                link.Role = member.Role.ToString();
                if (!member.IsActive) link.IsActive = false;
            }
        }
        if (AccessScope.Enforced)
        {
            await ValidateAccessLinksAsync(cancellationToken);
            foreach (var entry in ChangeTracker.Entries<Content>().Where(e => e.State == EntityState.Added))
            {
                if (entry.Entity.WorkspaceId != AccessScope.WorkspaceId) throw new UnauthorizedAccessException("Content workspace mismatch.");
                entry.Entity.PrimaryCreatorId = AccessScope.UserId;
                AccessScope.HistoricalContentIds = AccessScope.HistoricalContentIds.Append(entry.Entity.Id).Distinct().ToArray();
                AccessScope.EditableContentIds = AccessScope.EditableContentIds.Append(entry.Entity.Id).Distinct().ToArray();
            }
            foreach (var entry in ChangeTracker.Entries<Content>().Where(e => e.State == EntityState.Modified))
            {
                if (entry.Property(c => c.PrimaryCreatorId).IsModified) throw new UnauthorizedAccessException("Creator attribution is immutable.");
                if (entry.Entity.WorkspaceId != AccessScope.WorkspaceId || !AccessScope.IsOwner && !AccessScope.EditableContentIds.Contains(entry.Entity.Id))
                    throw new UnauthorizedAccessException("Content mutation is outside the authorized scope.");
            }
            foreach (var entry in ChangeTracker.Entries<CreditUsageRecord>().Where(e => e.State == EntityState.Added))
                await AttributeCreditEventAsync(entry.Entity, cancellationToken);

            var auditedTypes = new[] { typeof(Content), typeof(WorkspaceMember), typeof(WorkspaceInvitation), typeof(Team), typeof(TeamMember), typeof(TeamBrand), typeof(TeamChannelAccess), typeof(CollaborationTask), typeof(TemporaryAccessGrant), typeof(CreditUsageRecord) };
            foreach (var entry in ChangeTracker.Entries().Where(e => auditedTypes.Contains(e.Entity.GetType()) && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray())
            {
                AuditLogs.Add(new AuditLog
                {
                    ActorId = AccessScope.UserId, WorkspaceId = AccessScope.WorkspaceId,
                    RequestedBy = AccessScope.UserId, TeamId = AccessScope.ActiveTeamId,
                    AffectedUserId = entry.Entity is CreditUsageRecord credit ? credit.UserId :
                        entry.Entity is CollaborationTask task ? task.AssigneeId :
                        entry.Entity is WorkspaceMember member ? member.UserId :
                        entry.Entity is TeamMember teamMember ? teamMember.UserId : null,
                    TargetId = (Guid)entry.Property("Id").CurrentValue!, TargetTable = entry.Metadata.GetTableName()!,
                    ActionType = $"{entry.State.ToString().ToUpperInvariant()}_{entry.Metadata.ClrType.Name}",
                    // No arbitrary payloads, credentials or resource text in audit logs.
                    Notes = "Authorized workspace change"
                });
            }
        }
        return await SaveWithMutationGuardAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
