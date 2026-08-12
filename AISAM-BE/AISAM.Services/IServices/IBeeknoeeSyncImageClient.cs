namespace AISAM.Services.IServices;

// ── Request DTOs ──────────────────────────────────────────────────────────────

/// <summary>
/// Request gửi lên Beeknoee POST /v1/image/generations (JSON body).
/// Chỉ include field thực sự dùng — không include field của model khác.
/// </summary>
public sealed class BeeknoeeImageGenerationRequest
{
    /// <summary>Model ID chính xác theo doc (VD: "gemini-3-pro-image-preview").</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Prompt mô tả ảnh cần tạo.</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Kích thước ảnh. Mặc định: "1024x1024".
    /// Google Gemini sizes: 1024x1024, 768x1024, 1024x768, 576x1024, 1024x576 (1K);
    /// 2048x2048, 1536x2048, 2048x1536, 1152x2048, 2048x1152 (2K); v.v.
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Quality cho OpenAI models: "low"/"medium"/"high"/"auto".
    /// Google Gemini bỏ qua field này.
    /// </summary>
    public string? Quality { get; set; }

    /// <summary>
    /// Mảng URL ảnh đầu vào cho image-to-image (Google Gemini, Flux 2, Seedream, Wan, Grok).
    /// Có thể là URL công khai https:// hoặc data URI base64.
    /// Tối đa 10 ảnh (theo model).
    /// </summary>
    public List<string>? ImageUrls { get; set; }

    /// <summary>
    /// Resolution — chỉ dùng cho gpt-image-2: "1K"/"2K"/"4K".
    /// Không dùng cho gemini-3-pro-image-preview.
    /// </summary>
    public string? Resolution { get; set; }
}

/// <summary>
/// Request gửi lên Beeknoee POST /v1/image/edits (multipart/form-data).
/// Dành cho OpenAI models (gpt-image-1/1.5/2, dall-e-2, ...).
/// </summary>
public sealed class BeeknoeeImageEditRequest
{
    public string Model { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Quality { get; set; }

    /// <summary>Bytes của ảnh gốc cần edit (PNG/JPEG, ≤ 4MB).</summary>
    public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
    public string ImageFileName { get; set; } = "image.png";
    public string ImageMimeType { get; set; } = "image/png";

    /// <summary>Bytes của mask PNG (vùng transparent = vùng cần sửa). Tùy chọn.</summary>
    public byte[]? MaskBytes { get; set; }
}

// ── Response DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Kết quả parse từ response Beeknoee POST /v1/image/generations.
/// Map theo cấu trúc OpenAI-compatible + Beeknoee extensions (status, job_id, cost_vnd).
/// </summary>
public sealed class BeeknoeeSyncImageResult
{
    /// <summary>"COMPLETED" | "PROCESSING" | "PENDING" | "FAILED"</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Job ID (có khi status = PROCESSING/PENDING — PA1 không poll).</summary>
    public string? JobId { get; set; }

    /// <summary>Danh sách ảnh kết quả (có data khi status = COMPLETED).</summary>
    public List<BeeknoeeImageDataItem> Data { get; set; } = new();

    /// <summary>Chi phí theo đơn vị VNĐ (Beeknoee extension field).</summary>
    public decimal? CostVnd { get; set; }

    /// <summary>Thông báo lỗi từ Beeknoee (khi status = FAILED hoặc HTTP error).</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Raw HTTP status code (để phân biệt 401, 429, 400...).</summary>
    public int HttpStatusCode { get; set; }
}

public sealed class BeeknoeeImageDataItem
{
    /// <summary>URL ảnh (OpenAI models và một số Beeknoee models).</summary>
    public string? Url { get; set; }

    /// <summary>Base64 encoded ảnh (Google Gemini models).</summary>
    public string? B64Json { get; set; }
}

// ── Client interface ──────────────────────────────────────────────────────────

/// <summary>
/// HTTP client thuần cho Beeknoee Image API — Phương án 1 (sync-first).
/// Không tự poll, không tự retry; trả nguyên kết quả từ response đầu tiên.
/// Xử lý status PROCESSING/PENDING bằng cách báo lên caller qua BeeknoeeSyncImageResult.Status.
/// </summary>
public interface IBeeknoeeSyncImageClient
{
    /// <summary>
    /// Gọi POST /v1/image/generations với JSON body.
    /// Dùng cho: Google Gemini, Flux 2, Seedream, Wan, Grok, GPT Image (text-to-image).
    /// </summary>
    Task<BeeknoeeSyncImageResult> GenerateAsync(
        BeeknoeeImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gọi POST /v1/image/edits với multipart/form-data.
    /// Dùng cho: OpenAI models (gpt-image-1/1.5/2, dall-e-2, ...) image-to-image.
    /// </summary>
    Task<BeeknoeeSyncImageResult> EditAsync(
        BeeknoeeImageEditRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gọi GET /v1/image/generations/{jobId} — kiểm tra trạng thái job đang PROCESSING.
    /// Dùng bởi <c>BeeknoeeImagePollingBackgroundService</c>.
    /// Trả status COMPLETED/PROCESSING/PENDING/FAILED cùng data nếu đã xong.
    /// </summary>
    Task<BeeknoeeSyncImageResult> GetJobStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default);
}

