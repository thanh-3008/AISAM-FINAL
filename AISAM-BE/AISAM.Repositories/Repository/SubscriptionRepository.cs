using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AisamContext _context;

    public SubscriptionRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(subscription => subscription.Profile)
            .Where(subscription =>
                subscription.ProfileId == profileId &&
                !subscription.IsDeleted &&
                subscription.IsActive)
            .OrderByDescending(subscription => subscription.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(subscription => subscription.Profile)
            .FirstOrDefaultAsync(subscription => subscription.Id == id && !subscription.IsDeleted, cancellationToken);
    }

    public async Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        subscription.CreatedAt = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
    {
        var query = _context.AiGenerations
            .Include(generation => generation.Content)
            .Where(generation =>
                !generation.IsDeleted &&
                generation.Status == AiStatusEnum.Completed &&
                generation.Content.ProfileId == profileId &&
                generation.CreatedAt >= windowStart);

        if (windowEnd.HasValue)
        {
            query = query.Where(generation => generation.CreatedAt <= windowEnd.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
    {
        var query = _context.Posts
            .Include(post => post.Content)
            .Where(post =>
                !post.IsDeleted &&
                post.Status == ContentStatusEnum.Published &&
                post.Content.ProfileId == profileId &&
                post.PublishedAt >= windowStart);

        if (windowEnd.HasValue)
        {
            query = query.Where(post => post.PublishedAt <= windowEnd.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
