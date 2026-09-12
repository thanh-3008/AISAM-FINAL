using AISAM.API.Utils;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AISAM.API.Controllers;

[ApiController, Authorize, Route("api/permissions")]
public sealed class ResourcePermissionsController(IAccessControlService access, AisamContext? db = null) : ControllerBase
{
    public sealed record CheckItem(AccessResourceKind Kind, Guid ResourceId, ResourcePermission Permission, Guid? ChannelId = null);

    [HttpGet("context")]
    public async Task<IActionResult> Context(CancellationToken ct)
    {
        if (db is null) throw new InvalidOperationException("Database context is required.");
        var workspace = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var actor = UserClaimsHelper.GetUserIdOrThrow(User);
        var member = (AISAM.Data.Model.WorkspaceMember)HttpContext.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey]!;
        var grants = await (from grant in db.TeamChannelAccesses.AsNoTracking()
            join assignment in db.TeamBrands.AsNoTracking() on grant.TeamBrandId equals assignment.Id
            where assignment.IsActive && db.PermissionTeamIds.Contains(assignment.TeamId)
            orderby grant.Id
            select new { grant.Id, grant.CanView, grant.CanPublish, grant.CanManage }).ToListAsync(ct);
        var delegated = await db.TeamMembers.AsNoTracking().Where(m=>m.UserId==actor && m.IsActive)
            .OrderBy(m=>m.TeamId).Select(m=>new {m.TeamId,m.Permissions}).ToListAsync(ct);
        var value = JsonSerializer.Serialize(new {
            actor, workspace, member.Role, member.IsActive, member.Workspace.Status,
            brands=db.PermissionBrandIds.Order(), channels=db.PermissionChannelIds.Order(),
            teams=db.PermissionTeamIds.Order(), review=db.PermissionReviewBrandIds.Order(),
            viewAll=db.PermissionViewAllBrandIds.Order(), grants, delegated
        });
        return Ok(new { success=true, data=new { revision=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))) } });
    }

    // Only decisions for the authenticated caller. No resource metadata or actor override.
    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] CheckItem[] items, CancellationToken ct)
    {
        if (items.Length > 100) return BadRequest(new { message = "At most 100 permission checks per request." });
        var actor = UserClaimsHelper.GetUserIdOrThrow(User);
        var workspace = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var results = new List<bool>(items.Length);
        foreach (var item in items)
            results.Add((await access.CheckAsync(new(actor, workspace, item.Kind, item.ResourceId, item.Permission, item.ChannelId), ct)).Allowed);
        return Ok(new { success = true, data = results });
    }
}
