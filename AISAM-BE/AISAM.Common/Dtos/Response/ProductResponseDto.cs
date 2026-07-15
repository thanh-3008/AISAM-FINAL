namespace AISAM.Common.Dtos.Response
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? PrimaryUse { get; set; }
        public string? Usp { get; set; }
        public string? TargetAudience { get; set; }
        public string? VisualIdentity { get; set; }
        public string? KnowledgeProfile { get; set; }
        public decimal? Price { get; set; }
        public int Stock { get; set; }
        public List<string>? Images { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
