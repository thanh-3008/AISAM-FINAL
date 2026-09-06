using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text.Json;
using Fixture = AISAM.IntegrationTests.PermissionSecurityTests.Fixture;

namespace AISAM.IntegrationTests;

public sealed class MemberPerformanceAndMultiVideoTests
{
    // ---------------------------------------------------------------------------
    // Member Performance Analytics Tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task MembersPerformance_CreatorAndViewer_ReturnsForbidden()
    {
        await using var f = await Fixture.CreateAsync();

        // Test Creator
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        var creatorService = CreateAnalyticsService(f.Db);

        var creatorResult = await creatorService.GetMembersPerformanceAsync(
            f.Workspace.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(1));
        Assert.False(creatorResult.Success);
        Assert.Equal(403, (int)creatorResult.StatusCode);

        // Test Viewer
        await f.Resolve(WorkspaceMemberRoleEnum.Viewer);
        var viewerService = CreateAnalyticsService(f.Db);

        var viewerResult = await viewerService.GetMembersPerformanceAsync(
            f.Workspace.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(1));
        Assert.False(viewerResult.Success);
        Assert.Equal(403, (int)viewerResult.StatusCode);
    }

    [Fact]
    public async Task MembersPerformance_Manager_SeesOnlyOwnTeamMembers()
    {
        await using var f = await Fixture.CreateAsync();

        // Create a second team with a separate creator
        f.Db.AccessScope.Enforced = false;
        var team2 = new Team
        {
            Id = Guid.NewGuid(),
            WorkspaceId = f.Workspace.Id,
            Name = "Team 2",
            Status = TeamStatusEnum.Active
        };
        var outsiderUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"team2creator_{Guid.NewGuid():N}@test.com",
            FullName = "Team 2 Creator",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Role = UserRoleEnum.User,
        };
        var team2Member = new TeamMember
        {
            TeamId = team2.Id,
            UserId = outsiderUser.Id,
            Role = WorkspaceMemberRoleEnum.ContentCreator.ToString(),
            IsActive = true,
        };
        var team2WorkspaceMember = new WorkspaceMember
        {
            WorkspaceId = f.Workspace.Id,
            UserId = outsiderUser.Id,
            Role = WorkspaceMemberRoleEnum.ContentCreator,
            IsActive = true,
        };
        f.Db.Teams.Add(team2);
        f.Db.Users.Add(outsiderUser);
        f.Db.TeamMembers.Add(team2Member);
        f.Db.WorkspaceMembers.Add(team2WorkspaceMember);
        await f.Db.SaveChangesAsync();

        // Resolve as Manager of Team 1
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        var managerService = CreateAnalyticsService(f.Db);

        var managerResult = await managerService.GetMembersPerformanceAsync(
            f.Workspace.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(1));

        Assert.True(managerResult.Success);
        Assert.NotNull(managerResult.Data);

        var members = managerResult.Data.Members;
        // Manager must see Creator from Team 1
        Assert.Contains(members, m => m.UserId == f.Creator.Id);
        // Manager must NOT see outsiderUser from Team 2
        Assert.DoesNotContain(members, m => m.UserId == outsiderUser.Id);
    }

    [Fact]
    public async Task MembersPerformance_Owner_CanSeeAllWorkspaceMembers_WithAggregatedMetrics()
    {
        await using var f = await Fixture.CreateAsync();

        // Add dummy contents with various statuses for f.Creator
        f.Db.AccessScope.Enforced = false;
        var draftContent = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = f.Workspace.Id,
            TeamId = f.Team.Id,
            ProfileId = f.Profile.Id,
            BrandId = f.Brand.Id,
            PrimaryCreatorId = f.Creator.Id,
            Status = ContentStatusEnum.Draft,
            TextContent = "Draft content",
            CreatedAt = DateTime.UtcNow,
        };
        var approvedContent = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = f.Workspace.Id,
            TeamId = f.Team.Id,
            ProfileId = f.Profile.Id,
            BrandId = f.Brand.Id,
            PrimaryCreatorId = f.Creator.Id,
            Status = ContentStatusEnum.Approved,
            TextContent = "Approved content",
            CreatedAt = DateTime.UtcNow,
        };
        var publishedContent = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = f.Workspace.Id,
            TeamId = f.Team.Id,
            ProfileId = f.Profile.Id,
            BrandId = f.Brand.Id,
            PrimaryCreatorId = f.Creator.Id,
            Status = ContentStatusEnum.Published,
            TextContent = "Published content",
            CreatedAt = DateTime.UtcNow,
        };
        f.Db.Contents.AddRange(draftContent, approvedContent, publishedContent);

        // Add Post for published content
        var post = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = publishedContent.Id,
            IntegrationId = f.AllowedChannel.Id,
            PublishedAt = DateTime.UtcNow,
            Status = ContentStatusEnum.Published,
        };
        f.Db.Posts.Add(post);

        // Add performance report for published post
        var report = new PerformanceReport
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            Impressions = 1500,
            Engagement = 120,
            Clicks = 45,
            ReportDate = DateTime.UtcNow.Date,
        };
        f.Db.PerformanceReports.Add(report);
        await f.Db.SaveChangesAsync();

        // Resolve as Owner
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        var ownerService = CreateAnalyticsService(f.Db);

        var ownerResult = await ownerService.GetMembersPerformanceAsync(
            f.Workspace.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(1));

        Assert.True(ownerResult.Success);
        Assert.NotNull(ownerResult.Data);

        var creatorPerf = ownerResult.Data.Members.FirstOrDefault(m => m.UserId == f.Creator.Id);
        Assert.NotNull(creatorPerf);
        Assert.True(creatorPerf.TotalContentCreated >= 3);
        Assert.True(creatorPerf.DraftCount >= 1);
        Assert.True(creatorPerf.ApprovedCount >= 1);
        Assert.True(creatorPerf.PublishedCount >= 1);
        Assert.True(creatorPerf.TotalImpressions >= 1500);
        Assert.True(creatorPerf.TotalEngagement >= 120);
        Assert.True(creatorPerf.TotalClicks >= 45);
        Assert.True(creatorPerf.EngagementRate > 0);
    }

    // ---------------------------------------------------------------------------
    // Multi-Video Storage & Mapping Tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ContentService_CreateWithVideoUrls_PersistsJsonbAndLegacyField()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.AccessScope.Enforced = false;

        var service = CreateContentService(f.Db);

        var videoList = new List<string>
        {
            "https://cdn.example/video1.mp4",
            "https://cdn.example/video2.mp4"
        };

        var createReq = new CreateContentRequest
        {
            BrandId = f.Brand.Id,
            AdType = AdTypeEnum.VideoText,
            Title = "Multi-video test",
            TextContent = "Multi-video body",
            VideoUrls = videoList,
            Status = ContentStatusEnum.Draft,
        };

        var response = await service.CreateAsync(f.Profile.Id, createReq);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("https://cdn.example/video1.mp4", response.Data.VideoUrl);
        Assert.NotNull(response.Data.VideoUrls);
        Assert.Equal(2, response.Data.VideoUrls.Count);
        Assert.Equal("https://cdn.example/video1.mp4", response.Data.VideoUrls[0]);
        Assert.Equal("https://cdn.example/video2.mp4", response.Data.VideoUrls[1]);

        // Check in DB directly
        var contentInDb = await f.Db.Contents.FindAsync(response.Data.Id);
        Assert.NotNull(contentInDb);
        Assert.Equal("https://cdn.example/video1.mp4", contentInDb.VideoUrl);
        Assert.NotNull(contentInDb.VideoUrls);
        var parsed = JsonSerializer.Deserialize<List<string>>(contentInDb.VideoUrls);
        Assert.Equal(2, parsed!.Count);
    }

    [Fact]
    public async Task ContentService_LegacyCreateWithSingleVideoUrl_MapsBothVideoUrlAndVideoUrls()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.AccessScope.Enforced = false;

        var service = CreateContentService(f.Db);

        var createReq = new CreateContentRequest
        {
            BrandId = f.Brand.Id,
            AdType = AdTypeEnum.VideoText,
            Title = "Legacy single video test",
            TextContent = "Single video body",
            VideoUrl = "https://cdn.example/single.mp4",
            Status = ContentStatusEnum.Draft,
        };

        var response = await service.CreateAsync(f.Profile.Id, createReq);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("https://cdn.example/single.mp4", response.Data.VideoUrl);
        Assert.NotNull(response.Data.VideoUrls);
        Assert.Single(response.Data.VideoUrls);
        Assert.Equal("https://cdn.example/single.mp4", response.Data.VideoUrls[0]);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static AnalyticsService CreateAnalyticsService(AisamContext db)
    {
        return new AnalyticsService(
            new PerformanceReportRepository(db),
            new SocialIntegrationRepository(db),
            null!,
            null!,
            null!,
            new BrandRepository(db),
            new ContentCalendarRepository(db),
            NullLogger<AnalyticsService>.Instance,
            db.AccessScope,
            db);
    }

    private static ContentService CreateContentService(AisamContext db)
    {
        return new ContentService(
            new ContentRepository(db),
            new BrandRepository(db),
            new ProductRepository(db),
            new SocialIntegrationRepository(db),
            new SocialAccountRepository(db),
            new PostRepository(db),
            Array.Empty<IProviderService>(),
            new FakeSocialTokenProtector(),
            new FakeQuotaService(),
            new ContentCalendarRepository(db),
            new WorkspaceRepository(db),
            new NotificationRepository(db));
    }

    private sealed class FakeSocialTokenProtector : ISocialTokenProtector
    {
        public string Protect(string plaintext) => plaintext;
        public string Unprotect(string ciphertext) => ciphertext;
        public string? TryUnprotect(string ciphertext) => ciphertext;
    }

    private sealed class FakeQuotaService : IQuotaService
    {
        public Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto()));

        public Task<GenericResponse<QuotaSummaryDto>> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto()));

        public Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));

        public Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));

        public Task<GenericResponse<bool>> EnsureWorkspacePostQuotaAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
    }
}
