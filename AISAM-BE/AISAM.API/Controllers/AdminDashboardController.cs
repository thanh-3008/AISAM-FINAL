using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GenericResponse<object>>> GetSummary(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminDashboardService.GetSummaryAsync(adminUserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("charts")]
    public async Task<ActionResult<GenericResponse<object>>> GetCharts(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminDashboardService.GetChartsAsync(adminUserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("top-workspaces")]
    public async Task<ActionResult<GenericResponse<object>>> GetTopWorkspaces(
        [FromQuery] int limit = 10, 
        [FromQuery] string period = "month",
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminDashboardService.GetTopWorkspacesAsync(adminUserId, limit, period, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
