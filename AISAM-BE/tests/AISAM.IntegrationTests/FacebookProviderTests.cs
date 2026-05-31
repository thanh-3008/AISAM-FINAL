using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Services.Service;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;

namespace AISAM.IntegrationTests;

public class FacebookProviderTests
{
    [Fact]
    public async Task GetAuthUrlAsync_BuildsFacebookOAuthUrlWithConfiguredPermissions()
    {
        var provider = CreateProvider(new RecordingHandler(), CreateSettings());

        var url = await provider.GetAuthUrlAsync("state-123", "https://client/callback");

        Assert.Contains("client_id=app-id", url);
        Assert.Contains("state=state-123", url);
        Assert.Contains("pages_manage_posts", url);
    }

    [Fact]
    public async Task ExchangeCodeAsync_ReturnsSocialAccountDto_WhenFacebookReturnsTokenAndProfile()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"access_token":"user-token","expires_in":3600}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"fb-user","name":"Test User"}""");
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.ExchangeCodeAsync("oauth-code", "https://client/callback");

        Assert.Equal("facebook", result.Provider);
        Assert.Equal("fb-user", result.ProviderUserId);
        Assert.Equal("user-token", result.AccessToken);
        Assert.True(result.ExpiresAt > DateTime.UtcNow.AddMinutes(50));
    }

    [Fact]
    public async Task GetTargetsAsync_ReturnsAvailablePages()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"data":[{"id":"page-1","name":"Page One","category":"Retail","access_token":"page-token"}]}""");
        var provider = CreateProvider(handler, CreateSettings());

        var result = (await provider.GetTargetsAsync("user-token")).ToList();

        Assert.Single(result);
        Assert.Equal("page-1", result[0].ProviderTargetId);
        Assert.Equal("Page One", result[0].Name);
    }

    [Fact]
    public async Task PublishAsync_TextPost_SucceedsAgainstFeedEndpoint()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"feed-post-1"}""");
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.PublishAsync(CreateAccount(), CreateIntegration("page-token"), new PostDto
        {
            Message = "Hello Facebook"
        });

        Assert.True(result.Success);
        Assert.Equal("feed-post-1", result.ProviderPostId);
        Assert.Contains("/page-1/feed", handler.Requests[0].Url);
        Assert.Contains("message=Hello+Facebook", handler.Requests[0].Body);
        Assert.Contains("access_token=page-token", handler.Requests[0].Body);
    }

    [Fact]
    public async Task PublishAsync_SingleImage_SucceedsAgainstPhotosEndpoint()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"photo-post-1"}""");
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.PublishAsync(CreateAccount(), CreateIntegration("page-token"), new PostDto
        {
            Message = "Image post",
            ImageUrl = "https://cdn/image.jpg"
        });

        Assert.True(result.Success);
        Assert.Equal("photo-post-1", result.ProviderPostId);
        Assert.Contains("/page-1/photos", handler.Requests[0].Url);
        Assert.Contains("url=https%3A%2F%2Fcdn%2Fimage.jpg", handler.Requests[0].Body);
    }

    [Fact]
    public async Task PublishAsync_MultiImage_UploadsUnpublishedMediaThenPublishesFeed()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"media-1"}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"media-2"}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"feed-post-2"}""");
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.PublishAsync(CreateAccount(), CreateIntegration("page-token"), new PostDto
        {
            Message = "Gallery post",
            ImageUrls = new List<string> { "https://cdn/1.jpg", "https://cdn/2.jpg" }
        });

        Assert.True(result.Success);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Contains("published=false", handler.Requests[0].Body);
        Assert.Contains("published=false", handler.Requests[1].Body);
        Assert.Contains("attached_media%5B0%5D", handler.Requests[2].Body);
        Assert.Contains("attached_media%5B1%5D", handler.Requests[2].Body);
    }

    [Fact]
    public async Task PublishAsync_Video_SucceedsAgainstVideosEndpoint()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"video-post-1"}""");
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.PublishAsync(CreateAccount(), CreateIntegration("page-token"), new PostDto
        {
            Message = "Video post",
            VideoUrl = "https://cdn/video.mp4"
        });

        Assert.True(result.Success);
        Assert.Contains("/page-1/videos", handler.Requests[0].Url);
        Assert.Contains("file_url=https%3A%2F%2Fcdn%2Fvideo.mp4", handler.Requests[0].Body);
    }

    [Fact]
    public async Task PublishAsync_RetriesWithFreshPageToken_WhenInitialPageTokenFails()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.BadRequest, """{"error":{"message":"Invalid page token"}}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"data":[{"id":"page-1","name":"Page One","access_token":"fresh-page-token"}]}""");
        handler.EnqueueJson(HttpStatusCode.OK, """{"id":"feed-post-retried"}""");
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.PublishAsync(CreateAccount(), CreateIntegration("expired-page-token"), new PostDto
        {
            Message = "Retry post"
        });

        Assert.True(result.Success);
        Assert.Equal("feed-post-retried", result.ProviderPostId);
        Assert.Equal("fresh-page-token", result.RefreshedTargetAccessToken);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Contains("access_token=expired-page-token", handler.Requests[0].Body);
        Assert.Contains("/me/accounts", handler.Requests[1].Url);
        Assert.Contains("access_token=fresh-page-token", handler.Requests[2].Body);
    }

    [Fact]
    public async Task GetAuthUrlAsync_ThrowsClearError_WhenFacebookConfigIsMissing()
    {
        var provider = CreateProvider(new RecordingHandler(), new FacebookSettings());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetAuthUrlAsync("state", "https://client/callback"));

        Assert.Equal("Facebook integration is not configured.", exception.Message);
    }

    private static FacebookProvider CreateProvider(RecordingHandler handler, FacebookSettings settings)
    {
        return new FacebookProvider(new HttpClient(handler), Options.Create(settings), NullLogger<FacebookProvider>.Instance);
    }

    private static FacebookSettings CreateSettings()
    {
        return new FacebookSettings
        {
            AppId = "app-id",
            AppSecret = "app-secret",
            RedirectUri = "https://server/callback"
        };
    }

    private static SocialAccount CreateAccount()
    {
        return new SocialAccount
        {
            Id = Guid.NewGuid(),
            Platform = SocialPlatformEnum.Facebook,
            UserAccessToken = "user-token"
        };
    }

    private static SocialIntegration CreateIntegration(string accessToken)
    {
        return new SocialIntegration
        {
            Id = Guid.NewGuid(),
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "page-1",
            AccessToken = accessToken
        };
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        public List<RecordedRequest> Requests { get; } = new();

        public void EnqueueJson(HttpStatusCode statusCode, string json)
        {
            _responses.Enqueue(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json)
            });
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest
            {
                Method = request.Method.Method,
                Url = request.RequestUri?.ToString() ?? string.Empty,
                Body = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult() ?? string.Empty
            });

            return Task.FromResult(_responses.Dequeue());
        }
    }

    private sealed class RecordedRequest
    {
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
