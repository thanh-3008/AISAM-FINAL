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
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scheduledPostingService = scope.ServiceProvider.GetRequiredService<IScheduledPostingService>();
                await scheduledPostingService.RunDueSchedulesAsync(20, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled posting worker iteration failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
