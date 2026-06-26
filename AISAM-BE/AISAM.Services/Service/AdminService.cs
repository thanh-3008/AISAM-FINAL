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
}
