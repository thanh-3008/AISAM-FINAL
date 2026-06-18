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
        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.Create(new CreateContentRequest());

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetPaged_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakeContentService
        {
            PagedResult = GenericResponse<PagedResult<ContentResponseDto>>.CreateSuccess(new PagedResult<ContentResponseDto>())
        };
        var controller = CreateController(service, profileId);

        await controller.GetPaged();

        Assert.Equal(profileId, service.LastProfileId);
    }

    private static ContentController CreateController(IContentService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;

        return new ContentController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakeContentService : IContentService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<ContentResponseDto> CreateResult { get; set; } = GenericResponse<ContentResponseDto>.CreateSuccess(new ContentResponseDto());
        public GenericResponse<PagedResult<ContentResponseDto>> PagedResult { get; set; } = GenericResponse<PagedResult<ContentResponseDto>>.CreateSuccess(new PagedResult<ContentResponseDto>());

        public Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(CreateResult);
        }

        public Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, UpdateContentRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<GenericResponse<ContentResponseDto>> CreateInWorkspaceAsync(Guid workspaceId, Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default) => CreateAsync(profileId, request, cancellationToken);
        public Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => GetPagedAsync(workspaceId, request, brandId, adType, includeDeleted, status, cancellationToken);
        public Task<GenericResponse<ContentResponseDto>> GetByIdInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => GetByIdAsync(id, workspaceId, cancellationToken);
        public Task<GenericResponse<ContentResponseDto>> UpdateInWorkspaceAsync(Guid id, Guid workspaceId, UpdateContentRequest request, CancellationToken cancellationToken = default) => UpdateAsync(id, workspaceId, request, cancellationToken);
        public Task<GenericResponse<ContentResponseDto>> CloneInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => CloneAsync(id, workspaceId, cancellationToken);
        public Task<GenericResponse<bool>> SoftDeleteInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => SoftDeleteAsync(id, workspaceId, cancellationToken);
        public Task<GenericResponse<bool>> RestoreInWorkspaceAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default) => RestoreAsync(id, workspaceId, cancellationToken);
    }
}
