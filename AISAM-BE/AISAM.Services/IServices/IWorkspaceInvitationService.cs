using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices;

public interface IWorkspaceInvitationService
{
    Task<GenericResponse<WorkspaceInvitationResponseDto>> InviteAsync(
        Guid workspaceId,
        Guid inviterUserId,
        CreateWorkspaceInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task<GenericResponse<AcceptWorkspaceInvitationResponseDto>> AcceptAsync(
        Guid userId,
        AcceptWorkspaceInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task<GenericResponse<IReadOnlyList<WorkspaceInvitationResponseDto>>> GetPendingByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
}
