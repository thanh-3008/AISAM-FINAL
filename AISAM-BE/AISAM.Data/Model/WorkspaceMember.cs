using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model
{
    [Table("workspace_members")]
    public class WorkspaceMember
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

        [Required]
        [Column("role")]
        public WorkspaceMemberRoleEnum Role { get; set; }

        [Required]
        [Column("quota_mode")]
        public MemberQuotaModeEnum QuotaMode { get; set; } = MemberQuotaModeEnum.SharedPool;

        [Column("credit_limit")]
        public long? CreditLimit { get; set; }

        [Column("credit_used")]
        public long CreditUsed { get; set; }

        [Column("credit_period_start", TypeName = "date")]
        public DateTime? CreditPeriodStart { get; set; }

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
