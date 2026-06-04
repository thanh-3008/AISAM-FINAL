using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/social/integrations")]
[Authorize]
public sealed class SocialIntegrationController : ControllerBase
{
    private readonly ISocialService _socialService;

    public SocialIntegrationController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpDelete("{socialIntegrationId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteIntegration(
        Guid socialIntegrationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _socialService.UnlinkTargetAsync(GetProfileId(), socialIntegrationId, cancellationToken);
            if (!deleted)
            {
                return NotFound(GenericResponse<bool>.CreateError("Social integration not found.", HttpStatusCode.NotFound));
            }

            return Ok(GenericResponse<bool>.CreateSuccess(true));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<bool>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
    }

    [HttpGet("brand/{brandId:guid}")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<SocialIntegrationDto>>>> GetByBrand(
        Guid brandId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _socialService.GetIntegrationsByBrandAsync(GetProfileId(), brandId, cancellationToken);
            return Ok(GenericResponse<IReadOnlyList<SocialIntegrationDto>>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<IReadOnlyList<SocialIntegrationDto>>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
        catch (ArgumentException ex) when (string.Equals(ex.Message, "Brand not found.", StringComparison.Ordinal))
        {
            return NotFound(GenericResponse<IReadOnlyList<SocialIntegrationDto>>.CreateError(ex.Message, HttpStatusCode.NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(GenericResponse<IReadOnlyList<SocialIntegrationDto>>.CreateError(ex.Message));
        }
    }

    private Guid GetProfileId()
    {
        return ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
    }
}
