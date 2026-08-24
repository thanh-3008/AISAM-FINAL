using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response;

public sealed class ContentResponseDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Tags { get; set; }
    public string? StyleDescription { get; set; }
    public string? ContextDescription { get; set; }
    public string? RepresentativeCharacter { get; set; }
    public bool IsAiGenerated { get; set; }
    public ContentStatusEnum Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
