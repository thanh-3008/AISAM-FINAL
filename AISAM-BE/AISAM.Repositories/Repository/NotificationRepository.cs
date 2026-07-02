using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AisamContext _context;

    public NotificationRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(notification => notification.Id == id && !notification.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Notification>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query()
            .Where(notification => notification.ProfileId == profileId && !notification.IsDeleted)
            .OrderByDescending(notification => notification.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Notification>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .CountAsync(notification =>
                notification.ProfileId == profileId &&
                !notification.IsDeleted &&
                !notification.IsRead,
                cancellationToken);
    }

    public async Task<PagedResult<Notification>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(n => n.WorkspaceId == workspaceId && !n.IsDeleted).OrderByDescending(n => n.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<Notification> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public Task<int> GetUnreadCountByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => Query().CountAsync(n => n.WorkspaceId == workspaceId && !n.IsDeleted && !n.IsRead, cancellationToken);

    public async Task MarkAllAsReadByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var notifications = await Query().Where(n => n.WorkspaceId == workspaceId && !n.IsDeleted && !n.IsRead).ToListAsync(cancellationToken);
        foreach (var notification in notifications) notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        notification.CreatedAt = DateTime.UtcNow;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.CreatedAt = utcNow;
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        notification.IsDeleted = true;
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var notifications = await Query()
            .Where(notification => notification.ProfileId == profileId && !notification.IsDeleted && !notification.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Notification> Query()
    {
        return _context.Notifications
            .Include(notification => notification.Profile);
    }
}
