using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Repositories.IRepositories;
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
    private readonly IProfileRepository _profileRepository;

    public SocialAuthController(ISocialService socialService, IProfileRepository profileRepository)
    {
        _socialService = socialService;
        _profileRepository = profileRepository;
    }

    [HttpGet("facebook")]
    public async Task<ActionResult<GenericResponse<AuthUrlResponse>>> GetFacebookAuthUrl(CancellationToken cancellationToken = default)
    {
        try
        {
            var profileId = await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
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
            if (string.Equals(ex.Message, "Facebook integration is not configured.", StringComparison.Ordinal))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                    GenericResponse<AuthUrlResponse>.CreateError(ex.Message, HttpStatusCode.ServiceUnavailable));
            }

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
            var profileId = await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
            var result = await _socialService.LinkAccountInWorkspaceAsync("facebook", WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), profileId, request, cancellationToken);
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
            if (string.Equals(ex.Message, "Facebook integration is not configured.", StringComparison.Ordinal))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                    GenericResponse<SocialAccountDto>.CreateError(ex.Message, HttpStatusCode.ServiceUnavailable));
            }

            return BadRequest(GenericResponse<SocialAccountDto>.CreateError(ex.Message));
        }
    }

    [HttpGet("tiktok")]
    public async Task<ActionResult<GenericResponse<AuthUrlResponse>>> GetTikTokAuthUrl(CancellationToken cancellationToken = default)
    {
        try
        {
            var profileId = await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
            var result = await _socialService.GetAuthUrlAsync("tiktok", profileId, cancellationToken);
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
            if (string.Equals(ex.Message, "TikTok integration is not configured.", StringComparison.Ordinal))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                    GenericResponse<AuthUrlResponse>.CreateError(ex.Message, HttpStatusCode.ServiceUnavailable));
            }

            return BadRequest(GenericResponse<AuthUrlResponse>.CreateError(ex.Message));
        }
    }

    [HttpPost("tiktok/callback")]
    public async Task<ActionResult<GenericResponse<SocialAccountDto>>> HandleTikTokCallback(
        [FromBody] SocialCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profileId = await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
            var result = await _socialService.LinkAccountInWorkspaceAsync(
                "tiktok",
                WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
                profileId,
                request,
                cancellationToken);
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
            if (string.Equals(ex.Message, "TikTok integration is not configured.", StringComparison.Ordinal))
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                    GenericResponse<SocialAccountDto>.CreateError(ex.Message, HttpStatusCode.ServiceUnavailable));
            }

            return BadRequest(GenericResponse<SocialAccountDto>.CreateError(ex.Message));
        }
    }
}
