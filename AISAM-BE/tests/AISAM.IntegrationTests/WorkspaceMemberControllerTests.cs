using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class WorkspaceMemberControllerTests
{
    [Fact]
    public async Task Endpoints_UseActiveWorkspaceAndAuthenticatedUser()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var service = new FakeWorkspaceMemberService();
        var controller = CreateController(service, workspaceId, userId);

        await controller.GetMembers();
        Assert.Equal(workspaceId, service.LastWorkspaceId);
        Assert.Equal(userId, service.LastActorUserId);

        await controller.UpdateRole(memberId, new UpdateWorkspaceMemberRoleRequest { Role = WorkspaceMemberRoleEnum.Viewer });
        Assert.Equal(memberId, service.LastMemberId);

        await controller.UpdateQuota(memberId, new UpdateWorkspaceMemberQuotaRequest { QuotaMode = MemberQuotaModeEnum.SharedPool });
        Assert.Equal(memberId, service.LastMemberId);

        await controller.Remove(memberId);
        Assert.Equal(memberId, service.LastMemberId);

        await controller.TransferOwnership(new TransferWorkspaceOwnershipRequest { TargetMemberId = memberId });
        Assert.Equal(memberId, service.LastMemberId);
    }

    private static WorkspaceMemberController CreateController(
        IWorkspaceMemberService service,
        Guid workspaceId,
        Guid userId)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "Test"))
        };
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;
        return new WorkspaceMemberController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeWorkspaceMemberService : IWorkspaceMemberService
    {
        public Guid LastWorkspaceId { get; private set; }
        public Guid LastActorUserId { get; private set; }
        public Guid LastMemberId { get; private set; }

        public Task<GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>> GetMembersAsync(Guid workspaceId, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            Capture(workspaceId, actorUserId);
            return Task.FromResult(GenericResponse<IReadOnlyList<WorkspaceMemberResponseDto>>.CreateSuccess([]));
        }

        public Task<GenericResponse<WorkspaceMemberResponseDto>> UpdateRoleAsync(Guid workspaceId, Guid actorUserId, Guid memberId, UpdateWorkspaceMemberRoleRequest request, CancellationToken cancellationToken = default)
        {
            Capture(workspaceId, actorUserId, memberId);
            return Task.FromResult(GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(new WorkspaceMemberResponseDto()));
        }

        public Task<GenericResponse<WorkspaceMemberResponseDto>> UpdateQuotaAsync(Guid workspaceId, Guid actorUserId, Guid memberId, UpdateWorkspaceMemberQuotaRequest request, CancellationToken cancellationToken = default)
        {
            Capture(workspaceId, actorUserId, memberId);
            return Task.FromResult(GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(new WorkspaceMemberResponseDto()));
        }

        public Task<GenericResponse<object>> RemoveAsync(Guid workspaceId, Guid actorUserId, Guid memberId, CancellationToken cancellationToken = default)
        {
            Capture(workspaceId, actorUserId, memberId);
            return Task.FromResult(GenericResponse<object>.CreateSuccess(null));
        }

        public Task<GenericResponse<WorkspaceMemberResponseDto>> TransferOwnershipAsync(Guid workspaceId, Guid actorUserId, TransferWorkspaceOwnershipRequest request, CancellationToken cancellationToken = default)
        {
            Capture(workspaceId, actorUserId, request.TargetMemberId);
            return Task.FromResult(GenericResponse<WorkspaceMemberResponseDto>.CreateSuccess(new WorkspaceMemberResponseDto()));
        }

        private void Capture(Guid workspaceId, Guid actorUserId, Guid? memberId = null)
        {
            LastWorkspaceId = workspaceId;
            LastActorUserId = actorUserId;
            LastMemberId = memberId ?? LastMemberId;
        }
    }
}




