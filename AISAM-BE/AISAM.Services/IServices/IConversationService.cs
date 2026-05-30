using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices;

public interface IConversationService
{
    Task<GenericResponse<PagedResult<ConversationResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ConversationDetailDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
}
