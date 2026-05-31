using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/social-auth")]
[Authorize]
public sealed class SocialAuthController : ControllerBase
{
    private readonly ISocialService _socialService;

    public SocialAuthController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet("facebook")]
    public async Task<ActionResult<GenericResponse<AuthUrlResponse>>> GetFacebookAuthUrl(CancellationToken cancellationToken = default)
    {
        try
        {
            var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
            var result = await _socialService.GetAuthUrlAsync("facebook", profileId, cancellationToken);
            return Ok(GenericResponse<AuthUrlResponse>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<AuthUrlResponse>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(GenericResponse<AuthUrlResponse>.CreateError(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(GenericResponse<AuthUrlResponse>.CreateError(ex.Message));
        }
    }

    [HttpPost("facebook/callback")]
    public async Task<ActionResult<GenericResponse<SocialAccountDto>>> HandleFacebookCallback(
        [FromBody] SocialCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
            var result = await _socialService.LinkAccountAsync("facebook", profileId, request, cancellationToken);
            return Ok(GenericResponse<SocialAccountDto>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<SocialAccountDto>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(GenericResponse<SocialAccountDto>.CreateError(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(GenericResponse<SocialAccountDto>.CreateError(ex.Message));
        }
    }
}
