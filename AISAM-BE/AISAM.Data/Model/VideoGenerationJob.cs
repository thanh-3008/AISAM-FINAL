using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model;

[Table("video_generation_jobs")]
public class VideoGenerationJob
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
    [Column("original_prompt")]
    public string OriginalPrompt { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("provider")]
    public string Provider { get; set; } = string.Empty;

    [Column("is_fallback")]
    public bool IsFallback { get; set; }

    [Required]
    [Column("status")]
    public AiStatusEnum Status { get; set; } = AiStatusEnum.Pending;

    [MaxLength(255)]
    [Column("external_job_id")]
    public string? ExternalJobId { get; set; }

    [Column("segments_count")]
    public int? SegmentsCount { get; set; }

    [Column("current_segment")]
    public int? CurrentSegment { get; set; }

    [MaxLength(500)]
    [Column("video_url")]
    public string? VideoUrl { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    [ForeignKey("WorkspaceId")]
    public virtual Workspace Workspace { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
