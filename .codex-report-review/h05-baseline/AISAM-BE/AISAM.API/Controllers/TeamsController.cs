using AISAM.Common;
using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AISAM.API.Controllers;

[ApiController, Authorize, Route("api/teams")]
public sealed class TeamsController(AisamContext db, AccessScope scope) : ControllerBase
{
    public sealed record TeamRequest([Required, MaxLength(255)] string Name, string? Description, Guid[] BrandIds, Guid[] MemberIds);
    public sealed record BrandAccessRequest(ChannelAccessMode Mode, Guid[] ChannelIds);
    public sealed record TransferRequest(Guid TargetTeamId);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var teams = await db.Teams.AsNoTracking().Where(t => t.WorkspaceId == scope.WorkspaceId && !t.IsDeleted &&
            (scope.IsOwner || scope.TeamIds.Contains(t.Id))).Select(t => new
        {
            t.Id, t.Name, t.Description, t.CreatedAt, t.UpdatedAt,
            BrandIds = t.TeamBrands.Where(b => b.IsActive).Select(b => b.BrandId).ToArray(),
            MemberIds = t.TeamMembers.Where(m => m.IsActive).Select(m => m.UserId).ToArray(),
            BrandCount = t.TeamBrands.Count(b => b.IsActive)
        }).ToListAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(teams));
    }

    [HttpPost]
    public async Task<IActionResult> Create(TeamRequest request, CancellationToken ct)
    {
        if (!scope.IsOwner) return Forbid();
        if (!await Validate(request, ct)) return BadRequest("Invalid workspace members or brands.");
        var team = new Team { WorkspaceId = scope.WorkspaceId, Name = request.Name.Trim(), Description = request.Description };
        await SetLinks(team, request, ct);
        db.Teams.Add(team);
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { team.Id, team.Name, team.Description, request.BrandIds, request.MemberIds }));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TeamRequest request, CancellationToken ct)
    {
        if (!scope.IsOwner) return Forbid();
        var team = await db.Teams.Include(t => t.TeamMembers).Include(t => t.TeamBrands).ThenInclude(b => b.Channels)
            .FirstOrDefaultAsync(t => t.Id == id && t.WorkspaceId == scope.WorkspaceId && !t.IsDeleted, ct);
        if (team == null) return NotFound();
        if (!await Validate(request, ct)) return BadRequest("Invalid workspace members or brands.");
        team.Name = request.Name.Trim(); team.Description = request.Description; team.UpdatedAt = DateTime.UtcNow;
        await SetLinks(team, request, ct);
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<object>.CreateSuccess(new { team.Id, team.Name, team.Description, request.BrandIds, request.MemberIds }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!scope.IsOwner) return Forbid();
        var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == id && t.WorkspaceId == scope.WorkspaceId && !t.IsDeleted, ct);
        if (team == null) return NotFound();
        if (await db.Teams.CountAsync(t => t.WorkspaceId == scope.WorkspaceId && !t.IsDeleted, ct) <= 1)
            return Conflict("A workspace must retain at least one team.");
        team.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<bool>.CreateSuccess(true));
    }

    [HttpGet("{teamId:guid}/brands/{brandId:guid}/access")]
    public async Task<IActionResult> GetAccess(Guid teamId, Guid brandId, CancellationToken ct)
    {
        if (!scope.IsOwner && (scope.Role != WorkspaceMemberRoleEnum.Manager || !scope.TeamIds.Contains(teamId))) return Forbid();
        var access = await db.TeamBrands.Include(b => b.Channels).FirstOrDefaultAsync(b => b.TeamId == teamId && b.BrandId == brandId &&
            b.Team.WorkspaceId == scope.WorkspaceId && !b.Team.IsDeleted && b.IsActive, ct);
        return access == null ? NotFound() : Ok(GenericResponse<object>.CreateSuccess(new { Mode = access.ChannelAccessMode.ToString().ToUpperInvariant(), ChannelIds = access.Channels.Select(c => c.IntegrationId) }));
    }

    [HttpPut("{teamId:guid}/brands/{brandId:guid}/access")]
    public async Task<IActionResult> SetAccess(Guid teamId, Guid brandId, BrandAccessRequest request, CancellationToken ct)
    {
        if (!scope.IsOwner) return Forbid();
        if (!Enum.IsDefined(request.Mode)) return BadRequest("Invalid access mode.");
        var access = await db.TeamBrands.Include(b => b.Channels).FirstOrDefaultAsync(b => b.TeamId == teamId && b.BrandId == brandId &&
            b.Team.WorkspaceId == scope.WorkspaceId && !b.Team.IsDeleted && b.IsActive && b.Brand.WorkspaceId == scope.WorkspaceId, ct);
        if (access == null) return NotFound(); // Brand access must already exist.
        var ids = (request.ChannelIds ?? []).Distinct().ToArray();
        if (request.Mode == ChannelAccessMode.All && ids.Length > 0) return BadRequest("ALL mode must not contain specific channels.");
        if (await db.SocialIntegrations.CountAsync(i => ids.Contains(i.Id) && i.BrandId == brandId && !i.IsDeleted, ct) != ids.Length)
            return BadRequest("Channel is not part of the accessible brand.");
        // One SaveChanges transaction replaces the mode and children atomically.
        db.TeamChannelAccesses.RemoveRange(access.Channels.Where(c => !ids.Contains(c.IntegrationId)));
        foreach (var id in ids.Except(access.Channels.Select(c => c.IntegrationId)))
            access.Channels.Add(new TeamChannelAccess { TeamBrandId = access.Id, IntegrationId = id });
        access.ChannelAccessMode = request.Mode;
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<bool>.CreateSuccess(true));
    }

    [HttpPost("{teamId:guid}/members/{userId:guid}/transfer")]
    public async Task<IActionResult> Transfer(Guid teamId, Guid userId, TransferRequest request, CancellationToken ct)
    {
        if (!scope.IsOwner) return Forbid();
        var target = await db.Teams.Include(t => t.TeamMembers).FirstOrDefaultAsync(t => t.Id == request.TargetTeamId && t.WorkspaceId == scope.WorkspaceId && !t.IsDeleted, ct);
        var member = await db.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && m.Team.WorkspaceId == scope.WorkspaceId && m.IsActive, ct);
        if (target == null || member == null || teamId == target.Id) return BadRequest("Invalid team transfer.");
        member.IsActive = false;
        var existing = target.TeamMembers.FirstOrDefault(m => m.UserId == userId);
        if (existing != null) { existing.IsActive = true; existing.Role = member.Role; existing.JoinedAt = DateTime.UtcNow; }
        else target.TeamMembers.Add(new TeamMember { TeamId = target.Id, UserId = userId, Role = member.Role });
        await db.SaveChangesAsync(ct);
        return Ok(GenericResponse<bool>.CreateSuccess(true));
    }

    private async Task<bool> Validate(TeamRequest request, CancellationToken ct)
    {
        var members = (request.MemberIds ?? []).Distinct().ToArray();
        var brands = (request.BrandIds ?? []).Distinct().ToArray();
        return !string.IsNullOrWhiteSpace(request.Name) &&
            await db.WorkspaceMembers.CountAsync(m => m.WorkspaceId == scope.WorkspaceId && m.IsActive && members.Contains(m.UserId), ct) == members.Length &&
            await db.Brands.CountAsync(b => brands.Contains(b.Id) && !b.IsDeleted, ct) == brands.Length;
    }

    private async Task SetLinks(Team team, TeamRequest request, CancellationToken ct)
    {
        var members = await db.WorkspaceMembers.Where(m => m.WorkspaceId == scope.WorkspaceId && m.IsActive && (request.MemberIds ?? Array.Empty<Guid>()).Contains(m.UserId)).ToListAsync(ct);
        foreach (var existing in team.TeamMembers) existing.IsActive = members.Any(m => m.UserId == existing.UserId);
        foreach (var member in members.Where(m => !team.TeamMembers.Any(t => t.UserId == m.UserId)))
            team.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = member.UserId, Role = member.Role.ToString() });
        var brands = (request.BrandIds ?? []).Distinct().ToArray();
        foreach (var existing in team.TeamBrands)
        {
            existing.IsActive = brands.Contains(existing.BrandId);
            if (!existing.IsActive) { db.TeamChannelAccesses.RemoveRange(existing.Channels); existing.ChannelAccessMode = ChannelAccessMode.Specific; }
        }
        foreach (var id in brands.Where(id => !team.TeamBrands.Any(b => b.BrandId == id)))
            team.TeamBrands.Add(new TeamBrand { TeamId = team.Id, BrandId = id, ChannelAccessMode = ChannelAccessMode.Specific });
    }
}
