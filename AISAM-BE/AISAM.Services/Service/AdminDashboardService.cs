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
        private readonly IPerformanceReportRepository _performanceReportRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public AdminDashboardService(
            IUserRepository userRepository,
            IWorkspaceRepository workspaceRepository,
            IContentRepository contentRepository,
            IPaymentRepository paymentRepository,
            IAiGenerationRepository aiGenerationRepository,
            IPerformanceReportRepository performanceReportRepository,
            IAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _workspaceRepository = workspaceRepository;
            _contentRepository = contentRepository;
            _paymentRepository = paymentRepository;
            _aiGenerationRepository = aiGenerationRepository;
            _performanceReportRepository = performanceReportRepository;
            _auditLogRepository = auditLogRepository;
        }

        private static Task<GenericResponse<T>> Unauthorized<T>()
        {
            return Task.FromResult(GenericResponse<T>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden));
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
            var from30 = now.Date.AddDays(-29);
            var to = now.Date.AddDays(1);

            var userRegistrations = await _userRepository.GetDailyRegistrationsAsync(from30, to, cancellationToken);
            var dailyRevenue = await _paymentRepository.GetDailyRevenueAsync(from30, to, cancellationToken);
            var dailyTransactions = await _paymentRepository.GetDailyTransactionCountAsync(from30, to, cancellationToken);
            var dailyContent = await _contentRepository.GetDailyCreatedAsync(from30, to, cancellationToken);
            var dailyAi = await _aiGenerationRepository.GetDailyGenerationCountAsync(from30, to, cancellationToken);

            var userRegData = new List<object>();
            var revenueData = new List<object>();
            var contentData = new List<object>();
            var aiData = new List<object>();
            var revenue30Data = new List<object>();

            for (int i = 29; i >= 0; i--)
            {
                var date = now.Date.AddDays(-i);
                var key = date.ToString("MMM dd"); // Changed from "ddd" to "MMM dd" for 30 days to avoid duplicate day names
                
                userRegData.Add(new { name = key, users = userRegistrations.GetValueOrDefault(date, 0) });
                revenueData.Add(new { name = key, revenue = dailyRevenue.GetValueOrDefault(date, 0m) });
                contentData.Add(new { name = key, content = dailyContent.GetValueOrDefault(date, 0) });
                aiData.Add(new { name = key, generations = dailyAi.GetValueOrDefault(date, 0) });
                revenue30Data.Add(new { name = key, revenue = dailyRevenue.GetValueOrDefault(date, 0m) });
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

        public async Task<GenericResponse<object>> GetTopWorkspacesAsync(Guid adminUserId, int limit, string period = "month", CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var now = DateTime.UtcNow;
            DateTime from = period switch
            {
                "day" => now.AddDays(-1),
                "week" => now.AddDays(-7),
                "month" => now.AddDays(-30),
                "year" => now.AddDays(-365),
                "all" => DateTime.MinValue,
                _ => now.AddDays(-30)
            };

            var topByRevenue = await _paymentRepository.GetTopWorkspacesByRevenueAsync(limit, from, now, cancellationToken);
            var workspacesPerformance = await _performanceReportRepository.GetWorkspaceComparisonAsync(from, now, limit, cancellationToken);

            var allWorkspaceIds = topByRevenue.Select(x => x.WorkspaceId)
                .Union(workspacesPerformance.Select(x => x.WorkspaceId))
                .Distinct()
                .ToList();

            // Fetch workspace names to ensure all have valid names
            var activeWorkspaces = await _workspaceRepository.GetAllActiveAsync(cancellationToken);
            var workspaceNames = activeWorkspaces.ToDictionary(w => w.Id, w => w.Name);

            var result = allWorkspaceIds.Select(id =>
            {
                var rev = topByRevenue.FirstOrDefault(x => x.WorkspaceId == id)?.Revenue ?? 0m;
                var perf = workspacesPerformance.FirstOrDefault(x => x.WorkspaceId == id);
                var spend = perf?.Spend ?? 0m;
                var adRevenue = perf?.EstimatedRevenue ?? 0m;
                var roas = spend > 0 ? adRevenue / spend : 0m;
                var engagement = perf?.Engagement ?? 0;
                var name = workspaceNames.GetValueOrDefault(id, "Unknown Workspace");

                return new
                {
                    WorkspaceId = id,
                    WorkspaceName = name,
                    SaaSRevenue = rev,
                    AdSpend = spend,
                    AdRevenue = adRevenue,
                    Roas = Math.Round(roas, 2),
                    Engagement = engagement
                };
            })
            .OrderByDescending(x => x.SaaSRevenue)
            .Take(limit)
            .ToList();

            return GenericResponse<object>.CreateSuccess(result);
        }

        public async Task<GenericResponse<object>> GetAiCreditBreakdownAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var topWorkspaces = await _aiGenerationRepository.GetTopWorkspacesByGenerationAsync(50, cancellationToken);
            
            var allWorkspaces = await _workspaceRepository.GetAllActiveAsync(cancellationToken);
            var workspaceDict = allWorkspaces.ToDictionary(w => w.Id, w => w.Name);

            var result = topWorkspaces.Select(tw =>
            {
                Guid wsId = tw.WorkspaceId;
                return new
                {
                    WorkspaceId = wsId,
                    WorkspaceName = workspaceDict.GetValueOrDefault(wsId, "Unknown Workspace"),
                    TotalGenerations = tw.Count
                };
            }).ToList();

            return GenericResponse<object>.CreateSuccess(result);
        }

        public async Task<GenericResponse<object>> GetActiveUsersStatsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin) return await Unauthorized<object>();

            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);
            
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

            var dau = await _auditLogRepository.GetActiveUsersCountAsync(todayStart, todayEnd, cancellationToken);
            var mau = await _auditLogRepository.GetActiveUsersCountAsync(monthStart, monthEnd, cancellationToken);

            return GenericResponse<object>.CreateSuccess(new
            {
                DAU = dau,
                MAU = mau,
                Date = todayStart.ToString("yyyy-MM-dd"),
                Month = monthStart.ToString("yyyy-MM")
            });
        }
    }
}
