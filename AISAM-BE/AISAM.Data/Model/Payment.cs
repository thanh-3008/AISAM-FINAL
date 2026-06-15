using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("subscription_id")]
        public Guid? SubscriptionId { get; set; }

        [Column("workspace_id")]
        public Guid WorkspaceId { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [MaxLength(3)]
        [Column("currency")]
        public string Currency { get; set; } = "VND";

        [Required]
        [Column("status")]
        public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Pending;

        [Required]
        [Column("payment_type")]
        public PaymentTypeEnum PaymentType { get; set; } = PaymentTypeEnum.Subscription;

        [Column("credit_pack_code")]
        public CreditPackCodeEnum? CreditPackCode { get; set; }

        [Column("credit_amount")]
        public long? CreditAmount { get; set; }

        [MaxLength(50)]
        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [MaxLength(255)]
        [Column("transaction_id")]
        public string? TransactionId { get; set; }

        [MaxLength(500)]
        [Column("invoice_url")]
        public string? InvoiceUrl { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;
    }
}
