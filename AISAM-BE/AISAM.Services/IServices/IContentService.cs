using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IContentService
{
    Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, Guid workspaceId, CreateContentRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, Guid workspaceId, UpdateContentRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default);
}
