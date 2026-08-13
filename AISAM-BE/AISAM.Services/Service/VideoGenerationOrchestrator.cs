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
using AISAM.Data.Enumeration;

namespace AISAM.Services.Service;

public sealed class VideoGenerationOrchestrator : IVideoGenerationOrchestrator
{
    private readonly IAIVideoProvider _primaryProvider;
    private readonly ColabVideoStrategy _colabStrategy;
    private readonly VideoProviderSettings _settings;
    private readonly AISAM.Repositories.AisamContext _dbContext;
    private readonly ICreditService _creditService;
    private readonly ILogger<VideoGenerationOrchestrator> _logger;

    private const int VideoGenerationCredits = 20;

    public VideoGenerationOrchestrator(
        IAIVideoProvider primaryProvider,
        ColabVideoStrategy colabStrategy,
        IOptions<VideoProviderSettings> options,
        AISAM.Repositories.AisamContext dbContext,
        ICreditService creditService,
        ILogger<VideoGenerationOrchestrator> logger)
    {
        _primaryProvider = primaryProvider;
        _colabStrategy = colabStrategy;
        _settings = options.Value;
        _dbContext = dbContext;
        _creditService = creditService;
        _logger = logger;
    }

    public async Task<GenericResponse<VideoGenerationJob>> StartVideoGenerationAsync(
        Guid workspaceId, 
        Guid userId, 
        string prompt, 
        CancellationToken cancellationToken = default)
    {
        // Ensure credits are available before generating
        var creditCheck = await _creditService.EnsureCreditsAvailableAsync(workspaceId, userId, VideoGenerationCredits, cancellationToken: cancellationToken);
        if (!creditCheck.Success)
        {
            return GenericResponse<VideoGenerationJob>.CreateError(
                creditCheck.Message ?? "Insufficient credits for video generation.",
                (HttpStatusCode)creditCheck.StatusCode,
                creditCheck.Error?.ErrorCode);
        }

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

        // 1. Try Primary Provider (OpenAI/DeAPI)
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
                // Deduct credits ONLY after successful provider queueing
                var chargeResult = await _creditService.ConsumeCreditsAsync(workspaceId, userId, CreditActionEnum.GenerateVideo, VideoGenerationCredits, job.Id, cancellationToken: cancellationToken);
                if (!chargeResult.Success)
                {
                    job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
                    job.ErrorMessage = "Failed to deduct credits after generation.";
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return GenericResponse<VideoGenerationJob>.CreateError(chargeResult.Message!, (HttpStatusCode)chargeResult.StatusCode, chargeResult.Error?.ErrorCode);
                }
                
                await _dbContext.SaveChangesAsync(cancellationToken);
                return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Video generation started successfully.");
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

        // 2. Fallback to Colab (Last Resort)
        if (!_settings.EnableColabFallback)
        {
            job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return GenericResponse<VideoGenerationJob>.CreateError($"Primary provider failed and Colab fallback is disabled. {job.ErrorMessage}", HttpStatusCode.BadGateway);
        }

        _logger.LogInformation("Falling back to Colab video strategy for job {JobId}", job.Id);
        var colabResult = await _colabStrategy.StartVideoGenerationAsync(prompt, null, cancellationToken);
        if (colabResult.Success && !string.IsNullOrWhiteSpace(colabResult.JobId))
        {
            job.Provider = colabResult.ProviderName;
            job.ExternalJobId = colabResult.JobId;
            job.IsFallback = true;
            // Deduct credits ONLY after successful provider queueing
            var chargeResult = await _creditService.ConsumeCreditsAsync(workspaceId, userId, CreditActionEnum.GenerateVideo, VideoGenerationCredits, job.Id, cancellationToken: cancellationToken);
            if (!chargeResult.Success)
            {
                job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
                job.ErrorMessage = "Failed to deduct credits after generation.";
                await _dbContext.SaveChangesAsync(cancellationToken);
                return GenericResponse<VideoGenerationJob>.CreateError(chargeResult.Message!, (HttpStatusCode)chargeResult.StatusCode, chargeResult.Error?.ErrorCode);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Video generation started with fallback Colab provider.");
        }

        _logger.LogError("Colab failed or is unreachable. Reason: {Error}. DEVELOPER ACTION REQUIRED: Ngrok or Colab session might be dead.", colabResult.ErrorMessage);
        job.Status = AISAM.Data.Enumeration.AiStatusEnum.Failed;
        job.ErrorMessage = $"{job.ErrorMessage} | Colab Error: {colabResult.ErrorMessage}";
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
            // Do NOT mark as Completed here. Let the BackgroundService upload to Cloudinary and mark as Completed.
            _logger.LogInformation("Job {JobId} is Done at provider. Waiting for background service to upload and complete.", job.Id);
        }

        return GenericResponse<VideoGenerationJob>.CreateSuccess(job, "Job status checked.");
    }
}
