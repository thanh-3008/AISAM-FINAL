# AISAM Frontend (AISAM-FE)

This is a [Next.js](https://nextjs.org) project for the AISAM (AI-Powered Social Media Advertising Manager) Frontend, bootstrapped with `create-next-app` using Next.js 16, React 19, and Tailwind CSS v4.

## Getting Started

First, make sure you have installed the dependencies:

```bash
npm install
```

Copy `.env.example` to `.env.local` and fill in the required values:

```bash
NEXT_PUBLIC_API_URL=http://localhost:5027/api      # Backend API base URL
NEXT_PUBLIC_GOOGLE_CLIENT_ID=                       # Google OAuth client ID (for Google login)
```

Then, run the development server:

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Project Structure

The project follows the standard Next.js App Router structure:

```text
src/
├── app/
│   ├── favicon.ico
│   ├── globals.css              # Tailwind v4 configuration and global styles
│   ├── layout.tsx               # Root layout (fonts, metadata, etc.)
│   ├── page.tsx                 # Landing Page
│   │
│   ├── (auth)/                  # Authentication routes
│   │   ├── login/
│   │   │   └── page.tsx         # Login Page
│   │   ├── register/
│   │   │   └── page.tsx         # Register Page
│   │   ├── forgot-password/
│   │   │   └── page.tsx         # Forgot Password
│   │   └── reset-password/
│   │       └── page.tsx         # Reset Password
│   │
│   ├── (dashboard)/             # Dashboard routes (require login)
│   │   ├── layout.tsx           # Dashboard layout (Sidebar + Header)
│   │   ├── dashboard/
│   │   │   └── page.tsx         # Main Dashboard Overview (with Credit Balance & Post Quota)
│   │   ├── analytics/
│   │   │   └── page.tsx         # Analytics & Performance Reports
│   │   ├── approvals/
│   │   │   └── page.tsx         # Content Approvals
│   │   ├── brands/
│   │   │   ├── page.tsx         # Brands Listing (workspace-scoped)
│   │   │   └── [id]/
│   │   │       └── page.tsx     # Brand Detail (Products, Campaigns, Settings)
│   │   ├── calendar/
│   │   │   └── page.tsx         # Content Calendar (Month/Week/List views)
│   │   ├── notifications/
│   │   │   └── page.tsx         # Notifications (detail modal, delete, mark read)
│   │   ├── campaigns/
│   │   │   └── page.tsx         # Ad Campaigns Management
│   │   ├── content/
│   │   │   ├── page.tsx         # Content Library
│   │   │   ├── create/
│   │   │   │   └── page.tsx     # Create Content
│   │   │   ├── ai-generate/
│   │   │   │   └── page.tsx     # AI Content Generator (with Credit check)
│   │   │   └── [id]/
│   │   │       └── page.tsx     # Content Detail
│   │   ├── posts/
│   │   │   └── page.tsx         # Published Posts (with Post Quota display)
│   │   ├── social/
│   │   │   └── page.tsx         # Social Accounts
│   │   └── team/
│   │       └── page.tsx         # Team Management
│   │
│   ├── overview/
│   │   └── page.tsx             # Workspace selector (Personal auto-create / Business modal)
│   │
│   └── profiles/
│       ├── page.tsx             # Workspaces listing
│       ├── new/
│       │   └── page.tsx         # Create Workspace
│       └── [id]/
│           └── page.tsx         # Workspace Detail (Settings, Team, Billing, Credit Pack...)
│
├── components/
│   ├── brands/
│   │   ├── CreateBrandModal.tsx # Create Brand modal
│   │   ├── EditBrandModal.tsx   # Edit Brand modal
│   │   └── ProductModal.tsx     # Create/Edit Product modal
│   ├── campaigns/
│   │   ├── CampaignCard.tsx     # Campaign card component
│   │   ├── CampaignDetailModal.tsx
│   │   ├── CampaignEmptyState.tsx
│   │   ├── CampaignFilterBar.tsx
│   │   ├── CampaignStatsCards.tsx
│   │   ├── CreateCampaignModal.tsx
│   │   ├── DeleteConfirmModal.tsx
│   │   ├── EditCampaignModal.tsx
│   │   └── campaignUtils.ts
│   ├── layout/
│   │   ├── Header.tsx           # Dashboard Header (with Workspace Selector)
│   │   ├── Sidebar.tsx          # Dashboard Sidebar Navigation (with Workspace Selector)
│   │   └── WorkspaceSettingsSidebar.tsx  # Workspace settings sidebar
│   ├── profiles/
│   │   └── CreateProfileModal.tsx  # Create Workspace modal (Personal/Business)
│   └── team/
│       ├── BulkActionsBar.tsx
│       ├── CreateTeamModal.tsx
│       ├── DeleteConfirmModal.tsx
│       ├── DeleteMemberConfirmModal.tsx
│       ├── EditMemberModal.tsx
│       ├── EditTeamModal.tsx
│       ├── InviteMemberModal.tsx
│       ├── MemberCard.tsx
│       ├── RoleDonutChart.tsx
│       ├── TeamCard.tsx
│       ├── TeamDetailModal.tsx
│       ├── TeamEmptyState.tsx
│       ├── TeamFilterBar.tsx
│       ├── TeamListView.tsx
│       ├── TeamStatsCards.tsx
│       └── teamUtils.ts
│
├── hooks/
│   ├── useProfiles.ts           # Profile state management (legacy, deprecated)
│   └── useWorkspaces.ts         # Workspace state management + caching + fallback
│
├── lib/
│   ├── apiClient.ts             # API client (JSON + FormData) with X-Workspace-Id header
│   ├── auth.ts                  # Token management, refresh, user storage
│   ├── mockContent.ts           # Shared mock data for Content pages
│   ├── mockWorkspace.ts         # Mock data for Workspace, Credit Wallet, Post Quota
│   └── contentConstants.ts      # Shared constants (PlatformIcon, BRANDS, PRODUCTS, etc.)
│
├── services/
│   ├── analyticsService.ts      # Analytics data — Mock data only
│   ├── brandService.ts          # Brands/Products fetch — API first, mock fallback
│   ├── campaignService.ts       # Campaigns CRUD — localStorage mock
│   ├── contentService.ts        # Content CRUD + AI draft/chat — API first, mock fallback
│   ├── notificationService.ts   # Notifications list/detail + mark read/delete — API first, mock fallback
│   ├── postService.ts           # Posts listing — API first, mock fallback
│   ├── profileSettingsService.ts # Password, Payment, Subscription — API first, mock fallback
│   ├── scheduleService.ts       # Schedules CRUD — API first, mock fallback
│   ├── socialAccountService.ts  # Social accounts — localStorage mock
│   ├── teamService.ts           # Teams/Members CRUD — localStorage mock
│   └── workspaceService.ts      # Workspace dashboard, Credit Wallet, Post Quota — API first, mock fallback
│
└── stores/
    ├── profile-store.ts         # Legacy active profile store (deprecated)
    └── workspace-store.ts       # Active workspace selector store
```

## Styling & Design System
- The project uses **Tailwind CSS v4**. All custom tokens (colors, typography, spacing) are defined in `src/app/globals.css` using the `@theme` directive.
- Check out `DESIGN_SYSTEM.md` for the complete design guidelines, color palettes, and typography rules.
- We use [Material Symbols Outlined](https://fonts.google.com/icons) for iconography.

## Hướng dẫn sử dụng (Usage Guide)

Sau khi khởi chạy dự án, bạn có thể truy cập các đường dẫn sau để xem và kiểm tra các màn hình đã được xây dựng:

1. **Landing Page (`/`)**: Trang chủ giới thiệu sản phẩm.

2. **Trang Đăng Nhập (`/login`)**: Giao diện đăng nhập.
   - Hỗ trợ email/password + Google Sign-In (cần cấu hình `NEXT_PUBLIC_GOOGLE_CLIENT_ID`).

3. **Trang Đăng Ký (`/register`)**: Giao diện tạo tài khoản mới.
   - Có tích hợp thanh hiển thị độ mạnh mật khẩu.

4. **Trang Dashboard (`/dashboard`)**: Bảng điều khiển chính.

5. **Brands (`/brands`)**: Quản lý danh sách thương hiệu.
   - Tạo / Sửa / Xoá brand
   - Xem chi tiết brand với 3 tab: Products, Campaigns, Settings

6. **Brand Detail (`/brands/[id]`)**: Chi tiết thương hiệu.
   - **Products tab**: Xem danh sách sản phẩm, tạo/sửa/xoá sản phẩm (kèm upload ảnh)
   - **Campaigns tab**: Danh sách chiến dịch quảng cáo
   - **Settings tab**: Cập nhật thông tin brand

7. **Content Library (`/content`)**: Thư viện nội dung.
   - Danh sách content với filter theo brand, loại, trạng thái
   - Tạo content mới hoặc dùng AI generate

8. **AI Generate (`/content/ai-generate`)**: Tạo nội dung bằng AI.
   - Chat với AI để tạo content
   - Lưu content vào thư viện

9. **Approvals (`/approvals`)**: Duyệt nội dung.
   - Danh sách content chờ duyệt
   - Approve / Reject content

10. **Calendar (`/calendar`)**: Lịch đăng bài.
    - Xem lịch theo tháng/tuần/danh sách
    - Tạo / Sửa / Xoá lịch đăng

11. **Notifications (`/notifications`)**: Trung tâm thông báo.
    - Danh sách thông báo với filter All / Unread
    - Xem chi tiết thông báo (modal)
    - Đánh dấu đã đọc / Đánh dấu tất cả đã đọc
    - Xoá thông báo
    - Hiển thị số thông báo chưa đọc trên Header

12. **Posts (`/posts`)**: Bài đăng đã publish.
    - Danh sách bài đã đăng lên mạng xã hội
    - Xem chi tiết và xoá bài

13. **Campaigns (`/campaigns`)**: Quản lý chiến dịch quảng cáo.
    - Tạo / Sửa / Xoá campaign
    - Theo dõi hiệu suất (impressions, clicks, spend)
    - Bulk actions (chọn nhiều campaign)

14. **Team Management (`/team`)**: Quản lý nhóm và thành viên.
    - Tạo / Sửa / Xoá team
    - Mời thành viên mới
    - Phân quyền (Owner, Admin, Editor, Member, Viewer)
    - Xem biểu đồ phân bố roles

15. **Analytics (`/analytics`)**: Phân tích hiệu suất.
    - KPIs: Ad Spend, Conversion Rate, CPA, ROAS
    - Biểu đồ xu hướng
    - AI Insights và recommendations

16. **Social Accounts (`/social`)**: Quản lý tài khoản mạng xã hội.
    - Kết nối / Ngắt kết nối tài khoản
    - Xem thống kê followers, posts

## Workspace System (New)

### Overview
Hệ thống đã chuyển từ **Profile-based** sang **Workspace-based** ownership. Mỗi user có thể có nhiều workspaces (Personal hoặc Business).

### Workspace Types

| Type | Value | Description |
|------|-------|-------------|
| **Personal** | 1 | Workspace cá nhân, chỉ có Owner |
| **Business** | 2 | Workspace doanh nghiệp, hỗ trợ team members |

### Workspace Selector

Workspace selector có ở 2 vị trí:
1. **Sidebar** (bottom) - Click để mở dropdown chọn workspace
2. **Header** (left) - Click để mở dropdown chọn workspace

Khi click vào workspace, hệ thống sẽ:
- Lưu workspace vào localStorage (`aisam_active_workspace`)
- Gửi `X-Workspace-Id` header trong tất cả API requests
- Cập nhật UI hiển thị tên workspace

### Personal Workspace (Auto-create)
Khi user chọn "Continue" trên Personal Workspace card:
- Tự động tạo workspace với tên: `"{User's Name}'s Workspace"` hoặc `"{email prefix}'s Workspace"`
- Không cần nhập thông tin
- Redirect thẳng vào Dashboard

### Business Workspace (Modal)
Khi user chọn "Create & Select" trên Business Workspace card:
- Mở modal nhập:
  - **Workspace Name** (bắt buộc)
  - **Company Name** (tùy chọn)
- Sau khi tạo xong, redirect vào Dashboard

### Credit Balance & Post Quota

Dashboard hiển thị 2 KPI cards mới:
1. **AI Credits**: Hiển thị số credits còn lại / tổng (mock: 850/15000)
2. **Posts This Month**: Hiển thị số posts đã dùng / quota (mock: 124/1000)

### API Fallback Mechanism

```
1. Gọi API /workspaces/user/{userId}
   ↓ (fail vì BE chưa có)
2. Fallback sang /profiles/user/{userId}
   ↓ (map Profile → WorkspaceData)
3. Nếu vẫn fail → dùng mock data
```

### Mock Data

| API | Mock Value |
|-----|------------|
| Workspaces | 1 Personal Workspace |
| Credit Wallet | 850 / 15,000 credits |
| Post Quota | 124 / 1,000 posts |
| Dashboard | 850 credits, 876 posts remaining, 124 AI usage |

## Completed Pages & BE API Map

Tất cả các trang dưới đây đã kết nối với Backend thật (base URL configurable qua `NEXT_PUBLIC_API_URL`).

### Auth

| Page | Route | BE Endpoint | Method | Body | Status |
|------|-------|-------------|--------|------|--------|
| **Login** | `/login` | `/auth/login` | POST | `{ email, password }` | ✅ |
| | | `/auth/me` | GET | — (lấy user info sau login) | ✅ |
| | | `/auth/google` | POST | `{ idToken }` | ✅ |
| **Register** | `/register` | `/auth/register` | POST | `{ email, password, confirmPassword, fullName? }` | ✅ |
| **Forgot Password** | `/forgot-password` | `/auth/forgot-password` | POST | `{ email }` | ✅ |
| **Reset Password** | `/reset-password` | `/auth/reset-password` | POST | `{ email, token, newPassword, confirmPassword }` | ✅ |
| **Logout** | (sidebar) | `/auth/logout` | POST | `{ refreshToken? }` | ✅ |

### Workspaces

| Page | Route | BE Endpoint | Method | Body | Status |
|------|-------|-------------|--------|------|--------|
| **Overview** | `/overview` | `/workspaces/user/{userId}` | GET | — | ✅ (fallback → `/profiles`) |
| | | `/profiles/user/{userId}` | POST | FormData: `name, profileType, companyName?` | ✅ |
| **Workspaces List** | `/profiles` | `/profiles/user/{userId}` | GET | — | ✅ |
| **Create Workspace** | (modal) | `/profiles/user/{userId}` | POST | FormData | ✅ |
| **Workspace Detail** | `/profiles/[id]` | `/profiles/{id}` | GET | — | ✅ |
| | | `/profiles/{id}` | PUT | FormData | ✅ |
| | | `/profiles/{id}` | DELETE | — | ✅ |

> **Note**: BE chưa có `/workspaces` endpoints. FE fallback sang `/profiles` API và map `profileType` → `workspaceType`.

### Brands & Products

| Page | Route | BE Endpoint | Method | Body | Status |
|------|-------|-------------|--------|------|--------|
| **Brands List** | `/brands` | `/brands?profileId={id}&pageSize=100` | GET | — (PagedResult) | ✅ |
| **Create Brand** | (modal) | `/brands` | POST | JSON: `{ name, description?, logoUrl?, slogan?, usp?, targetAudience?, profileId }` | ✅ |
| **Edit Brand** | (modal) | `/brands/{id}` | PUT | JSON | ✅ |
| **Delete Brand** | (modal) | `/brands/{id}` | DELETE | — | ✅ |
| **Brand Detail** | `/brands/[id]` | `/brands/{id}` | GET | — | ✅ |
| **Update Brand** | (Settings tab) | `/brands/{id}` | PUT | JSON | ✅ |
| **Products List** | (tab) | `/products?brandId={id}` | GET | — (PagedResult) | ✅ |
| **Create Product** | (modal) | `/products` | POST | FormData: `name, brandId, description?, price?, ImageFiles?` | ✅ |
| **Edit Product** | (modal) | `/products/{id}` | PUT | FormData | ✅ |
| **Delete Product** | (modal) | `/products/{id}` | DELETE | — | ✅ |

### Content

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Content Library** | `/content` | `/content?page=&pageSize=&searchTerm=&brandId=&adType=&status=` | GET | Query params (PagedResult) | ✅ |
| | | `/content/{id}` | DELETE | — (soft delete) | ✅ |
| | | `/content/{id}/restore` | POST | — | ⏳ |
| **Create Content** | `/content/create` | `/content` | POST | JSON: `{ brandId, productId?, adType, title?, textContent, imageUrl?, videoUrl?, styleDescription?, contextDescription? }` | ✅ |
| **Content Detail** | `/content/[id]` | `/content/{id}` | GET | — | ✅ |
| | | `/content/{id}` | PUT | JSON: `{ productId?, adType?, title?, textContent?, imageUrl?, videoUrl? }` | ✅ |
| | | `/content/{id}` | DELETE | — | ✅ |
| **AI Generate** | `/content/ai-generate` | `/ai/generate-draft` | POST | JSON: `{ prompt, brandId?, productId? }` | ✅ |
| | | `/ai/chat` | POST | JSON: `{ message, history: [{ role, text }] }` | ✅ |
| | | `/content` | POST | JSON (lưu bài viết) | ✅ |
| **Approvals** | `/approvals` | `/content?page=&pageSize=&searchTerm=&brandId=&adType=&status=` | GET | Query params (lọc `Awaiting Approval` / `Published` / `Draft`) | ✅ |
| | | `/content/{id}` | PATCH | JSON: `{ status: 2 }` (Approve) | ✅ |
| | | `/content/{id}` | PATCH | JSON: `{ status: 3 }` (Reject / Request Changes) | ✅ |

### Notifications

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Notifications List** | `/notifications` | `/notifications?page=&pageSize=` | GET | Query params (PagedResult) | ✅ |
| **Notification Detail** | (modal) | `/notifications/{id}` | GET | — | ✅ |
| **Mark as Read** | (click) | `/notifications/{id}/mark-read` | POST | — | ✅ |
| **Mark All Read** | (button) | `/notifications/mark-all-read` | POST | — | ✅ |
| **Delete Notification** | (icon) | `/notifications/{id}` | DELETE | — | ✅ |
| **Unread Count** | (header badge) | `/notifications/unread-count` | GET | — | ✅ |

> **Note**: BE đã có `NotificationsController` đầy đủ. FE dùng mock data mặc định (`useMockData = true`), có flag sẵn để switch sang API thật.

### Calendar / Schedules

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Content Calendar** | `/calendar` | `/content-schedules?page=&pageSize=` | GET | Query params (PagedResult) | ✅ |
| | | `/content-schedules/upcoming?limit=` | GET | Query params (array) | ✅ |
| | | `/content-schedules/{id}` | GET | — | ✅ |
| **Create Schedule** | (modal) | `/content-schedules` | POST | JSON: `{ contentId, integrationId, scheduledAt }` | ✅ |
| **Edit Schedule** | (modal) | `/content-schedules/{id}` | PUT | JSON: `{ integrationId?, scheduledAt? }` | ✅ |
| **Delete Schedule** | (modal) | `/content-schedules/{id}` | DELETE | — | ✅ |

### Posts

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Posts List** | `/posts` | `/posts?page=&pageSize=&searchTerm=&brandId=&status=&platform=` | GET | Query params (PagedResult) | ✅ |
| **Post Detail** | (modal) | `/posts/{id}` | GET | — | ✅ |
| **Delete Post** | (modal) | `/posts/{id}` | DELETE | — | ✅ |

### Team Management

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Team List** | `/team` | — | — | — | ⏳ Mock (localStorage) |
| **Create Team** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Edit Team** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Delete Team** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Invite Member** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Edit Member** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Remove Member** | (modal) | — | — | — | ⏳ Mock (localStorage) |

> **Note**: BE đã có Data Models (`Team`, `TeamMember`, `TeamBrand`) nhưng chưa có Controller/Service/Repository.

### Analytics

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Analytics Dashboard** | `/analytics` | — | — | — | ⏳ Mock data |

> **Note**: BE có `PerformanceReport` model và `PerformanceReportRepository` nhưng chưa có Controller/Service.

### Campaigns

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Campaigns List** | `/campaigns` | — | — | — | ⏳ Mock (localStorage) |
| **Create Campaign** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Edit Campaign** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Delete Campaign** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Bulk Actions** | — | — | — | — | ⏳ Mock (localStorage) |

> **Note**: BE có entity `AdCampaign` trong DB nhưng chưa có Controller/Service.

### Social Accounts

| Page | Route | BE Endpoint | Method | Body / Params | Status |
|------|-------|-------------|--------|---------------|--------|
| **Social Accounts List** | `/social` | — | — | — | ⏳ Mock (localStorage) |
| **Add Account** | (modal) | — | — | — | ⏳ Mock (localStorage) |
| **Delete Account** | (modal) | — | — | — | ⏳ Mock (localStorage) |

> **Note**: BE có `SocialAccountController` và `SocialIntegrationController` nhưng FE chưa kết nối.

### Service Layer

| File | Mô tả | Fallback |
|------|-------|----------|
| `src/services/contentService.ts` | CRUD Content + AI draft/chat | `MOCK_CONTENT` / `MOCK_DETAILS` nếu API lỗi |
| `src/services/brandService.ts` | Brands + Products listing | `BRANDS` / `PRODUCTS` constants nếu API lỗi |
| `src/services/scheduleService.ts` | CRUD Schedules + upcoming | `MOCK_SCHEDULES` nếu API lỗi |
| `src/services/postService.ts` | Posts listing + delete | `MOCK_POSTS` nếu API lỗi |
| `src/services/notificationService.ts` | Notifications list/detail + mark read/delete | `MOCK_NOTIFICATIONS` nếu API lỗi hoặc `useMockData = true` |
| `src/services/campaignService.ts` | Campaigns CRUD | `INITIAL_MOCK_CAMPAIGNS` (localStorage) |
| `src/services/teamService.ts` | Teams + Members CRUD | `INITIAL_MOCK_TEAMS` / `INITIAL_MOCK_MEMBERS` (localStorage) |
| `src/services/analyticsService.ts` | Analytics data | `MOCK_ANALYTICS_DATA` (hardcoded) |
| `src/services/socialAccountService.ts` | Social accounts CRUD | `INITIAL_MOCK_ACCOUNTS` (localStorage) |
| `src/services/workspaceService.ts` | Workspace dashboard, Credit Wallet, Post Quota | `getMockWorkspaceDashboard()`, `getMockCreditWallet()`, `getMockPostQuota()` |
| `src/services/profileSettingsService.ts` | Password, Payment, Subscription | Mock data nếu API lỗi |

### Auth Flow
- JWT access token lưu trong `localStorage` key `aisam_token`
- Refresh token lưu trong `localStorage` key `aisam_refresh_token`
- User info lưu trong `localStorage` key `aisam_user`
- Active workspace lưu trong `localStorage` key `aisam_active_workspace`
- Tự động refresh token khi nhận 401 (qua `apiClient`/`apiFetch`)
- Refresh token single-use: BE revoke token cũ mỗi lần refresh
- Logout gọi `POST /auth/logout` + xoá toàn bộ storage

### API Layer
- **`apiClient()`** — dùng cho JSON body endpoints (auth, brands). Tự động thêm `Authorization` + `X-Workspace-Id`, auto-refresh 401.
- **`apiFetch()`** — dùng cho FormData endpoints (products, profiles). Tự động thêm `Authorization` + `X-Workspace-Id`, auto-refresh 401.
- Generic response wrapper: `{ success, data, message }`
- List endpoints trả về `PagedResult<T>`: `{ success, data: { data: T[], totalCount, page, pageSize } }`
- Brand CRUD dùng **JSON body** (`[FromBody]`) — đúng BE
- Product CRUD dùng **FormData** (`[FromForm]`) — đúng BE

### Workspace API (Mock/Fallback)

| API | Endpoint | Mock Data | Status |
|-----|----------|-----------|--------|
| **List Workspaces** | `/workspaces/user/{userId}` | 1 Personal Workspace | ✅ Fallback → `/profiles` |
| **Credit Wallet** | `/credits/wallet` | 850/15000 credits | ✅ Mock |
| **Post Quota** | `/quota/posts` | 124/1000 posts | ✅ Mock |
| **Workspace Dashboard** | `/workspaces/dashboard` | credits, posts, AI usage | ✅ Mock |

> **Note**: Các workspace API chưa có trên BE. FE dùng mock data từ `lib/mockWorkspace.ts`.

### Middleware Notes
- `ActiveWorkspaceMiddleware` yêu cầu header `X-Workspace-Id` cho các prefix: `/api/content`, `/api/dashboard`, `/api/social`, `/api/posts`, `/api/ai`, `/api/quota`, `/api/payment`
- Auth endpoints (`/api/auth/*`) và brand/product/profile endpoints **không** yêu cầu X-Workspace-Id
- **Legacy**: `X-Profile-Id` đã được thay thế bằng `X-Workspace-Id` trong tất cả API requests

### Sections chưa map BE (chỉ UI / mock / localStorage)

| Section | Route | Trạng thái | Chi tiết |
|---------|-------|-----------|----------|
| **Workspace Selector** | (sidebar, header) | ✅ Hoàn chỉnh | Dropdown chọn workspace, mock data fallback |
| **Credit Balance** | `/dashboard` | ✅ Hoàn chỉnh | Hiển thị 850/15000 credits (mock) |
| **Post Quota** | `/dashboard`, `/posts` | ✅ Hoàn chỉnh | Hiển thị 124/1000 posts (mock) |
| **Personal Workspace** | `/overview` | ✅ Hoàn chỉnh | Auto-create với tên user |
| **Business Workspace** | `/overview` | ✅ Hoàn chỉnh | Modal nhập tên công ty |
| **Notifications** | `/notifications` | ✅ Hoàn chỉnh | BE có `NotificationsController`, FE dùng mock (`useMockData = true`), sẵn sàng switch qua API thật |
| **Dashboard KPI & Charts** | `/dashboard` | ⏳ Một phần | Schedule section đã gọi BE, Credit/Post đã mock, KPI & charts khác vẫn mock |
| **Campaigns** | `/campaigns` | ⏳ Mock | BE có entity `AdCampaign`, chưa có Controller/Service |
| **Team Management** | `/team` | ⏳ Mock | BE có Models (`Team`, `TeamMember`, `TeamBrand`), chưa có Controller/Service/Repository |
| **Analytics** | `/analytics` | ⏳ Mock | BE có `PerformanceReport` model + Repository, chưa có Controller/Service |
| **Social Accounts** | `/social` | ⏳ Mock | BE có `SocialAccountController`, FE chưa kết nối |
| **Security (change password)** | (workspace) | ⏳ Mock | Trong Workspace Detail |
| **Billing & Quota** | (workspace) | ⏳ Một phần | Credit Wallet + Post Quota đã mock, Payment history vẫn mock |
| **Subscription** | (workspace) | ⏳ Mock | Hardcoded data, Credit Pack UI đã có |
| **Content list filters** | `/content` | ⏳ Một phần | `tags`, `platforms`, `date range` chỉ mock, BE chưa hỗ trợ |
| **Content thumbnails upload** | `/content` | ⏳ Mock | Chỉ mock object URL |
| **Approvals batch actions** | `/approvals` | ⏳ Một phần | Frontend gọi lần lượt từng item |
| **Approvals sort / search** | `/approvals` | ⏳ Một phần | `searchTerm`, `sortBy`, `sortDescending` đã có query params |

## Recent UI/UX Improvements (2026-01-11)

### Dashboard Sidebar Reorganization
Đã tái cấu trúc sidebar để giảm clutter và cải thiện navigation:

**Trước:**
```
Dashboard
  ├── Dashboard
  └── Workspace                    ← Đã xóa

Content Workspace
  └── ...

Marketing
  └── ...

Administration
  ├── Team Management
  ├── Members                      ← Đã xóa (gộp vào Workspace Settings)
  ├── Credit History               ← Đã xóa (gộp vào Workspace Settings)
  └── Buy Credits                  ← Đã xóa (gộp vào Workspace Settings)
```

**Sau:**
```
Dashboard
  └── Dashboard

Content Workspace
  └── ...

Marketing
  └── ...

Administration
  └── Team Management
```

### Workspace Settings Enhancements
Đã gộp các chức năng vào Workspace Settings (`/profiles/[id]`):

| Section | Tính năng mới |
|---------|---------------|
| **Overview** (mới) | KPI cards, Top Members, Usage Breakdown, Quick Actions |
| **Team** | Gộp Members từ dashboard, hiển thị real member data với filter |
| **Billing & Credits** | Thêm tab "Usage" để xem Credit History |
| **Subscription** | Gộp Buy Credits section với purchase confirmation dialog |

### New Pages & Routes

| Route | Description |
|-------|-------------|
| `/invitation/[token]` | Accept invitation page - hiển thị workspace info, role, quota mode |
| `/overview` | Thêm nút "Go to Dashboard" khi đã có workspace |

### Navigation Flow Improvements

1. **Create Workspace**
   - Sidebar (bottom) → Workspace selector → "Create New Workspace" → Modal
   - Header (left) → Workspace selector → "Create New Workspace" → Modal
   - Sau khi tạo → Redirect `/dashboard`

2. **Workspace Settings Access**
   - Header → Icon settings (⚙️) → `/profiles/[id]`
   - User menu dropdown đã xóa "Settings" để tránh trùng lặp

3. **Overview Page**
   - Thêm nút "Go to Dashboard" ở header
   - User có thể vào dashboard nhanh mà không cần chọn workspace

### Invite Member Flow
Đã hoàn thiện flow mời thành viên:

1. **Từ Workspace Settings** → Team tab → "Invite Member"
2. **Modal** → Chọn email + role (Viewer/Creator/Manager)
3. **API call** → `POST /workspaces/invitations`
4. **Email link** → `/invitation/[token]`
5. **Accept** → Redirect về workspace overview

### Mock Data for Testing

| Token | Trạng thái | Mô tả |
|-------|-----------|-------|
| `test` | Pending | Business workspace, ContentCreator, SharedPool |
| `demo` | Pending | Business workspace, Manager, MonthlyAssigned 5000 |
| `personal` | Pending | Personal workspace, Viewer |
| `expired` | Expired | Lời mời đã hết hạn |
| `cancelled` | Cancelled | Lời mời đã bị hủy |

### Files Changed

| File | Change |
|------|--------|
| `components/layout/Sidebar.tsx` | Xóa Members, Credit History, Buy Credits, Workspace; thêm CreateProfileModal |
| `components/layout/Header.tsx` | Thêm icon settings navigation, xóa Settings từ user menu |
| `components/layout/WorkspaceSettingsSidebar.tsx` | Thêm "overview" section |
| `components/profiles/CreateProfileModal.tsx` | Cải thiện UI với card selection, features list |
| `app/(auth)/invitation/[token]/page.tsx` | **NEW** - Accept invitation page |
| `app/profiles/[id]/page.tsx` | Thêm Overview section, merge Members/Credit History/Buy Credits |
| `app/overview/page.tsx` | Thêm "Go to Dashboard" button |
| `services/workspaceInvitationService.ts` | **NEW** - Invitation API service với mock data |

## UI/UX Enhancements (2026-01-11)

### Toast Notifications System
Thay thế tất cả `alert()` bằng toast notifications đẹp với 4 loại:

| Type | Icon | Color | Usage |
|------|------|-------|-------|
| **success** | `check_circle` | Emerald | Thành công (invite, update role, etc.) |
| **error** | `error` | Red | Lỗi (network, failed, etc.) |
| **warning** | `warning` | Amber | Cảnh báo (expired, limited mode) |
| **info** | `info` | Blue | Thông tin (feature coming soon) |

**Usage:**
```typescript
const { showToast } = useToast();
showToast({ type: "success", title: "Invitation sent", message: "Invitation sent to user@example.com" });
```

### Confirmation Modal Component
Thay thế `confirm()` bằng modal đẹp với 3 loại:

| Type | Icon | Color | Usage |
|------|------|-------|-------|
| **danger** | `dangerous` | Red | Xóa member, transfer ownership |
| **warning** | `warning` | Amber | Cảnh báo quan trọng |
| **info** | `info` | Blue | Xác nhận thông tin |

**Usage:**
```typescript
<ConfirmationModal
  isOpen={confirmModal.isOpen}
  onClose={() => setConfirmModal(prev => ({ ...prev, isOpen: false }))}
  onConfirm={confirmModal.onConfirm}
  title="Remove Member"
  message="Are you sure you want to remove this member?"
  type="danger"
  confirmText="Remove"
/>
```

### Team Members Enhancements

#### 1. Search Members
- Search box tìm theo tên hoặc email
- Real-time filter khi gõ
- Nút clear search
- Hiển thị "No members match your search" khi không tìm thấy

#### 2. Loading Skeletons
- Skeleton loading cho member list (5 items)
- Hiển thị avatar, name, email, role, status placeholders
- Smooth animation khi loading

#### 3. Empty States
- Empty state đẹp khi không có members
- Icon lớn + description
- CTA button "Invite first member"
- Different message cho search result vs no members

#### 4. Member Detail View
- Click vào avatar hoặc name để xem chi tiết
- Modal hiển thị:
  - Avatar + name + email
  - Role badge
  - Status badge
  - Joined date
  - Last active time
  - Quick action: "Change Role" button

#### 5. Bulk Actions
- Checkbox cho mỗi member
- Select all / deselect all
- Bulk action bar hiển thị khi có members được chọn
- Actions: Remove selected members
- Confirmation modal trước khi remove

#### 6. Pagination
- Phân trang 10 members/trang
- Hiển thị "Showing X to Y of Z members"
- Previous / Next buttons
- Page indicator "Page X of Y"

#### 7. Mobile Responsive
- Team header: buttons wrap trên mobile
- Bulk actions bar: full-width trên mobile
- Filters + search: stack vertically trên mobile
- Pagination: responsive layout

### Workspace Type Badge
Thêm badge Personal/Business vào:
- Sidebar workspace selector
- Header workspace selector
- Workspace dropdown list

**Style:**
- Personal: Blue badge với icon `person`
- Business: Purple badge với icon `business`

### Subscription State Banners

#### 1. Expired Banner
- Hiển thị khi subscription hết hạn
- Thông báo credits còn lại
- Button "Renew Subscription"
- Button "Dismiss"

#### 2. Limited Mode Banner
- Hiển thị khi workspace ở Limited Mode (< 90 ngày)
- Danh sách features bị khóa
- Button "Renew Now" (chỉ Owner thấy)
- Button "Dismiss"

#### 3. Archived Banner
- Hiển thị khi workspace Archived (90-180 ngày)
- Thông báo thời gian hết hạn
- Owner: "Renew Subscription" + "Export Data"
- Member: "Contact workspace owner"
- Button "Dismiss"

### Test Buttons (Subscription Tab)
Thêm 3 buttons để test các trạng thái:

| Button | Color | Action |
|--------|-------|--------|
| **Test Expired** | Amber | Toggle Expired Banner |
| **Test Limited** | Red | Toggle Limited Mode Banner |
| **Test Archived** | Gray | Toggle Archived Banner |

### Files Changed

| File | Change |
|------|--------|
| `contexts/ToastContext.tsx` | Toast system mới với 4 types, animations |
| `components/ui/ConfirmationModal.tsx` | **NEW** - Confirmation modal component |
| `components/ui/Toast.tsx` | **NEW** - Toast component (unused, kept for reference) |
| `app/profiles/[id]/page.tsx` | Toast integration, search, skeletons, empty states, member detail, bulk actions, pagination, responsive |
| `components/layout/Sidebar.tsx` | Workspace type badge |
| `components/layout/Header.tsx` | Workspace type badge |

## Workspace Migration (Profile → Workspace)

### Summary of Changes

Hệ thống đã chuyển từ **Profile-based** sang **Workspace-based** ownership theo Change Request.

### Files Changed

| Category | Files |
|----------|-------|
| **Store** | `stores/workspace-store.ts` (new), `stores/profile-store.ts` (deprecated) |
| **Hook** | `hooks/useWorkspaces.ts` (new), `hooks/useProfiles.ts` (deprecated) |
| **API Client** | `lib/apiClient.ts` - Changed `X-Profile-Id` → `X-Workspace-Id` |
| **Services** | `services/workspaceService.ts` (new), `services/profileSettingsService.ts` |
| **Mock Data** | `lib/mockWorkspace.ts` (new) |
| **Layout** | `components/layout/Header.tsx`, `components/layout/Sidebar.tsx` - Added Workspace Selector |
| **Settings** | `components/layout/WorkspaceSettingsSidebar.tsx` (new) |
| **Pages** | `app/overview/page.tsx`, `app/profiles/page.tsx`, `app/profiles/[id]/page.tsx` |
| **Dashboard** | `app/(dashboard)/dashboard/page.tsx` - Added Credit Balance & Post Quota cards |
| **AI Generate** | `app/(dashboard)/content/ai-generate/page.tsx` - Added Credit check |
| **Posts** | `app/(dashboard)/posts/page.tsx` - Added Post Quota display |
| **Auth** | `app/(auth)/login/page.tsx`, `app/(auth)/register/page.tsx` - Updated cache invalidation |

### API Header Change

```typescript
// Before
headers: { "X-Profile-Id": profile.id }

// After
headers: { "X-Workspace-Id": workspace.id }
```

### LocalStorage Keys

| Key | Description |
|-----|-------------|
| `aisam_active_workspace` | Active workspace (new) |
| `aisam_active_profile` | Legacy, auto-migrated to workspace |

### Migration Path

```
1. User logs in
2. Check localStorage for aisam_active_workspace
3. If not found, check aisam_active_profile (legacy)
4. Migrate legacy profile to workspace format
5. Call /workspaces/user/{userId} API
6. If API fails → fallback to /profiles/user/{userId}
7. If still fails → use mock data
```
