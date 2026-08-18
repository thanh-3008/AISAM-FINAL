using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Common.Models;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AISAM.Services.Service
{
    public sealed class PostInsightsSyncService : IPostInsightsSyncService
    {
        private const int BatchSize = 10;
        private readonly IPerformanceReportRepository _perfReportRepository;
        private readonly ISocialIntegrationRepository _socialIntegrationRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IReadOnlyDictionary<string, IProviderService> _providers;
        private readonly ISocialTokenProtector _tokenProtector;
        private readonly ILogger<PostInsightsSyncService> _logger;

        public PostInsightsSyncService(
            IPerformanceReportRepository perfReportRepository,
            ISocialIntegrationRepository socialIntegrationRepository,
            IContentRepository contentRepository,
            IPostRepository postRepository,
            IEnumerable<IProviderService> providers,
            ISocialTokenProtector tokenProtector,
            ILogger<PostInsightsSyncService> logger)
        {
            _perfReportRepository = perfReportRepository;
            _socialIntegrationRepository = socialIntegrationRepository;
            _contentRepository = contentRepository;
            _postRepository = postRepository;
            _providers = providers
                .Where(p => p.ProviderName.Equals("facebook", StringComparison.OrdinalIgnoreCase)
                    || p.ProviderName.Equals("instagram", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
            _tokenProtector = tokenProtector;
            _logger = logger;
        }

        public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
        {
            var result = await ProcessNextDetailedAsync(cancellationToken);
            return result.ProcessedCount > 0;
        }

        public async Task<PostInsightsSyncResultDto> ProcessNextDetailedAsync(CancellationToken cancellationToken = default)
        {
            var posts = await _perfReportRepository.GetPostsNeedingSyncAsync(BatchSize, cancellationToken);
            return await ProcessPostsAsync(posts, cancellationToken);
        }

        public async Task<PostInsightsSyncResultDto> ProcessWorkspaceAsync(
            Guid workspaceId,
            DateTime from,
            DateTime to,
            Guid? brandId = null,
            string? platform = null,
            CancellationToken cancellationToken = default)
        {
            var posts = await _perfReportRepository.GetPostsNeedingSyncAsync(
                50,
                workspaceId,
                from,
                to,
                brandId,
                platform,
                cancellationToken);
            return await ProcessPostsAsync(posts, cancellationToken);
        }

        private async Task<List<string>> ImportFacebookPagePostsAsync(
            Guid workspaceId,
            DateTime from,
            DateTime to,
            Guid? brandId,
            string? platform,
            CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(platform) && !platform.Equals("facebook", StringComparison.OrdinalIgnoreCase))
                return errors;

            if (!_providers.TryGetValue("facebook", out var provider) || provider is not FacebookProvider facebookProvider)
            {
                errors.Add("facebook: provider is not registered.");
                return errors;
            }

            var integrations = await _socialIntegrationRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
            var facebookIntegrations = integrations
                .Where(integration =>
                    !integration.IsDeleted &&
                    integration.IsActive &&
                    integration.Platform == SocialPlatformEnum.Facebook &&
                    !string.IsNullOrWhiteSpace(integration.ExternalId) &&
                    (!brandId.HasValue || integration.BrandId == brandId.Value))
                .ToList();

            if (facebookIntegrations.Count == 0)
            {
                errors.Add("facebook: no active page integrations found for this workspace/filter.");
                return errors;
            }

            foreach (var integration in facebookIntegrations)
            {
                var pageAccessToken = await ResolveAccessTokenAsync(integration, cancellationToken);
                if (string.IsNullOrWhiteSpace(pageAccessToken))
                {
                    errors.Add($"{integration.ExternalId}: missing or undecryptable Facebook page access token. Reconnect this page/target.");
                    continue;
                }

                try
                {
                    var publishedPosts = await facebookProvider.GetPublishedPostsAsync(
                        pageAccessToken,
                        integration.ExternalId!,
                        from,
                        to,
                        100,
                        cancellationToken);

                    foreach (var publishedPost in publishedPosts)
                    {
                        if (string.IsNullOrWhiteSpace(publishedPost.Id))
                            continue;

                        var normalizedExternalPostId = NormalizeFacebookPostId(publishedPost.Id, integration.ExternalId);
                        var postEntity = await _postRepository.GetByExternalPostIdInWorkspaceAsync(
                            workspaceId,
                            normalizedExternalPostId,
                            cancellationToken);
                        if (postEntity == null && publishedPost.Id.Contains('_', StringComparison.Ordinal))
                        {
                            var barePostId = publishedPost.Id[(publishedPost.Id.IndexOf('_') + 1)..];
                            postEntity = await _postRepository.GetByExternalPostIdInWorkspaceAsync(
                                workspaceId,
                                barePostId,
                                cancellationToken);
                        }

                        if (postEntity == null)
                        {
                            var message = string.IsNullOrWhiteSpace(publishedPost.Message)
                                ? "Facebook post"
                                : publishedPost.Message.Trim();
                            var title = message.Length > 80 ? $"{message[..80]}..." : message;

                            var content = await _contentRepository.AddAsync(new Content
                            {
                                ProfileId = integration.ProfileId,
                                WorkspaceId = integration.WorkspaceId,
                                BrandId = integration.BrandId,
                                AdType = AdTypeEnum.TextOnly,
                                Title = title,
                                TextContent = message,
                                Status = ContentStatusEnum.Published,
                                IsAiGenerated = false,
                                CreatedAt = publishedPost.CreatedTime ?? DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            }, cancellationToken);

                            postEntity = await _postRepository.AddAsync(new Post
                            {
                                ContentId = content.Id,
                                IntegrationId = integration.Id,
                                ExternalPostId = normalizedExternalPostId,
                                PublishedAt = publishedPost.CreatedTime ?? DateTime.UtcNow,
                                Status = ContentStatusEnum.Published
                            }, cancellationToken);

                        }

                        var importedInsights = ExtractPublishedPostInsights(publishedPost)
                            ?? await facebookProvider.GetPostInsightsAsync(pageAccessToken, publishedPost.Id, cancellationToken);
                        if (importedInsights != null)
                            await UpsertInsightsAsync(postEntity, importedInsights, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import Facebook page posts for integration {IntegrationId} ({ExternalId})", integration.Id, integration.ExternalId);
                    errors.Add($"{integration.ExternalId}: failed to import Facebook posts: {ex.Message}");
                }
            }

            return errors;
        }

        private async Task<PostInsightsSyncResultDto> ProcessPostsAsync(List<Post> posts, CancellationToken cancellationToken)
        {
            if (posts.Count == 0) return new PostInsightsSyncResultDto();

            var synced = 0;
            var skipped = 0;
            var errors = new List<string>();

            foreach (var post in posts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(post.ExternalPostId))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    var integration = post.Integration;
                    if (integration?.SocialAccount == null)
                    {
                        skipped++;
                        errors.Add($"{post.ExternalPostId}: missing social integration/account.");
                        continue;
                    }

                    if (!_providers.TryGetValue(integration.Platform.ToString().ToLowerInvariant(), out var provider))
                    {
                        _logger.LogDebug("Skipping post {PostId}: no insights provider for platform {Platform}", post.Id, integration.Platform);
                        skipped++;
                        errors.Add($"{post.ExternalPostId}: no insights provider for {integration.Platform}.");
                        continue;
                    }

                    var accessToken = await ResolveAccessTokenAsync(integration, cancellationToken);
                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        skipped++;
                        errors.Add($"{post.ExternalPostId}: missing or undecryptable access token. Reconnect this brand/page.");
                        continue;
                    }

                    FacebookPostInsightData? insights = null;
                    var platformName = integration.Platform.ToString().ToLowerInvariant();

                    if (platformName == "instagram")
                    {
                        insights = await provider.GetPostInsightsAsync(accessToken, post.ExternalPostId, cancellationToken);
                    }
                    else
                    {
                        var attemptedExternalIds = BuildExternalPostIdCandidates(post.ExternalPostId, integration.ExternalId).ToList();
                        foreach (var externalPostId in attemptedExternalIds)
                        {
                            insights = await provider.GetPostInsightsAsync(accessToken, externalPostId, cancellationToken);
                            if (insights != null)
                                break;
                        }
                    }

                    if (insights == null)
                    {
                        skipped++;
                        errors.Add($"{post.ExternalPostId}: provider returned no insights or engagement summary.");
                        continue;
                    }

                    _logger.LogInformation(
                        "[INSIGHTS-SYNC] Post {PostId} ({ExternalPostId}) raw data: Impressions={Impressions}, Clicks={Clicks}, EngagedUsers={EngagedUsers}, Reactions={Reactions}, Comments={Comments}, Shares={Shares}",
                        post.Id, post.ExternalPostId, insights.Impressions, insights.Clicks,
                        insights.EngagedUsers, insights.Reactions, insights.Comments, insights.Shares);

                    await UpsertInsightsAsync(post, insights, cancellationToken);
                    synced++;

                    if (!insights.Impressions.HasValue && insights.Diagnostics.Count > 0)
                    {
                        errors.Add($"{post.ExternalPostId}: impressions unavailable. {string.Join(" | ", insights.Diagnostics.Take(3))}");
                    }

                    _logger.LogInformation("Synced post insights for post {PostId} ({ExternalPostId}): {Impressions} impressions, {Engagement} engagement",
                        post.Id, post.ExternalPostId, insights.Impressions ?? insights.Views ?? 0, CalculateEngagement(insights));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync insights for post {PostId} ({ExternalPostId})", post.Id, post.ExternalPostId);
                    skipped++;
                    errors.Add($"{post.ExternalPostId}: {ex.Message}");
                }
            }

            return new PostInsightsSyncResultDto
            {
                ProcessedCount = posts.Count,
                SyncedCount = synced,
                SkippedCount = skipped,
                Errors = errors
            };
        }

        private async Task UpsertInsightsAsync(Post post, FacebookPostInsightData insights, CancellationToken cancellationToken)
        {
            var engagement = CalculateEngagement(insights);
            var impressions = insights.Impressions ?? insights.Views ?? 0;
            var clicks = insights.Clicks ?? 0;
            var ctr = impressions > 0 ? Math.Round((decimal)clicks / impressions, 4) : 0m;

            _logger.LogInformation(
                "[INSIGHTS-SYNC] Post {PostId} computed: Impressions={Impressions}, Clicks={Clicks}, Engagement={Engagement}, CTR={Ctr}",
                post.Id, impressions, clicks, engagement, ctr);

            var report = new PerformanceReport
            {
                PostId = post.Id,
                Impressions = impressions,
                Engagement = engagement,
                Clicks = clicks,
                Ctr = ctr,
                EstimatedRevenue = engagement * 0.01m,
                ReportDate = DateTime.UtcNow.Date,
                RawData = JsonSerializer.Serialize(insights),
                CreatedAt = DateTime.UtcNow
            };

            await _perfReportRepository.UpsertPostReportAsync(report, cancellationToken);
        }

        private static long CalculateEngagement(FacebookPostInsightData insights)
        {
            if (insights.EngagedUsers.HasValue && insights.EngagedUsers.Value > 0)
                return insights.EngagedUsers.Value;

            return (insights.Reactions ?? 0)
                + (insights.Comments ?? 0)
                + (insights.Shares ?? 0);
        }

        private static FacebookPostInsightData? ExtractPublishedPostInsights(FacebookPublishedPostData publishedPost)
        {
            var insightData = publishedPost.Insights?.Data;
            if (insightData == null || insightData.Count == 0)
                return null;

            var insights = new FacebookPostInsightData();

            foreach (var metric in insightData)
            {
                if (string.IsNullOrWhiteSpace(metric.Name))
                    continue;

                var rawValue = metric.Values?.LastOrDefault()?.Value;
                var numericValue = ExtractInsightNumber(rawValue);

                switch (metric.Name)
                {
                    case "post_impressions":
                        if (numericValue.HasValue) insights.Impressions = Math.Max(insights.Impressions ?? 0, numericValue.Value);
                        break;

                    case "post_impressions_unique":
                    case "post_reach":
                    case "post_activity_unique":
                        if (numericValue.HasValue) insights.Reach = Math.Max(insights.Reach ?? 0, numericValue.Value);
                        break;

                    case "post_engaged_users":
                        if (numericValue.HasValue) insights.EngagedUsers = Math.Max(insights.EngagedUsers ?? 0, numericValue.Value);
                        break;

                    case "post_total_media_view_unique":
                        if (numericValue.HasValue) insights.TotalMediaViewUnique = Math.Max(insights.TotalMediaViewUnique ?? 0, numericValue.Value);
                        break;

                    case "post_clicks_by_type":
                        var clickTotal = ExtractInsightObjectTotal(rawValue);
                        if (clickTotal.HasValue) insights.Clicks = Math.Max(insights.Clicks ?? 0, clickTotal.Value);
                        break;
                }
            }

            if (!insights.Impressions.HasValue && insights.Views.HasValue)
                insights.Impressions = insights.Views;
            if (!insights.Reach.HasValue && insights.Impressions.HasValue)
                insights.Reach = insights.Impressions;

            return insights.Impressions.HasValue
                || insights.Reach.HasValue
                || insights.EngagedUsers.HasValue
                || insights.Clicks.HasValue
                ? insights
                : null;
        }

        private static long? ExtractInsightNumber(object? rawValue)
        {
            return rawValue switch
            {
                null => null,
                long value => value,
                int value => value,
                decimal value => (long)value,
                double value => (long)value,
                float value => (long)value,
                string value when long.TryParse(value, out var parsed) => parsed,
                JsonElement { ValueKind: JsonValueKind.Number } element when element.TryGetInt64(out var parsed) => parsed,
                JsonElement { ValueKind: JsonValueKind.String } element when long.TryParse(element.GetString(), out var parsed) => parsed,
                _ => null
            };
        }

        private static long? ExtractInsightObjectTotal(object? rawValue)
        {
            if (rawValue is not JsonElement element || element.ValueKind != JsonValueKind.Object)
                return null;

            long total = 0;
            foreach (var property in element.EnumerateObject())
            {
                var propertyValue = ExtractInsightNumber(property.Value);
                if (propertyValue.HasValue)
                    total += propertyValue.Value;
            }

            return total > 0 ? total : null;
        }

        private static IEnumerable<string> BuildExternalPostIdCandidates(string externalPostId, string? pageId)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            bool YieldIfNew(string? value)
            {
                return !string.IsNullOrWhiteSpace(value) && seen.Add(value);
            }

            if (externalPostId.Contains('_'))
            {
                if (YieldIfNew(externalPostId))
                    yield return externalPostId;

                yield break;
            }

            if (!string.IsNullOrWhiteSpace(pageId))
            {
                var pagePostId = $"{pageId}_{externalPostId}";
                if (YieldIfNew(pagePostId))
                    yield return pagePostId;

                yield break;
            }

            if (YieldIfNew(externalPostId))
                yield return externalPostId;
        }

        private static string NormalizeFacebookPostId(string externalPostId, string? pageId)
        {
            if (externalPostId.Contains('_', StringComparison.Ordinal) || string.IsNullOrWhiteSpace(pageId))
                return externalPostId;

            return $"{pageId}_{externalPostId}";
        }

        private async Task<string?> ResolveAccessTokenAsync(SocialIntegration integration, CancellationToken cancellationToken)
        {
            var platformName = integration.Platform.ToString().ToLowerInvariant();

            if (platformName == "instagram")
            {
                var token = TryUnprotect(integration.AccessToken);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    System.IO.File.AppendAllText("instagram_debug.log", $"{DateTime.UtcNow:O} | TOKEN-SOURCE=integration.AccessToken | IntegrationId={integration.Id}\n");
                    return token;
                }

                System.IO.File.AppendAllText("instagram_debug.log", $"{DateTime.UtcNow:O} | TOKEN-NOT-FOUND | IntegrationId={integration.Id}\n");
                return null;
            }

            var current = TryUnprotect(integration.AccessToken);
            if (!string.IsNullOrWhiteSpace(current))
            {
                System.IO.File.AppendAllText("facebook_debug.log", $"{DateTime.UtcNow:O} | TOKEN-SOURCE=integration.AccessToken | IntegrationId={integration.Id}\n");
                return current;
            }

            var candidates = await _socialIntegrationRepository.GetByBrandIdAsync(integration.BrandId, cancellationToken);
            var workspaceCandidates = await _socialIntegrationRepository.GetByWorkspaceIdAsync(integration.WorkspaceId, cancellationToken);
            candidates = candidates
                .Concat(workspaceCandidates)
                .GroupBy(i => i.Id)
                .Select(g => g.First())
                .ToList();
            foreach (var candidate in BuildTokenCandidates(integration, candidates))
            {
                var fallback = TryUnprotect(candidate.AccessToken);
                if (!string.IsNullOrWhiteSpace(fallback))
                {
                    System.IO.File.AppendAllText("facebook_debug.log", $"{DateTime.UtcNow:O} | TOKEN-SOURCE=cross-integration.AccessToken | CandidateId={candidate.Id} | LegacyId={integration.Id}\n");
                    return fallback;
                }
            }

            System.IO.File.AppendAllText("facebook_debug.log", $"{DateTime.UtcNow:O} | TOKEN-NOT-FOUND | IntegrationId={integration.Id}\n");
            return null;
        }

        private static IEnumerable<SocialIntegration> BuildTokenCandidates(
            SocialIntegration integration,
            IReadOnlyList<SocialIntegration> candidates)
        {
            var activeSamePlatform = candidates
                .Where(i => i.Id != integration.Id
                    && i.IsActive
                    && !i.IsDeleted
                    && i.Platform == integration.Platform)
                .OrderByDescending(i => i.UpdatedAt)
                .ToList();

            foreach (var exactPage in activeSamePlatform
                .Where(i => !string.IsNullOrWhiteSpace(i.ExternalId)
                    && string.Equals(i.ExternalId, integration.ExternalId, StringComparison.OrdinalIgnoreCase)))
            {
                yield return exactPage;
            }

            foreach (var sameBrandFallback in activeSamePlatform
                .Where(i => string.IsNullOrWhiteSpace(integration.ExternalId)
                    || !string.Equals(i.ExternalId, integration.ExternalId, StringComparison.OrdinalIgnoreCase)))
            {
                yield return sameBrandFallback;
            }
        }

        private string? TryUnprotect(string? protectedToken)
        {
            return string.IsNullOrEmpty(protectedToken) ? null : _tokenProtector.TryUnprotect(protectedToken);
        }
    }
}
