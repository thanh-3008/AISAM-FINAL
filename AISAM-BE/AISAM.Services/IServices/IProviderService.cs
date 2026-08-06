using AISAM.Common.Models;
using AISAM.Data.Model;

namespace AISAM.Services.IServices;

public interface IProviderService
{
    string ProviderName { get; }
    Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default);
    Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default);

    // ─── Marketing API ───
    Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default);
    Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default);
    Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, CancellationToken cancellationToken = default);
    Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default);
    Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default);

    // ─── Status Update ───
    Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default);
    Task<bool> UpdateCampaignNameAsync(string adAccountId, string userAccessToken, string campaignId, string name, CancellationToken cancellationToken = default);
    Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default);
    Task<bool> UpdateAdSetBudgetAsync(string adAccountId, string userAccessToken, string adSetId, decimal dailyBudget, CancellationToken cancellationToken = default);
    Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default);

    // ─── Review / Status Polling ───
    Task<string?> GetAdEffectiveStatusAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default);
    Task<string?> GetAdSetEffectiveStatusAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default);

    // ─── Cleanup / Delete ───
    Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default);

    // ─── Post Insights ───
    Task<FacebookPostInsightData?> GetPostInsightsAsync(string accessToken, string postId, CancellationToken cancellationToken = default)
        => Task.FromResult<FacebookPostInsightData?>(null);
}
