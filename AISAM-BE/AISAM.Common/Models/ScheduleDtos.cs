namespace AISAM.Common.Models;

public sealed class CreateContentScheduleRequest
{
    public Guid ContentId { get; set; }
    public Guid IntegrationId { get; set; }
    public DateTime ScheduledAt { get; set; }
}

public sealed class UpdateContentScheduleRequest
{
    public Guid? IntegrationId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public sealed class ContentScheduleDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public Guid ContentId { get; set; }
    public Guid IntegrationId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
