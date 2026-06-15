using AISAM.Data.Enumeration;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ISocialAccountRepository
{
    Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccount>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default);
    Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default);
}
