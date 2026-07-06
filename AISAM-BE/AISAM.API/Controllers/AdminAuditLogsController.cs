using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminAuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AdminAuditLogsController(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await _auditLogRepository.GetPagedAsync(request, cancellationToken);
        var items = result.Data.Select(log => new
        {
            log.Id,
            log.ActorId,
            log.ActionType,
            log.TargetTable,
            log.TargetId,
            log.Notes,
            log.CreatedAt,
            ActorEmail = log.Actor?.Email ?? "Unknown"
        }).ToList();

        return Ok(GenericResponse<object>.CreateSuccess(new { Items = items, Total = result.TotalCount }));
    }
}
