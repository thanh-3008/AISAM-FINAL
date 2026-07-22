using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class ProductUrlExtractRequest
{
    [Required]
    [MaxLength(2000)]
    public string Url { get; set; } = string.Empty;
}
