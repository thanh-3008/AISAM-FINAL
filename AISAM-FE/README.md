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
│   │   └── brands/
│   │       ├── page.tsx         # Brands Listing
│   │       └── [id]/
│   │           └── page.tsx     # Brand Detail (Products, Campaigns, Settings)
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
│   ├── layout/
│   │   ├── Header.tsx           # Dashboard Header
│   │   └── Sidebar.tsx          # Dashboard Sidebar Navigation
│   └── profiles/
│       └── CreateProfileModal.tsx
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
│   ├── contentService.ts        # Content CRUD + AI draft/chat — API first, mock fallback
│   └── brandService.ts          # Brands/Products fetch — API first, mock fallback
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

### Service Layer

| File | Mô tả | Fallback |
|------|-------|----------|
| `src/services/contentService.ts` | CRUD Content + AI draft/chat | `MOCK_CONTENT` / `MOCK_DETAILS` nếu API lỗi |
| `src/services/brandService.ts` | Brands + Products listing | `BRANDS` / `PRODUCTS` constants nếu API lỗi |

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

### Sections chưa có BE (chỉ UI / mock)
- **Dashboard (`/dashboard`)** — hiển thị mock data, chưa gọi API
- **Campaigns** — BE chưa có CampaignController (chỉ có entity `AdCampaign` trong DB)
- **Team** — trong Profile Detail (section sidebar)
- **Security (change password)** — trong Profile Detail
- **Billing & Quota** — hardcoded data
- **Subscription** — hardcoded data
- **Content list filters (tags, platforms, date range)** — chỉ mock, BE chưa hỗ trợ
- **Content thumbnails upload** — chỉ mock object URL
