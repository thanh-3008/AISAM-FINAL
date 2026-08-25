using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private static DateTime ToUtc(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    [HttpGet("overview")]
    public async Task<ActionResult<GenericResponse<AnalyticsOverviewDto>>> GetOverview(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? brandId = null,
        [FromQuery] string? platform = null,
        [FromQuery] Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetOverviewAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            ToUtc(from), ToUtc(to), brandId, platform, campaignId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("time-series")]
    public async Task<ActionResult<GenericResponse<AnalyticsTimeSeriesDto>>> GetTimeSeries(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string granularity = "day",
        [FromQuery] string? metrics = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] string? platform = null,
        [FromQuery] Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetTimeSeriesAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            ToUtc(from), ToUtc(to), granularity, metrics, brandId, platform, campaignId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("scheduled-publishing")]
    public async Task<ActionResult<GenericResponse<ScheduledPublishingPerformanceDto>>> GetScheduledPublishingPerformance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? brandId = null,
        [FromQuery] string? platform = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetScheduledPublishingPerformanceAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            ToUtc(from), ToUtc(to), brandId, platform, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("channel-breakdown")]
    public async Task<ActionResult<GenericResponse<List<AnalyticsChannelBreakdownDto>>>> GetChannelBreakdown(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? brandId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetChannelBreakdownAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            ToUtc(from), ToUtc(to), brandId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("campaign-breakdown")]
    public async Task<ActionResult<GenericResponse<AnalyticsCampaignBreakdownDto>>> GetCampaignBreakdown(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? brandId = null,
        [FromQuery] string? platform = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "impressions",
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetCampaignBreakdownAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            ToUtc(from), ToUtc(to), brandId, platform, page, pageSize, sortBy, sortDescending, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("top-posts")]
    public async Task<ActionResult<GenericResponse<AnalyticsTopPostsDto>>> GetTopPosts(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? brandId = null,
        [FromQuery] string? platform = null,
        [FromQuery] string? metric = "engagement",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetTopPostsAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            ToUtc(from), ToUtc(to), brandId, platform, metric, page, pageSize, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("sync-status")]
    public async Task<ActionResult<GenericResponse<AnalyticsSyncStatusDto>>> GetSyncStatus(
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetSyncStatusAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("usage-breakdown")]
    public async Task<ActionResult<GenericResponse<UsageBreakdownDto>>> GetUsageBreakdown(
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetUsageBreakdownAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("ai-recommendations")]
    public async Task<ActionResult<GenericResponse<string>>> GetAiRecommendations(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? brandId = null,
        [FromQuery] string? platform = null,
        [FromQuery] bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetAiRecommendationsAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            ToUtc(from), ToUtc(to), brandId, platform, forceRefresh, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("audience")]
    public async Task<ActionResult<GenericResponse<AudienceBreakdownDto>>> GetAudienceBreakdown(
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetAudienceBreakdownAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
