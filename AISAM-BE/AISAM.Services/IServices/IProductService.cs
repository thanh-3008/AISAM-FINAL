using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IProductService
    {
        Task<GenericResponse<PagedResult<ProductResponseDto>>> GetPagedAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProductResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProductResponseDto>> CreateAsync(Guid workspaceId, ProductCreateRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProductResponseDto>> UpdateAsync(Guid id, Guid workspaceId, ProductUpdateRequestDto request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default);
    }
}
