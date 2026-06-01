using AISAM.Common;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IAIService
{
    Task<GenericResponse<AiGenerationResponse>> GenerateDraftAsync(Guid profileId, CreateDraftRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<AiGenerationResponse>> ImproveAsync(Guid contentId, Guid profileId, ImproveContentRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> ApproveAsync(Guid generationId, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsAsync(Guid contentId, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ChatResponse>> ChatAsync(Guid profileId, ChatRequest request, CancellationToken cancellationToken = default);
}
