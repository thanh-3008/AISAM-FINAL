using AISAM.Common.Models;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IPerformanceReportRepository
{
    Task<int> CountByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<AnalyticsTotals> GetAggregatedTotalsAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default);
    Task<AnalyticsSparklines> GetSparklinesAsync(Guid workspaceId, DateTime from, DateTime to, int days = 7, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default);
    Task<AnalyticsTotals> GetAggregatedTotalsForPreviousPeriodAsync(Guid workspaceId, DateTime currentFrom, DateTime currentTo, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default);
    Task<UsageBreakdownDto> GetUsageBreakdownAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AnalyticsPointDto>> GetDailyTimeSeriesAsync(Guid workspaceId, DateTime from, DateTime to, string[]? metrics = null, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AnalyticsChannelBreakdownDto>> GetChannelBreakdownAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CampaignAnalyticsItemDto> Items, int TotalCount)> GetCampaignBreakdownPagedAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, int page = 1, int pageSize = 20, string? sortBy = "impressions", bool sortDescending = true, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<TopPostItemDto> Items, int TotalCount)> GetTopPostsPagedAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, string? metric = "engagement", int page = 1, int pageSize = 10, bool sortDescending = true, CancellationToken cancellationToken = default);
    Task<AnalyticsTotals> GetAllWorkspaceTotalsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceAnalyticsItemDto>> GetWorkspaceComparisonAsync(DateTime from, DateTime to, int top = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CampaignAnalyticsItemDto>> GetTopCampaignsAllWorkspacesAsync(DateTime from, DateTime to, int top = 20, CancellationToken cancellationToken = default);

    Task<List<Post>> GetPostsNeedingSyncAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<List<Post>> GetPostsNeedingSyncAsync(int batchSize, Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, CancellationToken cancellationToken = default);
    Task UpsertPostReportAsync(PerformanceReport report, CancellationToken cancellationToken = default);
    Task<bool> IncrementTrackedClickAsync(Guid contentId, Guid integrationId, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLatestReportDateForPostAsync(Guid postId, CancellationToken cancellationToken = default);
}
