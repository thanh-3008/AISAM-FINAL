using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class PostServiceTests
{
    [Fact]
    public async Task GetPagedAsync_ReturnsOnlyPostsForActiveProfile()
    {
        var profileId = Guid.NewGuid();
        var ownBrand = new Brand { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Own brand" };
        var ownContent = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = ownBrand.Id,
            Brand = ownBrand,
            Title = "Owned content",
            TextContent = "Owned text"
        };
        var otherContent = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Brand = new Brand { Id = Guid.NewGuid(), ProfileId = Guid.NewGuid(), Name = "Other brand" },
            Title = "Other content",
            TextContent = "Other text"
        };
        var ownPost = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = ownContent.Id,
            Content = ownContent,
            IntegrationId = Guid.NewGuid(),
            PublishedAt = DateTime.UtcNow,
            Status = ContentStatusEnum.Published
        };
        var otherPost = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = otherContent.Id,
            Content = otherContent,
            IntegrationId = Guid.NewGuid(),
            PublishedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = ContentStatusEnum.Published
        };
        var service = new PostService(new FakePostRepository(ownPost, otherPost));

        var result = await service.GetPagedAsync(profileId, new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.True(result.Success);
        var item = Assert.Single(result.Data!.Data);
        Assert.Equal(ownPost.Id, item.Id);
        Assert.Equal("Owned content", item.ContentTitle);
        Assert.Equal("Own brand", item.BrandName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_ForAnotherProfilesPost()
    {
        var otherProfileId = Guid.NewGuid();
        var otherBrand = new Brand { Id = Guid.NewGuid(), ProfileId = otherProfileId, Name = "Other brand" };
        var otherContent = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfileId,
            BrandId = otherBrand.Id,
            Brand = otherBrand,
            Title = "Other content",
            TextContent = "Other text"
        };
        var otherPost = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = otherContent.Id,
            Content = otherContent,
            IntegrationId = Guid.NewGuid(),
            PublishedAt = DateTime.UtcNow,
            Status = ContentStatusEnum.Published
        };
        var service = new PostService(new FakePostRepository(otherPost));

        var result = await service.GetByIdAsync(Guid.NewGuid(), otherPost.Id);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Post not found.", result.Message);
    }

    [Fact]
    public async Task GetPagedAsync_AppliesOptionalBrandIdAndStatusFilters()
    {
        var profileId = Guid.NewGuid();
        var brandA = new Brand { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Brand A" };
        var brandB = new Brand { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Brand B" };
        var contentA = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandA.Id,
            Brand = brandA,
            Title = "Published A",
            TextContent = "A"
        };
        var contentB = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            BrandId = brandB.Id,
            Brand = brandB,
            Title = "Draft B",
            TextContent = "B"
        };
        var publishedPost = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = contentA.Id,
            Content = contentA,
            IntegrationId = Guid.NewGuid(),
            PublishedAt = DateTime.UtcNow,
            Status = ContentStatusEnum.Published
        };
        var draftPost = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = contentB.Id,
            Content = contentB,
            IntegrationId = Guid.NewGuid(),
            PublishedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = ContentStatusEnum.Draft
        };
        var service = new PostService(new FakePostRepository(publishedPost, draftPost));

        var result = await service.GetPagedAsync(
            profileId,
            new PaginationRequest { Page = 1, PageSize = 10 },
            brandA.Id,
            ContentStatusEnum.Published);

        Assert.True(result.Success);
        var item = Assert.Single(result.Data!.Data);
        Assert.Equal(publishedPost.Id, item.Id);
        Assert.Equal("Published", item.Status);
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly Dictionary<Guid, Post> _posts;

        public FakePostRepository(params Post[] posts)
        {
            _posts = posts.ToDictionary(post => post.Id);
        }

        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _posts.TryGetValue(id, out var post);
            return Task.FromResult(post is { IsDeleted: false } ? post : null);
        }

        public Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default)
        {
            var query = _posts.Values.Where(post => !post.IsDeleted && post.Content.ProfileId == profileId);

            if (brandId.HasValue)
            {
                query = query.Where(post => post.Content.BrandId == brandId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(post => post.Status == status.Value);
            }

            var data = query.OrderByDescending(post => post.PublishedAt).ToList();
            return Task.FromResult(new PagedResult<Post>
            {
                Data = data,
                TotalCount = data.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
        {
            post.IsDeleted = true;
            return Task.CompletedTask;
        }
    }
}
