using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

[Table("holiday_events")]
public class HolidayEvent
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("local_name")]
    public string? LocalName { get; set; }

    [Required]
    [Column("exact_date")]
    public DateTime ExactDate { get; set; }

    [Required]
    [Column("year")]
    public int Year { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("country_code")]
    public string CountryCode { get; set; } = "VN";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("is_manually_overridden")]
    public bool IsManuallyOverridden { get; set; } = false;
}
