using AISAM.Data.Enumeration;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class CreateWorkspaceInvitationRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public WorkspaceMemberRoleEnum Role { get; set; }
}

public sealed class AcceptWorkspaceInvitationRequest
{
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
}
