using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class CampaignInsightSnapshotRepository : ICampaignInsightSnapshotRepository
{
    private readonly AisamContext _context;

    public CampaignInsightSnapshotRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task UpsertRangeAsync(
        IReadOnlyCollection<CampaignInsightSnapshot> snapshots,
        CancellationToken cancellationToken = default)
    {
        if (snapshots.Count == 0)
            return;

        foreach (var snapshot in snapshots)
        {
            snapshot.SnapshotDate = snapshot.SnapshotDate.Date;
            snapshot.AttributionWindow = string.IsNullOrWhiteSpace(snapshot.AttributionWindow)
                ? "default"
                : snapshot.AttributionWindow;
            var existing = await _context.CampaignInsightSnapshots.SingleOrDefaultAsync(
                item => item.CampaignId == snapshot.CampaignId
                    && item.Platform == snapshot.Platform
                    && item.SnapshotDate == snapshot.SnapshotDate
                    && item.AttributionWindow == snapshot.AttributionWindow,
                cancellationToken);

            if (existing == null)
            {
                await _context.CampaignInsightSnapshots.AddAsync(snapshot, cancellationToken);
                continue;
            }

            existing.WorkspaceId = snapshot.WorkspaceId;
            existing.Currency = snapshot.Currency;
            existing.Impressions = snapshot.Impressions;
            existing.Reach = snapshot.Reach;
            existing.Clicks = snapshot.Clicks;
            existing.Engagement = snapshot.Engagement;
            existing.Spend = snapshot.Spend;
            existing.Conversions = snapshot.Conversions;
            existing.AttributedRevenue = snapshot.AttributedRevenue;
            existing.Source = snapshot.Source;
            existing.IsPartial = snapshot.IsPartial;
            existing.SyncedAt = snapshot.SyncedAt;
            existing.RawData = snapshot.RawData;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignInsightSnapshot>> GetRangeAsync(
        Guid workspaceId,
        DateTime from,
        DateTime to,
        Guid? brandId = null,
        string? platform = null,
        Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CampaignInsightSnapshots
            .AsNoTracking()
            .Where(item => item.WorkspaceId == workspaceId
                && item.SnapshotDate >= from.Date
                && item.SnapshotDate <= to.Date);

        if (brandId.HasValue)
            query = query.Where(item => item.Campaign.BrandId == brandId.Value);
        if (!string.IsNullOrWhiteSpace(platform))
            query = query.Where(item => item.Platform == platform.ToLower());
        if (campaignId.HasValue)
            query = query.Where(item => item.CampaignId == campaignId.Value);

        return await query.OrderBy(item => item.SnapshotDate).ToListAsync(cancellationToken);
    }

    public Task<DateTime?> GetLastSyncedAtAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return _context.CampaignInsightSnapshots
            .Where(item => item.WorkspaceId == workspaceId)
            .MaxAsync(item => (DateTime?)item.SyncedAt, cancellationToken);
    }
}
