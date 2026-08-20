using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("brands")]
    public class Brand
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
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(500)]
        [Column("logo_url")]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        [Column("slogan")]
        public string? Slogan { get; set; }

        [Column("usp")]
        public string? Usp { get; set; } // Unique Selling Points

        [Column("target_audience")]
        public string? TargetAudience { get; set; }

        [Column("accent_color")]
        public string? AccentColor { get; set; }

        [Column("body_font")]
        public string? BodyFont { get; set; }

        [Column("brand_values")]
        public string? BrandValues { get; set; }

        [Column("heading_font")]
        public string? HeadingFont { get; set; }

        [Column("preferred_terms")]
        public string? PreferredTerms { get; set; }

        [Column("primary_color")]
        public string? PrimaryColor { get; set; }

        [Column("prohibited_terms")]
        public string? ProhibitedTerms { get; set; }

        [Column("secondary_color")]
        public string? SecondaryColor { get; set; }

        [Column("tone_of_voice")]
        public string? ToneOfVoice { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProfileId")]
        public virtual Profile Profile { get; set; } = null!;

        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; } = null!;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
        public virtual ICollection<SocialIntegration> SocialIntegrations { get; set; } = new List<SocialIntegration>();
        public virtual ICollection<AdCampaign> AdCampaigns { get; set; } = new List<AdCampaign>();
        public virtual ICollection<TeamBrand> TeamBrands { get; set; } = new List<TeamBrand>();
    }
}
