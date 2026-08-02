using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class SubmitBusinessKycRequest
{
    [Required]
    [StringLength(20, MinimumLength = 8)]
    public string TaxId { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string LegalBusinessName { get; set; } = string.Empty;
}
