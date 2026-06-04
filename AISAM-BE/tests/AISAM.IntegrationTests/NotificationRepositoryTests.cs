using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class NotificationRepositoryTests
{
    [Fact]
    public async Task GetUnreadCountAsync_ReturnsOnlyActiveProfilesUnreadNotifications()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new NotificationRepository(context);

        var count = await repository.GetUnreadCountAsync(fixture.Profile.Id);

        Assert.Equal(1, count);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static NotificationRepositoryFixture SeedFixture(AisamContext context)
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

        var unreadOwned = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Title = "Unread",
            Message = "Unread owned",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = false
        };
        var readOwned = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Title = "Read",
            Message = "Read owned",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = true
        };
        var deletedOwned = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Title = "Deleted",
            Message = "Deleted owned",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = false,
            IsDeleted = true
        };
        var unreadOther = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            Title = "Other",
            Message = "Unread other",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = false
        };

        context.Users.AddRange(owner, otherUser);
        context.Profiles.AddRange(ownerProfile, otherProfile);
        context.Notifications.AddRange(unreadOwned, readOwned, deletedOwned, unreadOther);
        context.SaveChanges();

        return new NotificationRepositoryFixture(ownerProfile);
    }

    private sealed record NotificationRepositoryFixture(Profile Profile);
}
