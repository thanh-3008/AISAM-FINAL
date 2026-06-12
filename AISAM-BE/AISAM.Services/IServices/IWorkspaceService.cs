using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices;

public interface IWorkspaceService
{
    Task<GenericResponse<IReadOnlyList<WorkspaceResponseDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GenericResponse<WorkspaceResponseDto>> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<GenericResponse<WorkspaceResponseDto>> CreateAsync(Guid userId, CreateWorkspaceRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<WorkspaceResponseDto>> UpdateAsync(Guid id, Guid userId, UpdateWorkspaceRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> AdminSoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
