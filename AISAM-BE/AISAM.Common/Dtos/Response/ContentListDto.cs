using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response;

public sealed class ContentListDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid WorkspaceId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public string? TextContent { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? PlatformRejectionReason { get; set; }
    public string? RejectedPlatform { get; set; }
    public ContentStatusEnum Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
