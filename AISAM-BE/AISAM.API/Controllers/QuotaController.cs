using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/quota")]
[Authorize]
public sealed class QuotaController : ControllerBase
{
    private readonly IQuotaService _quotaService;
    private readonly ICreditUsageRecordRepository _creditUsageRecordRepository;

    public QuotaController(
        IQuotaService quotaService,
        ICreditUsageRecordRepository creditUsageRecordRepository)
    {
        _quotaService = quotaService;
        _creditUsageRecordRepository = creditUsageRecordRepository;
    }

    [HttpGet("workspace/current")]
    public async Task<ActionResult<GenericResponse<QuotaSummaryDto>>> GetCurrentWorkspaceQuota(CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var result = await _quotaService.GetWorkspaceSummaryAsync(workspaceId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("workspace/usage-history")]
    public async Task<ActionResult<GenericResponse<PagedResult<CreditUsageRecordDto>>>> GetWorkspaceUsageHistory(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var result = await _creditUsageRecordRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, cancellationToken);
        var mapped = new PagedResult<CreditUsageRecordDto>
        {
            Data = result.Data.Select(MapCreditUsageRecord).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(GenericResponse<PagedResult<CreditUsageRecordDto>>.CreateSuccess(
            mapped,
            "Credit usage history retrieved successfully."));
    }

    private static CreditUsageRecordDto MapCreditUsageRecord(CreditUsageRecord record)
    {
        return new CreditUsageRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            UserName = string.IsNullOrWhiteSpace(record.User.FullName) ? record.User.Email : record.User.FullName!,
            Action = FormatAction(record.Action),
            Credits = record.Credits,
            FeatureUsed = FormatFeature(record.Action),
            Status = record.Status == CreditUsageStatusEnum.Success ? "Success" : record.Status == CreditUsageStatusEnum.Failed ? "Failed" : "Pending",
            CreatedAt = record.CreatedAt
        };
    }

    private static string FormatAction(CreditActionEnum action)
        => action switch
        {
            CreditActionEnum.SubscriptionGrant => "Subscription Grant",
            CreditActionEnum.CreditPackGrant => "Credit Pack Grant",
            CreditActionEnum.GenerateText => "Generate Text",
            CreditActionEnum.RegenerateText => "Regenerate Text",
            CreditActionEnum.GenerateImage => "Generate Image",
            CreditActionEnum.GenerateVideo => "Generate Video",
            CreditActionEnum.TrendAnalysis => "Trend Analysis",
            CreditActionEnum.CampaignRecommendation => "Campaign Recommendation",
            _ => action.ToString()
        };

    private static string FormatFeature(CreditActionEnum action)
        => action switch
        {
            CreditActionEnum.SubscriptionGrant => "Subscription",
            CreditActionEnum.CreditPackGrant => "Credit Pack",
            CreditActionEnum.GenerateText or CreditActionEnum.RegenerateText => "AI Text",
            CreditActionEnum.GenerateImage => "AI Image",
            CreditActionEnum.GenerateVideo => "AI Video",
            CreditActionEnum.TrendAnalysis => "Trend Analysis",
            CreditActionEnum.CampaignRecommendation => "Campaign Recommendation",
            _ => action.ToString()
        };
}
