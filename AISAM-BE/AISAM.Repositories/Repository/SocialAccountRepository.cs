using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class SocialAccountRepository : ISocialAccountRepository
{
    private readonly AisamContext _context;

    public SocialAccountRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await Query()
            .FirstOrDefaultAsync(account => account.Id == id && !account.IsDeleted, cancellationToken);
        return FilterDeletedIntegrations(account);
    }

    public async Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await Query()
            .FirstOrDefaultAsync(account => account.Id == id && !account.IsDeleted, cancellationToken);
        return FilterDeletedIntegrations(account);
    }

    public async Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default)
    {
        var account = await Query()
            .FirstOrDefaultAsync(account =>
                account.ProfileId == profileId &&
                account.Platform == platform &&
                account.AccountId == accountId &&
                !account.IsDeleted,
                cancellationToken);
        return FilterDeletedIntegrations(account);
    }

    public async Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var accounts = await Query()
            .Where(account => account.ProfileId == profileId && !account.IsDeleted)
            .OrderByDescending(account => account.CreatedAt)
            .ToListAsync(cancellationToken);
        return accounts.Select(FilterDeletedIntegrations).OfType<SocialAccount>().ToList();
    }

    public async Task<IReadOnlyList<SocialAccount>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var accounts = await Query().Where(a => a.WorkspaceId == workspaceId && !a.IsDeleted).OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
        return accounts.Select(FilterDeletedIntegrations).OfType<SocialAccount>().ToList();
    }

    public async Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default)
    {
        account.CreatedAt = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;
        _context.SocialAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default)
    {
        account.UpdatedAt = DateTime.UtcNow;
        _context.SocialAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<SocialAccount> Query()
    {
        return _context.SocialAccounts
            .Include(account => account.Profile)
            .Include(account => account.SocialIntegrations)
                .ThenInclude(integration => integration.Brand);
    }

    private static SocialAccount? FilterDeletedIntegrations(SocialAccount? account)
    {
        if (account == null)
        {
            return null;
        }

        account.SocialIntegrations = account.SocialIntegrations
            .Where(integration => !integration.IsDeleted)
            .ToList();

        return account;
    }
}
