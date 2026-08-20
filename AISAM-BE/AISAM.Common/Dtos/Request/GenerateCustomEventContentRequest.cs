using System.ComponentModel.DataAnnotations;
using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Request;

public class GenerateCustomEventContentRequest
{
    [Required]
    public Guid BrandId { get; set; }

    [Required]
    [MaxLength(200)]
    public string EventName { get; set; } = string.Empty;

    public AdTypeEnum AdType { get; set; } = AdTypeEnum.TextOnly;
}
