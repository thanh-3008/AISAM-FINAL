using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize]
public sealed class TagsController : ControllerBase
{
    private readonly IContentService _contentService;

    public TagsController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<List<string>>>> GetTags(CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var result = await _contentService.GetDistinctTagsByWorkspaceAsync(workspaceId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
