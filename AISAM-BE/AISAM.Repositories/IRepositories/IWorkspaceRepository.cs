using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workspace>> GetLifecycleCandidatesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
