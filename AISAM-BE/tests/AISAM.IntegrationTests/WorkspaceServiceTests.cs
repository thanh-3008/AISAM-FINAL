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
        Assert.Equal(1, workspace.MemberLimit);
        var wallet = await context.CreditWallets.SingleAsync();
        Assert.Equal(workspace.Id, wallet.WorkspaceId);
        Assert.Equal(50, wallet.Balance);
        Assert.Single(context.Subscriptions);
        Assert.Single(context.CreditUsageRecords);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAllWorkspacesCurrentUserParticipatesIn()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var service = CreateService(context);

        await service.CreateAsync(user.Id, new CreateWorkspaceRequest { Name = "One", WorkspaceType = WorkspaceTypeEnum.Personal });
        AddBusinessWorkspace(context, user, "Two");

        var result = await service.GetByUserIdAsync(user.Id);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        Assert.All(result.Data, workspace => Assert.Equal(WorkspaceMemberRoleEnum.Owner, workspace.CurrentUserRole));
        Assert.Contains(result.Data, workspace => workspace.MemberLimit == 1);
    }

    [Fact]
    public async Task CreateAsync_RejectsBusinessWorkspaceWithoutSuccessfulPayment()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(user.Id, new CreateWorkspaceRequest
        {
            Name = "Unpaid Business",
            WorkspaceType = WorkspaceTypeEnum.Business
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal("BUSINESS_WORKSPACE_PAYMENT_REQUIRED", result.Error?.ErrorCode);
        Assert.Empty(context.Workspaces);
    }

    [Fact]
    public async Task CreateAsync_RejectsSecondPersonalWorkspaceForSameUser()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var service = CreateService(context);

        await service.CreateAsync(user.Id, new CreateWorkspaceRequest { Name = "Personal", WorkspaceType = WorkspaceTypeEnum.Personal });

        var result = await service.CreateAsync(user.Id, new CreateWorkspaceRequest { Name = "Another Personal", WorkspaceType = WorkspaceTypeEnum.Personal });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal("PERSONAL_WORKSPACE_LIMIT_REACHED", result.Error?.ErrorCode);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundForNonMember()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var nonMember = AddUser(context);
        var service = CreateService(context);
        var created = AddBusinessWorkspace(context, owner, "Private");

        var result = await service.GetByIdAsync(created.Id, nonMember.Id);

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
        var created = AddBusinessWorkspace(context, owner, "Business");
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = created.Id,
            UserId = manager.Id,
            Role = WorkspaceMemberRoleEnum.Manager
        });
        await context.SaveChangesAsync();

        var result = await service.UpdateAsync(created.Id, manager.Id, new UpdateWorkspaceRequest { Name = "Changed" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_AllowsOwnerToRenameWorkspace()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var service = CreateService(context);
        var created = AddBusinessWorkspace(context, owner, "Before");

        var result = await service.UpdateAsync(
            created.Id,
            owner.Id,
            new UpdateWorkspaceRequest { Name = "After" });

        Assert.True(result.Success);
        Assert.Equal("After", result.Data!.Name);
        Assert.Equal("After", (await context.Workspaces.SingleAsync()).Name);
    }

    [Fact]
    public async Task GetByIdAsync_SynchronizesExpiredBusinessWorkspaceLifecycle()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var service = CreateService(context);
        var created = AddBusinessWorkspace(context, owner, "Expired");
        var workspace = await context.Workspaces.SingleAsync();
        workspace.SubscriptionExpiredAt = DateTime.UtcNow.AddDays(-100);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(created.Id, owner.Id);

        Assert.True(result.Success);
        Assert.Equal(WorkspaceStatusEnum.Archived, result.Data!.Status);
        Assert.NotNull(result.Data.ArchivedAt);
    }

    [Fact]
    public async Task UpdateAsync_RejectsExpiredReadOnlyWorkspace()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var service = CreateService(context);
        var created = AddBusinessWorkspace(context, owner, "Expired");
        var workspace = await context.Workspaces.SingleAsync();
        workspace.SubscriptionExpiredAt = DateTime.UtcNow.AddDays(-1);
        await context.SaveChangesAsync();

        var result = await service.UpdateAsync(created.Id, owner.Id, new UpdateWorkspaceRequest { Name = "Blocked" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Equal("WORKSPACE_READ_ONLY", result.Error?.ErrorCode);
    }

    [Fact]
    public async Task AdminSoftDeleteAsync_SoftDeletesEligibleWorkspace()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var admin = AddUser(context, UserRoleEnum.Admin);
        var service = CreateService(context);
        var created = AddBusinessWorkspace(context, owner, "Eligible");
        var workspace = await context.Workspaces.SingleAsync();
        workspace.SubscriptionExpiredAt = DateTime.UtcNow.AddDays(-181);
        await context.SaveChangesAsync();

        var result = await service.AdminSoftDeleteAsync(created.Id, admin.Id);

        Assert.True(result.Success);
        Assert.Equal(WorkspaceStatusEnum.Deleted, workspace.Status);
        Assert.NotNull(workspace.DeletedAt);
        Assert.Null(await new WorkspaceRepository(context).GetByIdAsync(workspace.Id));
        Assert.NotNull(await new WorkspaceRepository(context).GetByIdIncludingDeletedAsync(workspace.Id));
    }

    [Fact]
    public async Task AdminSoftDeleteAsync_RejectsWorkspaceBeforeDeletionEligibility()
    {
        await using var context = CreateContext();
        var owner = AddUser(context);
        var admin = AddUser(context, UserRoleEnum.Admin);
        var service = CreateService(context);
        var created = AddBusinessWorkspace(context, owner, "Archived");
        var workspace = await context.Workspaces.SingleAsync();
        workspace.SubscriptionExpiredAt = DateTime.UtcNow.AddDays(-179);
        await context.SaveChangesAsync();

        var result = await service.AdminSoftDeleteAsync(created.Id, admin.Id);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(WorkspaceStatusEnum.Archived, workspace.Status);
    }

    private static WorkspaceService CreateService(AisamContext context)
    {
        return new WorkspaceService(
            new WorkspaceRepository(context),
            new UserRepository(context));
    }

    private static Workspace AddBusinessWorkspace(AisamContext context, User owner, string name)
    {
        var workspace = new Workspace
        {
            Name = name,
            WorkspaceType = WorkspaceTypeEnum.Business,
            Status = WorkspaceStatusEnum.Active,
            MemberLimit = 10,
            SubscriptionExpiredAt = DateTime.UtcNow.AddDays(30),
            CreditWallet = new CreditWallet { Balance = 15_000 },
            Members =
            [
                new WorkspaceMember
                {
                    UserId = owner.Id,
                    Role = WorkspaceMemberRoleEnum.Owner
                }
            ]
        };
        context.Workspaces.Add(workspace);
        context.SaveChanges();
        return workspace;
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static User AddUser(AisamContext context, UserRoleEnum role = UserRoleEnum.User)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            Role = role
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }
}
