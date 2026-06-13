using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;

        public BrandService(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<GenericResponse<PagedResult<BrandResponseDto>>> GetPagedAsync(
            Guid workspaceId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var brands = await _brandRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, includeDeleted, cancellationToken);

            return GenericResponse<PagedResult<BrandResponseDto>>.CreateSuccess(new PagedResult<BrandResponseDto>
            {
                Data = brands.Data.Select(brand => MapToDto(brand)).ToList(),
                TotalCount = brands.TotalCount,
                Page = brands.Page,
                PageSize = brands.PageSize
            }, "Brands retrieved successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null || brand.WorkspaceId != workspaceId)
            {
                return GenericResponse<BrandResponseDto>.CreateError("Brand not found");
            }

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(brand), "Brand retrieved successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> CreateAsync(
            Guid workspaceId,
            Guid profileId,
            Guid userId,
            CreateBrandRequest request,
            CancellationToken cancellationToken = default)
        {
            var brand = new Brand
            {
                ProfileId = profileId,
                WorkspaceId = workspaceId,
                Name = request.Name,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                Slogan = request.Slogan,
                Usp = request.Usp,
                TargetAudience = request.TargetAudience
            };

            var created = await _brandRepository.AddAsync(brand, cancellationToken);

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(created, userId), "Brand created successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> UpdateAsync(Guid id, Guid workspaceId, UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null || brand.WorkspaceId != workspaceId)
            {
                return GenericResponse<BrandResponseDto>.CreateError("Brand not found");
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                brand.Name = request.Name;
            }

            if (request.Description != null)
            {
                brand.Description = request.Description;
            }

            if (request.LogoUrl != null)
            {
                brand.LogoUrl = request.LogoUrl;
            }

            if (request.Slogan != null)
            {
                brand.Slogan = request.Slogan;
            }

            if (request.Usp != null)
            {
                brand.Usp = request.Usp;
            }

            if (request.TargetAudience != null)
            {
                brand.TargetAudience = request.TargetAudience;
            }

            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(brand), "Brand updated successfully");
        }

        public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null || brand.WorkspaceId != workspaceId)
            {
                return GenericResponse<bool>.CreateError("Brand not found");
            }

            brand.IsDeleted = true;
            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Brand deleted successfully");
        }

        public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (brand == null || brand.WorkspaceId != workspaceId)
            {
                return GenericResponse<bool>.CreateError("Brand not found");
            }

            if (!brand.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Brand is not deleted");
            }

            brand.IsDeleted = false;
            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Brand restored successfully");
        }

        private static BrandResponseDto MapToDto(Brand brand, Guid? fallbackUserId = null)
        {
            return new BrandResponseDto
            {
                Id = brand.Id,
                UserId = fallbackUserId ?? brand.Profile?.UserId ?? Guid.Empty,
                WorkspaceId = brand.WorkspaceId,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Slogan = brand.Slogan,
                Usp = brand.Usp,
                TargetAudience = brand.TargetAudience,
                ProfileId = brand.ProfileId,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt,
                ProductsCount = brand.Products.Count(p => !p.IsDeleted),
                ContentsCount = brand.Contents.Count(c => !c.IsDeleted)
            };
        }
    }
}
