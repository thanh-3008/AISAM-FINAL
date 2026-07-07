using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model;

[Table("automation_items")]
public sealed class AutomationItem
{
    [Key, Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
    [Column("automation_plan_id")] public Guid AutomationPlanId { get; set; }
    [Column("row_index")] public int RowIndex { get; set; }
    [MaxLength(30), Column("platform")] public string Platform { get; set; } = string.Empty;
    [MaxLength(64), Column("idempotency_key")] public string IdempotencyKey { get; set; } = string.Empty;
    [Column("brand_id")] public Guid BrandId { get; set; }
    [Column("product_id")] public Guid? ProductId { get; set; }
    [Column("content_id")] public Guid? ContentId { get; set; }
    [Column("content_calendar_id")] public Guid? ContentCalendarId { get; set; }
    [MaxLength(300), Column("topic")] public string Topic { get; set; } = string.Empty;
    [MaxLength(100), Column("objective")] public string? Objective { get; set; }
    [Column("requested_content_type")] public AutomationContentTypeEnum RequestedContentType { get; set; }
    [MaxLength(100), Column("tone")] public string? Tone { get; set; }
    [MaxLength(300), Column("cta")] public string? Cta { get; set; }
    [Column("notes")] public string? Notes { get; set; }
    [Column("scheduled_at")] public DateTime ScheduledAt { get; set; }
    [Column("status")] public AutomationItemStatusEnum Status { get; set; } = AutomationItemStatusEnum.Pending;
    [Column("estimated_credits")] public int EstimatedCredits { get; set; }
    [Column("used_credits")] public int UsedCredits { get; set; }
    [Column("validation_errors", TypeName = "jsonb")] public string? ValidationErrors { get; set; }
    [Column("source_json", TypeName = "jsonb")] public string SourceJson { get; set; } = "{}";
    [Column("generation_attempt_count")] public int GenerationAttemptCount { get; set; }
    [Column("last_error")] public string? LastError { get; set; }
    [MaxLength(500), Column("video_job_id")] public string? VideoJobId { get; set; }
    [MaxLength(100), Column("video_provider")] public string? VideoProvider { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AutomationPlanId))] public AutomationPlan AutomationPlan { get; set; } = null!;
    [ForeignKey(nameof(BrandId))] public Brand Brand { get; set; } = null!;
    [ForeignKey(nameof(ProductId))] public Product? Product { get; set; }
    [ForeignKey(nameof(ContentId))] public Content? Content { get; set; }
    [ForeignKey(nameof(ContentCalendarId))] public ContentCalendar? ContentCalendar { get; set; }
}
