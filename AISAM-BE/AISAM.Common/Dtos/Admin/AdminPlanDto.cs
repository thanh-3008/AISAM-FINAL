namespace AISAM.Common.Dtos.Admin;

public class AdminPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PlanType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public int CreditsPerCycle { get; set; }
    public int PostQuotaPerCycle { get; set; }
    public int MemberLimit { get; set; }
    public decimal MaxCreditBalance { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminCreatePlanRequest
{
    public string Name { get; set; } = string.Empty;
    public int PlanType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public string BillingCycle { get; set; } = "monthly";
    public int CreditsPerCycle { get; set; }
    public int PostQuotaPerCycle { get; set; }
    public int MemberLimit { get; set; }
    public decimal MaxCreditBalance { get; set; }
    public int SortOrder { get; set; }
}

public class AdminUpdatePlanRequest
{
    public string? Name { get; set; }
    public int? PlanType { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }
    public int? CreditsPerCycle { get; set; }
    public int? PostQuotaPerCycle { get; set; }
    public int? MemberLimit { get; set; }
    public decimal? MaxCreditBalance { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
