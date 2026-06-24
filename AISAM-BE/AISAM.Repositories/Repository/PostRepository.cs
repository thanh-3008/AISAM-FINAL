using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class PostRepository : IPostRepository
{
    private readonly AisamContext _context;

    public PostRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(post => post.Id == id && !post.IsDeleted, cancellationToken);
    }

    public async Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        post.CreatedAt = DateTime.UtcNow;
        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);
        return post;
    }

    public async Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(post => !post.IsDeleted && post.Content.ProfileId == profileId);

        if (brandId.HasValue)
        {
            query = query.Where(post => post.Content.BrandId == brandId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(post => post.Status == status.Value);
        }

        query = query.OrderByDescending(post => post.PublishedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Post>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Post>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(p => !p.IsDeleted && p.Content.WorkspaceId == workspaceId);
        if (brandId.HasValue) query = query.Where(p => p.Content.BrandId == brandId.Value);
        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        query = query.OrderByDescending(p => p.PublishedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<Post> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    private IQueryable<Post> Query()
    {
        return _context.Posts
            .Include(post => post.Content)
                .ThenInclude(content => content.Brand)
            .Include(post => post.Integration);
    }
}
