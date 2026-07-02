using System.Text;
using System.Text.Json;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

public sealed class FacebookProvider : IProviderService
{
    private readonly HttpClient _httpClient;
    private readonly FacebookSettings _settings;
    private readonly ILogger<FacebookProvider> _logger;

    public string ProviderName => "facebook";

    public FacebookProvider(HttpClient httpClient, IOptions<FacebookSettings> settings, ILogger<FacebookProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var permissions = string.Join(",", _settings.RequiredPermissions.Distinct());
        var authUrl = $"{_settings.OAuthUrl}/{_settings.GraphApiVersion}/dialog/oauth" +
                      $"?client_id={_settings.AppId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&scope={Uri.EscapeDataString(permissions)}" +
                      $"&response_type=code" +
                      $"&state={Uri.EscapeDataString(state)}";

        return Task.FromResult(authUrl);
    }

    public async Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var tokenUrl = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/oauth/access_token" +
                       $"?client_id={_settings.AppId}" +
                       $"&client_secret={_settings.AppSecret}" +
                       $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                       $"&code={Uri.EscapeDataString(code)}";

        var tokenResponse = await _httpClient.GetAsync(tokenUrl, cancellationToken);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(tokenContent));
        }

        var tokenData = Deserialize<FacebookTokenResponse>(tokenContent);
        if (string.IsNullOrWhiteSpace(tokenData.AccessToken))
        {
            throw new InvalidOperationException("Failed to obtain access token from Facebook.");
        }

        var userUrl = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me?fields=id,name";
        var userRequest = new HttpRequestMessage(HttpMethod.Get, userUrl)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken) }
        };
        var userResponse = await _httpClient.SendAsync(userRequest, cancellationToken);
        var userContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!userResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(userContent));
        }

        var userData = Deserialize<FacebookUserResponse>(userContent);
        if (string.IsNullOrWhiteSpace(userData.Id))
        {
            throw new InvalidOperationException("Failed to obtain user profile from Facebook.");
        }

        return new SocialAccountDto
        {
            Provider = ProviderName,
            ProviderUserId = userData.Id,
            AccessToken = tokenData.AccessToken,
            IsActive = true,
            ExpiresAt = tokenData.ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn.Value) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me/accounts?fields=id,name,category,access_token";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(content));
        }

        var pages = Deserialize<FacebookPageResponse>(content).Data ?? new List<FacebookPageData>();
        return pages
            .Where(page => !string.IsNullOrWhiteSpace(page.Id))
            .Select(page => new AvailableTargetDto
            {
                ProviderTargetId = page.Id!,
                Name = page.Name ?? string.Empty,
                Type = "page",
                Category = page.Category,
                IsActive = true
            })
            .ToList();
    }

    public async Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me/accounts?fields=id,access_token";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(content));
        }

        var pageIds = providerTargetIds.ToHashSet(StringComparer.Ordinal);
        var pages = Deserialize<FacebookPageResponse>(content).Data ?? new List<FacebookPageData>();
        return pages
            .Where(page => !string.IsNullOrWhiteSpace(page.Id) && !string.IsNullOrWhiteSpace(page.AccessToken) && pageIds.Contains(page.Id))
            .ToDictionary(page => page.Id!, page => page.AccessToken!, StringComparer.Ordinal);
    }

    public async Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        if (!string.IsNullOrWhiteSpace(post.VideoUrl))
        {
            return await PublishVideoAsync(account, integration, post, cancellationToken);
        }

        if (post.ImageUrls is { Count: > 1 })
        {
            return await PublishMultiImageAsync(integration, post, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(post.ImageUrl))
        {
            return await PublishSingleImageAsync(account, integration, post, cancellationToken);
        }

        return await PublishFeedAsync(account, integration, post, cancellationToken);
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Ad Accounts
    // ──────────────────────────────────────────────

    public async Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me/adaccounts?fields=id,name,account_id,account_status,currency,balance";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(content));
        }

        var result = Deserialize<FacebookAdAccountResponse>(content);
        return result?.Data ?? new List<FacebookAdAccountData>();
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Campaigns
    // ──────────────────────────────────────────────

    public async Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var mapped = MapObjective(objective);
        _logger.LogInformation("Creating Facebook campaign: objective={OriginalObjective}, mapped={MappedObjective}", objective, mapped);

        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";

        // Debug: verify token scopes
        try
        {
            var debugUrl = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/debug_token?input_token={Uri.EscapeDataString(userAccessToken)}&access_token={_settings.AppId}|{_settings.AppSecret}";
            var debugResponse = await _httpClient.GetAsync(debugUrl, cancellationToken);
            var debugContent = await debugResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("=== TOKEN DEBUG: {DebugInfo}", debugContent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to debug token: {Error}", ex.Message);
        }

        var fields = new Dictionary<string, string>
        {
            ["name"] = name,
            ["objective"] = mapped,
            ["status"] = "PAUSED",
            ["buying_type"] = "AUCTION",
            ["special_ad_categories"] = "[]",
            ["is_adset_budget_sharing_enabled"] = "false"
        };

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/campaigns";

        // Log full request details
        var bodyStr = string.Join("&", fields.Select(f => $"{f.Key}={Uri.EscapeDataString(f.Value)}"));
        _logger.LogInformation("=== REQUEST ===\nURL: POST {Url}\nAuthorization: Bearer {TokenPrefix}...\nBody: {Body}\n================", url, userAccessToken[..Math.Min(30, userAccessToken.Length)], bodyStr);

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("=== RESPONSE ===\nStatus: {StatusCode}\nBody: {Body}\n================", (int)response.StatusCode, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to create Facebook campaign: {GetErrorMessage(content)}");
        }

        var result = Deserialize<FacebookCampaignCreateResponse>(content);
        if (string.IsNullOrWhiteSpace(result?.Id))
        {
            throw new InvalidOperationException("Facebook returned empty campaign ID.");
        }

        _logger.LogInformation("Created Facebook campaign {FacebookCampaignId} for ad account {AdAccountId}", result.Id, adAccountId);
        return result.Id;
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Ad Sets
    // ──────────────────────────────────────────────

    public async Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";
        var adSetSettings = MapAdSetSettings(objective);
        var fields = new Dictionary<string, string>
        {
            ["name"] = name,
            ["campaign_id"] = campaignId,
            ["daily_budget"] = dailyBudget.HasValue ? $"{(long)Math.Max(dailyBudget.Value, 30000)}" : "30000",
            ["billing_event"] = adSetSettings.BillingEvent,
            ["optimization_goal"] = adSetSettings.OptimizationGoal,
            ["bid_strategy"] = "LOWEST_COST_WITHOUT_CAP",
            ["status"] = "PAUSED",
        };

        var utcNow = DateTime.UtcNow;
        var startUtc = startDate.HasValue ? startDate.Value.ToUniversalTime() : utcNow;
        fields["start_time"] = FormatFacebookDate(startUtc);

        // Facebook requires minimum 24h between start_time and end_time for daily_budget ad sets
        if (endDate.HasValue)
        {
            var endUtc = endDate.Value.ToUniversalTime();
            if ((endUtc - startUtc).TotalHours >= 24)
                fields["end_time"] = FormatFacebookDate(endUtc);
        }

        if (!string.IsNullOrWhiteSpace(targetingJson))
            fields["targeting"] = targetingJson;

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/adsets";
        var bodyStr = string.Join("&", fields.Select(f => $"{f.Key}={Uri.EscapeDataString(f.Value)}"));
        _logger.LogInformation("=== REQUEST ===\nURL: POST {Url}\nAuthorization: Bearer {TokenPrefix}...\nBody: {Body}\n================", url, userAccessToken[..Math.Min(30, userAccessToken.Length)], bodyStr);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("=== RESPONSE ===\nStatus: {StatusCode}\nBody: {Body}\n================", (int)response.StatusCode, content);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to create Facebook ad set: {GetErrorMessage(content)}");
        }

        var result = Deserialize<FacebookAdSetCreateResponse>(content);
        if (string.IsNullOrWhiteSpace(result?.Id))
        {
            throw new InvalidOperationException("Facebook returned empty ad set ID.");
        }

        _logger.LogInformation("Created Facebook ad set {AdSetId} for campaign {CampaignId}", result.Id, campaignId);
        return result.Id;
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Ad Creatives
    // ──────────────────────────────────────────────

    public async Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";

        var isExternalLink = !string.IsNullOrWhiteSpace(linkUrl)
            && !linkUrl.Contains("facebook.com", StringComparison.OrdinalIgnoreCase)
            && !linkUrl.Contains("fb.com", StringComparison.OrdinalIgnoreCase);

        Dictionary<string, object?> creativeObject;

        if (isExternalLink)
        {
            var linkData = new Dictionary<string, object?>
            {
                ["link"] = linkUrl,
                ["message"] = message,
                ["call_to_action"] = new Dictionary<string, object?>
                {
                    ["type"] = MapCallToAction(callToAction)
                }
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var parsed = ParseImageUrl(imageUrl);
                if (!string.IsNullOrWhiteSpace(parsed))
                    linkData["picture"] = parsed;
            }

            creativeObject = new Dictionary<string, object?>
            {
                ["page_id"] = pageId,
                ["link_data"] = linkData
            };
        }
        else
        {
            // No external link: create page post first, then use as creative
            var postId = await CreatePagePostAsync(pageId, userAccessToken, message, imageUrl, cancellationToken);

            creativeObject = new Dictionary<string, object?>
            {
                ["page_id"] = pageId,
                ["object_story_id"] = postId
            };
        }

        var jsonPayload = JsonSerializer.Serialize(creativeObject, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        var fields = new Dictionary<string, string>
        {
            ["object_story_spec"] = jsonPayload
        };

        _logger.LogInformation("Creating ad creative: link={Link} messageLen={MsgLen} image={HasImage} cta={Cta}",
            linkUrl, message?.Length ?? 0, !string.IsNullOrWhiteSpace(imageUrl), MapCallToAction(callToAction));

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/adcreatives";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Ad creative response: {Response}", content);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to create Facebook ad creative: {GetErrorMessage(content)}");
        }

        var result = Deserialize<FacebookAdCreativeCreateResponse>(content);
        if (string.IsNullOrWhiteSpace(result?.Id))
        {
            throw new InvalidOperationException("Facebook returned empty creative ID.");
        }

        _logger.LogInformation("Created Facebook ad creative {CreativeId}", result.Id);
        return result.Id;
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Ads
    // ──────────────────────────────────────────────

    public async Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";
        var fields = new Dictionary<string, string>
        {
            ["name"] = name,
            ["adset_id"] = adSetId,
            ["creative"] = JsonSerializer.Serialize(new { creative_id = creativeId }),
            ["status"] = status
        };

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{actId}/ads";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to create Facebook ad: {GetErrorMessage(content)}");
        }

        var result = Deserialize<FacebookAdCreateResponse>(content);
        if (string.IsNullOrWhiteSpace(result?.Id))
        {
            throw new InvalidOperationException("Facebook returned empty ad ID.");
        }

        _logger.LogInformation("Created Facebook ad {AdId} in ad set {AdSetId}", result.Id, adSetId);
        return result.Id;
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Insights
    // ──────────────────────────────────────────────

    public async Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{campaignId}/insights?fields=impressions,clicks,spend,actions,ctr,cpc";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get insights for campaign {CampaignId}: {Error}", campaignId, GetErrorMessage(content));
            return null;
        }

        var result = Deserialize<FacebookCampaignInsightsResponse>(content);
        return result?.Data?.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Impressions));
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Status Update
    // ──────────────────────────────────────────────

    public async Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default)
    {
        return await UpdateFacebookObjectStatusAsync($"{adAccountId}/campaigns/{campaignId}", userAccessToken, status, "campaign", cancellationToken);
    }

    public async Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default)
    {
        return await UpdateFacebookObjectStatusAsync($"{adAccountId}/adsets/{adSetId}", userAccessToken, status, "ad set", cancellationToken);
    }

    public async Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default)
    {
        return await UpdateFacebookObjectStatusAsync($"{adAccountId}/ads/{adId}", userAccessToken, status, "ad", cancellationToken);
    }

    private async Task<bool> UpdateFacebookObjectStatusAsync(string relativePath, string accessToken, string status, string label, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{relativePath}";
        var fields = new Dictionary<string, string> { ["status"] = status };
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to update Facebook {Label} status to {Status}: {Error}", label, status, GetErrorMessage(content));
            return false;
        }
        _logger.LogInformation("Updated Facebook {Label} status to {Status}", label, status);
        return true;
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Delete / Cleanup
    // ──────────────────────────────────────────────

    public async Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
    {
        return await DeleteFacebookObjectAsync($"{adAccountId}/campaigns/{campaignId}", userAccessToken, "campaign", cancellationToken);
    }

    public async Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default)
    {
        return await DeleteFacebookObjectAsync($"{adAccountId}/adsets/{adSetId}", userAccessToken, "ad set", cancellationToken);
    }

    public async Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default)
    {
        return await DeleteFacebookObjectAsync($"{adAccountId}/adcreatives/{creativeId}", userAccessToken, "ad creative", cancellationToken);
    }

    public async Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default)
    {
        return await DeleteFacebookObjectAsync($"{adAccountId}/ads/{adId}", userAccessToken, "ad", cancellationToken);
    }

    private async Task<string> CreatePagePostAsync(string pageId, string accessToken, string message, string? imageUrl, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{pageId}/feed";
        var fields = new Dictionary<string, string>
        {
            ["message"] = message
        };

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            var parsed = ParseImageUrl(imageUrl);
            if (!string.IsNullOrWhiteSpace(parsed))
                fields["link"] = parsed;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (content.Contains("pages_read_engagement") || content.Contains("pages_manage_posts") || content.Contains("sufficient administrative permission"))
                throw new InvalidOperationException("Facebook permissions insufficient. Please disconnect and reconnect your Facebook account to grant all required permissions.");
            throw new InvalidOperationException($"Failed to create page post: {GetErrorMessage(content)}");
        }

        var result = Deserialize<FacebookPostResponse>(content);
        if (string.IsNullOrWhiteSpace(result?.Id))
            throw new InvalidOperationException("Facebook returned empty post ID.");

        return result.Id;
    }

    private static string? ParseImageUrl(string imageUrl)
    {
        var parsed = imageUrl.Trim();
        if (parsed.StartsWith('[') && parsed.EndsWith(']'))
        {
            var urls = JsonSerializer.Deserialize<List<string>>(parsed);
            parsed = urls?.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u)) ?? string.Empty;
        }
        return string.IsNullOrWhiteSpace(parsed) ? null : parsed;
    }

    internal sealed class FacebookPostResponse
    {
        public string? Id { get; set; }
    }

    private async Task<bool> DeleteFacebookObjectAsync(string relativePath, string accessToken, string label, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{relativePath}";
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to delete Facebook {Label} ({Url}): {Error}", label, url, GetErrorMessage(content));
            return false;
        }
        _logger.LogInformation("Deleted Facebook {Label} successfully", label);
        return true;
    }

    // ──────────────────────────────────────────────
    //  Private helpers
    // ──────────────────────────────────────────────

    private async Task<PublishResultDto> PublishFeedAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{integration.ExternalId}/feed";
        var initial = await PostFormAsync(url, new Dictionary<string, string>
        {
            ["message"] = post.Message,
            ["access_token"] = integration.AccessToken
        }, cancellationToken);

        if (initial.Success)
        {
            return initial;
        }

        var refreshedToken = await TryRefreshPageTokenAsync(account, integration, cancellationToken);
        if (string.IsNullOrWhiteSpace(refreshedToken))
        {
            return initial;
        }

        var retried = await PostFormAsync(url, new Dictionary<string, string>
        {
            ["message"] = post.Message,
            ["access_token"] = refreshedToken
        }, cancellationToken);

        if (retried.Success)
        {
            retried.RefreshedTargetAccessToken = refreshedToken;
        }

        return retried;
    }

    private async Task<PublishResultDto> PublishSingleImageAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{integration.ExternalId}/photos";
        var payload = new Dictionary<string, string>
        {
            ["url"] = post.ImageUrl!,
            ["access_token"] = integration.AccessToken
        };

        if (!string.IsNullOrWhiteSpace(post.Message))
        {
            payload["message"] = post.Message;
        }

        var initial = await PostFormAsync(url, payload, cancellationToken);
        if (initial.Success)
        {
            return initial;
        }

        var refreshedToken = await TryRefreshPageTokenAsync(account, integration, cancellationToken);
        if (string.IsNullOrWhiteSpace(refreshedToken))
        {
            return initial;
        }

        payload["access_token"] = refreshedToken;
        var retried = await PostFormAsync(url, payload, cancellationToken);
        if (retried.Success)
        {
            retried.RefreshedTargetAccessToken = refreshedToken;
        }

        return retried;
    }

    private async Task<PublishResultDto> PublishMultiImageAsync(SocialIntegration integration, PostDto post, CancellationToken cancellationToken)
    {
        var uploadedMediaIds = new List<string>();
        foreach (var imageUrl in post.ImageUrls!)
        {
            var uploadResult = await PostFormAsync(
                $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{integration.ExternalId}/photos",
                new Dictionary<string, string>
                {
                    ["url"] = imageUrl,
                    ["published"] = "false",
                    ["access_token"] = integration.AccessToken
                },
                cancellationToken);

            if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.ProviderPostId))
            {
                return uploadResult;
            }

            uploadedMediaIds.Add(uploadResult.ProviderPostId);
        }

        var fields = new List<KeyValuePair<string, string>>
        {
            new("message", post.Message),
            new("access_token", integration.AccessToken)
        };

        for (var i = 0; i < uploadedMediaIds.Count; i++)
        {
            fields.Add(new KeyValuePair<string, string>($"attached_media[{i}]", JsonSerializer.Serialize(new { media_fbid = uploadedMediaIds[i] })));
        }

        return await PostFormAsync($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{integration.ExternalId}/feed", fields, cancellationToken);
    }

    private async Task<PublishResultDto> PublishVideoAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{integration.ExternalId}/videos";
        return await PostFormAsync(url, new Dictionary<string, string>
        {
            ["file_url"] = post.VideoUrl!,
            ["description"] = post.Message,
            ["access_token"] = integration.AccessToken
        }, cancellationToken);
    }

    private async Task<PublishResultDto> PostFormAsync(string url, IEnumerable<KeyValuePair<string, string>> fields, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(fields), cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Facebook publish request failed with status code {StatusCode}.", (int)response.StatusCode);
            return new PublishResultDto
            {
                Success = false,
                ErrorMessage = GetErrorMessage(content)
            };
        }

        var publishResponse = Deserialize<FacebookPostResponse>(content);
        return new PublishResultDto
        {
            Success = true,
            ProviderPostId = publishResponse.Id,
            PostedAt = DateTime.UtcNow
        };
    }

    private async Task<string?> TryRefreshPageTokenAsync(SocialAccount account, SocialIntegration integration, CancellationToken cancellationToken)
    {
        var tokenMap = await GetTargetAccessTokensAsync(account.UserAccessToken, new[] { integration.ExternalId ?? string.Empty }, cancellationToken);
        return tokenMap.TryGetValue(integration.ExternalId ?? string.Empty, out var token) ? token : null;
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.AppId) ||
            string.IsNullOrWhiteSpace(_settings.AppSecret) ||
            string.IsNullOrWhiteSpace(_settings.RedirectUri))
        {
            throw new InvalidOperationException("Facebook integration is not configured.");
        }
    }

    private static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to parse Facebook response.");
    }

    private static string GetErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "Facebook request failed.";
        }

        try
        {
            var error = JsonSerializer.Deserialize<FacebookErrorResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (error?.Error != null)
            {
                var msg = error.Error.Message ?? "Facebook request failed.";
                var details = $" [code={error.Error.Code}";
                if (error.Error.ErrorSubcode.HasValue)
                    details += $", subcode={error.Error.ErrorSubcode.Value}";
                if (error.Error.ErrorData?.BlameFieldSpecs?.Count > 0)
                    details += $", blame_fields=[{string.Join(", ", error.Error.ErrorData.BlameFieldSpecs.Select(s => string.Join(".", s)))}]";
                details += "]";
                return msg + details;
            }
        }
        catch (JsonException)
        {
        }

        return "Facebook request failed.";
    }

    private static string FormatFacebookDate(DateTime utcDateTime)
    {
        return utcDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+0000";
    }

    private static string MapObjective(string objective)
    {
        return objective.ToUpperInvariant() switch
        {
            "AWARENESS" => "OUTCOME_AWARENESS",
            "TRAFFIC" => "OUTCOME_TRAFFIC",
            "ENGAGEMENT" => "OUTCOME_ENGAGEMENT",
            "LEADS" => "OUTCOME_LEADS",
            "SALES" => "OUTCOME_SALES",
            "APP_PROMOTION" => "OUTCOME_APP_PROMOTION",
            _ => "OUTCOME_AWARENESS"
        };
    }

    private static (string OptimizationGoal, string BillingEvent) MapAdSetSettings(string objective)
    {
        return objective.ToUpperInvariant() switch
        {
            "TRAFFIC" => ("LINK_CLICKS", "IMPRESSIONS"),
            "ENGAGEMENT" => ("POST_ENGAGEMENT", "IMPRESSIONS"),
            "LEADS" => ("LEAD_GENERATION", "IMPRESSIONS"),
            "SALES" => ("LINK_CLICKS", "IMPRESSIONS"),
            "APP_PROMOTION" => ("LINK_CLICKS", "IMPRESSIONS"),
            _ => ("REACH", "IMPRESSIONS")
        };
    }

    private static string MapCallToAction(string? cta)
    {
        return (cta?.ToUpperInvariant()) switch
        {
            "LEARN_MORE" => "LEARN_MORE",
            "SHOP_NOW" => "SHOP_NOW",
            "SIGN_UP" => "SIGN_UP",
            "DOWNLOAD" => "DOWNLOAD",
            "CONTACT_US" => "CONTACT_US",
            "BOOK_NOW" => "BOOK_NOW",
            "GET_OFFER" => "GET_OFFER",
            "GET_QUOTE" => "GET_QUOTE",
            "SUBSCRIBE" => "SUBSCRIBE",
            "PLAY_NOW" => "PLAY_NOW",
            "INSTALL_APP" => "INSTALL_APP",
            "USE_APP" => "USE_APP",
            "WATCH_MORE" => "WATCH_MORE",
            _ => "LEARN_MORE"
        };
    }
}
