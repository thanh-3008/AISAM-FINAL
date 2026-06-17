using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class AdCampaignService : IAdCampaignService
{
    private readonly IAdCampaignRepository _campaignRepository;
    private readonly IBrandRepository _brandRepository;

    public AdCampaignService(IAdCampaignRepository campaignRepository, IBrandRepository brandRepository)
    {
        _campaignRepository = campaignRepository;
        _brandRepository = brandRepository;
    }

    public async Task<GenericResponse<PagedResult<AdCampaignDto>>> GetPagedByWorkspaceAsync(
        Guid workspaceId,
        PaginationRequest request,
        Guid? brandId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        if (brandId.HasValue && !await BrandBelongsToWorkspaceAsync(brandId.Value, workspaceId, cancellationToken))
        {
            return GenericResponse<PagedResult<AdCampaignDto>>.CreateError("Brand not found.", HttpStatusCode.NotFound);
        }

        var result = await _campaignRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, brandId, isActive, cancellationToken);
        return GenericResponse<PagedResult<AdCampaignDto>>.CreateSuccess(new PagedResult<AdCampaignDto>
        {
            Data = result.Data.Select(Map).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        }, "Ad campaigns retrieved successfully.");
    }

    public async Task<GenericResponse<AdCampaignDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);
        return campaign == null || campaign.WorkspaceId != workspaceId
            ? GenericResponse<AdCampaignDto>.CreateError("Ad campaign not found.", HttpStatusCode.NotFound)
            : GenericResponse<AdCampaignDto>.CreateSuccess(Map(campaign), "Ad campaign retrieved successfully.");
    }

    public async Task<GenericResponse<AdCampaignDto>> CreateInWorkspaceAsync(Guid workspaceId, Guid profileId, CreateAdCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateRequestAsync(workspaceId, request.BrandId, request.Name, request.Budget, request.StartDate, request.EndDate, cancellationToken);
        if (!validation.Success) return GenericResponse<AdCampaignDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);

        var campaign = new AdCampaign
        {
            WorkspaceId = workspaceId,
            ProfileId = profileId,
            BrandId = request.BrandId,
            AdAccountId = string.IsNullOrWhiteSpace(request.AdAccountId) ? $"act_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}" : request.AdAccountId.Trim(),
            Name = request.Name.Trim(),
            Objective = NormalizeObjective(request.Objective),
            Budget = request.Budget,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = false
        };

        await _campaignRepository.AddAsync(campaign, cancellationToken);
        var loaded = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken) ?? campaign;
        return GenericResponse<AdCampaignDto>.CreateSuccess(Map(loaded), "Ad campaign created successfully.");
    }

    public async Task<GenericResponse<AdCampaignDto>> UpdateInWorkspaceAsync(Guid workspaceId, Guid campaignId, UpdateAdCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);
        if (campaign == null || campaign.WorkspaceId != workspaceId)
        {
            return GenericResponse<AdCampaignDto>.CreateError("Ad campaign not found.", HttpStatusCode.NotFound);
        }

        var brandId = request.BrandId ?? campaign.BrandId;
        var name = request.Name ?? campaign.Name;
        var budget = request.Budget ?? campaign.Budget;
        var startDate = request.StartDate ?? campaign.StartDate;
        var endDate = request.EndDate ?? campaign.EndDate;
        var validation = await ValidateRequestAsync(workspaceId, brandId, name, budget, startDate, endDate, cancellationToken);
        if (!validation.Success) return GenericResponse<AdCampaignDto>.CreateError(validation.Message!, (HttpStatusCode)validation.StatusCode);

        campaign.BrandId = brandId;
        if (request.AdAccountId != null) campaign.AdAccountId = request.AdAccountId.Trim();
        if (request.Name != null) campaign.Name = request.Name.Trim();
        if (request.Objective != null) campaign.Objective = NormalizeObjective(request.Objective);
        campaign.Budget = budget;
        campaign.StartDate = startDate;
        campaign.EndDate = endDate;
        ApplyStatus(campaign, request.Status);

        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        var loaded = await _campaignRepository.GetByIdAsync(campaign.Id, cancellationToken) ?? campaign;
        return GenericResponse<AdCampaignDto>.CreateSuccess(Map(loaded), "Ad campaign updated successfully.");
    }

    public async Task<GenericResponse<bool>> DeleteInWorkspaceAsync(Guid workspaceId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);
        if (campaign == null || campaign.WorkspaceId != workspaceId)
        {
            return GenericResponse<bool>.CreateError("Ad campaign not found.", HttpStatusCode.NotFound);
        }

        campaign.IsDeleted = true;
        campaign.IsActive = false;
        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Ad campaign deleted successfully.");
    }

    public async Task<GenericResponse<AdCampaignDto>> SyncInWorkspaceAsync(Guid workspaceId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);
        if (campaign == null || campaign.WorkspaceId != workspaceId)
        {
            return GenericResponse<AdCampaignDto>.CreateError("Ad campaign not found.", HttpStatusCode.NotFound);
        }

        if (string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
        {
            campaign.FacebookCampaignId = $"fb_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
        campaign.IsActive = true;
        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        return GenericResponse<AdCampaignDto>.CreateSuccess(Map(campaign), "Ad campaign synced successfully.");
    }

    private async Task<GenericResponse<bool>> ValidateRequestAsync(Guid workspaceId, Guid brandId, string? name, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name)) return GenericResponse<bool>.CreateError("Campaign name is required.", HttpStatusCode.BadRequest);
        if (budget.HasValue && budget.Value < 0) return GenericResponse<bool>.CreateError("Budget must be greater than or equal to zero.", HttpStatusCode.BadRequest);
        if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date) return GenericResponse<bool>.CreateError("End date must be after start date.", HttpStatusCode.BadRequest);
        if (!await BrandBelongsToWorkspaceAsync(brandId, workspaceId, cancellationToken)) return GenericResponse<bool>.CreateError("Brand not found.", HttpStatusCode.NotFound);
        return GenericResponse<bool>.CreateSuccess(true);
    }

    private async Task<bool> BrandBelongsToWorkspaceAsync(Guid brandId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
        return brand != null && !brand.IsDeleted && brand.WorkspaceId == workspaceId;
    }

    private static void ApplyStatus(AdCampaign campaign, string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return;
        var normalized = status.Trim().ToUpperInvariant();
        if (normalized is "ACTIVE")
        {
            campaign.IsActive = true;
            campaign.FacebookCampaignId ??= $"fb_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
        else if (normalized is "PAUSED" or "DRAFT" or "COMPLETED")
        {
            campaign.IsActive = false;
        }
    }

    private static string NormalizeObjective(string? objective)
        => string.IsNullOrWhiteSpace(objective) ? "TRAFFIC" : objective.Trim().ToUpperInvariant();

    private static AdCampaignDto Map(AdCampaign campaign)
    {
        var status = campaign.EndDate.HasValue && campaign.EndDate.Value.Date < DateTime.UtcNow.Date
            ? "COMPLETED"
            : campaign.IsActive
                ? "ACTIVE"
                : string.IsNullOrWhiteSpace(campaign.FacebookCampaignId) ? "DRAFT" : "PAUSED";

        return new AdCampaignDto
        {
            Id = campaign.Id,
            WorkspaceId = campaign.WorkspaceId,
            BrandId = campaign.BrandId,
            BrandName = campaign.Brand?.Name,
            AdAccountId = campaign.AdAccountId,
            FacebookCampaignId = campaign.FacebookCampaignId,
            Name = campaign.Name,
            Objective = campaign.Objective ?? "TRAFFIC",
            Budget = campaign.Budget,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Status = status,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt,
            AdSets = campaign.AdSets.Where(a => !a.IsDeleted).Select(a => new AdSetDto
            {
                Id = a.Id,
                Name = a.Name,
                FacebookAdSetId = a.FacebookAdSetId,
                DailyBudget = a.DailyBudget,
                Status = a.Status ?? "PAUSED"
            }).ToList()
        };
    }
}
