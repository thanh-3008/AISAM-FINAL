using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ICampaignInsightSnapshotRepository
{
    Task UpsertRangeAsync(IReadOnlyCollection<CampaignInsightSnapshot> snapshots, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CampaignInsightSnapshot>> GetRangeAsync(
        Guid workspaceId,
        DateTime from,
        DateTime to,
        Guid? brandId = null,
        string? platform = null,
        Guid? campaignId = null,
        CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastSyncedAtAsync(Guid workspaceId, CancellationToken cancellationToken = default);
}
