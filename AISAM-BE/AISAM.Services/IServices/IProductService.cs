using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IProductService
    {
        Task<GenericResponse<PagedResult<ProductResponseDto>>> GetPagedAsync(PaginationRequest request, Guid userId, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProductResponseDto>> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProductResponseDto>> CreateAsync(Guid userId, ProductCreateRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProductResponseDto>> UpdateAsync(Guid id, Guid userId, ProductUpdateRequestDto request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
