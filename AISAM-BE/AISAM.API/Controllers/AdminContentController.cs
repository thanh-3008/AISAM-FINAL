using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/content")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminContentController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminContentController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetAllContent(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetAllContentAsync(adminUserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> SetContentStatus(
        Guid id,
        [FromBody] SetContentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.SetContentStatusAsync(adminUserId, id, request.Status, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class SetContentStatusRequest
{
    public int Status { get; set; }
}
