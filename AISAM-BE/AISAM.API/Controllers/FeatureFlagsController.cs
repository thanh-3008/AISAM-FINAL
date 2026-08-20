using AISAM.Common;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/feature-flags")]
[Authorize]
public sealed class FeatureFlagsController : ControllerBase
{
    private readonly ISystemSettingRepository _settingRepo;

    public FeatureFlagsController(ISystemSettingRepository settingRepo)
    {
        _settingRepo = settingRepo;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetFeatureFlags()
    {
        var setting = await _settingRepo.GetByKeyAsync("system.enabled_features");
        var features = new List<string>();
        if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
        {
            try
            {
                features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(setting.Value) ?? new List<string>();
            }
            catch { }
        }

        var maintenanceSetting = await _settingRepo.GetByKeyAsync("system.maintenance_mode");
        var maintenance = false;
        if (maintenanceSetting != null && bool.TryParse(maintenanceSetting.Value.Trim('"'), out var val))
            maintenance = val;

        return Ok(GenericResponse<object>.CreateSuccess(new
        {
            EnabledFeatures = features,
            MaintenanceMode = maintenance
        }));
    }
}
