using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response;

public class ConversationResponseDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int MessageCount { get; set; }
}
