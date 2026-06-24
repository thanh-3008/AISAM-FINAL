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
    [Route("api/products")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GenericResponse<PagedResult<ProductResponseDto>>>> GetProducts(
            [FromQuery] Guid? brandId = null,
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
                var result = await _productService.GetPagedAsync(new PaginationRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                }, workspaceId, userId, brandId, includeDeleted, cancellationToken);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<PagedResult<ProductResponseDto>>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, GenericResponse<PagedResult<ProductResponseDto>>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<ProductResponseDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _productService.GetByIdAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<ProductResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return StatusCode(500, GenericResponse<ProductResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<GenericResponse<ProductResponseDto>>> Create([FromForm] ProductCreateRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _productService.CreateAsync(WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, request, cancellationToken);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<ProductResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, GenericResponse<ProductResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<GenericResponse<ProductResponseDto>>> Update(Guid id, [FromForm] ProductUpdateRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _productService.UpdateAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, request, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<ProductResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, GenericResponse<ProductResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> SoftDelete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _productService.SoftDeleteAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<GenericResponse<bool>>> Restore(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = UserClaimsHelper.GetUserIdOrThrow(User);
                var result = await _productService.RestoreAsync(id, WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext), userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring product {ProductId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }


    }
}
