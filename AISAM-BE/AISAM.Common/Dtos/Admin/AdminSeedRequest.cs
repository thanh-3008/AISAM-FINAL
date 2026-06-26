namespace AISAM.Common.Dtos.Admin;

public class AdminUpdateRoleRequest
{
    public string Role { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class AdminUpdateUserStatusRequest
{
    public bool IsActive { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdminSeedDemoUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PlanType { get; set; }
}

public class AdminSeedBatchUsersRequest
{
    public int Count { get; set; } = 5;
    public string? PlanType { get; set; }
}
