using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class WorkspaceMemberService : IWorkspaceMemberService
{
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;

    public WorkspaceMemberService(IWorkspaceMemberRepository workspaceMemberRepository)
    {
        _workspaceMemberRepository = workspaceMemberRepository;
    }

    public async Task<GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>> GetMembersAsync(
        Guid workspaceId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (!await _workspaceMemberRepository.ExistsAsync(workspaceId, actorUserId, cancellationToken))
        {
            return GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>.CreateError(
                "You are not a member of this workspace.",
                HttpStatusCode.Forbidden);
        }

        var members = await _workspaceMemberRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        return GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>.CreateSuccess(
            members.Select(Map).ToList(),
            "Workspace members retrieved successfully.");
    }

    public async Task<GenericResponse<WorkspaceMemberResponseDto>> UpdateRoleAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorizationError = await RequireOwnerAsync(workspaceId, actorUserId, cancellationToken);
        if (authorizationError != null)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                authorizationError.Value.Message,
                authorizationError.Value.Status);
        }

        if (!Enum.IsDefined(request.Role) || request.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Use ownership transfer to assign the workspace owner.");
        }

        var member = await _workspaceMemberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member == null || member.WorkspaceId != workspaceId)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError("Workspace member not found.", HttpStatusCode.NotFound);
        }

        if (member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Workspace owner role cannot be changed. Transfer ownership first.");
        }

        member.Role = request.Role;
        await _workspaceMemberRepository.UpdateAsync(member, cancellationToken);
        return GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(
            Map(member),
            "Workspace member role updated successfully.");
    }

    public async Task<GenericResponse<object>> RemoveAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var authorizationError = await RequireOwnerAsync(workspaceId, actorUserId, cancellationToken);
        if (authorizationError != null)
        {
            return GenericResponse<object>.CreateError(
                authorizationError.Value.Message,
                authorizationError.Value.Status);
        }

        var member = await _workspaceMemberRepository.GetByIdAsync(memberId, cancellationToken);
        if (member == null || member.WorkspaceId != workspaceId)
        {
            return GenericResponse<object>.CreateError("Workspace member not found.", HttpStatusCode.NotFound);
        }

        if (member.Role == WorkspaceMemberRoleEnum.Owner)
        {
            return GenericResponse<object>.CreateError(
                "Workspace owner cannot be removed. Transfer ownership first.");
        }

        await _workspaceMemberRepository.RemoveAsync(memberId, cancellationToken);
        return GenericResponse<object>.CreateSuccess(null, "Workspace member removed successfully.");
    }

    public async Task<GenericResponse<WorkspaceMemberResponseDto>> TransferOwnershipAsync(
        Guid workspaceId,
        Guid actorUserId,
        TransferWorkspaceOwnershipRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorizationError = await RequireOwnerAsync(workspaceId, actorUserId, cancellationToken);
        if (authorizationError != null)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                authorizationError.Value.Message,
                authorizationError.Value.Status);
        }

        var target = await _workspaceMemberRepository.GetByIdAsync(request.TargetMemberId, cancellationToken);
        if (target == null || target.WorkspaceId != workspaceId)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Workspace member not found.",
                HttpStatusCode.NotFound);
        }

        if (target.Role != WorkspaceMemberRoleEnum.Manager)
        {
            return GenericResponse<WorkspaceMemberResponseDto>.CreateError(
                "Ownership can only be transferred to an active workspace manager.");
        }

        var newOwner = await _workspaceMemberRepository.TransferOwnershipAsync(
            workspaceId,
            actorUserId,
            request.TargetMemberId,
            cancellationToken);

        return GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(
            Map(newOwner),
            "Workspace ownership transferred successfully.");
    }

    private async Task<(string Message, HttpStatusCode Status)?> RequireOwnerAsync(
        Guid workspaceId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var actor = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(
            workspaceId,
            actorUserId,
            cancellationToken);

        if (actor?.Role != WorkspaceMemberRoleEnum.Owner)
        {
            return ("Only the workspace owner can manage members.", HttpStatusCode.Forbidden);
        }

        return actor.Workspace.Status == WorkspaceStatusEnum.Active
            ? null
            : ("Workspace must be active to manage members.", HttpStatusCode.Forbidden);
    }

    private static WorkspaceMemberResponseDto Map(WorkspaceMember member)
    {
        return new WorkspaceMemberResponseDto
        {
            Id = member.Id,
            UserId = member.UserId,
            Email = member.User.Email,
            FullName = member.User.FullName,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }
}
