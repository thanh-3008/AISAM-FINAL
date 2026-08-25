using AISAM.Common.Models;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace AISAM.IntegrationTests;

public class FallbackTextProviderTests
{
    [Fact]
    public async Task GenerateWithOptionsAsync_PropagatesSameConfigurationThroughEveryProvider()
    {
        var handlers = Enumerable.Range(0, 5)
            .Select(index => new CapturingHandler(index == 4))
            .ToArray();
        var settings = Options.Create(new GeminiSettings
        {
            ApiKey = "primary-key",
            FallbackApiKey = "fallback-key-1",
            FallbackApiKey2 = "fallback-key-2",
            FallbackApiKey3 = "fallback-key-3",
            FallbackApiKey4 = "fallback-key-4",
            Model = "gemini-3.6-flash",
            MaxTokens = 8192,
            Temperature = 0.7
        });

        var provider = new FallbackTextProvider(
            new GeminiTextClient(new HttpClient(handlers[0]), settings),
            new FallbackGeminiTextClient(new HttpClient(handlers[1]), settings),
            new FallbackGeminiTextClient2(new HttpClient(handlers[2]), settings),
            new FallbackGeminiTextClient3(new HttpClient(handlers[3]), settings),
            new FallbackGeminiTextClient4(new HttpClient(handlers[4]), settings),
            NullLogger<FallbackTextProvider>.Instance);

        var result = await provider.GenerateWithOptionsAsync(
            "Create analytics",
            new GeminiGenerationOptions("application/json", 4096, "low"));

        Assert.Equal("ok", result);
        Assert.All(handlers, handler =>
        {
            using var body = JsonDocument.Parse(handler.RequestBody!);
            var config = body.RootElement.GetProperty("generationConfig");
            Assert.Equal(4096, config.GetProperty("maxOutputTokens").GetInt32());
            Assert.Equal(0.7, config.GetProperty("temperature").GetDouble());
            Assert.Equal("application/json", config.GetProperty("responseMimeType").GetString());
            Assert.Equal("low", config.GetProperty("thinkingConfig").GetProperty("thinkingLevel").GetString());
        });
    }

    private sealed class CapturingHandler(bool succeeds) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return succeeds
                ? new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"candidates":[{"content":{"parts":[{"text":"ok"}]}}]}""")
                }
                : new HttpResponseMessage(HttpStatusCode.BadGateway)
                {
                    Content = new StringContent("""{"error":{"message":"temporary failure"}}""")
                };
        }
    }
}
