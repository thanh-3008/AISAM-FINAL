using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

[Table("campaign_insight_snapshots")]
public sealed class CampaignInsightSnapshot
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [Column("campaign_id")]
    public Guid CampaignId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("platform")]
    public string Platform { get; set; } = "facebook";

    [Column("snapshot_date", TypeName = "date")]
    public DateTime SnapshotDate { get; set; }

    [Required]
    [MaxLength(3)]
    [Column("currency")]
    public string Currency { get; set; } = "VND";

    [Column("impressions")]
    public long Impressions { get; set; }

    [Column("reach")]
    public long? Reach { get; set; }

    [Column("clicks")]
    public long Clicks { get; set; }

    [Column("engagement")]
    public long? Engagement { get; set; }

    [Column("spend", TypeName = "decimal(18,2)")]
    public decimal Spend { get; set; }

    [Column("conversions", TypeName = "decimal(18,4)")]
    public decimal? Conversions { get; set; }

    [Column("attributed_revenue", TypeName = "decimal(18,2)")]
    public decimal? AttributedRevenue { get; set; }

    [MaxLength(50)]
    [Column("attribution_window")]
    public string AttributionWindow { get; set; } = "default";

    [Required]
    [MaxLength(50)]
    [Column("source")]
    public string Source { get; set; } = "facebook";

    [Column("is_partial")]
    public bool IsPartial { get; set; }

    [Column("synced_at")]
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

    [Column("raw_data", TypeName = "jsonb")]
    public string? RawData { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public AdCampaign Campaign { get; set; } = null!;
}
