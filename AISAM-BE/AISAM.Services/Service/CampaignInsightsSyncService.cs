using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service
{
    public sealed class CampaignInsightsSyncService : ICampaignInsightsSyncService
    {
        private const int BatchSize = 10;
        private readonly IAdCampaignRepository _campaignRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ISocialService _socialService;
        private readonly Dictionary<string, IProviderService> _providers;
        private readonly ILogger<CampaignInsightsSyncService> _logger;

        public CampaignInsightsSyncService(
            IAdCampaignRepository campaignRepository,
            INotificationRepository notificationRepository,
            ISocialService socialService,
            IEnumerable<IProviderService> providers,
            ILogger<CampaignInsightsSyncService> logger)
        {
            _campaignRepository = campaignRepository;
            _notificationRepository = notificationRepository;
            _socialService = socialService;
            _providers = providers.Where(p => p.ProviderName == "facebook" || p.ProviderName == "instagram")
                .ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
            _logger = logger;
        }

        public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
        {
            var expiredByWorkspace = await _campaignRepository.UpdateExpiredCampaignsAsync(cancellationToken);
            foreach (var (workspaceId, count) in expiredByWorkspace)
            {
                _logger.LogInformation("Auto-completed {Count} expired campaigns in workspace {WorkspaceId}", count, workspaceId);
                await CreateNotificationAsync(workspaceId,
                    "Campaigns auto-completed",
                    $"{count} campaign(s) automatically completed due to expiration.",
                    NotificationTypeEnum.SystemUpdate, cancellationToken);
            }

            var insightsProcessed = await ProcessInsightsNextAsync(cancellationToken);
            var reviewProcessed = await ProcessReviewActivationAsync(cancellationToken);

            return insightsProcessed || reviewProcessed;
        }

        private async Task<bool> ProcessInsightsNextAsync(CancellationToken cancellationToken)
        {
            var campaigns = await _campaignRepository.GetDeployedCampaignsForSyncAsync(BatchSize, cancellationToken);

            foreach (var campaign in campaigns)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
                    continue;

                try
                {
                    var provider = GetProvider(campaign.Platform);
                    var accounts = await _socialService.GetProfileAccountsAsync(campaign.ProfileId, cancellationToken);
                    var account = accounts.FirstOrDefault(a => a.Provider == campaign.Platform);
                    if (account == null) continue;

                    var adToken = await _socialService.GetFacebookUserAccessTokenAsync(campaign.ProfileId, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(adToken)) account.AccessToken = adToken;

                    if (string.IsNullOrWhiteSpace(account.AccessToken))
                    {
                        _logger.LogWarning("Skipping insight sync for campaign {CampaignId}: missing access token.", campaign.Id);
                        continue;
                    }

                    var insights = await provider.GetCampaignInsightsAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, cancellationToken);

                    if (insights != null)
                    {
                        long.TryParse(insights.Impressions, out var impressions);
                        long.TryParse(insights.Clicks, out var clicks);
                        decimal.TryParse(insights.Spend, out var spend);
                        var conversions = FacebookProvider.ExtractConversions(insights);

                        await _campaignRepository.UpdateCampaignInsightsAsync(campaign.Id, impressions, clicks, spend, conversions, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to auto-sync insights for campaign {CampaignId}", campaign.Id);
                }
            }

            return campaigns.Count > 0;
        }

        private async Task<bool> ProcessReviewActivationAsync(CancellationToken cancellationToken)
        {
            var pendingCampaigns = await _campaignRepository.GetDeployedPendingActivationAsync(BatchSize, cancellationToken);

            foreach (var campaign in pendingCampaigns)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var provider = GetProvider(campaign.Platform);
                    var accounts = await _socialService.GetProfileAccountsAsync(campaign.ProfileId, cancellationToken);
                    var account = accounts.FirstOrDefault(a => a.Provider == campaign.Platform);
                    if (account == null) continue;

                    var adToken = await _socialService.GetFacebookUserAccessTokenAsync(campaign.ProfileId, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(adToken)) account.AccessToken = adToken;

                    if (string.IsNullOrWhiteSpace(account.AccessToken))
                    {
                        _logger.LogWarning("Skipping review check for campaign {CampaignId}: missing access token.", campaign.Id);
                        continue;
                    }

                    var adSets = await _campaignRepository.GetAdSetsByCampaignIdAsync(campaign.Id, cancellationToken);
                    if (adSets.Count == 0) continue;

                    var rejected = false;
                    var pending = false;
                    var hasCheckedAd = false;
                    var rejectionReason = string.Empty;

                    foreach (var adSet in adSets)
                    {
                        var ads = await _campaignRepository.GetAdsByAdSetIdAsync(adSet.Id, cancellationToken);
                        foreach (var ad in ads)
                        {
                            if (string.IsNullOrWhiteSpace(ad.AdId)) continue;
                            hasCheckedAd = true;
                            var effectiveStatus = (await provider.GetAdEffectiveStatusAsync(campaign.AdAccountId, account.AccessToken, ad.AdId, cancellationToken))
                                ?.ToUpperInvariant();

                            if (string.IsNullOrWhiteSpace(effectiveStatus))
                            {
                                pending = true;
                                continue;
                            }

                            if (effectiveStatus is "REJECTED" or "DISAPPROVED" or "WITH_ISSUES")
                            {
                                rejected = true;
                                rejectionReason = "Ad was rejected by Meta review. Please check your creative and content compliance.";
                                break;
                            }

                            if (effectiveStatus is "PENDING_REVIEW" or "IN_PROCESS" or "PROCESSING")
                            {
                                pending = true;
                            }
                            else if (effectiveStatus == "ACTIVE" || effectiveStatus == "PAUSED" || effectiveStatus == "CAMPAIGN_PAUSED" || effectiveStatus == "ADSET_PAUSED")
                            {
                            }
                            else
                            {
                                pending = true;
                                _logger.LogInformation("Campaign {CampaignId} ad {AdId} has non-terminal Meta status {Status}", campaign.Id, ad.AdId, effectiveStatus);
                            }
                        }
                        if (rejected) break;
                    }

                    if (!hasCheckedAd || pending)
                    {
                        if (campaign.Status != CampaignStatusEnum.PendingReview)
                            await _campaignRepository.UpdateCampaignStatusAsync(campaign.Id, CampaignStatusEnum.PendingReview, cancellationToken);
                        _logger.LogInformation("Campaign {CampaignId} remains pending Meta review", campaign.Id);
                        continue;
                    }

                    if (rejected)
                    {
                        _logger.LogInformation("Campaign {CampaignId} review rejected: {Reason}", campaign.Id, rejectionReason);
                        await _campaignRepository.UpdateCampaignStatusAsync(campaign.Id, CampaignStatusEnum.Rejected, cancellationToken);
                    }
                    else
                    {
                        await _campaignRepository.UpdateCampaignStatusAsync(campaign.Id, CampaignStatusEnum.Paused, cancellationToken);
                        _logger.LogInformation("Campaign {CampaignId} passed pending review checks and is ready to start", campaign.Id);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process review activation for campaign {CampaignId}", campaign.Id);
                }
            }

            return pendingCampaigns.Count > 0;
        }

        private async Task CreateNotificationAsync(Guid workspaceId, string title, string message, NotificationTypeEnum type, CancellationToken cancellationToken)
        {
            try
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    WorkspaceId = workspaceId,
                    Title = title,
                    Message = message,
                    Type = type,
                    IsRead = false
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create notification for workspace {WorkspaceId}: {Title}", workspaceId, title);
            }
        }

        private IProviderService GetProvider(string platform)
        {
            var key = platform?.ToLowerInvariant() ?? "facebook";
            if (_providers.TryGetValue(key, out var provider)) return provider;
            if (_providers.TryGetValue("facebook", out provider)) return provider;
            throw new InvalidOperationException("No ad provider available.");
        }
    }
}
