using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service
{
    public class AdCampaignService : IAdCampaignService
    {
        private const int StepCampaignCreated = 1;
        private const int StepAdSetCreated = 2;
        private const int StepAdCreativeCreated = 3;
        private const int StepAdCreated = 4;

        private readonly IAdCampaignRepository _campaignRepository;
        private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ISocialService _socialService;
        private readonly IProviderService _facebookProvider;
        private readonly IQuotaService _quotaService;
        private readonly ILogger<AdCampaignService> _logger;

        public AdCampaignService(
            IAdCampaignRepository campaignRepository,
            IWorkspaceMemberRepository workspaceMemberRepository,
            IBrandRepository brandRepository,
            IContentRepository contentRepository,
            ISocialService socialService,
            IQuotaService quotaService,
            IEnumerable<IProviderService> providers,
            ILogger<AdCampaignService> logger)
        {
            _campaignRepository = campaignRepository;
            _workspaceMemberRepository = workspaceMemberRepository;
            _brandRepository = brandRepository;
            _contentRepository = contentRepository;
            _socialService = socialService;
            _quotaService = quotaService;
            _facebookProvider = providers.First(p => p.ProviderName == "facebook");
            _logger = logger;
        }

        public async Task<GenericResponse<PagedResult<AdCampaignResponseDto>>> GetPagedByWorkspaceIdAsync(
            Guid workspaceId,
            Guid userId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateError(access.Message);
            }

            var campaigns = await _campaignRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, includeDeleted, cancellationToken);

            return GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateSuccess(new PagedResult<AdCampaignResponseDto>
            {
                Data = campaigns.Data.Select(MapToDto).ToList(),
                TotalCount = campaigns.TotalCount,
                Page = campaigns.Page,
                PageSize = campaigns.PageSize
            }, "Campaigns retrieved successfully");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");
            }

            if (campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);
            }

            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(campaign), "Campaign retrieved successfully");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> CreateAsync(Guid workspaceId, Guid userId, CreateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);
            }

            if (string.IsNullOrWhiteSpace(request.Name))
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign name is required.");
            if (string.IsNullOrWhiteSpace(request.AdAccountId))
                return GenericResponse<AdCampaignResponseDto>.CreateError("Ad account is required.");
            if (request.Budget.HasValue && request.Budget.Value <= 0)
                return GenericResponse<AdCampaignResponseDto>.CreateError("Budget must be greater than 0.");
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate.Value > request.EndDate.Value)
                return GenericResponse<AdCampaignResponseDto>.CreateError("End date must be after start date.");

            var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
            if (brand == null || brand.WorkspaceId != workspaceId)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Brand not found in this workspace");
            }

            var campaign = new AdCampaign
            {
                WorkspaceId = workspaceId,
                ProfileId = brand.ProfileId,
                BrandId = request.BrandId,
                ProductId = request.ProductId,
                ContentId = request.ContentId,
                Targeting = request.Targeting,
                AdAccountId = request.AdAccountId,
                Name = request.Name,
                Objective = request.Objective,
                Budget = request.Budget,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            var created = await _campaignRepository.AddAsync(campaign, cancellationToken);

            // Re-fetch to get up-to-date navigation properties
            var refreshed = await _campaignRepository.GetByIdAsync(created.Id, cancellationToken);
            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? created), "Campaign created successfully");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> UpdateAsync(Guid id, Guid workspaceId, Guid userId, UpdateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                campaign.Name = request.Name;
            }

            if (request.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByIdAsync(request.BrandId.Value, cancellationToken);
                if (brand == null || brand.WorkspaceId != workspaceId)
                {
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Brand not found in this workspace");
                }

                campaign.BrandId = request.BrandId.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.AdAccountId))
            {
                campaign.AdAccountId = request.AdAccountId;
            }

            if (request.Objective != null)
            {
                campaign.Objective = request.Objective;
            }

            if (request.ProductId.HasValue)
            {
                campaign.ProductId = request.ProductId.Value;
            }

            if (request.ContentId.HasValue)
            {
                campaign.ContentId = request.ContentId.Value;
            }

            if (request.Targeting != null)
            {
                campaign.Targeting = request.Targeting;
            }

            if (request.Budget.HasValue)
            {
                campaign.Budget = request.Budget.Value;
            }

            if (request.StartDate.HasValue)
            {
                campaign.StartDate = request.StartDate.Value;
            }

            if (request.EndDate.HasValue)
            {
                campaign.EndDate = request.EndDate.Value;
            }

            var wasActivated = false;
            var wasDeactivated = false;
            if (request.IsActive.HasValue)
            {
                wasActivated = request.IsActive.Value && !campaign.IsActive;
                wasDeactivated = !request.IsActive.Value && campaign.IsActive;
                campaign.IsActive = request.IsActive.Value;
            }

            if (request.Budget.HasValue && request.Budget.Value <= 0)
                return GenericResponse<AdCampaignResponseDto>.CreateError("Budget must be greater than 0.");

            await _campaignRepository.UpdateAsync(campaign, cancellationToken);

            // Sync status to Facebook if already deployed
            if (campaign.DeploymentStatus == DeploymentStatusEnum.Completed && !string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
            {
                try
                {
                    var (account, _, _) = await ResolveSocialContextAsync(campaign, cancellationToken);
                    var fbStatus = campaign.IsActive ? "ACTIVE" : "PAUSED";
                    await _facebookProvider.UpdateCampaignStatusAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, fbStatus, cancellationToken);

                    // Also update ad set and ad status
                    var adSets = await _campaignRepository.GetAdSetsByCampaignIdAsync(campaign.Id, cancellationToken);
                    foreach (var adSet in adSets)
                    {
                        if (!string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                            await _facebookProvider.UpdateAdSetStatusAsync(campaign.AdAccountId, account.AccessToken, adSet.FacebookAdSetId, fbStatus, cancellationToken);
                        var ads = await _campaignRepository.GetAdsByAdSetIdAsync(adSet.Id, cancellationToken);
                        foreach (var ad in ads)
                        {
                            if (!string.IsNullOrWhiteSpace(ad.AdId))
                                await _facebookProvider.UpdateAdStatusAsync(campaign.AdAccountId, account.AccessToken, ad.AdId, fbStatus, cancellationToken);
                        }
                    }

                    _logger.LogInformation("Synced campaign {CampaignId} status to {Status} on Facebook", campaign.Id, fbStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync status to Facebook for campaign {CampaignId}", campaign.Id);
                }
            }

            // Deploy to Facebook if just activated and not yet completed
            if (wasActivated && campaign.DeploymentStatus != DeploymentStatusEnum.Completed)
            {
                try
                {
                    await DeployCampaignToFacebookAsync(campaign, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Auto-deploy to Facebook failed during activation for campaign {CampaignId}", campaign.Id);
                }
            }

            var refreshed = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken);
            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? campaign), "Campaign updated successfully");
        }

        public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<bool>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            campaign.IsDeleted = true;
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Campaign deleted successfully");
        }

        public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<bool>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            if (!campaign.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Campaign is not deleted");
            }

            campaign.IsDeleted = false;
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Campaign restored successfully");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> DeployToFacebookAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);
            }

            if (campaign.DeploymentStatus == DeploymentStatusEnum.Completed)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(campaign), "Campaign already deployed to Facebook");
            }

            try
            {
                await DeployCampaignToFacebookAsync(campaign, cancellationToken);
                var refreshed = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken);
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? campaign), "Campaign deployed to Facebook successfully");
            }
            catch (Exception ex)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError($"Failed to deploy campaign to Facebook: {ex.Message}");
            }
        }

        public async Task<GenericResponse<bool>> CleanupFailedDeploymentAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<bool>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            if (campaign.DeploymentStatus != DeploymentStatusEnum.Failed && campaign.DeploymentStatus != DeploymentStatusEnum.InProgress)
            {
                return GenericResponse<bool>.CreateError("Campaign is not in a failed or in-progress state");
            }

            // Get social account for access token
            var accounts = await _socialService.GetProfileAccountsAsync(campaign.ProfileId, cancellationToken);
            var account = accounts.FirstOrDefault(a => a.Provider == "facebook");
            if (account == null)
            {
                return GenericResponse<bool>.CreateError("No Facebook account connected");
            }

            var errors = new List<string>();

            // Cleanup in reverse order: Ad → AdCreative → AdSet → Campaign
            var adSets = campaign.AdSets.Where(ads => !ads.IsDeleted).ToList();
            foreach (var adSet in adSets)
            {
                var ads = adSet.Ads.Where(a => !a.IsDeleted).ToList();
                foreach (var ad in ads)
                {
                    if (!string.IsNullOrWhiteSpace(ad.AdId))
                    {
                        var ok = await _facebookProvider.DeleteAdAsync(campaign.AdAccountId, account.AccessToken, ad.AdId, cancellationToken);
                        if (!ok) errors.Add($"Failed to delete ad {ad.AdId} on Facebook");
                    }
                    await _campaignRepository.HardDeleteAdAsync(ad.Id, cancellationToken);

                    if (ad.CreativeId != Guid.Empty && !string.IsNullOrWhiteSpace(ad.Creative?.CreativeId))
                    {
                        var ok = await _facebookProvider.DeleteAdCreativeAsync(campaign.AdAccountId, account.AccessToken, ad.Creative.CreativeId, cancellationToken);
                        if (!ok) errors.Add($"Failed to delete creative {ad.Creative.CreativeId} on Facebook");
                        await _campaignRepository.HardDeleteAdCreativeAsync(ad.CreativeId, cancellationToken);
                    }
                }

                if (!string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                {
                    var ok = await _facebookProvider.DeleteAdSetAsync(campaign.AdAccountId, account.AccessToken, adSet.FacebookAdSetId, cancellationToken);
                    if (!ok) errors.Add($"Failed to delete ad set {adSet.FacebookAdSetId} on Facebook");
                }
                await _campaignRepository.HardDeleteAdSetAsync(adSet.Id, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
            {
                var ok = await _facebookProvider.DeleteCampaignAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, cancellationToken);
                if (!ok) errors.Add($"Failed to delete campaign {campaign.FacebookCampaignId} on Facebook");
            }

            await _campaignRepository.ClearFacebookIdsAsync(campaign.Id, cancellationToken);

            if (errors.Count > 0)
            {
                _logger.LogWarning("Cleanup completed with {ErrorCount} non-fatal errors: {Errors}", errors.Count, string.Join("; ", errors));
            }

            return GenericResponse<bool>.CreateSuccess(true, errors.Count == 0
                ? "Deployment cleanup completed successfully"
                : $"Deployment cleanup completed with {errors.Count} warning(s)");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> SyncCampaignInsightsAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);

            if (string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign has not been deployed to Facebook yet.");

            try
            {
                var (account, _, _) = await ResolveSocialContextAsync(campaign, cancellationToken);
                var insights = await _facebookProvider.GetCampaignInsightsAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, cancellationToken);

                if (insights != null)
                {
                    long.TryParse(insights.Impressions, out var impressions);
                    long.TryParse(insights.Clicks, out var clicks);
                    decimal.TryParse(insights.Spend, out var spend);

                    await _campaignRepository.UpdateCampaignInsightsAsync(campaign.Id, impressions, clicks, spend, 0, cancellationToken);

                    _logger.LogInformation("Synced and saved insights for campaign {CampaignId}: impressions={Impressions}, clicks={Clicks}, spend={Spend}",
                        campaign.Id, impressions, clicks, spend);
                }
                else
                {
                    _logger.LogInformation("No insights available yet for campaign {CampaignId}", campaign.Id);
                }

                var refreshed = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken);
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? campaign), "Insights synced successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync insights for campaign {CampaignId}", campaign.Id);
                return GenericResponse<AdCampaignResponseDto>.CreateError("Failed to sync insights from Facebook.");
            }
        }

        // ──────────────────────────────────────────────
        //  Deployment with incremental save & retry
        // ──────────────────────────────────────────────

        private async Task DeployCampaignToFacebookAsync(AdCampaign campaign, CancellationToken cancellationToken)
        {
            var (account, integration, pageId) = await ResolveSocialContextAsync(campaign, cancellationToken);

            // ── Step 1: Campaign ──
            if (campaign.DeploymentStep < StepCampaignCreated)
            {
                var fbCampaignId = await _facebookProvider.CreateCampaignAsync(
                    campaign.AdAccountId,
                    account.AccessToken,
                    campaign.Name,
                    campaign.Objective ?? "AWARENESS",
                    campaign.Budget,
                    campaign.StartDate,
                    campaign.EndDate,
                    cancellationToken
                );

                campaign.FacebookCampaignId = fbCampaignId;
                campaign.DeploymentStatus = DeploymentStatusEnum.InProgress;
                campaign.DeploymentStep = StepCampaignCreated;
                await _campaignRepository.UpdateAsync(campaign, cancellationToken);
                _logger.LogInformation("Deploy step 1/4: Campaign {FbId} created and saved", fbCampaignId);
            }

            // ── Step 2: Ad Set ──
            var adSet = await _campaignRepository.GetAdSetByCampaignIdAsync(campaign.Id, cancellationToken);
            if (campaign.DeploymentStep < StepAdSetCreated || adSet == null || string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
            {
                var adSetName = $"{campaign.Name} - Ad Set";
                var fbAdSetId = await _facebookProvider.CreateAdSetAsync(
                    campaign.AdAccountId,
                    account.AccessToken,
                    campaign.FacebookCampaignId!,
                    adSetName,
                    campaign.Objective ?? "AWARENESS",
                    campaign.Budget.HasValue ? campaign.Budget.Value / 30 : null,
                    campaign.StartDate,
                    campaign.EndDate,
                    campaign.Targeting ?? "{\"geo_locations\":{\"countries\":[\"VN\"]}}",
                    cancellationToken
                );

                adSet = new AdSet
                {
                    CampaignId = campaign.Id,
                    Name = adSetName,
                    FacebookAdSetId = fbAdSetId,
                    DailyBudget = campaign.Budget.HasValue ? campaign.Budget.Value / 30 : null,
                    Status = "ACTIVE",
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate
                };
                await _campaignRepository.AddAdSetAsync(adSet, cancellationToken);
                campaign.DeploymentStep = StepAdSetCreated;
                await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.InProgress, StepAdSetCreated, cancellationToken);
                _logger.LogInformation("Deploy step 2/4: Ad Set {FbId} created and saved", fbAdSetId);
            }
            else
            {
                _logger.LogInformation("Deploy step 2/4: Reusing existing ad set {AdSetId}", adSet.Id);
            }

            // ── Step 3: Ad Creative ──
            AdCreative? creative = null;
            var existingAd = adSet.Id != Guid.Empty ? await _campaignRepository.GetAdByAdSetIdAsync(adSet.Id, cancellationToken) : null;
            if (existingAd?.CreativeId != null)
            {
                creative = await _campaignRepository.GetCreativeByIdAsync(existingAd.CreativeId, cancellationToken);
            }

            if (campaign.DeploymentStep < StepAdCreativeCreated || creative == null || string.IsNullOrWhiteSpace(creative.CreativeId))
            {
                Content? content = null;

                // Use campaign.ContentId if set by user
                if (campaign.ContentId.HasValue)
                {
                    content = await _contentRepository.GetByIdAsync(campaign.ContentId.Value, cancellationToken);
                }

                // Fallback: fetch published/approved content for the brand
                if (content == null)
                {
                    var contentRequest = new PaginationRequest { Page = 1, PageSize = 5 };
                    var brandContent = await _contentRepository.GetPagedByProfileIdAsync(
                        campaign.ProfileId, contentRequest,
                        brandId: campaign.BrandId,
                        status: ContentStatusEnum.Published,
                        cancellationToken: cancellationToken
                    );
                    if (brandContent.Data.Count == 0)
                    {
                        brandContent = await _contentRepository.GetPagedByProfileIdAsync(
                            campaign.ProfileId, contentRequest,
                            brandId: campaign.BrandId,
                            status: ContentStatusEnum.Approved,
                            cancellationToken: cancellationToken
                        );
                    }
                    content = brandContent.Data.FirstOrDefault();
                }

                var message = content?.TextContent ?? campaign.Name;
                var linkUrl = $"https://facebook.com/{pageId}";
                var imageUrl = content?.ImageUrl;

                var fbCreativeId = await _facebookProvider.CreateAdCreativeAsync(
                    campaign.AdAccountId,
                    account.AccessToken,
                    pageId,
                    message,
                    linkUrl,
                    imageUrl,
                    null,
                    cancellationToken
                );

                creative = new AdCreative
                {
                    AdAccountId = campaign.AdAccountId,
                    ContentId = content?.Id,
                    CreativeId = fbCreativeId,
                    CallToAction = "LEARN_MORE",
                    LinkUrl = linkUrl
                };
                await _campaignRepository.AddAdCreativeAsync(creative, cancellationToken);
                campaign.DeploymentStep = StepAdCreativeCreated;
                await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.InProgress, StepAdCreativeCreated, cancellationToken);
                _logger.LogInformation("Deploy step 3/4: Ad Creative {FbId} created and saved", fbCreativeId);
            }
            else
            {
                _logger.LogInformation("Deploy step 3/4: Reusing existing creative {CreativeId}", creative.Id);
            }

            // ── Step 4: Ad ──
            if (campaign.DeploymentStep < StepAdCreated || existingAd == null || string.IsNullOrWhiteSpace(existingAd.AdId))
            {
                var adName = $"{campaign.Name} - Ad";
                var fbAdId = await _facebookProvider.CreateAdAsync(
                    campaign.AdAccountId,
                    account.AccessToken,
                    adSet.FacebookAdSetId!,
                    creative!.CreativeId!,
                    adName,
                    "ACTIVE",
                    cancellationToken
                );

                var ad = new Ad
                {
                    AdSetId = adSet.Id,
                    CreativeId = creative.Id,
                    AdId = fbAdId,
                    Status = "ACTIVE"
                };
                await _campaignRepository.AddAdAsync(ad, cancellationToken);

                campaign.DeploymentStatus = DeploymentStatusEnum.Completed;
                campaign.DeploymentStep = StepAdCreated;
                await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.Completed, StepAdCreated, cancellationToken);
                _logger.LogInformation("Deploy step 4/4: Ad {FbId} created — deployment complete", fbAdId);
            }
            else
            {
                _logger.LogInformation("Deploy step 4/4: Ad already exists, marking complete");
                campaign.DeploymentStatus = DeploymentStatusEnum.Completed;
                campaign.DeploymentStep = StepAdCreated;
                await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.Completed, StepAdCreated, cancellationToken);
            }
        }

        private async Task<(SocialAccountDto Account, SocialIntegrationDto Integration, string PageId)> ResolveSocialContextAsync(AdCampaign campaign, CancellationToken cancellationToken)
        {
            var integrations = await _socialService.GetIntegrationsByBrandAsync(campaign.ProfileId, campaign.BrandId, cancellationToken);
            var integration = integrations.FirstOrDefault();
            if (integration == null)
            {
                throw new InvalidOperationException("No social integration found for this brand. Please connect a Facebook page first.");
            }

            var accounts = await _socialService.GetProfileAccountsAsync(campaign.ProfileId, cancellationToken);
            var account = accounts.FirstOrDefault(a => a.Provider == "facebook");
            if (account == null)
            {
                throw new InvalidOperationException("No Facebook account connected. Please connect your Facebook account first.");
            }

            return (account, integration, integration.ExternalId ?? string.Empty);
        }

        private async Task<(bool Success, string Message)> EnsureWorkspaceMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken)
        {
            var membership = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
            return membership == null
                ? (false, "You are not allowed to access this workspace")
                : (true, string.Empty);
        }

        private static AdCampaignResponseDto MapToDto(AdCampaign campaign)
        {
            return new AdCampaignResponseDto
            {
                Id = campaign.Id,
                ProfileId = campaign.ProfileId,
                WorkspaceId = campaign.WorkspaceId,
                BrandId = campaign.BrandId,
                BrandName = campaign.Brand?.Name ?? string.Empty,
                ProductId = campaign.ProductId,
                ProductName = campaign.Product?.Name,
                ContentId = campaign.ContentId,
                ContentTitle = campaign.Content?.Title,
                Targeting = campaign.Targeting,
                AdAccountId = campaign.AdAccountId,
                FacebookCampaignId = campaign.FacebookCampaignId,
                Name = campaign.Name,
                Objective = campaign.Objective,
                Budget = campaign.Budget,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                IsActive = campaign.IsActive,
                IsDeleted = campaign.IsDeleted,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                DeploymentStatus = campaign.DeploymentStatus,
                DeploymentStep = campaign.DeploymentStep,
                AdSets = campaign.AdSets.Select(ads => new AdSetSummaryDto
                {
                    Id = ads.Id,
                    Name = ads.Name,
                    FacebookAdSetId = ads.FacebookAdSetId,
                    DailyBudget = ads.DailyBudget,
                    Status = ads.Status,
                    Impressions = 0,
                    Clicks = 0,
                    Spend = 0,
                    Ads = ads.Ads.Where(a => !a.IsDeleted).Select(a => new AdSummaryDto
                    {
                        Id = a.Id,
                        AdId = a.AdId,
                        Status = a.Status,
                        CreativeId = a.Creative?.CreativeId,
                        CallToAction = a.Creative?.CallToAction,
                        LinkUrl = a.Creative?.LinkUrl
                    }).ToList()
                }).ToList(),
                Impressions = campaign.Impressions,
                Clicks = campaign.Clicks,
                Spend = campaign.Spend,
                Conversions = campaign.Conversions
            };
        }
    }
}
