using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository
{
    public class AdCampaignRepository : IAdCampaignRepository
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

            var query = Query()
                .Where(campaign => campaign.WorkspaceId == workspaceId && !campaign.IsDeleted);

            if (brandId.HasValue)
            {
                query = query.Where(campaign => campaign.BrandId == brandId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(campaign => campaign.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var pattern = $"%{request.SearchTerm}%";
                query = query.Where(campaign =>
                    EF.Functions.ILike(campaign.Name, pattern) ||
                    (campaign.Objective != null && EF.Functions.ILike(campaign.Objective, pattern)));
            }

            query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(campaign => campaign.Name) : query.OrderBy(campaign => campaign.Name),
                "updatedat" => request.SortDescending ? query.OrderByDescending(campaign => campaign.UpdatedAt) : query.OrderBy(campaign => campaign.UpdatedAt),
                "createdat" => request.SortDescending ? query.OrderByDescending(campaign => campaign.CreatedAt) : query.OrderBy(campaign => campaign.CreatedAt),
                _ => query.OrderByDescending(campaign => campaign.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<AdCampaign>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Query()
                .FirstOrDefaultAsync(campaign => campaign.Id == id && !campaign.IsDeleted, cancellationToken);
        }

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
        {
            return _context.AdCampaigns
                .Include(campaign => campaign.Brand)
                .Include(campaign => campaign.Workspace)
                .Include(campaign => campaign.AdSets);
        }
    }
}
