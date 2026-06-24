using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetPagedByProfileIdAsync(profileId, request, cancellationToken);

        return GenericResponse<PagedResult<NotificationListItemDto>>.CreateSuccess(new PagedResult<NotificationListItemDto>
        {
            Data = notifications.Data.Select(MapListItem).ToList(),
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

        return GenericResponse<NotificationDetailDto>.CreateSuccess(MapDetail(notification), "Notification retrieved successfully.");
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
            Data = notifications.Data.Select(MapListItem).ToList(), TotalCount = notifications.TotalCount, Page = notifications.Page, PageSize = notifications.PageSize
        }, "Notifications retrieved successfully.");
    }

    public async Task<GenericResponse<NotificationDetailDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        return notification == null || notification.WorkspaceId != workspaceId || notification.IsDeleted
            ? GenericResponse<NotificationDetailDto>.CreateError("Notification not found.", HttpStatusCode.NotFound)
            : GenericResponse<NotificationDetailDto>.CreateSuccess(MapDetail(notification), "Notification retrieved successfully.");
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

    private static NotificationListItemDto MapListItem(Notification notification)
    {
        return new NotificationListItemDto
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    private static NotificationDetailDto MapDetail(Notification notification)
    {
        return new NotificationDetailDto
        {
            Id = notification.Id,
            ProfileId = notification.ProfileId,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}
