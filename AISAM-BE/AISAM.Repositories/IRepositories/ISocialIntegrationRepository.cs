using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ISocialIntegrationRepository
{
    Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegration>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default);
    Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default);
}
