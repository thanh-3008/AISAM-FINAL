using AISAM.Services.Service;

namespace AISAM.IntegrationTests;

public class AiJsonResponseParserTests
{
    [Theory]
    [MemberData(nameof(ValidResponses))]
    public void Parse_ReturnsJson_WhenResponseContainsRecoverableJson(string response, string expectedJson)
    {
        var result = AiJsonResponseParser.Parse(response);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expectedJson, result.Json);
    }

    [Fact]
    public void Parse_AllowsTrailingCommas()
    {
        const string response = """
            {
              "name": "test",
              "items": [1, 2, 3,]
            }
            """;

        var result = AiJsonResponseParser.Parse(response);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(response, result.Json);
    }

    [Fact]
    public void Parse_RejectsTruncatedJson()
    {
        const string response = "{\"name\": \"test\",";

        var result = AiJsonResponseParser.Parse(response);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Json);
        Assert.Contains("truncated or unbalanced", result.ErrorMessage);
    }

    [Theory]
    [InlineData("This is not JSON.")]
    [InlineData("{\"name\": invalid}")]
    [InlineData("{\"name\": \"test\" /* comment */}")]
    public void Parse_RejectsContentThatIsNotValidJson(string response)
    {
        var result = AiJsonResponseParser.Parse(response);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Json);
        Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
    }

    public static TheoryData<string, string> ValidResponses => new()
    {
        {
            "{\"name\":\"test\",\"value\":123}",
            "{\"name\":\"test\",\"value\":123}"
        },
        {
            """
            ```json
            {"name":"test","value":123}
            ```
            """,
            "{\"name\":\"test\",\"value\":123}"
        },
        {
            """
            Here is the result:

            {"name":"test","value":123}
            """,
            "{\"name\":\"test\",\"value\":123}"
        },
        {
            """
            Here is the [requested] result:

            {"name":"test","value":123}
            """,
            "{\"name\":\"test\",\"value\":123}"
        },
        {
            """
            {"name":"test","value":123}

            I hope this helps.
            """,
            "{\"name\":\"test\",\"value\":123}"
        },
        {
            """
            Here is the result:

            {"name":"test","value":123}

            Let me know if you need anything else.
            """,
            "{\"name\":\"test\",\"value\":123}"
        },
        {
            "[{\"name\":\"A\"},{\"name\":\"B\"}]",
            "[{\"name\":\"A\"},{\"name\":\"B\"}]"
        },
        {
            "{\"message\":\"Example {text} with [brackets]\"}",
            "{\"message\":\"Example {text} with [brackets]\"}"
        },
        {
            "{\"message\":\"He said: \\\"hello {world}\\\"\"}",
            "{\"message\":\"He said: \\\"hello {world}\\\"\"}"
        }
    };
}
