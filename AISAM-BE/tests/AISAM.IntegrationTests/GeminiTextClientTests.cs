using AISAM.Common.Models;
using AISAM.Services.Service;
using Microsoft.Extensions.Options;
using System.Net;

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

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public bool WasCalled { get; private set; }
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(Response);
        }
    }
}
