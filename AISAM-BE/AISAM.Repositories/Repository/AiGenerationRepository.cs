using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class AiGenerationRepository : IAiGenerationRepository
{
    private readonly AisamContext _context;

    public AiGenerationRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<AiGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AiGenerations
            .Include(generation => generation.Content)
            .FirstOrDefaultAsync(generation => generation.Id == id && !generation.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<AiGenerationListDto>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        return await _context.AiGenerations
            .Where(generation => generation.ContentId == contentId && !generation.IsDeleted)
            .OrderByDescending(generation => generation.CreatedAt)
            .Select(generation => new AiGenerationListDto
            {
                Id = generation.Id,
                ContentId = generation.ContentId,
                AiPrompt = generation.AiPrompt,
                GeneratedImageUrl = generation.GeneratedImageUrl,
                GeneratedVideoUrl = generation.GeneratedVideoUrl,
                GeneratedText = generation.GeneratedText,
                VideoJobId = generation.VideoJobId,
                ProviderName = generation.ProviderName,
                Status = generation.Status,
                ErrorMessage = generation.ErrorMessage,
                CreatedAt = generation.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AiGeneration> AddAsync(AiGeneration generation, CancellationToken cancellationToken = default)
    {
        generation.CreatedAt = DateTime.UtcNow;
        generation.UpdatedAt = DateTime.UtcNow;
        _context.AiGenerations.Add(generation);
        await _context.SaveChangesAsync(cancellationToken);
        return generation;
    }

    public async Task UpdateAsync(AiGeneration generation, CancellationToken cancellationToken = default)
    {
        generation.UpdatedAt = DateTime.UtcNow;
        _context.AiGenerations.Update(generation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<DateTime, int>> GetDailyGenerationCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.AiGenerations
            .Where(g => g.CreatedAt >= from && g.CreatedAt <= to)
            .GroupBy(g => g.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, cancellationToken);
    }

    public async Task<int> GetTotalGenerationCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AiGenerations.CountAsync(cancellationToken);
    }

    public async Task<List<dynamic>> GetTopWorkspacesByGenerationAsync(int limit, CancellationToken cancellationToken = default)
    {
        var data = await _context.AiGenerations
            .Where(g => g.Content != null && !g.IsDeleted)
            .GroupBy(g => g.Content.WorkspaceId)
            .Select(g => new
            {
                WorkspaceId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return data.Cast<dynamic>().ToList();
    }
    public async Task<List<string>> GetRecentVideoPatternIdsByProductAsync(Guid productId, int limit = 3, CancellationToken cancellationToken = default)
    {
        var patternIds = await _context.AiGenerations
            .Include(g => g.Content)
            .Where(g => g.Content != null && g.Content.ProductId == productId && !g.IsDeleted && !string.IsNullOrWhiteSpace(g.PatternId) && (g.VideoJobId != null || g.GeneratedVideoUrl != null))
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => g.PatternId)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return patternIds.Where(p => p != null).Select(p => p!).ToList();
    }
}
