using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IContentRepository
{
    Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Content>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Content>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default);
    Task UpdateAsync(Content content, CancellationToken cancellationToken = default);
    Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}
