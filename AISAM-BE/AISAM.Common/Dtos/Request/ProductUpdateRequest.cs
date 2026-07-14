using Microsoft.AspNetCore.Http;
using System.ComponentModel;

namespace AISAM.Common.Dtos.Request
{
    public class ProductUpdateRequestDto
    {
        public Guid? BrandId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? PrimaryUse { get; set; }
        public string? Usp { get; set; }
        public string? TargetAudience { get; set; }
        public string? VisualIdentity { get; set; }
        public string? KnowledgeProfile { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public List<IFormFile>? ImageFiles { get; set; } 
    }
}
