using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class RejectContentRequest
{
    [Required(ErrorMessage = "Rejection notes are required.")]
    [MinLength(5, ErrorMessage = "Rejection notes must be at least 5 characters.")]
    [MaxLength(1000, ErrorMessage = "Notes must not exceed 1000 characters")]
    public string? Notes { get; set; }
}
