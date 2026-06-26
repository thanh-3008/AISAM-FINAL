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
}
