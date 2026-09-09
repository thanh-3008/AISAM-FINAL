using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AISAM.Data.Model;
[Table("content_media")]
public class ContentMedia
{
    [Key,Column("id")] public Guid Id {get;set;}=Guid.NewGuid();
    [Column("content_id")] public Guid ContentId {get;set;}
    [Column("asset_id")] public Guid AssetId {get;set;}
    [Column("sort_order")] public int SortOrder {get;set;}
    [Column("is_cover")] public bool IsCover {get;set;}
    [Column("alt_text"),MaxLength(1000)] public string? AltText {get;set;}
    [Column("caption"),MaxLength(2000)] public string? Caption {get;set;}
    public Content Content {get;set;}=null!;
    public Asset Asset {get;set;}=null!;
}
[Table("publish_snapshots")]
public class PublishSnapshot
{
    [Key,Column("id")] public Guid Id {get;set;}=Guid.NewGuid();
    [Column("content_id")] public Guid ContentId {get;set;}
    [Column("workspace_id")] public Guid WorkspaceId {get;set;}
    [Column("version")] public Guid Version {get;set;}
    [Column("payload",TypeName="jsonb")] public string Payload {get;set;}="{}";
    [Column("checksum"),MaxLength(64)] public string Checksum {get;set;}="";
    [Column("created_by")] public Guid? CreatedBy {get;set;}
    [Column("created_at")] public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
    public Content Content {get;set;}=null!;
    public List<SnapshotMedia> Media {get;set;}=[];
}
[Table("snapshot_media")]
public class SnapshotMedia
{
    [Key,Column("id")] public Guid Id {get;set;}=Guid.NewGuid();
    [Column("snapshot_id")] public Guid SnapshotId {get;set;}
    [Column("asset_id")] public Guid? AssetId {get;set;}
    [Column("sort_order")] public int SortOrder {get;set;}
    [Column("url")] public string Url {get;set;}="";
    [Column("mime_type")] public string? MimeType {get;set;}
    [Column("is_cover")] public bool IsCover {get;set;}
    [Column("alt_text")] public string? AltText {get;set;}
    [Column("caption")] public string? Caption {get;set;}
    [Column("size_bytes")] public long? SizeBytes {get;set;}
    [Column("duration_seconds")] public decimal? DurationSeconds {get;set;}
    [Column("width")] public int? Width {get;set;}
    [Column("height")] public int? Height {get;set;}
    [Column("checksum")] public string? Checksum {get;set;}
    public PublishSnapshot Snapshot {get;set;}=null!;
    public Asset? Asset {get;set;}
}
[Table("post_media")]
public class PostMedia
{
    [Key,Column("id")] public Guid Id {get;set;}=Guid.NewGuid();
    [Column("post_id")] public Guid PostId {get;set;}
    [Column("snapshot_media_id")] public Guid SnapshotMediaId {get;set;}
    [Column("provider_media_id")] public string? ProviderMediaId {get;set;}
    [Column("status")] public string Status {get;set;}="Pending";
    [Column("error_code")] public string? ErrorCode {get;set;}
    public Post Post {get;set;}=null!;
    public SnapshotMedia SnapshotMedia {get;set;}=null!;
}
