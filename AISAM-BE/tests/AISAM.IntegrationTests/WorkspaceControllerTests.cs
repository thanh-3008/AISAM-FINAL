using AISAM.API.Controllers;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class WorkspaceControllerTests
{
    [Fact]
    public async Task Create_UsesAuthenticatedUserAndReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var service = new FakeWorkspaceService();
        var controller = CreateController(service, userId);

        var result = await controller.Create(new CreateWorkspaceRequest
        {
            Name = "Workspace",
            WorkspaceType = WorkspaceTypeEnum.Personal
        });

        Assert.Equal(userId, service.LastUserId);
        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task GetMine_UsesAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var service = new FakeWorkspaceService();
        var controller = CreateController(service, userId);

        await controller.GetMine();

        Assert.Equal(userId, service.LastUserId);
    }

    private static WorkspaceController CreateController(IWorkspaceService service, Guid userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "Test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        return new WorkspaceController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeWorkspaceService : IWorkspaceService
    {
        public Guid LastUserId { get; private set; }

        public Task<GenericResponse<IReadOnlyList<WorkspaceResponseDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult(GenericResponse<IReadOnlyList<WorkspaceResponseDto>>.CreateSuccess([]));
        }

        public Task<GenericResponse<WorkspaceResponseDto>> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult(GenericResponse<WorkspaceResponseDto>.CreateSuccess(CreateResponse(id)));
        }

        public Task<GenericResponse<WorkspaceResponseDto>> CreateAsync(Guid userId, CreateWorkspaceRequest request, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult(GenericResponse<WorkspaceResponseDto>.CreateSuccess(CreateResponse(Guid.NewGuid())));
        }

        public Task<GenericResponse<WorkspaceResponseDto>> UpdateAsync(Guid id, Guid userId, UpdateWorkspaceRequest request, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.FromResult(GenericResponse<WorkspaceResponseDto>.CreateSuccess(CreateResponse(id)));
        }

        private static WorkspaceResponseDto CreateResponse(Guid id)
        {
            return new WorkspaceResponseDto
            {
                Id = id,
                Name = "Workspace",
                WorkspaceType = WorkspaceTypeEnum.Personal,
                Status = WorkspaceStatusEnum.Active,
                CurrentUserRole = WorkspaceMemberRoleEnum.Owner
            };
        }
    }
}
