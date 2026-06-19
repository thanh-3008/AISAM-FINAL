namespace AISAM.Common.Models
{
    public class AdSetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? FacebookAdSetId { get; set; }
        public decimal? DailyBudget { get; set; }
        public string Status { get; set; } = "PAUSED";
    }

    public class AdCampaignDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid ProfileId { get; set; }
        public Guid BrandId { get; set; }
        public string? BrandName { get; set; }
        public string AdAccountId { get; set; } = string.Empty;
        public string? FacebookCampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Objective { get; set; } = "TRAFFIC";
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string Status { get; set; } = "DRAFT";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<AdSetDto> AdSets { get; set; } = new();
    }

    public class CreateAdCampaignRequest
    {
        public Guid BrandId { get; set; }
        public string AdAccountId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Objective { get; set; } = "TRAFFIC";
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateAdCampaignRequest
    {
        public Guid? BrandId { get; set; }
        public string? AdAccountId { get; set; }
        public string? Name { get; set; }
        public string? Objective { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
    }
}
