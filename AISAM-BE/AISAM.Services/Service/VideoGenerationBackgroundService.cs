using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AISAM.Common.Models;

namespace AISAM.Services.Service;

public sealed class VideoGenerationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<VideoGenerationBackgroundService> _logger;

    public VideoGenerationBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<VideoGenerationBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[VideoGenerationBackgroundService] STARTED.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AisamContext>();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IVideoGenerationOrchestrator>();
                var mediaStorage = scope.ServiceProvider.GetRequiredService<IMediaStorageService>();
                var settings = scope.ServiceProvider.GetRequiredService<IOptions<VideoProviderSettings>>().Value;

                var pollingInterval = TimeSpan.FromSeconds(settings.PollingIntervalSeconds > 0 ? settings.PollingIntervalSeconds : 15);
                var timeoutLimit = TimeSpan.FromMinutes(settings.TimeoutMinutes > 0 ? settings.TimeoutMinutes : 30);
                var timeoutThreshold = DateTime.UtcNow.Subtract(timeoutLimit);

                var pendingJobs = await dbContext.VideoGenerationJobs
                    .Where(j => j.Status == AiStatusEnum.Processing && !string.IsNullOrEmpty(j.ExternalJobId))
                    .ToListAsync(stoppingToken);

                if (pendingJobs.Count > 0)
                {
                    _logger.LogInformation("[VideoGenerationBackgroundService] Found {Count} pending video jobs.", pendingJobs.Count);
                }

                foreach (var job in pendingJobs)
                {
                    try
                    {
                        if (job.CreatedAt < timeoutThreshold)
                        {
                            job.Status = AiStatusEnum.Failed;
                            job.ErrorMessage = "Video generation timed out.";
                            job.CompletedAt = DateTime.UtcNow;
                            dbContext.Update(job);
                            await dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogWarning("[VideoGenerationBackgroundService] Job {JobId} timed out.", job.Id);
                            continue;
                        }

                        // We can call orchestrator to get status, but orchestrator's CheckVideoStatusAsync doesn't upload the video.
                        // Let's check status directly via the provider.
                        // ── Route provider theo job.Provider ──────────────────────
                        // Convention: Beeknoee jobs có Provider bắt đầu bằng "Beeknoee"
                        //             DeAPI jobs: IsFallback=false, Provider != Beeknoee
                        //             Colab jobs: IsFallback=true
                        IAIVideoProvider provider;
                        BeeknoeeVideoClient? beeknoeeClient = null;

                        if (job.Provider != null && job.Provider.StartsWith("Beeknoee", StringComparison.OrdinalIgnoreCase))
                        {
                            var beeknoeeProvider = scope.ServiceProvider.GetRequiredService<BeeknoeeVideoProvider>();
                            beeknoeeClient = scope.ServiceProvider.GetRequiredService<BeeknoeeVideoClient>();
                            provider = beeknoeeProvider;
                        }
                        else if (job.IsFallback)
                        {
                            provider = scope.ServiceProvider.GetRequiredService<ColabVideoStrategy>();
                        }
                        else
                        {
                            provider = scope.ServiceProvider.GetRequiredService<FallbackVideoProvider>();
                        }

                        var result = await provider.CheckStatusAsync(job.ExternalJobId!, stoppingToken);

                        if (result.Status == VideoGenerationStatus.Failed)
                        {
                            job.Status = AiStatusEnum.Failed;
                            job.ErrorMessage = result.ErrorMessage;
                            job.CompletedAt = DateTime.UtcNow;
                            dbContext.Update(job);
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                        else if (result.Status == VideoGenerationStatus.Done && !string.IsNullOrWhiteSpace(result.MediaUrl))
                        {
                            _logger.LogInformation("[VideoGenerationBackgroundService] Job {JobId} completed. Downloading video...", job.Id);

                            try
                            {
                                byte[] bytes;
                                var baseUrl = scope.ServiceProvider
                                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<AISAM.Common.Models.BeeknoeeSettings>>()
                                    .Value.BaseUrl.TrimEnd('/');

                                bool isBeeknoeeProxyUrl = beeknoeeClient != null
                                    && result.MediaUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase)
                                    && result.MediaUrl.Contains("/v1/video/generations/");

                                if (isBeeknoeeProxyUrl && beeknoeeClient != null)
                                {
                                    // Beeknoee proxy URL — cần Authorization header
                                    // Trích job_id an toàn từ job.ExternalJobId ("beeknoee:{id}")
                                    var beeknoeeJobId = job.ExternalJobId != null && job.ExternalJobId.StartsWith("beeknoee:", StringComparison.OrdinalIgnoreCase)
                                        ? job.ExternalJobId["beeknoee:".Length..]
                                        : string.Empty;

                                    if (string.IsNullOrWhiteSpace(beeknoeeJobId))
                                    {
                                        throw new InvalidOperationException("Cannot extract Beeknoee JobId from ExternalJobId for downloading.");
                                    }

                                    _logger.LogInformation(
                                        "[VideoGenerationBackgroundService] Downloading via Beeknoee auth. BeeknoeeJobId={Id}", beeknoeeJobId);
                                    bytes = await beeknoeeClient.DownloadVideoAsync(beeknoeeJobId, stoppingToken);
                                }
                                else
                                {
                                    // Source URL public (Veo, Colab, DeAPI) — không cần auth
                                    using var httpClient = new HttpClient();
                                    bytes = await httpClient.GetByteArrayAsync(result.MediaUrl, stoppingToken);
                                }

                                var fileName = $"video-job-{job.Id}.mp4";
                                var uploadedUrl = await mediaStorage.UploadBytesAsync(bytes, "ai-videos", fileName, stoppingToken);

                                job.VideoUrl = uploadedUrl;
                                job.Status = AiStatusEnum.Completed;
                                job.CompletedAt = DateTime.UtcNow;
                                dbContext.Update(job);
                                await dbContext.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation(
                                    "[VideoGenerationBackgroundService] Job {JobId} video uploaded to {Url}.", job.Id, uploadedUrl);
                            }
                            catch (Exception ex)
                            {
                                job.Status = AiStatusEnum.Failed;
                                job.ErrorMessage = "Failed to download or upload generated video: " + ex.Message;
                                job.CompletedAt = DateTime.UtcNow;
                                dbContext.Update(job);
                                await dbContext.SaveChangesAsync(stoppingToken);
                                _logger.LogError(ex, "[VideoGenerationBackgroundService] Error finalizing job {JobId}", job.Id);
                            }
                        }
                        else if (job.IsFallback && result.Status == VideoGenerationStatus.Processing)
                        {
                            // Try to get progress if possible, for Colab we can do this inside ColabVideoStrategy or
                            // we would need ColabVideoStrategy to return the raw object.
                            // The current CheckStatusAsync doesn't return CurrentSegment yet.
                            // Since CheckStatusAsync returns VideoGenerationResult, we might just poll.
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[VideoGenerationBackgroundService] Failed to poll status for Job: {Id}", job.Id);
                    }
                }
                
                await Task.Delay(pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VideoGenerationBackgroundService] Error occurred during video polling iteration.");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("[VideoGenerationBackgroundService] STOPPED.");
    }
}
