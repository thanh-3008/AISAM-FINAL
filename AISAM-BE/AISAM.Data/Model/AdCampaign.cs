using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model
{
    [Table("ad_campaigns")]
    public class AdCampaign
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("profile_id")]
        public Guid ProfileId { get; set; }

        [Column("workspace_id")]
        public Guid WorkspaceId { get; set; }

        [Required]
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("ad_account_id")]
        public string AdAccountId { get; set; } = string.Empty;

        [Column("product_id")]
        public Guid? ProductId { get; set; }

        [Column("content_id")]
        public Guid? ContentId { get; set; }

        // Recorded from the actual deployment target. Legacy campaigns remain unattributed.
        [Column("integration_id")]
        public Guid? IntegrationId { get; set; }

        [Column("targeting", TypeName = "jsonb")]
        public string? Targeting { get; set; }

        [MaxLength(255)]
        [Column("facebook_campaign_id")]
        public string? FacebookCampaignId { get; set; }

        [MaxLength(10)]
        [Column("ad_account_currency")]
        public string? AdAccountCurrency { get; set; }

        [MaxLength(20)]
        [Column("platform")]
        public string Platform { get; set; } = "facebook";

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("objective")]
        public string? Objective { get; set; }

        [Column("budget", TypeName = "decimal(10,2)")]
        public decimal? Budget { get; set; }

        [Column("start_date", TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date", TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        [Column("landing_url")]
        public string? LandingUrl { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("status")]
        public CampaignStatusEnum Status { get; set; } = CampaignStatusEnum.Draft;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deployment_status")]
        public DeploymentStatusEnum DeploymentStatus { get; set; } = DeploymentStatusEnum.None;

        [Column("deployment_step")]
        public int DeploymentStep { get; set; }

        [MaxLength(2000)]
        [Column("deployment_message")]
        public string? DeploymentMessage { get; set; }

        // Insights
        [Column("impressions")]
        public long Impressions { get; set; }

        [Column("clicks")]
        public long Clicks { get; set; }

        [Column("spend", TypeName = "decimal(12,2)")]
        public decimal Spend { get; set; }

        [Column("conversions")]
        public long Conversions { get; set; }

        // Navigation properties
        [ForeignKey("ProfileId")]
        public virtual Profile Profile { get; set; } = null!;
        public virtual Workspace Workspace { get; set; } = null!;

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("ContentId")]
        public virtual Content? Content { get; set; }

        public virtual ICollection<AdSet> AdSets { get; set; } = new List<AdSet>();
    }
}
