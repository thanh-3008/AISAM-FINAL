using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IScheduledPostingService
{
    Task<SchedulerRunResultDto> RunDueSchedulesAsync(int batchSize, CancellationToken cancellationToken = default);
}
