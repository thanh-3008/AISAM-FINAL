using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(255)]
        [Column("category")]
        public string? Category { get; set; }

        [Column("primary_use")]
        public string? PrimaryUse { get; set; }

        [Column("usp")]
        public string? Usp { get; set; }

        [Column("target_audience")]
        public string? TargetAudience { get; set; }

        [Column("visual_identity")]
        public string? VisualIdentity { get; set; }

        [Column("knowledge_profile")]
        public string? KnowledgeProfile { get; set; }

        [MaxLength(2000)]
        [Column("product_url")]
        public string? ProductUrl { get; set; }

        [Column("price", TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Column("stock")]
        public int Stock { get; set; } = 0;

        [Column("images", TypeName = "jsonb")]
        public string? Images { get; set; } // JSON array of image URLs

        [Column("benefits")]
        public string? Benefits { get; set; }

        [Column("pain_points")]
        public string? PainPoints { get; set; }

        [Column("sku")]
        public string? Sku { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; } = null!;

        public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
    }
}
