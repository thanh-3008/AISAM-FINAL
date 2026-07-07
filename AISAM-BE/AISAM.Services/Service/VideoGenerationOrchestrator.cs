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

        bool colabFailed = false;

        // 1. Try Colab first if enabled (Free ngrok/session)
        if (_settings.EnableColabFallback)
        {
            _logger.LogInformation("Attempting Colab video strategy for job {JobId}", job.Id);
            var colabResult = await _colabStrategy.StartVideoGenerationAsync(prompt, null, cancellationToken);
            if (colabResult.Success && !string.IsNullOrWhiteSpace(colabResult.JobId))
            {
                job.Provider = colabResult.ProviderName;
                job.ExternalJobId = colabResult.JobId;
                job.IsFallback = false;
                job.Status = AISAM.Data.Enumeration.AiStatusEnum.Processing;
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Video generation started with Colab.");
            }
            
            _logger.LogError("Colab failed or is unreachable. Reason: {Error}. DEVELOPER ACTION REQUIRED: Ngrok or Colab session might be dead. Please restart Colab and update VIDEO_COLAB_BASE_URL.", colabResult.ErrorMessage);
            job.ErrorMessage = colabResult.ErrorMessage;
            colabFailed = true;
        }

        // 2. Fallback to Primary Provider (Pollen / OpenRouter)
        _logger.LogInformation("Attempting Pollen API (OpenRouter/DeAPI) fallback for job {JobId}", job.Id);
        
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
                job.IsFallback = colabFailed;
                job.Status = AISAM.Data.Enumeration.AiStatusEnum.Processing;
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Video generation started with Pollen API.");
            }
            
            _logger.LogWarning("Pollen API failed. Reason: {Error}", primaryResult.ErrorMessage);
            job.ErrorMessage = colabFailed 
                ? $"{job.ErrorMessage} | Pollen Error: {primaryResult.ErrorMessage}"
                : primaryResult.ErrorMessage;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Pollen API timed out after {Seconds} seconds.", _settings.PollenTimeoutSeconds);
            job.ErrorMessage = colabFailed 
                ? $"{job.ErrorMessage} | Pollen Error: Timed out."
                : "Pollen API timed out.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling Pollen API.");
            job.ErrorMessage = colabFailed 
                ? $"{job.ErrorMessage} | Pollen Exception: {ex.Message}"
                : ex.Message;
        }

        job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return GenericResponse<VideoGenerationJob>.CreateError(
            $"Dịch vụ sinh video đang tạm gián đoạn. {job.ErrorMessage}", 
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
