using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IAiGenerationRepository
{
    Task<AiGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AiGeneration>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<AiGeneration> AddAsync(AiGeneration generation, CancellationToken cancellationToken = default);
    Task UpdateAsync(AiGeneration generation, CancellationToken cancellationToken = default);
}
