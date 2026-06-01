using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;
using System.Text.Json;

namespace AISAM.Services.Service;

public sealed class ContentScheduleService : IContentScheduleService
{
    private readonly IContentRepository _contentRepository;
    private readonly ISocialIntegrationRepository _socialIntegrationRepository;
    private readonly IContentCalendarRepository _contentCalendarRepository;
    private readonly INotificationRepository _notificationRepository;

    public ContentScheduleService(
        IContentRepository contentRepository,
        ISocialIntegrationRepository socialIntegrationRepository,
        IContentCalendarRepository contentCalendarRepository,
        INotificationRepository notificationRepository)
    {
        _contentRepository = contentRepository;
        _socialIntegrationRepository = socialIntegrationRepository;
        _contentCalendarRepository = contentCalendarRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<GenericResponse<ContentScheduleDto>> CreateAsync(Guid profileId, CreateContentScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateContentAndIntegrationAsync(profileId, request.ContentId, request.IntegrationId, cancellationToken);
        if (!validationResult.Success)
        {
            return validationResult.Error!;
        }

        var scheduledAt = NormalizeScheduledAt(request.ScheduledAt);
        if (scheduledAt == default)
        {
            return GenericResponse<ContentScheduleDto>.CreateError("Scheduled time is invalid.", HttpStatusCode.BadRequest);
        }

        var schedule = new ContentCalendar
        {
            ContentId = validationResult.Content!.Id,
            Content = validationResult.Content,
            ProfileId = profileId,
            IntegrationId = validationResult.Integration!.Id,
            Integration = validationResult.Integration,
            ScheduledAt = scheduledAt,
            ScheduledDate = scheduledAt,
            ScheduledTime = scheduledAt.TimeOfDay,
            Timezone = "UTC",
            IntegrationIds = JsonSerializer.Serialize(new[] { validationResult.Integration.Id }),
            Status = ScheduleStatusEnum.Pending,
            AttemptCount = 0,
            IsActive = true,
            IsDeleted = false
        };

        await _contentCalendarRepository.AddAsync(schedule, cancellationToken);
        await CreateNotificationAsync(
            profileId,
            "Schedule created",
            $"Content {schedule.ContentId} was scheduled for {schedule.ScheduledAt:O}.",
            schedule.Id,
            cancellationToken);

        return GenericResponse<ContentScheduleDto>.CreateSuccess(Map(schedule), "Schedule created successfully.");
    }

    public async Task<GenericResponse<PagedResult<ContentScheduleDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var schedules = await _contentCalendarRepository.GetPagedByProfileIdAsync(profileId, request, cancellationToken);
        return GenericResponse<PagedResult<ContentScheduleDto>>.CreateSuccess(new PagedResult<ContentScheduleDto>
        {
            Data = schedules.Data.Select(Map).ToList(),
            TotalCount = schedules.TotalCount,
            Page = schedules.Page,
            PageSize = schedules.PageSize
        }, "Schedules retrieved successfully.");
    }

    public async Task<GenericResponse<ContentScheduleDto>> GetByIdAsync(Guid profileId, Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await _contentCalendarRepository.GetByIdAsync(scheduleId, cancellationToken);
        if (schedule == null || schedule.ProfileId != profileId || schedule.IsDeleted)
        {
            return GenericResponse<ContentScheduleDto>.CreateError("Schedule not found.", HttpStatusCode.NotFound);
        }

        return GenericResponse<ContentScheduleDto>.CreateSuccess(Map(schedule), "Schedule retrieved successfully.");
    }

    public async Task<GenericResponse<ContentScheduleDto>> UpdateAsync(Guid profileId, Guid scheduleId, UpdateContentScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var schedule = await _contentCalendarRepository.GetByIdAsync(scheduleId, cancellationToken);
        if (schedule == null || schedule.ProfileId != profileId || schedule.IsDeleted)
        {
            return GenericResponse<ContentScheduleDto>.CreateError("Schedule not found.", HttpStatusCode.NotFound);
        }

        if (schedule.Status == ScheduleStatusEnum.Completed)
        {
            return GenericResponse<ContentScheduleDto>.CreateError("Completed schedules cannot be updated.", HttpStatusCode.BadRequest);
        }

        var content = schedule.Content ?? await _contentRepository.GetByIdAsync(schedule.ContentId, cancellationToken);
        if (content == null || content.ProfileId != profileId || content.IsDeleted)
        {
            return GenericResponse<ContentScheduleDto>.CreateError("Content not found.", HttpStatusCode.NotFound);
        }

        if (content.Status == ContentStatusEnum.Published)
        {
            return GenericResponse<ContentScheduleDto>.CreateError("Published content cannot be scheduled again.", HttpStatusCode.BadRequest);
        }

        if (request.IntegrationId.HasValue)
        {
            var integrationResult = await ValidateIntegrationAsync(profileId, content, request.IntegrationId.Value, cancellationToken);
            if (!integrationResult.Success)
            {
                return integrationResult.Error!;
            }

            schedule.IntegrationId = integrationResult.Integration!.Id;
            schedule.Integration = integrationResult.Integration;
            schedule.IntegrationIds = JsonSerializer.Serialize(new[] { integrationResult.Integration.Id });
        }

        if (request.ScheduledAt.HasValue)
        {
            var scheduledAt = NormalizeScheduledAt(request.ScheduledAt.Value);
            if (scheduledAt == default)
            {
                return GenericResponse<ContentScheduleDto>.CreateError("Scheduled time is invalid.", HttpStatusCode.BadRequest);
            }

            schedule.ScheduledAt = scheduledAt;
            schedule.ScheduledDate = scheduledAt;
            schedule.ScheduledTime = scheduledAt.TimeOfDay;
            schedule.Timezone = "UTC";
        }

        await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
        await CreateNotificationAsync(
            profileId,
            "Schedule updated",
            $"Schedule {schedule.Id} was updated.",
            schedule.Id,
            cancellationToken);

        return GenericResponse<ContentScheduleDto>.CreateSuccess(Map(schedule), "Schedule updated successfully.");
    }

    public async Task<GenericResponse<bool>> DeleteAsync(Guid profileId, Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await _contentCalendarRepository.GetByIdAsync(scheduleId, cancellationToken);
        if (schedule == null || schedule.ProfileId != profileId || schedule.IsDeleted)
        {
            return GenericResponse<bool>.CreateError("Schedule not found.", HttpStatusCode.NotFound);
        }

        schedule.IsDeleted = true;
        schedule.IsActive = false;
        await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
        await CreateNotificationAsync(
            profileId,
            "Schedule deleted",
            $"Schedule {schedule.Id} was deleted.",
            schedule.Id,
            cancellationToken);

        return GenericResponse<bool>.CreateSuccess(true, "Schedule deleted successfully.");
    }

    public async Task<GenericResponse<IReadOnlyList<ContentScheduleDto>>> GetUpcomingAsync(Guid profileId, int limit, CancellationToken cancellationToken = default)
    {
        var schedules = await _contentCalendarRepository.GetUpcomingByProfileIdAsync(profileId, limit, cancellationToken);
        return GenericResponse<IReadOnlyList<ContentScheduleDto>>.CreateSuccess(schedules.Select(Map).ToList(), "Upcoming schedules retrieved successfully.");
    }

    private async Task<(bool Success, Content? Content, SocialIntegration? Integration, GenericResponse<ContentScheduleDto>? Error)> ValidateContentAndIntegrationAsync(
        Guid profileId,
        Guid contentId,
        Guid integrationId,
        CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
        if (content == null || content.ProfileId != profileId || content.IsDeleted)
        {
            return (false, null, null, GenericResponse<ContentScheduleDto>.CreateError("Content not found.", HttpStatusCode.NotFound));
        }

        if (content.Status == ContentStatusEnum.Published)
        {
            return (false, null, null, GenericResponse<ContentScheduleDto>.CreateError("Published content cannot be scheduled again.", HttpStatusCode.BadRequest));
        }

        var integrationResult = await ValidateIntegrationAsync(profileId, content, integrationId, cancellationToken);
        if (!integrationResult.Success)
        {
            return (false, null, null, integrationResult.Error);
        }

        return (true, content, integrationResult.Integration, null);
    }

    private async Task<(bool Success, SocialIntegration? Integration, GenericResponse<ContentScheduleDto>? Error)> ValidateIntegrationAsync(
        Guid profileId,
        Content content,
        Guid integrationId,
        CancellationToken cancellationToken)
    {
        var integration = await _socialIntegrationRepository.GetByIdAsync(integrationId, cancellationToken);
        if (integration == null || integration.ProfileId != profileId || integration.IsDeleted || integration.BrandId != content.BrandId)
        {
            return (false, null, GenericResponse<ContentScheduleDto>.CreateError("Social integration not found.", HttpStatusCode.NotFound));
        }

        return (true, integration, null);
    }

    private async Task CreateNotificationAsync(
        Guid profileId,
        string title,
        string message,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        await _notificationRepository.AddAsync(new Notification
        {
            ProfileId = profileId,
            Title = title,
            Message = message,
            Type = NotificationTypeEnum.PostScheduled,
            TargetId = scheduleId,
            TargetType = "content_schedule",
            IsRead = false
        }, cancellationToken);
    }

    private static DateTime NormalizeScheduledAt(DateTime value)
    {
        if (value == default)
        {
            return default;
        }

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static ContentScheduleDto Map(ContentCalendar schedule)
    {
        return new ContentScheduleDto
        {
            Id = schedule.Id,
            ProfileId = schedule.ProfileId,
            ContentId = schedule.ContentId,
            IntegrationId = schedule.IntegrationId ?? Guid.Empty,
            ScheduledAt = schedule.ScheduledAt ?? schedule.ScheduledDate,
            ExecutedAt = schedule.ExecutedAt,
            Status = schedule.Status.ToString(),
            AttemptCount = schedule.AttemptCount,
            LastError = schedule.LastError
        };
    }
}
