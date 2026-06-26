using AISAM.Common;
using AISAM.Common.Dtos.Admin;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.Services.Service;

public class AdminService : IAdminService
{
    private readonly AisamContext _context;

    public AdminService(AisamContext context)
    {
        _context = context;
    }

    public async Task<GenericResponse<AdminDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalWorkspaces = await _context.Workspaces.CountAsync(w => w.DeletedAt == null, cancellationToken);
        var activeSubscriptions = await _context.Subscriptions.CountAsync(s => s.IsActive && !s.IsDeleted, cancellationToken);
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == AISAM.Data.Enumeration.PaymentStatusEnum.Success && !p.IsDeleted)
            .SumAsync(p => p.Amount, cancellationToken);
        var activeUsers = await _context.Users
            .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt >= DateTime.UtcNow.AddDays(-30))
            .CountAsync(cancellationToken);

        var recentUsers = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(10)
            .Select(u => new AdminRecentUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var recentPayments = await _context.Payments
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(p => new AdminRecentPaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency ?? "VND",
                Status = p.Status.ToString(),
                UserEmail = p.User.Email,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var dto = new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalWorkspaces = totalWorkspaces,
            ActiveSubscriptions = activeSubscriptions,
            TotalRevenue = totalRevenue,
            RecentUsers = recentUsers,
            RecentPayments = recentPayments
        };

        return GenericResponse<AdminDashboardDto>.CreateSuccess(dto);
    }

    public async Task<GenericResponse<AdminPagedResult<AdminUserListDto>>> GetUsersAsync(
        int page, int pageSize, string? searchTerm, string? sortBy, bool sortDescending,
        string? role, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(term)
                || (u.FullName != null && u.FullName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<AISAM.Data.Enumeration.UserRoleEnum>(role, true, out var roleEnum))
        {
            query = query.Where(u => u.Role == roleEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = (sortBy?.ToLower(), sortDescending) switch
        {
            ("email", false) => query.OrderBy(u => u.Email),
            ("email", true) => query.OrderByDescending(u => u.Email),
            ("createdat", true) => query.OrderByDescending(u => u.CreatedAt),
            ("createdat", false) => query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role.ToString(),
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                WorkspaceCount = u.WorkspaceMembers.Count(wm => wm.Workspace.DeletedAt == null)
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return GenericResponse<AdminPagedResult<AdminUserListDto>>.CreateSuccess(new AdminPagedResult<AdminUserListDto>
        {
            Data = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        });
    }

    public async Task<GenericResponse<AdminUserDetailDto>> GetUserDetailAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Profiles)
            .Include(u => u.WorkspaceMembers).ThenInclude(wm => wm.Workspace)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return GenericResponse<AdminUserDetailDto>.CreateError("User not found.", System.Net.HttpStatusCode.NotFound);
        }

        var payments = await _context.Payments
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new AdminUserPaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency ?? "VND",
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var dto = new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Profiles = user.Profiles.Where(p => !p.IsDeleted).Select(p => new AdminUserProfileDto
            {
                Id = p.Id,
                Name = p.Name,
                CompanyName = p.CompanyName,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            }).ToList(),
            Workspaces = user.WorkspaceMembers.Where(wm => wm.Workspace.DeletedAt == null).Select(wm => new AdminUserWorkspaceDto
            {
                Id = wm.Workspace.Id,
                Name = wm.Workspace.Name,
                Type = wm.Workspace.WorkspaceType.ToString(),
                Status = wm.Workspace.Status.ToString(),
                Role = wm.Role.ToString(),
                CreatedAt = wm.Workspace.CreatedAt
            }).ToList(),
            Payments = payments
        };

        return GenericResponse<AdminUserDetailDto>.CreateSuccess(dto);
    }

    public async Task<GenericResponse<bool>> UpdateUserRoleAsync(
        Guid userId, string role, string reason, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<AISAM.Data.Enumeration.UserRoleEnum>(role, true, out var roleEnum))
        {
            return GenericResponse<bool>.CreateError($"Invalid role: {role}. Valid values: User, Vendor, Admin.", System.Net.HttpStatusCode.BadRequest);
        }

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return GenericResponse<bool>.CreateError("User not found.", System.Net.HttpStatusCode.NotFound);
        }

        user.Role = roleEnum;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<bool>.CreateSuccess(true, $"User role updated to {roleEnum}.");
    }

    public async Task<GenericResponse<bool>> UpdateUserStatusAsync(
        Guid userId, bool isActive, string reason, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return GenericResponse<bool>.CreateError("User not found.", System.Net.HttpStatusCode.NotFound);
        }

        if (!isActive)
        {
            var sessions = await _context.Sessions.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
            _context.Sessions.RemoveRange(sessions);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<bool>.CreateSuccess(true, isActive ? "User activated." : "User deactivated (all sessions revoked).");
    }

    public async Task<GenericResponse<AdminPagedResult<AdminWorkspaceListDto>>> GetWorkspacesAsync(
        int page, int pageSize, string? searchTerm, string? status, string? plan, CancellationToken cancellationToken = default)
    {
        var query = _context.Workspaces.Where(w => w.DeletedAt == null).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(w => w.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AISAM.Data.Enumeration.WorkspaceStatusEnum>(status, true, out var statusEnum))
        {
            query = query.Where(w => w.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(plan) && Enum.TryParse<AISAM.Data.Enumeration.SubscriptionPlanEnum>(plan, true, out var planEnum))
        {
            query = query.Where(w => w.Subscriptions.Any(s => s.IsActive && !s.IsDeleted && s.Plan == planEnum));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new AdminWorkspaceListDto
            {
                Id = w.Id,
                Name = w.Name,
                Type = w.WorkspaceType.ToString(),
                Status = w.Status.ToString(),
                Plan = w.Subscriptions.Where(s => s.IsActive && !s.IsDeleted).Select(s => s.Plan.ToString()).FirstOrDefault() ?? "Free",
                MemberCount = w.Members.Count,
                OwnerEmail = w.Members.Where(m => m.Role == AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Owner).Select(m => m.User.Email).FirstOrDefault() ?? "N/A",
                CreditBalance = w.CreditWallet != null ? w.CreditWallet.Balance : 0,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return GenericResponse<AdminPagedResult<AdminWorkspaceListDto>>.CreateSuccess(new AdminPagedResult<AdminWorkspaceListDto>
        {
            Data = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        });
    }

    public async Task<GenericResponse<AdminWorkspaceDetailDto>> GetWorkspaceDetailAsync(
        Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Members).ThenInclude(m => m.User)
            .Include(w => w.CreditWallet)
            .Include(w => w.Subscriptions)
            .FirstOrDefaultAsync(w => w.Id == workspaceId && w.DeletedAt == null, cancellationToken);

        if (workspace == null)
        {
            return GenericResponse<AdminWorkspaceDetailDto>.CreateError("Workspace not found.", System.Net.HttpStatusCode.NotFound);
        }

        var owner = workspace.Members.FirstOrDefault(m => m.Role == AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Owner);
        var activeSub = workspace.Subscriptions.FirstOrDefault(s => s.IsActive && !s.IsDeleted);

        var dto = new AdminWorkspaceDetailDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = string.Empty,
            Type = workspace.WorkspaceType.ToString(),
            Status = workspace.Status.ToString(),
            ExpiresAt = workspace.SubscriptionExpiredAt,
            CreatedAt = workspace.CreatedAt,
            Owner = owner != null ? new AdminWorkspaceOwnerDto
            {
                Id = owner.User.Id,
                Email = owner.User.Email,
                FullName = owner.User.FullName
            } : new AdminWorkspaceOwnerDto(),
            Members = workspace.Members.Select(m => new AdminWorkspaceMemberDto
            {
                UserId = m.UserId,
                Email = m.User.Email,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            }).ToList(),
            Subscription = activeSub != null ? new AdminWorkspaceSubscriptionDto
            {
                Id = activeSub.Id,
                Plan = activeSub.Plan.ToString(),
                IsActive = activeSub.IsActive,
                StartDate = activeSub.StartDate,
                EndDate = activeSub.EndDate
            } : null,
            CreditBalance = workspace.CreditWallet?.Balance ?? 0
        };

        return GenericResponse<AdminWorkspaceDetailDto>.CreateSuccess(dto);
    }

    public async Task<GenericResponse<AdminPagedResult<AdminSubscriptionDto>>> GetSubscriptionsAsync(
        int page, int pageSize, string? status, string? plan, Guid? workspaceId, CancellationToken cancellationToken = default)
    {
        var query = _context.Subscriptions.Where(s => !s.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && bool.TryParse(status, out var isActive))
            query = query.Where(s => s.IsActive == isActive);
        if (!string.IsNullOrWhiteSpace(plan) && Enum.TryParse<AISAM.Data.Enumeration.SubscriptionPlanEnum>(plan, true, out var planEnum))
            query = query.Where(s => s.Plan == planEnum);
        if (workspaceId.HasValue)
            query = query.Where(s => s.WorkspaceId == workspaceId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(s => s.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new AdminSubscriptionDto
            {
                Id = s.Id, WorkspaceId = s.WorkspaceId, WorkspaceName = s.Workspace.Name,
                Plan = s.Plan.ToString(), IsActive = s.IsActive, StartDate = s.StartDate,
                EndDate = s.EndDate, CreatedAt = s.CreatedAt
            }).ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return GenericResponse<AdminPagedResult<AdminSubscriptionDto>>.CreateSuccess(new AdminPagedResult<AdminSubscriptionDto>
        {
            Data = items, TotalCount = totalCount, Page = page, PageSize = pageSize,
            TotalPages = totalPages, HasNextPage = page < totalPages, HasPreviousPage = page > 1
        });
    }

    public async Task<GenericResponse<bool>> UpdateSubscriptionAsync(
        Guid subscriptionId, string? plan, bool? isActive, DateTime? endDate, string reason, CancellationToken cancellationToken = default)
    {
        var sub = await _context.Subscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken);
        if (sub == null) return GenericResponse<bool>.CreateError("Subscription not found.", System.Net.HttpStatusCode.NotFound);
        if (!string.IsNullOrWhiteSpace(plan) && Enum.TryParse<AISAM.Data.Enumeration.SubscriptionPlanEnum>(plan, true, out var planEnum))
            sub.Plan = planEnum;
        if (isActive.HasValue) sub.IsActive = isActive.Value;
        if (endDate.HasValue) sub.EndDate = endDate.Value;
        sub.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Subscription updated.");
    }

    public async Task<GenericResponse<AdminPagedResult<AdminPaymentDto>>> GetPaymentsAsync(
        int page, int pageSize, string? status, Guid? userId, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.Where(p => !p.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AISAM.Data.Enumeration.PaymentStatusEnum>(status, true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);
        if (userId.HasValue) query = query.Where(p => p.UserId == userId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new AdminPaymentDto
            {
                Id = p.Id, UserId = p.UserId, UserEmail = p.User.Email, WorkspaceId = p.WorkspaceId,
                Amount = p.Amount, Currency = p.Currency ?? "VND", Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod, TransactionId = p.TransactionId, CreatedAt = p.CreatedAt
            }).ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return GenericResponse<AdminPagedResult<AdminPaymentDto>>.CreateSuccess(new AdminPagedResult<AdminPaymentDto>
        {
            Data = items, TotalCount = totalCount, Page = page, PageSize = pageSize,
            TotalPages = totalPages, HasNextPage = page < totalPages, HasPreviousPage = page > 1
        });
    }

    public async Task<GenericResponse<bool>> UpdatePaymentStatusAsync(
        Guid paymentId, string status, string reason, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<AISAM.Data.Enumeration.PaymentStatusEnum>(status, true, out var statusEnum))
            return GenericResponse<bool>.CreateError($"Invalid payment status: {status}", System.Net.HttpStatusCode.BadRequest);
        var payment = await _context.Payments.FindAsync(new object[] { paymentId }, cancellationToken);
        if (payment == null) return GenericResponse<bool>.CreateError("Payment not found.", System.Net.HttpStatusCode.NotFound);
        payment.Status = statusEnum;
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Payment status updated.");
    }

    public async Task<GenericResponse<AdminPagedResult<AdminAuditLogDto>>> GetAuditLogsAsync(
        int page, int pageSize, Guid? actorId, string? targetTable, string? action, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<AISAM.Data.Model.AuditLog>().AsQueryable();

        if (actorId.HasValue) query = query.Where(a => a.ActorId == actorId.Value);
        if (!string.IsNullOrWhiteSpace(targetTable))
            query = query.Where(a => a.TargetTable != null && a.TargetTable.Contains(targetTable));
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.ActionType != null && a.ActionType.Contains(action));
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(a => a.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AdminAuditLogDto
            {
                Id = a.Id, ActorId = a.ActorId,
                ActorEmail = a.Actor != null ? a.Actor.Email : null,
                Action = a.ActionType, TargetTable = a.TargetTable,
                TargetId = a.TargetId, CreatedAt = a.CreatedAt
            }).ToListAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return GenericResponse<AdminPagedResult<AdminAuditLogDto>>.CreateSuccess(new AdminPagedResult<AdminAuditLogDto>
        {
            Data = items, TotalCount = totalCount, Page = page, PageSize = pageSize,
            TotalPages = totalPages, HasNextPage = page < totalPages, HasPreviousPage = page > 1
        });
    }
}
