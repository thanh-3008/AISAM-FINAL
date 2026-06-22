using AISAM.Data.Enumeration;

namespace AISAM.Common.Models;

public sealed class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public int MaxTokens { get; set; } = 2048;
    public double Temperature { get; set; } = 0.7;
}

public sealed class CreateDraftRequest
{
    public Guid BrandId { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public string Prompt { get; set; } = string.Empty;
}

public sealed class ImproveContentRequest
{
    public string Prompt { get; set; } = string.Empty;
}

public sealed class AiGenerationResponse
{
    public Guid AiGenerationId { get; set; }
    public Guid ContentId { get; set; }
    public string? GeneratedText { get; set; }
    public AiStatusEnum Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ChatRequest
{
    public Guid? BrandId { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
}

public sealed class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public Guid ConversationId { get; set; }
    public bool ShouldCreateContent { get; set; }
}
