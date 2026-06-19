using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/credit-usage")]
[Authorize]
public sealed class CreditUsageController : ControllerBase
{
    private readonly ICreditUsageRecordRepository _creditUsageRepository;

    public CreditUsageController(ICreditUsageRecordRepository creditUsageRepository)
    {
        _creditUsageRepository = creditUsageRepository;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<CreditUsageRecordDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _creditUsageRepository.GetPagedByWorkspaceIdAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            new PaginationRequest { Page = page, PageSize = pageSize },
            cancellationToken);

        var mapped = new PagedResult<CreditUsageRecordDto>
        {
            Data = result.Data.Select(MapRecord).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(GenericResponse<PagedResult<CreditUsageRecordDto>>.CreateSuccess(mapped));
    }

    private static CreditUsageRecordDto MapRecord(Data.Model.CreditUsageRecord record)
    {
        return new CreditUsageRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            UserName = record.User?.FullName ?? record.User?.Email ?? "Unknown",
            Action = MapActionName(record.Action),
            FeatureUsed = MapFeatureUsed(record.Action),
            Credits = record.Credits,
            Status = record.Status == CreditUsageStatusEnum.Success ? "Success" : "Failed",
            CreatedAt = record.CreatedAt
        };
    }

    private static string MapActionName(CreditActionEnum action) => action switch
    {
        CreditActionEnum.SubscriptionGrant => "Subscription Grant",
        CreditActionEnum.CreditPackGrant => "Credit Pack Purchase",
        CreditActionEnum.GenerateText => "Generate Text",
        CreditActionEnum.RegenerateText => "Regenerate Text",
        CreditActionEnum.GenerateImage => "Generate Image",
        CreditActionEnum.GenerateVideo => "Generate Video",
        CreditActionEnum.TrendAnalysis => "Trend Analysis",
        CreditActionEnum.CampaignRecommendation => "Campaign Recommendation",
        _ => action.ToString()
    };

    private static string MapFeatureUsed(CreditActionEnum action) => action switch
    {
        CreditActionEnum.SubscriptionGrant => "Subscription",
        CreditActionEnum.CreditPackGrant => "Credit Pack",
        CreditActionEnum.GenerateText => "AI Content",
        CreditActionEnum.RegenerateText => "AI Content",
        CreditActionEnum.GenerateImage => "AI Image",
        CreditActionEnum.GenerateVideo => "AI Video",
        CreditActionEnum.TrendAnalysis => "Analytics",
        CreditActionEnum.CampaignRecommendation => "Campaign",
        _ => "Other"
    };
}
