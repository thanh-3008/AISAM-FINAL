using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response;

public sealed class AiGenerationListDto
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public string AiPrompt { get; set; } = string.Empty;
    public string? GeneratedImageUrl { get; set; }
    public string? GeneratedVideoUrl { get; set; }
    public string? GeneratedText { get; set; }
    public string? VideoJobId { get; set; }
    public string? ProviderName { get; set; }
    public AiStatusEnum Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
