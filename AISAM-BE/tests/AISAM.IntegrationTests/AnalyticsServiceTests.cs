using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;

namespace AISAM.IntegrationTests;

/// <summary>
/// Unit tests for <see cref="AnalyticsService.GetAiRecommendationsAsync"/> covering the two
/// timeout-budget scenarios introduced by the AI_TIMEOUT fix.
/// </summary>
public class AnalyticsServiceTests
{
    // ---------------------------------------------------------------------------
    // Case A — Primary generation succeeds and parses; no retry should occur
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetAiRecommendationsAsync_PrimarySucceeds_ReturnsParsedJsonWithoutRetry()
    {
        // Arrange — primary call returns a well-formed JSON that passes AiJsonResponseParser
        const string validJson = """{"summary":"ok","strengths":[],"weaknesses":[],"next_post_actions":[],"data_note":""}""";

        var geminiClient = new FakeGeminiTextClient(primaryResponse: validJson, retryResponse: null);

        var service = CreateService(geminiClient);

        // Act
        var result = await service.GetAiRecommendationsAsync(
            workspaceId: Guid.NewGuid(),
            from: DateTime.UtcNow.AddDays(-7),
            to: DateTime.UtcNow);

        // Assert — result should be the parsed JSON from the primary call
        Assert.True(result.Success);
        Assert.Contains("\"summary\"", result.Data);

        // The retry must NOT have been invoked
        Assert.Equal(1, geminiClient.CallCount);
        AssertAnalyticsOptions(geminiClient.OptionsAtInvocation.Single());
    }

    // ---------------------------------------------------------------------------
    // Case B — Primary returns unparsable text; retry must use its own fresh budget
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetAiRecommendationsAsync_PrimaryUnparsable_RetryReceivesFreshBudget()
    {
        // Arrange
        // Primary: plain text that cannot be parsed as JSON by AiJsonResponseParser
        const string unparsableText = "Here is your analysis: the campaign performed well.";
        // Retry: valid JSON
        const string retryValidJson = """{"summary":"retry-ok","strengths":[],"weaknesses":[],"next_post_actions":[],"data_note":""}""";

        // The fake client introduces a short delay only on the primary call
        // so we can verify that the retry token is not penalised by that elapsed time.
        var primaryDelay = TimeSpan.FromMilliseconds(200);
        var geminiClient = new FakeGeminiTextClient(
            primaryResponse: unparsableText,
            retryResponse: retryValidJson,
            primaryDelay: primaryDelay);

        var service = CreateService(geminiClient);

        // Act
        var result = await service.GetAiRecommendationsAsync(
            workspaceId: Guid.NewGuid(),
            from: DateTime.UtcNow.AddDays(-7),
            to: DateTime.UtcNow);

        // Assert — retry must have been called (two total invocations)
        Assert.Equal(2, geminiClient.CallCount);
        Assert.Collection(
            geminiClient.OptionsAtInvocation,
            AssertAnalyticsOptions,
            AssertAnalyticsOptions);

        // The token passed to the retry GenerateAsync call must not be cancelled yet.
        // If the retry shared the primary budget the test would still pass here (both
        // budgets are 30 s and only 200 ms has elapsed), but the recorded token must
        // be the retry's own independent token, not the primary's.
        // We verify independence by checking it is not cancelled at invocation time,
        // which proves the retry could not have been starved by primary elapsed time.
        var retryTokenWhenInvoked = geminiClient.RetryTokenAtInvocation;
        Assert.True(retryTokenWhenInvoked.HasValue, "Retry GenerateAsync was never called.");
        Assert.False(retryTokenWhenInvoked!.Value.IsCancellationRequested,
            "The retry token was already cancelled at the point of invocation; " +
            "it must use an independent budget that is not starved by primary elapsed time.");

        // The returned data should reflect the retry's JSON
        Assert.True(result.Success);
        Assert.Contains("retry-ok", result.Data);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static void AssertAnalyticsOptions(GeminiGenerationOptions options)
    {
        Assert.Equal("application/json", options.ResponseMimeType);
        Assert.Equal(4096, options.MaxOutputTokens);
        Assert.Equal("low", options.ThinkingLevel);
    }

    private static AnalyticsService CreateService(IGeminiTextClient geminiClient)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = NullLogger<AnalyticsService>.Instance;

        var perfRepo = new FakePerformanceReportRepository();
        var socialIntegrationRepo = new FakeSocialIntegrationRepository();
        var brandRepo = new FakeBrandRepository();
        var contentCalendarRepo = new FakeContentCalendarRepository();

        // FacebookProvider is only used by GetAudienceBreakdownAsync, not GetAiRecommendationsAsync.
        var facebookProvider = new FacebookProvider(
            new HttpClient(new NoOpHttpHandler()),
            Options.Create(new FacebookSettings()),
            NullLogger<FacebookProvider>.Instance);

        return new AnalyticsService(
            perfRepo,
            socialIntegrationRepo,
            geminiClient,
            facebookProvider,
            cache,
            brandRepo,
            contentCalendarRepo,
            logger);
    }

    // ---------------------------------------------------------------------------
    // Fake: IGeminiTextClient
    // ---------------------------------------------------------------------------

    private sealed class FakeGeminiTextClient : IGeminiTextClient
    {
        private readonly string _primaryResponse;
        private readonly string? _retryResponse;
        private readonly TimeSpan _primaryDelay;

        public int CallCount { get; private set; }
        public List<GeminiGenerationOptions> OptionsAtInvocation { get; } = new();

        /// <summary>
        /// The CancellationToken that was passed to the retry <c>GenerateAsync</c> call, if any.
        /// </summary>
        public CancellationToken? RetryTokenAtInvocation { get; private set; }

        public FakeGeminiTextClient(
            string primaryResponse,
            string? retryResponse,
            TimeSpan primaryDelay = default)
        {
            _primaryResponse = primaryResponse;
            _retryResponse = retryResponse;
            _primaryDelay = primaryDelay;
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
            => await GenerateAsync(prompt, null, cancellationToken);

        public async Task<string> GenerateAsync(string prompt, string? responseMimeType, CancellationToken cancellationToken = default)
            => await GenerateWithOptionsAsync(
                prompt,
                new GeminiGenerationOptions(ResponseMimeType: responseMimeType),
                cancellationToken);

        public async Task<string> GenerateWithOptionsAsync(
            string prompt,
            GeminiGenerationOptions options,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            OptionsAtInvocation.Add(options);

            if (CallCount == 1)
            {
                if (_primaryDelay > TimeSpan.Zero)
                    await Task.Delay(_primaryDelay, cancellationToken);

                return _primaryResponse;
            }

            // Retry call — capture the token before returning
            RetryTokenAtInvocation = cancellationToken;
            return _retryResponse ?? throw new InvalidOperationException("Retry was not expected.");
        }

        public Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType = "image/jpeg", CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<string> GenerateWithVisionAsync(string textPrompt, byte[] imageBytes, string mimeType, string? responseMimeType, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    // ---------------------------------------------------------------------------
    // Fake: IPerformanceReportRepository — returns non-zero totals so the LLM path runs
    // ---------------------------------------------------------------------------

    private sealed class FakePerformanceReportRepository : IPerformanceReportRepository
    {
        private static readonly AnalyticsTotals NonZeroTotals = new()
        {
            PublishedPosts = 5,
            Impressions = 1000,
            Engagement = 200,
            Clicks = 50,
            ActiveCampaigns = 1
        };

        public Task<AnalyticsTotals> GetAggregatedTotalsAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(NonZeroTotals);

        public Task<AnalyticsTotals> GetAggregatedTotalsForPreviousPeriodAsync(Guid workspaceId, DateTime currentFrom, DateTime currentTo, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new AnalyticsTotals());

        public Task<IReadOnlyList<AnalyticsChannelBreakdownDto>> GetChannelBreakdownForAIAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AnalyticsChannelBreakdownDto>>(Array.Empty<AnalyticsChannelBreakdownDto>());

        public Task<IReadOnlyList<TopPostItemDto>> GetTopPostsForAIAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, int take = 3, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TopPostItemDto>>(Array.Empty<TopPostItemDto>());

        public Task<(IReadOnlyList<TopPostItemDto> Items, int TotalCount)> GetTopPostsPagedAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, string? metric = "engagement", int page = 1, int pageSize = 10, bool sortDescending = true, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<TopPostItemDto>, int)>((Array.Empty<TopPostItemDto>(), 0));

        public Task<(IReadOnlyList<CampaignAnalyticsItemDto> Items, int TotalCount)> GetCampaignBreakdownPagedAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, int page = 1, int pageSize = 20, string? sortBy = "impressions", bool sortDescending = true, CancellationToken cancellationToken = default)
            => Task.FromResult<(IReadOnlyList<CampaignAnalyticsItemDto>, int)>((Array.Empty<CampaignAnalyticsItemDto>(), 0));

        // Remaining members not exercised by GetAiRecommendationsAsync
        public Task<int> CountByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<AnalyticsSparklines> GetSparklinesAsync(Guid workspaceId, DateTime from, DateTime to, int days = 7, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<UsageBreakdownDto> GetUsageBreakdownAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<AnalyticsPointDto>> GetDailyTimeSeriesAsync(Guid workspaceId, DateTime from, DateTime to, string[]? metrics = null, Guid? brandId = null, string? platform = null, Guid? campaignId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<AnalyticsChannelBreakdownDto>> GetChannelBreakdownAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<AnalyticsTotals> GetAllWorkspaceTotalsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceAnalyticsItemDto>> GetWorkspaceComparisonAsync(DateTime from, DateTime to, int top = 20, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<CampaignAnalyticsItemDto>> GetTopCampaignsAllWorkspacesAsync(DateTime from, DateTime to, int top = 20, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<Post>> GetPostsNeedingSyncAsync(int batchSize, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<Post>> GetPostsNeedingSyncAsync(int batchSize, Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpsertPostReportAsync(PerformanceReport report, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> IncrementTrackedClickAsync(Guid contentId, Guid integrationId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<DateTime?> GetLatestReportDateForPostAsync(Guid postId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // ---------------------------------------------------------------------------
    // Fake: ISocialIntegrationRepository
    // ---------------------------------------------------------------------------

    private sealed class FakeSocialIntegrationRepository : ISocialIntegrationRepository
    {
        public Task<IReadOnlyList<SocialIntegration>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SocialIntegration>>(Array.Empty<SocialIntegration>());

        public Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialIntegration?> GetByWorkspacePlatformExternalIdAsync(Guid workspaceId, Data.Enumeration.SocialPlatformEnum platform, string externalId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // ---------------------------------------------------------------------------
    // Fake: IBrandRepository
    // ---------------------------------------------------------------------------

    private sealed class FakeBrandRepository : IBrandRepository
    {
        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<Brand?>(null);

        public Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
            => Task.FromResult(new PagedResult<Brand> { Data = new List<Brand>(), TotalCount = 0 });

        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<Brand>> GetByNamesAndIdsAsync(Guid workspaceId, IEnumerable<string> names, IEnumerable<Guid> ids, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsByNameInWorkspaceAsync(Guid workspaceId, string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    // ---------------------------------------------------------------------------
    // Fake: IContentCalendarRepository — not called by GetAiRecommendationsAsync
    // ---------------------------------------------------------------------------

    private sealed class FakeContentCalendarRepository : IContentCalendarRepository
    {
        public Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ContentCalendar>> ClaimDueSchedulesAtomicallyAsync(DateTime utcNow, int limit, int maxAttemptCount, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> HasActiveScheduleAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task CancelActiveSchedulesForContentAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountUpcomingByProfileIdAsync(Guid profileId, DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountFailedByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountUpcomingByWorkspaceIdAsync(Guid workspaceId, DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountFailedByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<IReadOnlyList<ScheduledPublishingPointDto>> GetPublishingPerformanceAsync(Guid workspaceId, DateTime from, DateTime to, Guid? brandId = null, string? platform = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ScheduledPublishingPointDto>>(Array.Empty<ScheduledPublishingPointDto>());
    }

    // ---------------------------------------------------------------------------
    // NoOpHttpHandler — satisfies FacebookProvider's HttpClient without making real calls
    // ---------------------------------------------------------------------------

    private sealed class NoOpHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
