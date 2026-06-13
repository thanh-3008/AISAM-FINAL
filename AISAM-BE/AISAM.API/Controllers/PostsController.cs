using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/posts")]
[Authorize]
public sealed class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<PostListItemDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? brandId = null,
        [FromQuery] ContentStatusEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _postService.GetPagedAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            new PaginationRequest
            {
                Page = page,
                PageSize = pageSize
            },
            brandId,
            status,
            cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{postId:guid}")]
    public async Task<ActionResult<GenericResponse<PostListItemDto>>> GetById(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var result = await _postService.GetByIdAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), postId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
