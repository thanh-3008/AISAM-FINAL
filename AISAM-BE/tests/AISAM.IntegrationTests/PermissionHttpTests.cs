using AISAM.API.Controllers;
using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Data;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AISAM.IntegrationTests;

public sealed class PermissionHttpTests
{
    [Fact]
    public async Task MemberTransfer_ChangesAccessVersionEvenWhenManagerBrandAndChannelsStaySame()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f); using var client = await ClientFor(f, host, f.Manager);
        using var before = System.Text.Json.JsonDocument.Parse(await client.GetStringAsync("/api/access/context"));
        var membership = await f.Db.TeamMembers.SingleAsync(m => m.UserId == f.OtherCreator.Id);
        membership.IsActive = false; f.Db.SaveChanges();
        using var after = System.Text.Json.JsonDocument.Parse(await client.GetStringAsync("/api/access/context"));
        Assert.NotEqual(before.RootElement.GetProperty("data").GetProperty("version").GetString(),
            after.RootElement.GetProperty("data").GetProperty("version").GetString());
    }

    private static AuthService Auth(PermissionSecurityTests.Fixture f) => new(new UserRepository(f.Db), new SessionRepository(f.Db), null!,
        Microsoft.Extensions.Options.Options.Create(new AISAM.Common.Config.JwtSettings { SecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)) }),
        Microsoft.Extensions.Options.Options.Create(new AISAM.Common.Config.GoogleSettings()));

    [Theory]
    [InlineData("logout")]
    [InlineData("logout-all")]
    [InlineData("inactive-user")]
    public async Task SessionLifecycle_ExistingSignedTokenDeniedAfterRealStateChange(string change)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f); using var client = await ClientFor(f, host, f.Creator);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/access/context")).StatusCode);
        var session = await f.Db.Sessions.SingleAsync();
        session.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)); f.Db.SaveChanges();
        if (change == "logout") await Auth(f).LogoutAsync(f.Creator.Id, session.RefreshToken);
        else if (change == "logout-all") await Auth(f).LogoutAllSessionsAsync(f.Creator.Id);
        else { f.Creator.IsActive = false; f.Db.SaveChanges(); }
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/access/context")).StatusCode);
    }

    [Fact]
    public async Task RefreshRotation_RevokesOldSignedSession_AndReuseRevokesReplacement()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f); using var client = await ClientFor(f, host, f.Creator);
        var session = await f.Db.Sessions.SingleAsync();
        session.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)); f.Db.SaveChanges();
        var auth = Auth(f); var response = await auth.RefreshTokenAsync(session.RefreshToken, null, null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/access/context")).StatusCode);
        Assert.True(await f.Db.Sessions.AnyAsync(s => s.Id != session.Id && s.IsActive));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.RefreshTokenAsync(session.RefreshToken, null, null));
        Assert.False(await f.Db.Sessions.AnyAsync(s => s.IsActive));
    }

    [Fact]
    public async Task RevokedTimestampAlone_PreventsRefreshEvenIfActiveFlagIsStale()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var session = new Session { UserId = f.Creator.Id, IsActive = true, RevokedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1), RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) };
        f.Db.Sessions.Add(session); f.Db.SaveChanges();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Auth(f).RefreshTokenAsync(session.RefreshToken, null, null));
    }

    [Fact]
    public async Task LegacySignedTokenWithoutSessionId_IsRejected()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Creator, includeSession: false);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/access/context")).StatusCode);
    }
    [Fact]
    public async Task TemporaryEdit_UsesRealMutationServiceAndImmediatelyDeniesRevocation()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var grant = await f.AddGrant(DateTime.UtcNow.AddHours(1));
        f.Db.SaveChanges();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Creator);
        using var edit = new StringContent("{\"title\":\"Updated with explicit edit grant\"}", System.Text.Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync($"/api/content/{f.OtherContent.Id}", edit)).StatusCode);
        Assert.Equal("Updated with explicit edit grant", await f.Db.Contents.AsNoTracking().Where(c => c.Id == f.OtherContent.Id).Select(c => c.Title).SingleAsync());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"/api/content/{f.OtherContent.Id}")).StatusCode);
        grant.RevokedAt = DateTime.UtcNow; f.Db.SaveChanges();
        using var deniedEdit = new StringContent("{\"title\":\"Must not persist\"}", System.Text.Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsync($"/api/content/{f.OtherContent.Id}", deniedEdit)).StatusCode);
        Assert.Equal("Updated with explicit edit grant", await f.Db.Contents.AsNoTracking().Where(c => c.Id == f.OtherContent.Id).Select(c => c.Title).SingleAsync());
    }

    [Theory]
    [InlineData("POST", "/api/content")]
    [InlineData("PUT", "/api/content/00000000-0000-0000-0000-000000000001")]
    [InlineData("DELETE", "/api/content/00000000-0000-0000-0000-000000000001/")]
    [InlineData("POST", "/api/content/00000000-0000-0000-0000-000000000001/submit")]
    [InlineData("POST", "/api/posts")]
    [InlineData("POST", "/api/content-schedules")]
    public async Task ViewerMutation_IsDeniedBeforeControllerExecution(string method, string path)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Viewer);
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json") };
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task ContentActions_ViewerCanReadSafeCapabilitiesWithoutAnalytics()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Viewer);
        var response = await client.GetAsync($"/api/access/content/{f.OwnContent.Id}/actions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement.GetProperty("data");
        Assert.True(data.GetProperty("View").GetBoolean());
        foreach (var item in data.EnumerateObject().Where(p => p.Name != "View")) Assert.False(item.Value.GetBoolean());
    }

    [Theory]
    [InlineData("/api/analytics/overview")]
    [InlineData("/api/analytics/overview/")]
    [InlineData("/API/ANALYTICS/overview")]
    [InlineData("/api/dashboard/summary/")]
    [InlineData("/api/access/me/analytics")]
    [InlineData("/api/credit-usage/")]
    public async Task ViewerCannotBypassAnalyticsThroughRouteVariants(string path)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Viewer);
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatorHistory_RejectsTamperedUserAndWorkspace()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Creator);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/access/creator-history/{f.Creator.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/access/creator-history/{f.OtherCreator.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/access/content/{f.OtherContent.Id}/analytics")).StatusCode);
        client.DefaultRequestHeaders.Remove("X-Workspace-Id");
        client.DefaultRequestHeaders.Add("X-Workspace-Id", f.OtherWorkspace.Id.ToString());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/access/creator-history/{f.Creator.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("revoked")]
    [InlineData("expired")]
    [InlineData("inactive")]
    public async Task SessionChange_InvalidatesExistingSignedToken(string change)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Creator);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/access/context")).StatusCode);
        var session = await f.Db.Sessions.SingleAsync();
        if (change == "revoked") session.RevokedAt = DateTime.UtcNow;
        if (change == "expired") session.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
        if (change == "inactive") session.IsActive = false;
        f.Db.SaveChanges();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/access/context")).StatusCode);
    }

    [Fact]
    public async Task WorkspaceRoleDowngrade_UsesDatabaseRoleImmediately()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        using var host = CreateServer(f);
        using var client = await ClientFor(f, host, f.Creator);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/access/me/analytics")).StatusCode);
        var membership = await f.Db.WorkspaceMembers.SingleAsync(m => m.UserId == f.Creator.Id);
        membership.Role = AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Viewer; f.Db.SaveChanges();
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/access/me/analytics")).StatusCode);
    }

    private static readonly SymmetricSecurityKey TestKey = new(RandomNumberGenerator.GetBytes(64));

    private static TestServer CreateServer(PermissionSecurityTests.Fixture fixture)
        => new(new WebHostBuilder().ConfigureServices(services =>
        {
            services.AddLogging(); services.AddRouting();
            services.AddScoped<AccessScope>();
            services.AddDbContext<AisamContext>(options => options.UseSqlite(fixture.Connection));
            services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ResourceAccessService>(); services.AddScoped<CollaborationAccessService>();
            services.AddScoped<ContentAuthorizationService>();
            services.AddScoped<IContentRepository, ContentRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISocialIntegrationRepository, SocialIntegrationRepository>();
            services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IContentCalendarRepository, ContentCalendarRepository>();
            services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
            services.AddScoped<IQuotaService, QuotaService>();
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
            services.AddScoped<ISocialTokenProtector, SocialTokenProtector>();
            services.AddScoped<IContentService, ContentService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, ValidateAudience = false, ValidateLifetime = true,
                    ValidateIssuerSigningKey = true, IssuerSigningKey = TestKey, ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents { OnTokenValidated = CurrentSessionValidation.ValidateAsync };
            });
            services.AddAuthorization();
            services.AddControllers().AddApplicationPart(typeof(AccessController).Assembly);
        }).Configure(app =>
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>(); app.UseRouting(); app.UseAuthentication();
            app.UseMiddleware<ActiveWorkspaceMiddleware>(); app.UseMiddleware<ResourceAccessMiddleware>();
            app.UseAuthorization(); app.UseEndpoints(endpoints => endpoints.MapControllers());
        }));

    private static Task<HttpClient> ClientFor(PermissionSecurityTests.Fixture fixture, TestServer server, User user, bool includeSession = true)
    {
        var session = new Session { UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddHours(1) };
        fixture.Db.Sessions.Add(session); fixture.Db.SaveChanges();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Role, user.Role.ToString()) };
        if (includeSession) claims.Add(new Claim("sid", session.Id.ToString()));
        var token = new JwtSecurityToken(claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(TestKey, SecurityAlgorithms.HmacSha256));
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        client.DefaultRequestHeaders.Add("X-Workspace-Id", fixture.Workspace.Id.ToString());
        return Task.FromResult(client);
    }
}
