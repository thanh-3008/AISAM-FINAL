using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;

/// <summary>
/// Background service tự động poll trạng thái ảnh Beeknoee đang PROCESSING/PENDING.
///
/// Cách hoạt động:
/// - Mỗi 30s (có job) / 120s (rảnh): query DB tìm AiGeneration có
///   Status=Processing, ProviderName bắt đầu bằng "Beeknoee/", VideoJobId != null.
/// - Với mỗi job: gọi GET /v1/image/generations/{jobId} trên Beeknoee.
///   - COMPLETED → download/decode ảnh → upload Cloudinary → trừ credit → DB Completed.
///   - PROCESSING/PENDING → bỏ qua, chờ vòng tiếp.
///   - FAILED / hết 3 ngày lưu trữ → DB Failed.
///
/// Pattern y chang <see cref="VideoPollingBackgroundService"/> — dùng IServiceScopeFactory
/// để resolve scoped services trong hosted service singleton.
/// </summary>
public sealed class BeeknoeeImagePollingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BeeknoeeImagePollingBackgroundService> _logger;

    // Beeknoee lưu ảnh 3 ngày — sau đó job không còn truy cập được
    private static readonly TimeSpan MaxJobAge = TimeSpan.FromDays(3);

    public BeeknoeeImagePollingBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BeeknoeeImagePollingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[BeeknoeeImagePolling] Background service STARTED.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var hasJobs = false;
            try
            {
                hasJobs = await PollAllPendingJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BeeknoeeImagePolling] Unhandled error in polling iteration.");
            }

            try
            {
                // Backoff: 30s khi có job đang chờ, 120s khi rảnh
                var delay = hasJobs ? 30 : 120;
                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("[BeeknoeeImagePolling] Background service STOPPED.");
    }

    private async Task<bool> PollAllPendingJobsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AisamContext>();
        var beeknoeeClient = scope.ServiceProvider.GetRequiredService<IBeeknoeeSyncImageClient>();
        var mediaStorage = scope.ServiceProvider.GetRequiredService<IMediaStorageService>();
        var creditService = scope.ServiceProvider.GetRequiredService<ICreditService>();

        // Query: AiGeneration đang chờ Beeknoee hoàn thành
        var jobs = await db.AiGenerations
            .Include(g => g.Content)
                .ThenInclude(c => c.Profile)
            .Where(g => g.Status == AiStatusEnum.Processing
                        && g.VideoJobId != null          // field tái sử dụng lưu beeknoee job_id
                        && g.ProviderName != null && g.ProviderName.StartsWith("Beeknoee/")
                        && g.GeneratedImageUrl == null)  // chưa có ảnh
            .ToListAsync(ct);

        if (jobs.Count == 0)
        {
            _logger.LogDebug("[BeeknoeeImagePolling] No pending Beeknoee image jobs.");
            return false;
        }

        _logger.LogInformation("[BeeknoeeImagePolling] Found {Count} pending Beeknoee image jobs.", jobs.Count);

        var now = DateTime.UtcNow;

        foreach (var job in jobs)
        {
            try
            {
                // Kiểm tra quá 3 ngày (Beeknoee không lưu ảnh lâu hơn)
                if (now - job.CreatedAt > MaxJobAge)
                {
                    _logger.LogWarning(
                        "[BeeknoeeImagePolling] Job {GenId} / Beeknoee JobId={JobId} hết hạn 3 ngày — đánh dấu Failed.",
                        job.Id, job.VideoJobId);
                    job.Status = AiStatusEnum.Failed;
                    job.ErrorMessage = "Beeknoee job hết hạn lưu trữ (>3 ngày). Vui lòng tạo ảnh mới.";
                    db.Update(job);
                    await db.SaveChangesAsync(ct);
                    continue;
                }

                await PollSingleJobAsync(job, db, beeknoeeClient, mediaStorage, creditService, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[BeeknoeeImagePolling] Failed to poll job {GenId} (BeeknoeeJobId={JobId}).",
                    job.Id, job.VideoJobId);
            }
        }

        return true;
    }

    private async Task PollSingleJobAsync(
        Data.Model.AiGeneration job,
        AisamContext db,
        IBeeknoeeSyncImageClient beeknoeeClient,
        IMediaStorageService mediaStorage,
        ICreditService creditService,
        CancellationToken ct)
    {
        var beeknoeeJobId = job.VideoJobId!;
        _logger.LogInformation(
            "[BeeknoeeImagePolling] Polling GenId={GenId} | BeeknoeeJobId={JobId}",
            job.Id, beeknoeeJobId);

        var pollResult = await beeknoeeClient.GetJobStatusAsync(beeknoeeJobId, ct);

        var status = pollResult.Status.ToUpperInvariant();

        switch (status)
        {
            case "PROCESSING":
            case "PENDING":
                _logger.LogDebug(
                    "[BeeknoeeImagePolling] GenId={GenId} still {Status} — will retry.",
                    job.Id, status);
                return; // Chờ vòng tiếp — không update DB

            case "FAILED":
                var failMsg = pollResult.ErrorMessage ?? "Beeknoee generation thất bại (FAILED).";
                _logger.LogWarning(
                    "[BeeknoeeImagePolling] GenId={GenId} FAILED: {Error}", job.Id, failMsg);
                job.Status = AiStatusEnum.Failed;
                job.ErrorMessage = failMsg;
                db.Update(job);
                await db.SaveChangesAsync(ct);
                return;

            case "COMPLETED":
                await HandleCompletedJobAsync(job, pollResult, db, mediaStorage, creditService, ct);
                return;

            default:
                // Status không xác định — nếu có data, thử xử lý như COMPLETED
                if (pollResult.Data.Count > 0)
                {
                    _logger.LogWarning(
                        "[BeeknoeeImagePolling] GenId={GenId} unknown status '{Status}' but has data — treating as COMPLETED.",
                        job.Id, pollResult.Status);
                    await HandleCompletedJobAsync(job, pollResult, db, mediaStorage, creditService, ct);
                }
                else
                {
                    _logger.LogWarning(
                        "[BeeknoeeImagePolling] GenId={GenId} unknown status '{Status}' with no data — skipping.",
                        job.Id, pollResult.Status);
                }
                return;
        }
    }

    private async Task HandleCompletedJobAsync(
        Data.Model.AiGeneration job,
        BeeknoeeSyncImageResult pollResult,
        AisamContext db,
        IMediaStorageService mediaStorage,
        ICreditService creditService,
        CancellationToken ct)
    {
        if (pollResult.Data.Count == 0)
        {
            _logger.LogError(
                "[BeeknoeeImagePolling] GenId={GenId} COMPLETED but data[] empty — marking Failed.",
                job.Id);
            job.Status = AiStatusEnum.Failed;
            job.ErrorMessage = "Beeknoee COMPLETED nhưng không có dữ liệu ảnh (data[] rỗng).";
            db.Update(job);
            await db.SaveChangesAsync(ct);
            return;
        }

        var first = pollResult.Data[0];
        string cloudinaryUrl;

        try
        {
            if (!string.IsNullOrWhiteSpace(first.B64Json))
            {
                // Google Gemini trả base64
                var imageBytes = Convert.FromBase64String(first.B64Json);
                var fileName = $"beeknoee-{job.Id}.png";
                cloudinaryUrl = await mediaStorage.UploadBytesAsync(imageBytes, "ai-images", fileName, ct);
            }
            else if (!string.IsNullOrWhiteSpace(first.Url))
            {
                // OpenAI/khác trả URL — download rồi upload Cloudinary
                using var http = new System.Net.Http.HttpClient();
                var imageBytes = await http.GetByteArrayAsync(first.Url, ct);
                var ext = first.Url.Contains(".webp", StringComparison.OrdinalIgnoreCase) ? "webp" : "png";
                var fileName = $"beeknoee-{job.Id}.{ext}";
                cloudinaryUrl = await mediaStorage.UploadBytesAsync(imageBytes, "ai-images", fileName, ct);
            }
            else
            {
                _logger.LogError(
                    "[BeeknoeeImagePolling] GenId={GenId} COMPLETED but data[0] has no url or b64_json.",
                    job.Id);
                job.Status = AiStatusEnum.Failed;
                job.ErrorMessage = "Beeknoee COMPLETED nhưng data[0] không có url lẫn b64_json.";
                db.Update(job);
                await db.SaveChangesAsync(ct);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[BeeknoeeImagePolling] GenId={GenId} failed to download/upload image from Beeknoee.",
                job.Id);
            job.Status = AiStatusEnum.Failed;
            job.ErrorMessage = $"Upload ảnh từ Beeknoee thất bại: {ex.Message}";
            db.Update(job);
            await db.SaveChangesAsync(ct);
            return;
        }

        // Cập nhật DB
        job.GeneratedImageUrl = cloudinaryUrl;
        job.Status = AiStatusEnum.Completed;
        db.Update(job);
        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[BeeknoeeImagePolling] ✅ GenId={GenId} COMPLETED. CostVnd={Cost} | Url={Url}",
            job.Id, pollResult.CostVnd, cloudinaryUrl);

        // Trừ credit (resolve userId từ Profile hoặc workspace owner)
        try
        {
            var userId = await ResolveUserIdAsync(job, db, ct);
            if (userId.HasValue)
            {
                await creditService.ConsumeCreditsAsync(
                    job.Content.WorkspaceId,
                    userId.Value,
                    Data.Enumeration.CreditActionEnum.GenerateImage,
                    5, // ImageGenerationCredits
                    job.Id,
                    cancellationToken: ct);
            }
            else
            {
                _logger.LogWarning(
                    "[BeeknoeeImagePolling] Could not resolve UserId for GenId={GenId} — credit not deducted.",
                    job.Id);
            }
        }
        catch (Exception ex)
        {
            // Credit deduction failure không được làm mất kết quả ảnh đã upload
            _logger.LogError(ex,
                "[BeeknoeeImagePolling] GenId={GenId} credit deduction failed (image still saved).",
                job.Id);
        }
    }

    private static async Task<Guid?> ResolveUserIdAsync(
        Data.Model.AiGeneration job,
        AisamContext db,
        CancellationToken ct)
    {
        if (job.Content?.Profile != null)
            return job.Content.Profile.UserId;

        if (job.Content == null) return null;

        // Fallback: workspace owner
        var owner = await db.WorkspaceMembers
            .Where(m => m.WorkspaceId == job.Content.WorkspaceId
                        && m.IsActive
                        && m.Role == Data.Enumeration.WorkspaceMemberRoleEnum.Owner)
            .OrderBy(m => m.Id)
            .FirstOrDefaultAsync(ct);

        return owner?.UserId;
    }
}
