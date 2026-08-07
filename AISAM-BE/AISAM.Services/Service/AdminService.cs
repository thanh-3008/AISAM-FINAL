using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Linq;
using System.Net;

namespace AISAM.Services.Service
{
    public sealed class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IAdCampaignRepository _adCampaignRepository;
        private readonly ISocialAccountRepository _socialAccountRepository;

        public AdminService(
            IUserRepository userRepository,
            IWorkspaceRepository workspaceRepository,
            IPaymentRepository paymentRepository,
            IContentRepository contentRepository,
            IAuditLogRepository auditLogRepository,
            ISubscriptionRepository subscriptionRepository,
            IAdCampaignRepository adCampaignRepository,
            ISocialAccountRepository socialAccountRepository)
        {
            _userRepository = userRepository;
            _workspaceRepository = workspaceRepository;
            _paymentRepository = paymentRepository;
            _contentRepository = contentRepository;
            _auditLogRepository = auditLogRepository;
            _subscriptionRepository = subscriptionRepository;
            _adCampaignRepository = adCampaignRepository;
            _socialAccountRepository = socialAccountRepository;
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

            var workspaces = await _workspaceRepository.GetByUserIdAsync(userId, cancellationToken);
            var workspaceDetails = workspaces.Select(w => new
            {
                w.Id, w.Name, w.WorkspaceType, w.Status, w.CreatedAt,
                TypeName = w.WorkspaceType.ToString()
            }).ToList();

            var subscriptions = new List<object>();
            var campaigns = new List<object>();

            foreach (var w in workspaces)
            {
                var sub = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(w.Id, cancellationToken);
                if (sub != null)
                {
                    subscriptions.Add(new
                    {
                        sub.Id,
                        WorkspaceId = w.Id,
                        WorkspaceName = w.Name,
                        PlanType = (int)sub.Plan,
                        Status = sub.IsActive ? 1 : 0,
                        CurrentPeriodEnd = sub.EndDate
                    });
                }

                var camps = await _adCampaignRepository.GetPagedByWorkspaceIdAsync(w.Id, new PaginationRequest { PageSize = 100 }, false, cancellationToken);
                foreach (var c in camps.Data)
                {
                    campaigns.Add(new
                    {
                        c.Id,
                        WorkspaceId = w.Id,
                        WorkspaceName = w.Name,
                        c.Name,
                        Status = c.IsActive ? 2 : 0,
                        c.Impressions,
                        c.Clicks,
                        c.Spend,
                        c.Conversions,
                        c.CreatedAt
                    });
                }
            }

            var rawPayments = await _paymentRepository.GetByUserIdAsync(userId, cancellationToken);
            var payments = rawPayments.Select(p => new
            {
                p.Id,
                p.Amount,
                p.Currency,
                p.Status,
                p.PaymentMethod,
                p.CreatedAt,
                WorkspaceName = p.Workspace?.Name
            }).ToList();

            List<object> sessions;
            try
            {
                var userSessions = await _userRepository.GetSessionsAsync(userId, cancellationToken);
                sessions = userSessions.Select(s => new { s.CreatedAt, s.UserAgent, s.IsActive }).ToList<object>();
            }
            catch { sessions = new List<object>(); }

            return GenericResponse<object>.CreateSuccess(new
            {
                user.Id, user.Email, user.FullName, user.Role, user.IsEmailVerified, user.CreatedAt,
                RoleName = user.Role.ToString(),
                Workspaces = workspaceDetails,
                WorkspaceCount = workspaceDetails.Count,
                Subscriptions = subscriptions,
                Payments = payments,
                Campaigns = campaigns,
                Sessions = sessions,
                SessionCount = sessions.Count
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

            var oldStatus = user.IsEmailVerified;
            user.IsEmailVerified = isActive;
            await _userRepository.UpdateAsync(user);
            await LogAuditAsync(adminUserId, isActive ? "ACTIVATE_USER" : "DEACTIVATE_USER", "users", userId,
                oldValues: $"{{\"isEmailVerified\": {oldStatus.ToString().ToLower()}}}",
                newValues: $"{{\"isEmailVerified\": {isActive.ToString().ToLower()}}}");
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
            await LogAuditAsync(adminUserId, "DELETE_USER", "users", userId,
                notes: $"Deleted user: {user.Email}");
            return GenericResponse<bool>.CreateSuccess(true, "User deleted.");
        }

        public async Task<GenericResponse<bool>> SetUserRoleAsync(Guid adminUserId, Guid userId, int role, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return GenericResponse<bool>.CreateError("User not found.", HttpStatusCode.NotFound);

            if (user.Id == adminUserId)
                return GenericResponse<bool>.CreateError("Cannot change your own role.", HttpStatusCode.Forbidden);

            var oldRole = user.Role;
            user.Role = (UserRoleEnum)role;
            await _userRepository.UpdateAsync(user);
            await LogAuditAsync(adminUserId, "CHANGE_USER_ROLE", "users", userId,
                oldValues: $"{{\"role\": {(int)oldRole}}}",
                newValues: $"{{\"role\": {role}}}",
                notes: $"Role changed from {oldRole} to {(UserRoleEnum)role}");
            return GenericResponse<bool>.CreateSuccess(true, "User role updated.");
        }

        public async Task<GenericResponse<object>> GetAdminsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var admins = await _userRepository.GetAdminsAsync(cancellationToken);
            return GenericResponse<object>.CreateSuccess(admins.Select(a => new { a.Id, a.Email, a.FullName, a.CreatedAt }));
        }

        public async Task<GenericResponse<object>> GetWorkspacesAsync(
            Guid adminUserId, PaginationRequest request, int? workspaceType = null, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var pagedResult = await _workspaceRepository.GetPagedAllAsync(request, workspaceType, cancellationToken);
            var mappedData = pagedResult.Data.Select(ws => (object)new
            {
                ws.Id,
                ws.Name,
                ws.WorkspaceType,
                ws.Status,
                ws.MemberLimit,
                ws.SubscriptionExpiredAt,
                ws.CreatedAt,
                ws.UpdatedAt,
                AiCreditBalance = ws.CreditWallet?.Balance ?? 0,
                AiCreditReserved = ws.CreditWallet?.ReservedBalance ?? 0
            }).ToList();

            var result = new PagedResult<object>
            {
                Data = mappedData,
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };

            Console.WriteLine($"DEBUG: GetWorkspacesAsync returning {result.TotalCount} items");
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

            var members = ws.Members.Select(m => new
            {
                m.UserId,
                m.User.FullName,
                m.User.Email,
                RoleName = m.Role.ToString(),
                m.IsActive,
                m.JoinedAt
            }).ToList();

            var recentPosts = await _contentRepository.GetPagedByWorkspaceIdAsync(workspaceId, new PaginationRequest { PageSize = 50 }, cancellationToken: cancellationToken);

            var posts = recentPosts.Data.Select(c => new
            {
                c.Id,
                c.Title,
                c.TextContent,
                c.ImageUrl,
                c.VideoUrl,
                c.Status,
                StatusName = c.Status.ToString(),
                c.CreatedAt
            }).ToList();

            var socialAccounts = await _socialAccountRepository.GetByWorkspaceIdAsync(workspaceId, cancellationToken);
            var socialAccountsData = socialAccounts.Select(sa => new
            {
                sa.Id,
                sa.Platform,
                PlatformName = sa.Platform.ToString(),
                sa.AccountId,
                IsExpired = sa.ExpiresAt.HasValue && sa.ExpiresAt.Value < DateTime.UtcNow,
                sa.CreatedAt
            }).ToList();

            var activeSubscription = await _subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(workspaceId, cancellationToken);
            var subscriptionData = activeSubscription == null ? null : new
            {
                activeSubscription.Id,
                activeSubscription.Plan,
                PlanName = activeSubscription.Plan.ToString(),
                activeSubscription.StartDate,
                activeSubscription.EndDate
            };

            var adCampaignsPaged = await _adCampaignRepository.GetPagedByWorkspaceIdAsync(workspaceId, new PaginationRequest { Page = 1, PageSize = 1000 }, false, cancellationToken);
            var adCampaigns = adCampaignsPaged.Data;
            var totalAdSpend = adCampaigns.Sum(c => c.Spend);
            var totalAdImpressions = adCampaigns.Sum(c => c.Impressions);
            var totalAdClicks = adCampaigns.Sum(c => c.Clicks);

            return GenericResponse<object>.CreateSuccess(new
            {
                ws.Id,
                ws.Name,
                ws.WorkspaceType,
                ws.Status,
                ws.MemberLimit,
                ws.SubscriptionExpiredAt,
                ws.CreatedAt,
                ws.UpdatedAt,
                TypeName = ws.WorkspaceType.ToString(),
                StatusName = ws.Status.ToString(),
                Members = members,
                Posts = posts,
                AiCreditBalance = ws.CreditWallet?.Balance ?? 0,
                AiCreditReserved = ws.CreditWallet?.ReservedBalance ?? 0,
                SocialAccounts = socialAccountsData,
                SocialAccountCount = socialAccountsData.Count,
                ActiveSubscription = subscriptionData,
                TotalAdSpend = totalAdSpend,
                TotalAdImpressions = totalAdImpressions,
                TotalAdClicks = totalAdClicks
            });
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

            var oldStatus = ws.Status;
            ws.Status = (WorkspaceStatusEnum)status;
            await _workspaceRepository.UpdateAsync(ws, cancellationToken);
            await LogAuditAsync(adminUserId, "UPDATE_WORKSPACE_STATUS", "workspaces", workspaceId,
                oldValues: $"{{\"status\": {(int)oldStatus}}}",
                newValues: $"{{\"status\": {status}}}");
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
            await LogAuditAsync(adminUserId, "DELETE_WORKSPACE", "workspaces", workspaceId,
                notes: "Workspace permanently deleted");
            return GenericResponse<bool>.CreateSuccess(true, "Workspace deleted.");
        }

        public async Task<GenericResponse<object>> GetPaymentsAsync(
            Guid adminUserId, PaginationRequest request, int? status = null, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            Console.WriteLine($"[DEBUG] adminUserId: {adminUserId}, admin: {admin != null}, Role: {admin?.Role}");
            if (admin?.Role != UserRoleEnum.Admin)
            {
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", System.Net.HttpStatusCode.Forbidden);
            }

            PaymentStatusEnum? statusEnum = status.HasValue ? (PaymentStatusEnum)status.Value : null;

            var pagedPayments = await _paymentRepository.GetPagedAllAsync(request, statusEnum, cancellationToken);
            
            var items = pagedPayments.Data.Select(p => (object)new
            {
                p.Id,
                p.UserId,
                UserEmail = p.User?.Email ?? "Unknown",
                p.Amount,
                p.Currency,
                p.Status,
                p.PaymentType,
                p.TransactionId,
                p.RefundedAt,
                p.RefundReason,
                p.CreatedAt
            }).ToList();

            var result = new PagedResult<object>
            {
                Data = items,
                TotalCount = pagedPayments.TotalCount,
                Page = pagedPayments.Page,
                PageSize = pagedPayments.PageSize
            };

            return GenericResponse<object>.CreateSuccess(result);
        }

        public async Task<GenericResponse<object>> GetAllContentAsync(
            Guid adminUserId, PaginationRequest request, int? status = null, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError(
                    "Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var pagedContent = await _contentRepository.GetPagedAllAsync(request, status.HasValue ? (ContentStatusEnum?)status.Value : null, cancellationToken);
            
            var items = pagedContent.Data.Select(c => (object)new
            {
                c.Id,
                c.Title,
                c.TextContent,
                c.Status,
                c.AdType,
                c.IsAiGenerated,
                c.PlatformRejectionReason,
                c.RejectedPlatform,
                c.WorkspaceId,
                c.CreatedAt
            }).ToList();

            var result = new PagedResult<object>
            {
                Data = items,
                TotalCount = pagedContent.TotalCount,
                Page = pagedContent.Page,
                PageSize = pagedContent.PageSize
            };

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

            var oldStatus = content.Status;
            content.Status = (ContentStatusEnum)status;
            await _contentRepository.UpdateAsync(content, cancellationToken);
            await LogAuditAsync(adminUserId, "UPDATE_CONTENT_STATUS", "contents", contentId,
                oldValues: $"{{\"status\": {(int)oldStatus}}}",
                newValues: $"{{\"status\": {status}}}");
            return GenericResponse<bool>.CreateSuccess(true, "Content status updated.");
        }

        public async Task<GenericResponse<bool>> DeleteContentAsync(Guid adminUserId, Guid contentId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            await _contentRepository.DeleteAsync(contentId, cancellationToken);
            await LogAuditAsync(adminUserId, "DELETE_CONTENT", "contents", contentId,
                notes: "Content deleted");
            return GenericResponse<bool>.CreateSuccess(true, "Content deleted.");
        }

        public async Task<GenericResponse<bool>> RefundPaymentAsync(Guid adminUserId, Guid paymentId, string reason, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
                return GenericResponse<bool>.CreateError("Payment not found.", HttpStatusCode.NotFound);

            if (payment.Status == PaymentStatusEnum.Refunded)
                return GenericResponse<bool>.CreateError("Payment is already refunded.", HttpStatusCode.BadRequest);

            payment.Status = PaymentStatusEnum.Refunded;
            payment.RefundedAt = DateTime.UtcNow;
            payment.RefundReason = reason;
            payment.RefundedBy = adminUserId;

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await LogAuditAsync(adminUserId, "REFUND_PAYMENT", "payments", paymentId,
                notes: $"Payment refunded. Reason: {reason}");
            return GenericResponse<bool>.CreateSuccess(true, "Payment refunded successfully.");
        }

        public async Task<GenericResponse<object>> GetAuditLogsAsync(Guid adminUserId, Common.Dtos.Request.AuditLogFilterRequest request, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var result = await _auditLogRepository.GetPagedAsync(request, cancellationToken);
            
            var mappedData = result.Data.Select(al => new
            {
                al.Id,
                al.ActionType,
                al.TargetTable,
                al.TargetId,
                al.CreatedAt,
                al.Notes,
                ActorEmail = al.Actor?.Email,
                ActorName = al.Actor?.FullName
            }).ToList();

            var mappedResult = new PagedResult<object>
            {
                Data = mappedData.Cast<object>().ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return GenericResponse<object>.CreateSuccess(mappedResult);
        }

        public async Task<GenericResponse<string>> ExportAuditLogsCsvAsync(Guid adminUserId, Common.Dtos.Request.AuditLogFilterRequest request, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<string>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            // Fetch all matching data (or a large page size for export)
            request.Page = 1;
            request.PageSize = 10000; // Limit export to 10k rows for now
            var result = await _auditLogRepository.GetPagedAsync(request, cancellationToken);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Id,Date,Action,Table,TargetId,ActorEmail,ActorName,Notes");

            foreach (var log in result.Data)
            {
                var date = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                var notes = log.Notes?.Replace("\"", "\"\"") ?? "";
                sb.AppendLine($"{log.Id},{date},{log.ActionType},{log.TargetTable},{log.TargetId},{log.Actor?.Email},{log.Actor?.FullName},\"{notes}\"");
            }

            return GenericResponse<string>.CreateSuccess(sb.ToString());
        }

        private async Task LogAuditAsync(Guid actorId, string actionType, string targetTable, Guid targetId, string? notes = null, string? oldValues = null, string? newValues = null)
        {
            await _auditLogRepository.AddAsync(new AuditLog
            {
                ActorId = actorId,
                ActionType = actionType,
                TargetTable = targetTable,
                TargetId = targetId,
                Notes = notes,
                OldValues = oldValues,
                NewValues = newValues
            });
        }
    }
}
