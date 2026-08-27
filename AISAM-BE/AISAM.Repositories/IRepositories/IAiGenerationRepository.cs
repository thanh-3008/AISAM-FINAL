using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories;

public interface IAiGenerationRepository
{
    Task<AiGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiGeneration?> GetActiveVideoByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AiGenerationListDto>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<AiGeneration> AddAsync(AiGeneration generation, CancellationToken cancellationToken = default);
    Task UpdateAsync(AiGeneration generation, CancellationToken cancellationToken = default);
    Task<Dictionary<DateTime, int>> GetDailyGenerationCountAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<int> GetTotalGenerationCountAsync(CancellationToken cancellationToken = default);
    Task<List<dynamic>> GetTopWorkspacesByGenerationAsync(int limit, CancellationToken cancellationToken = default);
    Task<List<string>> GetRecentVideoPatternIdsByProductAsync(Guid productId, int limit = 3, CancellationToken cancellationToken = default);
}
