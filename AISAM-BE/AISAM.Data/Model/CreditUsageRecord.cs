using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model
{
    [Table("credit_usage_records")]
    public class CreditUsageRecord
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("workspace_id")]
        public Guid WorkspaceId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        // Immutable attribution at transaction time, not the user's current team.
        [Column("team_id")] public Guid? TeamId { get; set; }
        [Column("brand_id")] public Guid? BrandId { get; set; }
        [Column("integration_id")] public Guid? IntegrationId { get; set; }
        [Column("balance_before")] public long? BalanceBefore { get; set; }
        [Column("balance_after")] public long? BalanceAfter { get; set; }
        [Column("reference_id")] public Guid? ReferenceId { get; set; }

        [Column("ai_generation_id")]
        public Guid? AiGenerationId { get; set; }

        [Required]
        [Column("action")]
        public CreditActionEnum Action { get; set; }

        [Required]
        [Column("credits")]
        public long Credits { get; set; }

        [Required]
        [Column("status")]
        public CreditUsageStatusEnum Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("AiGenerationId")]
        public virtual AiGeneration? AiGeneration { get; set; }
    }
}
