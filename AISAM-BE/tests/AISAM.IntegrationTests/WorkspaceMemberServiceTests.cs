using AISAM.Common.Dtos.Request;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.IntegrationTests;

public class WorkspaceMemberServiceTests
{
    [Fact]
    public async Task GetMembersAsync_AllowsActiveMember()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var service = CreateService(context);

        var result = await service.GetMembersAsync(fixture.Workspace.Id, fixture.Viewer.UserId);

        Assert.True(result.Success);
        Assert.Equal(3, result.Data!.Count);
    }

    [Fact]
    public async Task UpdateRoleAsync_AllowsOwnerToUpdateNonOwner()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var service = CreateService(context);

        var result = await service.UpdateRoleAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            fixture.Viewer.Id,
            new UpdateWorkspaceMemberRoleRequest { Role = WorkspaceMemberRoleEnum.ContentCreator });

        Assert.True(result.Success);
        Assert.Equal(WorkspaceMemberRoleEnum.ContentCreator, result.Data!.Role);
    }

    [Fact]
    public async Task UpdateRoleAsync_RejectsNonOwnerAndOwnerTarget()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var service = CreateService(context);

        var nonOwnerResult = await service.UpdateRoleAsync(
            fixture.Workspace.Id,
            fixture.Manager.UserId,
            fixture.Viewer.Id,
            new UpdateWorkspaceMemberRoleRequest { Role = WorkspaceMemberRoleEnum.ContentCreator });
        var ownerTargetResult = await service.UpdateRoleAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            fixture.Owner.Id,
            new UpdateWorkspaceMemberRoleRequest { Role = WorkspaceMemberRoleEnum.Manager });

        Assert.Equal((int)HttpStatusCode.Forbidden, nonOwnerResult.StatusCode);
        Assert.Equal((int)HttpStatusCode.BadRequest, ownerTargetResult.StatusCode);
    }

    [Fact]
    public async Task UpdateQuotaAsync_AllowsBusinessProOwnerToAssignMonthlyLimit()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, SubscriptionPlanEnum.Premium);
        var service = CreateService(context);

        var result = await service.UpdateQuotaAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            fixture.Viewer.Id,
            new UpdateWorkspaceMemberQuotaRequest
            {
                QuotaMode = MemberQuotaModeEnum.MonthlyAssignedLimit,
                CreditLimit = 100
            });

        Assert.True(result.Success);
        Assert.Equal(MemberQuotaModeEnum.MonthlyAssignedLimit, result.Data!.QuotaMode);
        Assert.Equal(100, result.Data.CreditLimit);
        Assert.Equal(0, result.Data.CreditUsed);
    }

    [Fact]
    public async Task UpdateQuotaAsync_RejectsAssignedLimitForBusinessPlus()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, SubscriptionPlanEnum.Plus);
        var service = CreateService(context);

        var result = await service.UpdateQuotaAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            fixture.Viewer.Id,
            new UpdateWorkspaceMemberQuotaRequest
            {
                QuotaMode = MemberQuotaModeEnum.LifetimeAssignedLimit,
                CreditLimit = 100
            });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task RemoveAsync_AllowsOwnerToRemoveNonOwner()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var service = CreateService(context);

        var result = await service.RemoveAsync(fixture.Workspace.Id, fixture.Owner.UserId, fixture.Viewer.Id);

        Assert.True(result.Success);
        Assert.False(await context.WorkspaceMembers.Where(member => member.Id == fixture.Viewer.Id).Select(member => member.IsActive).SingleAsync());
    }

    [Fact]
    public async Task RemoveAsync_RejectsNonOwnerAndOwnerTarget()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var service = CreateService(context);

        var nonOwnerResult = await service.RemoveAsync(fixture.Workspace.Id, fixture.Manager.UserId, fixture.Viewer.Id);
        var ownerTargetResult = await service.RemoveAsync(fixture.Workspace.Id, fixture.Owner.UserId, fixture.Owner.Id);

        Assert.Equal((int)HttpStatusCode.Forbidden, nonOwnerResult.StatusCode);
        Assert.Equal((int)HttpStatusCode.BadRequest, ownerTargetResult.StatusCode);
    }

    [Fact]
    public async Task MemberManagement_RejectsLimitedWorkspaceButStillAllowsMemberList()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        fixture.Workspace.Status = WorkspaceStatusEnum.Limited;
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var listResult = await service.GetMembersAsync(fixture.Workspace.Id, fixture.Viewer.UserId);
        var updateResult = await service.UpdateRoleAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            fixture.Viewer.Id,
            new UpdateWorkspaceMemberRoleRequest { Role = WorkspaceMemberRoleEnum.ContentCreator });
        var removeResult = await service.RemoveAsync(fixture.Workspace.Id, fixture.Owner.UserId, fixture.Viewer.Id);

        Assert.True(listResult.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, updateResult.StatusCode);
        Assert.Equal((int)HttpStatusCode.Forbidden, removeResult.StatusCode);
    }

    [Fact]
    public async Task TransferOwnershipAsync_AllowsOwnerToTransferToManager()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var service = CreateService(context);

        var result = await service.TransferOwnershipAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            new TransferWorkspaceOwnershipRequest { TargetMemberId = fixture.Manager.Id });

        Assert.True(result.Success);
        Assert.Equal(WorkspaceMemberRoleEnum.Owner, result.Data!.Role);
        Assert.Equal(WorkspaceMemberRoleEnum.Manager, fixture.Owner.Role);
        Assert.Equal(WorkspaceMemberRoleEnum.Owner, fixture.Manager.Role);

        var formerOwnerManageResult = await service.RemoveAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            fixture.Viewer.Id);
        var newOwnerManageResult = await service.UpdateRoleAsync(
            fixture.Workspace.Id,
            fixture.Manager.UserId,
            fixture.Viewer.Id,
            new UpdateWorkspaceMemberRoleRequest { Role = WorkspaceMemberRoleEnum.ContentCreator });

        Assert.Equal((int)HttpStatusCode.Forbidden, formerOwnerManageResult.StatusCode);
        Assert.True(newOwnerManageResult.Success);
    }

    [Fact]
    public async Task TransferOwnershipAsync_RejectsNonOwnerNonManagerAndLimitedWorkspace()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var service = CreateService(context);

        var nonOwnerResult = await service.TransferOwnershipAsync(
            fixture.Workspace.Id,
            fixture.Manager.UserId,
            new TransferWorkspaceOwnershipRequest { TargetMemberId = fixture.Manager.Id });
        var nonManagerResult = await service.TransferOwnershipAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            new TransferWorkspaceOwnershipRequest { TargetMemberId = fixture.Viewer.Id });
        fixture.Workspace.Status = WorkspaceStatusEnum.Limited;
        await context.SaveChangesAsync();
        var limitedResult = await service.TransferOwnershipAsync(
            fixture.Workspace.Id,
            fixture.Owner.UserId,
            new TransferWorkspaceOwnershipRequest { TargetMemberId = fixture.Manager.Id });

        Assert.Equal((int)HttpStatusCode.Forbidden, nonOwnerResult.StatusCode);
        Assert.Equal((int)HttpStatusCode.BadRequest, nonManagerResult.StatusCode);
        Assert.Equal((int)HttpStatusCode.Forbidden, limitedResult.StatusCode);
    }

    private static WorkspaceMemberService CreateService(AisamContext context)
        => new(
            new WorkspaceMemberRepository(context),
            new WorkspaceRepository(context),
            new SubscriptionRepository(context));

    private static WorkspaceMemberFixture SeedWorkspace(
        AisamContext context,
        SubscriptionPlanEnum plan = SubscriptionPlanEnum.Premium)
    {
        var ownerUser = AddUser(context);
        var managerUser = AddUser(context);
        var viewerUser = AddUser(context);
        var workspace = new Workspace
        {
            Name = "Business",
            WorkspaceType = WorkspaceTypeEnum.Business,
            MemberLimit = 10
        };
        var owner = CreateMember(workspace, ownerUser, WorkspaceMemberRoleEnum.Owner);
        var manager = CreateMember(workspace, managerUser, WorkspaceMemberRoleEnum.Manager);
        var viewer = CreateMember(workspace, viewerUser, WorkspaceMemberRoleEnum.Viewer);
        var subscription = new Subscription
        {
            WorkspaceId = workspace.Id,
            Plan = plan,
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(29),
            IsActive = true
        };
        context.AddRange(workspace, owner, manager, viewer, subscription);
        context.SaveChanges();
        return new WorkspaceMemberFixture(workspace, owner, manager, viewer);
    }

    private static WorkspaceMember CreateMember(Workspace workspace, User user, WorkspaceMemberRoleEnum role)
        => new()
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = user.Id,
            User = user,
            Role = role
        };

    private static User AddUser(AisamContext context)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        context.Users.Add(user);
        return user;
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AisamContext(options);
    }

    private sealed record WorkspaceMemberFixture(
        Workspace Workspace,
        WorkspaceMember Owner,
        WorkspaceMember Manager,
        WorkspaceMember Viewer);
}




