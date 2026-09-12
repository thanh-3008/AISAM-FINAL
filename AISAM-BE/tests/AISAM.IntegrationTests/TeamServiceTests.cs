using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AISAM.IntegrationTests;

public class TeamServiceTests
{
    private static AisamContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AisamContext(options);
    }

    private sealed class FakeAccessControlService : IAccessControlService
    {
        public Task<AccessDecision> CheckAsync(AccessRequest request, CancellationToken ct = default)
            => Task.FromResult(AccessDecision.Permit);
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid actorId, Guid workspaceId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Guid>>([]);
    }

    [Fact]
    public async Task CreateAsync_AllowsWorkspaceManager_EvenIfNotInAnyTeam()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var wm = new User { Email = "wm@test.com", IsActive = true };

        db.Add(workspace);
        db.Add(wm);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = wm.Id, Role = WorkspaceMemberRoleEnum.WorkspaceManager, IsActive = true });
        await db.SaveChangesAsync();

        var teamService = new TeamService(db, new FakeAccessControlService());

        var result = await teamService.CreateAsync(wm.Id, workspace.Id, new TeamService.CreateTeamRequest("Engineering", null, null));

        Assert.NotNull(result);
        Assert.Equal("Engineering", result.Name);
        Assert.Single(result.Members);
        Assert.Equal(wm.Id, result.Members[0].UserId);
        Assert.Equal(TeamRoleEnum.Manager.ToString(), result.Members[0].Role);
    }

    [Fact]
    public async Task CreateAsync_RejectsMemberAndTeamManager()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var tm = new User { Email = "tm@test.com", IsActive = true };
        var existingTeam = new Team { WorkspaceId = workspace.Id, Name = "Marketing", Status = TeamStatusEnum.Active };

        db.Add(workspace);
        db.Add(tm);
        db.Add(existingTeam);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = tm.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new TeamMember { TeamId = existingTeam.Id, UserId = tm.Id, Role = TeamRoleEnum.Manager, IsActive = true });
        await db.SaveChangesAsync();

        var teamService = new TeamService(db, new FakeAccessControlService());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            teamService.CreateAsync(tm.Id, workspace.Id, new TeamService.CreateTeamRequest("New Team", null, null)));
    }

    [Fact]
    public async Task UpdateAsync_RejectsTeamManager()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var tm = new User { Email = "tm@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Marketing", Status = TeamStatusEnum.Active };

        db.Add(workspace);
        db.Add(tm);
        db.Add(team);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = tm.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new TeamMember { TeamId = team.Id, UserId = tm.Id, Role = TeamRoleEnum.Manager, IsActive = true });
        await db.SaveChangesAsync();

        var teamService = new TeamService(db, new FakeAccessControlService());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            teamService.UpdateAsync(tm.Id, workspace.Id, team.Id, new TeamService.UpdateTeamRequest("Renamed", null)));
    }

    [Fact]
    public async Task AddMemberAsync_AllowsTeamManagerInOwnTeam()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var tm = new User { Email = "tm@test.com", IsActive = true };
        var targetUser = new User { Email = "target@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Marketing", Status = TeamStatusEnum.Active };

        db.Add(workspace);
        db.AddRange(tm, targetUser);
        db.Add(team);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = tm.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = targetUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new TeamMember { TeamId = team.Id, UserId = tm.Id, Role = TeamRoleEnum.Manager, IsActive = true });
        await db.SaveChangesAsync();

        var teamService = new TeamService(db, new FakeAccessControlService());

        var added = await teamService.AddMemberAsync(tm.Id, workspace.Id, team.Id, targetUser.Id, "ContentCreator");

        Assert.NotNull(added);
        Assert.Equal(targetUser.Id, added.UserId);
        Assert.Equal(TeamRoleEnum.ContentCreator.ToString(), added.Role);
    }

    [Fact]
    public async Task AddMemberAsync_RejectsTeamManagerInOtherTeam()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var tm = new User { Email = "tm@test.com", IsActive = true };
        var targetUser = new User { Email = "target@test.com", IsActive = true };
        var team1 = new Team { WorkspaceId = workspace.Id, Name = "Team 1", Status = TeamStatusEnum.Active };
        var team2 = new Team { WorkspaceId = workspace.Id, Name = "Team 2", Status = TeamStatusEnum.Active };

        db.Add(workspace);
        db.AddRange(tm, targetUser);
        db.AddRange(team1, team2);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = tm.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = targetUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        // tm is manager of team1, NOT team2
        db.Add(new TeamMember { TeamId = team1.Id, UserId = tm.Id, Role = TeamRoleEnum.Manager, IsActive = true });
        await db.SaveChangesAsync();

        var teamService = new TeamService(db, new FakeAccessControlService());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            teamService.AddMemberAsync(tm.Id, workspace.Id, team2.Id, targetUser.Id, "ContentCreator"));
    }

    [Fact]
    public async Task AddMemberAsync_AllowsWorkspaceManager_EvenIfNotInTeam()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var wm = new User { Email = "wm@test.com", IsActive = true };
        var targetUser = new User { Email = "target@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Marketing", Status = TeamStatusEnum.Active };

        db.Add(workspace);
        db.AddRange(wm, targetUser);
        db.Add(team);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = wm.Id, Role = WorkspaceMemberRoleEnum.WorkspaceManager, IsActive = true });
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = targetUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        await db.SaveChangesAsync();

        var teamService = new TeamService(db, new FakeAccessControlService());

        var added = await teamService.AddMemberAsync(wm.Id, workspace.Id, team.Id, targetUser.Id, "Viewer");

        Assert.NotNull(added);
        Assert.Equal(targetUser.Id, added.UserId);
        Assert.Equal(TeamRoleEnum.Viewer.ToString(), added.Role);
    }

    [Fact]
    public async Task AddMemberAsync_RejectsPlainMember()
    {
        await using var db = CreateDbContext();
        var workspace = new Workspace { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        var plainMember = new User { Email = "creator@test.com", IsActive = true };
        var targetUser = new User { Email = "target@test.com", IsActive = true };
        var team = new Team { WorkspaceId = workspace.Id, Name = "Marketing", Status = TeamStatusEnum.Active };

        db.Add(workspace);
        db.AddRange(plainMember, targetUser);
        db.Add(team);
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = plainMember.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new WorkspaceMember { WorkspaceId = workspace.Id, UserId = targetUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true });
        db.Add(new TeamMember { TeamId = team.Id, UserId = plainMember.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true });
        await db.SaveChangesAsync();

        var teamService = new TeamService(db, new FakeAccessControlService());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            teamService.AddMemberAsync(plainMember.Id, workspace.Id, team.Id, targetUser.Id, "Viewer"));
    }
}
