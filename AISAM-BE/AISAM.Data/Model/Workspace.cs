using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model
{
    [Table("workspaces")]
    public class Workspace
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("workspace_type")]
        public WorkspaceTypeEnum WorkspaceType { get; set; }

        [Required]
        [Column("status")]
        public WorkspaceStatusEnum Status { get; set; } = WorkspaceStatusEnum.Active;

        [Required]
        [Column("member_limit")]
        public int MemberLimit { get; set; } = 1;

        [Column("subscription_expired_at")]
        public DateTime? SubscriptionExpiredAt { get; set; }

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
        public virtual ICollection<WorkspaceInvitation> Invitations { get; set; } = new List<WorkspaceInvitation>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual CreditWallet? CreditWallet { get; set; }
        public virtual ICollection<CreditUsageRecord> CreditUsageRecords { get; set; } = new List<CreditUsageRecord>();
        public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
    }
}
