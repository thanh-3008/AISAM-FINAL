using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IBrandService
    {
        Task<GenericResponse<PagedResult<BrandResponseDto>>> GetPagedByProfileIdAsync(Guid profileId, Guid userId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<GenericResponse<BrandResponseDto>> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<BrandResponseDto>> CreateAsync(Guid userId, CreateBrandRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<BrandResponseDto>> UpdateAsync(Guid id, Guid userId, UpdateBrandRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
