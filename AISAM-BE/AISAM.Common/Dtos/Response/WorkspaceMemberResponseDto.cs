using AISAM.Data.Enumeration;

namespace AISAM.Common.Dtos.Response;

public sealed class WorkspaceMemberResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public WorkspaceMemberRoleEnum Role { get; set; }
    public MemberQuotaModeEnum QuotaMode { get; set; }
    public long? CreditLimit { get; set; }
    public long CreditUsed { get; set; }
    public DateTime? CreditPeriodStart { get; set; }
    public DateTime JoinedAt { get; set; }
}
