using AISAM.Common.Messages;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AISAM.Services.Service;

public sealed class ScheduledPostingService : IScheduledPostingService
{
    private readonly IContentCalendarRepository _contentCalendarRepository;
    private readonly IContentService _contentService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IContentRepository _contentRepository;

    public ScheduledPostingService(
        IContentCalendarRepository contentCalendarRepository,
        IContentService contentService,
        INotificationRepository notificationRepository,
        IProfileRepository profileRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IContentRepository contentRepository)
    {
        _contentCalendarRepository = contentCalendarRepository;
        _contentService = contentService;
        _notificationRepository = notificationRepository;
        _profileRepository = profileRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _contentRepository = contentRepository;
    }

    private const int MaxRetryAttempts = 3;

    public async Task<SchedulerRunResultDto> RunDueSchedulesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ContentCalendar> schedules;
        try
        {
            schedules = await _contentCalendarRepository.ClaimDueSchedulesAtomicallyAsync(DateTime.UtcNow, batchSize, MaxRetryAttempts, cancellationToken);
        }
        catch (Exception ex) when (IsDuplicateKeyError(ex))
        {
            schedules = Array.Empty<ContentCalendar>();
        }

        var result = new SchedulerRunResultDto
        {
            ScannedCount = schedules.Count
        };

        foreach (var schedule in schedules)
        {
            try
            {
                if (schedule.Workspace != null)
                {
                    WorkspaceLifecyclePolicy.SynchronizeStatus(schedule.Workspace, DateTime.UtcNow);
                    if (WorkspaceLifecyclePolicy.IsReadOnly(schedule.Workspace.Status))
                    {
                        throw new InvalidOperationException(MessageConstants.Schedule.WorkspaceBlocked);
                    }
                }

                var integrationId = schedule.IntegrationId ?? Guid.Empty;
                var publishResult = await _contentService.PublishScheduledAsync(
                    schedule.ContentId,
                    integrationId,
                    schedule.ProfileId,
                    schedule.WorkspaceId,
                    cancellationToken);
                if (publishResult.Success)
                {
                    schedule.Status = ScheduleStatusEnum.Completed;
                    schedule.ExecutedAt = DateTime.UtcNow;
                    schedule.LastError = null;
                    schedule.AttemptCount = 0;
                    await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                    await CreateNotificationAsync(
                        schedule.ProfileId,
                        MessageConstants.Schedule.SchedulePublishSucceeded,
                        string.Format(MessageConstants.Schedule.ContentPublishedSuccessfully, schedule.ContentId),
                        schedule.Id,
                        cancellationToken,
                        schedule.WorkspaceId);

                    result.SuccessCount++;
                    continue;
                }

                schedule.AttemptCount += 1;
                schedule.LastError = publishResult.Message ?? MessageConstants.Content.PublishingFailed;
                var isPermanentFail = schedule.AttemptCount >= MaxRetryAttempts;
                schedule.Status = isPermanentFail
                    ? ScheduleStatusEnum.Failed
                    : ScheduleStatusEnum.Pending;
                if (isPermanentFail)
                {
                    var content = await _contentRepository.GetByIdAsync(schedule.ContentId, cancellationToken);
                    if (content != null && content.Status == ContentStatusEnum.Approved)
                    {
                        content.Status = ContentStatusEnum.Draft;
                        await _contentRepository.UpdateAsync(content, cancellationToken);
                    }
                }
                await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                var notifyTitle = isPermanentFail ? MessageConstants.Schedule.SchedulePublishFailed : MessageConstants.Schedule.SchedulePublishWillRetry;
                await CreateNotificationAsync(
                    schedule.ProfileId,
                    notifyTitle,
                    schedule.LastError,
                    schedule.Id,
                    cancellationToken,
                    schedule.WorkspaceId);

                result.FailedCount++;
            }
            catch (Exception ex)
            {
                schedule.AttemptCount += 1;
                schedule.LastError = ex.Message;
                var isPermanentFail = schedule.AttemptCount >= MaxRetryAttempts;
                schedule.Status = isPermanentFail
                    ? ScheduleStatusEnum.Failed
                    : ScheduleStatusEnum.Pending;
                if (isPermanentFail)
                {
                    var content = await _contentRepository.GetByIdAsync(schedule.ContentId, cancellationToken);
                    if (content != null && content.Status == ContentStatusEnum.Approved)
                    {
                        content.Status = ContentStatusEnum.Draft;
                        await _contentRepository.UpdateAsync(content, cancellationToken);
                    }
                }
                await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                var catchNotifyTitle = isPermanentFail ? MessageConstants.Schedule.SchedulePublishFailed : MessageConstants.Schedule.SchedulePublishWillRetry;
                await CreateNotificationAsync(
                    schedule.ProfileId,
                    catchNotifyTitle,
                    schedule.LastError,
                    schedule.Id,
                    cancellationToken,
                    schedule.WorkspaceId);

                result.FailedCount++;
            }
        }

        return result;
    }

    private async Task CreateNotificationAsync(
        Guid profileId,
        string title,
        string? message,
        Guid scheduleId,
        CancellationToken cancellationToken,
        Guid workspaceId)
    {
        await _notificationRepository.AddAsync(new Notification
        {
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            Title = title,
            Message = message ?? MessageConstants.Content.PublishingFailed,
            Type = NotificationTypeEnum.SystemUpdate,
            TargetId = scheduleId,
            TargetType = "content_schedule",
            IsRead = false
        }, cancellationToken);
    }

    private static bool IsDuplicateKeyError(Exception ex)
    {
        while (ex != null)
        {
            if (ex is PostgresException pg && pg.SqlState == "23505")
                return true;
            ex = ex.InnerException;
        }
        return false;
    }
}
