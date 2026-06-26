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
    Task<GenericResponse<AdminPagedResult<AdminWorkspaceListDto>>> GetWorkspacesAsync(int page, int pageSize, string? searchTerm, string? status, string? plan, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminWorkspaceDetailDto>> GetWorkspaceDetailAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPagedResult<AdminSubscriptionDto>>> GetSubscriptionsAsync(int page, int pageSize, string? status, string? plan, Guid? workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> UpdateSubscriptionAsync(Guid subscriptionId, string? plan, bool? isActive, DateTime? endDate, string reason, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPagedResult<AdminPaymentDto>>> GetPaymentsAsync(int page, int pageSize, string? status, Guid? userId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> UpdatePaymentStatusAsync(Guid paymentId, string status, string reason, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPagedResult<AdminAuditLogDto>>> GetAuditLogsAsync(int page, int pageSize, Guid? actorId, string? targetTable, string? action, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
