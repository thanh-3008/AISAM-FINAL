using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository
{
    public class BrandRepository : IBrandRepository
    {
        private readonly AisamContext _context;

        public BrandRepository(AisamContext context)
        {
            _context = context;
        }

        public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .AsSplitQuery()
                .Include(b => b.Profile)
                .Include(b => b.Workspace)
                .Include(b => b.Products)
                .Include(b => b.Contents)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        }

        public async Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .AsSplitQuery()
                .Include(b => b.Profile)
                .Include(b => b.Workspace)
                .Include(b => b.Products)
                .Include(b => b.Contents)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(
            Guid workspaceId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var query = _context.Brands
                .AsSplitQuery()
                .Include(b => b.Profile)
                .Include(b => b.Workspace)
                .Include(b => b.Products)
                .Include(b => b.Contents)
                .Where(b => b.WorkspaceId == workspaceId);

            if (!includeDeleted)
            {
                query = query.Where(b => !b.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchPattern = $"%{request.SearchTerm}%";
                query = query.Where(b =>
                    EF.Functions.ILike(b.Name, searchPattern) ||
                    (b.Description != null && EF.Functions.ILike(b.Description, searchPattern)));
            }

            query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<Brand> { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
        }

        public async Task<PagedResult<Brand>> GetPagedByProfileIdAsync(
            Guid profileId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var query = _context.Brands
                .AsSplitQuery()
                .Include(b => b.Profile)
                .Include(b => b.Products)
                .Include(b => b.Contents)
                .Where(b => b.ProfileId == profileId);

            if (!includeDeleted)
            {
                query = query.Where(b => !b.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchPattern = $"%{request.SearchTerm}%";
                query = query.Where(b =>
                    EF.Functions.ILike(b.Name, searchPattern) ||
                    (b.Description != null && EF.Functions.ILike(b.Description, searchPattern)));
            }

            query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Brand>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            brand.CreatedAt = DateTime.UtcNow;
            brand.UpdatedAt = DateTime.UtcNow;

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync(cancellationToken);
            return brand;
        }

        public async Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            brand.UpdatedAt = DateTime.UtcNow;
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
