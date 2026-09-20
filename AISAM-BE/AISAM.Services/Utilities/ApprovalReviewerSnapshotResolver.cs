using AISAM.Data.Enumeration;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Utilities;

public sealed record ApprovalReviewerSnapshot(string Name, string Role);

public static class ApprovalReviewerSnapshotResolver
{
    public static async Task<ApprovalReviewerSnapshot> ResolveAsync(
        AisamContext context,
        Guid workspaceId,
        Guid? teamId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users.IgnoreQueryFilters().AsNoTracking()
            .Where(value => value.Id == userId)
            .Select(value => new { value.FullName, value.Email })
            .FirstOrDefaultAsync(cancellationToken);
        var name = user?.FullName ?? user?.Email ?? "Người dùng không xác định";

        var workspaceMember = await context.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking()
            .Where(value => value.WorkspaceId == workspaceId && value.UserId == userId && value.IsActive)
            .Select(value => new { value.WorkspaceRoleV2, value.Role })
            .FirstOrDefaultAsync(cancellationToken);

        if (workspaceMember?.WorkspaceRoleV2 == WorkspaceRoleV2.Owner || workspaceMember?.Role == WorkspaceMemberRoleEnum.Owner)
            return new ApprovalReviewerSnapshot(name, "Chủ workspace");
        if (workspaceMember?.WorkspaceRoleV2 == WorkspaceRoleV2.WorkspaceManager)
            return new ApprovalReviewerSnapshot(name, "Quản lý workspace");

        if (teamId.HasValue)
        {
            var teamRole = await (
                from member in context.TeamMembers.IgnoreQueryFilters().AsNoTracking()
                join team in context.Teams.IgnoreQueryFilters().AsNoTracking() on member.TeamId equals team.Id
                where member.TeamId == teamId.Value && member.UserId == userId && member.IsActive
                select new { member.Role, team.Name })
                .FirstOrDefaultAsync(cancellationToken);
            if (teamRole?.Role == TeamRoleEnum.Manager)
                return new ApprovalReviewerSnapshot(name, $"Quản lý Team · {teamRole.Name}");
        }

        if (workspaceMember?.Role == WorkspaceMemberRoleEnum.Manager)
            return new ApprovalReviewerSnapshot(name, "Quản lý");

        return new ApprovalReviewerSnapshot(name, "Người duyệt");
    }
}
