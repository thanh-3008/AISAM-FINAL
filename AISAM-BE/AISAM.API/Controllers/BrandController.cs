using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers
{
    [ApiController]
    [Route("api/brands")]
    [Authorize]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IBrandService brandService, ILogger<BrandController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GenericResponse<PagedResult<BrandResponseDto>>>> GetBrands(
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
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _brandService.GetPagedAsync(workspaceId, new PaginationRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                }, includeDeleted, cancellationToken);

                return StatusCode(result.StatusCode, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<PagedResult<BrandResponseDto>>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands");
                return StatusCode(500, GenericResponse<PagedResult<BrandResponseDto>>.CreateError("System error"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<BrandResponseDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _brandService.GetByIdAsync(id, workspaceId, cancellationToken);
                return StatusCode(result.StatusCode, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BrandResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand {BrandId}", id);
                return StatusCode(500, GenericResponse<BrandResponseDto>.CreateError("System error"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GenericResponse<BrandResponseDto>>> Create([FromBody] CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _brandService.CreateAsync(workspaceId, profileId, userId, request, cancellationToken);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
                }

                return StatusCode(result.StatusCode, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BrandResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, GenericResponse<BrandResponseDto>.CreateError("System error"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GenericResponse<BrandResponseDto>>> Update(Guid id, [FromBody] UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _brandService.UpdateAsync(id, workspaceId, request, cancellationToken);
                return StatusCode(result.StatusCode, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BrandResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {BrandId}", id);
                return StatusCode(500, GenericResponse<BrandResponseDto>.CreateError("System error"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> SoftDelete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _brandService.SoftDeleteAsync(id, workspaceId, cancellationToken);
                return StatusCode(result.StatusCode, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {BrandId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error"));
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<GenericResponse<bool>>> Restore(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _brandService.RestoreAsync(id, workspaceId, cancellationToken);
                return StatusCode(result.StatusCode, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring brand {BrandId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error"));
            }
        }
    }
}
