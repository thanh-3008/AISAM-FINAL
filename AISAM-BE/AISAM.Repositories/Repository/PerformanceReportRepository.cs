using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            .Where(c => c.IsActive && c.DeploymentStatus == DeploymentStatusEnum.None
                && (c.StartDate ?? c.CreatedAt) <= to
                && (c.EndDate == null || c.EndDate >= from))
            .CountAsync(cancellationToken);

        var campaignAgg = await campaignsQuery
            .Where(c => (c.StartDate ?? c.CreatedAt) <= to && (c.EndDate ?? c.StartDate ?? c.CreatedAt) >= from)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Impressions = g.Sum(c => c.Impressions),
                Clicks = g.Sum(c => c.Clicks),
                Spend = g.Sum(c => c.Spend),
                Conversions = g.Sum(c => c.Conversions)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var perfReportsQuery = _context.PerformanceReports
            .Where(pr => !pr.IsDeleted
                && pr.ReportDate >= from && pr.ReportDate <= to
                && pr.Post != null && !pr.Post.IsDeleted
                && pr.Post.Content != null && !pr.Post.Content.IsDeleted
                && pr.Post.Content.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            perfReportsQuery = perfReportsQuery.Where(pr => pr.Post!.Content!.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(platform))
        {
            var platformEnum = ParsePlatform(platform);
            perfReportsQuery = perfReportsQuery.Where(pr => pr.Post!.Integration.Platform == platformEnum);
        }

        var perfAgg = await perfReportsQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Impressions = g.Sum(pr => pr.Impressions),
                Engagement = g.Sum(pr => pr.Engagement),
                Revenue = g.Sum(pr => pr.EstimatedRevenue)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var perfClicks = (await perfReportsQuery
                .Select(pr => pr.RawData)
                .ToListAsync(cancellationToken))
            .Sum(ExtractClicks);
        var perfReach = (await perfReportsQuery
                .Select(pr => pr.RawData)
                .ToListAsync(cancellationToken))
            .Sum(ExtractReach);
        var totalImpressions = (campaignAgg?.Impressions ?? 0) + (perfAgg?.Impressions ?? 0);
        var totalClicks = (campaignAgg?.Clicks ?? 0) + perfClicks;

        var perfReportCount = await perfReportsQuery.CountAsync(cancellationToken);

        return new AnalyticsTotals
        {
            Impressions = totalImpressions,
            Engagement = perfAgg?.Engagement ?? 0,
            Clicks = totalClicks,
            Conversions = campaignAgg?.Conversions ?? 0,
            Ctr = totalImpressions > 0 ? Math.Round((decimal)totalClicks / totalImpressions * 100, 2) : 0,
            Spend = campaignAgg?.Spend ?? 0,
            EstimatedRevenue = perfAgg?.Revenue ?? 0,
            PublishedPosts = publishedPosts,
            ActiveCampaigns = activeCampaigns,
            Reach = perfReach
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
                && (c.StartDate ?? c.CreatedAt) <= to && (c.EndDate ?? c.StartDate ?? c.CreatedAt) >= from);
        if (brandId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.BrandId == brandId.Value);
        if (campaignId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.Id == campaignId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            campaignsQuery = campaignsQuery.Where(c => c.Platform.ToLower() == platform.ToLower());

        var campaignByDay = await campaignsQuery
            .GroupBy(c => (c.StartDate ?? c.CreatedAt).Date)
            .Select(g => new
            {
                Date = g.Key,
                Impressions = g.Sum(c => c.Impressions),
                Clicks = g.Sum(c => c.Clicks),
                Spend = g.Sum(c => c.Spend),
                Conversions = g.Sum(c => c.Conversions)
            })
            .ToListAsync(cancellationToken);

        var reportRowsQuery = _context.PerformanceReports
            .Where(pr => !pr.IsDeleted
                && pr.ReportDate >= from && pr.ReportDate <= to
                && pr.Post != null && !pr.Post.IsDeleted
                && pr.Post.Content != null && !pr.Post.Content.IsDeleted
                && pr.Post.Content.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            reportRowsQuery = reportRowsQuery.Where(pr => pr.Post!.Content!.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
        {
            var platformEnum = ParsePlatform(platform);
            reportRowsQuery = reportRowsQuery.Where(pr => pr.Post!.Integration.Platform == platformEnum);
        }

        var reportRows = await reportRowsQuery
            .Select(pr => new
            {
                Date = pr.ReportDate.Date,
                pr.Impressions,
                pr.Engagement,
                pr.EstimatedRevenue,
                pr.RawData
            })
            .ToListAsync(cancellationToken);

        var reportsByDay = reportRows
            .GroupBy(pr => pr.Date)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Impressions = g.Sum(pr => pr.Impressions),
                    Engagement = g.Sum(pr => pr.Engagement),
                    Revenue = g.Sum(pr => pr.EstimatedRevenue),
                    Clicks = g.Sum(pr => ExtractClicks(pr.RawData)),
                    Reach = g.Sum(pr => ExtractReach(pr.RawData))
                });

        var points = days.Select(day =>
        {
            var dayPosts = postsByDay.FirstOrDefault(p => p.Date == day)?.Count ?? 0;
            var dayCampaign = campaignByDay.FirstOrDefault(c => c.Date == day);
            reportsByDay.TryGetValue(day, out var dayReport);
            var impressions = (dayCampaign?.Impressions ?? 0) + (dayReport?.Impressions ?? 0);
            var clicks = (dayCampaign?.Clicks ?? 0) + (dayReport?.Clicks ?? 0);

            return new AnalyticsPointDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                PublishedPosts = dayPosts,
                Impressions = impressions,
                Clicks = clicks,
                Conversions = dayCampaign?.Conversions ?? 0,
                Spend = dayCampaign?.Spend ?? 0,
                Ctr = impressions > 0 ? Math.Round((decimal)clicks / impressions * 100, 2) : 0,
                Reach = dayReport?.Reach ?? 0,
                Engagement = dayReport?.Engagement ?? 0,
                EstimatedRevenue = dayReport?.Revenue ?? 0,
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
                .ThenInclude(p => p.PerformanceReports.Where(pr => !pr.IsDeleted && pr.ReportDate >= from && pr.ReportDate <= to))
            .ToListAsync(cancellationToken);

        return integrations
            .GroupBy(si => si.Platform)
            .Select(g =>
            {
                var platformStr = g.Key.ToString().ToLower();
                var postCount = g.Sum(si => si.Posts.Count);
                var reports = g.SelectMany(si => si.Posts).SelectMany(p => p.PerformanceReports).ToList();
                var impressions = reports.Sum(pr => pr.Impressions);
                var engagement = reports.Sum(pr => pr.Engagement);
                var clicks = reports.Sum(pr => ExtractClicks(pr.RawData));
                var reach = reports.Sum(pr => ExtractReach(pr.RawData));
                return new AnalyticsChannelBreakdownDto
                {
                    Platform = platformStr,
                    IntegrationId = g.First().Id,
                    DisplayName = $"{platformStr} ({g.Count()} accounts)",
                    PublishedPosts = postCount,
                    Impressions = impressions,
                    Reach = reach,
                    Engagement = engagement,
                    Clicks = clicks,
                    Ctr = impressions > 0 ? Math.Round((decimal)clicks / impressions * 100, 2) : 0,
                    Spend = 0,
                    LastSyncedAt = reports.Count > 0 ? reports.Max(pr => (DateTime?)pr.CreatedAt) : g.Max(si => (DateTime?)si.UpdatedAt)
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
        if (!string.IsNullOrWhiteSpace(platform))
            query = query.Where(c => c.Platform.ToLower() == platform.ToLower());

        query = query.Where(c => (c.StartDate ?? c.CreatedAt) <= to && (c.EndDate ?? c.StartDate ?? c.CreatedAt) >= from);

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
                CampaignName = c.Name,
                BrandName = c.Brand != null ? c.Brand.Name : string.Empty,
                Platform = c.Platform,
                Objective = c.Objective,
                Status = c.IsActive ? "ACTIVE" : "PAUSED",
                Budget = c.Budget,
                Impressions = c.Impressions,
                Reach = 0,
                Engagement = c.Conversions,
                Clicks = c.Clicks,
                Ctr = c.Impressions > 0 ? Math.Round((decimal)c.Clicks / c.Impressions * 100, 2) : 0,
                Spend = c.Spend,
                EstimatedRevenue = 0,
                Conversions = c.Conversions,
                Cpa = c.Conversions > 0 ? c.Spend / c.Conversions : 0,
                Roas = 0
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
            .Include(p => p.PerformanceReports.Where(pr => !pr.IsDeleted && pr.ReportDate >= from && pr.ReportDate <= to))
            .Where(p => !p.IsDeleted
                && p.PublishedAt >= from && p.PublishedAt <= to
                && p.Content != null && !p.Content.IsDeleted
                && p.Integration != null
                && (p.Content.WorkspaceId == workspaceId || p.Integration.WorkspaceId == workspaceId));

        if (brandId.HasValue)
            postsQuery = postsQuery.Where(p => p.Content!.BrandId == brandId.Value || p.Integration.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(platform))
            postsQuery = postsQuery.Where(p => p.Integration.Platform == ParsePlatform(platform));

        var totalCount = await postsQuery.CountAsync(cancellationToken);

        var allPosts = await postsQuery.ToListAsync(cancellationToken);

        IOrderedEnumerable<Post> ordered = metric?.ToLower() switch
        {
            "recent" => sortDescending ? allPosts.OrderByDescending(p => p.PublishedAt) : allPosts.OrderBy(p => p.PublishedAt),
            "impressions" => sortDescending ? allPosts.OrderByDescending(p => GetLatestReport(p)?.Impressions ?? 0).ThenByDescending(p => p.PerformanceReports.Count) : allPosts.OrderBy(p => GetLatestReport(p)?.Impressions ?? 0).ThenBy(p => p.PerformanceReports.Count),
            "clicks" => sortDescending ? allPosts.OrderByDescending(GetPostClicks) : allPosts.OrderBy(GetPostClicks),
            "ctr" => sortDescending ? allPosts.OrderByDescending(GetPostCtr) : allPosts.OrderBy(GetPostCtr),
            _ => sortDescending ? allPosts.OrderByDescending(GetPostEngagement).ThenByDescending(p => p.PerformanceReports.Count) : allPosts.OrderBy(GetPostEngagement).ThenBy(p => p.PerformanceReports.Count),
        };

        var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var items = paged.Select(p =>
        {
            var latestReport = GetLatestReport(p);
            var impressions = latestReport?.Impressions ?? 0;
            var clicks = GetPostClicks(p);
            var reach = ExtractReach(latestReport?.RawData);
            var totalMediaViewUnique = ExtractTotalMediaViewUnique(latestReport?.RawData);
            return new TopPostItemDto
        {
            PostId = p.Id,
            ContentId = p.ContentId,
            ContentTitle = p.Content?.Title,
            BrandName = p.Content?.Brand?.Name,
            Platform = p.Integration.Platform.ToString().ToLower(),
            PublishedAt = p.PublishedAt,
            ExternalPostId = p.ExternalPostId,
            Impressions = impressions,
            Reach = reach,
            Engagement = latestReport == null ? 0 : ExtractEngagement(latestReport.RawData, latestReport.Engagement),
            Clicks = clicks,
            TotalMediaViewUnique = totalMediaViewUnique,
            Ctr = impressions > 0 ? Math.Round((decimal)clicks / impressions * 100, 2) : 0
        };
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

        var campaignsQuery = _context.AdCampaigns
            .Where(c => !c.IsDeleted && c.WorkspaceId == workspaceId
                && (c.StartDate ?? c.CreatedAt) <= to && (c.EndDate ?? c.StartDate ?? c.CreatedAt) >= sparkDays.First());
        if (brandId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            campaignsQuery = campaignsQuery.Where(c => c.Platform.ToLower() == platform.ToLower());
        if (campaignId.HasValue)
            campaignsQuery = campaignsQuery.Where(c => c.Id == campaignId.Value);

        var campaignByDay = await campaignsQuery
            .GroupBy(c => (c.StartDate ?? c.CreatedAt).Date)
            .Select(g => new
            {
                Date = g.Key,
                Impressions = g.Sum(c => c.Impressions),
                Clicks = g.Sum(c => c.Clicks),
                Spend = g.Sum(c => c.Spend),
                Conversions = g.Sum(c => c.Conversions)
            })
            .ToListAsync(cancellationToken);

        var reportRowsQuery = _context.PerformanceReports
            .Where(pr => !pr.IsDeleted
                && pr.ReportDate >= sparkDays.First() && pr.ReportDate <= to
                && pr.Post != null && !pr.Post.IsDeleted
                && pr.Post.Content != null && !pr.Post.Content.IsDeleted
                && pr.Post.Content.WorkspaceId == workspaceId);

        if (brandId.HasValue)
            reportRowsQuery = reportRowsQuery.Where(pr => pr.Post!.Content!.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            reportRowsQuery = reportRowsQuery.Where(pr => pr.Post!.Integration.Platform == ParsePlatform(platform));

        var reportRows = await reportRowsQuery
            .Select(pr => new
            {
                Date = pr.ReportDate.Date,
                pr.Impressions,
                pr.Engagement,
                pr.RawData
            })
            .ToListAsync(cancellationToken);

        var reportsByDay = reportRows
            .GroupBy(pr => pr.Date)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Impressions = g.Sum(pr => pr.Impressions),
                    Engagement = g.Sum(pr => pr.Engagement),
                    Clicks = g.Sum(pr => ExtractClicks(pr.RawData))
                });

        return new AnalyticsSparklines
        {
            Impressions = sparkDays.Select(d =>
            {
                reportsByDay.TryGetValue(d, out var report);
                return (decimal)((campaignByDay.FirstOrDefault(c => c.Date == d)?.Impressions ?? 0) + (report?.Impressions ?? 0));
            }).ToList(),
            Engagement = sparkDays.Select(d =>
            {
                reportsByDay.TryGetValue(d, out var report);
                return (decimal)(report?.Engagement ?? 0);
            }).ToList(),
            Clicks = sparkDays.Select(d =>
            {
                reportsByDay.TryGetValue(d, out var report);
                return (decimal)((campaignByDay.FirstOrDefault(c => c.Date == d)?.Clicks ?? 0) + (report?.Clicks ?? 0));
            }).ToList(),
            Conversions = sparkDays.Select(d => (decimal)(campaignByDay.FirstOrDefault(c => c.Date == d)?.Conversions ?? 0)).ToList(),
            Ctr = sparkDays.Select(d =>
            {
                var campaign = campaignByDay.FirstOrDefault(c => c.Date == d);
                reportsByDay.TryGetValue(d, out var report);
                var impressions = (campaign?.Impressions ?? 0) + (report?.Impressions ?? 0);
                var clicks = (campaign?.Clicks ?? 0) + (report?.Clicks ?? 0);
                return impressions > 0 ? Math.Round((decimal)clicks / impressions * 100, 2) : 0;
            }).ToList(),
            Spend = sparkDays.Select(d => campaignByDay.FirstOrDefault(c => c.Date == d)?.Spend ?? 0).ToList(),
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

    public async Task AddAsync(PerformanceReport report, CancellationToken cancellationToken = default)
    {
        await _context.PerformanceReports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<PerformanceReport> reports, CancellationToken cancellationToken = default)
    {
        await _context.PerformanceReports.AddRangeAsync(reports, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertPostReportAsync(PerformanceReport report, CancellationToken cancellationToken = default)
    {
        if (!report.PostId.HasValue)
            throw new ArgumentException("Post report must have a PostId.", nameof(report));

        var existing = await _context.PerformanceReports
            .FirstOrDefaultAsync(pr => !pr.IsDeleted
                && pr.PostId == report.PostId
                && pr.ReportDate == report.ReportDate, cancellationToken);

        if (existing == null)
        {
            await _context.PerformanceReports.AddAsync(report, cancellationToken);
        }
        else
        {
            existing.Impressions = report.Impressions;
            existing.Engagement = report.Engagement;
            existing.Ctr = report.Ctr;
            existing.EstimatedRevenue = report.EstimatedRevenue;
            existing.RawData = PreserveTrackedClicks(report.RawData, existing.RawData);
            existing.CreatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IncrementTrackedClickAsync(Guid contentId, Guid integrationId, CancellationToken cancellationToken = default)
    {
        var post = await _context.Posts
            .Where(p => !p.IsDeleted && p.ContentId == contentId && p.IntegrationId == integrationId)
            .OrderByDescending(p => p.PublishedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return false;

        var reportDate = DateTime.UtcNow.Date;
        var report = await _context.PerformanceReports
            .FirstOrDefaultAsync(pr => !pr.IsDeleted && pr.PostId == post.Id && pr.ReportDate == reportDate, cancellationToken);

        if (report == null)
        {
            report = new PerformanceReport
            {
                PostId = post.Id,
                ReportDate = reportDate,
                RawData = "{\"trackedClicks\":1}",
                CreatedAt = DateTime.UtcNow
            };
            await _context.PerformanceReports.AddAsync(report, cancellationToken);
        }
        else
        {
            report.RawData = IncrementTrackedClicks(report.RawData);
            report.CreatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DateTime?> GetLatestReportDateForPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReports
            .Where(pr => pr.PostId == postId && !pr.IsDeleted)
            .MaxAsync(pr => (DateTime?)pr.ReportDate, cancellationToken);
    }

    public async Task<List<Post>> GetPostsNeedingSyncAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(p => p.Integration)
                .ThenInclude(i => i.SocialAccount)
            .Include(p => p.Integration.Workspace)
            .Where(p => !p.IsDeleted
                && !string.IsNullOrWhiteSpace(p.ExternalPostId)
                && p.PublishedAt <= DateTime.UtcNow
                && p.Integration != null)
            .OrderByDescending(p => p.PublishedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Post>> GetPostsNeedingSyncAsync(
        int batchSize,
        Guid workspaceId,
        DateTime from,
        DateTime to,
        Guid? brandId = null,
        string? platform = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Posts
            .Include(p => p.Integration)
                .ThenInclude(i => i.SocialAccount)
            .Include(p => p.Content)
            .Where(p => !p.IsDeleted
                && !string.IsNullOrWhiteSpace(p.ExternalPostId)
                && p.PublishedAt >= from
                && p.PublishedAt <= to
                && p.Content != null
                && !p.Content.IsDeleted
                && p.Integration != null
                && (p.Content.WorkspaceId == workspaceId || p.Integration.WorkspaceId == workspaceId));

        if (brandId.HasValue)
            query = query.Where(p => p.Content!.BrandId == brandId.Value || p.Integration.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(platform))
            query = query.Where(p => p.Integration.Platform == ParsePlatform(platform));

        return await query
            .OrderBy(p => p.Integration.Platform == SocialPlatformEnum.Facebook ? 0 : 1)
            .ThenByDescending(p => p.PublishedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private static long ExtractClicks(string? rawData)
    {
        var metaClicks = ExtractMetaClicks(rawData);
        var trackedClicks = ExtractTrackedClicks(rawData);
        return Math.Max(metaClicks, trackedClicks);
    }

    private static long ExtractMetaClicks(string? rawData) =>
        ReadLongProperty(rawData, "clicks");

    private static long ExtractTrackedClicks(string? rawData) =>
        ReadLongProperty(rawData, "trackedClicks");

    private static long ExtractEngagement(string? rawData, long fallback)
    {
        var reactions = ReadLongProperty(rawData, "reactions");
        var comments = ReadLongProperty(rawData, "comments");
        var shares = ReadLongProperty(rawData, "shares");
        var calculated = reactions + comments + shares;

        return calculated > 0 ? calculated : fallback;
    }

    private static long ExtractReach(string? rawData)
    {
        return ReadLongProperty(rawData, "reach");
    }

    private static long ExtractTotalMediaViewUnique(string? rawData) =>
        ReadLongProperty(rawData, "total_media_view_unique");

    private static decimal GetPostCtr(Post post)
    {
        var report = GetLatestReport(post);
        var impressions = report?.Impressions ?? 0;
        if (impressions <= 0)
            return 0;

        var clicks = GetPostClicks(post);
        return (decimal)clicks / impressions * 100;
    }

    private static long GetPostClicks(Post post)
    {
        var latestReport = GetLatestReport(post);
        var latestMetaClicks = ExtractMetaClicks(latestReport?.RawData);
        var trackedClicks = post.PerformanceReports.Sum(report => ExtractTrackedClicks(report.RawData));
        return Math.Max(latestMetaClicks, trackedClicks);
    }

    private static long GetPostEngagement(Post post)
    {
        var report = GetLatestReport(post);
        return report == null ? 0 : ExtractEngagement(report.RawData, report.Engagement);
    }

    private static PerformanceReport? GetLatestReport(Post post)
    {
        return post.PerformanceReports
            .OrderByDescending(report => report.ReportDate)
            .ThenByDescending(report => report.CreatedAt)
            .FirstOrDefault();
    }

    private static string PreserveTrackedClicks(string? incomingRawData, string? existingRawData)
    {
        var trackedClicks = ReadLongProperty(existingRawData, "trackedClicks");
        if (trackedClicks <= 0)
            return incomingRawData ?? string.Empty;

        var node = ParseObject(incomingRawData);
        node["trackedClicks"] = trackedClicks;
        return node.ToJsonString();
    }

    private static string IncrementTrackedClicks(string? rawData)
    {
        var node = ParseObject(rawData);
        var current = ReadLongProperty(rawData, "trackedClicks");
        node["trackedClicks"] = current + 1;
        return node.ToJsonString();
    }

    private static long ReadLongProperty(string? rawData, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(rawData))
            return 0;

        try
        {
            using var doc = JsonDocument.Parse(rawData);
            return doc.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
                ? property.GetInt64()
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static JsonObject ParseObject(string? rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
            return new JsonObject();

        try
        {
            return JsonNode.Parse(rawData) as JsonObject ?? new JsonObject();
        }
        catch
        {
            return new JsonObject();
        }
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
            .Where(c => c.IsActive && c.DeploymentStatus == DeploymentStatusEnum.None
                && (c.StartDate ?? c.CreatedAt) <= to
                && (c.EndDate == null || c.EndDate >= from))
            .CountAsync(cancellationToken);

        var campaignAgg = await campaignsQuery
            .Where(c => (c.StartDate ?? c.CreatedAt) <= to && (c.EndDate ?? c.StartDate ?? c.CreatedAt) >= from)
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
                .CountAsync(c => !c.IsDeleted && c.WorkspaceId == ws.Id && c.IsActive
                    && (c.StartDate ?? c.CreatedAt) <= to
                    && (c.EndDate == null || c.EndDate >= from), cancellationToken);

            var campAgg = await _context.AdCampaigns
                .Where(c => !c.IsDeleted && c.WorkspaceId == ws.Id && (c.StartDate ?? c.CreatedAt) <= to && (c.EndDate ?? c.StartDate ?? c.CreatedAt) >= from)
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
            .Where(c => !c.IsDeleted && (c.StartDate ?? c.CreatedAt) <= to && (c.EndDate ?? c.StartDate ?? c.CreatedAt) >= from)
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
