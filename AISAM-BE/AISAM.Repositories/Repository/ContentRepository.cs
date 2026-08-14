using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class ContentRepository : IContentRepository
{
    private readonly AisamContext _context;

    public ContentRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(content => content.Id == id && !content.IsDeleted, cancellationToken);
    }

    public async Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(content => content.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Content>> GetPagedByProfileIdAsync(
        Guid profileId,
        PaginationRequest request,
        Guid? brandId = null,
        AdTypeEnum? adType = null,
        bool includeDeleted = false,
        ContentStatusEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(content => content.ProfileId == profileId);

        if (brandId.HasValue)
        {
            query = query.Where(content => content.BrandId == brandId.Value);
        }

        if (!includeDeleted)
        {
            query = query.Where(content => !content.IsDeleted);
        }

        if (adType.HasValue)
        {
            query = query.Where(content => content.AdType == adType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(content => content.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchPattern = $"%{request.SearchTerm}%";
            query = query.Where(content =>
                (content.Title != null && EF.Functions.ILike(content.Title, searchPattern)) ||
                EF.Functions.ILike(content.TextContent, searchPattern));
        }

        query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(content => content.Title) : query.OrderBy(content => content.Title),
            "updatedat" => request.SortDescending ? query.OrderByDescending(content => content.UpdatedAt) : query.OrderBy(content => content.UpdatedAt),
            "createdat" => request.SortDescending ? query.OrderByDescending(content => content.CreatedAt) : query.OrderBy(content => content.CreatedAt),
            _ => query.OrderByDescending(content => content.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Content>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default)
    {
        content.CreatedAt = DateTime.UtcNow;
        content.UpdatedAt = DateTime.UtcNow;
        _context.Contents.Add(content);
        await _context.SaveChangesAsync(cancellationToken);
        return content;
    }

    public async Task<PagedResult<Content>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(c => c.WorkspaceId == workspaceId);
        if (brandId.HasValue) query = query.Where(c => c.BrandId == brandId.Value);
        if (!includeDeleted) query = query.Where(c => !c.IsDeleted);
        if (adType.HasValue) query = query.Where(c => c.AdType == adType.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var words = request.SearchTerm.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var pattern = $"%{word}%";
                query = query.Where(c => c.Title != null && EF.Functions.ILike(c.Title, pattern));
            }
        }
        query = query.OrderByDescending(c => c.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<Content> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task UpdateAsync(Content content, CancellationToken cancellationToken = default)
    {
        content.UpdatedAt = DateTime.UtcNow;
        var entry = _context.Entry(content);
        if (entry.State == EntityState.Detached)
        {
            _context.Contents.Attach(content);
            entry.State = EntityState.Modified;
        }

        foreach (var approval in content.Approvals)
        {
            var approvalEntry = _context.Entry(approval);
            // EF Core might infer 'Modified' for newly added items with a non-empty Guid.
            // If the approval is not Unchanged, we check if it was recently created.
            if (approvalEntry.State != EntityState.Unchanged)
            {
                // If the approval was created recently, we can safely assume it's a new entry that needs to be inserted.
                if (approval.CreatedAt >= DateTime.UtcNow.AddMinutes(-5))
                {
                    approvalEntry.State = EntityState.Added;
                }
                else
                {
                    // Existing approvals are immutable
                    approvalEntry.State = EntityState.Unchanged;
                }
            }
        }
        
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            var failedEntry = ex.Entries.FirstOrDefault();
            var entity = failedEntry?.Entity;
            var entityType = entity?.GetType().Name;
            var entityId = "unknown";
            var state = failedEntry?.State.ToString();
            if (entity is AISAM.Data.Model.Content c) entityId = c.Id.ToString();
            else if (entity is AISAM.Data.Model.Approval a) entityId = a.Id.ToString();
            
            throw new Exception($"Concurrency exception on {entityType} with ID {entityId}, State: {state}", ex);
        }
    }

    public async Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var tagsJsonList = await _context.Contents
            .Where(c => c.WorkspaceId == workspaceId && c.Tags != null && !c.IsDeleted)
            .Select(c => c.Tags)
            .Distinct()
            .ToListAsync(cancellationToken);

        return tagsJsonList
            .SelectMany(t => System.Text.Json.JsonSerializer.Deserialize<List<string>>(t!) ?? [])
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var tagsJsonList = await _context.Contents
            .Where(c => c.ProfileId == profileId && c.Tags != null && !c.IsDeleted)
            .Select(c => c.Tags)
            .Distinct()
            .ToListAsync(cancellationToken);

        return tagsJsonList
            .SelectMany(t => System.Text.Json.JsonSerializer.Deserialize<List<string>>(t!) ?? [])
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default)
    {
        return await _context.Contents
            .CountAsync(content =>
                content.WorkspaceId == workspaceId &&
                content.AdType == adType &&
                !content.IsDeleted,
                cancellationToken);
    }

    public async Task<PagedResult<Content>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Contents.AsNoTracking().Where(c => !c.IsDeleted);
        
        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var words = request.SearchTerm.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var pattern = $"%{word}%";
                query = query.Where(c => c.Title != null && EF.Functions.ILike(c.Title, pattern));
            }
        }

        var total = await query.CountAsync(cancellationToken);
        
        // Apply sorting
        if (!status.HasValue)
        {
            query = query.OrderByDescending(c => c.Status == ContentStatusEnum.Flagged)
                         .ThenByDescending(c => c.CreatedAt);
        }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        return new PagedResult<Content> { Data = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var content = await _context.Contents.FindAsync(new object[] { id }, cancellationToken);
        if (content != null)
        {
            content.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contents.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Contents
            .Where(c => c.CreatedAt >= from && c.CreatedAt <= to && !c.IsDeleted)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, cancellationToken);
    }

    private IQueryable<Content> Query()
    {
        return _context.Contents
            .Include(content => content.Brand)
            .Include(content => content.Product)
            .Include(content => content.Approvals);
    }
}
