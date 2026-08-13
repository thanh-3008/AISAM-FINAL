using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;
using System.Globalization;

namespace AISAM.Services.Service;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IContentCalendarRepository? _contentCalendarRepository;

    public NotificationService(
        INotificationRepository notificationRepository,
        IContentCalendarRepository? contentCalendarRepository = null)
    {
        _notificationRepository = notificationRepository;
        _contentCalendarRepository = contentCalendarRepository;
    }

    public async Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetPagedByProfileIdAsync(profileId, request, cancellationToken);

        return GenericResponse<PagedResult<NotificationListItemDto>>.CreateSuccess(new PagedResult<NotificationListItemDto>
        {
            Data = await MapListItemsAsync(notifications.Data, cancellationToken),
            TotalCount = notifications.TotalCount,
            Page = notifications.Page,
            PageSize = notifications.PageSize
        }, "Notifications retrieved successfully.");
    }

    public async Task<GenericResponse<NotificationDetailDto>> GetByIdAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null || notification.ProfileId != profileId || notification.IsDeleted)
        {
            return GenericResponse<NotificationDetailDto>.CreateError("Notification not found.", HttpStatusCode.NotFound);
        }

        var display = await GetDisplayContentAsync(notification, cancellationToken);
        return GenericResponse<NotificationDetailDto>.CreateSuccess(MapDetail(notification, display), "Notification retrieved successfully.");
    }

    public async Task<GenericResponse<bool>> MarkReadAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null || notification.ProfileId != profileId || notification.IsDeleted)
        {
            return GenericResponse<bool>.CreateError("Notification not found.", HttpStatusCode.NotFound);
        }

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Notification marked as read.");
    }

    public async Task<GenericResponse<bool>> MarkAllReadAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllAsReadAsync(profileId, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "All notifications marked as read.");
    }

    public async Task<GenericResponse<UnreadNotificationCountDto>> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(profileId, cancellationToken);
        return GenericResponse<UnreadNotificationCountDto>.CreateSuccess(new UnreadNotificationCountDto
        {
            Count = count
        }, "Unread notification count retrieved successfully.");
    }

    public async Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, cancellationToken);
        return GenericResponse<PagedResult<NotificationListItemDto>>.CreateSuccess(new PagedResult<NotificationListItemDto>
        {
            Data = await MapListItemsAsync(notifications.Data, cancellationToken), TotalCount = notifications.TotalCount, Page = notifications.Page, PageSize = notifications.PageSize
        }, "Notifications retrieved successfully.");
    }

    public async Task<GenericResponse<NotificationDetailDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        return notification == null || notification.WorkspaceId != workspaceId || notification.IsDeleted
            ? GenericResponse<NotificationDetailDto>.CreateError("Notification not found.", HttpStatusCode.NotFound)
            : GenericResponse<NotificationDetailDto>.CreateSuccess(
                MapDetail(notification, await GetDisplayContentAsync(notification, cancellationToken)),
                "Notification retrieved successfully.");
    }

    public async Task<GenericResponse<bool>> MarkReadInWorkspaceAsync(Guid workspaceId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null || notification.WorkspaceId != workspaceId || notification.IsDeleted)
            return GenericResponse<bool>.CreateError("Notification not found.", HttpStatusCode.NotFound);
        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Notification marked as read.");
    }

    public async Task<GenericResponse<bool>> MarkAllReadInWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllAsReadByWorkspaceIdAsync(workspaceId, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "All notifications marked as read.");
    }

    public async Task<GenericResponse<UnreadNotificationCountDto>> GetUnreadCountByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => GenericResponse<UnreadNotificationCountDto>.CreateSuccess(new UnreadNotificationCountDto { Count = await _notificationRepository.GetUnreadCountByWorkspaceIdAsync(workspaceId, cancellationToken) }, "Unread notification count retrieved successfully.");

    public async Task<GenericResponse<bool>> DeleteInWorkspaceAsync(Guid workspaceId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null || notification.WorkspaceId != workspaceId || notification.IsDeleted)
            return GenericResponse<bool>.CreateError("Notification not found.", HttpStatusCode.NotFound);
        await _notificationRepository.DeleteAsync(notification, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Notification deleted successfully.");
    }

    private async Task<List<NotificationListItemDto>> MapListItemsAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken)
    {
        var items = new List<NotificationListItemDto>();
        foreach (var notification in notifications)
        {
            var display = await GetDisplayContentAsync(notification, cancellationToken);
            items.Add(MapListItem(notification, display));
        }

        return items;
    }

    private async Task<NotificationDisplayContent> GetDisplayContentAsync(
        Notification notification,
        CancellationToken cancellationToken)
    {
        if (_contentCalendarRepository == null ||
            notification.TargetType != "content_schedule" ||
            notification.TargetId is not Guid scheduleId ||
            scheduleId == Guid.Empty ||
            !notification.Title.Equals("Scheduled publish succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return new NotificationDisplayContent(notification.Title, notification.Message, notification.Type);
        }

        var schedule = await _contentCalendarRepository.GetByIdAsync(scheduleId, cancellationToken);
        if (schedule == null)
            return new NotificationDisplayContent(notification.Title, notification.Message, notification.Type);

        var platform = schedule.Integration?.Platform.ToString() ?? "social media";
        var destination = string.IsNullOrWhiteSpace(schedule.Integration?.TargetName)
            ? platform
            : $"{platform} ({schedule.Integration.TargetName.Trim()})";
        var contentSummary = !string.IsNullOrWhiteSpace(schedule.Content?.Title)
            ? schedule.Content.Title.Trim()
            : CreateExcerpt(schedule.Content?.TextContent);
        var publishedAt = schedule.ExecutedAt ?? notification.CreatedAt;
        var utcTime = publishedAt.Kind == DateTimeKind.Utc ? publishedAt : publishedAt.ToUniversalTime();
        var formattedTime = utcTime.ToString("MMM d, yyyy 'at' HH:mm 'UTC'", CultureInfo.InvariantCulture);

        return new NotificationDisplayContent(
            $"Post published successfully on {platform}",
            $"Your post \"{contentSummary}\" was published to {destination} on {formattedTime}.",
            NotificationTypeEnum.PostScheduled);
    }

    private static string CreateExcerpt(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Untitled post";
        var normalized = string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 100 ? normalized : $"{normalized[..97]}...";
    }

    private static NotificationListItemDto MapListItem(Notification notification, NotificationDisplayContent display)
    {
        return new NotificationListItemDto
        {
            Id = notification.Id,
            Type = display.Type.ToString(),
            Title = display.Title,
            Message = display.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    private static NotificationDetailDto MapDetail(Notification notification, NotificationDisplayContent display)
    {
        return new NotificationDetailDto
        {
            Id = notification.Id,
            ProfileId = notification.ProfileId,
            Type = display.Type.ToString(),
            Title = display.Title,
            Message = display.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    private sealed record NotificationDisplayContent(
        string Title,
        string Message,
        NotificationTypeEnum Type);
}
