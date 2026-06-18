using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IWorkspaceInvitationRepository
{
    Task<WorkspaceInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<WorkspaceInvitation?> GetByWorkspaceAndIdAsync(Guid workspaceId, Guid id, CancellationToken cancellationToken = default);
    Task<WorkspaceInvitation?> GetPendingByWorkspaceAndEmailAsync(Guid workspaceId, string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkspaceInvitation>> GetPendingByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<int> CountPendingByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceInvitation> AddAsync(WorkspaceInvitation invitation, CancellationToken cancellationToken = default);
    Task<WorkspaceMember> AcceptAsync(WorkspaceInvitation invitation, Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkspaceInvitation invitation, CancellationToken cancellationToken = default);
}
