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
    [JsonPropertyName("date_start")]
    public string? DateStart { get; set; }

    [JsonPropertyName("date_stop")]
    public string? DateStop { get; set; }

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

    [JsonPropertyName("reach")]
    public string? Reach { get; set; }

    [JsonPropertyName("account_currency")]
    public string? AccountCurrency { get; set; }

    [JsonPropertyName("action_values")]
    public List<FacebookActionData>? ActionValues { get; set; }
}

public sealed class CampaignDailyInsightDto
{
    public DateTime Date { get; set; }
    public long Impressions { get; set; }
    public long? Reach { get; set; }
    public long Clicks { get; set; }
    public decimal Spend { get; set; }
    public decimal? Conversions { get; set; }
    public decimal? AttributedRevenue { get; set; }
    public string Currency { get; set; } = "VND";
    public string? AttributionWindow { get; set; }
    public bool IsPartial { get; set; }
    public string? RawData { get; set; }
}

public sealed class FacebookActionData
{
    [JsonPropertyName("action_type")]
    public string? ActionType { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

// ─── Page Insights Models ───

public sealed class FacebookPageInsightsResponse
{
    [JsonPropertyName("data")]
    public List<FacebookPageInsightValue>? Data { get; set; }
}

public sealed class FacebookPageInsightValue
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("period")]
    public string? Period { get; set; }

    [JsonPropertyName("values")]
    public List<FacebookInsightPeriodValue>? Values { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public sealed class FacebookInsightPeriodValue
{
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("end_time")]
    public string? EndTime { get; set; }
}
