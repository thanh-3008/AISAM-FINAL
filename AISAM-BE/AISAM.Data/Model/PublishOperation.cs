using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;
[Table("publish_requests")]
public sealed class PublishRequest
{
    [Column("id")]public Guid Id{get;set;}=Guid.NewGuid();
    [Column("workspace_id")]public Guid WorkspaceId{get;set;}
    [Column("actor_id")]public Guid ActorId{get;set;}
    [Column("idempotency_key"),MaxLength(128)]public string IdempotencyKey{get;set;}="";
}

[Table("publish_operations")]
public sealed class PublishOperation
{
    [Column("id")]public Guid Id{get;set;}=Guid.NewGuid();
    [Column("workspace_id")]public Guid WorkspaceId{get;set;}
    [Column("content_id")]public Guid ContentId{get;set;}
    [Column("snapshot_id")]public Guid SnapshotId{get;set;}
    [Column("integration_id")]public Guid IntegrationId{get;set;}
    [Column("actor_id")]public Guid ActorId{get;set;}
    [Column("idempotency_key"),MaxLength(128)]public string IdempotencyKey{get;set;}="";
    [Column("status"),MaxLength(32)]public string Status{get;set;}="Queued";
    [Column("attempts")]public int Attempts{get;set;}
    [Column("provider_id")]public string? ProviderId{get;set;}
    [Column("error_code"),MaxLength(100)]public string? ErrorCode{get;set;}
    [Column("media_results",TypeName="jsonb")]public string MediaResults{get;set;}="[]";
    [Column("created_at")]public DateTime CreatedAt{get;set;}=DateTime.UtcNow;
    [Column("updated_at")]public DateTime UpdatedAt{get;set;}=DateTime.UtcNow;
    public PublishSnapshot Snapshot{get;set;}=null!;
}
