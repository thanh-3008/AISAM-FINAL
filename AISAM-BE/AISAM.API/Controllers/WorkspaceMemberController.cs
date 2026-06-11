using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/workspace-members")]
[Authorize]
public sealed class WorkspaceMemberController : ControllerBase
{
    private readonly IWorkspaceMemberService _workspaceMemberService;

    public WorkspaceMemberController(IWorkspaceMemberService workspaceMemberService)
    {
        _workspaceMemberService = workspaceMemberService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>>> GetMembers(
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceMemberService.GetMembersAsync(workspaceId, userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{memberId:guid}/role")]
    public async Task<ActionResult<GenericResponse<WorkspaceMemberResponseDto>>> UpdateRole(
        Guid memberId,
        [FromBody] UpdateWorkspaceMemberRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceMemberService.UpdateRoleAsync(workspaceId, userId, memberId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{memberId:guid}")]
    public async Task<ActionResult<GenericResponse<object>>> Remove(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceMemberService.RemoveAsync(workspaceId, userId, memberId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
