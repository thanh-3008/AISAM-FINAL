namespace AISAM.Common.Models;

public sealed class FacebookSettings
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string GraphApiVersion { get; set; } = "v25.0";
    public string BaseUrl { get; set; } = "https://graph.facebook.com";
    public string OAuthUrl { get; set; } = "https://www.facebook.com";
    public List<string> RequiredPermissions { get; set; } = new()
    {
        "pages_manage_posts",
        "pages_read_engagement",
        "pages_read_user_content",
        "pages_show_list",
        "read_insights",
        "pages_manage_ads",
        "ads_management",
        "ads_read",
        "business_management"
    };
}
