# Admin UI Design Token Synchronization

## Overview

Dong bo hoan toan UI cua admin section voi user section — chuyen tu hardcoded hex colors sang design tokens, dung chung Header, nang cap tat ca component admin len chat luong tuong duong user. Admin section se co dark mode, glass effect, animation framer-motion, notification, theme toggle.

**Date:** 2026-06-28
**Status:** Design Approved

---

## Current State vs Target State

| Aspect | Current (Admin) | Target (After Sync) |
|---|---|---|
| Colors | Hardcoded hex (`#191b24`, `#004ccd`, `#731be5`, `white`, `gray-200`) | Design tokens (`text-on-surface`, `text-primary`, `bg-surface-container-lowest`, `border-outline-variant/20`) |
| Dark mode | Broken everywhere | Full dark mode via token inheritance |
| Glass effects | None | `backdrop-blur-xl`, `glass-panel` utility |
| Shadows | Minimal (`hover:shadow-md` only) | Consistent (`shadow-sm` cards, `shadow-2xl` modals) |
| Typography | Raw classes (`text-xs`, `text-2xl`) | Semantic classes (`text-label-xs`, `text-headline-sm`) |
| Animation | None | `motion.div` framer-motion fade-up entry |
| Header | 67-line bare component | Shared `Header.tsx` (359 lines, notification + theme toggle) |
| Sidebar | 64-line simple nav | New rich sidebar with glass effect, collapsible, animation |
| Confirm dialog | Custom `AdminConfirmDialog` | Shared `ConfirmationModal` |
| Empty state | 8-line icon+text | Rich multi-element with animation + CTA |
| Dashboard | 79 lines, 5 stat + 2 lists | ~400 lines, Recharts, CountUp, gradient, animation |

---

## Design Token Mapping

### Color Tokens

| Hardcoded (Old) | Token (New) | Usage |
|---|---|---|
| `#faf8ff` | `bg-background` / `bg-surface-gray` | Layout background |
| `#191b24` | `text-on-surface` | Headings, primary text |
| `#424656` | `text-on-surface-variant` | Secondary text, labels |
| `#004ccd` | `text-primary`, `bg-primary`, `border-primary` | Accent, buttons, links |
| `#731be5` | `text-secondary`, `bg-secondary` | Purple accents |
| `bg-white` | `bg-surface-container-lowest` | Card backgrounds |
| `border-gray-200` | `border-outline-variant/20` | Card borders |
| `border-gray-100` | `border-outline-variant/10` | Row dividers |
| `bg-gray-100` | `bg-surface-container` | Skeleton, hover states |
| `#DA1E28` | `text-danger-red`, `bg-danger-red` | Danger actions |
| `#198038` | `text-success-green` | Success status |
| `#F1C21B` | `text-warning-amber` | Warning status |
| `focus:border-[#004ccd]` | `focus:border-primary focus:ring-1 focus:ring-primary/30` | Input focus |

### Typography Tokens

| Old | New |
|---|---|
| `text-2xl font-bold` | `text-headline-sm` |
| `text-xl font-semibold` | `text-headline-sm` |
| `text-sm` | `text-body-sm` |
| `text-xs uppercase tracking-wider font-semibold` | `text-label-xs uppercase tracking-wider font-semibold` |
| `text-[11px] font-semibold` | `text-label-sm font-semibold` |

---

## Phase 1: Foundation Components

### 1.1 `AdminStatusBadge` — Retrofit colors only

**File:** `src/components/admin/AdminStatusBadge.tsx`

**Changes:**
- Replace all hardcoded hex in variant map with design tokens
- Update typography: `text-[11px]` → `text-label-sm`
- Keep all 26 variant names and logic unchanged

**Variant color mapping:**
```
Active/Inactive/Success → bg-success-green/10 text-success-green
Suspended/Pending → bg-warning-amber/10 text-warning-amber
Cancelled/Failed → bg-danger-red/10 text-danger-red
Archived/Limited/Free → bg-on-surface-variant/10 text-on-surface-variant
Admin → bg-secondary/10 text-secondary
User → bg-primary/10 text-primary
Vendor → bg-warning-amber/10 text-warning-amber
```

### 1.2 `AdminStatsCard` — Full upgrade

**File:** `src/components/admin/AdminStatsCard.tsx`

**Current:** Plain white card, standalone icon, no shadow

**New design:**
```
Card wrapper: bg-surface-container-lowest/80 backdrop-blur-sm border border-outline-variant/30
               rounded-2xl shadow-sm p-6
               hover:shadow-md hover:border-outline-variant/50 transition-all duration-200

Icon: wrapped in colored box (w-12 h-12 rounded-xl)
      bg-primary/10 text-primary (or appropriate color per card type)

Label: text-label-xs text-on-surface-variant uppercase font-semibold

Value: text-headline-md text-on-surface font-bold

Trend: arrow icon + percentage, inline next to value
       positive: text-success-green, negative: text-danger-red
```

**Props unchanged:** `label`, `value`, `icon`, `trend`

### 1.3 `AdminDataTable` — Full upgrade

**File:** `src/components/admin/AdminDataTable.tsx`

**Current:** No wrapper, no shadow, ugly skeleton, no sorting

**New design:**
```
Wrapper: bg-surface-container-lowest/80 backdrop-blur-sm border border-outline-variant/30
         rounded-2xl shadow-sm overflow-hidden

Header row: bg-surface-container-low text-label-xs text-on-surface-variant
           uppercase tracking-wider font-semibold px-6 py-4
           cursor-pointer (sortable columns) with sort arrow icons

Body rows: divide-y divide-outline-variant/10
          hover:bg-surface-container/40 transition-colors
          text-body-sm text-on-surface (primary cell)
          text-body-sm text-on-surface-variant (secondary cell)

Loading skeleton: bg-surface-container animate-pulse rounded-xl (5 rows matching columns)

Empty state: renders <AdminEmptyState /> inside the table

Pagination bar: flex justify-between items-center px-6 py-3 border-t border-outline-variant/10
               "Showing X-Y of Z" label: text-label-sm text-on-surface-variant
               Page buttons: numbered with ellipsis
               Active page: bg-primary text-on-primary rounded-lg
               Inactive page: hover:bg-surface-container rounded-lg text-on-surface-variant
               Previous/Next: disabled:opacity-40, with chevron icons
```

**New feature: Sortable columns**
- Add sort state to the generic `<T>` component
- Column definition adds `sortable?: boolean` and `sortKey?: string`
- Click header toggles ascending/descending/neutral
- Arrow icon shows current sort direction

**Props unchanged** except column definition extended with sort options.

### 1.4 `AdminEmptyState` — Full rewrite

**File:** `src/components/admin/AdminEmptyState.tsx`

**Current:** 8 lines, single icon + text

**New design (~50 lines):**
```
Wrapper: motion.div with fade-up animation

Icon container: w-20 h-20 rounded-full bg-surface-container flex items-center justify-center
               Icon: text-5xl text-on-surface-variant/30 (material symbol)

Title: text-headline-sm text-on-surface font-semibold mt-4

Message: text-body-sm text-on-surface-variant mt-1 text-center max-w-sm

CTA button (optional): mt-4 bg-primary text-on-primary px-5 py-2.5 rounded-xl
                       hover:bg-primary-container transition-colors font-semibold text-body-sm
                       onClick from props
```

**Props:** `icon` (string), `title` (string), `message` (string), `actionLabel?` (string), `onAction?` (() => void)

**Modes:**
- "No data" → icon=`search`, title="No data found", message="There are no items to display yet."
- "No results" → icon=`filter_alt_off`, title="No matching results", message="Try adjusting your search or filters."

### 1.5 Remove `AdminConfirmDialog`, adopt shared `ConfirmationModal`

**Files:**
- **Delete:** `src/components/admin/AdminConfirmDialog.tsx`
- **Already exists:** `src/components/ui/ConfirmationModal.tsx` (fully tokenized, animated, supports danger/warning/info)

**Import change in all admin pages that used `AdminConfirmDialog`:**
```diff
- import { AdminConfirmDialog } from "@/components/admin/AdminConfirmDialog";
+ import { ConfirmationModal } from "@/components/ui/ConfirmationModal";
```

**Usage example:**
```tsx
<ConfirmationModal
  isOpen={showConfirm}
  onClose={() => setShowConfirm(false)}
  onConfirm={handleDeactivate}
  type="danger"
  title="Deactivate User?"
  message="This will revoke all active sessions and prevent login."
  isLoading={isMutating}
/>
```

---

## Phase 2: Layout

### 2.1 Remove `AdminHeader`, share user `Header`

**Files:**
- **Delete:** `src/components/admin/AdminHeader.tsx`
- **Already exists:** `src/components/layout/Header.tsx` (359 lines, notification polling, theme toggle, search, user menu)

**Why this works:** The user Header already has:
- Admin panel link (gated by `isAdmin()` check) — admin users will see "User App" link instead
- Notification bell with unread badge, polling every 30s, mark-all-read
- Dark/light theme toggle
- User avatar dropdown with logout
- Search bar

**Change in `(admin)/layout.tsx`:**
```diff
- import { AdminHeader } from "@/components/admin/AdminHeader";
+ import { Header } from "@/components/layout/Header";

- <AdminHeader />
+ <Header />
```

### 2.2 Rewrite `AdminSidebar`

**File:** `src/components/admin/AdminSidebar.tsx` (rewrite, ~150 lines)

**Current:** 64 lines, `bg-white`, hardcoded colors, no collapse, no animation

**New design:**
```
Container: fixed left-0 top-0 h-screen z-50
          bg-surface-container-lowest/90 backdrop-blur-xl
          border-r border-outline-variant/30
          transition-all duration-300
          Width: collapsed(72px) / expanded(260px)

Logo area: p-4 flex items-center gap-3
          Logo icon: w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-secondary
                     flex items-center justify-center text-white font-bold text-lg
          Brand text: text-headline-sm font-bold text-on-surface 
                     hidden when collapsed

Nav sections: p-3 space-y-1
Section label: text-label-xs text-on-surface-variant uppercase tracking-widest px-3 py-2

Nav items (9): Dashboard, Users, Workspaces, Subscriptions, Payments, Plans, Audit Logs, Tools, Config

Item active: bg-gradient-to-r from-primary/10 to-transparent
            text-primary font-semibold
            Left bar indicator: absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 rounded-r-full bg-primary scale-100 transition-transform

Item inactive: text-on-surface-variant hover:bg-surface-container hover:text-on-surface
              rounded-xl px-3 py-2.5 transition-all

Item icon + label: flex items-center gap-3 text-body-sm
                  Label hidden when collapsed (icon-only mode)

Collapse toggle: absolute bottom-20 right-0 translate-x-1/2
                w-6 h-6 rounded-full bg-surface-container-lowest border border-outline-variant/30
                shadow-sm flex items-center justify-center cursor-pointer
                hover:bg-surface-container transition-colors

Bottom section: border-t border-outline-variant/10 p-3
               "Back to App" link: text-body-sm text-on-surface-variant
               hover:text-on-surface transition-colors flex items-center gap-2
```

**Props:** `collapsed` (bool), `onToggle` (() => void) — managed by SidebarProvider

### 2.3 Update Admin Layout

**File:** `src/app/(admin)/layout.tsx`

**Changes:**
```tsx
"use client";
import { AdminGuard } from "@/components/admin/AdminGuard";
import { AdminSidebar } from "@/components/admin/AdminSidebar";
import { Header } from "@/components/layout/Header";
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

---

## Phase 3: Admin Dashboard

**File:** `src/app/(admin)/admin/dashboard/page.tsx` (rewrite, target ~400 lines)

### Sections

#### 3.1 Hero Stats Row
4-5 KPI cards in a responsive grid:
- **Total Revenue** (VND): `motion.div` fade-up, CountUp animation, bg-primary icon box
- **Active Users** (30d): CountUp, bg-success-green icon box
- **Active Subscriptions**: CountUp, bg-secondary icon box
- **Total Workspaces**: CountUp, bg-warning-amber icon box
- **Conversion/Pending**: CountUp, bg-on-surface-variant icon box

Each card uses upgraded `AdminStatsCard` with trend indicator.

#### 3.2 Charts Section
Two Recharts side-by-side:
- **Revenue Trend** (AreaChart): gradient fill `url(#revenueGradient)` from primary to transparent, CartesianGrid with outline-variant/10, Tooltip with glass-panel style, 7D/30D/90D toggle buttons
- **User Signups** (BarChart): bars with primary fill, hover effect, same toggle

#### 3.3 Bottom Grid
Two `AdminDataTable` instances side-by-side:
- Recent Users (top 10)
- Recent Payments (top 10)

#### 3.4 Animation & Effects
- Custom CSS keyframes: `fade-up`, `scale-in`, `bar-grow`
- `motion.div` framer-motion for section entry with staggerChildren
- Header gradient with decorative blur orb via `before:` pseudo-element
- Cards: `glass-panel` utility class where appropriate

**Data fetching:** Keep existing `useAdminDashboard` hook unchanged (API unchanged).

---

## Phase 4: Admin Pages

All 9 pages receive token + typography update. Each page follows the same pattern:

### Standard Page Template
```tsx
"use client";
import { motion } from "motion/react";
// ...

export default function AdminXxxPage() {
  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="space-y-6"
    >
      {/* Page Heading */}
      <div>
        <h1 className="text-headline-sm text-on-surface">Page Title</h1>
        <p className="text-body-sm text-on-surface-variant mt-1">Page description</p>
      </div>

      {/* Content Card */}
      <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
        {/* Table / Form / List content */}
      </div>
    </motion.div>
  );
}
```

### Page-by-page changes

| Page | File | Specific changes |
|---|---|---|
| **Users List** | `admin/users/page.tsx` | Token swap, search input focus ring to primary, table uses upgraded `AdminDataTable`, badge uses new `AdminStatusBadge` |
| **User Detail** | `admin/users/[id]/page.tsx` | Token swap, avatar circle: `bg-primary/20 text-primary`, detail cards: `bg-surface-container-low border-outline-variant/10`, payment list uses new token badges |
| **Workspaces** | `admin/workspaces/page.tsx` | Token swap, table uses upgraded `AdminDataTable` |
| **Subscriptions** | `admin/subscriptions/page.tsx` | Token swap, table uses upgraded `AdminDataTable` |
| **Payments** | `admin/payments/page.tsx` | Token swap, table uses upgraded `AdminDataTable`, status badge uses new `AdminStatusBadge` |
| **Plans** | `admin/plans/page.tsx` | Token swap, plan cards: `bg-surface-container-lowest/80 backdrop-blur-sm border-outline-variant/20 shadow-sm rounded-2xl`, "Create Plan" button: `bg-primary hover:bg-primary-container`, inactive badge: `bg-on-surface-variant/10` |
| **Audit Logs** | `admin/audit-logs/page.tsx` | Token swap, table uses upgraded `AdminDataTable` (page size 20) |
| **Tools** | `admin/tools/page.tsx` | Token swap, seed form cards: tokenized, input with focus ring primary, buttons: `bg-primary hover:bg-primary-container`, `AdminConfirmDialog` -> `ConfirmationModal` |
| **Config** | `admin/config/page.tsx` | Token swap, form inputs focus ring, save button: `bg-primary` |

### Delete Confirmation Pattern
All pages that had `AdminConfirmDialog` now use shared `ConfirmationModal`:
- Users page (deactivate user, change role)
- Workspaces page (delete workspace)
- Payments page (update payment status)
- Subscriptions page (update subscription)
- Tools page (seed confirmation)
- Config page (save confirmation)

---

## Files to Delete

| File | Reason |
|---|---|
| `src/components/admin/AdminHeader.tsx` | Replaced by shared `Header.tsx` |
| `src/components/admin/AdminConfirmDialog.tsx` | Replaced by shared `ConfirmationModal` |

---

## Files to Modify (Summary)

| # | File | Phase | Action |
|---|---|---|---|
| 1 | `src/components/admin/AdminStatusBadge.tsx` | 1 | Retrofit colors |
| 2 | `src/components/admin/AdminStatsCard.tsx` | 1 | Full upgrade |
| 3 | `src/components/admin/AdminDataTable.tsx` | 1 | Full upgrade + sort |
| 4 | `src/components/admin/AdminEmptyState.tsx` | 1 | Full rewrite |
| 5 | `src/components/admin/AdminSidebar.tsx` | 2 | Full rewrite |
| 6 | `src/app/(admin)/layout.tsx` | 2 | Use shared Header + SidebarProvider |
| 7 | `src/app/(admin)/admin/dashboard/page.tsx` | 3 | Full rewrite |
| 8-16 | 9 admin page files | 4 | Token + typography swap |

---

## Not-in-Scope

- Backend API changes (no API changes needed)
- User-facing section changes (untouched)
- `AdminGuard` component (works fine as-is)
- `AISAM-FE-Admin` directory (already abandoned, not touched)
- New admin features (this is UI sync only, no new functionality)
- Shared `Toast.tsx` token migration (out of scope for this task)

---

## Success Criteria

1. Admin section functions identically in light and dark mode
2. All admin pages use design tokens, no hardcoded hex colors remain
3. Admin header has notification + theme toggle working (shared from user)
4. Admin sidebar has glass effect, collapsible, smooth animation
5. Admin dashboard has Recharts, CountUp animation, rich visuals
6. All confirmation dialogs use shared `ConfirmationModal` with animation
7. All admin pages have fade-up entry animation via framer-motion
8. No regression in existing admin functionality
9. Dev server compiles without errors, `npm run build` passes
