using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;

namespace AISAM.Services.IServices;

public interface IPostService
{
    Task<GenericResponse<PagedResult<PostListItemDto>>> GetPagedAsync(
        Guid profileId,
        PaginationRequest request,
        Guid? brandId = null,
        ContentStatusEnum? status = null,
        CancellationToken cancellationToken = default);

    Task<GenericResponse<PostListItemDto>> GetByIdAsync(
        Guid profileId,
        Guid postId,
        CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<PostListItemDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<PostListItemDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid postId, CancellationToken cancellationToken = default);
}
