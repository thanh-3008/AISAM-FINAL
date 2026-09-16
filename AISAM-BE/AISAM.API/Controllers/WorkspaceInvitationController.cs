using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ServiceFilter(typeof(AISAM.API.Filters.WorkspaceHrV2ConcurrencyFilter))]
[ApiController]
[Route("api/workspace-invitations")]
[Authorize]
public sealed class WorkspaceInvitationController : ControllerBase
{
    private readonly AISAM.Services.Access.IAccessControlService? _access;
    private readonly IWorkspaceInvitationService _workspaceInvitationService;

    public WorkspaceInvitationController(IWorkspaceInvitationService workspaceInvitationService, AISAM.Services.Access.IAccessControlService? access=null)
    {
        _workspaceInvitationService = workspaceInvitationService; _access=access;
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
        if(_access is AISAM.Services.Access.RbacV2AccessAdapter &&
            (!HttpContext.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey,out var value) ||
             value is not AISAM.Data.Model.WorkspaceMember member || !member.IsActive ||
             member.WorkspaceRoleV2 is not (AISAM.Data.Enumeration.WorkspaceRoleV2.Owner or AISAM.Data.Enumeration.WorkspaceRoleV2.WorkspaceManager)))
            return StatusCode(403,GenericResponse<IReadOnlyList<WorkspaceInvitationResponseDto>>.CreateError("Not allowed to view pending invitations.",System.Net.HttpStatusCode.Forbidden));
        var result = await _workspaceInvitationService.GetPendingByWorkspaceAsync(workspaceId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{invitationId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> Revoke(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceInvitationService.RevokeAsync(workspaceId, userId, invitationId, cancellationToken);
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

    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<GenericResponse<WorkspaceInvitationResponseDto>>> Validate(
        string token,
        CancellationToken cancellationToken = default)
    {
        var result = await _workspaceInvitationService.ValidateAsync(token, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
