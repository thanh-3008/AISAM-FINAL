using AISAM.Data.Enumeration;

namespace AISAM.Common.Models;

public sealed class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public int MaxTokens { get; set; } = 2048;
    public double Temperature { get; set; } = 0.7;
    public string? OpenRouterApiKey { get; set; }
    public string? OpenRouterModel { get; set; }
    public string? FallbackApiKey { get; set; }
    public string? FallbackApiKey2 { get; set; }
    public string? FallbackApiKey3 { get; set; }
    public string? FallbackApiKey4 { get; set; }
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
    public string Content { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
}

public sealed class AiGenerationResponse
{
    public Guid AiGenerationId { get; set; }
    public Guid ContentId { get; set; }
    public string? GeneratedText { get; set; }
    public string? GeneratedImageUrl { get; set; }
    public string? GeneratedVideoUrl { get; set; }
    public string? VideoJobId { get; set; }
    public string? ProviderUsed { get; set; }
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
    public string? GenerationMode { get; set; }
    public string? UploadedPrimaryImageUrl { get; set; }
    public string? SelectedProductImageUrl { get; set; }
    public bool UseOriginalProductImages { get; set; }
    public string? UserPrompt { get; set; }
}

public sealed class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public Guid ConversationId { get; set; }
    public bool ShouldCreateContent { get; set; }
    public Guid? CreatedContentId { get; set; }
}

public sealed class ContentVariationResponse
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string[] Hashtags { get; set; } = Array.Empty<string>();
    public string? Rationale { get; set; }
}

public sealed class ParsedChatResponse
{
    public string Intent { get; set; }
    public bool ShouldCreateContent { get; set; }
    public string? Prompt { get; set; }
    public string? Response { get; set; }

    public ParsedChatResponse(string intent, bool shouldCreateContent, string? prompt, string? response)
    {
        Intent = intent;
        ShouldCreateContent = shouldCreateContent;
        Prompt = prompt;
        Response = response;
    }
}
