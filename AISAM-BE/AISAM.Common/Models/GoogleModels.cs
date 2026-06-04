namespace AISAM.Common.Models;

public sealed class GoogleTokenResponse
{
    public string? access_token { get; set; }
    public int expires_in { get; set; }
    public string? refresh_token { get; set; }
}
