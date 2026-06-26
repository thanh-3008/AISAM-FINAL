namespace AISAM.Common.Dtos.Admin;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalWorkspaces { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<AdminRecentUserDto> RecentUsers { get; set; } = new();
    public List<AdminRecentPaymentDto> RecentPayments { get; set; } = new();
}

public class AdminRecentUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminRecentPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
