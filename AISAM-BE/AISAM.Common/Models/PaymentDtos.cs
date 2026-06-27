using AISAM.Data.Enumeration;

namespace AISAM.Common.Models;

public sealed class CreateCheckoutRequest
{
    public PaymentTypeEnum PaymentType { get; set; } = PaymentTypeEnum.Subscription;
    public string PlanCode { get; set; } = string.Empty;
    public CreditPackCodeEnum? CreditPackCode { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public sealed class CreateBusinessWorkspaceCheckoutRequest
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public sealed class SynchronizeBusinessWorkspaceCheckoutRequest
{
    public string Reference { get; set; } = string.Empty;
}

public sealed class PayOSCheckoutResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string? PaymentLinkId { get; set; }
    public string? OrderCode { get; set; }
}

public sealed class PaymentHistoryItemDto
{
    public Guid Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CurrentSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PromptQuota { get; set; }
    public int ImageQuota { get; set; }
    public int PostQuota { get; set; }
    public int PlatformQuota { get; set; }
    public int AccountQuota { get; set; }
    public int AnalysisLevel { get; set; }
    public decimal AdBudgetMonthly { get; set; }
    public int AdCampaignQuota { get; set; }
}
