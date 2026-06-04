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
}
