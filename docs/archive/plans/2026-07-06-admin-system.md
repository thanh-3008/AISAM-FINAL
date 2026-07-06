# Admin System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build an integrated admin panel into AISAM with a separate route group, dedicated AdminLayout, and full system management capabilities (users, workspaces, payments, content, analytics, audit logs, settings).

**Architecture:** Next.js App Router route group `(admin)` with dedicated `AdminLayout`, 7 new backend controllers under `/api/admin` with `[Authorize(Roles = nameof(UserRoleEnum.Admin))]`, 1 new DB table `system_settings`, admin seed via migration. Admin routes are outside `ActiveWorkspaceMiddleware` protected prefixes so no workspace context needed.

**Tech Stack:** .NET 8 / C# 12 (backend), Next.js 16 App Router + React 19 + Tailwind CSS v4 + Material Symbols icons (frontend), PostgreSQL via EF Core 9

**Spec reference:** `docs/superpowers/specs/2026-07-06-admin-system-design.md`

---

## File Structure Map

```
AISAM-BE/
├── AISAM.Data/
│   ├── Model/SystemSetting.cs                              [NEW]
│   └── Enumeration/UserRoleEnum.cs                         [EXISTS - Admin=2]
├── AISAM.Repositories/
│   ├── IR repositories/ISystemSettingRepository.cs          [NEW]
│   ├── Repository/SystemSettingRepository.cs               [NEW]
│   └── AISAMContext.cs                                     [MODIFY - add DbSet]
├── AISAM.Services/
│   ├── IServices/
│   │   ├── IAdminService.cs                                [NEW]
│   │   ├── IAdminDashboardService.cs                       [NEW]
│   │   └── IAdminSettingsService.cs                        [NEW]
│   └── Service/
│       ├── AdminService.cs                                 [NEW]
│       ├── AdminDashboardService.cs                        [NEW]
│       └── AdminSettingsService.cs                         [NEW]
├── AISAM.API/
│   ├── Controllers/
│   │   ├── AdminDashboardController.cs                     [NEW]
│   │   ├── AdminUsersController.cs                         [NEW]
│   │   ├── AdminWorkspacesController.cs                    [NEW]
│   │   ├── AdminPaymentsController.cs                      [NEW]
│   │   ├── AdminContentController.cs                       [NEW]
│   │   ├── AdminAuditLogsController.cs                     [NEW]
│   │   └── AdminSettingsController.cs                      [NEW]
│   ├── Middleware/
│   │   └── ActiveWorkspaceMiddleware.cs                    [NO CHANGE - admin routes auto-skip]
│   └── Program.cs                                          [MODIFY - register services]
├── AISAM.Common/
│   └── Models/                                             [NEW DTOs as needed]

AISAM-FE/
├── src/
│   ├── middleware.ts                                        [NEW]
│   ├── app/
│   │   ├── (auth)/login/page.tsx                           [MODIFY - admin redirect]
│   │   └── (admin)/                                        [NEW route group]
│   │       ├── layout.tsx                                   [NEW]
│   │       └── admin/
│   │           ├── dashboard/page.tsx                       [NEW]
│   │           ├── users/page.tsx                           [NEW]
│   │           ├── users/[id]/page.tsx                      [NEW]
│   │           ├── workspaces/page.tsx                      [NEW]
│   │           ├── workspaces/[id]/page.tsx                 [NEW]
│   │           ├── payments/page.tsx                        [NEW]
│   │           ├── subscriptions/page.tsx                   [NEW]
│   │           ├── content/page.tsx                         [NEW]
│   │           ├── analytics/page.tsx                       [NEW]
│   │           ├── audit-logs/page.tsx                      [NEW]
│   │           └── settings/
│   │               ├── page.tsx                             [NEW]
│   │               ├── ai-providers/page.tsx                 [NEW]
│   │               ├── email/page.tsx                       [NEW]
│   │               └── system/page.tsx                      [NEW]
│   ├── components/
│   │   └── admin/
│   │       ├── AdminSidebar.tsx                             [NEW]
│   │       ├── AdminHeader.tsx                              [NEW]
│   │       ├── AdminDataTable.tsx                           [NEW]
│   │       ├── AdminStatsCard.tsx                           [NEW]
│   │       └── StatusBadge.tsx                              [NEW]
│   ├── services/
│   │   └── adminService.ts                                  [NEW]
│   ├── lib/
│   │   └── auth.ts                                          [MODIFY - add getUserRole]
│   └── hooks/
│       └── useAdminGuard.ts                                 [NEW]
```

---

## Phase 1 — Foundation

### Task 1: Add getUserRole to auth module

**Files:**
- Modify: `AISAM-FE/src/lib/auth.ts`

- [ ] **Step 1: Add getUserRoleFromToken and CLAIM_ROLE constant**

The project already has `CLAIM_ROLE` defined. Add a new export function `getUserRoleFromToken()`.

Read the file at `AISAM-FE/src/lib/auth.ts`, find the CLAIM_ROLE constant (around line 15), verify it exists. Then add after the `getUserFromToken` function:

```ts
export function getUserRoleFromToken(): string | null {
  if (typeof window === "undefined") return null;
  try {
    const token = getToken();
    if (!token) return null;
    const decoded = JSON.parse(atob(token.split(".")[1]));
    return (
      decoded[CLAIM_ROLE] ||
      decoded["role"] ||
      decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
      null
    );
  } catch {
    return null;
  }
}

export function isAdmin(): boolean {
  return getUserRoleFromToken() === "Admin";
}
```

- [ ] **Step 2: Verify by running existing tests**

Run: `npm test -- --run` in `AISAM-FE`
Expected: All existing tests still pass

- [ ] **Step 3: Commit**

```bash
git add AISAM-FE/src/lib/auth.ts
git commit -m "feat: add getUserRoleFromToken and isAdmin helpers"
```

---

### Task 2: Create middleware.ts for route protection

**Files:**
- Create: `AISAM-FE/src/middleware.ts`

- [ ] **Step 1: Create the middleware**

```ts
import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const CLAIM_ROLE =
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role";

function getRoleFromRequest(request: NextRequest): string | null {
  const token = request.cookies.get("aisam_token")?.value;
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return (
      payload[CLAIM_ROLE] ||
      payload["role"] ||
      payload[
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
      ] ||
      null
    );
  } catch {
    return null;
  }
}

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const role = getRoleFromRequest(request);

  if (pathname.startsWith("/admin") && role !== "Admin") {
    const url = request.nextUrl.clone();
    url.pathname = "/dashboard";
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*"],
};
```

- [ ] **Step 2: Commit**

```bash
git add AISAM-FE/src/middleware.ts
git commit -m "feat: add middleware to protect /admin routes"
```

---

### Task 3: Modify login page to redirect admin users

**Files:**
- Modify: `AISAM-FE/src/app/(auth)/login/page.tsx`

- [ ] **Step 1: Read current login page**

Read `AISAM-FE/src/app/(auth)/login/page.tsx` and locate the `handleSubmit` function and the redirect logic (near `router.push(getRedirectUrl())`).

- [ ] **Step 2: Add admin-aware redirect**

After the line that calls `setStoredUser(result.data.user)` in the `handleSubmit` function, add an admin redirect check. Find:

```tsx
router.push(getRedirectUrl());
```

Replace the redirect logic block (the part after storing tokens and before the router.push) with:

```tsx
if (result.data.user) {
  setStoredUser(result.data.user);
}
// Fetch full profile to get role
try {
  const meRes = await apiClient("/auth/me");
  if (meRes?.data?.role === "Admin") {
    router.push("/admin/dashboard");
    return;
  }
} catch {
  // fall through to normal redirect
}
router.push(getRedirectUrl());
```

- [ ] **Step 3: Commit**

```bash
git add AISAM-FE/src/app/\(auth\)/login/page.tsx
git commit -m "feat: redirect admin users to /admin/dashboard after login"
```

---

### Task 4: Create SystemSetting entity

**Files:**
- Create: `AISAM-BE/AISAM.Data/Model/SystemSetting.cs`
- Modify: `AISAM-BE/AISAM.Repositories/AISAMContext.cs`

- [ ] **Step 1: Create the entity**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISAM.Data.Model
{
    [Table("system_settings")]
    public class SystemSetting
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Column("value", TypeName = "jsonb")]
        public string Value { get; set; } = "{}";

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }
}
```

- [ ] **Step 2: Register in DbContext**

Read `AISAM-BE/AISAM.Repositories/AISAMContext.cs`. Find the existing `DbSet` declarations (e.g., `public DbSet<AuditLog> AuditLogs { get; set; }`). Add after the last `DbSet`:

```csharp
public DbSet<SystemSetting> SystemSettings { get; set; }
```

Then in `OnModelCreating`, add after the last entity configuration:

```csharp
modelBuilder.Entity<SystemSetting>(entity =>
{
    entity.HasKey(ss => ss.Id);
    entity.HasIndex(ss => ss.Key).IsUnique();
});
```

- [ ] **Step 3: Create migration**

```bash
dotnet ef migrations add AddSystemSettings -p AISAM-BE/AISAM.Repositories -s AISAM-BE/AISAM.API -o Migrations
```

- [ ] **Step 4: Add admin seed to migration Up method**

Find the generated migration file in `AISAM-BE/AISAM.Repositories/Migrations/`. Edit the `Up` method, adding after `CreateTable` for `system_settings`:

```csharp
migrationBuilder.InsertData(
    table: "users",
    columns: new[] { "id", "email", "full_name", "role", "is_email_verified", "password_hash", "password_salt", "created_at" },
    values: new object[] {
        Guid.NewGuid(),
        "admin@aisam.com",
        "Super Admin",
        2, // Admin
        true,
        "$2a$11$K7Q5pY8z8z8z8z8z8z8z8u", // bcrypt hash placeholder - replace with real hash
        "$2a$11$K7Q5pY8z8z8z8z8z8z8z8",
        DateTime.UtcNow
    });
```

Note: The actual password hash must be generated. Replace the placeholder with a real bcrypt hash matching your password policy.

- [ ] **Step 5: Build and verify**

```bash
dotnet build AISAM-BE/AISAM.sln
```

Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add AISAM-BE/AISAM.Data/Model/SystemSetting.cs AISAM-BE/AISAM.Repositories/AISAMContext.cs AISAM-BE/AISAM.Repositories/Migrations/
git commit -m "feat: add SystemSetting entity with migration and admin seed"
```

---

### Task 5: Extend repositories with admin query methods

**Files:**
- Modify: `AISAM-BE/AISAM.Repositories/IRepositories/IUserRepository.cs`
- Modify: `AISAM-BE/AISAM.Repositories/IRepositories/IWorkspaceRepository.cs`
- Modify: `AISAM-BE/AISAM.Repositories/IRepositories/IPaymentRepository.cs`
- Modify: `AISAM-BE/AISAM.Repositories/IRepositories/IContentRepository.cs`
- Modify: `AISAM-BE/AISAM.Repositories/Repository/UserRepository.cs`
- Modify: `AISAM-BE/AISAM.Repositories/Repository/WorkspaceRepository.cs`
- Modify: `AISAM-BE/AISAM.Repositories/Repository/PaymentRepository.cs`
- Modify: `AISAM-BE/AISAM.Repositories/Repository/ContentRepository.cs`
- Modify: `AISAM-BE/AISAM.Common/Dtos/Response/UserListDto.cs`

- [ ] **Step 1: Extend UserListDto with admin fields**

Read `AISAM-BE/AISAM.Common/Dtos/Response/UserListDto.cs`. Add nullable fields (won't break existing consumers):

```csharp
namespace AISAM.Common.Dtos.Response;

public class UserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SocialAccountsCount { get; set; }
    public string? FullName { get; set; }
    public int? Role { get; set; }
    public string? RoleName { get; set; }
    public bool? IsEmailVerified { get; set; }
}
```

- [ ] **Step 2: Add methods to IUserRepository**

Read `IUserRepository.cs`, add after existing methods:

```csharp
Task<int> GetCountAsync(CancellationToken cancellationToken = default);
Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
Task<PagedResult<UserListDto>> GetPagedUsersWithRoleFilterAsync(PaginationRequest request, int? role, bool? isEmailVerified, string? search, CancellationToken cancellationToken = default);
```

- [ ] **Step 3: Add methods to UserRepository**

Read `UserRepository.cs`, implement:

```csharp
public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
{
    return await _context.Users.CountAsync(cancellationToken);
}

public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
{
    var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    if (user != null)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public async Task<PagedResult<UserListDto>> GetPagedUsersWithRoleFilterAsync(
    PaginationRequest request, int? role, bool? isEmailVerified, string? search, CancellationToken cancellationToken = default)
{
    var query = _context.Users.AsNoTracking();

    if (role.HasValue)
        query = query.Where(u => (int)u.Role == role.Value);
    if (isEmailVerified.HasValue)
        query = query.Where(u => u.IsEmailVerified == isEmailVerified.Value);
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(u => u.Email.Contains(search) || (u.FullName != null && u.FullName.Contains(search)));

    var total = await query.CountAsync(cancellationToken);
    var users = await query
        .OrderByDescending(u => u.CreatedAt)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);

    var dtos = users.Select(u => new UserListDto
    {
        Id = u.Id,
        Email = u.Email,
        FullName = u.FullName,
        Role = (int)u.Role,
        RoleName = u.Role.ToString(),
        IsEmailVerified = u.IsEmailVerified,
        CreatedAt = u.CreatedAt
    }).ToList();

    return new PagedResult<UserListDto> { Items = dtos, Total = total };
}
```

- [ ] **Step 4: Add methods to IWorkspaceRepository**

```csharp
Task<PagedResult<Workspace>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
```

- [ ] **Step 5: Add methods to WorkspaceRepository**

```csharp
public async Task<PagedResult<Workspace>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
{
    var query = _context.Workspaces.AsNoTracking();
    var total = await query.CountAsync(cancellationToken);
    var items = await query
        .OrderByDescending(w => w.CreatedAt)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);
    return new PagedResult<Workspace> { Items = items, Total = total };
}

public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
{
    var ws = await _context.Workspaces.FindAsync(new object[] { id }, cancellationToken);
    if (ws != null)
    {
        _context.Workspaces.Remove(ws);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 6: Add methods to IPaymentRepository**

```csharp
Task<PagedResult<Payment>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
```

- [ ] **Step 7: Add methods to PaymentRepository**

```csharp
public async Task<PagedResult<Payment>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
{
    var query = _context.Payments.AsNoTracking();
    var total = await query.CountAsync(cancellationToken);
    var items = await query
        .OrderByDescending(p => p.CreatedAt)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);
    return new PagedResult<Payment> { Items = items, Total = total };
}
```

- [ ] **Step 8: Add methods to IContentRepository**

```csharp
Task<PagedResult<Content>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
```

- [ ] **Step 9: Add methods to ContentRepository**

```csharp
public async Task<PagedResult<Content>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
{
    var query = _context.Contents.AsNoTracking().Where(c => !c.IsDeleted);
    var total = await query.CountAsync(cancellationToken);
    var items = await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(cancellationToken);
    return new PagedResult<Content> { Items = items, Total = total };
}

public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
{
    var content = await _context.Contents.FindAsync(new object[] { id }, cancellationToken);
    if (content != null)
    {
        content.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

- [ ] **Step 10: Build**

```bash
dotnet build AISAM-BE/AISAM.sln
```

- [ ] **Step 11: Commit**

```bash
git add AISAM-BE/AISAM.Repositories/
git commit -m "feat: extend repositories with admin query and delete methods"
```

---

### Task 6: Create backend services

**Files:**
- Create: `AISAM-BE/AISAM.Repositories/IRepositories/ISystemSettingRepository.cs`
- Create: `AISAM-BE/AISAM.Repositories/Repository/SystemSettingRepository.cs`
- Create: `AISAM-BE/AISAM.Services/IServices/IAdminService.cs`
- Create: `AISAM-BE/AISAM.Services/IServices/IAdminDashboardService.cs`
- Create: `AISAM-BE/AISAM.Services/IServices/IAdminSettingsService.cs`
- Create: `AISAM-BE/AISAM.Services/Service/AdminService.cs`
- Create: `AISAM-BE/AISAM.Services/Service/AdminDashboardService.cs`
- Create: `AISAM-BE/AISAM.Services/Service/AdminSettingsService.cs`
- Modify: `AISAM-BE/AISAM.API/Program.cs`

- [ ] **Step 1: Create ISystemSettingRepository**

```csharp
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories
{
    public interface ISystemSettingRepository
    {
        Task<SystemSetting?> GetByKeyAsync(string key);
        Task<List<SystemSetting>> GetAllAsync();
        Task<SystemSetting> UpsertAsync(SystemSetting setting);
    }
}
```

- [ ] **Step 2: Create SystemSettingRepository**

```csharp
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;

namespace AISAM.Repositories.Repository
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly AisamContext _context;

        public SystemSettingRepository(AisamContext context)
        {
            _context = context;
        }

        public async Task<SystemSetting?> GetByKeyAsync(string key)
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<List<SystemSetting>> GetAllAsync()
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SystemSetting> UpsertAsync(SystemSetting setting)
        {
            var existing = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == setting.Key);

            if (existing != null)
            {
                existing.Value = setting.Value;
                existing.Description = setting.Description;
                existing.UpdatedBy = setting.UpdatedBy;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.SystemSettings.Add(setting);
            }

            await _context.SaveChangesAsync();
            return existing ?? setting;
        }
    }
}
```

- [ ] **Step 3: Create IAdminService**

```csharp
using AISAM.Common;
using AISAM.Common.Dtos;

namespace AISAM.Services.IServices
{
    public interface IAdminService
    {
        // Users
        Task<GenericResponse<PagedResult<UserListDto>>> GetUsersAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetUserDetailAsync(Guid adminUserId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SetUserStatusAsync(Guid adminUserId, Guid userId, bool isActive, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> DeleteUserAsync(Guid adminUserId, Guid userId, CancellationToken cancellationToken = default);

        // Workspaces
        Task<GenericResponse<object>> GetWorkspacesAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetWorkspaceDetailAsync(Guid adminUserId, Guid workspaceId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SetWorkspaceStatusAsync(Guid adminUserId, Guid workspaceId, int status, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> DeleteWorkspaceAsync(Guid adminUserId, Guid workspaceId, CancellationToken cancellationToken = default);

        // Payments
        Task<GenericResponse<object>> GetPaymentsAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);

        // Content
        Task<GenericResponse<object>> GetAllContentAsync(Guid adminUserId, PaginationRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SetContentStatusAsync(Guid adminUserId, Guid contentId, int status, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 4: Create IAdminDashboardService**

```csharp
using AISAM.Common;

namespace AISAM.Services.IServices
{
    public interface IAdminDashboardService
    {
        Task<GenericResponse<object>> GetSummaryAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetChartsAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<GenericResponse<object>> GetRevenueStatsAsync(Guid adminUserId, string period, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 5: Create IAdminSettingsService**

```csharp
using AISAM.Common;

namespace AISAM.Services.IServices
{
    public interface IAdminSettingsService
    {
        Task<GenericResponse<object>> GetAllSettingsAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> UpsertSettingAsync(Guid adminUserId, string key, string value, string? description, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> UpsertSettingsBatchAsync(Guid adminUserId, Dictionary<string, string> settings, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 6: Create AdminService** (abbreviated — full implementation in code)

```csharp
using AISAM.Common;
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

        ws.Status = (Data.Enumeration.WorkspaceStatusEnum)status;
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

        content.Status = (Data.Enumeration.ContentStatusEnum)status;
        await _contentRepository.UpdateAsync(content, cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Content status updated.");
    }
}
}
```

- [ ] **Step 7: Create AdminDashboardService**

```csharp
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
                UserRegistrations = new object[] { },
                Revenue = new object[] { }
            });
        }

        public async Task<GenericResponse<object>> GetRevenueStatsAsync(Guid adminUserId, string period, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            return GenericResponse<object>.CreateSuccess(new
            {
                Period = period,
                TotalRevenue = 0m
            });
        }
    }
}
```

- [ ] **Step 8: Create AdminSettingsService**

```csharp
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service
{
    public sealed class AdminSettingsService : IAdminSettingsService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISystemSettingRepository _systemSettingRepository;

        public AdminSettingsService(
            IUserRepository userRepository,
            ISystemSettingRepository systemSettingRepository)
        {
            _userRepository = userRepository;
            _systemSettingRepository = systemSettingRepository;
        }

        public async Task<GenericResponse<object>> GetAllSettingsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var settings = await _systemSettingRepository.GetAllAsync();
            return GenericResponse<object>.CreateSuccess(settings);
        }

        public async Task<GenericResponse<bool>> UpsertSettingAsync(
            Guid adminUserId, string key, string value, string? description, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var setting = new SystemSetting
            {
                Key = key,
                Value = value,
                Description = description,
                UpdatedBy = adminUserId
            };

            await _systemSettingRepository.UpsertAsync(setting);
            return GenericResponse<bool>.CreateSuccess(true, "Setting saved.");
        }

        public async Task<GenericResponse<bool>> UpsertSettingsBatchAsync(
            Guid adminUserId, Dictionary<string, string> settings, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            foreach (var kvp in settings)
            {
                var setting = new SystemSetting
                {
                    Key = kvp.Key,
                    Value = kvp.Value,
                    UpdatedBy = adminUserId
                };
                await _systemSettingRepository.UpsertAsync(setting);
            }

            return GenericResponse<bool>.CreateSuccess(true, "Settings saved.");
        }
    }
}
```

- [ ] **Step 9: Register in Program.cs**

Read `AISAM-BE/AISAM.API/Program.cs`. Find the service registration section (where `AddScoped` calls are). Add:

```csharp
builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminSettingsService, AdminSettingsService>();
```

- [ ] **Step 10: Build**

```bash
dotnet build AISAM-BE/AISAM.sln
```

Expected: Build succeeds

- [ ] **Step 11: Commit**

```bash
git add AISAM-BE/
git commit -m "feat: add admin services and SystemSetting repository"
```

---

### Task 7: Create backend admin controllers

**Files:**
- Create: `AISAM-BE/AISAM.API/Controllers/AdminDashboardController.cs`
- Create: `AISAM-BE/AISAM.API/Controllers/AdminUsersController.cs`
- Create: `AISAM-BE/AISAM.API/Controllers/AdminWorkspacesController.cs`
- Create: `AISAM-BE/AISAM.API/Controllers/AdminPaymentsController.cs`
- Create: `AISAM-BE/AISAM.API/Controllers/AdminContentController.cs`
- Create: `AISAM-BE/AISAM.API/Controllers/AdminAuditLogsController.cs`
- Create: `AISAM-BE/AISAM.API/Controllers/AdminSettingsController.cs`

- [ ] **Step 1: Create AdminDashboardController**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(IAdminDashboardService dashboardService, ILogger<AdminDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GenericResponse<object>>> GetSummary(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _dashboardService.GetSummaryAsync(adminUserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("charts")]
    public async Task<ActionResult<GenericResponse<object>>> GetCharts(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _dashboardService.GetChartsAsync(adminUserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
```

- [ ] **Step 2: Create AdminUsersController**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(IAdminService adminService, ILogger<AdminUsersController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var request = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await _adminService.GetUsersAsync(adminUserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenericResponse<object>>> GetUserDetail(
        Guid id, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetUserDetailAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> SetUserStatus(
        Guid id, [FromBody] SetStatusRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.SetUserStatusAsync(adminUserId, id, request.IsActive, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteUser(
        Guid id, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.DeleteUserAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class SetStatusRequest
{
    public bool IsActive { get; set; }
}
```

- [ ] **Step 3: Create AdminWorkspacesController**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/workspaces")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminWorkspacesController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminWorkspacesController> _logger;

    public AdminWorkspacesController(IAdminService adminService, ILogger<AdminWorkspacesController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetWorkspaces(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var request = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await _adminService.GetWorkspacesAsync(adminUserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenericResponse<object>>> GetWorkspaceDetail(
        Guid id, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.GetWorkspaceDetailAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> SetWorkspaceStatus(
        Guid id, [FromBody] SetWorkspaceStatusRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.SetWorkspaceStatusAsync(adminUserId, id, request.Status, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<GenericResponse<bool>>> DeleteWorkspace(
        Guid id, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.DeleteWorkspaceAsync(adminUserId, id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class SetWorkspaceStatusRequest
{
    public int Status { get; set; }
}
```

- [ ] **Step 4: Create AdminPaymentsController**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminPaymentsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAdminDashboardService _dashboardService;
    private readonly ILogger<AdminPaymentsController> _logger;

    public AdminPaymentsController(
        IAdminService adminService,
        IAdminDashboardService dashboardService,
        ILogger<AdminPaymentsController> logger)
    {
        _adminService = adminService;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetPayments(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var request = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await _adminService.GetPaymentsAsync(adminUserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("revenue/stats")]
    public async Task<ActionResult<GenericResponse<object>>> GetRevenueStats(
        [FromQuery] string period = "month", CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _dashboardService.GetRevenueStatsAsync(adminUserId, period, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
```

- [ ] **Step 5: Create AdminContentController**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/content")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminContentController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminContentController> _logger;

    public AdminContentController(IAdminService adminService, ILogger<AdminContentController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetAllContent(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var request = new PaginationRequest { Page = page, PageSize = pageSize };
        var result = await _adminService.GetAllContentAsync(adminUserId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<GenericResponse<bool>>> SetContentStatus(
        Guid id, [FromBody] SetContentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _adminService.SetContentStatusAsync(adminUserId, id, request.Status, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

public class SetContentStatusRequest
{
    public int Status { get; set; }
}
```

- [ ] **Step 6: Create AdminAuditLogsController**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminAuditLogsController : ControllerBase
{
    private readonly ILogger<AdminAuditLogsController> _logger;

    public AdminAuditLogsController(ILogger<AdminAuditLogsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetAuditLogs(CancellationToken cancellationToken = default)
    {
        return StatusCode(200, GenericResponse<object>.CreateSuccess(new { Items = Array.Empty<object>(), Total = 0 }));
    }
}
```

- [ ] **Step 7: Create AdminSettingsController**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminSettingsController : ControllerBase
{
    private readonly IAdminSettingsService _settingsService;
    private readonly ILogger<AdminSettingsController> _logger;

    public AdminSettingsController(IAdminSettingsService settingsService, ILogger<AdminSettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<object>>> GetSettings(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _settingsService.GetAllSettingsAsync(adminUserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch]
    public async Task<ActionResult<GenericResponse<bool>>> UpsertSettings(
        [FromBody] Dictionary<string, string> settings, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _settingsService.UpsertSettingsBatchAsync(adminUserId, settings, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
```

- [ ] **Step 8: Build**

```bash
dotnet build AISAM-BE/AISAM.sln
```

Expected: Build succeeds with all 7 new controllers

- [ ] **Step 9: Commit**

```bash
git add AISAM-BE/AISAM.API/Controllers/Admin*.cs AISAM-BE/AISAM.API/Controllers/Set*.cs
git commit -m "feat: add 7 admin controllers with role-based authorization"
```

---

### Task 8: Create admin frontend layout & components

**Files:**
- Create: `AISAM-FE/src/app/(admin)/layout.tsx`
- Create: `AISAM-FE/src/components/admin/AdminSidebar.tsx`
- Create: `AISAM-FE/src/components/admin/AdminHeader.tsx`
- Create: `AISAM-FE/src/components/admin/AdminStatsCard.tsx`
- Create: `AISAM-FE/src/components/admin/AdminDataTable.tsx`
- Create: `AISAM-FE/src/components/admin/StatusBadge.tsx`
- Create: `AISAM-FE/src/hooks/useAdminGuard.ts`
- Create: `AISAM-FE/src/services/adminService.ts`

- [ ] **Step 1: Create useAdminGuard hook**

```tsx
"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { isAdmin } from "@/lib/auth";

export function useAdminGuard() {
  const router = useRouter();

  useEffect(() => {
    if (!isAdmin()) {
      router.push("/login");
    }
  }, [router]);

  return { isAdmin: isAdmin() };
}
```

- [ ] **Step 2: Create adminService**

```ts
import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

export interface AdminDashboardSummary {
  totalUsers: number;
  totalWorkspaces: number;
  totalContent: number;
  totalRevenue: number;
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  role: number;
  roleName: string;
  isEmailVerified: boolean;
  createdAt: string;
}

export interface AdminWorkspace {
  id: string;
  name: string;
  workspaceType: number;
  status: number;
  createdAt: string;
}

export interface AdminPayment {
  id: string;
  userId: string;
  amount: number;
  currency: string;
  status: number;
  paymentType: number;
  createdAt: string;
}

export interface AdminContent {
  id: string;
  title: string;
  workspaceId: string;
  status: number;
  isAiGenerated: boolean;
  createdAt: string;
}

export async function fetchAdminDashboardSummary(): Promise<AdminDashboardSummary | null> {
  try {
    const res: GenericResponse<AdminDashboardSummary> = await apiClient("/admin/dashboard/summary");
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminUsers(page = 1, pageSize = 20): Promise<{ items: AdminUser[]; total: number } | null> {
  try {
    const res: GenericResponse<{ items: AdminUser[]; total: number }> = await apiClient(`/admin/users?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminUserDetail(id: string): Promise<AdminUser | null> {
  try {
    const res: GenericResponse<AdminUser> = await apiClient(`/admin/users/${id}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function setUserStatus(id: string, isActive: boolean): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/users/${id}/status`, { data: { isActive }, method: "PATCH" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function deleteUser(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/users/${id}`, { method: "DELETE" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function fetchAdminWorkspaces(page = 1, pageSize = 20): Promise<{ items: AdminWorkspace[]; total: number } | null> {
  try {
    const res: GenericResponse<{ items: AdminWorkspace[]; total: number }> = await apiClient(`/admin/workspaces?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminPayments(page = 1, pageSize = 20): Promise<{ items: AdminPayment[]; total: number } | null> {
  try {
    const res: GenericResponse<{ items: AdminPayment[]; total: number }> = await apiClient(`/admin/payments?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminContent(page = 1, pageSize = 20): Promise<{ items: AdminContent[]; total: number } | null> {
  try {
    const res: GenericResponse<{ items: AdminContent[]; total: number }> = await apiClient(`/admin/content?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
  } catch { return null; }
}
```

- [ ] **Step 3: Create AdminSidebar**

```tsx
"use client";

import { usePathname, useRouter } from "next/navigation";
import { logout } from "@/lib/auth";

type NavItem = {
  label: string;
  href: string;
  icon: string;
};

const adminNavItems: NavItem[] = [
  { label: "Dashboard", href: "/admin/dashboard", icon: "space_dashboard" },
  { label: "Users", href: "/admin/users", icon: "group" },
  { label: "Workspaces", href: "/admin/workspaces", icon: "apartment" },
  { label: "Payments", href: "/admin/payments", icon: "payments" },
  { label: "Content", href: "/admin/content", icon: "description" },
  { label: "Analytics", href: "/admin/analytics", icon: "bar_chart" },
  { label: "Audit Logs", href: "/admin/audit-logs", icon: "history" },
  { label: "Settings", href: "/admin/settings", icon: "settings" },
];

export default function AdminSidebar() {
  const pathname = usePathname();
  const router = useRouter();

  const handleLogout = async () => {
    await logout();
    router.push("/login");
  };

  return (
    <aside className="fixed left-0 top-0 h-full w-64 bg-gray-950 text-gray-100 flex flex-col z-50">
      <div className="p-6 border-b border-gray-800">
        <h1 className="text-xl font-bold text-white">AISAM</h1>
        <p className="text-xs text-gray-500 mt-1">Admin Panel</p>
      </div>

      <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
        {adminNavItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <button
              key={item.href}
              onClick={() => router.push(item.href)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                isActive
                  ? "bg-blue-600 text-white"
                  : "text-gray-400 hover:bg-gray-800 hover:text-gray-200"
              }`}
            >
              <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
              {item.label}
            </button>
          );
        })}
      </nav>

      <div className="p-4 border-t border-gray-800">
        <button
          onClick={handleLogout}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm text-gray-400 hover:bg-gray-800 hover:text-red-400 transition-colors"
        >
          <span className="material-symbols-outlined text-[20px]">logout</span>
          Logout
        </button>
      </div>
    </aside>
  );
}
```

- [ ] **Step 4: Create AdminHeader**

```tsx
"use client";

interface AdminHeaderProps {
  title?: string;
  breadcrumbs?: { label: string; href?: string }[];
}

export default function AdminHeader({ title, breadcrumbs }: AdminHeaderProps) {
  return (
    <header className="sticky top-0 z-40 h-16 bg-gray-50 border-b border-gray-200 flex items-center justify-between px-8">
      <div className="flex items-center gap-4">
        {breadcrumbs && breadcrumbs.length > 0 ? (
          <nav className="flex items-center gap-2 text-sm">
            {breadcrumbs.map((crumb, i) => (
              <span key={i} className="flex items-center gap-2">
                {i > 0 && <span className="text-gray-400">/</span>}
                {crumb.href ? (
                  <a href={crumb.href} className="text-gray-600 hover:text-gray-900">
                    {crumb.label}
                  </a>
                ) : (
                  <span className="text-gray-900 font-medium">{crumb.label}</span>
                )}
              </span>
            ))}
          </nav>
        ) : (
          <h2 className="text-lg font-semibold text-gray-900">{title || "Admin"}</h2>
        )}
      </div>
      <div className="flex items-center gap-3">
        <span className="text-xs px-2.5 py-1 rounded-full bg-red-100 text-red-700 font-medium">
          Admin
        </span>
      </div>
    </header>
  );
}
```

- [ ] **Step 5: Create AdminLayout**

```tsx
"use client";

import AdminSidebar from "@/components/admin/AdminSidebar";
import AdminHeader from "@/components/admin/AdminHeader";
import { useAdminGuard } from "@/hooks/useAdminGuard";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  useAdminGuard();

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <AdminSidebar />
      <div className="flex-1 flex flex-col ml-64">
        {children}
      </div>
    </div>
  );
}
```

- [ ] **Step 6: Create StatusBadge**

```tsx
"use client";

interface StatusBadgeProps {
  status: string;
  variant?: "success" | "warning" | "error" | "info" | "neutral";
}

const variants: Record<string, string> = {
  success: "bg-emerald-100 text-emerald-700",
  warning: "bg-amber-100 text-amber-700",
  error: "bg-red-100 text-red-700",
  info: "bg-blue-100 text-blue-700",
  neutral: "bg-gray-100 text-gray-700",
};

export default function StatusBadge({ status, variant = "neutral" }: StatusBadgeProps) {
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${variants[variant]}`}>
      {status}
    </span>
  );
}
```

- [ ] **Step 7: Create AdminStatsCard**

```tsx
"use client";

interface AdminStatsCardProps {
  title: string;
  value: string | number;
  icon: string;
  change?: string;
  changePositive?: boolean;
}

export default function AdminStatsCard({ title, value, icon, change, changePositive }: AdminStatsCardProps) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
      <div className="flex items-center justify-between mb-3">
        <span className="text-sm text-gray-500">{title}</span>
        <span className="material-symbols-outlined text-2xl text-gray-400">{icon}</span>
      </div>
      <div className="text-2xl font-bold text-gray-900">{value}</div>
      {change && (
        <div className={`text-sm mt-1 ${changePositive ? "text-emerald-600" : "text-red-600"}`}>
          {change}
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 8: Create AdminDataTable**

```tsx
"use client";

interface Column<T> {
  key: string;
  header: string;
  render?: (item: T) => React.ReactNode;
}

interface AdminDataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyField: string;
  onRowClick?: (item: T) => void;
}

export default function AdminDataTable<T extends Record<string, any>>({
  columns,
  data,
  keyField,
  onRowClick,
}: AdminDataTableProps<T>) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <table className="w-full">
        <thead>
          <tr className="bg-gray-50 border-b border-gray-200">
            {columns.map((col) => (
              <th key={col.key} className="text-left text-xs font-semibold text-gray-500 uppercase tracking-wider px-6 py-3">
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {data.map((item) => (
            <tr
              key={item[keyField]}
              onClick={() => onRowClick?.(item)}
              className={`${onRowClick ? "cursor-pointer hover:bg-gray-50" : ""} transition-colors`}
            >
              {columns.map((col) => (
                <td key={col.key} className="px-6 py-4 text-sm text-gray-700">
                  {col.render ? col.render(item) : item[col.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

- [ ] **Step 9: Verify build**

```bash
cd AISAM-FE; npm run build
```

Expected: Build succeeds with new admin components

- [ ] **Step 10: Commit**

```bash
git add AISAM-FE/src/app/\(admin\)/ AISAM-FE/src/components/admin/ AISAM-FE/src/hooks/useAdminGuard.ts AISAM-FE/src/services/adminService.ts
git commit -m "feat: add admin layout, sidebar, header, and base components"
```

---

### Task 9: Create Admin Dashboard page

**Files:**
- Create: `AISAM-FE/src/app/(admin)/admin/dashboard/page.tsx`

- [ ] **Step 1: Create the dashboard page**

```tsx
"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import { fetchAdminDashboardSummary, AdminDashboardSummary } from "@/services/adminService";

export default function AdminDashboardPage() {
  const [summary, setSummary] = useState<AdminDashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchAdminDashboardSummary().then((data) => {
      setSummary(data);
      setLoading(false);
    });
  }, []);

  return (
    <>
      <AdminHeader title="Dashboard" />
      <main className="flex-1 p-8 overflow-y-auto space-y-8">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">System Overview</h2>
          <p className="text-gray-500 mt-1">Monitor key metrics across the platform.</p>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 animate-pulse">
                <div className="h-4 w-24 bg-gray-200 rounded mb-3" />
                <div className="h-8 w-16 bg-gray-200 rounded" />
              </div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <AdminStatsCard title="Total Users" value={summary?.totalUsers ?? 0} icon="group" />
            <AdminStatsCard title="Total Workspaces" value={summary?.totalWorkspaces ?? 0} icon="apartment" />
            <AdminStatsCard title="Total Content" value={summary?.totalContent ?? 0} icon="description" />
            <AdminStatsCard title="Total Revenue" value={`${(summary?.totalRevenue ?? 0).toLocaleString()} VND`} icon="payments" />
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Quick Actions</h3>
            <div className="grid grid-cols-2 gap-3">
              <a href="/admin/users" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">group_add</span>
                Manage Users
              </a>
              <a href="/admin/workspaces" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">apartment</span>
                Manage Workspaces
              </a>
              <a href="/admin/payments" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">receipt_long</span>
                View Payments
              </a>
              <a href="/admin/settings" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">settings</span>
                System Settings
              </a>
            </div>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">System Info</h3>
            <div className="space-y-3 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-500">Admin Role</span>
                <span className="font-medium">Super Admin</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Access Level</span>
                <span className="font-medium">Full System Access</span>
              </div>
            </div>
          </div>
        </div>
      </main>
    </>
  );
}
```

- [ ] **Step 2: Verify build**

```bash
cd AISAM-FE; npm run build
```

Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add AISAM-FE/src/app/\(admin\)/admin/dashboard/
git commit -m "feat: add admin dashboard page with stats and quick actions"
```

---

### Task 10: Create Admin Users page

**Files:**
- Create: `AISAM-FE/src/app/(admin)/admin/users/page.tsx`

- [ ] **Step 1: Create users list page**

```tsx
"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminUsers, setUserStatus, deleteUser, AdminUser } from "@/services/adminService";

export default function AdminUsersPage() {
  const router = useRouter();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminUsers(page);
    if (data) {
      setUsers(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadUsers(); }, [loadUsers]);

  const handleToggleStatus = async (userId: string, currentActive: boolean) => {
    const ok = await setUserStatus(userId, !currentActive);
    if (ok) loadUsers();
  };

  const handleDelete = async (userId: string) => {
    if (!confirm("Are you sure you want to delete this user? This action cannot be undone.")) return;
    const ok = await deleteUser(userId);
    if (ok) loadUsers();
  };

  const columns = [
    { key: "email", header: "Email" },
    { key: "fullName", header: "Name" },
    {
      key: "role",
      header: "Role",
      render: (u: AdminUser) => <StatusBadge status={u.roleName} variant={u.role === 2 ? "error" : "info"} />,
    },
    {
      key: "status",
      header: "Status",
      render: (u: AdminUser) => (
        <StatusBadge status={u.isEmailVerified ? "Active" : "Inactive"} variant={u.isEmailVerified ? "success" : "warning"} />
      ),
    },
    {
      key: "actions",
      header: "Actions",
      render: (u: AdminUser) => (
        <div className="flex items-center gap-2">
          <button
            onClick={(e) => { e.stopPropagation(); handleToggleStatus(u.id, u.isEmailVerified); }}
            className="text-xs px-2 py-1 rounded bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors"
          >
            {u.isEmailVerified ? "Deactivate" : "Activate"}
          </button>
          {u.role !== 2 && (
            <button
              onClick={(e) => { e.stopPropagation(); handleDelete(u.id); }}
              className="text-xs px-2 py-1 rounded bg-red-50 hover:bg-red-100 text-red-600 transition-colors"
            >
              Delete
            </button>
          )}
        </div>
      ),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Users" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Users</h2>
            <p className="text-gray-500 mt-1">{total} total users</p>
          </div>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />
            ))}
          </div>
        ) : (
          <>
            <AdminDataTable
              columns={columns}
              data={users}
              keyField="id"
              onRowClick={(user) => router.push(`/admin/users/${user.id}`)}
            />
            <div className="flex items-center justify-between">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50"
              >
                Previous
              </button>
              <span className="text-sm text-gray-500">Page {page}</span>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={page * 20 >= total}
                className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </>
        )}
      </main>
    </>
  );
}
```

- [ ] **Step 2: Create user detail page**

Create `AISAM-FE/src/app/(admin)/admin/users/[id]/page.tsx`:

```tsx
"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminUserDetail, deleteUser, AdminUser } from "@/services/adminService";

export default function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [user, setUser] = useState<AdminUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    fetchAdminUserDetail(id).then((data) => {
      setUser(data);
      setLoading(false);
    });
  }, [id]);

  if (loading) {
    return (
      <>
        <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Loading..." }]} />
        <main className="flex-1 p-8">
          <div className="animate-pulse space-y-4">
            <div className="h-8 w-64 bg-gray-200 rounded" />
            <div className="h-4 w-96 bg-gray-200 rounded" />
          </div>
        </main>
      </>
    );
  }

  if (!user) {
    return (
      <>
        <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Not Found" }]} />
        <main className="flex-1 p-8">
          <p className="text-gray-500">User not found.</p>
        </main>
      </>
    );
  }

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: user.email }]} />
      <main className="flex-1 p-8 space-y-6">
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">User Details</h3>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div>
              <dt className="text-gray-500">Email</dt>
              <dd className="font-medium text-gray-900">{user.email}</dd>
            </div>
            <div>
              <dt className="text-gray-500">Full Name</dt>
              <dd className="font-medium text-gray-900">{user.fullName}</dd>
            </div>
            <div>
              <dt className="text-gray-500">Role</dt>
              <dd><StatusBadge status={user.roleName} variant={user.role === 2 ? "error" : "info"} /></dd>
            </div>
            <div>
              <dt className="text-gray-500">Status</dt>
              <dd><StatusBadge status={user.isEmailVerified ? "Active" : "Inactive"} variant={user.isEmailVerified ? "success" : "warning"} /></dd>
            </div>
            <div>
              <dt className="text-gray-500">Created At</dt>
              <dd className="font-medium text-gray-900">{new Date(user.createdAt).toLocaleDateString()}</dd>
            </div>
          </dl>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => router.push("/admin/users")}
            className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Back to Users
          </button>
          {user.role !== 2 && (
            <button
              onClick={async () => {
                if (!confirm("Are you sure?")) return;
                await deleteUser(user.id);
                router.push("/admin/users");
              }}
              className="px-4 py-2 text-sm rounded-lg bg-red-600 text-white hover:bg-red-700 transition-colors"
            >
              Delete User
            </button>
          )}
        </div>
      </main>
    </>
  );
}
```

- [ ] **Step 3: Build and commit**

```bash
cd AISAM-FE; npm run build
```

Expected: Build succeeds

```bash
git add AISAM-FE/src/app/\(admin\)/admin/users/
git commit -m "feat: add admin users list and detail pages"
```

---

### Task 11: Create Admin Payments page

**Files:**
- Create: `AISAM-FE/src/app/(admin)/admin/payments/page.tsx`

- [ ] **Step 1: Create payments page**

```tsx
"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminPayments, AdminPayment } from "@/services/adminService";

export default function AdminPaymentsPage() {
  const [payments, setPayments] = useState<AdminPayment[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadPayments = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminPayments(page);
    if (data) {
      setPayments(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadPayments(); }, [loadPayments]);

  const columns = [
    {
      key: "id",
      header: "Transaction ID",
      render: (p: AdminPayment) => <span className="font-mono text-xs text-gray-500">{p.id.substring(0, 8)}...</span>,
    },
    {
      key: "amount",
      header: "Amount",
      render: (p: AdminPayment) => <span className="font-medium">{p.amount.toLocaleString()} {p.currency}</span>,
    },
    {
      key: "status",
      header: "Status",
      render: (p: AdminPayment) => (
        <StatusBadge
          status={p.status === 1 ? "Completed" : p.status === 0 ? "Pending" : "Failed"}
          variant={p.status === 1 ? "success" : p.status === 0 ? "warning" : "error"}
        />
      ),
    },
    {
      key: "createdAt",
      header: "Date",
      render: (p: AdminPayment) => new Date(p.createdAt).toLocaleDateString(),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Payments" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Payments</h2>
          <p className="text-gray-500 mt-1">{total} total transactions</p>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}
          </div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={payments} keyField="id" />
            <div className="flex items-center justify-between">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Previous</button>
              <span className="text-sm text-gray-500">Page {page}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={page * 20 >= total} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Next</button>
            </div>
          </>
        )}
      </main>
    </>
  );
}
```

- [ ] **Step 2: Build and commit**

```bash
cd AISAM-FE; npm run build
```

Expected: Build succeeds

```bash
git add AISAM-FE/src/app/\(admin\)/admin/payments/
git commit -m "feat: add admin payments page"
```

---

## Phase 2 — Extended Admin (tasks outlined, impl details TBD in follow-up plan)

### Task 12: Admin Workspaces page
### Task 13: Admin Content page
### Task 14: Admin Analytics page
### Task 15: Audit logging middleware

---

## Phase 3 — System Configuration (tasks outlined, impl details TBD in follow-up plan)

### Task 16: Admin Audit Logs page
### Task 17: Admin Settings — AI providers
### Task 18: Admin Settings — Email
### Task 19: Admin Settings — System
