using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AISAM.Common.Models;

public sealed class CreateAutomationPlanRequest
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(80)] public string Timezone { get; set; } = "UTC";
    public List<AutomationImportRowRequest> Rows { get; set; } = new();
}

public sealed class AutomationImportRowRequest
{
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public List<string> Platforms { get; set; } = new();
    public string ContentType { get; set; } = "Auto";
    public string? Tone { get; set; }
    public string? Cta { get; set; }
    public string? Notes { get; set; }
    public DateTime ScheduledAt { get; set; }
}

public sealed class AutomationPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SourceFileName { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string Status { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int ValidItems { get; set; }
    public int FailedItems { get; set; }
    public int EstimatedCredits { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ReservedCredits { get; set; }
    public int UsedCredits { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ReleasedCredits { get; set; }
    public bool AutoApprove { get; set; }
    public Guid? TemplateSourcePlanId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public List<AutomationItemDto> Items { get; set; } = new();
}

public sealed class AutomationItemDto
{
    public Guid Id { get; set; }
    public int RowIndex { get; set; }
    public string Platform { get; set; } = string.Empty;
    public Guid? BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public Guid? ContentId { get; set; }
    public Guid? ContentCalendarId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? Tone { get; set; }
    public string? Cta { get; set; }
    public string? Notes { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int EstimatedCredits { get; set; }
    public int UsedCredits { get; set; }
    public int GenerationAttemptCount { get; set; }
    public string? LastError { get; set; }
    public string? GeneratedText { get; set; }
    public string? GeneratedImageUrl { get; set; }
    public string? GeneratedVideoUrl { get; set; }
    public string? VideoProvider { get; set; }
    public IReadOnlyList<AutomationValidationError> ValidationErrors { get; set; } = Array.Empty<AutomationValidationError>();
}

public sealed class AutomationValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class RejectAutomationItemRequest
{
    public string? Notes { get; set; }
}

public sealed class ImportGoogleSheetRequest
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [Required, Url] public string Url { get; set; } = string.Empty;
    [MaxLength(80)] public string Timezone { get; set; } = "UTC";
}

public sealed class CloneAutomationPlanRequest
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [Range(1, 3650)] public int ShiftDays { get; set; } = 7;
}

public sealed class SetAutomationAutoApproveRequest
{
    public bool Enabled { get; set; }
}

public sealed class UpdateAutomationItemRequest
{
    public Guid? BrandId { get; set; }
    public Guid? ProductId { get; set; }
    [Required, MaxLength(300)] public string Topic { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Platform { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string ContentType { get; set; } = string.Empty;
    [MaxLength(100)] public string? Objective { get; set; }
    [MaxLength(100)] public string? Tone { get; set; }
    [MaxLength(300)] public string? Cta { get; set; }
    public string? Notes { get; set; }
    public DateTime ScheduledAt { get; set; }
}

public sealed class AutomationPerformanceDto
{
    public Guid PlanId { get; set; }
    public int TotalItems { get; set; }
    public int ScheduledItems { get; set; }
    public int PublishedItems { get; set; }
    public int FailedItems { get; set; }
    public long Impressions { get; set; }
    public long Engagement { get; set; }
    public decimal AverageCtr { get; set; }
    public decimal EstimatedRevenue { get; set; }
}

public sealed class AutomationTargetDto
{
    public Guid IntegrationId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public bool IsScheduled { get; set; }
    public Guid? ScheduleId { get; set; }
}

public sealed class ApproveAutomationTargetsRequest
{
    [MinLength(1)] public List<Guid> IntegrationIds { get; set; } = [];
}
