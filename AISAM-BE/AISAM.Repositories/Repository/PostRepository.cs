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
        var post = await QueryWithAttribution()
            .FirstOrDefaultAsync(post => post.Id == id && !post.IsDeleted, cancellationToken);
        await PopulateTeamNamesAsync(post is null ? [] : [post], cancellationToken);
        return post;
    }

    public async Task<Post?> GetByIntegrationAndExternalPostIdAsync(Guid integrationId, string externalPostId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(post =>
                !post.IsDeleted &&
                post.IntegrationId == integrationId &&
                post.ExternalPostId == externalPostId,
                cancellationToken);
    }

    public async Task<Post?> GetByExternalPostIdInWorkspaceAsync(Guid workspaceId, string externalPostId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .FirstOrDefaultAsync(post =>
                !post.IsDeleted &&
                post.ExternalPostId == externalPostId &&
                post.Content != null && post.Content.WorkspaceId == workspaceId,
                cancellationToken);
    }

    public async Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
    {
        post.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
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
        var query = QueryWithAttribution().AsNoTracking()
            .Where(post => !post.IsDeleted && post.Content.ProfileId == profileId);

        if (brandId.HasValue)
        {
            query = query.Where(post => post.Content.BrandId == brandId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(post => post.Status == status.Value);
        }

        query = query.OrderByDescending(post => post.PublishedAt).ThenByDescending(post => post.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        await PopulateTeamNamesAsync(data, cancellationToken);

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
        var query = QueryWithAttribution().AsNoTracking()
            .Where(p => !p.IsDeleted && p.Content.WorkspaceId == workspaceId);
        if (brandId.HasValue) query = query.Where(p => p.Content.BrandId == brandId.Value);
        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        query = query.OrderByDescending(p => p.PublishedAt).ThenByDescending(p => p.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        await PopulateTeamNamesAsync(data, cancellationToken);
        return new PagedResult<Post> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<List<Post>> GetPublishedByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(p => p.ContentId == contentId && !p.IsDeleted && p.ExternalPostId != null)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Post> Query()
    {
        return _context.Posts
            .Include(post => post.Content)
                .ThenInclude(content => content.Brand)
            .Include(post => post.Integration);
    }

    private IQueryable<Post> QueryWithAttribution()
    {
        return Query()
            .Include(post => post.Content)
                .ThenInclude(content => content.PrimaryCreator)
            .Include(post => post.Content)
                .ThenInclude(content => content.Approvals.Where(approval => !approval.IsDeleted))
                    .ThenInclude(approval => approval.ApproverUser);
    }

    private async Task PopulateTeamNamesAsync(IEnumerable<Post> posts, CancellationToken cancellationToken)
    {
        var rows = posts.Where(post => post.Content.TeamId.HasValue).ToList();
        var teamIds = rows.Select(post => post.Content.TeamId!.Value).Distinct().ToArray();
        if (teamIds.Length == 0) return;
        var names = await _context.Teams.IgnoreQueryFilters().AsNoTracking()
            .Where(team => teamIds.Contains(team.Id))
            .ToDictionaryAsync(team => team.Id, team => team.Name, cancellationToken);
        foreach (var post in rows)
            post.Content.TeamName = names.GetValueOrDefault(post.Content.TeamId!.Value);
    }
}
