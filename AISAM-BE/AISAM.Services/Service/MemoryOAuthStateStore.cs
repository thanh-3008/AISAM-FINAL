using AISAM.Services.IServices;
using Microsoft.Extensions.Caching.Memory;

namespace AISAM.Services.Service;

public sealed class MemoryOAuthStateStore : IOAuthStateStore
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);
    private readonly IMemoryCache _cache;

    public MemoryOAuthStateStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> CreateAsync(Guid profileId, string provider, CancellationToken cancellationToken = default)
    {
        var state = Guid.NewGuid().ToString("N");
        var payload = new OAuthStatePayload
        {
            State = state,
            ProfileId = profileId,
            Provider = provider,
            ExpiresAtUtc = DateTime.UtcNow.Add(Expiration)
        };

        _cache.Set(GetKey(state), payload, payload.ExpiresAtUtc);
        return Task.FromResult(state);
    }

    public Task<OAuthStatePayload?> ConsumeAsync(string state, Guid profileId, string provider, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetKey(state), out OAuthStatePayload? payload))
        {
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        _cache.Remove(GetKey(state));

        if (payload == null ||
            payload.ExpiresAtUtc <= DateTime.UtcNow ||
            payload.ProfileId != profileId ||
            !string.Equals(payload.Provider, provider, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        return Task.FromResult<OAuthStatePayload?>(payload);
    }

    private static string GetKey(string state) => $"oauth-state:{state}";
}
