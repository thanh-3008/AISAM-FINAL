using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController, Authorize, Route("api/audit-logs")]
public sealed class WorkspaceAuditLogsController(AisamContext db, AccessScope scope) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<WorkspaceAuditLogResponseDto>>>> Get(
        [FromQuery] AuditLogFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!scope.IsOwner && scope.Role != WorkspaceMemberRoleEnum.Manager)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                GenericResponse<PagedResult<WorkspaceAuditLogResponseDto>>.CreateError(
                    "Audit logs require Owner or Manager access.",
                    HttpStatusCode.Forbidden,
                    "RESOURCE_ACCESS_DENIED"));

        var query = ApplyFilters(db.AuditLogsForRead(scope.WorkspaceId), request);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var page = new PagedResult<WorkspaceAuditLogResponseDto>
        {
            Data = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Ok(GenericResponse<PagedResult<WorkspaceAuditLogResponseDto>>.CreateSuccess(page));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenericResponse<WorkspaceAuditLogResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!scope.IsOwner && scope.Role != WorkspaceMemberRoleEnum.Manager)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                GenericResponse<WorkspaceAuditLogResponseDto>.CreateError(
                    "Audit logs require Owner or Manager access.",
                    HttpStatusCode.Forbidden,
                    "RESOURCE_ACCESS_DENIED"));

        var log = await db.AuditLogsForRead(scope.WorkspaceId)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return log == null
            ? NotFound(GenericResponse<WorkspaceAuditLogResponseDto>.CreateError(
                "Audit log not found.", HttpStatusCode.NotFound))
            : Ok(GenericResponse<WorkspaceAuditLogResponseDto>.CreateSuccess(log));
    }

    private static IQueryable<WorkspaceAuditLogResponseDto> ApplyFilters(
        IQueryable<WorkspaceAuditLogResponseDto> query,
        AuditLogFilterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ActionType))
            query = query.Where(log => log.ActionType == request.ActionType);
        if (!string.IsNullOrWhiteSpace(request.TargetTable))
            query = query.Where(log => log.TargetTable == request.TargetTable);
        if (request.FromDate.HasValue)
            query = query.Where(log => log.CreatedAt >= request.FromDate.Value);
        if (request.ToDate.HasValue)
        {
            var exclusiveEnd = request.ToDate.Value.Date.AddDays(1);
            query = query.Where(log => log.CreatedAt < exclusiveEnd);
        }
        if (request.ActorId.HasValue)
            query = query.Where(log => log.ActorId == request.ActorId.Value);
        return query;
    }
}
