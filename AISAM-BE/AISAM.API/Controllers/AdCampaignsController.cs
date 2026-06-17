using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/ad-campaigns")]
[Authorize]
public sealed class AdCampaignsController : ControllerBase
{
    private readonly IAdCampaignService _adCampaignService;

    public AdCampaignsController(IAdCampaignService adCampaignService)
    {
        _adCampaignService = adCampaignService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<AdCampaignDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] Guid? brandId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adCampaignService.GetPagedByWorkspaceAsync(GetWorkspaceId(), new PaginationRequest
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        }, brandId, isActive, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{campaignId:guid}")]
    public async Task<ActionResult<GenericResponse<AdCampaignDto>>> GetById(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var result = await _adCampaignService.GetByIdInWorkspaceAsync(GetWorkspaceId(), campaignId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<AdCampaignDto>>> Create([FromBody] CreateAdCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _adCampaignService.CreateInWorkspaceAsync(GetWorkspaceId(), GetProfileId(), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{campaignId:guid}")]
    public async Task<ActionResult<GenericResponse<AdCampaignDto>>> Update(Guid campaignId, [FromBody] UpdateAdCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _adCampaignService.UpdateInWorkspaceAsync(GetWorkspaceId(), campaignId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{campaignId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> Delete(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var result = await _adCampaignService.DeleteInWorkspaceAsync(GetWorkspaceId(), campaignId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{campaignId:guid}/sync")]
    public async Task<ActionResult<GenericResponse<AdCampaignDto>>> Sync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var result = await _adCampaignService.SyncInWorkspaceAsync(GetWorkspaceId(), campaignId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetProfileId() => ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
    private Guid GetWorkspaceId() => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
}
