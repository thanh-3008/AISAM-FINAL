namespace AISAM.Common.Dtos.Response
{
    public class BulkCampaignResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<BulkCampaignItemResult> Results { get; set; } = new();
    }

    public class BulkCampaignItemResult
    {
        public bool Success { get; set; }
        public Guid? CampaignId { get; set; }
        public string? Error { get; set; }
        public AdCampaignResponseDto? Campaign { get; set; }
    }
}
