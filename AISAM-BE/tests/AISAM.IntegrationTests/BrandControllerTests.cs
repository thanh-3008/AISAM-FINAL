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
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class BrandControllerTests
{
    [Fact]
    public async Task GetBrands_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakeBrandService();
        var controller = CreateController(service, Guid.NewGuid(), workspaceId, Guid.NewGuid());

        await controller.GetBrands();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    [Fact]
    public async Task Create_UsesValidatedWorkspaceProfileAndUserContext()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var service = new FakeBrandService();
        var controller = CreateController(service, profileId, workspaceId, userId);

        await controller.Create(new CreateBrandRequest { Name = "Brand" });

        Assert.Equal(workspaceId, service.LastWorkspaceId);
        Assert.Equal(profileId, service.LastProfileId);
        Assert.Equal(userId, service.LastUserId);
    }

    [Fact]
    public async Task Update_ReturnsServiceStatusCode_WhenValidationFails()
    {
        var service = new FakeBrandService
        {
            UpdateResult = GenericResponse<BrandResponseDto>.CreateError("Brand not found", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await controller.Update(Guid.NewGuid(), new UpdateBrandRequest());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    private static BrandController CreateController(FakeBrandService service, Guid profileId, Guid workspaceId, Guid userId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ], "Test"));

        return new BrandController(service, NullLogger<BrandController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeBrandService : IBrandService
    {
        public Guid LastWorkspaceId { get; private set; }
        public Guid LastProfileId { get; private set; }
        public Guid LastUserId { get; private set; }
        public GenericResponse<BrandResponseDto> CreateResult { get; set; } = GenericResponse<BrandResponseDto>.CreateSuccess(new BrandResponseDto { Id = Guid.NewGuid() });
        public GenericResponse<BrandResponseDto> UpdateResult { get; set; } = GenericResponse<BrandResponseDto>.CreateSuccess(new BrandResponseDto());

        public Task<GenericResponse<PagedResult<BrandResponseDto>>> GetPagedAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<PagedResult<BrandResponseDto>>.CreateSuccess(new PagedResult<BrandResponseDto>()));
        }

        public Task<GenericResponse<BrandResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<BrandResponseDto>.CreateSuccess(new BrandResponseDto { Id = id }));
        }

        public Task<GenericResponse<BrandResponseDto>> CreateAsync(Guid workspaceId, Guid profileId, Guid userId, CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            LastProfileId = profileId;
            LastUserId = userId;
            return Task.FromResult(CreateResult);
        }

        public Task<GenericResponse<BrandResponseDto>> UpdateAsync(Guid id, Guid workspaceId, UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(UpdateResult);
        }

        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));

        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
    }
}
