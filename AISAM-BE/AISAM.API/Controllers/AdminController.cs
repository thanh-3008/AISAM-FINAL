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
    private readonly IPlanService _planService;

    public AdminController(IAdminService adminService, IPlanService planService)
    {
        _adminService = adminService;
        _planService = planService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<GenericResponse<AdminDashboardDto>>> GetDashboard(
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetDashboardAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminUserListDto>>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _adminService.GetUsersAsync(page, pageSize, searchTerm, sortBy, sortDescending, role, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<GenericResponse<AdminUserDetailDto>>> GetUserDetail(
        Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUserDetailAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<ActionResult<GenericResponse<bool>>> UpdateUserRole(
        Guid id, [FromBody] AdminUpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.UpdateUserRoleAsync(id, request.Role, request.Reason, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> UpdateUserStatus(
        Guid id, [FromBody] AdminUpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.UpdateUserStatusAsync(id, request.IsActive, request.Reason, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("workspaces")]
    public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminWorkspaceListDto>>>> GetWorkspaces(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? plan = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100;
        var result = await _adminService.GetWorkspacesAsync(page, pageSize, searchTerm, status, plan, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("workspaces/{id:guid}")]
    public async Task<ActionResult<GenericResponse<AdminWorkspaceDetailDto>>> GetWorkspaceDetail(
        Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetWorkspaceDetailAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("subscriptions")]
    public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminSubscriptionDto>>>> GetSubscriptions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null, [FromQuery] string? plan = null,
        [FromQuery] Guid? workspaceId = null, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1; if (pageSize > 100) pageSize = 100;
        var result = await _adminService.GetSubscriptionsAsync(page, pageSize, status, plan, workspaceId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("subscriptions/{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> UpdateSubscription(
        Guid id, [FromBody] AdminUpdateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.UpdateSubscriptionAsync(id, request.Plan, request.IsActive, request.EndDate, request.Reason, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("payments")]
    public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminPaymentDto>>>> GetPayments(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null, [FromQuery] Guid? userId = null, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1; if (pageSize > 100) pageSize = 100;
        var result = await _adminService.GetPaymentsAsync(page, pageSize, status, userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("payments/{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> UpdatePaymentStatus(
        Guid id, [FromBody] AdminUpdatePaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.UpdatePaymentStatusAsync(id, request.Status, request.Reason, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("plans")]
    public async Task<ActionResult<GenericResponse<List<AdminPlanDto>>>> GetPlans(CancellationToken cancellationToken = default)
    {
        var result = await _planService.GetAllAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("plans/{id:guid}")]
    public async Task<ActionResult<GenericResponse<AdminPlanDto>>> GetPlan(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _planService.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("plans")]
    public async Task<ActionResult<GenericResponse<AdminPlanDto>>> CreatePlan([FromBody] AdminCreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _planService.CreateAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("plans/{id:guid}")]
    public async Task<ActionResult<GenericResponse<AdminPlanDto>>> UpdatePlan(Guid id, [FromBody] AdminUpdatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _planService.UpdateAsync(id, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("plans/{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeletePlan(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _planService.DeleteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
