using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class AdCampaignRepository : IAdCampaignRepository
{
    private readonly AisamContext _context;

    public AdCampaignRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdCampaign>> GetPagedByWorkspaceIdAsync(
        Guid workspaceId,
        PaginationRequest request,
        Guid? brandId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var query = Query().Where(c => c.WorkspaceId == workspaceId && !c.IsDeleted);

        if (brandId.HasValue) query = query.Where(c => c.BrandId == brandId.Value);
        if (isActive.HasValue) query = query.Where(c => c.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var pattern = $"%{request.SearchTerm}%";
            query = query.Where(c => EF.Functions.ILike(c.Name, pattern));
        }

        query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "updatedat" => request.SortDescending ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt),
            "createdat" => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<AdCampaign> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Query().FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

    public async Task<AdCampaign> AddAsync(AdCampaign campaign, CancellationToken cancellationToken = default)
    {
        campaign.CreatedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;
        _context.AdCampaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);
        return campaign;
    }

    public async Task UpdateAsync(AdCampaign campaign, CancellationToken cancellationToken = default)
    {
        campaign.UpdatedAt = DateTime.UtcNow;
        _context.AdCampaigns.Update(campaign);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<AdCampaign> Query()
        => _context.AdCampaigns.Include(c => c.Brand).Include(c => c.AdSets);
}
