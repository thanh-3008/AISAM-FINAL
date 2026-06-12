using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service;

public sealed class WorkspaceLifecycleProcessingService : IWorkspaceLifecycleProcessingService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceLifecycleService _workspaceLifecycleService;

    public WorkspaceLifecycleProcessingService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceLifecycleService workspaceLifecycleService)
    {
        _workspaceRepository = workspaceRepository;
        _workspaceLifecycleService = workspaceLifecycleService;
    }

    public async Task<int> RunBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var workspaces = await _workspaceRepository.GetLifecycleCandidatesAsync(batchSize, cancellationToken);
        var updatedCount = 0;

        foreach (var workspace in workspaces)
        {
            if (!_workspaceLifecycleService.TrySynchronizePersistenceState(workspace))
            {
                continue;
            }

            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
            updatedCount++;
        }

        return updatedCount;
    }
}
