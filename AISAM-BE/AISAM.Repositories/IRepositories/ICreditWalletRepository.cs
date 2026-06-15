using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface ICreditWalletRepository
{
    Task<CreditWallet?> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<CreditWallet> AddAsync(CreditWallet wallet, CancellationToken cancellationToken = default);
    Task UpdateAsync(CreditWallet wallet, CancellationToken cancellationToken = default);
}
