using AISAM.Data.Enumeration;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class CreateWorkspaceRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public WorkspaceTypeEnum WorkspaceType { get; set; }
}

public sealed class UpdateWorkspaceRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? CompanyName { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }
}
