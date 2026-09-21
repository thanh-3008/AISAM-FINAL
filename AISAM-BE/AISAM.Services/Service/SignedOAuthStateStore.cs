using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AISAM.Services.IServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class SignedOAuthStateStore : IOAuthStateStore
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);
    private readonly byte[] _signingKey;
    private readonly IMemoryCache? _cache;
    private readonly ILogger<SignedOAuthStateStore>? _logger;

    public SignedOAuthStateStore(string signingSecret, IMemoryCache? cache = null, ILogger<SignedOAuthStateStore>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(signingSecret))
        {
            throw new ArgumentException("OAuth state signing secret is required.", nameof(signingSecret));
        }

        _signingKey = Encoding.UTF8.GetBytes(signingSecret);
        _cache = cache;
        _logger = logger;
    }

    public Task<string> CreateAsync(Guid profileId, string provider, string? origin = null, string? redirectUri = null, CancellationToken cancellationToken = default)
    {
        var payload = new OAuthStatePayload
        {
            State = Guid.NewGuid().ToString("N"),
            ProfileId = profileId,
            Provider = NormalizeProvider(provider),
            Origin = origin,
            RedirectUri = redirectUri,
            ExpiresAtUtc = DateTime.UtcNow.Add(Expiration)
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadPart = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var signaturePart = Sign(payloadPart);

        return Task.FromResult($"{payloadPart}.{signaturePart}");
    }

    public Task<OAuthStatePayload?> ConsumeAsync(string state, Guid profileId, string provider, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            _logger?.LogWarning("OAuth state validation failed: state parameter is null or empty for provider {Provider}.", provider);
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        var parts = state.Split('.', 2);
        if (parts.Length != 2)
        {
            _logger?.LogWarning("OAuth state validation failed: state format invalid (missing signature dot) for provider {Provider}.", provider);
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        var payloadPart = parts[0];
        var signaturePart = parts[1];
        var expectedSignaturePart = Sign(payloadPart);

        if (!FixedTimeEquals(signaturePart, expectedSignaturePart))
        {
            _logger?.LogWarning("OAuth state validation failed: HMAC signature mismatch for provider {Provider}.", provider);
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        try
        {
            var payloadBytes = Base64UrlDecode(payloadPart);
            var payload = JsonSerializer.Deserialize<OAuthStatePayload>(payloadBytes);
            if (payload == null)
            {
                _logger?.LogWarning("OAuth state validation failed: deserialized payload is null for provider {Provider}.", provider);
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            var shortStateId = payload.State?.Length >= 8 ? payload.State[..8] : (payload.State ?? "unknown");

            if (payload.ExpiresAtUtc <= DateTime.UtcNow)
            {
                _logger?.LogWarning("OAuth state validation failed: state {StateId} expired at {ExpiresAtUtc} (current UTC: {CurrentUtc}) for provider {Provider}.",
                    shortStateId, payload.ExpiresAtUtc, DateTime.UtcNow, provider);
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            if (payload.ProfileId != profileId)
            {
                _logger?.LogWarning("OAuth state validation failed: ProfileId mismatch for state {StateId}. Expected {ExpectedProfileId}, actual {ActualProfileId} for provider {Provider}.",
                    shortStateId, payload.ProfileId, profileId, provider);
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            if (!string.Equals(payload.Provider, NormalizeProvider(provider), StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogWarning("OAuth state validation failed: Provider mismatch for state {StateId}. Expected {ExpectedProvider}, actual {ActualProvider}.",
                    shortStateId, payload.Provider, provider);
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            if (string.IsNullOrWhiteSpace(payload.State))
            {
                _logger?.LogWarning("OAuth state validation failed: State ID is empty for provider {Provider}.", provider);
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            if (_cache != null)
            {
                var cacheKey = $"oauth-consumed:{payload.State}";
                if (_cache.TryGetValue(cacheKey, out _))
                {
                    // State already consumed once; reject replay attempt
                    _logger?.LogWarning("OAuth state validation failed: Replay detected for state {StateId} and provider {Provider}.", shortStateId, provider);
                    return Task.FromResult<OAuthStatePayload?>(null);
                }

                _cache.Set(cacheKey, true, payload.ExpiresAtUtc);
            }

            return Task.FromResult<OAuthStatePayload?>(payload);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "OAuth state validation failed: exception during state decode/deserialize for provider {Provider}.", provider);
            return Task.FromResult<OAuthStatePayload?>(null);
        }
    }

    public string? TryPeekOrigin(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return null;
        }

        var parts = state.Split('.', 2);
        if (parts.Length != 2)
        {
            return null;
        }

        var payloadPart = parts[0];
        var signaturePart = parts[1];
        var expectedSignaturePart = Sign(payloadPart);

        if (!FixedTimeEquals(signaturePart, expectedSignaturePart))
        {
            return null;
        }

        try
        {
            var payloadBytes = Base64UrlDecode(payloadPart);
            var payload = JsonSerializer.Deserialize<OAuthStatePayload>(payloadBytes);
            if (payload == null || payload.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return null;
            }

            return payload.Origin;
        }
        catch
        {
            return null;
        }
    }

    private string Sign(string payloadPart)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadPart)));
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
            CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string NormalizeProvider(string provider)
        => provider.Trim().ToLowerInvariant();

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var padding = base64.Length % 4;
        if (padding > 0)
        {
            base64 = base64.PadRight(base64.Length + 4 - padding, '=');
        }

        return Convert.FromBase64String(base64);
    }
}
