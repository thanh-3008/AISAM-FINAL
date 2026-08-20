using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices;

public interface IHolidayService
{
    Task<GenericResponse<IEnumerable<HolidayEventDto>>> GetUpcomingAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> GetSuggestionAsync(Guid workspaceId, Guid profileId, Guid userId, Guid brandId, Guid holidayId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> GenerateHolidayVideoAsync(Guid workspaceId, Guid profileId, Guid userId, Guid brandId, Guid holidayId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> GetCustomEventSuggestionAsync(Guid workspaceId, Guid profileId, Guid userId, AISAM.Common.Dtos.Request.GenerateCustomEventContentRequest request, CancellationToken cancellationToken = default);
}
