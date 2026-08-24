using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AISAM.Services.IServices;

namespace AISAM.Services.Service
{
    public sealed class CampaignInsightsBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CampaignInsightsBackgroundService> _logger;

        public CampaignInsightsBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<CampaignInsightsBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var processed = false;
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var syncService = scope.ServiceProvider.GetRequiredService<ICampaignInsightsSyncService>();
                    processed = await syncService.ProcessNextAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Campaign insights sync worker iteration failed");
                }

                try
                {
                    var delay = processed ? TimeSpan.FromSeconds(30) : TimeSpan.FromMinutes(5);
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
