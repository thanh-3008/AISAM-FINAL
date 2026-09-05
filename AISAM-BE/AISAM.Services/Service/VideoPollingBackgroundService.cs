using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

public sealed class VideoPollingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<VideoPollingBackgroundService> _logger;

    public VideoPollingBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<VideoPollingBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[VideoPolling] Background service STARTED.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var hasJobs = false;
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AisamContext>();
                var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();

                var pendingJobs = await dbContext.AiGenerations
                    // The status service reloads the generation after acquiring its per-generation
                    // lock. Keep this discovery query detached so it cannot supply a stale tracked
                    // entity that overwrites a concurrent API/background transition.
                    .AsNoTracking()
                    .Include(g => g.Content)
                        .ThenInclude(c => c.Profile)
                    .Where(g => (g.Status == AiStatusEnum.Processing && !string.IsNullOrEmpty(g.VideoJobId))
                             || (g.Status == AiStatusEnum.Completed && g.GeneratedVideoUrl != null && g.Content.VideoUrl == null))
                    .ToListAsync(stoppingToken);

                hasJobs = pendingJobs.Count > 0;

                _logger.LogInformation("[VideoPolling] Found {Count} pending/sync-needed video jobs.", pendingJobs.Count);

                foreach (var job in pendingJobs)
                {
                    try
                    {
                        var decision = await scope.ServiceProvider.GetRequiredService<ExecutionAuthorizationService>()
                            .CheckAsync("AiGeneration", job.Id, "AiGenerate", stoppingToken);
                        if (!decision.Allowed) continue;
                        // Fix for already completed videos that missed the Content update
                        if (job.Status == AiStatusEnum.Completed && job.GeneratedVideoUrl != null && job.Content?.VideoUrl == null)
                        {
                            if (job.Content != null)
                            {
                                job.Content.VideoUrl = job.GeneratedVideoUrl;
                                dbContext.Update(job.Content);
                                await dbContext.SaveChangesAsync(stoppingToken);
                                _logger.LogInformation("[VideoPolling] Synced missing VideoUrl for ContentId: {ContentId}", job.ContentId);
                            }
                            continue;
                        }

                        if (job.Content == null)
                        {
                            _logger.LogWarning("[VideoPolling] Job {JobId} has no Content, skipping.", job.Id);
                            continue;
                        }

                        var workspaceId = job.Content.WorkspaceId;

                        var operation = await dbContext.Set<ExecutionOperation>().AsNoTracking().SingleAsync(
                            o => o.ResourceType == "AiGeneration" && o.ReferenceId == job.Id && o.RequestedAction == "AiGenerate", stoppingToken);
                        var userId = operation.ActorUserId;

                        _logger.LogInformation("[VideoPolling] Polling job {GenId} VideoJobId={VideoJobId}", job.Id, job.VideoJobId);

                        // This will poll the provider and update the DB if done/failed.
                        var result = await aiService.CheckVideoStatusAsync(job.Id, workspaceId, userId, stoppingToken);

                        _logger.LogInformation("[VideoPolling] Poll result for {GenId}: StatusCode={StatusCode} Data.Status={Status}",
                            job.Id, result.StatusCode, result.Data?.Status);

                        // Thêm buffer delay giữa các job để tránh DeAPI rate limit (429)
                        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[VideoPolling] Failed to poll status for AiGeneration Id: {Id}", job.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VideoPolling] Error occurred during video polling iteration.");
            }

            try
            {
                var delaySeconds = hasJobs ? 45 : 120; // Tăng delay lên 45s để tránh DeAPI rate limit
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("[VideoPolling] Background service STOPPED.");
    }
}
