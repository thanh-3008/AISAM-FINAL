using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class CampaignInsightSyncService : ICampaignInsightSyncService
{
    private readonly IAdCampaignRepository _campaignRepository;
    private readonly ICampaignInsightSnapshotRepository _snapshotRepository;
    private readonly IWorkspaceMemberRepository _memberRepository;
    private readonly ISocialService _socialService;
    private readonly FacebookProvider _facebookProvider;
    private readonly ILogger<CampaignInsightSyncService> _logger;

    public CampaignInsightSyncService(
        IAdCampaignRepository campaignRepository,
        ICampaignInsightSnapshotRepository snapshotRepository,
        IWorkspaceMemberRepository memberRepository,
        ISocialService socialService,
        FacebookProvider facebookProvider,
        ILogger<CampaignInsightSyncService> logger)
    {
        _campaignRepository = campaignRepository;
        _snapshotRepository = snapshotRepository;
        _memberRepository = memberRepository;
        _socialService = socialService;
        _facebookProvider = facebookProvider;
        _logger = logger;
    }

    public async Task<GenericResponse<AnalyticsSyncResultDto>> SyncAsync(
        Guid workspaceId,
        Guid userId,
        AnalyticsSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
        if (member == null || !member.IsActive ||
            member.Role is not (WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager))
            return GenericResponse<AnalyticsSyncResultDto>.CreateError(
                "You do not have permission to sync campaign analytics.");

        var from = request.From.Date;
        var to = request.To.Date;
        if (to < from)
            return GenericResponse<AnalyticsSyncResultDto>.CreateError(
                "The analytics end date must be on or after the start date.");
        if ((to - from).TotalDays > 90)
            return GenericResponse<AnalyticsSyncResultDto>.CreateError(
                "A single analytics sync cannot exceed 90 days.");

        var page = await _campaignRepository.GetPagedByWorkspaceIdAsync(
            workspaceId,
            new PaginationRequest { Page = 1, PageSize = 100 },
            cancellationToken: cancellationToken);
        var campaigns = page.Data
            .Where(campaign => !string.IsNullOrWhiteSpace(campaign.FacebookCampaignId))
            .Where(campaign => !request.CampaignId.HasValue || campaign.Id == request.CampaignId.Value)
            .ToList();

        var warnings = new List<string>();
        var succeeded = 0;
        var upserted = 0;
        DateTime? lastSyncedAt = null;

        foreach (var campaign in campaigns)
        {
            try
            {
                var accounts = await _socialService.GetProfileAccountsAsync(campaign.ProfileId, cancellationToken);
                var account = accounts.FirstOrDefault(item =>
                    item.IsActive && item.Provider.Equals("facebook", StringComparison.OrdinalIgnoreCase));
                if (account == null)
                    throw new InvalidOperationException("No active Facebook account is connected.");

                var token = await _socialService.GetFacebookUserAccessTokenAsync(
                    campaign.ProfileId, cancellationToken);
                if (string.IsNullOrWhiteSpace(token))
                    token = account.AccessToken;
                if (string.IsNullOrWhiteSpace(token))
                    throw new InvalidOperationException("The Facebook access token is unavailable.");

                var daily = await _facebookProvider.GetCampaignDailyInsightsAsync(
                    campaign.AdAccountId,
                    token,
                    campaign.FacebookCampaignId!,
                    from,
                    to,
                    cancellationToken);
                var syncedAt = DateTime.UtcNow;
                var snapshots = daily.Select(item => new CampaignInsightSnapshot
                {
                    WorkspaceId = workspaceId,
                    CampaignId = campaign.Id,
                    Platform = campaign.Platform.ToLowerInvariant(),
                    SnapshotDate = item.Date.Date,
                    Currency = item.Currency,
                    Impressions = item.Impressions,
                    Reach = item.Reach,
                    Clicks = item.Clicks,
                    Spend = item.Spend,
                    Conversions = item.Conversions,
                    AttributedRevenue = item.AttributedRevenue,
                    AttributionWindow = string.IsNullOrWhiteSpace(item.AttributionWindow)
                        ? "default"
                        : item.AttributionWindow,
                    Source = "meta",
                    IsPartial = item.IsPartial,
                    SyncedAt = syncedAt,
                    RawData = item.RawData
                }).ToList();

                await _snapshotRepository.UpsertRangeAsync(snapshots, cancellationToken);
                await _campaignRepository.UpdateCampaignInsightsAsync(
                    campaign.Id,
                    snapshots.Sum(item => item.Impressions),
                    snapshots.Sum(item => item.Clicks),
                    snapshots.Sum(item => item.Spend),
                    (long)snapshots.Sum(item => item.Conversions ?? 0),
                    cancellationToken);

                succeeded++;
                upserted += snapshots.Count;
                lastSyncedAt = syncedAt;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Campaign analytics sync failed for {CampaignId}", campaign.Id);
                warnings.Add($"{campaign.Name}: {ex.Message}");
            }
        }

        var status = campaigns.Count == 0
            ? "no_data"
            : succeeded == campaigns.Count
                ? "succeeded"
                : succeeded > 0 ? "partial" : "failed";
        var result = new AnalyticsSyncResultDto
        {
            Status = status,
            CampaignsRequested = campaigns.Count,
            CampaignsSucceeded = succeeded,
            SnapshotsUpserted = upserted,
            LastSyncedAt = lastSyncedAt,
            Warnings = warnings
        };
        return GenericResponse<AnalyticsSyncResultDto>.CreateSuccess(result, "Analytics sync completed.");
    }
}
