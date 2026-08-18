using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class WorkspaceRepositoryTests
{
    [Fact]
    public async Task GetByUserIdAsync_ReturnsAllActiveWorkspacesForUser()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceRepository(context);

        var result = await repository.GetByUserIdAsync(fixture.User.Id);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, workspace => workspace.Id == fixture.FirstWorkspace.Id);
        Assert.Contains(result, workspace => workspace.Id == fixture.SecondWorkspace.Id);
        Assert.DoesNotContain(result, workspace => workspace.Id == fixture.InactiveWorkspace.Id);
    }

    [Fact]
    public async Task AddAsync_AndUpdateAsync_PersistWorkspace()
    {
        await using var context = CreateContext();
        var repository = new WorkspaceRepository(context);
        var workspace = new Workspace
        {
            Name = "Initial workspace",
            WorkspaceType = WorkspaceTypeEnum.Personal
        };

        await repository.AddAsync(workspace);
        workspace.Name = "Updated workspace";
        await repository.UpdateAsync(workspace);

        var persisted = await repository.GetByIdAsync(workspace.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Updated workspace", persisted.Name);
    }

    [Fact]
    public async Task AddAsync_RejectsDuplicateMembershipInSameWorkspace()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);
        var duplicate = new WorkspaceMember
        {
            WorkspaceId = fixture.FirstWorkspace.Id,
            UserId = fixture.User.Id,
            Role = WorkspaceMemberRoleEnum.Manager
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(duplicate));

        Assert.Equal("User is already a member of this workspace.", exception.Message);
    }

    [Fact]
    public async Task AddAsync_RejectsAssigningOwnerRole()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);
        var ownerMembership = new WorkspaceMember
        {
            WorkspaceId = fixture.SecondWorkspace.Id,
            UserId = Guid.NewGuid(),
            Role = WorkspaceMemberRoleEnum.Owner
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(ownerMembership));

        Assert.Equal("Use workspace creation or ownership transfer to assign the owner.", exception.Message);
    }

    [Fact]
    public async Task AddAsync_ReactivatesInactiveMembershipWithoutCreatingDuplicate()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);
        var reactivatedMembership = new WorkspaceMember
        {
            WorkspaceId = fixture.InactiveWorkspace.Id,
            UserId = fixture.User.Id,
            Role = WorkspaceMemberRoleEnum.ContentCreator,
            QuotaMode = MemberQuotaModeEnum.MonthlyAssignedLimit,
            CreditLimit = 100
        };

        var result = await repository.AddAsync(reactivatedMembership);

        Assert.Equal(fixture.InactiveMembership.Id, result.Id);
        Assert.True(result.IsActive);
        Assert.Equal(WorkspaceMemberRoleEnum.ContentCreator, result.Role);
        Assert.Equal(MemberQuotaModeEnum.MonthlyAssignedLimit, result.QuotaMode);
        Assert.Equal(100, result.CreditLimit);
        Assert.Equal(
            1,
            await context.WorkspaceMembers.CountAsync(member =>
                member.WorkspaceId == fixture.InactiveWorkspace.Id &&
                member.UserId == fixture.User.Id));
    }

    [Fact]
    public async Task RemoveAsync_DeactivatesMembership()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);

        var removed = await repository.RemoveAsync(fixture.SecondMembership.Id);

        Assert.True(removed);
        Assert.False(await repository.ExistsAsync(fixture.SecondWorkspace.Id, fixture.User.Id));
        Assert.Null(await repository.GetByWorkspaceAndUserAsync(fixture.SecondWorkspace.Id, fixture.User.Id));
    }

    [Fact]
    public async Task RemoveAsync_RejectsRemovingWorkspaceOwner()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.RemoveAsync(fixture.FirstMembership.Id));

        Assert.Equal("Workspace owner cannot be removed. Transfer ownership first.", exception.Message);
        Assert.True(await repository.ExistsAsync(fixture.FirstWorkspace.Id, fixture.User.Id));
    }

    [Fact]
    public async Task UpdateAsync_RejectsChangingWorkspaceOwnerRole()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);
        context.Entry(fixture.FirstMembership).State = EntityState.Detached;
        fixture.FirstMembership.Role = WorkspaceMemberRoleEnum.Manager;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.UpdateAsync(fixture.FirstMembership));

        Assert.Equal("Workspace owner role cannot be changed. Transfer ownership first.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RejectsPromotingMemberToOwnerWithoutOwnershipTransfer()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);
        context.Entry(fixture.SecondMembership).State = EntityState.Detached;
        fixture.SecondMembership.Role = WorkspaceMemberRoleEnum.Owner;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.UpdateAsync(fixture.SecondMembership));

        Assert.Equal("Use ownership transfer to change the workspace owner.", exception.Message);
    }

    [Fact]
    public async Task TransferOwnershipAsync_SwapsOwnerAndManagerRoles()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);

        var newOwner = await repository.TransferOwnershipAsync(
            fixture.SecondWorkspace.Id,
            fixture.SecondOwner.UserId,
            fixture.SecondMembership.Id);

        Assert.Equal(WorkspaceMemberRoleEnum.Owner, newOwner.Role);
        Assert.Equal(
            WorkspaceMemberRoleEnum.Manager,
            await context.WorkspaceMembers
                .Where(member => member.Id == fixture.SecondOwner.Id)
                .Select(member => member.Role)
                .SingleAsync());
        Assert.Equal(
            1,
            await context.WorkspaceMembers.CountAsync(member =>
                member.WorkspaceId == fixture.SecondWorkspace.Id &&
                member.IsActive &&
                member.Role == WorkspaceMemberRoleEnum.Owner));
    }

    [Fact]
    public async Task TransferOwnershipAsync_RejectsInvalidOwnerInvariantWithoutChangingRoles()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);
        fixture.SecondOwner.Role = WorkspaceMemberRoleEnum.Manager;
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.TransferOwnershipAsync(
                fixture.SecondWorkspace.Id,
                fixture.SecondOwner.UserId,
                fixture.SecondMembership.Id));

        Assert.Equal("Workspace must have exactly one current owner.", exception.Message);
        Assert.Equal(
            0,
            await context.WorkspaceMembers.CountAsync(member =>
                member.WorkspaceId == fixture.SecondWorkspace.Id &&
                member.Role == WorkspaceMemberRoleEnum.Owner));
        Assert.Equal(
            WorkspaceMemberRoleEnum.Manager,
            await context.WorkspaceMembers.Where(member => member.Id == fixture.SecondMembership.Id).Select(member => member.Role).SingleAsync());
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static WorkspaceRepositoryFixture SeedMemberships(AisamContext context)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var firstWorkspace = new Workspace
        {
            Name = "First",
            WorkspaceType = WorkspaceTypeEnum.Personal
        };
        var secondWorkspace = new Workspace
        {
            Name = "Second",
            WorkspaceType = WorkspaceTypeEnum.Business
        };
        var inactiveWorkspace = new Workspace
        {
            Name = "Inactive membership",
            WorkspaceType = WorkspaceTypeEnum.Business
        };
        var firstMembership = new WorkspaceMember
        {
            WorkspaceId = firstWorkspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Owner
        };
        var secondMembership = new WorkspaceMember
        {
            WorkspaceId = secondWorkspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Manager
        };
        var secondOwnerUser = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var secondOwner = new WorkspaceMember
        {
            WorkspaceId = secondWorkspace.Id,
            UserId = secondOwnerUser.Id,
            Role = WorkspaceMemberRoleEnum.Owner
        };
        var inactiveMembership = new WorkspaceMember
        {
            WorkspaceId = inactiveWorkspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Viewer,
            IsActive = false
        };

        context.AddRange(
            user,
            secondOwnerUser,
            firstWorkspace,
            secondWorkspace,
            inactiveWorkspace,
            firstMembership,
            secondMembership,
            secondOwner,
            inactiveMembership);
        context.SaveChanges();

        return new WorkspaceRepositoryFixture(
            user,
            firstWorkspace,
            secondWorkspace,
            inactiveWorkspace,
            firstMembership,
            secondMembership,
            secondOwner,
            inactiveMembership);
    }

    private sealed record WorkspaceRepositoryFixture(
        User User,
        Workspace FirstWorkspace,
        Workspace SecondWorkspace,
        Workspace InactiveWorkspace,
        WorkspaceMember FirstMembership,
        WorkspaceMember SecondMembership,
        WorkspaceMember SecondOwner,
        WorkspaceMember InactiveMembership);
}




