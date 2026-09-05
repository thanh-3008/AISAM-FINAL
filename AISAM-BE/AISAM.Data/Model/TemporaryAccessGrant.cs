using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

[Table("temporary_access_grants")]
public sealed class TemporaryAccessGrant
{
    [Key, Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
    [Column("workspace_id")] public Guid WorkspaceId { get; set; }
    [Column("task_id")] public Guid TaskId { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("granted_by")] public Guid GrantedBy { get; set; }
    [Column("granted_at")] public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    [Column("expires_at")] public DateTime ExpiresAt { get; set; }
    [Column("revoked_at")] public DateTime? RevokedAt { get; set; }
    [Required, MaxLength(1000), Column("reason")] public string Reason { get; set; } = string.Empty;
    // A grant supplements resource access; it never overrides the workspace role.
    [Column("can_edit")] public bool CanEdit { get; set; }
    public CollaborationTask Task { get; set; } = null!;
}
