using System;
using System.Linq;
using System.Threading.Tasks;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class PerformanceReportRepositoryTests
{
    private static (AisamContext Context, Microsoft.Data.Sqlite.SqliteConnection Connection) CreateContext()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseSqlite(connection)
            .Options;
            
        var context = new AisamContext(options);
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");
        
        return (context, connection);
    }

    [Fact]
    public async Task GetTopPostsForAIAsync_UsesLatestSnapshot_NotSum()
    {
        var (context, connection) = CreateContext();
        await using var _ = connection;
        await using var __ = context;
        var workspaceId = Guid.NewGuid();
        
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
        var profile = new Profile { Id = Guid.NewGuid(), UserId = user.Id, Name = "Profile", ProfileType = ProfileTypeEnum.Basic, Status = ProfileStatusEnum.Active };
        var brand = new Brand { Id = Guid.NewGuid(), ProfileId = profile.Id, Name = "Brand", WorkspaceId = workspaceId };
        var integration = new SocialIntegration { Id = Guid.NewGuid(), Platform = SocialPlatformEnum.Facebook, BrandId = brand.Id, WorkspaceId = workspaceId, ProfileId = profile.Id, ExternalId = "ext", AccessToken = "tok", TargetName = "tgt", TargetType = "type", TargetCategory = "cat" };
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = brand.Id, ProfileId = profile.Id };
        
        var postMultipleReports = new Post { Id = Guid.NewGuid(), ContentId = content.Id, Content = content, IntegrationId = integration.Id, Integration = integration, PublishedAt = DateTime.UtcNow.AddDays(-10), ExternalPostId = "ext1" };
        
        var postOneReport = new Post { Id = Guid.NewGuid(), ContentId = content.Id, Content = content, IntegrationId = integration.Id, Integration = integration, PublishedAt = DateTime.UtcNow.AddDays(-10), ExternalPostId = "ext2" };
        
        var postZeroReports = new Post { Id = Guid.NewGuid(), ContentId = content.Id, Content = content, IntegrationId = integration.Id, Integration = integration, PublishedAt = DateTime.UtcNow.AddDays(-10), ExternalPostId = "ext3" };

        var workspace = new Workspace { Id = workspaceId, Name = "Test" };
        context.Workspaces.Add(workspace);
        context.Users.Add(user);

        context.Profiles.Add(profile);
        context.Brands.Add(brand);
        context.SocialIntegrations.Add(integration);
        context.Contents.Add(content);
        context.Posts.AddRange(postMultipleReports, postOneReport, postZeroReports);

        var from = DateTime.UtcNow.AddDays(-30);
        var to = DateTime.UtcNow.AddDays(1);

        // Multiple reports snapshot simulating accumulation over time
        context.PerformanceReports.AddRange(
            new PerformanceReport { Id = Guid.NewGuid(), PostId = postMultipleReports.Id, Post = postMultipleReports, ReportDate = DateTime.UtcNow.AddDays(-5), Impressions = 10, Clicks = 2, Engagement = 5, Reach = 4 },
            new PerformanceReport { Id = Guid.NewGuid(), PostId = postMultipleReports.Id, Post = postMultipleReports, ReportDate = DateTime.UtcNow.AddDays(-1), Impressions = 50, Clicks = 10, Engagement = 25, Reach = 20 } // Latest!
        );

        // One report
        context.PerformanceReports.Add(
            new PerformanceReport { Id = Guid.NewGuid(), PostId = postOneReport.Id, Post = postOneReport, ReportDate = DateTime.UtcNow.AddDays(-2), Impressions = 20, Clicks = 5, Engagement = 10, Reach = 8 }
        );

        await context.SaveChangesAsync();

        var repo = new PerformanceReportRepository(context);

        // Act
        var topPosts = await repo.GetTopPostsForAIAsync(workspaceId, from, to);

        // Assert
        Assert.Equal(2, topPosts.Count); // Zero reports post is not returned

        // The top post should be postMultipleReports (Engagement = 25 vs 10)
        var top1 = topPosts.First();
        Assert.Equal(postMultipleReports.Id, top1.PostId);
        
        // Important: It must be exactly the snapshot values (50, 10, 25, 20), not the sum (60, 12, 30, 24)!
        Assert.Equal(50, top1.Impressions);
        Assert.Equal(10, top1.Clicks);
        Assert.Equal(25, top1.Engagement);
        Assert.Equal(20, top1.Reach);

        var top2 = topPosts.Skip(1).First();
        Assert.Equal(postOneReport.Id, top2.PostId);
        Assert.Equal(20, top2.Impressions);
        Assert.Equal(5, top2.Clicks);
        Assert.Equal(10, top2.Engagement);
    }

    [Fact]
    public async Task GetTopPostsForAIAsync_EmptyTimeframe_ReturnsEmpty()
    {
        var (context, connection) = CreateContext();
        await using var _ = connection;
        await using var __ = context;
        var workspaceId = Guid.NewGuid();
        
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
        var profile = new Profile { Id = Guid.NewGuid(), UserId = user.Id, Name = "Profile", ProfileType = ProfileTypeEnum.Basic, Status = ProfileStatusEnum.Active };
        var brand = new Brand { Id = Guid.NewGuid(), ProfileId = profile.Id, Name = "Brand", WorkspaceId = workspaceId };
        var integration = new SocialIntegration { Id = Guid.NewGuid(), Platform = SocialPlatformEnum.Facebook, BrandId = brand.Id, WorkspaceId = workspaceId, ProfileId = profile.Id, ExternalId = "ext", AccessToken = "tok", TargetName = "tgt", TargetType = "type", TargetCategory = "cat" };
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = brand.Id, ProfileId = profile.Id };
        var post = new Post { Id = Guid.NewGuid(), ContentId = content.Id, Content = content, IntegrationId = integration.Id, Integration = integration, PublishedAt = DateTime.UtcNow.AddDays(-10), ExternalPostId = "ext1" };
        var workspace = new Workspace { Id = workspaceId, Name = "Test" };
        context.Workspaces.Add(workspace);
        context.Users.Add(user);
        
        context.Profiles.Add(profile);
        context.Brands.Add(brand);
        context.SocialIntegrations.Add(integration);
        context.Contents.Add(content);
        context.Posts.Add(post);
        
        context.PerformanceReports.Add(
            new PerformanceReport { Id = Guid.NewGuid(), PostId = post.Id, Post = post, ReportDate = DateTime.UtcNow.AddDays(-20), Impressions = 20, Clicks = 5, Engagement = 10, Reach = 8 }
        );
        await context.SaveChangesAsync();

        var repo = new PerformanceReportRepository(context);

        // Act: Query for a time frame with no reports
        var topPosts = await repo.GetTopPostsForAIAsync(workspaceId, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

        // Assert
        Assert.Empty(topPosts);
    }

    [Fact]
    public async Task GetChannelBreakdownForAIAsync_AggregatesCorrectly()
    {
        var (context, connection) = CreateContext();
        await using var _ = connection;
        await using var __ = context;
        var workspaceId = Guid.NewGuid();
        
        var user = new User { Id = Guid.NewGuid(), Email = "test2@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
        var profile = new Profile { Id = Guid.NewGuid(), UserId = user.Id, Name = "Profile", ProfileType = ProfileTypeEnum.Basic, Status = ProfileStatusEnum.Active };
        var brand = new Brand { Id = Guid.NewGuid(), ProfileId = profile.Id, Name = "Brand", WorkspaceId = workspaceId };
        var integrationFacebook = new SocialIntegration { Id = Guid.NewGuid(), Platform = SocialPlatformEnum.Facebook, BrandId = brand.Id, WorkspaceId = workspaceId, ProfileId = profile.Id, ExternalId = "extfb", AccessToken = "tok", TargetName = "tgt", TargetType = "type", TargetCategory = "cat" };
        var integrationInsta = new SocialIntegration { Id = Guid.NewGuid(), Platform = SocialPlatformEnum.Instagram, BrandId = brand.Id, WorkspaceId = workspaceId, ProfileId = profile.Id, ExternalId = "extig", AccessToken = "tok", TargetName = "tgt", TargetType = "type", TargetCategory = "cat" };
        
        var content = new Content { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = brand.Id, ProfileId = profile.Id };
        
        var postFb = new Post { Id = Guid.NewGuid(), ContentId = content.Id, Content = content, IntegrationId = integrationFacebook.Id, Integration = integrationFacebook, PublishedAt = DateTime.UtcNow.AddDays(-10), ExternalPostId = "ext1" };
        var postIg = new Post { Id = Guid.NewGuid(), ContentId = content.Id, Content = content, IntegrationId = integrationInsta.Id, Integration = integrationInsta, PublishedAt = DateTime.UtcNow.AddDays(-10), ExternalPostId = "ext2" };
        
        var workspace = new Workspace { Id = workspaceId, Name = "Test" };
        context.Workspaces.Add(workspace);
        context.Users.Add(user);
        
        context.Profiles.Add(profile);
        context.Brands.Add(brand);
        context.SocialIntegrations.AddRange(integrationFacebook, integrationInsta);
        context.Contents.Add(content);
        context.Posts.AddRange(postFb, postIg);
        
        // Add 2 snapshot reports for FB post, 1 for IG post
        context.PerformanceReports.AddRange(
            new PerformanceReport { Id = Guid.NewGuid(), PostId = postFb.Id, Post = postFb, ReportDate = DateTime.UtcNow.AddDays(-5), Impressions = 10, Clicks = 2, Engagement = 5, Reach = 4 },
            new PerformanceReport { Id = Guid.NewGuid(), PostId = postFb.Id, Post = postFb, ReportDate = DateTime.UtcNow.AddDays(-1), Impressions = 50, Clicks = 10, Engagement = 25, Reach = 20 }, // latest FB
            new PerformanceReport { Id = Guid.NewGuid(), PostId = postIg.Id, Post = postIg, ReportDate = DateTime.UtcNow.AddDays(-2), Impressions = 30, Clicks = 5, Engagement = 15, Reach = 10 } // latest IG
        );

        // Add Campaign for Facebook
        var campaignFb = new AdCampaign { Id = Guid.NewGuid(), WorkspaceId = workspaceId, BrandId = brand.Id, Platform = "facebook", Spend = 100.5m, Status = CampaignStatusEnum.Active, DeploymentStatus = DeploymentStatusEnum.Completed, FacebookCampaignId = "fbcamp" };
        context.AdCampaigns.Add(campaignFb);

        await context.SaveChangesAsync();

        var repo = new PerformanceReportRepository(context);

        // Act
        var breakdowns = await repo.GetChannelBreakdownForAIAsync(workspaceId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(1));

        // Assert
        Assert.Equal(2, breakdowns.Count);
        
        var fb = breakdowns.Single(b => b.Platform == "facebook");
        Assert.Equal(1, fb.PublishedPosts); // 1 FB post
        Assert.Equal(50, fb.Impressions);
        Assert.Equal(10, fb.Clicks);
        Assert.Equal(25, fb.Engagement);
        Assert.Equal(100.5m, fb.Spend);
        
        var ig = breakdowns.Single(b => b.Platform == "instagram");
        Assert.Equal(1, ig.PublishedPosts); // 1 IG post
        Assert.Equal(30, ig.Impressions);
        Assert.Equal(5, ig.Clicks);
        Assert.Equal(15, ig.Engagement);
        Assert.Equal(0m, ig.Spend);
    }
}
