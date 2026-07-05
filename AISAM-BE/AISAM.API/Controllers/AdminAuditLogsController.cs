using AISAM.Common;
using AISAM.Data.Enumeration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminAuditLogsController : ControllerBase
{
    [HttpGet]
    public ActionResult<GenericResponse<object>> GetAuditLogs()
    {
        var result = GenericResponse<object>.CreateSuccess(
            new { Items = Array.Empty<object>(), Total = 0 });
        return Ok(result);
    }
}
