using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            ["daily_budget"] = dailyBudget.HasValue ? $"{(long)dailyBudget.Value}" : "30000",
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
            _logger.LogWarning("Failed to create Facebook ad set. Status={Status}, Body={Body}", (int)response.StatusCode, content);
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

        public async Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, string? objectStoryId = null, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var actId = adAccountId.StartsWith("act_", StringComparison.OrdinalIgnoreCase) ? adAccountId : $"act_{adAccountId}";

        var fields = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(objectStoryId))
        {
            fields["object_story_id"] = objectStoryId!;
            _logger.LogInformation("Creating ad creative from existing post: object_story_id={StoryId}", objectStoryId);
        }
        else
        {
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
                var linkData = new Dictionary<string, object?>
                {
                    ["link"] = linkUrl,
                    ["message"] = message
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

            var jsonPayload = JsonSerializer.Serialize(creativeObject, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            fields["object_story_spec"] = jsonPayload;

            _logger.LogInformation("Creating ad creative: link={Link} messageLen={MsgLen} image={HasImage} cta={Cta}",
                linkUrl, message?.Length ?? 0, !string.IsNullOrWhiteSpace(imageUrl), MapCallToAction(callToAction));
        }

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
    //  Page Insights — Audience Analytics
    // ──────────────────────────────────────────────

    public async Task<FacebookPageInsightsResponse?> GetPageInsightsAsync(string pageId, string accessToken, string metrics, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{pageId}/insights?metric={metrics}";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get page insights for {PageId}: {Error}", pageId, GetErrorMessage(content));
            return null;
        }
        return Deserialize<FacebookPageInsightsResponse>(content);
    }

    // ──────────────────────────────────────────────
    //  Marketing API — Status Update
    // ──────────────────────────────────────────────

    public async Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default)
    {
        return await UpdateFacebookObjectStatusAsync($"{adAccountId}/campaigns/{campaignId}", userAccessToken, status, "campaign", cancellationToken);
    }

    public async Task<bool> UpdateCampaignNameAsync(string adAccountId, string userAccessToken, string campaignId, string name, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{adAccountId}/campaigns/{campaignId}";
        var fields = new Dictionary<string, string> { ["name"] = name };
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default)
    {
        return await UpdateFacebookObjectStatusAsync($"{adAccountId}/adsets/{adSetId}", userAccessToken, status, "ad set", cancellationToken);
    }

    public async Task<bool> UpdateAdSetBudgetAsync(string adAccountId, string userAccessToken, string adSetId, decimal dailyBudget, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{adAccountId}/adsets/{adSetId}";
        var fields = new Dictionary<string, string> { ["daily_budget"] = $"{(long)dailyBudget}" };
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
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
    //  Marketing API — Review / Status Polling
    // ──────────────────────────────────────────────

    public async Task<string?> GetAdEffectiveStatusAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{adId}?fields=effective_status,status,ad_review_feedback";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get ad {AdId} status: {Error}", adId, GetErrorMessage(content));
            return null;
        }
        try
        {
            using var doc = JsonDocument.Parse(content);
            var date = doc.RootElement;
            return date.TryGetProperty("effective_status", out var es) ? es.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<string?> GetAdSetEffectiveStatusAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{adSetId}?fields=effective_status,status";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get ad set {AdSetId} status: {Error}", adSetId, GetErrorMessage(content));
            return null;
        }
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            return root.TryGetProperty("effective_status", out var es) ? es.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // ──────────────────────────────────────────────
    //  Post Insights
    // ──────────────────────────────────────────────

    public async Task<FacebookPostInsightData?> GetPostInsightsAsync(string accessToken, string postId, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var insights = new FacebookPostInsightData();

        var coreMetrics = new[] { "post_media_view", "post_total_media_view_unique" };
        await FetchAndMergePostInsightFieldsAsync(accessToken, postId, coreMetrics, insights, cancellationToken);

        var impressionMetrics = new[] { "post_impressions", "post_impressions_unique" };
        await FetchAndMergePostInsightMetricsAsync(accessToken, postId, impressionMetrics, insights, cancellationToken);

        if (!insights.Impressions.HasValue && insights.Views.HasValue)
            insights.Impressions = insights.Views;
        if (!insights.Reach.HasValue && insights.TotalMediaViewUnique.HasValue)
            insights.Reach = insights.TotalMediaViewUnique;
        if (!insights.Reach.HasValue && insights.Impressions.HasValue)
            insights.Reach = insights.Impressions;

        var reactions = "post_reactions_like_total,post_reactions_love_total,post_reactions_wow_total,post_reactions_haha_total,post_reactions_sorry_total,post_reactions_anger_total";
        await FetchAndMergePostInsightMetricsAsync(accessToken, postId, reactions.Split(','), insights, cancellationToken);

        var clickMetrics = new[] { "post_clicks", "post_clicks_by_type" };
        await FetchAndMergePostInsightMetricsAsync(accessToken, postId, clickMetrics, insights, cancellationToken);

        var fields = "reactions.limit(0).summary(true),comments.limit(0).summary(true)";
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{postId}?fields={Uri.EscapeDataString(fields)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("reactions", out var r) && r.TryGetProperty("summary", out var rs) &&
                    rs.TryGetProperty("total_count", out var rc) && rc.ValueKind == JsonValueKind.Number)
                    insights.Reactions = rc.GetInt64();
                if (root.TryGetProperty("comments", out var c) && c.TryGetProperty("summary", out var cs) &&
                    cs.TryGetProperty("total_count", out var cc) && cc.ValueKind == JsonValueKind.Number)
                    insights.Comments = cc.GetInt64();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse post engagement for {PostId}", postId);
            }
        }

        return insights.Impressions.HasValue
            || insights.Reach.HasValue
            || insights.Views.HasValue
            || insights.EngagedUsers.HasValue
            || insights.Clicks.HasValue
            || insights.Reactions.HasValue
            || insights.Comments.HasValue
            || insights.Shares.HasValue
            ? insights
            : null;
    }

    public async Task<IReadOnlyList<FacebookPublishedPostData>> GetPublishedPostsAsync(
        string accessToken,
        string pageId,
        DateTime from,
        DateTime to,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var posts = new List<FacebookPublishedPostData>();
        var since = new DateTimeOffset(from).ToUnixTimeSeconds();
        var until = new DateTimeOffset(to).ToUnixTimeSeconds();
        var fields = Uri.EscapeDataString("id,message,created_time,permalink_url,insights");
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{pageId}/published_posts?fields={fields}&since={since}&until={until}&limit={Math.Clamp(limit, 1, 100)}";

        while (!string.IsNullOrWhiteSpace(url) && posts.Count < limit)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
            };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get published posts for page {PageId}: {Error}", pageId, GetErrorMessage(content));
                break;
            }

            var result = Deserialize<FacebookPublishedPostsResponse>(content);
            if (result?.Data != null)
                posts.AddRange(result.Data.Where(post => !string.IsNullOrWhiteSpace(post.Id)));

            url = result?.Paging?.Next;
        }

        return posts
            .Where(post => post.CreatedTime == null || (post.CreatedTime.Value >= from && post.CreatedTime.Value <= to))
            .OrderByDescending(post => post.CreatedTime ?? DateTime.MinValue)
            .Take(limit)
            .ToList();
    }

    private async Task MergePostViewSummaryAsync(string accessToken, string postId, FacebookPostInsightData insights, CancellationToken cancellationToken)
    {
        var views = await GetPostLongFieldAsync(accessToken, postId, "views", cancellationToken)
            ?? await GetPostLongFieldAsync(accessToken, postId, "view_count", cancellationToken);
        if (views.HasValue)
            insights.Views = Math.Max(insights.Views ?? 0, views.Value);
    }

    private async Task<long?> GetPostLongFieldAsync(string accessToken, string postId, string field, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{postId}?fields={field}";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug("Post field {Field} is unavailable for {PostId}: {Error}", field, postId, GetErrorMessage(content));
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.TryGetProperty(field, out var value)
                ? ExtractInsightNumber(value)
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task FetchAndMergePostInsightMetricsAsync(
        string accessToken,
        string postId,
        IReadOnlyCollection<string> metrics,
        FacebookPostInsightData insights,
        CancellationToken cancellationToken,
        string? period = null)
    {
        if (metrics.Count == 0)
            return;

        var metricParam = string.Join(",", metrics);
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{postId}/insights?metric={Uri.EscapeDataString(metricParam)}";
        if (!string.IsNullOrWhiteSpace(period))
            url += $"&period={Uri.EscapeDataString(period)}";

        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = GetErrorMessage(content);
            if (errorMessage.Contains("valid insights metric", StringComparison.OrdinalIgnoreCase))
            {
                insights.Diagnostics.Add($"{postId}:{metricParam}{(period == null ? "" : $":period={period}")}: unsupported metric ({errorMessage})");
                System.IO.File.AppendAllText("facebook_debug.log", $"{DateTime.UtcNow:O} | SKIP-METRIC | {postId}: {metricParam} - {errorMessage}\n");
                return;
            }

            _logger.LogWarning(
                "Failed to get Facebook post insight metrics {Metrics} for {PostId}: {Error}",
                metricParam,
                postId,
                errorMessage);
            insights.Diagnostics.Add($"{postId}:{metricParam}{(period == null ? "" : $":period={period}")}: {errorMessage}");
            System.IO.File.AppendAllText("facebook_debug.log", $"{DateTime.UtcNow:O} | ERROR-METRIC | {postId}: {metricParam} - {errorMessage}\n");
            return;
        }

        _logger.LogInformation(
            "Facebook post insights raw response for {PostId}, metrics {Metrics}: {Content}",
            postId,
            metricParam,
            content);

        var result = Deserialize<FacebookPostInsightsResponse>(content);
        if (result?.Data == null || result.Data.Count == 0)
            insights.Diagnostics.Add($"{postId}:{metricParam}{(period == null ? "" : $":period={period}")}: empty data");
        MergeInsightMetrics(result?.Data, insights);
    }

    private async Task FetchAndMergePostInsightFieldsAsync(
        string accessToken,
        string postId,
        IReadOnlyCollection<string> metrics,
        FacebookPostInsightData insights,
        CancellationToken cancellationToken)
    {
        if (metrics.Count == 0)
            return;

        var metricParam = string.Join(",", metrics);
        var uriBuilder = new UriBuilder($"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{postId}");
        var queryParams = new Dictionary<string, string>
        {
            ["fields"] = $"insights.metric({metricParam})",
            ["access_token"] = accessToken
        };
        uriBuilder.Query = BuildQueryString(queryParams);
        var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var logDir = AppDomain.CurrentDomain.BaseDirectory;
            System.IO.File.AppendAllText(System.IO.Path.Combine(logDir, "fb_insights.log"), $"{DateTime.UtcNow:O} | URL={RedactAccessToken(uriBuilder.Uri)} | Status={response.StatusCode} | Body={content}\n");
        }
        catch (Exception ex)
        {
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb_error.log"), $"Failed to write log: {ex.Message}\n"); } catch { }
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = GetErrorMessage(content);
            insights.Diagnostics.Add($"{postId}:fields insights.metric({metricParam}): {errorMessage}");
            _logger.LogWarning("Failed to get Facebook post insight fields {Metrics} for {PostId}: {Error}", metricParam, postId, errorMessage);
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("insights", out var insightsElement)
                || !insightsElement.TryGetProperty("data", out var dataElement))
            {
                insights.Diagnostics.Add($"{postId}:fields insights.metric({metricParam}): no insights.data");
                return;
            }

            var metricData = JsonSerializer.Deserialize<List<FacebookPostInsightMetric>>(
                dataElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (metricData == null || metricData.Count == 0)
                insights.Diagnostics.Add($"{postId}:fields insights.metric({metricParam}): empty data");

            MergeInsightMetrics(metricData, insights);
        }
        catch (JsonException ex)
        {
            insights.Diagnostics.Add($"{postId}:fields insights.metric({metricParam}): parse error {ex.Message}");
            _logger.LogWarning(ex, "Failed to parse Facebook post insight fields for {PostId}", postId);
        }
    }

    private static string BuildQueryString(IReadOnlyDictionary<string, string> queryParams)
    {
        return string.Join("&", queryParams.Select(kvp =>
            $"{EscapeGraphQueryValue(kvp.Key)}={EscapeGraphQueryValue(kvp.Value)}"));
    }

    private static string EscapeGraphQueryValue(string value)
    {
        return Uri.EscapeDataString(value)
            .Replace("(", "%28", StringComparison.Ordinal)
            .Replace(")", "%29", StringComparison.Ordinal);
    }

    private static string RedactAccessToken(Uri uri)
    {
        var builder = new UriBuilder(uri);
        var pairs = builder.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair =>
            {
                var separatorIndex = pair.IndexOf('=');
                if (separatorIndex < 0)
                    return pair;

                var key = Uri.UnescapeDataString(pair[..separatorIndex]);
                return key.Equals("access_token", StringComparison.OrdinalIgnoreCase)
                    ? $"{pair[..separatorIndex]}=***"
                    : pair;
            });

        builder.Query = string.Join("&", pairs);
        return builder.Uri.ToString();
    }

    private static long? ExtractInsightNumber(object? rawValue)
    {
        return rawValue switch
        {
            long value => value,
            int value => value,
            double value => (long)value,
            decimal value => (long)value,
            string value when long.TryParse(value, out var parsed) => parsed,
            JsonElement { ValueKind: JsonValueKind.Number } value when value.TryGetInt64(out var parsed) => parsed,
            JsonElement { ValueKind: JsonValueKind.String } value when long.TryParse(value.GetString(), out var parsed) => parsed,
            _ => null
        };
    }

    private static void MergeInsightMetrics(IEnumerable<FacebookPostInsightMetric>? metrics, FacebookPostInsightData insights)
    {
        if (metrics == null)
            return;

        foreach (var metric in metrics)
        {
            var rawValue = metric.Values?.FirstOrDefault()?.Value;
            if (rawValue == null)
                continue;

            var numericValue = ExtractInsightNumber(rawValue);
            switch (metric.Name)
            {
                case "post_impressions":
                case "impressions":
                case "post_media_view":
                    if (numericValue.HasValue) insights.Impressions = Math.Max(insights.Impressions ?? 0, numericValue.Value);
                    break;
                case "post_total_media_view_unique":
                    if (numericValue.HasValue) insights.TotalMediaViewUnique = Math.Max(insights.TotalMediaViewUnique ?? 0, numericValue.Value);
                    if (numericValue.HasValue) insights.Reach = Math.Max(insights.Reach ?? 0, numericValue.Value);
                    break;
                case "post_impressions_unique":
                case "reach":
                    if (numericValue.HasValue) insights.Reach = Math.Max(insights.Reach ?? 0, numericValue.Value);
                    break;
                case "post_engaged_users":
                case "engaged_users":
                    if (numericValue.HasValue) insights.EngagedUsers = Math.Max(insights.EngagedUsers ?? 0, numericValue.Value);
                    break;
                case "post_clicks":
                case "clicks":
                    if (numericValue.HasValue) insights.Clicks = Math.Max(insights.Clicks ?? 0, numericValue.Value);
                    break;
                case "post_views":
                case "post_video_views":
                case "video_views":
                    if (numericValue.HasValue) insights.Views = Math.Max(insights.Views ?? 0, numericValue.Value);
                    break;
                case "post_clicks_by_type":
                    var clickTotal = ExtractInsightObjectTotal(rawValue);
                    if (clickTotal.HasValue) insights.Clicks = Math.Max(insights.Clicks ?? 0, clickTotal.Value);
                    break;
                case "post_reactions_like_total":
                case "post_reactions_love_total":
                case "post_reactions_wow_total":
                case "post_reactions_haha_total":
                case "post_reactions_sorry_total":
                case "post_reactions_anger_total":
                case "post_reactions_by_type_total":
                    if (numericValue.HasValue)
                        insights.Reactions = (insights.Reactions ?? 0) + numericValue.Value;
                    else
                    {
                        var objTotal = ExtractInsightObjectTotal(rawValue);
                        if (objTotal.HasValue) insights.Reactions = Math.Max(insights.Reactions ?? 0, objTotal.Value);
                    }
                    break;
            }
        }
    }

    private static long? ExtractInsightObjectTotal(object? rawValue)
    {
        if (rawValue is not JsonElement { ValueKind: JsonValueKind.Object } json)
            return null;

        long total = 0;
        var hasAnyValue = false;
        foreach (var property in json.EnumerateObject())
        {
            var value = property.Value.ValueKind switch
            {
                JsonValueKind.Number when property.Value.TryGetInt64(out var parsed) => parsed,
                JsonValueKind.String when long.TryParse(property.Value.GetString(), out var parsed) => parsed,
                _ => (long?)null
            };

            if (!value.HasValue)
                continue;

            total += value.Value;
            hasAnyValue = true;
        }

        return hasAnyValue ? total : null;
    }

    private async Task MergePostEngagementSummaryAsync(string accessToken, string postId, FacebookPostInsightData insights, CancellationToken cancellationToken)
    {
        var fields = Uri.EscapeDataString("reactions.limit(0).summary(true),comments.limit(0).summary(true),shares");
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{postId}?fields={fields}";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get post engagement summary for {PostId}: {Error}", postId, GetErrorMessage(content));
        }
        else
        {
            var summary = Deserialize<FacebookPostEngagementResponse>(content);
            var reactions = summary.Reactions?.Summary?.TotalCount;
            var comments = summary.Comments?.Summary?.TotalCount;
            var shares = summary.Shares?.Count;

            if (reactions.HasValue) insights.Reactions = Math.Max(insights.Reactions ?? 0, reactions.Value);
            if (comments.HasValue) insights.Comments = Math.Max(insights.Comments ?? 0, comments.Value);
            if (shares.HasValue) insights.Shares = Math.Max(insights.Shares ?? 0, shares.Value);
        }

        if (!insights.Reactions.HasValue)
        {
            var reactionCount = await GetSummaryEdgeCountAsync(accessToken, postId, "reactions", cancellationToken);
            if (reactionCount.HasValue) insights.Reactions = reactionCount.Value;
        }

        if (!insights.Reactions.HasValue)
        {
            var likeCount = await GetSummaryEdgeCountAsync(accessToken, postId, "likes", cancellationToken);
            if (likeCount.HasValue) insights.Reactions = likeCount.Value;
        }

        if (!insights.Comments.HasValue)
        {
            var commentCount = await GetSummaryEdgeCountAsync(accessToken, postId, "comments", cancellationToken);
            if (commentCount.HasValue) insights.Comments = commentCount.Value;
        }

        if (!insights.Shares.HasValue)
        {
            var shareCount = await GetSharesCountAsync(accessToken, postId, cancellationToken);
            if (shareCount.HasValue) insights.Shares = shareCount.Value;
        }
    }

    private async Task<long?> GetSummaryEdgeCountAsync(string accessToken, string postId, string edge, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{postId}/{edge}?summary=true&limit=0";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get post {Edge} summary for {PostId}: {Error}", edge, postId, GetErrorMessage(content));
            return null;
        }

        var summary = Deserialize<FacebookSummaryListResponse>(content);
        return summary.Summary?.TotalCount;
    }

    private async Task<long?> GetSharesCountAsync(string accessToken, string postId, CancellationToken cancellationToken)
    {
        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/{postId}?fields=shares";
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get post shares summary for {PostId}: {Error}", postId, GetErrorMessage(content));
            return null;
        }

        var summary = Deserialize<FacebookPostEngagementResponse>(content);
        return summary.Shares?.Count;
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
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("post_id")]
        public string? PostId { get; set; }
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
        var payload = new Dictionary<string, string>
        {
            ["message"] = post.Message,
            ["access_token"] = integration.AccessToken
        };
        if (!string.IsNullOrWhiteSpace(post.LinkUrl))
        {
            payload["link"] = post.LinkUrl;
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
            ProviderPostId = publishResponse.PostId ?? publishResponse.Id,
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
            PropertyNameCaseInsensitive = true,
            Converters = { new FacebookDateTimeConverter() }
        }) ?? throw new InvalidOperationException("Failed to parse Facebook response.");
    }

    private sealed class FacebookDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] Formats =
        {
            "yyyy-MM-ddTHH:mm:sszzz",    // 2026-08-03T12:04:37+0000 (Facebook)
            "yyyy-MM-ddTHH:mm:sszz",      // 2026-08-03T12:04:37+00 (short tz)
            "yyyy-MM-ddTHH:mm:ssZ",       // 2026-08-03T12:04:37Z (UTC)
            "yyyy-MM-ddTHH:mm:ss"         // 2026-08-03T12:04:37 (no tz)
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (DateTime.TryParseExact(str, Formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var date))
                    return date;
            }
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var unixMs))
                return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss+0000"));
        }
    }

    private static string GetErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "Facebook request failed (empty response).";
        }

        try
        {
            var error = JsonSerializer.Deserialize<FacebookErrorResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (error?.Error != null)
            {
                if (error.Error.Code == 100 && error.Error.ErrorSubcode == 1885183)
                {
                    return "Creative khong hop le: app/content co the duoc tao khi Meta app con Development mode hoac bai viet chua public. "
                        + "Hay chuyen app sang Live mode, reconnect Facebook, tao campaign/content moi bang asset public roi deploy lai. "
                        + $"[code={error.Error.Code}, subcode={error.Error.ErrorSubcode}]";
                }

                if (error.Error.Code == 100 && error.Error.ErrorSubcode == 1359188)
                {
                    return "Tai khoan quang cao Facebook chua co phuong thuc thanh toan. "
                        + "Vui long truy cap Trung tam Lap hoa don va Thanh toan cua Facebook de them phuong thuc thanh toan hop le. "
                        + $"[code={error.Error.Code}, subcode={error.Error.ErrorSubcode}]";
                }

                var userMsg = error.Error.ErrorUserMsg ?? error.Error.Message ?? "Facebook request failed.";
                var details = $" [code={error.Error.Code}";
                if (error.Error.ErrorSubcode.HasValue)
                    details += $", subcode={error.Error.ErrorSubcode.Value}";
                if (error.Error.ErrorData?.BlameFieldSpecs?.Count > 0)
                    details += $", blame_fields=[{string.Join(", ", error.Error.ErrorData.BlameFieldSpecs.Select(s => string.Join(".", s)))}]";
                details += "]";
                return userMsg + details;
            }
        }
        catch (JsonException)
        {
        }

        var truncated = content.Length > 500 ? content[..500] + "..." : content;
        return $"Facebook request failed. Response: {truncated}";
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
            "AWARENESS" => ("REACH", "IMPRESSIONS"),
            "TRAFFIC" => ("LINK_CLICKS", "IMPRESSIONS"),
            "ENGAGEMENT" => ("POST_ENGAGEMENT", "IMPRESSIONS"),
            "LEADS" => ("LEAD_GENERATION", "IMPRESSIONS"),
            "SALES" => ("LINK_CLICKS", "IMPRESSIONS"),
            "APP_PROMOTION" => ("LINK_CLICKS", "IMPRESSIONS"),
            _ => ("IMPRESSIONS", "IMPRESSIONS")
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
