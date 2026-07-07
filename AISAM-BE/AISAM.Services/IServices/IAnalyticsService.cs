using AISAM.Common;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IAnalyticsService
{
    Task<GenericResponse<AnalyticsOverviewDto>> GetOverviewAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<AnalyticsTimeSeriesDto>> GetTimeSeriesAsync(Guid workspaceId, DateTime from, DateTime to, string granularity = "day", string? metrics = null, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<List<AnalyticsChannelBreakdownDto>>> GetChannelBreakdownAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<AnalyticsCampaignBreakdownDto>> GetCampaignBreakdownAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, int page = 1, int pageSize = 20, string? sortBy = "impressions", bool sortDescending = true, CancellationToken cancellationToken = default);
    Task<GenericResponse<AnalyticsTopPostsDto>> GetTopPostsAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, string? metric = "engagement", int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<GenericResponse<AnalyticsSyncStatusDto>> GetSyncStatusAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<UsageBreakdownDto>> GetUsageBreakdownAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<string>> GetAiRecommendationsAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<AudienceBreakdownDto>> GetAudienceBreakdownAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
