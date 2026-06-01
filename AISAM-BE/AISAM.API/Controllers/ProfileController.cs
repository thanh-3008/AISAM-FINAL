using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace AISAM.API.Controllers
{
    [ApiController]
    [Route("api/profiles")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileService profileService, ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<GenericResponse<IEnumerable<ProfileResponseDto>>>> GetUserProfiles(
            Guid userId,
            [FromQuery] string? search = null,
            [FromQuery] bool? isDeleted = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetUserIdOrThrow();
                if (userId != currentUserId)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        GenericResponse<IEnumerable<ProfileResponseDto>>.CreateError("You are not allowed to access another user's profiles", HttpStatusCode.Forbidden));
                }

                var result = await _profileService.SearchUserProfilesAsync(userId, search, isDeleted, cancellationToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<IEnumerable<ProfileResponseDto>>.CreateError("Invalid token", HttpStatusCode.Unauthorized));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profiles for user {UserId}", userId);
                return StatusCode(500, GenericResponse<IEnumerable<ProfileResponseDto>>.CreateError("System error"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<ProfileResponseDto>>> GetProfile(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetUserIdOrThrow();
                var result = await _profileService.GetProfileByIdAsync(id, currentUserId, cancellationToken);
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<ProfileResponseDto>.CreateError("Invalid token", HttpStatusCode.Unauthorized));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile {ProfileId}", id);
                return StatusCode(500, GenericResponse<ProfileResponseDto>.CreateError("System error"));
            }
        }

        [HttpPost("user/{userId}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<GenericResponse<ProfileResponseDto>>> CreateProfileForm(
            Guid userId,
            [FromForm] CreateProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetUserIdOrThrow();
                if (userId != currentUserId)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden,
                        GenericResponse<ProfileResponseDto>.CreateError("You are not allowed to create profiles for another user", HttpStatusCode.Forbidden));
                }

                var result = await _profileService.CreateProfileAsync(userId, request, cancellationToken);
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, result);
                }

                return CreatedAtAction(nameof(GetProfile), new { id = result.Data!.Id }, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<ProfileResponseDto>.CreateError("Invalid token", HttpStatusCode.Unauthorized));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating profile for user {UserId}", userId);
                return StatusCode(500, GenericResponse<ProfileResponseDto>.CreateError("System error"));
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<GenericResponse<ProfileResponseDto>>> UpdateProfile(
            Guid id,
            [FromForm] UpdateProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetUserIdOrThrow();
                var result = await _profileService.UpdateProfileAsync(id, currentUserId, request, cancellationToken);
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<ProfileResponseDto>.CreateError("Invalid token", HttpStatusCode.Unauthorized));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", id);
                return StatusCode(500, GenericResponse<ProfileResponseDto>.CreateError("System error"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> DeleteProfile(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetUserIdOrThrow();
                var result = await _profileService.DeleteProfileAsync(id, currentUserId, cancellationToken);
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token", HttpStatusCode.Unauthorized));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile {ProfileId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error"));
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<GenericResponse<bool>>> RestoreProfile(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = GetUserIdOrThrow();
                var result = await _profileService.RestoreProfileAsync(id, currentUserId, cancellationToken);
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token", HttpStatusCode.Unauthorized));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring profile {ProfileId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error"));
            }
        }

        private Guid GetUserIdOrThrow()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            return userId;
        }
    }
}
