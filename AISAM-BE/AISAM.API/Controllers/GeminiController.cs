using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public sealed class GeminiController : ControllerBase
{
    private readonly IAIService _aiService;

    public GeminiController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("generate-draft")]
    public async Task<ActionResult<GenericResponse<AiGenerationResponse>>> GenerateDraft(
        [FromBody] CreateDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var result = await _aiService.GenerateDraftAsync(
            GetProfileId(),
            membership.WorkspaceId,
            membership.UserId,
            request,
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("improve/{contentId:guid}")]
    public async Task<ActionResult<GenericResponse<AiGenerationResponse>>> Improve(
        Guid contentId,
        [FromBody] ImproveContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var result = await _aiService.ImproveAsync(
            contentId,
            GetProfileId(),
            membership.WorkspaceId,
            membership.UserId,
            request,
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("approve/{aiGenerationId:guid}")]
    public async Task<ActionResult<GenericResponse<ContentResponseDto>>> Approve(
        Guid aiGenerationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _aiService.ApproveInWorkspaceAsync(aiGenerationId, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("generations/{contentId:guid}")]
    public async Task<ActionResult<GenericResponse<IEnumerable<AiGenerationResponse>>>> GetGenerations(
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _aiService.GetGenerationsInWorkspaceAsync(contentId, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("chat")]
    public async Task<ActionResult<GenericResponse<ChatResponse>>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var result = await _aiService.ChatInWorkspaceAsync(GetProfileId(), membership.WorkspaceId, membership.UserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("generate-image")]
    public async Task<ActionResult> GenerateImage(
        [FromBody] GenerateImageRequest request, CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var result = await _aiService.GenerateImageAsync(
            membership.WorkspaceId, membership.UserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("generate-video")]
    public async Task<ActionResult> GenerateVideo(
        [FromBody] GenerateVideoRequest request, CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var result = await _aiService.StartVideoGenerationAsync(
            membership.WorkspaceId, membership.UserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("video-status/{generationId:guid}")]
    public async Task<ActionResult> VideoStatus(
        Guid generationId, CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var result = await _aiService.CheckVideoStatusAsync(
            generationId, membership.WorkspaceId, membership.UserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetProfileId()
    {
        return ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
    }

}
