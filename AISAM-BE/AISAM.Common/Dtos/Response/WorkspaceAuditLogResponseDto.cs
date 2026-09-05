namespace AISAM.Common.Dtos.Response;

public sealed class WorkspaceAuditLogResponseDto
{
    public Guid Id { get; set; }
    public Guid ActorId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? AffectedUserId { get; set; }
    public Guid? RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public bool ExecutedBySystem { get; set; }
    public Guid? ReferenceId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public DateTime CreatedAt { get; set; }
}
