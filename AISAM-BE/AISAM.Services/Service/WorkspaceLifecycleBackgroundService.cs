using AISAM.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class WorkspaceLifecycleBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<WorkspaceLifecycleBackgroundService> _logger;

    public WorkspaceLifecycleBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<WorkspaceLifecycleBackgroundService> logger)
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
                var service = scope.ServiceProvider.GetRequiredService<IWorkspaceLifecycleProcessingService>();
                await service.RunBatchAsync(100, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Workspace lifecycle worker iteration failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
