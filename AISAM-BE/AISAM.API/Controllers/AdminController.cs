using AISAM.Common;
using AISAM.Common.Dtos.Admin;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<GenericResponse<AdminDashboardDto>>> GetDashboard(
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetDashboardAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
