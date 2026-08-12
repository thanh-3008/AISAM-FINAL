using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

/// <summary>
/// HTTP client cho Beeknoee Image API — Phương án 1 (sync-first).
///
/// Nguyên tắc bắt buộc:
/// - KHÔNG tự poll khi nhận PROCESSING/PENDING — trả thẳng cho caller.
/// - KHÔNG ghi log toàn bộ base64 ảnh.
/// - Ghi log: URL gọi (không có key), status nhận về, cost_vnd.
/// </summary>
public sealed class BeeknoeeSyncImageClient : IBeeknoeeSyncImageClient
{
    private readonly HttpClient _http;
    private readonly BeeknoeeSettings _settings;
    private readonly ILogger<BeeknoeeSyncImageClient> _logger;

    // JSON options tái sử dụng — camelCase + ignore null
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public BeeknoeeSyncImageClient(
        HttpClient http,
        IOptions<BeeknoeeSettings> settings,
        ILogger<BeeknoeeSyncImageClient> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task<BeeknoeeSyncImageResult> GenerateAsync(
        BeeknoeeImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("/v1/image/generations");
        _logger.LogInformation(
            "[Beeknoee] POST {Url} | model={Model} | size={Size} | imageRefs={ImageRefCount}",
            url, request.Model, request.Size ?? "(default)",
            request.ImageUrls?.Count ?? 0);

        // Serialize payload — snake_case theo Beeknoee doc
        var payload = BuildGenerationPayload(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        AddAuthHeader(httpRequest);
        httpRequest.Content = JsonContent.Create(payload, options: JsonOpts);

        return await SendAndParseAsync(httpRequest, cancellationToken);
    }

    public async Task<BeeknoeeSyncImageResult> EditAsync(
        BeeknoeeImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("/v1/image/edits");
        _logger.LogInformation(
            "[Beeknoee] POST {Url} (edit/multipart) | model={Model} | size={Size}",
            url, request.Model, request.Size ?? "(default)");

        var form = BuildEditForm(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        AddAuthHeader(httpRequest);
        httpRequest.Content = form;

        return await SendAndParseAsync(httpRequest, cancellationToken);
    }

    public async Task<BeeknoeeSyncImageResult> GetJobStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return new BeeknoeeSyncImageResult
            {
                Status = "FAILED",
                ErrorMessage = "JobId rỗng — không thể poll status.",
                HttpStatusCode = 0
            };
        }

        // Beeknoee alias chuẩn OpenAI: GET /v1/image/generations/{id}
        var url = BuildUrl($"/v1/image/generations/{jobId}");
        _logger.LogDebug("[Beeknoee] GET {Url} (poll job status)", url);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthHeader(httpRequest);

        return await SendAndParseAsync(httpRequest, cancellationToken);
    }


    // ── Private helpers ───────────────────────────────────────────────────────

    private string BuildUrl(string path)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        return $"{baseUrl}{path}";
    }

    private void AddAuthHeader(HttpRequestMessage req)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogWarning("[Beeknoee] BEEKNOEE_API_KEY is not configured.");
        }
        req.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    /// <summary>Xây dựng JSON payload cho /v1/image/generations (snake_case).</summary>
    private static Dictionary<string, object?> BuildGenerationPayload(BeeknoeeImageGenerationRequest req)
    {
        var payload = new Dictionary<string, object?>
        {
            ["model"] = req.Model,
            ["prompt"] = req.Prompt,
        };

        if (!string.IsNullOrWhiteSpace(req.Size))
            payload["size"] = req.Size;

        if (!string.IsNullOrWhiteSpace(req.Quality))
            payload["quality"] = req.Quality;

        if (!string.IsNullOrWhiteSpace(req.Resolution))
            payload["resolution"] = req.Resolution;

        if (req.ImageUrls is { Count: > 0 })
            payload["image_urls"] = req.ImageUrls;

        return payload;
    }

    /// <summary>Xây dựng multipart form cho /v1/image/edits.</summary>
    private static MultipartFormDataContent BuildEditForm(BeeknoeeImageEditRequest req)
    {
        var form = new MultipartFormDataContent();

        form.Add(new StringContent(req.Model), "model");
        form.Add(new StringContent(req.Prompt), "prompt");

        if (!string.IsNullOrWhiteSpace(req.Size))
            form.Add(new StringContent(req.Size), "size");

        if (!string.IsNullOrWhiteSpace(req.Quality))
            form.Add(new StringContent(req.Quality), "quality");

        var imageContent = new ByteArrayContent(req.ImageBytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(req.ImageMimeType);
        form.Add(imageContent, "image", req.ImageFileName);

        if (req.MaskBytes is { Length: > 0 })
        {
            var maskContent = new ByteArrayContent(req.MaskBytes);
            maskContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            form.Add(maskContent, "mask", "mask.png");
        }

        return form;
    }

    /// <summary>Gửi request và parse response thành BeeknoeeSyncImageResult.</summary>
    private async Task<BeeknoeeSyncImageResult> SendAndParseAsync(
        HttpRequestMessage req,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _http.SendAsync(req, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var httpCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = ExtractErrorMessage(body) ?? $"HTTP {httpCode}";
                _logger.LogWarning(
                    "[Beeknoee] Request failed. HTTP={Code} | Error={Error}",
                    httpCode, errorMsg);

                return new BeeknoeeSyncImageResult
                {
                    Status = "FAILED",
                    HttpStatusCode = httpCode,
                    ErrorMessage = httpCode switch
                    {
                        401 => "Beeknoee API key không hợp lệ hoặc hết hạn (HTTP 401). Kiểm tra BEEKNOEE_API_KEY.",
                        429 => $"Beeknoee rate limit vượt quá (HTTP 429). {errorMsg}",
                        400 => $"Request không hợp lệ (HTTP 400): {errorMsg}",
                        _ => $"Beeknoee API lỗi (HTTP {httpCode}): {errorMsg}"
                    }
                };
            }

            return ParseSuccessBody(body, httpCode);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("[Beeknoee] Request cancelled by caller.");
            return new BeeknoeeSyncImageResult { Status = "FAILED", ErrorMessage = "Request bị huỷ.", HttpStatusCode = 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Beeknoee] Unexpected exception sending image request.");
            return new BeeknoeeSyncImageResult { Status = "FAILED", ErrorMessage = ex.Message, HttpStatusCode = 0 };
        }
    }

    /// <summary>
    /// Parse JSON response thành BeeknoeeSyncImageResult.
    /// Hỗ trợ 2 format: OpenAI-compatible (data[]) và Beeknoee extension (status, job_id, cost_vnd).
    /// </summary>
    private BeeknoeeSyncImageResult ParseSuccessBody(string body, int httpCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var result = new BeeknoeeSyncImageResult { HttpStatusCode = httpCode };

            // ── status field (Beeknoee extension) ────────────────────────────
            result.Status = root.TryGetProperty("status", out var statusEl)
                ? statusEl.GetString() ?? "COMPLETED"
                : "COMPLETED"; // Nếu không có status field → coi là COMPLETED (OpenAI-compat)

            // ── job_id (có khi PROCESSING/PENDING) ───────────────────────────
            if (root.TryGetProperty("job_id", out var jobIdEl))
                result.JobId = jobIdEl.GetString();

            // ── cost_vnd (Beeknoee extension) ────────────────────────────────
            if (root.TryGetProperty("cost_vnd", out var costEl) &&
                costEl.ValueKind == JsonValueKind.Number)
            {
                result.CostVnd = costEl.GetDecimal();
                _logger.LogInformation("[Beeknoee] Generation cost: {CostVnd} VNĐ | status={Status} | job_id={JobId}",
                    result.CostVnd, result.Status, result.JobId ?? "(none)");
            }
            else
            {
                _logger.LogInformation("[Beeknoee] Generation status={Status} | job_id={JobId}",
                    result.Status, result.JobId ?? "(none)");
            }

            // ── data[] array (ảnh kết quả) ────────────────────────────────────
            if (root.TryGetProperty("data", out var dataEl) &&
                dataEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataEl.EnumerateArray())
                {
                    var dataItem = new BeeknoeeImageDataItem();

                    if (item.TryGetProperty("url", out var urlEl))
                        dataItem.Url = urlEl.GetString();
                    else if (item.TryGetProperty("b64_json", out var b64El))
                        dataItem.B64Json = b64El.GetString();

                    // Download URL dạng relative path → ghép domain
                    if (!string.IsNullOrEmpty(dataItem.Url) &&
                        dataItem.Url.StartsWith('/'))
                    {
                        dataItem.Url = $"{_settings.BaseUrl.TrimEnd('/')}{dataItem.Url}";
                    }

                    result.Data.Add(dataItem);
                }
            }

            // ── Kiểm tra FAILED với error message ────────────────────────────
            if (result.Status.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = ExtractErrorMessage(body) ?? "Beeknoee generation thất bại (FAILED).";
                _logger.LogWarning("[Beeknoee] Generation FAILED: {Error}", result.ErrorMessage);
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[Beeknoee] Failed to parse response JSON. Body (first 200 chars): {Body}",
                body.Length > 200 ? body[..200] : body);
            return new BeeknoeeSyncImageResult
            {
                Status = "FAILED",
                HttpStatusCode = httpCode,
                ErrorMessage = $"Parse lỗi response Beeknoee: {ex.Message}"
            };
        }
    }

    /// <summary>Trích xuất thông báo lỗi từ JSON error body (nhiều format khác nhau).</summary>
    private static string? ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Format 1: { "error": { "message": "..." } }
            if (root.TryGetProperty("error", out var errorEl))
            {
                if (errorEl.ValueKind == JsonValueKind.Object &&
                    errorEl.TryGetProperty("message", out var msgEl))
                    return msgEl.GetString();
                if (errorEl.ValueKind == JsonValueKind.String)
                    return errorEl.GetString();
            }

            // Format 2: { "message": "..." }
            if (root.TryGetProperty("message", out var directMsgEl))
                return directMsgEl.GetString();

            // Format 3: { "detail": "..." }
            if (root.TryGetProperty("detail", out var detailEl))
                return detailEl.GetString();
        }
        catch
        {
            // Không phải JSON — trả raw body (truncated)
            return body.Length > 300 ? body[..300] : body;
        }
        return null;
    }
}
