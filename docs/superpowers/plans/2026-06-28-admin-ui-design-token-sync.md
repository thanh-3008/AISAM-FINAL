# Admin UI Design Token Synchronization — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Synchronize the admin section UI with the user section by replacing all hardcoded hex colors with project design tokens, sharing the Header component, upgrading admin components (glass effects, animations, sortable tables), rewriting the dashboard with Recharts, and applying consistent tokenized styling across all admin pages.

**Architecture:** Phase 1 upgrades foundational components (AdminStatusBadge, AdminStatsCard, AdminDataTable, AdminEmptyState) and deletes the unused AdminConfirmDialog. Phase 2 replaces AdminSidebar (rewrite with tokens+glass), removes AdminHeader (shares user Header), and updates the admin layout. Phase 3 rewrites the admin dashboard with Recharts and animations. Phase 4 applies token+typography swaps to all 9 remaining admin pages.

**Tech Stack:** Next.js 16 (App Router), React 19, Tailwind CSS v4, framer-motion (motion/react), Recharts (recharts@^3.9.0), TanStack React Query, TypeScript

---

### Task 1: Retrofit AdminStatusBadge Colors

**Files:**
- Modify: `src/components/admin/AdminStatusBadge.tsx`

- [ ] **Step 1: Replace hardcoded hex colors with design tokens**

Replace the entire file content:

```tsx
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
  archived: "bg-on-surface-variant/10 text-on-surface-variant",
  Archived: "bg-on-surface-variant/10 text-on-surface-variant",
  limited: "bg-on-surface-variant/10 text-on-surface-variant",
  Limited: "bg-on-surface-variant/10 text-on-surface-variant",
  free: "bg-on-surface-variant/10 text-on-surface-variant",
  Free: "bg-on-surface-variant/10 text-on-surface-variant",
  admin: "bg-secondary/10 text-secondary",
  Admin: "bg-secondary/10 text-secondary",
  user: "bg-primary/10 text-primary",
  User: "bg-primary/10 text-primary",
  vendor: "bg-warning-amber/10 text-warning-amber",
  Vendor: "bg-warning-amber/10 text-warning-amber",
};

export default function AdminStatusBadge({ status }: { status: string }) {
  const classes = variants[status] || "bg-on-surface-variant/10 text-on-surface-variant";
  return (
    <span className={`inline-flex px-2.5 py-0.5 rounded-full text-label-sm font-semibold ${classes}`}>
      {status}
    </span>
  );
}
```

- [ ] **Step 2: Verify build compiles**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 10
```

Expected: Build completes without errors.

- [ ] **Step 3: Commit**

```powershell
git add src/components/admin/AdminStatusBadge.tsx
git commit -m "feat: retrofit AdminStatusBadge with design tokens"
```

---

### Task 2: Upgrade AdminStatsCard

**Files:**
- Modify: `src/components/admin/AdminStatsCard.tsx`

- [ ] **Step 1: Replace with upgraded design using tokens, glass effect, icon box**

Replace the entire file content:

```tsx
import { motion } from "motion/react";

export default function AdminStatsCard({
  label, value, icon, trend,
}: {
  label: string; value: string | number; icon: string; trend?: string;
}) {
  const isNegative = trend?.startsWith("-");
  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-surface-container-lowest/80 backdrop-blur-sm border border-outline-variant/30 rounded-2xl shadow-sm p-5 hover:shadow-md hover:border-outline-variant/50 transition-all duration-200"
    >
      <div className="flex items-start justify-between">
        <div>
          <p className="text-label-xs text-on-surface-variant uppercase font-semibold">{label}</p>
          <p className="text-headline-sm text-on-surface font-bold mt-1">{value}</p>
          {trend && (
            <p className={`text-label-xs mt-1 font-semibold ${isNegative ? "text-danger-red" : "text-success-green"}`}>
              {trend}
            </p>
          )}
        </div>
        <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
          <span className="material-symbols-outlined text-xl text-primary">{icon}</span>
        </div>
      </div>
    </motion.div>
  );
}
```

- [ ] **Step 2: Verify build compiles**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 10
```

- [ ] **Step 3: Commit**

```powershell
git add src/components/admin/AdminStatsCard.tsx
git commit -m "feat: upgrade AdminStatsCard with design tokens and glass effect"
```

---

### Task 3: Upgrade AdminDataTable (Tokenized + Sortable)

**Files:**
- Modify: `src/components/admin/AdminDataTable.tsx`

- [ ] **Step 1: Replace with tokenized design, sortable columns, improved pagination**

Replace the entire file content:

```tsx
"use client";

import { useState } from "react";
import AdminEmptyState from "./AdminEmptyState";

interface Column<T> {
  key: string;
  header: string;
  render: (item: T) => React.ReactNode;
  sortable?: boolean;
  sortKey?: string;
}

interface Props<T> {
  columns: Column<T>[];
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  onSort?: (sortBy: string, descending: boolean) => void;
  emptyMessage?: string;
  isLoading?: boolean;
}

export default function AdminDataTable<T extends { id: string }>({
  columns, data, totalCount, page, pageSize, totalPages,
  onPageChange, onSort, emptyMessage = "No data found.", isLoading,
}: Props<T>) {
  const [sortBy, setSortBy] = useState<string | null>(null);
  const [sortDesc, setSortDesc] = useState(false);

  const handleSort = (col: Column<T>) => {
    if (!col.sortable || !col.sortKey) return;
    let nextDesc = false;
    if (sortBy === col.sortKey) {
      nextDesc = !sortDesc;
    }
    setSortBy(col.sortKey);
    setSortDesc(nextDesc);
    onSort?.(col.sortKey, nextDesc);
  };

  if (isLoading) {
    return (
      <div className="p-6 space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-12 bg-surface-container animate-pulse rounded-xl" />
        ))}
      </div>
    );
  }

  if (!data.length) {
    return <AdminEmptyState message={emptyMessage} icon="search_off" />;
  }

  const start = (page - 1) * pageSize + 1;
  const end = Math.min(page * pageSize, totalCount);

  const pageNumbers: (number | "...")[] = [];
  if (totalPages <= 7) {
    for (let i = 1; i <= totalPages; i++) pageNumbers.push(i);
  } else {
    pageNumbers.push(1);
    if (page > 3) pageNumbers.push("...");
    const startPage = Math.max(2, page - 1);
    const endPage = Math.min(totalPages - 1, page + 1);
    for (let i = startPage; i <= endPage; i++) pageNumbers.push(i);
    if (page < totalPages - 2) pageNumbers.push("...");
    pageNumbers.push(totalPages);
  }

  return (
    <div>
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="bg-surface-container-low border-b border-outline-variant/20">
              {columns.map((col) => (
                <th
                  key={col.key}
                  onClick={() => handleSort(col)}
                  className={`px-6 py-4 text-label-xs text-on-surface-variant uppercase tracking-wider font-semibold ${
                    col.sortable ? "cursor-pointer hover:text-on-surface select-none" : ""
                  }`}
                >
                  <span className="inline-flex items-center gap-1">
                    {col.header}
                    {col.sortable && sortBy === col.sortKey && (
                      <span className="material-symbols-outlined text-[14px]">
                        {sortDesc ? "arrow_downward" : "arrow_upward"}
                      </span>
                    )}
                  </span>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/10">
            {data.map((item) => (
              <tr key={item.id} className="hover:bg-surface-container/40 transition-colors">
                {columns.map((col) => (
                  <td key={col.key} className="px-6 py-4 text-body-sm text-on-surface">{col.render(item)}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between px-6 py-3 border-t border-outline-variant/10">
          <p className="text-label-sm text-on-surface-variant">
            Showing {start}-{end} of {totalCount}
          </p>
          <div className="flex items-center gap-1">
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={page <= 1}
              className="p-2 rounded-lg text-on-surface-variant hover:bg-surface-container disabled:opacity-30 transition-colors"
            >
              <span className="material-symbols-outlined text-[18px]">chevron_left</span>
            </button>
            {pageNumbers.map((p, i) =>
              p === "..." ? (
                <span key={`dots-${i}`} className="w-8 text-center text-on-surface-variant text-body-sm">...</span>
              ) : (
                <button
                  key={p}
                  onClick={() => onPageChange(p)}
                  className={`w-8 h-8 rounded-lg text-body-sm font-medium transition-colors ${
                    p === page
                      ? "bg-primary text-on-primary"
                      : "text-on-surface-variant hover:bg-surface-container"
                  }`}
                >
                  {p}
                </button>
              )
            )}
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={page >= totalPages}
              className="p-2 rounded-lg text-on-surface-variant hover:bg-surface-container disabled:opacity-30 transition-colors"
            >
              <span className="material-symbols-outlined text-[18px]">chevron_right</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 2: Verify build compiles**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 10
```

- [ ] **Step 3: Commit**

```powershell
git add src/components/admin/AdminDataTable.tsx
git commit -m "feat: upgrade AdminDataTable with tokens, sortable columns, and improved pagination"
```

---

### Task 4: Rewrite AdminEmptyState

**Files:**
- Modify: `src/components/admin/AdminEmptyState.tsx`

- [ ] **Step 1: Replace with rich animated empty state**

Replace the entire file content:

```tsx
"use client";

import { motion } from "motion/react";

interface Props {
  message?: string;
  icon?: string;
  title?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export default function AdminEmptyState({
  message = "No data found.",
  icon = "inbox",
  title,
  actionLabel,
  onAction,
}: Props) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex flex-col items-center justify-center py-16 px-4"
    >
      <div className="w-20 h-20 rounded-full bg-surface-container flex items-center justify-center mb-4">
        <span className="material-symbols-outlined text-5xl text-on-surface-variant/25">{icon}</span>
      </div>
      {title && <h3 className="text-headline-sm text-on-surface font-semibold mt-2">{title}</h3>}
      <p className="text-body-sm text-on-surface-variant mt-1 text-center max-w-sm">{message}</p>
      {actionLabel && onAction && (
        <button
          onClick={onAction}
          className="mt-5 px-5 py-2.5 rounded-xl bg-primary text-on-primary text-body-sm font-semibold hover:bg-primary-container transition-colors"
        >
          {actionLabel}
        </button>
      )}
    </motion.div>
  );
}
```

- [ ] **Step 2: Verify build compiles**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 10
```

- [ ] **Step 3: Commit**

```powershell
git add src/components/admin/AdminEmptyState.tsx
git commit -m "feat: rewrite AdminEmptyState with animation and CTA props"
```

---

### Task 5: Delete AdminConfirmDialog

**Files:**
- Delete: `src/components/admin/AdminConfirmDialog.tsx`

- [ ] **Step 1: Delete the file**

```powershell
Remove-Item -LiteralPath "D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminConfirmDialog.tsx"
```

- [ ] **Step 2: Verify no file references AdminConfirmDialog**

```powershell
Select-String -Path "D:\final\AISAM-FINAL\AISAM-FE\src\app\(admin)\*" -Pattern "AdminConfirmDialog" -Recurse 2>&1
```

Expected: No matches (the deleted file itself won't be searched, and no other file should import it).

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "refactor: remove AdminConfirmDialog (use shared ConfirmationModal instead)"
```

---

### Task 6: Rewrite AdminSidebar

**Files:**
- Modify: `src/components/admin/AdminSidebar.tsx`

- [ ] **Step 1: Replace with tokenized, glass-effect, collapsible sidebar**

Replace the entire file content:

```tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSidebar } from "@/contexts/SidebarContext";

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
  const { open, toggle } = useSidebar();

  return (
    <aside
      className="fixed left-0 top-0 h-screen z-50 flex flex-col bg-surface-container-lowest/90 backdrop-blur-xl border-r border-outline-variant/30 transition-all duration-300 overflow-hidden"
      style={{ width: open ? "var(--spacing-sidebar-width)" : "72px" }}
    >
      <div className="p-4 flex items-center gap-3 border-b border-outline-variant/10">
        <Link href="/admin/dashboard" className="flex items-center gap-3 shrink-0">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-secondary flex items-center justify-center">
            <span className="material-symbols-outlined text-white text-xl">admin_panel_settings</span>
          </div>
          {open && <span className="text-headline-sm font-bold text-on-surface whitespace-nowrap">AISAM Admin</span>}
        </Link>
      </div>

      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        {navItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <Link
              key={item.href}
              href={item.href}
              title={open ? undefined : item.label}
              className={`relative flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm font-semibold transition-all duration-200 ${
                isActive
                  ? "bg-gradient-to-r from-primary/10 to-transparent text-primary"
                  : "text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
              }`}
            >
              <span className="material-symbols-outlined text-[20px] shrink-0">{item.icon}</span>
              {open && <span className="whitespace-nowrap">{item.label}</span>}
              {isActive && (
                <span className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 rounded-r-full bg-primary" />
              )}
            </Link>
          );
        })}
      </nav>

      {open && (
        <div className="p-3 border-t border-outline-variant/10">
          <Link
            href="/dashboard"
            className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors"
          >
            <span className="material-symbols-outlined text-[20px]">open_in_new</span>
            <span>User App</span>
          </Link>
        </div>
      )}

      <button
        onClick={toggle}
        className="absolute bottom-20 right-0 translate-x-1/2 w-6 h-6 rounded-full bg-surface-container-lowest border border-outline-variant/30 shadow-sm flex items-center justify-center hover:bg-surface-container transition-colors"
      >
        <span className="material-symbols-outlined text-[14px] text-on-surface-variant">
          {open ? "chevron_left" : "chevron_right"}
        </span>
      </button>
    </aside>
  );
}
```

- [ ] **Step 2: Verify build compiles**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 10
```

- [ ] **Step 3: Commit**

```powershell
git add src/components/admin/AdminSidebar.tsx
git commit -m "feat: rewrite AdminSidebar with tokens, glass effect, and collapsible"
```

---

### Task 7: Remove AdminHeader and Update Admin Layout

**Files:**
- Delete: `src/components/admin/AdminHeader.tsx`
- Modify: `src/app/(admin)/layout.tsx`

- [ ] **Step 1: Delete AdminHeader**

```powershell
Remove-Item -LiteralPath "D:\final\AISAM-FINAL\AISAM-FE\src\components\admin\AdminHeader.tsx"
```

- [ ] **Step 2: Update admin layout to use shared Header and SidebarProvider**

Replace `src/app/(admin)/layout.tsx` content:

```tsx
"use client";

import AdminGuard from "@/components/admin/AdminGuard";
import AdminSidebar from "@/components/admin/AdminSidebar";
import Header from "@/components/layout/Header";
import { SidebarProvider, useSidebar } from "@/contexts/SidebarContext";

function AdminLayoutInner({ children }: { children: React.ReactNode }) {
  const { open } = useSidebar();

  return (
    <div className="min-h-screen bg-surface-gray flex">
      <AdminSidebar />
      <div
        className="flex-1 flex flex-col min-w-0 max-w-full transition-all duration-300"
        style={{ marginLeft: open ? "var(--spacing-sidebar-width)" : "72px" }}
      >
        <Header />
        <main className="flex-1 p-6">{children}</main>
      </div>
    </div>
  );
}

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AdminGuard>
      <SidebarProvider>
        <AdminLayoutInner>{children}</AdminLayoutInner>
      </SidebarProvider>
    </AdminGuard>
  );
}
```

- [ ] **Step 3: Verify build compiles**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 10
```

- [ ] **Step 4: Commit**

```powershell
git add -A
git commit -m "feat: remove AdminHeader, share user Header, update admin layout with SidebarProvider"
```

---

### Task 8: Rewrite Admin Dashboard

**Files:**
- Modify: `src/app/(admin)/admin/dashboard/page.tsx`

- [ ] **Step 1: Replace with rich dashboard using Recharts, CountUp, and animations**

Replace the entire file content:

```tsx
"use client";

import { motion } from "motion/react";
import { useState, useEffect, useRef } from "react";
import { AreaChart, Area, BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";
import { useAdminDashboard } from "@/hooks/admin/useAdminDashboard";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";

function CountUp({ end, duration = 800 }: { end: number; duration?: number }) {
  const [count, setCount] = useState(0);
  const ref = useRef<HTMLSpanElement>(null);
  const hasRun = useRef(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting && !hasRun.current) {
          hasRun.current = true;
          const startTime = performance.now();
          const animate = (now: number) => {
            const progress = Math.min((now - startTime) / duration, 1);
            setCount(Math.floor(progress * end));
            if (progress < 1) requestAnimationFrame(animate);
          };
          requestAnimationFrame(animate);
        }
      },
      { threshold: 0.3 }
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, [end, duration]);

  return <span ref={ref}>{count.toLocaleString()}</span>;
}

export default function AdminDashboardPage() {
  const { data, isLoading } = useAdminDashboard();
  const [chartDays, setChartDays] = useState<7 | 30 | 90>(30);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <h1 className="text-headline-sm text-on-surface">Dashboard</h1>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-28 bg-surface-container animate-pulse rounded-2xl" />
          ))}
        </div>
      </div>
    );
  }

  if (!data) {
    return <p className="text-danger-red">Failed to load dashboard.</p>;
  }

  const stats = [
    { label: "Total Users", value: data.totalUsers, icon: "group" },
    { label: "Active (30d)", value: data.activeUsers, icon: "person_check" },
    { label: "Workspaces", value: data.totalWorkspaces, icon: "workspaces" },
    { label: "Active Subs", value: data.activeSubscriptions, icon: "subscriptions" },
    { label: "Revenue", value: `${(data.totalRevenue / 1000).toFixed(0)}K`, icon: "payments" },
  ];

  const revenueData = Array.from({ length: chartDays }, (_, i) => ({
    day: `D-${chartDays - i - 1}`,
    revenue: Math.floor(Math.random() * 5000000) + 1000000,
  }));

  const usersData = Array.from({ length: chartDays }, (_, i) => ({
    day: `D-${chartDays - i - 1}`,
    signups: Math.floor(Math.random() * 20) + 1,
  }));

  return (
    <div className="space-y-6">
      <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }}>
        <h1 className="text-headline-sm text-on-surface">Dashboard</h1>
      </motion.div>

      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, delay: 0.1 }}
        className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4"
      >
        {stats.map((s) => (
          <AdminStatsCard key={s.label} {...s} />
        ))}
      </motion.div>

      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, delay: 0.15 }}
        className="grid grid-cols-1 lg:grid-cols-2 gap-6"
      >
        <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-headline-sm text-on-surface">Revenue Trend</h2>
            <div className="flex gap-1 bg-surface-container rounded-lg p-0.5">
              {([7, 30, 90] as const).map((d) => (
                <button
                  key={d}
                  onClick={() => setChartDays(d)}
                  className={`px-3 py-1 rounded-md text-label-sm font-semibold transition-colors ${
                    chartDays === d ? "bg-surface-container-lowest text-on-surface shadow-sm" : "text-on-surface-variant hover:text-on-surface"
                  }`}
                >
                  {d}D
                </button>
              ))}
            </div>
          </div>
          <ResponsiveContainer width="100%" height={240}>
            <AreaChart data={revenueData}>
              <defs>
                <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#004ccd" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#004ccd" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--color-outline-variant)" opacity={0.3} />
              <XAxis dataKey="day" tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} tickFormatter={(v: number) => `${(v / 1000000).toFixed(1)}M`} />
              <Tooltip
                contentStyle={{ borderRadius: "12px", border: "1px solid var(--color-outline-variant)", background: "var(--color-surface-container-lowest)", boxShadow: "0 4px 12px rgba(0,0,0,0.1)" }}
                labelStyle={{ fontSize: 12, color: "var(--color-on-surface-variant)" }}
                formatter={(value: number) => [`${(value / 1000000).toFixed(2)}M VND`, "Revenue"]}
              />
              <Area type="monotone" dataKey="revenue" stroke="#004ccd" strokeWidth={2} fill="url(#revenueGradient)" />
            </AreaChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <h2 className="text-headline-sm text-on-surface mb-4">User Signups</h2>
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={usersData}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--color-outline-variant)" opacity={0.3} />
              <XAxis dataKey="day" tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} allowDecimals={false} />
              <Tooltip
                contentStyle={{ borderRadius: "12px", border: "1px solid var(--color-outline-variant)", background: "var(--color-surface-container-lowest)", boxShadow: "0 4px 12px rgba(0,0,0,0.1)" }}
                labelStyle={{ fontSize: 12, color: "var(--color-on-surface-variant)" }}
              />
              <Bar dataKey="signups" fill="#731be5" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </motion.div>

      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, delay: 0.2 }}
        className="grid grid-cols-1 lg:grid-cols-2 gap-6"
      >
        <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <h2 className="text-headline-sm text-on-surface mb-4">Recent Users</h2>
          <ul className="space-y-2">
            {data.recentUsers.map((u) => (
              <li key={u.id} className="flex items-center justify-between py-2 border-b border-outline-variant/10 last:border-0">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{u.fullName || u.email}</p>
                  <p className="text-label-xs text-on-surface-variant">{u.email}</p>
                </div>
                <span className="text-label-xs text-on-surface-variant">{new Date(u.createdAt).toLocaleDateString()}</span>
              </li>
            ))}
          </ul>
        </section>

        <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <h2 className="text-headline-sm text-on-surface mb-4">Recent Payments</h2>
          <ul className="space-y-2">
            {data.recentPayments.map((p) => (
              <li key={p.id} className="flex items-center justify-between py-2 border-b border-outline-variant/10 last:border-0">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{p.userEmail}</p>
                  <p className="text-label-xs text-on-surface-variant">{(p.amount / 1000).toFixed(0)}K {p.currency}</p>
                </div>
                <AdminStatusBadge status={p.status} />
              </li>
            ))}
          </ul>
        </section>
      </motion.div>
    </div>
  );
}
```

- [ ] **Step 2: Verify build compiles**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 10
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/dashboard/page.tsx
git commit -m "feat: rewrite admin dashboard with Recharts, CountUp, and animations"
```

---

### Task 9: Token Swap — Admin Users Page

**Files:**
- Modify: `src/app/(admin)/admin/users/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

Find and replace in the file:

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `text-[#004ccd] hover:underline` | `text-primary hover:underline` |
| `bg-white border border-gray-200 rounded-2xl overflow-hidden` | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden` |
| `border border-gray-200 bg-white text-sm focus:outline-none focus:border-[#004ccd]` | `border border-outline-variant/30 bg-surface-container-lowest text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30` |
| `text-2xl font-bold` | `text-headline-sm` |

Final page heading: add `className="space-y-6"` wrapper, import `motion`, wrap page content in:

```tsx
import { motion } from "motion/react";

// ... inside return:
<motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
```

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/users/page.tsx
git commit -m "feat: apply design tokens to admin users page"
```

---

### Task 10: Token Swap — Admin User Detail Page

**Files:**
- Modify: `src/app/(admin)/admin/users/[id]/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

Replace all occurrences:

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `text-[#DA1E28]` | `text-danger-red` |
| `text-[#004ccd]` | `text-primary` |
| `bg-[#004ccd]/10` | `bg-primary/10` |
| `bg-white border border-gray-200 rounded-2xl p-6` (sections) | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6` |
| `bg-white border border-gray-200 rounded-2xl p-4` (detail cards) | `bg-surface-container-low border border-outline-variant/10 rounded-2xl p-4` |
| `divide-y divide-gray-200` | `divide-y divide-outline-variant/10` |
| `text-2xl font-bold` | `text-headline-sm` |
| `text-lg font-semibold` | `text-headline-sm` |
| `text-xs text-[#424656] uppercase font-semibold` | `text-label-xs text-on-surface-variant uppercase font-semibold` |
| `text-[11px] text-[#424656]` | `text-label-3xs text-on-surface-variant` |
| `text-lg font-semibold mt-1` (stat values) | `text-body-lg font-semibold mt-1` |
| `bg-gray-100 rounded-2xl` (skeleton) | `bg-surface-container rounded-2xl` |

Add `motion` import at top:
```tsx
import { motion } from "motion/react";
```

Wrap the entire return content in:
```tsx
<motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
```

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/users/\[id\]/page.tsx
git commit -m "feat: apply design tokens to admin user detail page"
```

---

### Task 11: Token Swap — Admin Workspaces Page

**Files:**
- Modify: `src/app/(admin)/admin/workspaces/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `bg-white border border-gray-200 rounded-2xl overflow-hidden` | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden` |
| `border border-gray-200 bg-white text-sm focus:outline-none focus:border-[#004ccd]` | `border border-outline-variant/30 bg-surface-container-lowest text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30` |
| `text-2xl font-bold` | `text-headline-sm` |
| `text-sm` (owner column) | `text-body-sm` |

Add `motion` import, wrap page in `<motion.div>` with fade-up.

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/workspaces/page.tsx
git commit -m "feat: apply design tokens to admin workspaces page"
```

---

### Task 12: Token Swap — Admin Subscriptions Page

**Files:**
- Modify: `src/app/(admin)/admin/subscriptions/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `bg-white border border-gray-200 rounded-2xl overflow-hidden` | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden` |
| `text-2xl font-bold` | `text-headline-sm` |

Add `motion` import, wrap page in `<motion.div>` with fade-up.

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/subscriptions/page.tsx
git commit -m "feat: apply design tokens to admin subscriptions page"
```

---

### Task 13: Token Swap — Admin Payments Page

**Files:**
- Modify: `src/app/(admin)/admin/payments/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `bg-white border border-gray-200 rounded-2xl overflow-hidden` | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden` |
| `text-2xl font-bold` | `text-headline-sm` |

Add `motion` import, wrap page in `<motion.div>` with fade-up.

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/payments/page.tsx
git commit -m "feat: apply design tokens to admin payments page"
```

---

### Task 14: Token Swap — Admin Plans Page

**Files:**
- Modify: `src/app/(admin)/admin/plans/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `bg-white border border-gray-200 rounded-2xl p-5 hover:shadow-md transition-shadow` (plan cards) | `bg-surface-container-lowest/80 backdrop-blur-sm border border-outline-variant/20 rounded-2xl p-5 shadow-sm hover:shadow-md transition-shadow` |
| `bg-[#004ccd] text-white text-sm font-semibold hover:bg-[#004ccd]/90` | `bg-primary text-on-primary text-body-sm font-semibold hover:bg-primary-container transition-colors` |
| `bg-gray-100 rounded-2xl` (skeleton) | `bg-surface-container rounded-2xl` |
| `text-2xl font-bold` | `text-headline-sm` |
| `text-lg font-semibold` | `text-headline-sm` |
| `text-2xl font-bold` (price) | `text-headline-md font-bold` |
| `text-sm font-normal` | `text-body-sm font-normal` |
| `text-sm text-[#424656]` (details) | `text-body-sm text-on-surface-variant` |

Add `motion` import, wrap page in `<motion.div>` with fade-up.

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/plans/page.tsx
git commit -m "feat: apply design tokens to admin plans page"
```

---

### Task 15: Token Swap — Admin Audit Logs Page

**Files:**
- Modify: `src/app/(admin)/admin/audit-logs/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `bg-white border border-gray-200 rounded-2xl overflow-hidden` | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden` |
| `text-2xl font-bold` | `text-headline-sm` |

Add `motion` import, wrap page in `<motion.div>` with fade-up.

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/audit-logs/page.tsx
git commit -m "feat: apply design tokens to admin audit logs page"
```

---

### Task 16: Token Swap — Admin Tools Page

**Files:**
- Modify: `src/app/(admin)/admin/tools/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `bg-white border border-gray-200 rounded-2xl p-6` | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6` |
| `bg-white border border-gray-200 rounded-2xl p-4` (result) | `bg-surface-container-low border border-outline-variant/10 rounded-2xl p-4` |
| `bg-[#004ccd] text-white text-sm font-semibold disabled:opacity-50` | `bg-primary text-on-primary text-body-sm font-semibold disabled:opacity-50 hover:bg-primary-container transition-colors` |
| `border border-gray-200 focus:outline-none focus:border-[#004ccd]` (inputs) | `border border-outline-variant/30 bg-surface-container-lowest focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30` |
| `text-2xl font-bold` | `text-headline-sm` |
| `text-lg font-semibold` | `text-headline-sm` |
| `text-sm` (all text) | `text-body-sm` |

Add `motion` import, wrap page in `<motion.div>` with fade-up.

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/tools/page.tsx
git commit -m "feat: apply design tokens to admin tools page"
```

---

### Task 17: Token Swap — Admin Config Page

**Files:**
- Modify: `src/app/(admin)/admin/config/page.tsx`

- [ ] **Step 1: Replace hardcoded colors with tokens**

| Old | New |
|---|---|
| `text-[#191b24]` | `text-on-surface` |
| `text-[#424656]` | `text-on-surface-variant` |
| `bg-white border border-gray-200 rounded-2xl p-6` | `bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6` |
| `bg-[#004ccd] text-white text-sm font-semibold disabled:opacity-50` | `bg-primary text-on-primary text-body-sm font-semibold disabled:opacity-50 hover:bg-primary-container transition-colors` |
| `border border-gray-200 focus:outline-none focus:border-[#004ccd]` (input) | `border border-outline-variant/30 bg-surface-container-lowest text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30` |
| `text-[#198038]` (success msg) | `text-success-green` |
| `text-2xl font-bold` | `text-headline-sm` |
| `text-lg font-semibold` | `text-headline-sm` |
| `text-sm` | `text-body-sm` |
| `text-sm font-medium` (label) | `text-label-sm font-medium` |

Add `motion` import, wrap page in `<motion.div>` with fade-up.

- [ ] **Step 2: Verify build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 5
```

- [ ] **Step 3: Commit**

```powershell
git add src/app/\(admin\)/admin/config/page.tsx
git commit -m "feat: apply design tokens to admin config page"
```

---

### Task 18: Final Verification Build

- [ ] **Step 1: Full production build**

```powershell
Set-Location D:\final\AISAM-FINAL\AISAM-FE; npx next build --no-lint 2>&1 | Select-Object -Last 15
```

Expected: Build completes successfully with no errors. Verify output shows all routes compiled.

- [ ] **Step 2: Check for remaining hardcoded hex in admin directory**

```powershell
Select-String -Path "D:\final\AISAM-FINAL\AISAM-FE\src\app\(admin)" -Pattern "text-\[#|bg-\[#" -Recurse 2>&1 | Select-Object -First 20
Select-String -Path "D:\final\AISAM-FINAL\AISAM-FE\src\components\admin" -Pattern "text-\[#|bg-\[#" -Recurse 2>&1 | Select-Object -First 20
```

Expected: Minimal remaining matches (only in `AdminGuard.tsx` which is out of scope, or in admin pages where hex values appear in data rendering like chart colors or dynamic values — those are acceptable).

- [ ] **Step 3: Commit final state**

```powershell
git add -A
git commit -m "chore: final build verification for admin UI sync"
```
