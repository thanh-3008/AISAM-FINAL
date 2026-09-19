using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IDeviceTokenRepository
{
    Task<DeviceToken> RegisterOrUpdateAsync(Guid profileId, string token, string platform, string? deviceName, CancellationToken cancellationToken = default);
    Task<bool> UnregisterAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceToken>> GetActiveTokensByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceToken>> GetActiveTokensByProfileIdsAsync(IEnumerable<Guid> profileIds, CancellationToken cancellationToken = default);
    Task DeactivateTokensAsync(IEnumerable<string> invalidTokens, CancellationToken cancellationToken = default);
}
