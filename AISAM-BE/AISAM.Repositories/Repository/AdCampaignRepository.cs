using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
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
                .Include(ac => ac.Product)
                .Include(ac => ac.Content)
                .Include(ac => ac.AdSets.Where(ads => !ads.IsDeleted))
                    .ThenInclude(ads => ads.Ads.Where(a => !a.IsDeleted))
                        .ThenInclude(a => a.Creative)
                .FirstOrDefaultAsync(ac => ac.Id == id && !ac.IsDeleted, cancellationToken);
        }

        public async Task<AdCampaign?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AdCampaigns
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.Product)
                .Include(ac => ac.Content)
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
                .AsNoTracking()
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.Product)
                .Include(ac => ac.Content)
                .Include(ac => ac.AdSets.Where(ads => !ads.IsDeleted))
                    .ThenInclude(ads => ads.Ads.Where(a => !a.IsDeleted))
                        .ThenInclude(a => a.Creative)
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
            try
            {
                var entry = _context.Entry(campaign);
                if (entry.State == EntityState.Detached)
                {
                    _context.AdCampaigns.Update(campaign);
                }
            }
            catch
            {
                _context.AdCampaigns.Update(campaign);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SetFacebookCampaignIdAsync(Guid campaignId, string facebookCampaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(ac => ac.Id == campaignId, cancellationToken);
            if (campaign != null)
            {
                campaign.FacebookCampaignId = facebookCampaignId;
                campaign.UpdatedAt = DateTime.UtcNow;
                _context.Entry(campaign).Property(e => e.FacebookCampaignId).IsModified = true;
                _context.Entry(campaign).Property(e => e.UpdatedAt).IsModified = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task AddAdSetAsync(AdSet adSet, CancellationToken cancellationToken = default)
        {
            _context.AdSets.Add(adSet);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddAdCreativeAsync(AdCreative creative, CancellationToken cancellationToken = default)
        {
            _context.AdCreatives.Add(creative);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddAdAsync(Ad ad, CancellationToken cancellationToken = default)
        {
            _context.Ads.Add(ad);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ─── Deployment step tracking ───

        public async Task UpdateDeploymentStatusAsync(Guid campaignId, DeploymentStatusEnum status, int step, CancellationToken cancellationToken = default)
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(ac => ac.Id == campaignId, cancellationToken);
            if (campaign != null)
            {
                campaign.DeploymentStatus = status;
                campaign.DeploymentStep = step;
                campaign.UpdatedAt = DateTime.UtcNow;
                _context.Entry(campaign).Property(e => e.DeploymentStatus).IsModified = true;
                _context.Entry(campaign).Property(e => e.DeploymentStep).IsModified = true;
                _context.Entry(campaign).Property(e => e.UpdatedAt).IsModified = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateDeploymentFailureAsync(Guid campaignId, int step, string message, CancellationToken cancellationToken = default)
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(ac => ac.Id == campaignId, cancellationToken);
            if (campaign != null)
            {
                campaign.DeploymentStatus = DeploymentStatusEnum.Failed;
                campaign.DeploymentStep = step;
                campaign.DeploymentMessage = message;
                campaign.Status = CampaignStatusEnum.Rejected;
                campaign.IsActive = false;
                campaign.UpdatedAt = DateTime.UtcNow;
                _context.Entry(campaign).Property(e => e.DeploymentStatus).IsModified = true;
                _context.Entry(campaign).Property(e => e.DeploymentStep).IsModified = true;
                _context.Entry(campaign).Property(e => e.DeploymentMessage).IsModified = true;
                _context.Entry(campaign).Property(e => e.Status).IsModified = true;
                _context.Entry(campaign).Property(e => e.IsActive).IsModified = true;
                _context.Entry(campaign).Property(e => e.UpdatedAt).IsModified = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateCampaignStatusAsync(Guid campaignId, CampaignStatusEnum status, CancellationToken cancellationToken = default)
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(ac => ac.Id == campaignId, cancellationToken);
            if (campaign != null)
            {
                campaign.Status = status;
                campaign.UpdatedAt = DateTime.UtcNow;
                _context.Entry(campaign).Property(e => e.Status).IsModified = true;
                _context.Entry(campaign).Property(e => e.UpdatedAt).IsModified = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateCampaignInsightsAsync(Guid campaignId, long impressions, long clicks, decimal spend, long conversions, CancellationToken cancellationToken = default)
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(ac => ac.Id == campaignId, cancellationToken);
            if (campaign != null)
            {
                campaign.Impressions = impressions;
                campaign.Clicks = clicks;
                campaign.Spend = spend;
                campaign.Conversions = conversions;
                campaign.UpdatedAt = DateTime.UtcNow;
                _context.Entry(campaign).Property(e => e.Impressions).IsModified = true;
                _context.Entry(campaign).Property(e => e.Clicks).IsModified = true;
                _context.Entry(campaign).Property(e => e.Spend).IsModified = true;
                _context.Entry(campaign).Property(e => e.Conversions).IsModified = true;
                _context.Entry(campaign).Property(e => e.UpdatedAt).IsModified = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<AdSet?> GetAdSetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.AdSets
                .Where(ads => ads.CampaignId == campaignId && !ads.IsDeleted)
                .OrderByDescending(ads => ads.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Ad?> GetAdByAdSetIdAsync(Guid adSetId, CancellationToken cancellationToken = default)
        {
            return await _context.Ads
                .Where(a => a.AdSetId == adSetId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AdCreative?> GetCreativeByIdAsync(Guid creativeId, CancellationToken cancellationToken = default)
        {
            return await _context.AdCreatives
                .FirstOrDefaultAsync(ac => ac.Id == creativeId && !ac.IsDeleted, cancellationToken);
        }

        public async Task<IReadOnlyList<AdSet>> GetAdSetsByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.AdSets
                .AsNoTracking()
                .Where(ads => ads.CampaignId == campaignId && !ads.IsDeleted)
                .OrderByDescending(ads => ads.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Ad>> GetAdsByAdSetIdAsync(Guid adSetId, CancellationToken cancellationToken = default)
        {
            return await _context.Ads
                .AsNoTracking()
                .Where(a => a.AdSetId == adSetId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Ad>> GetAdsByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.Ads
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.AdSet != null && a.AdSet.CampaignId == campaignId && !a.AdSet.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // ─── Cleanup ───

        public async Task HardDeleteAdAsync(Guid adId, CancellationToken cancellationToken = default)
        {
            var ad = await _context.Ads.FindAsync(new object[] { adId }, cancellationToken);
            if (ad != null)
            {
                _context.Ads.Remove(ad);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task HardDeleteAdCreativeAsync(Guid creativeId, CancellationToken cancellationToken = default)
        {
            var creative = await _context.AdCreatives.FindAsync(new object[] { creativeId }, cancellationToken);
            if (creative != null)
            {
                _context.AdCreatives.Remove(creative);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task HardDeleteAdSetAsync(Guid adSetId, CancellationToken cancellationToken = default)
        {
            var adSet = await _context.AdSets.FindAsync(new object[] { adSetId }, cancellationToken);
            if (adSet != null)
            {
                _context.AdSets.Remove(adSet);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task ClearFacebookIdsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(ac => ac.Id == campaignId, cancellationToken);
            if (campaign != null)
            {
                campaign.FacebookCampaignId = null;
                campaign.DeploymentStatus = DeploymentStatusEnum.None;
                campaign.DeploymentStep = 0;
                campaign.DeploymentMessage = null;
                campaign.Status = CampaignStatusEnum.Draft;
                campaign.IsActive = false;
                campaign.UpdatedAt = DateTime.UtcNow;
                _context.Entry(campaign).Property(e => e.FacebookCampaignId).IsModified = true;
                _context.Entry(campaign).Property(e => e.DeploymentStatus).IsModified = true;
                _context.Entry(campaign).Property(e => e.DeploymentStep).IsModified = true;
                _context.Entry(campaign).Property(e => e.DeploymentMessage).IsModified = true;
                _context.Entry(campaign).Property(e => e.Status).IsModified = true;
                _context.Entry(campaign).Property(e => e.IsActive).IsModified = true;
                _context.Entry(campaign).Property(e => e.UpdatedAt).IsModified = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Dictionary<Guid, int>> UpdateExpiredCampaignsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredCampaigns = await _context.AdCampaigns
                .Where(ac => !ac.IsDeleted && ac.IsActive && ac.EndDate.HasValue && ac.EndDate.Value < now)
                .ToListAsync(cancellationToken);

            foreach (var campaign in expiredCampaigns)
            {
                campaign.IsActive = false;
                campaign.Status = CampaignStatusEnum.Completed;
                campaign.UpdatedAt = now;
            }

            if (expiredCampaigns.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return expiredCampaigns
                .GroupBy(c => c.WorkspaceId)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IReadOnlyList<AdCampaign>> GetDeployedCampaignsForSyncAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            return await _context.AdCampaigns
                .AsNoTracking()
                .Where(ac => !ac.IsDeleted
                    && ac.DeploymentStatus == DeploymentStatusEnum.Completed
                    && !string.IsNullOrWhiteSpace(ac.FacebookCampaignId))
                .OrderBy(ac => ac.UpdatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AdCampaign>> GetDeployedPendingActivationAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            return await _context.AdCampaigns
                .AsNoTracking()
                .Where(ac => !ac.IsDeleted
                    && ac.DeploymentStatus == DeploymentStatusEnum.Completed
                    && !string.IsNullOrWhiteSpace(ac.FacebookCampaignId)
                    && ac.Status == CampaignStatusEnum.PendingReview)
                .OrderBy(ac => ac.UpdatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AdCampaign>> GetActiveCampaignsPastEndDateAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.AdCampaigns
                .AsNoTracking()
                .Where(ac => !ac.IsDeleted && ac.IsActive && ac.EndDate.HasValue && ac.EndDate.Value < now)
                .ToListAsync(cancellationToken);
        }

    }
}
