using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.API.Controllers
{
    [ApiController]
    [Route("api/campaigns")]
    [Authorize]
    public class AdCampaignController : ControllerBase
    {
        private readonly IAdCampaignService _campaignService;
        private readonly ILogger<AdCampaignController> _logger;

        public AdCampaignController(IAdCampaignService campaignService, ILogger<AdCampaignController> logger)
        {
            _campaignService = campaignService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GenericResponse<PagedResult<AdCampaignResponseDto>>>> GetCampaigns(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = true,
            [FromQuery] bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.GetPagedByWorkspaceIdAsync(workspaceId, userId, new PaginationRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                }, includeDeleted, cancellationToken);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaigns");
                return StatusCode(500, GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateError($"Lỗi hệ thống: {ex.Message}", HttpStatusCode.InternalServerError));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.GetByIdAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> Create([FromBody] CreateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.CreateAsync(workspaceId, userId, request, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign");
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> Update(Guid id, [FromBody] UpdateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.UpdateAsync(id, workspaceId, userId, request, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> SoftDelete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.SoftDeleteAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<GenericResponse<bool>>> Restore(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.RestoreAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/deploy")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> Deploy(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.DeployAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> Activate(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.ActivateAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/sync-insights")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> SyncInsights(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.SyncCampaignInsightsAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing insights for campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/cleanup")]
        public async Task<ActionResult<GenericResponse<bool>>> CleanupFailedDeployment(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.CleanupFailedDeploymentAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up deployment for campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> Duplicate(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.DuplicateAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpGet("{id}/preview")]
        public async Task<ActionResult<GenericResponse<CampaignPreviewDto>>> Preview(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.GetPreviewAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<CampaignPreviewDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<CampaignPreviewDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<GenericResponse<BulkCampaignResultDto>>> BulkCreate([FromBody] BulkCreateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.BulkCreateAsync(workspaceId, userId, request, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BulkCampaignResultDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating campaigns");
                return StatusCode(500, GenericResponse<BulkCampaignResultDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpDelete("bulk")]
        public async Task<ActionResult<GenericResponse<BulkCampaignResultDto>>> BulkDelete([FromBody] BulkDeleteAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.BulkDeleteAsync(workspaceId, userId, request, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BulkCampaignResultDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting campaigns");
                return StatusCode(500, GenericResponse<BulkCampaignResultDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("bulk/deploy")]
        public async Task<ActionResult<GenericResponse<BulkCampaignResultDto>>> BulkDeploy([FromBody] BulkDeployAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.BulkDeployAsync(workspaceId, userId, request, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BulkCampaignResultDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deploying campaigns");
                return StatusCode(500, GenericResponse<BulkCampaignResultDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }


    }
}
