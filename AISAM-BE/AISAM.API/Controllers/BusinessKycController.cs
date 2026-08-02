using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/business-kyc")]
[Authorize]
public sealed class BusinessKycController : ControllerBase
{
    private readonly IBusinessKycService _businessKycService;

    public BusinessKycController(IBusinessKycService businessKycService)
    {
        _businessKycService = businessKycService;
    }

    [HttpPost("submit-kyc")]
    public async Task<ActionResult<GenericResponse<BusinessKycVerificationResponse>>> SubmitKyc(
        [FromBody] SubmitBusinessKycRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _businessKycService.SubmitAsync(userId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
