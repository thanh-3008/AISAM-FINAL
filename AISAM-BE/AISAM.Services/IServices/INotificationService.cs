using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface INotificationService
{
    Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<NotificationDetailDto>> GetByIdAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> MarkReadAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> MarkAllReadAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<UnreadNotificationCountDto>> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
        => GetPagedAsync(workspaceId, request, cancellationToken);
    Task<GenericResponse<NotificationDetailDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid notificationId, CancellationToken cancellationToken = default)
        => GetByIdAsync(workspaceId, notificationId, cancellationToken);
    Task<GenericResponse<bool>> MarkReadInWorkspaceAsync(Guid workspaceId, Guid notificationId, CancellationToken cancellationToken = default)
        => MarkReadAsync(workspaceId, notificationId, cancellationToken);
    Task<GenericResponse<bool>> MarkAllReadInWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => MarkAllReadAsync(workspaceId, cancellationToken);
    Task<GenericResponse<UnreadNotificationCountDto>> GetUnreadCountByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => GetUnreadCountAsync(workspaceId, cancellationToken);
    Task<GenericResponse<bool>> DeleteInWorkspaceAsync(Guid workspaceId, Guid notificationId, CancellationToken cancellationToken = default);
}
