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

        public async Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AdCampaigns
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.AdSets.Where(ads => !ads.IsDeleted))
                .FirstOrDefaultAsync(ac => ac.Id == id && !ac.IsDeleted, cancellationToken);
        }

        public async Task<AdCampaign?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AdCampaigns
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.AdSets)
                .FirstOrDefaultAsync(ac => ac.Id == id, cancellationToken);
        }

        public async Task<PagedResult<AdCampaign>> GetPagedByWorkspaceIdAsync(
            Guid workspaceId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var query = _context.AdCampaigns
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.AdSets.Where(ads => !ads.IsDeleted))
                .Where(ac => ac.WorkspaceId == workspaceId);

            if (!includeDeleted)
            {
                query = query.Where(ac => !ac.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchPattern = $"%{request.SearchTerm}%";
                query = query.Where(ac =>
                    EF.Functions.ILike(ac.Name, searchPattern) ||
                    (ac.Objective != null && EF.Functions.ILike(ac.Objective, searchPattern)));
            }

            query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(ac => ac.Name) : query.OrderBy(ac => ac.Name),
                "budget" => request.SortDescending ? query.OrderByDescending(ac => ac.Budget) : query.OrderBy(ac => ac.Budget),
                "startdate" => request.SortDescending ? query.OrderByDescending(ac => ac.StartDate) : query.OrderBy(ac => ac.StartDate),
                _ => query.OrderByDescending(ac => ac.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new PagedResult<AdCampaign>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
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
    }
}
