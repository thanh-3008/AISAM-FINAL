using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class EffectivePermissionContextTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public readonly DbContextOptions<AisamContext> Options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public AisamContext Db { get; }
        public User Actor { get; } = new() { Email = "actor@example.test" };
        public User OtherUser { get; } = new() { Email = "other@example.test" };
        public Workspace Workspace { get; } = new() { WorkspaceType = WorkspaceTypeEnum.Business, Status = WorkspaceStatusEnum.Active };
        public Brand BrandA { get; } = new() { Name = "Brand A" };
        public Brand BrandB { get; } = new() { Name = "Brand B" };
        public Team Team1 { get; } = new() { Name = "Team 1", Status = TeamStatusEnum.Active };
        public Team Team2 { get; } = new() { Name = "Team 2", Status = TeamStatusEnum.Active };
        public Team Team3 { get; } = new() { Name = "Team 3", Status = TeamStatusEnum.Active };

        public Fixture()
        {
            Db = new(Options);
        }

        public async Task SetupAsync(WorkspaceMemberRoleEnum workspaceRole = WorkspaceMemberRoleEnum.Member)
        {
            BrandA.WorkspaceId = Workspace.Id;
            BrandB.WorkspaceId = Workspace.Id;
            Team1.WorkspaceId = Workspace.Id;
            Team2.WorkspaceId = Workspace.Id;
            Team3.WorkspaceId = Workspace.Id;

            var wmActor = new WorkspaceMember { WorkspaceId = Workspace.Id, UserId = Actor.Id, Role = workspaceRole, IsActive = true };
            var wmOther = new WorkspaceMember { WorkspaceId = Workspace.Id, UserId = OtherUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };

            Db.AddRange(Actor, OtherUser, Workspace, BrandA, BrandB, Team1, Team2, Team3, wmActor, wmOther);
            await Db.SaveChangesAsync();
        }

        public async Task AssignTeamToBrandAsync(Team team, Brand brand)
        {
            Db.TeamBrands.Add(new TeamBrand { TeamId = team.Id, BrandId = brand.Id, IsActive = true });
            await Db.SaveChangesAsync();
        }

        public async Task AddMemberToTeamAsync(User user, Team team, TeamRoleEnum role)
        {
            Db.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = user.Id, Role = role, IsActive = true });
            await Db.SaveChangesAsync();
        }

        public ValueTask DisposeAsync() => Db.DisposeAsync();
    }

    [Fact]
    public async Task MaxPrivilegeRule_ResolvesHighestRole_WhenUserInMultipleTeamsForSameBrand()
    {
        await using var f = new Fixture();
        await f.SetupAsync(WorkspaceMemberRoleEnum.Member);

        // Team 1 is connected to Brand A with Viewer
        await f.AssignTeamToBrandAsync(f.Team1, f.BrandA);
        await f.AddMemberToTeamAsync(f.Actor, f.Team1, TeamRoleEnum.Viewer);

        // Team 2 is also connected to Brand A with ContentCreator
        await f.AssignTeamToBrandAsync(f.Team2, f.BrandA);
        await f.AddMemberToTeamAsync(f.Actor, f.Team2, TeamRoleEnum.ContentCreator);

        var service = new AccessControlService(f.Db);

        // Even though Team 1 has Viewer (which cannot edit), Team 2 has ContentCreator -> Actor can edit own content!
        var ownContent = new Content { WorkspaceId = f.Workspace.Id, BrandId = f.BrandA.Id, PrimaryCreatorId = f.Actor.Id, TeamId = f.Team2.Id };
        f.Db.Contents.Add(ownContent);
        await f.Db.SaveChangesAsync();

        var editDecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Content, ownContent.Id, ResourcePermission.ContentEdit));
        Assert.True(editDecision.Allowed);

        // Now upgrade: Team 3 is connected to Brand A with Manager
        await f.AssignTeamToBrandAsync(f.Team3, f.BrandA);
        await f.AddMemberToTeamAsync(f.Actor, f.Team3, TeamRoleEnum.Manager);

        // Other user creates content in Team 3
        var teamContent = new Content { WorkspaceId = f.Workspace.Id, BrandId = f.BrandA.Id, PrimaryCreatorId = f.OtherUser.Id, TeamId = f.Team3.Id };
        f.Db.Contents.Add(teamContent);
        await f.Db.SaveChangesAsync();

        // Manager in Team 3 can edit content in Team 3, even if created by OtherUser!
        var managerEditDecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Content, teamContent.Id, ResourcePermission.ContentEdit));
        Assert.True(managerEditDecision.Allowed);
    }

    [Fact]
    public async Task TeamDataIsolation_PreventsTeamManagerFromEditingContentOfAnotherTeam()
    {
        await using var f = new Fixture();
        await f.SetupAsync(WorkspaceMemberRoleEnum.Member);

        // Team 1 and Team 2 both manage Brand A
        await f.AssignTeamToBrandAsync(f.Team1, f.BrandA);
        await f.AssignTeamToBrandAsync(f.Team2, f.BrandA);

        // Actor is Manager of Team 1 only
        await f.AddMemberToTeamAsync(f.Actor, f.Team1, TeamRoleEnum.Manager);

        // OtherUser creates content in Team 2
        var team2Content = new Content { WorkspaceId = f.Workspace.Id, BrandId = f.BrandA.Id, PrimaryCreatorId = f.OtherUser.Id, TeamId = f.Team2.Id };
        f.Db.Contents.Add(team2Content);
        await f.Db.SaveChangesAsync();

        var service = new AccessControlService(f.Db);

        // Actor (Team 1 Manager) CANNOT edit content of Team 2!
        var decision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Content, team2Content.Id, ResourcePermission.ContentEdit));
        Assert.False(decision.Allowed);
        Assert.Equal(403, decision.StatusCode);
    }

    [Fact]
    public async Task MultiBrandRoleDecoupling_ResolvesIndependentRolesPerBrand()
    {
        await using var f = new Fixture();
        await f.SetupAsync(WorkspaceMemberRoleEnum.Member);

        // Team 1 -> Brand A (Manager)
        await f.AssignTeamToBrandAsync(f.Team1, f.BrandA);
        await f.AddMemberToTeamAsync(f.Actor, f.Team1, TeamRoleEnum.Manager);

        // Team 2 -> Brand B (Viewer)
        await f.AssignTeamToBrandAsync(f.Team2, f.BrandB);
        await f.AddMemberToTeamAsync(f.Actor, f.Team2, TeamRoleEnum.Viewer);

        var service = new AccessControlService(f.Db);

        // On Brand A: Actor has Manager -> Can Create Content
        var createBrandADecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Brand, f.BrandA.Id, ResourcePermission.ContentCreate));
        Assert.True(createBrandADecision.Allowed);

        // On Brand B: Actor has Viewer -> CANNOT Create Content
        var createBrandBDecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Brand, f.BrandB.Id, ResourcePermission.ContentCreate));
        Assert.False(createBrandBDecision.Allowed);
    }

    [Fact]
    public async Task WorkspaceManager_CannotManageBilling_ButCanManageCrossTeamContent()
    {
        await using var f = new Fixture();
        await f.SetupAsync(WorkspaceMemberRoleEnum.WorkspaceManager);

        // Brand A linked to Team 1 and Team 2
        await f.AssignTeamToBrandAsync(f.Team1, f.BrandA);
        await f.AssignTeamToBrandAsync(f.Team2, f.BrandA);

        // WorkspaceManager belongs to Team 2
        await f.AddMemberToTeamAsync(f.Actor, f.Team2, TeamRoleEnum.Manager);

        // OtherUser creates content in Team 1
        var content = new Content { WorkspaceId = f.Workspace.Id, BrandId = f.BrandA.Id, PrimaryCreatorId = f.OtherUser.Id, TeamId = f.Team1.Id };
        f.Db.Contents.Add(content);
        await f.Db.SaveChangesAsync();

        var service = new AccessControlService(f.Db);

        // 1. WorkspaceManager CANNOT manage billing (Owner only!)
        var billingDecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Workspace, f.Workspace.Id, ResourcePermission.BillingManage));
        Assert.False(billingDecision.Allowed);
        Assert.Equal(403, billingDecision.StatusCode);

        // 2. WorkspaceManager CAN edit cross-team content
        var contentEditDecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Content, content.Id, ResourcePermission.ContentEdit));
        Assert.True(contentEditDecision.Allowed);
    }

    [Fact]
    public async Task Owner_HasUniversalAccessIncludingBillingAndCrossTeam()
    {
        await using var f = new Fixture();
        await f.SetupAsync(WorkspaceMemberRoleEnum.Owner);

        await f.AssignTeamToBrandAsync(f.Team1, f.BrandA);
        var content = new Content { WorkspaceId = f.Workspace.Id, BrandId = f.BrandA.Id, PrimaryCreatorId = f.OtherUser.Id, TeamId = f.Team1.Id };
        f.Db.Contents.Add(content);
        await f.Db.SaveChangesAsync();

        var service = new AccessControlService(f.Db);

        // Owner CAN manage billing
        var billingDecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Workspace, f.Workspace.Id, ResourcePermission.BillingManage));
        Assert.True(billingDecision.Allowed);

        // Owner CAN edit any content
        var contentEditDecision = await service.CheckAsync(new AccessRequest(f.Actor.Id, f.Workspace.Id, AccessResourceKind.Content, content.Id, ResourcePermission.ContentEdit));
        Assert.True(contentEditDecision.Allowed);
    }
}
