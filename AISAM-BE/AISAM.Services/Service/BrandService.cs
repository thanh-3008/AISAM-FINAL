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
        private readonly IProfileRepository _profileRepository;

        public BrandService(IBrandRepository brandRepository, IProfileRepository profileRepository)
        {
            _brandRepository = brandRepository;
            _profileRepository = profileRepository;
        }

        public async Task<GenericResponse<PagedResult<BrandResponseDto>>> GetPagedByProfileIdAsync(
            Guid profileId,
            Guid userId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var access = await EnsureProfileOwnerAsync(profileId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<PagedResult<BrandResponseDto>>.CreateError(access.Message);
            }

            var brands = await _brandRepository.GetPagedByProfileIdAsync(profileId, request, includeDeleted, cancellationToken);

            return GenericResponse<PagedResult<BrandResponseDto>>.CreateSuccess(new PagedResult<BrandResponseDto>
            {
                Data = brands.Data.Select(MapToDto).ToList(),
                TotalCount = brands.TotalCount,
                Page = brands.Page,
                PageSize = brands.PageSize
            }, "Brands retrieved successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<BrandResponseDto>.CreateError("Brand not found");
            }

            var access = await EnsureProfileOwnerAsync(brand.ProfileId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<BrandResponseDto>.CreateError(access.Message);
            }

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(brand), "Brand retrieved successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> CreateAsync(Guid userId, CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.ProfileId.HasValue)
            {
                return GenericResponse<BrandResponseDto>.CreateError("ProfileId is required");
            }

            var access = await EnsureProfileOwnerAsync(request.ProfileId.Value, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<BrandResponseDto>.CreateError(access.Message);
            }

            var brand = new Brand
            {
                ProfileId = request.ProfileId.Value,
                Name = request.Name,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                Slogan = request.Slogan,
                Usp = request.Usp,
                TargetAudience = request.TargetAudience
            };

            var created = await _brandRepository.AddAsync(brand, cancellationToken);

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(created), "Brand created successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> UpdateAsync(Guid id, Guid userId, UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<BrandResponseDto>.CreateError("Brand not found");
            }

            var access = await EnsureProfileOwnerAsync(brand.ProfileId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<BrandResponseDto>.CreateError(access.Message);
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

        public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<bool>.CreateError("Brand not found");
            }

            var access = await EnsureProfileOwnerAsync(brand.ProfileId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            brand.IsDeleted = true;
            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Brand deleted successfully");
        }

        public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<bool>.CreateError("Brand not found");
            }

            var access = await EnsureProfileOwnerAsync(brand.ProfileId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            if (!brand.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Brand is not deleted");
            }

            brand.IsDeleted = false;
            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Brand restored successfully");
        }

        private async Task<(bool Success, string Message)> EnsureProfileOwnerAsync(Guid profileId, Guid userId, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(profileId, cancellationToken);
            if (profile == null)
            {
                return (false, "Profile not found");
            }

            if (profile.UserId != userId)
            {
                return (false, "You are not allowed to access this profile");
            }

            return (true, string.Empty);
        }

        private static BrandResponseDto MapToDto(Brand brand)
        {
            return new BrandResponseDto
            {
                Id = brand.Id,
                UserId = brand.Profile.UserId,
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
