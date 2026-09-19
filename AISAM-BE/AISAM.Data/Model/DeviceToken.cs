using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("device_tokens")]
    public class DeviceToken
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("profile_id")]
        public Guid ProfileId { get; set; }

        [Required]
        [MaxLength(512)]
        [Column("token")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        [Column("platform")]
        public string Platform { get; set; } = "android"; // "android", "ios", "web"

        [MaxLength(128)]
        [Column("device_name")]
        public string? DeviceName { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_active_at")]
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProfileId")]
        public virtual Profile Profile { get; set; } = null!;
    }
}
