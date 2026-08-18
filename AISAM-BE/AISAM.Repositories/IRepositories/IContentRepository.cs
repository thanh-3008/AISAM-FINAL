using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IContentRepository
{
    Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentListDto>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentListDto>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default);
    Task UpdateAsync(Content content, CancellationToken cancellationToken = default);
    Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentListDto>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
