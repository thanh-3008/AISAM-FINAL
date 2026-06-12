using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace AISAM.IntegrationTests;

public class WorkspaceInvitationServiceTests
{
    [Fact]
    public async Task InviteAsync_AllowsBusinessWorkspaceOwnerAndSendsEmail()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var emailService = new FakeEmailService();
        var service = CreateService(context, emailService);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = " Invited@Example.com ",
            Role = WorkspaceMemberRoleEnum.ContentCreator
        });

        Assert.True(result.Success);
        Assert.Equal("invited@example.com", result.Data!.Email);
        Assert.Equal("invited@example.com", emailService.LastRecipient);
        Assert.Contains("/workspace/invitations/accept?token=", emailService.LastInvitationLink);
        Assert.Equal(1, await context.WorkspaceInvitations.CountAsync());
    }

    [Fact]
    public async Task InviteAsync_PersistsMonthlyAssignedLimitForBusinessPro()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business, subscriptionPlan: SubscriptionPlanEnum.Premium);
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "invited@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer,
            QuotaMode = MemberQuotaModeEnum.MonthlyAssignedLimit,
            CreditLimit = 250
        });

        Assert.True(result.Success);
        Assert.Equal(MemberQuotaModeEnum.MonthlyAssignedLimit, result.Data!.QuotaMode);
        Assert.Equal(250, result.Data.CreditLimit);

        var invitation = await context.WorkspaceInvitations.SingleAsync();
        Assert.Equal(MemberQuotaModeEnum.MonthlyAssignedLimit, invitation.QuotaMode);
        Assert.Equal(250, invitation.CreditLimit);
    }

    [Fact]
    public async Task InviteAsync_RejectsAssignedLimitForBusinessPlus()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business, subscriptionPlan: SubscriptionPlanEnum.Plus);
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "invited@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer,
            QuotaMode = MemberQuotaModeEnum.LifetimeAssignedLimit,
            CreditLimit = 100
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Empty(context.WorkspaceInvitations);
    }

    [Theory]
    [InlineData(WorkspaceTypeEnum.Personal, WorkspaceMemberRoleEnum.Owner, HttpStatusCode.Forbidden)]
    [InlineData(WorkspaceTypeEnum.Business, WorkspaceMemberRoleEnum.Manager, HttpStatusCode.Forbidden)]
    public async Task InviteAsync_RejectsUnsupportedWorkspaceOrInviter(
        WorkspaceTypeEnum workspaceType,
        WorkspaceMemberRoleEnum inviterRole,
        HttpStatusCode expectedStatus)
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, workspaceType, inviterRole);
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "invited@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer
        });

        Assert.False(result.Success);
        Assert.Equal((int)expectedStatus, result.StatusCode);
        Assert.Empty(context.WorkspaceInvitations);
    }

    [Fact]
    public async Task InviteAsync_RejectsOwnerRole()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "invited@example.com",
            Role = WorkspaceMemberRoleEnum.Owner
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task InviteAsync_RejectsDuplicatePendingInvitation()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var service = CreateService(context);
        var request = new CreateWorkspaceInvitationRequest
        {
            Email = "invited@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer
        };
        await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, request);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, request);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(1, await context.WorkspaceInvitations.CountAsync());
    }

    [Fact]
    public async Task InviteAsync_RejectsExistingMemberRegardlessOfEmailCase()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var existingMember = AddUser(context, "Existing.Member@Example.com");
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = fixture.Workspace.Id,
            UserId = existingMember.Id,
            Role = WorkspaceMemberRoleEnum.Viewer
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "existing.member@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
        Assert.Empty(context.WorkspaceInvitations);
    }

    [Fact]
    public async Task InviteAsync_RejectsWhenActiveMembersAndPendingInvitationsReachLimit()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business, memberLimit: 2);
        AddInvitation(context, fixture, "reserved@example.com");
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "blocked@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task InviteAsync_AllowsMoreThanTenSlotsWhenWorkspaceLimitIsFifty()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business, memberLimit: 50);
        for (var index = 0; index < 10; index++)
        {
            context.WorkspaceInvitations.Add(new WorkspaceInvitation
            {
                WorkspaceId = fixture.Workspace.Id,
                InvitedByUserId = fixture.Owner.Id,
                Email = $"reserved-{index}@example.com",
                Role = WorkspaceMemberRoleEnum.Viewer,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "allowed@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer
        });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task AcceptAsync_CreatesMembershipAndMarksInvitationAccepted()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var invitedUser = AddUser(context, "invited@example.com");
        var invitation = AddInvitation(context, fixture, invitedUser.Email);
        var service = CreateService(context);

        var result = await service.AcceptAsync(invitedUser.Id, new AcceptWorkspaceInvitationRequest
        {
            Token = invitation.Token
        });

        Assert.True(result.Success);
        Assert.Equal(WorkspaceMemberRoleEnum.ContentCreator, result.Data!.Role);
        Assert.NotNull((await context.WorkspaceInvitations.SingleAsync()).AcceptedAt);
        var membership = await context.WorkspaceMembers.SingleAsync(member => member.UserId == invitedUser.Id);
        Assert.Equal(WorkspaceMemberRoleEnum.ContentCreator, membership.Role);
        Assert.True(membership.IsActive);
    }

    [Fact]
    public async Task AcceptAsync_RejectsWhenWorkspaceReachedLimitAfterInvitationCreation()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business, memberLimit: 2);
        var invitedUser = AddUser(context, "invited@example.com");
        var invitation = AddInvitation(context, fixture, invitedUser.Email);
        var existingUser = AddUser(context, "existing@example.com");
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = fixture.Workspace.Id,
            UserId = existingUser.Id,
            Role = WorkspaceMemberRoleEnum.Viewer
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.AcceptAsync(invitedUser.Id, new AcceptWorkspaceInvitationRequest { Token = invitation.Token });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Conflict, result.StatusCode);
        Assert.Null((await context.WorkspaceInvitations.SingleAsync(item => item.Id == invitation.Id)).AcceptedAt);
    }

    [Fact]
    public async Task AcceptAsync_RejectsInvitationAfterWorkspaceLeavesActiveState()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var invitedUser = AddUser(context, "invited@example.com");
        var invitation = AddInvitation(context, fixture, invitedUser.Email);
        fixture.Workspace.Status = WorkspaceStatusEnum.Limited;
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.AcceptAsync(invitedUser.Id, new AcceptWorkspaceInvitationRequest { Token = invitation.Token });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Null((await context.WorkspaceInvitations.SingleAsync(item => item.Id == invitation.Id)).AcceptedAt);
    }

    [Fact]
    public async Task InviteAsync_RejectsRuntimeLimitedWorkspaceEvenIfPersistedStatusIsStillActive()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        fixture.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-30);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.InviteAsync(fixture.Workspace.Id, fixture.Owner.Id, new CreateWorkspaceInvitationRequest
        {
            Email = "invited@example.com",
            Role = WorkspaceMemberRoleEnum.Viewer
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Empty(context.WorkspaceInvitations);
    }

    [Fact]
    public async Task AcceptAsync_RejectsRuntimeArchivedWorkspaceEvenIfPersistedStatusIsStillActive()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var invitedUser = AddUser(context, "invited@example.com");
        var invitation = AddInvitation(context, fixture, invitedUser.Email);
        fixture.Workspace.SubscriptionExpiredAt = DateTime.UtcNow.Date.AddDays(-90);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.AcceptAsync(invitedUser.Id, new AcceptWorkspaceInvitationRequest { Token = invitation.Token });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Null((await context.WorkspaceInvitations.SingleAsync(item => item.Id == invitation.Id)).AcceptedAt);
    }

    [Fact]
    public async Task AcceptAsync_RejectsAuthenticatedUserWithDifferentEmail()
    {
        await using var context = CreateContext();
        var fixture = SeedWorkspace(context, WorkspaceTypeEnum.Business);
        var invitedUser = AddUser(context, "invited@example.com");
        var otherUser = AddUser(context, "other@example.com");
        var invitation = AddInvitation(context, fixture, invitedUser.Email);
        var service = CreateService(context);

        var result = await service.AcceptAsync(otherUser.Id, new AcceptWorkspaceInvitationRequest
        {
            Token = invitation.Token
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Null((await context.WorkspaceInvitations.SingleAsync()).AcceptedAt);
    }

    private static WorkspaceInvitationService CreateService(AisamContext context, IEmailService? emailService = null)
    {
        return new WorkspaceInvitationService(
            new WorkspaceRepository(context),
            new WorkspaceMemberRepository(context),
            new WorkspaceInvitationRepository(context),
            new SubscriptionRepository(context),
            new UserRepository(context),
            emailService ?? new FakeEmailService(),
            new WorkspaceLifecycleService(),
            Options.Create(new FrontendSettings { BaseUrl = "http://localhost:3000" }));
    }

    private static WorkspaceInvitationFixture SeedWorkspace(
        AisamContext context,
        WorkspaceTypeEnum workspaceType,
        WorkspaceMemberRoleEnum ownerRole = WorkspaceMemberRoleEnum.Owner,
        int? memberLimit = null,
        SubscriptionPlanEnum subscriptionPlan = SubscriptionPlanEnum.Premium)
    {
        var owner = AddUser(context, $"{Guid.NewGuid():N}@example.com");
        var workspace = new Workspace
        {
            Name = "Workspace",
            WorkspaceType = workspaceType,
            MemberLimit = memberLimit ?? (workspaceType == WorkspaceTypeEnum.Business ? 10 : 1),
            Members =
            [
                new WorkspaceMember
                {
                    UserId = owner.Id,
                    Role = ownerRole
                }
            ]
        };
        context.Workspaces.Add(workspace);
        if (workspaceType == WorkspaceTypeEnum.Business)
        {
            context.Subscriptions.Add(new Subscription
            {
                WorkspaceId = workspace.Id,
                Plan = subscriptionPlan,
                StartDate = DateTime.UtcNow.Date.AddDays(-1),
                EndDate = DateTime.UtcNow.Date.AddDays(29),
                IsActive = true
            });
        }
        context.SaveChanges();
        return new WorkspaceInvitationFixture(owner, workspace);
    }

    private static WorkspaceInvitation AddInvitation(
        AisamContext context,
        WorkspaceInvitationFixture fixture,
        string email)
    {
        var invitation = new WorkspaceInvitation
        {
            WorkspaceId = fixture.Workspace.Id,
            InvitedByUserId = fixture.Owner.Id,
            Email = email,
            Role = WorkspaceMemberRoleEnum.ContentCreator,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        context.WorkspaceInvitations.Add(invitation);
        context.SaveChanges();
        return invitation;
    }

    private static User AddUser(AisamContext context, string email)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AisamContext(options);
    }

    private sealed record WorkspaceInvitationFixture(User Owner, Workspace Workspace);

    private sealed class FakeEmailService : IEmailService
    {
        public string? LastRecipient { get; private set; }
        public string? LastInvitationLink { get; private set; }

        public Task SendTeamInvitationAsync(string email, string teamName, string inviterName, string invitationLink)
        {
            LastRecipient = email;
            LastInvitationLink = invitationLink;
            return Task.CompletedTask;
        }

        public Task SendEmailVerificationAsync(string email, string userName, string verificationToken) => Task.CompletedTask;
        public Task SendPasswordResetAsync(string email, string userName, string resetToken) => Task.CompletedTask;
        public Task SendWelcomeEmailAsync(string email, string userName) => Task.CompletedTask;
        public Task SendNotificationEmailAsync(string email, string subject, string message) => Task.CompletedTask;
        public Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null) => Task.FromResult(true);
    }
}
