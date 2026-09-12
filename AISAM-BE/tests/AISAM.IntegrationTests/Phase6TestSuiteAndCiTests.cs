using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class Phase6TestSuiteAndCiTests
{
    private static AisamContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AisamContext(options);
    }

    [Fact]
    public async Task MaxPrivilege_UserInTwoTeamsOnSameBrand_ResolvesToHigherRoleManager()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand 1" };
        var team1 = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team 1", Status = TeamStatusEnum.Active };
        var team2 = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team 2", Status = TeamStatusEnum.Active };

        var user = new User { Id = Guid.NewGuid(), Email = "dual@test.com", FullName = "Dual Team User", IsActive = true };
        var creator = new User { Id = Guid.NewGuid(), Email = "creator@test.com", FullName = "Creator", IsActive = true };

        var content = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = team2.Id,
            PrimaryCreatorId = creator.Id,
            Status = ContentStatusEnum.PendingApproval
        };

        db.AddRange(workspace, brand, team1, team2, user, creator,
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = user.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creator.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new TeamBrand { TeamId = team1.Id, BrandId = brand.Id, IsActive = true },
            new TeamBrand { TeamId = team2.Id, BrandId = brand.Id, IsActive = true },
            // In Team 1, User is Viewer. In Team 2, User is Manager.
            new TeamMember { TeamId = team1.Id, UserId = user.Id, Role = TeamRoleEnum.Viewer, IsActive = true },
            new TeamMember { TeamId = team2.Id, UserId = user.Id, Role = TeamRoleEnum.Manager, IsActive = true },
            new TeamMember { TeamId = team2.Id, UserId = creator.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            content
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var accessControl = new AccessControlService(db);

        // Verify Max Privilege allows reviewing content (Manager privilege overrides Viewer)
        var reviewDecision = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Content, content.Id, ResourcePermission.ApprovalReview
        ));
        Assert.True(reviewDecision.Allowed);

        // Verify Max Privilege allows editing content in Team 2
        var editDecision = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Content, content.Id, ResourcePermission.ContentEdit
        ));
        Assert.True(editDecision.Allowed);

        // Verify Brand resolution helper produces Manager
        var maxRole = EffectivePermissionContext.ResolveMaxRole(new[] { TeamRoleEnum.Viewer, TeamRoleEnum.Manager });
        Assert.Equal(TeamRoleEnum.Manager, maxRole);
    }

    [Fact]
    public async Task MaxPrivilege_UserInTwoTeams_CreatorAndViewer_ResolvesToCreator()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand Beta" };
        var team1 = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team 1", Status = TeamStatusEnum.Active };
        var team2 = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team 2", Status = TeamStatusEnum.Active };

        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", FullName = "User", IsActive = true };
        var otherUser = new User { Id = Guid.NewGuid(), Email = "other@test.com", FullName = "Other User", IsActive = true };

        var ownContent = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = team2.Id,
            PrimaryCreatorId = user.Id,
            Status = ContentStatusEnum.Draft
        };

        var otherContent = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = team2.Id,
            PrimaryCreatorId = otherUser.Id,
            Status = ContentStatusEnum.PendingApproval
        };

        db.AddRange(workspace, brand, team1, team2, user, otherUser,
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = user.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = otherUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new TeamBrand { TeamId = team1.Id, BrandId = brand.Id, IsActive = true },
            new TeamBrand { TeamId = team2.Id, BrandId = brand.Id, IsActive = true },
            // In Team 1: Viewer. In Team 2: ContentCreator.
            new TeamMember { TeamId = team1.Id, UserId = user.Id, Role = TeamRoleEnum.Viewer, IsActive = true },
            new TeamMember { TeamId = team2.Id, UserId = user.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            new TeamMember { TeamId = team2.Id, UserId = otherUser.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            ownContent, otherContent
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var accessControl = new AccessControlService(db);

        // 1. Can create content on Brand Beta (Creator privilege overrides Viewer)
        var createDecision = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Brand, brand.Id, ResourcePermission.ContentCreate
        ));
        Assert.True(createDecision.Allowed);

        // 2. Can edit own content
        var editOwnDecision = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Content, ownContent.Id, ResourcePermission.ContentEdit
        ));
        Assert.True(editOwnDecision.Allowed);

        // 3. CANNOT edit other's content (only creator, not manager)
        var editOtherDecision = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Content, otherContent.Id, ResourcePermission.ContentEdit
        ));
        Assert.False(editOtherDecision.Allowed);

        // 4. CANNOT review other's content without delegated key
        var reviewDecision = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Content, otherContent.Id, ResourcePermission.ApprovalReview
        ));
        Assert.False(reviewDecision.Allowed);

        // Max role resolution check
        var maxRole = EffectivePermissionContext.ResolveMaxRole(new[] { TeamRoleEnum.Viewer, TeamRoleEnum.ContentCreator });
        Assert.Equal(TeamRoleEnum.ContentCreator, maxRole);
    }

    [Fact]
    public async Task MaxPrivilege_CrossBrandIsolation_DifferentRolesOnDifferentBrandsDoNotLeak()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var brandAlpha = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand Alpha" };
        var brandBeta = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand Beta" };

        var teamAlpha = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Alpha", Status = TeamStatusEnum.Active };
        var teamBeta = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Beta", Status = TeamStatusEnum.Active };

        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", FullName = "User", IsActive = true };
        var creator = new User { Id = Guid.NewGuid(), Email = "creator@test.com", FullName = "Creator", IsActive = true };

        var contentAlpha = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brandAlpha.Id,
            TeamId = teamAlpha.Id,
            PrimaryCreatorId = creator.Id,
            Status = ContentStatusEnum.PendingApproval
        };

        var contentBeta = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brandBeta.Id,
            TeamId = teamBeta.Id,
            PrimaryCreatorId = creator.Id,
            Status = ContentStatusEnum.PendingApproval
        };

        db.AddRange(workspace, brandAlpha, brandBeta, teamAlpha, teamBeta, user, creator,
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = user.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creator.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new TeamBrand { TeamId = teamAlpha.Id, BrandId = brandAlpha.Id, IsActive = true },
            new TeamBrand { TeamId = teamBeta.Id, BrandId = brandBeta.Id, IsActive = true },
            // User is Manager in Team Alpha, but Viewer in Team Beta
            new TeamMember { TeamId = teamAlpha.Id, UserId = user.Id, Role = TeamRoleEnum.Manager, IsActive = true },
            new TeamMember { TeamId = teamBeta.Id, UserId = user.Id, Role = TeamRoleEnum.Viewer, IsActive = true },
            new TeamMember { TeamId = teamAlpha.Id, UserId = creator.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            new TeamMember { TeamId = teamBeta.Id, UserId = creator.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            contentAlpha, contentBeta
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var accessControl = new AccessControlService(db);

        // 1. On Brand Alpha (Manager): CAN review
        var reviewAlpha = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Content, contentAlpha.Id, ResourcePermission.ApprovalReview
        ));
        Assert.True(reviewAlpha.Allowed);

        // 2. On Brand Beta (Viewer): CANNOT review (privilege does not leak across brands!)
        var reviewBeta = await accessControl.CheckAsync(new AccessRequest(
            user.Id, workspace.Id, AccessResourceKind.Content, contentBeta.Id, ResourcePermission.ApprovalReview
        ));
        Assert.False(reviewBeta.Allowed);
    }

    [Fact]
    public async Task ViewerStrictTeamDataIsolation_SeesOnlyApprovedOrPublishedOfOwnTeam_DraftAndOtherTeamsHidden()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), WorkspaceType = WorkspaceTypeEnum.Business };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand 1" };
        var teamAlpha = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Alpha", Status = TeamStatusEnum.Active };
        var teamBeta = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Beta", Status = TeamStatusEnum.Active };

        var viewer = new User { Id = Guid.NewGuid(), Email = "viewer@test.com", IsActive = true };
        var creator = new User { Id = Guid.NewGuid(), Email = "creator@test.com", IsActive = true };

        var c1ApprovedAlpha = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamAlpha.Id, PrimaryCreatorId = creator.Id, Status = ContentStatusEnum.Approved };
        var c2PublishedAlpha = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamAlpha.Id, PrimaryCreatorId = creator.Id, Status = ContentStatusEnum.Published };
        var c3DraftAlpha = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamAlpha.Id, PrimaryCreatorId = creator.Id, Status = ContentStatusEnum.Draft };
        var c4PendingAlpha = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamAlpha.Id, PrimaryCreatorId = creator.Id, Status = ContentStatusEnum.PendingApproval };

        var c5ApprovedBeta = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamBeta.Id, PrimaryCreatorId = creator.Id, Status = ContentStatusEnum.Approved };
        var c6PublishedBeta = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamBeta.Id, PrimaryCreatorId = creator.Id, Status = ContentStatusEnum.Published };
        var c7DraftBeta = new Content { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = teamBeta.Id, PrimaryCreatorId = creator.Id, Status = ContentStatusEnum.Draft };

        db.AddRange(workspace, brand, teamAlpha, teamBeta, viewer, creator,
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = viewer.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creator.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new TeamBrand { TeamId = teamAlpha.Id, BrandId = brand.Id, IsActive = true },
            new TeamBrand { TeamId = teamBeta.Id, BrandId = brand.Id, IsActive = true },
            new TeamMember { TeamId = teamAlpha.Id, UserId = viewer.Id, Role = TeamRoleEnum.Viewer, IsActive = true },
            new TeamMember { TeamId = teamAlpha.Id, UserId = creator.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            new TeamMember { TeamId = teamBeta.Id, UserId = creator.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            c1ApprovedAlpha, c2PublishedAlpha, c3DraftAlpha, c4PendingAlpha, c5ApprovedBeta, c6PublishedBeta, c7DraftBeta
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // Configure EF Core Query Filter for Viewer in Team Alpha
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = workspace.Id;
        db.PermissionActorId = viewer.Id;
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = false;
        db.PermissionManager = false;
        db.PermissionCreator = false; // Pure Viewer
        db.PermissionBrandIds = [brand.Id];
        db.PermissionTeamIds = [teamAlpha.Id];

        var visibleContents = await db.Contents.ToListAsync();

        // 1. Only C1 (Approved) and C2 (Published) of Team Alpha are visible
        Assert.Equal(2, visibleContents.Count);
        Assert.Contains(visibleContents, c => c.Id == c1ApprovedAlpha.Id);
        Assert.Contains(visibleContents, c => c.Id == c2PublishedAlpha.Id);

        // 2. Draft and Pending of Team Alpha are strictly hidden from Viewer
        Assert.DoesNotContain(visibleContents, c => c.Id == c3DraftAlpha.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == c4PendingAlpha.Id);

        // 3. ALL contents of Team Beta are strictly hidden
        Assert.DoesNotContain(visibleContents, c => c.Id == c5ApprovedBeta.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == c6PublishedBeta.Id);
        Assert.DoesNotContain(visibleContents, c => c.Id == c7DraftBeta.Id);
    }

    [Fact]
    public async Task LegacyContentBackfill_DefaultTeamVisibility_IsolatedFromOtherTeams()
    {
        await using var db = CreateInMemoryContext();
        var workspace = new Workspace { Id = Guid.NewGuid(), WorkspaceType = WorkspaceTypeEnum.Business };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand 1" };

        var defaultTeam = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Default Team", Status = TeamStatusEnum.Active };
        var otherTeam = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Gamma", Status = TeamStatusEnum.Active };

        var defaultMemberUser = new User { Id = Guid.NewGuid(), Email = "defaultmember@test.com", IsActive = true };
        var otherMemberUser = new User { Id = Guid.NewGuid(), Email = "othermember@test.com", IsActive = true };
        var wsManagerUser = new User { Id = Guid.NewGuid(), Email = "wsmanager@test.com", IsActive = true };

        // Legacy content backfilled into Default Team (D-02)
        var legacyContent = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = defaultTeam.Id, // backfilled
            PrimaryCreatorId = defaultMemberUser.Id,
            Status = ContentStatusEnum.Approved
        };

        // New content in Team Gamma
        var gammaContent = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = otherTeam.Id,
            PrimaryCreatorId = otherMemberUser.Id,
            Status = ContentStatusEnum.Approved
        };

        db.AddRange(workspace, brand, defaultTeam, otherTeam, defaultMemberUser, otherMemberUser, wsManagerUser,
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = defaultMemberUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = otherMemberUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = wsManagerUser.Id, Role = WorkspaceMemberRoleEnum.WorkspaceManager, IsActive = true },
            new TeamBrand { TeamId = defaultTeam.Id, BrandId = brand.Id, IsActive = true },
            new TeamBrand { TeamId = otherTeam.Id, BrandId = brand.Id, IsActive = true },
            new TeamMember { TeamId = defaultTeam.Id, UserId = defaultMemberUser.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            new TeamMember { TeamId = otherTeam.Id, UserId = otherMemberUser.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            legacyContent, gammaContent
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // 1. Default Member queries contents -> sees legacy content, does NOT see Team Gamma
        db.PermissionScopeEnabled = true;
        db.PermissionWorkspaceId = workspace.Id;
        db.PermissionActorId = defaultMemberUser.Id;
        db.PermissionOwner = false;
        db.PermissionWorkspaceManager = false;
        db.PermissionCreator = true;
        db.PermissionBrandIds = [brand.Id];
        db.PermissionTeamIds = [defaultTeam.Id];

        var defaultMemberContents = await db.Contents.ToListAsync();
        Assert.Single(defaultMemberContents);
        Assert.Equal(legacyContent.Id, defaultMemberContents[0].Id);

        // 2. Other Member (Team Gamma) queries contents -> sees gamma content, does NOT see legacy content
        db.ChangeTracker.Clear();
        db.PermissionActorId = otherMemberUser.Id;
        db.PermissionTeamIds = [otherTeam.Id];

        var otherMemberContents = await db.Contents.ToListAsync();
        Assert.Single(otherMemberContents);
        Assert.Equal(gammaContent.Id, otherMemberContents[0].Id);

        // 3. WorkspaceManager queries contents -> sees BOTH legacy and gamma content
        db.ChangeTracker.Clear();
        db.PermissionActorId = wsManagerUser.Id;
        db.PermissionWorkspaceManager = true;
        db.PermissionTeamIds = [];

        var wsManagerContents = await db.Contents.ToListAsync();
        Assert.Equal(2, wsManagerContents.Count);
        Assert.Contains(wsManagerContents, c => c.Id == legacyContent.Id);
        Assert.Contains(wsManagerContents, c => c.Id == gammaContent.Id);
    }
}
