using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AISAM.Services.Service;

public sealed class InstagramProvider : IProviderService
{
    private readonly HttpClient _httpClient;
    private readonly InstagramSettings _settings;
    public string ProviderName => "instagram";

    public InstagramProvider(HttpClient httpClient, IOptions<InstagramSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var scope = string.Join(",", _settings.RequiredPermissions.Distinct());
        return Task.FromResult($"{_settings.OAuthUrl}/{_settings.GraphApiVersion}/dialog/oauth" +
            $"?client_id={_settings.AppId}&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(scope)}&response_type=code&state={Uri.EscapeDataString(state)}");
    }

    public async Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var token = await GetAsync<TokenResponse>($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/oauth/access_token" +
            $"?client_id={_settings.AppId}&client_secret={_settings.AppSecret}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}&code={Uri.EscapeDataString(code)}", cancellationToken);
        if (string.IsNullOrWhiteSpace(token.AccessToken))
            throw new InvalidOperationException("Failed to obtain Instagram access token.");

        var user = await GetAsync<UserResponse>($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me" +
            $"?fields=id&access_token={Uri.EscapeDataString(token.AccessToken)}", cancellationToken);
        if (string.IsNullOrWhiteSpace(user.Id))
            throw new InvalidOperationException("Failed to obtain Meta user for Instagram.");

        return new SocialAccountDto
        {
            Provider = ProviderName,
            ProviderUserId = user.Id,
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(token.ExpiresIn.Value) : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var pages = await GetPagesAsync(accessToken, cancellationToken);
        return pages.Where(page => !string.IsNullOrWhiteSpace(page.InstagramBusinessAccount?.Id))
            .Select(page => new AvailableTargetDto
            {
                ProviderTargetId = page.InstagramBusinessAccount!.Id!,
                Name = page.InstagramBusinessAccount.Username ?? page.Name ?? "Instagram account",
                Type = "instagram_business_account",
                Category = page.Category,
                ProfilePictureUrl = page.InstagramBusinessAccount.ProfilePictureUrl,
                IsActive = true
            }).ToList();
    }

    public async Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default)
    {
        var ids = providerTargetIds.ToHashSet(StringComparer.Ordinal);
        var pages = await GetPagesAsync(userAccessToken, cancellationToken);
        return pages.Where(page => page.InstagramBusinessAccount?.Id is not null &&
                                   !string.IsNullOrWhiteSpace(page.AccessToken) &&
                                   ids.Contains(page.InstagramBusinessAccount.Id))
            .ToDictionary(page => page.InstagramBusinessAccount!.Id!, page => page.AccessToken!, StringComparer.Ordinal);
    }

    public async Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        if (string.IsNullOrWhiteSpace(integration.ExternalId) || string.IsNullOrWhiteSpace(integration.AccessToken))
            return Failed("Instagram account is not linked correctly. Please reconnect it.");

        var images = (post.ImageUrls ?? new List<string>())
            .Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList();
        if (!string.IsNullOrWhiteSpace(post.ImageUrl) && images.Count == 0)
            images.Add(post.ImageUrl);

        if (!string.IsNullOrWhiteSpace(post.VideoUrl) && images.Count > 0)
            return Failed("An Instagram post cannot mix a Reel video with images.");
        if (string.IsNullOrWhiteSpace(post.VideoUrl) && images.Count == 0)
            return Failed("Instagram requires an image, carousel, or video.");
        if (images.Count > 10)
            return Failed("An Instagram carousel supports at most 10 images.");

        if (!string.IsNullOrWhiteSpace(post.VideoUrl))
            return await PublishReelAsync(integration, post, cancellationToken);
        if (images.Count > 1)
            return await PublishCarouselAsync(integration, post.Message, images, cancellationToken);
        return await PublishImageAsync(integration, post.Message, images[0], cancellationToken);
    }

    private async Task<PublishResultDto> PublishImageAsync(SocialIntegration integration, string caption, string imageUrl, CancellationToken cancellationToken)
    {
        var container = await CreateContainerAsync(integration, new Dictionary<string, string>
        {
            ["image_url"] = imageUrl,
            ["caption"] = caption
        }, cancellationToken);
        return container.Success
            ? await PublishContainerAsync(integration, container.Id!, cancellationToken)
            : Failed(container.Error!);
    }

    private async Task<PublishResultDto> PublishReelAsync(SocialIntegration integration, PostDto post, CancellationToken cancellationToken)
    {
        var container = await CreateContainerAsync(integration, new Dictionary<string, string>
        {
            ["media_type"] = "REELS",
            ["video_url"] = post.VideoUrl!,
            ["caption"] = post.Message,
            ["share_to_feed"] = "true"
        }, cancellationToken);
        if (!container.Success) return Failed(container.Error!);

        var ready = await WaitForContainerAsync(integration, container.Id!, cancellationToken);
        return ready.Success
            ? await PublishContainerAsync(integration, container.Id!, cancellationToken)
            : Failed(ready.Error!);
    }

    private async Task<PublishResultDto> PublishCarouselAsync(SocialIntegration integration, string caption, IReadOnlyList<string> imageUrls, CancellationToken cancellationToken)
    {
        var children = new List<string>();
        foreach (var imageUrl in imageUrls)
        {
            var child = await CreateContainerAsync(integration, new Dictionary<string, string>
            {
                ["image_url"] = imageUrl,
                ["is_carousel_item"] = "true"
            }, cancellationToken);
            if (!child.Success) return Failed(child.Error!);
            children.Add(child.Id!);
        }

        var parent = await CreateContainerAsync(integration, new Dictionary<string, string>
        {
            ["media_type"] = "CAROUSEL",
            ["children"] = string.Join(',', children),
            ["caption"] = caption
        }, cancellationToken);
        return parent.Success
            ? await PublishContainerAsync(integration, parent.Id!, cancellationToken)
            : Failed(parent.Error!);
    }

    private async Task<ApiResult> CreateContainerAsync(SocialIntegration integration, Dictionary<string, string> fields, CancellationToken cancellationToken)
    {
        fields["access_token"] = integration.AccessToken;
        return await PostAsync($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{integration.ExternalId}/media", fields, cancellationToken);
    }

    private async Task<PublishResultDto> PublishContainerAsync(SocialIntegration integration, string creationId, CancellationToken cancellationToken)
    {
        var result = await PostAsync($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{integration.ExternalId}/media_publish",
            new Dictionary<string, string> { ["creation_id"] = creationId, ["access_token"] = integration.AccessToken }, cancellationToken);
        return result.Success
            ? new PublishResultDto { Success = true, ProviderPostId = result.Id, PostedAt = DateTime.UtcNow }
            : Failed(result.Error!);
    }

    private async Task<ApiResult> WaitForContainerAsync(SocialIntegration integration, string containerId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var status = await GetAsync<ContainerStatus>($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{containerId}" +
                $"?fields=status_code,status&access_token={Uri.EscapeDataString(integration.AccessToken)}", cancellationToken);
            if (string.Equals(status.StatusCode, "FINISHED", StringComparison.OrdinalIgnoreCase)) return ApiResult.Ok(containerId);
            if (string.Equals(status.StatusCode, "ERROR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status.StatusCode, "EXPIRED", StringComparison.OrdinalIgnoreCase))
                return ApiResult.Fail(status.Status ?? $"Instagram container status is {status.StatusCode}.");
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
        return ApiResult.Fail("Instagram timed out while processing the video. Please try again.");
    }

    private async Task<ApiResult> PostAsync(string url, Dictionary<string, string> fields, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(fields), cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ApiResult.Fail(TryDeserialize<ErrorResponse>(json)?.Error?.Message ?? "Instagram request failed.");
        var data = TryDeserialize<IdResponse>(json);
        return string.IsNullOrWhiteSpace(data?.Id) ? ApiResult.Fail("Instagram returned an invalid response.") : ApiResult.Ok(data.Id);
    }

    private static PublishResultDto Failed(string message) => new() { Success = false, ErrorMessage = message };

    private async Task<List<PageData>> GetPagesAsync(string token, CancellationToken cancellationToken)
    {
        const string fields = "id,name,category,access_token,instagram_business_account{id,username,profile_picture_url}";
        var result = await GetAsync<PageResponse>($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me/accounts" +
            $"?fields={Uri.EscapeDataString(fields)}&access_token={Uri.EscapeDataString(token)}", cancellationToken);
        return result.Data ?? new List<PageData>();
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(TryDeserialize<ErrorResponse>(json)?.Error?.Message ?? "Instagram request failed.");
        return TryDeserialize<T>(json) ?? throw new InvalidOperationException("Failed to parse Instagram response.");
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.AppId) || string.IsNullOrWhiteSpace(_settings.AppSecret) || string.IsNullOrWhiteSpace(_settings.RedirectUri))
            throw new InvalidOperationException("Instagram integration is not configured.");
    }

    private static T? TryDeserialize<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch (JsonException) { return default; }
    }

    public async Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me/adaccounts?fields=id,name,account_id,account_status,currency,balance";
        using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(TryDeserialize<ErrorResponse>(content)?.Error?.Message ?? "Failed to fetch ad accounts.");
        return TryDeserialize<FacebookAdAccountResponse>(content)?.Data ?? new List<FacebookAdAccountData>();
    }

    public async Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var mapped = MapMetaObjective(objective);
        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";
        var fields = new Dictionary<string, string>
        {
            ["name"] = name, ["objective"] = mapped, ["status"] = "PAUSED",
            ["buying_type"] = "AUCTION", ["special_ad_categories"] = "[]",
            ["is_adset_budget_sharing_enabled"] = "false"
        };
        var result = await PostMarketingAsync($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/campaigns", fields, userAccessToken, cancellationToken);
        return result;
    }

    public async Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";
        var settings = MapMetaAdSetSettings(objective);
        var fields = new Dictionary<string, string>
        {
            ["name"] = name, ["campaign_id"] = campaignId,
            ["daily_budget"] = dailyBudget.HasValue ? $"{(long)Math.Max(dailyBudget.Value, 30000)}" : "30000",
            ["billing_event"] = settings.BillingEvent, ["optimization_goal"] = settings.OptimizationGoal,
            ["bid_strategy"] = "LOWEST_COST_WITHOUT_CAP", ["status"] = "PAUSED"
        };
        var utcNow = DateTime.UtcNow;
        var startUtc = startDate.HasValue ? startDate.Value.ToUniversalTime() : utcNow;
        fields["start_time"] = FormatMetaDate(startUtc);
        if (endDate.HasValue)
        {
            var endUtc = endDate.Value.ToUniversalTime();
            if ((endUtc - startUtc).TotalHours >= 24) fields["end_time"] = FormatMetaDate(endUtc);
        }
        fields["targeting"] = targetingJson ?? "{\"geo_locations\":{\"countries\":[\"VN\"]}}";
        fields["publisher_platforms"] = "[\"instagram\"]";
        fields["instagram_positions"] = "[\"stream\",\"story\",\"explore\",\"reels\"]";
        var result = await PostMarketingAsync($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/adsets", fields, userAccessToken, cancellationToken);
        return result;
    }

    public async Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";

        Dictionary<string, object?> creativeObject;

        if (!string.IsNullOrWhiteSpace(instagramMediaId) && !string.IsNullOrWhiteSpace(instagramActorId))
        {
            creativeObject = new Dictionary<string, object?>
            {
                ["instagram_actor_id"] = instagramActorId,
                ["instagram_media_id"] = instagramMediaId
            };
        }
        else
        {
            var link = string.IsNullOrWhiteSpace(linkUrl) ? $"https://www.facebook.com/{pageId}" : linkUrl;
            creativeObject = new Dictionary<string, object?>
            {
                ["page_id"] = pageId,
                ["link_data"] = new Dictionary<string, object?>
                {
                    ["link"] = link,
                    ["message"] = string.IsNullOrWhiteSpace(message) ? "Learn more" : message
                }
            };
            if (!string.IsNullOrWhiteSpace(instagramActorId))
                creativeObject["instagram_actor_id"] = instagramActorId;
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var pic = InstagramImageUrl(imageUrl);
                if (!string.IsNullOrWhiteSpace(pic) && creativeObject["link_data"] is Dictionary<string, object?> ld)
                    ld["picture"] = pic;
            }
        }

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(creativeObject, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower });
        var fields = new Dictionary<string, string> { ["object_story_spec"] = jsonPayload };
        var result = await PostMarketingAsync($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/adcreatives", fields, userAccessToken, cancellationToken);
        return result;
    }

    public async Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";
        var fields = new Dictionary<string, string>
        {
            ["name"] = name, ["adset_id"] = adSetId,
            ["creative"] = System.Text.Json.JsonSerializer.Serialize(new { creative_id = creativeId }), ["status"] = status
        };
        var result = await PostMarketingAsync($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/ads", fields, userAccessToken, cancellationToken);
        return result;
    }

    public async Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        try
        {
            var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{campaignId}/insights?fields=impressions,clicks,spend,actions,ctr,cpc";
            using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            var result = TryDeserialize<FacebookCampaignInsightsResponse>(content);
            return result?.Data?.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Impressions));
        }
        catch { return null; }
    }

    public async Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default)
        => await UpdateMetaObjectStatusAsync($"{adAccountId}/campaigns/{campaignId}", userAccessToken, status, cancellationToken);

    public async Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default)
        => await UpdateMetaObjectStatusAsync($"{adAccountId}/adsets/{adSetId}", userAccessToken, status, cancellationToken);

    public async Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default)
        => await UpdateMetaObjectStatusAsync($"{adAccountId}/ads/{adId}", userAccessToken, status, cancellationToken);

    public async Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
        => await DeleteMetaObjectAsync($"{adAccountId}/campaigns/{campaignId}", userAccessToken, cancellationToken);

    public async Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default)
        => await DeleteMetaObjectAsync($"{adAccountId}/adsets/{adSetId}", userAccessToken, cancellationToken);

    public async Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default)
        => await DeleteMetaObjectAsync($"{adAccountId}/adcreatives/{creativeId}", userAccessToken, cancellationToken);

    public async Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default)
        => await DeleteMetaObjectAsync($"{adAccountId}/ads/{adId}", userAccessToken, cancellationToken);

    private static string? InstagramImageUrl(string imageUrl)
    {
        var parsed = imageUrl.Trim();
        if (parsed.StartsWith('[') && parsed.EndsWith(']'))
        {
            try { var urls = JsonSerializer.Deserialize<List<string>>(parsed); parsed = urls?.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u)) ?? string.Empty; }
            catch { parsed = string.Empty; }
        }
        return string.IsNullOrWhiteSpace(parsed) ? null : parsed;
    }

    private async Task<string> PostMarketingAsync(string url, Dictionary<string, string> fields, string accessToken, CancellationToken cancellationToken)
    {
        var bodyStr = string.Join("&", fields.Select(f => $"{Uri.EscapeDataString(f.Key)}={Uri.EscapeDataString(f.Value)}"));
        using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var fbErr = TryDeserialize<ErrorResponse>(json);
            var msg = fbErr?.Error?.Message ?? json;
            throw new InvalidOperationException($"Marketing API failed [POST {url}]: {msg} (body: {bodyStr[..Math.Min(bodyStr.Length, 200)]})");
        }
        var data = TryDeserialize<IdResponse>(json);
        if (string.IsNullOrWhiteSpace(data?.Id)) throw new InvalidOperationException("Marketing API returned empty ID.");
        return data.Id;
    }

    private async Task<bool> UpdateMetaObjectStatusAsync(string relativePath, string accessToken, string status, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{relativePath}";
        using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["status"] = status })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> DeleteMetaObjectAsync(string relativePath, string accessToken, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{relativePath}";
        using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Delete, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private static string MapMetaObjective(string objective) => objective.ToUpperInvariant() switch
    {
        "AWARENESS" => "OUTCOME_AWARENESS", "TRAFFIC" => "OUTCOME_TRAFFIC",
        "ENGAGEMENT" => "OUTCOME_ENGAGEMENT", "LEADS" => "OUTCOME_LEADS",
        "SALES" => "OUTCOME_SALES", "APP_PROMOTION" => "OUTCOME_APP_PROMOTION",
        _ => "OUTCOME_AWARENESS"
    };

    private static (string OptimizationGoal, string BillingEvent) MapMetaAdSetSettings(string objective) => objective.ToUpperInvariant() switch
    {
        "TRAFFIC" => ("LINK_CLICKS", "IMPRESSIONS"), "ENGAGEMENT" => ("POST_ENGAGEMENT", "IMPRESSIONS"),
        "LEADS" => ("LEAD_GENERATION", "IMPRESSIONS"), "SALES" => ("LINK_CLICKS", "IMPRESSIONS"),
        "APP_PROMOTION" => ("LINK_CLICKS", "IMPRESSIONS"), _ => ("REACH", "IMPRESSIONS")
    };

    private static string MapMetaCallToAction(string? cta) => (cta?.ToUpperInvariant()) switch
    {
        "LEARN_MORE" or "SHOP_NOW" or "SIGN_UP" or "DOWNLOAD" or "CONTACT_US"
            or "BOOK_NOW" or "GET_OFFER" or "GET_QUOTE" or "SUBSCRIBE"
            or "PLAY_NOW" or "INSTALL_APP" or "USE_APP" or "WATCH_MORE" => cta!.ToUpperInvariant(),
        _ => "LEARN_MORE"
    };

    private static string FormatMetaDate(DateTime utcDateTime) => utcDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+0000";

    private sealed class TokenResponse { [JsonPropertyName("access_token")] public string? AccessToken { get; set; } [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; } }
    private sealed class UserResponse { public string? Id { get; set; } }
    private sealed class PageResponse { public List<PageData>? Data { get; set; } }
    private sealed class PageData { public string? Name { get; set; } public string? Category { get; set; } [JsonPropertyName("access_token")] public string? AccessToken { get; set; } [JsonPropertyName("instagram_business_account")] public InstagramAccount? InstagramBusinessAccount { get; set; } }
    private sealed class InstagramAccount { public string? Id { get; set; } public string? Username { get; set; } [JsonPropertyName("profile_picture_url")] public string? ProfilePictureUrl { get; set; } }
    private sealed class ErrorResponse { public ErrorData? Error { get; set; } }
    private sealed class ErrorData { public string? Message { get; set; } }
    private sealed class IdResponse { public string? Id { get; set; } }
    private sealed class ContainerStatus { [JsonPropertyName("status_code")] public string? StatusCode { get; set; } public string? Status { get; set; } }
    private sealed record ApiResult(bool Success, string? Id, string? Error)
    {
        public static ApiResult Ok(string id) => new(true, id, null);
        public static ApiResult Fail(string error) => new(false, null, error);
    }
}
