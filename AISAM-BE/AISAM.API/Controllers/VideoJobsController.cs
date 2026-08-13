using System;
using System.Threading;
using System.Threading.Tasks;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Model;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/ai/video-jobs")]
[Authorize]
public sealed class VideoJobsController : ControllerBase
{
    private readonly IVideoGenerationOrchestrator _orchestrator;

    public VideoJobsController(IVideoGenerationOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<VideoJobResponseDto>>> CreateVideoJob(
        [FromBody] CreateVideoJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);

        var userId = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext).UserId;

        var result = await _orchestrator.StartVideoGenerationAsync(
            workspaceId, 
            userId, 
            request.Prompt, 
            cancellationToken);

        if (!result.Success || result.Data == null)
        {
            return StatusCode((int)result.StatusCode, result);
        }

        var responseData = MapToDto(result.Data);
        var successResponse = GenericResponse<VideoJobResponseDto>.CreateSuccess(responseData, result.Message ?? "Operation successful");
        
        // Add a warning message if it fell back to Colab
        if (result.Data.IsFallback)
        {
            successResponse.Message = "Đang xử lý bằng phương án dự phòng (Colab Wan2.2), thời gian có thể lâu hơn bình thường.";
        }

        return StatusCode((int)result.StatusCode, successResponse);
    }

    [HttpGet("{jobId:guid}")]
    public async Task<ActionResult<GenericResponse<VideoJobResponseDto>>> GetVideoJobStatus(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);

        var result = await _orchestrator.CheckVideoStatusAsync(jobId, workspaceId, cancellationToken);
        
        if (!result.Success || result.Data == null)
        {
            return StatusCode((int)result.StatusCode, result);
        }

        var responseData = MapToDto(result.Data);
        var successResponse = GenericResponse<VideoJobResponseDto>.CreateSuccess(responseData, result.Message ?? "Operation successful");
        
        // Include warning for frontend
        if (result.Data.IsFallback && result.Data.Status != AISAM.Data.Enumeration.AiStatusEnum.Completed && result.Data.Status != AISAM.Data.Enumeration.AiStatusEnum.Failed)
        {
            successResponse.Message = "Đang xử lý bằng phương án dự phòng (Colab Wan2.2), thời gian có thể lâu hơn bình thường.";
        }

        return StatusCode((int)result.StatusCode, successResponse);
    }

    private static VideoJobResponseDto MapToDto(VideoGenerationJob job)
    {
        return new VideoJobResponseDto
        {
            Id = job.Id,
            WorkspaceId = job.WorkspaceId,
            OriginalPrompt = job.OriginalPrompt,
            Provider = job.Provider,
            IsFallback = job.IsFallback,
            Status = job.Status.ToString(),
            SegmentsCount = job.SegmentsCount,
            CurrentSegment = job.CurrentSegment,
            VideoUrl = job.VideoUrl,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            CompletedAt = job.CompletedAt
        };
    }
}

public class CreateVideoJobRequest
{
    public string Prompt { get; set; } = string.Empty;
}

public class VideoJobResponseDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string OriginalPrompt { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? SegmentsCount { get; set; }
    public int? CurrentSegment { get; set; }
    public string? VideoUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
