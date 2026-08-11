using AISAM.Common.Models;
using AISAM.Services.Service;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;

namespace AISAM.IntegrationTests;

public class TikTokProviderTests
{
    [Fact]
    public async Task GetAuthUrlAsync_BuildsTikTokOAuthV2Url()
    {
        var provider = CreateProvider(new RecordingHandler(), CreateSettings());

        var url = await provider.GetAuthUrlAsync("state-123", "https://client/social-callback/tiktok");

        Assert.Contains("client_key=client-key", url);
        Assert.Contains("scope=user.info.basic", url);
        Assert.Contains("video.publish", url);
        Assert.Contains("response_type=code", url);
        Assert.Contains("state=state-123", url);
        Assert.Contains("redirect_uri=https%3A%2F%2Fclient%2Fsocial-callback%2Ftiktok", url);
    }

    [Fact]
    public async Task ExchangeCodeAsync_ReturnsTokensAndValidatedOpenId()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token":"access-token",
          "refresh_token":"refresh-token",
          "open_id":"open-id",
          "expires_in":86400,
          "scope":"user.info.basic,video.publish"
        }
        """);
        handler.EnqueueJson(HttpStatusCode.OK, """
        {
          "data":{"user":{"open_id":"open-id","display_name":"AISAM TikTok","avatar_url":"https://cdn/avatar.jpg"}},
          "error":{"code":"ok","message":""}
        }
        """);
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.ExchangeCodeAsync("oauth-code", "https://client/social-callback/tiktok");

        Assert.Equal("tiktok", result.Provider);
        Assert.Equal("open-id", result.ProviderUserId);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.True(result.ExpiresAt > DateTime.UtcNow.AddHours(23));
        Assert.Equal("POST", handler.Requests[0].Method);
        Assert.Contains("/v2/oauth/token/", handler.Requests[0].Url);
        Assert.Contains("client_secret=client-secret", handler.Requests[0].Body);
        Assert.Equal("Bearer access-token", handler.Requests[1].Authorization);
    }

    [Fact]
    public async Task ExchangeCodeAsync_RejectsTokenWithoutVideoPublishScope()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """
        {
          "access_token":"access-token",
          "refresh_token":"refresh-token",
          "open_id":"open-id",
          "expires_in":86400,
          "scope":"user.info.basic"
        }
        """);
        var provider = CreateProvider(handler, CreateSettings());

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ExchangeCodeAsync("oauth-code", "https://client/social-callback/tiktok"));

        Assert.Contains("video.publish", error.Message);
        Assert.Contains("disconnect and reconnect", error.Message);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetTargetsAsync_ReturnsTikTokAccountAsLinkableTarget()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """
        {
          "data":{"user":{"open_id":"open-id","display_name":"AISAM TikTok","avatar_url":"https://cdn/avatar.jpg"}},
          "error":{"code":"ok","message":""}
        }
        """);
        var provider = CreateProvider(handler, CreateSettings());

        var target = Assert.Single(await provider.GetTargetsAsync("access-token"));

        Assert.Equal("open-id", target.ProviderTargetId);
        Assert.Equal("AISAM TikTok", target.Name);
        Assert.Equal("tiktok_account", target.Type);
        Assert.Equal("https://cdn/avatar.jpg", target.ProfilePictureUrl);
    }

    [Fact]
    public async Task GetAuthUrlAsync_ThrowsClearErrorWhenConfigurationIsMissing()
    {
        var provider = CreateProvider(new RecordingHandler(), new TikTokSettings());

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.GetAuthUrlAsync("state", "https://client/social-callback/tiktok"));

        Assert.Equal("TikTok integration is not configured.", error.Message);
    }

    [Fact]
    public async Task PublishAsync_UploadsVideoWithPrivateDirectPost()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(HttpStatusCode.OK, """
        {
          "data": {
            "privacy_level_options": ["SELF_ONLY"],
            "comment_disabled": false,
            "duet_disabled": true,
            "stitch_disabled": true
          },
          "error": {"code":"ok","message":""}
        }
        """);
        handler.EnqueueBytes(HttpStatusCode.OK, new byte[] { 1, 2, 3, 4 }, "video/mp4");
        handler.EnqueueJson(HttpStatusCode.OK, """
        {
          "data": {
            "publish_id": "publish-123",
            "upload_url": "https://open-upload.tiktokapis.com/video/upload-123"
          },
          "error": {"code":"ok","message":""}
        }
        """);
        handler.EnqueueJson(HttpStatusCode.Created, "{}");
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.PublishAsync(
            new AISAM.Data.Model.SocialAccount { UserAccessToken = "access-token" },
            new AISAM.Data.Model.SocialIntegration { AccessToken = "access-token" },
            new PostDto
            {
                Message = "AISAM TikTok test",
                VideoUrl = "https://res.cloudinary.com/demo/video/upload/sample.mp4"
            });

        Assert.True(result.Success);
        Assert.Equal("publish-123", result.ProviderPostId);
        Assert.Equal(4, handler.Requests.Count);
        Assert.Contains("/v2/post/publish/creator_info/query/", handler.Requests[0].Url);
        Assert.Equal("https://res.cloudinary.com/demo/video/upload/sample.mp4", handler.Requests[1].Url);
        Assert.Contains("/v2/post/publish/video/init/", handler.Requests[2].Url);
        Assert.Contains("\"privacy_level\":\"SELF_ONLY\"", handler.Requests[2].Body);
        Assert.Contains("\"source\":\"FILE_UPLOAD\"", handler.Requests[2].Body);
        Assert.Equal("PUT", handler.Requests[3].Method);
        Assert.Equal("bytes 0-3/4", handler.Requests[3].ContentRange);
        Assert.Equal("video/mp4", handler.Requests[3].ContentType);
    }

    [Fact]
    public async Task PublishAsync_RequiresVideo()
    {
        var handler = new RecordingHandler();
        var provider = CreateProvider(handler, CreateSettings());

        var result = await provider.PublishAsync(
            new AISAM.Data.Model.SocialAccount(),
            new AISAM.Data.Model.SocialIntegration(),
            new PostDto { Message = "Text only" });

        Assert.False(result.Success);
        Assert.Equal("TikTok Direct Post currently requires a video.", result.ErrorMessage);
        Assert.Empty(handler.Requests);
    }

    private static TikTokProvider CreateProvider(RecordingHandler handler, TikTokSettings settings) =>
        new(new HttpClient(handler), Options.Create(settings), NullLogger<TikTokProvider>.Instance);

    private static TikTokSettings CreateSettings() => new()
    {
        ClientKey = "client-key",
        ClientSecret = "client-secret",
        RedirectUri = "https://client/social-callback/tiktok"
    };

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        public List<RecordedRequest> Requests { get; } = new();

        public void EnqueueJson(HttpStatusCode statusCode, string json) =>
            _responses.Enqueue(new HttpResponseMessage(statusCode) { Content = new StringContent(json) });

        public void EnqueueBytes(HttpStatusCode statusCode, byte[] bytes, string contentType)
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            _responses.Enqueue(new HttpResponseMessage(statusCode) { Content = content });
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest
            {
                Method = request.Method.Method,
                Url = request.RequestUri?.ToString() ?? string.Empty,
                Body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken),
                Authorization = request.Headers.Authorization?.ToString() ?? string.Empty,
                ContentRange = request.Content?.Headers.ContentRange?.ToString() ?? string.Empty,
                ContentType = request.Content?.Headers.ContentType?.MediaType ?? string.Empty
            });
            return _responses.Dequeue();
        }
    }

    private sealed class RecordedRequest
    {
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Authorization { get; set; } = string.Empty;
        public string ContentRange { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}




