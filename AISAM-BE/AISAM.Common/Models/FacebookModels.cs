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

    [JsonPropertyName("error_user_msg")]
    public string? ErrorUserMsg { get; set; }

    [JsonPropertyName("error_user_title")]
    public string? ErrorUserTitle { get; set; }

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

public sealed class FacebookPostInsightData
{
    [JsonPropertyName("impressions")]
    public long? Impressions { get; set; }

    [JsonPropertyName("reach")]
    public long? Reach { get; set; }

    [JsonPropertyName("views")]
    public long? Views { get; set; }

    [JsonPropertyName("engaged_users")]
    public long? EngagedUsers { get; set; }

    [JsonPropertyName("clicks")]
    public long? Clicks { get; set; }

    [JsonPropertyName("reactions")]
    public long? Reactions { get; set; }

    [JsonPropertyName("comments")]
    public long? Comments { get; set; }

    [JsonPropertyName("shares")]
    public long? Shares { get; set; }

    [JsonPropertyName("total_media_view_unique")]
    public long? TotalMediaViewUnique { get; set; }

    [JsonPropertyName("diagnostics")]
    public List<string> Diagnostics { get; set; } = new();
}

public sealed class FacebookPostInsightsResponse
{
    [JsonPropertyName("data")]
    public List<FacebookPostInsightMetric>? Data { get; set; }
}

public sealed class FacebookPostInsightMetric
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("period")]
    public string? Period { get; set; }

    [JsonPropertyName("values")]
    public List<FacebookPostInsightValue>? Values { get; set; }
}

public sealed class FacebookPostInsightValue
{
    [JsonPropertyName("value")]
    public object? Value { get; set; }
}

public sealed class FacebookPostEngagementResponse
{
    [JsonPropertyName("insights")]
    public FacebookPostInsightsResponse? Insights { get; set; }

    [JsonPropertyName("views")]
    public long? Views { get; set; }

    [JsonPropertyName("view_count")]
    public long? ViewCount { get; set; }

    [JsonPropertyName("reactions")]
    public FacebookSummaryEdge? Reactions { get; set; }

    [JsonPropertyName("comments")]
    public FacebookSummaryEdge? Comments { get; set; }

    [JsonPropertyName("shares")]
    public FacebookShareSummary? Shares { get; set; }
}

public sealed class FacebookSummaryEdge
{
    [JsonPropertyName("summary")]
    public FacebookCountSummary? Summary { get; set; }
}

public sealed class FacebookCountSummary
{
    [JsonPropertyName("total_count")]
    public long? TotalCount { get; set; }
}

public sealed class FacebookShareSummary
{
    [JsonPropertyName("count")]
    public long? Count { get; set; }
}

public sealed class FacebookSummaryListResponse
{
    [JsonPropertyName("summary")]
    public FacebookCountSummary? Summary { get; set; }
}

public sealed class FacebookPublishedPostsResponse
{
    [JsonPropertyName("data")]
    public List<FacebookPublishedPostData>? Data { get; set; }

    [JsonPropertyName("paging")]
    public FacebookPaging? Paging { get; set; }
}

public sealed class FacebookPublishedPostData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("created_time")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("permalink_url")]
    public string? PermalinkUrl { get; set; }

    [JsonPropertyName("promotable_id")]
    public string? PromotableId { get; set; }

    [JsonPropertyName("status_type")]
    public string? StatusType { get; set; }

    [JsonPropertyName("insights")]
    public FacebookPostInsightsResponse? Insights { get; set; }
}

public sealed class FacebookPaging
{
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
