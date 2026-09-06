using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public sealed class ContentOwnershipBoundaryTests
{
    [Fact]
    public async Task SharedBrand_ManagerReadsOnlyOwningTeamContentAndAnalytics()
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(f.ManagerA.Id, f.TeamA.Id);

        Assert.Equal([f.ContentA.Id], await f.Db.Contents.Select(content => content.Id).ToArrayAsync());
        Assert.Equal([f.PostA.Id], await f.Db.Posts.Select(post => post.Id).ToArrayAsync());
        Assert.Equal([f.ContentA.Id], await f.Db.ContentCalendars.Select(calendar => calendar.ContentId).ToArrayAsync());
        Assert.Equal(10, await f.Db.ContentAnalyticsReports().SumAsync(report => report.Impressions));
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentA.Id, ContentAction.View));
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentA.Id, ContentAction.Edit));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentB.Id, ContentAction.View));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentB.Id, ContentAction.Edit));
    }

    [Fact]
    public async Task SharedBrand_ViewerCannotReadOtherTeamOrLegacyUnresolvedContent()
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(f.ViewerA.Id, f.TeamA.Id);

        Assert.Equal([f.ContentA.Id], await f.Db.Contents.Select(content => content.Id).ToArrayAsync());
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentA.Id, ContentAction.View));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentB.Id, ContentAction.View));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.LegacyContent.Id, ContentAction.View));
    }

    [Fact]
    public async Task LegacyUnresolvedContent_IsOwnerVisibleAndCreatorHistoryOnly()
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(f.Owner.Id, f.TeamA.Id);
        Assert.Contains(f.LegacyContent.Id, await f.Db.Contents.Select(content => content.Id).ToArrayAsync());

        await f.Resolve(f.CreatorA.Id, f.TeamA.Id);
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.LegacyContent.Id, ContentAction.View));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.LegacyContent.Id, ContentAction.Edit));
    }

    [Fact]
    public async Task TeamTransfer_RetainsHistoricalViewButDoesNotTransferOwnershipOrEditAuthority()
    {
        await using var f = await Fixture.CreateAsync();
        var oldLink = await f.Db.TeamMembers.SingleAsync(member => member.TeamId == f.TeamA.Id && member.UserId == f.CreatorA.Id);
        oldLink.IsActive = false;
        f.Db.TeamMembers.Add(new TeamMember { TeamId = f.TeamB.Id, UserId = f.CreatorA.Id, Role = nameof(WorkspaceMemberRoleEnum.ContentCreator) });
        await f.Db.SaveChangesAsync();

        await f.Resolve(f.CreatorA.Id, f.TeamB.Id);
        Assert.Equal(f.TeamA.Id, f.ContentA.TeamId);
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentA.Id, ContentAction.View));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.ContentA.Id, ContentAction.Edit));
    }

    [Fact]
    public async Task AuthenticatedCreation_AttributesActiveTeamAndRejectsAmbiguity()
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(f.CreatorA.Id, f.TeamA.Id, write: true);
        var created = await new ContentRepository(f.Db).AddAsync(f.NewContent(f.CreatorA));
        Assert.Equal(f.TeamA.Id, created.TeamId);
        Assert.Equal(f.TeamA.Id, await f.Db.AuditLogs.IgnoreQueryFilters()
            .Where(audit => audit.TargetId == created.Id)
            .Select(audit => audit.TeamId)
            .SingleAsync());

        // The owner belongs to both teams. Without an active team a shared brand is
        // deliberately ambiguous and creation must fail closed.
        await f.Resolve(f.Owner.Id, null, write: true);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new ContentRepository(f.Db).AddAsync(f.NewContent(f.Owner)));
    }

    [Fact]
    public async Task PersistenceRejectsOwnershipMutationAndCrossWorkspaceTeam()
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(f.Owner.Id, f.TeamA.Id, write: true);
        f.ContentA.TeamId = f.TeamB.Id;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Db.SaveChangesAsync());

        f.Db.ChangeTracker.Clear();
        await f.Resolve(f.Owner.Id, f.TeamA.Id, write: true);
        var foreignTeam = new Team { WorkspaceId = Guid.NewGuid(), Name = "Foreign" };
        f.Db.Teams.Add(foreignTeam);
        var invalid = f.NewContent(f.Owner); invalid.TeamId = foreignTeam.Id;
        f.Db.Contents.Add(invalid);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task ModelUsesOptionalCompositeRestrictForeignKeyForLegacySafeRollout()
    {
        await using var f = await Fixture.CreateAsync();
        var entity = f.Db.Model.FindEntityType(typeof(Content))!;
        Assert.True(entity.FindProperty(nameof(Content.TeamId))!.IsNullable);
        var foreignKey = Assert.Single(entity.GetForeignKeys().Where(key => key.PrincipalEntityType.ClrType == typeof(Team)));
        Assert.Equal([nameof(Content.TeamId), nameof(Content.WorkspaceId)], foreignKey.Properties.Select(property => property.Name));
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public AisamContext Db { get; private set; } = null!;
        public ResourceAccessService Resolver => new(Db);
        public ContentAuthorizationService Authorization => new(Db, Resolver, new CollaborationAccessService(Db));
        public Workspace Workspace { get; } = new() { Name = "Ownership boundary" };
        public User Owner { get; } = User("owner");
        public User ManagerA { get; } = User("manager-a");
        public User ManagerB { get; } = User("manager-b");
        public User CreatorA { get; } = User("creator-a");
        public User CreatorB { get; } = User("creator-b");
        public User ViewerA { get; } = User("viewer-a");
        public Profile Profile { get; private set; } = null!;
        public Brand Brand { get; private set; } = null!;
        public Team TeamA { get; } = new() { Name = "Team A", Status = TeamStatusEnum.Active };
        public Team TeamB { get; } = new() { Name = "Team B", Status = TeamStatusEnum.Active };
        public Content ContentA { get; private set; } = null!;
        public Content ContentB { get; private set; } = null!;
        public Content LegacyContent { get; private set; } = null!;
        public Post PostA { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var f = new Fixture();
            await f.connection.OpenAsync();
            f.Db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.connection).Options);
            await f.Db.Database.EnsureCreatedAsync();
            f.Profile = new Profile { UserId = f.CreatorA.Id, Name = "Creator A" };
            f.Brand = new Brand { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, Name = "Shared brand" };
            f.TeamA.WorkspaceId = f.Workspace.Id;
            f.TeamB.WorkspaceId = f.Workspace.Id;
            f.Db.AddRange(f.Owner, f.ManagerA, f.ManagerB, f.CreatorA, f.CreatorB, f.ViewerA, f.Profile, f.Workspace, f.Brand, f.TeamA, f.TeamB);
            f.AddMembership(f.Owner, WorkspaceMemberRoleEnum.Owner, f.TeamA, f.TeamB);
            f.AddMembership(f.ManagerA, WorkspaceMemberRoleEnum.Manager, f.TeamA);
            f.AddMembership(f.ManagerB, WorkspaceMemberRoleEnum.Manager, f.TeamB);
            f.AddMembership(f.CreatorA, WorkspaceMemberRoleEnum.ContentCreator, f.TeamA);
            f.AddMembership(f.CreatorB, WorkspaceMemberRoleEnum.ContentCreator, f.TeamB);
            f.AddMembership(f.ViewerA, WorkspaceMemberRoleEnum.Viewer, f.TeamA);
            f.Db.TeamBrands.AddRange(
                new TeamBrand { TeamId = f.TeamA.Id, BrandId = f.Brand.Id, ChannelAccessMode = ChannelAccessMode.All },
                new TeamBrand { TeamId = f.TeamB.Id, BrandId = f.Brand.Id, ChannelAccessMode = ChannelAccessMode.All });
            var account = new SocialAccount { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id };
            var integration = new SocialIntegration { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, BrandId = f.Brand.Id,
                SocialAccountId = account.Id, Platform = SocialPlatformEnum.Facebook };
            f.Db.AddRange(account, integration);
            f.ContentA = f.NewContent(f.CreatorA, f.TeamA.Id, "Team A content");
            f.ContentB = f.NewContent(f.CreatorB, f.TeamB.Id, "Team B content");
            f.LegacyContent = f.NewContent(f.CreatorA, null, "Unresolved legacy content");
            f.Db.Contents.AddRange(f.ContentA, f.ContentB, f.LegacyContent);
            f.PostA = new Post { ContentId = f.ContentA.Id, IntegrationId = integration.Id };
            var postB = new Post { ContentId = f.ContentB.Id, IntegrationId = integration.Id };
            f.Db.Posts.AddRange(f.PostA, postB);
            f.Db.ContentCalendars.AddRange(
                new ContentCalendar { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, ContentId = f.ContentA.Id, IntegrationId = integration.Id, ScheduledAt = DateTime.UtcNow.AddDays(1) },
                new ContentCalendar { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, ContentId = f.ContentB.Id, IntegrationId = integration.Id, ScheduledAt = DateTime.UtcNow.AddDays(2) });
            f.Db.PerformanceReports.AddRange(
                new PerformanceReport { PostId = f.PostA.Id, Impressions = 10 },
                new PerformanceReport { PostId = postB.Id, Impressions = 999 });
            await f.Db.SaveChangesAsync();
            return f;
        }

        public Task<AccessScope> Resolve(Guid userId, Guid? teamId, bool write = false) => Resolver.ResolveAsync(Workspace.Id, userId, write, teamId);

        public Content NewContent(User creator, Guid? teamId = null, string title = "New content") => new()
        {
            WorkspaceId = Workspace.Id, TeamId = teamId, ProfileId = Profile.Id, BrandId = Brand.Id,
            PrimaryCreatorId = creator.Id, Title = title, TextContent = title, AdType = AdTypeEnum.TextOnly
        };

        private void AddMembership(User user, WorkspaceMemberRoleEnum role, params Team[] teams)
        {
            Db.WorkspaceMembers.Add(new WorkspaceMember { WorkspaceId = Workspace.Id, UserId = user.Id, Role = role });
            foreach (var team in teams) Db.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = user.Id, Role = role.ToString() });
        }

        private static User User(string name) => new()
        {
            Email = $"{name}-{Guid.NewGuid():N}@example.test", PasswordHash = "test-only", PasswordSalt = "test-only"
        };

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
