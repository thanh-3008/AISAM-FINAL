using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    // Scoped context, assigned only by authenticated middleware or a worker.
    public Guid? ExecutionActorId { get; set; }
    public bool ExecutionIsSystem { get; set; } = true;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidatePermissionRelationsAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await ValidatePermissionRelationsAsync(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private async Task<T> RequiredResourceAsync<T>(Guid id, CancellationToken ct) where T : class
    {
        var entity = await Set<T>().FindAsync(new object[] { id }, ct);
        if (entity is null || Entry(entity).State == EntityState.Deleted)
            throw new UnauthorizedAccessException("Permission relation references an unavailable resource.");
        return entity;
    }

    private async Task ValidatePermissionRelationsAsync(CancellationToken ct)
    {
        ChangeTracker.DetectChanges();
        await PrepareMediaAsync(ct);
        // Snapshot entries before loading metadata into the tracker.
        var changed = ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray();
        foreach (var entry in changed)
        {
            if(PermissionScopeEnabled && BeforePermissionMutation is not null)
                await BeforePermissionMutation(entry.Entity,entry.State,ct);
            if(entry.Entity is Conversation conversation && entry.State==EntityState.Added && ExecutionActorId.HasValue)
                conversation.CreatedByUserId=ExecutionActorId;
            if (entry.Entity is Content draft)
            {
                if (entry.State == EntityState.Added)
                {
                    if (ExecutionActorId.HasValue)
                        draft.PrimaryCreatorId = ExecutionActorId;
                    await EnsureContentTeamAsync(draft, ct);
                }
                if (ExecutionActorId.HasValue)
                    draft.UpdatedByUserId = ExecutionActorId;
            }
            if (entry.Entity is Post post && entry.State == EntityState.Added)
            {
                post.PublishedByUserId = ExecutionActorId;
                post.ExecutedBySystem = ExecutionIsSystem;
            }
            if (entry.Entity is ContentCalendar schedule && entry.State == EntityState.Added && ExecutionActorId.HasValue)
                schedule.ScheduledByUserId = ExecutionActorId;
            // Tenant and ownership identifiers are not editable fields. Transfers
            // need a separate audited operation, not an ordinary update DTO.
            var immutable = entry.Entity switch
            {
                Team => new[] { nameof(Team.WorkspaceId) },
                AutomationPlan => new[] { nameof(AutomationPlan.WorkspaceId), nameof(AutomationPlan.CreatedByUserId) },
                Brand => new[] { nameof(Brand.WorkspaceId) },
                SocialIntegration => new[] { nameof(SocialIntegration.WorkspaceId), nameof(SocialIntegration.BrandId) },
                Content => new[] { nameof(Content.PrimaryCreatorId), nameof(Content.WorkspaceId) },
                TeamBrand => new[] { nameof(TeamBrand.TeamId), nameof(TeamBrand.BrandId) },
                _ => Array.Empty<string>()
            };
            if (entry.State == EntityState.Modified && immutable.Any(name => entry.Property(name).IsModified &&
                !Equals(entry.Property(name).OriginalValue, entry.Property(name).CurrentValue)))
                throw new UnauthorizedAccessException("Resource ownership cannot be changed through a normal update.");

            switch (entry.Entity)
            {
                case TeamBrand assignment when entry.State != EntityState.Deleted:
                    var team = await RequiredResourceAsync<Team>(assignment.TeamId, ct);
                    var brand = await RequiredResourceAsync<Brand>(assignment.BrandId, ct);
                    if (team.WorkspaceId == Guid.Empty || team.WorkspaceId != brand.WorkspaceId)
                        throw new UnauthorizedAccessException("Team and Brand must belong to the same workspace.");
                    break;
                case TeamChannelAccess channel when entry.State != EntityState.Deleted:
                    if ((channel.CanPublish || channel.CanManage) && !channel.CanView)
                        throw new UnauthorizedAccessException("Channel publish/manage requires view permission.");
                    var teamBrand = await RequiredResourceAsync<TeamBrand>(channel.TeamBrandId, ct);
                    var channelTeam = await RequiredResourceAsync<Team>(teamBrand.TeamId, ct);
                    var channelBrand = await RequiredResourceAsync<Brand>(teamBrand.BrandId, ct);
                    var integration = await RequiredResourceAsync<SocialIntegration>(channel.IntegrationId, ct);
                    if (channelTeam.WorkspaceId == Guid.Empty || channelTeam.WorkspaceId != channelBrand.WorkspaceId ||
                        channelBrand.WorkspaceId != integration.WorkspaceId || channelBrand.Id != integration.BrandId)
                        throw new UnauthorizedAccessException("Channel assignment must stay within its Team workspace and Brand.");
                    break;
                case Content content when entry.State == EntityState.Added && content.PrimaryCreatorId.HasValue:
                    var creator = await RequiredResourceAsync<User>(content.PrimaryCreatorId.Value, ct);
                    var trackedMember = ChangeTracker.Entries<WorkspaceMember>().FirstOrDefault(e =>
                        e.Entity.UserId == creator.Id && e.Entity.WorkspaceId == content.WorkspaceId);
                    var membership = trackedMember is not null
                        ? trackedMember.State != EntityState.Deleted && trackedMember.Entity.IsActive
                        : await WorkspaceMembers.AnyAsync(m => m.UserId == creator.Id &&
                            m.WorkspaceId == content.WorkspaceId && m.IsActive, ct);
                    if (!creator.IsActive || !membership)
                        throw new UnauthorizedAccessException("Content creator must be an active workspace member.");
                    break;
            }
            if(ExecutionActorId is { } auditActor)
            {
                var audit = entry.Entity switch
                {
                    Content c => ("contents",c.Id,c.WorkspaceId,entry.State==EntityState.Added?"content.create":c.IsDeleted || entry.State==EntityState.Deleted?"content.delete":"content.edit"),
                    Post p => ("posts",p.Id,PermissionWorkspaceId,p.IsDeleted || entry.State==EntityState.Deleted?"post.delete":entry.State==EntityState.Added?"post.publish":"post.update"),
                    Approval a => ("approvals",a.Id,PermissionWorkspaceId,"approval.review"),
                    SocialIntegration i => ("social_integrations",i.Id,i.WorkspaceId,i.IsDeleted?"social.disconnect":"social.connect"),
                    SocialAccount a => ("social_accounts",a.Id,a.WorkspaceId,a.IsDeleted?"social.disconnect":"social.connect"),
                    _ => ((string?)null,Guid.Empty,Guid.Empty,"")
                };
                if(audit.Item1 is not null && !ChangeTracker.Entries<AuditLog>().Any(a=>a.State==EntityState.Added && a.Entity.TargetId==audit.Item2 && a.Entity.ActionType==audit.Item4))
                    AuditLogs.Add(new AuditLog {ActorId=auditActor,WorkspaceId=audit.Item3==Guid.Empty?null:audit.Item3,
                        TargetTable=audit.Item1,TargetId=audit.Item2,ActionType=audit.Item4,Result="allowed",ExecutedBySystem=ExecutionIsSystem});
            }
        }
    }

    private async Task EnsureContentTeamAsync(Content content, CancellationToken ct)
    {
        if (content.WorkspaceId == Guid.Empty || content.BrandId == Guid.Empty)
            return;

        Guid resolvedTeamId = content.TeamId ?? Guid.Empty;

        if (resolvedTeamId == Guid.Empty)
        {
            var creatorId = content.PrimaryCreatorId ?? ExecutionActorId;

            // 1. Check if creator belongs to an active team assigned to this brand in the workspace
            if (creatorId.HasValue && creatorId.Value != Guid.Empty)
            {
                resolvedTeamId = await (from tb in TeamBrands.AsNoTracking()
                                        join tm in TeamMembers.AsNoTracking() on tb.TeamId equals tm.TeamId
                                        join t in Teams.AsNoTracking() on tb.TeamId equals t.Id
                                        where tb.BrandId == content.BrandId && tb.IsActive
                                           && t.WorkspaceId == content.WorkspaceId && !t.IsDeleted && t.Status == AISAM.Data.Enumeration.TeamStatusEnum.Active
                                           && tm.UserId == creatorId.Value && tm.IsActive
                                        select t.Id).FirstOrDefaultAsync(ct);
            }

            // 2. If not found, check if any active team in workspace is assigned to this brand
            if (resolvedTeamId == Guid.Empty)
            {
                resolvedTeamId = await (from tb in TeamBrands.AsNoTracking()
                                        join t in Teams.AsNoTracking() on tb.TeamId equals t.Id
                                        where tb.BrandId == content.BrandId && tb.IsActive
                                           && t.WorkspaceId == content.WorkspaceId && !t.IsDeleted && t.Status == AISAM.Data.Enumeration.TeamStatusEnum.Active
                                        select t.Id).FirstOrDefaultAsync(ct);
            }

            if (resolvedTeamId != Guid.Empty)
            {
                content.TeamId = resolvedTeamId;
            }
        }

        if (content.TeamId.HasValue && content.TeamId.Value != Guid.Empty)
        {
            var targetTeamId = content.TeamId.Value;
            // Ensure TeamBrand assignment exists so trigger aisam_content_team_boundary succeeds
            var hasBrandLink = (ChangeTracker.Entries<TeamBrand>().Any(e => e.Entity.TeamId == targetTeamId && e.Entity.BrandId == content.BrandId && e.Entity.IsActive && e.State != EntityState.Deleted))
                || await TeamBrands.AnyAsync(tb => tb.TeamId == targetTeamId && tb.BrandId == content.BrandId && tb.IsActive, ct);

            if (!hasBrandLink)
            {
                var teamExists = (ChangeTracker.Entries<Team>().Any(e => e.Entity.Id == targetTeamId && e.State != EntityState.Deleted))
                    || await Teams.AnyAsync(t => t.Id == targetTeamId && t.WorkspaceId == content.WorkspaceId && !t.IsDeleted, ct);

                if (teamExists)
                {
                    TeamBrands.Add(new TeamBrand
                    {
                        TeamId = targetTeamId,
                        BrandId = content.BrandId,
                        IsActive = true,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }
        }
    }
}
