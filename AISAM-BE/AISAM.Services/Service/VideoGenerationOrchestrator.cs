using System;
using System.Threading;
using System.Threading.Tasks;
using AISAM.Common;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AISAM.Common.Models;
using System.Net;

namespace AISAM.Services.Service;

public sealed class VideoGenerationOrchestrator : IVideoGenerationOrchestrator
{
    private readonly FallbackVideoProvider _primaryProvider;
    private readonly ColabVideoStrategy _colabStrategy;
    private readonly VideoProviderSettings _settings;
    private readonly AISAM.Repositories.AisamContext _dbContext;
    private readonly ILogger<VideoGenerationOrchestrator> _logger;

    public VideoGenerationOrchestrator(
        FallbackVideoProvider primaryProvider,
        ColabVideoStrategy colabStrategy,
        IOptions<VideoProviderSettings> options,
        AISAM.Repositories.AisamContext dbContext,
        ILogger<VideoGenerationOrchestrator> logger)
    {
        _primaryProvider = primaryProvider;
        _colabStrategy = colabStrategy;
        _settings = options.Value;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GenericResponse<VideoGenerationJob>> StartVideoGenerationAsync(
        Guid workspaceId, 
        Guid userId, 
        string prompt, 
        CancellationToken cancellationToken = default)
    {
        var job = new VideoGenerationJob
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            OriginalPrompt = prompt,
            Status = AISAM.Data.Enumeration.AiStatusEnum.Pending,
            SegmentsCount = _settings.DefaultSegmentCount > 0 ? _settings.DefaultSegmentCount : 3,
            CurrentSegment = 0
        };

        _dbContext.VideoGenerationJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 1. Try Primary Provider (Pollen / OpenRouter)
        _logger.LogInformation("Attempting primary video provider for job {JobId}", job.Id);
        
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_settings.PollenTimeoutSeconds > 0)
        {
            cts.CancelAfter(TimeSpan.FromSeconds(_settings.PollenTimeoutSeconds));
        }

        try
        {
            var primaryResult = await _primaryProvider.StartVideoGenerationAsync(prompt, null, cts.Token);
            if (primaryResult.Success && !string.IsNullOrWhiteSpace(primaryResult.JobId))
            {
                job.Provider = primaryResult.ProviderName;
                job.ExternalJobId = primaryResult.JobId;
                job.IsFallback = false;
                job.Status = AISAM.Data.Enumeration.AiStatusEnum.Processing;
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Video generation started with primary provider.");
            }
            
            _logger.LogWarning("Primary provider failed. Reason: {Error}", primaryResult.ErrorMessage);
            job.ErrorMessage = primaryResult.ErrorMessage;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Primary provider timed out after {Seconds} seconds.", _settings.PollenTimeoutSeconds);
            job.ErrorMessage = "Primary provider timed out.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling primary provider.");
            job.ErrorMessage = ex.Message;
        }

        // 2. Fallback to Colab if enabled
        if (!_settings.EnableColabFallback)
        {
            job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return GenericResponse<VideoGenerationJob>.CreateError("Primary provider failed and fallback is disabled.", HttpStatusCode.BadGateway);
        }

        _logger.LogInformation("Falling back to Colab video strategy for job {JobId}", job.Id);
        
        var fallbackResult = await _colabStrategy.StartVideoGenerationAsync(prompt, null, cancellationToken);
        if (fallbackResult.Success && !string.IsNullOrWhiteSpace(fallbackResult.JobId))
        {
            job.Provider = fallbackResult.ProviderName;
            job.ExternalJobId = fallbackResult.JobId;
            job.IsFallback = true;
            job.Status = AISAM.Data.Enumeration.AiStatusEnum.Processing;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Video generation started with fallback Colab provider.");
        }

        job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
        job.ErrorMessage = string.IsNullOrEmpty(job.ErrorMessage) 
            ? fallbackResult.ErrorMessage 
            : $"{job.ErrorMessage} | Fallback Error: {fallbackResult.ErrorMessage}";
            
        await _dbContext.SaveChangesAsync(cancellationToken);
        return GenericResponse<VideoGenerationJob>.CreateError(
            $"Dịch vụ sinh video đang tạm gián đoạn. {fallbackResult.ErrorMessage}", 
            HttpStatusCode.BadGateway);
    }

    public async Task<GenericResponse<VideoGenerationJob>> CheckVideoStatusAsync(
        Guid jobId, 
        Guid workspaceId, 
        CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.VideoGenerationJobs.FindAsync(new object[] { jobId }, cancellationToken);
        if (job == null || job.WorkspaceId != workspaceId)
        {
            return GenericResponse<VideoGenerationJob>.CreateError("Video job not found.", HttpStatusCode.NotFound);
        }

        if (job.Status == AISAM.Data.Enumeration.AiStatusEnum.Completed || job.Status == AISAM.Data.Enumeration.AiStatusEnum.Failed)
        {
            return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Job already finished.");
        }

        if (string.IsNullOrWhiteSpace(job.ExternalJobId))
        {
            return GenericResponse<VideoGenerationJob>.CreateError("External JobId is missing.", HttpStatusCode.BadRequest);
        }

        IAIVideoProvider activeProvider = job.IsFallback ? _colabStrategy : _primaryProvider;
        var result = await activeProvider.CheckStatusAsync(job.ExternalJobId, cancellationToken);

        if (result.Status == VideoGenerationStatus.Failed)
        {
            job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
            job.ErrorMessage = result.ErrorMessage;
            job.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Video generation failed.");
        }

        if (result.Status == VideoGenerationStatus.Done && !string.IsNullOrWhiteSpace(result.MediaUrl))
        {
            // Cập nhật trực tiếp nếu provider đã trả về URL.
            // Background service sẽ upload lên Cloudinary và ghi đè VideoUrl sau.
            job.Status = AISAM.Data.Enumeration.AiStatusEnum.Completed;
            job.VideoUrl = result.MediaUrl;
            job.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Job {JobId} marked Completed via status check. VideoUrl={Url}", job.Id, result.MediaUrl);
        }

        return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Job status checked.");
    }
}
