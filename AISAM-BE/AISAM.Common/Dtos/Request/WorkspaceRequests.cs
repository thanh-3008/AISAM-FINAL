using AISAM.Data.Enumeration;
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request;

public sealed class CreateWorkspaceRequest
{
    [Required]
    [MaxLength(255, ErrorMessage = "Name must not exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public WorkspaceTypeEnum WorkspaceType { get; set; }
}

public sealed class UpdateWorkspaceRequest
{
    [Required]
    [MaxLength(255, ErrorMessage = "Name must not exceed 255 characters")]
    public string Name { get; set; } = string.Empty;
}
