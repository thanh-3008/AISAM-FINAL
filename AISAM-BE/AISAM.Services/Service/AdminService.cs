using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service
{
    public sealed class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IContentRepository _contentRepository;

        public AdminService(
            IUserRepository userRepository,
            IWorkspaceRepository workspaceRepository,
            IPaymentRepository paymentRepository,
            IContentRepository contentRepository)
        {
            _userRepository = userRepository;
            _workspaceRepository = workspaceRepository;
            _paymentRepository = paymentRepository;
            _contentRepository = contentRepository;
        }

        public async Task<GenericResponse<PagedResult<UserListDto>>> GetUsersAsync(
            Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<PagedResult<UserListDto>>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var result = await _userRepository.GetPagedUsersWithRoleFilterAsync(request, null, null, null, cancellationToken);
            return GenericResponse<PagedResult<UserListDto>>.CreateSuccess(result);
        }

        public async Task<GenericResponse<object>> GetUserDetailAsync(
            Guid adminUserId, Guid userId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return GenericResponse<object>.CreateError("User not found.", HttpStatusCode.NotFound);

            return GenericResponse<object>.CreateSuccess(new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Role,
                user.IsEmailVerified,
                user.CreatedAt,
                RoleName = user.Role.ToString()
            });
        }

        public async Task<GenericResponse<bool>> SetUserStatusAsync(
            Guid adminUserId, Guid userId, bool isActive, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return GenericResponse<bool>.CreateError("User not found.", HttpStatusCode.NotFound);

            user.IsEmailVerified = isActive;
            await _userRepository.UpdateAsync(user);
            return GenericResponse<bool>.CreateSuccess(true, isActive ? "User activated." : "User deactivated.");
        }

        public async Task<GenericResponse<bool>> DeleteUserAsync(
            Guid adminUserId, Guid userId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return GenericResponse<bool>.CreateError("User not found.", HttpStatusCode.NotFound);

            if (user.Role == UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Cannot delete an admin user.", HttpStatusCode.Forbidden);

            await _userRepository.DeleteAsync(userId, cancellationToken);
            return GenericResponse<bool>.CreateSuccess(true, "User deleted.");
        }

        public async Task<GenericResponse<object>> GetWorkspacesAsync(
            Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var result = await _workspaceRepository.GetPagedAllAsync(request, cancellationToken);
            return GenericResponse<object>.CreateSuccess(result);
        }

        public async Task<GenericResponse<object>> GetWorkspaceDetailAsync(
            Guid adminUserId, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var ws = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
            if (ws == null)
                return GenericResponse<object>.CreateError("Workspace not found.", HttpStatusCode.NotFound);

            return GenericResponse<object>.CreateSuccess(ws);
        }

        public async Task<GenericResponse<bool>> SetWorkspaceStatusAsync(
            Guid adminUserId, Guid workspaceId, int status, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var ws = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);
            if (ws == null)
                return GenericResponse<bool>.CreateError("Workspace not found.", HttpStatusCode.NotFound);

            ws.Status = (WorkspaceStatusEnum)status;
            await _workspaceRepository.UpdateAsync(ws, cancellationToken);
            return GenericResponse<bool>.CreateSuccess(true, "Workspace status updated.");
        }

        public async Task<GenericResponse<bool>> DeleteWorkspaceAsync(
            Guid adminUserId, Guid workspaceId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            await _workspaceRepository.DeleteAsync(workspaceId, cancellationToken);
            return GenericResponse<bool>.CreateSuccess(true, "Workspace deleted.");
        }

        public async Task<GenericResponse<object>> GetPaymentsAsync(
            Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var result = await _paymentRepository.GetPagedAllAsync(request, cancellationToken);
            return GenericResponse<object>.CreateSuccess(result);
        }

        public async Task<GenericResponse<object>> GetAllContentAsync(
            Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var result = await _contentRepository.GetPagedAllAsync(request, cancellationToken);
            return GenericResponse<object>.CreateSuccess(result);
        }

        public async Task<GenericResponse<bool>> SetContentStatusAsync(
            Guid adminUserId, Guid contentId, int status, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
            if (content == null)
                return GenericResponse<bool>.CreateError("Content not found.", HttpStatusCode.NotFound);

            content.Status = (ContentStatusEnum)status;
            await _contentRepository.UpdateAsync(content, cancellationToken);
            return GenericResponse<bool>.CreateSuccess(true, "Content status updated.");
        }
    }
}
