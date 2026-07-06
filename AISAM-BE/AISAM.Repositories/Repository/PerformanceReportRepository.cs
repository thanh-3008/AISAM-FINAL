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
            Conversions = campaignAgg?.Conversions ?? 0,
            Ctr = campaignAgg?.Clicks > 0
                ? Math.Round((decimal)campaignAgg.Clicks / (campaignAgg.Impressions > 0 ? campaignAgg.Impressions : 1) * 100, 2)
                : (perfAgg?.Ctr != null ? Math.Round(perfAgg.Ctr, 2) : 0),
            Spend = campaignAgg?.Spend ?? 0,
            EstimatedRevenue = perfAgg?.Revenue ?? 0,
            PublishedPosts = publishedPosts,
            ActiveCampaigns = activeCampaigns,
            Reach = 0
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

        var campaignsQuery = _context.AdCampaigns
            .Where(c => !c.IsDeleted && c.WorkspaceId == workspaceId
                && ((c.StartDate >= from && c.StartDate <= to) || (c.CreatedAt >= from && c.CreatedAt <= to)));
        if (brandId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.BrandId == brandId.Value);
        if (campaignId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.Id == campaignId.Value);

        var campaignAgg = await campaignsQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Impressions = g.Sum(c => c.Impressions),
                Clicks = g.Sum(c => c.Clicks),
                Spend = g.Sum(c => c.Spend),
                Conversions = g.Sum(c => c.Conversions)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalDays = days.Count;
        var points = days.Select((day, index) =>
        {
            var dayPosts = postsByDay.FirstOrDefault(p => p.Date == day)?.Count ?? 0;
            var dayFraction = totalDays > 0 ? (decimal)(index + 1) / totalDays : 0;

            return new AnalyticsPointDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                PublishedPosts = dayPosts,
                Impressions = (long)((campaignAgg?.Impressions ?? 0) * dayFraction / totalDays),
                Clicks = (long)((campaignAgg?.Clicks ?? 0) * dayFraction / totalDays),
                Conversions = (long)((campaignAgg?.Conversions ?? 0) * dayFraction / totalDays),
                Spend = (campaignAgg?.Spend ?? 0) * dayFraction / totalDays,
                Ctr = campaignAgg?.Impressions > 0
                    ? Math.Round((decimal)(campaignAgg.Clicks) / campaignAgg.Impressions * 100, 2) : 0,
                Reach = 0,
                Engagement = 0,
                EstimatedRevenue = 0,
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

        return integrations
            .GroupBy(si => si.Platform)
            .Select(g =>
            {
                var platformStr = g.Key.ToString().ToLower();
                var postCount = g.Sum(si => si.Posts.Count);
                return new AnalyticsChannelBreakdownDto
                {
                    Platform = platformStr,
                    IntegrationId = g.First().Id,
                    DisplayName = $"{platformStr} ({g.Count()} accounts)",
                    PublishedPosts = postCount,
                    Impressions = 0,
                    Reach = 0,
                    Engagement = 0,
                    Clicks = 0,
                    Ctr = 0,
                    Spend = 0,
                    LastSyncedAt = g.Max(si => (DateTime?)si.UpdatedAt)
                };
            })
            .ToList();
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

        var totalCount = await query.CountAsync(cancellationToken);

        IQueryable<AdCampaign> orderedQuery = sortBy?.ToLower() switch
        {
            "clicks" => sortDescending ? query.OrderByDescending(c => c.Clicks) : query.OrderBy(c => c.Clicks),
            "ctr" => sortDescending ? query.OrderByDescending(c => c.Impressions > 0 ? (decimal)c.Clicks / c.Impressions : 0) : query.OrderBy(c => c.Impressions > 0 ? (decimal)c.Clicks / c.Impressions : 0),
            "spend" => sortDescending ? query.OrderByDescending(c => c.Spend) : query.OrderBy(c => c.Spend),
            "engagement" => sortDescending ? query.OrderByDescending(c => c.Conversions) : query.OrderBy(c => c.Conversions),
            _ => sortDescending ? query.OrderByDescending(c => c.Impressions) : query.OrderBy(c => c.Impressions),
        };

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CampaignAnalyticsItemDto
            {
                CampaignId = c.Id,
                Name = c.Name,
                Platform = null,
                Objective = c.Objective,
                Status = c.IsActive ? "ACTIVE" : "PAUSED",
                Budget = c.Budget,
                Impressions = c.Impressions,
                Reach = 0,
                Engagement = c.Conversions,
                Clicks = c.Clicks,
                Ctr = c.Impressions > 0 ? Math.Round((decimal)c.Clicks / c.Impressions * 100, 2) : 0,
                Spend = c.Spend,
                EstimatedRevenue = 0
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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
        var sparkDays = Enumerable.Range(0, days)
            .Select(d => to.Date.AddDays(-d))
            .OrderBy(d => d)
            .ToList();

        var postsByDay = await _context.Posts
            .Where(p => !p.IsDeleted && p.PublishedAt >= sparkDays.First() && p.PublishedAt <= to
                && p.Content != null && !p.Content.IsDeleted
                && p.Content.WorkspaceId == workspaceId)
            .GroupBy(p => p.PublishedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var campaignByDay = await _context.AdCampaigns
            .Where(c => !c.IsDeleted && c.WorkspaceId == workspaceId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalImpressions = g.Sum(c => c.Impressions),
                TotalClicks = g.Sum(c => c.Clicks),
                TotalSpend = g.Sum(c => c.Spend),
                TotalConversions = g.Sum(c => c.Conversions)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var avgDailyImpressions = sparkDays.Count > 0 ? (campaignByDay?.TotalImpressions ?? 0) / (decimal)sparkDays.Count : 0;
        var avgDailyClicks = sparkDays.Count > 0 ? (campaignByDay?.TotalClicks ?? 0) / (decimal)sparkDays.Count : 0;
        var avgDailySpend = sparkDays.Count > 0 ? (campaignByDay?.TotalSpend ?? 0) / sparkDays.Count : 0;
        var avgDailyConversions = sparkDays.Count > 0 ? (campaignByDay?.TotalConversions ?? 0) / (decimal)sparkDays.Count : 0;

        return new AnalyticsSparklines
        {
            Impressions = sparkDays.Select(_ => avgDailyImpressions).ToList(),
            Engagement = sparkDays.Select(d => (decimal)(postsByDay.FirstOrDefault(p => p.Date == d)?.Count ?? 0)).ToList(),
            Clicks = sparkDays.Select(_ => avgDailyClicks).ToList(),
            Conversions = sparkDays.Select(_ => avgDailyConversions).ToList(),
            Ctr = sparkDays.Select(d => (decimal)(postsByDay.FirstOrDefault(p => p.Date == d)?.Count ?? 0)).ToList(),
            Spend = sparkDays.Select(_ => avgDailySpend).ToList(),
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
}
