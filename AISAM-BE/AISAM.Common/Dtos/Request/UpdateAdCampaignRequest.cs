using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class UpdateAdCampaignRequest
    {
        [MaxLength(255, ErrorMessage = "Name must not exceed 255 characters")]
        public string? Name { get; set; }

        public Guid? BrandId { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? ContentId { get; set; }

        public string? Targeting { get; set; }

        [MaxLength(255, ErrorMessage = "Ad account ID must not exceed 255 characters")]
        public string? AdAccountId { get; set; }

        [MaxLength(100, ErrorMessage = "Objective must not exceed 100 characters")]
        public string? Objective { get; set; }

        public decimal? Budget { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }

        [MaxLength(500, ErrorMessage = "Landing URL must not exceed 500 characters")]
        public string? LandingUrl { get; set; }
    }
}
