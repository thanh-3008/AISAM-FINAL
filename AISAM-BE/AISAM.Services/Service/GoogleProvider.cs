using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class GoogleProvider : IProviderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleProvider> _logger;

    public GoogleProvider(HttpClient httpClient, ILogger<GoogleProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public string ProviderName => "google";

    public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Google OAuth is not available in Phase C.");
        throw new NotSupportedException("Google OAuth is not available in Phase C.");
    }

    public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google OAuth is not available in Phase C.");
    }

    public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<AvailableTargetDto>>(Array.Empty<AvailableTargetDto>());
    }

    public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dictionary<string, string>());
    }

    public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google publishing is not available in Phase C.");
    }

    public Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }

    public Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Google Ads is not available in Phase C.");
    }
}
