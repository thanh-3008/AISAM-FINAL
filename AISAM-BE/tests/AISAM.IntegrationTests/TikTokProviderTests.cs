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
          "expires_in":86400
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

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest
            {
                Method = request.Method.Method,
                Url = request.RequestUri?.ToString() ?? string.Empty,
                Body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken),
                Authorization = request.Headers.Authorization?.ToString() ?? string.Empty
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
    }
}
