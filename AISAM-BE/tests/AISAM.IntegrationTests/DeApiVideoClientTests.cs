using System.Text.Json;
using AISAM.Services.Service;
using Xunit;

namespace AISAM.IntegrationTests;

public class DeApiVideoClientTests
{
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
}
