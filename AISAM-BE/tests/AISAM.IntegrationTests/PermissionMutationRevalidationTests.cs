using AISAM.API.Controllers;
using AISAM.Common.Dtos.Request;
using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public sealed class PermissionMutationRevalidationTests
{
    [Fact]
    public async Task Teams_Create_DoesNotCommitAfterOwnerAuthorityWasRevoked()
    {
        await using var fixture = await PermissionSecurityTests.Fixture.CreateAsync();
        await fixture.Resolver.ResolveAsync(fixture.Workspace.Id, fixture.Owner.Id, write: true);

        await RevokeOwnerAsync(fixture);

        var controller = new TeamsController(fixture.Db, fixture.Db.AccessScope);
        var request = new TeamsController.TeamRequest("Rejected stale mutation", null, [], []);

        await Assert.ThrowsAsync<MutationAuthorizationException>(() => controller.Create(request, default));

        await using var verifier = CreateContext(fixture);
        Assert.False(await verifier.Teams.AnyAsync(team => team.Name == "Rejected stale mutation"));
    }

    [Fact]
    public async Task WorkspaceMember_UpdateRole_DoesNotCommitWhenOwnerIsRevokedAfterInitialCheck()
    {
        await using var fixture = await PermissionSecurityTests.Fixture.CreateAsync();
        await fixture.Resolver.ResolveAsync(fixture.Workspace.Id, fixture.Owner.Id, write: true);
        var target = await fixture.Db.WorkspaceMembers
            .AsNoTracking()
            .SingleAsync(member => member.UserId == fixture.Manager.Id);
        var inner = new WorkspaceMemberRepository(fixture.Db);
        var repository = new RevokingWorkspaceMemberRepository(inner, () => RevokeOwnerAsync(fixture));
        var service = new WorkspaceMemberService(repository, null!, null!, fixture.Db);

        await Assert.ThrowsAsync<MutationAuthorizationException>(() => service.UpdateRoleAsync(
            fixture.Workspace.Id,
            fixture.Owner.Id,
            target.Id,
            new UpdateWorkspaceMemberRoleRequest { Role = WorkspaceMemberRoleEnum.Viewer }));

        await using var verifier = CreateContext(fixture);
        Assert.Equal(
            WorkspaceMemberRoleEnum.Manager,
            await verifier.WorkspaceMembers
                .Where(member => member.Id == target.Id)
                .Select(member => member.Role)
                .SingleAsync());
    }

    private static AisamContext CreateContext(PermissionSecurityTests.Fixture fixture)
        => new(new DbContextOptionsBuilder<AisamContext>().UseSqlite(fixture.Connection).Options);

    private static async Task RevokeOwnerAsync(PermissionSecurityTests.Fixture fixture)
    {
        await using var writer = CreateContext(fixture);
        var owner = await writer.WorkspaceMembers.SingleAsync(member =>
            member.WorkspaceId == fixture.Workspace.Id && member.UserId == fixture.Owner.Id);
        owner.Role = WorkspaceMemberRoleEnum.Manager;
        await writer.SaveChangesAsync();
    }

    private sealed class RevokingWorkspaceMemberRepository(
        IWorkspaceMemberRepository inner,
        Func<Task> revoke) : IWorkspaceMemberRepository
    {
        private bool revoked;

        public Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
            => inner.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);

        public async Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!revoked)
            {
                revoked = true;
                await revoke();
            }

            return await inner.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => inner.GetByWorkspaceIdAsync(workspaceId, cancellationToken);

        public Task<IReadOnlyList<WorkspaceMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => inner.GetByUserIdAsync(userId, cancellationToken);

        public Task<WorkspaceMember> AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default)
            => inner.AddAsync(member, cancellationToken);

        public Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default)
            => inner.UpdateAsync(member, cancellationToken);

        public Task<WorkspaceMember> TransferOwnershipAsync(Guid workspaceId, Guid currentOwnerUserId, Guid targetMemberId, CancellationToken cancellationToken = default)
            => inner.TransferOwnershipAsync(workspaceId, currentOwnerUserId, targetMemberId, cancellationToken);

        public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
            => inner.RemoveAsync(id, cancellationToken);

        public Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
            => inner.ExistsAsync(workspaceId, userId, cancellationToken);
    }
}
