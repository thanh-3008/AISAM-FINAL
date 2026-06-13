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
    public async Task GetPaged_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakePostService
        {
            PagedResult = GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>())
        };
        var controller = CreateController(service, workspaceId);

        await controller.GetPaged();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
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

    private static PostsController CreateController(IPostService service, Guid workspaceId)
    {
        var context = new DefaultHttpContext();
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;

        return new PostsController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class FakePostService : IPostService
    {
        public Guid LastWorkspaceId { get; private set; }
        public GenericResponse<PagedResult<PostListItemDto>> PagedResult { get; set; } = GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>());
        public GenericResponse<PostListItemDto> DetailResult { get; set; } = GenericResponse<PostListItemDto>.CreateSuccess(new PostListItemDto());

        public Task<GenericResponse<PagedResult<PostListItemDto>>> GetPagedAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(PagedResult);
        }

        public Task<GenericResponse<PostListItemDto>> GetByIdAsync(Guid workspaceId, Guid postId, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(DetailResult);
        }
    }
}
