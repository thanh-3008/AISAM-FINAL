using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    // Defense in depth for authenticated writes. This does not replace database
    // constraints or an explicit execution policy for unscoped background work.
    private async Task ValidateAccessLinksAsync(CancellationToken ct)
    {
        if (!AccessScope.Enforced) return;
        var workspace = AccessScope.WorkspaceId;
        static void Require(bool valid)
        {
            if (!valid) throw new UnauthorizedAccessException("Permission relation is outside the workspace or resource scope.");
        }
        IEnumerable<T> Changed<T>() where T : class => ChangeTracker.Entries<T>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified).Select(e => e.Entity).ToArray();
        async Task<bool> TeamInWorkspace(Guid id) => Teams.Local.Any(t => t.Id == id && t.WorkspaceId == workspace) ||
            await Teams.AnyAsync(t => t.Id == id && t.WorkspaceId == workspace, ct);
        async Task<bool> MemberInWorkspace(Guid user) => WorkspaceMembers.Local.Any(m => m.WorkspaceId == workspace && m.UserId == user && m.IsActive) ||
            await WorkspaceMembers.AnyAsync(m => m.WorkspaceId == workspace && m.UserId == user && m.IsActive, ct);

        foreach (var team in Changed<Team>()) Require(team.WorkspaceId == workspace && workspace != Guid.Empty);
        foreach (var member in Changed<TeamMember>())
        {
            Require(await TeamInWorkspace(member.TeamId));
            if (member.IsActive) Require(await MemberInWorkspace(member.UserId));
        }
        foreach (var link in Changed<TeamBrand>())
        {
            Require(await TeamInWorkspace(link.TeamId));
            Require(await Brands.IgnoreQueryFilters().AnyAsync(b => b.Id == link.BrandId && b.WorkspaceId == workspace, ct));
        }
        foreach (var channel in Changed<TeamChannelAccess>())
        {
            var link = TeamBrands.Local.FirstOrDefault(b => b.Id == channel.TeamBrandId) ??
                await TeamBrands.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == channel.TeamBrandId && b.Team.WorkspaceId == workspace, ct);
            Require(link != null && link.IsActive && link.ChannelAccessMode == ChannelAccessMode.Specific);
            Require(await TeamInWorkspace(link!.TeamId));
            Require(await SocialIntegrations.IgnoreQueryFilters().AnyAsync(i => i.Id == channel.IntegrationId && i.WorkspaceId == workspace && i.BrandId == link.BrandId, ct));
        }
        foreach (var participation in Changed<ContentParticipation>())
        {
            Require(participation.WorkspaceId == workspace && await MemberInWorkspace(participation.UserId));
            Require(await Contents.IgnoreQueryFilters().AnyAsync(c => c.Id == participation.ContentId && c.WorkspaceId == workspace, ct));
        }
        foreach (var task in Changed<CollaborationTask>())
        {
            Require(task.WorkspaceId == workspace && await TeamInWorkspace(task.TeamId));
            if (task.Status is CollaborationTaskStatus.Pending or CollaborationTaskStatus.InProgress)
                Require(await MemberInWorkspace(task.AssigneeId));
            var brand = await Contents.IgnoreQueryFilters().Where(c => c.Id == task.ContentId && c.WorkspaceId == workspace)
                .Select(c => (Guid?)c.BrandId).FirstOrDefaultAsync(ct);
            Require(brand.HasValue);
            if (task.IntegrationId.HasValue) Require(await SocialIntegrations.IgnoreQueryFilters().AnyAsync(i =>
                i.Id == task.IntegrationId && i.WorkspaceId == workspace && i.BrandId == brand, ct));
        }
        foreach (var grant in Changed<TemporaryAccessGrant>())
        {
            var task = CollaborationTasks.Local.FirstOrDefault(t => t.Id == grant.TaskId) ??
                await CollaborationTasks.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == grant.TaskId && t.WorkspaceId == workspace, ct);
            Require(grant.WorkspaceId == workspace && task?.WorkspaceId == workspace && task.AssigneeId == grant.UserId);
            Require(grant.ExpiresAt > grant.GrantedAt);
        }
    }
}
