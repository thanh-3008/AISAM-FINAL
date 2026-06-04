using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class PaymentRepositoryTests
{
    [Fact]
    public async Task GetCurrentActiveByProfileIdAsync_ReturnsOnlyCurrentProfilesActiveSubscription()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new SubscriptionRepository(context);

        var subscription = await repository.GetCurrentActiveByProfileIdAsync(fixture.OwnerProfile.Id);

        Assert.NotNull(subscription);
        Assert.Equal(fixture.ActiveSubscription.Id, subscription!.Id);
    }

    [Fact]
    public async Task GetHistoryByProfileIdAsync_ReturnsPaymentsSortedNewestFirst()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new PaymentRepository(context);

        var result = await repository.GetPagedByProfileIdAsync(
            fixture.OwnerProfile.Id,
            new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(
            new[] { fixture.NewestPayment.Id, fixture.OlderPayment.Id },
            result.Data.Select(payment => payment.Id).ToArray());
    }

    [Fact]
    public async Task CountSuccessfulPromptUsageAsync_CountsOnlyCompletedGenerationsInsideSubscriptionWindow()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new SubscriptionRepository(context);

        var count = await repository.CountSuccessfulPromptUsageAsync(
            fixture.OwnerProfile.Id,
            fixture.ActiveSubscription.StartDate,
            fixture.ActiveSubscription.EndDate);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CountSuccessfulPostUsageAsync_CountsOnlyPublishedPostsInsideSubscriptionWindow()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new SubscriptionRepository(context);

        var count = await repository.CountSuccessfulPostUsageAsync(
            fixture.OwnerProfile.Id,
            fixture.ActiveSubscription.StartDate,
            fixture.ActiveSubscription.EndDate);

        Assert.Equal(1, count);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static PaymentRepositoryFixture SeedFixture(AisamContext context)
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var otherUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        var ownerProfile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            Name = "Owner",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };
        var otherProfile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = otherUser.Id,
            Name = "Other",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };

        var activeSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Plan = SubscriptionPlanEnum.Plus,
            QuotaPostsPerMonth = 5,
            QuotaAIContentPerDay = 3,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 30),
            IsActive = true
        };
        var inactiveSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Plan = SubscriptionPlanEnum.Free,
            StartDate = new DateTime(2026, 5, 1),
            EndDate = new DateTime(2026, 5, 31),
            IsActive = false
        };
        var otherSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            Plan = SubscriptionPlanEnum.Premium,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 30),
            IsActive = true
        };

        ownerProfile.SubscriptionId = activeSubscription.Id;
        otherProfile.SubscriptionId = otherSubscription.Id;

        var olderPayment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            SubscriptionId = activeSubscription.Id,
            Amount = 100_000m,
            Status = PaymentStatusEnum.Success,
            PaymentMethod = "PayOS",
            TransactionId = "txn-owner-old",
            CreatedAt = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc)
        };
        var newestPayment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            SubscriptionId = activeSubscription.Id,
            Amount = 200_000m,
            Status = PaymentStatusEnum.Success,
            PaymentMethod = "PayOS",
            TransactionId = "txn-owner-new",
            CreatedAt = new DateTime(2026, 6, 2, 8, 0, 0, DateTimeKind.Utc)
        };
        var otherProfilePayment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = otherUser.Id,
            SubscriptionId = otherSubscription.Id,
            Amount = 300_000m,
            Status = PaymentStatusEnum.Success,
            PaymentMethod = "PayOS",
            TransactionId = "txn-other",
            CreatedAt = new DateTime(2026, 6, 2, 9, 0, 0, DateTimeKind.Utc)
        };
        var userOnlyPayment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            SubscriptionId = null,
            Amount = 50_000m,
            Status = PaymentStatusEnum.Pending,
            PaymentMethod = "PayOS",
            TransactionId = "txn-owner-null-sub",
            CreatedAt = new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)
        };

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Name = "Owner Brand"
        };
        var otherBrand = new Brand
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            Name = "Other Brand"
        };

        var ownerContent = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            BrandId = brand.Id,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Owner content",
            Status = ContentStatusEnum.Draft
        };
        var otherContent = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            BrandId = otherBrand.Id,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Other content",
            Status = ContentStatusEnum.Draft
        };

        var promptInsideWindow = new AiGeneration
        {
            Id = Guid.NewGuid(),
            ContentId = ownerContent.Id,
            AiPrompt = "inside",
            GeneratedText = "ok",
            Status = AiStatusEnum.Completed,
            CreatedAt = new DateTime(2026, 6, 2, 7, 0, 0, DateTimeKind.Utc)
        };
        var promptOutsideWindow = new AiGeneration
        {
            Id = Guid.NewGuid(),
            ContentId = ownerContent.Id,
            AiPrompt = "outside",
            GeneratedText = "old",
            Status = AiStatusEnum.Completed,
            CreatedAt = new DateTime(2026, 5, 20, 7, 0, 0, DateTimeKind.Utc)
        };
        var promptFailedInsideWindow = new AiGeneration
        {
            Id = Guid.NewGuid(),
            ContentId = ownerContent.Id,
            AiPrompt = "failed",
            Status = AiStatusEnum.Failed,
            ErrorMessage = "boom",
            CreatedAt = new DateTime(2026, 6, 2, 7, 30, 0, DateTimeKind.Utc)
        };
        var otherPromptInsideWindow = new AiGeneration
        {
            Id = Guid.NewGuid(),
            ContentId = otherContent.Id,
            AiPrompt = "other",
            GeneratedText = "other",
            Status = AiStatusEnum.Completed,
            CreatedAt = new DateTime(2026, 6, 2, 8, 0, 0, DateTimeKind.Utc)
        };

        var ownerSocialAccount = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "token"
        };
        var otherSocialAccount = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "token"
        };

        var ownerIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            BrandId = brand.Id,
            SocialAccountId = ownerSocialAccount.Id,
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "page-token"
        };
        var otherIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            BrandId = otherBrand.Id,
            SocialAccountId = otherSocialAccount.Id,
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "page-token"
        };

        var publishedInsideWindow = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = ownerContent.Id,
            IntegrationId = ownerIntegration.Id,
            PublishedAt = new DateTime(2026, 6, 3, 8, 0, 0, DateTimeKind.Utc),
            Status = ContentStatusEnum.Published,
            CreatedAt = new DateTime(2026, 6, 3, 8, 0, 0, DateTimeKind.Utc)
        };
        var publishedOutsideWindow = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = ownerContent.Id,
            IntegrationId = ownerIntegration.Id,
            PublishedAt = new DateTime(2026, 5, 21, 8, 0, 0, DateTimeKind.Utc),
            Status = ContentStatusEnum.Published,
            CreatedAt = new DateTime(2026, 5, 21, 8, 0, 0, DateTimeKind.Utc)
        };
        var draftInsideWindow = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = ownerContent.Id,
            IntegrationId = ownerIntegration.Id,
            PublishedAt = new DateTime(2026, 6, 4, 8, 0, 0, DateTimeKind.Utc),
            Status = ContentStatusEnum.Draft,
            CreatedAt = new DateTime(2026, 6, 4, 8, 0, 0, DateTimeKind.Utc)
        };
        var otherPublishedInsideWindow = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = otherContent.Id,
            IntegrationId = otherIntegration.Id,
            PublishedAt = new DateTime(2026, 6, 3, 10, 0, 0, DateTimeKind.Utc),
            Status = ContentStatusEnum.Published,
            CreatedAt = new DateTime(2026, 6, 3, 10, 0, 0, DateTimeKind.Utc)
        };

        context.Users.AddRange(owner, otherUser);
        context.Profiles.AddRange(ownerProfile, otherProfile);
        context.Subscriptions.AddRange(activeSubscription, inactiveSubscription, otherSubscription);
        context.Payments.AddRange(olderPayment, newestPayment, otherProfilePayment, userOnlyPayment);
        context.Brands.AddRange(brand, otherBrand);
        context.Contents.AddRange(ownerContent, otherContent);
        context.AiGenerations.AddRange(promptInsideWindow, promptOutsideWindow, promptFailedInsideWindow, otherPromptInsideWindow);
        context.SocialAccounts.AddRange(ownerSocialAccount, otherSocialAccount);
        context.SocialIntegrations.AddRange(ownerIntegration, otherIntegration);
        context.Posts.AddRange(publishedInsideWindow, publishedOutsideWindow, draftInsideWindow, otherPublishedInsideWindow);
        context.SaveChanges();

        return new PaymentRepositoryFixture(ownerProfile, activeSubscription, olderPayment, newestPayment);
    }

    private sealed record PaymentRepositoryFixture(
        Profile OwnerProfile,
        Subscription ActiveSubscription,
        Payment OlderPayment,
        Payment NewestPayment);
}
