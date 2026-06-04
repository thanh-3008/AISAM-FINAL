using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.Extensions.Caching.Memory;

namespace AISAM.IntegrationTests;

public class OAuthStateStoreTests
{
    [Fact]
    public async Task CreateAsync_ThenConsumeAsync_ReturnsStoredProfileAndProvider_OnceOnly()
    {
        var store = new MemoryOAuthStateStore(new MemoryCache(new MemoryCacheOptions()));
        var profileId = Guid.NewGuid();

        var state = await store.CreateAsync(profileId, "facebook");
        var payload = await store.ConsumeAsync(state, profileId, "facebook");
        var secondRead = await store.ConsumeAsync(state, profileId, "facebook");

        Assert.NotNull(payload);
        Assert.Equal(profileId, payload!.ProfileId);
        Assert.Equal("facebook", payload.Provider);
        Assert.Null(secondRead);
    }

    [Fact]
    public async Task ConsumeAsync_ReturnsNull_WhenStateExpired()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var state = Guid.NewGuid().ToString("N");
        var profileId = Guid.NewGuid();
        cache.Set($"oauth-state:{state}", new OAuthStatePayload
        {
            State = state,
            ProfileId = profileId,
            Provider = "facebook",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1)
        });

        var store = new MemoryOAuthStateStore(cache);

        var payload = await store.ConsumeAsync(state, profileId, "facebook");

        Assert.Null(payload);
    }

    [Fact]
    public async Task ConsumeAsync_ReturnsNull_WhenProfileIdDoesNotMatch()
    {
        var store = new MemoryOAuthStateStore(new MemoryCache(new MemoryCacheOptions()));
        var state = await store.CreateAsync(Guid.NewGuid(), "facebook");

        var payload = await store.ConsumeAsync(state, Guid.NewGuid(), "facebook");

        Assert.Null(payload);
    }
}
