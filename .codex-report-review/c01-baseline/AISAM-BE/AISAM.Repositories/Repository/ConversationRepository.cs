using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly AisamContext _context;

    public ConversationRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(conversation => conversation.Id == id && !conversation.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Conversation>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(conversation => conversation.ProfileId == profileId && !conversation.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchPattern = $"%{request.SearchTerm}%";
            query = query.Where(conversation =>
                conversation.Title != null && EF.Functions.ILike(conversation.Title, searchPattern));
        }

        query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(conversation => conversation.Title) : query.OrderBy(conversation => conversation.Title),
            "createdat" => request.SortDescending ? query.OrderByDescending(conversation => conversation.CreatedAt) : query.OrderBy(conversation => conversation.CreatedAt),
            _ => query.OrderByDescending(conversation => conversation.UpdatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Conversation>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Conversation>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(conversation => conversation.WorkspaceId == workspaceId && !conversation.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchPattern = $"%{request.SearchTerm}%";
            query = query.Where(conversation => conversation.Title != null && EF.Functions.ILike(conversation.Title, searchPattern));
        }
        query = query.OrderByDescending(conversation => conversation.UpdatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<Conversation> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<Conversation?> GetActiveAsync(Guid profileId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(conversation =>
            conversation.ProfileId == profileId &&
            conversation.BrandId == brandId &&
            conversation.ProductId == productId &&
            conversation.AdType == adType &&
            conversation.IsActive &&
            !conversation.IsDeleted,
            cancellationToken);
    }

    public Task<Conversation?> GetActiveByWorkspaceIdAsync(Guid workspaceId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default)
        => Query().FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId && c.BrandId == brandId && c.ProductId == productId && c.AdType == adType && c.IsActive && !c.IsDeleted, cancellationToken);

    public async Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        conversation.CreatedAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        conversation.UpdatedAt = DateTime.UtcNow;
        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        message.CreatedAt = DateTime.UtcNow;
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, AiGeneration>> GetGenerationsByVideoJobIdsAsync(
        Guid workspaceId,
        IEnumerable<string> videoJobIds,
        CancellationToken cancellationToken = default)
    {
        var ids = videoJobIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (ids.Count == 0) return new Dictionary<string, AiGeneration>();

        var generations = await _context.AiGenerations
            .Where(generation => !generation.IsDeleted &&
                                 generation.Content.WorkspaceId == workspaceId &&
                                 generation.VideoJobId != null &&
                                 ids.Contains(generation.VideoJobId))
            .OrderByDescending(generation => generation.CreatedAt)
            .ToListAsync(cancellationToken);

        return generations
            .GroupBy(generation => generation.VideoJobId!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    private IQueryable<Conversation> Query()
    {
        return _context.Conversations
            .Include(conversation => conversation.ChatMessages.OrderBy(message => message.CreatedAt))
                .ThenInclude(message => message.AiGeneration)
            .Include(conversation => conversation.Brand)
            .Include(conversation => conversation.Product);
    }
}
