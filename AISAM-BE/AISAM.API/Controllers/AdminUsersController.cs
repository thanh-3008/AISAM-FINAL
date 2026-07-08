using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminUsersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<UserListDto>>>> GetUsers(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetUsersAsync(adminUserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenericResponse<object>>> GetUserDetail(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetUserDetailAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> SetUserStatus(
        Guid id,
        [FromBody] SetStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.SetUserStatusAsync(adminUserId, id, request.IsActive, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.DeleteUserAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<GenericResponse<bool>>> SetUserRole(Guid id, [FromBody] SetRoleRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.SetUserRoleAsync(adminUserId, id, request.Role, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class SetStatusRequest
{
    public bool IsActive { get; set; }
}

public class SetRoleRequest
{
    public int Role { get; set; }
}
