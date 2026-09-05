using AISAM.Data.Model;
using AISAM.Common.Models;

namespace AISAM.Repositories.IRepositories;

public interface IAutomationRepository
{
    Task AddAsync(AutomationPlan plan, CancellationToken cancellationToken = default);
    Task<AutomationPlan?> GetByIdAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationPlan>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<AutomationPlan?> GetByIdForReadAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task<IReadOnlyList<AutomationPlan>> GetByWorkspaceForReadAsync(Guid workspaceId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<AutomationPerformanceDto?> GetPerformanceAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default)
        => Task.FromResult<AutomationPerformanceDto?>(null);
}
