using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    public DbSet<TeamChannelAccess> TeamChannelAccesses => Set<TeamChannelAccess>();
    public DbSet<CollaborationTask> CollaborationTasks => Set<CollaborationTask>();
    public DbSet<ContentParticipation> ContentParticipations => Set<ContentParticipation>();
    public DbSet<TemporaryAccessGrant> TemporaryAccessGrants => Set<TemporaryAccessGrant>();

    private void ConfigureAccessControl(ModelBuilder model)
    {
        model.Entity<ExecutionOperation>().HasIndex(o => new { o.ResourceType, o.ReferenceId, o.RequestedAction }).IsUnique();
        model.Entity<ExecutionOperation>().HasIndex(o => new { o.WorkspaceId, o.TeamId, o.CreatedAt });
        model.Entity<ExecutionOperation>().HasOne<Workspace>().WithMany().HasForeignKey(o => o.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ExecutionOperation>().HasOne<User>().WithMany().HasForeignKey(o => o.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Team>().HasAlternateKey(t => new { t.Id, t.WorkspaceId });
        model.Entity<ExecutionOperation>().HasOne<Team>().WithMany().HasForeignKey(o => new { o.TeamId, o.WorkspaceId })
            .HasPrincipalKey(t => new { t.Id, t.WorkspaceId }).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Team>().HasOne(t => t.Workspace).WithMany().HasForeignKey(t => t.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        // Keep legacy unassigned-content reads compatible with EF. Composite Content
        // references are enforced by PostgreSQL migration using this unique index.
        model.Entity<Content>().HasIndex(c => new { c.Id, c.WorkspaceId }).IsUnique();
        model.Entity<CollaborationTask>().HasAlternateKey(t => new { t.Id, t.WorkspaceId });
        model.Entity<CollaborationTask>().HasOne<Team>().WithMany().HasForeignKey(t => new { t.TeamId, t.WorkspaceId })
            .HasPrincipalKey(t => new { t.Id, t.WorkspaceId }).OnDelete(DeleteBehavior.Restrict);
        model.Entity<TemporaryAccessGrant>().HasOne<CollaborationTask>().WithMany().HasForeignKey(g => new { g.TaskId, g.WorkspaceId })
            .HasPrincipalKey(t => new { t.Id, t.WorkspaceId }).OnDelete(DeleteBehavior.Restrict);
        // A single transactional SaveChanges owns task transition + audit + notifications.
        // Concurrent expiry or extension must win once; losing writers roll back atomically.
        model.Entity<CollaborationTask>().Property(t => t.UpdatedAt).IsConcurrencyToken();
        model.Entity<TemporaryAccessGrant>().Property(g => g.RevokedAt).IsConcurrencyToken();
        model.Entity<TeamMember>().HasIndex(t => new { t.TeamId, t.UserId }).IsUnique();
        model.Entity<TeamBrand>().HasIndex(t => new { t.TeamId, t.BrandId }).IsUnique();
        model.Entity<TeamChannelAccess>().HasIndex(t => new { t.TeamBrandId, t.IntegrationId }).IsUnique();
        model.Entity<TeamChannelAccess>().HasOne(t => t.TeamBrand).WithMany(t => t.Channels).HasForeignKey(t => t.TeamBrandId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<TeamChannelAccess>().HasOne(t => t.Integration).WithMany().HasForeignKey(t => t.IntegrationId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Content>().HasIndex(c => new { c.WorkspaceId, c.PrimaryCreatorId });
        model.Entity<ContentParticipation>().HasIndex(p => new { p.ContentId, p.UserId }).IsUnique();
        model.Entity<ContentParticipation>().HasOne(p => p.Content).WithMany(c => c.Participations).HasForeignKey(p => p.ContentId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CollaborationTask>().HasOne(t => t.Content).WithMany().HasForeignKey(t => t.ContentId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CollaborationTask>().HasOne(t => t.Team).WithMany().HasForeignKey(t => t.TeamId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CollaborationTask>().HasIndex(t => new { t.WorkspaceId, t.AssigneeId, t.Status });
        model.Entity<TemporaryAccessGrant>().HasOne(g => g.Task).WithMany().HasForeignKey(g => g.TaskId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<TemporaryAccessGrant>().HasIndex(g => new { g.UserId, g.ExpiresAt });
        model.Entity<CreditUsageRecord>().HasIndex(r => new { r.WorkspaceId, r.TeamId, r.CreatedAt });

        // Every predicate retains workspace isolation, including historical OWN/ASSIGNED reads.
        model.Entity<Content>().HasQueryFilter(c => !AccessScope.Enforced ||
            c.WorkspaceId == AccessScope.WorkspaceId && (AccessScope.IsOwner ||
                (AccessScope.IsWrite ? AccessScope.EditableContentIds.Contains(c.Id) :
                    AccessScope.IsCreator ? AccessScope.HistoricalContentIds.Contains(c.Id) : AccessScope.BrandIds.Contains(c.BrandId))));
        model.Entity<Brand>().HasQueryFilter(b => !AccessScope.Enforced ||
            b.WorkspaceId == AccessScope.WorkspaceId && (AccessScope.IsOwner || AccessScope.BrandIds.Contains(b.Id)));
        model.Entity<Product>().HasQueryFilter(p => !AccessScope.Enforced ||
            p.Brand.WorkspaceId == AccessScope.WorkspaceId && (AccessScope.IsOwner || AccessScope.BrandIds.Contains(p.BrandId)));
        model.Entity<SocialIntegration>().HasQueryFilter(i => !AccessScope.Enforced ||
            i.WorkspaceId == AccessScope.WorkspaceId && (AccessScope.IsOwner || AccessScope.IntegrationIds.Contains(i.Id)));
        model.Entity<SocialAccount>().HasQueryFilter(a => !AccessScope.Enforced ||
            a.WorkspaceId == AccessScope.WorkspaceId && (AccessScope.IsOwner || a.SocialIntegrations.Any(i => AccessScope.IntegrationIds.Contains(i.Id))));
        model.Entity<Post>().HasQueryFilter(p => !AccessScope.Enforced ||
            p.Content.WorkspaceId == AccessScope.WorkspaceId && p.Integration.WorkspaceId == AccessScope.WorkspaceId &&
            (AccessScope.IsOwner || (AccessScope.IsCreator ? AccessScope.HistoricalContentIds.Contains(p.ContentId) :
                AccessScope.BrandIds.Contains(p.Content.BrandId) && AccessScope.IntegrationIds.Contains(p.IntegrationId))));
        model.Entity<PerformanceReport>().HasQueryFilter(r => !AccessScope.Enforced || r.Post != null &&
            r.Post.Content.WorkspaceId == AccessScope.WorkspaceId && r.Post.Integration.WorkspaceId == AccessScope.WorkspaceId &&
            (AccessScope.IsOwner || (AccessScope.IsCreator ? AccessScope.HistoricalContentIds.Contains(r.Post.ContentId) :
                AccessScope.BrandIds.Contains(r.Post.Content.BrandId) && AccessScope.IntegrationIds.Contains(r.Post.IntegrationId))));
        model.Entity<CreditUsageRecord>().HasQueryFilter(r => !AccessScope.Enforced || r.WorkspaceId == AccessScope.WorkspaceId &&
            (AccessScope.IsOwner || AccessScope.Role == WorkspaceMemberRoleEnum.ContentCreator && r.UserId == AccessScope.UserId ||
             AccessScope.Role == WorkspaceMemberRoleEnum.Manager && r.TeamId.HasValue && AccessScope.TeamIds.Contains(r.TeamId.Value)));
        model.Entity<AdCampaign>().HasQueryFilter(c => !AccessScope.Enforced || c.WorkspaceId == AccessScope.WorkspaceId &&
            (AccessScope.IsOwner || AccessScope.BrandIds.Contains(c.BrandId)));
        model.Entity<ContentCalendar>().HasQueryFilter(c => !AccessScope.Enforced || c.WorkspaceId == AccessScope.WorkspaceId &&
            (AccessScope.IsOwner || (AccessScope.IsCreator ? AccessScope.HistoricalContentIds.Contains(c.ContentId) :
                Contents.Any(content => content.Id == c.ContentId && content.WorkspaceId == AccessScope.WorkspaceId &&
                    AccessScope.BrandIds.Contains(content.BrandId)) &&
                c.IntegrationId.HasValue && AccessScope.IntegrationIds.Contains(c.IntegrationId.Value))));
        model.Entity<AiGeneration>().HasQueryFilter(g => !AccessScope.Enforced || g.Content.WorkspaceId == AccessScope.WorkspaceId &&
            (AccessScope.IsOwner || AccessScope.Role != WorkspaceMemberRoleEnum.Viewer && Set<ExecutionOperation>().Any(e => e.WorkspaceId == AccessScope.WorkspaceId &&
                e.ResourceType == "AiGeneration" && e.ReferenceId == g.Id && e.ResourceId == g.ContentId && e.ActorUserId == AccessScope.UserId)));
        model.Entity<CollaborationTask>().HasQueryFilter(t => !AccessScope.Enforced || t.WorkspaceId == AccessScope.WorkspaceId &&
            (AccessScope.IsOwner || AccessScope.Role == WorkspaceMemberRoleEnum.Manager && AccessScope.TeamIds.Contains(t.TeamId) || t.AssigneeId == AccessScope.UserId));
    }
}
