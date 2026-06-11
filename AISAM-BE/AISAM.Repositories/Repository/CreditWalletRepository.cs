using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class CreditWalletRepository : ICreditWalletRepository
{
    private readonly AisamContext _context;

    public CreditWalletRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<CreditWallet?> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.CreditWallets
            .FirstOrDefaultAsync(wallet => wallet.WorkspaceId == workspaceId, cancellationToken);
    }

    public async Task<CreditWallet> AddAsync(CreditWallet wallet, CancellationToken cancellationToken = default)
    {
        wallet.CreatedAt = DateTime.UtcNow;
        wallet.UpdatedAt = wallet.CreatedAt;
        _context.CreditWallets.Add(wallet);
        await _context.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task UpdateAsync(CreditWallet wallet, CancellationToken cancellationToken = default)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _context.CreditWallets.Update(wallet);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
