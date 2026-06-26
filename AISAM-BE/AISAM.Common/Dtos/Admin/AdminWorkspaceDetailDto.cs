namespace AISAM.Common.Dtos.Admin;

public class AdminWorkspaceDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public AdminWorkspaceOwnerDto Owner { get; set; } = new();
    public List<AdminWorkspaceMemberDto> Members { get; set; } = new();
    public AdminWorkspaceSubscriptionDto? Subscription { get; set; }
    public decimal CreditBalance { get; set; }
}

public class AdminWorkspaceOwnerDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

public class AdminWorkspaceMemberDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class AdminWorkspaceSubscriptionDto
{
    public Guid Id { get; set; }
    public string Plan { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class AdminUpdateWorkspaceStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
