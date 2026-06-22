namespace AISAM.Common.Dtos.Response
{
    public class AdCampaignResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string AdAccountId { get; set; } = string.Empty;
        public string? FacebookCampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Objective { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<AdSetSummaryDto> AdSets { get; set; } = new();
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public decimal Spend { get; set; }
        public long Conversions { get; set; }
    }

    public class AdSetSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? FacebookAdSetId { get; set; }
        public decimal? DailyBudget { get; set; }
        public string? Status { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public decimal Spend { get; set; }
    }
}
