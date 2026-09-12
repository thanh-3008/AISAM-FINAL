using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AISAM.IntegrationTests;

public class AssignmentServiceTests
{
    private static AisamContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AisamContext(options);
    }

    [Fact]
    public async Task ChangeAsync_ThrowsInvalidOperationException_WhenTeamHasNoManager()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var owner = new User { Email = "owner@test.com", IsActive = true };
        var member = new User { Email = "creator@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Team Without Manager", Status = TeamStatusEnum.Active };
        var brand = new Brand { WorkspaceId = workspace.Id, Name = "Test Brand" };

        db.Add(workspace);
        db.AddRange(owner, member);
        db.Add(team);
        db.Add(brand);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = owner.Id, Role = WorkspaceMemberRoleEnum.Owner, IsActive = true });
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = member.Id, Role = WorkspaceMemberRoleEnum.ContentCreator, IsActive = true });
        // Team has only a ContentCreator, no Manager
        db.Add(new TeamMember { TeamId = team.Id, UserId = member.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true });
        await db.SaveChangesAsync();

        var access = new FakeAccessControlService();
        var assignmentService = new AssignmentService(db, access);

        var snapshot = await assignmentService.ReadAsync(owner.Id, workspace.Id, brand.Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            assignmentService.ChangeAsync(new AssignmentChange(
                owner.Id, workspace.Id, brand.Id, team.Id,
                snapshot.Revision, true)));

        Assert.Equal("TEAM_REQUIRES_MANAGER", ex.Message);
    }

    [Fact]
    public async Task ChangeAsync_Succeeds_WhenTeamHasManager()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var owner = new User { Email = "owner@test.com", IsActive = true };
        var manager = new User { Email = "manager@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Team With Manager", Status = TeamStatusEnum.Active };
        var brand = new Brand { WorkspaceId = workspace.Id, Name = "Test Brand" };

        db.Add(workspace);
        db.AddRange(owner, manager);
        db.Add(team);
        db.Add(brand);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = owner.Id, Role = WorkspaceMemberRoleEnum.Owner, IsActive = true });
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = manager.Id, Role = WorkspaceMemberRoleEnum.Manager, IsActive = true });
        // Team has a Manager
        db.Add(new TeamMember { TeamId = team.Id, UserId = manager.Id, Role = TeamRoleEnum.Manager, IsActive = true });
        await db.SaveChangesAsync();

        var access = new FakeAccessControlService();
        var assignmentService = new AssignmentService(db, access);

        var snapshot = await assignmentService.ReadAsync(owner.Id, workspace.Id, brand.Id);

        var result = await assignmentService.ChangeAsync(new AssignmentChange(
            owner.Id, workspace.Id, brand.Id, team.Id,
            snapshot.Revision, true));

        Assert.NotNull(result);
        var assignedTeam = Assert.Single(result.Teams.Where(t => t.TeamId == team.Id));
        Assert.True(assignedTeam.IsActive);
    }

    [Fact]
    public async Task ChangeAsync_ThrowsAssignmentAccessException_WhenActorIsTeamManagerButNotOwnerOrWorkspaceManager()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var teamManagerUser = new User { Email = "teammanager@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Team A", Status = TeamStatusEnum.Active };
        var brand = new Brand { WorkspaceId = workspace.Id, Name = "Brand A" };

        db.Add(workspace);
        db.Add(teamManagerUser);
        db.Add(team);
        db.Add(brand);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = teamManagerUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new TeamMember { TeamId = team.Id, UserId = teamManagerUser.Id, Role = TeamRoleEnum.Manager, IsActive = true });
        await db.SaveChangesAsync();

        var access = new FakeAccessControlService();
        var assignmentService = new AssignmentService(db, access);

        var snapshot = await assignmentService.ReadAsync(teamManagerUser.Id, workspace.Id, brand.Id);

        // Member + Team Manager attempts to link brand to team -> must be denied (NC-01)
        var ex = await Assert.ThrowsAsync<AssignmentAccessException>(() =>
            assignmentService.ChangeAsync(new AssignmentChange(
                teamManagerUser.Id, workspace.Id, brand.Id, team.Id,
                snapshot.Revision, true)));

        Assert.Equal(403, ex.Decision.StatusCode);
    }

    [Fact]
    public async Task ChangeAsync_Succeeds_WhenActorIsWorkspaceManager()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var wmUser = new User { Email = "wm@test.com", IsActive = true };
        var memberUser = new User { Email = "manager@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Team A", Status = TeamStatusEnum.Active };
        var brand = new Brand { WorkspaceId = workspace.Id, Name = "Brand A" };

        db.Add(workspace);
        db.AddRange(wmUser, memberUser);
        db.Add(team);
        db.Add(brand);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = wmUser.Id, Role = WorkspaceMemberRoleEnum.WorkspaceManager, IsActive = true });
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = memberUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new TeamMember { TeamId = team.Id, UserId = memberUser.Id, Role = TeamRoleEnum.Manager, IsActive = true });
        await db.SaveChangesAsync();

        var access = new FakeAccessControlService();
        var assignmentService = new AssignmentService(db, access);

        var snapshot = await assignmentService.ReadAsync(wmUser.Id, workspace.Id, brand.Id);

        // WorkspaceManager links brand to team -> must succeed (NC-01 & NC-05)
        var result = await assignmentService.ChangeAsync(new AssignmentChange(
            wmUser.Id, workspace.Id, brand.Id, team.Id,
            snapshot.Revision, true));

        Assert.NotNull(result);
        var assignedTeam = Assert.Single(result.Teams.Where(t => t.TeamId == team.Id));
        Assert.True(assignedTeam.IsActive);
    }

    private sealed class FakeAccessControlService : IAccessControlService
    {
        public Task<AccessDecision> CheckAsync(AccessRequest request, CancellationToken ct = default)
            => Task.FromResult(AccessDecision.Permit);
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid actorId, Guid workspaceId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Guid>>([]);
    }
}
