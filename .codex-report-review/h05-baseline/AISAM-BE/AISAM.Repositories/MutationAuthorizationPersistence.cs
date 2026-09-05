using AISAM.Data;
using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    private sealed record MutationStamp(Guid WorkspaceId, long Revision, Func<CancellationToken, Task<bool>> Revalidate, DateTime AuthorizedAt);
    private readonly Dictionary<string, MutationStamp> mutationStamps = new();

    public void RegisterMutationAuthorization(Guid workspace, string key, long revision, Func<CancellationToken, Task<bool>> revalidate) =>
        mutationStamps[key] = new(workspace, revision, revalidate, DateTime.UtcNow);

    private async Task<HashSet<Guid>> ChangedPermissionWorkspacesAsync(CancellationToken ct)
    {
        var result = new HashSet<Guid>();
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray())
        {
            Guid? workspace = entry.Entity switch
            {
                WorkspaceMember m => m.WorkspaceId,
                Team t => t.WorkspaceId,
                Brand b => b.WorkspaceId,
                SocialIntegration i => i.WorkspaceId,
                CollaborationTask t => t.WorkspaceId,
                TemporaryAccessGrant g => g.WorkspaceId,
                ContentParticipation p => p.WorkspaceId,
                Workspace w when entry.State == EntityState.Modified && (entry.Property(nameof(Workspace.Status)).IsModified || entry.Property(nameof(Workspace.SubscriptionExpiredAt)).IsModified) => w.Id,
                Content c when entry.State == EntityState.Modified && (entry.Property(nameof(Content.BrandId)).IsModified || entry.Property(nameof(Content.IsDeleted)).IsModified || entry.Property(nameof(Content.PrimaryCreatorId)).IsModified) => c.WorkspaceId,
                _ => null
            };
            if (entry.Entity is TeamMember member)
                workspace = Teams.Local.FirstOrDefault(t => t.Id == member.TeamId)?.WorkspaceId ??
                    await Teams.Where(t => t.Id == member.TeamId).Select(t => (Guid?)t.WorkspaceId).FirstOrDefaultAsync(ct);
            if (entry.Entity is TeamBrand brand)
                workspace = Teams.Local.FirstOrDefault(t => t.Id == brand.TeamId)?.WorkspaceId ??
                    await Teams.Where(t => t.Id == brand.TeamId).Select(t => (Guid?)t.WorkspaceId).FirstOrDefaultAsync(ct);
            if (entry.Entity is TeamChannelAccess channel)
            {
                var team = TeamBrands.Local.FirstOrDefault(b => b.Id == channel.TeamBrandId)?.TeamId;
                workspace = team.HasValue ? await Teams.Where(t => t.Id == team).Select(t => (Guid?)t.WorkspaceId).FirstOrDefaultAsync(ct) :
                    await TeamBrands.Where(b => b.Id == channel.TeamBrandId).Select(b => (Guid?)b.Team.WorkspaceId).FirstOrDefaultAsync(ct);
            }
            if (entry.State == EntityState.Modified && entry.Entity is TeamMember or TeamBrand)
            {
                var previousTeam = (Guid)entry.Property("TeamId").OriginalValue!;
                result.UnionWith(await Teams.Where(t => t.Id == previousTeam).Select(t => t.WorkspaceId).ToListAsync(ct));
            }
            if (entry.State == EntityState.Modified && entry.Entity is TeamChannelAccess)
            {
                var previousLink = (Guid)entry.Property("TeamBrandId").OriginalValue!;
                result.UnionWith(await TeamBrands.IgnoreQueryFilters().Where(b => b.Id == previousLink).Select(b => b.Team.WorkspaceId).ToListAsync(ct));
            }
            if (entry.Entity is User user && entry.State == EntityState.Modified &&
                (entry.Property(nameof(User.IsActive)).IsModified || entry.Property(nameof(User.Role)).IsModified))
                result.UnionWith(await WorkspaceMembers.Where(m => m.UserId == user.Id).Select(m => m.WorkspaceId).ToListAsync(ct));
            if (workspace.HasValue && workspace.Value != Guid.Empty) result.Add(workspace.Value);
            // Moving a permission row must invalidate both the old and new workspace.
            if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("WorkspaceId") != null &&
                entry.Property("WorkspaceId").OriginalValue is Guid previous && previous != Guid.Empty && workspace.HasValue)
                result.Add(previous);
        }
        return result;
    }

    private async Task<int> SaveWithMutationGuardAsync(bool acceptAllChanges, CancellationToken ct)
    {
        var changed = await ChangedPermissionWorkspacesAsync(ct);
        if (!Database.IsRelational())
        {
            await ValidateMutationStampsAsync(ct);
            foreach (var id in changed)
            {
                var workspace = await Workspaces.FindAsync([id], ct);
                if (workspace != null) workspace.PermissionRevision++;
            }
            return await base.SaveChangesAsync(acceptAllChanges, ct);
        }
        return await Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var ownTransaction = Database.CurrentTransaction == null;
            await using var transaction = ownTransaction ? await Database.BeginTransactionAsync(ct) : null;
            var ambient = Database.CurrentTransaction;
            if (!ownTransaction)
            {
                if (ambient?.SupportsSavepoints != true)
                    throw new InvalidOperationException("Mutation authorization requires transaction savepoints.");
                await ambient.CreateSavepointAsync("aisam_mutation_guard", ct);
            }
            try
            {
                var workspaces = changed.Concat(mutationStamps.Values.Select(s => s.WorkspaceId)).Distinct().Order().ToArray();
                foreach (var workspace in workspaces)
                {
                    var stamps = mutationStamps.Values.Where(s => s.WorkspaceId == workspace).ToArray();
                    if (stamps.Select(s => s.Revision).Distinct().Count() > 1) throw new MutationAuthorizationException();
                    if (stamps.Length > 0)
                    {
                        // A conditional no-op UPDATE locks this workspace until commit.
                        // Permission writers acquire the same row before changing permissions.
                        var expected = stamps[0].Revision;
                        if (await Database.ExecuteSqlInterpolatedAsync($"UPDATE workspaces SET permission_revision = permission_revision WHERE id = {workspace} AND permission_revision = {expected}", ct) != 1)
                            throw new MutationAuthorizationException();
                    }
                    if (changed.Contains(workspace))
                        await Database.ExecuteSqlInterpolatedAsync($"UPDATE workspaces SET permission_revision = permission_revision + 1 WHERE id = {workspace}", ct);
                }
                await ValidateMutationStampsAsync(ct);
                var count = await base.SaveChangesAsync(false, ct);
                // A slow write can cross a grant deadline. Recheck inside the same
                // transaction so expiry detected here rolls the write back as well.
                await ValidateMutationStampsAsync(ct);
                var revisions = await Workspaces.AsNoTracking().Where(w => workspaces.Contains(w.Id)).Select(w => new { w.Id, w.PermissionRevision }).ToListAsync(ct);
                if (transaction != null) await transaction.CommitAsync(ct);
                if (!ownTransaction) await Database.CurrentTransaction!.ReleaseSavepointAsync("aisam_mutation_guard", ct);
                if (acceptAllChanges) ChangeTracker.AcceptAllChanges();
                foreach (var revision in revisions)
                    foreach (var key in mutationStamps.Where(s => s.Value.WorkspaceId == revision.Id).Select(s => s.Key).ToArray())
                        mutationStamps[key] = mutationStamps[key] with { Revision = revision.PermissionRevision };
                return count;
            }
            catch
            {
                // A caller must not be able to catch a denial and commit the rejected
                // write using an outer transaction it owns.
                if (!ownTransaction) await ambient!.RollbackToSavepointAsync("aisam_mutation_guard", CancellationToken.None);
                throw;
            }
        });
    }

    private async Task ValidateMutationStampsAsync(CancellationToken ct)
    {
        foreach (var stamp in mutationStamps.Values.ToArray())
        {
            if (!Database.IsRelational() && !await Workspaces.AsNoTracking().AnyAsync(w => w.Id == stamp.WorkspaceId && w.PermissionRevision == stamp.Revision, ct))
                throw new MutationAuthorizationException();
            try
            {
                if (!await stamp.Revalidate(ct)) throw new MutationAuthorizationException();
            }
            catch (UnauthorizedAccessException) { throw new MutationAuthorizationException(); }
        }
    }

    public override int SaveChanges() => SaveChanges(true);
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // Authenticated and attributed system writes share all async preparation,
        // validation and audit invariants. Keep legacy unscoped fixture seeding intact.
        if (AccessScope.Enforced || BackgroundAttribution != null)
            return SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();
        ChangeTracker.DetectChanges();
        if (ChangeTracker.Entries<ExecutionOperation>().Any(e => e.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Execution attribution is immutable.");
        return SaveWithMutationGuardAsync(acceptAllChangesOnSuccess, default).GetAwaiter().GetResult();
    }
}
