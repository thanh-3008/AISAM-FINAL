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
[Route("api/social/accounts")]
[Authorize]
public sealed class SocialAccountsController : ControllerBase
{
    private readonly ISocialService _socialService;
    private readonly IProfileRepository _profileRepository;

    public SocialAccountsController(ISocialService socialService, IProfileRepository profileRepository)
    {
        _socialService = socialService;
        _profileRepository = profileRepository;
    }

    [HttpGet("me")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<SocialAccountDto>>>> GetMyAccounts(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _socialService.GetWorkspaceAccountsAsync(GetWorkspaceId(), cancellationToken);
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
            var result = await _socialService.ListAvailableTargetsInWorkspaceAsync(GetWorkspaceId(), socialAccountId, cancellationToken);
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
            var result = await _socialService.GetLinkedTargetsInWorkspaceAsync(GetWorkspaceId(), socialAccountId, cancellationToken);
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
        if (!string.Equals(request.Provider, "facebook", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Provider, "instagram", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Provider, "tiktok", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(GenericResponse<SocialAccountDto>.CreateError("Only Facebook, Instagram and TikTok are supported."));
        }

        try
        {
            var profileId = await GetProfileIdAsync(cancellationToken);
            var result = await _socialService.LinkSelectedTargetsInWorkspaceAsync(GetWorkspaceId(), profileId, socialAccountId, request, cancellationToken);
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

    [HttpGet("{socialAccountId:guid}/ad-accounts")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<FacebookAdAccountData>>>> GetAdAccounts(
        Guid socialAccountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _socialService.GetAdAccountsForSocialAccountInWorkspaceAsync(GetWorkspaceId(), socialAccountId, cancellationToken);
            return Ok(GenericResponse<IReadOnlyList<FacebookAdAccountData>>.CreateSuccess(result));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(GenericResponse<IReadOnlyList<FacebookAdAccountData>>.CreateError("Invalid profile context.", HttpStatusCode.Unauthorized));
        }
        catch (ArgumentException ex) when (IsNotFoundMessage(ex.Message))
        {
            return NotFound(GenericResponse<IReadOnlyList<FacebookAdAccountData>>.CreateError(ex.Message, HttpStatusCode.NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(GenericResponse<IReadOnlyList<FacebookAdAccountData>>.CreateError(ex.Message));
        }
    }

    [HttpDelete("{socialAccountId:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteAccount(Guid socialAccountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _socialService.UnlinkAccountInWorkspaceAsync(GetWorkspaceId(), socialAccountId, cancellationToken);
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

    private async Task<Guid> GetProfileIdAsync(CancellationToken cancellationToken)
    {
        return await WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
    }

    private Guid GetWorkspaceId() => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);

    private static bool IsNotFoundMessage(string message)
    {
        return string.Equals(message, "Social account not found.", StringComparison.Ordinal)
            || string.Equals(message, "Brand not found.", StringComparison.Ordinal);
    }
}
