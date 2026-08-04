namespace AISAM.Common.Models;

public sealed class PostDto
{
    public string Message { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<string>? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
    public string? LinkUrl { get; set; }
}

public sealed class PublishResultDto
{
    public bool Success { get; set; }
    public string? ProviderPostId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PostedAt { get; set; }
    public string? RefreshedTargetAccessToken { get; set; }
}
