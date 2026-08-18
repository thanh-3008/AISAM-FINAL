using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class WorkspaceInvitationRepositoryTests
{
    [Fact]
    public async Task AddAsync_NormalizesEmailAndPersistsInvitation()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var repository = new WorkspaceInvitationRepository(context);
        var invitation = CreateInvitation(fixture, "  INVITED@Example.COM  ");

        var result = await repository.AddAsync(invitation);

        Assert.Equal("invited@example.com", result.Email);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.Equal(1, await context.WorkspaceInvitations.CountAsync());
    }

    [Fact]
    public async Task PendingQueries_ReturnOnlyUnexpiredUnresolvedInvitations()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var repository = new WorkspaceInvitationRepository(context);
        var pending = CreateInvitation(fixture, "pending@example.com");
        var accepted = CreateInvitation(fixture, "accepted@example.com");
        accepted.AcceptedAt = DateTime.UtcNow;
        var revoked = CreateInvitation(fixture, "revoked@example.com");
        revoked.RevokedAt = DateTime.UtcNow;
        var expired = CreateInvitation(fixture, "expired@example.com");
        expired.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        context.WorkspaceInvitations.AddRange(pending, accepted, revoked, expired);
        await context.SaveChangesAsync();

        var result = await repository.GetPendingByWorkspaceIdAsync(fixture.Workspace.Id);
        var count = await repository.CountPendingByWorkspaceIdAsync(fixture.Workspace.Id);

        Assert.Single(result);
        Assert.Equal(pending.Id, result[0].Id);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetPendingByWorkspaceAndEmailAsync_NormalizesLookupEmail()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var repository = new WorkspaceInvitationRepository(context);
        var invitation = CreateInvitation(fixture, "member@example.com");
        context.WorkspaceInvitations.Add(invitation);
        await context.SaveChangesAsync();

        var result = await repository.GetPendingByWorkspaceAndEmailAsync(
            fixture.Workspace.Id,
            " MEMBER@EXAMPLE.COM ");

        Assert.NotNull(result);
        Assert.Equal(invitation.Id, result.Id);
        Assert.Equal(fixture.Inviter.Id, result.InvitedByUser.Id);
    }

    [Fact]
    public async Task GetByTokenAsync_ReturnsResolvedInvitationForAudit()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var repository = new WorkspaceInvitationRepository(context);
        var invitation = CreateInvitation(fixture, "accepted@example.com");
        invitation.AcceptedAt = DateTime.UtcNow;
        context.WorkspaceInvitations.Add(invitation);
        await context.SaveChangesAsync();

        var result = await repository.GetByTokenAsync(invitation.Token);

        Assert.NotNull(result);
        Assert.Equal(invitation.Id, result.Id);
        Assert.NotNull(result.AcceptedAt);
    }

    [Fact]
    public async Task AcceptAsync_ReactivatesExistingInactiveMembershipAndAcceptsInvitation()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context);
        var invitedUser = new User
        {
            Email = "invited@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var inactiveMembership = new WorkspaceMember
        {
            WorkspaceId = fixture.Workspace.Id,
            UserId = invitedUser.Id,
            Role = WorkspaceMemberRoleEnum.Viewer,
            IsActive = false
        };
        var invitation = CreateInvitation(fixture, invitedUser.Email);
        context.AddRange(invitedUser, inactiveMembership, invitation);
        await context.SaveChangesAsync();
        var repository = new WorkspaceInvitationRepository(context);

        var result = await repository.AcceptAsync(invitation, invitedUser.Id);

        Assert.Equal(inactiveMembership.Id, result.Id);
        Assert.True(result.IsActive);
        Assert.Equal(WorkspaceMemberRoleEnum.ContentCreator, result.Role);
        Assert.NotNull(invitation.AcceptedAt);
        Assert.Equal(1, await context.WorkspaceMembers.CountAsync(member =>
            member.WorkspaceId == fixture.Workspace.Id && member.UserId == invitedUser.Id));
    }

    private static WorkspaceInvitation CreateInvitation(
        WorkspaceInvitationFixture fixture,
        string email)
    {
        return new WorkspaceInvitation
        {
            WorkspaceId = fixture.Workspace.Id,
            InvitedByUserId = fixture.Inviter.Id,
            Email = email,
            Role = WorkspaceMemberRoleEnum.ContentCreator,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }

    private static WorkspaceInvitationFixture SeedWorkspace(AisamContext context)
    {
        var inviter = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var workspace = new Workspace
        {
            Name = "Business workspace",
            WorkspaceType = WorkspaceTypeEnum.Business
        };
        context.AddRange(inviter, workspace);
        context.SaveChanges();
        return new WorkspaceInvitationFixture(inviter, workspace);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private sealed record WorkspaceInvitationFixture(User Inviter, Workspace Workspace);
}




