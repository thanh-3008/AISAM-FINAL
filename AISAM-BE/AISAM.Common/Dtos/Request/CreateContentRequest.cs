using AISAM.Data.Enumeration;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class CreateContentRequest
{
    public Guid BrandId { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    [MaxLength(255, ErrorMessage = "Title must not exceed 255 characters")]
    public string? Title { get; set; }
    public string TextContent { get; set; } = string.Empty;
    /// <summary>
    /// Legacy single image URL (backward compat). If ImageUrls is provided, it takes precedence.
    /// </summary>
    public string? ImageUrl { get; set; }
    /// <summary>
    /// Multi-image support: ordered list of image URLs, max 5.
    /// When provided, serialized as JSON array into image_url JSONB column.
    /// </summary>
    [MaxLength(5, ErrorMessage = "Maximum 5 images allowed per post.")]
    public List<string>? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? StyleDescription { get; set; }
    public string? ContextDescription { get; set; }
    public string? RepresentativeCharacter { get; set; }
    public ContentStatusEnum? Status { get; set; }
    public bool IsAiGenerated { get; set; }
    public List<string>? Tags { get; set; }
}

