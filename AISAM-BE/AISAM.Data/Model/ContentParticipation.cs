using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

// Historical participation is retained after task completion/revocation (CP-2).
[Table("content_participations")]
public sealed class ContentParticipation
{
    [Key, Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
    [Column("workspace_id")] public Guid WorkspaceId { get; set; }
    [Column("content_id")] public Guid ContentId { get; set; }
    [Column("user_id")] public Guid UserId { get; set; }
    [Column("recorded_by")] public Guid RecordedBy { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Content Content { get; set; } = null!;
}
