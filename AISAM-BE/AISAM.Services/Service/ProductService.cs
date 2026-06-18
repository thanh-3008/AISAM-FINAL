using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using AISAM.Common.Config;

namespace AISAM.Services.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IWebHostEnvironment? _environment;
        private readonly MediaStorageSettings _mediaStorageSettings;
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        };
        private const long MaxImageBytes = 10 * 1024 * 1024;

        public ProductService(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IWebHostEnvironment? environment = null,
            IOptions<MediaStorageSettings>? mediaStorageSettings = null)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _environment = environment;
            _mediaStorageSettings = mediaStorageSettings?.Value ?? new MediaStorageSettings();
        }

        public async Task<GenericResponse<PagedResult<ProductResponseDto>>> GetPagedAsync(
            PaginationRequest request,
            Guid workspaceId,
            Guid userId,
            Guid? brandId = null,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            if (brandId.HasValue)
            {
                var access = await EnsureBrandWorkspaceAccessAsync(brandId.Value, workspaceId, userId, cancellationToken);
                if (!access.Success)
                {
                    return GenericResponse<PagedResult<ProductResponseDto>>.CreateError(access.Message);
                }
            }

            var products = await _productRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, brandId, includeDeleted, cancellationToken);
            var visibleProducts = products.Data.Select(MapToDto).ToList();

            return GenericResponse<PagedResult<ProductResponseDto>>.CreateSuccess(new PagedResult<ProductResponseDto>
            {
                Data = visibleProducts,
                TotalCount = products.TotalCount,
                Page = products.Page,
                PageSize = products.PageSize
            }, "Products retrieved successfully");
        }

        public async Task<GenericResponse<ProductResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return GenericResponse<ProductResponseDto>.CreateError("Product not found");
            }

            if (!IsBrandVisibleInWorkspace(product.Brand, workspaceId, userId))
            {
                return GenericResponse<ProductResponseDto>.CreateError("You are not allowed to access this product");
            }

            return GenericResponse<ProductResponseDto>.CreateSuccess(MapToDto(product), "Product retrieved successfully");
        }

        public async Task<GenericResponse<ProductResponseDto>> CreateAsync(Guid workspaceId, Guid userId, ProductCreateRequest request, CancellationToken cancellationToken = default)
        {
            var access = await EnsureBrandWorkspaceAccessAsync(request.BrandId, workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<ProductResponseDto>.CreateError(access.Message);
            }

            var imageValidation = ValidateImageFiles(request.ImageFiles);
            if (!imageValidation.Success)
            {
                return GenericResponse<ProductResponseDto>.CreateError(imageValidation.Message);
            }

            var imageUrls = await SaveProductImagesAsync(request.BrandId, request.ImageFiles, cancellationToken);

            var product = new Product
            {
                BrandId = request.BrandId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Images = JsonSerializer.Serialize(imageUrls)
            };

            var created = await _productRepository.AddAsync(product, cancellationToken);
            var loaded = await _productRepository.GetByIdAsync(created.Id, cancellationToken) ?? created;

            return GenericResponse<ProductResponseDto>.CreateSuccess(MapToDto(loaded), "Product created successfully");
        }

        public async Task<GenericResponse<ProductResponseDto>> UpdateAsync(Guid id, Guid workspaceId, Guid userId, ProductUpdateRequestDto request, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return GenericResponse<ProductResponseDto>.CreateError("Product not found");
            }

            if (!IsBrandVisibleInWorkspace(product.Brand, workspaceId, userId))
            {
                return GenericResponse<ProductResponseDto>.CreateError("You are not allowed to update this product");
            }

            if (request.BrandId.HasValue && request.BrandId.Value != product.BrandId)
            {
                var access = await EnsureBrandWorkspaceAccessAsync(request.BrandId.Value, workspaceId, userId, cancellationToken);
                if (!access.Success)
                {
                    return GenericResponse<ProductResponseDto>.CreateError(access.Message);
                }

                product.BrandId = request.BrandId.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                product.Name = request.Name;
            }

            if (request.Description != null)
            {
                product.Description = request.Description;
            }

            if (request.Price.HasValue)
            {
                product.Price = request.Price.Value;
            }

            var imageValidation = ValidateImageFiles(request.ImageFiles);
            if (!imageValidation.Success)
            {
                return GenericResponse<ProductResponseDto>.CreateError(imageValidation.Message);
            }

            var newImageUrls = await SaveProductImagesAsync(product.BrandId, request.ImageFiles, cancellationToken);
            if (newImageUrls.Count > 0)
            {
                var existingImages = ParseImages(product.Images);
                existingImages.AddRange(newImageUrls);
                product.Images = JsonSerializer.Serialize(existingImages);
            }

            await _productRepository.UpdateAsync(product, cancellationToken);
            var loaded = await _productRepository.GetByIdAsync(product.Id, cancellationToken) ?? product;

            return GenericResponse<ProductResponseDto>.CreateSuccess(MapToDto(loaded), "Product updated successfully");
        }

        public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return GenericResponse<bool>.CreateError("Product not found");
            }

            if (!IsBrandVisibleInWorkspace(product.Brand, workspaceId, userId))
            {
                return GenericResponse<bool>.CreateError("You are not allowed to delete this product");
            }

            product.IsDeleted = true;
            await _productRepository.UpdateAsync(product, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Product deleted successfully");
        }

        public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (product == null)
            {
                return GenericResponse<bool>.CreateError("Product not found");
            }

            if (!IsBrandVisibleInWorkspace(product.Brand, workspaceId, userId))
            {
                return GenericResponse<bool>.CreateError("You are not allowed to restore this product");
            }

            if (!product.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Product is not deleted");
            }

            product.IsDeleted = false;
            await _productRepository.UpdateAsync(product, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Product restored successfully");
        }

        private async Task<(bool Success, string Message)> EnsureBrandWorkspaceAccessAsync(Guid brandId, Guid workspaceId, Guid userId, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
            if (brand == null)
            {
                return (false, "Brand not found");
            }

            if (!IsBrandVisibleInWorkspace(brand, workspaceId, userId))
            {
                return (false, "You are not allowed to access this brand");
            }

            return (true, string.Empty);
        }

        private static bool IsBrandVisibleInWorkspace(Brand brand, Guid workspaceId, Guid userId)
        {
            return brand.WorkspaceId == workspaceId;
        }

        private static ProductResponseDto MapToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                BrandId = product.BrandId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Images = ParseImages(product.Images),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private static List<string> ParseImages(string? images)
        {
            if (string.IsNullOrWhiteSpace(images))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(images) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static (bool Success, string Message) ValidateImageFiles(List<IFormFile>? files)
        {
            if (files == null || files.Count == 0)
            {
                return (true, string.Empty);
            }

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    return (false, "Product image file is empty.");
                }

                if (file.Length > MaxImageBytes)
                {
                    return (false, "Product image file must be 10MB or smaller.");
                }

                if (!AllowedImageContentTypes.Contains(file.ContentType))
                {
                    return (false, "Product image must be a JPEG, PNG, WebP, or GIF file.");
                }
            }

            return (true, string.Empty);
        }

        private async Task<List<string>> SaveProductImagesAsync(Guid brandId, List<IFormFile>? files, CancellationToken cancellationToken)
        {
            var urls = new List<string>();
            if (files == null || files.Count == 0)
            {
                return urls;
            }

            var rootPath = _mediaStorageSettings.ResolveUploadRootPath(_environment?.ContentRootPath);

            var relativeDirectory = Path.Combine("uploads", "products", brandId.ToString("N"));
            var uploadDirectory = Path.Combine(rootPath, relativeDirectory);
            Directory.CreateDirectory(uploadDirectory);

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension;
                var fileName = $"{Guid.NewGuid():N}{safeExtension}";
                var filePath = Path.Combine(uploadDirectory, fileName);

                await using var stream = File.Create(filePath);
                await file.CopyToAsync(stream, cancellationToken);

                urls.Add($"/uploads/products/{brandId:N}/{fileName}");
            }

            return urls;
        }
    }
}
