namespace AISAM.Common.Models;

public sealed class TikTokSettings
{
    public string ClientKey { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string OAuthUrl { get; set; } = "https://www.tiktok.com/v2/auth/authorize/";
    public string ApiBaseUrl { get; set; } = "https://open.tiktokapis.com";
    public List<string> RequiredScopes { get; set; } = new() { "user.info.basic", "video.publish" };
    public string DefaultPrivacyLevel { get; set; } = "SELF_ONLY";
    public List<string> AllowedMediaHosts { get; set; } = new() { "res.cloudinary.com" };
    public int MaxUploadSizeMb { get; set; } = 128;
}
