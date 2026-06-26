using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

[Table("subscription_plans")]
public class SubscriptionPlan
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("plan_type")]
    public int PlanType { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [MaxLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "VND";

    [MaxLength(50)]
    [Column("billing_cycle")]
    public string BillingCycle { get; set; } = "monthly";

    [Column("credits_per_cycle")]
    public int CreditsPerCycle { get; set; }

    [Column("post_quota_per_cycle")]
    public int PostQuotaPerCycle { get; set; }

    [Column("member_limit")]
    public int MemberLimit { get; set; }

    [Column("max_credit_balance")]
    public decimal MaxCreditBalance { get; set; }

    [Column("features", TypeName = "jsonb")]
    public string? Features { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
