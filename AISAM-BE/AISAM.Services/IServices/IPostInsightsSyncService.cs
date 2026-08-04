namespace AISAM.Services.IServices
{
    public interface IPostInsightsSyncService
    {
        Task<AISAM.Common.Models.PostInsightsSyncResultDto> ProcessNextDetailedAsync(CancellationToken cancellationToken = default);
        Task<AISAM.Common.Models.PostInsightsSyncResultDto> ProcessWorkspaceAsync(
            Guid workspaceId,
            DateTime from,
            DateTime to,
            Guid? brandId = null,
            string? platform = null,
            CancellationToken cancellationToken = default);
        Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default);
    }
}
