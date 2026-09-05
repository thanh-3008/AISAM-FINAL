using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

public enum CollaborationTaskStatus { Pending, InProgress, Completed, Blocked }

[Table("collaboration_tasks")]
public sealed class CollaborationTask
{
    [Key, Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
    [Column("workspace_id")] public Guid WorkspaceId { get; set; }
    [Column("team_id")] public Guid TeamId { get; set; }
    [Column("content_id")] public Guid ContentId { get; set; }
    [Column("integration_id")] public Guid? IntegrationId { get; set; }
    [Column("assignee_id")] public Guid AssigneeId { get; set; }
    [Column("assigned_by")] public Guid AssignedBy { get; set; }
    [Required, MaxLength(255), Column("title")] public string Title { get; set; } = string.Empty;
    [Column("status")] public CollaborationTaskStatus Status { get; set; }
    [MaxLength(64), Column("blocked_reason")] public string? BlockedReason { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Content Content { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
