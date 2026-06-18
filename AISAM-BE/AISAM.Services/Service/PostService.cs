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
        Guid profileId,
        PaginationRequest request,
        Guid? brandId = null,
        ContentStatusEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var posts = await _postRepository.GetPagedByProfileIdAsync(profileId, request, brandId, status, cancellationToken);

        return GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>
        {
            Data = posts.Data.Select(MapToDto).ToList(),
            TotalCount = posts.TotalCount,
            Page = posts.Page,
            PageSize = posts.PageSize
        }, "Posts retrieved successfully.");
    }

    public async Task<GenericResponse<PostListItemDto>> GetByIdAsync(Guid profileId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post == null || post.IsDeleted || post.Content?.ProfileId != profileId)
        {
            return GenericResponse<PostListItemDto>.CreateError("Post not found.", HttpStatusCode.NotFound);
        }

        return GenericResponse<PostListItemDto>.CreateSuccess(MapToDto(post), "Post retrieved successfully.");
    }

    public async Task<GenericResponse<PagedResult<PostListItemDto>>> GetPagedByWorkspaceAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
    {
        var posts = await _postRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, brandId, status, cancellationToken);
        return GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>
        { Data = posts.Data.Select(MapToDto).ToList(), TotalCount = posts.TotalCount, Page = posts.Page, PageSize = posts.PageSize }, "Posts retrieved successfully.");
    }

    public async Task<GenericResponse<PostListItemDto>> GetByIdInWorkspaceAsync(Guid workspaceId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        return post == null || post.IsDeleted || post.Content?.WorkspaceId != workspaceId
            ? GenericResponse<PostListItemDto>.CreateError("Post not found.", HttpStatusCode.NotFound)
            : GenericResponse<PostListItemDto>.CreateSuccess(MapToDto(post), "Post retrieved successfully.");
    }

    private static string MapAdType(AdTypeEnum adType) => adType switch
    {
        AdTypeEnum.TextOnly => "TEXT",
        AdTypeEnum.ImageText => "IMAGE",
        AdTypeEnum.VideoText => "VIDEO",
        _ => adType.ToString()
    };

    private static string? MapPlatform(SocialPlatformEnum? platform) => platform?.ToString().ToLowerInvariant();

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
            ContentTitle = post.Content?.Title,
            BrandName = post.Content?.Brand?.Name,
            Platform = MapPlatform(post.Integration?.Platform),
            Type = post.Content != null ? MapAdType(post.Content.AdType) : null,
            Caption = post.Content?.TextContent
        };
    }
}
