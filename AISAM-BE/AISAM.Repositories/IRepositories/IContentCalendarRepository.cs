using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IContentCalendarRepository
{
    Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default);
    Task<int> CountUpcomingByProfileIdAsync(Guid profileId, DateTime utcNow, CancellationToken cancellationToken = default);
    Task<int> CountFailedByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default);
    Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default);
}
