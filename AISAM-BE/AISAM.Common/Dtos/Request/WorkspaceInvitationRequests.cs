using AISAM.Data.Enumeration;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class CreateWorkspaceInvitationRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public WorkspaceMemberRoleEnum Role { get; set; }

    public MemberQuotaModeEnum QuotaMode { get; set; } = MemberQuotaModeEnum.SharedPool;

    [Range(1, long.MaxValue)]
    public long? CreditLimit { get; set; }
}

public sealed class AcceptWorkspaceInvitationRequest
{
    [Required]
    [MaxLength(500, ErrorMessage = "Token must not exceed 500 characters")]
    public string Token { get; set; } = string.Empty;
}
