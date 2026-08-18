using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.IntegrationTests;

public class PostsControllerTests
{
    [Fact]
    public async Task GetPaged_UsesValidatedActiveProfileFromHttpContext()
    {
        var profileId = Guid.NewGuid();
        var service = new FakePostService
        {
            PagedResult = GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>())
        };
        var controller = CreateController(service, profileId);

        await controller.GetPaged();

        Assert.Equal(profileId, service.LastProfileId);
    }

    [Fact]
    public async Task GetById_ReturnsServiceStatusCode_WhenPostBelongsToAnotherProfile()
    {
        var service = new FakePostService
        {
            DetailResult = GenericResponse<PostListItemDto>.CreateError("Post not found.", HttpStatusCode.NotFound)
        };
        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.GetById(Guid.NewGuid());

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
    }

    private static PostsController CreateController(IPostService service, Guid profileId)
    {
        var context = new DefaultHttpContext();
        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profileId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = profileId;

        return new PostsController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakePostService : IPostService
    {
        public Guid LastProfileId { get; private set; }
        public GenericResponse<PagedResult<PostListItemDto>> PagedResult { get; set; } = GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>());
        public GenericResponse<PostListItemDto> DetailResult { get; set; } = GenericResponse<PostListItemDto>.CreateSuccess(new PostListItemDto());

        public Task<GenericResponse<PagedResult<PostListItemDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<PostListItemDto>> GetByIdAsync(Guid profileId, Guid postId, CancellationToken cancellationToken = default)
        {
            LastProfileId = profileId;
            return Task.FromResult(DetailResult);
        }

        public Task<GenericResponse<bool>> DeleteAsync(Guid id, Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<bool>.CreateSuccess(true));
    }
}




