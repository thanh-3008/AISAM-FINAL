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

    [HttpGet("profile/{profileId:guid}")]
    public async Task<ActionResult<GenericResponse<QuotaSummaryDto>>> GetProfileQuota(Guid profileId, CancellationToken cancellationToken = default)
    {
        var activeProfileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
        if (activeProfileId != profileId)
        {
            var error = GenericResponse<QuotaSummaryDto>.CreateError("Profile not found.", HttpStatusCode.NotFound);
            return StatusCode(error.StatusCode, error);
        }

        var result = await _quotaService.GetSummaryAsync(profileId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
