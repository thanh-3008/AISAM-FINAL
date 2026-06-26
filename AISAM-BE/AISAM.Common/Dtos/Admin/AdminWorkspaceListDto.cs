namespace AISAM.Common.Dtos.Admin;

public class AdminWorkspaceListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public decimal CreditBalance { get; set; }
    public DateTime CreatedAt { get; set; }
}
