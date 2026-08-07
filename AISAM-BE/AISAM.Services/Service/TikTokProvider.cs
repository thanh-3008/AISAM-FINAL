using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
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

        var grantedScopes = token.Scope
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingScopes = _settings.RequiredScopes
            .Where(scope => !grantedScopes.Contains(scope))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (missingScopes.Count > 0)
        {
            throw new InvalidOperationException(
                $"TikTok authorization is missing required scope(s): {string.Join(", ", missingScopes)}. " +
                "Enable the scopes in TikTok Developer Portal, then disconnect and reconnect TikTok.");
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

    public async Task<PublishResultDto> PublishAsync(
        SocialAccount account,
        SocialIntegration integration,
        PostDto post,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        if (string.IsNullOrWhiteSpace(post.VideoUrl))
        {
            return Failure("TikTok Direct Post currently requires a video.");
        }

        try
        {
            var creator = await GetCreatorInfoAsync(integration.AccessToken, cancellationToken);
            var privacyLevel = _settings.DefaultPrivacyLevel.Trim().ToUpperInvariant();
            if (!creator.PrivacyLevelOptions.Contains(privacyLevel, StringComparer.Ordinal))
            {
                return Failure($"TikTok account does not allow the configured privacy level {privacyLevel}.");
            }

            var video = await DownloadVideoAsync(post.VideoUrl, cancellationToken);
            var upload = await InitializeVideoPostAsync(
                integration.AccessToken,
                post.Message,
                privacyLevel,
                creator,
                video.Bytes.LongLength,
                cancellationToken);

            await UploadVideoAsync(upload, video, cancellationToken);
            _logger.LogInformation("TikTok Direct Post accepted with publish id {PublishId}.", upload.PublishId);
            return new PublishResultDto
            {
                Success = true,
                ProviderPostId = upload.PublishId,
                PostedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or JsonException)
        {
            _logger.LogWarning(ex, "TikTok Direct Post failed.");
            return Failure(ex.Message);
        }
    }

    private async Task<TikTokCreatorInfo> GetCreatorInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_settings.ApiBaseUrl.TrimEnd('/')}/v2/post/publish/creator_info/query/");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureTikTokSuccess(response, content, "TikTok creator information request failed.");
        return Deserialize<TikTokCreatorInfoResponse>(content).Data
            ?? throw new InvalidOperationException("TikTok did not return creator information.");
    }

    private async Task<TikTokVideo> DownloadVideoAsync(string videoUrl, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("TikTok video URL must be an absolute HTTPS URL.");
        }

        if (!_settings.AllowedMediaHosts.Any(host => HostMatches(uri.Host, host)))
        {
            throw new InvalidOperationException($"TikTok video host '{uri.Host}' is not allowed.");
        }

        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant() ?? "";
        if (contentType is not ("video/mp4" or "video/quicktime" or "video/webm"))
        {
            throw new InvalidOperationException("TikTok only accepts MP4, QuickTime, or WebM video files.");
        }

        var maxBytes = Math.Max(1, _settings.MaxUploadSizeMb) * 1024L * 1024L;
        if (response.Content.Headers.ContentLength > maxBytes)
        {
            throw new InvalidOperationException($"TikTok video exceeds the configured {_settings.MaxUploadSizeMb} MB limit.");
        }

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var destination = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;
            if (destination.Length + read > maxBytes)
            {
                throw new InvalidOperationException($"TikTok video exceeds the configured {_settings.MaxUploadSizeMb} MB limit.");
            }
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        if (destination.Length == 0)
        {
            throw new InvalidOperationException("TikTok video is empty.");
        }

        return new TikTokVideo(destination.ToArray(), contentType);
    }

    private async Task<TikTokUploadSession> InitializeVideoPostAsync(
        string accessToken,
        string message,
        string privacyLevel,
        TikTokCreatorInfo creator,
        long videoSize,
        CancellationToken cancellationToken)
    {
        const long maxChunkSize = 64L * 1024L * 1024L;
        var chunkSize = Math.Min(videoSize, maxChunkSize);
        var totalChunkCount = Math.Max(1, videoSize / maxChunkSize);
        var payload = new
        {
            post_info = new
            {
                title = Truncate(message, 2200),
                privacy_level = privacyLevel,
                disable_duet = creator.DuetDisabled,
                disable_comment = creator.CommentDisabled,
                disable_stitch = creator.StitchDisabled,
                brand_content_toggle = false,
                brand_organic_toggle = false
            },
            source_info = new
            {
                source = "FILE_UPLOAD",
                video_size = videoSize,
                chunk_size = chunkSize,
                total_chunk_count = totalChunkCount
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_settings.ApiBaseUrl.TrimEnd('/')}/v2/post/publish/video/init/");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureTikTokSuccess(response, content, "TikTok video initialization failed.");
        var data = Deserialize<TikTokPublishResponse>(content).Data;
        if (string.IsNullOrWhiteSpace(data?.PublishId) || string.IsNullOrWhiteSpace(data.UploadUrl))
        {
            throw new InvalidOperationException("TikTok did not return a publish id and upload URL.");
        }
        return new TikTokUploadSession(data.PublishId, data.UploadUrl, chunkSize, totalChunkCount);
    }

    private async Task UploadVideoAsync(TikTokUploadSession upload, TikTokVideo video, CancellationToken cancellationToken)
    {
        var totalSize = video.Bytes.LongLength;
        var offset = 0;
        for (var chunkIndex = 0L; chunkIndex < upload.TotalChunkCount; chunkIndex++)
        {
            var remaining = video.Bytes.Length - offset;
            var length = chunkIndex == upload.TotalChunkCount - 1
                ? remaining
                : checked((int)upload.ChunkSize);
            using var request = new HttpRequestMessage(HttpMethod.Put, upload.UploadUrl);
            request.Content = new ByteArrayContent(video.Bytes, offset, length);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(video.ContentType);
            request.Content.Headers.ContentRange = new ContentRangeHeaderValue(offset, offset + length - 1, totalSize);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(GetErrorMessage(content, "TikTok video upload failed."));
            }
            offset += length;
        }
    }

    private static bool HostMatches(string actualHost, string allowedHost)
    {
        var normalized = allowedHost.Trim().TrimStart('.');
        return actualHost.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
               actualHost.EndsWith($".{normalized}", StringComparison.OrdinalIgnoreCase);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static PublishResultDto Failure(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };

    private static void EnsureTikTokSuccess(HttpResponseMessage response, string content, string fallback)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(content, fallback));
        }

        var envelope = Deserialize<TikTokErrorEnvelope>(content);
        if (!string.IsNullOrWhiteSpace(envelope.Error?.Code) &&
            !string.Equals(envelope.Error.Code, "ok", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(envelope.Error.Message ?? fallback);
        }
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

    public Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad account management.");

    public Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API campaign creation.");

    public Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad set creation.");

    public Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, string? objectStoryId = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad creative creation.");

    public Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad creation.");

    public Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API campaign insights.");

    public Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API campaign status updates.");

    public Task<bool> UpdateCampaignNameAsync(string adAccountId, string userAccessToken, string campaignId, string name, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API campaign name updates.");

    public Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad set status updates.");

    public Task<bool> UpdateAdSetBudgetAsync(string adAccountId, string userAccessToken, string adSetId, decimal dailyBudget, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad set budget updates.");

    public Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad status updates.");

    public Task<string?> GetAdEffectiveStatusAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);

    public Task<string?> GetAdSetEffectiveStatusAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);

    public Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API campaign deletion.");

    public Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad set deletion.");

    public Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad creative deletion.");

    public Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("TikTok does not support marketing API ad deletion.");

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
        [JsonPropertyName("scope")] public string Scope { get; set; } = string.Empty;
    }

    private sealed class TikTokUserInfoResponse
    {
        public TikTokUserData? Data { get; set; }
        public TikTokApiError? Error { get; set; }
    }

    private sealed class TikTokUserData { public TikTokUser? User { get; set; } }

    private sealed class TikTokCreatorInfoResponse
    {
        public TikTokCreatorInfo? Data { get; set; }
        public TikTokApiError? Error { get; set; }
    }

    private sealed class TikTokCreatorInfo
    {
        [JsonPropertyName("privacy_level_options")]
        public List<string> PrivacyLevelOptions { get; set; } = new();

        [JsonPropertyName("comment_disabled")]
        public bool CommentDisabled { get; set; }

        [JsonPropertyName("duet_disabled")]
        public bool DuetDisabled { get; set; }

        [JsonPropertyName("stitch_disabled")]
        public bool StitchDisabled { get; set; }
    }

    private sealed class TikTokPublishResponse
    {
        public TikTokPublishData? Data { get; set; }
        public TikTokApiError? Error { get; set; }
    }

    private sealed class TikTokPublishData
    {
        [JsonPropertyName("publish_id")]
        public string PublishId { get; set; } = string.Empty;

        [JsonPropertyName("upload_url")]
        public string UploadUrl { get; set; } = string.Empty;
    }

    private sealed record TikTokVideo(byte[] Bytes, string ContentType);
    private sealed record TikTokUploadSession(
        string PublishId,
        string UploadUrl,
        long ChunkSize,
        long TotalChunkCount);

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

    public Task<FacebookPostInsightData?> GetPostInsightsAsync(string accessToken, string postId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<FacebookPostInsightData?>(null);
    }
}
