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
