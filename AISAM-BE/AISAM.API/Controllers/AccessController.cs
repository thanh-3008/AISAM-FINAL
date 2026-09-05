using AISAM.Common;
using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AISAM.API.Controllers;

[ApiController, Authorize, Route("api/access")]
public sealed class AccessController(AisamContext db, AccessScope scope) : ControllerBase
{
    [HttpGet("content/{contentId:guid}/actions")]
    public async Task<IActionResult> ContentActions(Guid contentId, [FromServices] ContentAuthorizationService authorization,
        [FromQuery] Guid? channelId = null, CancellationToken ct = default)
    {
        var actions = await authorization.GetActionsAsync(scope.WorkspaceId, contentId, channelId, ct);
        return Ok(GenericResponse<object>.CreateSuccess(actions));
    }

    [HttpGet("context")]
    public IActionResult Context() => Ok(GenericResponse<object>.CreateSuccess(new
    {
        scope.WorkspaceId, scope.UserId, Role = scope.Role.ToString(), scope.TeamIds,
        scope.Version,
        CanViewAnalytics = scope.CanViewAggregate, CanViewOwnAnalytics = scope.Role != WorkspaceMemberRoleEnum.Viewer,
        CanManageTeams = scope.IsOwner, CanManageTasks = scope.CanViewAggregate,
        CanCreateContent = scope.Role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager or WorkspaceMemberRoleEnum.ContentCreator,
        CanReviewContent = scope.CanViewAggregate, CanPublish = scope.CanViewAggregate
    }));

    [HttpGet("me/analytics")]
    public async Task<IActionResult> OwnAnalytics(CancellationToken ct)
    {
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer) return Forbid();
        var contentIds = await db.Contents.Where(c => c.PrimaryCreatorId == scope.UserId || c.Participations.Any(p => p.UserId == scope.UserId && p.WorkspaceId == scope.WorkspaceId)).Select(c => c.Id).ToListAsync(ct);
        var reports = await db.ContentAnalyticsReports().Where(r => !r.IsDeleted && r.Post != null && contentIds.Contains(r.Post.ContentId))
            .Select(r => new { r.Impressions, r.Engagement, r.Clicks, r.EstimatedRevenue }).ToListAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { ContentCount = contentIds.Count, Impressions = reports.Sum(r => r.Impressions), Engagement = reports.Sum(r => r.Engagement), Clicks = reports.Sum(r => r.Clicks) }));
    }

    [HttpGet("content/{contentId:guid}/analytics")]
    public async Task<IActionResult> ContentAnalytics(Guid contentId, CancellationToken ct)
    {
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer) return Forbid();
        if (!await db.Contents.AnyAsync(c => c.Id == contentId, ct)) return NotFound();
        var reports = await db.ContentAnalyticsReports().Where(r => !r.IsDeleted && r.Post != null && r.Post.ContentId == contentId)
            .Select(r => new { r.ReportDate, r.Impressions, r.Engagement, r.Clicks, r.Reach }).ToListAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(reports));
    }

    [HttpGet("creator-history/{userId:guid}")]
    public async Task<IActionResult> History(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (scope.Role == WorkspaceMemberRoleEnum.Viewer || scope.IsCreator && userId != scope.UserId ||
            scope.Role == WorkspaceMemberRoleEnum.Manager && !scope.MemberIds.Contains(userId)) return Forbid();
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Contents.AsNoTracking().Where(c => c.PrimaryCreatorId == userId || c.Participations.Any(p => p.UserId == userId && p.WorkspaceId == scope.WorkspaceId));
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new { c.Id, c.Title, c.PrimaryCreatorId, c.Status, c.CreatedAt }).ToListAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { Data = items, TotalCount = total, Page = page, PageSize = pageSize }));
    }
}
