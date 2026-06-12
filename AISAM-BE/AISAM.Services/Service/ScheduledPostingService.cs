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
    private readonly IWorkspaceLifecycleService _workspaceLifecycleService;

    public ScheduledPostingService(
        IContentCalendarRepository contentCalendarRepository,
        IContentService contentService,
        INotificationRepository notificationRepository,
        IProfileRepository profileRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IWorkspaceLifecycleService workspaceLifecycleService)
    {
        _contentCalendarRepository = contentCalendarRepository;
        _contentService = contentService;
        _notificationRepository = notificationRepository;
        _profileRepository = profileRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _workspaceLifecycleService = workspaceLifecycleService;
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

                var integrationId = schedule.IntegrationId ?? Guid.Empty;
                var workspaceResolution = await ResolveWorkspaceAccessAsync(schedule.ProfileId, cancellationToken);
                if (workspaceResolution.BlockedByLifecycle)
                {
                    schedule.Status = ScheduleStatusEnum.Failed;
                    schedule.AttemptCount += 1;
                    schedule.LastError = "Workspace must be active to publish scheduled content.";
                    await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                    await CreateNotificationAsync(
                        schedule.ProfileId,
                        "Scheduled publish failed",
                        schedule.LastError,
                        schedule.Id,
                        cancellationToken);

                    result.FailedCount++;
                    continue;
                }

                var workspaceId = workspaceResolution.WorkspaceId;
                var publishResult = workspaceId.HasValue
                    ? await _contentService.PublishAsync(schedule.ContentId, integrationId, schedule.ProfileId, workspaceId.Value, cancellationToken)
                    : await _contentService.PublishAsync(schedule.ContentId, integrationId, schedule.ProfileId, cancellationToken);
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
                        cancellationToken);

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
                    cancellationToken);

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
                    cancellationToken);

                result.FailedCount++;
            }
        }

        return result;
    }

    private async Task<(Guid? WorkspaceId, bool BlockedByLifecycle)> ResolveWorkspaceAccessAsync(Guid profileId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId, cancellationToken);
        if (profile == null)
        {
            return (null, false);
        }

        var memberships = await _workspaceMemberRepository.GetByUserIdAsync(profile.UserId, cancellationToken);
        var activeMemberships = memberships.Where(member => member.IsActive).ToList();
        if (activeMemberships.Count == 0)
        {
            return (null, false);
        }

        foreach (var membership in activeMemberships)
        {
            if (_workspaceLifecycleService.ResolveState(membership.Workspace) == WorkspaceLifecycleState.Active)
            {
                return (membership.WorkspaceId, false);
            }
        }

        return (null, true);
    }

    private async Task CreateNotificationAsync(
        Guid profileId,
        string title,
        string? message,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        await _notificationRepository.AddAsync(new Notification
        {
            ProfileId = profileId,
            Title = title,
            Message = message ?? "Publishing failed.",
            Type = NotificationTypeEnum.SystemUpdate,
            TargetId = scheduleId,
            TargetType = "content_schedule",
            IsRead = false
        }, cancellationToken);
    }
}
