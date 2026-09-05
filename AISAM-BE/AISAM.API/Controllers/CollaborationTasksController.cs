using AISAM.Common;
using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AISAM.API.Controllers;

[ApiController, Authorize, Route("api/collaboration-tasks")]
public sealed class CollaborationTasksController(AisamContext db, AccessScope scope, CollaborationAccessService access, IConfiguration configuration, ContentAuthorizationService authorization) : ControllerBase
{
    public sealed record AssignRequest(Guid ContentId, Guid TeamId, Guid AssigneeId, Guid? IntegrationId, [Required, MaxLength(255)] string Title);
    public sealed record GrantRequest([Required, MaxLength(1000)] string Reason, DateTime? ExpiresAt, bool CanEdit = true);
    public sealed record StatusRequest(CollaborationTaskStatus Status);

    [HttpPost("preflight")]
    public async Task<IActionResult> Preflight(AssignRequest request, CancellationToken ct)
    {
        await authorization.EnsureAsync(scope.WorkspaceId, request.ContentId, ContentAction.Assign, request.IntegrationId, ct);
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer || !await db.Contents.AnyAsync(c => c.Id == request.ContentId, ct)) return Forbid();
        var allowed = await access.HasTeamAccessAsync(scope.WorkspaceId, request.TeamId, request.AssigneeId, request.ContentId, request.IntegrationId, ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { Result = allowed ? "ALLOWED" : "ADDITIONAL_ACCESS_REQUIRED" }));
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer) return Forbid();
        await access.ExpireAsync(DateTime.UtcNow, scope.WorkspaceId, ct);
        var tasks = await db.CollaborationTasks.AsNoTracking().Select(t => new { t.Id, t.Title, t.ContentId, t.TeamId, t.AssigneeId, t.Status, t.BlockedReason, t.CreatedAt, t.UpdatedAt }).ToListAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(tasks));
    }

    [HttpPost]
    public async Task<IActionResult> Assign(AssignRequest request, CancellationToken ct)
    {
        await authorization.EnsureAsync(scope.WorkspaceId, request.ContentId, ContentAction.Assign, request.IntegrationId, ct);
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer || !await db.Contents.AnyAsync(c => c.Id == request.ContentId, ct)) return Forbid();
        if (!await ValidTarget(request, ct)) return BadRequest("Assignment target is unavailable.");
        var allowed = await access.HasTeamAccessAsync(scope.WorkspaceId, request.TeamId, request.AssigneeId, request.ContentId, request.IntegrationId, ct);
        var task = new CollaborationTask { WorkspaceId = scope.WorkspaceId, ContentId = request.ContentId, TeamId = request.TeamId, AssigneeId = request.AssigneeId,
            IntegrationId = request.IntegrationId, AssignedBy = scope.UserId, Title = request.Title.Trim(), Status = allowed ? CollaborationTaskStatus.Pending : CollaborationTaskStatus.Blocked,
            BlockedReason = allowed ? null : "ADDITIONAL_ACCESS_REQUIRED" };
        db.CollaborationTasks.Add(task);
        if (allowed) await access.RecordParticipationAsync(task, scope.UserId, ct);
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { task.Id, Result = allowed ? "ALLOWED" : "NEED_APPROVAL" }));
    }

    [HttpPost("{id:guid}/access")]
    public async Task<IActionResult> GrantOrExtend(Guid id, GrantRequest request, CancellationToken ct)
    {
        var task = await FindManagedTask(id, ct);
        if (task == null) return Forbid();
        if (task.Status == CollaborationTaskStatus.Completed) return Conflict("Completed tasks cannot receive new access.");
        var expires = request.ExpiresAt?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(configuration.GetValue<int?>("AccessControl:TemporaryAccessDays") ?? 3);
        if (expires <= DateTime.UtcNow || string.IsNullOrWhiteSpace(request.Reason)) return BadRequest("A future expiry and reason are required.");
        var now = DateTime.UtcNow;
        var old = await db.TemporaryAccessGrants.Where(g => g.TaskId == id && g.RevokedAt == null).ToListAsync(ct);
        foreach (var grant in old) grant.RevokedAt = now;
        db.TemporaryAccessGrants.Add(new TemporaryAccessGrant { WorkspaceId = scope.WorkspaceId, TaskId = task.Id, UserId = task.AssigneeId,
            GrantedBy = scope.UserId, GrantedAt = now, ExpiresAt = expires, Reason = request.Reason, CanEdit = request.CanEdit });
        task.Status = CollaborationTaskStatus.Pending; task.BlockedReason = null; task.UpdatedAt = now;
        await access.RecordParticipationAsync(task, scope.UserId, ct);
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { task.Id, ExpiresAt = expires }));
    }

    [HttpDelete("{id:guid}/access")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var task = await FindManagedTask(id, ct);
        if (task == null) return Forbid();
        var grants = await db.TemporaryAccessGrants.Where(g => g.TaskId == id && g.RevokedAt == null).ToListAsync(ct);
        foreach (var grant in grants) grant.RevokedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await access.ExpireAsync(DateTime.UtcNow, scope.WorkspaceId, ct);
        return Ok(GenericResponse<bool>.CreateSuccess(true));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, StatusRequest request, CancellationToken ct)
    {
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer || !Enum.IsDefined(request.Status) || request.Status == CollaborationTaskStatus.Blocked) return Forbid();
        await access.ExpireAsync(DateTime.UtcNow, scope.WorkspaceId, ct);
        var task = await db.CollaborationTasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (task == null) return NotFound();
        if (task.Status == CollaborationTaskStatus.Blocked) return StatusCode(403, GenericResponse<object>.CreateError("Task access has expired or been revoked.", System.Net.HttpStatusCode.Forbidden, task.BlockedReason));
        if (task.Status == CollaborationTaskStatus.Completed) return Conflict("Task is already completed.");
        task.Status = request.Status; task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<bool>.CreateSuccess(true));
    }

    [HttpPost("{id:guid}/reassign")]
    public async Task<IActionResult> Reassign(Guid id, AssignRequest request, CancellationToken ct)
    {
        var task = await FindManagedTask(id, ct);
        if (task == null) return Forbid();
        await authorization.EnsureAsync(scope.WorkspaceId, task.ContentId, ContentAction.Reassign, request.IntegrationId, ct);
        if (request.ContentId != task.ContentId || !await ValidTarget(request, ct)) return BadRequest("Invalid reassignment.");
        if (!await access.HasTeamAccessAsync(scope.WorkspaceId, request.TeamId, request.AssigneeId, request.ContentId, request.IntegrationId, ct))
            return Ok(GenericResponse<object>.CreateSuccess(new { Result = "ADDITIONAL_ACCESS_REQUIRED" }));
        foreach (var grant in await db.TemporaryAccessGrants.Where(g => g.TaskId == id && g.RevokedAt == null).ToListAsync(ct)) grant.RevokedAt = DateTime.UtcNow;
        task.TeamId = request.TeamId; task.AssigneeId = request.AssigneeId; task.IntegrationId = request.IntegrationId;
        task.Status = CollaborationTaskStatus.Pending; task.BlockedReason = null; task.UpdatedAt = DateTime.UtcNow;
        await access.RecordParticipationAsync(task, scope.UserId, ct);
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { Result = "ALLOWED" }));
    }

    private async Task<CollaborationTask?> FindManagedTask(Guid id, CancellationToken ct)
    {
        if (!scope.CanViewAggregate) return null;
        var task = await db.CollaborationTasks.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id && t.WorkspaceId == scope.WorkspaceId, ct);
        if (task == null || !await db.Contents.AnyAsync(c => c.Id == task.ContentId, ct) ||
            !scope.IsOwner && task.IntegrationId.HasValue && !scope.IntegrationIds.Contains(task.IntegrationId.Value)) return null;
        return scope.IsOwner || scope.TeamIds.Contains(task.TeamId) ? task : null;
    }

    private async Task<bool> ValidTarget(AssignRequest request, CancellationToken ct)
    {
        if (!await db.TeamMembers.AnyAsync(m => m.TeamId == request.TeamId && m.UserId == request.AssigneeId && m.IsActive && m.Team.WorkspaceId == scope.WorkspaceId && !m.Team.IsDeleted, ct)) return false;
        if (!await db.WorkspaceMembers.AnyAsync(m => m.UserId == request.AssigneeId && m.WorkspaceId == scope.WorkspaceId && m.IsActive && m.Role != WorkspaceMemberRoleEnum.Viewer, ct)) return false;
        return !request.IntegrationId.HasValue || await db.SocialIntegrations.AnyAsync(i => i.Id == request.IntegrationId &&
            db.Contents.Any(c => c.Id == request.ContentId && c.BrandId == i.BrandId), ct);
    }
}
