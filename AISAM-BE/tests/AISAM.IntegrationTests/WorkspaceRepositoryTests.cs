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
    public async Task RemoveAsync_DeactivatesMembership()
    {
        await using var context = CreateContext();
        var fixture = SeedMemberships(context);
        var repository = new WorkspaceMemberRepository(context);

        var removed = await repository.RemoveAsync(fixture.FirstMembership.Id);

        Assert.True(removed);
        Assert.False(await repository.ExistsAsync(fixture.FirstWorkspace.Id, fixture.User.Id));
        Assert.Null(await repository.GetByWorkspaceAndUserAsync(fixture.FirstWorkspace.Id, fixture.User.Id));
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
        var inactiveMembership = new WorkspaceMember
        {
            WorkspaceId = inactiveWorkspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Viewer,
            IsActive = false
        };

        context.AddRange(
            user,
            firstWorkspace,
            secondWorkspace,
            inactiveWorkspace,
            firstMembership,
            secondMembership,
            inactiveMembership);
        context.SaveChanges();

        return new WorkspaceRepositoryFixture(
            user,
            firstWorkspace,
            secondWorkspace,
            inactiveWorkspace,
            firstMembership);
    }

    private sealed record WorkspaceRepositoryFixture(
        User User,
        Workspace FirstWorkspace,
        Workspace SecondWorkspace,
        Workspace InactiveWorkspace,
        WorkspaceMember FirstMembership);
}
