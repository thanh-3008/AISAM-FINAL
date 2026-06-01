using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class ScheduledPostingServiceTests
{
    [Fact]
    public async Task RunDueSchedulesAsync_PublishesDueScheduleAndMarksCompleted_WhenPublishSucceeds()
    {
        var schedule = CreateDueSchedule();
        var notificationRepository = new FakeNotificationRepository();
        var contentService = new FakeContentService
        {
            PublishResult = GenericResponse<PublishResultDto>.CreateSuccess(new PublishResultDto
            {
                Success = true,
                ProviderPostId = "post-1",
                PostedAt = DateTime.UtcNow
            })
        };
        var repository = new FakeContentCalendarRepository(schedule);
        var service = new ScheduledPostingService(repository, contentService, notificationRepository);

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(1, result.ScannedCount);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(ScheduleStatusEnum.Completed, schedule.Status);
        Assert.NotNull(schedule.ExecutedAt);
        Assert.Null(schedule.LastError);
        Assert.Single(notificationRepository.Notifications.Values);
        Assert.Equal("Scheduled publish succeeded", notificationRepository.Notifications.Values.Single().Title);
    }

    [Fact]
    public async Task RunDueSchedulesAsync_MarksFailedAndCreatesNotification_WhenPublishFails()
    {
        var schedule = CreateDueSchedule();
        var notificationRepository = new FakeNotificationRepository();
        var contentService = new FakeContentService
        {
            PublishResult = GenericResponse<PublishResultDto>.CreateError("Facebook rejected the request.", HttpStatusCode.BadGateway)
        };
        var repository = new FakeContentCalendarRepository(schedule);
        var service = new ScheduledPostingService(repository, contentService, notificationRepository);

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(1, result.ScannedCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(ScheduleStatusEnum.Failed, schedule.Status);
        Assert.Equal(1, schedule.AttemptCount);
        Assert.Equal("Facebook rejected the request.", schedule.LastError);
        Assert.Single(notificationRepository.Notifications.Values);
        Assert.Equal("Scheduled publish failed", notificationRepository.Notifications.Values.Single().Title);
    }

    [Fact]
    public async Task RunDueSchedulesAsync_DoesNotReprocessCompletedSchedules()
    {
        var completed = CreateDueSchedule();
        completed.Status = ScheduleStatusEnum.Completed;
        var contentService = new FakeContentService();
        var service = new ScheduledPostingService(
            new FakeContentCalendarRepository(completed),
            contentService,
            new FakeNotificationRepository());

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(0, result.ScannedCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(0, contentService.PublishCallCount);
    }

    private static ContentCalendar CreateDueSchedule()
    {
        return new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            ContentId = Guid.NewGuid(),
            IntegrationId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow.AddMinutes(-2),
            ScheduledDate = DateTime.UtcNow.AddMinutes(-2),
            Status = ScheduleStatusEnum.Pending
        };
    }

    private sealed class FakeContentCalendarRepository : IContentCalendarRepository
    {
        public Dictionary<Guid, ContentCalendar> Schedules { get; }

        public FakeContentCalendarRepository(params ContentCalendar[] schedules)
        {
            Schedules = schedules.ToDictionary(schedule => schedule.Id);
        }

        public Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountUpcomingByProfileIdAsync(Guid profileId, DateTime utcNow, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountFailedByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ContentCalendar> result = Schedules.Values
                .Where(schedule =>
                    !schedule.IsDeleted &&
                    schedule.Status == ScheduleStatusEnum.Pending &&
                    (schedule.ScheduledAt ?? schedule.ScheduledDate) <= utcNow)
                .OrderBy(schedule => schedule.ScheduledAt ?? schedule.ScheduledDate)
                .Take(limit)
                .ToList();

            return Task.FromResult(result);
        }

        public Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default)
        {
            Schedules[schedule.Id] = schedule;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeContentService : AISAM.Services.IServices.IContentService
    {
        public int PublishCallCount { get; private set; }
        public GenericResponse<PublishResultDto> PublishResult { get; set; } = GenericResponse<PublishResultDto>.CreateSuccess(new PublishResultDto
        {
            Success = true,
            ProviderPostId = "post-id",
            PostedAt = DateTime.UtcNow
        });

        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default)
        {
            PublishCallCount++;
            return Task.FromResult(PublishResult);
        }

        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> CreateAsync(Guid profileId, AISAM.Common.Dtos.Request.CreateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PagedResult<AISAM.Common.Dtos.Response.ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, AISAM.Common.Dtos.Request.UpdateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        public Dictionary<Guid, Notification> Notifications { get; } = new();

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Notification>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            Notifications[notification.Id] = notification;
            return Task.FromResult(notification);
        }
    }
}
