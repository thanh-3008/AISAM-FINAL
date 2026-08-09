using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Messages;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;

namespace AISAM.Services.Service
{
    public class AdCampaignService : IAdCampaignService
    {
        private const int StepCampaignCreated = 1;
        private const int StepAdSetCreated = 2;
        private const int StepAdCreativeCreated = 3;
        private const int StepAdCreated = 4;

        private const decimal MinDailyBudget = 30000;

        private readonly IAdCampaignRepository _campaignRepository;
        private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IContentRepository _contentRepository;
        private readonly ISocialService _socialService;
        private readonly IPostRepository _postRepository;
        private readonly Dictionary<string, IProviderService> _providers;
        private readonly ILogger<AdCampaignService> _logger;

        public AdCampaignService(
            IAdCampaignRepository campaignRepository,
            IWorkspaceMemberRepository workspaceMemberRepository,
            IWorkspaceRepository workspaceRepository,
            ISubscriptionRepository subscriptionRepository,
            IBrandRepository brandRepository,
            IContentRepository contentRepository,
            ISocialService socialService,
            IPostRepository postRepository,
            IEnumerable<IProviderService> providers,
            ILogger<AdCampaignService> logger)
        {
            _campaignRepository = campaignRepository;
            _workspaceMemberRepository = workspaceMemberRepository;
            _workspaceRepository = workspaceRepository;
            _subscriptionRepository = subscriptionRepository;
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

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrWhiteSpace(request.Name))
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign name is required.");
            if (string.IsNullOrWhiteSpace(request.AdAccountId))
                return GenericResponse<AdCampaignResponseDto>.CreateError("Ad account is required.");
            if (request.Budget.HasValue)
            {
                var currency = request.AdAccountCurrency ?? "VND";
                var minBudget = currency == "USD" ? 100m : 30000m;
                var currencyLabel = currency == "USD" ? "USD" : "VND";
                if (request.Budget.Value < minBudget)
                    return GenericResponse<AdCampaignResponseDto>.CreateError($"Budget must be at least {minBudget:N0} {currencyLabel}.");
            }
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate.Value > request.EndDate.Value)
                return GenericResponse<AdCampaignResponseDto>.CreateError("End date must be after start date.");

            if (request.Variants is { Count: > 0 })
            {
                var totalShare = request.Variants.Sum(v => v.BudgetShare);
                if (totalShare != 100)
                    return GenericResponse<AdCampaignResponseDto>.CreateError($"Variant budget shares must sum to 100% (currently {totalShare}%).");
            }

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
                AdAccountCurrency = request.AdAccountCurrency,
                Platform = string.IsNullOrWhiteSpace(request.Platform) ? "facebook" : request.Platform.ToLowerInvariant(),
                Name = request.Name,
                Objective = request.Objective,
                Budget = request.Budget,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LandingUrl = request.LandingUrl,
                IsActive = false,
                Status = CampaignStatusEnum.Draft
            };

            var created = await _campaignRepository.AddAsync(campaign, cancellationToken);

            if (request.Variants is { Count: > 0 })
            {
                foreach (var variant in request.Variants)
                {
                    var variantBudget = request.Budget.HasValue
                        ? request.Budget.Value * variant.BudgetShare / 100m
                        : (decimal?)null;
                    var adSet = new AdSet
                    {
                        CampaignId = created.Id,
                        Name = $"{request.Name} - {variant.NameSuffix}",
                        Targeting = variant.Targeting,
                        DailyBudget = variantBudget.HasValue ? CalculateDailyBudget(variantBudget.Value, request.StartDate, request.EndDate) : null,
                        Status = "PAUSED",
                        StartDate = request.StartDate,
                        EndDate = request.EndDate
                    };
                    await _campaignRepository.AddAdSetAsync(adSet, cancellationToken);
                }
            }

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

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
            }

            var isDeployed = campaign.DeploymentStatus == DeploymentStatusEnum.Completed && !string.IsNullOrWhiteSpace(campaign.FacebookCampaignId);
            var provider = GetProvider(campaign.Platform);
            var budgetChanged = false;
            var nameChanged = false;

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                campaign.Name = request.Name;
                nameChanged = true;
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
                var currency = campaign.AdAccountCurrency ?? "VND";
                var minBudget = currency == "USD" ? 100m : 30000m;
                var currencyLabel = currency == "USD" ? "USD" : "VND";
                if (request.Budget.Value < minBudget)
                    return GenericResponse<AdCampaignResponseDto>.CreateError($"Budget must be at least {minBudget:N0} {currencyLabel}.");

                campaign.Budget = request.Budget.Value;
                budgetChanged = true;
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

                if (!isDeployed)
                {
                    if (wasActivated)
                        campaign.Status = CampaignStatusEnum.Active;
                    else if (wasDeactivated)
                        campaign.Status = CampaignStatusEnum.Paused;
                }
            }

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

                    campaign.Status = campaign.IsActive ? CampaignStatusEnum.Active : CampaignStatusEnum.Paused;

                    if (nameChanged)
                    {
                        await provider.UpdateCampaignNameAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, campaign.Name, cancellationToken);
                        _logger.LogInformation("Synced name for campaign {CampaignId}", campaign.Id);
                    }

                    if (budgetChanged && campaign.Budget.HasValue)
                    {
                        foreach (var adSet in adSets)
                        {
                            if (!string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                            {
                                var dailyBudget = CalculateDailyBudget(campaign.Budget.Value, campaign.StartDate, campaign.EndDate);
                                await provider.UpdateAdSetBudgetAsync(campaign.AdAccountId, account.AccessToken, adSet.FacebookAdSetId, dailyBudget, cancellationToken);
                                _logger.LogInformation("Synced budget for ad set {AdSetId}", adSet.FacebookAdSetId);
                            }
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
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign must be deployed to Facebook before activation.");
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

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<bool>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
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
            campaign.Status = CampaignStatusEnum.Paused;
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

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<bool>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
            }

            if (!campaign.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Campaign is not deleted");
            }

            campaign.IsDeleted = false;
            campaign.Status = CampaignStatusEnum.Draft;
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

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
            }

            if (campaign.DeploymentStatus == DeploymentStatusEnum.Completed)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(campaign), "Campaign already deployed");
            }

            if (campaign.Budget.HasValue)
            {
                var daily = CalculateDailyBudget(campaign.Budget.Value, campaign.StartDate, campaign.EndDate);
                var minDaily = campaign.AdAccountCurrency == "USD" ? 100m : 30000m;
                if (daily < minDaily)
                    return GenericResponse<AdCampaignResponseDto>.CreateError($"Daily budget ({daily:N0}) is below minimum ({minDaily:N0}). Increase budget or reduce duration.");
            }

            try
            {
                await DeployCampaignAsync(campaign, cancellationToken);
                var refreshed = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken);
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? campaign), "Campaign sent to Meta successfully. AISAM will mark it ready to start once pending review checks pass.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Deploy failed for campaign {CampaignId}", campaign.Id);
                try
                {
                    campaign.DeploymentStatus = DeploymentStatusEnum.Failed;
                    campaign.DeploymentMessage = ex.Message;
                    campaign.Status = CampaignStatusEnum.Rejected;
                    campaign.IsActive = false;
                    await _campaignRepository.UpdateDeploymentFailureAsync(campaign.Id, campaign.DeploymentStep, ex.Message, cancellationToken);
                }
                catch (Exception saveEx)
                {
                    _logger?.LogWarning(saveEx, "Failed to save deployment failure state for campaign {CampaignId}", campaign.Id);
                }
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

        public async Task<GenericResponse<AdCampaignResponseDto>> ActivateAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
                return GenericResponse<AdCampaignResponseDto>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);

            if (campaign.DeploymentStatus != DeploymentStatusEnum.Completed || string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign must be fully deployed before activation.");

            if (campaign.Status != CampaignStatusEnum.Paused && campaign.Status != CampaignStatusEnum.PendingReview)
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign is not in a state that can be activated.");

            try
            {
                var provider = GetProvider(campaign.Platform);
                var (account, _, _, _) = await ResolveSocialContextAsync(campaign, cancellationToken);
                await ActivateCampaignOnMetaAsync(campaign, provider, account, cancellationToken);

                var refreshed = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken);
                return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? campaign), "Campaign activated successfully.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to activate campaign {CampaignId}", campaign.Id);
                return GenericResponse<AdCampaignResponseDto>.CreateError($"Failed to activate campaign: {ex.Message}");
            }
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
                    var conversions = FacebookProvider.ExtractConversions(insights);

                    await _campaignRepository.UpdateCampaignInsightsAsync(campaign.Id, impressions, clicks, spend, conversions, cancellationToken);

                    _logger.LogInformation("Synced insights for campaign {CampaignId}: impressions={Impressions}, clicks={Clicks}, spend={Spend}, conversions={Conversions}",
                        campaign.Id, impressions, clicks, spend, conversions);
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

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
            }

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
                IsActive = false,
                Status = CampaignStatusEnum.Draft
            };

            var created = await _campaignRepository.AddAsync(campaign, cancellationToken);
            var refreshed = await _campaignRepository.GetByIdAsync(created.Id, cancellationToken);
            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(refreshed ?? created), "Campaign duplicated successfully");
        }

        public async Task<GenericResponse<BulkCampaignResultDto>> BulkCreateAsync(Guid workspaceId, Guid userId, BulkCreateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<BulkCampaignResultDto>.CreateError(access.Message);
            }

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<BulkCampaignResultDto>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
            }

            var result = new BulkCampaignResultDto { TotalRequested = request.Items.Count };
            var subscription = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(workspaceId, cancellationToken);

            foreach (var item in request.Items)
            {
                try
                {
                    var createResult = await CreateAsync(workspaceId, userId, item, cancellationToken);
                    if (createResult.Success && createResult.Data != null)
                    {
                        result.SuccessCount++;
                        result.Results.Add(new BulkCampaignItemResult { Success = true, CampaignId = createResult.Data.Id, Campaign = createResult.Data });
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Results.Add(new BulkCampaignItemResult { Success = false, Error = createResult.Message });
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Results.Add(new BulkCampaignItemResult { Success = false, Error = ex.Message });
                    _logger.LogWarning(ex, "Bulk create campaign '{Name}' failed", item.Name);
                }
            }

            return GenericResponse<BulkCampaignResultDto>.CreateSuccess(result,
                result.FailedCount == 0
                    ? $"All {result.SuccessCount} campaigns created successfully."
                    : $"{result.SuccessCount} created, {result.FailedCount} failed.");
        }

        public async Task<GenericResponse<BulkCampaignResultDto>> BulkDeleteAsync(Guid workspaceId, Guid userId, BulkDeleteAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<BulkCampaignResultDto>.CreateError(access.Message);
            }

            var result = new BulkCampaignResultDto { TotalRequested = request.CampaignIds.Count };

            foreach (var campaignId in request.CampaignIds)
            {
                try
                {
                    var deleteResult = await SoftDeleteAsync(campaignId, workspaceId, userId, cancellationToken);
                    if (deleteResult.Success)
                    {
                        result.SuccessCount++;
                        result.Results.Add(new BulkCampaignItemResult { Success = true, CampaignId = campaignId });
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Results.Add(new BulkCampaignItemResult { Success = false, CampaignId = campaignId, Error = deleteResult.Message });
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Results.Add(new BulkCampaignItemResult { Success = false, CampaignId = campaignId, Error = ex.Message });
                    _logger.LogWarning(ex, "Bulk delete campaign {CampaignId} failed", campaignId);
                }
            }

            return GenericResponse<BulkCampaignResultDto>.CreateSuccess(result,
                result.FailedCount == 0
                    ? $"All {result.SuccessCount} campaigns deleted successfully."
                    : $"{result.SuccessCount} deleted, {result.FailedCount} failed.");
        }

        public async Task<GenericResponse<BulkCampaignResultDto>> BulkDeployAsync(Guid workspaceId, Guid userId, BulkDeployAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<BulkCampaignResultDto>.CreateError(access.Message);
            }

            var workspaceBlocked = await CheckWorkspaceReadOnlyAsync(workspaceId, cancellationToken);
            if (workspaceBlocked != null)
            {
                return GenericResponse<BulkCampaignResultDto>.CreateError(workspaceBlocked, HttpStatusCode.Forbidden);
            }

            var result = new BulkCampaignResultDto { TotalRequested = request.CampaignIds.Count };

            foreach (var campaignId in request.CampaignIds)
            {
                try
                {
                    var deployResult = await DeployAsync(campaignId, workspaceId, userId, cancellationToken);
                    if (deployResult.Success && deployResult.Data != null)
                    {
                        result.SuccessCount++;
                        result.Results.Add(new BulkCampaignItemResult { Success = true, CampaignId = campaignId, Campaign = deployResult.Data });
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Results.Add(new BulkCampaignItemResult { Success = false, CampaignId = campaignId, Error = deployResult.Message });
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Results.Add(new BulkCampaignItemResult { Success = false, CampaignId = campaignId, Error = ex.Message });
                    _logger.LogWarning(ex, "Bulk deploy campaign {CampaignId} failed", campaignId);
                }
            }

            return GenericResponse<BulkCampaignResultDto>.CreateSuccess(result,
                result.FailedCount == 0
                    ? $"All {result.SuccessCount} campaigns deployed successfully."
                    : $"{result.SuccessCount} deployed, {result.FailedCount} failed.");
        }

        public async Task<GenericResponse<CampaignPreviewDto>> GetPreviewAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
                return GenericResponse<CampaignPreviewDto>.CreateError("Campaign not found");

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
                return GenericResponse<CampaignPreviewDto>.CreateError(access.Message);

            Content? content = null;
            if (campaign.ContentId.HasValue)
                content = await _contentRepository.GetByIdAsync(campaign.ContentId.Value, cancellationToken);

            if (content == null)
            {
                var contentRequest = new PaginationRequest { Page = 1, PageSize = 1 };
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

            var preview = new CampaignPreviewDto
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                Platform = campaign.Platform,
                Objective = campaign.Objective,
                Message = content?.TextContent ?? campaign.Name,
                ImageUrl = content?.ImageUrl,
                LinkUrl = campaign.LandingUrl,
                CallToAction = "LEARN_MORE",
                Budget = campaign.Budget,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate
            };

            return GenericResponse<CampaignPreviewDto>.CreateSuccess(preview, "Campaign preview retrieved successfully");
        }

        private async Task DeployCampaignAsync(AdCampaign campaign, CancellationToken cancellationToken)
        {
            var provider = GetProvider(campaign.Platform);

            if (string.IsNullOrWhiteSpace(campaign.AdAccountId))
                throw new InvalidOperationException("Ad account is required. Please assign an ad account to this campaign.");

            if (!campaign.Budget.HasValue || campaign.Budget.Value <= 0)
                throw new InvalidOperationException("Budget is required and must be greater than zero.");

            if (campaign.StartDate.HasValue && campaign.EndDate.HasValue && campaign.StartDate.Value > campaign.EndDate.Value)
                throw new InvalidOperationException("End date must be after start date.");

            var (account, integration, pageId, igActorId) = await ResolveSocialContextAsync(campaign, cancellationToken);

            if (string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
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
                    _logger.LogInformation("Deploy: Campaign {FbId} created on {Platform}", fbCampaignId, campaign.Platform);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Create Campaign failed: {ex.Message}", ex);
                }
            }

            var adSetsToDeploy = await ResolveAdSetsAsync(campaign, cancellationToken);

            if (adSetsToDeploy.Count == 0)
            {
                adSetsToDeploy = new List<AdSet> { new AdSet
                {
                    CampaignId = campaign.Id,
                    Name = $"{campaign.Name} - Ad Set",
                    DailyBudget = campaign.Budget.HasValue ? CalculateDailyBudget(campaign.Budget.Value, campaign.StartDate, campaign.EndDate) : null,
                    Status = "PAUSED",
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate
                }};
            }

            foreach (var adSet in adSetsToDeploy)
            {
                if (string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                {
                    try
                    {
                        var dailyBudget = adSet.DailyBudget
                            ?? (campaign.Budget.HasValue ? CalculateDailyBudget(campaign.Budget.Value, campaign.StartDate, campaign.EndDate) : null);
                        var targeting = adSet.Targeting ?? campaign.Targeting ?? "{\"geo_locations\":{\"countries\":[\"VN\"]}}";

                        var fbAdSetId = await provider.CreateAdSetAsync(
                            campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId!,
                            adSet.Name, campaign.Objective ?? "AWARENESS",
                            dailyBudget, campaign.StartDate, campaign.EndDate,
                            targeting, cancellationToken
                        );

                        adSet.FacebookAdSetId = fbAdSetId;
                        await _campaignRepository.AddAdSetAsync(adSet, cancellationToken);
                        _logger.LogInformation("Deploy: Ad Set {FbId} '{Name}' created", fbAdSetId, adSet.Name);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Create Ad Set '{adSet.Name}' failed: {ex.Message}", ex);
                    }
                }

                var existingAd = adSet.Id != Guid.Empty ? await _campaignRepository.GetAdByAdSetIdAsync(adSet.Id, cancellationToken) : null;
                var creative = existingAd?.CreativeId != null ? await _campaignRepository.GetCreativeByIdAsync(existingAd.CreativeId, cancellationToken) : null;

                if (creative == null || string.IsNullOrWhiteSpace(creative.CreativeId))
                {
                    try
                    {
                        var content = await ResolveContentAsync(campaign, cancellationToken);
                        var message = content?.TextContent ?? campaign.Name;
                        var linkUrl = campaign.LandingUrl;
                        if (string.IsNullOrWhiteSpace(linkUrl))
                        {
                            throw new InvalidOperationException(
                                "Landing URL is required for Facebook ad creative. Please provide a public URL for your campaign.");
                        }
                        var imageUrl = content?.ImageUrl;

                        string? instagramMediaId = null;
                        string? instagramActorId = igActorId;
                        string? objectStoryId = null;

                        if (content != null)
                        {
                            var posts = await _postRepository.GetPublishedByContentIdAsync(content.Id, cancellationToken);
                            if (campaign.Platform == "instagram")
                            {
                                var igPost = posts.FirstOrDefault(p => p.Integration?.Platform == SocialPlatformEnum.Instagram && !string.IsNullOrWhiteSpace(p.ExternalPostId));
                                if (igPost != null)
                                {
                                    instagramMediaId = igPost.ExternalPostId;
                                    instagramActorId = igPost.Integration?.ExternalId ?? igActorId;
                                }
                            }
                            else if (campaign.Platform == "facebook")
                            {
                                var fbPost = posts.FirstOrDefault(p => p.Integration?.Platform == SocialPlatformEnum.Facebook && !string.IsNullOrWhiteSpace(p.ExternalPostId));
                                if (fbPost != null)
                                {
                                    objectStoryId = fbPost.ExternalPostId;
                                    _logger.LogInformation("Deploy: Using existing FB post {PostId} as object_story_id", objectStoryId);
                                }
                            }
                        }

                        var fbCreativeId = await provider.CreateAdCreativeAsync(
                            campaign.AdAccountId, account.AccessToken, pageId,
                            message, linkUrl, imageUrl, null,
                            instagramMediaId, instagramActorId, objectStoryId, cancellationToken
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
                        _logger.LogInformation("Deploy: Creative {FbId} for '{Name}' created", fbCreativeId, adSet.Name);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Create Creative for '{adSet.Name}' failed: {ex.Message}", ex);
                    }
                }

                if (existingAd == null || string.IsNullOrWhiteSpace(existingAd.AdId))
                {
                    try
                    {
                        var adName = $"{adSet.Name} - Ad";
                        var fbAdId = await provider.CreateAdAsync(
                            campaign.AdAccountId, account.AccessToken,
                            adSet.FacebookAdSetId!, creative!.CreativeId!,
                            adName, "PAUSED", cancellationToken
                        );

                        var ad = new Ad { AdSetId = adSet.Id, CreativeId = creative.Id, AdId = fbAdId, Status = "PAUSED" };
                        await _campaignRepository.AddAdAsync(ad, cancellationToken);
                        _logger.LogInformation("Deploy: Ad {FbId} for '{Name}' created", fbAdId, adSet.Name);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Create Ad for '{adSet.Name}' failed: {ex.Message}", ex);
                    }
                }
            }

            campaign.DeploymentStatus = DeploymentStatusEnum.Completed;
            campaign.DeploymentStep = StepAdCreated;
            campaign.Status = CampaignStatusEnum.PendingReview;
            campaign.IsActive = false;
            campaign.DeploymentMessage = null;
            try
            {
                await _campaignRepository.UpdateDeploymentStatusAsync(campaign.Id, DeploymentStatusEnum.Completed, StepAdCreated, cancellationToken);
                await _campaignRepository.UpdateCampaignStatusAsync(campaign.Id, CampaignStatusEnum.PendingReview, cancellationToken);
            }
            catch (Exception statusEx)
            {
                _logger?.LogWarning(statusEx, "Failed to save final pending-review state for campaign {CampaignId}. Facebook objects created successfully.", campaign.Id);
            }
            _logger?.LogInformation("Deploy: Campaign {CampaignId} fully deployed in PENDING_REVIEW state with {Count} ad set(s). Awaiting Meta review before activation.", campaign.Id, adSetsToDeploy.Count);
        }

        private async Task<List<AdSet>> ResolveAdSetsAsync(AdCampaign campaign, CancellationToken cancellationToken)
        {
            var existing = await _campaignRepository.GetAdSetsByCampaignIdAsync(campaign.Id, cancellationToken);
            return existing.ToList();
        }

        private async Task<Content?> ResolveContentAsync(AdCampaign campaign, CancellationToken cancellationToken)
        {
            Content? content = null;
            if (campaign.ContentId.HasValue)
                content = await _contentRepository.GetByIdAsync(campaign.ContentId.Value, cancellationToken);

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

            return content;
        }

        private async Task ActivateCampaignOnMetaAsync(AdCampaign campaign, IProviderService provider, SocialAccountDto account, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
                {
                    await provider.UpdateCampaignStatusAsync(campaign.AdAccountId, account.AccessToken, campaign.FacebookCampaignId, "ACTIVE", cancellationToken);

                    var adSets = await _campaignRepository.GetAdSetsByCampaignIdAsync(campaign.Id, cancellationToken);
                    foreach (var adSet in adSets)
                    {
                        if (!string.IsNullOrWhiteSpace(adSet.FacebookAdSetId))
                        {
                            await provider.UpdateAdSetStatusAsync(campaign.AdAccountId, account.AccessToken, adSet.FacebookAdSetId, "ACTIVE", cancellationToken);
                        }
                        var ads = await _campaignRepository.GetAdsByAdSetIdAsync(adSet.Id, cancellationToken);
                        foreach (var ad in ads)
                        {
                            if (!string.IsNullOrWhiteSpace(ad.AdId))
                            {
                                await provider.UpdateAdStatusAsync(campaign.AdAccountId, account.AccessToken, ad.AdId, "ACTIVE", cancellationToken);
                            }
                        }
                    }

                    _logger.LogInformation("Activated campaign {CampaignId} on {Platform}", campaign.Id, campaign.Platform);
                }

                campaign.IsActive = true;
                campaign.Status = CampaignStatusEnum.Active;
                await _campaignRepository.UpdateAsync(campaign, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to activate campaign {CampaignId} on {Platform}. Campaign remains paused.", campaign.Id, campaign.Platform);
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

        private async Task<string?> CheckWorkspaceReadOnlyAsync(Guid workspaceId, CancellationToken cancellationToken)
        {
            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
            if (workspace != null)
            {
                WorkspaceLifecyclePolicy.SynchronizeStatus(workspace, DateTime.UtcNow);
                if (WorkspaceLifecyclePolicy.IsReadOnly(workspace.Status))
                {
                    return MessageConstants.Campaign.WorkspaceExpiredOrInactive;
                }
            }
            return null;
        }

        private static decimal CalculateDailyBudget(decimal totalBudget, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && endDate.Value > startDate.Value)
            {
                var days = (int)Math.Max(1, (endDate.Value - startDate.Value).TotalDays);
                return Math.Max(totalBudget / days, MinDailyBudget);
            }
            return Math.Max(totalBudget / 30, MinDailyBudget);
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
                AdAccountCurrency = campaign.AdAccountCurrency,
                FacebookCampaignId = campaign.FacebookCampaignId,
                Platform = campaign.Platform,
                Name = campaign.Name,
                Objective = campaign.Objective,
                Budget = campaign.Budget,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                IsActive = campaign.IsActive,
                IsDeleted = campaign.IsDeleted,
                Status = campaign.Status,
                LandingUrl = campaign.LandingUrl,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                DeploymentStatus = campaign.DeploymentStatus,
                DeploymentStep = campaign.DeploymentStep,
                DeploymentMessage = campaign.DeploymentMessage,
                AdSets = MapAdSetsWithMetrics(campaign),
                Impressions = campaign.Impressions,
                Clicks = campaign.Clicks,
                Spend = campaign.Spend,
                Conversions = campaign.Conversions
            };
        }

        private static List<AdSetSummaryDto> MapAdSetsWithMetrics(AdCampaign campaign)
        {
            var activeAdSets = campaign.AdSets.Where(ads => !ads.IsDeleted).ToList();
            var count = activeAdSets.Count;
            if (count == 0) return new List<AdSetSummaryDto>();

            var impressionsPerSet = count > 1 ? campaign.Impressions / count : campaign.Impressions;
            var clicksPerSet = count > 1 ? campaign.Clicks / count : campaign.Clicks;
            var spendPerSet = count > 1 ? campaign.Spend / count : campaign.Spend;

            return activeAdSets.Select(ads => new AdSetSummaryDto
            {
                Id = ads.Id,
                Name = ads.Name,
                FacebookAdSetId = ads.FacebookAdSetId,
                DailyBudget = ads.DailyBudget,
                Status = ads.Status,
                Impressions = impressionsPerSet,
                Clicks = clicksPerSet,
                Spend = spendPerSet,
                Ads = ads.Ads.Where(a => !a.IsDeleted).Select(a => new AdSummaryDto
                {
                    Id = a.Id,
                    AdId = a.AdId,
                    Status = a.Status,
                    CreativeId = a.Creative?.CreativeId,
                    CallToAction = a.Creative?.CallToAction,
                    LinkUrl = a.Creative?.LinkUrl
                }).ToList()
            }).ToList();
        }
    }
}
