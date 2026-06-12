namespace AISAM.Services.IServices;

public interface IWorkspaceLifecycleProcessingService
{
    Task<int> RunBatchAsync(int batchSize, CancellationToken cancellationToken = default);
}
