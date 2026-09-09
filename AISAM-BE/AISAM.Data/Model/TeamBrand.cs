using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("team_brands")]
    public class TeamBrand
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("team_id")]
        public Guid TeamId { get; set; }

        [Required]
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Existing database value; unknown modes must not imply all-channel access.
        [Column("channel_access_mode")]
        public int ChannelAccessMode { get; set; }

        public virtual ICollection<TeamChannelAccess> Channels { get; set; } = new List<TeamChannelAccess>();

        // Navigation properties
        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; } = null!;

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; } = null!;
    }
}
