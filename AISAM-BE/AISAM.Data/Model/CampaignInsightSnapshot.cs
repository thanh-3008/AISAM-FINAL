using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("campaign_insight_snapshots")]
    public class CampaignInsightSnapshot
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("workspace_id")]
        public Guid WorkspaceId { get; set; }

        [Required]
        [Column("campaign_id")]
        public Guid CampaignId { get; set; }

        [Required]
        [Column("snapshot_date", TypeName = "date")]
        public DateTime SnapshotDate { get; set; }

        [Column("platform")]
        public string? Platform { get; set; }

        [Column("source")]
        public string? Source { get; set; }

        [Column("currency")]
        public string? Currency { get; set; }

        [Column("spend", TypeName = "numeric")]
        public decimal? Spend { get; set; }

        [Column("impressions")]
        public long? Impressions { get; set; }

        [Column("reach")]
        public long? Reach { get; set; }

        [Column("clicks")]
        public long? Clicks { get; set; }

        [Column("engagement")]
        public long? Engagement { get; set; }

        [Column("conversions", TypeName = "numeric")]
        public decimal? Conversions { get; set; }

        [Column("attributed_revenue", TypeName = "numeric")]
        public decimal? AttributedRevenue { get; set; }

        [Column("attribution_window")]
        public string? AttributionWindow { get; set; }

        [Column("is_partial")]
        public bool IsPartial { get; set; } = false;

        [Column("raw_data", TypeName = "jsonb")]
        public string? RawData { get; set; }

        [Column("synced_at")]
        public DateTime? SyncedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        [ForeignKey("CampaignId")]
        public virtual AdCampaign? AdCampaign { get; set; }
    }
}
