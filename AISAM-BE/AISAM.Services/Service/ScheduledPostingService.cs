using AISAM.Common.Messages;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Globalization;

namespace AISAM.Services.Service;

public sealed class ScheduledPostingService : IScheduledPostingService
{
    private readonly IContentCalendarRepository _contentCalendarRepository;
    private readonly IContentService _contentService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly IContentRepository _contentRepository;
    private readonly AISAM.Repositories.AisamContext? _context;
    private readonly PublishOperationService? _operations;
    private readonly ILogger<ScheduledPostingService>? _logger;

    public ScheduledPostingService(
        IContentCalendarRepository contentCalendarRepository,
        IContentService contentService,
        INotificationRepository notificationRepository,
        IProfileRepository profileRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        IContentRepository contentRepository,
        AISAM.Repositories.AisamContext? context = null,
        PublishOperationService? operations = null,
        ILogger<ScheduledPostingService>? logger = null)
    {
        _contentCalendarRepository = contentCalendarRepository;
        _contentService = contentService;
        _notificationRepository = notificationRepository;
        _profileRepository = profileRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _contentRepository = contentRepository;
        _context = context;
        _operations = operations;
        _logger = logger;
    }

    private const int MaxRetryAttempts = 3;

    public async Task<SchedulerRunResultDto> RunDueSchedulesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        if (_context?.Database.IsNpgsql() == true)
        {
            // An abandoned claim may have sent a provider request. Never put it back
            // into Pending; require reconciliation even when the process crashed.
            var abandonedIds = await _context.Database.SqlQueryRaw<Guid>("""
                UPDATE content_calendar SET status = {0}, last_error = 'PUBLISH_OUTCOME_UNKNOWN', updated_at = NOW()
                WHERE status = {1} AND updated_at < NOW() - INTERVAL '24 hours'
                    AND is_deleted = false
                RETURNING id AS "Value"
                """, (int)ScheduleStatusEnum.Failed, (int)ScheduleStatusEnum.Processing).ToListAsync(cancellationToken);
            foreach (var abandoned in await _context.ContentCalendars.AsNoTracking()
                .Where(s => abandonedIds.Contains(s.Id)).ToListAsync(cancellationToken))
                await CreateNotificationAsync(abandoned.ProfileId, "Scheduled publication needs reconciliation",
                    "PUBLISH_OUTCOME_UNKNOWN", abandoned.Id, cancellationToken, abandoned.WorkspaceId);
        }
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
            var previousActor = _context?.ExecutionActorId;
            var previousSnapshot = _context?.ExecutionSnapshotId;
            var previousSystem = _context?.ExecutionIsSystem ?? true;
            if (_context is not null)
            {
                _context.ExecutionActorId = schedule.ScheduledByUserId;
                _context.ExecutionSnapshotId = schedule.SnapshotId;
                _context.ExecutionIsSystem = true;
            }
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
                var publishResult = _operations is not null
                    ? await PublishDurablyAsync(schedule, cancellationToken)
                    : await _contentService.PublishScheduledAsync(
                    schedule.ContentId,
                    integrationId,
                    schedule.ProfileId,
                    schedule.WorkspaceId,
                    cancellationToken);
                if(publishResult.Data?.RequiresReconciliation==true)
                {
                    schedule.Status=ScheduleStatusEnum.Failed;
                    schedule.LastError="PUBLISH_OUTCOME_PENDING: "+publishResult.Data.ProviderPostId;
                    await _contentCalendarRepository.UpdateAsync(schedule,cancellationToken);
                    await CreateNotificationAsync(schedule.ProfileId,"Scheduled publication needs reconciliation",schedule.LastError,schedule.Id,cancellationToken,schedule.WorkspaceId);
                    result.FailedCount++;
                    continue; // Do not retry an accepted provider upload blindly.
                }
                if (publishResult.Success)
                {
                    schedule.Status = ScheduleStatusEnum.Completed;
                    schedule.ExecutedAt = DateTime.UtcNow;
                    schedule.LastError = null;
                    schedule.AttemptCount = 0;
                    await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                    await CreateNotificationAsync(
                        schedule.ProfileId,
                        BuildPublishSuccessTitle(schedule),
                        BuildPublishSuccessMessage(schedule, publishResult.Data?.PostedAt ?? schedule.ExecutedAt.Value),
                        schedule.Id,
                        cancellationToken,
                        schedule.WorkspaceId,
                        NotificationTypeEnum.PostScheduled);

                    result.SuccessCount++;
                    continue;
                }

                schedule.AttemptCount += 1;
                schedule.LastError = publishResult.Message ?? MessageConstants.Content.PublishingFailed;
                var isPermanentFail = schedule.AttemptCount >= MaxRetryAttempts || publishResult.Error?.ErrorCode != "PUBLISH_RETRY_SAFE";
                schedule.Status = isPermanentFail
                    ? ScheduleStatusEnum.Failed
                    : ScheduleStatusEnum.Pending;
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
            catch (PublishPreparationUnavailableException)
            {
                schedule.AttemptCount++;
                schedule.LastError = "PUBLISH_RETRY_SAFE";
                schedule.Status = schedule.AttemptCount < MaxRetryAttempts ? ScheduleStatusEnum.Pending : ScheduleStatusEnum.Failed;
                await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
                await CreateNotificationAsync(schedule.ProfileId, "Scheduled publication preparation unavailable",
                    schedule.LastError, schedule.Id, cancellationToken, schedule.WorkspaceId);
                result.FailedCount++;
            }
            catch (Exception ex)
            {
                schedule.AttemptCount += 1;
                schedule.LastError = ex is AISAM.Repositories.ResourceMutationDeniedException ? "AUTOMATION_ACCESS_REVOKED" :
                    ex is InvalidOperationException && ex.Message==MessageConstants.Schedule.WorkspaceBlocked ? MessageConstants.Schedule.WorkspaceBlocked : "PUBLISH_OUTCOME_UNKNOWN";
                var isPermanentFail = true;
                schedule.Status = isPermanentFail
                    ? ScheduleStatusEnum.Failed
                    : ScheduleStatusEnum.Pending;
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
            finally
            {
                if (_context is not null)
                {
                    _context.ExecutionActorId = previousActor;
                    _context.ExecutionSnapshotId = previousSnapshot;
                    _context.ExecutionIsSystem = previousSystem;
                }
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
        Guid workspaceId,
        NotificationTypeEnum type = NotificationTypeEnum.SystemUpdate)
    {
        try
        {
        await _notificationRepository.AddAsync(new Notification
        {
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            Title = title,
            Message = message ?? MessageConstants.Content.PublishingFailed,
            Type = type,
            TargetId = scheduleId,
            TargetType = "content_schedule",
            IsRead = false
        }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Notification delivery must never change or replay a persisted publication outcome.
            _logger?.LogWarning(ex, "Could not notify schedule {ScheduleId}", scheduleId);
        }
    }

    private async Task<AISAM.Common.GenericResponse<AISAM.Common.Models.PublishResultDto>> PublishDurablyAsync(ContentCalendar schedule, CancellationToken ct)
    {
        var operation = (await _operations!.StartScheduledAsync(schedule, ct)).Single();
        if (operation.Status == "Published")
            return AISAM.Common.GenericResponse<AISAM.Common.Models.PublishResultDto>.CreateSuccess(new()
            { Success = true, ProviderPostId = operation.ProviderId, PostedAt = operation.UpdatedAt });
        if (operation.Status is "Queued" or "UploadingMedia" or "Publishing" ||
            operation.ErrorCode is "PUBLISH_OUTCOME_UNKNOWN" or "PUBLISH_OUTCOME_PENDING")
            return AISAM.Common.GenericResponse<AISAM.Common.Models.PublishResultDto>.CreateSuccess(new()
            { RequiresReconciliation = true, ProviderPostId = operation.ProviderId });
        return AISAM.Common.GenericResponse<AISAM.Common.Models.PublishResultDto>.CreateError(
            operation.ErrorCode ?? "Scheduled publication needs attention.", System.Net.HttpStatusCode.Conflict, operation.ErrorCode);
    }

    private static string BuildPublishSuccessTitle(ContentCalendar schedule)
    {
        var platform = schedule.Integration?.Platform.ToString() ?? "social media";
        return $"Post published successfully on {platform}";
    }

    private static string BuildPublishSuccessMessage(ContentCalendar schedule, DateTime publishedAt)
    {
        var content = schedule.Content;
        var contentSummary = !string.IsNullOrWhiteSpace(content?.Title)
            ? content.Title.Trim()
            : CreateExcerpt(content?.TextContent);
        var platform = schedule.Integration?.Platform.ToString() ?? "social media";
        var destination = string.IsNullOrWhiteSpace(schedule.Integration?.TargetName)
            ? platform
            : $"{platform} ({schedule.Integration.TargetName.Trim()})";
        var utcTime = publishedAt.Kind == DateTimeKind.Utc ? publishedAt : publishedAt.ToUniversalTime();
        var formattedTime = utcTime.ToString("MMM d, yyyy 'at' HH:mm 'UTC'", CultureInfo.InvariantCulture);

        return $"Your post \"{contentSummary}\" was published to {destination} on {formattedTime}.";
    }

    private static string CreateExcerpt(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Untitled post";

        var normalized = string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 100 ? normalized : $"{normalized[..97]}...";
    }

    private static bool IsDuplicateKeyError(Exception ex)
    {
        Exception? current = ex;
        while (current != null)
        {
            if (current is PostgresException pg && pg.SqlState == "23505")
                return true;
            current = current.InnerException;
        }
        return false;
    }
}
