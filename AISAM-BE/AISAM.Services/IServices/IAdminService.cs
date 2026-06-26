using AISAM.Common;
using AISAM.Common.Dtos.Admin;

namespace AISAM.Services.IServices;

public interface IAdminService
{
    Task<GenericResponse<AdminDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPagedResult<AdminUserListDto>>> GetUsersAsync(int page, int pageSize, string? searchTerm, string? sortBy, bool sortDescending, string? role, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminUserDetailDto>> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> UpdateUserRoleAsync(Guid userId, string role, string reason, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> UpdateUserStatusAsync(Guid userId, bool isActive, string reason, CancellationToken cancellationToken = default);
}
