using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class ProductUrlExtractRequest
{
    [Required]
    [MaxLength(2000, ErrorMessage = "URL must not exceed 2000 characters")]
    public string Url { get; set; } = string.Empty;
}
