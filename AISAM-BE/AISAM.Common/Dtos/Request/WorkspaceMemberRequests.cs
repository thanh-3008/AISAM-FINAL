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

public sealed class UpdateWorkspaceMemberQuotaRequest
{
    [Required]
    public MemberQuotaModeEnum QuotaMode { get; set; } = MemberQuotaModeEnum.SharedPool;

    [Range(1, long.MaxValue)]
    public long? CreditLimit { get; set; }
}
