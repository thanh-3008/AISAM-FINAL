using AISAM.API.Controllers;
using AISAM.Common.Dtos.Request;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class DeviceTokenRepositoryTests
{
    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    [Fact]
    public async Task RegisterOrUpdateAsync_CreatesNewDeviceToken()
    {
        await using var context = CreateContext();
        var repository = new DeviceTokenRepository(context);
        var profileId = Guid.NewGuid();

        var deviceToken = await repository.RegisterOrUpdateAsync(
            profileId, "fcm_test_token_123", "android", "Pixel 7");

        Assert.NotNull(deviceToken);
        Assert.Equal("fcm_test_token_123", deviceToken.Token);
        Assert.Equal(profileId, deviceToken.ProfileId);
        Assert.Equal("android", deviceToken.Platform);
        Assert.Equal("Pixel 7", deviceToken.DeviceName);
        Assert.True(deviceToken.IsActive);

        var count = await context.DeviceTokens.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task RegisterOrUpdateAsync_UpdatesExistingDeviceToken()
    {
        await using var context = CreateContext();
        var repository = new DeviceTokenRepository(context);
        var initialProfileId = Guid.NewGuid();
        var newProfileId = Guid.NewGuid();

        await repository.RegisterOrUpdateAsync(
            initialProfileId, "fcm_token_xyz", "android", "Galaxy S21");

        // Same token registers under a new profile (e.g. user switch on device)
        var updated = await repository.RegisterOrUpdateAsync(
            newProfileId, "fcm_token_xyz", "android", "Galaxy S21 Updated");

        Assert.Equal(newProfileId, updated.ProfileId);
        Assert.Equal("Galaxy S21 Updated", updated.DeviceName);
        Assert.True(updated.IsActive);

        var count = await context.DeviceTokens.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UnregisterAsync_MarksTokenAsInactive()
    {
        await using var context = CreateContext();
        var repository = new DeviceTokenRepository(context);
        var profileId = Guid.NewGuid();

        await repository.RegisterOrUpdateAsync(profileId, "fcm_active_token", "ios", "iPhone 15");

        var result = await repository.UnregisterAsync("fcm_active_token");
        Assert.True(result);

        var tokenInDb = await context.DeviceTokens.FirstAsync(d => d.Token == "fcm_active_token");
        Assert.False(tokenInDb.IsActive);

        var activeTokens = await repository.GetActiveTokensByProfileIdAsync(profileId);
        Assert.Empty(activeTokens);
    }

    [Fact]
    public async Task GetActiveTokensByProfileIdAsync_ReturnsOnlyActiveTokens()
    {
        await using var context = CreateContext();
        var repository = new DeviceTokenRepository(context);
        var profileId = Guid.NewGuid();

        await repository.RegisterOrUpdateAsync(profileId, "token_1", "android", "Device 1");
        await repository.RegisterOrUpdateAsync(profileId, "token_2", "android", "Device 2");
        await repository.UnregisterAsync("token_1");

        var activeTokens = await repository.GetActiveTokensByProfileIdAsync(profileId);
        Assert.Single(activeTokens);
        Assert.Equal("token_2", activeTokens[0].Token);
    }

    [Fact]
    public async Task DeactivateTokensAsync_MarksInvalidTokensInactive()
    {
        await using var context = CreateContext();
        var repository = new DeviceTokenRepository(context);
        var profileId = Guid.NewGuid();

        await repository.RegisterOrUpdateAsync(profileId, "dead_token_1", "android", "Old phone");
        await repository.RegisterOrUpdateAsync(profileId, "dead_token_2", "ios", "Old iPad");
        await repository.RegisterOrUpdateAsync(profileId, "valid_token", "android", "New phone");

        await repository.DeactivateTokensAsync(new[] { "dead_token_1", "dead_token_2" });

        var active = await repository.GetActiveTokensByProfileIdAsync(profileId);
        Assert.Single(active);
        Assert.Equal("valid_token", active[0].Token);
    }

    [Fact]
    public async Task CreateAndPushAsync_CreatesNotificationAndInvokesPush()
    {
        await using var context = CreateContext();
        var notificationRepo = new NotificationRepository(context);
        var fakePushService = new FakePushNotificationService();
        var notificationService = new NotificationService(notificationRepo, null, fakePushService);

        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();

        var result = await notificationService.CreateAndPushAsync(
            profileId,
            workspaceId,
            "Approval Needed",
            "Please review the post.",
            NotificationTypeEnum.ApprovalNeeded);

        Assert.True(result.Success);

        var count = await notificationRepo.GetUnreadCountAsync(profileId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task NotificationsController_RegisterAndUnregisterEndpoints_WorkCorrectly()
    {
        await using var context = CreateContext();
        var deviceTokenRepo = new DeviceTokenRepository(context);
        var notificationRepo = new NotificationRepository(context);
        var notificationService = new NotificationService(notificationRepo);

        var profileId = Guid.NewGuid();
        var controller = new NotificationsController(notificationService, null, deviceTokenRepo);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ActiveProfileId"] = profileId;
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("profile_id", profileId.ToString())
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // 1. Register device
        var registerResult = await controller.RegisterDevice(new RegisterDeviceTokenRequest
        {
            Token = "ctrl_test_token",
            Platform = "android",
            DeviceName = "Test Emulator"
        });

        var okResult = Assert.IsType<OkObjectResult>(registerResult.Result);
        Assert.NotNull(okResult.Value);

        var activeTokens = await deviceTokenRepo.GetActiveTokensByProfileIdAsync(profileId);
        Assert.Single(activeTokens);
        Assert.Equal("ctrl_test_token", activeTokens[0].Token);

        // 2. Unregister device
        var unregisterResult = await controller.UnregisterDevice(new UnregisterDeviceTokenRequest
        {
            Token = "ctrl_test_token"
        });

        var unregOk = Assert.IsType<OkObjectResult>(unregisterResult.Result);
        Assert.NotNull(unregOk.Value);

        var remainingActive = await deviceTokenRepo.GetActiveTokensByProfileIdAsync(profileId);
        Assert.Empty(remainingActive);
    }

    private sealed class FakePushNotificationService : IPushNotificationService
    {
        public List<(Guid ProfileId, string Title, string Message)> SentNotifications { get; } = new();

        public Task SendNotificationAsync(
            Guid profileId,
            string title,
            string message,
            IDictionary<string, string>? data = null,
            CancellationToken cancellationToken = default)
        {
            SentNotifications.Add((profileId, title, message));
            return Task.CompletedTask;
        }

        public Task SendNotificationToProfilesAsync(
            IEnumerable<Guid> profileIds,
            string title,
            string message,
            IDictionary<string, string>? data = null,
            CancellationToken cancellationToken = default)
        {
            foreach (var pid in profileIds)
            {
                SentNotifications.Add((pid, title, message));
            }
            return Task.CompletedTask;
        }
    }
}
