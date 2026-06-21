using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class ContentCalendarRepository : IContentCalendarRepository
{
    private readonly AisamContext _context;

    public ContentCalendarRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(schedule => schedule.Id == id && !schedule.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query()
            .Where(schedule => schedule.ProfileId == profileId && !schedule.IsDeleted && (schedule.Content.Status == AISAM.Data.Enumeration.ContentStatusEnum.Approved || schedule.Content.Status == AISAM.Data.Enumeration.ContentStatusEnum.Published))
            .OrderBy(schedule => schedule.ScheduledAt ?? schedule.ScheduledDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContentCalendar>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var take = Math.Clamp(limit, 1, 100);

        return await Query()
            .Where(schedule =>
                schedule.ProfileId == profileId &&
                !schedule.IsDeleted &&
                (schedule.ScheduledAt ?? schedule.ScheduledDate) > utcNow)
            .OrderBy(schedule => schedule.ScheduledAt ?? schedule.ScheduledDate)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ContentCalendar>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(s => s.WorkspaceId == workspaceId && !s.IsDeleted && (s.Content.Status == AISAM.Data.Enumeration.ContentStatusEnum.Approved || s.Content.Status == AISAM.Data.Enumeration.ContentStatusEnum.Published)).OrderBy(s => s.ScheduledAt ?? s.ScheduledDate);
        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<ContentCalendar> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<IReadOnlyList<ContentCalendar>> GetUpcomingByWorkspaceIdAsync(Guid workspaceId, int limit, CancellationToken cancellationToken = default)
        => await Query().Where(s => s.WorkspaceId == workspaceId && !s.IsDeleted && (s.ScheduledAt ?? s.ScheduledDate) > DateTime.UtcNow)
            .OrderBy(s => s.ScheduledAt ?? s.ScheduledDate).Take(Math.Clamp(limit, 1, 100)).ToListAsync(cancellationToken);

    public async Task<int> CountUpcomingByProfileIdAsync(Guid profileId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(schedule =>
                schedule.ProfileId == profileId &&
                !schedule.IsDeleted &&
                (schedule.ScheduledAt ?? schedule.ScheduledDate) > utcNow)
            .CountAsync(cancellationToken);
    }

    public async Task<int> CountFailedByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(schedule =>
                schedule.ProfileId == profileId &&
                !schedule.IsDeleted &&
                schedule.Status == ScheduleStatusEnum.Failed)
            .CountAsync(cancellationToken);
    }

    public async Task<int> CountUpcomingByWorkspaceIdAsync(Guid workspaceId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(schedule =>
                schedule.WorkspaceId == workspaceId &&
                !schedule.IsDeleted &&
                (schedule.ScheduledAt ?? schedule.ScheduledDate) > utcNow)
            .CountAsync(cancellationToken);
    }

    public async Task<int> CountFailedByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(schedule =>
                schedule.WorkspaceId == workspaceId &&
                !schedule.IsDeleted &&
                schedule.Status == ScheduleStatusEnum.Failed)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 100);

        return await Query()
            .Where(schedule =>
                !schedule.IsDeleted &&
                schedule.Status == ScheduleStatusEnum.Pending &&
                (schedule.ScheduledAt ?? schedule.ScheduledDate) <= utcNow)
            .OrderBy(schedule => schedule.ScheduledAt ?? schedule.ScheduledDate)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentCalendar>> ClaimDueSchedulesAtomicallyAsync(DateTime utcNow, int limit, int maxAttemptCount, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 100);

        var ids = await _context.Database.SqlQueryRaw<Guid>(
            """
            UPDATE content_calendar
            SET status = {0}, updated_at = NOW()
            WHERE id IN (
                SELECT id FROM content_calendar
                WHERE is_deleted = false
                  AND (
                      (status = {1} AND (scheduled_at IS NOT NULL AND scheduled_at <= {2}))
                      OR
                      (status = {3} AND attempt_count < {4} AND (scheduled_at IS NOT NULL AND scheduled_at <= {2}))
                  )
                ORDER BY scheduled_at
                LIMIT {5}
                FOR UPDATE SKIP LOCKED
            )
            RETURNING id
            """,
            (int)ScheduleStatusEnum.Processing,
            (int)ScheduleStatusEnum.Pending,
            utcNow,
            (int)ScheduleStatusEnum.Failed,
            maxAttemptCount,
            take
        ).ToListAsync(cancellationToken);

        if (ids.Count == 0)
            return [];

        return await Query()
            .Where(s => ids.Contains(s.Id))
            .OrderBy(s => s.ScheduledAt ?? s.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveScheduleAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        return await _context.ContentCalendars
            .AnyAsync(s =>
                s.ContentId == contentId &&
                !s.IsDeleted &&
                (s.Status == ScheduleStatusEnum.Pending || s.Status == ScheduleStatusEnum.Processing),
                cancellationToken);
    }

    public async Task CancelActiveSchedulesForContentAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        var active = await _context.ContentCalendars
            .Where(s => s.ContentId == contentId && !s.IsDeleted &&
                        (s.Status == ScheduleStatusEnum.Pending || s.Status == ScheduleStatusEnum.Processing))
            .ToListAsync(cancellationToken);
        foreach (var s in active)
        {
            s.Status = ScheduleStatusEnum.Completed;
            s.ExecutedAt = DateTime.UtcNow;
            s.LastError = null;
            s.AttemptCount = 0;
            s.IsActive = false;
            s.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default)
    {
        schedule.CreatedAt = DateTime.UtcNow;
        schedule.UpdatedAt = DateTime.UtcNow;
        _context.ContentCalendars.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        return schedule;
    }

    public async Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default)
    {
        schedule.UpdatedAt = DateTime.UtcNow;
        _context.ContentCalendars.Update(schedule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ContentCalendar> Query()
    {
        return _context.ContentCalendars
            .Include(schedule => schedule.Content)
                .ThenInclude(content => content.Brand)
            .Include(schedule => schedule.Profile)
            .Include(schedule => schedule.Integration)
            .Include(schedule => schedule.Workspace);
    }
}
