using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

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

        var userUrl = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me?fields=id,name&access_token={tokenData.AccessToken}";
        var userResponse = await _httpClient.GetAsync(userUrl, cancellationToken);
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

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me/accounts?fields=id,name,category,access_token&access_token={Uri.EscapeDataString(accessToken)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
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

        var url = $"{_settings.BaseUrl}/{_settings.GraphApiVersion}/me/accounts?fields=id,access_token&access_token={Uri.EscapeDataString(userAccessToken)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
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

            if (!string.IsNullOrWhiteSpace(error?.Error?.Message))
            {
                return error.Error.Message!;
            }
        }
        catch (JsonException)
        {
        }

        return "Facebook request failed.";
    }
}
