using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class PerformanceReportRepository : IPerformanceReportRepository
{
    private readonly AisamContext _context;

    public PerformanceReportRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<int> CountByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReports
            .Include(report => report.Post)
                .ThenInclude(post => post!.Content)
            .Where(report =>
                !report.IsDeleted &&
                report.Post != null &&
                report.Post.Content != null &&
                report.Post.Content.ProfileId == profileId)
            .CountAsync(cancellationToken);
    }

    public async Task<AnalyticsTotals> GetAggregatedTotalsAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, string? platform = null, Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var postsQuery = _context.Posts
            .Where(p => !p.IsDeleted
                && p.PublishedAt >= from && p.PublishedAt <= to
                && p.Content != null && !p.Content.IsDeleted
                && p.Content.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            postsQuery = postsQuery.Where(p => p.Content!.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(platform))
        {
            var platformEnum = ParsePlatform(platform);
            postsQuery = postsQuery.Where(p => p.Integration.Platform == platformEnum);
        }

        var campaignsQuery = _context.AdCampaigns
            .Where(c => !c.IsDeleted && c.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.BrandId == brandId.Value);

        if (campaignId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.Id == campaignId.Value);

        var publishedPosts = await postsQuery.CountAsync(cancellationToken);
        var activeCampaigns = await campaignsQuery
            .Where(c => c.IsActive && c.DeploymentStatus == DeploymentStatusEnum.None && (c.EndDate == null || c.EndDate >= from))
            .CountAsync(cancellationToken);

        var snapshotQuery = _context.CampaignInsightSnapshots
            .Where(snapshot => snapshot.WorkspaceId == workspaceId
                && snapshot.SnapshotDate >= from.Date
                && snapshot.SnapshotDate <= to.Date);
        if (brandId.HasValue)
            snapshotQuery = snapshotQuery.Where(snapshot => snapshot.Campaign.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            snapshotQuery = snapshotQuery.Where(snapshot => snapshot.Platform == platform.ToLower());
        if (campaignId.HasValue)
            snapshotQuery = snapshotQuery.Where(snapshot => snapshot.CampaignId == campaignId.Value);

        var campaignAgg = await snapshotQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Impressions = g.Sum(snapshot => snapshot.Impressions),
                Reach = g.Sum(snapshot => snapshot.Reach ?? 0),
                Clicks = g.Sum(snapshot => snapshot.Clicks),
                Spend = g.Sum(snapshot => snapshot.Spend),
                Conversions = g.Sum(snapshot => snapshot.Conversions ?? 0),
                Revenue = g.Sum(snapshot => snapshot.AttributedRevenue ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var perfAgg = await _context.PerformanceReports
            .Where(pr => !pr.IsDeleted
                && pr.ReportDate >= from && pr.ReportDate <= to
                && pr.Post != null && !pr.Post.IsDeleted
                && pr.Post.Content != null && !pr.Post.Content.IsDeleted
                && pr.Post.Content.WorkspaceId == workspaceId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Impressions = g.Sum(pr => pr.Impressions),
                Engagement = g.Sum(pr => pr.Engagement),
                Ctr = g.Average(pr => pr.Ctr),
                Revenue = g.Sum(pr => pr.EstimatedRevenue)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new AnalyticsTotals
        {
            Impressions = (campaignAgg?.Impressions ?? 0) + (perfAgg?.Impressions ?? 0),
            Engagement = perfAgg?.Engagement ?? 0,
            Clicks = campaignAgg?.Clicks ?? 0,
            Conversions = (long)(campaignAgg?.Conversions ?? 0),
            Ctr = campaignAgg?.Clicks > 0
                ? Math.Round((decimal)campaignAgg.Clicks / (campaignAgg.Impressions > 0 ? campaignAgg.Impressions : 1) * 100, 2)
                : (perfAgg?.Ctr != null ? Math.Round(perfAgg.Ctr, 2) : 0),
            Spend = campaignAgg?.Spend ?? 0,
            EstimatedRevenue = (campaignAgg?.Revenue ?? 0) + (perfAgg?.Revenue ?? 0),
            PublishedPosts = publishedPosts,
            ActiveCampaigns = activeCampaigns,
            Reach = campaignAgg?.Reach ?? 0
        };
    }

    public async Task<IReadOnlyList<AnalyticsPointDto>> GetDailyTimeSeriesAsync(
        Guid workspaceId, DateTime from, DateTime to,
        string[]? metrics = null, Guid? brandId = null, string? platform = null, Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var days = Enumerable.Range(0, (to.Date - from.Date).Days + 1)
            .Select(d => from.Date.AddDays(d))
            .ToList();

        var postsQuery = _context.Posts
            .Where(p => !p.IsDeleted
                && p.PublishedAt >= from && p.PublishedAt <= to
                && p.Content != null && !p.Content.IsDeleted
                && p.Content.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            postsQuery = postsQuery.Where(p => p.Content!.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            postsQuery = postsQuery.Where(p => p.Integration.Platform == ParsePlatform(platform));

        var postsByDay = await postsQuery
            .GroupBy(p => p.PublishedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var snapshotsQuery = _context.CampaignInsightSnapshots
            .Where(snapshot => snapshot.WorkspaceId == workspaceId
                && snapshot.SnapshotDate >= from.Date
                && snapshot.SnapshotDate <= to.Date);
        if (brandId.HasValue)
            snapshotsQuery = snapshotsQuery.Where(snapshot => snapshot.Campaign.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            snapshotsQuery = snapshotsQuery.Where(snapshot => snapshot.Platform == platform.ToLower());
        if (campaignId.HasValue)
            snapshotsQuery = snapshotsQuery.Where(snapshot => snapshot.CampaignId == campaignId.Value);

        var snapshotsByDay = await snapshotsQuery
            .GroupBy(snapshot => snapshot.SnapshotDate)
            .Select(g => new
            {
                Date = g.Key,
                Impressions = g.Sum(c => c.Impressions),
                Reach = g.Sum(c => c.Reach ?? 0),
                Clicks = g.Sum(c => c.Clicks),
                Spend = g.Sum(c => c.Spend),
                Conversions = g.Sum(c => c.Conversions ?? 0),
                Revenue = g.Sum(c => c.AttributedRevenue ?? 0)
            })
            .ToListAsync(cancellationToken);

        var performanceByDay = await _context.PerformanceReports
            .Where(report => !report.IsDeleted
                && report.ReportDate >= from.Date
                && report.ReportDate <= to.Date
                && report.Post != null
                && report.Post.Content != null
                && report.Post.Content.WorkspaceId == workspaceId)
            .GroupBy(report => report.ReportDate)
            .Select(g => new
            {
                Date = g.Key,
                Engagement = g.Sum(report => report.Engagement),
                Revenue = g.Sum(report => report.EstimatedRevenue)
            })
            .ToListAsync(cancellationToken);

        if (snapshotsByDay.Count == 0 && postsByDay.Count == 0 && performanceByDay.Count == 0)
            return [];

        var points = days.Select(day =>
        {
            var dayPosts = postsByDay.FirstOrDefault(p => p.Date == day)?.Count ?? 0;
            var snapshot = snapshotsByDay.FirstOrDefault(item => item.Date == day);
            var performance = performanceByDay.FirstOrDefault(item => item.Date == day);

            return new AnalyticsPointDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                PublishedPosts = dayPosts,
                Impressions = snapshot?.Impressions ?? 0,
                Clicks = snapshot?.Clicks ?? 0,
                Conversions = (long)(snapshot?.Conversions ?? 0),
                Spend = snapshot?.Spend ?? 0,
                Ctr = snapshot?.Impressions > 0
                    ? Math.Round((decimal)snapshot.Clicks / snapshot.Impressions * 100, 2) : 0,
                Reach = snapshot?.Reach ?? 0,
                Engagement = performance?.Engagement ?? 0,
                EstimatedRevenue = (snapshot?.Revenue ?? 0) + (performance?.Revenue ?? 0),
                ActiveCampaigns = 0
            };
        }).ToList();

        return points;
    }

    public async Task<IReadOnlyList<AnalyticsChannelBreakdownDto>> GetChannelBreakdownAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, CancellationToken cancellationToken = default)
    {
        var integrationsQuery = _context.SocialIntegrations
            .Where(si => !si.IsDeleted && si.IsActive && si.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            integrationsQuery = integrationsQuery.Where(si => si.BrandId == brandId.Value);

        var integrations = await integrationsQuery
            .Include(si => si.Posts.Where(p => !p.IsDeleted && p.PublishedAt >= from && p.PublishedAt <= to))
            .ToListAsync(cancellationToken);
        var snapshotQuery = _context.CampaignInsightSnapshots
            .Where(snapshot => snapshot.WorkspaceId == workspaceId
                && snapshot.SnapshotDate >= from.Date
                && snapshot.SnapshotDate <= to.Date);
        if (brandId.HasValue)
            snapshotQuery = snapshotQuery.Where(snapshot => snapshot.Campaign.BrandId == brandId.Value);
        var snapshotChannels = await snapshotQuery
            .GroupBy(snapshot => snapshot.Platform)
            .Select(g => new
            {
                Platform = g.Key,
                Impressions = g.Sum(item => item.Impressions),
                Reach = g.Sum(item => item.Reach ?? 0),
                Clicks = g.Sum(item => item.Clicks),
                Spend = g.Sum(item => item.Spend),
                LastSyncedAt = g.Max(item => item.SyncedAt)
            })
            .ToListAsync(cancellationToken);

        var platforms = integrations.Select(item => item.Platform.ToString().ToLower())
            .Concat(snapshotChannels.Select(item => item.Platform))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        return platforms.Select(platform =>
        {
            var platformIntegrations = integrations
                .Where(item => item.Platform.ToString().Equals(platform, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var metrics = snapshotChannels.FirstOrDefault(item =>
                item.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));
            return new AnalyticsChannelBreakdownDto
            {
                Platform = platform,
                IntegrationId = platformIntegrations.FirstOrDefault()?.Id,
                DisplayName = $"{platform} ({platformIntegrations.Count} accounts)",
                PublishedPosts = platformIntegrations.Sum(item => item.Posts.Count),
                Impressions = metrics?.Impressions ?? 0,
                Reach = metrics?.Reach ?? 0,
                Clicks = metrics?.Clicks ?? 0,
                Ctr = metrics?.Impressions > 0
                    ? Math.Round((decimal)metrics.Clicks / metrics.Impressions * 100, 2) : 0,
                Spend = metrics?.Spend ?? 0,
                LastSyncedAt = metrics?.LastSyncedAt
            };
        }).ToList();
    }

    public async Task<(IReadOnlyList<CampaignAnalyticsItemDto> Items, int TotalCount)> GetCampaignBreakdownPagedAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, string? platform = null,
        int page = 1, int pageSize = 20, string? sortBy = "impressions", bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AdCampaigns
            .Include(c => c.Brand)
            .Where(c => !c.IsDeleted && c.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            query = query.Where(c => c.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            query = query.Where(c => c.Platform == platform.ToLower());

        var campaigns = await query.AsNoTracking().ToListAsync(cancellationToken);
        var campaignIds = campaigns.Select(campaign => campaign.Id).ToList();
        var snapshots = await _context.CampaignInsightSnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.WorkspaceId == workspaceId
                && campaignIds.Contains(snapshot.CampaignId)
                && snapshot.SnapshotDate >= from.Date
                && snapshot.SnapshotDate <= to.Date)
            .ToListAsync(cancellationToken);

        var allItems = campaigns.Select(c =>
        {
            var campaignSnapshots = snapshots.Where(snapshot => snapshot.CampaignId == c.Id).ToList();
            var impressions = campaignSnapshots.Sum(snapshot => snapshot.Impressions);
            var clicks = campaignSnapshots.Sum(snapshot => snapshot.Clicks);
            var spend = campaignSnapshots.Sum(snapshot => snapshot.Spend);
            var conversions = campaignSnapshots.Sum(snapshot => snapshot.Conversions ?? 0);
            var revenue = campaignSnapshots.Sum(snapshot => snapshot.AttributedRevenue ?? 0);
            return new CampaignAnalyticsItemDto
            {
                CampaignId = c.Id,
                Name = c.Name,
                CampaignName = c.Name,
                BrandName = c.Brand.Name,
                Platform = c.Platform,
                Objective = c.Objective,
                Status = c.IsActive ? "ACTIVE" : "PAUSED",
                Budget = c.Budget,
                Impressions = impressions,
                Reach = campaignSnapshots.Sum(snapshot => snapshot.Reach ?? 0),
                Engagement = campaignSnapshots.Sum(snapshot => snapshot.Engagement ?? 0),
                Clicks = clicks,
                Conversions = (long)conversions,
                Ctr = impressions > 0 ? Math.Round((decimal)clicks / impressions * 100, 2) : 0,
                Spend = spend,
                EstimatedRevenue = revenue,
                Cpa = conversions > 0 ? spend / conversions : 0,
                Roas = spend > 0 ? revenue / spend : 0
            };
        }).ToList();

        Func<CampaignAnalyticsItemDto, decimal> sortValue = sortBy?.ToLower() switch
        {
            "clicks" => item => item.Clicks,
            "ctr" => item => item.Ctr,
            "spend" => item => item.Spend,
            "conversions" => item => item.Conversions,
            "roas" => item => item.Roas,
            _ => item => item.Impressions
        };
        var ordered = sortDescending
            ? allItems.OrderByDescending(sortValue)
            : allItems.OrderBy(sortValue);
        var items = ordered.Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .ToList();

        return (items, allItems.Count);
    }

    public async Task<(IReadOnlyList<TopPostItemDto> Items, int TotalCount)> GetTopPostsPagedAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, string? platform = null, string? metric = "engagement",
        int page = 1, int pageSize = 10, bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var postsQuery = _context.Posts
            .Include(p => p.Content)
                .ThenInclude(c => c!.Brand)
            .Include(p => p.Integration)
            .Include(p => p.PerformanceReports.Where(pr => !pr.IsDeleted))
            .Where(p => !p.IsDeleted
                && p.PublishedAt >= from && p.PublishedAt <= to
                && p.Content != null && !p.Content.IsDeleted
                && p.Content.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            postsQuery = postsQuery.Where(p => p.Content!.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(platform))
            postsQuery = postsQuery.Where(p => p.Integration.Platform == ParsePlatform(platform));

        var totalCount = await postsQuery.CountAsync(cancellationToken);

        var allPosts = await postsQuery.ToListAsync(cancellationToken);

        IOrderedEnumerable<Post> ordered = metric?.ToLower() switch
        {
            "impressions" => sortDescending ? allPosts.OrderByDescending(p => p.PerformanceReports.Sum(pr => pr.Impressions)).ThenByDescending(p => p.PerformanceReports.Count) : allPosts.OrderBy(p => p.PerformanceReports.Sum(pr => pr.Impressions)).ThenBy(p => p.PerformanceReports.Count),
            "clicks" => sortDescending ? allPosts.OrderByDescending(p => p.PerformanceReports.Sum(pr => pr.Impressions)).ThenByDescending(p => p.PerformanceReports.Average(pr => pr.Ctr)) : allPosts.OrderBy(p => p.PerformanceReports.Sum(pr => pr.Impressions)).ThenBy(p => p.PerformanceReports.Average(pr => pr.Ctr)),
            "ctr" => sortDescending ? allPosts.OrderByDescending(p => p.PerformanceReports.Any() ? p.PerformanceReports.Average(pr => pr.Ctr) : 0) : allPosts.OrderBy(p => p.PerformanceReports.Any() ? p.PerformanceReports.Average(pr => pr.Ctr) : 0),
            _ => sortDescending ? allPosts.OrderByDescending(p => p.PerformanceReports.Sum(pr => pr.Engagement)).ThenByDescending(p => p.PerformanceReports.Count) : allPosts.OrderBy(p => p.PerformanceReports.Sum(pr => pr.Engagement)).ThenBy(p => p.PerformanceReports.Count),
        };

        var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var items = paged.Select(p => new TopPostItemDto
        {
            PostId = p.Id,
            ContentId = p.ContentId,
            ContentTitle = p.Content?.Title,
            BrandName = p.Content?.Brand?.Name,
            Platform = p.Integration.Platform.ToString().ToLower(),
            PublishedAt = p.PublishedAt,
            ExternalPostId = p.ExternalPostId,
            Impressions = p.PerformanceReports.Sum(pr => pr.Impressions),
            Reach = 0,
            Engagement = p.PerformanceReports.Sum(pr => pr.Engagement),
            Clicks = 0,
            Ctr = p.PerformanceReports.Any() ? Math.Round(p.PerformanceReports.Average(pr => pr.Ctr), 2) : 0
        }).ToList();

        return (items, totalCount);
    }

    public async Task<AnalyticsSparklines> GetSparklinesAsync(
        Guid workspaceId, DateTime from, DateTime to, int days = 7,
        Guid? brandId = null, string? platform = null, Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var sparkFrom = to.Date.AddDays(-(Math.Max(days, 1) - 1));
        var points = await GetDailyTimeSeriesAsync(
            workspaceId, sparkFrom, to, null, brandId, platform, campaignId, cancellationToken);

        return new AnalyticsSparklines
        {
            Impressions = points.Select(point => (decimal)point.Impressions).ToList(),
            Engagement = points.Select(point => (decimal)point.Engagement).ToList(),
            Clicks = points.Select(point => (decimal)point.Clicks).ToList(),
            Conversions = points.Select(point => (decimal)point.Conversions).ToList(),
            Ctr = points.Select(point => point.Ctr).ToList(),
            Spend = points.Select(point => point.Spend).ToList(),
        };
    }

    public async Task<AnalyticsTotals> GetAggregatedTotalsForPreviousPeriodAsync(
        Guid workspaceId, DateTime currentFrom, DateTime currentTo,
        Guid? brandId = null, string? platform = null, Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var periodLength = currentTo - currentFrom;
        var prevTo = currentFrom.AddDays(-1);
        var prevFrom = prevTo - periodLength;

        return await GetAggregatedTotalsAsync(workspaceId, prevFrom, prevTo, brandId, platform, campaignId, cancellationToken);
    }

    public async Task<UsageBreakdownDto> GetUsageBreakdownAsync(
        Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var totalAi = await _context.AiGenerations
            .Where(a => a.Content != null && !a.Content.IsDeleted && a.Content.WorkspaceId == workspaceId)
            .CountAsync(cancellationToken);

        var totalContents = await _context.Contents
            .Where(c => !c.IsDeleted && c.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);

        var totalCount = totalContents.Count;

        var textCount = totalContents.Count(c => c.AdType == AdTypeEnum.TextOnly);
        var imageCount = totalContents.Count(c => c.AdType == AdTypeEnum.ImageText);
        var videoCount = totalContents.Count(c => c.AdType == AdTypeEnum.VideoText);
        var otherCount = totalCount - textCount - imageCount - videoCount;

        return new UsageBreakdownDto
        {
            Items = new List<UsageBreakdownItemDto>
            {
                new() { Category = "Text Generation", Count = textCount, Percentage = totalCount > 0 ? Math.Round((decimal)textCount / totalCount * 100, 1) : 0 },
                new() { Category = "Image Generation", Count = imageCount, Percentage = totalCount > 0 ? Math.Round((decimal)imageCount / totalCount * 100, 1) : 0 },
                new() { Category = "Video Generation", Count = videoCount, Percentage = totalCount > 0 ? Math.Round((decimal)videoCount / totalCount * 100, 1) : 0 },
                new() { Category = "Other", Count = otherCount, Percentage = totalCount > 0 ? Math.Round((decimal)otherCount / totalCount * 100, 1) : 0 },
            }
        };
    }

    private static SocialPlatformEnum ParsePlatform(string platform)
    {
        return platform.ToLower() switch
        {
            "facebook" => SocialPlatformEnum.Facebook,
            "instagram" => SocialPlatformEnum.Instagram,
            "tiktok" => SocialPlatformEnum.TikTok,
            "twitter" => SocialPlatformEnum.Twitter,
            "google" => SocialPlatformEnum.Google,
            _ => SocialPlatformEnum.Facebook
        };
    }

    public async Task<AnalyticsTotals> GetAllWorkspaceTotalsAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var campaignsQuery = _context.AdCampaigns
            .Where(c => !c.IsDeleted);

        var postsQuery = _context.Posts
            .Where(p => !p.IsDeleted
                && p.PublishedAt >= from && p.PublishedAt <= to
                && p.Content != null && !p.Content.IsDeleted);

        var publishedPosts = await postsQuery.CountAsync(cancellationToken);
        var activeCampaigns = await campaignsQuery
            .Where(c => c.IsActive && c.DeploymentStatus == DeploymentStatusEnum.None && (c.EndDate == null || c.EndDate >= from))
            .CountAsync(cancellationToken);

        var campaignAgg = await campaignsQuery
            .Where(c => c.StartDate >= from || c.StartDate == null || c.CreatedAt >= from)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Impressions = g.Sum(c => c.Impressions),
                Clicks = g.Sum(c => c.Clicks),
                Spend = g.Sum(c => c.Spend),
                Conversions = g.Sum(c => c.Conversions)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var perfAgg = await _context.PerformanceReports
            .Where(pr => !pr.IsDeleted && pr.ReportDate >= from && pr.ReportDate <= to)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Engagement = g.Sum(pr => pr.Engagement),
                EstimatedRevenue = g.Sum(pr => pr.EstimatedRevenue)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var impressions = campaignAgg?.Impressions ?? 0;
        var clicks = campaignAgg?.Clicks ?? 0;
        var ctr = impressions > 0 ? (decimal)clicks / impressions * 100 : 0;

        return new AnalyticsTotals
        {
            Impressions = impressions,
            Clicks = clicks,
            Ctr = ctr,
            Spend = campaignAgg?.Spend ?? 0,
            Conversions = campaignAgg?.Conversions ?? 0,
            Engagement = perfAgg?.Engagement ?? 0,
            EstimatedRevenue = perfAgg?.EstimatedRevenue ?? 0,
            PublishedPosts = publishedPosts,
            ActiveCampaigns = activeCampaigns
        };
    }

    public async Task<IReadOnlyList<WorkspaceAnalyticsItemDto>> GetWorkspaceComparisonAsync(
        DateTime from, DateTime to, int top = 20, CancellationToken cancellationToken = default)
    {
        var workspaces = await _context.Workspaces
            .AsNoTracking()
            .Where(w => w.Status == WorkspaceStatusEnum.Active)
            .Select(w => new { w.Id, w.Name })
            .ToListAsync(cancellationToken);

        var result = new List<WorkspaceAnalyticsItemDto>();

        foreach (var ws in workspaces)
        {
            var posts = await _context.Posts
                .CountAsync(p => !p.IsDeleted && p.PublishedAt >= from && p.PublishedAt <= to && p.Content != null && p.Content.WorkspaceId == ws.Id, cancellationToken);

            var campaigns = await _context.AdCampaigns
                .CountAsync(c => !c.IsDeleted && c.WorkspaceId == ws.Id && c.IsActive && (c.EndDate == null || c.EndDate >= from), cancellationToken);

            var campAgg = await _context.AdCampaigns
                .Where(c => !c.IsDeleted && c.WorkspaceId == ws.Id && (c.StartDate >= from || c.StartDate == null || c.CreatedAt >= from))
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Impressions = g.Sum(c => c.Impressions),
                    Clicks = g.Sum(c => c.Clicks),
                    Spend = g.Sum(c => c.Spend)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var perfAgg = await _context.PerformanceReports
                .Where(pr => !pr.IsDeleted && pr.ReportDate >= from && pr.ReportDate <= to && pr.Post != null && pr.Post.Content != null && pr.Post.Content.WorkspaceId == ws.Id)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Engagement = g.Sum(pr => pr.Engagement),
                    EstimatedRevenue = g.Sum(pr => pr.EstimatedRevenue)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var impressions = campAgg?.Impressions ?? 0;
            var clicks = campAgg?.Clicks ?? 0;

            result.Add(new WorkspaceAnalyticsItemDto
            {
                WorkspaceId = ws.Id,
                WorkspaceName = ws.Name,
                PublishedPosts = posts,
                ActiveCampaigns = campaigns,
                Impressions = impressions,
                Clicks = clicks,
                Spend = campAgg?.Spend ?? 0,
                Engagement = perfAgg?.Engagement ?? 0,
                Ctr = impressions > 0 ? (decimal)clicks / impressions * 100 : 0,
                EstimatedRevenue = perfAgg?.EstimatedRevenue ?? 0,
                Roas = (campAgg?.Spend ?? 0) > 0 ? (perfAgg?.EstimatedRevenue ?? 0) / (campAgg?.Spend ?? 1) : 0
            });
        }

        return result.OrderByDescending(w => w.EstimatedRevenue).Take(top).ToList();
    }

    public async Task<IReadOnlyList<CampaignAnalyticsItemDto>> GetTopCampaignsAllWorkspacesAsync(
        DateTime from, DateTime to, int top = 20, CancellationToken cancellationToken = default)
    {
        var campaigns = await _context.AdCampaigns
            .AsNoTracking()
            .Where(c => !c.IsDeleted && (c.StartDate >= from || c.StartDate == null || c.CreatedAt >= from))
            .Include(c => c.Workspace)
            .OrderByDescending(c => c.Impressions)
            .Take(top)
            .Select(c => new CampaignAnalyticsItemDto
            {
                CampaignName = c.Name,
                BrandName = "",
                Status = c.IsActive ? "active" : (c.EndDate != null && c.EndDate < DateTime.UtcNow ? "completed" : "paused"),
                Impressions = c.Impressions,
                Clicks = c.Clicks,
                Spend = c.Spend,
                Conversions = c.Conversions,
                Ctr = c.Impressions > 0 ? (decimal)c.Clicks / c.Impressions * 100 : 0,
                Cpa = c.Conversions > 0 ? c.Spend / c.Conversions : 0,
                Roas = c.Spend > 0 ? c.Conversions * 50 / c.Spend : 0
            })
            .ToListAsync(cancellationToken);

        return campaigns;
    }
}
