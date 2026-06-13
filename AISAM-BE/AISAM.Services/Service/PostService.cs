using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service;

public sealed class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<GenericResponse<PagedResult<PostListItemDto>>> GetPagedAsync(
        Guid workspaceId,
        PaginationRequest request,
        Guid? brandId = null,
        ContentStatusEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var posts = await _postRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, brandId, status, cancellationToken);

        return GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>
        {
            Data = posts.Data.Select(MapToDto).ToList(),
            TotalCount = posts.TotalCount,
            Page = posts.Page,
            PageSize = posts.PageSize
        }, "Posts retrieved successfully.");
    }

    public async Task<GenericResponse<PostListItemDto>> GetByIdAsync(Guid workspaceId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post == null || post.IsDeleted || !BelongsToWorkspace(post, workspaceId))
        {
            return GenericResponse<PostListItemDto>.CreateError("Post not found.", HttpStatusCode.NotFound);
        }

        return GenericResponse<PostListItemDto>.CreateSuccess(MapToDto(post), "Post retrieved successfully.");
    }

    private static PostListItemDto MapToDto(Post post)
    {
        return new PostListItemDto
        {
            Id = post.Id,
            ContentId = post.ContentId,
            IntegrationId = post.IntegrationId,
            ExternalPostId = post.ExternalPostId,
            PublishedAt = post.PublishedAt,
            Status = post.Status.ToString(),
            ContentTitle = post.Content.Title,
            BrandName = post.Content.Brand?.Name
        };
    }

    private static bool BelongsToWorkspace(Post post, Guid workspaceId)
    {
        return post.Content.WorkspaceId == workspaceId ||
               (post.Content.WorkspaceId == null && post.Content.Brand?.WorkspaceId == workspaceId);
    }
}
