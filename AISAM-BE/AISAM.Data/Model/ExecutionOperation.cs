using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

// Attribution is a snapshot, never a credential or a grant of authority.
[Table("execution_operations")]
public sealed class ExecutionOperation
{
    [Key, Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
    [Column("workspace_id")] public Guid WorkspaceId { get; set; }
    [Column("actor_user_id")] public Guid ActorUserId { get; set; }
    [Column("team_id")] public Guid? TeamId { get; set; }
    [Column("resource_id")] public Guid ResourceId { get; set; }
    [Column("resource_type"), MaxLength(50)] public string ResourceType { get; set; } = "";
    [Column("brand_id")] public Guid? BrandId { get; set; }
    [Column("integration_id")] public Guid? IntegrationId { get; set; }
    [Column("requested_action"), MaxLength(50)] public string RequestedAction { get; set; } = "";
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("reference_id")] public Guid ReferenceId { get; set; }
    [Column("approval_authority"), MaxLength(50)] public string? ApprovalAuthority { get; set; }
    [Column("approved_by")] public Guid? ApprovedBy { get; set; }
    [Column("approved_at")] public DateTime? ApprovedAt { get; set; }
    [Column("execution_policy"), MaxLength(80)] public string ExecutionPolicy { get; set; } = "UNRESOLVED_OQ_008";
    [Column("execution_version")] public int ExecutionVersion { get; set; } = 1;
    [Column("policy_version")] public int PolicyVersion { get; set; }
    [Column("enqueue_authorized_at")] public DateTime? EnqueueAuthorizedAt { get; set; }
}
