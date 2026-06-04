using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Notification>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default);
}
