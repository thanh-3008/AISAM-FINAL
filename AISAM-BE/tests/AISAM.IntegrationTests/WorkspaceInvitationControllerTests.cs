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

public class WorkspaceInvitationControllerTests
{
    [Fact]
    public async Task Invite_UsesActiveWorkspaceAndAuthenticatedUser()
    {
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var service = new FakeWorkspaceInvitationService();
        var controller = CreateController(service, userId, workspaceId);

        await controller.Invite(new CreateWorkspaceInvitationRequest
        {
            Email = "invited@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer
        });

        Assert.Equal(workspaceId, service.LastWorkspaceId);
        Assert.Equal(userId, service.LastUserId);
    }

    [Fact]
    public async Task Accept_UsesAuthenticatedUserWithoutActiveWorkspace()
    {
        var userId = Guid.NewGuid();
        var service = new FakeWorkspaceInvitationService();
        var controller = CreateController(service, userId);

        await controller.Accept(new AcceptWorkspaceInvitationRequest { Token = "token" });

        Assert.Equal(userId, service.LastUserId);
    }

    private static WorkspaceInvitationController CreateController(
        IWorkspaceInvitationService service,
        Guid userId,
        Guid? workspaceId = null)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "Test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        if (workspaceId.HasValue)
        {
            context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId.Value;
        }

        return new WorkspaceInvitationController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeWorkspaceInvitationService : IWorkspaceInvitationService
    {
        public Guid LastWorkspaceId { get; private set; }
        public Guid LastUserId { get; private set; }

        public Task<GenericResponse<WorkspaceInvitationResponseDto>> InviteAsync(
            Guid workspaceId,
            Guid inviterUserId,
            CreateWorkspaceInvitationRequest request,
            CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            LastUserId = inviterUserId;
            return Task.FromResult(GenericResponse<WorkspaceInvitationResponseDto>.CreateSuccess(new WorkspaceInvitationResponseDto()));
        }

        public Task<GenericResponse<WorkspaceInvitationResponseDto>> ValidateAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<WorkspaceInvitationResponseDto>.CreateSuccess(new WorkspaceInvitationResponseDto()));
        }

        public Task<GenericResponse<AcceptWorkspaceInvitationResponseDto>> AcceptAsync(
            Guid userId,
            AcceptWorkspaceInvitationRequest request,
            CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult(GenericResponse<AcceptWorkspaceInvitationResponseDto>.CreateSuccess(new AcceptWorkspaceInvitationResponseDto()));
        }

        public Task<GenericResponse<IReadOnlyList<WorkspaceInvitationResponseDto>>> GetPendingByWorkspaceAsync(
            Guid workspaceId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<IReadOnlyList<WorkspaceInvitationResponseDto>>.CreateSuccess(new List<WorkspaceInvitationResponseDto>()));
        }

        public Task<GenericResponse<bool>> RevokeAsync(
            Guid workspaceId,
            Guid userId,
            Guid invitationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
        }
    }
}
