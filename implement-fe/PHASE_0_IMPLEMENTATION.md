# Phase 0 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `0.1` den `0.5` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>).

Pham vi Phase 0:

- Tao frontend moi trong `AISAM-FE`
- Dung duoc `pnpm install`, `pnpm dev`, `pnpm build`
- Co API foundation, type foundation, env foundation bam dung `AISAM-BE`
- Chua implement page nghiep vu
- Khong khoa kien truc vao user-app nho hep; phai de du cho payment, approval, ads, admin va storage ve sau

Khong lam trong Phase 0:

- Auth UI
- Profile UI
- Dashboard UI
- Brand/Product/Content UI
- Social, Notifications, Scheduling UI

Luu y target product:

- Phase 0 phai chuan bi foundation de sau nay them payment, quota, team, approval, ads, reports va admin ma khong pha cau truc.
- Moi quyet dinh ve session, api client, types va route grouping phai nghi den product scope tong the trong `README.md` va `requirement.md`.

Can cu backend da doi chieu truc tiep cho Phase 0:

- `AISAM-BE/AISAM.Common/GenericResponse.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/AuthRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateProfileRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateProfileRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateBrandRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateBrandRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/ProductCreateRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/ProductUpdateRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/SocialCallbackRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/AuthResponse.cs`
- `AISAM-BE/AISAM.Common/Dtos/PaginationDtos.cs`
- `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- `AISAM-BE/AISAM.Data/Enumeration/AdTypeEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/ContentStatusEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/AiStatusEnum.cs`

## Tong quan thu tu lam

1. Task 0.1 - Khoi tao project Next.js
2. Task 0.2 - Dung skeleton thu muc frontend
3. Task 0.3 - Them env va config layer
4. Task 0.4 - Tao API client chung
5. Task 0.5 - Tao types core va enum mapper
6. Chay verify tong the Phase 0

## Task 0.1 - Khoi tao project `AISAM-FE`

### Muc tieu

- Co app Next.js App Router moi, TypeScript-ready.
- Build duoc truoc khi them nghiep vu.

### Cach lam

Neu thu muc `AISAM-FE` dang chi co file markdown, giu lai cac file docs va tao source code moi ben canh chung.

Lenh tham chieu:

```text
cd AISAM-FINAL
pnpm create next-app AISAM-FE --ts --app --eslint --src-dir --import-alias "@/*"
```

Neu khong the tao lai bang CLI vi thu muc da ton tai, tao tay cac file toi thieu:

```text
AISAM-FE/package.json
AISAM-FE/tsconfig.json
AISAM-FE/next.config.ts
AISAM-FE/postcss.config.mjs
AISAM-FE/eslint.config.mjs
AISAM-FE/src/app/layout.tsx
AISAM-FE/src/app/page.tsx
AISAM-FE/src/app/globals.css
```

### Cau hinh toi thieu can co

`package.json`

- `next`
- `react`
- `react-dom`
- `typescript`
- `eslint`
- `eslint-config-next`

Co the them ngay:

- `zod`
- `sonner`
- `lucide-react`
- `@tanstack/react-query`
- `clsx`
- `tailwind-merge`

### Man hinh tam thoi

`src/app/page.tsx` chi can hien:

```text
AISAM Frontend is ready.
```

Khong tao landing page marketing trong Phase 0.

### File can co sau task

```text
AISAM-FE/
  package.json
  tsconfig.json
  next.config.ts
  postcss.config.mjs
  eslint.config.mjs
  src/
    app/
      layout.tsx
      page.tsx
      globals.css
```

### Definition of Done

- `pnpm install` pass
- `pnpm dev` start duoc
- `pnpm build` pass
- Route `/` render duoc

### Verify

```text
cd AISAM-FE
pnpm install
pnpm build
pnpm dev
```

## Task 0.2 - Tao cau truc frontend foundation

### Muc tieu

- Co folder structure ro rang truoc khi viet feature code.
- Tach biet shell chung, feature modules, providers, hooks, types.

### Thu muc can tao

```text
AISAM-FE/src/components
AISAM-FE/src/components/layout
AISAM-FE/src/components/states
AISAM-FE/src/components/ui
AISAM-FE/src/features
AISAM-FE/src/features/auth
AISAM-FE/src/features/profile
AISAM-FE/src/features/dashboard
AISAM-FE/src/features/brands
AISAM-FE/src/features/products
AISAM-FE/src/features/content
AISAM-FE/src/features/ai
AISAM-FE/src/features/conversations
AISAM-FE/src/features/social
AISAM-FE/src/features/posts
AISAM-FE/src/features/notifications
AISAM-FE/src/features/schedules
AISAM-FE/src/hooks
AISAM-FE/src/lib
AISAM-FE/src/lib/api
AISAM-FE/src/lib/auth
AISAM-FE/src/lib/profile
AISAM-FE/src/providers
AISAM-FE/src/types
AISAM-FE/src/constants
```

### Quy uoc can chot ngay

- `src/features/<module>/api/*` cho request layer
- `src/features/<module>/components/*` cho UI module
- `src/features/<module>/schemas/*` cho zod schema neu can
- `src/providers/*` chi de chua app-level providers
- `src/types/*` chi chua type shared
- `src/components/ui/*` cho primitive UI reusable

### File placeholder nen tao

```text
src/features/.gitkeep
src/components/ui/.gitkeep
src/hooks/.gitkeep
src/constants/.gitkeep
```

Hoac tao README nho trong `src/features`:

```text
Each feature owns its api, components, and local helpers.
```

### Definition of Done

- Toan bo folder structure ton tai
- Team co the bat dau tao file feature ma khong can tranh luan lai cau truc

### Verify

- `rg --files AISAM-FE/src` thay duoc skeleton
- `pnpm build` van pass

## Task 0.3 - Tao env va config frontend

### Muc tieu

- Loai bo hardcode API base URL
- Co 1 diem doc config runtime cho toan app

### File can tao

```text
AISAM-FE/.env.example
AISAM-FE/src/lib/config.ts
```

### Noi dung `.env.example`

```text
NEXT_PUBLIC_API_BASE_URL=http://localhost:5283/api
NEXT_PUBLIC_APP_ENV=development
NEXT_PUBLIC_ENABLE_DEV_TOOLS=true
```

### Noi dung `src/lib/config.ts`

Can expose toi thieu:

```ts
export const appConfig = {
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5283/api",
  appEnv: process.env.NEXT_PUBLIC_APP_ENV ?? "development",
  enableDevTools: process.env.NEXT_PUBLIC_ENABLE_DEV_TOOLS === "true",
}
```

### Rule bat buoc

- Component va feature khong doc `process.env` truc tiep
- Moi runtime config di qua `appConfig`
- Khong dat bat ky backend secret nao vao frontend env

### Gia dinh runtime can chot ngay

- Backend local mac dinh: `http://localhost:5283/api`
- Frontend chi doc bien `NEXT_PUBLIC_*`
- Dev tools UI ve sau duoc bat/tat bang `NEXT_PUBLIC_ENABLE_DEV_TOOLS`

### Definition of Done

- Co 1 config object duy nhat cho frontend
- Doi base URL chi can sua env

### Verify

- Import `appConfig.apiBaseUrl` tu 1 file test compile duoc
- `pnpm build` pass

## Task 0.4 - Tao API client chung

### Muc tieu

- Tao request layer typed va on dinh
- Dung 1 implementation cho toan bo app

### File can tao

```text
AISAM-FE/src/lib/api/client.ts
AISAM-FE/src/lib/api/endpoints.ts
AISAM-FE/src/lib/api/errors.ts
AISAM-FE/src/lib/auth/session-storage.ts
AISAM-FE/src/lib/profile/active-profile-storage.ts
```

### Contract can ho tro

`client.ts`

- `api.get<T>()`
- `api.post<T>()`
- `api.put<T>()`
- `api.patch<T>()`
- `api.delete<T>()`

Mau contract:

```ts
type RequestOptions = {
  requireAuth?: boolean
  headers?: Record<string, string>
  signal?: AbortSignal
}
```

Khuyen nghi them:

```ts
type BodyLike = Record<string, unknown> | unknown[] | FormData | undefined
```

### Hanh vi bat buoc

#### 1. Header merge

- `Content-Type: application/json` cho request JSON
- bo qua `Content-Type` khi body la `FormData`
- them `Authorization` neu co access token
- them `X-Profile-Id` neu co active profile id

Can ghi ro trong code:

- Khong phai moi request protected deu bat buoc `X-Profile-Id`
- Backend chi enforce `X-Profile-Id` tren cac prefix trong `ActiveProfileMiddleware`
- Cac route auth, profile, brand, product khong di qua middleware nay

Protected prefixes that su dung `X-Profile-Id`:

```text
/api/content
/api/content-schedules
/api/dashboard
/api/dev/scheduler
/api/ai
/api/conversations
/api/social-auth
/api/social
/api/posts
/api/notifications
```

Hanh vi backend khi `X-Profile-Id` sai:

- `401` neu thieu header hoac header khong parse duoc `Guid`
- `404` neu profile khong ton tai
- `403` neu profile khong thuoc user dang login

#### 2. Session source

`session-storage.ts` can co:

- `getSession()`
- `setSession()`
- `clearSession()`
- `getAccessToken()`
- `getRefreshToken()`
- `hasSession()`
- `updateSessionTokens()`

Session can luu duoc shape cua backend `TokenResponse`:

```ts
{
  accessToken: string
  refreshToken: string
  expiresAt: string
  tokenType: "Bearer"
  user: AuthUserDto
}
```

`active-profile-storage.ts` can co:

- `getActiveProfileId()`
- `setActiveProfileId()`
- `clearActiveProfileId()`
- `hasActiveProfile()`

#### 3. Error handling

`errors.ts` can co:

```ts
export type ErrorDetails = {
  errorCode?: string
  errorMessage?: string
  stackTrace?: string
  validationErrors?: Record<string, string[]>
}

export class ApiError extends Error {
  statusCode: number
  errorDetails?: ErrorDetails
  timestamp?: string
}
```

Can map dung envelope backend that:

```ts
{
  success: boolean
  message?: string
  statusCode: number
  data?: unknown
  error?: ErrorDetails
  timestamp: string
}
```

#### 4. Refresh token flow

Refresh logic chi can o muc foundation:

- Neu request protected bi `401`
- va request hien tai khong phai `/auth/login` hoac `/auth/refresh`
- thi goi `POST /auth/refresh`
- neu refresh thanh cong thi retry request 1 lan
- neu refresh fail thi clear session

Payload refresh dung contract backend:

```ts
{ refreshToken: string }
```

Can co co che tranh refresh song song:

- `isRefreshing`
- `refreshPromise`

### `endpoints.ts` phien ban Phase 0

Chi can khai bao endpoint active va endpoint placeholder comments.

Active endpoints nen tao ngay:

```ts
export const endpoints = {
  auth: {
    register: "/auth/register",
    login: "/auth/login",
    google: "/auth/google",
    refresh: "/auth/refresh",
    logout: "/auth/logout",
    logoutAll: "/auth/logout-all",
    sessions: "/auth/sessions",
    me: "/auth/me",
    forgotPassword: "/auth/forgot-password",
    resetPassword: "/auth/reset-password",
    changePasswordWithToken: "/auth/change-password-with-token",
    verifyEmail: (token: string) => `/auth/verify-email?token=${encodeURIComponent(token)}`,
    resendVerifyEmail: "/auth/verify-email/resend",
    changePassword: "/auth/change-password",
  },
  profiles: {
    byUser: (userId: string, search?: string, isDeleted?: boolean) => "...",
    byId: (id: string) => `/profiles/${id}`,
    create: (userId: string) => `/profiles/user/${userId}`,
    update: (id: string) => `/profiles/${id}`,
    delete: (id: string) => `/profiles/${id}`,
    restore: (id: string) => `/profiles/${id}/restore`,
  },
  brands: {},
  products: {},
  content: {},
  ai: {},
  conversations: {},
  socialAuth: {},
  socialAccounts: {},
  socialIntegrations: {},
  posts: {},
  notifications: {},
  schedules: {},
  dashboard: {},
}
```

`profiles.byUser()` nen viet day du ngay:

```ts
byUser: (userId: string, search?: string, isDeleted?: boolean) => {
  const params = new URLSearchParams()
  if (search) params.set("search", search)
  if (typeof isDeleted === "boolean") params.set("isDeleted", String(isDeleted))
  const query = params.toString()
  return `/profiles/user/${userId}${query ? `?${query}` : ""}`
}
```

Khuyen nghi khai bao san endpoint groups active ngay trong Phase 0:

- `brands`
- `products`
- `content`
- `ai`
- `conversations`
- `socialAuth`
- `socialAccounts`
- `socialIntegrations`
- `posts`
- `notifications`
- `schedules`
- `dashboard`

Ly do:

- Giu 1 diem su that cho route names
- Giam sua dong loat o Phase 1+

### Definition of Done

- API client compile pass
- Co refresh token retry flow co ban
- Co helper cho session va active profile
- Chua can page nghiep vu van build pass

### Verify

- `pnpm build` pass
- Co the viet `src/app/page.tsx` goi import `api` ma khong loi compile

## Task 0.5 - Tao types core va enum mapper

### Muc tieu

- Chot typed contracts ngay tu dau
- Giam sua dong loat o cac phase sau

### File can tao

```text
AISAM-FE/src/types/api.ts
AISAM-FE/src/types/auth.ts
AISAM-FE/src/types/profile.ts
AISAM-FE/src/types/brand.ts
AISAM-FE/src/types/product.ts
AISAM-FE/src/types/content.ts
AISAM-FE/src/types/ai.ts
AISAM-FE/src/types/conversation.ts
AISAM-FE/src/types/social.ts
AISAM-FE/src/types/post.ts
AISAM-FE/src/types/notification.ts
AISAM-FE/src/types/schedule.ts
AISAM-FE/src/types/dashboard.ts
AISAM-FE/src/constants/enums.ts
```

### Noi dung toi thieu cho `api.ts`

```ts
export type ErrorDetails = {
  errorCode?: string
  errorMessage?: string
  stackTrace?: string
  validationErrors?: Record<string, string[]>
}

export type ApiResponse<T> = {
  success: boolean
  message?: string
  statusCode: number
  data?: T
  error?: ErrorDetails
  timestamp: string
}

export type PagedResult<T> = {
  data: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}
```

Luu y:

- Backend that dung field `error`, khong phai `errors`
- `message` co the null
- `data` co the null o logout, change-password, verify-email

### Noi dung toi thieu cho `auth.ts`

- `AuthUserDto`
- `AuthSession`
- `CurrentUserResponse`
- `LoginRequest`
- `RegisterRequest`
- `RefreshTokenRequest`
- `LogoutRequest`
- `ChangePasswordRequest`
- `GoogleLoginRequest`
- `SessionDto`

Contract request exact theo backend:

```ts
type RegisterRequest = {
  email: string
  password: string
  confirmPassword: string
  fullName?: string
}

type LoginRequest = {
  email: string
  password: string
}

type RefreshTokenRequest = {
  refreshToken: string
}

type LogoutRequest = {
  refreshToken?: string
}

type ChangePasswordRequest = {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

type GoogleLoginRequest = {
  idToken: string
}
```

Luu y:

- `POST /api/Auth/login` va `POST /api/Auth/register` tra `TokenResponse`
- `TokenResponse` backend shape:
  - `accessToken`
  - `refreshToken`
  - `expiresAt`
  - `tokenType`
  - `user`
- `GET /api/Auth/me` khong tra `UserDto` day du, ma tra object toi gian:
  - `id`
  - `email`
  - `fullName`
  - `role`

Can tach:

- `AuthUserDto` cho `TokenResponse.user`
- `CurrentUserResponse` cho `/auth/me`

### Noi dung toi thieu cho `profile.ts`

- `ProfileResponseDto`
- `CreateProfileFormValues`
- `UpdateProfileFormValues`

Luu y:

- Form profile la `multipart/form-data`
- Backend MVP khong dung upload avatar file
- `CreateProfileRequest` that:
  - `name`
  - `profileType`
  - `companyName?`
  - `bio?`
  - `avatarUrl?`
  - `avatarFile?`
- `UpdateProfileRequest` that:
  - `name?`
  - `profileType?`
  - `companyName?`
  - `bio?`
  - `avatarUrl?`
  - `avatarFile?`

### Noi dung toi thieu cho `brand.ts`

- `BrandResponseDto`
- `CreateBrandRequest`
- `UpdateBrandRequest`

Field request can map ngay:

- `name`
- `description?`
- `logoUrl?`
- `slogan?`
- `usp?`
- `targetAudience?`
- `profileId?`

### Noi dung toi thieu cho `product.ts`

- `ProductResponseDto`
- `ProductCreateFormValues`
- `ProductUpdateFormValues`

Field request exact:

- `ProductCreateRequest`
  - `brandId`
  - `name`
  - `description?`
  - `price?`
  - `imageFiles?`
- `ProductUpdateRequestDto`
  - `brandId?`
  - `name?`
  - `description?`
  - `price?`
  - `imageFiles?`

Luu y:

- Kieu request la `multipart/form-data`
- Phase 1+ UI van nen de shape `File[]` cho dung contract, nhung khong submit file cho MVP

### Noi dung toi thieu cho `content.ts`

- `ContentResponseDto`
- `CreateContentRequest`
- `UpdateContentRequest`

Luu y:

- `adType` la enum number
- khong model no thanh string union

### Noi dung toi thieu cho `ai.ts`

- `CreateDraftRequest`
- `ImproveContentRequest`
- `AiGenerationResponse`
- `ChatRequest`
- `ChatResponse`

### Noi dung toi thieu cho `conversation.ts`

- `ConversationResponseDto`
- `ConversationDetailDto`
- `ChatMessageDto`

### Noi dung toi thieu cho `social.ts`

- `AuthUrlResponse`
- `SocialAccountDto`
- `AvailableTargetDto`
- `SocialTargetDto`
- `LinkSelectedTargetsRequest`
- `SocialCallbackRequest`

`SocialCallbackRequest` backend hien tai chi co:

- `code`
- `state`

### Noi dung toi thieu cho `post.ts`

- `PostListItemDto`
- `PublishResultDto`

### Noi dung toi thieu cho `notification.ts`

- `NotificationListItemDto`
- `NotificationDetailDto`
- `UnreadNotificationCountDto`

### Noi dung toi thieu cho `schedule.ts`

- `CreateContentScheduleRequest`
- `UpdateContentScheduleRequest`
- `ContentScheduleDto`

### Noi dung toi thieu cho `dashboard.ts`

- `DashboardSummaryDto`

### `constants/enums.ts`

Can map toi thieu:

- `AdTypeEnum`
- `ContentStatusEnum`
- `AiStatusEnum`

Index exact hien tai:

```ts
export const adTypeValues = {
  TextOnly: 0,
  ImageText: 1,
  VideoText: 2,
} as const

export const contentStatusValues = {
  Draft: 0,
  PendingApproval: 1,
  Approved: 2,
  Rejected: 3,
  Published: 4,
} as const

export const aiStatusValues = {
  Pending: 0,
  Completed: 1,
  Failed: 2,
} as const
```

Co the them label maps:

```ts
export const adTypeLabels = {
  0: "TextOnly",
  1: "ImageText",
  2: "VideoText",
} as const
```

### Definition of Done

- Tat ca type active ton tai
- Khong dung `any` trong api client foundation
- Enum map co the dung ngay cho badge/select sau nay

### Verify

- `pnpm build` pass
- Khong co lint error obvious do type import

## Verify tong Phase 0

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- `/` render duoc
- import `appConfig`, `api`, `endpoints`, `types` khong loi
- co the doc va clear session tu storage helper
- co the doc va clear active profile tu storage helper
- request helper khong crash khi khong co session
- request helper khong crash khi khong co active profile
- `ApiError` parse duoc response loi backend

## Deliverable sau Phase 0

Can co it nhat:

```text
AISAM-FE/
  FRONTEND_CODE_PLAN.md
  PHASE_0_IMPLEMENTATION.md
  .env.example
  package.json
  src/
    app/
    lib/
      api/
      auth/
      profile/
      config.ts
    providers/
    hooks/
    types/
    constants/
    features/
    components/
```

## Risk can tranh trong Phase 0

- Dung endpoint name sai tu dau, dan den sua day chuyen ve sau
- Hardcode API base URL trong component
- Tron session storage logic vao UI component
- Goi `X-Profile-Id` cho route auth public
- Dung `any` cho response foundation
- Co gang implement feature UI som khi ha tang chua on dinh
- Dung nham `UserDto` cua `TokenResponse` cho `/auth/me`
- Gia dinh sai field envelope la `errors` thay vi `error`

## Rule chuyen sang Phase 1

Chi bat dau Phase 1 khi:

- Phase 0 build pass
- env config doc duoc
- API client co refresh flow co ban
- storage helper cho session va active profile da san sang
- typed contracts core da xong
