using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class ContentService : IContentService
{
    private readonly IContentRepository _contentRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IProductRepository _productRepository;

    public ContentService(
        IContentRepository contentRepository,
        IBrandRepository brandRepository,
        IProductRepository productRepository)
    {
        _contentRepository = contentRepository;
        _brandRepository = brandRepository;
        _productRepository = productRepository;
    }

    public async Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateBrandAndProductAsync(profileId, request.BrandId, request.ProductId, cancellationToken);
        if (!validation.Success)
        {
            return GenericResponse<ContentResponseDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
        }

        var content = new Content
        {
            ProfileId = profileId,
            BrandId = request.BrandId,
            ProductId = request.ProductId,
            AdType = request.AdType,
            Title = request.Title,
            TextContent = request.TextContent,
            ImageUrl = FormatImageUrlForJsonb(request.ImageUrl),
            VideoUrl = request.VideoUrl,
            StyleDescription = request.StyleDescription,
            ContextDescription = request.ContextDescription,
            RepresentativeCharacter = request.RepresentativeCharacter,
            Status = ContentStatusEnum.Draft
        };

        await _contentRepository.AddAsync(content, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), "Content created successfully.");
    }

    public async Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        if (brandId.HasValue)
        {
            var validation = await ValidateBrandAndProductAsync(profileId, brandId.Value, null, cancellationToken);
            if (!validation.Success)
            {
                return GenericResponse<PagedResult<ContentResponseDto>>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
            }
        }

        var result = await _contentRepository.GetPagedByProfileIdAsync(profileId, request, brandId, adType, includeDeleted, status, cancellationToken);
        return GenericResponse<PagedResult<ContentResponseDto>>.CreateSuccess(new PagedResult<ContentResponseDto>
        {
            Data = result.Data.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        }, "Contents retrieved successfully.");
    }

    public async Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return NotFound();
        }

        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), "Content retrieved successfully.");
    }

    public async Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, UpdateContentRequest request, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return NotFound();
        }

        var validation = await ValidateBrandAndProductAsync(profileId, content.BrandId, request.ProductId, cancellationToken);
        if (!validation.Success)
        {
            return GenericResponse<ContentResponseDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);
        }

        if (request.ProductId.HasValue) content.ProductId = request.ProductId;
        if (request.AdType.HasValue) content.AdType = request.AdType.Value;
        if (request.Title != null) content.Title = request.Title;
        if (request.TextContent != null) content.TextContent = request.TextContent;
        if (request.ImageUrl != null) content.ImageUrl = FormatImageUrlForJsonb(request.ImageUrl);
        if (request.VideoUrl != null) content.VideoUrl = request.VideoUrl;
        if (request.StyleDescription != null) content.StyleDescription = request.StyleDescription;
        if (request.ContextDescription != null) content.ContextDescription = request.ContextDescription;
        if (request.RepresentativeCharacter != null) content.RepresentativeCharacter = request.RepresentativeCharacter;

        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(content), "Content updated successfully.");
    }

    public async Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var existing = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null || existing.ProfileId != profileId)
        {
            return NotFound();
        }

        var clone = new Content
        {
            ProfileId = existing.ProfileId,
            BrandId = existing.BrandId,
            Brand = existing.Brand,
            ProductId = existing.ProductId,
            Product = existing.Product,
            AdType = existing.AdType,
            Title = existing.Title,
            TextContent = existing.TextContent,
            ImageUrl = existing.ImageUrl,
            VideoUrl = existing.VideoUrl,
            StyleDescription = existing.StyleDescription,
            ContextDescription = existing.ContextDescription,
            RepresentativeCharacter = existing.RepresentativeCharacter,
            Status = ContentStatusEnum.Draft
        };

        await _contentRepository.AddAsync(clone, cancellationToken);
        return GenericResponse<ContentResponseDto>.CreateSuccess(MapToDto(clone), "Content cloned successfully.");
    }

    public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError("Content not found.", HttpStatusCode.NotFound);
        }

        content.IsDeleted = true;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Content deleted successfully.");
    }

    public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
        if (content == null || content.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError("Content not found.", HttpStatusCode.NotFound);
        }

        if (!content.IsDeleted)
        {
            return GenericResponse<bool>.CreateError("Content is not deleted.", HttpStatusCode.BadRequest);
        }

        content.IsDeleted = false;
        content.Status = ContentStatusEnum.Draft;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Content restored successfully.");
    }

    private async Task<GenericResponse<bool>> ValidateBrandAndProductAsync(Guid profileId, Guid brandId, Guid? productId, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        if (brand == null || brand.ProfileId != profileId)
        {
            return GenericResponse<bool>.CreateError("Brand not found.", HttpStatusCode.NotFound);
        }

        if (productId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
            if (product == null)
            {
                return GenericResponse<bool>.CreateError("Product not found.", HttpStatusCode.NotFound);
            }

            if (product.BrandId != brandId)
            {
                return GenericResponse<bool>.CreateError("Product does not belong to the selected brand.", HttpStatusCode.BadRequest);
            }
        }

        return GenericResponse<bool>.CreateSuccess(true);
    }

    private static GenericResponse<ContentResponseDto> NotFound()
    {
        return GenericResponse<ContentResponseDto>.CreateError("Content not found.", HttpStatusCode.NotFound);
    }

    private static string? FormatImageUrlForJsonb(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var trimmed = imageUrl.Trim();
        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
        {
            return trimmed;
        }

        return System.Text.Json.JsonSerializer.Serialize(new[] { trimmed });
    }

    private static ContentResponseDto MapToDto(Content content)
    {
        return new ContentResponseDto
        {
            Id = content.Id,
            ProfileId = content.ProfileId,
            BrandId = content.BrandId,
            BrandName = content.Brand?.Name,
            ProductId = content.ProductId,
            AdType = content.AdType,
            Title = content.Title,
            TextContent = content.TextContent,
            ImageUrl = content.ImageUrl,
            VideoUrl = content.VideoUrl,
            StyleDescription = content.StyleDescription,
            ContextDescription = content.ContextDescription,
            RepresentativeCharacter = content.RepresentativeCharacter,
            Status = content.Status,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }
}
