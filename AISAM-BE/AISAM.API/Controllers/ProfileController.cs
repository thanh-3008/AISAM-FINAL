using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                var result = await _profileService.SearchUserProfilesAsync(userId, search, isDeleted, cancellationToken);
                return Ok(result);
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
                var result = await _profileService.GetProfileByIdAsync(id, cancellationToken);
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
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
                var result = await _profileService.CreateProfileAsync(userId, request, cancellationToken);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetProfile), new { id = result.Data!.Id }, result);
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
                var result = await _profileService.UpdateProfileAsync(id, request, cancellationToken);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
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
                var result = await _profileService.DeleteProfileAsync(id, cancellationToken);
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
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
                var result = await _profileService.RestoreProfileAsync(id, cancellationToken);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring profile {ProfileId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error"));
            }
        }
    }
}
