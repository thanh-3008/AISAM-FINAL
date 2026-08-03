namespace AISAM.Services.IServices
{
    public interface ICampaignInsightsSyncService
    {
        Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default);
    }
}
