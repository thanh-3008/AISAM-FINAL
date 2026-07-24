using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default);
        Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameInWorkspaceAsync(Guid workspaceId, string name, CancellationToken cancellationToken = default);
    }
}
