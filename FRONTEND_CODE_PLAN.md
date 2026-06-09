# AISAM Frontend - Comprehensive Code Plan

## Approved Frontend Direction Change - Active Workspace

Nguon: `CHANGE_REQUEST_WORKSPACE_SUBSCRIPTION_CREDIT_ANALYSIS.md`.

- Planned, chi trien khai sau khi backend Workspace API san sang.
- Active context muc tieu chuyen tu Profile Store sang Workspace Store.
- Request ownership se gui `X-Workspace-Id`.
- Profile van ton tai de quan ly thong tin ca nhan/doanh nghiep.
- Bo sung Workspace selector, member/invitation, ownership transfer, billing/credits, member quota va lifecycle states.
- Khong doi Profile flow hien tai truoc khi backend migration build/test thanh cong.

**Status:** Ready for implementation via AI  
**Version:** 1.0  
**Last Updated:** 2026-06-03

---

## Table of Contents

1. [Overview & Objectives](#overview--objectives)
2. [Technology Stack](#technology-stack)
3. [Architecture & State Management](#architecture--state-management)
4. [Frontend Phases (FE-01 to FE-05)](#frontend-phases)
5. [Project File Structure](#project-file-structure)
6. [API Integration & Best Practices](#api-integration--best-practices)
7. [User Story to API Mapping](#user-story-to-api-mapping)
8. [Development Workflow](#development-workflow)
9. [Definition of Done](#definition-of-done)
10. [Common Gotchas & Solutions](#common-gotchas--solutions)
11. [Critical Path & Timeline](#critical-path--timeline)
12. [Next Steps](#next-steps)

---

## Overview & Objectives

Frontend implementation plan cho AISAM project (AI-powered social media advertising manager), dựa trên:
- 
requirement.md - Requirement tổng thể
- BACKEND_CODE_PLAN.md - Backend architecture + phases
- CODEBASE_UPDATE.md - Current backend status
- User story details (US-01 đến US-14)
- Current BE codebase structure

### Mục tiêu

Tạo **responsive, type-safe, testable** Next.js frontend app:
1. Tích hợp đúng với API contract backend
2. Support core user journeys: Auth → Profile → Content → Publish
3. Khả năng mở rộng cho future features
4. Best practices: error handling, session management, state management

---

## Technology Stack

| Layer | Tech | Rationale |
|-------|------|-----------|
| Framework | Next.js 14+ | Server components, optimization, App Router |
| Language | TypeScript | Type safety, strict mode |
| Styling | Tailwind CSS + shadcn/ui | Utility-first, accessible components |
| State | Zustand | Lightweight global state (auth, profile) |
| Data | TanStack Query | Caching, sync, automatic refetch |
| Validation | Zod | Type-safe schema validation |
| Form | React Hook Form | Performance-optimized form handling |
| Testing | Vitest + React Testing Library | Unit + integration tests |
| E2E | Playwright | Critical flow testing |
| HTTP | Fetch API | Native, interceptor support |

---

## Architecture & State Management

### Auth Store (Zustand)
\\\	ypescript
{
  accessToken: string | null
  refreshToken: string | null
  expiresAt: Date | null
  user: User | null
  isAuthenticated: boolean
  
  setAuth(tokens, user)
  refreshToken()
  clearAuth()
}
\\\

### Profile Store (Zustand)
\\\	ypescript
{
  profiles: Profile[]
  activeProfileId: string | null
  loading: boolean
  
  setProfiles(profiles)
  setActiveProfile(profileId)
  addProfile(profile)
  clearProfiles()
}
\\\

### Query Cache (TanStack Query)
- Automatic caching, refetch strategies
- Cache invalidation on mutations
- Stale-while-revalidate pattern

### API Response Wrapper

All backend responses wrapped in \GenericResponse<T>\:
\\\json
{
  "success": true,
  "message": "Operation successful",
  "statusCode": 200,
  "data": { /* actual data */ },
  "error": null
}
\\\

Frontend must:
1. Check \success\ field
2. Extract \data\ for business logic
3. If \error\ present, parse \error.validationErrors\ or \error.errorMessage\
4. Fallback to \message\ field

---

## Frontend Phases

### Phase FE-01: Foundation & Auth MVP (2-3 weeks)

**Mục tiêu:** Tạo nền tảng frontend + auth + profile selection

**Backend dependency:** Auth ✅ + Profile ✅

**Deliverables:**
- Project setup (Next.js, Tailwind, TanStack Query, Zustand)
- Auth pages: \/sign-up\, \/login\, \/forgot-password\, \/reset-password\, \/resend-verification\
- Session management: token refresh interceptor, logout
- Account page: \/account\ (GET /api/Auth/me)
- Profile selection page: \/profiles\ (list profiles)
- Create profile page: \/profiles/new\ (first profile creation)
- Route guards: public/protected routes

**User Stories Covered:**
- US-01: Sign up (\POST /api/Auth/register\)
- US-02: Login (\POST /api/Auth/login\)
- US-03: Token refresh (\POST /api/Auth/refresh\) - auto in interceptor
- US-04: Account info (\GET /api/Auth/me\)
- US-05: Logout (\POST /api/Auth/logout\)
- US-08: Resend verification (\POST /api/Auth/verify-email/resend\)
- US-10: Reset password (\POST /api/Auth/reset-password\)
- US-12: Create profile (\POST /api/profiles/user/{userId}\)
- US-13: List profiles (\GET /api/profiles/user/{userId}\)

**Key Tasks:**
1. Setup Next.js 14 + TypeScript + Tailwind + shadcn/ui
2. Setup TanStack Query client
3. Setup Zustand stores (auth, profile)
4. Setup API fetcher + interceptor (token injection, refresh flow)
5. Create route guards (protected, public)
6. Setup error handling + toast system

**Acceptance:**
- ✅ \
pm run dev\ runs correctly
- ✅ \/\ redirects properly (auth → dashboard, public → login)
- ✅ Project builds clean
- ✅ All auth pages functional
- ✅ Token refresh works
- ✅ Profile selection works

---

### Phase FE-02: Master Data (Brand & Product) (1-2 weeks)

**Mục tiêu:** Tạo flow quản lý brand, product

**Backend dependency:** Brand ✅ + Product ✅

**Deliverables:**
1. **Brand management:**
   - \/brands\ - Danh sách brand
   - \/brands/new\ - Tạo brand
   - \/brands/{brandId}\ - Chi tiết brand

2. **Product management:**
   - \/products\ - Danh sách product (filter by brand)
   - \/products/new\ - Tạo product
   - \/products/{productId}\ - Chi tiết product

3. **Dashboard landing page** (link to create content)

**Acceptance:**
- ✅ Brand CRUD works
- ✅ Product CRUD works
- ✅ Filter product by brand
- ✅ All validations match backend rules

---

### Phase FE-03: Content & AI (3-4 weeks)

**Mục tiêu:** Tạo flow tạo content, AI generate, approval

**Backend dependency:** Content ⏳ + AI ⏳ + Approval ⏳

**Pages:**
1. \/dashboard\ - Dashboard overview
2. \/content/new\ - Create new content
3. \/content/{contentId}\ - Edit content
4. \/content/pending-approval\ - Approval queue (for approvers)
5. \/content/scheduled\ - Content calendar

**Features:**
- AI generate/improve variant
- Conversation history (AI chat)
- Content lifecycle management
- Approval workflow

---

### Phase FE-04: Scheduling & Social (2-3 weeks)

**Backend dependency:** Scheduling ⏳ + Social ⏳

**Pages:**
1. \/content/scheduled\ - Scheduling calendar
2. \/social/accounts\ - Connected social accounts
3. Publish workflow

**Features:**
- Content calendar view
- Schedule modal (publish now / schedule)
- Social accounts management
- Publish workflow

---

### Phase FE-05: Notifications & Admin (1-2 weeks)

**Backend dependency:** Notification ⏳ + Admin ⏳

**Pages:**
1. \/notifications\ - Notification list
2. \/admin/users\ - User management (if demo needed)

**Features:**
- Notification bell + list
- Admin user management (if needed for demo)

---

## Project File Structure

\\\
AISAM-FE/
├── src/
│   ├── app/                              # Next.js App Router
│   │   ├── (public)/                     # Public routes (auth flows)
│   │   │   ├── login/page.tsx            # US-02
│   │   │   ├── sign-up/page.tsx          # US-01
│   │   │   ├── forgot-password/page.tsx  # US-09
│   │   │   ├── reset-password/page.tsx   # US-10
│   │   │   └── resend-verification/page.tsx  # US-08
│   │   ├── (protected)/                  # Protected routes (auth required)
│   │   │   ├── account/page.tsx          # US-04
│   │   │   ├── profiles/
│   │   │   │   ├── page.tsx              # US-13 (list profiles)
│   │   │   │   └── new/page.tsx          # US-12 (create profile)
│   │   │   ├── dashboard/page.tsx        # Content dashboard
│   │   │   ├── brands/
│   │   │   │   ├── page.tsx              # Brand list
│   │   │   │   ├── new/page.tsx          # Brand create
│   │   │   │   └── [brandId]/page.tsx    # Brand detail
│   │   │   ├── products/
│   │   │   │   ├── page.tsx              # Product list
│   │   │   │   ├── new/page.tsx          # Product create
│   │   │   │   └── [productId]/page.tsx  # Product detail
│   │   │   ├── content/
│   │   │   │   ├── page.tsx              # Content list
│   │   │   │   ├── new/page.tsx          # Content create
│   │   │   │   └── [contentId]/page.tsx  # Content edit
│   │   │   └── layout.tsx                # Protected layout (header, sidebar)
│   │   └── layout.tsx                    # Root layout
│   ├── components/
│   │   ├── layout/                       # Layout components
│   │   │   ├── Header.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── UserMenu.tsx
│   │   ├── shared/                       # Reusable components
│   │   │   ├── Button.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── Toast.tsx
│   │   │   ├── Skeleton.tsx
│   │   │   └── ErrorBoundary.tsx
│   │   └── auth/                         # Auth-specific components
│   │       ├── LoginForm.tsx
│   │       ├── SignUpForm.tsx
│   │       └── ResetPasswordForm.tsx
│   ├── features/                         # Feature modules (grouped by domain)
│   │   ├── auth/
│   │   │   ├── api/auth-api.ts           # API calls
│   │   │   ├── hooks/use-auth.ts         # Custom auth hook
│   │   │   ├── schemas/auth-schemas.ts   # Zod schemas
│   │   │   └── components/
│   │   ├── profile/
│   │   │   ├── api/profile-api.ts
│   │   │   ├── hooks/use-profiles.ts
│   │   │   ├── schemas/profile-schemas.ts
│   │   │   └── components/
│   │   ├── brand/
│   │   │   ├── api/brand-api.ts
│   │   │   ├── hooks/use-brands.ts
│   │   │   ├── schemas/brand-schemas.ts
│   │   │   └── components/
│   │   ├── product/
│   │   │   ├── api/product-api.ts
│   │   │   ├── hooks/use-products.ts
│   │   │   ├── schemas/product-schemas.ts
│   │   │   └── components/
│   │   ├── content/
│   │   │   ├── api/content-api.ts
│   │   │   ├── hooks/use-content.ts
│   │   │   ├── schemas/content-schemas.ts
│   │   │   └── components/
│   │   ├── ai/
│   │   │   ├── api/ai-api.ts
│   │   │   └── components/
│   │   └── [other-features]/
│   ├── stores/                           # Global state (Zustand)
│   │   ├── auth-store.ts                 # Auth state
│   │   └── profile-store.ts              # Profile state
│   ├── lib/                              # Utilities & helpers
│   │   ├── api/
│   │   │   ├── fetcher.ts                # API client + interceptor
│   │   │   └── client.ts                 # Fetch instance
│   │   ├── guards/
│   │   │   ├── protected-route.tsx
│   │   │   └── public-route.tsx
│   │   ├── auth/
│   │   │   └── session.ts                # Session helpers
│   │   └── utils/
│   │       ├── error-handler.ts
│   │       ├── storage.ts
│   │       └── formatters.ts
│   ├── types/
│   │   ├── api.ts                        # API response/request types
│   │   ├── auth.ts                       # Auth types
│   │   ├── entities.ts                   # Domain entities
│   │   └── errors.ts
│   ├── config/
│   │   └── constants.ts                  # API URL, endpoints
│   └── styles/
│       └── globals.css
├── tests/
│   ├── unit/
│   │   ├── auth/
│   │   ├── profile/
│   │   └── [features]/
│   ├── integration/
│   │   └── mocks/
│   └── e2e/
│       └── critical-flows.spec.ts
├── .env.example
├── .env.local                            # Git ignored
├── next.config.ts
├── tailwind.config.ts
├── tsconfig.json
├── package.json
└── README.md
\\\

---

## API Integration & Best Practices

### Key Principles

1. **Contract-first:** User story detail defines API contract
2. **Error handling:** Proper mapping of HTTP status codes
3. **Token management:** Bearer token in header + auto-refresh on 401
4. **Validation:** Client-side (Zod) + server-side response handling

### Interceptor Flow

\\\
API Request
    ↓
1. Inject Bearer token (from auth-store)
    ↓
2. If accessToken expired → call refresh endpoint
    ↓
3. Retry original request with new token
    ↓
4. If refresh fails (401) → logout + redirect /login
    ↓
5. Handle response: 2xx success, 4xx/5xx errors
\\\

### Best Practices

**Error handling:**
- 400/401/403/500 → user-friendly messages
- Max 1-2 refresh retries to prevent infinite loops
- Logout on 401, don't show persistent modal

**State Management:**
- Auth store: (accessToken, refreshToken, expiresAt, user)
- Profile store: (profiles list, active profile)
- Query cache: TanStack Query for server state
- Local UI state: React useState for forms, modals

**Component Structure:**
- Functional components only
- Custom hooks: Extract reusable logic
- Props drilling: Use Context for deeply nested data
- Composition: Small, composable components

**Form Handling:**
- React Hook Form for performance
- Zod for schema validation
- Async validation: debounce email check
- Field-level errors: from Zod + backend

**Route Protection:**
- Layout-based guards: \(protected)\ directory group
- Client-side guards: HOC for granular checks
- Redirect logic: Auth + on \/login\ → \/profiles\; No auth + on \/dashboard\ → \/login\

**Session Management:**
- Token storage: localStorage (or sessionStorage)
- Tab sync: \storage\ event listener
- Session expiry: Check expiresAt before requests
- Auto-refresh: If needed

**Testing:**
- Unit tests: Schemas, helpers, store logic
- Integration tests: API mock (MSW) + components
- E2E tests: Playwright for critical flows
- Coverage: >80% for critical paths

---

## User Story to API Mapping

\\\
# AISAM Frontend - User Story to Backend API Mapping

**Purpose:** Chá»‰ rÃµ má»‘i quan há»‡ giá»¯a má»—i user story FE vÃ  backend endpoints

---

## ðŸŽ¯ Auth & Session Management

### US-01: Sign up (ÄÄƒng kÃ½ tÃ i khoáº£n)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /sign-up` |
| **Backend Endpoint** | `POST /api/Auth/register` |
| **Request** | `{ email, password, confirmPassword, fullName }` |
| **Success Response** | `200 OK` with `TokenResponse` (accessToken, refreshToken, expiresAt, user) |
| **Error Response** | `400` (email exists, validation), `500` (server error) |
| **Frontend Actions** | Save tokens â†’ auth-store â†’ redirect `/profiles` |
| **Components** | SignUpForm, validation with Zod |
| **Files to Create** | <ul><li>`src/app/(public)/sign-up/page.tsx`</li><li>`src/features/auth/components/SignUpForm.tsx`</li></ul> |

---

### US-02: Login (ÄÄƒng nháº­p)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /login` |
| **Backend Endpoint** | `POST /api/Auth/login` |
| **Request** | `{ email, password }` |
| **Success Response** | `200 OK` with `TokenResponse` |
| **Error Response** | `401` (invalid credentials), `500` (server error) |
| **Frontend Actions** | Save tokens â†’ auth-store â†’ redirect `/profiles` |
| **Components** | LoginForm, links to `/sign-up` and `/forgot-password` |
| **Files to Create** | <ul><li>`src/app/(public)/login/page.tsx`</li><li>`src/features/auth/components/LoginForm.tsx`</li></ul> |

---

### US-03: Token Refresh (LÃ m má»›i phiÃªn)

| Aspect | Details |
|--------|---------|
| **Trigger** | Auto-refresh in API client interceptor when accessToken expires |
| **Backend Endpoint** | `POST /api/Auth/refresh` |
| **Request** | `{ refreshToken }` |
| **Success Response** | `200 OK` with new `TokenResponse` |
| **Error Response** | `401` (invalid/expired refresh token) |
| **Frontend Actions** | Update auth-store with new tokens â†’ retry original request |
| **On Failure** | Clear auth-store â†’ redirect `/login` â†’ show toast "Session expired" |
| **Implementation** | `src/lib/api/fetcher.ts` (interceptor logic) |
| **Files to Create** | <ul><li>`src/lib/api/fetcher.ts`</li><li>`src/stores/auth-store.ts`</li></ul> |
| **Note** | Backend rotates refresh token on each refresh; frontend must overwrite |

---

### US-04: Account Info (Xem thÃ´ng tin tÃ i khoáº£n)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /account` (protected) |
| **Backend Endpoint** | `GET /api/Auth/me` |
| **Request Headers** | `Authorization: Bearer <accessToken>` |
| **Success Response** | `200 OK` with `{ id, email, fullName, role }` |
| **Error Response** | `401` (token invalid/expired), `500` (server error) |
| **Frontend Actions** | Display user info; if 401 â†’ try refresh â†’ if still fail â†’ redirect login |
| **Components** | AccountInfo display component, loading skeleton |
| **Files to Create** | <ul><li>`src/app/(protected)/account/page.tsx`</li><li>`src/features/auth/components/AccountInfo.tsx`</li></ul> |
| **Note** | Does NOT include isEmailVerified or timestamps; those come from login/register response |

---

### US-05: Logout (ÄÄƒng xuáº¥t)

| Aspect | Details |
|--------|---------|
| **Trigger** | User clicks logout button (in header/user menu) |
| **Backend Endpoint** | `POST /api/Auth/logout` |
| **Request** | `{ refreshToken }` |
| **Request Headers** | `Authorization: Bearer <accessToken>` |
| **Success Response** | `200 OK` with `data: null` |
| **Error Response** | `401`, `500` (but frontend clears session anyway) |
| **Frontend Actions** | Clear auth-store + clear profile-store â†’ redirect `/login` |
| **Components** | UserMenu.tsx with logout button |
| **Files to Create** | <ul><li>`src/components/layout/UserMenu.tsx`</li></ul> |
| **Note** | Frontend clears session regardless of API success/failure |

---

### US-08: Resend Email Verification

| Aspect | Details |
|--------|---------|
| **Page** | `GET /resend-verification` (public) |
| **Backend Endpoint** | `POST /api/Auth/verify-email/resend` |
| **Request** | `{ email }` |
| **Success Response** | `200 OK` (always) with neutral message |
| **Frontend Behavior** | Show neutral message even if email doesn't exist (backend policy) |
| **Components** | ResendVerificationForm with email input |
| **Files to Create** | <ul><li>`src/app/(public)/resend-verification/page.tsx`</li><li>`src/features/auth/components/ResendVerificationForm.tsx`</li></ul> |
| **Note** | Backend returns `200` regardless; frontend must NOT indicate if email exists |

---

### US-10: Reset Password

| Aspect | Details |
|--------|---------|
| **Page** | `GET /reset-password?token=xxx&email=yyy` |
| **Backend Endpoint** | `POST /api/Auth/reset-password` |
| **Request** | `{ email, token, newPassword, confirmPassword }` |
| **Query Params** | `token` (from email link), `email` (prefilled) |
| **Success Response** | `200 OK` with message "Password reset successfully..." |
| **Error Response** | `400` (invalid/expired token), `500` (server error) |
| **Frontend Actions** | Show success message + CTA "Back to login" â†’ redirect `/login` |
| **Components** | ResetPasswordForm, parse URL query params |
| **Files to Create** | <ul><li>`src/app/(public)/reset-password/page.tsx`</li><li>`src/features/auth/components/ResetPasswordForm.tsx`</li></ul> |
| **Note** | Backend revokes all sessions after password reset |

---

## ðŸ‘¤ Profile Management

### US-12: Create Profile (Táº¡o business profile)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /profiles/new` (protected) |
| **Backend Endpoint** | `POST /api/profiles/user/{userId}` |
| **Request Headers** | `Authorization: Bearer <accessToken>` |
| **Request Content-Type** | `multipart/form-data` |
| **Request Fields** | `name`, `profileType`, `companyName`, `bio`, `avatarUrl` |
| **Success Response** | `201 Created` with `ProfileResponseDto` |
| **Error Response** | `401` (not authenticated), `403` (userId mismatch), `400`/`500` |
| **Frontend Actions** | Create FormData â†’ POST â†’ on success: set active profile in profile-store â†’ redirect `/dashboard` |
| **Components** | CreateProfileForm with FormData builder |
| **Files to Create** | <ul><li>`src/app/(protected)/profiles/new/page.tsx`</li><li>`src/features/profile/components/CreateProfileForm.tsx`</li></ul> |
| **Notes** | <ul><li>userId from auth-store</li><li>Backend rejects avatarFile upload (use URL only for MVP)</li><li>profileType is enum (0=Free, 1=Basic, 2=Pro)</li></ul> |

---

### US-13: List Profiles (Chá»n/xem danh sÃ¡ch profile)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /profiles` (protected) |
| **Backend Endpoint** | `GET /api/profiles/user/{userId}` |
| **Request Headers** | `Authorization: Bearer <accessToken>` |
| **Success Response** | `200 OK` with array of `ProfileResponseDto` |
| **Error Response** | `401` (not authenticated), `500` |
| **Frontend Actions** | List profiles â†’ user selects â†’ set active profile in profile-store â†’ redirect to relevant page (e.g., `/dashboard` or `/brands`) |
| **Components** | ProfileList with profile cards, selection buttons |
| **Files to Create** | <ul><li>`src/app/(protected)/profiles/page.tsx`</li><li>`src/features/profile/components/ProfileList.tsx`</li><li>`src/features/profile/components/ProfileCard.tsx`</li></ul> |
| **Note** | First-time users (no profiles) should see CTA to create profile |

---

### US-14: Edit Profile (Cáº­p nháº­t profile - optional MVP)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /profiles/{profileId}/edit` (protected) |
| **Backend Endpoint** | `GET /api/profiles/{profileId}` (to prefill) + `PUT /api/profiles/{profileId}` (to update) |
| **Request Headers** | `Authorization: Bearer <accessToken>` |
| **Success Response** | `200 OK` with updated `ProfileResponseDto` |
| **Error Response** | `401`, `403` (ownership), `400`/`500` |
| **Frontend Actions** | Load profile â†’ edit form â†’ submit PUT â†’ on success: update profile-store â†’ redirect back |
| **Components** | EditProfileForm (similar to CreateProfileForm but with existing data) |
| **Files to Create** | <ul><li>`src/app/(protected)/profiles/[profileId]/edit/page.tsx`</li><li>`src/features/profile/components/EditProfileForm.tsx`</li></ul> |
| **Note** | Similar to create but prefilled with current data |

---

## ðŸ¢ Brand Management

### Brand List

| Aspect | Details |
|--------|---------|
| **Page** | `GET /brands` (protected) |
| **Backend Endpoint** | `GET /api/brands?profileId={profileId}` |
| **Request Headers** | `Authorization: Bearer <accessToken>` |
| **Query Params** | `profileId` (from active profile) |
| **Success Response** | `200 OK` with array of brands |
| **Frontend Actions** | List brands with actions (edit, delete) â†’ CTA to create new |
| **Components** | BrandList, BrandCard, BrandActions |
| **Files to Create** | <ul><li>`src/app/(protected)/brands/page.tsx`</li><li>`src/features/brand/components/BrandList.tsx`</li></ul> |

---

### Brand Create

| Aspect | Details |
|--------|---------|
| **Page** | `GET /brands/new` (protected) |
| **Backend Endpoint** | `POST /api/brands` |
| **Request Headers** | `Authorization: Bearer <accessToken>` |
| **Request Fields** | `name`, `logo`, `slogan`, `colorTheme`, `usp`, `targetAudience`, `tone` |
| **Success Response** | `201 Created` with brand data |
| **Frontend Actions** | Submit form â†’ on success: add to brand-store â†’ redirect `/brands` or `/brands/{brandId}` |
| **Components** | BrandForm with rich fields |
| **Files to Create** | <ul><li>`src/app/(protected)/brands/new/page.tsx`</li><li>`src/features/brand/components/BrandForm.tsx`</li></ul> |

---

## ðŸ“¦ Product Management

### Product List

| Aspect | Details |
|--------|---------|
| **Page** | `GET /products` (protected, optionally filtered) |
| **Backend Endpoint** | `GET /api/products?brandId={brandId}` |
| **Query Params** | `brandId` (optional filter) |
| **Frontend Actions** | List products, filter by brand, link to detail/edit |
| **Components** | ProductList, ProductCard, BrandFilter |
| **Files to Create** | <ul><li>`src/app/(protected)/products/page.tsx`</li><li>`src/features/product/components/ProductList.tsx`</li></ul> |

---

### Product Create

| Aspect | Details |
|--------|---------|
| **Page** | `GET /products/new` (protected) |
| **Backend Endpoint** | `POST /api/products` |
| **Request Fields** | `name`, `description`, `price`, `sellingPoints`, `brandId`, `image` (optional) |
| **Success Response** | `201 Created` with product data |
| **Frontend Actions** | Submit form â†’ add to product-store â†’ redirect `/products` |
| **Components** | ProductForm |
| **Files to Create** | <ul><li>`src/app/(protected)/products/new/page.tsx`</li><li>`src/features/product/components/ProductForm.tsx`</li></ul> |

---

## ðŸŽ¨ Content & AI Management (Phase FE-03+)

### Content Create (Táº¡o content má»›i)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /content/new` (protected) |
| **Backend Endpoint** | `POST /api/content` |
| **Request Fields** | `profileId`, `brandId`, `productId` (optional), `contentType`, `platform` |
| **Frontend Actions** | Form submission â†’ create draft content â†’ redirect to editor |
| **Status** | â³ Waiting for backend Content module |

---

### AI Generate Variant

| Aspect | Details |
|--------|---------|
| **Trigger** | User clicks "Generate with AI" in content editor |
| **Backend Endpoint** | `POST /api/ai/generate` |
| **Request Fields** | `contentId`, `brandId`, `productId`, `prompt`, `contentType`, `tone` |
| **Success Response** | `200 OK` with generated variant |
| **Frontend Actions** | Show loading â†’ display generated content â†’ allow user to select/refine |
| **Status** | â³ Waiting for backend AI module |

---

### Content Approval Queue (Cho approvers)

| Aspect | Details |
|--------|---------|
| **Page** | `GET /content/pending-approval` (protected, approver role) |
| **Backend Endpoint** | `GET /api/approval/pending` |
| **Frontend Actions** | List pending content â†’ approve/reject â†’ add feedback |
| **Status** | â³ Waiting for backend Approval module |

---

### Content Scheduling

| Aspect | Details |
|--------|---------|
| **Page** | `GET /content/scheduled` (protected) |
| **Backend Endpoint** | `GET /api/content/scheduled` + `POST /api/scheduling/schedule` |
| **Frontend Actions** | Calendar view â†’ schedule content â†’ display upcoming posts |
| **Status** | â³ Waiting for backend Scheduling module |

---

## ðŸ“± Social Integration (Phase FE-04+)

### Connect Social Account

| Aspect | Details |
|--------|---------|
| **Trigger** | User clicks "Connect Facebook" or similar |
| **Backend Endpoint** | OAuth redirect â†’ `POST /api/social/connect` |
| **Frontend Actions** | Redirect to OAuth URL â†’ handle callback â†’ store social account token |
| **Status** | â³ Waiting for backend Social Integration module |

---

### Publish Content

| Aspect | Details |
|--------|---------|
| **Trigger** | User clicks "Publish now" or "Schedule post" |
| **Backend Endpoint** | `POST /api/content/publish` or `POST /api/scheduling/schedule` |
| **Request Fields** | `contentId`, `socialAccountId`, `publishTime` |
| **Frontend Actions** | Submit â†’ show confirmation â†’ redirect to calendar/dashboard |
| **Status** | â³ Waiting for backend Publishing/Scheduling module |

---

## ðŸ”” Notifications & Admin (Phase FE-05+)

### Notification List

| Aspect | Details |
|--------|---------|
| **Trigger** | Bell icon in header |
| **Backend Endpoint** | `GET /api/notifications` |
| **Frontend Actions** | Dropdown list of notifications â†’ mark as read |
| **Status** | â³ Waiting for backend Notification module |

---

## ðŸ“Š Summary Table: User Stories & Endpoints

| US | Story | Frontend Page | Backend Endpoint | Method | Status |
|----|-------|--------------|------------------|--------|--------|
| US-01 | Sign up | `/sign-up` | `/api/Auth/register` | POST | âœ… Ready |
| US-02 | Login | `/login` | `/api/Auth/login` | POST | âœ… Ready |
| US-03 | Refresh | (interceptor) | `/api/Auth/refresh` | POST | âœ… Ready |
| US-04 | Account | `/account` | `/api/Auth/me` | GET | âœ… Ready |
| US-05 | Logout | (user menu) | `/api/Auth/logout` | POST | âœ… Ready |
| US-08 | Resend email | `/resend-verification` | `/api/Auth/verify-email/resend` | POST | âœ… Ready |
| US-10 | Reset password | `/reset-password` | `/api/Auth/reset-password` | POST | âœ… Ready |
| US-12 | Create profile | `/profiles/new` | `/api/profiles/user/{userId}` | POST | âœ… Ready |
| US-13 | List profiles | `/profiles` | `/api/profiles/user/{userId}` | GET | âœ… Ready |
| US-14 | Edit profile | `/profiles/{id}/edit` | `/api/profiles/{id}` | PUT | âœ… Ready |
| US-XX | Brand list | `/brands` | `/api/brands` | GET | âœ… Ready |
| US-XX | Brand create | `/brands/new` | `/api/brands` | POST | âœ… Ready |
| US-XX | Product list | `/products` | `/api/products` | GET | âœ… Ready |
| US-XX | Product create | `/products/new` | `/api/products` | POST | âœ… Ready |
| US-XX | Content create | `/content/new` | `/api/content` | POST | â³ Pending |
| US-XX | AI generate | (in editor) | `/api/ai/generate` | POST | â³ Pending |
| US-XX | Approval queue | `/content/pending-approval` | `/api/approval/pending` | GET | â³ Pending |
| US-XX | Scheduling | `/content/scheduled` | `/api/content/scheduled` | GET | â³ Pending |
| US-XX | Social connect | (modal) | OAuth flow | - | â³ Pending |
| US-XX | Publish | (button/modal) | `/api/content/publish` | POST | â³ Pending |

---

## ðŸ› ï¸ Implementation Order

Based on backend availability:

### **Batch 1: Phase FE-01** (Auth + Profile)
- âœ… US-01, US-02, US-03, US-04, US-05, US-08, US-10
- âœ… US-12, US-13, US-14
- Dependencies: All âœ… ready

### **Batch 2: Phase FE-02** (Brand + Product)
- âœ… Brand CRUD
- âœ… Product CRUD
- Dependencies: All âœ… ready

### **Batch 3: Phase FE-03** (Content + AI)
- â³ Content CRUD
- â³ AI Generation
- â³ Approval workflow
- Dependencies: Backend Content/AI modules â³ pending

### **Batch 4: Phase FE-04** (Scheduling + Social)
- â³ Content scheduling
- â³ Social account connect
- â³ Publishing
- Dependencies: Backend Scheduling/Social modules â³ pending

### **Batch 5: Phase FE-05** (Notifications + Admin)
- â³ Notification list
- â³ Admin user management (if needed)
- Dependencies: Backend Notification module â³ pending

---

**Last Updated:** 2026-06-03  
**Mapping Version:** 1.0  
**Status:** Ready for FE implementation

\\\

---

## Development Workflow

### Per User Story

1. **Checkout** feature branch: \git checkout -b feat/us-XX-[desc]\
2. **Read** user story detail → understand API contract
3. **Create** API layer: \src/features/[feature]/api/[feature]-api.ts\
   - Test with Swagger against backend
4. **Create** schema: \src/features/[feature]/schemas/[feature]-schemas.ts\
5. **Create** component: \src/features/[feature]/components/[Component].tsx\
   - Client-side validation
   - Error handling
6. **Create** page: \src/app/(public|protected)/[route]/page.tsx\
7. **Test:** \
pm run test\, manual API test, verify AC
8. **Build & lint:** \
pm run build\, \
pm run lint\
9. **Commit:** \git commit -m "feat(us-XX): [description]"\
10. **PR:** Link to backend PR

### Backend Integration Checklist

Before FE implementation:
- ✅ Backend endpoint documented (Swagger)
- ✅ API tested with sample data
- ✅ Error cases tested (400, 401, 500)
- ✅ Response DTO shape confirmed
- ✅ Request DTO validation rules confirmed

---

## Definition of Done

For each user story to be **DONE**:

- [ ] Requirements from user story detail fully implemented
- [ ] API integration matches backend contract exactly
- [ ] Client-side validation per spec
- [ ] All AC (Acceptance Criteria) met
- [ ] Error handling for all error codes
- [ ] Loading states + skeleton for async ops
- [ ] Form data persisted if validation fails
- [ ] Toast/notification for success/error
- [ ] Route protection applied
- [ ] Unit tests written
- [ ] Integration tests with MSW
- [ ] Manual testing against live backend
- [ ] \
pm run build\ passes
- [ ] \
pm run lint\ passes
- [ ] No console errors
- [ ] No breaking changes
- [ ] Commit includes US ticket number
- [ ] PR links backend PR (if applicable)

---

## Common Gotchas & Solutions

| Issue | Root Cause | Solution |
|-------|-----------|----------|
| Infinite refresh loop | Retry logic not bounded | Max 1-2 refresh attempts |
| 401 infinite redirect | Auth check not working | Check \expiresAt\ before request |
| FormData not working | Wrong content-type | Use \multipart/form-data\ for file endpoints |
| Enum mismatch | Backend returns number, frontend expects string | Map enum in types layer |
| CORS errors | Frontend URL not in backend CORS policy | Add frontend URL to backend config |
| Token lost on reload | Token stored in memory only | Persist to localStorage |
| Stale data | Query cache not invalidated | Invalidate after mutations |
| Response status mismatch | Backend returns \statusCode: 200\ even on \201 Created\ | Check \success\ field instead |
| Token rotation | Refresh returns new refresh token | Overwrite old token, not append |
| File upload | Backend MVP rejects file upload | Use URL-only approach for now |
| Email verification | Backend returns neutral response | Don't indicate if email exists |
| Logout on 401 | Session not cleared properly | Always redirect to login, don't show modal |

---

## Critical Path & Timeline

| Phase | Weeks | Dependencies | Priority |
|-------|-------|--------------|----------|
| FE-01: Setup + Auth | 2-3 | Backend Auth ✅ | P0 |
| FE-02: Profile + Master Data | 1-2 | Backend Profile ✅ | P0 |
| FE-03: Content + AI | 3-4 | Backend Content ⏳ | P1 |
| FE-04: Scheduling + Social | 2-3 | Backend Scheduling ⏳ | P1 |
| FE-05: Notifications + Admin | 1-2 | Backend Notification ⏳ | P2 |

**MVP Demo ready:** Full flow from signup → create content → publish (Weeks 1-8)

---

## Next Steps

1. **Review this plan** with team + stakeholders
2. **Setup development environment**
   - Clone AISAM-FE repo
   - \
pm install\
   - Configure \.env.local\ with backend URL
3. **Start Phase FE-01**
   - Foundation setup (Next.js, Zustand, TanStack Query)
   - Implement auth pages + profile selection per user stories
4. **Coordinate with backend team**
   - Ensure backend phases complete before FE phases start
   - Regular sync on API contracts
5. **Setup CI/CD**
   - Linting (ESLint, Prettier)
   - Testing (Vitest, Playwright)
   - Build verification

---

**Document Version:** 1.0  
**Status:** Ready for implementation via AI  
**Last Updated:** 2026-06-03
