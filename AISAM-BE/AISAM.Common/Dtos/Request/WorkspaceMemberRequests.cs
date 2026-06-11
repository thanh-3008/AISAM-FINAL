using AISAM.Data.Enumeration;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class UpdateWorkspaceMemberRoleRequest
{
    [Required]
    public WorkspaceMemberRoleEnum Role { get; set; }
}

public sealed class TransferWorkspaceOwnershipRequest
{
    [Required]
    public Guid TargetMemberId { get; set; }
}
