using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Linq;
using System.Net;

namespace AISAM.Services.Service
{
    public sealed class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAiGenerationRepository _aiGenerationRepository;

        public AdminDashboardService(
            IUserRepository userRepository,
            IWorkspaceRepository workspaceRepository,
            IContentRepository contentRepository,
            IPaymentRepository paymentRepository,
            IAiGenerationRepository aiGenerationRepository)
        {
            _userRepository = userRepository;
            _workspaceRepository = workspaceRepository;
            _contentRepository = contentRepository;
            _paymentRepository = paymentRepository;
            _aiGenerationRepository = aiGenerationRepository;
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
            var totalAiGenerations = await _aiGenerationRepository.GetTotalGenerationCountAsync(cancellationToken);

            return GenericResponse<object>.CreateSuccess(new
            {
                TotalUsers = totalUsers,
                TotalWorkspaces = totalWorkspaces,
                TotalContent = totalContent,
                TotalRevenue = totalRevenue,
                TotalAiGenerations = totalAiGenerations
            });
        }

        public async Task<GenericResponse<object>> GetChartsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var now = DateTime.UtcNow;
            var from7 = now.Date.AddDays(-6);
            var to = now.Date.AddDays(1);

            var userRegistrations = await _userRepository.GetDailyRegistrationsAsync(from7, to, cancellationToken);
            var dailyRevenue = await _paymentRepository.GetDailyRevenueAsync(from7, to, cancellationToken);
            var dailyTransactions = await _paymentRepository.GetDailyTransactionCountAsync(from7, to, cancellationToken);
            var dailyContent = await _contentRepository.GetDailyCreatedAsync(from7, to, cancellationToken);
            var dailyAi = await _aiGenerationRepository.GetDailyGenerationCountAsync(from7, to, cancellationToken);

            var from30 = now.Date.AddDays(-29);
            var revenue30 = await _paymentRepository.GetDailyRevenueAsync(from30, to, cancellationToken);

            var userRegData = new List<object>();
            var revenueData = new List<object>();
            var contentData = new List<object>();
            var aiData = new List<object>();

            for (int i = 6; i >= 0; i--)
            {
                var date = now.Date.AddDays(-i);
                var key = date.ToString("ddd");
                userRegData.Add(new { name = key, users = userRegistrations.GetValueOrDefault(date, 0) });
                revenueData.Add(new { name = key, revenue = dailyRevenue.GetValueOrDefault(date, 0m) });
                contentData.Add(new { name = key, content = dailyContent.GetValueOrDefault(date, 0) });
                aiData.Add(new { name = key, generations = dailyAi.GetValueOrDefault(date, 0) });
            }

            var revenue30Data = new List<object>();
            for (int i = 29; i >= 0; i--)
            {
                var date = now.Date.AddDays(-i);
                revenue30Data.Add(new { name = date.ToString("MMM dd"), revenue = revenue30.GetValueOrDefault(date, 0m) });
            }

            return GenericResponse<object>.CreateSuccess(new
            {
                UserRegistrations = userRegData,
                Revenue = revenueData,
                ContentCreated = contentData,
                AiGenerations = aiData,
                Revenue30Day = revenue30Data
            });
        }

        public async Task<GenericResponse<object>> GetRevenueStatsAsync(Guid adminUserId, string period, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var now = DateTime.UtcNow;
            DateTime from = period switch
            {
                "week" => now.AddDays(-7),
                "month" => now.AddDays(-30),
                "year" => now.AddDays(-365),
                _ => now.AddDays(-30)
            };

            var revenue = await _paymentRepository.GetTotalRevenueAsync(cancellationToken);
            var transactions = await _paymentRepository.GetDailyTransactionCountAsync(from, now, cancellationToken);

            return GenericResponse<object>.CreateSuccess(new
            {
                Period = period,
                TotalRevenue = revenue,
                TotalTransactions = transactions.Values.Sum()
            });
        }
    }
}
