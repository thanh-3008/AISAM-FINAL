using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class RegisterDeviceTokenRequest
{
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Platform { get; set; } = "android";

    [MaxLength(128)]
    public string? DeviceName { get; set; }
}

public sealed class UnregisterDeviceTokenRequest
{
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;
}
