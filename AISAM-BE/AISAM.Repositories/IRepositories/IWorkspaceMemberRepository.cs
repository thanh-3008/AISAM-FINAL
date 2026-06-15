using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IWorkspaceMemberRepository
{
    Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WorkspaceMember> AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default);
    Task<WorkspaceMember> TransferOwnershipAsync(Guid workspaceId, Guid currentOwnerUserId, Guid targetMemberId, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
}
