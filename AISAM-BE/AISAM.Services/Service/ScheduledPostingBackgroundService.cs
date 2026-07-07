using AISAM.Common.Messages;
using AISAM.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class ScheduledPostingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ScheduledPostingBackgroundService> _logger;

    public ScheduledPostingBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ScheduledPostingBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var hasJobs = false;
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scheduledPostingService = scope.ServiceProvider.GetRequiredService<IScheduledPostingService>();
                var result = await scheduledPostingService.RunDueSchedulesAsync(20, stoppingToken);
                hasJobs = result.ScannedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MessageConstants.Schedule.WorkerIterationFailed);
            }

            try
            {
                var delaySeconds = hasJobs ? 15 : 60; // Backoff to 60s when idle
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
