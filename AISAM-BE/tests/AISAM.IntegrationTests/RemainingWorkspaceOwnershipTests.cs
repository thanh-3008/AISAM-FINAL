using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class RemainingWorkspaceOwnershipTests
{
    [Fact]
    public void WorkspaceOwnershipColumns_AreRequiredAfterBackfillLock()
    {
        using var context = CreateContext();
        var ownershipTypes = new[]
        {
            typeof(Brand), typeof(Content), typeof(SocialAccount), typeof(SocialIntegration),
            typeof(ContentCalendar), typeof(Conversation), typeof(Notification), typeof(AdCampaign),
            typeof(Subscription)
        };

        foreach (var ownershipType in ownershipTypes)
        {
            var property = context.Model.FindEntityType(ownershipType)!.FindProperty("WorkspaceId")!;
            Assert.False(property.IsNullable, $"{ownershipType.Name}.WorkspaceId must be required.");
        }

        var paymentWorkspace = context.Model.FindEntityType(typeof(Payment))!.FindProperty("WorkspaceId")!;
        Assert.True(paymentWorkspace.IsNullable, "Payment.WorkspaceId must remain nullable until a Business workspace payment succeeds.");
    }

    [Fact]
    public async Task WorkspaceQueries_IsolateContentPostsCalendarConversationNotificationAndSocialAsync()
    {
        await using var context = CreateContext();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var fixture = Seed(context, first, second);

        var contents = await new ContentRepository(context).GetPagedByWorkspaceIdAsync(first, new PaginationRequest());
        var posts = await new PostRepository(context).GetPagedByWorkspaceIdAsync(first, new PaginationRequest());
        var schedules = await new ContentCalendarRepository(context).GetPagedByWorkspaceIdAsync(first, new PaginationRequest());
        var conversations = await new ConversationRepository(context).GetPagedByWorkspaceIdAsync(first, new PaginationRequest());
        var notifications = await new NotificationRepository(context).GetPagedByWorkspaceIdAsync(first, new PaginationRequest());
        var socialAccounts = await new SocialAccountRepository(context).GetByWorkspaceIdAsync(first);

        Assert.All(contents.Data, item => Assert.Equal(first, item.WorkspaceId));
        Assert.All(posts.Data, item => Assert.Equal(first, item.Content.WorkspaceId));
        Assert.All(schedules.Data, item => Assert.Equal(first, item.WorkspaceId));
        Assert.All(conversations.Data, item => Assert.Equal(first, item.WorkspaceId));
        Assert.All(notifications.Data, item => Assert.Equal(first, item.WorkspaceId));
        Assert.All(socialAccounts, item => Assert.Equal(first, item.WorkspaceId));
        Assert.Equal(6, new[] { contents.TotalCount, posts.TotalCount, schedules.TotalCount, conversations.TotalCount, notifications.TotalCount, socialAccounts.Count }.Count(count => count == 1));
        Assert.Equal(first, fixture.FirstCampaign.WorkspaceId);
        Assert.Equal(second, fixture.SecondCampaign.WorkspaceId);
    }

    private static AisamContext CreateContext()
        => new(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options);

    private static Fixture Seed(AisamContext context, Guid first, Guid second)
    {
        var user = new User { Email = $"{Guid.NewGuid():N}@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
        // Matching IDs ensures nullable legacy rows would leak if workspace queries
        // accidentally compared WorkspaceId input with ProfileId.
        var profile = new Profile { Id = first, User = user, UserId = user.Id, Name = "Profile", ProfileType = ProfileTypeEnum.Basic };
        var firstWorkspace = new Workspace { Id = first, Name = "First", WorkspaceType = WorkspaceTypeEnum.Business };
        var secondWorkspace = new Workspace { Id = second, Name = "Second", WorkspaceType = WorkspaceTypeEnum.Business };
        var firstBrand = Brand(profile, firstWorkspace, "First brand");
        var secondBrand = Brand(profile, secondWorkspace, "Second brand");
        var firstContent = Content(profile, firstWorkspace, firstBrand, "First content");
        var secondContent = Content(profile, secondWorkspace, secondBrand, "Second content");
        var firstAccount = Account(profile, firstWorkspace);
        var secondAccount = Account(profile, secondWorkspace);
        var firstIntegration = Integration(profile, firstWorkspace, firstBrand, firstAccount);
        var secondIntegration = Integration(profile, secondWorkspace, secondBrand, secondAccount);
        var firstCampaign = Campaign(profile, firstWorkspace, firstBrand);
        var secondCampaign = Campaign(profile, secondWorkspace, secondBrand);
        var legacyBrand = new Brand { Profile = profile, ProfileId = profile.Id, Name = "Legacy brand" };
        var legacyContent = new Content { Profile = profile, ProfileId = profile.Id, Brand = legacyBrand, BrandId = legacyBrand.Id, AdType = AdTypeEnum.TextOnly, TextContent = "Legacy content", Status = ContentStatusEnum.Approved };
        var legacyAccount = new SocialAccount { Profile = profile, ProfileId = profile.Id, Platform = SocialPlatformEnum.Facebook, UserAccessToken = "legacy-token" };

        context.AddRange(
            user, profile, firstWorkspace, secondWorkspace, firstBrand, secondBrand, firstContent, secondContent,
            firstAccount, secondAccount, firstIntegration, secondIntegration,
            new Post { Content = firstContent, ContentId = firstContent.Id, Integration = firstIntegration, IntegrationId = firstIntegration.Id, PublishedAt = DateTime.UtcNow },
            new Post { Content = secondContent, ContentId = secondContent.Id, Integration = secondIntegration, IntegrationId = secondIntegration.Id, PublishedAt = DateTime.UtcNow },
            new ContentCalendar { WorkspaceId = first, Profile = profile, ProfileId = profile.Id, Content = firstContent, ContentId = firstContent.Id, ScheduledDate = DateTime.UtcNow.AddDays(1) },
            new ContentCalendar { WorkspaceId = second, Profile = profile, ProfileId = profile.Id, Content = secondContent, ContentId = secondContent.Id, ScheduledDate = DateTime.UtcNow.AddDays(1) },
            new Conversation { WorkspaceId = first, Profile = profile, ProfileId = profile.Id, AdType = AdTypeEnum.TextOnly },
            new Conversation { WorkspaceId = second, Profile = profile, ProfileId = profile.Id, AdType = AdTypeEnum.TextOnly },
            new Notification { WorkspaceId = first, Profile = profile, ProfileId = profile.Id, Title = "First", Message = "First", Type = NotificationTypeEnum.SystemUpdate },
            new Notification { WorkspaceId = second, Profile = profile, ProfileId = profile.Id, Title = "Second", Message = "Second", Type = NotificationTypeEnum.SystemUpdate },
            legacyBrand, legacyContent, legacyAccount,
            new Post { Content = legacyContent, ContentId = legacyContent.Id, Integration = firstIntegration, IntegrationId = firstIntegration.Id, PublishedAt = DateTime.UtcNow },
            new ContentCalendar { Profile = profile, ProfileId = profile.Id, Content = legacyContent, ContentId = legacyContent.Id, ScheduledDate = DateTime.UtcNow.AddDays(1) },
            new Conversation { Profile = profile, ProfileId = profile.Id, AdType = AdTypeEnum.TextOnly },
            new Notification { Profile = profile, ProfileId = profile.Id, Title = "Legacy", Message = "Legacy", Type = NotificationTypeEnum.SystemUpdate },
            firstCampaign, secondCampaign);
        context.SaveChanges();
        return new Fixture(firstCampaign, secondCampaign);
    }

    private static Brand Brand(Profile profile, Workspace workspace, string name)
        => new() { Profile = profile, ProfileId = profile.Id, Workspace = workspace, WorkspaceId = workspace.Id, Name = name };
    private static Content Content(Profile profile, Workspace workspace, Brand brand, string text)
        => new() { Profile = profile, ProfileId = profile.Id, Workspace = workspace, WorkspaceId = workspace.Id, Brand = brand, BrandId = brand.Id, AdType = AdTypeEnum.TextOnly, TextContent = text, Status = ContentStatusEnum.Approved };
    private static SocialAccount Account(Profile profile, Workspace workspace)
        => new() { Profile = profile, ProfileId = profile.Id, Workspace = workspace, WorkspaceId = workspace.Id, Platform = SocialPlatformEnum.Facebook, UserAccessToken = "token" };
    private static SocialIntegration Integration(Profile profile, Workspace workspace, Brand brand, SocialAccount account)
        => new() { Profile = profile, ProfileId = profile.Id, Workspace = workspace, WorkspaceId = workspace.Id, Brand = brand, BrandId = brand.Id, SocialAccount = account, SocialAccountId = account.Id, Platform = SocialPlatformEnum.Facebook, AccessToken = "token" };
    private static AdCampaign Campaign(Profile profile, Workspace workspace, Brand brand)
        => new() { Profile = profile, ProfileId = profile.Id, Workspace = workspace, WorkspaceId = workspace.Id, Brand = brand, BrandId = brand.Id, AdAccountId = "account", Name = "Campaign" };

    private sealed record Fixture(AdCampaign FirstCampaign, AdCampaign SecondCampaign);
}




