namespace AISAM.Common.Dtos.Response;

public sealed class ProductUrlExtractResponseDto
{
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public List<string> Images { get; set; } = new();
    public string SourceUrl { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public string? TargetAudience { get; set; }
    public string? Tone { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string? RecommendedCTA { get; set; }
    public string ImportStatus { get; set; } = "Draft";
}
