using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

/// <summary>
/// IAIImageProvider implementation dùng Beeknoee API — Phương án 1 (sync-first).
///
/// Nguyên tắc:
/// - Nếu Beeknoee trả COMPLETED ngay → xử lý và trả AIMediaResult.OkBytes/OkUrl.
/// - Nếu Beeknoee trả PROCESSING/PENDING → trả AIMediaResult.Async(...) ngay lập tức,
///   KHÔNG poll, KHÔNG chờ, nhường lại cho Phương án 2.
/// - Nếu FAILED hoặc HTTP error → trả AIMediaResult.Fail(...) với thông báo rõ ràng.
///
/// Code này hoàn toàn tách biệt với FallbackImageProvider (DeAPI/OpenRouter/HuggingFace),
/// không thay thế provider cũ, có thể coexist song song.
/// </summary>
public sealed class BeeknoeeImageProvider : IAIImageProvider
{
    private readonly IBeeknoeeSyncImageClient _client;
    private readonly BeeknoeeSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BeeknoeeImageProvider> _logger;

    public string ProviderName => "Beeknoee";

    public BeeknoeeImageProvider(
        IBeeknoeeSyncImageClient client,
        IOptions<BeeknoeeSettings> settings,
        HttpClient httpClient,
        ILogger<BeeknoeeImageProvider> logger)
    {
        _client = client;
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AIMediaResult> GenerateImageAsync(
        string prompt,
        ImageGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[BeeknoeeImageProvider] GenerateImageAsync start. Model={Model} | Size={W}x{H} | ReferenceUrls={RefCount}",
            _settings.DefaultImageModel,
            options?.Width ?? 1024,
            options?.Height ?? 1024,
            options?.ReferenceImageUrls?.Count ?? 0);

        // Validate API key trước khi gọi
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogError("[BeeknoeeImageProvider] BEEKNOEE_API_KEY chưa được cấu hình.");
            return AIMediaResult.Fail(
                "Beeknoee API key chưa được cấu hình. Đặt biến môi trường BEEKNOEE_API_KEY.",
                ProviderName);
        }

        // Chọn giữa image generation và image edit dựa vào reference URLs
        var referenceUrls = options?.ReferenceImageUrls?
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList() ?? new List<string>();

        // Với model gemini và reference URLs → dùng image_urls trong JSON body (không dùng /edits)
        var request = new BeeknoeeImageGenerationRequest
        {
            Model = _settings.DefaultImageModel,
            Prompt = prompt,
            Size = BuildSizeString(options),
            ImageUrls = referenceUrls.Count > 0 ? referenceUrls : null,
        };

        var result = await _client.GenerateAsync(request, cancellationToken);

        return MapResult(result);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Map BeeknoeeSyncImageResult → AIMediaResult theo trạng thái.
    /// Đây là điểm duy nhất xử lý logic COMPLETED/PROCESSING/FAILED.
    /// </summary>
    private AIMediaResult MapResult(BeeknoeeSyncImageResult result)
    {
        var status = result.Status.ToUpperInvariant();

        switch (status)
        {
            case "COMPLETED":
                return HandleCompleted(result);

            case "PROCESSING":
            case "PENDING":
                // PA1 không poll — báo rõ cho caller để có thể dùng PA2 sau
                _logger.LogWarning(
                    "[BeeknoeeImageProvider] Model trả {Status} — yêu cầu xử lý bất đồng bộ (Phương án 2). JobId={JobId}",
                    status, result.JobId ?? "(none)");
                return AIMediaResult.Async(result.JobId ?? string.Empty, ProviderName);

            case "FAILED":
                _logger.LogError(
                    "[BeeknoeeImageProvider] Generation FAILED. Error={Error}",
                    result.ErrorMessage);
                return AIMediaResult.Fail(
                    result.ErrorMessage ?? "Beeknoee generation thất bại.",
                    ProviderName);

            default:
                // Status không xác định — cố gắng extract ảnh, nếu không có thì báo lỗi
                _logger.LogWarning(
                    "[BeeknoeeImageProvider] Unknown status '{Status}' — attempting to extract image data.",
                    result.Status);
                return result.Data.Count > 0
                    ? HandleCompleted(result)
                    : AIMediaResult.Fail($"Beeknoee trả status không xác định: '{result.Status}'", ProviderName);
        }
    }

    /// <summary>
    /// Xử lý kết quả COMPLETED: ưu tiên base64 (Google Gemini) → URL.
    /// Download URL về bytes nếu cần để upload Cloudinary.
    /// </summary>
    private AIMediaResult HandleCompleted(BeeknoeeSyncImageResult result)
    {
        if (result.Data.Count == 0)
        {
            _logger.LogError("[BeeknoeeImageProvider] COMPLETED nhưng data[] rỗng.");
            return AIMediaResult.Fail("Beeknoee COMPLETED nhưng không có dữ liệu ảnh.", ProviderName);
        }

        var first = result.Data[0];

        // Ưu tiên base64 (Google Gemini trả b64_json, không trả URL)
        if (!string.IsNullOrWhiteSpace(first.B64Json))
        {
            try
            {
                var bytes = Convert.FromBase64String(first.B64Json);
                _logger.LogInformation(
                    "[BeeknoeeImageProvider] ✅ SUCCESS (base64). Size={Size} bytes | CostVnd={Cost}",
                    bytes.Length, result.CostVnd);
                return AIMediaResult.OkBytes(bytes, ProviderName);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "[BeeknoeeImageProvider] Failed to decode base64 image.");
                return AIMediaResult.Fail("Không thể decode base64 ảnh từ Beeknoee.", ProviderName);
            }
        }

        // URL — trả thẳng về để caller upload Cloudinary
        if (!string.IsNullOrWhiteSpace(first.Url))
        {
            _logger.LogInformation(
                "[BeeknoeeImageProvider] ✅ SUCCESS (URL). CostVnd={Cost}",
                result.CostVnd);
            return AIMediaResult.OkUrl(first.Url, ProviderName);
        }

        _logger.LogError("[BeeknoeeImageProvider] COMPLETED nhưng data[0] không có url lẫn b64_json.");
        return AIMediaResult.Fail("Beeknoee COMPLETED nhưng không có url hoặc b64_json trong data[0].", ProviderName);
    }

    /// <summary>Chuyển Width/Height từ ImageGenerationOptions sang chuỗi size Beeknoee (VD: "1024x1024").</summary>
    private static string? BuildSizeString(ImageGenerationOptions? options)
    {
        if (options == null) return null;
        if (options.Width <= 0 || options.Height <= 0) return null;
        // Mặc định 1024x1024 → không cần gửi (để server dùng default)
        if (options.Width == 1024 && options.Height == 1024) return null;
        return $"{options.Width}x{options.Height}";
    }
}
