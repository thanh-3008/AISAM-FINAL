using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response
{
    public class AdCampaignResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? ContentId { get; set; }
        public string? ContentTitle { get; set; }
        public string? Targeting { get; set; }
        public string AdAccountId { get; set; } = string.Empty;
        public string? AdAccountCurrency { get; set; }
        public string? FacebookCampaignId { get; set; }
        public string Platform { get; set; } = "facebook";
        public string Name { get; set; } = string.Empty;
        public string? Objective { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public CampaignStatusEnum Status { get; set; }
        public string? LandingUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DeploymentStatusEnum DeploymentStatus { get; set; }
        public int DeploymentStep { get; set; }
        public string? DeploymentMessage { get; set; }
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
        public List<AdSummaryDto> Ads { get; set; } = new();
    }

    public class AdSummaryDto
    {
        public Guid Id { get; set; }
        public string? AdId { get; set; }
        public string? Status { get; set; }
        public string? CreativeId { get; set; }
        public string? CallToAction { get; set; }
        public string? LinkUrl { get; set; }
    }
}
