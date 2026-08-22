namespace AISAM.Common.Models;

public class NotificationListItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? TargetId { get; set; }
    public string? TargetType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class NotificationDetailDto : NotificationListItemDto
{
    public Guid ProfileId { get; set; }
}

public sealed class UnreadNotificationCountDto
{
    public int Count { get; set; }
}
