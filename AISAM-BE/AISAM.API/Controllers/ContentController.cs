using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public sealed class ContentController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly IProfileRepository _profileRepository;
    private readonly IMediaStorageService _mediaStorageService;
    private static readonly HashSet<string> AllowedMediaContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "video/mp4",
        "video/webm",
        "video/quicktime"
    };
    private const long MaxMediaBytes = 50 * 1024 * 1024;

    public ContentController(
        IContentService contentService, 
        IProfileRepository profileRepository,
        IMediaStorageService? mediaStorageService = null)
    {
        _contentService = contentService;
        _profileRepository = profileRepository;
        _mediaStorageService = mediaStorageService ?? new UnconfiguredMediaStorageService();
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> Create(
        [FromBody] CreateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.CreateInWorkspaceAsync(GetWorkspaceId(), await GetProfileIdAsync(cancellationToken), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("media")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<GenericResponse<ContentMediaUploadResponse>>> UploadMedia(
        [FromForm] ContentMediaUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var file = request.File;
        if (file == null || file.Length <= 0)
        {
            return BadRequest(GenericResponse<ContentMediaUploadResponse>.CreateError("Media file is required."));
        }

        if (file.Length > MaxMediaBytes)
        {
            return BadRequest(GenericResponse<ContentMediaUploadResponse>.CreateError("Media file must be 50MB or smaller."));
        }

        if (!AllowedMediaContentTypes.Contains(file.ContentType))
        {
            return BadRequest(GenericResponse<ContentMediaUploadResponse>.CreateError("Media file must be JPEG, PNG, WebP, GIF, MP4, WebM, or MOV."));
        }

        var workspaceId = GetWorkspaceId();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeExtension = string.IsNullOrWhiteSpace(extension)
            ? (file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ? ".mp4" : ".jpg")
            : extension;
        var fileName = $"{Guid.NewGuid():N}{safeExtension}";

        string url;
        try
        {
            url = await _mediaStorageService.UploadAsync(file, $"content/{workspaceId:N}", fileName, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            var status = ex is ArgumentException ? HttpStatusCode.BadRequest : HttpStatusCode.ServiceUnavailable;
            return StatusCode((int)status,
                GenericResponse<ContentMediaUploadResponse>.CreateError(ex.Message, status));
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                GenericResponse<ContentMediaUploadResponse>.CreateError(
                    $"Upload failed: {ex.GetType().Name} - {ex.Message}",
                    HttpStatusCode.InternalServerError));
        }

        var response = new ContentMediaUploadResponse
        {
            Url = url,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Size = file.Length
        };

        return Ok(GenericResponse<ContentMediaUploadResponse>.CreateSuccess(response, "Media uploaded successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<ContentResponseDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] Guid? brandId = null,
        [FromQuery] AdTypeEnum? adType = null,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] ContentStatusEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.GetPagedByWorkspaceAsync(GetWorkspaceId(), new PaginationRequest
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        }, brandId, adType, includeDeleted, status, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{contentId:guid}")]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> GetById(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.GetByIdInWorkspaceAsync(contentId, GetWorkspaceId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{contentId:guid}")]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> Update(
        Guid contentId,
        [FromBody] UpdateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.UpdateInWorkspaceAsync(contentId, GetWorkspaceId(), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{contentId:guid}/clone")]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> Clone(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.CloneInWorkspaceAsync(contentId, GetWorkspaceId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{contentId:guid}/publish/{integrationId:guid}")]
    public async Task<ActionResult<GenericResponse<PublishResultDto>>> Publish(
        Guid contentId,
        Guid integrationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.PublishAsync(
            contentId,
            integrationId,
            await GetProfileIdAsync(cancellationToken),
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{contentId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> SoftDelete(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.SoftDeleteInWorkspaceAsync(contentId, GetWorkspaceId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{contentId:guid}/restore")]
    public async Task<ActionResult<GenericResponse<bool>>> Restore(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.RestoreInWorkspaceAsync(contentId, GetWorkspaceId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private async Task<Guid> GetProfileIdAsync(CancellationToken cancellationToken)
    {
        return await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
    }

    private Guid GetWorkspaceId() => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);

    private sealed class UnconfiguredMediaStorageService : IMediaStorageService
    {
        public Task<string> UploadAsync(IFormFile file, string folder, string fileName, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Supabase storage is not configured.");
            
        public Task<string> UploadBytesAsync(byte[] data, string folder, string fileName, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Supabase storage is not configured.");
    }
}
