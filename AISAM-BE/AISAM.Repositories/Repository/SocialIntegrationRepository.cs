using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class SocialIntegrationRepository : ISocialIntegrationRepository
{
    private readonly AisamContext _context;

    public SocialIntegrationRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(integration => integration.Id == id && !integration.IsDeleted, cancellationToken);
    }

    public async Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(integration =>
                integration.SocialAccountId == socialAccountId &&
                integration.ExternalId == externalId &&
                !integration.IsDeleted,
                cancellationToken);
    }

    public async Task<SocialIntegration?> GetByWorkspacePlatformExternalIdAsync(
        Guid workspaceId,
        SocialPlatformEnum platform,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(integration =>
                integration.WorkspaceId == workspaceId &&
                integration.Platform == platform &&
                integration.ExternalId == externalId &&
                !integration.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(integration => integration.SocialAccountId == socialAccountId && !integration.IsDeleted)
            .OrderByDescending(integration => integration.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(integration => integration.BrandId == brandId && !integration.IsDeleted)
            .OrderByDescending(integration => integration.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SocialIntegration>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(integration => integration.WorkspaceId == workspaceId && !integration.IsDeleted)
            .OrderByDescending(integration => integration.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default)
    {
        integration.CreatedAt = DateTime.UtcNow;
        integration.UpdatedAt = DateTime.UtcNow;
        _context.SocialIntegrations.Add(integration);
        await _context.SaveChangesAsync(cancellationToken);
        return integration;
    }

    public async Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default)
    {
        integration.UpdatedAt = DateTime.UtcNow;
        _context.SocialIntegrations.Update(integration);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<SocialIntegration> Query()
    {
        return _context.SocialIntegrations
            .Include(integration => integration.SocialAccount)
            .Include(integration => integration.Brand)
            .Include(integration => integration.Profile)
            .Include(integration => integration.Posts.Where(post => !post.IsDeleted));
    }
}
