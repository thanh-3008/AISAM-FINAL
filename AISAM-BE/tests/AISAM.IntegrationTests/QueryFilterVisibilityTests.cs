using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class QueryFilterVisibilityTests
{
    private static AisamContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AisamContext(options);
    }

    [Fact]
    public async Task ViewerInTeamA_SeesApprovedAndPublishedOfTeamA_DoesNotSeeDraftOrOtherTeams()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), WorkspaceType = WorkspaceTypeEnum.Business };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand 1" };
        var teamA = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team A", Status = TeamStatusEnum.Active };
        var teamB = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team B", Status = TeamStatusEnum.Active };

        var viewerUser = new User { Id = Guid.NewGuid(), Email = "viewer@test.com", IsActive = true };
        var creatorUser = new User { Id = Guid.NewGuid(), Email = "creator@test.com", IsActive = true };

        var linkA = new TeamBrand { Id = Guid.NewGuid(), TeamId = teamA.Id, BrandId = brand.Id, IsActive = true };
        var linkB = new TeamBrand { Id = Guid.NewGuid(), TeamId = teamB.Id, BrandId = brand.Id, IsActive = true };

        var wmViewer = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = viewerUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };
        var wmCreator = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creatorUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };
        var tmViewer = new TeamMember { TeamId = teamA.Id, UserId = viewerUser.Id, Role = TeamRoleEnum.Viewer, IsActive = true };

        var aDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = creatorUser.Id, Status = ContentStatusEnum.Draft };
        var aPending = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = creatorUser.Id, Status = ContentStatusEnum.PendingApproval };
        var aApproved = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = creatorUser.Id, Status = ContentStatusEnum.Approved };
        var aPublished = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = creatorUser.Id, Status = ContentStatusEnum.Published };

        var bDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = creatorUser.Id, Status = ContentStatusEnum.Draft };
        var bApproved = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = creatorUser.Id, Status = ContentStatusEnum.Approved };
        var bPublished = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = creatorUser.Id, Status = ContentStatusEnum.Published };

        db.AddRange(workspace, brand, teamA, teamB, viewerUser, creatorUser, linkA, linkB, wmViewer, wmCreator, tmViewer);
        db.AddRange(aDraft, aPending, aApproved, aPublished, bDraft, bApproved, bPublished);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // Enable permission scope for viewerUser in Team A
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = workspace.Id;
        db.PermissionActorId = viewerUser.Id;
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = false;
        db.PermissionManager = false;
        db.PermissionCreator = false;
        db.PermissionBrandIds = [brand.Id];
        db.PermissionTeamIds = [teamA.Id];

        var visibleContents = await db.Contents.ToListAsync();

        Assert.Equal(2, visibleContents.Count);
        Assert.Contains(visibleContents, c => c.Id == aApproved.Id);
        Assert.Contains(visibleContents, c => c.Id == aPublished.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == aDraft.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == aPending.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == bDraft.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == bApproved.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == bPublished.Id);
    }

    [Fact]
    public async Task CreatorInTeamA_SeesOwnDraftAndTeammateApproved_DoesNotSeeOtherDraftOrOtherTeam()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), WorkspaceType = WorkspaceTypeEnum.Business };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand 1" };
        var teamA = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team A", Status = TeamStatusEnum.Active };
        var teamB = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team B", Status = TeamStatusEnum.Active };

        var creator1 = new User { Id = Guid.NewGuid(), Email = "c1@test.com", IsActive = true };
        var creator2 = new User { Id = Guid.NewGuid(), Email = "c2@test.com", IsActive = true };
        var creator3 = new User { Id = Guid.NewGuid(), Email = "c3@test.com", IsActive = true };

        var linkA = new TeamBrand { Id = Guid.NewGuid(), TeamId = teamA.Id, BrandId = brand.Id, IsActive = true };
        var linkB = new TeamBrand { Id = Guid.NewGuid(), TeamId = teamB.Id, BrandId = brand.Id, IsActive = true };

        var c1OwnDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = creator1.Id, Status = ContentStatusEnum.Draft };
        var c2OtherDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = creator2.Id, Status = ContentStatusEnum.Draft };
        var c2OtherApproved = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = creator2.Id, Status = ContentStatusEnum.Approved };
        var c3TeamBDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = creator3.Id, Status = ContentStatusEnum.Draft };
        var c3TeamBApproved = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = creator3.Id, Status = ContentStatusEnum.Approved };

        var wm1 = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creator1.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };
        var wm2 = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creator2.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };
        var wm3 = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creator3.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };
        var tm1 = new TeamMember { TeamId = teamA.Id, UserId = creator1.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true };
        var tm2 = new TeamMember { TeamId = teamA.Id, UserId = creator2.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true };
        var tm3 = new TeamMember { TeamId = teamB.Id, UserId = creator3.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true };

        db.AddRange(workspace, brand, teamA, teamB, creator1, creator2, creator3, linkA, linkB, wm1, wm2, wm3, tm1, tm2, tm3);
        db.AddRange(c1OwnDraft, c2OtherDraft, c2OtherApproved, c3TeamBDraft, c3TeamBApproved);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // Query as creator1 in Team A
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = workspace.Id;
        db.PermissionActorId = creator1.Id;
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = false;
        db.PermissionManager = false;
        db.PermissionCreator = true;
        db.PermissionBrandIds = [brand.Id];
        db.PermissionTeamIds = [teamA.Id];

        var visible = await db.Contents.ToListAsync();

        Assert.Equal(2, visible.Count);
        Assert.Contains(visible, c => c.Id == c1OwnDraft.Id);
        Assert.Contains(visible, c => c.Id == c2OtherApproved.Id);
        Assert.DoesNotContain(visible, c => c.Id == c2OtherDraft.Id);
        Assert.DoesNotContain(visible, c => c.Id == c3TeamBDraft.Id);
        Assert.DoesNotContain(visible, c => c.Id == c3TeamBApproved.Id);
    }

    [Fact]
    public async Task ManagerInTeamA_SeesAllContentInTeamA_DoesNotSeeTeamB()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), WorkspaceType = WorkspaceTypeEnum.Business };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand 1" };
        var teamA = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team A", Status = TeamStatusEnum.Active };
        var teamB = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team B", Status = TeamStatusEnum.Active };

        var manager = new User { Id = Guid.NewGuid(), Email = "manager@test.com", IsActive = true };
        var otherUser = new User { Id = Guid.NewGuid(), Email = "other@test.com", IsActive = true };

        var linkA = new TeamBrand { Id = Guid.NewGuid(), TeamId = teamA.Id, BrandId = brand.Id, IsActive = true };
        var linkB = new TeamBrand { Id = Guid.NewGuid(), TeamId = teamB.Id, BrandId = brand.Id, IsActive = true };

        var aDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = otherUser.Id, Status = ContentStatusEnum.Draft };
        var aApproved = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = otherUser.Id, Status = ContentStatusEnum.Approved };
        var bDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = otherUser.Id, Status = ContentStatusEnum.Draft };
        var bApproved = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = otherUser.Id, Status = ContentStatusEnum.Approved };

        var wmManager = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = manager.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };
        var wmOther = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = otherUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true };
        var tmManager = new TeamMember { TeamId = teamA.Id, UserId = manager.Id, Role = TeamRoleEnum.Manager, IsActive = true };
        var tmOther = new TeamMember { TeamId = teamA.Id, UserId = otherUser.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true };

        db.AddRange(workspace, brand, teamA, teamB, manager, otherUser, linkA, linkB, wmManager, wmOther, tmManager, tmOther);
        db.AddRange(aDraft, aApproved, bDraft, bApproved);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // Scope as manager of Team A
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = workspace.Id;
        db.PermissionActorId = manager.Id;
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = false;
        db.PermissionManager = true;
        db.PermissionManagerBrandIds = [brand.Id];
        db.PermissionBrandIds = [brand.Id];
        db.PermissionTeamIds = [teamA.Id];

        var visible = await db.Contents.ToListAsync();

        Assert.Equal(2, visible.Count);
        Assert.Contains(visible, c => c.Id == aDraft.Id);
        Assert.Contains(visible, c => c.Id == aApproved.Id);
        Assert.DoesNotContain(visible, c => c.Id == bDraft.Id);
        Assert.DoesNotContain(visible, c => c.Id == bApproved.Id);
    }

    [Fact]
    public async Task WorkspaceManagerAndOwner_SeeAllContentAcrossAllTeamsAndStatuses()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), WorkspaceType = WorkspaceTypeEnum.Business };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand 1" };
        var teamA = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team A", Status = TeamStatusEnum.Active };
        var teamB = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team B", Status = TeamStatusEnum.Active };

        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", IsActive = true };
        var wmUser = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = user.Id, Role = WorkspaceMemberRoleEnum.Owner, IsActive = true };

        var aDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = user.Id, Status = ContentStatusEnum.Draft };
        var aApproved = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamA.Id, PrimaryCreatorId = user.Id, Status = ContentStatusEnum.Approved };
        var bDraft = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = user.Id, Status = ContentStatusEnum.Draft };
        var bPublished = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamB.Id, PrimaryCreatorId = user.Id, Status = ContentStatusEnum.Published };

        db.AddRange(workspace, brand, teamA, teamB, user, wmUser);
        db.AddRange(aDraft, aApproved, bDraft, bPublished);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // 1. Check Owner
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = workspace.Id;
        db.PermissionActorId = user.Id;
        db.PermissionOwner = true;
        db.PermissionWorkspaceManager = false;

        Assert.Equal(4, await db.Contents.CountAsync());

        // 2. Check WorkspaceManager
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = true;

        Assert.Equal(4, await db.Contents.CountAsync());
    }

    [Fact]
    public async Task WorkspaceMemberFilter_OwnerAndWorkspaceManagerSeeAll_NormalMembersSeeTeammates()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), WorkspaceType = WorkspaceTypeEnum.Business };
        var teamA = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team A", Status = TeamStatusEnum.Active };
        var teamB = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team B", Status = TeamStatusEnum.Active };

        var owner = new User { Id = Guid.NewGuid(), Email = "owner@test.com", IsActive = true };
        var wmUser = new User { Id = Guid.NewGuid(), Email = "wm@test.com", IsActive = true };
        var m1 = new User { Id = Guid.NewGuid(), Email = "m1@test.com", IsActive = true };
        var m2 = new User { Id = Guid.NewGuid(), Email = "m2@test.com", IsActive = true };
        var m3 = new User { Id = Guid.NewGuid(), Email = "m3@test.com", IsActive = true };

        var members = new[]
        {
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = owner.Id, Role = WorkspaceMemberRoleEnum.Owner, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = wmUser.Id, Role = WorkspaceMemberRoleEnum.WorkspaceManager, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = m1.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = m2.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = m3.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true }
        };

        var teamMembers = new[]
        {
            new TeamMember { TeamId = teamA.Id, UserId = m1.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            new TeamMember { TeamId = teamA.Id, UserId = m2.Id, Role = TeamRoleEnum.Viewer, IsActive = true },
            new TeamMember { TeamId = teamB.Id, UserId = m3.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true }
        };

        db.Add(workspace);
        db.AddRange(teamA, teamB);
        db.AddRange(owner, wmUser, m1, m2, m3);
        db.AddRange(members);
        db.AddRange(teamMembers);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // 1. Owner sees all 5
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = workspace.Id;
        db.PermissionActorId = owner.Id;
        db.PermissionOwner = true;
        db.PermissionWorkspaceManager = false;
        Assert.Equal(5, await db.WorkspaceMembers.CountAsync());

        // 2. WorkspaceManager sees all 5
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = true;
        Assert.Equal(5, await db.WorkspaceMembers.CountAsync());

        // 3. m1 (Team A member) sees self (m1) and m2 (shared Team A)
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = false;
        db.PermissionActorId = m1.Id;
        db.PermissionTeamIds = [teamA.Id];

        var m1Visible = await db.WorkspaceMembers.ToListAsync();
        Assert.Contains(m1Visible, m => m.UserId == m1.Id);
        Assert.Contains(m1Visible, m => m.UserId == m2.Id);
        Assert.DoesNotContain(m1Visible, m => m.UserId == m3.Id);
    }
}
