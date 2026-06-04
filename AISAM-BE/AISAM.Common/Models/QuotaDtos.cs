namespace AISAM.Common.Models;

public sealed class QuotaSummaryDto
{
    public string PlanName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime WindowStart { get; set; }
    public DateTime? WindowEnd { get; set; }
    public int PromptQuotaLimit { get; set; }
    public int PromptUsage { get; set; }
    public int PromptRemaining { get; set; }
    public int PostQuotaLimit { get; set; }
    public int PostUsage { get; set; }
    public int PostRemaining { get; set; }
}
