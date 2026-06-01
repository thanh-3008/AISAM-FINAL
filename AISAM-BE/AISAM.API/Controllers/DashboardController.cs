using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
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

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GenericResponse<DashboardSummaryDto>>> GetSummary(CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetSummaryAsync(GetProfileId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetProfileId()
    {
        return ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
    }
}
