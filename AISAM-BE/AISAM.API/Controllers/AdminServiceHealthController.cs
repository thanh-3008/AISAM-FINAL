using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/service-health")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminServiceHealthController : ControllerBase
{
    private readonly IBackgroundJobHealthService _healthService;

    public AdminServiceHealthController(IBackgroundJobHealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetHealth()
    {
        var status = await _healthService.GetStatusAsync();
        return Ok(GenericResponse<object>.CreateSuccess(status));
    }
}
