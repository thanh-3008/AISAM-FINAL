using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class SubscriptionRepositoryTests
{
    [Fact]
    public async Task GetCurrentActiveByProfileIdAsync_IgnoresExpiredSubscription()
    {
        await using var context = CreateContext();
        var profile = AddProfile(context);
        context.Subscriptions.Add(new Subscription
        {
            ProfileId = profile.Id,
            Plan = SubscriptionPlanEnum.Plus,
            StartDate = DateTime.UtcNow.Date.AddDays(-40),
            EndDate = DateTime.UtcNow.Date.AddDays(-1),
            IsActive = true
        });
        await context.SaveChangesAsync();
        var repository = new SubscriptionRepository(context);

        var result = await repository.GetCurrentActiveByProfileIdAsync(profile.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentActiveByProfileIdAsync_ReturnsSubscriptionInsideActiveDates()
    {
        await using var context = CreateContext();
        var profile = AddProfile(context);
        var subscription = new Subscription
        {
            ProfileId = profile.Id,
            Plan = SubscriptionPlanEnum.Plus,
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(1),
            IsActive = true
        };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();
        var repository = new SubscriptionRepository(context);

        var result = await repository.GetCurrentActiveByProfileIdAsync(profile.Id);

        Assert.Equal(subscription.Id, result?.Id);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AisamContext(options);
    }

    private static Profile AddProfile(AisamContext context)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var profile = new Profile
        {
            UserId = user.Id,
            User = user,
            Name = "Owner",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };
        context.Profiles.Add(profile);
        context.SaveChanges();
        return profile;
    }
}
