using AISAM.Common.Dtos.Response;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class ContentScheduleServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPendingSchedule_WhenContentAndIntegrationBelongToProfile()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = Guid.NewGuid(),
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Approved content",
            Status = ContentStatusEnum.Approved
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = content.BrandId,
            SocialAccountId = Guid.NewGuid(),
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "token"
        };
        var scheduleRepository = new FakeContentCalendarRepository();
        var notificationRepository = new FakeNotificationRepository();
        var service = CreateService(
            contentRepository: new FakeContentRepository(content),
            socialIntegrationRepository: new FakeSocialIntegrationRepository(integration),
            contentCalendarRepository: scheduleRepository,
            notificationRepository: notificationRepository);

        var scheduledAt = DateTime.UtcNow.AddHours(2);
        var result = await service.CreateInWorkspaceAsync(workspaceId, profileId, new CreateContentScheduleRequest
        {
            ContentId = content.Id,
            IntegrationId = integration.Id,
            ScheduledAt = scheduledAt
        });

        Assert.True(result.Success);
        var schedule = Assert.Single(scheduleRepository.Schedules.Values);
        Assert.Equal(ScheduleStatusEnum.Pending, schedule.Status);
        Assert.Equal(profileId, schedule.ProfileId);
        Assert.Equal(workspaceId, schedule.WorkspaceId);
        Assert.Equal(content.Id, schedule.ContentId);
        Assert.Equal(integration.Id, schedule.IntegrationId);
        Assert.Equal(scheduledAt, schedule.ScheduledAt);
        var notification = Assert.Single(notificationRepository.Notifications.Values);
        Assert.Equal(NotificationTypeEnum.PostScheduled, notification.Type);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenIntegrationBelongsToAnotherProfile()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = Guid.NewGuid(),
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Approved content",
            Status = ContentStatusEnum.Approved
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            BrandId = content.BrandId,
            SocialAccountId = Guid.NewGuid(),
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "token"
        };
        var service = CreateService(
            contentRepository: new FakeContentRepository(content),
            socialIntegrationRepository: new FakeSocialIntegrationRepository(integration));

        var result = await service.CreateAsync(profileId, new CreateContentScheduleRequest
        {
            ContentId = content.Id,
            IntegrationId = integration.Id,
            ScheduledAt = DateTime.UtcNow.AddHours(2)
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Social integration not found.", result.Message);
    }

    [Fact]
    public async Task CreateAsync_AllowsPublishedContentToBeScheduledForAnotherIntegration()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = Guid.NewGuid(),
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Published content",
            Status = ContentStatusEnum.Published
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            BrandId = content.BrandId,
            SocialAccountId = Guid.NewGuid(),
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "token"
        };
        var service = CreateService(
            contentRepository: new FakeContentRepository(content),
            socialIntegrationRepository: new FakeSocialIntegrationRepository(integration));

        var result = await service.CreateInWorkspaceAsync(workspaceId, profileId, new CreateContentScheduleRequest
        {
            ContentId = content.Id,
            IntegrationId = integration.Id,
            ScheduledAt = DateTime.UtcNow.AddHours(2)
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(integration.Id, result.Data.IntegrationId);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBadRequest_WhenScheduleAlreadyCompleted()
    {
        var profileId = Guid.NewGuid();
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = Guid.NewGuid(),
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Draft content",
            Status = ContentStatusEnum.Draft
        };
        var integration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = content.BrandId,
            SocialAccountId = Guid.NewGuid(),
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "token"
        };
        var schedule = new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            ContentId = content.Id,
            Content = content,
            IntegrationId = integration.Id,
            Integration = integration,
            ScheduledAt = DateTime.UtcNow.AddHours(1),
            ScheduledDate = DateTime.UtcNow.AddHours(1),
            Status = ScheduleStatusEnum.Completed
        };
        var service = CreateService(
            contentRepository: new FakeContentRepository(content),
            socialIntegrationRepository: new FakeSocialIntegrationRepository(integration),
            contentCalendarRepository: new FakeContentCalendarRepository(schedule));

        var result = await service.UpdateAsync(profileId, schedule.Id, new UpdateContentScheduleRequest
        {
            ScheduledAt = DateTime.UtcNow.AddHours(3)
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Completed schedules cannot be updated.", result.Message);
    }

    [Fact]
    public async Task GetUpcomingAsync_ReturnsOnlyFutureSchedulesForProfile()
    {
        var profileId = Guid.NewGuid();
        var otherProfileId = Guid.NewGuid();
        var schedules = new[]
        {
            new ContentCalendar
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                ContentId = Guid.NewGuid(),
                ScheduledAt = DateTime.UtcNow.AddMinutes(15),
                ScheduledDate = DateTime.UtcNow.AddMinutes(15),
                IntegrationId = Guid.NewGuid(),
                Status = ScheduleStatusEnum.Pending
            },
            new ContentCalendar
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                ContentId = Guid.NewGuid(),
                ScheduledAt = DateTime.UtcNow.AddHours(1),
                ScheduledDate = DateTime.UtcNow.AddHours(1),
                IntegrationId = Guid.NewGuid(),
                Status = ScheduleStatusEnum.Pending
            },
            new ContentCalendar
            {
                Id = Guid.NewGuid(),
                ProfileId = otherProfileId,
                ContentId = Guid.NewGuid(),
                ScheduledAt = DateTime.UtcNow.AddMinutes(20),
                ScheduledDate = DateTime.UtcNow.AddMinutes(20),
                IntegrationId = Guid.NewGuid(),
                Status = ScheduleStatusEnum.Pending
            },
            new ContentCalendar
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                ContentId = Guid.NewGuid(),
                ScheduledAt = DateTime.UtcNow.AddMinutes(-10),
                ScheduledDate = DateTime.UtcNow.AddMinutes(-10),
                IntegrationId = Guid.NewGuid(),
                Status = ScheduleStatusEnum.Pending
            }
        };
        var service = CreateService(contentCalendarRepository: new FakeContentCalendarRepository(schedules));

        var result = await service.GetUpcomingAsync(profileId, 10);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        Assert.All(result.Data, item => Assert.Equal(profileId, item.ProfileId));
        Assert.True(result.Data[0].ScheduledAt <= result.Data[1].ScheduledAt);
    }

    private static ContentScheduleService CreateService(
        IContentRepository? contentRepository = null,
        ISocialIntegrationRepository? socialIntegrationRepository = null,
        IContentCalendarRepository? contentCalendarRepository = null,
        INotificationRepository? notificationRepository = null)
    {
        return new ContentScheduleService(
            contentRepository ?? new FakeContentRepository(),
            socialIntegrationRepository ?? new FakeSocialIntegrationRepository(),
            contentCalendarRepository ?? new FakeContentCalendarRepository(),
            notificationRepository ?? new FakeNotificationRepository(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ContentScheduleService>.Instance);
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        private readonly Dictionary<Guid, Content> _contents;

        public FakeContentRepository(params Content[] contents)
        {
            _contents = contents.ToDictionary(content => content.Id);
        }

        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _contents.TryGetValue(id, out var content);
            return Task.FromResult(content is { IsDeleted: false } ? content : null);
        }

        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _contents.TryGetValue(id, out var content);
            return Task.FromResult(content);
        }

        public Task<PagedResult<ContentListDto>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default)
        {
            _contents[content.Id] = content;
            return Task.CompletedTask;
        }

        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<PagedResult<ContentListDto>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
    }

    private sealed class FakeSocialIntegrationRepository : ISocialIntegrationRepository
    {
        private readonly Dictionary<Guid, SocialIntegration> _integrations;

        public FakeSocialIntegrationRepository(params SocialIntegration[] integrations)
        {
            _integrations = integrations.ToDictionary(integration => integration.Id);
        }

        public Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _integrations.TryGetValue(id, out var integration);
            return Task.FromResult(integration is { IsDeleted: false } ? integration : null);
        }

        public Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialIntegration?> GetByWorkspacePlatformExternalIdAsync(Guid workspaceId, SocialPlatformEnum platform, string externalId, CancellationToken cancellationToken = default) => Task.FromResult<SocialIntegration?>(null);
        public Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default)
        {
            _integrations[integration.Id] = integration;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeContentCalendarRepository : IContentCalendarRepository
    {
        public Dictionary<Guid, ContentCalendar> Schedules { get; }

        public FakeContentCalendarRepository(params ContentCalendar[] schedules)
        {
            Schedules = schedules.ToDictionary(schedule => schedule.Id);
        }

        public Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Schedules.TryGetValue(id, out var schedule);
            return Task.FromResult(schedule is { IsDeleted: false } ? schedule : null);
        }

        public Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var data = Schedules.Values
                .Where(schedule => schedule.ProfileId == profileId && !schedule.IsDeleted)
                .OrderBy(schedule => schedule.ScheduledAt ?? schedule.ScheduledDate)
                .ToList();

            return Task.FromResult(new PagedResult<ContentCalendar>
            {
                Data = data,
                TotalCount = data.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            IReadOnlyList<ContentCalendar> result = Schedules.Values
                .Where(schedule =>
                    schedule.ProfileId == profileId &&
                    !schedule.IsDeleted &&
                    (schedule.ScheduledAt ?? schedule.ScheduledDate) > now)
                .OrderBy(schedule => schedule.ScheduledAt ?? schedule.ScheduledDate)
                .Take(limit)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<int> CountUpcomingByProfileIdAsync(Guid profileId, DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var count = Schedules.Values.Count(schedule =>
                schedule.ProfileId == profileId &&
                !schedule.IsDeleted &&
                (schedule.ScheduledAt ?? schedule.ScheduledDate) > utcNow);

            return Task.FromResult(count);
        }

        public Task<int> CountFailedByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            var count = Schedules.Values.Count(schedule =>
                schedule.ProfileId == profileId &&
                !schedule.IsDeleted &&
                schedule.Status == ScheduleStatusEnum.Failed);

            return Task.FromResult(count);
        }

        public Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default)
        {
            Schedules[schedule.Id] = schedule;
            return Task.FromResult(schedule);
        }

        public Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default)
        {
            Schedules[schedule.Id] = schedule;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ContentCalendar>> ClaimDueSchedulesAtomicallyAsync(DateTime utcNow, int limit, int maxAttemptCount, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> HasActiveScheduleAsync(Guid contentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Schedules.Values.Any(schedule =>
                schedule.ContentId == contentId && !schedule.IsDeleted &&
                (schedule.Status == ScheduleStatusEnum.Pending || schedule.Status == ScheduleStatusEnum.Processing)));

        public Task<bool> HasActiveScheduleAsync(Guid contentId, Guid integrationId, CancellationToken cancellationToken = default)
            => Task.FromResult(Schedules.Values.Any(schedule =>
                schedule.ContentId == contentId && schedule.IntegrationId == integrationId && !schedule.IsDeleted &&
                (schedule.Status == ScheduleStatusEnum.Pending || schedule.Status == ScheduleStatusEnum.Processing)));

        public Task CancelActiveSchedulesForContentAsync(Guid contentId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        public Dictionary<Guid, Notification> Notifications { get; } = new();

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Notifications.TryGetValue(id, out var notification);
            return Task.FromResult(notification);
        }

        public Task<PagedResult<Notification>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            Notifications[notification.Id] = notification;
            return Task.FromResult(notification);
        }

        public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}

