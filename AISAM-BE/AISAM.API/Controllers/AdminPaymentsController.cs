using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminPaymentsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminPaymentsController(IAdminService adminService, IAdminDashboardService adminDashboardService)
    {
        _adminService = adminService;
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetPayments(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetPaymentsAsync(adminUserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("revenue/stats")]
    public async Task<ActionResult<GenericResponse<object>>> GetRevenueStats(
        [FromQuery] string period = "monthly",
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminDashboardService.GetRevenueStatsAsync(adminUserId, period, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
