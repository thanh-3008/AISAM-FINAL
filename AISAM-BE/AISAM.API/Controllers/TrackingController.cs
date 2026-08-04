using System.Text;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/t")]
public sealed class TrackingController : ControllerBase
{
    private readonly IPerformanceReportRepository _performanceReportRepository;
    private readonly ILogger<TrackingController> _logger;

    public TrackingController(
        IPerformanceReportRepository performanceReportRepository,
        ILogger<TrackingController> logger)
    {
        _performanceReportRepository = performanceReportRepository;
        _logger = logger;
    }

    [HttpGet("c/{contentId:guid}/i/{integrationId:guid}")]
    public async Task<IActionResult> TrackContentClick(
        Guid contentId,
        Guid integrationId,
        [FromQuery(Name = "u")] string encodedUrl,
        CancellationToken cancellationToken)
    {
        var targetUrl = DecodeTargetUrl(encodedUrl);
        if (!IsSafeRedirectUrl(targetUrl))
            return BadRequest("Invalid target URL.");

        var updated = await _performanceReportRepository.IncrementTrackedClickAsync(contentId, integrationId, cancellationToken);
        if (!updated)
        {
            _logger.LogWarning(
                "Tracked click could not be linked to a post. ContentId={ContentId}, IntegrationId={IntegrationId}",
                contentId,
                integrationId);
        }

        return Redirect(targetUrl!);
    }

    private static string? DecodeTargetUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
            return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSafeRedirectUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
