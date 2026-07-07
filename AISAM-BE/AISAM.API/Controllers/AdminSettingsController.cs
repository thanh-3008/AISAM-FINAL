using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminSettingsController : ControllerBase
{
    private readonly IAdminSettingsService _adminSettingsService;

    public AdminSettingsController(IAdminSettingsService adminSettingsService)
    {
        _adminSettingsService = adminSettingsService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetAllSettings(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminSettingsService.GetAllSettingsAsync(adminUserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch]
    public async Task<ActionResult<GenericResponse<bool>>> UpsertSettingsBatch(
        [FromBody] Dictionary<string, string> settings,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminSettingsService.UpsertSettingsBatchAsync(adminUserId, settings, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
