using AISAM.API.Controllers;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Reflection;

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

    [Fact]
    public async Task AdminSoftDelete_DelegatesToService()
    {
        var userId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var service = new FakeWorkspaceService();
        var controller = CreateController(service, userId, UserRoleEnum.Admin);

        var result = await controller.AdminSoftDelete(workspaceId);

        Assert.Equal(workspaceId, service.LastWorkspaceId);
        Assert.IsType<ObjectResult>(result.Result);
    }

    [Fact]
    public void AdminSoftDelete_RequiresAdminRole()
    {
        var method = typeof(WorkspaceController).GetMethod(nameof(WorkspaceController.AdminSoftDelete));

        var authorizeAttribute = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorizeAttribute);
        Assert.Equal(nameof(UserRoleEnum.Admin), authorizeAttribute.Roles);
    }

    private static WorkspaceController CreateController(IWorkspaceService service, Guid userId, UserRoleEnum? role = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (role.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        return new WorkspaceController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeWorkspaceService : IWorkspaceService
    {
        public Guid LastUserId { get; private set; }
        public Guid LastWorkspaceId { get; private set; }

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

        public Task<GenericResponse<bool>> AdminSoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = id;
            return Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
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
