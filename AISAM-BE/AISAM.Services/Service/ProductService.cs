using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Text.Json;

namespace AISAM.Services.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IMediaStorageService? _mediaStorageService;
        private const int MaxImageCount = 5;
        private const long MaxImageBytes = 10 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        public ProductService(IProductRepository productRepository, IBrandRepository brandRepository, IMediaStorageService mediaStorageService)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _mediaStorageService = mediaStorageService;
        }

        public ProductService(IProductRepository productRepository, IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
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

            var product = new Product
            {
                BrandId = request.BrandId,
                Name = request.Name,
                Description = request.Description,
                Category = NormalizeOptional(request.Category),
                PrimaryUse = NormalizeOptional(request.PrimaryUse),
                Usp = NormalizeOptional(request.Usp),
                TargetAudience = NormalizeOptional(request.TargetAudience),
                VisualIdentity = NormalizeOptional(request.VisualIdentity),
                ProductUrl = NormalizeUrl(request.ProductUrl),
                Price = request.Price,
                Stock = request.Stock,
                Images = JsonSerializer.Serialize(new List<string>())
            };

            var imageError = ValidateImages(request.ImageFiles);
            if (imageError != null) return GenericResponse<ProductResponseDto>.CreateError(imageError);
            if (request.ImageFiles is { Count: > 0 })
            {
                try { product.Images = JsonSerializer.Serialize(await UploadImagesAsync(product.Id, product.BrandId, request.ImageFiles, cancellationToken)); }
                catch (Exception ex) { return GenericResponse<ProductResponseDto>.CreateError(ex.Message); }
            }

            product.KnowledgeProfile = BuildKnowledgeProfile(
                NormalizeOptional(request.KnowledgeProfile),
                product,
                DeserializeImages(product.Images));

            var created = await _productRepository.AddAsync(product, cancellationToken);
            var loaded = await _productRepository.GetByIdAsync(created.Id, cancellationToken) ?? created;

            return GenericResponse<ProductResponseDto>.CreateSuccess(MapToDto(loaded), "Product created successfully");
        }

        public async Task<GenericResponse<ProductResponseDto>> CreateReviewedImportAsync(Guid workspaceId, Guid userId, ProductImportReviewRequest request, CancellationToken cancellationToken = default)
        {
            var access = await EnsureBrandWorkspaceAccessAsync(request.BrandId, workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<ProductResponseDto>.CreateError(access.Message);
            }

            var images = request.Images
                .Where(IsHttpUrl)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxImageCount)
                .ToList();

            var benefits = CleanList(request.Benefits).Take(4).ToList();
            var features = CleanList(request.Features);
            var keywords = CleanList(request.Keywords);

            var product = new Product
            {
                BrandId = request.BrandId,
                Name = request.ProductName.Trim(),
                Description = NormalizeOptional(request.Description),
                ProductUrl = NormalizeUrl(request.SourceUrl),
                Price = request.Price,
                Stock = 0,
                Images = JsonSerializer.Serialize(images),
                PrimaryUse = benefits.Count > 0 ? string.Join("; ", benefits) : null,
                Usp = benefits.FirstOrDefault(),
                TargetAudience = NormalizeOptional(request.TargetAudience),
                VisualIdentity = images.Count > 0
                    ? $"Imported reference images: {images.Count}. Use these URLs as real product references for future image generation."
                    : null
            };

            product.KnowledgeProfile = JsonSerializer.Serialize(new
            {
                importStatus = "Reviewed",
                sourceUrl = product.ProductUrl,
                productName = product.Name,
                description = product.Description,
                price = product.Price,
                benefits,
                features,
                targetAudience = NormalizeOptional(request.TargetAudience),
                tone = NormalizeOptional(request.Tone),
                keywords,
                recommendedCTA = NormalizeOptional(request.RecommendedCTA),
                images,
                importedAt = DateTime.UtcNow
            });

            var created = await _productRepository.AddAsync(product, cancellationToken);
            var loaded = await _productRepository.GetByIdAsync(created.Id, cancellationToken) ?? created;

            return GenericResponse<ProductResponseDto>.CreateSuccess(MapToDto(loaded), "Reviewed product imported successfully");
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

            if (request.Category != null)
            {
                product.Category = NormalizeOptional(request.Category);
            }

            if (request.PrimaryUse != null)
            {
                product.PrimaryUse = NormalizeOptional(request.PrimaryUse);
            }

            if (request.Usp != null)
            {
                product.Usp = NormalizeOptional(request.Usp);
            }

            if (request.TargetAudience != null)
            {
                product.TargetAudience = NormalizeOptional(request.TargetAudience);
            }

            if (request.VisualIdentity != null)
            {
                product.VisualIdentity = NormalizeOptional(request.VisualIdentity);
            }

            if (request.ProductUrl != null)
            {
                product.ProductUrl = NormalizeUrl(request.ProductUrl);
            }

            if (request.Price.HasValue)
            {
                product.Price = request.Price.Value;
            }

            if (request.Stock.HasValue)
            {
                product.Stock = request.Stock.Value;
            }

            var updateImageError = ValidateImages(request.ImageFiles);
            if (updateImageError != null) return GenericResponse<ProductResponseDto>.CreateError(updateImageError);
            if (request.ImageFiles is { Count: > 0 })
            {
                try
                {
                    var images = !string.IsNullOrWhiteSpace(product.Images) ? JsonSerializer.Deserialize<List<string>>(product.Images) ?? [] : [];
                    if (images.Count + request.ImageFiles.Count > MaxImageCount)
                        return GenericResponse<ProductResponseDto>.CreateError($"A product can contain at most {MaxImageCount} images.");
                    images.AddRange(await UploadImagesAsync(product.Id, product.BrandId, request.ImageFiles, cancellationToken));
                    product.Images = JsonSerializer.Serialize(images);
                }
                catch (Exception ex) { return GenericResponse<ProductResponseDto>.CreateError(ex.Message); }
            }

            product.KnowledgeProfile = BuildKnowledgeProfile(
                request.KnowledgeProfile != null ? NormalizeOptional(request.KnowledgeProfile) : product.KnowledgeProfile,
                product,
                DeserializeImages(product.Images));

            await _productRepository.UpdateAsync(product, cancellationToken);
            var loaded = await _productRepository.GetByIdAsync(product.Id, cancellationToken) ?? product;

            return GenericResponse<ProductResponseDto>.CreateSuccess(MapToDto(loaded), "Product updated successfully");
        }

        public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (product == null)
            {
                return GenericResponse<bool>.CreateError("Product not found");
            }

            if (!IsBrandVisibleInWorkspace(product.Brand, workspaceId, userId))
            {
                return GenericResponse<bool>.CreateError("You are not allowed to delete this product");
            }

            if (product.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Product is already deleted");
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
                Category = product.Category,
                PrimaryUse = product.PrimaryUse,
                Usp = product.Usp,
                TargetAudience = product.TargetAudience,
                VisualIdentity = product.VisualIdentity,
                KnowledgeProfile = product.KnowledgeProfile,
                ProductUrl = product.ProductUrl,
                Price = product.Price,
                Stock = product.Stock,
                Images = DeserializeImages(product.Images),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static List<string> CleanList(IEnumerable<string>? values)
        {
            return values?
                .Select(NormalizeOptional)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
        }

        private static bool IsHttpUrl(string? value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static string? NormalizeUrl(string? value)
        {
            var normalized = NormalizeOptional(value);
            return IsHttpUrl(normalized) ? normalized : null;
        }

        private static List<string> DeserializeImages(string? images)
        {
            if (string.IsNullOrWhiteSpace(images)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(images) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }

        private static string? BuildKnowledgeProfile(string? providedProfile, Product product, IReadOnlyCollection<string> imageUrls)
        {
            if (!string.IsNullOrWhiteSpace(providedProfile))
            {
                return providedProfile.Trim();
            }

            var lines = new List<string>
            {
                $"Tên sản phẩm: {product.Name}"
            };

            AddLine(lines, "Ngành hàng", product.Category);
            AddLine(lines, "Mô tả", product.Description);
            AddLine(lines, "Công dụng chính", product.PrimaryUse);
            AddLine(lines, "Điểm khác biệt/USP", product.Usp);
            AddLine(lines, "Link sản phẩm", product.ProductUrl);
            AddLine(lines, "Đối tượng mục tiêu", product.TargetAudience);
            AddLine(lines, "Định hình hình ảnh", product.VisualIdentity);

            if (imageUrls.Count > 0)
            {
                lines.Add($"Ảnh tham khảo: {imageUrls.Count} ảnh sản phẩm đã được lưu. Khi tạo ảnh, xem đây là nguồn tham chiếu ngoại hình sản phẩm.");
            }

            return lines.Count > 1 ? string.Join(Environment.NewLine, lines) : null;
        }

        private static void AddLine(ICollection<string> lines, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                lines.Add($"{label}: {value.Trim()}");
            }
        }

        private string? ValidateImages(IReadOnlyCollection<Microsoft.AspNetCore.Http.IFormFile>? files)
        {
            if (files is null || files.Count == 0) return null;
            if (_mediaStorageService is null) return "Product image storage is not configured.";
            if (files.Count > MaxImageCount) return $"You can upload at most {MaxImageCount} product images.";
            foreach (var file in files)
            {
                if (file.Length <= 0) return $"Image '{file.FileName}' is empty.";
                if (file.Length > MaxImageBytes) return $"Image '{file.FileName}' exceeds the 10 MB limit.";
                if (!AllowedImageTypes.Contains(file.ContentType)) return $"Image '{file.FileName}' must be JPEG, PNG, WEBP, or GIF.";
            }
            return null;
        }

        private async Task<List<string>> UploadImagesAsync(Guid productId, Guid brandId, IReadOnlyCollection<Microsoft.AspNetCore.Http.IFormFile> files, CancellationToken cancellationToken)
        {
            var urls = new List<string>();
            var index = 0;
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{productId:N}-{index++}-{Guid.NewGuid():N}{extension}";
                urls.Add(await _mediaStorageService!.UploadAsync(file, $"products/{brandId:N}", fileName, cancellationToken));
            }
            return urls;
        }
    }
}
