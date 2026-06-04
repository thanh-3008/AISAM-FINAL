using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class NotificationServiceTests
{
    [Fact]
    public async Task GetPagedAsync_ReturnsOnlyActiveProfilesNotifications()
    {
        var profileId = Guid.NewGuid();
        var own = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Title = "Owned",
            Message = "Owned notification",
            Type = NotificationTypeEnum.SystemUpdate
        };
        var other = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            Title = "Other",
            Message = "Other notification",
            Type = NotificationTypeEnum.SystemUpdate
        };
        var service = new NotificationService(new FakeNotificationRepository(own, other));

        var result = await service.GetPagedAsync(profileId, new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.True(result.Success);
        var item = Assert.Single(result.Data!.Data);
        Assert.Equal(own.Id, item.Id);
        Assert.Equal("Owned", item.Title);
    }

    [Fact]
    public async Task MarkReadAsync_ReturnsNotFound_ForAnotherProfilesNotification()
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            Title = "Other",
            Message = "Other notification",
            Type = NotificationTypeEnum.SystemUpdate
        };
        var service = new NotificationService(new FakeNotificationRepository(notification));

        var result = await service.MarkReadAsync(Guid.NewGuid(), notification.Id);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task MarkAllReadAsync_OnlyMarksCurrentProfilesNotifications()
    {
        var profileId = Guid.NewGuid();
        var own = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Title = "Owned",
            Message = "Owned notification",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = false
        };
        var other = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            Title = "Other",
            Message = "Other notification",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = false
        };
        var repository = new FakeNotificationRepository(own, other);
        var service = new NotificationService(repository);

        var result = await service.MarkAllReadAsync(profileId);

        Assert.True(result.Success);
        Assert.True(own.IsRead);
        Assert.False(other.IsRead);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsProfilesUnreadCount()
    {
        var profileId = Guid.NewGuid();
        var ownUnread = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Title = "Owned unread",
            Message = "Owned unread notification",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = false
        };
        var ownRead = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Title = "Owned read",
            Message = "Owned read notification",
            Type = NotificationTypeEnum.SystemUpdate,
            IsRead = true
        };
        var service = new NotificationService(new FakeNotificationRepository(ownUnread, ownRead));

        var result = await service.GetUnreadCountAsync(profileId);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.Count);
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private readonly Dictionary<Guid, Notification> _notifications;

        public FakeNotificationRepository(params Notification[] notifications)
        {
            _notifications = notifications.ToDictionary(notification => notification.Id);
        }

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _notifications.TryGetValue(id, out var notification);
            return Task.FromResult(notification is { IsDeleted: false } ? notification : null);
        }

        public Task<PagedResult<Notification>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var data = _notifications.Values
                .Where(notification => notification.ProfileId == profileId && !notification.IsDeleted)
                .OrderByDescending(notification => notification.CreatedAt)
                .ToList();

            return Task.FromResult(new PagedResult<Notification>
            {
                Data = data,
                TotalCount = data.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_notifications.Values.Count(notification =>
                notification.ProfileId == profileId &&
                !notification.IsDeleted &&
                !notification.IsRead));
        }

        public Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            _notifications[notification.Id] = notification;
            return Task.FromResult(notification);
        }

        public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
        {
            foreach (var notification in notifications)
            {
                _notifications[notification.Id] = notification;
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            _notifications[notification.Id] = notification;
            return Task.CompletedTask;
        }

        public Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            foreach (var notification in _notifications.Values.Where(notification =>
                         notification.ProfileId == profileId &&
                         !notification.IsDeleted &&
                         !notification.IsRead))
            {
                notification.IsRead = true;
            }

            return Task.CompletedTask;
        }
    }
}
