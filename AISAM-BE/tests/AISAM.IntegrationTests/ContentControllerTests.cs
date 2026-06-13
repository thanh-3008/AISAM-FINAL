using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class ContentControllerTests
{
    [Fact]
    public async Task Create_ReturnsServiceStatusCode_WhenValidationFails()
    {
        var service = new FakeContentService
        {
            CreateResult = GenericResponse<ContentResponseDto>.CreateError("Brand not found.", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid(), Guid.NewGuid());

        var result = await controller.Create(new CreateContentRequest());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetPaged_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakeContentService
        {
            PagedResult = GenericResponse<PagedResult<ContentResponseDto>>.CreateSuccess(new PagedResult<ContentResponseDto>())
        };
        var controller = CreateController(service, Guid.NewGuid(), workspaceId);

        await controller.GetPaged();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    [Fact]
    public async Task Create_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var service = new FakeContentService();
        var controller = CreateController(service, profileId, workspaceId);

        await controller.Create(new CreateContentRequest());

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    private static ContentController CreateController(IContentService service, Guid profileId, Guid workspaceId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;

        return new ContentController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeContentService : IContentService
    {
        public Guid LastProfileId { get; private set; }
        public Guid LastWorkspaceId { get; private set; }
        public GenericResponse<ContentResponseDto> CreateResult { get; set; } = GenericResponse<ContentResponseDto>.CreateSuccess(new ContentResponseDto());
        public GenericResponse<PagedResult<ContentResponseDto>> PagedResult { get; set; } = GenericResponse<PagedResult<ContentResponseDto>>.CreateSuccess(new PagedResult<ContentResponseDto>());

        public Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, Guid workspaceId, CreateContentRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            LastWorkspaceId = workspaceId;
            return Task.FromResult(CreateResult);
        }

        public Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, Guid workspaceId, UpdateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
