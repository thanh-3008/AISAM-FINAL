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
        var today = DateTime.UtcNow.Date;

        return await _context.Subscriptions
            .Include(subscription => subscription.Profile)
            .Where(subscription =>
                subscription.ProfileId == profileId &&
                !subscription.IsDeleted &&
                subscription.IsActive &&
                subscription.StartDate <= today &&
                (!subscription.EndDate.HasValue || subscription.EndDate.Value >= today))
            .OrderByDescending(subscription =>
                subscription.Plan == SubscriptionPlanEnum.Premium ? 3 :
                subscription.Plan == SubscriptionPlanEnum.Plus ? 2 :
                subscription.Plan == SubscriptionPlanEnum.PlusTrial ? 1 :
                0)
            .ThenByDescending(subscription => subscription.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetCurrentActiveByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Subscriptions
            .Include(subscription => subscription.Workspace)
            .Where(subscription =>
                subscription.WorkspaceId == workspaceId &&
                !subscription.IsDeleted &&
                subscription.IsActive &&
                subscription.StartDate <= today &&
                (!subscription.EndDate.HasValue || subscription.EndDate.Value >= today))
            .OrderByDescending(subscription =>
                subscription.Plan == SubscriptionPlanEnum.Premium ? 3 :
                subscription.Plan == SubscriptionPlanEnum.Plus ? 2 :
                subscription.Plan == SubscriptionPlanEnum.PlusTrial ? 1 :
                0)
            .ThenByDescending(subscription => subscription.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(subscription => subscription.Profile)
            .Include(subscription => subscription.Workspace)
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
        var utcWindowStart = NormalizeUtc(windowStart);
        var utcWindowEndExclusive = NormalizeEndExclusiveUtc(windowEnd);

        var query = _context.AiGenerations
            .Include(generation => generation.Content)
            .Where(generation =>
                !generation.IsDeleted &&
                generation.Status == AiStatusEnum.Completed &&
                generation.Content.ProfileId == profileId &&
                generation.CreatedAt >= utcWindowStart);

        if (utcWindowEndExclusive.HasValue)
        {
            query = query.Where(generation => generation.CreatedAt < utcWindowEndExclusive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
    {
        var utcWindowStart = NormalizeUtc(windowStart);
        var utcWindowEndExclusive = NormalizeEndExclusiveUtc(windowEnd);

        var query = _context.Posts
            .Include(post => post.Content)
            .Where(post =>
                !post.IsDeleted &&
                post.Status == ContentStatusEnum.Published &&
                post.Content.ProfileId == profileId &&
                post.PublishedAt >= utcWindowStart);

        if (utcWindowEndExclusive.HasValue)
        {
            query = query.Where(post => post.PublishedAt < utcWindowEndExclusive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountSuccessfulPromptUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
    {
        var utcWindowStart = NormalizeUtc(windowStart);
        var utcWindowEndExclusive = NormalizeEndExclusiveUtc(windowEnd);

        var query = _context.AiGenerations
            .Include(generation => generation.Content)
            .Where(generation =>
                !generation.IsDeleted &&
                generation.Status == AiStatusEnum.Completed &&
                generation.Content.WorkspaceId == workspaceId &&
                generation.CreatedAt >= utcWindowStart);

        if (utcWindowEndExclusive.HasValue)
        {
            query = query.Where(generation => generation.CreatedAt < utcWindowEndExclusive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountSuccessfulPostUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default)
    {
        var utcWindowStart = NormalizeUtc(windowStart);
        var utcWindowEndExclusive = NormalizeEndExclusiveUtc(windowEnd);

        var query = _context.Posts
            .Include(post => post.Content)
            .Where(post =>
                !post.IsDeleted &&
                post.Status == ContentStatusEnum.Published &&
                post.Content.WorkspaceId == workspaceId &&
                post.PublishedAt >= utcWindowStart);

        if (utcWindowEndExclusive.HasValue)
        {
            query = query.Where(post => post.PublishedAt < utcWindowEndExclusive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static DateTime? NormalizeEndExclusiveUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var end = NormalizeUtc(value.Value);
        return end.TimeOfDay == TimeSpan.Zero ? end.AddDays(1) : end;
    }
}
