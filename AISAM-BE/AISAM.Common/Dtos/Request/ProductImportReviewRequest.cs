using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class ProductImportReviewRequest
{
    [Required]
    public Guid BrandId { get; set; }

    [Required]
    [MaxLength(255, ErrorMessage = "Product name must not exceed 255 characters")]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "Description must not exceed 4000 characters")]
    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public List<string> Images { get; set; } = new();

    [MaxLength(2000, ErrorMessage = "Source URL must not exceed 2000 characters")]
    public string? SourceUrl { get; set; }

    public List<string> Benefits { get; set; } = new();

    public List<string> Features { get; set; } = new();

    [MaxLength(2000, ErrorMessage = "Target audience must not exceed 2000 characters")]
    public string? TargetAudience { get; set; }

    [MaxLength(200, ErrorMessage = "Tone must not exceed 200 characters")]
    public string? Tone { get; set; }

    public List<string> Keywords { get; set; } = new();

    [MaxLength(500, ErrorMessage = "Recommended CTA must not exceed 500 characters")]
    public string? RecommendedCTA { get; set; }
}
