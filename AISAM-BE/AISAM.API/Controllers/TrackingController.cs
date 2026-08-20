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
    private readonly IConfiguration _configuration;

    public TrackingController(
        IPerformanceReportRepository performanceReportRepository,
        ILogger<TrackingController> logger,
        IConfiguration configuration)
    {
        _performanceReportRepository = performanceReportRepository;
        _logger = logger;
        _configuration = configuration;
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
            return BadRequest("Invalid or unapproved target URL.");

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

    private bool IsSafeRedirectUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return false;
            
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var allowedDomains = _configuration.GetSection("Tracking:AllowedDomains").Get<string[]>();
        if (allowedDomains != null && allowedDomains.Length > 0)
        {
            return allowedDomains.Contains(uri.Host, StringComparer.OrdinalIgnoreCase);
        }

        // Fallback to checking against a static safe list or allow if no strict list is configured
        // In a real SMB platform, we might allow any domain but check against Google Safe Browsing API.
        return true;
    }
}
