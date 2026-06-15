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
}
