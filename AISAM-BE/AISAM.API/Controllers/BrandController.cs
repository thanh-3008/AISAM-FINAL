using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using AISAM.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _brandService.GetPagedByWorkspaceIdAsync(workspaceId, userId, new PaginationRequest
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
                return Unauthorized(GenericResponse<PagedResult<BrandResponseDto>>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands");
                return StatusCode(500, GenericResponse<PagedResult<BrandResponseDto>>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<BrandResponseDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _brandService.GetByIdAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BrandResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand {BrandId}", id);
                return StatusCode(500, GenericResponse<BrandResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GenericResponse<BrandResponseDto>>> Create([FromBody] CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _brandService.CreateAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, request, cancellationToken);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BrandResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, GenericResponse<BrandResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GenericResponse<BrandResponseDto>>> Update(Guid id, [FromBody] UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _brandService.UpdateAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, request, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<BrandResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {BrandId}", id);
                return StatusCode(500, GenericResponse<BrandResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> SoftDelete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _brandService.SoftDeleteAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {BrandId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<GenericResponse<bool>>> Restore(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _brandService.RestoreAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring brand {BrandId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }


    }
}
