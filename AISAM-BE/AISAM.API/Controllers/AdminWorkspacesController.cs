using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/workspaces")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminWorkspacesController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminWorkspacesController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetWorkspaces(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? type = null,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetWorkspacesAsync(adminUserId, request, type, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenericResponse<object>>> GetWorkspaceDetail(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetWorkspaceDetailAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> SetWorkspaceStatus(
        Guid id,
        [FromBody] SetWorkspaceStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.SetWorkspaceStatusAsync(adminUserId, id, request.Status, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteWorkspace(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.DeleteWorkspaceAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class SetWorkspaceStatusRequest
{
    public int Status { get; set; }
}
