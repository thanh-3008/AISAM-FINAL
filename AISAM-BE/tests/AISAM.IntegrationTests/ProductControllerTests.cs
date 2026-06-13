using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace AISAM.IntegrationTests;

public class ProductControllerTests
{
    [Fact]
    public async Task GetProducts_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakeProductService();
        var controller = CreateController(service, workspaceId);

        await controller.GetProducts();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    [Fact]
    public async Task Create_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakeProductService();
        var controller = CreateController(service, workspaceId);

        await controller.Create(new ProductCreateRequest());

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    [Fact]
    public async Task GetById_ReturnsServiceStatusCode_WhenValidationFails()
    {
        var service = new FakeProductService
        {
            GetByIdResult = GenericResponse<ProductResponseDto>.CreateError("Product not found", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.GetById(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    private static ProductController CreateController(FakeProductService service, Guid workspaceId)
    {
        var context = new DefaultHttpContext();
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;

        return new ProductController(service, NullLogger<ProductController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeProductService : IProductService
    {
        public Guid LastWorkspaceId { get; private set; }
        public GenericResponse<ProductResponseDto> GetByIdResult { get; set; } = GenericResponse<ProductResponseDto>.CreateSuccess(new ProductResponseDto());

        public Task<GenericResponse<PagedResult<ProductResponseDto>>> GetPagedAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<PagedResult<ProductResponseDto>>.CreateSuccess(new PagedResult<ProductResponseDto>()));
        }

        public Task<GenericResponse<ProductResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GetByIdResult);
        }

        public Task<GenericResponse<ProductResponseDto>> CreateAsync(Guid workspaceId, ProductCreateRequest request, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<ProductResponseDto>.CreateSuccess(new ProductResponseDto { Id = Guid.NewGuid() }));
        }

        public Task<GenericResponse<ProductResponseDto>> UpdateAsync(Guid id, Guid workspaceId, ProductUpdateRequestDto request, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<ProductResponseDto>.CreateSuccess(new ProductResponseDto()));
        }

        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
        }

        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
        }
    }
}
