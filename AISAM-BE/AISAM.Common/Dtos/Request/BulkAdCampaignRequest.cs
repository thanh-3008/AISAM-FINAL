using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class BulkCreateAdCampaignRequest
    {
        [Required]
        public List<CreateAdCampaignRequest> Items { get; set; } = new();
    }

    public class BulkDeleteAdCampaignRequest
    {
        [Required]
        public List<Guid> CampaignIds { get; set; } = new();
    }

    public class BulkDeployAdCampaignRequest
    {
        [Required]
        public List<Guid> CampaignIds { get; set; } = new();
    }

    public class BulkUpdateAdCampaignRequest
    {
        [Required]
        public List<BulkUpdateAdCampaignItem> Items { get; set; } = new();
    }

    public class BulkUpdateAdCampaignItem
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public UpdateAdCampaignRequest Request { get; set; } = null!;
    }
}
