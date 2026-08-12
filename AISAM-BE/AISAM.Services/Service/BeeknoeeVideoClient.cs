using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AISAM.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.Services.Service;

// ── DTOs ─────────────────────────────────────────────────────────────────────

/// <summary>Request body cho POST /v1/video/generations.</summary>
public sealed class BeeknoeeVideoRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "veo-3.1-fast-generate-preview";

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("negative_prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NegativePrompt { get; set; }

    [JsonPropertyName("aspect_ratio")]
    public string AspectRatio { get; set; } = "16:9";

    [JsonPropertyName("duration")]
    public string Duration { get; set; } = "8";

    [JsonPropertyName("resolution")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Resolution { get; set; }

    [JsonPropertyName("person_generation")]
    public string PersonGeneration { get; set; } = "allow_adult";

    /// <summary>Base64 ảnh — frame đầu tiên (image-to-video). Null = text-to-video.</summary>
    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image { get; set; }

    /// <summary>Base64 ảnh — frame cuối (dual frame control).</summary>
    [JsonPropertyName("last_frame")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastFrame { get; set; }

    /// <summary>Mảng base64 ảnh tham chiếu style (tối đa 3). Chỉ Veo.</summary>
    [JsonPropertyName("reference_images")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? ReferenceImages { get; set; }
}

/// <summary>
/// Response từ Beeknoee video API — dùng cho cả:
/// - POST /v1/video/generations (khi submit: chỉ có JobId + Status)
/// - GET  /v1/video/generations/{id} (khi poll: đầy đủ fields)
/// </summary>
public sealed class BeeknoeeVideoStatusResult
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    /// <summary>PENDING · PROCESSING · COMPLETED · FAILED · TIMEOUT</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// Relative path download proxy của Beeknoee (cần Authorization header).
    /// VD: "/v1/video/generations/abc-123/download"
    /// </summary>
    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }

    /// <summary>
    /// URL gốc từ provider — có thể public (một số Veo) hoặc null.
    /// Dùng cái này trước khi fallback sang video_url.
    /// </summary>
    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("cost_vnd")]
    public int? CostVnd { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("estimated_time_seconds")]
    public int? EstimatedTimeSeconds { get; set; }

    [JsonPropertyName("elapsed_seconds")]
    public int? ElapsedSeconds { get; set; }

    [JsonPropertyName("poll_count")]
    public int? PollCount { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    // ── Computed helpers ──────────────────────────────────────────────────────
    [JsonIgnore]
    public bool IsSuccess => Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase);
    [JsonIgnore]
    public bool IsFailed => Status.Equals("FAILED", StringComparison.OrdinalIgnoreCase)
                         || Status.Equals("TIMEOUT", StringComparison.OrdinalIgnoreCase);
    [JsonIgnore]
    public bool IsPending => Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase)
                          || Status.Equals("PROCESSING", StringComparison.OrdinalIgnoreCase);

    /// <summary>Tạo instance lỗi khi không parse được response.</summary>
    public static BeeknoeeVideoStatusResult Error(string message) => new()
    {
        Status = "FAILED",
        ErrorMessage = message
    };
}

// ── HTTP Client ───────────────────────────────────────────────────────────────

/// <summary>
/// HTTP client thuần cho Beeknoee Video API.
/// Không tự poll, không tự retry.
/// Inject qua DI — dùng HttpClient với timeout ngắn cho mỗi request.
/// </summary>
public sealed class BeeknoeeVideoClient
{
    private readonly HttpClient _http;
    private readonly BeeknoeeSettings _settings;
    private readonly ILogger<BeeknoeeVideoClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public BeeknoeeVideoClient(
        HttpClient http,
        IOptions<BeeknoeeSettings> options,
        ILogger<BeeknoeeVideoClient> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// POST /v1/video/generations — Submit video generation job.
    /// Trả về { job_id, status: "PENDING" } ngay lập tức.
    /// </summary>
    public async Task<BeeknoeeVideoStatusResult> StartAsync(
        BeeknoeeVideoRequest request,
        CancellationToken ct = default)
    {
        var url = BuildUrl("/v1/video/generations");
        _logger.LogInformation(
            "[BeeknoeeVideo] POST {Url} | model={Model} | duration={Duration}s | ar={AR}",
            url, request.Model, request.Duration, request.AspectRatio);

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        AddAuthHeader(httpRequest);

        return await SendAndParseAsync(httpRequest, ct);
    }

    /// <summary>
    /// GET /v1/video/generations/{beeknoeeJobId} — Poll trạng thái job.
    /// Gọi bởi <see cref="VideoGenerationBackgroundService"/> mỗi interval.
    /// </summary>
    public async Task<BeeknoeeVideoStatusResult> GetStatusAsync(
        string beeknoeeJobId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(beeknoeeJobId))
            return BeeknoeeVideoStatusResult.Error("BeeknoeeJobId rỗng.");

        var url = BuildUrl($"/v1/video/generations/{beeknoeeJobId}");
        _logger.LogDebug("[BeeknoeeVideo] GET {Url} (poll status)", url);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthHeader(httpRequest);

        return await SendAndParseAsync(httpRequest, ct);
    }

    /// <summary>
    /// GET /v1/video/generations/{beeknoeeJobId}/download — Tải video bytes.
    /// Dùng khi source_url null (cần Authorization header — Beeknoee proxy).
    /// </summary>
    public async Task<byte[]> DownloadVideoAsync(
        string beeknoeeJobId,
        CancellationToken ct = default)
    {
        var url = BuildUrl($"/v1/video/generations/{beeknoeeJobId}/download");
        _logger.LogInformation("[BeeknoeeVideo] GET {Url} (download video bytes)", url);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthHeader(httpRequest);

        // Dùng timeout lớn hơn để download file lớn
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromMinutes(5));

        var response = await _http.SendAsync(httpRequest, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Beeknoee download failed: {response.StatusCode} — {errorBody}");
        }

        return await response.Content.ReadAsByteArrayAsync(cts.Token);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private string BuildUrl(string path) =>
        $"{_settings.BaseUrl.TrimEnd('/')}{path}";

    private void AddAuthHeader(HttpRequestMessage request) =>
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

    private async Task<BeeknoeeVideoStatusResult> SendAndParseAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
        try
        {
            var response = await _http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            _logger.LogDebug(
                "[BeeknoeeVideo] HTTP {StatusCode} | Body={Body}",
                (int)response.StatusCode,
                body.Length > 500 ? body[..500] + "..." : body);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[BeeknoeeVideo] Request failed. HTTP {StatusCode}: {Body}",
                    (int)response.StatusCode, body);
                return BeeknoeeVideoStatusResult.Error(
                    $"Beeknoee HTTP {(int)response.StatusCode}: {body}");
            }

            var result = JsonSerializer.Deserialize<BeeknoeeVideoStatusResult>(body, JsonOptions);
            if (result == null)
                return BeeknoeeVideoStatusResult.Error("Beeknoee trả về response rỗng (null).");

            _logger.LogInformation(
                "[BeeknoeeVideo] JobId={JobId} | Status={Status} | Cost={Cost}đ",
                result.JobId, result.Status, result.CostVnd ?? 0);

            return result;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError("[BeeknoeeVideo] Request timeout: {Message}", ex.Message);
            return BeeknoeeVideoStatusResult.Error("Request timeout khi gọi Beeknoee Video API.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BeeknoeeVideo] Unexpected error.");
            return BeeknoeeVideoStatusResult.Error($"Lỗi không mong đợi: {ex.Message}");
        }
    }
}
