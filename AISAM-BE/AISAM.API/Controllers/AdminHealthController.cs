using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/system-health")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminHealthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;

    public AdminHealthController(
        IUserRepository userRepository,
        ISystemSettingRepository systemSettingRepository)
    {
        _userRepository = userRepository;
        _systemSettingRepository = systemSettingRepository;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetSystemHealth(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var checks = new List<object>();

        try
        {
            var userCount = await _userRepository.GetCountAsync(cancellationToken);
            checks.Add(new { Name = "Database", Status = "Healthy", Detail = $"{userCount} users" });
        }
        catch (Exception ex) { checks.Add(new { Name = "Database", Status = "Unhealthy", Detail = ex.Message }); }

        try
        {
            var settings = await _systemSettingRepository.GetAllAsync();
            checks.Add(new { Name = "Configuration", Status = "Healthy", Detail = $"{settings.Count} settings" });
        }
        catch (Exception ex) { checks.Add(new { Name = "Configuration", Status = "Unhealthy", Detail = ex.Message }); }

        var allHealthy = true;
        foreach (dynamic c in checks)
        {
            if (c.Status != "Healthy")
            {
                allHealthy = false;
                break;
            }
        }

        return Ok(GenericResponse<object>.CreateSuccess(new
        {
            OverallStatus = allHealthy ? "Healthy" : "Degraded",
            Checks = checks,
            CheckedAt = DateTime.UtcNow
        }));
    }
}
