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
        [FromQuery] string? actionType = null, [FromQuery] string? targetTable = null,
        [FromQuery] string? searchTerm = null, [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null, [FromQuery] Guid? actorId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new AISAM.Common.Dtos.Request.AuditLogFilterRequest 
        { 
            Page = page, 
            PageSize = pageSize,
            ActionType = actionType,
            TargetTable = targetTable,
            SearchTerm = searchTerm,
            FromDate = fromDate,
            ToDate = toDate,
            ActorId = actorId
        };
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
            ActorEmail = log.Actor?.Email ?? "Unknown",
            ActorName = log.Actor?.FullName ?? "Unknown",
            HasDiff = log.OldValues != null || log.NewValues != null
        }).ToList();

        return Ok(GenericResponse<object>.CreateSuccess(new { Data = items, TotalCount = result.TotalCount }));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportAuditLogsCsv(
        [FromQuery] string? actionType = null, [FromQuery] string? targetTable = null,
        [FromQuery] string? searchTerm = null, [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null, [FromQuery] Guid? actorId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new AISAM.Common.Dtos.Request.AuditLogFilterRequest 
        { 
            Page = 1, 
            PageSize = 10000, // Large number for export
            ActionType = actionType,
            TargetTable = targetTable,
            SearchTerm = searchTerm,
            FromDate = fromDate,
            ToDate = toDate,
            ActorId = actorId
        };
        
        var result = await _auditLogRepository.GetPagedAsync(request, cancellationToken);
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,Date,Action,Table,TargetId,ActorEmail,ActorName,Notes");

        foreach (var log in result.Data)
        {
            var date = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            var notes = log.Notes?.Replace("\"", "\"\"") ?? "";
            sb.AppendLine($"{log.Id},{date},{log.ActionType},{log.TargetTable},{log.TargetId},{log.Actor?.Email},{log.Actor?.FullName},\"{notes}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"AuditLogs_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenericResponse<object>>> GetAuditLogDetail(
        Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _auditLogRepository.GetByIdAsync(id, cancellationToken);
        if (log == null)
            return NotFound(GenericResponse<object>.CreateError("Audit log not found.", System.Net.HttpStatusCode.NotFound));

        return Ok(GenericResponse<object>.CreateSuccess(new
        {
            log.Id,
            log.ActorId,
            log.ActionType,
            log.TargetTable,
            log.TargetId,
            log.Notes,
            log.OldValues,
            log.NewValues,
            log.CreatedAt,
            ActorEmail = log.Actor?.Email ?? "Unknown"
        }));
    }
}
