using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class ScheduledPostingService : IScheduledPostingService
{
    private readonly IContentCalendarRepository _contentCalendarRepository;
    private readonly IContentService _contentService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;

    public ScheduledPostingService(
        IContentCalendarRepository contentCalendarRepository,
        IContentService contentService,
        INotificationRepository notificationRepository,
        IProfileRepository profileRepository,
        IWorkspaceMemberRepository workspaceMemberRepository)
    {
        _contentCalendarRepository = contentCalendarRepository;
        _contentService = contentService;
        _notificationRepository = notificationRepository;
        _profileRepository = profileRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
    }

    public async Task<SchedulerRunResultDto> RunDueSchedulesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var schedules = await _contentCalendarRepository.GetDueSchedulesAsync(DateTime.UtcNow, batchSize, cancellationToken);
        var result = new SchedulerRunResultDto
        {
            ScannedCount = schedules.Count
        };

        foreach (var schedule in schedules)
        {
            try
            {
                schedule.Status = ScheduleStatusEnum.Processing;
                await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);

                if (schedule.Workspace != null)
                {
                    WorkspaceLifecyclePolicy.SynchronizeStatus(schedule.Workspace, DateTime.UtcNow);
                    if (WorkspaceLifecyclePolicy.IsReadOnly(schedule.Workspace.Status))
                    {
                        throw new InvalidOperationException("Scheduled publishing is blocked because the workspace is expired or inactive.");
                    }
                }

                var integrationId = schedule.IntegrationId ?? Guid.Empty;
                var publishResult = await _contentService.PublishAsync(
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
                    await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                    await CreateNotificationAsync(
                        schedule.ProfileId,
                        "Scheduled publish succeeded",
                        $"Content {schedule.ContentId} was published successfully.",
                        schedule.Id,
                        cancellationToken,
                        schedule.WorkspaceId);

                    result.SuccessCount++;
                    continue;
                }

                schedule.Status = ScheduleStatusEnum.Failed;
                schedule.AttemptCount += 1;
                schedule.LastError = publishResult.Message ?? "Publishing failed.";
                await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                await CreateNotificationAsync(
                    schedule.ProfileId,
                    "Scheduled publish failed",
                    schedule.LastError,
                    schedule.Id,
                    cancellationToken,
                    schedule.WorkspaceId);

                result.FailedCount++;
            }
            catch (Exception ex)
            {
                schedule.Status = ScheduleStatusEnum.Failed;
                schedule.AttemptCount += 1;
                schedule.LastError = ex.Message;
                await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                await CreateNotificationAsync(
                    schedule.ProfileId,
                    "Scheduled publish failed",
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
            Message = message ?? "Publishing failed.",
            Type = NotificationTypeEnum.SystemUpdate,
            TargetId = scheduleId,
            TargetType = "content_schedule",
            IsRead = false
        }, cancellationToken);
    }
}
