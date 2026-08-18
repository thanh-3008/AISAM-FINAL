using AISAM.Data;
using AISAM.Repositories;
using AISAM.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin-ops")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(IgnoreApi = true)] // Ẩn khỏi Swagger
public class AdminOpsController : ControllerBase
{
    private readonly AisamContext _context;

    public AdminOpsController(AisamContext context)
    {
        _context = context;
    }

    [HttpPost("backfill-reach")]
    public async Task<IActionResult> BackfillReach(int batchSize = 500, CancellationToken cancellationToken = default)
    {
        var processed = 0;
        var updated = 0;

        while (true)
        {
            var reports = await _context.PerformanceReports
                .Where(pr => pr.Reach == 0 && pr.RawData != null)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (reports.Count == 0)
                break;

            foreach (var report in reports)
            {
                var reach = ExtractReach(report.RawData);
                if (reach > 0)
                {
                    report.Reach = reach;
                    updated++;
                }
                else
                {
                    // Đánh dấu là đã xử lý nhưng không có reach bằng cách set Reach thành -1 (nếu muốn) 
                    // hoặc tạm bỏ qua. Ở đây ta giữ nguyên nhưng cần cẩn thận infinite loop nếu Reach cứ bằng 0.
                    // Để tránh infinite loop với những report THỰC SỰ có reach = 0:
                    // Ta cần một flag hoặc chỉ query RawData có chứa "reach"
                    report.Reach = -1; // Tạm dùng -1 để đánh dấu đã quét qua
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            processed += reports.Count;
        }

        // Chuyển các Reach = -1 về lại 0
        await _context.Database.ExecuteSqlRawAsync("UPDATE performance_reports SET reach = 0 WHERE reach = -1", cancellationToken);

        return Ok(new { Processed = processed, Updated = updated });
    }

    private static long ExtractReach(string? rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData)) return 0;
        try
        {
            using var doc = JsonDocument.Parse(rawData);
            return doc.RootElement.TryGetProperty("reach", out var prop) && prop.ValueKind == JsonValueKind.Number
                ? prop.GetInt64()
                : 0;
        }
        catch
        {
            return 0;
        }
    }
}
