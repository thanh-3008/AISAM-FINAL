namespace AISAM.Common.Models;

public sealed class AnalyticsFilters
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public Guid? BrandId { get; set; }
    public string? Platform { get; set; }
    public Guid? CampaignId { get; set; }
}

public class AnalyticsTotals
{
    public long Impressions { get; set; }
    public long Reach { get; set; }
    public long Engagement { get; set; }
    public long Clicks { get; set; }
    public long Conversions { get; set; }
    public decimal Ctr { get; set; }
    public decimal Spend { get; set; }
    public decimal EstimatedRevenue { get; set; }
    public int PublishedPosts { get; set; }
    public int ActiveCampaigns { get; set; }
}

public sealed class AnalyticsChanges
{
    public decimal ImpressionsPct { get; set; }
    public decimal EngagementPct { get; set; }
    public decimal CtrPct { get; set; }
    public decimal SpendPct { get; set; }
    public decimal ClicksPct { get; set; }
    public decimal ConversionRatePct { get; set; }
    public decimal CpaPct { get; set; }
    public decimal RoasPct { get; set; }
}

public sealed class AnalyticsSparklines
{
    public IReadOnlyList<decimal> Impressions { get; set; } = [];
    public IReadOnlyList<decimal> Engagement { get; set; } = [];
    public IReadOnlyList<decimal> Clicks { get; set; } = [];
    public IReadOnlyList<decimal> Conversions { get; set; } = [];
    public IReadOnlyList<decimal> Ctr { get; set; } = [];
    public IReadOnlyList<decimal> Spend { get; set; } = [];
}

public sealed class UsageBreakdownDto
{
    public IReadOnlyList<UsageBreakdownItemDto> Items { get; set; } = [];
}

public sealed class UsageBreakdownItemDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public sealed class AudienceBreakdownDto
{
    public IReadOnlyList<GeographicItemDto> Geographic { get; set; } = [];
    public IReadOnlyList<DemographicItemDto> Demographics { get; set; } = [];
    public IReadOnlyList<DeviceItemDto> Devices { get; set; } = [];
}

public sealed class GeographicItemDto
{
    public string Country { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int Count { get; set; }
}

public sealed class DemographicItemDto
{
    public string Group { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int Count { get; set; }
}

public sealed class DeviceItemDto
{
    public string Device { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
}

public sealed class DataFreshness
{
    public DateTime? LastSyncedAt { get; set; }
    public bool IsPartial { get; set; }
    public string Status { get; set; } = "no_data";
    public IReadOnlyList<string> Sources { get; set; } = [];
    public IReadOnlyList<string> Warnings { get; set; } = [];
}

public sealed class AnalyticsSyncRequest
{
    public Guid? CampaignId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public sealed class AnalyticsSyncResultDto
{
    public string Status { get; set; } = "failed";
    public int CampaignsRequested { get; set; }
    public int CampaignsSucceeded { get; set; }
    public int SnapshotsUpserted { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public IReadOnlyList<string> Warnings { get; set; } = [];
}

public sealed class AnalyticsOverviewDto
{
    public DateRangeDto DateRange { get; set; } = new();
    public AnalyticsTotals Totals { get; set; } = new();
    public AnalyticsChanges Changes { get; set; } = new();
    public AnalyticsSparklines Sparklines { get; set; } = new();
    public DataFreshness DataFreshness { get; set; } = new();
}

public sealed class DateRangeDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public sealed class AnalyticsTimeSeriesDto
{
    public string Granularity { get; set; } = "day";
    public IReadOnlyList<AnalyticsPointDto> Points { get; set; } = [];
}

public sealed class AnalyticsPointDto : AnalyticsTotals
{
    public string Date { get; set; } = string.Empty;
}

public sealed class AnalyticsChannelBreakdownDto
{
    public string Platform { get; set; } = string.Empty;
    public Guid? IntegrationId { get; set; }
    public string? DisplayName { get; set; }
    public long Impressions { get; set; }
    public long Reach { get; set; }
    public long Engagement { get; set; }
    public long Clicks { get; set; }
    public decimal Ctr { get; set; }
    public decimal Spend { get; set; }
    public int PublishedPosts { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

public sealed class AnalyticsCampaignBreakdownDto
{
    public IReadOnlyList<CampaignAnalyticsItemDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public sealed class CampaignAnalyticsItemDto
{
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public string? Objective { get; set; }
    public string? Status { get; set; }
    public decimal? Budget { get; set; }
    public long Impressions { get; set; }
    public long Reach { get; set; }
    public long Engagement { get; set; }
    public long Clicks { get; set; }
    public decimal Ctr { get; set; }
    public decimal Spend { get; set; }
    public decimal EstimatedRevenue { get; set; }
    public long Conversions { get; set; }
    public decimal Cpa { get; set; }
    public decimal Roas { get; set; }
}

public class WorkspaceAnalyticsItemDto
{
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public int PublishedPosts { get; set; }
    public int ActiveCampaigns { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public decimal Spend { get; set; }
    public long Engagement { get; set; }
    public decimal Ctr { get; set; }
    public decimal EstimatedRevenue { get; set; }
    public decimal Roas { get; set; }
}

public class AdminAnalyticsOverviewDto
{
    public AnalyticsTotals Totals { get; set; } = new();
    public AnalyticsSparklines Sparklines { get; set; } = new();
    public IReadOnlyList<WorkspaceAnalyticsItemDto> TopWorkspaces { get; set; } = Array.Empty<WorkspaceAnalyticsItemDto>();
    public IReadOnlyList<CampaignAnalyticsItemDto> TopCampaigns { get; set; } = Array.Empty<CampaignAnalyticsItemDto>();
    public UsageBreakdownDto UsageBreakdown { get; set; } = new();
}

public sealed class AnalyticsTopPostsDto
{
    public IReadOnlyList<TopPostItemDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

public sealed class TopPostItemDto
{
    public Guid PostId { get; set; }
    public Guid? ContentId { get; set; }
    public string? ContentTitle { get; set; }
    public string? BrandName { get; set; }
    public string Platform { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public string? ExternalPostId { get; set; }
    public long Impressions { get; set; }
    public long Reach { get; set; }
    public long Engagement { get; set; }
    public long Clicks { get; set; }
    public decimal Ctr { get; set; }
}

public sealed class AnalyticsSyncStatusDto
{
    public IReadOnlyList<ProviderSyncStatusDto> Providers { get; set; } = [];
}

public sealed class ProviderSyncStatusDto
{
    public string Platform { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string Status { get; set; } = "not_configured";
    public string? Message { get; set; }
}
