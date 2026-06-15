using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IBrandService
    {
        Task<GenericResponse<PagedResult<BrandResponseDto>>> GetPagedByWorkspaceIdAsync(Guid workspaceId, Guid userId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<GenericResponse<BrandResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<BrandResponseDto>> CreateAsync(Guid workspaceId, Guid userId, CreateBrandRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<BrandResponseDto>> UpdateAsync(Guid id, Guid workspaceId, Guid userId, UpdateBrandRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    }
}
