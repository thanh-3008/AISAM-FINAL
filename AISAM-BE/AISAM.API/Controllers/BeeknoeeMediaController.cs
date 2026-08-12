using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.API.Controllers;


// ── Request/Response DTOs cho endpoint này ────────────────────────────────────

/// <summary>Request body cho POST /api/beeknoee/generate-image.</summary>
public sealed class BeeknoeeGenerateImageApiRequest
{
    /// <summary>Content ID để liên kết với AiGeneration record.</summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Prompt mô tả ảnh. Nếu null, hệ thống tự build từ Content.TextContent.
    /// </summary>
    public string? CustomPrompt { get; set; }

    /// <summary>
    /// Model ID Beeknoee. Nếu null, dùng default từ BeeknoeeSettings.DefaultImageModel.
    /// VD: "gemini-3-pro-image-preview", "gpt-image-1-mini".
    /// </summary>
    public string? Model { get; set; }

    /// <summary>Kích thước ảnh theo format "WxH" (VD: "1024x1024"). Null = server default.</summary>
    public string? Size { get; set; }

    /// <summary>
    /// Danh sách URL ảnh tham chiếu cho image-to-image (Gemini, Flux, Seedream, Wan, Grok).
    /// Để trống cho text-to-image.
    /// </summary>
    public List<string>? ReferenceImageUrls { get; set; }
}

/// <summary>Response body cho POST /api/beeknoee/generate-image.</summary>
public sealed class BeeknoeeGenerateImageApiResponse
{
    /// <summary>ID của AiGeneration record được tạo.</summary>
    public Guid AiGenerationId { get; set; }

    /// <summary>URL ảnh đã upload lên Cloudinary (chỉ có khi Completed = true).</summary>
    public string? ImageUrl { get; set; }

    /// <summary>true nếu ảnh đã sẵn sàng (status COMPLETED).</summary>
    public bool Completed { get; set; }

    /// <summary>
    /// true nếu provider yêu cầu xử lý bất đồng bộ (PROCESSING/PENDING) —
    /// không hỗ trợ trong Phương án 1. Dùng Phương án 2 (polling) để tiếp tục.
    /// </summary>
    public bool RequiresAsyncHandling { get; set; }

    /// <summary>Job ID từ Beeknoee (chỉ có khi RequiresAsyncHandling = true).</summary>
    public string? BeeknoeeJobId { get; set; }

    /// <summary>Provider đã thực sự tạo ảnh.</summary>
    public string? ProviderUsed { get; set; }

    /// <summary>Chi phí ước tính (VNĐ) — lấy từ Beeknoee response.</summary>
    public decimal? CostVnd { get; set; }

    public string? ErrorMessage { get; set; }
}

// ── Controller ────────────────────────────────────────────────────────────────

/// <summary>
/// Endpoint demo Beeknoee Image Generation API — Phương án 1 (sync-first).
///
/// Route: api/beeknoee (tách biệt khỏi api/ai để không ảnh hưởng tính năng hiện có).
/// </summary>
[ApiController]
[Route("api/beeknoee")]
[Authorize]
public sealed class BeeknoeeMediaController : ControllerBase
{
    private readonly IBeeknoeeSyncImageClient _beeknoeeClient;
    private readonly IContentRepository _contentRepository;
    private readonly IAiGenerationRepository _generationRepository;
    private readonly IMediaStorageService _mediaStorage;
    private readonly ICreditService _creditService;
    private readonly BeeknoeeSettings _settings;
    private readonly ILogger<BeeknoeeMediaController> _logger;

    // Credit cost cho ảnh Beeknoee — cùng với GenerateImage hiện tại (5 credits)
    private const long ImageGenerationCredits = 5;

    public BeeknoeeMediaController(
        IBeeknoeeSyncImageClient beeknoeeClient,
        IContentRepository contentRepository,
        IAiGenerationRepository generationRepository,
        IMediaStorageService mediaStorage,
        ICreditService creditService,
        Microsoft.Extensions.Options.IOptions<BeeknoeeSettings> settings,
        ILogger<BeeknoeeMediaController> logger)
    {
        _beeknoeeClient = beeknoeeClient;
        _contentRepository = contentRepository;
        _generationRepository = generationRepository;
        _mediaStorage = mediaStorage;
        _creditService = creditService;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Tạo ảnh bằng Beeknoee API (Phương án 1 — sync-first).
    ///
    /// - Nếu model trả COMPLETED ngay: upload ảnh lên Cloudinary, trừ credit, trả URL.
    /// - Nếu model trả PROCESSING/PENDING: trả requiresAsyncHandling=true kèm jobId,
    ///   KHÔNG poll, KHÔNG treo request. Caller tự xử lý (PA2 sẽ implement sau).
    /// - Nếu FAILED hoặc lỗi HTTP: trả error rõ ràng.
    /// </summary>
    [HttpPost("generate-image")]
    [ProducesResponseType(typeof(GenericResponse<BeeknoeeGenerateImageApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericResponse<BeeknoeeGenerateImageApiResponse>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(GenericResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GenericResponse<object>), StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(typeof(GenericResponse<object>), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult> GenerateImage(
        [FromBody] BeeknoeeGenerateImageApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);
        var workspaceId = membership.WorkspaceId;
        var userId = membership.UserId;

        // ── 1. Validate content ───────────────────────────────────────────────
        var content = await _contentRepository.GetByIdAsync(request.ContentId, cancellationToken);
        if (content == null || content.WorkspaceId != workspaceId)
        {
            return BadRequest(GenericResponse<object>.CreateError(
                "Content không tồn tại hoặc không thuộc workspace này.",
                HttpStatusCode.NotFound));
        }

        // ── 2. Check credits ──────────────────────────────────────────────────
        var creditCheck = await _creditService.EnsureCreditsAvailableAsync(
            workspaceId, userId, ImageGenerationCredits, cancellationToken: cancellationToken);
        if (!creditCheck.Success)
        {
            return StatusCode(402, GenericResponse<object>.CreateError(
                creditCheck.Message ?? "Không đủ credits.",
                HttpStatusCode.PaymentRequired,
                creditCheck.Error?.ErrorCode));
        }

        // ── 3. Build prompt ───────────────────────────────────────────────────
        var prompt = string.IsNullOrWhiteSpace(request.CustomPrompt)
            ? $"Generate a professional social media image for: {content.TextContent ?? content.Title}"
            : request.CustomPrompt;

        // ── 4. Build Beeknoee request ─────────────────────────────────────────
        var model = string.IsNullOrWhiteSpace(request.Model)
            ? _settings.DefaultImageModel
            : request.Model;

        var beeknoeeRequest = new BeeknoeeImageGenerationRequest
        {
            Model = model,
            Prompt = prompt,
            Size = request.Size,
            ImageUrls = request.ReferenceImageUrls is { Count: > 0 }
                ? request.ReferenceImageUrls
                : null,
        };

        // ── 5. Create AiGeneration record (pending) ───────────────────────────
        var generation = await _generationRepository.AddAsync(new AiGeneration
        {
            ContentId = content.Id,
            AiPrompt = prompt,
            Status = AiStatusEnum.Pending,
            ProviderName = $"Beeknoee/{model}"
        }, cancellationToken);

        _logger.LogInformation(
            "[BeeknoeeMediaController] GenerateImage start. ContentId={ContentId} | Model={Model} | GenId={GenId}",
            request.ContentId, model, generation.Id);

        // ── 6. Call Beeknoee API ──────────────────────────────────────────────
        var beeknoeeResult = await _beeknoeeClient.GenerateAsync(beeknoeeRequest, cancellationToken);

        // ── 7. Handle PROCESSING/PENDING (PA1 không poll) ────────────────────
        if (beeknoeeResult.Status.Equals("PROCESSING", StringComparison.OrdinalIgnoreCase) ||
            beeknoeeResult.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase))
        {
            generation.Status = AiStatusEnum.Processing;
            generation.VideoJobId = beeknoeeResult.JobId; // reuse field để lưu job_id
            await _generationRepository.UpdateAsync(generation, cancellationToken);

            _logger.LogWarning(
                "[BeeknoeeMediaController] Model '{Model}' trả {Status} — yêu cầu xử lý bất đồng bộ (PA2). JobId={JobId} | GenId={GenId}",
                model, beeknoeeResult.Status, beeknoeeResult.JobId, generation.Id);

            // HTTP 202 Accepted — rõ ràng "đã nhận nhưng chưa xong"
            return StatusCode(202, GenericResponse<BeeknoeeGenerateImageApiResponse>.CreateSuccess(
                new BeeknoeeGenerateImageApiResponse
                {
                    AiGenerationId = generation.Id,
                    Completed = false,
                    RequiresAsyncHandling = true,
                    BeeknoeeJobId = beeknoeeResult.JobId,
                    ProviderUsed = $"Beeknoee/{model}",
                },
                $"Model '{model}' đang xử lý bất đồng bộ (status: {beeknoeeResult.Status}). " +
                "Tính năng polling (Phương án 2) sẽ được hỗ trợ trong bản tiếp theo."));
        }

        // ── 8. Handle FAILED ──────────────────────────────────────────────────
        if (beeknoeeResult.Status.Equals("FAILED", StringComparison.OrdinalIgnoreCase) ||
            beeknoeeResult.Data.Count == 0 && !beeknoeeResult.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            generation.Status = AiStatusEnum.Failed;
            generation.ErrorMessage = beeknoeeResult.ErrorMessage ?? "Beeknoee generation thất bại.";
            await _generationRepository.UpdateAsync(generation, cancellationToken);

            _logger.LogError(
                "[BeeknoeeMediaController] Generation FAILED. Model={Model} | Error={Error} | GenId={GenId}",
                model, generation.ErrorMessage, generation.Id);

            return StatusCode(502, GenericResponse<object>.CreateError(
                $"Tạo ảnh thất bại: {generation.ErrorMessage}",
                HttpStatusCode.BadGateway));
        }

        // ── 9. Handle COMPLETED — upload ảnh lên Cloudinary ─────────────────
        try
        {
            var first = beeknoeeResult.Data[0];
            string cloudinaryUrl;

            if (!string.IsNullOrWhiteSpace(first.B64Json))
            {
                // Google Gemini trả base64
                var imageBytes = Convert.FromBase64String(first.B64Json);
                var fileName = $"beeknoee-{generation.Id}.png";
                cloudinaryUrl = await _mediaStorage.UploadBytesAsync(
                    imageBytes, "ai-images", fileName, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(first.Url))
            {
                // OpenAI/khác trả URL — download rồi upload
                using var http = new HttpClient();
                var imageBytes = await http.GetByteArrayAsync(first.Url, cancellationToken);
                var ext = first.Url.Contains(".webp") ? "webp" : "png";
                var fileName = $"beeknoee-{generation.Id}.{ext}";
                cloudinaryUrl = await _mediaStorage.UploadBytesAsync(
                    imageBytes, "ai-images", fileName, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("COMPLETED nhưng không có url hoặc b64_json.");
            }

            // ── 10. Update generation record ──────────────────────────────────
            generation.GeneratedImageUrl = cloudinaryUrl;
            generation.Status = AiStatusEnum.Completed;
            await _generationRepository.UpdateAsync(generation, cancellationToken);

            // ── 11. Deduct credits ────────────────────────────────────────────
            await _creditService.ConsumeCreditsAsync(
                workspaceId, userId,
                CreditActionEnum.GenerateImage,
                ImageGenerationCredits,
                generation.Id,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "[BeeknoeeMediaController] ✅ SUCCESS. Model={Model} | CostVnd={Cost} | GenId={GenId} | Url={Url}",
                model, beeknoeeResult.CostVnd, generation.Id, cloudinaryUrl);

            return Ok(GenericResponse<BeeknoeeGenerateImageApiResponse>.CreateSuccess(
                new BeeknoeeGenerateImageApiResponse
                {
                    AiGenerationId = generation.Id,
                    ImageUrl = cloudinaryUrl,
                    Completed = true,
                    RequiresAsyncHandling = false,
                    ProviderUsed = $"Beeknoee/{model}",
                    CostVnd = beeknoeeResult.CostVnd,
                },
                "Tạo ảnh thành công."));
        }
        catch (Exception ex)
        {
            generation.Status = AiStatusEnum.Failed;
            generation.ErrorMessage = ex.Message;
            await _generationRepository.UpdateAsync(generation, cancellationToken);

            _logger.LogError(ex,
                "[BeeknoeeMediaController] Upload hoặc decode ảnh thất bại. GenId={GenId}", generation.Id);

            return StatusCode(500, GenericResponse<object>.CreateError(
                "Không thể upload ảnh sau khi tạo. Vui lòng thử lại.",
                HttpStatusCode.InternalServerError));
        }
    }

    // ── Status Endpoint (Phase E) ─────────────────────────────────────────────

    /// <summary>
    /// Kiểm tra trạng thái một AiGeneration do Beeknoee tạo.
    ///
    /// Dùng sau khi nhận HTTP 202 từ generate-image (khi model trả PROCESSING).
    /// Background service tự động poll Beeknoee và cập nhật DB — endpoint này chỉ đọc DB,
    /// KHÔNG trigger poll mới.
    /// </summary>
    [HttpGet("generation/{aiGenerationId:guid}")]
    [ProducesResponseType(typeof(GenericResponse<BeeknoeeGenerateImageApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GenericResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetGenerationStatus(
        Guid aiGenerationId,
        CancellationToken cancellationToken = default)
    {
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(HttpContext);

        var generation = await _generationRepository.GetByIdAsync(aiGenerationId, cancellationToken);
        if (generation == null || generation.Content?.WorkspaceId != membership.WorkspaceId)
        {
            return NotFound(GenericResponse<object>.CreateError(
                "AiGeneration không tồn tại hoặc không thuộc workspace này.",
                HttpStatusCode.NotFound));
        }

        var response = new BeeknoeeGenerateImageApiResponse
        {
            AiGenerationId = generation.Id,
            ImageUrl = generation.GeneratedImageUrl,
            Completed = generation.Status == AiStatusEnum.Completed,
            RequiresAsyncHandling = generation.Status == AiStatusEnum.Processing,
            BeeknoeeJobId = generation.VideoJobId,
            ProviderUsed = generation.ProviderName,
            ErrorMessage = generation.Status == AiStatusEnum.Failed ? generation.ErrorMessage : null,
        };

        var message = generation.Status switch
        {
            AiStatusEnum.Completed => "Ảnh đã hoàn thành.",
            AiStatusEnum.Processing => "Đang xử lý — background service đang poll Beeknoee (30s interval).",
            AiStatusEnum.Pending => "Đang chờ xử lý.",
            AiStatusEnum.Failed => $"Tạo ảnh thất bại: {generation.ErrorMessage}",
            _ => $"Trạng thái: {generation.Status}"
        };

        return Ok(GenericResponse<BeeknoeeGenerateImageApiResponse>.CreateSuccess(response, message));
    }
}

