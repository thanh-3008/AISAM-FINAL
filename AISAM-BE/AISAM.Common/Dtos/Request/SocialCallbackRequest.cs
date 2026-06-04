namespace AISAM.Common.Dtos.Request;

public sealed class SocialCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
