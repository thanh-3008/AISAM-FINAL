using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/quota")]
[Authorize]
public sealed class QuotaController : ControllerBase
{
    private readonly IQuotaService _quotaService;

    public QuotaController(IQuotaService quotaService)
    {
        _quotaService = quotaService;
    }

    [HttpGet("workspace/current")]
    public async Task<ActionResult<GenericResponse<QuotaSummaryDto>>> GetCurrentWorkspaceQuota(CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var result = await _quotaService.GetWorkspaceSummaryAsync(workspaceId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
