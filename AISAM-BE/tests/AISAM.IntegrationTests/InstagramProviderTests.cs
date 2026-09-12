using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Services.Service;
using Microsoft.Extensions.Options;
using System.Net;

namespace AISAM.IntegrationTests;

public class InstagramProviderTests
{
    [Fact]
    public async Task GetAuthUrlAsync_RequestsPublishingPermission()
    {
        var provider = CreateProvider(new RecordingHandler());
        var url = await provider.GetAuthUrlAsync("state", "https://client/callback");
        Assert.Contains("instagram_content_publish", url);
    }

    [Fact]
    public async Task PublishAsync_Image_CreatesAndPublishesContainer()
    {
        var handler = new RecordingHandler();
        handler.Enqueue("""{"id":"container-1"}""");
        handler.Enqueue("""{"id":"media-1"}""");

        var result = await CreateProvider(handler).PublishAsync(Account(), Integration(), new PostDto
        {
            Message = "Hello Instagram",
            ImageUrl = "https://cdn.example/image.jpg"
        });

        Assert.True(result.Success);
        Assert.Equal("media-1", result.ProviderPostId);
        Assert.Contains("/ig-1/media", handler.Requests[0].Url);
        Assert.Contains("image_url=https%3A%2F%2Fcdn.example%2Fimage.jpg", handler.Requests[0].Body);
        Assert.Contains("/ig-1/media_publish", handler.Requests[1].Url);
        Assert.Contains("creation_id=container-1", handler.Requests[1].Body);
    }

    [Fact]
    public async Task PublishAsync_Reel_WaitsUntilFinishedThenPublishes()
    {
        var handler = new RecordingHandler();
        handler.Enqueue("""{"id":"reel-container"}""");
        handler.Enqueue("""{"status_code":"FINISHED"}""");
        handler.Enqueue("""{"id":"reel-media"}""");

        var result = await CreateProvider(handler).PublishAsync(Account(), Integration(), new PostDto
        {
            VideoUrl = "https://cdn.example/video.mp4",
            Message = "My reel"
        });

        Assert.True(result.Success);
        Assert.Contains("media_type=REELS", handler.Requests[0].Body);
        Assert.Contains("fields=status_code", handler.Requests[1].Url);
    }

    [Fact]
    public async Task PublishAsync_Carousel_CreatesChildrenParentAndPublishes()
    {
        var handler = new RecordingHandler();
        handler.Enqueue("""{"id":"child-1"}""");
        handler.Enqueue("""{"id":"child-2"}""");
        handler.Enqueue("""{"id":"parent-1"}""");
        handler.Enqueue("""{"id":"media-1"}""");

        var result = await CreateProvider(handler).PublishAsync(Account(), Integration(), new PostDto
        {
            ImageUrls = new() { "https://cdn.example/1.jpg", "https://cdn.example/2.jpg" },
            Message = "Album"
        });

        Assert.True(result.Success);
        Assert.Contains("is_carousel_item=true", handler.Requests[0].Body);
        Assert.Contains("media_type=CAROUSEL", handler.Requests[2].Body);
        Assert.Contains("children=child-1%2Cchild-2", handler.Requests[2].Body);
    }

    [Fact]
    public async Task VerifiedMultiVideoWaitsForEveryChildAndPreservesProviderIds()
    {
        var handler=new RecordingHandler();var integration=Integration();
        handler.Enqueue("""{"id":"v1"}""");handler.Enqueue("""{"status_code":"FINISHED"}""");
        handler.Enqueue("""{"id":"v2"}""");handler.Enqueue("""{"status_code":"FINISHED"}""");
        handler.Enqueue("""{"id":"parent"}""");handler.Enqueue("""{"id":"published"}""");
        var provider=new InstagramProvider(new HttpClient(handler),Options.Create(new InstagramSettings{AppId="test",AppSecret="test",RedirectUri="https://client.test/callback",VerifiedCarouselIntegrationIds=[integration.Id]}));
        var stages=new List<(string Stage,int Requests)>();
        var result=await provider.PublishAsync(Account(),integration,new(){Media=[new(Guid.NewGuid(),"https://cdn.test/1.mp4","video/mp4"),new(Guid.NewGuid(),"https://cdn.test/2.mp4","video/mp4")],Progress=(stage,items,ct)=>{stages.Add((stage,handler.Requests.Count));return Task.CompletedTask;}});
        Assert.True(result.Success);Assert.Equal(new[]{"v1","v2"},result.Media.Select(m=>m.ProviderMediaId));Assert.All(result.Media,m=>Assert.Equal("Published",m.Status));
        Assert.Contains("children=v1%2Cv2",handler.Requests[4].Body);
        Assert.Equal(new[]{("UploadingMedia",2),("UploadingMedia",4),("Publishing",5)},stages);
    }

    [Fact]
    public async Task FailedChildDoesNotPublishIncompleteCarousel()
    {
        var handler=new RecordingHandler();handler.Enqueue("""{"id":"first"}""");handler.Enqueue("{}");
        var result=await CreateProvider(handler).PublishAsync(Account(),Integration(),new(){Media=[new(Guid.NewGuid(),"https://cdn.test/1.jpg","image/jpeg"),new(Guid.NewGuid(),"https://cdn.test/2.jpg","image/jpeg")]});
        Assert.False(result.Success);Assert.Equal(2,handler.Requests.Count);Assert.Equal("Uploaded",result.Media[0].Status);Assert.Equal("Failed",result.Media[1].Status);
        Assert.DoesNotContain(handler.Requests,r=>r.Url.Contains("media_publish"));
    }

    private static InstagramProvider CreateProvider(RecordingHandler handler) => new(
        new HttpClient(handler),
        Options.Create(new InstagramSettings
        {
            AppId = "app-id", AppSecret = "app-secret", RedirectUri = "https://client/callback"
        }));

    private static SocialAccount Account() => new() { Platform = SocialPlatformEnum.Instagram, UserAccessToken = "user-token" };
    private static SocialIntegration Integration() => new()
    {
        Platform = SocialPlatformEnum.Instagram,
        ExternalId = "ig-1",
        AccessToken = "page-token"
    };

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        public List<RecordedRequest> Requests { get; } = new();
        public void Enqueue(string json) => _responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest(
                request.RequestUri!.ToString(),
                request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken)));
            return _responses.Dequeue();
        }
    }

    private sealed record RecordedRequest(string Url, string Body);
}




