using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class ApprovalReviewerSnapshotResolverTests
{
    [Fact]
    public async Task ResolveAsync_ReturnsWorkspaceOwnerSnapshot()
    {
        await using var context = CreateContext();
        var workspace = new Workspace();
        var user = new User { Email = "owner@example.test", FullName = "Workspace Owner" };
        context.AddRange(workspace, user, new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Owner,
            WorkspaceRoleV2 = WorkspaceRoleV2.Owner
        });
        await context.SaveChangesAsync();

        var result = await ApprovalReviewerSnapshotResolver.ResolveAsync(
            context, workspace.Id, null, user.Id);

        Assert.Equal("Workspace Owner", result.Name);
        Assert.Equal("Chủ workspace", result.Role);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsTeamManagerSnapshotForWorkspaceMember()
    {
        await using var context = CreateContext();
        var workspace = new Workspace();
        var user = new User { Email = "manager@example.test", FullName = "Team Manager" };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Growth" };
        context.AddRange(workspace, user, team,
            new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserId = user.Id,
                Role = WorkspaceMemberRoleEnum.Viewer,
                WorkspaceRoleV2 = WorkspaceRoleV2.Member
            },
            new TeamMember
            {
                TeamId = team.Id,
                UserId = user.Id,
                Role = TeamRoleEnum.Manager
            });
        await context.SaveChangesAsync();

        var result = await ApprovalReviewerSnapshotResolver.ResolveAsync(
            context, workspace.Id, team.Id, user.Id);

        Assert.Equal("Team Manager", result.Name);
        Assert.Equal("Quản lý Team · Growth", result.Role);
    }

    private static AisamContext CreateContext() => new(
        new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);
}
