using AISAM.Data.Model;

namespace AISAM.Common.Dtos.Response;

public sealed class ConversationDetailDto : ConversationResponseDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public sealed class ChatMessageDto
{
    public Guid Id { get; set; }
    public ChatSenderType SenderType { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? AiGenerationId { get; set; }
    public Guid? ContentId { get; set; }
    public DateTime CreatedAt { get; set; }
}
