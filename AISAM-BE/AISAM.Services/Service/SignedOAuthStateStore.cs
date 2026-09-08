using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AISAM.Services.IServices;
using Microsoft.Extensions.Caching.Memory;

namespace AISAM.Services.Service;

public sealed class SignedOAuthStateStore : IOAuthStateStore
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);
    private readonly byte[] _signingKey;
    private readonly IMemoryCache? _cache;

    public SignedOAuthStateStore(string signingSecret, IMemoryCache? cache = null)
    {
        if (string.IsNullOrWhiteSpace(signingSecret))
        {
            throw new ArgumentException("OAuth state signing secret is required.", nameof(signingSecret));
        }

        _signingKey = Encoding.UTF8.GetBytes(signingSecret);
        _cache = cache;
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
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        var parts = state.Split('.', 2);
        if (parts.Length != 2)
        {
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        var payloadPart = parts[0];
        var signaturePart = parts[1];
        var expectedSignaturePart = Sign(payloadPart);

        if (!FixedTimeEquals(signaturePart, expectedSignaturePart))
        {
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        try
        {
            var payloadBytes = Base64UrlDecode(payloadPart);
            var payload = JsonSerializer.Deserialize<OAuthStatePayload>(payloadBytes);
            if (payload == null ||
                payload.ExpiresAtUtc <= DateTime.UtcNow ||
                payload.ProfileId != profileId ||
                !string.Equals(payload.Provider, NormalizeProvider(provider), StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(payload.State))
            {
                return Task.FromResult<OAuthStatePayload?>(null);
            }

            if (_cache != null)
            {
                var cacheKey = $"oauth-consumed:{payload.State}";
                if (_cache.TryGetValue(cacheKey, out _))
                {
                    // State already consumed once; reject replay attempt
                    return Task.FromResult<OAuthStatePayload?>(null);
                }

                _cache.Set(cacheKey, true, payload.ExpiresAtUtc);
            }

            return Task.FromResult<OAuthStatePayload?>(payload);
        }
        catch
        {
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
