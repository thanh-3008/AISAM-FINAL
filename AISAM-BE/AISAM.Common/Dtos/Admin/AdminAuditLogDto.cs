namespace AISAM.Common.Dtos.Admin;

public class AdminAuditLogDto
{
    public Guid Id { get; set; }
    public Guid ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public string? Action { get; set; }
    public string? TargetTable { get; set; }
    public Guid TargetId { get; set; }
    public DateTime CreatedAt { get; set; }
}
