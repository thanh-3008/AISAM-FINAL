using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class ProductImportReviewRequest
{
    [Required]
    public Guid BrandId { get; set; }

    [Required]
    [MaxLength(255)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public List<string> Images { get; set; } = new();

    [MaxLength(2000)]
    public string? SourceUrl { get; set; }

    public List<string> Benefits { get; set; } = new();

    public List<string> Features { get; set; } = new();

    [MaxLength(2000)]
    public string? TargetAudience { get; set; }

    [MaxLength(50)]
    public string? Tone { get; set; }

    public List<string> Keywords { get; set; } = new();

    [MaxLength(500)]
    public string? RecommendedCTA { get; set; }
}
