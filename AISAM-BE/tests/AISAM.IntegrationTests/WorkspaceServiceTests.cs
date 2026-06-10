using AISAM.Common.Dtos.Request;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.IntegrationTests;

public class WorkspaceServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesWorkspaceWithExactlyOneOwner()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(user.Id, new CreateWorkspaceRequest
        {
            Name = "Personal workspace",
            WorkspaceType = WorkspaceTypeEnum.Personal
        });

        Assert.True(result.Success);
        var workspace = await context.Workspaces.Include(item => item.Members).SingleAsync();
        var owner = Assert.Single(workspace.Members);
        Assert.Equal(user.Id, owner.UserId);
        Assert.Equal(WorkspaceMemberRoleEnum.Owner, owner.Role);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAllWorkspacesCurrentUserParticipatesIn()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var service = CreateService(context);

        await service.CreateAsync(user.Id, new CreateWorkspaceRequest { Name = "One", WorkspaceType = WorkspaceTypeEnum.Personal });
        await service.CreateAsync(user.Id, new CreateWorkspaceRequest { Name = "Two", WorkspaceType = WorkspaceTypeEnum.Business });

        var result = await service.GetByUserIdAsync(user.Id);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        Assert.All(result.Data, workspace => Assert.Equal(WorkspaceMemberRoleEnum.Owner, workspace.CurrentUserRole));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundForNonMember()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var nonMember = AddUser(context);
        var service = CreateService(context);
        var created = await service.CreateAsync(owner.Id, new CreateWorkspaceRequest
        {
            Name = "Private",
            WorkspaceType = WorkspaceTypeEnum.Business
        });

        var result = await service.GetByIdAsync(created.Data!.Id, nonMember.Id);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsForbiddenForNonOwnerMember()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var manager = AddUser(context);
        var service = CreateService(context);
        var created = await service.CreateAsync(owner.Id, new CreateWorkspaceRequest
        {
            Name = "Business",
            WorkspaceType = WorkspaceTypeEnum.Business
        });
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = created.Data!.Id,
            UserId = manager.Id,
            Role = WorkspaceMemberRoleEnum.Manager
        });
        await context.SaveChangesAsync();

        var result = await service.UpdateAsync(created.Data.Id, manager.Id, new UpdateWorkspaceRequest { Name = "Changed" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_AllowsOwnerToRenameWorkspace()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var service = CreateService(context);
        var created = await service.CreateAsync(owner.Id, new CreateWorkspaceRequest
        {
            Name = "Before",
            WorkspaceType = WorkspaceTypeEnum.Business
        });

        var result = await service.UpdateAsync(
            created.Data!.Id,
            owner.Id,
            new UpdateWorkspaceRequest { Name = "After" });

        Assert.True(result.Success);
        Assert.Equal("After", result.Data!.Name);
        Assert.Equal("After", (await context.Workspaces.SingleAsync()).Name);
    }

    private static WorkspaceService CreateService(AisamContext context)
    {
        return new WorkspaceService(new WorkspaceRepository(context), new UserRepository(context));
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static User AddUser(AisamContext context)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }
}
