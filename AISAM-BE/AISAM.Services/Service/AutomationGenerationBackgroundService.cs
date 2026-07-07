using AISAM.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class AutomationGenerationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationGenerationBackgroundService> _logger;

    public AutomationGenerationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AutomationGenerationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = false;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                processed = await scope.ServiceProvider.GetRequiredService<IAutomationGenerationService>()
                    .ProcessNextAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Automation generation worker iteration failed.");
            }

            try { await Task.Delay(processed ? TimeSpan.FromMilliseconds(250) : TimeSpan.FromSeconds(5), stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
