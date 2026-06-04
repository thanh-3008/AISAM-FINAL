namespace AISAM.Common.Models;

public sealed class PostListItemDto
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public Guid IntegrationId { get; set; }
    public string? ExternalPostId { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ContentTitle { get; set; }
    public string? BrandName { get; set; }
}
