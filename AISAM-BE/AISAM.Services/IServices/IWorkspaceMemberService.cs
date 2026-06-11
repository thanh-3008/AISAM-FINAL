using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices;

public interface IWorkspaceMemberService
{
    Task<GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>> GetMembersAsync(
        Guid workspaceId,
        Guid actorUserId,
        CancellationToken cancellationToken = default);

    Task<GenericResponse<WorkspaceMemberResponseDto>> UpdateRoleAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid memberId,
        UpdateWorkspaceMemberRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<GenericResponse<object>> RemoveAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid memberId,
        CancellationToken cancellationToken = default);
}
