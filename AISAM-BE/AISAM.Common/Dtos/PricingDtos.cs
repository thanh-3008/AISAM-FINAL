namespace AISAM.Common.Dtos;

public class SubscriptionPlanDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Credits { get; set; }
    public int PostsPerMonth { get; set; }
    public int Members { get; set; }
    public System.Collections.Generic.List<string> Features { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class CreditPackDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Credits { get; set; }
    public bool IsActive { get; set; } = true;
}
