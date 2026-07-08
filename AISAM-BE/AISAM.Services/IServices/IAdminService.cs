using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IAdminService
    {
        Task<GenericResponse<PagedResult<UserListDto>>> GetUsersAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetUserDetailAsync(Guid adminUserId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SetUserStatusAsync(Guid adminUserId, Guid userId, bool isActive, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> DeleteUserAsync(Guid adminUserId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SetUserRoleAsync(Guid adminUserId, Guid userId, int role, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetAdminsAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetWorkspacesAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetWorkspaceDetailAsync(Guid adminUserId, Guid workspaceId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SetWorkspaceStatusAsync(Guid adminUserId, Guid workspaceId, int status, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> DeleteWorkspaceAsync(Guid adminUserId, Guid workspaceId, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetPaymentsAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetAllContentAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SetContentStatusAsync(Guid adminUserId, Guid contentId, int status, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> DeleteContentAsync(Guid adminUserId, Guid contentId, CancellationToken cancellationToken = default);
    }
}
