using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/workspace-invitations")]
[Authorize]
public sealed class WorkspaceInvitationController : ControllerBase
{
    private readonly IWorkspaceInvitationService _workspaceInvitationService;

    public WorkspaceInvitationController(IWorkspaceInvitationService workspaceInvitationService)
    {
        _workspaceInvitationService = workspaceInvitationService;
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<WorkspaceInvitationResponseDto>>> Invite(
        [FromBody] CreateWorkspaceInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceInvitationService.InviteAsync(workspaceId, userId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<WorkspaceInvitationResponseDto>>>> GetPending(
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var result = await _workspaceInvitationService.GetPendingByWorkspaceAsync(workspaceId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("accept")]
    public async Task<ActionResult<GenericResponse<AcceptWorkspaceInvitationResponseDto>>> Accept(
        [FromBody] AcceptWorkspaceInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceInvitationService.AcceptAsync(userId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
