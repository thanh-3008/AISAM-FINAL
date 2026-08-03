namespace AISAM.Common.Dtos.Response
{
    public class CampaignPreviewDto
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Platform { get; set; } = "facebook";
        public string? Objective { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? CallToAction { get; set; } = "LEARN_MORE";
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
