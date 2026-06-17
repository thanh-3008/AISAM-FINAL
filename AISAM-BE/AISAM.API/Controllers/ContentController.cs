using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public sealed class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> Create(
        [FromBody] CreateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.CreateInWorkspaceAsync(GetWorkspaceId(), GetProfileId(), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
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

    [HttpPost("{contentId:guid}/approve")]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> Approve(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.ApproveInWorkspaceAsync(contentId, GetWorkspaceId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{contentId:guid}/reject")]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> Reject(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentService.RejectInWorkspaceAsync(contentId, GetWorkspaceId(), cancellationToken);
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
            GetProfileId(),
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

    private Guid GetProfileId()
    {
        return ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
    }

    private Guid GetWorkspaceId() => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
}
