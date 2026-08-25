using AISAM.Common.Models;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace AISAM.IntegrationTests;

public class GeminiTextClientTests
{
    [Fact]
    public async Task GenerateAsync_ThrowsClearErrorBeforeHttpCall_WhenApiKeyIsMissing()
    {
        var handler = new FakeHttpMessageHandler();
        var client = new GeminiTextClient(new HttpClient(handler), Options.Create(new GeminiSettings()));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GenerateAsync("Create an ad"));

        Assert.Equal("Gemini API key is not configured.", exception.Message);
        Assert.False(handler.WasCalled);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsTrimmedText_WhenGeminiResponseIsValid()
    {
        var handler = new FakeHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                      "candidates": [
                        {
                          "content": {
                            "parts": [
                              { "text": "  Generated ad copy  " }
                            ]
                          }
                        }
                      ]
                    }
                    """)
            }
        };
        var client = new GeminiTextClient(new HttpClient(handler), Options.Create(new GeminiSettings
        {
            ApiKey = "test-key"
        }));

        var result = await client.GenerateAsync("Create an ad");

        Assert.Equal("Generated ad copy", result);
        Assert.True(handler.WasCalled);
    }

    [Fact]
    public async Task GenerateAsync_UsesConfiguredDefaults_AndOmitsThinkingConfig()
    {
        var handler = CreateSuccessfulHandler();
        var client = new GeminiTextClient(new HttpClient(handler), Options.Create(new GeminiSettings
        {
            ApiKey = "test-key",
            MaxTokens = 8192,
            Temperature = 0.4
        }));

        await client.GenerateAsync("Create an ad");

        using var body = JsonDocument.Parse(handler.RequestBody!);
        var config = body.RootElement.GetProperty("generationConfig");
        Assert.Equal(8192, config.GetProperty("maxOutputTokens").GetInt32());
        Assert.Equal(0.4, config.GetProperty("temperature").GetDouble());
        Assert.Equal("text/plain", config.GetProperty("responseMimeType").GetString());
        Assert.False(config.TryGetProperty("thinkingConfig", out _));
    }

    [Fact]
    public async Task GenerateWithOptionsAsync_UsesPerCallOverrides()
    {
        var handler = CreateSuccessfulHandler();
        var client = new GeminiTextClient(new HttpClient(handler), Options.Create(new GeminiSettings
        {
            ApiKey = "test-key",
            MaxTokens = 8192,
            Temperature = 0.7
        }));

        await client.GenerateWithOptionsAsync(
            "Create analytics",
            new("application/json", 4096, "low"));

        using var body = JsonDocument.Parse(handler.RequestBody!);
        var config = body.RootElement.GetProperty("generationConfig");
        Assert.Equal(4096, config.GetProperty("maxOutputTokens").GetInt32());
        Assert.Equal(0.7, config.GetProperty("temperature").GetDouble());
        Assert.Equal("application/json", config.GetProperty("responseMimeType").GetString());
        Assert.Equal("low", config.GetProperty("thinkingConfig").GetProperty("thinkingLevel").GetString());
    }

    private static FakeHttpMessageHandler CreateSuccessfulHandler()
        => new()
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"candidates":[{"content":{"parts":[{"text":"ok"}]}}]}""")
            }
        };

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public bool WasCalled { get; private set; }
        public string? RequestBody { get; private set; }
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return Response;
        }
    }
}




