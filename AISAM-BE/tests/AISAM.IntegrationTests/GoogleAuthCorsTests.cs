using System.Net;
using AISAM.API.Controllers;
using AISAM.Common;
using AISAM.Common.Config;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

public sealed class GoogleAuthCorsTests
{
    private static IConfiguration CreateConfig(params string[] allowedOrigins)
    {
        var dict = new Dictionary<string, string?>();
        for (int i = 0; i < allowedOrigins.Length; i++)
        {
            dict[$"AllowedOrigins:{i}"] = allowedOrigins[i];
        }
        dict["FrontendSettings:BaseUrl"] = "https://aisam.io.vn";

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    [Theory]
    [InlineData("https://aisam.io.vn")]
    [InlineData("https://aisam.ddns.net")]
    [InlineData("http://localhost:3000")]
    public void GoogleAuthOrigins_ArePermittedByOriginResolver(string origin)
    {
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net", "http://localhost:3000");
        var resolver = new OriginResolver(config);

        Assert.True(resolver.IsAllowedOrigin(origin));
        Assert.Equal(origin.TrimEnd('/'), resolver.ResolveOrigin(origin));
    }

    [Theory]
    [InlineData("https://evil.com")]
    [InlineData("https://malicious-site.org")]
    [InlineData("https://aisam.ddns.net.attacker.com")]
    public void UntrustedOrigins_AreRejectedByOriginResolver(string maliciousOrigin)
    {
        var config = CreateConfig("https://aisam.io.vn", "https://aisam.ddns.net");
        var resolver = new OriginResolver(config);

        Assert.False(resolver.IsAllowedOrigin(maliciousOrigin));
        Assert.Throws<InvalidOperationException>(() => resolver.ResolveOrigin(maliciousOrigin));
    }

    [Fact]
    public async Task GoogleLogin_Returns401_WhenClientIdNotConfigured()
    {
        await using var context = CreateContext();
        var userRepo = new UserRepository(context);
        var sessionRepo = new InMemorySessionRepository();
        var emailService = new NoOpEmailService();
        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "super-secret-key-that-is-at-least-thirty-two-bytes-long",
            Issuer = "AISAM.Tests",
            Audience = "AISAM.Tests"
        });
        var emptyGoogleSettings = Options.Create(new GoogleSettings { ClientId = "" });

        var authService = new AuthService(userRepo, sessionRepo, emailService, jwtSettings, emptyGoogleSettings);
        var controller = new AuthController(authService, NullLogger<AuthController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var response = await controller.GoogleLogin(new GoogleLoginRequest { IdToken = "some-fake-token" });

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
        var body = Assert.IsType<GenericResponse<object>>(unauthorizedResult.Value);
        Assert.False(body.Success);
        Assert.Equal("Google login is not configured", body.Message);
    }

    [Fact]
    public async Task GoogleLogin_Returns401_WhenIdTokenIsInvalidJwt()
    {
        await using var context = CreateContext();
        var userRepo = new UserRepository(context);
        var sessionRepo = new InMemorySessionRepository();
        var emailService = new NoOpEmailService();
        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "super-secret-key-that-is-at-least-thirty-two-bytes-long",
            Issuer = "AISAM.Tests",
            Audience = "AISAM.Tests"
        });
        var googleSettings = Options.Create(new GoogleSettings { ClientId = "test-client-id.apps.googleusercontent.com" });

        var authService = new AuthService(userRepo, sessionRepo, emailService, jwtSettings, googleSettings);
        var controller = new AuthController(authService, NullLogger<AuthController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var response = await controller.GoogleLogin(new GoogleLoginRequest { IdToken = "not-a-valid-jwt-token" });

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
        var body = Assert.IsType<GenericResponse<object>>(unauthorizedResult.Value);
        Assert.False(body.Success);
        Assert.Contains("Invalid Google token", body.Message);
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
        public Task<Session?> FindByRefreshTokenAsync(string refreshToken) => Task.FromResult(_sessions.FirstOrDefault(s => s.RefreshToken == refreshToken));
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
