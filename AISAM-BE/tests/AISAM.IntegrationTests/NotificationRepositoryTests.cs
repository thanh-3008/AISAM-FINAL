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

    [Fact]
    public async Task WorkspaceQueries_ReturnPersonalAndWorkspaceNotificationsOnly()
    {
        await using var context = CreateContext();
        var workspaceId = Guid.NewGuid();
        var profileA = Guid.NewGuid();
        var profileB = Guid.NewGuid();
        context.Profiles.AddRange(new Profile { Id = profileA, UserId = Guid.NewGuid(), Name = "A", ProfileType = ProfileTypeEnum.Basic }, new Profile { Id = profileB, UserId = Guid.NewGuid(), Name = "B", ProfileType = ProfileTypeEnum.Basic });
        context.Notifications.AddRange(
            new Notification { WorkspaceId = workspaceId, ProfileId = profileA, Title = "A", Message = "A", Type = NotificationTypeEnum.ApprovalNeeded },
            new Notification { WorkspaceId = workspaceId, ProfileId = profileB, Title = "B", Message = "B", Type = NotificationTypeEnum.ApprovalNeeded },
            new Notification { WorkspaceId = workspaceId, ProfileId = Guid.Empty, Title = "Workspace", Message = "Workspace", Type = NotificationTypeEnum.SystemUpdate, IsRead = false });
        await context.SaveChangesAsync();
        var repository = new NotificationRepository(context);

        var result = await repository.GetPagedByWorkspaceIdAsync(workspaceId, profileA, new PaginationRequest { Page = 1, PageSize = 10 });
        var unreadCount = await repository.GetUnreadCountByWorkspaceIdAsync(workspaceId, profileA);

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Data, notification => notification.ProfileId == profileA);
        Assert.Contains(result.Data, notification => notification.ProfileId == Guid.Empty);
        Assert.DoesNotContain(result.Data, notification => notification.ProfileId == profileB);
        Assert.Equal(2, unreadCount);
    }

    [Fact]
    public async Task MarkAllAsReadByWorkspaceIdAsync_MarksCurrentAndWorkspaceNotificationsOnly()
    {
        await using var context = CreateContext();
        var workspaceId = Guid.NewGuid();
        var profileA = Guid.NewGuid();
        var profileB = Guid.NewGuid();
        context.Profiles.AddRange(new Profile { Id = profileA, UserId = Guid.NewGuid(), Name = "A", ProfileType = ProfileTypeEnum.Basic }, new Profile { Id = profileB, UserId = Guid.NewGuid(), Name = "B", ProfileType = ProfileTypeEnum.Basic });
        var own = new Notification { WorkspaceId = workspaceId, ProfileId = profileA, Title = "A", Message = "A", Type = NotificationTypeEnum.ApprovalNeeded };
        var other = new Notification { WorkspaceId = workspaceId, ProfileId = profileB, Title = "B", Message = "B", Type = NotificationTypeEnum.ApprovalNeeded };
        var workspace = new Notification { WorkspaceId = workspaceId, ProfileId = Guid.Empty, Title = "Workspace", Message = "Workspace", Type = NotificationTypeEnum.SystemUpdate };
        context.Notifications.AddRange(own, other, workspace);
        await context.SaveChangesAsync();

        await new NotificationRepository(context).MarkAllAsReadByWorkspaceIdAsync(workspaceId, profileA);

        Assert.True(own.IsRead);
        Assert.False(other.IsRead);
        Assert.True(workspace.IsRead);
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




