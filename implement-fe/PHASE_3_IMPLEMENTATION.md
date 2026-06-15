# Phase 3 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `3.1` den `3.2` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu truc tiep voi backend dashboard hien tai trong `AISAM-BE`.

Pham vi Phase 3:

- Tao app shell dung cho workspace sau khi user da login va da co active profile
- Chot protected routing cho nhom route `(app)`
- Hoan thien dashboard overview page voi summary widgets dung contract backend that
- Dat nen UI de Phase 4 tro di gan vao brands, products, content, social, posts, notifications, schedules
- Chuan bi vi tri de sau nay them quota/subscription/reporting widgets theo target product

Khong lam trong Phase 3:

- Brand/Product pages
- Content library, AI, conversation
- Social Facebook flow
- Posts, Notifications, Scheduling pages chi tiet
- Payment, Team, Approval, Ads

Luu y target product:

- `README.md` va `requirement.md` xem dashboard, reports, analytics, quota/subscription overview la mot phan nang luc san pham.
- Phase 3 hien tai moi cover summary backend-ready; khong duoc doc no nhu dashboard scope day du.

Can cu backend da doi chieu truc tiep cho Phase 3:

- `AISAM-BE/AISAM.API/Controllers/DashboardController.cs`
- `AISAM-BE/AISAM.Services/Service/DashboardService.cs`
- `AISAM-BE/AISAM.Common/Models/DashboardSummaryDto.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`
- `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- `AISAM-BE/AISAM.API/Utils/ProfileContextHelper.cs` thong qua behavior controller/middleware

## Tong quan thu tu lam

1. Task 3.1 - Tao app shell va protected routing
2. Task 3.2 - Tao dashboard summary widgets
3. Chay verify tong the Phase 3

## Contract backend dashboard can chot truoc khi code

### Route active

```text
GET /api/dashboard/summary
```

### Header bat buoc

Route nay can dong thoi:

- `Authorization: Bearer <accessToken>`
- `X-Profile-Id: <activeProfileId>`

Ly do:

- controller co `[Authorize]`
- `ActiveProfileMiddleware` protect prefix `/api/dashboard`

### Middleware behavior can biet

Neu request vao `/api/dashboard/*` ma thieu hoac sai context, backend tra:

- `401` neu chua login
- `401` neu thieu hoac invalid `X-Profile-Id`
- `404` neu profile khong ton tai
- `403` neu profile khong thuoc user dang login

Frontend Phase 3 phai co UX ro cho 4 truong hop nay, khong chi hien generic crash.

### Envelope response

```ts
type ApiResponse<T> = {
  success: boolean
  message?: string
  statusCode: number
  data?: T | null
  error?: {
    errorCode?: string
    errorMessage?: string
    stackTrace?: string
    validationErrors?: Record<string, string[]>
  }
  timestamp: string
}
```

### Dashboard summary DTO exact

```ts
type DashboardSummaryDto = {
  draftContentCount: number
  publishedContentCount: number
  pendingApprovalContentCount: number
  upcomingScheduleCount: number
  failedScheduleCount: number
  activeSocialIntegrationCount: number
  publishedPostCount: number
  unreadNotificationCount: number
}
```

### Metric scope that backend dang tong hop

`DashboardService` hien tai chi tong hop:

- content `Draft`
- content `PendingApproval`
- content `Published`
- tong post records theo profile
- unread notifications
- active social integrations
- upcoming schedules
- failed schedules

Khong co trong DTO hien tai:

- trend theo ngay
- chart time-series
- revenue
- CTR/engagement
- campaign performance

Frontend Phase 3 khong duoc gia dinh co analytics nang hon DTO nay.

## Task 3.1 - Tao app shell va protected routing

### Muc tieu

- Co dashboard layout co header, sidebar, content area
- Bao ve nhom route workspace bang auth + active profile guards
- Chot duong di chinh sau login/onboarding

### File can tao

```text
AISAM-FE/src/app/(app)/layout.tsx
AISAM-FE/src/app/(app)/dashboard/page.tsx
AISAM-FE/src/components/layout/app-shell.tsx
AISAM-FE/src/components/layout/sidebar.tsx
AISAM-FE/src/components/layout/header.tsx
AISAM-FE/src/components/layout/profile-context-banner.tsx
AISAM-FE/src/components/layout/nav-link-list.tsx
AISAM-FE/src/lib/navigation/app-routes.ts
AISAM-FE/src/lib/navigation/post-login-redirect.ts
```

Neu can tach them:

```text
AISAM-FE/src/components/layout/app-shell-skeleton.tsx
AISAM-FE/src/components/layout/mobile-nav.tsx
```

### Route structure can chot

Nhom route app nen song o:

```text
/(app)/dashboard
/(app)/brands
/(app)/posts
/(app)/notifications
/(app)/calendar
/(app)/social-accounts
...
```

Phase 3 moi can page `/dashboard`, nhung layout va nav phai du cho cac route Phase 4+ gan vao khong phai doi structure.

### Access rule cho `(app)` group

De vao layout `(app)`, user phai:

1. da login
2. auth bootstrap xong
3. profile bootstrap xong
4. co `activeProfileId` hop le

Neu khong dung, redirect:

- chua login -> `/auth/login`
- da login nhung chua co profile -> `/onboarding`
- da login co profile nhung `activeProfileId` null do storage stale -> auto-chon profile hop le; neu van null thi `/onboarding`

### Guard flow can chot

Trong `(app)/layout.tsx`:

1. doc `useAuth()`
2. doc `useProfile()`
3. neu auth/profile dang bootstrap:
   - render shell loading state
4. neu chua login:
   - redirect `/auth/login`
5. neu da login nhung `profiles.length === 0`:
   - redirect `/onboarding`
6. neu da login, co profiles, nhung `activeProfileId === null`:
   - co the cho provider auto set lai
   - neu xong van null, redirect `/onboarding`
7. neu hop le:
   - render `AppShell`

### App shell layout can co

`app-shell.tsx` nen co:

- sidebar ben trai
- header ben tren
- content outlet ben phai
- mobile nav hoac drawer cho man hinh hep

Khong can dung card-heavy marketing layout. Day la operational workspace.

### Sidebar nav de xuat

It nhat nen co:

- Dashboard
- Brands
- Products hoac vao tu Brand detail sau
- Contents
- Conversations
- Social Accounts
- Posts
- Notifications
- Calendar
- Account

Nhung Phase 3 chua can enable het route. Route chua xong co the:

- hien disabled
- hoac co link neu page placeholder da ton tai ve sau

Khong duoc tro user vao route khong ton tai.

### Header can co

It nhat nen hien:

- ten app/workspace
- active profile name
- profile switcher trigger
- shortcut vao account
- thong bao unread placeholder co the gan sau

Header nen dung `activeProfile` tu ProfileProvider, khong fetch them API.

### Profile context banner

`profile-context-banner.tsx` dung de xu ly case:

- active profile dang `Pending`
- active profile dang `Suspended`
- active profile dang `Cancelled` va state chua refresh kip

MVP behavior:

- `Pending`: chi hien badge/status text
- `Active`: no banner
- `Suspended`: hien banner thong bao workspace bi han che
- `Cancelled`: provider nen chuyen profile khac hoac redirect onboarding; banner chi la fallback tam

### Post-login redirect can doi

Phase 1 dang redirect sau login ve `/account`. Tu Phase 3 tro di nen chot helper:

```ts
type PostLoginRouteInput = {
  hasProfiles: boolean
  activeProfileId: string | null
}
```

Rule:

- `!hasProfiles` -> `/onboarding`
- `hasProfiles && activeProfileId` -> `/dashboard`
- `hasProfiles && !activeProfileId` -> tam thoi `/dashboard` neu provider se auto set truoc render, hoac `/profiles/<firstId>` neu team muon explicit

Khuyen nghi don gian:

```text
/dashboard
```

khi provider da co auto-set active profile dung.

### Definition of Done

- Group `(app)` co shell dung duoc
- Chua login vao route `(app)` bi redirect login
- Da login chua co profile bi redirect onboarding
- Da login co active profile vao `/dashboard` duoc
- Shell co header, sidebar, content slot on dinh tren desktop va mobile co ban

### Verify

- Test route `/dashboard` khi logout
- Test route `/dashboard` khi account moi chua co profile
- Test route `/dashboard` sau khi co 1 profile
- Test reload F5 khi dang o route `(app)` va active profile ton tai

## Task 3.2 - Tao dashboard summary widgets

### Muc tieu

- Hien tong quan workspace hien tai bang 8 metric MVP
- Mapping loi middleware/backend thanh UI de user biet van de nam o auth hay profile context

### File can tao

```text
AISAM-FE/src/features/dashboard/api/get-summary.ts
AISAM-FE/src/features/dashboard/components/dashboard-summary.tsx
AISAM-FE/src/features/dashboard/components/summary-card.tsx
AISAM-FE/src/features/dashboard/components/dashboard-summary-grid.tsx
AISAM-FE/src/features/dashboard/components/dashboard-empty-state.tsx
AISAM-FE/src/features/dashboard/components/dashboard-error-state.tsx
AISAM-FE/src/types/dashboard.ts
```

Neu team dung React Query:

```text
AISAM-FE/src/features/dashboard/hooks/use-dashboard-summary.ts
```

### API helper can co

`get-summary.ts`

```ts
export async function getDashboardSummary() {
  return api.get<DashboardSummaryDto>(endpoints.dashboard.summary, {
    requireAuth: true,
  })
}
```

Khong can truyen `profileId` qua query/body.

Backend lay profile tu `X-Profile-Id` header thong qua middleware.

### Route page

`src/app/(app)/dashboard/page.tsx` nen:

1. render heading/workspace title
2. render `DashboardSummary`
3. de cho phan recent activity hay shortcuts ve sau, nhung khong fake data

### UI metrics can hien

8 cards:

1. Draft Content
2. Published Content
3. Pending Approval
4. Upcoming Schedules
5. Failed Schedules
6. Active Social Integrations
7. Published Posts
8. Unread Notifications

Khuyen nghi dung icon nhe, khong can chart.

### Mapping field -> label can ro

```ts
draftContentCount -> Draft Content
publishedContentCount -> Published Content
pendingApprovalContentCount -> Pending Approval
upcomingScheduleCount -> Upcoming Schedules
failedScheduleCount -> Failed Schedules
activeSocialIntegrationCount -> Active Social Integrations
publishedPostCount -> Published Posts
unreadNotificationCount -> Unread Notifications
```

### Loading/empty/error state can co

#### Loading

- render summary skeleton grid
- giu layout on dinh

#### Success

- render 8 cards

#### Empty

Ve mat DTO, backend van luon tra object 8 so, ke ca khi tat ca bang `0`.
Vi vay "empty" o dashboard khong phai la `data = null`, ma la:

- ca 8 metric bang `0`

Trong truong hop nay:

- van render cards
- co the them empty hint nho: workspace chua co content/post/schedule

Khong thay bang man hinh empty full-page neu da co DTO hop le.

#### Error

Can tach 3 nhom:

1. `401` do login het han
   - clear session neu can theo auth foundation
   - redirect login
2. `401/404/403` do active profile invalid
   - refresh profiles
   - neu co profile hop le khac thi auto-chon lai va retry
   - neu khong co thi redirect `/onboarding`
3. `500` hoac loi khac
   - render `dashboard-error-state` voi nut retry

### Auto-recovery khi active profile stale

Case thuc te quan trong:

- user dang luu `activeProfileId`
- profile vua bi delete/restore/doi state
- `/dashboard/summary` tra `404` hoac `403`

Frontend flow khuyen nghi:

1. bat error tu request summary
2. `profile.refreshProfiles()`
3. kiem tra `activeProfileId` moi
4. neu da co profile hop le khac:
   - retry summary 1 lan
5. neu khong:
   - redirect onboarding

Khong retry vo han.

### Summary card contract

`summary-card.tsx` nen nhan:

```ts
type SummaryCardProps = {
  label: string
  value: number
  description?: string
  tone?: "default" | "warning" | "danger" | "success"
  href?: string
}
```

`href` la optional de Phase 4+ co the click card sang module lien quan.

Khuyen nghi map tone:

- `failedScheduleCount` -> `danger` neu > 0
- `pendingApprovalContentCount` -> `warning` neu > 0
- `unreadNotificationCount` -> `warning` neu > 0
- con lai `default`

### Data refresh policy

MVP co the dung:

- fetch on page load
- refetch khi user refresh page
- optional refetch on window focus neu team dung React Query

Khong can polling lien tuc trong Phase 3.

### Definition of Done

- Dashboard goi dung `/api/dashboard/summary`
- Request co `Authorization` va `X-Profile-Id`
- Render dung 8 metric
- Co loading, zero-data hint, error state
- Xu ly duoc active profile stale ma khong crash

### Verify

- Test dashboard voi profile moi, cac metric = 0
- Test dashboard voi du lieu co san
- Test xoa key `activeProfileId` roi vao dashboard
- Test active profile bi stale va app tu recover/redirect dung

## Verify tong Phase 3

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- `/dashboard` la protected route
- `X-Profile-Id` duoc gui len `/api/dashboard/summary`
- user chua co profile khong goi dashboard API
- dashboard hien du 8 metric voi response `200`
- middleware error `Missing or invalid X-Profile-Id header` duoc xu ly thanh redirect/refresh profile dung
- shell khong crash khi session reload lai sau F5

## Deliverable sau Phase 3

Can co it nhat:

```text
AISAM-FE/
  PHASE_3_IMPLEMENTATION.md
  src/
    app/
      (app)/
        layout.tsx
        dashboard/
          page.tsx
    components/
      layout/
        app-shell.tsx
        sidebar.tsx
        header.tsx
        profile-context-banner.tsx
    features/
      dashboard/
        api/
        components/
        hooks/
    lib/
      navigation/
        app-routes.ts
        post-login-redirect.ts
    types/
      dashboard.ts
```

## Risk can tranh trong Phase 3

- Giu redirect sau login o `/account`, khong cap nhat sang app shell flow
- Goi `/dashboard/stats` thay vi `/dashboard/summary`
- Co active profile trong storage nhung khong gui `X-Profile-Id`
- Hien empty state full-page khi DTO hop le nhung tat ca metric bang 0
- Build chart/analytics gia ma backend chua co field
- Retry summary vo han khi active profile stale
- Sidebar link den route chua ton tai lam user gap 404

## Rule chuyen sang Phase 4

Chi bat dau Phase 4 khi:

- Phase 3 build pass
- app shell hoat dong on dinh
- dashboard summary goi dung contract backend
- auth + profile guards phoi hop dung trong `(app)` layout
- active profile stale duoc xu ly ma khong can user clear storage thu cong
