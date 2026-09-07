using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    private async Task PrepareContentOwnershipAsync(CancellationToken ct)
    {
        foreach (var entry in ChangeTracker.Entries<Content>().Where(e => e.State == EntityState.Added).ToArray())
        {
            var content = entry.Entity;
            if (!content.TeamId.HasValue)
            {
                var preferred = AccessScope.Enforced ? AccessScope.ActiveTeamId : BackgroundAttribution?.TeamId;
                if (preferred.HasValue)
                {
                    content.TeamId = preferred;
                }
                else if (AccessScope.Enforced || BackgroundAttribution != null)
                {
                    var accessibleTeams = AccessScope.Enforced ? AccessScope.TeamIds : [BackgroundAttribution!.TeamId ?? Guid.Empty];
                    var candidates = await TeamBrands.IgnoreQueryFilters().AsNoTracking()
                        .Where(link => link.IsActive && link.BrandId == content.BrandId &&
                            link.Team.WorkspaceId == content.WorkspaceId && !link.Team.IsDeleted &&
                            accessibleTeams.Contains(link.TeamId))
                        .Select(link => link.TeamId).Distinct().Take(2).ToArrayAsync(ct);
                    if (candidates.Length == 1)
                    {
                        content.TeamId = candidates[0];
                    }
                    else if (candidates.Length == 0 && AccessScope.Enforced && AccessScope.IsOwner && accessibleTeams.Length == 1)
                    {
                        content.TeamId = accessibleTeams[0];
                    }
                }
            }

            // Unscoped contexts are used by migrations and legacy test fixtures. They
            // may preserve NULL, but authenticated/background production writes fail closed.
            if (!content.TeamId.HasValue)
            {
                if (AccessScope.Enforced || BackgroundAttribution != null)
                    throw new UnauthorizedAccessException("An unambiguous owning team is required to create content.");
                continue;
            }

            var valid = Teams.Local.Any(team => team.Id == content.TeamId && team.WorkspaceId == content.WorkspaceId && !team.IsDeleted &&
                    TeamBrands.Local.Any(link => link.TeamId == team.Id && link.BrandId == content.BrandId && link.IsActive)) ||
                await Teams.IgnoreQueryFilters().AsNoTracking().AnyAsync(team =>
                    team.Id == content.TeamId && team.WorkspaceId == content.WorkspaceId && !team.IsDeleted &&
                    TeamBrands.IgnoreQueryFilters().Any(link => link.TeamId == team.Id && link.BrandId == content.BrandId && link.IsActive), ct);

            if (!valid && content.TeamId.HasValue)
            {
                var teamExists = Teams.Local.Any(t => t.Id == content.TeamId && t.WorkspaceId == content.WorkspaceId && !t.IsDeleted) ||
                    await Teams.IgnoreQueryFilters().AnyAsync(t => t.Id == content.TeamId && t.WorkspaceId == content.WorkspaceId && !t.IsDeleted, ct);
                var brandHasNoActiveLinks = !await TeamBrands.IgnoreQueryFilters().AnyAsync(link => link.BrandId == content.BrandId && link.IsActive, ct) &&
                    !TeamBrands.Local.Any(link => link.BrandId == content.BrandId && link.IsActive);
                if (teamExists && brandHasNoActiveLinks && (AccessScope.IsOwner || !AccessScope.Enforced))
                {
                    TeamBrands.Add(new TeamBrand
                    {
                        TeamId = content.TeamId.Value,
                        BrandId = content.BrandId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true,
                        ChannelAccessMode = ChannelAccessMode.All
                    });
                    valid = true;
                }
            }

            if (!valid) throw new UnauthorizedAccessException("Content owning team must be active in the workspace and have access to the brand.");
        }

        foreach (var entry in ChangeTracker.Entries<Content>().Where(e => e.State == EntityState.Modified))
        {
            if (entry.Property(c => c.TeamId).IsModified && entry.Property(c => c.TeamId).OriginalValue is not null)
                throw new UnauthorizedAccessException("Content team ownership is immutable.");
        }
    }
}
