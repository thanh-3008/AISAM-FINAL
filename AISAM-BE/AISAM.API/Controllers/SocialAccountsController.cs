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
[Route("api/social/accounts")]
[Authorize]
public sealed class SocialAccountsController : ControllerBase
{
    private readonly ISocialService _socialService;

    public SocialAccountsController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<SocialAccountDto>>>> GetMyAccounts(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _socialService.GetProfileAccountsAsync(GetProfileId(), cancellationToken);
            return Ok(GenericResponse<IReadOnlyList<SocialAccountDto>>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<IReadOnlyList<SocialAccountDto>>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
    }

    [HttpGet("{socialAccountId:guid}/available-targets")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<AvailableTargetDto>>>> GetAvailableTargets(
        Guid socialAccountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _socialService.ListAvailableTargetsForAccountAsync(GetProfileId(), socialAccountId, cancellationToken);
            return Ok(GenericResponse<IReadOnlyList<AvailableTargetDto>>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<IReadOnlyList<AvailableTargetDto>>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
        catch (ArgumentException ex) when (IsNotFoundMessage(ex.Message))
        {
            return NotFound(GenericResponse<IReadOnlyList<AvailableTargetDto>>.CreateError(ex.Message, HttpStatusCode.NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(GenericResponse<IReadOnlyList<AvailableTargetDto>>.CreateError(ex.Message));
        }
    }

    [HttpGet("{socialAccountId:guid}/linked-targets")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<SocialTargetDto>>>> GetLinkedTargets(
        Guid socialAccountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _socialService.GetLinkedTargetsAsync(GetProfileId(), socialAccountId, cancellationToken);
            return Ok(GenericResponse<IReadOnlyList<SocialTargetDto>>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<IReadOnlyList<SocialTargetDto>>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
        catch (ArgumentException ex) when (IsNotFoundMessage(ex.Message))
        {
            return NotFound(GenericResponse<IReadOnlyList<SocialTargetDto>>.CreateError(ex.Message, HttpStatusCode.NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(GenericResponse<IReadOnlyList<SocialTargetDto>>.CreateError(ex.Message));
        }
    }

    [HttpPost("{socialAccountId:guid}/link-targets")]
    public async Task<ActionResult<GenericResponse<SocialAccountDto>>> LinkTargets(
        Guid socialAccountId,
        [FromBody] LinkSelectedTargetsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.Provider, "facebook", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(GenericResponse<SocialAccountDto>.CreateError("Only Facebook is supported in Phase C."));
        }

        try
        {
            var result = await _socialService.LinkSelectedTargetsForAccountAsync(GetProfileId(), socialAccountId, request, cancellationToken);
            return Ok(GenericResponse<SocialAccountDto>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<SocialAccountDto>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
        catch (ArgumentException ex) when (IsNotFoundMessage(ex.Message))
        {
            return NotFound(GenericResponse<SocialAccountDto>.CreateError(ex.Message, HttpStatusCode.NotFound));
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

    [HttpDelete("{socialAccountId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteAccount(Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _socialService.UnlinkAccountAsync(GetProfileId(), socialAccountId, cancellationToken);
            if (!deleted)
            {
                return NotFound(GenericResponse<bool>.CreateError("Social account not found.", HttpStatusCode.NotFound));
            }

            return Ok(GenericResponse<bool>.CreateSuccess(true));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<bool>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
    }

    private Guid GetProfileId()
    {
        return ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
    }

    private static bool IsNotFoundMessage(string message)
    {
        return string.Equals(message, "Social account not found.", StringComparison.Ordinal)
            || string.Equals(message, "Brand not found.", StringComparison.Ordinal);
    }
}
