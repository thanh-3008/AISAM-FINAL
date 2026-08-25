using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Common.Dtos;
using AISAM.Services.IServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace AISAM.Services.Service;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IPerformanceReportRepository _performanceReportRepo;
    private readonly ISocialIntegrationRepository _socialIntegrationRepo;
    private readonly IGeminiTextClient _geminiTextClient;
    private readonly FacebookProvider _facebookProvider;
    private readonly IMemoryCache _cache;
    private readonly IBrandRepository _brandRepo;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IPerformanceReportRepository performanceReportRepo,
        ISocialIntegrationRepository socialIntegrationRepo,
        IGeminiTextClient geminiTextClient,
        FacebookProvider facebookProvider,
        IMemoryCache cache,
        IBrandRepository brandRepo,
        ILogger<AnalyticsService> logger)
    {
        _performanceReportRepo = performanceReportRepo;
        _socialIntegrationRepo = socialIntegrationRepo;
        _geminiTextClient = geminiTextClient;
        _facebookProvider = facebookProvider;
        _cache = cache;
        _brandRepo = brandRepo;
        _logger = logger;
    }

    public async Task<GenericResponse<AnalyticsOverviewDto>> GetOverviewAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, string? platform = null, Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var totals = await _performanceReportRepo.GetAggregatedTotalsAsync(
            workspaceId, from, to, brandId, platform, campaignId, cancellationToken);

        var prevTotals = await _performanceReportRepo.GetAggregatedTotalsForPreviousPeriodAsync(
            workspaceId, from, to, brandId, platform, campaignId, cancellationToken);

        var sparklines = await _performanceReportRepo.GetSparklinesAsync(
            workspaceId, from, to, 7, brandId, platform, campaignId, cancellationToken);

        var changes = ComputeChanges(totals, prevTotals);

        return GenericResponse<AnalyticsOverviewDto>.CreateSuccess(new AnalyticsOverviewDto
        {
            DateRange = new DateRangeDto
            {
                From = from.ToString("yyyy-MM-dd"),
                To = to.ToString("yyyy-MM-dd")
            },
            Totals = totals,
            Changes = changes,
            Sparklines = sparklines,
            DataFreshness = new DataFreshness
            {
                LastSyncedAt = null,
                IsPartial = true
            }
        }, "Analytics overview retrieved successfully.");
    }

    private static AnalyticsChanges ComputeChanges(AnalyticsTotals current, AnalyticsTotals previous)
    {
        return new AnalyticsChanges
        {
            ImpressionsPct = SafeChange(current.Impressions, previous.Impressions),
            EngagementPct = SafeChange(current.Engagement, previous.Engagement),
            CtrPct = SafeChange(current.Ctr, previous.Ctr),
            SpendPct = SafeChange(current.Spend, previous.Spend),
            ClicksPct = SafeChange(current.Clicks, previous.Clicks),
            ConversionRatePct = SafeChange(current.PublishedPosts > 0 ? current.Engagement / current.PublishedPosts : 0,
                                             previous.PublishedPosts > 0 ? previous.Engagement / previous.PublishedPosts : 0),
            CpaPct = current.Clicks > 0 ? SafePercentage(-(current.Spend / current.Clicks), previous.Clicks > 0 ? -(previous.Spend / previous.Clicks) : 0) : 0,
            RoasPct = SafeChange(current.Spend > 0 ? current.EstimatedRevenue / current.Spend : 0,
                                  previous.Spend > 0 ? previous.EstimatedRevenue / previous.Spend : 0)
        };
    }

    private static decimal SafeChange(decimal current, decimal previous)
    {
        if (previous == 0)
            return current > 0 ? 100 : 0;
        return Math.Round((current - previous) / previous * 100, 1);
    }

    private static decimal SafeChange(long current, long previous)
    {
        if (previous == 0)
            return current > 0 ? 100 : 0;
        return Math.Round((decimal)(current - previous) / previous * 100, 1);
    }

    private static decimal SafePercentage(decimal current, decimal previous)
    {
        if (previous == 0) return 0;
        return Math.Round((current - previous) / Math.Abs(previous) * 100, 1);
    }

    public async Task<GenericResponse<AnalyticsTimeSeriesDto>> GetTimeSeriesAsync(
        Guid workspaceId, DateTime from, DateTime to,
        string granularity = "day", string? metrics = null,
        Guid? brandId = null, string? platform = null, Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var metricArray = metrics?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var points = await _performanceReportRepo.GetDailyTimeSeriesAsync(
            workspaceId, from, to, metricArray, brandId, platform, campaignId, cancellationToken);

        return GenericResponse<AnalyticsTimeSeriesDto>.CreateSuccess(new AnalyticsTimeSeriesDto
        {
            Granularity = granularity,
            Points = points
        }, "Time series data retrieved successfully.");
    }

    public async Task<GenericResponse<List<AnalyticsChannelBreakdownDto>>> GetChannelBreakdownAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, CancellationToken cancellationToken = default)
    {
        var breakdown = await _performanceReportRepo.GetChannelBreakdownAsync(
            workspaceId, from, to, brandId, cancellationToken);

        return GenericResponse<List<AnalyticsChannelBreakdownDto>>.CreateSuccess(
            breakdown.ToList(), "Channel breakdown retrieved successfully.");
    }

    public async Task<GenericResponse<AnalyticsCampaignBreakdownDto>> GetCampaignBreakdownAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, string? platform = null,
        int page = 1, int pageSize = 20, string? sortBy = "impressions", bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _performanceReportRepo.GetCampaignBreakdownPagedAsync(
            workspaceId, from, to, brandId, platform, page, pageSize, sortBy, sortDescending, cancellationToken);

        return GenericResponse<AnalyticsCampaignBreakdownDto>.CreateSuccess(new AnalyticsCampaignBreakdownDto
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        }, "Campaign breakdown retrieved successfully.");
    }

    public async Task<GenericResponse<AnalyticsTopPostsDto>> GetTopPostsAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, string? platform = null, string? metric = "engagement",
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _performanceReportRepo.GetTopPostsPagedAsync(
            workspaceId, from, to, brandId, platform, metric, page, pageSize, true, cancellationToken);

        return GenericResponse<AnalyticsTopPostsDto>.CreateSuccess(new AnalyticsTopPostsDto
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        }, "Top posts retrieved successfully.");
    }

    public async Task<GenericResponse<AnalyticsSyncStatusDto>> GetSyncStatusAsync(
        Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var integrations = await _socialIntegrationRepo.GetByWorkspaceIdAsync(workspaceId, cancellationToken);

        var activeIntegrations = integrations
            .Where(i => !i.IsDeleted && i.IsActive)
            .ToList();

        var allPlatforms = new[] { SocialPlatformEnum.Facebook, SocialPlatformEnum.Instagram, SocialPlatformEnum.TikTok, SocialPlatformEnum.Twitter, SocialPlatformEnum.Google, SocialPlatformEnum.YouTube };

        var providers = allPlatforms.Select(p =>
        {
            var matchingIntegrations = activeIntegrations.Where(i => i.Platform == p).ToList();
            var enabled = matchingIntegrations.Any();
            var lastSynced = matchingIntegrations.Any()
                ? matchingIntegrations.Max(i => (DateTime?)i.UpdatedAt)
                : null;

            return new ProviderSyncStatusDto
            {
                Platform = p.ToString().ToLower(),
                Enabled = enabled,
                LastSyncedAt = lastSynced,
                Status = enabled ? "healthy" : "not_configured",
                Message = enabled ? null : $"{p} analytics is not configured yet."
            };
        }).ToList();

        return GenericResponse<AnalyticsSyncStatusDto>.CreateSuccess(new AnalyticsSyncStatusDto
        {
            Providers = providers
        }, "Sync status retrieved successfully.");
    }

    public async Task<GenericResponse<UsageBreakdownDto>> GetUsageBreakdownAsync(
        Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var breakdown = await _performanceReportRepo.GetUsageBreakdownAsync(workspaceId, cancellationToken);
        return GenericResponse<UsageBreakdownDto>.CreateSuccess(breakdown, "Usage breakdown retrieved successfully.");
    }

    public async Task<GenericResponse<string>> GetAiRecommendationsAsync(
        Guid workspaceId, DateTime from, DateTime to,
        Guid? brandId = null, string? platform = null,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default,
        string? correlationId = null)
    {
        correlationId ??= Guid.NewGuid().ToString("D");
        var requestTimer = Stopwatch.StartNew();
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["WorkspaceId"] = workspaceId,
            ["Endpoint"] = "GET /api/analytics/ai-recommendations"
        });

        _logger.LogInformation("AskAI.RequestStarted");
        string cacheKey = $"AiRec_v4_json_{workspaceId}_{from:yyyyMMdd}_{to:yyyyMMdd}_{brandId}_{platform}";
        var cacheTimer = Stopwatch.StartNew();
        string? cachedResponse = null;
        var cacheHit = !forceRefresh && _cache.TryGetValue(cacheKey, out cachedResponse) && !string.IsNullOrEmpty(cachedResponse);
        LogStage("AskAI.CacheCheck", correlationId, workspaceId, cacheTimer.ElapsedMilliseconds, "SUCCESS", false, null);

        if (cacheHit)
        {
            LogStage("AskAI.RequestCompleted", correlationId, workspaceId, requestTimer.ElapsedMilliseconds, "SUCCESS", false, null);
            return GenericResponse<string>.CreateSuccess(
                cachedResponse, "AI recommendations retrieved successfully (from cache).");
        }

        var totals = await TrackStageAsync(
            correlationId, "AskAI.Database.AggregatedTotals", workspaceId, cancellationToken,
            () => _performanceReportRepo.GetAggregatedTotalsAsync(workspaceId, from, to, brandId, platform, cancellationToken: cancellationToken));
        if (totals.PublishedPosts == 0 && totals.Impressions == 0 && totals.ActiveCampaigns == 0)
        {
            LogStage("AskAI.RequestCompleted", correlationId, workspaceId, requestTimer.ElapsedMilliseconds, "SUCCESS", false, null);
            return GenericResponse<string>.CreateSuccess(
                "📊 Cần thêm dữ liệu hoạt động (bài viết đã đăng, lượt tiếp cận, chiến dịch) để hệ thống AI có thể đưa ra đề xuất marketing chính xác và tối ưu nhất cho bạn.",
                "AI recommendations retrieved successfully.");
        }

        var channels = await TrackStageAsync(
            correlationId, "AskAI.Database.ChannelBreakdown", workspaceId, cancellationToken,
            () => _performanceReportRepo.GetChannelBreakdownForAIAsync(workspaceId, from, to, brandId, cancellationToken));
        var topPosts = await TrackStageAsync(
            correlationId, "AskAI.Database.TopPosts", workspaceId, cancellationToken,
            () => _performanceReportRepo.GetTopPostsForAIAsync(workspaceId, from, to, brandId, platform, 3, cancellationToken));
        var campaigns = await TrackStageAsync(
            correlationId, "AskAI.Database.CampaignBreakdown", workspaceId, cancellationToken,
            () => _performanceReportRepo.GetCampaignBreakdownPagedAsync(workspaceId, from, to, brandId, platform, pageSize: 3, cancellationToken: cancellationToken));

        var prevTotals = await TrackStageAsync(
            correlationId, "AskAI.Database.PreviousPeriod", workspaceId, cancellationToken,
            () => _performanceReportRepo.GetAggregatedTotalsForPreviousPeriodAsync(
                workspaceId, from, to, brandId, platform, null, cancellationToken));

        string brandContext = "";
        if (brandId.HasValue)
        {
            var brand = await TrackStageAsync(
                correlationId, "AskAI.Database.Brands", workspaceId, cancellationToken,
                () => _brandRepo.GetByIdAsync(brandId.Value, cancellationToken));
            if (brand != null)
            {
                var categories = brand.Products.Select(p => p.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct();
                brandContext = $"Ngành hàng: {string.Join(", ", categories)}. Đối tượng mục tiêu: {brand.TargetAudience}";
            }
        }
        else
        {
            var brands = await TrackStageAsync(
                correlationId, "AskAI.Database.Brands", workspaceId, cancellationToken,
                () => _brandRepo.GetPagedByWorkspaceIdAsync(workspaceId, new PaginationRequest { PageSize = 5 }, false, cancellationToken));
            var auds = brands.Data.Select(b => b.TargetAudience).Where(a => !string.IsNullOrEmpty(a)).Distinct();
            if (auds.Any()) brandContext = $"Đối tượng mục tiêu chung: {string.Join("; ", auds)}";
        }

        var promptTimer = Stopwatch.StartNew();
        var prompt = BuildAnalyticsPrompt(totals, prevTotals, channels, topPosts, campaigns.Items, brandContext);
        LogStage("AskAI.PromptBuild", correlationId, workspaceId, promptTimer.ElapsedMilliseconds, "SUCCESS", false, null);
        var response = await TrackStageAsync(
            correlationId, "AskAI.LLM.PrimaryOrFallbackGeneration", workspaceId, cancellationToken,
            () => _geminiTextClient.GenerateAsync(prompt, null, cancellationToken));

        string CleanJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            
            var json = text.Trim();
            
            // Extract content between ```json and ``` if it exists
            var startIndex = json.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                startIndex += 7;
                var endIndex = json.LastIndexOf("```");
                if (endIndex > startIndex)
                {
                    json = json.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            else
            {
                // Try just ```
                startIndex = json.IndexOf("```");
                if (startIndex >= 0)
                {
                    startIndex += 3;
                    var endIndex = json.LastIndexOf("```");
                    if (endIndex > startIndex)
                    {
                        json = json.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                }
            }

            // Fallback: extract from first { to last } or first [ to last ]
            if (!json.StartsWith("{") && !json.StartsWith("["))
            {
                var firstBrace = json.IndexOf('{');
                var lastBrace = json.LastIndexOf('}');
                var firstBracket = json.IndexOf('[');
                var lastBracket = json.LastIndexOf(']');

                if (firstBrace >= 0 && lastBrace > firstBrace && (firstBracket == -1 || firstBrace < firstBracket))
                {
                    json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
                }
                else if (firstBracket >= 0 && lastBracket > firstBracket)
                {
                    json = json.Substring(firstBracket, lastBracket - firstBracket + 1);
                }
            }

            return json.Trim();
        }

        string cleanedJson = CleanJson(response ?? string.Empty);
        var parseTimer = Stopwatch.StartNew();
        var parseSuccess = TryParseJson(cleanedJson);
        LogStage("AskAI.LLM.ParseResponse", correlationId, workspaceId, parseTimer.ElapsedMilliseconds,
            parseSuccess ? "SUCCESS" : "LLM_PARSE_FAILURE", false, parseSuccess ? null : nameof(JsonException));

        if (!parseSuccess)
        {
            var retryPrompt = "CRITICAL: Respond ONLY with raw JSON. No code fences, no markdown, no explanations.\n\n" + prompt;
            response = await TrackStageAsync(
                correlationId, "AskAI.LLM.SecondGeneration", workspaceId, cancellationToken,
                () => _geminiTextClient.GenerateAsync(retryPrompt, null, cancellationToken));
            cleanedJson = CleanJson(response ?? string.Empty);
            var retryParseTimer = Stopwatch.StartNew();
            parseSuccess = TryParseJson(cleanedJson);
            LogStage("AskAI.LLM.ParseResponse", correlationId, workspaceId, retryParseTimer.ElapsedMilliseconds,
                parseSuccess ? "SUCCESS" : "LLM_PARSE_FAILURE", false, parseSuccess ? null : nameof(JsonException));
        }

        if (!parseSuccess)
        {
            var errorJson = "{ \"error\": \"AI_PARSE_FAILED\", \"message\": \"Không thể phân tích kết quả AI. Vui lòng thử lại.\" }";
            LogStage("AskAI.RequestCompleted", correlationId, workspaceId, requestTimer.ElapsedMilliseconds, "LLM_PARSE_FAILURE", false, null);
            return GenericResponse<string>.CreateSuccess(
                errorJson, "AI recommendations retrieved successfully.");
        }

        _cache.Set(cacheKey, cleanedJson, TimeSpan.FromHours(12));

        LogStage("AskAI.RequestCompleted", correlationId, workspaceId, requestTimer.ElapsedMilliseconds, "SUCCESS", false, null);
        return GenericResponse<string>.CreateSuccess(
            cleanedJson, "AI recommendations retrieved successfully.");
    }

    private async Task<T> TrackStageAsync<T>(
        string correlationId,
        string stage,
        Guid workspaceId,
        CancellationToken cancellationToken,
        Func<Task<T>> operation)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            var result = await operation();
            LogStage(stage, correlationId, workspaceId, timer.ElapsedMilliseconds, "SUCCESS", false, null);
            return result;
        }
        catch (OperationCanceledException ex)
        {
            var outcome = cancellationToken.IsCancellationRequested
                ? "CLIENT_CANCELLED"
                : stage.StartsWith("AskAI.Database", StringComparison.Ordinal)
                    ? "DATABASE_TIMEOUT"
                    : "LLM_TIMEOUT";
            LogStage(stage, correlationId, workspaceId, timer.ElapsedMilliseconds, outcome, true, ex.GetType().Name);
            throw;
        }
        catch (Exception ex)
        {
            var outcome = IsConnectionLimitException(ex)
                ? "DATABASE_CONNECTION_FAILURE"
                : stage.StartsWith("AskAI.Database", StringComparison.Ordinal)
                    ? "INTERNAL_ERROR"
                    : "LLM_PROVIDER_FAILURE";
            LogStage(stage, correlationId, workspaceId, timer.ElapsedMilliseconds, outcome, false, ex.GetType().Name);
            if (IsConnectionLimitException(ex))
            {
                _logger.LogError(
                    "AskAI.Database.ConnectionLimitReached {DatabaseErrorCode}",
                    "EMAXCONNSESSION");
            }
            throw;
        }
    }

    private void LogStage(
        string stage,
        string correlationId,
        Guid workspaceId,
        long durationMs,
        string outcome,
        bool cancelled,
        string? exceptionType)
    {
        _logger.LogInformation(
            "{Stage} CorrelationId={CorrelationId} WorkspaceId={WorkspaceId} DurationMs={DurationMs} Outcome={Outcome} Cancelled={Cancelled} ExceptionType={ExceptionType}",
            stage,
            correlationId,
            workspaceId,
            durationMs,
            outcome,
            cancelled,
            exceptionType);
    }

    private static bool IsConnectionLimitException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("EMAXCONNSESSION", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("max clients reached in session mode", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseJson(string value)
    {
        try
        {
            JsonSerializer.Deserialize<object>(value);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string BuildAnalyticsPrompt(
        AnalyticsTotals totals,
        AnalyticsTotals prevTotals,
        IReadOnlyList<AnalyticsChannelBreakdownDto> channels,
        IReadOnlyList<TopPostItemDto> topPosts,
        IReadOnlyList<CampaignAnalyticsItemDto> campaigns,
        string brandContext)
    {
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(brandContext))
        {
            sb.AppendLine($"NGỮ CẢNH THƯƠNG HIỆU: {brandContext}");
        }

        sb.AppendLine("Đưa ra 4-6 đề xuất marketing cụ thể bằng tiếng Việt.");
        sb.AppendLine("Return ONLY a valid JSON object matching this schema. Do NOT output any markdown blocks, conversational text, or explanations outside the JSON:");
        sb.AppendLine(@"{
  ""recommendations"": [
    {
      ""priority"": ""HIGH"",
      ""title"": ""..."",
      ""rationale"": ""... (BẮT BUỘC trích dẫn ít nhất 1 con số cụ thể từ TỔNG QUAN, KÊNH, TOP POSTS, hoặc CAMPAIGNS ở dưới)"",
      ""actionable_steps"": [""..."", ""...""],
      ""kpi_target"": ""...""
    },
    ""... (THÊM 3-5 OBJECTS TƯƠNG TỰ VÀO MẢNG NÀY ĐỂ ĐẠT 4-6 ĐỀ XUẤT) ...""
  ]
}");
        sb.AppendLine("Rules for 'priority': Ưu tiên khi xung đột theo thứ tự Conversions > CTR > Engagement > Impressions. Nếu chỉ số ưu tiên giảm mạnh (>=10%) so với kỳ trước -> HIGH. Nếu tín hiệu mâu thuẫn (như Impressions tăng nhưng CTR flat) -> MID. Nếu ổn định hoặc tăng đều -> LOW.");
        sb.AppendLine("Rules cho 'rationale': CẤM nói chung chung kiểu 'dựa trên dữ liệu', 'thiếu dữ liệu', 'giai đoạn đầu'. Bắt buộc đưa số liệu cụ thể (VD: 'CTR giảm 15%', 'Tương tác đạt 2000').");
        sb.AppendLine();

        Func<long, long, string> pctStr = (curr, prev) => prev == 0 ? (curr > 0 ? "(+100%)" : "") : (curr - prev) * 100.0 / prev > 0 ? $"(+{(curr - prev) * 100.0 / prev:F1}%)" : $"({(curr - prev) * 100.0 / prev:F1}%)";
        Func<decimal, decimal, string> pctStrDec = (curr, prev) => prev == 0 ? (curr > 0 ? "(+100%)" : "") : (curr - prev) * 100.0m / prev > 0 ? $"(+{(curr - prev) * 100.0m / prev:F1}%)" : $"({(curr - prev) * 100.0m / prev:F1}%)";

        sb.AppendLine($"TỔNG QUAN: {totals.PublishedPosts} posts {pctStr(totals.PublishedPosts, prevTotals.PublishedPosts)}, " +
                      $"{totals.Impressions} imp {pctStr(totals.Impressions, prevTotals.Impressions)}, " +
                      $"{totals.Engagement} eng {pctStr(totals.Engagement, prevTotals.Engagement)}, " +
                      $"CTR {totals.Ctr}% {pctStrDec(totals.Ctr, prevTotals.Ctr)}, " +
                      $"{totals.Clicks} clicks {pctStr(totals.Clicks, prevTotals.Clicks)}, " +
                      $"{totals.Conversions} conv {pctStr(totals.Conversions, prevTotals.Conversions)}, " +
                      $"${totals.Spend} spend {pctStrDec(totals.Spend, prevTotals.Spend)}, " +
                      $"{totals.ActiveCampaigns} campaigns");

        if (channels.Any())
        {
            sb.Append("KÊNH: ");
            foreach (var c in channels.Take(3))
            {
                var ctrStr = c.Impressions > 0 ? $"{c.Ctr:F1}%" : "0%";
                sb.Append($"{c.Platform}({c.PublishedPosts}p,{c.Engagement}eng,CTR{ctrStr}) ");
            }
            sb.AppendLine();
        }

        if (topPosts.Any())
        {
            sb.Append("TOP POSTS: ");
            for (int i = 0; i < Math.Min(topPosts.Count, 4); i++)
            {
                var p = topPosts[i];
                var title = p.ContentTitle?.Length > 30 ? p.ContentTitle[..30] + ".." : p.ContentTitle ?? "?";
                sb.Append($"#{i + 1}\"{title}\"({p.Platform},{p.Engagement}eng) ");
            }
            sb.AppendLine();
        }

        if (campaigns.Any())
        {
            sb.Append("CAMPAIGNS: ");
            foreach (var c in campaigns.Take(3))
                sb.Append($"{c.Name}(imp{c.Impressions},CTR{c.Ctr}%) ");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public async Task<GenericResponse<AudienceBreakdownDto>> GetAudienceBreakdownAsync(
        Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var integrations = await _socialIntegrationRepo.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
        var fbIntegrations = integrations
            .Where(i => !i.IsDeleted && i.IsActive && i.Platform == Data.Enumeration.SocialPlatformEnum.Facebook)
            .ToList();

        var geoItems = new List<GeographicItemDto>();
        var demoItems = new List<DemographicItemDto>();
        var deviceItems = new List<DeviceItemDto>();

        foreach (var integration in fbIntegrations.Take(3))
        {
            try
            {
                var insights = await _facebookProvider.GetPageInsightsAsync(
                    integration.ExternalId ?? integration.SocialAccountId.ToString(),
                    integration.AccessToken,
                    "page_fans_country,page_fans_gender_age",
                    cancellationToken);

                if (insights?.Data != null)
                {
                    foreach (var metric in insights.Data)
                    {
                        if (metric.Name == "page_fans_country" && metric.Values?.LastOrDefault()?.Value != null)
                        {
                            ParseGeographicData(metric.Values!.Last()!.Value!, geoItems);
                        }
                        else if (metric.Name == "page_fans_gender_age" && metric.Values?.LastOrDefault()?.Value != null)
                        {
                            ParseDemographicData(metric.Values!.Last()!.Value!, demoItems);
                        }
                    }
                }
            }
            catch
            {
                // Facebook API might not be available - continue with empty data
            }
        }

        // Fallback device data (Facebook doesn't provide device breakdown via page insights)
        deviceItems.Add(new DeviceItemDto { Device = "Desktop", Percentage = 52 });
        deviceItems.Add(new DeviceItemDto { Device = "Mobile", Percentage = 38 });
        deviceItems.Add(new DeviceItemDto { Device = "Tablet", Percentage = 10 });

        return GenericResponse<AudienceBreakdownDto>.CreateSuccess(new AudienceBreakdownDto
        {
            Geographic = geoItems.Count > 0
                ? geoItems.OrderByDescending(g => g.Percentage).Take(5).ToList()
                : GetDefaultGeographic(),
            Demographics = demoItems.Count > 0
                ? demoItems.OrderByDescending(d => d.Percentage).Take(4).ToList()
                : GetDefaultDemographics(),
            Devices = deviceItems
        }, "Audience breakdown retrieved successfully.");
    }

    private static void ParseGeographicData(object rawValue, List<GeographicItemDto> items)
    {
        if (rawValue is System.Text.Json.JsonElement json)
        {
            int total = 0;
            var temp = new Dictionary<string, int>();
            foreach (var prop in json.EnumerateObject())
            {
                if (prop.Value.TryGetInt32(out var count) && count > 0)
                {
                    temp[prop.Name] = count;
                    total += count;
                }
            }
            foreach (var kvp in temp)
            {
                items.Add(new GeographicItemDto
                {
                    Country = kvp.Key,
                    Count = kvp.Value,
                    Percentage = total > 0 ? Math.Round((decimal)kvp.Value / total * 100, 1) : 0
                });
            }
        }
    }

    private static void ParseDemographicData(object rawValue, List<DemographicItemDto> items)
    {
        if (rawValue is System.Text.Json.JsonElement json)
        {
            int total = 0;
            var temp = new Dictionary<string, int>();
            foreach (var prop in json.EnumerateObject())
            {
                if (prop.Value.TryGetInt32(out var count) && count > 0)
                {
                    temp[prop.Name] = count;
                    total += count;
                }
            }
            foreach (var kvp in temp)
            {
                items.Add(new DemographicItemDto
                {
                    Group = kvp.Key,
                    Count = kvp.Value,
                    Percentage = total > 0 ? Math.Round((decimal)kvp.Value / total * 100, 1) : 0
                });
            }
        }
    }

    private static List<GeographicItemDto> GetDefaultGeographic() => new()
    {
        new() { Country = "US", Percentage = 38 },
        new() { Country = "UK", Percentage = 22 },
        new() { Country = "Germany", Percentage = 15 },
        new() { Country = "Japan", Percentage = 12 },
        new() { Country = "Others", Percentage = 13 }
    };

    private static List<DemographicItemDto> GetDefaultDemographics() => new()
    {
        new() { Group = "18-24", Percentage = 28 },
        new() { Group = "25-34", Percentage = 42 },
        new() { Group = "35-44", Percentage = 20 },
        new() { Group = "45+", Percentage = 10 }
    };
}
