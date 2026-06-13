using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model
{
    [Table("content_calendar")]
    public class ContentCalendar
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("content_id")]
        public Guid ContentId { get; set; }

        [Required]
        [Column("scheduled_date")]
        public DateTime ScheduledDate { get; set; }

        [Column("scheduled_time")]
        public TimeSpan? ScheduledTime { get; set; }

        [MaxLength(50)]
        [Column("timezone")]
        public string Timezone { get; set; } = "UTC";

        [Column("repeat_type")]
        public RepeatTypeEnum RepeatType { get; set; } = RepeatTypeEnum.None;

        [Column("repeat_interval")]
        public int RepeatInterval { get; set; } = 1; // Every N days/weeks/months

        [Column("repeat_until")]
        public DateTime? RepeatUntil { get; set; } // Optional end date for recurring

        [Column("next_scheduled_date")]
        public DateTime? NextScheduledDate { get; set; } // For recurring schedules

        [Column("integration_ids")]
        public string? IntegrationIds { get; set; } // JSON array of selected integration IDs

        [Column("integration_id")]
        public Guid? IntegrationId { get; set; }

        [Column("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }

        [Column("executed_at")]
        public DateTime? ExecutedAt { get; set; }

        [Column("status")]
        public ScheduleStatusEnum Status { get; set; } = ScheduleStatusEnum.Pending;

        [Column("attempt_count")]
        public int AttemptCount { get; set; } = 0;

        [Column("last_error")]
        public string? LastError { get; set; }

        [Column("profile_id")]
        public Guid ProfileId { get; set; } // Profile who created the schedule

        [Column("workspace_id")]
        public Guid? WorkspaceId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ContentId")]
        public virtual Content Content { get; set; } = null!;

        [ForeignKey("IntegrationId")]
        public virtual SocialIntegration? Integration { get; set; }

        [ForeignKey("ProfileId")]
        public virtual Profile Profile { get; set; } = null!;
        public virtual Workspace? Workspace { get; set; }
    }
}
