using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model
{
    [Table("contents")]
    public class Content
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("profile_id")]
        public Guid ProfileId { get; set; }

        [Column("workspace_id")]
        public Guid WorkspaceId { get; set; }

        // Legacy rows intentionally remain null; never infer creator from ProfileId.
        [Column("primary_creator_id")]
        public Guid? PrimaryCreatorId { get; set; }

        public ICollection<ContentParticipation> Participations { get; set; } = new List<ContentParticipation>();

        [Required]
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Column("product_id")]
        public Guid? ProductId { get; set; }

        [Required]
        [Column("ad_type")]
        public AdTypeEnum AdType { get; set; }

        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        [Required]
        [Column("text_content")]
        public string TextContent { get; set; } = string.Empty;

        [Column("image_url", TypeName = "jsonb")]
		public string? ImageUrl { get; set; }

        [Column("tags", TypeName = "jsonb")]
        public string? Tags { get; set; }

        [MaxLength(500)]
        [Column("video_url")]
        public string? VideoUrl { get; set; }

        [Column("style_description")]
        public string? StyleDescription { get; set; }

        [Column("context_description")]
        public string? ContextDescription { get; set; }

        [Column("representative_character")]
        public string? RepresentativeCharacter { get; set; }

        [Required]
        [Column("status")]
        public ContentStatusEnum Status { get; set; } = ContentStatusEnum.Draft;

        [Column("is_ai_generated")]
        public bool IsAiGenerated { get; set; } = false;

        [MaxLength(100)]
        [Column("generated_source")]
        public string? GeneratedSource { get; set; }

        [MaxLength(1000)]
        [Column("platform_rejection_reason")]
        public string? PlatformRejectionReason { get; set; }

        [MaxLength(50)]
        [Column("rejected_platform")]
        public string? RejectedPlatform { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [MaxLength(500)]
        [Column("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProfileId")]
        public virtual Profile Profile { get; set; } = null!;
        public virtual Workspace Workspace { get; set; } = null!;

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public virtual ICollection<ContentCalendar> ContentCalendars { get; set; } = new List<ContentCalendar>();
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<AdCreative> AdCreatives { get; set; } = new List<AdCreative>();
        public virtual ICollection<AiGeneration> AiGenerations { get; set; } = new List<AiGeneration>();
    }
}
