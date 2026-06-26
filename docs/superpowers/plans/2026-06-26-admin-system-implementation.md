# Admin System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a fully integrated admin management system with role-based access control, merged into the existing AISAM user app.

**Architecture:** Single `AdminOnly` authorization policy in BE, single `AdminController` with dedicated service classes. FE uses route group `(admin)` within AISAM-FE with dedicated `AdminSidebar`/`AdminHeader`/`AdminGuard` components. Auth shared between user and admin — role check post-login determines redirect.

**Tech Stack:** .NET 8 (BE), Next.js 15 + React 19 + TypeScript + TanStack Query + Zustand (FE)

**Spec:** `docs/superpowers/specs/2026-06-26-admin-system-design.md`

---

## File Structure Map

### BE New Files
```
AISAM.Common/Dtos/Admin/
  AdminDashboardDto.cs
  AdminUserListDto.cs
  AdminUserDetailDto.cs
  AdminWorkspaceListDto.cs
  AdminWorkspaceDetailDto.cs
  AdminSubscriptionDto.cs
  AdminPaymentDto.cs
  AdminPlanDto.cs
  AdminAuditLogDto.cs
  AdminSystemConfigDto.cs
  AdminSeedRequest.cs

AISAM.Data/Model/
  SubscriptionPlan.cs (new entity)
  SystemConfig.cs (new entity)

AISAM.Services/IServices/
  IAdminService.cs
  IPlanService.cs

AISAM.Services/Service/
  AdminService.cs
  PlanService.cs

AISAM.API/Controllers/
  AdminController.cs
```

### BE Modified Files
```
AISAM.API/Program.cs (add AdminOnly policy, register admin DI)
AISAM.Repositories/AISAMContext.cs (add DbSet<SubscriptionPlan>, DbSet<SystemConfig>)
AISAM.Repositories/Migrations/ (2 new migrations)
```

### FE New Files
```
src/app/(admin)/
  layout.tsx
  dashboard/page.tsx
  users/page.tsx
  users/[id]/page.tsx
  workspaces/page.tsx
  workspaces/[id]/page.tsx
  subscriptions/page.tsx
  payments/page.tsx
  plans/page.tsx
  plans/new/page.tsx
  plans/[id]/page.tsx
  audit-logs/page.tsx
  tools/page.tsx
  config/page.tsx

src/components/admin/
  AdminSidebar.tsx
  AdminHeader.tsx
  AdminGuard.tsx
  AdminDataTable.tsx
  AdminStatsCard.tsx
  AdminStatusBadge.tsx
  AdminEmptyState.tsx
  AdminConfirmDialog.tsx

src/hooks/admin/
  useAdminDashboard.ts
  useAdminUsers.ts
  useAdminUserDetail.ts
  useAdminWorkspaces.ts
  useAdminWorkspaceDetail.ts
  useAdminSubscriptions.ts
  useAdminPayments.ts
  useAdminPlans.ts
  useAdminAuditLogs.ts
  useAdminConfig.ts
  useAdminMutations.ts

src/services/
  adminService.ts
```

### FE Modified Files
```
src/lib/auth.ts (add getRoleFromToken, isAdmin helper)
src/app/login/page.tsx (add admin redirect after login)
src/components/layout/Header.tsx (add "Admin Panel" link in user dropdown)
```

---

## Phase 10A: BE - Admin Authorization Policy + Dashboard API

### Task 10A.1: Add AdminOnly authorization policy

**Files:**
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Program.cs`

- [ ] **Step 1: Add AdminOnly policy**

In `Program.cs`, find line 117: `builder.Services.AddAuthorization();` and replace with:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(nameof(AISAM.Data.Enumeration.UserRoleEnum.Admin)));
});
```

- [ ] **Step 2: Build and verify**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add AISAM.API/Program.cs
git commit -m "feat(admin): add AdminOnly authorization policy"
```

---

### Task 10A.2: Create Admin DTOs (dashboard, shared)

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminDashboardDto.cs`
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminPagedResult.cs`

- [ ] **Step 1: Create AdminPagedResult**

```csharp
// AISAM.Common/Dtos/Admin/AdminPagedResult.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminPagedResult<T>
{
    public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

- [ ] **Step 2: Create AdminDashboardDto**

```csharp
// AISAM.Common/Dtos/Admin/AdminDashboardDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalWorkspaces { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<AdminRecentUserDto> RecentUsers { get; set; } = new();
    public List<AdminRecentPaymentDto> RecentPayments { get; set; } = new();
}

public class AdminRecentUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminRecentPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add AISAM.Common/Dtos/Admin/
git commit -m "feat(admin): add admin dashboard DTOs"
```

---

### Task 10A.3: Create IAdminService interface

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\IServices\IAdminService.cs`

- [ ] **Step 1: Write interface**

```csharp
// AISAM.Services/IServices/IAdminService.cs
using AISAM.Common;
using AISAM.Common.Dtos.Admin;

namespace AISAM.Services.IServices;

public interface IAdminService
{
    Task<GenericResponse<AdminDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add AISAM.Services/IServices/IAdminService.cs
git commit -m "feat(admin): add IAdminService interface"
```

---

### Task 10A.4: Create AdminService implementation

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\Service\AdminService.cs`

- [ ] **Step 1: Write service**

```csharp
// AISAM.Services/Service/AdminService.cs
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
        var totalWorkspaces = await _context.Workspaces.CountAsync(w => !w.IsDeleted, cancellationToken);
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
```

- [ ] **Step 2: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add AISAM.Services/Service/AdminService.cs
git commit -m "feat(admin): add AdminService with dashboard stats"
```

---

### Task 10A.5: Create AdminController with dashboard endpoint

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`

- [ ] **Step 1: Write controller**

```csharp
// AISAM.API/Controllers/AdminController.cs
using AISAM.Common;
using AISAM.Common.Dtos.Admin;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<GenericResponse<AdminDashboardDto>>> GetDashboard(
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetDashboardAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
```

- [ ] **Step 2: Register DI in Program.cs**

In `Program.cs`, add after line 169 (after `IWorkspaceDashboardService`):

```csharp
builder.Services.AddScoped<IAdminService, AdminService>();
```

Also add the using statement to imports if needed:
```csharp
using AISAM.Services.IServices;
using AISAM.Services.Service;
```
These are already imported (services are in the existing usings).

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 4: Run and test via Swagger**

```bash
dotnet run --project AISAM.API
```

Open Swagger. Test `GET /api/admin/dashboard`:
- Without token: expect 401
- With non-admin token: expect 403
- With admin token: expect 200 with dashboard data

- [ ] **Step 5: Commit**

```bash
git add AISAM.API/Controllers/AdminController.cs AISAM.API/Program.cs
git commit -m "feat(admin): add AdminController with dashboard endpoint"
```

---

## Phase 10B: BE - User Management API

### Task 10B.1: Create AdminUserListDto

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminUserListDto.cs`

- [ ] **Step 1: Write DTO**

```csharp
// AISAM.Common/Dtos/Admin/AdminUserListDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminUserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int WorkspaceCount { get; set; }
}
```

- [ ] **Step 2: Create AdminUserDetailDto**

```csharp
// AISAM.Common/Dtos/Admin/AdminUserDetailDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminUserDetailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<AdminUserProfileDto> Profiles { get; set; } = new();
    public List<AdminUserWorkspaceDto> Workspaces { get; set; } = new();
    public List<AdminUserPaymentDto> Payments { get; set; } = new();
}

public class AdminUserProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminUserWorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminUserPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

- [ ] **Step 3: Create update request DTOs**

```csharp
// AISAM.Common/Dtos/Admin/AdminSeedRequest.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminUpdateRoleRequest
{
    public string Role { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class AdminUpdateUserStatusRequest
{
    public bool IsActive { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdminSeedDemoUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PlanType { get; set; }
}

public class AdminSeedBatchUsersRequest
{
    public int Count { get; set; } = 5;
    public string? PlanType { get; set; }
}
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build
git add AISAM.Common/Dtos/Admin/
git commit -m "feat(admin): add user and request DTOs"
```

---

### Task 10B.2: Add user management methods to AdminService

**Files:**
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\IServices\IAdminService.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\Service\AdminService.cs`

- [ ] **Step 1: Update IAdminService interface**

Replace the existing interface content with:

```csharp
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
}
```

- [ ] **Step 2: Add methods to AdminService**

Add these methods to `AdminService.cs` (after the existing constructor and GetDashboardAsync):

```csharp
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
            WorkspaceCount = u.WorkspaceMembers.Count(wm => !wm.Workspace.IsDeleted)
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
        Workspaces = user.WorkspaceMembers.Where(wm => !wm.Workspace.IsDeleted).Select(wm => new AdminUserWorkspaceDto
        {
            Id = wm.Workspace.Id,
            Name = wm.Workspace.Name,
            Type = wm.Workspace.Type.ToString(),
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

    // Note: User entity doesn't have IsActive. We store this as a role gate:
    // Setting a user to inactive means revoking all sessions.
    if (!isActive)
    {
        var sessions = await _context.Sessions.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
        _context.Sessions.RemoveRange(sessions);
    }

    user.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync(cancellationToken);

    return GenericResponse<bool>.CreateSuccess(true, isActive ? "User activated." : "User deactivated (all sessions revoked).");
}
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add AISAM.Services/IServices/IAdminService.cs AISAM.Services/Service/AdminService.cs
git commit -m "feat(admin): add user management methods to AdminService"
```

---

### Task 10B.3: Add user endpoints to AdminController

**Files:**
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`

- [ ] **Step 1: Add user management endpoints**

Add these methods to `AdminController.cs` (after the existing GetDashboard method):

```csharp
[HttpGet("users")]
public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminUserListDto>>>> GetUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchTerm = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool sortDescending = true,
    [FromQuery] string? role = null,
    CancellationToken cancellationToken = default)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var result = await _adminService.GetUsersAsync(page, pageSize, searchTerm, sortBy, sortDescending, role, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpGet("users/{id:guid}")]
public async Task<ActionResult<GenericResponse<AdminUserDetailDto>>> GetUserDetail(
    Guid id, CancellationToken cancellationToken = default)
{
    var result = await _adminService.GetUserDetailAsync(id, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpPatch("users/{id:guid}/role")]
public async Task<ActionResult<GenericResponse<bool>>> UpdateUserRole(
    Guid id, [FromBody] AdminUpdateRoleRequest request, CancellationToken cancellationToken = default)
{
    var result = await _adminService.UpdateUserRoleAsync(id, request.Role, request.Reason, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpPatch("users/{id:guid}/status")]
public async Task<ActionResult<GenericResponse<bool>>> UpdateUserStatus(
    Guid id, [FromBody] AdminUpdateUserStatusRequest request, CancellationToken cancellationToken = default)
{
    var result = await _adminService.UpdateUserStatusAsync(id, request.IsActive, request.Reason, cancellationToken);
    return StatusCode(result.StatusCode, result);
}
```

- [ ] **Step 2: Build and test**

```bash
dotnet build
dotnet run --project AISAM.API
```

Test with Swagger:
- `GET /api/admin/users?page=1&pageSize=5` → 200 with paged users
- `GET /api/admin/users?searchTerm=admin` → 200 with filtered results
- `GET /api/admin/users/{id}` → 200 with user detail
- `PATCH /api/admin/users/{id}/role` with `{"role":"Admin","reason":"test"}` → 200
- Without admin token → 403

- [ ] **Step 3: Commit**

```bash
git add AISAM.API/Controllers/AdminController.cs
git commit -m "feat(admin): add user management endpoints"
```

---

## Phase 10C: BE - Workspace Management API

### Task 10C.1: Create workspace DTOs and add methods

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminWorkspaceListDto.cs`
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminWorkspaceDetailDto.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\IServices\IAdminService.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\Service\AdminService.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`

- [ ] **Step 1: Create workspace DTOs**

```csharp
// AISAM.Common/Dtos/Admin/AdminWorkspaceListDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminWorkspaceListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public decimal CreditBalance { get; set; }
    public DateTime CreatedAt { get; set; }
}

// AISAM.Common/Dtos/Admin/AdminWorkspaceDetailDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminWorkspaceDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public AdminWorkspaceOwnerDto Owner { get; set; } = new();
    public List<AdminWorkspaceMemberDto> Members { get; set; } = new();
    public AdminWorkspaceSubscriptionDto? Subscription { get; set; }
    public decimal CreditBalance { get; set; }
}

public class AdminWorkspaceOwnerDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

public class AdminWorkspaceMemberDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class AdminWorkspaceSubscriptionDto
{
    public Guid Id { get; set; }
    public string Plan { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class AdminUpdateWorkspaceStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Add interface methods**

In `IAdminService.cs`, add:

```csharp
Task<GenericResponse<AdminPagedResult<AdminWorkspaceListDto>>> GetWorkspacesAsync(int page, int pageSize, string? searchTerm, string? status, string? plan, CancellationToken cancellationToken = default);
Task<GenericResponse<AdminWorkspaceDetailDto>> GetWorkspaceDetailAsync(Guid workspaceId, CancellationToken cancellationToken = default);
```

- [ ] **Step 3: Add service methods**

In `AdminService.cs`, add:

```csharp
public async Task<GenericResponse<AdminPagedResult<AdminWorkspaceListDto>>> GetWorkspacesAsync(
    int page, int pageSize, string? searchTerm, string? status, string? plan, CancellationToken cancellationToken = default)
{
    var query = _context.Workspaces.Where(w => !w.IsDeleted).AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var term = searchTerm.Trim().ToLower();
        query = query.Where(w => w.Name.ToLower().Contains(term));
    }

    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AISAM.Data.Enumeration.WorkspaceStatusEnum>(status, true, out var statusEnum))
    {
        query = query.Where(w => w.Status == statusEnum);
    }

    if (!string.IsNullOrWhiteSpace(plan))
    {
        query = query.Where(w => w.Subscriptions.Any(s => s.IsActive && s.Plan.ToString() == plan));
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
            Type = w.Type.ToString(),
            Status = w.Status.ToString(),
            Plan = w.Subscriptions.FirstOrDefault(s => s.IsActive)!.Plan.ToString(),
            MemberCount = w.Members.Count,
            OwnerEmail = w.Members.FirstOrDefault(m => m.Role == AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Owner)!.User.Email,
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
        .FirstOrDefaultAsync(w => w.Id == workspaceId && !w.IsDeleted, cancellationToken);

    if (workspace == null)
    {
        return GenericResponse<AdminWorkspaceDetailDto>.CreateError("Workspace not found.", System.Net.HttpStatusCode.NotFound);
    }

    var owner = workspace.Members.FirstOrDefault(m => m.Role == AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Owner);
    var activeSub = workspace.Subscriptions.FirstOrDefault(s => s.IsActive);

    var dto = new AdminWorkspaceDetailDto
    {
        Id = workspace.Id,
        Name = workspace.Name,
        Description = workspace.Description ?? string.Empty,
        Type = workspace.Type.ToString(),
        Status = workspace.Status.ToString(),
        ExpiresAt = workspace.ExpiresAt,
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
            JoinedAt = m.CreatedAt
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
```

- [ ] **Step 4: Add controller endpoints**

In `AdminController.cs`, add:

```csharp
[HttpGet("workspaces")]
public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminWorkspaceListDto>>>> GetWorkspaces(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchTerm = null,
    [FromQuery] string? status = null,
    [FromQuery] string? plan = null,
    CancellationToken cancellationToken = default)
{
    if (page < 1) page = 1;
    if (pageSize > 100) pageSize = 100;
    var result = await _adminService.GetWorkspacesAsync(page, pageSize, searchTerm, status, plan, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpGet("workspaces/{id:guid}")]
public async Task<ActionResult<GenericResponse<AdminWorkspaceDetailDto>>> GetWorkspaceDetail(
    Guid id, CancellationToken cancellationToken = default)
{
    var result = await _adminService.GetWorkspaceDetailAsync(id, cancellationToken);
    return StatusCode(result.StatusCode, result);
}
```

Also add the existing `AdminSoftDelete` endpoint to AdminController (consolidate from WorkspaceController):

```csharp
[HttpPatch("workspaces/{id:guid}/status")]
public async Task<ActionResult<GenericResponse<bool>>> UpdateWorkspaceStatus(
    Guid id, [FromBody] AdminUpdateWorkspaceStatusRequest request, CancellationToken cancellationToken = default)
{
    if (!Enum.TryParse<AISAM.Data.Enumeration.WorkspaceStatusEnum>(request.Status, true, out var statusEnum))
    {
        return BadRequest(GenericResponse<bool>.CreateError($"Invalid status: {request.Status}"));
    }

    var workspace = await _context.Workspaces.FindAsync(new object[] { id }, cancellationToken);
    if (workspace == null)
    {
        return NotFound(GenericResponse<bool>.CreateError("Workspace not found.", System.Net.HttpStatusCode.NotFound));
    }

    workspace.Status = statusEnum;
    await _context.SaveChangesAsync(cancellationToken);
    return Ok(GenericResponse<bool>.CreateSuccess(true, $"Workspace status updated to {statusEnum}."));
}
```

NOTE: Add `private readonly AisamContext _context;` field to AdminController (not just AdminService). Alternatively inject `IWorkspaceService` for the status update. Let's use `_context` directly:

Add to AdminController's constructor and field:
```csharp
private readonly AisamContext _context;

public AdminController(IAdminService adminService, AisamContext context)
{
    _adminService = adminService;
    _context = context;
}
```

- [ ] **Step 5: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "feat(admin): add workspace management endpoints"
```

---

## Phase 10D: BE - Subscription & Payment Admin API

### Task 10D: Add subscription and payment admin endpoints

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminSubscriptionDto.cs`
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminPaymentDto.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\IServices\IAdminService.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\Service\AdminService.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`

- [ ] **Step 1: Create subscription and payment DTOs**

```csharp
// AISAM.Common/Dtos/Admin/AdminSubscriptionDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUpdateSubscriptionRequest
{
    public string? Plan { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

// AISAM.Common/Dtos/Admin/AdminPaymentDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminPaymentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public Guid? WorkspaceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUpdatePaymentStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Add interface methods**

In `IAdminService.cs`, add:

```csharp
Task<GenericResponse<AdminPagedResult<AdminSubscriptionDto>>> GetSubscriptionsAsync(int page, int pageSize, string? status, string? plan, Guid? workspaceId, CancellationToken cancellationToken = default);
Task<GenericResponse<bool>> UpdateSubscriptionAsync(Guid subscriptionId, string? plan, bool? isActive, DateTime? endDate, string reason, CancellationToken cancellationToken = default);
Task<GenericResponse<AdminPagedResult<AdminPaymentDto>>> GetPaymentsAsync(int page, int pageSize, string? status, Guid? userId, CancellationToken cancellationToken = default);
Task<GenericResponse<bool>> UpdatePaymentStatusAsync(Guid paymentId, string status, string reason, CancellationToken cancellationToken = default);
```

- [ ] **Step 3: Add service methods**

In `AdminService.cs`, add:

```csharp
public async Task<GenericResponse<AdminPagedResult<AdminSubscriptionDto>>> GetSubscriptionsAsync(
    int page, int pageSize, string? status, string? plan, Guid? workspaceId, CancellationToken cancellationToken = default)
{
    var query = _context.Subscriptions.Where(s => !s.IsDeleted).AsQueryable();

    if (!string.IsNullOrWhiteSpace(status) && bool.TryParse(status, out var isActive))
    {
        query = query.Where(s => s.IsActive == isActive);
    }

    if (!string.IsNullOrWhiteSpace(plan) && Enum.TryParse<AISAM.Data.Enumeration.SubscriptionPlanEnum>(plan, true, out var planEnum))
    {
        query = query.Where(s => s.Plan == planEnum);
    }

    if (workspaceId.HasValue)
    {
        query = query.Where(s => s.WorkspaceId == workspaceId.Value);
    }

    var totalCount = await query.CountAsync(cancellationToken);

    var items = await query
        .OrderByDescending(s => s.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(s => new AdminSubscriptionDto
        {
            Id = s.Id,
            WorkspaceId = s.WorkspaceId,
            WorkspaceName = s.Workspace.Name,
            Plan = s.Plan.ToString(),
            IsActive = s.IsActive,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            CreatedAt = s.CreatedAt
        })
        .ToListAsync(cancellationToken);

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
    if (sub == null)
    {
        return GenericResponse<bool>.CreateError("Subscription not found.", System.Net.HttpStatusCode.NotFound);
    }

    if (!string.IsNullOrWhiteSpace(plan) && Enum.TryParse<AISAM.Data.Enumeration.SubscriptionPlanEnum>(plan, true, out var planEnum))
    {
        sub.Plan = planEnum;
    }
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
    {
        query = query.Where(p => p.Status == statusEnum);
    }

    if (userId.HasValue)
    {
        query = query.Where(p => p.UserId == userId.Value);
    }

    var totalCount = await query.CountAsync(cancellationToken);

    var items = await query
        .OrderByDescending(p => p.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new AdminPaymentDto
        {
            Id = p.Id,
            UserId = p.UserId,
            UserEmail = p.User.Email,
            WorkspaceId = p.WorkspaceId,
            Amount = p.Amount,
            Currency = p.Currency ?? "VND",
            Status = p.Status.ToString(),
            PaymentMethod = p.PaymentMethod,
            TransactionId = p.TransactionId,
            CreatedAt = p.CreatedAt
        })
        .ToListAsync(cancellationToken);

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
    {
        return GenericResponse<bool>.CreateError($"Invalid payment status: {status}", System.Net.HttpStatusCode.BadRequest);
    }

    var payment = await _context.Payments.FindAsync(new object[] { paymentId }, cancellationToken);
    if (payment == null)
    {
        return GenericResponse<bool>.CreateError("Payment not found.", System.Net.HttpStatusCode.NotFound);
    }

    payment.Status = statusEnum;
    await _context.SaveChangesAsync(cancellationToken);
    return GenericResponse<bool>.CreateSuccess(true, "Payment status updated.");
}
```

- [ ] **Step 4: Add controller endpoints**

In `AdminController.cs`, add:

```csharp
[HttpGet("subscriptions")]
public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminSubscriptionDto>>>> GetSubscriptions(
    [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
    [FromQuery] string? status = null, [FromQuery] string? plan = null,
    [FromQuery] Guid? workspaceId = null, CancellationToken cancellationToken = default)
{
    if (page < 1) page = 1;
    if (pageSize > 100) pageSize = 100;
    var result = await _adminService.GetSubscriptionsAsync(page, pageSize, status, plan, workspaceId, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpPatch("subscriptions/{id:guid}")]
public async Task<ActionResult<GenericResponse<bool>>> UpdateSubscription(
    Guid id, [FromBody] AdminUpdateSubscriptionRequest request, CancellationToken cancellationToken = default)
{
    var result = await _adminService.UpdateSubscriptionAsync(id, request.Plan, request.IsActive, request.EndDate, request.Reason, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpGet("payments")]
public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminPaymentDto>>>> GetPayments(
    [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
    [FromQuery] string? status = null, [FromQuery] Guid? userId = null,
    CancellationToken cancellationToken = default)
{
    if (page < 1) page = 1;
    if (pageSize > 100) pageSize = 100;
    var result = await _adminService.GetPaymentsAsync(page, pageSize, status, userId, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpPatch("payments/{id:guid}/status")]
public async Task<ActionResult<GenericResponse<bool>>> UpdatePaymentStatus(
    Guid id, [FromBody] AdminUpdatePaymentStatusRequest request, CancellationToken cancellationToken = default)
{
    var result = await _adminService.UpdatePaymentStatusAsync(id, request.Status, request.Reason, cancellationToken);
    return StatusCode(result.StatusCode, result);
}
```

- [ ] **Step 5: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 6: Commit**

```bash
git add .
git commit -m "feat(admin): add subscription and payment admin endpoints"
```

---

## Phase 10E: BE - Dynamic Plans CRUD API + Migration

### Task 10E.1: Create SubscriptionPlan entity

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Data\Model\SubscriptionPlan.cs`

- [ ] **Step 1: Write entity**

```csharp
// AISAM.Data/Model/SubscriptionPlan.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

[Table("subscription_plans")]
public class SubscriptionPlan
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("plan_type")]
    public int PlanType { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [MaxLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "VND";

    [MaxLength(50)]
    [Column("billing_cycle")]
    public string BillingCycle { get; set; } = "monthly";

    [Column("credits_per_cycle")]
    public int CreditsPerCycle { get; set; }

    [Column("post_quota_per_cycle")]
    public int PostQuotaPerCycle { get; set; }

    [Column("member_limit")]
    public int MemberLimit { get; set; }

    [Column("max_credit_balance")]
    public decimal MaxCreditBalance { get; set; }

    [Column("features", TypeName = "jsonb")]
    public string? Features { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Register DbSet in AISAMContext**

In `AISAMContext.cs`, add after existing DbSets:

```csharp
public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
```

- [ ] **Step 3: Create migration**

```bash
dotnet ef migrations add AddSubscriptionPlans --project AISAM.Repositories --startup-project AISAM.API
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build
git add .
git commit -m "feat(admin): add SubscriptionPlan entity and migration"
```

---

### Task 10E.2: Create PlanService and CRUD endpoints

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\IServices\IPlanService.cs`
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\Service\PlanService.cs`
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminPlanDto.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Program.cs`

- [ ] **Step 1: Create AdminPlanDto**

```csharp
// AISAM.Common/Dtos/Admin/AdminPlanDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PlanType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public int CreditsPerCycle { get; set; }
    public int PostQuotaPerCycle { get; set; }
    public int MemberLimit { get; set; }
    public decimal MaxCreditBalance { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminCreatePlanRequest
{
    public string Name { get; set; } = string.Empty;
    public int PlanType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public string BillingCycle { get; set; } = "monthly";
    public int CreditsPerCycle { get; set; }
    public int PostQuotaPerCycle { get; set; }
    public int MemberLimit { get; set; }
    public decimal MaxCreditBalance { get; set; }
    public int SortOrder { get; set; }
}

public class AdminUpdatePlanRequest
{
    public string? Name { get; set; }
    public int? PlanType { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }
    public int? CreditsPerCycle { get; set; }
    public int? PostQuotaPerCycle { get; set; }
    public int? MemberLimit { get; set; }
    public decimal? MaxCreditBalance { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
```

- [ ] **Step 2: Create IPlanService and PlanService**

```csharp
// AISAM.Services/IServices/IPlanService.cs
using AISAM.Common;
using AISAM.Common.Dtos.Admin;

namespace AISAM.Services.IServices;

public interface IPlanService
{
    Task<GenericResponse<List<AdminPlanDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPlanDto>> CreateAsync(AdminCreatePlanRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<AdminPlanDto>> UpdateAsync(Guid id, AdminUpdatePlanRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

```csharp
// AISAM.Services/Service/PlanService.cs
using AISAM.Common;
using AISAM.Common.Dtos.Admin;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.Services.Service;

public class PlanService : IPlanService
{
    private readonly AisamContext _context;

    public PlanService(AisamContext context)
    {
        _context = context;
    }

    public async Task<GenericResponse<List<AdminPlanDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return GenericResponse<List<AdminPlanDto>>.CreateSuccess(plans);
    }

    public async Task<GenericResponse<AdminPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        if (plan == null)
            return GenericResponse<AdminPlanDto>.CreateError("Plan not found.", HttpStatusCode.NotFound);

        return GenericResponse<AdminPlanDto>.CreateSuccess(MapToDto(plan));
    }

    public async Task<GenericResponse<AdminPlanDto>> CreateAsync(AdminCreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = new SubscriptionPlan
        {
            Name = request.Name,
            PlanType = request.PlanType,
            Price = request.Price,
            Currency = request.Currency,
            BillingCycle = request.BillingCycle,
            CreditsPerCycle = request.CreditsPerCycle,
            PostQuotaPerCycle = request.PostQuotaPerCycle,
            MemberLimit = request.MemberLimit,
            MaxCreditBalance = request.MaxCreditBalance,
            IsActive = true,
            SortOrder = request.SortOrder
        };

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<AdminPlanDto>.CreateSuccess(MapToDto(plan));
    }

    public async Task<GenericResponse<AdminPlanDto>> UpdateAsync(Guid id, AdminUpdatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        if (plan == null)
            return GenericResponse<AdminPlanDto>.CreateError("Plan not found.", HttpStatusCode.NotFound);

        if (request.Name != null) plan.Name = request.Name;
        if (request.PlanType.HasValue) plan.PlanType = request.PlanType.Value;
        if (request.Price.HasValue) plan.Price = request.Price.Value;
        if (request.Currency != null) plan.Currency = request.Currency;
        if (request.BillingCycle != null) plan.BillingCycle = request.BillingCycle;
        if (request.CreditsPerCycle.HasValue) plan.CreditsPerCycle = request.CreditsPerCycle.Value;
        if (request.PostQuotaPerCycle.HasValue) plan.PostQuotaPerCycle = request.PostQuotaPerCycle.Value;
        if (request.MemberLimit.HasValue) plan.MemberLimit = request.MemberLimit.Value;
        if (request.MaxCreditBalance.HasValue) plan.MaxCreditBalance = request.MaxCreditBalance.Value;
        if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
        if (request.SortOrder.HasValue) plan.SortOrder = request.SortOrder.Value;
        plan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<AdminPlanDto>.CreateSuccess(MapToDto(plan));
    }

    public async Task<GenericResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(new object[] { id }, cancellationToken);
        if (plan == null)
            return GenericResponse<bool>.CreateError("Plan not found.", HttpStatusCode.NotFound);

        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Plan deleted.");
    }

    private static AdminPlanDto MapToDto(SubscriptionPlan p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        PlanType = p.PlanType,
        Price = p.Price,
        Currency = p.Currency,
        BillingCycle = p.BillingCycle,
        CreditsPerCycle = p.CreditsPerCycle,
        PostQuotaPerCycle = p.PostQuotaPerCycle,
        MemberLimit = p.MemberLimit,
        MaxCreditBalance = p.MaxCreditBalance,
        IsActive = p.IsActive,
        SortOrder = p.SortOrder,
        CreatedAt = p.CreatedAt
    };
}
```

- [ ] **Step 3: Register DI in Program.cs**

After line 169, add:

```csharp
builder.Services.AddScoped<IPlanService, PlanService>();
```

- [ ] **Step 4: Add plan endpoints to AdminController**

```csharp
[HttpGet("plans")]
public async Task<ActionResult<GenericResponse<List<AdminPlanDto>>>> GetPlans(
    CancellationToken cancellationToken = default)
{
    var result = await _planService.GetAllAsync(cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpGet("plans/{id:guid}")]
public async Task<ActionResult<GenericResponse<AdminPlanDto>>> GetPlan(
    Guid id, CancellationToken cancellationToken = default)
{
    var result = await _planService.GetByIdAsync(id, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpPost("plans")]
public async Task<ActionResult<GenericResponse<AdminPlanDto>>> CreatePlan(
    [FromBody] AdminCreatePlanRequest request, CancellationToken cancellationToken = default)
{
    var result = await _planService.CreateAsync(request, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpPut("plans/{id:guid}")]
public async Task<ActionResult<GenericResponse<AdminPlanDto>>> UpdatePlan(
    Guid id, [FromBody] AdminUpdatePlanRequest request, CancellationToken cancellationToken = default)
{
    var result = await _planService.UpdateAsync(id, request, cancellationToken);
    return StatusCode(result.StatusCode, result);
}

[HttpDelete("plans/{id:guid}")]
public async Task<ActionResult<GenericResponse<bool>>> DeletePlan(
    Guid id, CancellationToken cancellationToken = default)
{
    var result = await _planService.DeleteAsync(id, cancellationToken);
    return StatusCode(result.StatusCode, result);
}
```

Add to AdminController constructor:
```csharp
private readonly IPlanService _planService;

public AdminController(IAdminService adminService, IPlanService planService, AisamContext context)
{
    _adminService = adminService;
    _planService = planService;
    _context = context;
}
```

- [ ] **Step 5: Build and commit**

```bash
dotnet build
git add .
git commit -m "feat(admin): add dynamic plans CRUD endpoints"
```

---

## Phase 10F: BE - Audit Log API + Admin Tools

### Task 10F.1: Add audit log endpoint

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminAuditLogDto.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\IServices\IAdminService.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Services\Service\AdminService.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`

- [ ] **Step 1: Create DTO**

```csharp
// AISAM.Common/Dtos/Admin/AdminAuditLogDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminAuditLogDto
{
    public Guid Id { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public string? Action { get; set; }
    public string? TargetTable { get; set; }
    public string? TargetId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

- [ ] **Step 2: Add interface and service method**

In `IAdminService.cs`:
```csharp
Task<GenericResponse<AdminPagedResult<AdminAuditLogDto>>> GetAuditLogsAsync(int page, int pageSize, Guid? actorId, string? targetTable, string? action, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
```

In `AdminService.cs`:
```csharp
public async Task<GenericResponse<AdminPagedResult<AdminAuditLogDto>>> GetAuditLogsAsync(
    int page, int pageSize, Guid? actorId, string? targetTable, string? action, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
{
    var query = _context.Set<AISAM.Data.Model.AuditLog>().AsQueryable();
    // Note: AuditLog entity already exists in the codebase

    if (actorId.HasValue)
        query = query.Where(a => a.ActorId == actorId.Value);
    if (!string.IsNullOrWhiteSpace(targetTable))
        query = query.Where(a => a.TargetTable != null && a.TargetTable.Contains(targetTable));
    if (!string.IsNullOrWhiteSpace(action))
        query = query.Where(a => a.Action != null && a.Action.Contains(action));
    if (from.HasValue)
        query = query.Where(a => a.CreatedAt >= from.Value);
    if (to.HasValue)
        query = query.Where(a => a.CreatedAt <= to.Value);

    var totalCount = await query.CountAsync(cancellationToken);

    var items = await query
        .OrderByDescending(a => a.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(a => new AdminAuditLogDto
        {
            Id = a.Id,
            ActorId = a.ActorId,
            ActorEmail = a.Actor != null ? a.Actor.Email : null,
            Action = a.Action,
            TargetTable = a.TargetTable,
            TargetId = a.TargetId,
            CreatedAt = a.CreatedAt
        })
        .ToListAsync(cancellationToken);

    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    return GenericResponse<AdminPagedResult<AdminAuditLogDto>>.CreateSuccess(new AdminPagedResult<AdminAuditLogDto>
    {
        Data = items, TotalCount = totalCount, Page = page, PageSize = pageSize,
        TotalPages = totalPages, HasNextPage = page < totalPages, HasPreviousPage = page > 1
    });
}
```

- [ ] **Step 3: Add controller endpoint**

```csharp
[HttpGet("audit-logs")]
public async Task<ActionResult<GenericResponse<AdminPagedResult<AdminAuditLogDto>>>> GetAuditLogs(
    [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
    [FromQuery] Guid? actorId = null, [FromQuery] string? targetTable = null,
    [FromQuery] string? action = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
    CancellationToken cancellationToken = default)
{
    var result = await _adminService.GetAuditLogsAsync(page, pageSize, actorId, targetTable, action, from, to, cancellationToken);
    return StatusCode(result.StatusCode, result);
}
```

---

### Task 10F.2: Add admin tools (seed) endpoints

**Files:**
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`

- [ ] **Step 1: Add seed endpoints**

```csharp
[HttpPost("seed/demo-user")]
public async Task<ActionResult<GenericResponse<object>>> SeedDemoUser(
    [FromBody] AdminSeedDemoUserRequest request, CancellationToken cancellationToken = default)
{
    var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
    if (existing != null)
    {
        return Conflict(GenericResponse<object>.CreateError("Email already exists.", System.Net.HttpStatusCode.Conflict));
    }

    var hasher = new PasswordHasher<User>();
    var user = new User
    {
        Email = request.Email,
        FullName = request.FullName,
        Role = AISAM.Data.Enumeration.UserRoleEnum.User,
        IsEmailVerified = true,
        PasswordHash = hasher.HashPassword(null!, request.Password)
    };
    // Properly hash password using existing AuthService pattern. For simplicity in seed, we'll use BCrypt or similar.
    // Note: Check existing AuthService for the actual hashing method used.
    // For now, use a simple hash:
    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

    _context.Users.Add(user);
    await _context.SaveChangesAsync(cancellationToken);

    return Ok(GenericResponse<object>.CreateSuccess(new { userId = user.Id, email = user.Email }, "Demo user created."));
}

[HttpPost("seed/batch-users")]
public async Task<ActionResult<GenericResponse<object>>> SeedBatchUsers(
    [FromBody] AdminSeedBatchUsersRequest request, CancellationToken cancellationToken = default)
{
    var createdIds = new List<Guid>();
    for (int i = 0; i < Math.Min(request.Count, 50); i++)
    {
        var email = $"demo-user-{Guid.NewGuid().ToString()[..8]}@aisam.dev";
        var user = new User
        {
            Email = email,
            FullName = $"Demo User {i + 1}",
            Role = AISAM.Data.Enumeration.UserRoleEnum.User,
            IsEmailVerified = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo@123")
        };
        _context.Users.Add(user);
        createdIds.Add(user.Id);
    }
    await _context.SaveChangesAsync(cancellationToken);
    return Ok(GenericResponse<object>.CreateSuccess(new { count = createdIds.Count, ids = createdIds }, $"{createdIds.Count} demo users created."));
}
```

Add import: `using Microsoft.AspNetCore.Identity;` (for PasswordHasher) or use BCrypt directly. Check the existing AuthService to match the hashing method.

- [ ] **Step 2: Build and commit**

```bash
dotnet build
git add .
git commit -m "feat(admin): add audit log and seed endpoints"
```

---

## Phase 10G: BE - System Configuration API

### Task 10G: Add system config endpoints

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Data\Model\SystemConfig.cs`
- Create: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Common\Dtos\Admin\AdminSystemConfigDto.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.API\Controllers\AdminController.cs`
- Modify: `D:\final\AISAM-FINAL\AISAM-BE\AISAM.Repositories\AISAMContext.cs`

- [ ] **Step 1: Create SystemConfig entity**

```csharp
// AISAM.Data/Model/SystemConfig.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model;

[Table("system_configs")]
public class SystemConfig
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("value", TypeName = "jsonb")]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Register DbSet and create migration**

In `AISAMContext.cs`:
```csharp
public DbSet<SystemConfig> SystemConfigs { get; set; }
```

```bash
dotnet ef migrations add AddSystemConfigs --project AISAM.Repositories --startup-project AISAM.API
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
```

- [ ] **Step 3: Create DTO**

```csharp
// AISAM.Common/Dtos/Admin/AdminSystemConfigDto.cs
namespace AISAM.Common.Dtos.Admin;

public class AdminSystemConfigDto
{
    public Dictionary<string, object> Config { get; set; } = new();
}

public class AdminUpdateSystemConfigRequest
{
    public Dictionary<string, object> Config { get; set; } = new();
}
```

- [ ] **Step 4: Add controller endpoints**

```csharp
[HttpGet("config")]
public async Task<ActionResult<GenericResponse<AdminSystemConfigDto>>> GetConfig(
    CancellationToken cancellationToken = default)
{
    var configs = await _context.SystemConfigs.ToListAsync(cancellationToken);
    var dict = configs.ToDictionary(c => c.Key, c => (object)c.Value);
    return Ok(GenericResponse<AdminSystemConfigDto>.CreateSuccess(new AdminSystemConfigDto { Config = dict }));
}

[HttpPut("config")]
public async Task<ActionResult<GenericResponse<bool>>> UpdateConfig(
    [FromBody] AdminUpdateSystemConfigRequest request, CancellationToken cancellationToken = default)
{
    foreach (var kvp in request.Config)
    {
        var existing = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.Key == kvp.Key, cancellationToken);
        if (existing != null)
        {
            existing.Value = System.Text.Json.JsonSerializer.Serialize(kvp.Value);
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.SystemConfigs.Add(new AISAM.Data.Model.SystemConfig
            {
                Key = kvp.Key,
                Value = System.Text.Json.JsonSerializer.Serialize(kvp.Value)
            });
        }
    }
    await _context.SaveChangesAsync(cancellationToken);
    return Ok(GenericResponse<bool>.CreateSuccess(true, "Configuration updated."));
}
```

- [ ] **Step 5: Build and commit**

```bash
dotnet build
git add .
git commit -m "feat(admin): add system configuration endpoints"
```

---

## Phase 20A: FE - Admin Layout + Sidebar + Guard + Auth Flow

### Task 20A.1: Add isAdmin helper to auth.ts

**Files:**
- Modify: `D:\final\AISAM-FINAL\AISAM-FE\src\lib\auth.ts`

- [ ] **Step 1: Add getRoleFromToken and isAdmin**

Add to `src/lib/auth.ts` after the existing `getUserFromToken` function:

```typescript
export function getRoleFromToken(): string | null {
  const token = getToken();
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload[CLAIM_ROLE] || null;
  } catch {
    return null;
  }
}

export function isAdmin(): boolean {
  const role = getRoleFromToken();
  return role === "Admin" || role === "2";
}
```

- [ ] **Step 2: Run lint**

```bash
npm run lint
```

Expected: No errors.

- [ ] **Step 3: Commit**

```bash
git add src/lib/auth.ts
git commit -m "feat(admin): add isAdmin helper to auth"
```

---

### Task 20A.2: Create AdminGuard component

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminGuard.tsx`

- [ ] **Step 1: Write component**

```tsx
"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getToken, isAdmin } from "@/lib/auth";

export default function AdminGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [verified, setVerified] = useState(false);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace("/login");
      return;
    }

    if (!isAdmin()) {
      router.replace("/dashboard");
      return;
    }

    setVerified(true);
  }, [router]);

  if (!verified) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-surface">
        <div className="flex flex-col items-center gap-4">
          <div className="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
          <p className="text-body-sm text-on-surface-variant">Verifying access...</p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
```

- [ ] **Step 2: Commit**

```bash
git add src/components/admin/AdminGuard.tsx
git commit -m "feat(admin): add AdminGuard route protection"
```

---

### Task 20A.3: Create AdminSidebar component

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminSidebar.tsx`

- [ ] **Step 1: Write component**

```tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const navItems = [
  { label: "Dashboard", href: "/admin/dashboard", icon: "space_dashboard" },
  { label: "Users", href: "/admin/users", icon: "group" },
  { label: "Workspaces", href: "/admin/workspaces", icon: "workspaces" },
  { label: "Subscriptions", href: "/admin/subscriptions", icon: "subscriptions" },
  { label: "Payments", href: "/admin/payments", icon: "payments" },
  { label: "Plans", href: "/admin/plans", icon: "auto_awesome" },
  { label: "Audit Logs", href: "/admin/audit-logs", icon: "history" },
  { label: "Tools", href: "/admin/tools", icon: "build" },
  { label: "Configuration", href: "/admin/config", icon: "settings" },
];

export default function AdminSidebar() {
  const pathname = usePathname();

  return (
    <aside className="fixed left-0 top-0 h-screen w-[260px] bg-surface-container-lowest border-r border-outline-variant flex flex-col z-40">
      <div className="p-6 border-b border-outline-variant">
        <Link href="/admin/dashboard" className="flex items-center gap-2">
          <span className="material-symbols-outlined text-secondary text-2xl">admin_panel_settings</span>
          <span className="text-headline-sm font-bold text-on-surface">AISAM Admin</span>
        </Link>
      </div>

      <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
        {navItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-4 py-2.5 rounded-xl text-body-sm font-semibold transition-all duration-200 ${
                isActive
                  ? "bg-primary/10 text-primary"
                  : "text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
              }`}
            >
              <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
              {item.label}
              {isActive && (
                <span className="ml-auto w-1.5 h-1.5 rounded-full bg-primary" />
              )}
            </Link>
          );
        })}
      </nav>

      <div className="p-4 border-t border-outline-variant">
        <Link
          href="/dashboard"
          className="flex items-center gap-3 px-4 py-2.5 rounded-xl text-body-sm text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors"
        >
          <span className="material-symbols-outlined text-[20px]">open_in_new</span>
          User App
        </Link>
      </div>
    </aside>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add src/components/admin/AdminSidebar.tsx
git commit -m "feat(admin): add AdminSidebar navigation"
```

---

### Task 20A.4: Create AdminHeader component

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminHeader.tsx`

- [ ] **Step 1: Write component**

```tsx
"use client";

import { useRouter } from "next/navigation";
import { useState, useEffect } from "react";
import { getUserFromToken, logout } from "@/lib/auth";

export default function AdminHeader() {
  const router = useRouter();
  const [user, setUser] = useState<{ name?: string; email?: string } | null>(null);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    setUser(getUserFromToken());
  }, []);

  const handleLogout = async () => {
    await logout();
    router.replace("/login");
  };

  return (
    <header className="h-16 bg-surface-container-lowest border-b border-outline-variant flex items-center justify-between px-6 sticky top-0 z-30">
      <div />
      <div className="relative">
        <button
          onClick={() => setMenuOpen(!menuOpen)}
          className="flex items-center gap-3 px-3 py-2 rounded-xl hover:bg-surface-container transition-colors"
        >
          <div className="w-8 h-8 rounded-full bg-secondary/20 flex items-center justify-center">
            <span className="text-label-sm font-bold text-secondary">
              {user?.name?.charAt(0)?.toUpperCase() || "A"}
            </span>
          </div>
          <div className="text-left hidden sm:block">
            <p className="text-body-sm font-semibold text-on-surface">{user?.name || "Admin"}</p>
            <p className="text-label-xs text-on-surface-variant">{user?.email}</p>
          </div>
          <span className="material-symbols-outlined text-[18px] text-on-surface-variant">expand_more</span>
        </button>

        {menuOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
            <div className="absolute right-0 top-full mt-2 w-56 bg-surface-container-lowest border border-outline-variant rounded-xl shadow-lg z-20 py-2">
              <button
                onClick={handleLogout}
                className="w-full flex items-center gap-3 px-4 py-2.5 text-body-sm text-danger-red hover:bg-surface-container transition-colors"
              >
                <span className="material-symbols-outlined text-[18px]">logout</span>
                Logout
              </button>
            </div>
          </>
        )}
      </div>
    </header>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add src/components/admin/AdminHeader.tsx
git commit -m "feat(admin): add AdminHeader with user menu"
```

---

### Task 20A.5: Create admin layout

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\app\(admin)\layout.tsx`

- [ ] **Step 1: Write layout**

```tsx
import AdminSidebar from "@/components/admin/AdminSidebar";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminGuard from "@/components/admin/AdminGuard";
import { ToastProvider } from "@/contexts/ToastContext";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AdminGuard>
      <div className="min-h-screen bg-surface flex">
        <AdminSidebar />
        <div className="flex-1 flex flex-col ml-[260px]">
          <AdminHeader />
          <main className="flex-1 p-6">{children}</main>
        </div>
      </div>
    </AdminGuard>
  );
}
```

- [ ] **Step 2: Run build**

```bash
npm run build
```

Expected: Build succeeds (admin pages will be empty placeholders, that's fine).

- [ ] **Step 3: Commit**

```bash
git add src/app/\(admin\)/layout.tsx src/components/admin/
git commit -m "feat(admin): add admin layout with sidebar, header, guard"
```

---

### Task 20A.6: Add admin redirect in login page

**Files:**
- Modify: `D:\final\AISAM-FINAL\AISAM-FE\src\app\(auth)\login\page.tsx`

- [ ] **Step 1: Find login success handler**

Read the login page to find where redirect happens after successful login. Add admin role check:

```typescript
import { isAdmin } from "@/lib/auth";

// In the login success handler, after storing tokens:
if (isAdmin()) {
  router.push("/admin/dashboard");
} else {
  router.push("/dashboard");
}
```

- [ ] **Step 2: Add "Admin Panel" link in user Header dropdown**

In `src/components/layout/Header.tsx`, find the user dropdown menu and add:

```tsx
import { isAdmin } from "@/lib/auth";

// In the dropdown menu JSX:
{isAdmin() && (
  <Link
    href="/admin/dashboard"
    className="flex items-center gap-2 px-4 py-2 text-body-sm hover:bg-surface-container transition-colors"
  >
    <span className="material-symbols-outlined text-[18px]">admin_panel_settings</span>
    Admin Panel
  </Link>
)}
```

- [ ] **Step 3: Build and commit**

```bash
npm run build
git add .
git commit -m "feat(admin): add admin redirect on login and admin panel link in header"
```

---

## Phase 20B: FE - Admin Dashboard Page

### Task 20B.1: Create shared admin components

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminStatsCard.tsx`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminDataTable.tsx`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminStatusBadge.tsx`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminEmptyState.tsx`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminConfirmDialog.tsx`

- [ ] **Step 1: Create AdminStatsCard**

```tsx
// src/components/admin/AdminStatsCard.tsx
export default function AdminStatsCard({
  label, value, icon, trend,
}: {
  label: string; value: string | number; icon: string; trend?: string;
}) {
  return (
    <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-5 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-label-sm text-on-surface-variant uppercase tracking-wider">{label}</p>
          <p className="text-headline-lg font-bold text-on-surface mt-1">{value}</p>
          {trend && <p className="text-label-xs text-success-green mt-1">{trend}</p>}
        </div>
        <span className="material-symbols-outlined text-2xl text-primary">{icon}</span>
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Create AdminDataTable**

```tsx
// src/components/admin/AdminDataTable.tsx
"use client";

import { useState } from "react";

interface Column<T> {
  key: string;
  header: string;
  render: (item: T) => React.ReactNode;
  sortable?: boolean;
}

interface Props<T> {
  columns: Column<T>[];
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  onSort?: (key: string, descending: boolean) => void;
  emptyMessage?: string;
  isLoading?: boolean;
}

export default function AdminDataTable<T extends { id: string }>({
  columns, data, totalCount, page, pageSize, totalPages,
  onPageChange, onSort, emptyMessage = "No data found.", isLoading,
}: Props<T>) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-12 bg-surface-container rounded-xl animate-pulse" />
        ))}
      </div>
    );
  }

  if (!data.length) {
    return (
      <div className="text-center py-16">
        <span className="material-symbols-outlined text-4xl text-outline/40">search_off</span>
        <p className="text-body-sm text-on-surface-variant mt-2">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div>
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="border-b border-outline-variant">
              {columns.map((col) => (
                <th key={col.key} className="px-4 py-3 text-label-sm text-on-surface-variant uppercase tracking-wider">
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((item) => (
              <tr key={item.id} className="border-b border-outline-variant/50 hover:bg-surface-container/50 transition-colors">
                {columns.map((col) => (
                  <td key={col.key} className="px-4 py-3 text-body-sm text-on-surface">{col.render(item)}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between px-4 py-3 border-t border-outline-variant mt-2">
          <p className="text-label-sm text-on-surface-variant">
            Showing {(page - 1) * pageSize + 1}-{Math.min(page * pageSize, totalCount)} of {totalCount}
          </p>
          <div className="flex gap-2">
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={page <= 1}
              className="px-3 py-1.5 rounded-lg text-body-sm border border-outline-variant disabled:opacity-30 hover:bg-surface-container transition-colors"
            >
              Previous
            </button>
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={page >= totalPages}
              className="px-3 py-1.5 rounded-lg text-body-sm border border-outline-variant disabled:opacity-30 hover:bg-surface-container transition-colors"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 3: Create AdminStatusBadge**

```tsx
// src/components/admin/AdminStatusBadge.tsx
const variants: Record<string, string> = {
  active: "bg-success-green/10 text-success-green",
  Active: "bg-success-green/10 text-success-green",
  success: "bg-success-green/10 text-success-green",
  Success: "bg-success-green/10 text-success-green",
  suspended: "bg-warning-amber/10 text-warning-amber",
  Suspended: "bg-warning-amber/10 text-warning-amber",
  pending: "bg-warning-amber/10 text-warning-amber",
  Pending: "bg-warning-amber/10 text-warning-amber",
  cancelled: "bg-danger-red/10 text-danger-red",
  Cancelled: "bg-danger-red/10 text-danger-red",
  failed: "bg-danger-red/10 text-danger-red",
  Failed: "bg-danger-red/10 text-danger-red",
  archived: "bg-outline/10 text-on-surface-variant",
  Archived: "bg-outline/10 text-on-surface-variant",
  limited: "bg-outline/10 text-on-surface-variant",
  Limited: "bg-outline/10 text-on-surface-variant",
};

export default function AdminStatusBadge({ status }: { status: string }) {
  const classes = variants[status] || "bg-outline/10 text-on-surface-variant";
  return (
    <span className={`inline-flex px-2.5 py-0.5 rounded-full text-label-xs font-semibold ${classes}`}>
      {status}
    </span>
  );
}
```

- [ ] **Step 4: Create AdminEmptyState and AdminConfirmDialog**

```tsx
// src/components/admin/AdminEmptyState.tsx
export default function AdminEmptyState({ message = "No data found.", icon = "inbox" }: { message?: string; icon?: string }) {
  return (
    <div className="text-center py-16">
      <span className="material-symbols-outlined text-5xl text-outline/30">{icon}</span>
      <p className="text-body-sm text-on-surface-variant mt-3">{message}</p>
    </div>
  );
}
```

```tsx
// src/components/admin/AdminConfirmDialog.tsx
"use client";

interface Props {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
  isLoading?: boolean;
  variant?: "danger" | "warning";
}

export default function AdminConfirmDialog({
  open, title, message, confirmLabel = "Confirm", onConfirm, onCancel, isLoading, variant = "danger",
}: Props) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/30" onClick={onCancel} />
      <div className="relative bg-surface-container-lowest rounded-2xl p-6 max-w-md w-full mx-4 shadow-xl border border-outline-variant">
        <h3 className="text-headline-sm font-semibold text-on-surface">{title}</h3>
        <p className="text-body-sm text-on-surface-variant mt-2">{message}</p>
        <div className="flex justify-end gap-3 mt-6">
          <button
            onClick={onCancel}
            disabled={isLoading}
            className="px-4 py-2 rounded-xl text-body-sm border border-outline-variant hover:bg-surface-container transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={isLoading}
            className={`px-4 py-2 rounded-xl text-body-sm text-white transition-colors ${
              variant === "danger" ? "bg-danger-red hover:bg-danger-red/90" : "bg-warning-amber hover:bg-warning-amber/90"
            }`}
          >
            {isLoading ? "Processing..." : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
```

- [ ] **Step 5: Commit**

```bash
git add src/components/admin/
git commit -m "feat(admin): add shared admin UI components"
```

---

### Task 20B.2: Create admin service and dashboard hook

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\services\adminService.ts`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\hooks\admin\useAdminDashboard.ts`

- [ ] **Step 1: Create adminService**

```typescript
// src/services/adminService.ts
import { apiClient } from "@/lib/apiClient";

export interface AdminDashboardData {
  totalUsers: number;
  activeUsers: number;
  totalWorkspaces: number;
  activeSubscriptions: number;
  totalRevenue: number;
  recentUsers: Array<{ id: string; email: string; fullName?: string; role: string; createdAt: string }>;
  recentPayments: Array<{ id: string; amount: number; currency: string; status: string; userEmail: string; createdAt: string }>;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface AdminUserListItem {
  id: string;
  email: string;
  fullName?: string;
  role: string;
  isEmailVerified: boolean;
  createdAt: string;
  lastLoginAt?: string;
  workspaceCount: number;
}

export interface AdminUserDetail {
  id: string;
  email: string;
  fullName?: string;
  role: string;
  isEmailVerified: boolean;
  createdAt: string;
  lastLoginAt?: string;
  profiles: Array<{ id: string; name: string; companyName?: string; status: string; createdAt: string }>;
  workspaces: Array<{ id: string; name: string; type: string; status: string; role: string; createdAt: string }>;
  payments: Array<{ id: string; amount: number; currency: string; status: string; createdAt: string }>;
}

export async function fetchAdminDashboard(): Promise<AdminDashboardData> {
  const res = await apiClient("/admin/dashboard");
  return res.data;
}

export async function fetchAdminUsers(params: {
  page?: number; pageSize?: number; searchTerm?: string; sortBy?: string; sortDescending?: boolean; role?: string;
}): Promise<PagedResult<AdminUserListItem>> {
  const query = new URLSearchParams();
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));
  if (params.searchTerm) query.set("searchTerm", params.searchTerm);
  if (params.sortBy) query.set("sortBy", params.sortBy);
  if (params.sortDescending !== undefined) query.set("sortDescending", String(params.sortDescending));
  if (params.role) query.set("role", params.role);
  const res = await apiClient(`/admin/users?${query.toString()}`);
  return res.data;
}

export async function fetchAdminUserDetail(userId: string): Promise<AdminUserDetail> {
  const res = await apiClient(`/admin/users/${userId}`);
  return res.data;
}

export async function updateUserRole(userId: string, role: string, reason: string): Promise<void> {
  await apiClient(`/admin/users/${userId}/role`, { method: "PATCH", data: { role, reason } });
}

export async function updateUserStatus(userId: string, isActive: boolean, reason: string): Promise<void> {
  await apiClient(`/admin/users/${userId}/status`, { method: "PATCH", data: { isActive, reason } });
}
```

- [ ] **Step 2: Create useAdminDashboard hook**

```typescript
// src/hooks/admin/useAdminDashboard.ts
"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchAdminDashboard } from "@/services/adminService";

export function useAdminDashboard() {
  return useQuery({
    queryKey: ["admin", "dashboard"],
    queryFn: fetchAdminDashboard,
    staleTime: 60_000,
  });
}
```

- [ ] **Step 3: Create the admin dashboard page**

```tsx
// src/app/(admin)/dashboard/page.tsx
"use client";

import { useAdminDashboard } from "@/hooks/admin/useAdminDashboard";
import AdminStatsCard from "@/components/admin/AdminStatsCard";

export default function AdminDashboardPage() {
  const { data, isLoading, error } = useAdminDashboard();

  if (isLoading) {
    return (
      <div className="space-y-6">
        <h1 className="text-headline-md font-bold text-on-surface">Dashboard</h1>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-28 bg-surface-container rounded-2xl animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  if (error || !data) {
    return <p className="text-danger-red">Failed to load dashboard.</p>;
  }

  const stats = [
    { label: "Total Users", value: data.totalUsers, icon: "group" },
    { label: "Active (30d)", value: data.activeUsers, icon: "person_check" },
    { label: "Workspaces", value: data.totalWorkspaces, icon: "workspaces" },
    { label: "Active Subs", value: data.activeSubscriptions, icon: "subscriptions" },
    { label: "Revenue", value: `${(data.totalRevenue / 1000).toFixed(0)}K VND`, icon: "payments" },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-headline-md font-bold text-on-surface">Dashboard</h1>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
        {stats.map((s) => (
          <AdminStatsCard key={s.label} {...s} />
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <section className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-6">
          <h2 className="text-headline-sm font-semibold text-on-surface mb-4">Recent Users</h2>
          <ul className="space-y-2">
            {data.recentUsers.map((u) => (
              <li key={u.id} className="flex items-center justify-between py-2 border-b border-outline-variant/50 last:border-0">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{u.fullName || u.email}</p>
                  <p className="text-label-xs text-on-surface-variant">{u.email}</p>
                </div>
                <span className="text-label-xs text-on-surface-variant">{new Date(u.createdAt).toLocaleDateString()}</span>
              </li>
            ))}
          </ul>
        </section>

        <section className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-6">
          <h2 className="text-headline-sm font-semibold text-on-surface mb-4">Recent Payments</h2>
          <ul className="space-y-2">
            {data.recentPayments.map((p) => (
              <li key={p.id} className="flex items-center justify-between py-2 border-b border-outline-variant/50 last:border-0">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{p.userEmail}</p>
                  <p className="text-label-xs text-on-surface-variant">{(p.amount / 1000).toFixed(0)}K {p.currency}</p>
                </div>
                <span className={`text-label-xs font-semibold px-2 py-0.5 rounded-full ${
                  p.status === "Success" ? "bg-success-green/10 text-success-green" : "bg-outline/10 text-on-surface-variant"
                }`}>{p.status}</span>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Build and commit**

```bash
npm run build
git add .
git commit -m "feat(admin): add admin dashboard page with stats and recent activity"
```

---

## Phase 20C: FE - User Management Pages

### Task 20C: Create user list and detail pages

**Files:**
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\hooks\admin\useAdminUsers.ts`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\hooks\admin\useAdminUserDetail.ts`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\hooks\admin\useAdminMutations.ts`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\app\(admin)\users\page.tsx`
- Create: `D:\final\AISAM-FINAL\AISAM-FE\src\app\(admin)\users\[id]\page.tsx`

- [ ] **Step 1: Create hooks**

```typescript
// src/hooks/admin/useAdminUsers.ts
"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchAdminUsers } from "@/services/adminService";

export function useAdminUsers(params: {
  page?: number; pageSize?: number; searchTerm?: string; sortBy?: string; sortDescending?: boolean; role?: string;
}) {
  return useQuery({
    queryKey: ["admin", "users", params],
    queryFn: () => fetchAdminUsers(params),
    staleTime: 30_000,
  });
}
```

```typescript
// src/hooks/admin/useAdminUserDetail.ts
"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchAdminUserDetail } from "@/services/adminService";

export function useAdminUserDetail(userId: string) {
  return useQuery({
    queryKey: ["admin", "users", userId],
    queryFn: () => fetchAdminUserDetail(userId),
    enabled: !!userId,
  });
}
```

```typescript
// src/hooks/admin/useAdminMutations.ts
"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateUserRole, updateUserStatus } from "@/services/adminService";

export function useUpdateUserRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, role, reason }: { userId: string; role: string; reason: string }) =>
      updateUserRole(userId, role, reason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["admin", "users"] }); },
  });
}

export function useUpdateUserStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, isActive, reason }: { userId: string; isActive: boolean; reason: string }) =>
      updateUserStatus(userId, isActive, reason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["admin", "users"] }); },
  });
}
```

- [ ] **Step 2: Create user list page**

```tsx
// src/app/(admin)/users/page.tsx
"use client";

import { useState } from "react";
import Link from "next/link";
import { useAdminUsers } from "@/hooks/admin/useAdminUsers";
import AdminDataTable from "@/components/admin/AdminDataTable";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";
import type { AdminUserListItem } from "@/services/adminService";

export default function AdminUsersPage() {
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");

  const { data, isLoading } = useAdminUsers({ page, pageSize: 10, searchTerm: searchTerm || undefined });

  const columns = [
    { key: "email", header: "Email", render: (u: AdminUserListItem) => <span className="font-medium">{u.email}</span> },
    { key: "fullName", header: "Name", render: (u: AdminUserListItem) => u.fullName || "-" },
    { key: "role", header: "Role", render: (u: AdminUserListItem) => <AdminStatusBadge status={u.role} /> },
    { key: "verified", header: "Verified", render: (u: AdminUserListItem) => u.isEmailVerified ? "Yes" : "No" },
    { key: "workspaces", header: "Workspaces", render: (u: AdminUserListItem) => u.workspaceCount },
    { key: "createdAt", header: "Joined", render: (u: AdminUserListItem) => new Date(u.createdAt).toLocaleDateString() },
    {
      key: "actions", header: "", render: (u: AdminUserListItem) => (
        <Link href={`/admin/users/${u.id}`} className="text-primary hover:underline text-body-sm">View</Link>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-headline-md font-bold text-on-surface">Users</h1>
      </div>

      <div className="flex gap-4">
        <input
          type="text"
          placeholder="Search by email or name..."
          value={searchTerm}
          onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
          className="flex-1 max-w-sm px-4 py-2 rounded-xl border border-outline-variant bg-surface-container-lowest text-body-sm focus:outline-none focus:border-primary"
        />
      </div>

      <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl overflow-hidden">
        <AdminDataTable
          columns={columns}
          data={data?.data || []}
          totalCount={data?.totalCount || 0}
          page={page}
          pageSize={10}
          totalPages={data?.totalPages || 1}
          onPageChange={setPage}
          isLoading={isLoading}
          emptyMessage="No users found."
        />
      </div>
    </div>
  );
}
```

- [ ] **Step 3: Create user detail page**

```tsx
// src/app/(admin)/users/[id]/page.tsx
"use client";

import { useParams } from "next/navigation";
import { useAdminUserDetail } from "@/hooks/admin/useAdminUserDetail";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";
import AdminEmptyState from "@/components/admin/AdminEmptyState";

export default function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading, error } = useAdminUserDetail(id);

  if (isLoading) return <div className="animate-pulse space-y-4">Loading...</div>;
  if (error || !data) return <p className="text-danger-red">Failed to load user.</p>;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
          <span className="text-headline-sm font-bold text-primary">{data.fullName?.charAt(0)?.toUpperCase() || data.email.charAt(0).toUpperCase()}</span>
        </div>
        <div>
          <h1 className="text-headline-md font-bold text-on-surface">{data.fullName || data.email}</h1>
          <p className="text-body-sm text-on-surface-variant">{data.email} · <AdminStatusBadge status={data.role} /></p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-4">
          <p className="text-label-sm text-on-surface-variant uppercase">Email Verified</p>
          <p className="text-body-lg font-semibold mt-1">{data.isEmailVerified ? "Yes" : "No"}</p>
        </div>
        <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-4">
          <p className="text-label-sm text-on-surface-variant uppercase">Joined</p>
          <p className="text-body-lg font-semibold mt-1">{new Date(data.createdAt).toLocaleDateString()}</p>
        </div>
        <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-4">
          <p className="text-label-sm text-on-surface-variant uppercase">Last Login</p>
          <p className="text-body-lg font-semibold mt-1">{data.lastLoginAt ? new Date(data.lastLoginAt).toLocaleDateString() : "Never"}</p>
        </div>
      </div>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-6">
        <h2 className="text-headline-sm font-semibold text-on-surface mb-4">Workspaces</h2>
        {data.workspaces.length === 0 ? (
          <AdminEmptyState message="No workspaces." />
        ) : (
          <ul className="divide-y divide-outline-variant">
            {data.workspaces.map((w) => (
              <li key={w.id} className="py-3 flex items-center justify-between">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{w.name}</p>
                  <p className="text-label-xs text-on-surface-variant">{w.type} · {w.role}</p>
                </div>
                <AdminStatusBadge status={w.status} />
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-6">
        <h2 className="text-headline-sm font-semibold text-on-surface mb-4">Recent Payments</h2>
        {data.payments.length === 0 ? (
          <AdminEmptyState message="No payments." />
        ) : (
          <ul className="divide-y divide-outline-variant">
            {data.payments.map((p) => (
              <li key={p.id} className="py-3 flex items-center justify-between">
                <span className="text-body-sm text-on-surface">{(p.amount / 1000).toFixed(0)}K {p.currency}</span>
                <div className="flex items-center gap-3">
                  <span className="text-label-xs text-on-surface-variant">{new Date(p.createdAt).toLocaleDateString()}</span>
                  <AdminStatusBadge status={p.status} />
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
```

- [ ] **Step 4: Build and commit**

```bash
npm run build
git add .
git commit -m "feat(admin): add user list and detail pages"
```

---

## Phase 20D-20G: FE - Remaining Admin Pages

For brevity, all remaining pages follow the same pattern as user management. Each page needs:

1. A hook in `src/hooks/admin/`
2. Service methods in `src/services/adminService.ts`
3. A page in `src/app/(admin)/`

### Task 20D: Workspace management pages (list + detail)
- `src/hooks/admin/useAdminWorkspaces.ts`
- `src/hooks/admin/useAdminWorkspaceDetail.ts`
- `src/app/(admin)/workspaces/page.tsx` — table: name, type, status, plan, members, owner, credits
- `src/app/(admin)/workspaces/[id]/page.tsx` — detail: members list, subscription info, credit wallet

### Task 20E: Subscription & payment pages
- `src/hooks/admin/useAdminSubscriptions.ts`
- `src/hooks/admin/useAdminPayments.ts`
- `src/app/(admin)/subscriptions/page.tsx` — table: workspace, plan, active/end date, actions
- `src/app/(admin)/payments/page.tsx` — table: user, amount, status, date, actions

### Task 20F: Dynamic plans pages (list + create + edit)
- `src/hooks/admin/useAdminPlans.ts`
- `src/app/(admin)/plans/page.tsx` — card list: plan name, price, credits, member limit, active toggle
- `src/app/(admin)/plans/new/page.tsx` — create form
- `src/app/(admin)/plans/[id]/page.tsx` — edit form

### Task 20G: Audit logs, tools, config pages
- `src/hooks/admin/useAdminAuditLogs.ts`
- `src/hooks/admin/useAdminConfig.ts`
- `src/app/(admin)/audit-logs/page.tsx` — table with filters: actor, table, action, date range
- `src/app/(admin)/tools/page.tsx` — seed demo user form + batch seed
- `src/app/(admin)/config/page.tsx` — config form sections

### Implementation Pattern (for each page):

**Hook example (useAdminWorkspaces.ts):**
```typescript
"use client";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";

export function useAdminWorkspaces(params: { page?: number; pageSize?: number; searchTerm?: string; status?: string }) {
  return useQuery({
    queryKey: ["admin", "workspaces", params],
    queryFn: async () => {
      const query = new URLSearchParams();
      if (params.page) query.set("page", String(params.page));
      if (params.pageSize) query.set("pageSize", String(params.pageSize));
      if (params.searchTerm) query.set("searchTerm", params.searchTerm);
      if (params.status) query.set("status", params.status);
      const res = await apiClient(`/admin/workspaces?${query.toString()}`);
      return res.data;
    },
    staleTime: 30_000,
  });
}
```

**Service additions (adminService.ts) — add these exports:**
```typescript
export async function fetchAdminWorkspaces(params: { page?: number; pageSize?: number; searchTerm?: string; status?: string }) {
  const query = new URLSearchParams();
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));
  if (params.searchTerm) query.set("searchTerm", params.searchTerm);
  if (params.status) query.set("status", params.status);
  const res = await apiClient(`/admin/workspaces?${query.toString()}`);
  return res.data;
}
```

Each page follows the same structure as the Users page above, adapted for the specific data.

---

## Phase 20H: FE - Polish + Responsive + Final Integration

### Task 20H.1: Error handling and loading states

- [ ] Review all admin pages for consistent loading skeleton/spinner
- [ ] Add error boundaries for each admin page
- [ ] Ensure toast notifications on mutation success/failure
- [ ] Add confirmation dialogs for destructive actions

### Task 20H.2: Responsive testing

- [ ] Test admin sidebar collapse on mobile
- [ ] Test table overflow on narrow screens

### Task 20H.3: Final build

```bash
npm run build
npm run lint
```

Expected: Build succeeds, no lint errors.

### Task 20H.4: Commit and tag

```bash
git add .
git commit -m "feat(admin): complete admin system implementation"
```

---

## Verification Checklist

After all phases, verify:

### Backend
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes (all existing tests)
- [ ] Swagger shows all `/api/admin/*` endpoints
- [ ] Admin token → 200 on all admin endpoints
- [ ] Non-admin token → 403 on all admin endpoints
- [ ] No token → 401 on all admin endpoints
- [ ] Migrations applied: `SubscriptionPlans`, `SystemConfigs` tables exist

### Frontend
- [ ] `npm run build` passes
- [ ] `npm run lint` passes
- [ ] Admin login → `/admin/dashboard` redirect
- [ ] User login → `/dashboard` redirect (unchanged)
- [ ] Admin sidebar shows all navigation items
- [ ] Non-admin cannot access `/admin/*` routes (redirects to `/dashboard`)
- [ ] Admin dashboard renders stats from real API
- [ ] User list pageable, searchable
- [ ] User detail shows workspaces and payments
- [ ] Admin header has logout + "User App" link
- [ ] User header has "Admin Panel" link (for admin users only)

---

## Implementation Order Summary

```
Phase 10A → 10B → 10C → 10D → 10E → 10F → 10G (BE done)
Phase 20A → 20B → 20C → 20D → 20E → 20F → 20G → 20H (FE done)
```

Each phase builds on the previous. BE must complete before corresponding FE phase. Tasks within a phase can sometimes be parallelized but are listed sequentially for safety.
