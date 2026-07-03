using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Request;

public sealed class UpdateContentRequest
{
    public Guid? ProductId { get; set; }
    public AdTypeEnum? AdType { get; set; }
    public string? Title { get; set; }
    public string? TextContent { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? StyleDescription { get; set; }
    public string? ContextDescription { get; set; }
    public string? RepresentativeCharacter { get; set; }
    public ContentStatusEnum? Status { get; set; }
    public List<string>? Tags { get; set; }
}
