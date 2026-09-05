using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AISAM.IntegrationTests;

public sealed class PermissionSecurityTests
{
    [Fact]
    public void PostgreSqlProvider_TranslatesSensitiveProjectionsWithoutConnecting()
    {
        var scope = new AccessScope { Enforced = true, WorkspaceId = Guid.NewGuid(), UserId = Guid.NewGuid(), Role = WorkspaceMemberRoleEnum.Manager };
        using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseNpgsql("Host=localhost;Database=sql_translation_only").Options, scope);
        var campaignSql = db.CampaignMetadata(scope.WorkspaceId).ToQueryString();
        Assert.Contains("workspace_id", campaignSql);
        Assert.Contains("CASE", campaignSql);
        Assert.Contains("workspace_id", db.AutomationItemsForAnalytics(scope.WorkspaceId).ToQueryString());
        Assert.Contains("workspace_id", db.MemberDirectory(scope.WorkspaceId).ToQueryString());
        Assert.Contains("workspace_id", db.WorkspaceUsageContents(scope.WorkspaceId).ToQueryString());
    }

    [Theory]
    [InlineData("brand")]
    [InlineData("task")]
    [InlineData("participation")]
    [InlineData("grant")]
    public async Task PermissionRelations_RejectCrossWorkspaceWrites(string relation)
    {
        await using var f = await Fixture.CreateAsync();
        var task = await f.AddTask();
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        switch (relation)
        {
            case "brand": f.Db.TeamBrands.Add(new TeamBrand { TeamId = f.Team.Id, BrandId = f.ForeignContent.BrandId }); break;
            case "task": task.ContentId = f.ForeignContent.Id; break;
            case "participation": f.Db.ContentParticipations.Add(new ContentParticipation { WorkspaceId = f.Workspace.Id, ContentId = f.ForeignContent.Id, UserId = f.Creator.Id, RecordedBy = f.Owner.Id }); break;
            case "grant": f.Db.TemporaryAccessGrants.Add(new TemporaryAccessGrant { WorkspaceId = f.OtherWorkspace.Id, TaskId = task.Id, UserId = f.Creator.Id, GrantedBy = f.Owner.Id, GrantedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1), Reason = "Test" }); break;
        }
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task TemporaryEdit_DoesNotSurviveRemovalFromAssignedTeam()
    {
        await using var f = await Fixture.CreateAsync();
        await f.AddGrant(DateTime.UtcNow.AddHours(1));
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
        var membership = await f.Db.TeamMembers.SingleAsync(m => m.UserId == f.Creator.Id);
        membership.IsActive = false; f.Db.SaveChanges();
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
    }

    [Theory]
    [InlineData("display", false)]
    [InlineData("nested_runtime_configuration", true)]
    [InlineData("credential_rotation", true)]
    public async Task SettingAudit_NeverStoresArbitraryInput(string key, bool batch)
    {
        await using var f = await Fixture.CreateAsync();
        f.Owner.Role = UserRoleEnum.Admin; f.Db.SaveChanges();
        var service = new AdminSettingsService(new UserRepository(f.Db), new SystemSettingRepository(f.Db), new AuditLogRepository(f.Db));
        var synthetic = "SYNTHETIC_TEST_ONLY_" + Guid.NewGuid().ToString("N");
        var value = JsonSerializer.Serialize(new { connection = new { credential = synthetic } });
        if (batch) await service.UpsertSettingsBatchAsync(f.Owner.Id, new() { [key] = value });
        else await service.UpsertSettingAsync(f.Owner.Id, key, value, synthetic);
        var audit = await f.Db.AuditLogs.AsNoTracking().SingleAsync(a => a.ActionType == "UPDATE_SYSTEM_SETTING");
        Assert.Null(audit.OldValues); Assert.Null(audit.NewValues);
        Assert.DoesNotContain(synthetic, JsonSerializer.Serialize(audit));
        Assert.Equal(f.Owner.Id, audit.ActorId);
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer)]
    public async Task CampaignMetadata_DoesNotSerializeProtectedMetrics(WorkspaceMemberRoleEnum role)
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(role);
        var results = await f.Db.CampaignMetadata(f.Workspace.Id).ToListAsync();
        Assert.Equal(2, results.Count);
        foreach (var campaign in results)
        {
            Assert.False(campaign.CanViewAnalytics);
            Assert.Null(campaign.Impressions);
            Assert.Null(campaign.Spend);
            var json = JsonSerializer.Serialize(campaign);
            Assert.DoesNotContain("\"Impressions\"", json);
            Assert.DoesNotContain("\"Conversions\"", json);
        }
        Assert.Contains("CASE", f.Db.CampaignMetadata(f.Workspace.Id).ToQueryString());
    }

    [Fact]
    public async Task ManagerCampaignMetrics_ExcludeDeniedChannel_AndRevokeIsImmediate()
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        Assert.Equal(f.AllowedCampaign.Id, (await f.Db.CampaignsForAnalytics().SingleAsync()).Id);
        var metadata = await f.Db.CampaignMetadata(f.Workspace.Id).ToListAsync();
        Assert.True(metadata.Single(c => c.Id == f.AllowedCampaign.Id).CanViewAnalytics);
        Assert.Null(metadata.Single(c => c.Id == f.DeniedCampaign.Id).Impressions);
        f.Db.TeamChannelAccesses.RemoveRange(await f.Db.TeamChannelAccesses.ToListAsync());
        f.Db.SaveChanges();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        Assert.Empty(await f.Db.CampaignsForAnalytics().ToListAsync());
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.Owner)]
    [InlineData(WorkspaceMemberRoleEnum.Manager)]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer)]
    public async Task WorkspaceAccounting_IsIndependentOfVisibility(WorkspaceMemberRoleEnum role)
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(role);
        var repository = new SubscriptionRepository(f.Db);
        Assert.Equal(2, await repository.CountSuccessfulPromptUsageByWorkspaceIdAsync(f.Workspace.Id, DateTime.UtcNow.AddDays(-1), null));
        Assert.Throws<UnauthorizedAccessException>(() => f.Db.WorkspaceUsageContents(f.OtherWorkspace.Id));
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer)]
    public async Task MemberDirectory_DoesNotExposeOtherUsersCredit(WorkspaceMemberRoleEnum role)
    {
        await using var f = await Fixture.CreateAsync();
        await f.Resolve(role);
        var members = await f.Db.MemberDirectory(f.Workspace.Id).ToListAsync();
        Assert.NotEmpty(members);
        foreach (var member in members.Where(m => role == WorkspaceMemberRoleEnum.Viewer || m.UserId != f.Db.AccessScope.UserId))
        {
            Assert.Null(member.CreditUsed); Assert.Null(member.CreditLimit); Assert.Null(member.QuotaMode);
        }
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.Manager)]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer)]
    public async Task MemberDirectory_OmitsMembersOutsideRequestersVisibleTeams(WorkspaceMemberRoleEnum role)
    {
        await using var f = await Fixture.CreateAsync();
        var outsider = new User
        {
            Email = $"{Guid.NewGuid():N}@example.test",
            PasswordHash = "test-only",
            PasswordSalt = "test-only"
        };
        var otherTeam = new Team
        {
            WorkspaceId = f.Workspace.Id,
            Name = "Other team",
            Status = TeamStatusEnum.Active
        };
        f.Db.Users.Add(outsider);
        f.Db.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = f.Workspace.Id,
            UserId = outsider.Id,
            Role = WorkspaceMemberRoleEnum.ContentCreator
        });
        f.Db.Teams.Add(otherTeam);
        f.Db.TeamMembers.Add(new TeamMember
        {
            TeamId = otherTeam.Id,
            UserId = outsider.Id,
            Role = nameof(WorkspaceMemberRoleEnum.ContentCreator)
        });
        f.Db.SaveChanges();

        await f.Resolve(role);
        var members = await f.Db.MemberDirectory(f.Workspace.Id).ToListAsync();

        Assert.DoesNotContain(members, member => member.UserId == outsider.Id);
        Assert.Contains(members, member => member.UserId == f.Db.AccessScope.UserId);
    }

    [Fact]
    public async Task MemberDirectory_OwnerStillSeesWorkspaceMembersOutsideAnySingleTeam()
    {
        await using var f = await Fixture.CreateAsync();
        var outsider = new User
        {
            Email = $"{Guid.NewGuid():N}@example.test",
            PasswordHash = "test-only",
            PasswordSalt = "test-only"
        };
        f.Db.Users.Add(outsider);
        f.Db.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = f.Workspace.Id,
            UserId = outsider.Id,
            Role = WorkspaceMemberRoleEnum.ContentCreator
        });
        f.Db.SaveChanges();

        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        var members = await f.Db.MemberDirectory(f.Workspace.Id).ToListAsync();

        Assert.Contains(members, member => member.UserId == outsider.Id);
    }

    [Fact]
    public async Task HistoricalView_AfterTeamRemoval_DoesNotRestoreBrandAccess()
    {
        await using var f = await Fixture.CreateAsync();
        var link = await f.Db.TeamMembers.SingleAsync(m => m.UserId == f.Creator.Id);
        link.IsActive = false; f.Db.SaveChanges();
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.Empty(f.Db.AccessScope.BrandIds);
        Assert.Empty(await f.Db.Brands.ToListAsync());
        Assert.Empty(await f.Db.Products.ToListAsync());
        Assert.Empty(await f.Db.AdCampaigns.ToListAsync());
        Assert.NotNull(await new ContentRepository(f.Db).GetByIdAsync(f.OwnContent.Id));
        Assert.Null(await new ContentRepository(f.Db).GetByIdAsync(f.OtherContent.Id));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OwnContent.Id, ContentAction.Delete));
        Assert.False(await f.Authorization.AllowsAsync(f.OtherWorkspace.Id, f.ForeignContent.Id, ContentAction.View));
        var member = await f.Db.WorkspaceMembers.SingleAsync(m => m.UserId == f.Creator.Id && m.WorkspaceId == f.Workspace.Id);
        member.IsActive = false; f.Db.SaveChanges();
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OwnContent.Id, ContentAction.View));
    }

    [Theory]
    [InlineData(ContentAction.Edit, true)]
    [InlineData(ContentAction.Delete, false)]
    [InlineData(ContentAction.Clone, false)]
    [InlineData(ContentAction.Assign, false)]
    [InlineData(ContentAction.Publish, false)]
    public async Task TemporaryCanEdit_DoesNotGrantOtherActions(ContentAction action, bool expected)
    {
        await using var f = await Fixture.CreateAsync();
        await f.AddGrant(DateTime.UtcNow.AddMinutes(5));
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.Equal(expected, await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, action));
    }

    [Theory]
    [InlineData(-60, false)]
    [InlineData(0, false)]
    [InlineData(600, true)]
    public async Task TemporaryExpiry_IsEnforcedWithoutWorker(int seconds, bool expected)
    {
        await using var f = await Fixture.CreateAsync();
        var grant = await f.AddGrant(DateTime.UtcNow.AddSeconds(seconds));
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.Equal(expected, await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
        grant.RevokedAt = DateTime.UtcNow; f.Db.SaveChanges();
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
        Assert.Equal(CollaborationTaskStatus.Pending, (await f.Db.CollaborationTasks.IgnoreQueryFilters().SingleAsync()).Status);
    }

    [Fact]
    public async Task AssignedPendingTask_LosesEditImmediatelyWhenChannelRevoked()
    {
        await using var f = await Fixture.CreateAsync();
        await f.AddTask();
        var channel = new TeamChannelAccess { TeamBrandId = f.TeamBrand.Id, IntegrationId = f.DeniedChannel.Id };
        f.Db.TeamChannelAccesses.Add(channel); f.Db.SaveChanges();
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
        f.Db.TeamChannelAccesses.Remove(channel); f.Db.SaveChanges();
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
        Assert.DoesNotContain(f.OtherContent.Id, f.Db.AccessScope.EditableContentIds);
    }

    [Fact]
    public async Task AutomationCounts_UseOnlyAuthorizedItems()
    {
        await using var f = await Fixture.CreateAsync();
        var plan = new AutomationPlan { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id };
        foreach (var channel in new[] { f.AllowedChannel, f.DeniedChannel })
        {
            var calendar = new ContentCalendar { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, ContentId = f.OwnContent.Id, IntegrationId = channel.Id };
            f.Db.ContentCalendars.Add(calendar);
            plan.Items.Add(new AutomationItem { AutomationPlanId = plan.Id, RowIndex = plan.Items.Count, IdempotencyKey = Guid.NewGuid().ToString(),
                BrandId = f.Brand.Id, ContentId = f.OwnContent.Id, ContentCalendarId = calendar.Id, Status = AutomationItemStatusEnum.Scheduled });
        }
        f.Db.AutomationPlans.Add(plan); f.Db.SaveChanges();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        var result = await new AutomationRepository(f.Db).GetPerformanceAsync(f.Workspace.Id, plan.Id);
        Assert.NotNull(result); Assert.Equal(1, result.TotalItems); Assert.Equal(1, result.ScheduledItems);
    }

    internal sealed class Fixture : IAsyncDisposable
    {
        public SqliteConnection Connection { get; } = new("Data Source=:memory:");
        public AisamContext Db { get; private set; } = null!;
        public ResourceAccessService Resolver => new(Db);
        public ContentAuthorizationService Authorization => new(Db, Resolver, new CollaborationAccessService(Db));
        public Workspace Workspace { get; } = new() { Name = "Security workspace" };
        public Workspace OtherWorkspace { get; } = new() { Name = "Other workspace" };
        public User Creator { get; } = NewUser();
        public User OtherCreator { get; } = NewUser();
        public User Manager { get; } = NewUser();
        public User Viewer { get; } = NewUser();
        public User Owner { get; } = NewUser();
        public Profile Profile { get; private set; } = null!;
        public Brand Brand { get; private set; } = null!;
        public Content OwnContent { get; private set; } = null!;
        public Content OtherContent { get; private set; } = null!;
        public Content ForeignContent { get; private set; } = null!;
        public Team Team { get; private set; } = null!;
        public TeamBrand TeamBrand { get; private set; } = null!;
        public SocialIntegration AllowedChannel { get; private set; } = null!;
        public SocialIntegration DeniedChannel { get; private set; } = null!;
        public AdCampaign AllowedCampaign { get; private set; } = null!;
        public AdCampaign DeniedCampaign { get; private set; } = null!;
        public static async Task<Fixture> CreateAsync()
        {
            var f = new Fixture(); await f.Connection.OpenAsync();
            f.Db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.Connection).Options);
            await f.Db.Database.EnsureCreatedAsync();
            f.Db.Users.AddRange(f.Creator, f.OtherCreator, f.Manager, f.Viewer, f.Owner);
            f.Profile = new Profile { UserId = f.Creator.Id, Name = "Creator" };
            f.Db.Profiles.Add(f.Profile); f.Db.Workspaces.AddRange(f.Workspace, f.OtherWorkspace);
            f.Brand = new Brand { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, Name = "Brand" };
            var foreignBrand = new Brand { WorkspaceId = f.OtherWorkspace.Id, ProfileId = f.Profile.Id, Name = "Foreign brand" };
            f.Db.Brands.AddRange(f.Brand, foreignBrand);
            f.OwnContent = f.Content(f.Workspace, f.Brand, f.Creator);
            f.OtherContent = f.Content(f.Workspace, f.Brand, f.OtherCreator);
            f.ForeignContent = f.Content(f.OtherWorkspace, foreignBrand, f.Creator);
            f.Db.Contents.AddRange(f.OwnContent, f.OtherContent, f.ForeignContent);
            f.Team = new Team { WorkspaceId = f.Workspace.Id, Name = "Team", Status = TeamStatusEnum.Active };
            f.Db.Teams.Add(f.Team);
            foreach (var (user, role) in new[] { (f.Creator, WorkspaceMemberRoleEnum.ContentCreator), (f.OtherCreator, WorkspaceMemberRoleEnum.ContentCreator),
                (f.Manager, WorkspaceMemberRoleEnum.Manager), (f.Viewer, WorkspaceMemberRoleEnum.Viewer), (f.Owner, WorkspaceMemberRoleEnum.Owner) })
            {
                f.Db.WorkspaceMembers.Add(new WorkspaceMember { WorkspaceId = f.Workspace.Id, UserId = user.Id, Role = role, CreditUsed = 42, CreditLimit = 100 });
                f.Db.TeamMembers.Add(new TeamMember { TeamId = f.Team.Id, UserId = user.Id, Role = role.ToString() });
            }
            var account = new SocialAccount { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id };
            f.Db.SocialAccounts.Add(account);
            f.AllowedChannel = new SocialIntegration { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, BrandId = f.Brand.Id, SocialAccountId = account.Id, Platform = SocialPlatformEnum.Facebook };
            f.DeniedChannel = new SocialIntegration { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, BrandId = f.Brand.Id, SocialAccountId = account.Id, Platform = SocialPlatformEnum.TikTok };
            f.Db.SocialIntegrations.AddRange(f.AllowedChannel, f.DeniedChannel);
            f.TeamBrand = new TeamBrand { TeamId = f.Team.Id, BrandId = f.Brand.Id, ChannelAccessMode = ChannelAccessMode.Specific };
            f.TeamBrand.Channels.Add(new TeamChannelAccess { TeamBrandId = f.TeamBrand.Id, IntegrationId = f.AllowedChannel.Id });
            f.Db.TeamBrands.Add(f.TeamBrand);
            f.AllowedCampaign = new AdCampaign { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, BrandId = f.Brand.Id, IntegrationId = f.AllowedChannel.Id, Impressions = 10, Spend = 20 };
            f.DeniedCampaign = new AdCampaign { WorkspaceId = f.Workspace.Id, ProfileId = f.Profile.Id, BrandId = f.Brand.Id, IntegrationId = f.DeniedChannel.Id, Impressions = 999, Spend = 999 };
            f.Db.AdCampaigns.AddRange(f.AllowedCampaign, f.DeniedCampaign); f.Db.SaveChanges();
            return f;
        }
        private static User NewUser() => new() { Email = $"{Guid.NewGuid():N}@example.test", PasswordHash = "test-only", PasswordSalt = "test-only" };
        private Content Content(Workspace workspace, Brand brand, User creator) => new() { WorkspaceId = workspace.Id, ProfileId = Profile.Id, BrandId = brand.Id,
            PrimaryCreatorId = creator.Id, TextContent = "Test content", IsAiGenerated = true };
        public Task<AccessScope> Resolve(WorkspaceMemberRoleEnum role) => Resolver.ResolveAsync(Workspace.Id,
            role switch { WorkspaceMemberRoleEnum.Owner => Owner.Id, WorkspaceMemberRoleEnum.Manager => Manager.Id, WorkspaceMemberRoleEnum.Viewer => Viewer.Id, _ => Creator.Id }, false);
        public async Task<CollaborationTask> AddTask()
        {
            var task = new CollaborationTask { WorkspaceId = Workspace.Id, TeamId = Team.Id, ContentId = OtherContent.Id, AssigneeId = Creator.Id,
                AssignedBy = Owner.Id, IntegrationId = DeniedChannel.Id, Title = "Collaboration", Status = CollaborationTaskStatus.Pending };
            Db.CollaborationTasks.Add(task); Db.SaveChanges(); await Task.CompletedTask; return task;
        }
        public async Task<TemporaryAccessGrant> AddGrant(DateTime expires)
        {
            var task = await AddTask();
            var grant = new TemporaryAccessGrant { WorkspaceId = Workspace.Id, TaskId = task.Id, UserId = Creator.Id, GrantedBy = Owner.Id,
                GrantedAt = DateTime.UtcNow.AddHours(-1), ExpiresAt = expires, Reason = "Security test", CanEdit = true };
            Db.TemporaryAccessGrants.Add(grant); Db.SaveChanges(); return grant;
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await Connection.DisposeAsync(); }
    }
}
