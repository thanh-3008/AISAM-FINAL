using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service
{
    public sealed class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUserRepository _userRepository;

        public AdminDashboardService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<GenericResponse<object>> GetSummaryAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var totalUsers = await _userRepository.GetCountAsync(cancellationToken);
            return GenericResponse<object>.CreateSuccess(new
            {
                TotalUsers = totalUsers,
                TotalWorkspaces = 0,
                TotalContent = 0,
                TotalRevenue = 0m
            });
        }

        public async Task<GenericResponse<object>> GetChartsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            return GenericResponse<object>.CreateSuccess(new
            {
                UserRegistrations = Array.Empty<object>(),
                Revenue = Array.Empty<object>()
            });
        }

        public async Task<GenericResponse<object>> GetRevenueStatsAsync(Guid adminUserId, string period, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            return GenericResponse<object>.CreateSuccess(new { Period = period, TotalRevenue = 0m });
        }
    }
}
