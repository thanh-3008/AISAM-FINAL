using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default);
    Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Post>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
