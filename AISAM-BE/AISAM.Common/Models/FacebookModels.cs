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

    [JsonPropertyName("error_data")]
    public FacebookErrorData? ErrorData { get; set; }
}

public sealed class FacebookErrorData
{
    [JsonPropertyName("blame_field_specs")]
    public List<List<string>>? BlameFieldSpecs { get; set; }
}

// ─── Marketing API Models ───

public sealed class FacebookAdAccountResponse
{
    [JsonPropertyName("data")]
    public List<FacebookAdAccountData>? Data { get; set; }
}

public sealed class FacebookAdAccountData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("account_status")]
    public int AccountStatus { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("balance")]
    public string? Balance { get; set; }
}

public sealed class FacebookCampaignCreateResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public sealed class FacebookAdSetCreateResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public sealed class FacebookAdCreativeCreateResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public sealed class FacebookAdCreateResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public sealed class FacebookCampaignInsightsResponse
{
    [JsonPropertyName("data")]
    public List<FacebookInsightData>? Data { get; set; }
}

public sealed class FacebookInsightData
{
    [JsonPropertyName("impressions")]
    public string? Impressions { get; set; }

    [JsonPropertyName("clicks")]
    public string? Clicks { get; set; }

    [JsonPropertyName("spend")]
    public string? Spend { get; set; }

    [JsonPropertyName("actions")]
    public List<FacebookActionData>? Actions { get; set; }

    [JsonPropertyName("ctr")]
    public string? Ctr { get; set; }

    [JsonPropertyName("cpc")]
    public string? Cpc { get; set; }
}

public sealed class FacebookActionData
{
    [JsonPropertyName("action_type")]
    public string? ActionType { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
