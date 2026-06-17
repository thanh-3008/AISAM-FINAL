using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IContentService
{
    Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, UpdateContentRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> ApproveInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => Task.FromResult(GenericResponse<ContentResponseDto>.CreateError("Content approval is not supported."));
    Task<GenericResponse<ContentResponseDto>> RejectInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => Task.FromResult(GenericResponse<ContentResponseDto>.CreateError("Content rejection is not supported."));
    Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentResponseDto>> CreateInWorkspaceAsync(Guid workspaceId, Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default)
        => CreateAsync(profileId, request, cancellationToken);
    Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        => GetPagedAsync(workspaceId, request, brandId, adType, includeDeleted, status, cancellationToken);
    Task<GenericResponse<ContentResponseDto>> GetByIdInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, workspaceId, cancellationToken);
    Task<GenericResponse<ContentResponseDto>> UpdateInWorkspaceAsync(Guid id, Guid workspaceId, UpdateContentRequest request, CancellationToken cancellationToken = default)
        => UpdateAsync(id, workspaceId, request, cancellationToken);
    Task<GenericResponse<ContentResponseDto>> CloneInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => CloneAsync(id, workspaceId, cancellationToken);
    Task<GenericResponse<bool>> SoftDeleteInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => SoftDeleteAsync(id, workspaceId, cancellationToken);
    Task<GenericResponse<bool>> RestoreInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        => RestoreAsync(id, workspaceId, cancellationToken);
}
