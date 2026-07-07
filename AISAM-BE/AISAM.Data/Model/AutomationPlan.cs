using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISAM.Data.Enumeration;

namespace AISAM.Data.Model;

[Table("automation_plans")]
public sealed class AutomationPlan
{
    [Key, Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
    [Column("workspace_id")] public Guid WorkspaceId { get; set; }
    [Column("profile_id")] public Guid ProfileId { get; set; }
    [MaxLength(200), Column("name")] public string Name { get; set; } = string.Empty;
    [MaxLength(255), Column("source_file_name")] public string? SourceFileName { get; set; }
    [MaxLength(80), Column("timezone")] public string Timezone { get; set; } = "UTC";
    [Column("status")] public AutomationPlanStatusEnum Status { get; set; } = AutomationPlanStatusEnum.Uploaded;
    [Column("total_items")] public int TotalItems { get; set; }
    [Column("valid_items")] public int ValidItems { get; set; }
    [Column("failed_items")] public int FailedItems { get; set; }
    [Column("estimated_credits")] public int EstimatedCredits { get; set; }
    [Column("reserved_credits")] public int ReservedCredits { get; set; }
    [Column("used_credits")] public int UsedCredits { get; set; }
    [Column("released_credits")] public int ReleasedCredits { get; set; }
    [Column("auto_approve")] public bool AutoApprove { get; set; }
    [Column("template_source_plan_id")] public Guid? TemplateSourcePlanId { get; set; }
    [Column("is_deleted")] public bool IsDeleted { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Column("confirmed_at")] public DateTime? ConfirmedAt { get; set; }

    public ICollection<AutomationItem> Items { get; set; } = new List<AutomationItem>();
}
