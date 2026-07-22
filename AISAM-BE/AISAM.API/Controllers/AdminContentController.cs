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
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? status = null,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var request = new PaginationRequest { Page = page, PageSize = pageSize, SearchTerm = search };
        var result = await _adminService.GetAllContentAsync(adminUserId, request, status, cancellationToken);
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

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteContent(Guid id, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.DeleteContentAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class SetContentStatusRequest
{
    public int Status { get; set; }
}
