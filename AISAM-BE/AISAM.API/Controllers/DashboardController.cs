using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IProfileRepository? _profileRepository;

    public DashboardController(IDashboardService dashboardService, IProfileRepository? profileRepository = null)
    {
        _dashboardService = dashboardService;
        _profileRepository = profileRepository;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GenericResponse<DashboardSummaryDto>>> GetSummary(CancellationToken cancellationToken = default)
    {
        var profileId = _profileRepository == null
            ? ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext)
            : await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
        var result = await _dashboardService.GetWorkspaceSummaryAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            profileId,
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
