using AISAM.Common;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IAIService
{
    Task<GenericResponse<AiGenerationResponse>> GenerateDraftAsync(Guid profileId, Guid workspaceId, Guid userId, CreateDraftRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<AiGenerationResponse>> ImproveAsync(Guid contentId, Guid profileId, Guid workspaceId, Guid userId, ImproveContentRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> ApproveAsync(Guid generationId, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsAsync(Guid contentId, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ChatResponse>> ChatAsync(Guid profileId, ChatRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ChatResponse>> ChatInWorkspaceAsync(Guid profileId, Guid workspaceId, Guid userId, ChatRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> ApproveInWorkspaceAsync(Guid generationId, Guid workspaceId, CancellationToken cancellationToken = default)
        => ApproveAsync(generationId, workspaceId, cancellationToken);
    Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsInWorkspaceAsync(Guid contentId, Guid workspaceId, CancellationToken cancellationToken = default)
        => GetGenerationsAsync(contentId, workspaceId, cancellationToken);
}
