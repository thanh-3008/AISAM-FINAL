using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/workspace-dashboard")]
[Authorize]
public sealed class WorkspaceDashboardController : ControllerBase
{
    private readonly IWorkspaceDashboardService _workspaceDashboardService;

    public WorkspaceDashboardController(IWorkspaceDashboardService workspaceDashboardService)
    {
        _workspaceDashboardService = workspaceDashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GenericResponse<WorkspaceDashboardSummaryDto>>> GetSummary(
        CancellationToken cancellationToken = default)
    {
        var result = await _workspaceDashboardService.GetSummaryAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
