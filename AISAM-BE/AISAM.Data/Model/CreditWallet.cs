using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("credit_wallets")]
    public class CreditWallet
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("workspace_id")]
        public Guid WorkspaceId { get; set; }

        [Required]
        [Column("balance")]
        public long Balance { get; set; }

        [Column("reserved_balance")]
        public long ReservedBalance { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;
    }
}
