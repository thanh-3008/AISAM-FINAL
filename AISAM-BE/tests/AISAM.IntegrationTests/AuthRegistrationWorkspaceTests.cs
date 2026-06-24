using AISAM.Common.Config;
using AISAM.Common.Dtos.Request;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

public class AuthRegistrationWorkspaceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesPersonalWorkspaceWithOwnerMembership()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var request = CreateRegisterRequest();

        await service.RegisterAsync(request, "test-agent", "127.0.0.1");

        var user = await context.Users
            .Include(item => item.WorkspaceMembers)
            .ThenInclude(member => member.Workspace)
            .SingleAsync();
        var membership = Assert.Single(user.WorkspaceMembers);
        Assert.Equal(WorkspaceMemberRoleEnum.Owner, membership.Role);
        Assert.Equal(WorkspaceTypeEnum.Personal, membership.Workspace.WorkspaceType);
        Assert.Equal($"{request.FullName}'s Workspace", membership.Workspace.Name);
    }

    [Fact]
    public async Task RegisterAsync_CreatesExactlyOnePersonalWorkspace()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        await service.RegisterAsync(CreateRegisterRequest(), null, null);

        Assert.Equal(1, await context.Workspaces.CountAsync());
        Assert.Equal(1, await context.WorkspaceMembers.CountAsync());
        Assert.Equal(1, await context.CreditWallets.CountAsync());
        Assert.Equal(50, (await context.CreditWallets.SingleAsync()).Balance);
        Assert.Equal(1, await context.Subscriptions.CountAsync(subscription => subscription.Plan == SubscriptionPlanEnum.Free));
        Assert.Equal(1, await context.CreditUsageRecords.CountAsync(record => record.Action == CreditActionEnum.SubscriptionGrant));
        Assert.Equal(1, await context.WorkspaceMembers.CountAsync(member => member.Role == WorkspaceMemberRoleEnum.Owner));
    }

    [Fact]
    public async Task RegisterAsync_ReturnsDefaultWorkspaceWithInitialCredits()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var response = await service.RegisterAsync(CreateRegisterRequest(), null, null);

        Assert.NotNull(response.DefaultWorkspace);
        Assert.Equal(50, response.DefaultWorkspace.CreditBalance);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_DoesNotCreateAnotherWorkspace()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var request = CreateRegisterRequest();
        await service.RegisterAsync(request, null, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request, null, null));

        Assert.Equal(1, await context.Users.CountAsync());
        Assert.Equal(1, await context.Workspaces.CountAsync());
        Assert.Equal(1, await context.WorkspaceMembers.CountAsync());
        Assert.Equal(1, await context.CreditWallets.CountAsync());
    }

    private static AuthService CreateService(AisamContext context)
    {
        return new AuthService(
            new UserRepository(context),
            new InMemorySessionRepository(),
            new NoOpEmailService(),
            Options.Create(new JwtSettings
            {
                SecretKey = "aisam-test-secret-key-with-at-least-thirty-two-characters",
                Issuer = "AISAM.Tests",
                Audience = "AISAM.Tests"
            }),
            Options.Create(new GoogleSettings()));
    }

    private static RegisterRequest CreateRegisterRequest()
    {
        return new RegisterRequest
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "Test User"
        };
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private sealed class InMemorySessionRepository : ISessionRepository
    {
        private readonly List<Session> _sessions = [];

        public Task<Session> CreateAsync(Session session)
        {
            _sessions.Add(session);
            return Task.FromResult(session);
        }

        public Task<Session?> GetByIdAsync(Guid id) => Task.FromResult(_sessions.FirstOrDefault(session => session.Id == id));
        public Task<Session?> GetByRefreshTokenAsync(string refreshToken) => Task.FromResult(_sessions.FirstOrDefault(session => session.RefreshToken == refreshToken));
        public Task<List<Session>> GetActiveSessionsByUserIdAsync(Guid userId) => Task.FromResult(_sessions.Where(session => session.UserId == userId && session.IsActive).ToList());
        public Task UpdateAsync(Session session) => Task.CompletedTask;
        public Task RevokeSessionAsync(Guid sessionId) => Task.CompletedTask;
        public Task RevokeAllUserSessionsAsync(Guid userId) => Task.CompletedTask;
        public Task DeleteExpiredSessionsAsync() => Task.CompletedTask;
    }

    private sealed class NoOpEmailService : IEmailService
    {
        public Task SendEmailVerificationAsync(string email, string userName, string verificationToken) => Task.CompletedTask;
        public Task SendPasswordResetAsync(string email, string userName, string resetToken) => Task.CompletedTask;
        public Task SendWelcomeEmailAsync(string email, string userName) => Task.CompletedTask;
        public Task SendTeamInvitationAsync(string email, string teamName, string inviterName, string invitationLink) => Task.CompletedTask;
        public Task SendNotificationEmailAsync(string email, string subject, string message) => Task.CompletedTask;
        public Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainTextBody = null) => Task.FromResult(true);
    }
}
