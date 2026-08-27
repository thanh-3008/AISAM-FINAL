using System.Text.Json;
using AISAM.Services.Service;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace AISAM.IntegrationTests;

public class DeApiVideoClientTests
{
    [Fact]
    public async Task StartAsync_UsesV2VideoGenerationEndpoint_WhenLegacyBaseUrlIsConfigured()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse(HttpStatusCode.OK, """{"data":{"request_id":"job-123"}}""");
        });
        var client = CreateClient(handler, "https://api.deapi.ai/api/v1");

        var result = await client.StartAsync("make a video", new VideoGenerationOptions(), CancellationToken.None);

        Assert.Equal(VideoGenerationStatus.Queued, result.Status);
        Assert.Equal("deapi:job-123", result.JobId);
        Assert.Equal("https://api.deapi.ai/api/v2/videos/generations", requestedUri?.ToString());
    }

    [Fact]
    public async Task StartAsync_UsesV2AnimationEndpoint_WithoutLtxOnlyParameters_ForMiniMax()
    {
        Uri? creationUri = null;
        HttpContent? creationContent = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) };

            creationUri = request.RequestUri;
            creationContent = request.Content;
            return JsonResponse(HttpStatusCode.OK, """{"data":{"request_id":"image-job"}}""");
        });
        var client = new DeApiVideoClient(
            new HttpClient(handler),
            Options.Create(new VideoProviderSettings
            {
                DeApiApiKey = "test-key",
                DeApiBaseUrl = "https://unused.example/api/v1",
                DeApiImg2VideoBaseUrl = "https://api.deapi.ai/api/v1",
                DeApiImg2VideoModel = "MiniMaxH3_33B_Turbo_INT8"
            }),
            NullLogger<DeApiVideoClient>.Instance);

        var result = await client.StartAsync("animate", new VideoGenerationOptions
        {
            FirstFrameImageUrl = "https://images.example/frame.jpg"
        }, CancellationToken.None);
        var multipartBody = await creationContent!.ReadAsStringAsync();

        Assert.Equal(VideoGenerationStatus.Queued, result.Status);
        Assert.Equal("https://api.deapi.ai/api/v2/videos/animations", creationUri?.ToString());
        Assert.DoesNotContain("name=steps", multipartBody);
        Assert.DoesNotContain("name=guidance", multipartBody);
    }

    [Fact]
    public async Task PollAsync_StripsInternalPrefix_AndStopsOnPermanent404()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse(HttpStatusCode.NotFound, """{"message":"No query results for model [JobRequest]."}""");
        });
        var client = CreateClient(handler);

        var result = await client.PollAsync("deapi:job-404", CancellationToken.None);

        Assert.Equal(VideoGenerationStatus.Failed, result.Status);
        Assert.Contains("was not found", result.ErrorMessage);
        Assert.Equal("https://api.deapi.ai/api/v2/jobs/job-404", requestedUri?.ToString());
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task StartAsync_ReturnsRetryGuidance_WhenProviderRateLimitsCreation()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var response = JsonResponse(HttpStatusCode.TooManyRequests, """{"message":"Too Many Attempts."}""");
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(90));
            return response;
        });
        var client = CreateClient(handler);

        var result = await client.StartAsync("make a video", new VideoGenerationOptions(), CancellationToken.None);

        Assert.Equal(VideoGenerationStatus.Failed, result.Status);
        Assert.Equal("DeAPI rate limit reached. Retry after 90 seconds.", result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task PollAsync_KeepsJobProcessing_WhenProviderRateLimitsStatusCheck()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var response = JsonResponse(HttpStatusCode.TooManyRequests, """{"message":"Too Many Attempts."}""");
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(60));
            return response;
        });
        var client = CreateClient(handler);

        var result = await client.PollAsync("deapi:rate-limited-job", CancellationToken.None);

        Assert.Equal(VideoGenerationStatus.Processing, result.Status);
        Assert.Equal("deapi:rate-limited-job", result.JobId);
        Assert.Equal(1, handler.CallCount);

        var deferredResult = await client.PollAsync("deapi:rate-limited-job", CancellationToken.None);
        Assert.Equal(VideoGenerationStatus.Processing, deferredResult.Status);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task PollAsync_PreservesRateLimit_WhenFallbackKeyReturnsNotFound()
    {
        var handler = new StubHttpMessageHandler(request =>
            request.Headers.Authorization?.Parameter == "primary-key"
                ? JsonResponse(HttpStatusCode.TooManyRequests, """{"message":"Too Many Attempts."}""")
                : JsonResponse(HttpStatusCode.NotFound, "{\"message\":\"JobRequest not found.\"}"));
        var client = new DeApiVideoClient(
            new HttpClient(handler),
            Options.Create(new VideoProviderSettings
            {
                DeApiApiKey = "primary-key",
                DeApiApiKeyFallback = "fallback-key",
                DeApiBaseUrl = "https://api.deapi.ai/api/v2"
            }),
            NullLogger<DeApiVideoClient>.Instance);

        var result = await client.PollAsync("deapi:mixed-rate-limit", CancellationToken.None);

        Assert.Equal(VideoGenerationStatus.Processing, result.Status);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task PollAsync_ReturnsCompletedVideo_FromV2DoneResponse()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(
            HttpStatusCode.OK,
            """{"data":{"status":"done","result_url":"https://results.deapi.ai/video.mp4"}}"""));
        var client = CreateClient(handler);

        var result = await client.PollAsync("deapi:completed-job", CancellationToken.None);

        Assert.Equal(VideoGenerationStatus.Done, result.Status);
        Assert.Equal("https://results.deapi.ai/video.mp4", result.MediaUrl);
    }

    [Fact]
    public void TryExtractVideoUrl_Should_Parse_ResultUrl_Field()
    {
        var json = """
        {"data":{"status":"done","preview":null,"result_url":"https://results.deapi.ai/17910524/tLhNdEoJY.mp4"}}
        """;
        using var doc = JsonDocument.Parse(json);
        var url = DeApiVideoClient.TryExtractVideoUrl(doc.RootElement);

        Assert.Equal("https://results.deapi.ai/17910524/tLhNdEoJY.mp4", url);
    }

    [Fact]
    public void TryExtractVideoUrl_Should_Return_Null_When_No_Url_Field_Exists()
    {
        var json = """{"data":{"status":"done","preview":null}}""";
        using var doc = JsonDocument.Parse(json);
        var url = DeApiVideoClient.TryExtractVideoUrl(doc.RootElement);

        Assert.Null(url);
    }

    private static DeApiVideoClient CreateClient(HttpMessageHandler handler, string? baseUrl = "https://api.deapi.ai/api/v2")
    {
        return new DeApiVideoClient(
            new HttpClient(handler),
            Options.Create(new VideoProviderSettings
            {
                DeApiApiKey = "test-key",
                DeApiBaseUrl = baseUrl,
                DeApiModel = "Ltx2_3_22B_Dist_INT8"
            }),
            NullLogger<DeApiVideoClient>.Instance);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) => new(statusCode)
    {
        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
    };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public int CallCount { get; private set; }

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_handler(request));
        }
    }
}




