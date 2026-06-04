using System.Text.Json.Serialization;

namespace AISAM.Common.Models;

public sealed class FacebookTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
}

public sealed class FacebookUserResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public sealed class FacebookPageResponse
{
    [JsonPropertyName("data")]
    public List<FacebookPageData>? Data { get; set; }
}

public sealed class FacebookPageData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
}

public sealed class FacebookPostResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public sealed class FacebookErrorResponse
{
    [JsonPropertyName("error")]
    public FacebookError? Error { get; set; }
}

public sealed class FacebookError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("error_subcode")]
    public int? ErrorSubcode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
