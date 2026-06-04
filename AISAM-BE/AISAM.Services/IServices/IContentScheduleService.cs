using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IContentScheduleService
{
    Task<GenericResponse<ContentScheduleDto>> CreateAsync(Guid profileId, CreateContentScheduleRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<ContentScheduleDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentScheduleDto>> GetByIdAsync(Guid profileId, Guid scheduleId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentScheduleDto>> UpdateAsync(Guid profileId, Guid scheduleId, UpdateContentScheduleRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> DeleteAsync(Guid profileId, Guid scheduleId, CancellationToken cancellationToken = default);
    Task<GenericResponse<IReadOnlyList<ContentScheduleDto>>> GetUpcomingAsync(Guid profileId, int limit, CancellationToken cancellationToken = default);
}
