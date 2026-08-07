namespace AISAM.Common.Dtos.Response;

public class UserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SocialAccountsCount { get; set; }
    public string? FullName { get; set; }
    public int? Role { get; set; }
    public string? RoleName { get; set; }
    public bool? IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
}
