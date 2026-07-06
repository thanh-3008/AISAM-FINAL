using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminAnalyticsController : ControllerBase
{
    private readonly IPerformanceReportRepository _reportRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IContentRepository _contentRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<AdminAnalyticsController> _logger;

    public AdminAnalyticsController(
        IPerformanceReportRepository reportRepository,
        IUserRepository userRepository,
        IWorkspaceRepository workspaceRepository,
        IContentRepository contentRepository,
        IPaymentRepository paymentRepository,
        ILogger<AdminAnalyticsController> logger)
    {
        _reportRepository = reportRepository;
        _userRepository = userRepository;
        _workspaceRepository = workspaceRepository;
        _contentRepository = contentRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<GenericResponse<object>>> GetOverview(
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var fromDate = from ?? now.AddDays(-30);
        var toDate = to ?? now;

        var totals = await _reportRepository.GetAllWorkspaceTotalsAsync(fromDate, toDate, cancellationToken);
        var topWorkspaces = await _reportRepository.GetWorkspaceComparisonAsync(fromDate, toDate, 10, cancellationToken);
        var topCampaigns = await _reportRepository.GetTopCampaignsAllWorkspacesAsync(fromDate, toDate, 10, cancellationToken);
        var totalUsers = await _userRepository.GetCountAsync(cancellationToken);
        var totalWorkspaces = await _workspaceRepository.GetCountAsync(cancellationToken);
        var totalContent = await _contentRepository.GetCountAsync(cancellationToken);
        var totalRevenue = await _paymentRepository.GetTotalRevenueAsync(cancellationToken);

        var result = new
        {
            Totals = totals,
            TopWorkspaces = topWorkspaces,
            TopCampaigns = topCampaigns,
            SystemStats = new
            {
                TotalUsers = totalUsers,
                TotalWorkspaces = totalWorkspaces,
                TotalContent = totalContent,
                TotalRevenue = totalRevenue
            },
            Period = new { From = fromDate, To = toDate }
        };

        return Ok(GenericResponse<object>.CreateSuccess(result));
    }

    [HttpGet("workspace-comparison")]
    public async Task<ActionResult<GenericResponse<object>>> GetWorkspaceComparison(
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] int top = 20,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var fromDate = from ?? now.AddDays(-30);
        var toDate = to ?? now;

        var comparison = await _reportRepository.GetWorkspaceComparisonAsync(fromDate, toDate, top, cancellationToken);
        return Ok(GenericResponse<object>.CreateSuccess(comparison));
    }

    [HttpGet("top-campaigns")]
    public async Task<ActionResult<GenericResponse<object>>> GetTopCampaigns(
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] int top = 20,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var fromDate = from ?? now.AddDays(-30);
        var toDate = to ?? now;

        var campaigns = await _reportRepository.GetTopCampaignsAllWorkspacesAsync(fromDate, toDate, top, cancellationToken);
        return Ok(GenericResponse<object>.CreateSuccess(campaigns));
    }

    [HttpGet("export")]
    public async Task<ActionResult> ExportReport(
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var fromDate = from ?? now.AddDays(-30);
        var toDate = to ?? now;

        var totals = await _reportRepository.GetAllWorkspaceTotalsAsync(fromDate, toDate, cancellationToken);
        var workspaceComparison = await _reportRepository.GetWorkspaceComparisonAsync(fromDate, toDate, 50, cancellationToken);
        var topCampaigns = await _reportRepository.GetTopCampaignsAllWorkspacesAsync(fromDate, toDate, 50, cancellationToken);

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Impressions,{totals.Impressions}");
        csv.AppendLine($"Total Clicks,{totals.Clicks}");
        csv.AppendLine($"CTR,{totals.Ctr:F2}%");
        csv.AppendLine($"Total Spend,{totals.Spend:C}");
        csv.AppendLine($"Total Engagement,{totals.Engagement}");
        csv.AppendLine($"Estimated Revenue,{totals.EstimatedRevenue:C}");
        csv.AppendLine($"Published Posts,{totals.PublishedPosts}");
        csv.AppendLine($"Active Campaigns,{totals.ActiveCampaigns}");
        csv.AppendLine();
        csv.AppendLine("Workspace,Posts,Campaigns,Impressions,Clicks,CTR,Spend,Engagement,Revenue,ROAS");
        foreach (var w in workspaceComparison)
        {
            csv.AppendLine($"{EscapeCsv(w.WorkspaceName)},{w.PublishedPosts},{w.ActiveCampaigns},{w.Impressions},{w.Clicks},{w.Ctr:F2}%,{w.Spend},{w.Engagement},{w.EstimatedRevenue},{w.Roas:F2}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"admin-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.csv");
    }

    private static string EscapeCsv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
