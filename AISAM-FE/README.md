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
│   │   │   └── page.tsx         # Main Dashboard Overview
│   │   ├── analytics/
│   │   │   └── page.tsx         # Analytics & Performance Reports
│   │   ├── approvals/
│   │   │   └── page.tsx         # Content Approvals
│   │   ├── brands/
│   │   │   ├── page.tsx         # Brands Listing
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
│   │   │   │   └── page.tsx     # AI Content Generator
│   │   │   └── [id]/
│   │   │       └── page.tsx     # Content Detail
│   │   ├── posts/
│   │   │   └── page.tsx         # Published Posts
│   │   ├── social/
│   │   │   └── page.tsx         # Social Accounts
│   │   └── team/
│   │       └── page.tsx         # Team Management
│   │
│   ├── overview/
│   │   └── page.tsx             # Overview / Profile selector
│   │
│   └── profiles/
│       ├── page.tsx             # Profiles listing
│       ├── new/
│       │   └── page.tsx         # Create Profile
│       └── [id]/
│           └── page.tsx         # Profile Detail (Settings, Team, Billing...)
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
│   │   ├── Header.tsx           # Dashboard Header
│   │   └── Sidebar.tsx          # Dashboard Sidebar Navigation
│   ├── profiles/
│   │   └── CreateProfileModal.tsx
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
│   └── useProfiles.ts           # Profile state management + caching
│
├── lib/
│   ├── apiClient.ts             # API client (JSON + FormData) with 401 auto-refresh
│   ├── auth.ts                  # Token management, refresh, user storage
│   ├── mockContent.ts           # Shared mock data for Content pages
│   └── contentConstants.ts      # Shared constants (PlatformIcon, BRANDS, PRODUCTS, etc.)
│
├── services/
│   ├── analyticsService.ts      # Analytics data — Mock data only
│   ├── brandService.ts          # Brands/Products fetch — API first, mock fallback
│   ├── campaignService.ts       # Campaigns CRUD — localStorage mock
│   ├── contentService.ts        # Content CRUD + AI draft/chat — API first, mock fallback
│   ├── notificationService.ts   # Notifications list/detail + mark read/delete — API first, mock fallback
│   ├── postService.ts           # Posts listing — API first, mock fallback
│   ├── scheduleService.ts       # Schedules CRUD — API first, mock fallback
│   ├── socialAccountService.ts  # Social accounts — localStorage mock
│   └── teamService.ts           # Teams/Members CRUD — localStorage mock
│
└── stores/
    └── profile-store.ts         # Zustand-like active profile selector store
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

### Profiles

| Page | Route | BE Endpoint | Method | Body | Status |
|------|-------|-------------|--------|------|--------|
| **Overview** | `/overview` | `/profiles/user/{userId}` | GET | — | ✅ |
| **Profiles List** | `/profiles` | `/profiles/user/{userId}` | GET | — | ✅ |
| **Create Profile** | (modal) | `/profiles` | POST | FormData | ✅ |
| **Profile Detail** | `/profiles/[id]` | `/profiles/{id}` | GET | — | ✅ |
| | | `/profiles/{id}` | PUT | FormData | ✅ |
| | | `/profiles/{id}` | DELETE | — | ✅ |

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

### Auth Flow
- JWT access token lưu trong `localStorage` key `aisam_token`
- Refresh token lưu trong `localStorage` key `aisam_refresh_token`
- User info lưu trong `localStorage` key `aisam_user`
- Tự động refresh token khi nhận 401 (qua `apiClient`/`apiFetch`)
- Refresh token single-use: BE revoke token cũ mỗi lần refresh
- Logout gọi `POST /auth/logout` + xoá toàn bộ storage

### API Layer
- **`apiClient()`** — dùng cho JSON body endpoints (auth, brands). Tự động thêm `Authorization` + `X-Profile-Id`, auto-refresh 401.
- **`apiFetch()`** — dùng cho FormData endpoints (products, profiles). Tự động thêm `Authorization` + `X-Profile-Id`, auto-refresh 401.
- Generic response wrapper: `{ success, data, message }`
- List endpoints trả về `PagedResult<T>`: `{ success, data: { data: T[], totalCount, page, pageSize } }`
- Brand CRUD dùng **JSON body** (`[FromBody]`) — đúng BE
- Product CRUD dùng **FormData** (`[FromForm]`) — đúng BE

### Middleware Notes
- `ActiveProfileMiddleware` yêu cầu header `X-Profile-Id` cho các prefix: `/api/content`, `/api/dashboard`, `/api/social`, `/api/posts`, `/api/ai`, `/api/quota`, `/api/payment`
- Auth endpoints (`/api/auth/*`) và brand/product/profile endpoints **không** yêu cầu X-Profile-Id

### Sections chưa map BE (chỉ UI / mock / localStorage)

| Section | Route | Trạng thái | Chi tiết |
|---------|-------|-----------|----------|
| **Notifications** | `/notifications` | ✅ Hoàn chỉnh | BE có `NotificationsController`, FE dùng mock (`useMockData = true`), sẵn sàng switch qua API thật |
| **Dashboard KPI & Charts** | `/dashboard` | ⏳ Một phần | Schedule section đã gọi BE, KPI & charts vẫn mock |
| **Campaigns** | `/campaigns` | ⏳ Mock | BE có entity `AdCampaign`, chưa có Controller/Service |
| **Team Management** | `/team` | ⏳ Mock | BE có Models (`Team`, `TeamMember`, `TeamBrand`), chưa có Controller/Service/Repository |
| **Analytics** | `/analytics` | ⏳ Mock | BE có `PerformanceReport` model + Repository, chưa có Controller/Service |
| **Social Accounts** | `/social` | ⏳ Mock | BE có `SocialAccountController`, FE chưa kết nối |
| **Security (change password)** | (profile) | ⏳ Mock | Trong Profile Detail |
| **Billing & Quota** | (profile) | ⏳ Mock | Hardcoded data |
| **Subscription** | (profile) | ⏳ Mock | Hardcoded data |
| **Content list filters** | `/content` | ⏳ Một phần | `tags`, `platforms`, `date range` chỉ mock, BE chưa hỗ trợ |
| **Content thumbnails upload** | `/content` | ⏳ Mock | Chỉ mock object URL |
| **Approvals batch actions** | `/approvals` | ⏳ Một phần | Frontend gọi lần lượt từng item |
| **Approvals sort / search** | `/approvals` | ⏳ Một phần | `searchTerm`, `sortBy`, `sortDescending` đã có query params |
