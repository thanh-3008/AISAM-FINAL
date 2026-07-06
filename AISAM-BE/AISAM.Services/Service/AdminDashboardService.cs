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
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IPaymentRepository _paymentRepository;

        public AdminDashboardService(
            IUserRepository userRepository,
            IWorkspaceRepository workspaceRepository,
            IContentRepository contentRepository,
            IPaymentRepository paymentRepository)
        {
            _userRepository = userRepository;
            _workspaceRepository = workspaceRepository;
            _contentRepository = contentRepository;
            _paymentRepository = paymentRepository;
        }

        private async Task<GenericResponse<T>> Unauthorized<T>()
        {
            return GenericResponse<T>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);
        }

        public async Task<GenericResponse<object>> GetSummaryAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var totalUsers = await _userRepository.GetCountAsync(cancellationToken);
            var totalWorkspaces = await _workspaceRepository.GetCountAsync(cancellationToken);
            var totalContent = await _contentRepository.GetCountAsync(cancellationToken);
            var totalRevenue = await _paymentRepository.GetTotalRevenueAsync(cancellationToken);

            return GenericResponse<object>.CreateSuccess(new
            {
                TotalUsers = totalUsers,
                TotalWorkspaces = totalWorkspaces,
                TotalContent = totalContent,
                TotalRevenue = totalRevenue
            });
        }

        public async Task<GenericResponse<object>> GetChartsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var now = DateTime.UtcNow;
            var userRegistrations = new List<object>();
            var revenue = new List<object>();

            for (int i = 6; i >= 0; i--)
            {
                var date = now.Date.AddDays(-i);
                userRegistrations.Add(new { name = date.ToString("ddd"), users = 0 });
                revenue.Add(new { name = $"Week {4 - i / 2}", revenue = 0m });
            }

            return GenericResponse<object>.CreateSuccess(new
            {
                UserRegistrations = userRegistrations,
                Revenue = revenue
            });
        }

        public async Task<GenericResponse<object>> GetRevenueStatsAsync(Guid adminUserId, string period, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var totalRevenue = await _paymentRepository.GetTotalRevenueAsync(cancellationToken);
            return GenericResponse<object>.CreateSuccess(new { Period = period, TotalRevenue = totalRevenue });
        }
    }
}
