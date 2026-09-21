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
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? CreatorId { get; set; }
    public string? CreatorName { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Platform { get; set; }
    public string? Type { get; set; }
    public string? Caption { get; set; }
    public string? RichTextJson { get; set; }
    public int? RichTextVersion { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}
