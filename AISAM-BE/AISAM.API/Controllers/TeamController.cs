using AISAM.API.Utils;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController, Authorize, Route("api/teams")]
public sealed class TeamController(TeamService teamService) : ControllerBase
{
    private Guid Actor => UserClaimsHelper.GetUserIdOrThrow(User);
    private Guid Workspace => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);

    // ── List Teams ──────────────────────────────────────────────────────

    /// <summary>
    /// GET /api/teams/manage — List all teams in the workspace with member/brand counts.
    /// Uses /manage path to avoid conflict with existing GET /api/teams in ResourceAssignmentsController.
    /// </summary>
    [HttpGet("manage")]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        try
        {
            var (items, totalCount) = await teamService.ListAsync(Actor, Workspace, page, pageSize, ct);
            return Ok(new { success = true, statusCode = 200, data = new { items, totalCount } });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
    }

    // ── Get Team Detail ─────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        try
        {
            var team = await teamService.GetByIdAsync(Actor, Workspace, id, ct);
            return team is null
                ? NotFound(new { success = false, message = "Team not found." })
                : Ok(new { success = true, statusCode = 200, data = team });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
    }

    // ── Create Team ─────────────────────────────────────────────────────

    public sealed record CreateTeamBody(string Name, string? Description, List<TeamService.TeamMemberInput>? Members);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeamBody body, CancellationToken ct = default)
    {
        try
        {
            var request = new TeamService.CreateTeamRequest(body.Name, body.Description, body.Members);
            var team = await teamService.CreateAsync(Actor, Workspace, request, ct);
            return CreatedAtAction(nameof(GetById), new { id = team.Id }, new { success = true, statusCode = 201, data = team });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ── Update Team ─────────────────────────────────────────────────────

    public sealed record UpdateTeamBody(string Name, string? Description);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamBody body, CancellationToken ct = default)
    {
        try
        {
            var request = new TeamService.UpdateTeamRequest(body.Name, body.Description);
            var team = await teamService.UpdateAsync(Actor, Workspace, id, request, ct);
            return Ok(new { success = true, statusCode = 200, data = team });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Team not found." });
        }
    }

    // ── Delete Team ─────────────────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        try
        {
            await teamService.DeleteAsync(Actor, Workspace, id, ct);
            return Ok(new { success = true, statusCode = 200, message = "Team deleted." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Team not found." });
        }
    }

    // ── Add Team Member ─────────────────────────────────────────────────

    public sealed record AddMemberBody(Guid UserId, string? Role);

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberBody body, CancellationToken ct = default)
    {
        try
        {
            var member = await teamService.AddMemberAsync(Actor, Workspace, id, body.UserId, body.Role ?? "", ct);
            return Ok(new { success = true, statusCode = 200, data = member });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Team or member not found." });
        }
    }

    // ── Remove Team Member ──────────────────────────────────────────────

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken ct = default)
    {
        try
        {
            await teamService.RemoveMemberAsync(Actor, Workspace, id, userId, ct);
            return Ok(new { success = true, statusCode = 200, message = "Member removed." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Team member not found." });
        }
    }

    // ── Update Team Member Role ─────────────────────────────────────────

    public sealed record UpdateMemberRoleBody(string Role);

    [HttpPut("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid userId,
        [FromBody] UpdateMemberRoleBody body, CancellationToken ct = default)
    {
        try
        {
            var member = await teamService.UpdateMemberRoleAsync(Actor, Workspace, id, userId, body.Role, ct);
            return Ok(new { success = true, statusCode = 200, data = member });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Team member not found." });
        }
    }
}
