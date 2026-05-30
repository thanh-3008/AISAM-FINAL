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

    public async Task<IEnumerable<AiGeneration>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        return await _context.AiGenerations
            .Include(generation => generation.Content)
            .Where(generation => generation.ContentId == contentId && !generation.IsDeleted)
            .OrderByDescending(generation => generation.CreatedAt)
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
}
