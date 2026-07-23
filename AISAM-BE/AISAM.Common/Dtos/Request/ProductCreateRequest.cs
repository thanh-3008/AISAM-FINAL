using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class ProductCreateRequest
    {
        [Required]
        public Guid BrandId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(255)]
        public string? Category { get; set; }

        [MaxLength(2000)]
        public string? PrimaryUse { get; set; }

        [MaxLength(2000)]
        public string? Usp { get; set; }

        [MaxLength(2000)]
        public string? TargetAudience { get; set; }

        [MaxLength(4000)]
        public string? VisualIdentity { get; set; }

        [MaxLength(6000)]
        public string? KnowledgeProfile { get; set; }

        [MaxLength(2000)]
        public string? ProductUrl { get; set; }

        public decimal? Price { get; set; }

        public int Stock { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }
    }
}
