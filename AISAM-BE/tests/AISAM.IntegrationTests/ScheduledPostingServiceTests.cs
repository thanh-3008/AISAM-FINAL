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
        schedule.Content = new Content
        {
            Id = schedule.ContentId,
            Title = "Summer collection launch",
            TextContent = "Discover our new summer collection.",
            WorkspaceId = schedule.WorkspaceId
        };
        schedule.Integration = new SocialIntegration
        {
            Id = schedule.IntegrationId!.Value,
            Platform = SocialPlatformEnum.Instagram,
            TargetName = "AISAM Studio",
            WorkspaceId = schedule.WorkspaceId
        };
        var notificationRepository = new FakeNotificationRepository();
        var postedAt = new DateTime(2026, 8, 13, 8, 30, 0, DateTimeKind.Utc);
        var contentService = new FakeContentService
        {
            PublishResult = GenericResponse<PublishResultDto>.CreateSuccess(new PublishResultDto
            {
                Success = true,
                ProviderPostId = "post-1",
                PostedAt = postedAt
            })
        };
        var repository = new FakeContentCalendarRepository(schedule);
        var profileRepository = new FakeProfileRepository(new Profile
        {
            Id = schedule.ProfileId,
            UserId = Guid.NewGuid(),
            Name = "Profile",
            ProfileType = ProfileTypeEnum.Basic
        });
        var membershipRepository = new FakeWorkspaceMemberRepository(new WorkspaceMember
        {
            WorkspaceId = Guid.NewGuid(),
            Workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = "Workspace",
                WorkspaceType = WorkspaceTypeEnum.Business,
                Status = WorkspaceStatusEnum.Active
            },
            UserId = profileRepository.Profiles.Values.Single().UserId,
            Role = WorkspaceMemberRoleEnum.ContentCreator,
            IsActive = true
        });
        var service = new ScheduledPostingService(repository, contentService, notificationRepository, profileRepository, membershipRepository, new FakeContentRepository());

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(1, result.ScannedCount);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(ScheduleStatusEnum.Completed, schedule.Status);
        Assert.NotNull(schedule.ExecutedAt);
        Assert.Null(schedule.LastError);
        Assert.Single(notificationRepository.Notifications.Values);
        var notification = notificationRepository.Notifications.Values.Single();
        Assert.Equal("Post published successfully on Instagram", notification.Title);
        Assert.Equal(
            "Your post \"Summer collection launch\" was published to Instagram (AISAM Studio) on Aug 13, 2026 at 08:30 UTC.",
            notification.Message);
        Assert.Equal(NotificationTypeEnum.PostScheduled, notification.Type);
        Assert.Equal(schedule.WorkspaceId, contentService.LastWorkspaceId);
    }

    [Fact]
    public async Task RunDueSchedulesAsync_MarksFailedAndCreatesNotification_WhenPublishFails()
    {
        var schedule = CreateDueSchedule();
        schedule.AttemptCount = 2; // Setup for permanent failure
        var notificationRepository = new FakeNotificationRepository();
        var contentService = new FakeContentService
        {
            PublishResult = GenericResponse<PublishResultDto>.CreateError("Facebook rejected the request.", HttpStatusCode.BadGateway)
        };
        var repository = new FakeContentCalendarRepository(schedule);
        var service = new ScheduledPostingService(
            repository,
            contentService,
            notificationRepository,
            new FakeProfileRepository(),
            new FakeWorkspaceMemberRepository(),
            new FakeContentRepository());

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(1, result.ScannedCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(ScheduleStatusEnum.Failed, schedule.Status);
        Assert.Equal(3, schedule.AttemptCount);
        Assert.Equal("Facebook rejected the request.", schedule.LastError);
        Assert.Single(notificationRepository.Notifications.Values);
        Assert.Equal("Scheduled publish failed", notificationRepository.Notifications.Values.Single().Title);
    }

    [Fact]
    public async Task RunDueSchedulesAsync_MarksFailed_WhenPublishIsBlockedByPostQuota()
    {
        var schedule = CreateDueSchedule();
        schedule.AttemptCount = 2; // Setup for permanent failure
        var notificationRepository = new FakeNotificationRepository();
        var contentService = new FakeContentService
        {
            PublishResult = GenericResponse<PublishResultDto>.CreateError(
                "Post quota has been exceeded for the current subscription.",
                HttpStatusCode.Forbidden,
                "POST_QUOTA_EXCEEDED")
        };
        var repository = new FakeContentCalendarRepository(schedule);
        var service = new ScheduledPostingService(
            repository,
            contentService,
            notificationRepository,
            new FakeProfileRepository(),
            new FakeWorkspaceMemberRepository(),
            new FakeContentRepository());

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(1, result.ScannedCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(ScheduleStatusEnum.Failed, schedule.Status);
        Assert.Equal(3, schedule.AttemptCount);
        Assert.Equal("Post quota has been exceeded for the current subscription.", schedule.LastError);
        Assert.Single(notificationRepository.Notifications.Values);
        Assert.Equal("Scheduled publish failed", notificationRepository.Notifications.Values.Single().Title);
    }

    [Fact]
    public async Task RunDueSchedulesAsync_DoesNotReprocessCompletedSchedules()
    {
        var completed = CreateDueSchedule();
        completed.Status = ScheduleStatusEnum.Completed;
        var contentService = new FakeContentService();
        var repository = new FakeContentCalendarRepository(completed);
        var notificationRepository = new FakeNotificationRepository();
        var service = new ScheduledPostingService(
            repository,
            contentService,
            notificationRepository,
            new FakeProfileRepository(),
            new FakeWorkspaceMemberRepository(),
            new FakeContentRepository());

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(0, result.ScannedCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(ScheduleStatusEnum.Completed, completed.Status);
    }

    [Fact]
    public async Task RunDueSchedulesAsync_BlocksExpiredWorkspaceWithoutFallingBackToLegacyProfilePublish()
    {
        var schedule = CreateDueSchedule();
        schedule.AttemptCount = 2; // Setup for permanent failure
        var userId = Guid.NewGuid();
        var profileRepository = new FakeProfileRepository(new Profile
        {
            Id = schedule.ProfileId,
            UserId = userId,
            Name = "Profile",
            ProfileType = ProfileTypeEnum.Basic
        });
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Expired",
            WorkspaceType = WorkspaceTypeEnum.Business,
            Status = WorkspaceStatusEnum.Active,
            SubscriptionExpiredAt = DateTime.UtcNow.AddDays(-1)
        };
        schedule.WorkspaceId = workspace.Id;
        schedule.Workspace = workspace;
        var membershipRepository = new FakeWorkspaceMemberRepository(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = userId,
            Role = WorkspaceMemberRoleEnum.ContentCreator,
            IsActive = true
        });
        var contentService = new FakeContentService();
        var service = new ScheduledPostingService(
            new FakeContentCalendarRepository(schedule),
            contentService,
            new FakeNotificationRepository(),
            profileRepository,
            membershipRepository,
            new FakeContentRepository());

        var result = await service.RunDueSchedulesAsync(20);

        Assert.Equal(1, result.FailedCount);
        Assert.Equal(0, contentService.PublishCallCount);
        Assert.Equal(ScheduleStatusEnum.Failed, schedule.Status);
        Assert.Contains("workspace is expired or inactive", schedule.LastError);
    }

    private static ContentCalendar CreateDueSchedule()
    {
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Active workspace",
            WorkspaceType = WorkspaceTypeEnum.Business,
            Status = WorkspaceStatusEnum.Active
        };
        return new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            Workspace = workspace,
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

        public Task<IReadOnlyList<ContentCalendar>> ClaimDueSchedulesAtomicallyAsync(DateTime utcNow, int limit, int maxAttemptCount, CancellationToken cancellationToken = default)
        {
            var due = Schedules.Values
                .Where(s =>
                    !s.IsDeleted &&
                    (s.Status == ScheduleStatusEnum.Pending ||
                     (s.Status == ScheduleStatusEnum.Failed && s.AttemptCount < maxAttemptCount)) &&
                    (s.ScheduledAt ?? s.ScheduledDate) <= utcNow)
                .OrderBy(s => s.ScheduledAt ?? s.ScheduledDate)
                .Take(limit)
                .ToList();
            foreach (var s in due) { s.Status = ScheduleStatusEnum.Processing; }
            IReadOnlyList<ContentCalendar> result = due;
            return Task.FromResult(result);
        }

        public Task<bool> HasActiveScheduleAsync(Guid contentId, CancellationToken cancellationToken = default)
            => Task.FromResult(Schedules.Values.Any(s => s.ContentId == contentId && !s.IsDeleted && (s.Status == ScheduleStatusEnum.Pending || s.Status == ScheduleStatusEnum.Processing)));

        public Task CancelActiveSchedulesForContentAsync(Guid contentId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<PagedResult<ContentCalendar>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
            => GetPagedByProfileIdAsync(workspaceId, request, cancellationToken);

        public Task<IReadOnlyList<ContentCalendar>> GetUpcomingByWorkspaceIdAsync(Guid workspaceId, int limit, CancellationToken cancellationToken = default)
            => GetUpcomingByProfileIdAsync(workspaceId, limit, cancellationToken);

        public Task<int> CountUpcomingByWorkspaceIdAsync(Guid workspaceId, DateTime utcNow, CancellationToken cancellationToken = default)
            => CountUpcomingByProfileIdAsync(workspaceId, utcNow, cancellationToken);

        public Task<int> CountFailedByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => CountFailedByProfileIdAsync(workspaceId, cancellationToken);

        public Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default)
        {
            Schedules[schedule.Id] = schedule;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeContentService : AISAM.Services.IServices.IContentService
    {
        public int PublishCallCount { get; private set; }
        public Guid? LastWorkspaceId { get; private set; }
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

        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            PublishCallCount++;
            LastWorkspaceId = workspaceId;
            return Task.FromResult(PublishResult);
        }

        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> CreateAsync(Guid profileId, AISAM.Common.Dtos.Request.CreateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PagedResult<AISAM.Common.Dtos.Response.ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, AISAM.Common.Dtos.Request.UpdateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SoftDeleteInWorkspaceAsync(Guid id, Guid workspaceId, WorkspaceMemberRoleEnum memberRole, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<List<string>>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SubmitForApprovalAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> ApproveAsync(Guid id, Guid workspaceId, Guid approverUserId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<AISAM.Common.Dtos.Response.ContentResponseDto>> RejectAsync(Guid id, Guid workspaceId, Guid approverUserId, string? notes, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public Dictionary<Guid, Profile> Profiles { get; }

        public FakeProfileRepository(params Profile[] profiles)
        {
            Profiles = profiles.ToDictionary(profile => profile.Id);
        }

        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Profiles.GetValueOrDefault(id));

        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeWorkspaceMemberRepository : IWorkspaceMemberRepository
    {
        public Dictionary<(Guid WorkspaceId, Guid UserId), WorkspaceMember> Memberships { get; }

        public FakeWorkspaceMemberRepository(params WorkspaceMember[] memberships)
        {
            Memberships = memberships.ToDictionary(member => (member.WorkspaceId, member.UserId));
        }

        public Task<IReadOnlyList<WorkspaceMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkspaceMember>>(Memberships.Values.Where(member => member.UserId == userId).ToList());

        public Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember> AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember> TransferOwnershipAsync(Guid workspaceId, Guid currentOwnerUserId, Guid targetMemberId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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

        public Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Content?>(null);
        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Content>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Content>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default) => Task.FromResult(content);
        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Content>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
    }
}
