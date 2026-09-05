using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("actor_id")]
        public Guid ActorId { get; set; }

        [Column("workspace_id")]
        public Guid? WorkspaceId { get; set; }
        [Column("team_id")] public Guid? TeamId { get; set; }
        [Column("affected_user_id")] public Guid? AffectedUserId { get; set; }
        [Column("requested_by")] public Guid? RequestedBy { get; set; }
        [Column("approved_by")] public Guid? ApprovedBy { get; set; }
        [Column("executed_by_system")] public bool ExecutedBySystem { get; set; }
        [Column("reference_id")] public Guid? ReferenceId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("action_type")]
        public string ActionType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("target_table")]
        public string TargetTable { get; set; } = string.Empty;

        [Required]
        [Column("target_id")]
        public Guid TargetId { get; set; }

        [Column("old_values", TypeName = "jsonb")]
        public string? OldValues { get; set; }

        [Column("new_values", TypeName = "jsonb")]
        public string? NewValues { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ActorId")]
        public virtual User Actor { get; set; } = null!;
    }
}
