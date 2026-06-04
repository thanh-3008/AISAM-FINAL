namespace AISAM.Repositories.IRepositories;

public interface IPerformanceReportRepository
{
    Task<int> CountByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
}
