using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class UpdateAdCampaignRequest
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        public Guid? BrandId { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? ContentId { get; set; }

        public string? Targeting { get; set; }

        [MaxLength(255)]
        public string? AdAccountId { get; set; }

        [MaxLength(100)]
        public string? Objective { get; set; }

        public decimal? Budget { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
