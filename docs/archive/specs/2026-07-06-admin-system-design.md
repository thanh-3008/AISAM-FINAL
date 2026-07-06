# Admin System Design

**Date:** 2026-07-06  
**Status:** Approved  
**Project:** AISAM — AI-Powered Social Media Advertising Manager

## 1. Overview

Integrate a full-featured admin panel into the existing AISAM monolith. Admin uses the same codebase and login, but sees a separate interface with system-wide management capabilities. No separate deployment or codebase.

### Key Decisions

| Decision | Choice |
|----------|--------|
| Admin scope | Super Admin — manage all users, workspaces, payments, content, analytics, audit logs, system config |
| Number of admins | Single Super Admin, seeded directly into database |
| UI approach | Separate route group `(admin)/admin/*` with dedicated AdminLayout |
| Login flow | Shared `/login` page, middleware redirects based on `role` claim |
| Feature set | Full package: Users, Workspaces, Payments, Subscriptions, Content, Analytics, Audit Logs, System Config |

## 2. Route Structure

```
src/app/
├── (auth)/                     # Existing — login, register, forgot/reset password
├── (dashboard)/                # Existing — user-facing pages
│   ├── layout.tsx              # UserLayout (Sidebar + Header)
│   └── ...
│
└── (admin)/                    # NEW — admin route group
    ├── layout.tsx              # AdminLayout (AdminSidebar + AdminHeader)
    └── admin/
        ├── dashboard/          # /admin/dashboard
        ├── users/              # /admin/users
        │   └── [id]/           # /admin/users/[id]
        ├── workspaces/         # /admin/workspaces
        │   └── [id]/           # /admin/workspaces/[id]
        ├── payments/           # /admin/payments
        ├── subscriptions/      # /admin/subscriptions
        ├── content/            # /admin/content
        ├── analytics/          # /admin/analytics
        ├── audit-logs/         # /admin/audit-logs
        └── settings/           # /admin/settings
            ├── ai-providers/   # AI provider configuration
            ├── email/          # SMTP + email templates
            └── system/         # Rate limits, maintenance mode
```

### Admin Sidebar Navigation

```
Dashboard        — System overview stats
Users            — User list, detail, lock/unlock, delete
Workspaces       — Workspace list, detail, status management
Payments         — Transaction history, subscriptions
Content          — All content across workspaces, moderate/delete
Analytics        — Platform-wide charts (users, revenue, AI usage)
Audit Logs       — Admin action history with diff viewer
Settings         — AI providers, email templates, system config
```

### Middleware Rules

- `role !== Admin` requesting `/admin/*` → redirect `/dashboard`, 403 toast
- `role === Admin` requesting non-`/admin/*` pages → redirect `/admin/dashboard`

## 3. Backend — Admin Controllers

All new controllers use `[Authorize(Roles = "Admin")]` attribute.

### 3.1 AdminDashboardController — `api/admin/dashboard`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/summary` | Total users, workspaces, content, posts, revenue today/week/month |
| GET | `/charts` | Chart data: user registrations by day, revenue by day, AI usage by day |

### 3.2 AdminUsersController — `api/admin/users`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Paginated user list (search by email/name, filter by role/status) |
| GET | `/{id}` | User detail: profile, workspaces, subscription, login history |
| PATCH | `/{id}/status` | Lock/unlock user (Active / Disabled) |
| DELETE | `/{id}` | Permanently delete user + cascade data |

### 3.3 AdminWorkspacesController — `api/admin/workspaces`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Paginated workspace list (search by name, filter by type/status) |
| GET | `/{id}` | Workspace detail: members, subscription, credits, content count |
| PATCH | `/{id}/status` | Change status (Active / Limited / Archived / EligibleForDeletion / Deleted) |
| DELETE | `/{id}` | Delete workspace + cascade |

### 3.4 AdminPaymentsController — `api/admin/payments`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Paginated transaction list (filter by date/status/type) |
| GET | `/{id}` | Transaction detail |
| GET | `/subscriptions` | Paginated subscription list (filter by plan/status) |
| PATCH | `/subscriptions/{id}` | Modify subscription (extend, cancel, change plan) |
| GET | `/revenue/stats` | Revenue stats by week/month/year |

### 3.5 AdminContentController — `api/admin/content`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | All content across system (filter by workspace/status/type) |
| PATCH | `/{id}/status` | Approve / Reject / Take down content |
| DELETE | `/{id}` | Delete violating content |

### 3.6 AdminAuditLogsController — `api/admin/audit-logs`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Paginated audit logs (filter by actor/action/date) |
| GET | `/{id}` | Audit log detail with old_values / new_values diff |

### 3.7 AdminSettingsController — `api/admin/settings`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all system settings |
| PATCH | `/ai` | Configure AI providers (model, API key, credit costs) |
| PATCH | `/email` | Configure SMTP + email templates |
| PATCH | `/system` | Rate limits, maintenance mode, feature toggles |

## 4. Database Changes

### 4.1 New Table: `system_settings`

```sql
CREATE TABLE system_settings (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key             VARCHAR(100) NOT NULL UNIQUE,
    value           JSONB NOT NULL,
    description     VARCHAR(500),
    updated_by      UUID REFERENCES users(id),
    updated_at      TIMESTAMPTZ DEFAULT NOW()
);
```

Sample keys: `ai.default_model`, `email.smtp_host`, `system.maintenance_mode`, `system.rate_limit_rpm`

### 4.2 Existing Tables Reused

| Table | Admin Usage |
|-------|-------------|
| `users`, `sessions` | User management, lock/unlock, login history |
| `workspaces`, `workspace_members` | Workspace management |
| `payments`, `subscriptions`, `credit_usage_records` | Payment & revenue tracking |
| `contents`, `posts`, `brands` | Content moderation |
| `audit_logs` | Already exists — read from it |

### 4.3 Admin Seed

Migration seeds one admin user:
- Email from env `ADMIN_EMAIL` (default: `admin@aisam.com`)
- Password from env `ADMIN_PASSWORD`
- `role = Admin (2)`

### 4.4 Audit Logging

All admin mutations (lock user, delete workspace, modify subscription, change settings) automatically write to `audit_logs` via a service layer call. The existing table schema (`actor_id`, `action_type`, `target_table`, `target_id`, `old_values`, `new_values`) is sufficient.

### 4.5 Migration Count

**1 single migration** creating `system_settings` + seeding admin user.

## 5. Frontend Components

### 5.1 AdminLayout

- `(admin)/layout.tsx` — wraps all admin pages
- Reads `user.role` from JWT/context
- Renders `AdminSidebar` + `AdminHeader` + `<main>{children}</main>`

### 5.2 AdminSidebar

- Logo + "Admin Panel" label
- 8 menu items with Radix UI icons
- Active state with accent background
- Footer: admin avatar + email + logout button
- Color scheme: `bg-gray-950` (distinct from user sidebar)

### 5.3 AdminHeader

- Breadcrumb trail
- Quick search (by user/workspace/payment ID)
- Notification bell (shared)
- No workspace selector

### 5.4 Shared (Reused) Components

From existing `components/ui/`: Modal, Dialog, Toast, Badge, Card, Button, Input, Select, Tabs

From existing `lib/`: `apiClient.ts` (JWT + headers)

### 5.5 New Components

| Component | Purpose |
|-----------|---------|
| `AdminDataTable` | Paginated table with search, column sort |
| `AdminStatsCard` | Stat card with icon, value, percent change |
| `AdminFilterBar` | Filters by date range, status, role, plan... |
| `AuditLogDiff` | Side-by-side diff of old_values vs new_values |
| `StatusBadge` | Colored badge for user/workspace/payment status |
| `SystemConfigForm` | Dynamic form with JSON editor for settings |

## 6. Implementation Phases

### Phase 1 — Foundation (Backend + Core Admin Pages)

1. Migration: create `system_settings` table, seed admin user
2. Backend: 7 admin controllers with `[Authorize(Roles = "Admin")]`
3. Middleware: protect `/admin/*` routes, redirect by role
4. Frontend: AdminLayout + AdminSidebar + AdminHeader
5. Pages: Dashboard, Users (list + detail), Payments + Subscriptions

### Phase 2 — Extended Admin

6. Pages: Workspaces (list + detail + status), Content (moderate/delete), Analytics (charts)
7. Backend: Audit logging in all admin mutation services

### Phase 3 — System Configuration

8. Pages: Audit Logs (list + diff viewer)
9. Pages: Settings — AI providers, Email templates, System config

## 7. Constraints & Notes

- No changes to existing user-facing routes or APIs
- Admin reuses existing auth system (JWT, refresh tokens, middleware pipeline)
- Workspace-scoped middleware must skip for `/admin/*` routes (no `X-Workspace-Id` header required)
- Audit logs: read-only from existing table; write on admin mutations only
- All admin endpoints return 403 if caller role is not Admin
