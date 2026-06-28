using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AISAM.Services.Service;

public sealed class TikTokProvider : IProviderService
{
    private readonly HttpClient _httpClient;
    private readonly TikTokSettings _settings;
    private readonly ILogger<TikTokProvider> _logger;

    public string ProviderName => "tiktok";

    public TikTokProvider(HttpClient httpClient, IOptions<TikTokSettings> settings, ILogger<TikTokProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var scopes = string.Join(',', _settings.RequiredScopes.Distinct(StringComparer.Ordinal));
        var url = $"{_settings.OAuthUrl.TrimEnd('/')}/" +
                  $"?client_key={Uri.EscapeDataString(_settings.ClientKey)}" +
                  $"&scope={Uri.EscapeDataString(scopes)}" +
                  "&response_type=code" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&state={Uri.EscapeDataString(state)}";
        return Task.FromResult(url);
    }

    public async Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        using var response = await _httpClient.PostAsync(
            $"{_settings.ApiBaseUrl.TrimEnd('/')}/v2/oauth/token/",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_key"] = _settings.ClientKey,
                ["client_secret"] = _settings.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirectUri
            }),
            cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(content, "TikTok token exchange failed."));
        }

        var token = Deserialize<TikTokTokenResponse>(content);
        if (string.IsNullOrWhiteSpace(token.AccessToken) || string.IsNullOrWhiteSpace(token.OpenId))
        {
            throw new InvalidOperationException(GetErrorMessage(content, "TikTok did not return an access token."));
        }

        // Validate the token and account identity before persisting it.
        var profile = await GetUserInfoAsync(token.AccessToken, cancellationToken);
        var providerUserId = string.IsNullOrWhiteSpace(profile.OpenId) ? token.OpenId : profile.OpenId;

        return new SocialAccountDto
        {
            Provider = ProviderName,
            ProviderUserId = providerUserId,
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            IsActive = true,
            ExpiresAt = token.ExpiresIn > 0 ? DateTime.UtcNow.AddSeconds(token.ExpiresIn) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var user = await GetUserInfoAsync(accessToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(user.OpenId))
        {
            return Array.Empty<AvailableTargetDto>();
        }

        return new[]
        {
            new AvailableTargetDto
            {
                ProviderTargetId = user.OpenId,
                Name = string.IsNullOrWhiteSpace(user.DisplayName) ? user.OpenId : user.DisplayName,
                Type = "tiktok_account",
                ProfilePictureUrl = user.AvatarUrl,
                IsActive = true
            }
        };
    }

    public async Task<Dictionary<string, string>> GetTargetAccessTokensAsync(
        string userAccessToken,
        IEnumerable<string> providerTargetIds,
        CancellationToken cancellationToken = default)
    {
        var user = await GetUserInfoAsync(userAccessToken, cancellationToken);
        var requested = providerTargetIds.ToHashSet(StringComparer.Ordinal);
        return !string.IsNullOrWhiteSpace(user.OpenId) && requested.Contains(user.OpenId)
            ? new Dictionary<string, string>(StringComparer.Ordinal) { [user.OpenId] = userAccessToken }
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public Task<PublishResultDto> PublishAsync(
        SocialAccount account,
        SocialIntegration integration,
        PostDto post,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("TikTok publishing was requested before Content Posting API was enabled.");
        return Task.FromResult(new PublishResultDto
        {
            Success = false,
            ErrorMessage = "TikTok Content Posting API is not enabled for this application."
        });
    }

    private async Task<TikTokUser> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_settings.ApiBaseUrl.TrimEnd('/')}/v2/user/info/?fields=open_id,display_name,avatar_url");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(content, "TikTok user profile request failed."));
        }

        var result = Deserialize<TikTokUserInfoResponse>(content);
        if (!string.IsNullOrWhiteSpace(result.Error?.Code) && !string.Equals(result.Error.Code, "ok", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(result.Error.Message ?? "TikTok user profile request failed.");
        }
        return result.Data?.User ?? new TikTokUser();
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientKey) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret) ||
            string.IsNullOrWhiteSpace(_settings.RedirectUri))
        {
            throw new InvalidOperationException("TikTok integration is not configured.");
        }
    }

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("Failed to parse TikTok response.");

    private static string GetErrorMessage(string content, string fallback)
    {
        try
        {
            var error = JsonSerializer.Deserialize<TikTokErrorEnvelope>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return error?.ErrorDescription ?? error?.Error?.Message ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private sealed class TikTokTokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
        [JsonPropertyName("open_id")] public string OpenId { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")] public long ExpiresIn { get; set; }
    }

    private sealed class TikTokUserInfoResponse
    {
        public TikTokUserData? Data { get; set; }
        public TikTokApiError? Error { get; set; }
    }

    private sealed class TikTokUserData { public TikTokUser? User { get; set; } }
    private sealed class TikTokUser
    {
        [JsonPropertyName("open_id")] public string OpenId { get; set; } = string.Empty;
        [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
        [JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; }
    }
    private sealed class TikTokApiError { public string? Code { get; set; } public string? Message { get; set; } }
    private sealed class TikTokErrorEnvelope
    {
        [JsonPropertyName("error_description")] public string? ErrorDescription { get; set; }
        public TikTokApiError? Error { get; set; }
    }
}
