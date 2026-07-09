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
using System.Globalization;

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
        private readonly IPostRepository _postRepository;
        private readonly Dictionary<string, IProviderService> _providers;
        private readonly ILogger<AdCampaignService> _logger;

        public AdCampaignService(
            IAdCampaignRepository campaignRepository,
            IWorkspaceMemberRepository workspaceMemberRepository,
            IBrandRepository brandRepository,
            IContentRepository contentRepository,
            ISocialService socialService,
            IPostRepository postRepository,
            IEnumerable<IProviderService> providers,
            ILogger<AdCampaignService> logger)
        {
            _campaignRepository = campaignRepository;
            _workspaceMemberRepository = workspaceMemberRepository;
            _brandRepository = brandRepository;
            _contentRepository = contentRepository;
            _socialService = socialService;
            _postRepository = postRepository;
            _providers = providers.Where(p => p.ProviderName == "facebook" || p.ProviderName == "instagram")
                .ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
            _logger = logger;
        }

        private IProviderService GetProvider(string platform)
        {
            var key = platform?.ToLowerInvariant() ?? "facebook";
            if (_providers.TryGetValue(key, out var provider)) return provider;
            if (_providers.TryGetValue("facebook", out provider)) return provider;
            throw new InvalidOperationException("No ad provider available.");
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
                Platform = string.IsNullOrWhiteSpace(request.Platform) ? "facebook" : request.Platform.ToLowerInvariant(),
                Name = request.Name,
                Objective = request.Objective,
                Budget = request.Budget,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LandingUrl = request.LandingUrl,
                IsActive = false
            };

            var created = await _campaignRepository.AddAsync(campaign, cancellationToken);

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

            var isDeployed = campaign.DeploymentStatus == DeploymentStatusEnum.Completed && !string.IsNullOrWhiteSpace(campaign.FacebookCampaignId);
            var provider = GetProvider(campaign.Platform);

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
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Ad account cannot be changed after deployment. Stop the campaign first.");
                campaign.AdAccountId = request.AdAccountId;
            }

            if (request.Objective != null)
            {
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Objective cannot be changed after deployment. Stop the campaign first.");
                campaign.Objective = request.Objective;
            }

            if (request.ProductId.HasValue)
            {
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Product cannot be changed after deployment. Stop the campaign first.");
                campaign.ProductId = request.ProductId.Value;
            }

            if (request.ContentId.HasValue)
            {
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Content cannot be changed after deployment. Stop the campaign first.");
                campaign.ContentId = request.ContentId.Value;
            }

            if (request.Targeting != null)
            {
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Targeting cannot be changed after deployment. Stop the campaign first.");
                campaign.Targeting = request.Targeting;
            }

            if (request.Budget.HasValue)
            {
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Budget cannot be changed after deployment. Stop the campaign first.");
                campaign.Budget = request.Budget.Value;
            }

            if (request.StartDate.HasValue)
            {
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Start date cannot be changed after deployment. Stop the campaign first.");
                campaign.StartDate = request.StartDate.Value;
            }

            if (request.EndDate.HasValue)
            {
                if (isDeployed)
                    return GenericResponse<AdCampaignResponseDto>.CreateError("End date cannot be changed after deployment. Stop the campaign first.");
                campaign.EndDate = request.EndDate.Value;
            }

            if (request.LandingUrl != null)
            {
                campaign.LandingUrl = request.LandingUrl;
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

            if (campaign.DeploymentStatus == DeploymentStatusEnum.Completed && !string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
            {
                try
                {
                    var (account, _, _, _) = await ResolveSocialContextAsync(campaign, cancellationToken);
                    var adStatus = campaign.IsActive ? "ACTIVE" : "PAUSED";
                    await provider.UpdateCampaignStatusAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, adStatus, cancellationToken);

                    var adSets = await _campaignRepository.GetAdSetsByCampaignIdAsync(campaign.Id, cancellationToken);
                    foreach (var adSet in adSets)
                    {
                        if (!string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                            await provider.UpdateAdSetStatusAsync(campaign.AdAccountId, account.AccessToken, adSet.FacebookAdSetId, adStatus, cancellationToken);
                        var ads = await _campaignRepository.GetAdsByAdSetIdAsync(adSet.Id, cancellationToken);
                        foreach (var ad in ads)
                        {
                            if (!string.IsNullOrWhiteSpace(ad.AdId))
                                await provider.UpdateAdStatusAsync(campaign.AdAccountId, account.AccessToken, ad.AdId, adStatus, cancellationToken);
                        }
                    }

                    _logger.LogInformation("Synced campaign {CampaignId} status to {Status} on {Platform}", campaign.Id, adStatus, campaign.Platform);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync status to {Platform} for campaign {CampaignId}", campaign.Platform, campaign.Id);
                }
            }

            if (wasActivated && campaign.DeploymentStatus != DeploymentStatusEnum.Completed)
            {
                try
                {
                    await DeployCampaignAsync(campaign, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Auto-deploy to {Platform} failed during activation for campaign {CampaignId}", campaign.Platform, campaign.Id);
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

            if (campaign.DeploymentStatus == DeploymentStatusEnum.Completed && !string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
            {
                try
                {
                    var provider = GetProvider(campaign.Platform);
                    var (account, _, _, _) = await ResolveSocialContextAsync(campaign, cancellationToken);
                    await provider.UpdateCampaignStatusAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, "PAUSED", cancellationToken);

                    var adSets = await _campaignRepository.GetAdSetsByCampaignIdAsync(campaign.Id, cancellationToken);
                    foreach (var adSet in adSets)
                    {
                        if (!string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                            await provider.UpdateAdSetStatusAsync(campaign.AdAccountId, account.AccessToken, adSet.FacebookAdSetId, "PAUSED", cancellationToken);
                        var ads = await _campaignRepository.GetAdsByAdSetIdAsync(adSet.Id, cancellationToken);
                        foreach (var ad in ads)
                        {
                            if (!string.IsNullOrWhiteSpace(ad.AdId))
                                await provider.UpdateAdStatusAsync(campaign.AdAccountId, account.AccessToken, ad.AdId, "PAUSED", cancellationToken);
                        }
                    }

                    _logger.LogInformation("Paused campaign {CampaignId} on {Platform} before deletion", campaign.Id, campaign.Platform);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to pause campaign {CampaignId} on {Platform} during deletion", campaign.Id, campaign.Platform);
                }
            }

            campaign.IsDeleted = true;
            campaign.IsActive = false;
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

        public async Task<GenericResponse<AdCampaignResponseDto>> DeployAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
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
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(campaign), "Campaign already deployed");
            }

            try
            {
                await DeployCampaignAsync(campaign, cancellationToken);
                var refreshed = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken);
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? campaign), $"Campaign deployed to {campaign.Platform} successfully");
            }
            catch (Exception ex)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError($"Failed to deploy campaign: {ex.Message}");
            }
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> DeployToFacebookAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await DeployAsync(id, workspaceId, userId, cancellationToken);
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

            var provider = GetProvider(campaign.Platform);
            var (account, _, _, _) = await ResolveSocialContextAsync(campaign, cancellationToken);

            var errors = new List<string>();

            var adSets = campaign.AdSets.Where(ads => !ads.IsDeleted).ToList();
            foreach (var adSet in adSets)
            {
                var ads = adSet.Ads.Where(a => !a.IsDeleted).ToList();
                foreach (var ad in ads)
                {
                    if (!string.IsNullOrWhiteSpace(ad.AdId))
                    {
                        var ok = await provider.DeleteAdAsync(campaign.AdAccountId, account.AccessToken, ad.AdId, cancellationToken);
                        if (!ok) errors.Add($"Failed to delete ad {ad.AdId}");
                    }
                    await _campaignRepository.HardDeleteAdAsync(ad.Id, cancellationToken);

                    if (ad.CreativeId != Guid.Empty && !string.IsNullOrWhiteSpace(ad.Creative?.CreativeId))
                    {
                        var ok = await provider.DeleteAdCreativeAsync(campaign.AdAccountId, account.AccessToken, ad.Creative.CreativeId, cancellationToken);
                        if (!ok) errors.Add($"Failed to delete creative {ad.Creative.CreativeId}");
                        await _campaignRepository.HardDeleteAdCreativeAsync(ad.CreativeId, cancellationToken);
                    }
                }

                if (!string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                {
                    var ok = await provider.DeleteAdSetAsync(campaign.AdAccountId, account.AccessToken, adSet.FacebookAdSetId, cancellationToken);
                    if (!ok) errors.Add($"Failed to delete ad set {adSet.FacebookAdSetId}");
                }
                await _campaignRepository.HardDeleteAdSetAsync(adSet.Id, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
            {
                var ok = await provider.DeleteCampaignAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, cancellationToken);
                if (!ok) errors.Add($"Failed to delete campaign {campaign.FacebookCampaignId}");
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
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign has not been deployed yet.");

            try
            {
                var provider = GetProvider(campaign.Platform);
                var (account, _, _, _) = await ResolveSocialContextAsync(campaign, cancellationToken);
                var insights = await provider.GetCampaignInsightsAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, cancellationToken);

                if (insights != null)
                {
                    long.TryParse(insights.Impressions, out var impressions);
                    long.TryParse(insights.Clicks, out var clicks);
                    decimal.TryParse(insights.Spend, out var spend);

                    await _campaignRepository.UpdateCampaignInsightsAsync(campaign.Id, impressions, clicks, spend, 0, cancellationToken);

                    _logger.LogInformation("Synced insights for campaign {CampaignId}: impressions={Impressions}, clicks={Clicks}, spend={Spend}",
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
                return GenericResponse<AdCampaignResponseDto>.CreateError("Failed to sync insights.");
            }
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> DuplicateAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var original = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (original == null || original.WorkspaceId != workspaceId)
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);

            var campaign = new AdCampaign
            {
                WorkspaceId = original.WorkspaceId,
                ProfileId = original.ProfileId,
                BrandId = original.BrandId,
                ProductId = original.ProductId,
                ContentId = original.ContentId,
                Targeting = original.Targeting,
                AdAccountId = original.AdAccountId,
                Platform = original.Platform,
                Name = $"{original.Name} (copy)",
                Objective = original.Objective,
                Budget = original.Budget,
                LandingUrl = original.LandingUrl,
                IsActive = false
            };

            var created = await _campaignRepository.AddAsync(campaign, cancellationToken);
            var refreshed = await _campaignRepository.GetByIdAsync(created.Id, cancellationToken);
            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? created), "Campaign duplicated successfully");
        }

        private async Task DeployCampaignAsync(AdCampaign campaign, CancellationToken cancellationToken)
        {
            var provider = GetProvider(campaign.Platform);
            var (account, integration, pageId, igActorId) = await ResolveSocialContextAsync(campaign, cancellationToken);

            if (campaign.DeploymentStep < StepCampaignCreated)
            {
                try
                {
                    var fbCampaignId = await provider.CreateCampaignAsync(
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
                    _logger.LogInformation("Deploy step 1/4: Campaign {FbId} created on {Platform}", fbCampaignId, campaign.Platform);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Step 1/4 (Create Campaign) failed: {ex.Message}", ex);
                }
            }

            var adSet = await _campaignRepository.GetAdSetByCampaignIdAsync(campaign.Id, cancellationToken);
            if (campaign.DeploymentStep < StepAdSetCreated || adSet == null || string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
            {
                try
                {
                    var adSetName = $"{campaign.Name} - Ad Set";
                    var fbAdSetId = await provider.CreateAdSetAsync(
                        campaign.AdAccountId,
                        account.AccessToken,
                        campaign.FacebookCampaignId!,
                        adSetName,
                        campaign.Objective ?? "AWARENESS",
                        campaign.Budget.HasValue ? CalculateDailyBudget(campaign.Budget.Value, campaign.StartDate, campaign.EndDate) : null,
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
                        DailyBudget = campaign.Budget.HasValue ? CalculateDailyBudget(campaign.Budget.Value, campaign.StartDate, campaign.EndDate) : null,
                        Status = "PAUSED",
                        StartDate = campaign.StartDate,
                        EndDate = campaign.EndDate
                    };
                    await _campaignRepository.AddAdSetAsync(adSet, cancellationToken);
                    campaign.DeploymentStep = StepAdSetCreated;
                    await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.InProgress, StepAdSetCreated, cancellationToken);
                    _logger.LogInformation("Deploy step 2/4: Ad Set {FbId} created on {Platform}", fbAdSetId, campaign.Platform);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Step 2/4 (Create Ad Set) failed: {ex.Message}", ex);
                }
            }
            else
            {
                _logger.LogInformation("Deploy step 2/4: Reusing existing ad set {AdSetId}", adSet.Id);
            }

            AdCreative? creative = null;
            var existingAd = adSet.Id != Guid.Empty ? await _campaignRepository.GetAdByAdSetIdAsync(adSet.Id, cancellationToken) : null;
            if (existingAd?.CreativeId != null)
            {
                creative = await _campaignRepository.GetCreativeByIdAsync(existingAd.CreativeId, cancellationToken);
            }

            if (campaign.DeploymentStep < StepAdCreativeCreated || creative == null || string.IsNullOrWhiteSpace(creative.CreativeId))
            {
                try
                {
                    Content? content = null;

                    if (campaign.ContentId.HasValue)
                    {
                        content = await _contentRepository.GetByIdAsync(campaign.ContentId.Value, cancellationToken);
                    }

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
                    var linkUrl = campaign.LandingUrl ?? $"https://www.facebook.com/{pageId}";
                    var imageUrl = content?.ImageUrl;

                string? instagramMediaId = null;
                string? instagramActorId = igActorId;
                if (campaign.Platform == "instagram" && content != null)
                {
                    var posts = await _postRepository.GetPublishedByContentIdAsync(content.Id, cancellationToken);
                    var igPost = posts.FirstOrDefault(p => p.Integration?.Platform == SocialPlatformEnum.Instagram && !string.IsNullOrWhiteSpace(p.ExternalPostId));
                    if (igPost != null)
                    {
                        instagramMediaId = igPost.ExternalPostId;
                        instagramActorId = igPost.Integration?.ExternalId ?? igActorId;
                    }
                }

                    var fbCreativeId = await provider.CreateAdCreativeAsync(
                        campaign.AdAccountId,
                        account.AccessToken,
                        pageId,
                        message,
                        linkUrl,
                        imageUrl,
                        null,
                        instagramMediaId,
                        instagramActorId,
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

                    campaign.LandingUrl = linkUrl;
                    await _campaignRepository.AddAdCreativeAsync(creative, cancellationToken);
                    campaign.DeploymentStep = StepAdCreativeCreated;
                    await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.InProgress, StepAdCreativeCreated, cancellationToken);
                    _logger.LogInformation("Deploy step 3/4: Ad Creative {FbId} created on {Platform}", fbCreativeId, campaign.Platform);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Step 3/4 (Create Ad Creative) failed: {ex.Message}", ex);
                }
            }
            else
            {
                _logger.LogInformation("Deploy step 3/4: Reusing existing creative {CreativeId}", creative.Id);
            }

            if (campaign.DeploymentStep < StepAdCreated || existingAd == null || string.IsNullOrWhiteSpace(existingAd.AdId))
            {
                try
                {
                    var adName = $"{campaign.Name} - Ad";
                    var fbAdId = await provider.CreateAdAsync(
                        campaign.AdAccountId,
                        account.AccessToken,
                        adSet.FacebookAdSetId!,
                        creative!.CreativeId!,
                        adName,
                        "PAUSED",
                        cancellationToken
                    );

                    var ad = new Ad
                    {
                        AdSetId = adSet.Id,
                        CreativeId = creative.Id,
                        AdId = fbAdId,
                        Status = "PAUSED"
                    };
                    await _campaignRepository.AddAdAsync(ad, cancellationToken);

                    campaign.DeploymentStatus = DeploymentStatusEnum.Completed;
                    campaign.DeploymentStep = StepAdCreated;
                    await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.Completed, StepAdCreated, cancellationToken);
                    _logger.LogInformation("Deploy step 4/4: Ad {FbId} created on {Platform} — deployment complete", fbAdId, campaign.Platform);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Step 4/4 (Create Ad) failed: {ex.Message}", ex);
                }
            }
            else
            {
                _logger.LogInformation("Deploy step 4/4: Ad already exists, marking complete");
                campaign.DeploymentStatus = DeploymentStatusEnum.Completed;
                campaign.DeploymentStep = StepAdCreated;
                await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.Completed, StepAdCreated, cancellationToken);
            }
        }

        private async Task<(SocialAccountDto Account, SocialIntegrationDto Integration, string PageId, string? InstagramActorId)> ResolveSocialContextAsync(AdCampaign campaign, CancellationToken cancellationToken)
        {
            var integrations = await _socialService.GetIntegrationsByBrandAsync(campaign.ProfileId, campaign.BrandId, cancellationToken);

            var fbIntegration = integrations.FirstOrDefault(i => string.Equals(i.Platform, "facebook", StringComparison.OrdinalIgnoreCase));
            if (fbIntegration == null)
            {
                throw new InvalidOperationException("No Facebook page connected for this brand. Please connect a Facebook page in Social Accounts first.");
            }

            var accounts = await _socialService.GetProfileAccountsAsync(campaign.ProfileId, cancellationToken);
            var account = accounts.FirstOrDefault(a => a.Provider == "facebook");
            if (account == null)
            {
                throw new InvalidOperationException("No Facebook account connected. Please connect your Facebook account first.");
            }

            var adToken = await _socialService.GetFacebookUserAccessTokenAsync(campaign.ProfileId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(adToken)) account.AccessToken = adToken;

            string? instagramActorId = null;
            if (campaign.Platform == "instagram")
            {
                var igIntegration = integrations.FirstOrDefault(i => string.Equals(i.Platform, "instagram", StringComparison.OrdinalIgnoreCase));
                if (igIntegration == null)
                    throw new InvalidOperationException("No Instagram account connected. Please connect your Instagram Business account in Social Accounts first.");

                if (string.IsNullOrWhiteSpace(igIntegration.ExternalId))
                    throw new InvalidOperationException("Instagram account is not properly linked. Please reconnect your Instagram Business account in Social Accounts.");

                instagramActorId = igIntegration.ExternalId;
            }

            return (account, fbIntegration, fbIntegration.ExternalId ?? string.Empty, instagramActorId);
        }

        private async Task<(bool Success, string Message)> EnsureWorkspaceMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken)
        {
            var membership = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
            return membership == null
                ? (false, "You are not allowed to access this workspace")
                : (true, string.Empty);
        }

        private static decimal CalculateDailyBudget(decimal totalBudget, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && endDate.Value > startDate.Value)
            {
                var days = (int)Math.Max(1, (endDate.Value - startDate.Value).TotalDays);
                return totalBudget / days;
            }
            return totalBudget / 30;
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
                Platform = campaign.Platform,
                Name = campaign.Name,
                Objective = campaign.Objective,
                Budget = campaign.Budget,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                IsActive = campaign.IsActive,
                IsDeleted = campaign.IsDeleted,
                LandingUrl = campaign.LandingUrl,
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
