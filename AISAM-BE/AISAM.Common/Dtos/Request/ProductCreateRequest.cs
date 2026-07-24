using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class ProductCreateRequest
    {
        [Required(ErrorMessage = "Brand is required")]
        public Guid BrandId { get; set; }
        
        [Required]
        [MaxLength(255, ErrorMessage = "Name must not exceed 255 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
        public string? Description { get; set; }

        [MaxLength(255, ErrorMessage = "Category must not exceed 255 characters")]
        public string? Category { get; set; }

        [MaxLength(2000, ErrorMessage = "Primary use must not exceed 2000 characters")]
        public string? PrimaryUse { get; set; }

        [MaxLength(2000, ErrorMessage = "USP must not exceed 2000 characters")]
        public string? Usp { get; set; }

        [MaxLength(2000, ErrorMessage = "Target audience must not exceed 2000 characters")]
        public string? TargetAudience { get; set; }

        [MaxLength(4000, ErrorMessage = "Visual identity must not exceed 4000 characters")]
        public string? VisualIdentity { get; set; }

        [MaxLength(6000, ErrorMessage = "Knowledge profile must not exceed 6000 characters")]
        public string? KnowledgeProfile { get; set; }

        [MaxLength(2000, ErrorMessage = "Product URL must not exceed 2000 characters")]
        public string? ProductUrl { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
        public int Stock { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }
    }
}
