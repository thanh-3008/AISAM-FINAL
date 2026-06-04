namespace AISAM.Common.Models;

public sealed class CreateCheckoutRequest
{
    public string PlanCode { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
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
}
