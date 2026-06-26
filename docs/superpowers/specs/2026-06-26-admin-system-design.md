# Admin System Design - AISAM

## Overview

Xay dung he thong quan ly admin tich hop chung vao he thong user (AISAM-FE), phan quyen bang role `Admin` duy nhat. Admin co toan quyen truy cap tat ca chuc nang quan tri.

**Date:** 2026-06-26
**Status:** Design Approved - Ready for Implementation

## Architecture

### High-Level

```
AISAM-FE (Next.js 15)
├── (dashboard)/*    → User routes (co san)
└── (admin)/*        → Admin routes (NEW)
    ├── AdminSidebar, AdminHeader, AdminGuard
    └── Dung chung: Auth Store, API Client, Design System

AISAM-BE (.NET 8)
├── Existing Controllers (auth, workspace, content, AI, ...)
└── AdminController.cs  (NEW) → /api/admin/*
    └── Protected by [Authorize(Policy = "AdminOnly")]
```

### Authorization Model

- 1 role `Admin` (UserRoleEnum.Admin = 2) trong JWT claim
- Policy `"AdminOnly"` trong `Program.cs`: `RequireRole("Admin")`
- Admin co full access tat ca admin APIs
- JWT da co role claim tu `AuthService`

### Integration Points

- FE: Admin login dung chung `/login` endpoint. Sau login, check role:
  - `Admin` → redirect `/admin/dashboard`
  - `User` → redirect `/dashboard`
- FE: User header dropdown hien "Admin Panel" link neu role la Admin
- FE: Admin route guard check `isAuthenticated && isAdmin`
- BE: AdminController dung lai cac repository/service co san

## Backend Design

### Authorization Setup (Program.cs)

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
```

### AdminController

All endpoints protected with `[Authorize(Policy = "AdminOnly")]` at controller level.

Base route: `/api/admin`

#### Dashboard

```
GET /api/admin/dashboard
Response: {
  totalUsers, activeUsers, totalWorkspaces,
  totalRevenue, activeSubscriptions,
  recentUsers[], recentPayments[],
  usersByPlan{}, revenueByMonth{}
}
```

#### User Management

```
GET    /api/admin/users?page=&pageSize=&searchTerm=&sortBy=&sortDescending=&role=
GET    /api/admin/users/{id}
       Response: { user, profiles[], workspaces[], payments[], subscriptions[] }
PATCH  /api/admin/users/{id}/role        Body: { role: UserRoleEnum, reason: string }
PATCH  /api/admin/users/{id}/status      Body: { isActive: bool, reason: string }
```

#### Workspace Management

```
GET    /api/admin/workspaces?page=&pageSize=&searchTerm=&status=&plan=
GET    /api/admin/workspaces/{id}
       Response: { workspace, owner, members[], creditWallet, subscription }
PATCH  /api/admin/workspaces/{id}/status Body: { status, reason }
DELETE /api/admin/workspaces/{id}        (Soft delete - da co endpoint, add admin policy)
```

#### Subscription & Payment Management

```
GET    /api/admin/subscriptions?page=&pageSize=&status=&plan=&workspaceId=
GET    /api/admin/subscriptions/{id}
PATCH  /api/admin/subscriptions/{id}     Body: { plan?, isActive?, endDate?, reason }
GET    /api/admin/payments?page=&pageSize=&status=&userId=
GET    /api/admin/payments/{id}
PATCH  /api/admin/payments/{id}/status   Body: { status, reason }
```

#### Dynamic Plan Management

New entity `SubscriptionPlan` with migration:

```
SubscriptionPlan:
  Id, Name, PlanType (enum), Price, Currency, BillingCycle
  CreditsPerCycle, PostQuotaPerCycle, MemberLimit
  MaxCreditBalance, AIFeatureAccess (JSON)
  IsActive, SortOrder, CreatedAt, UpdatedAt

GET    /api/admin/plans
GET    /api/admin/plans/{id}
POST   /api/admin/plans                  Body: { name, planType, price, creditsPerCycle, ... }
PUT    /api/admin/plans/{id}             Body: { name?, price?, creditsPerCycle?, ... }
DELETE /api/admin/plans/{id}             (Soft delete)
```

#### Audit Logs

Reuse existing `AuditLog` entity:

```
GET /api/admin/audit-logs?page=&pageSize=&actorId=&targetTable=&action=&from=&to=
Response: PagedResult<{
  id, actorId, actorEmail, action, targetTable, targetId,
  oldValues (JSON), newValues (JSON), createdAt
}>
```

#### Admin Tools (Seed Data)

```
POST /api/admin/seed/demo-user        Body: { email, password, fullName, planType }
POST /api/admin/seed/batch-users      Body: { count, planType }
```
Both create users with workspaces, profiles, and sample data. Protected to development/demo environments only.

#### System Configuration

```
GET  /api/admin/config
PUT  /api/admin/config                Body: { aiProvider?, emailSettings?, paymentGateway?, ... }
```

Stored in a `SystemConfig` table (key-value pairs) or appsettings override.

### Service Layer

- `AdminService` - aggregates data from existing repositories
- `AdminDashboardService` - computes system stats
- `AdminAuditLogService` - queries + new audit writes
- `PlanService` - CRUD for dynamic plans

### DTOs

New folder: `AISAM.Common/Dtos/Admin/`
- `AdminDashboardDto.cs`
- `AdminUserDetailDto.cs`
- `AdminUserListDto.cs`
- `AdminWorkspaceListDto.cs`
- `AdminWorkspaceDetailDto.cs`
- `AdminSubscriptionDto.cs`
- `AdminPaymentDto.cs`
- `SubscriptionPlanDto.cs`
- `AdminAuditLogDto.cs`
- `SystemConfigDto.cs`

### Database Migrations

1. `SubscriptionPlan` entity + migration (new table)
2. `SystemConfig` entity + migration (new table)
3. Audit log auto-write in service layer (existing `audit_logs` table)

## Frontend Design

### Route Structure

```
src/app/
├── (dashboard)/          ← User routes (existing, unchanged)
└── (admin)/              ← Admin route group (NEW)
    ├── layout.tsx        ← AdminLayout: Guard + Sidebar + Header + Content
    ├── dashboard/
    │   └── page.tsx      ← System stats overview
    ├── users/
    │   ├── page.tsx      ← User list table
    │   └── [id]/
    │       └── page.tsx  ← User detail (tabs: Overview, Workspaces, Payments)
    ├── workspaces/
    │   ├── page.tsx      ← Workspace list table
    │   └── [id]/
    │       └── page.tsx  ← Workspace detail (Members, Subscription, Wallet)
    ├── subscriptions/
    │   └── page.tsx      ← All subscriptions table
    ├── payments/
    │   └── page.tsx      ← All payments table
    ├── plans/
    │   ├── page.tsx      ← Plans list + sort
    │   ├── new/
    │   │   └── page.tsx  ← Create plan form
    │   └── [id]/
    │       └── page.tsx  ← Edit plan form
    ├── audit-logs/
    │   └── page.tsx      ← Audit logs table with filters
    ├── tools/
    │   └── page.tsx      ← Seed data forms + batch actions
    └── config/
        └── page.tsx      ← System config form (sections: AI, Email, Payment)
```

### Auth Flow

1. Login page (`/login`) unchanged - dung chung cho user va admin
2. Sau login thanh cong, check `user.role`:
   - `Admin` hoac `2` → redirect `/admin/dashboard`
   - `User`, `Vendor` → redirect `/dashboard`
3. Header user dropdown: hien "Admin Panel" link khi `isAdmin === true`
4. AdminGuard trong `(admin)/layout.tsx`:
   ```
   if (!isAuthenticated) → redirect /login
   if (!isAdmin) → redirect /dashboard + toast "Access denied"
   ```

### Component Tree

```
(admin)/layout.tsx
├── ThemeProvider (existing)
├── ToastProvider (existing)
├── QueryClientProvider (existing)
├── AdminGuard
│   ├── Loading: "Verifying access..."
│   ├── Forbidden: redirect
│   └── Authorized:
│       ├── AdminSidebar
│       │   ├── NavItem: Dashboard
│       │   ├── NavItem: Users
│       │   ├── NavItem: Workspaces
│       │   ├── NavItem: Subscriptions
│       │   ├── NavItem: Payments
│       │   ├── NavItem: Plans
│       │   ├── NavItem: Audit Logs
│       │   ├── Divider
│       │   ├── NavItem: Tools
│       │   └── NavItem: Configuration
│       ├── AdminHeader
│       │   ├── Page title (breadcrumb)
│       │   ├── Link "User App" → /dashboard
│       │   └── UserMenu (logout)
│       └── {children} (main content)
```

### Shared Components

New admin-specific components:
- `src/components/admin/AdminSidebar.tsx`
- `src/components/admin/AdminHeader.tsx`
- `src/components/admin/AdminGuard.tsx`
- `src/components/admin/AdminDataTable.tsx` - Generic table with search, sort, pagination
- `src/components/admin/AdminStatsCard.tsx` - Dashboard metrics card
- `src/components/admin/AdminStatusBadge.tsx` - Status indicator
- `src/components/admin/AdminEmptyState.tsx` - Empty state placeholder
- `src/components/admin/AdminConfirmDialog.tsx` - Confirmation modal

Reuse from existing:
- `src/lib/shared/Toast.tsx`
- `src/lib/shared/ConfirmationModal.tsx`
- `src/lib/shared/ThemeProvider.tsx`
- All shadcn-style UI components (Button, Input, Select, Badge, Card, Modal, Skeleton...)

### Hooks (TanStack Query)

- `useAdminDashboard()` - GET `/api/admin/dashboard`
- `useAdminUsers(query)` - GET `/api/admin/users`
- `useAdminUserDetail(id)` - GET `/api/admin/users/{id}`
- `useUpdateUserRole()` - PATCH mutation
- `useUpdateUserStatus()` - PATCH mutation
- `useAdminWorkspaces(query)` - GET `/api/admin/workspaces`
- `useAdminWorkspaceDetail(id)` - GET `/api/admin/workspaces/{id}`
- `useUpdateWorkspaceStatus()` - PATCH mutation
- `useAdminSubscriptions(query)` - GET `/api/admin/subscriptions`
- `useUpdateSubscription()` - PATCH mutation
- `useAdminPayments(query)` - GET `/api/admin/payments`
- `useUpdatePaymentStatus()` - PATCH mutation
- `useAdminPlans()` - GET `/api/admin/plans`
- `useCreatePlan()` - POST mutation
- `useUpdatePlan()` - PUT mutation
- `useDeletePlan()` - DELETE mutation
- `useAdminAuditLogs(query)` - GET `/api/admin/audit-logs`
- `useSeedDemoUser()` - POST mutation
- `useSeedBatchUsers()` - POST mutation
- `useAdminConfig()` - GET/PUT mutation

### Auth Store Extension

Add computed `isAdmin` to existing Zustand auth store:

```typescript
isAdmin: boolean  // user?.role === "Admin" || user?.role === 2
```

### API Client Extension

Admin API client builds on existing fetcher:
- Injects `Authorization: Bearer <token>` (existing)
- Does NOT inject `X-Workspace-Id` for `/api/admin/*` routes (existing middleware skips unknown prefixes)
- On 403: show toast + redirect
- On 401: attempt refresh, on failure redirect login

### Page States (every page)

| State | Handling |
|-------|----------|
| Loading | Skeleton / spinner |
| Empty | AdminEmptyState with message |
| Error | Error card + retry button |
| Success | Render data |
| Backend not ready (404) | Info banner "API chua active" |

## Design Tokens

All admin pages use existing AISAM design system from `DESIGN_SYSTEM.md`:
- Colors: Primary (#004ccd), Secondary (#731be5), Surface (#faf8ff)
- Typography: Plus Jakarta Sans, Headline/body/label scale
- Spacing: 8px grid, 24px gutter
- Radius: 8px (base), 16px (cards), 9999px (badges)
- Effects: Level 1 shadow for cards, glass panels for modals

## Error Handling

### Backend
- All admin endpoints return `GenericResponse<T>` envelope
- 401 on missing/invalid token
- 403 on non-admin role
- 400 on validation errors (FluentValidation)
- 404 on resource not found
- 500 on server errors (caught by middleware)

### Frontend
- TanStack Query error boundary per page
- Toast notifications for mutation success/failure
- ConfirmationModal for destructive actions (delete workspace, change role, seed data)
- AdminGuard blocks rendering before auth check complete

## Security

- Backend enforces admin role via policy (server-side, non-bypassable)
- Frontend guard is UX only, NOT security
- Admin API requests do not include `X-Workspace-Id`
- Audit log records all admin write actions
- Seed endpoints gated to development environment (check `IWebHostEnvironment`)

## Testing Strategy

### Backend
- Unit tests: AdminService, PlanService, AdminDashboardService
- Integration tests: AdminController endpoints (auth, pagination, mutations)
- Test all admin endpoints with:
  - Valid admin token → 200
  - Valid user token (non-admin) → 403
  - Missing token → 401
  - Invalid data → 400

### Frontend
- Unit tests: AdminGuard, isAdminRole helper
- Integration tests: AdminSidebar, AdminDataTable with MSW
- E2E: Admin login → dashboard → user list → user detail flow

## Implementation Phases

### Backend Phases (10A → 10G)

| Phase | Content | Files |
|-------|---------|-------|
| 10A | Admin Policy + Dashboard API | Program.cs, AdminController, AdminDashboardService, DTOs |
| 10B | User Management API | User admin endpoints in AdminController, DTOs |
| 10C | Workspace Management API | Workspace admin endpoints, DTOs |
| 10D | Subscription & Payment API | Sub/payment admin endpoints, DTOs |
| 10E | Dynamic Plans CRUD + Migration | SubscriptionPlan entity, migration, PlanService, endpoints |
| 10F | Audit Log API + Admin Tools | Audit log endpoints + seed endpoints + DevelopmentOnly guard |
| 10G | System Config API | SystemConfig entity, migration, endpoints |

### Frontend Phases (20A → 20H)

| Phase | Content | Files |
|-------|---------|-------|
| 20A | Admin Layout + Sidebar + Guard + Auth | layout.tsx, AdminSidebar, AdminHeader, AdminGuard, auth store update |
| 20B | Dashboard Page | (admin)/dashboard/page.tsx, useAdminDashboard, AdminStatsCard |
| 20C | User Management Pages | users/page.tsx, users/[id]/page.tsx, hooks, AdminDataTable |
| 20D | Workspace Management Pages | workspaces/page.tsx, workspaces/[id]/page.tsx, hooks |
| 20E | Subscription & Payment Pages | subscriptions/page.tsx, payments/page.tsx, hooks |
| 20F | Dynamic Plans Pages | plans/page.tsx, plans/new/page.tsx, plans/[id]/page.tsx, hooks |
| 20G | Audit Logs + Tools + Config Pages | audit-logs/page.tsx, tools/page.tsx, config/page.tsx, hooks |
| 20H | Polish + Responsive + Final Integration | All admin pages, responsive testing, edge cases |

### Dependency Order

```
10A (Auth Policy) → 10B, 10C, 10D, 10E, 10F, 10G (parallelizable after 10A)
20A (Layout) → 20B → 20C, 20D, 20E, 20F, 20G (parallelizable after 20B) → 20H
```

Backend must be done before frontend for each corresponding feature.
