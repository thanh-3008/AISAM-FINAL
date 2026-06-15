# Phase 1 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `1.1` den `1.5` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu truc tiep voi backend auth hien tai trong `AISAM-BE`.

Pham vi Phase 1:

- Hoan thien auth va session flow cho frontend moi trong `AISAM-FE`
- Dung duoc login, register, forgot password, reset password, verify email
- Co auth provider, route guard co ban, session persistence, account page
- Chua implement active profile onboarding va protected app shell can `X-Profile-Id`
- Chuan bi auth foundation de sau nay dung chung cho user app, payment flow, approval flow va admin entry points

Khong lam trong Phase 1:

- Profile onboarding
- Dashboard app shell
- Brand/Product/Content pages
- Social, Notifications, Scheduling pages
- Payment, Team, Approval, Ads

Luu y target product:

- Auth phase khong chi phuc vu login vao dashboard user, ma phai duoc thiet ke de support ca subscription/payment/account security va nhung man hinh admin sau nay.

Can cu backend da doi chieu truc tiep cho Phase 1:

- `AISAM-BE/AISAM.API/Controllers/AuthController.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/AuthRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/EmailRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/AuthResponse.cs`
- `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- `AISAM-BE/AISAM.Data/Enumeration/UserRoleEnum.cs`

## Tong quan thu tu lam

1. Task 1.1 - Tao auth provider va auth store
2. Task 1.2 - Tao login page
3. Task 1.3 - Tao register page
4. Task 1.4 - Tao forgot/reset password va verify email pages
5. Task 1.5 - Tao account page va session management UI
6. Chay verify tong the Phase 1

## Contract backend auth can chot truoc khi code

### Route active

```text
POST /api/Auth/register
POST /api/Auth/login
POST /api/Auth/google
POST /api/Auth/refresh
POST /api/Auth/logout
POST /api/Auth/logout-all
GET  /api/Auth/sessions
POST /api/Auth/change-password
GET  /api/Auth/me
POST /api/Auth/forgot-password
POST /api/Auth/reset-password
POST /api/Auth/change-password-with-token
GET  /api/Auth/verify-email?token=...
POST /api/Auth/verify-email/resend
```

### Envelope response

Tat ca route auth deu bam `GenericResponse<T>`:

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

Luu y:

- Backend dung field `error`, khong phai `errors`
- `data` co the la `null` o logout, logout-all, change-password, forgot password, verify email
- `statusCode` nam trong body, nhung frontend van phai uu tien HTTP status that de xu ly retry/redirect

### Token response

`POST /api/Auth/register`, `POST /api/Auth/login`, `POST /api/Auth/google`, `POST /api/Auth/refresh` deu tra `TokenResponse`:

```ts
type AuthSession = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  tokenType: string
  user: {
    id: string
    email: string
    fullName?: string | null
    role: 0 | 1 | 2
    isEmailVerified: boolean
    createdAt: string
    lastLoginAt?: string | null
  }
}
```

Role map dung enum backend:

```ts
type UserRole = 0 | 1 | 2

const userRoleLabels = {
  0: "User",
  1: "Vendor",
  2: "Admin",
} as const
```

### Current user response

`GET /api/Auth/me` khong tra `UserDto` day du, ma chi tra object toi gian:

```ts
type CurrentUserResponse = {
  id: string
  email: string
  fullName?: string | null
  role?: string | null
}
```

Can tach rieng `CurrentUserResponse` va `AuthSession.user`. Khong dung chung mot type.

### Session list response

`GET /api/Auth/sessions` tra:

```ts
type SessionDto = {
  id: string
  createdAt: string
  expiresAt: string
  userAgent?: string | null
  ipAddress?: string | null
  isActive: boolean
}
```

### Request DTO exact theo backend

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

type ForgotPasswordRequest = {
  email: string
}

type ResetPasswordRequest = {
  email: string
  token: string
  newPassword: string
  confirmPassword: string
}
```

### Validation frontend nen bam

Theo DataAnnotations hien tai:

- `email`: required, format email hop le
- `password`: required
- `register.password`: min 8
- `register.confirmPassword`: phai giong `password`
- `changePassword.newPassword`: min 8
- `changePassword.confirmPassword`: phai giong `newPassword`
- `fullName`: max 255

Forgot/reset password DTO trong backend khong co DataAnnotations, nhung frontend van nen validate toi thieu:

- email required, format hop le
- newPassword min 8
- confirmPassword khop `newPassword`
- token khong duoc rong

### Rule header quan trong

- Public auth endpoints khong gui `Authorization`
- Protected auth endpoints gui `Authorization: Bearer <accessToken>`
- Auth endpoints khong can `X-Profile-Id`
- `AuthController` khong nam trong `ActiveProfileMiddleware`

Route thuoc Task 1.5 nhu `/auth/me`, `/auth/sessions`, `/auth/logout-all`, `/auth/change-password` chi can login, khong can active profile.

## Task 1.1 - Tao auth provider va auth store

### Muc tieu

- Tao auth state trung tam cho toan app
- Persist session vao browser storage
- Expose auth actions typed va on dinh cho Phase 1, Phase 2, Phase 3

### File can tao

```text
AISAM-FE/src/providers/auth-provider.tsx
AISAM-FE/src/hooks/use-auth.ts
AISAM-FE/src/lib/auth/session-storage.ts
AISAM-FE/src/lib/auth/auth-guards.ts
AISAM-FE/src/features/auth/api/login.ts
AISAM-FE/src/features/auth/api/register.ts
AISAM-FE/src/features/auth/api/refresh-session.ts
AISAM-FE/src/features/auth/api/logout.ts
AISAM-FE/src/features/auth/api/logout-all-sessions.ts
AISAM-FE/src/features/auth/api/get-current-user.ts
AISAM-FE/src/features/auth/api/get-active-sessions.ts
AISAM-FE/src/types/auth.ts
```

Neu Task 0.4 da co `session-storage.ts`, task nay bo sung implementation auth-specific, khong tao file trung lap.

### Session shape can luu

Frontend nen luu nguyen `TokenResponse` da parse:

```ts
type StoredSession = AuthSession
```

Khong luu rieng le tung key `accessToken`, `refreshToken`, `userId` neu khong co ly do ky thuat ro rang. Luu 1 blob typed de giam sai lech giua storage va memory state.

### API layer can co

`login.ts`

```ts
export async function login(input: LoginRequest) {
  return api.post<AuthSession>(endpoints.auth.login, input, { requireAuth: false })
}
```

`register.ts`

```ts
export async function register(input: RegisterRequest) {
  return api.post<AuthSession>(endpoints.auth.register, input, { requireAuth: false })
}
```

`refresh-session.ts`

```ts
export async function refreshSession(input: RefreshTokenRequest) {
  return api.post<AuthSession>(endpoints.auth.refresh, input, {
    requireAuth: false,
    skipAuthRedirect: true,
    skipProfileHeader: true,
  })
}
```

`logout.ts`

```ts
export async function logout(input?: LogoutRequest) {
  return api.post<null>(endpoints.auth.logout, input ?? {}, { requireAuth: true })
}
```

`get-current-user.ts`

```ts
export async function getCurrentUser() {
  return api.get<CurrentUserResponse>(endpoints.auth.me, { requireAuth: true })
}
```

### Auth context contract

`auth-provider.tsx` nen expose it nhat:

```ts
type AuthContextValue = {
  session: AuthSession | null
  currentUser: CurrentUserResponse | null
  isAuthenticated: boolean
  isBootstrapping: boolean
  isRefreshing: boolean
  login: (input: LoginRequest) => Promise<AuthSession>
  register: (input: RegisterRequest) => Promise<AuthSession>
  logout: (options?: { allSessions?: boolean }) => Promise<void>
  refreshSession: () => Promise<AuthSession | null>
  reloadCurrentUser: () => Promise<CurrentUserResponse | null>
  clearSession: () => void
}
```

### Quy uoc state can chot

- `session`: doc tu storage sau khi hydrate client
- `currentUser`: co the lay tu `session.user` de render som, nhung van nen co `reloadCurrentUser()` de dong bo voi `/auth/me`
- `isBootstrapping`: `true` trong lan load dau tien khi provider doc storage
- `isRefreshing`: `true` trong luc refresh token dang chay

Khuyen nghi:

- Khi app mount, neu khong co session trong storage thi `isBootstrapping = false` ngay
- Neu co session nhung token co ve het han hoac can verify lai, goi `reloadCurrentUser()`
- Neu `/auth/me` tra `401`, clear session

### Storage helper can co

`session-storage.ts` toi thieu:

```ts
const SESSION_STORAGE_KEY = "aisam.auth.session"

export function getSession(): AuthSession | null
export function setSession(session: AuthSession): void
export function clearSession(): void
export function getAccessToken(): string | null
export function getRefreshToken(): string | null
export function hasSession(): boolean
export function updateSessionTokens(next: AuthSession): void
```

Rule implementation:

- Bao ve SSR: neu `typeof window === "undefined"` thi tra `null`
- Parse JSON an toan, parse fail thi `clearSession()`
- Khong luu `CurrentUserResponse` vao key rieng neu khong can

### Route guard co ban can tao

`auth-guards.ts` nen co:

```ts
export function requireGuest(isAuthenticated: boolean, redirectTo = "/account"): void
export function requireAuth(isAuthenticated: boolean, redirectTo = "/auth/login"): void
```

Hoac neu team uu tien pattern component:

```ts
export function GuestOnly({ children }: { children: ReactNode }): JSX.Element
export function ProtectedOnly({ children }: { children: ReactNode }): JSX.Element
```

Phase 1 chua can app-wide middleware phuc tap. Chi can du de:

- page auth redirect neu da login
- page account redirect neu chua login

### Flow login/register/logout can dong bo voi API client

1. `login()` submit body exact theo DTO
2. Nhan `ApiResponse<AuthSession>`
3. `setSession(data)`
4. Gan `session` vao state
5. Co the set `currentUser` tu `data.user` mapping sang object toi gian de render ngay
6. Sau do goi `reloadCurrentUser()` neu can

Flow logout:

1. Lay `refreshToken` tu storage
2. Goi `POST /auth/logout` voi body `{ refreshToken }`
3. Dung thanh cong hay fail van `clearSession()` local
4. Redirect ve `/auth/login`

Flow logout all:

1. Goi `POST /auth/logout-all`
2. Clear session local
3. Redirect ve `/auth/login`

### Rule refresh token can biet

Backend co route `POST /api/Auth/refresh` public. Frontend can:

- Chi retry 1 lan cho moi request fail `401`
- Khong refresh neu request hien tai da la `/auth/login` hoac `/auth/refresh`
- Neu refresh fail: clear session, clear current user, redirect ve login neu dang o protected page

Khong viet refresh logic lap lai trong provider neu Task 0.4 da dat o `api client`. Provider chi can expose `refreshSession()` goi lai lop foundation.

### Google login

Task 1.1 nen de san API helper:

```ts
POST /api/Auth/google
body: { idToken: string }
```

Nhung chua can tao UI button neu trong `FRONTEND_CODE_PLAN.md` chua tach task rieng. Giu helper o muc optional de tranh doi contract sau nay.

### Definition of Done

- Provider wrap duoc app trong `src/app/layout.tsx`
- `useAuth()` doc duoc session va current user
- Login/register/logout/logout-all/refresh co helper typed
- Session persist qua page refresh
- Clear session dung khi refresh fail hoac `/auth/me` tra `401`

### Verify

- `pnpm build` pass
- Refresh browser sau login van con session
- Xoa session key trong devtools khong lam app crash
- Khi storage JSON loi, provider tu clear key thay vi throw

## Task 1.2 - Tao login page

### Muc tieu

- Cung cap flow dang nhap email/password day du
- Mapping loi backend ro rang ra UI

### File can tao

```text
AISAM-FE/src/app/auth/login/page.tsx
AISAM-FE/src/features/auth/components/login-form.tsx
AISAM-FE/src/features/auth/schemas/login-schema.ts
```

Neu team dung React Hook Form, bo sung:

```text
AISAM-FE/src/features/auth/hooks/use-login-form.ts
```

### Request/response can bam

Request:

```ts
{
  email: string
  password: string
}
```

Success response:

```ts
ApiResponse<AuthSession>
```

Failure:

- `401` voi `error.errorMessage` neu sai credential
- `500` neu backend loi noi bo

### UI can co

- 1 form voi `email`, `password`
- Nut submit
- Link sang `sign-up`
- Link sang `forgot-password`

Khong can dua profile selection vao cung page nay. Login page chi chot auth xong moi route tiep.

### Validation frontend

- email required, dung email format
- password required

Khong dat rule khac voi backend o login page.

### Hanh vi submit

1. Disable submit khi dang submit
2. Goi `auth.login(values)`
3. Neu thanh cong:
   - neu chua co session guard sau login: route sang `/onboarding` hoac route shell ma Phase 2 se quyet
   - tam thoi Phase 1 nen redirect ve `/account` de co man hinh verify auth thanh cong
4. Neu that bai:
   - uu tien hien `ApiError.errorDetails.errorMessage`
   - fallback sang `message`

### Rule redirect

Phase 1 chua xong profile onboarding, vi vay redirect sau login nen don gian:

```text
/account
```

Khong redirect som sang dashboard vi Phase 2/3 moi co guard active profile va shell app.

### Empty/loading/error state

- `idle`: form editable
- `submitting`: khoa input + hien spinner
- `submitError`: hien message ngay tren form
- `success`: redirect, khong can success banner dung lai

### Definition of Done

- Login thanh cong luu session
- Login page redirect ve `/account`
- Sai password hien loi backend
- Da login truy cap `/auth/login` bi redirect ve `/account`

### Verify

- Test 1 account dung credential
- Test sai password
- Test email format sai o client
- Test F5 sau login, vao thang `/account`

## Task 1.3 - Tao register page

### Muc tieu

- Tao account moi bang contract backend that
- Khong bo sot `confirmPassword`

### File can tao

```text
AISAM-FE/src/app/auth/sign-up/page.tsx
AISAM-FE/src/features/auth/components/register-form.tsx
AISAM-FE/src/features/auth/schemas/register-schema.ts
```

### Request/response can bam

Request:

```ts
{
  email: string
  password: string
  confirmPassword: string
  fullName?: string
}
```

Luu y backend:

- `password` min 8
- `confirmPassword` phai khop `password`
- `fullName` toi da 255 ky tu

Success response:

```ts
ApiResponse<AuthSession>
```

### UI can co

- `fullName`
- `email`
- `password`
- `confirmPassword`
- submit button
- link quay ve login

### Validation frontend

- email required, format hop le
- password required, min 8
- confirmPassword required, phai khop password
- fullName optional, max 255

### Hanh vi submit

1. Goi `auth.register(values)`
2. Neu thanh cong:
   - session local duoc luu ngay
   - redirect sang `/auth/verify-email`
3. Neu loi:
   - duplicate email va invalid operation thuong ve `400`
   - hien `error.errorMessage` truoc

Ly do redirect sang `verify-email`:

- Backend register tra session ngay
- Backend cung co verify-email flow rieng
- Phase 1 chua onboarding profile, nen verify email la diem den hop ly hon dashboard

### Luu y thuc te voi backend

Viec da login khong dong nghia da verify email. UI can hien thong diep ro o `verify-email` page, khong assume user da verify.

### Definition of Done

- Register submit dung DTO
- Session duoc luu sau register
- Redirect ve `/auth/verify-email`
- Duplicate email va password mismatch hien dung loi

### Verify

- Tao account moi thanh cong
- Test `confirmPassword` sai
- Test password < 8
- Test fullName > 255 bi chan o client

## Task 1.4 - Tao forgot/reset password va verify email pages

### Muc tieu

- Hoan thien self-service auth truoc khi vao app shell
- Dung dung endpoint va query token flow cua backend

### File can tao

```text
AISAM-FE/src/app/auth/forgot-password/page.tsx
AISAM-FE/src/app/auth/update-password/page.tsx
AISAM-FE/src/app/auth/verify-email/page.tsx
AISAM-FE/src/features/auth/components/forgot-password-form.tsx
AISAM-FE/src/features/auth/components/reset-password-form.tsx
AISAM-FE/src/features/auth/components/verify-email-status.tsx
AISAM-FE/src/features/auth/api/forgot-password.ts
AISAM-FE/src/features/auth/api/reset-password.ts
AISAM-FE/src/features/auth/api/verify-email.ts
AISAM-FE/src/features/auth/api/resend-verify-email.ts
AISAM-FE/src/features/auth/schemas/forgot-password-schema.ts
AISAM-FE/src/features/auth/schemas/reset-password-schema.ts
```

### Phan A - Forgot password

Route:

```text
POST /api/Auth/forgot-password
```

Request:

```ts
{
  email: string
}
```

Hanh vi backend:

- Luon tra `200` message trung lap, ke ca khi email khong ton tai
- Frontend khong duoc co UI logic suy doan email ton tai hay khong

UI state:

- email input
- submit button
- success message o cung page

Thong diep frontend nen hien:

```text
If the email exists, a password reset link has been sent.
```

Khong can phan biet success/fail business theo email ton tai.

### Phan B - Reset/update password

Backend dang expose 2 route cung dung `ResetPasswordRequest`:

```text
POST /api/Auth/reset-password
POST /api/Auth/change-password-with-token
```

Khuyen nghi frontend Phase 1:

- Chon 1 route duy nhat cho UI `update-password`: `POST /api/Auth/change-password-with-token`
- Giu `POST /api/Auth/reset-password` trong endpoint map de tuong thich, nhung khong dung song song trong cung flow UI

Ly do:

- `change-password-with-token` dat ten gan voi route page `update-password`
- Giam confusion khi debug frontend

Request exact:

```ts
{
  email: string
  token: string
  newPassword: string
  confirmPassword: string
}
```

Token source:

- doc tu query string, vi du `?token=...&email=...`
- neu email khong co trong query, cho phep user nhap email thu cong

Validation frontend:

- email required
- token required
- newPassword min 8
- confirmPassword khop `newPassword`

Success behavior:

- hien success state
- CTA quay ve `/auth/login`

Failure behavior:

- `400`: hien `Invalid or expired reset token`
- `500`: hien generic server error

### Phan C - Verify email

Backend routes:

```text
GET  /api/Auth/verify-email?token=...
POST /api/Auth/verify-email/resend
```

Request resend:

```ts
{
  email: string
}
```

Flow verify page nen chia 2 mode:

#### Mode 1 - Co token trong URL

1. Page mount
2. Doc `token` tu query string
3. Auto goi `GET /auth/verify-email?token=<encoded>`
4. Hien state `verifying`
5. Sau do hien `success` hoac `error`

#### Mode 2 - Khong co token

1. Hien form nho nhap email
2. Goi `POST /auth/verify-email/resend`
3. Hien thong diep backend neutral:

```text
If the email exists and is not verified, a verification email has been sent.
```

### State machine nen co cho verify page

```ts
type VerifyState =
  | "idle"
  | "verifying"
  | "verified"
  | "verify-error"
  | "resending"
  | "resent"
  | "resend-error"
```

### Definition of Done

- Forgot password dung route `/auth/forgot-password`
- Reset password dung 1 route da chot, khong song song ca 2
- Verify email auto call khi co `token`
- Verify resend hien message neutral, khong leak thong tin email ton tai

### Verify

- Submit forgot password voi email bat ky
- Mo verify page co `?token=...`
- Test verify token sai
- Test resend verify email
- Test update password voi token hop le va token het han

## Task 1.5 - Tao account page va session management UI

### Muc tieu

- Co man hinh xac nhan user da dang nhap dung
- Hien thong tin account, sessions, change password, logout all

### File can tao

```text
AISAM-FE/src/app/account/page.tsx
AISAM-FE/src/features/auth/components/account-overview.tsx
AISAM-FE/src/features/auth/components/session-list.tsx
AISAM-FE/src/features/auth/components/change-password-form.tsx
AISAM-FE/src/features/auth/api/change-password.ts
AISAM-FE/src/features/auth/schemas/change-password-schema.ts
```

### Route backend can bam

```text
GET  /api/Auth/me
GET  /api/Auth/sessions
POST /api/Auth/logout-all
POST /api/Auth/change-password
POST /api/Auth/logout
```

### Rule access

- `/account` la protected page
- Chi can `Authorization`
- Khong can `X-Profile-Id`

Neu frontend foundation dang tu dong them `X-Profile-Id` khi storage co gia tri, van khong gay hai o `/auth/*`, nhung implementation tot hon la support option `skipProfileHeader: true` cho auth feature calls.

### Phan A - Account overview

`account-overview.tsx` hien toi thieu:

- email
- fullName
- role
- email verification status
- createdAt neu co tu `session.user`
- lastLoginAt neu co tu `session.user`

Luu y:

- `/auth/me` khong tra `isEmailVerified`, `createdAt`, `lastLoginAt`
- De hien du thong tin, page nen uu tien ket hop:
  - `session.user` tu login/register/refresh
  - `currentUser` tu `/auth/me` cho field real-time co ban

Khong fetch them endpoint nao khac chi de bo sung field.

### Phan B - Session list

`GET /auth/sessions` tra danh sach session active cua user.

`session-list.tsx` nen hien:

- `userAgent`
- `ipAddress`
- `createdAt`
- `expiresAt`
- `isActive`

Khuyen nghi:

- Neu co the xac dinh session hien tai qua token issue time hoac metadata thi danh dau "Current session"
- Neu khong xac dinh duoc, khong can co badge nay trong Phase 1

### Phan C - Change password

Request:

```ts
{
  currentPassword: string
  newPassword: string
  confirmPassword: string
}
```

Validation:

- currentPassword required
- newPassword min 8
- confirmPassword khop `newPassword`

Hanh vi sau submit thanh cong:

1. Backend tra message: `Password changed successfully. Please login again.`
2. Frontend hien success toast/banner
3. Goi `clearSession()`
4. Redirect ve `/auth/login`

Khong nen giu session cu sau khi doi mat khau vi backend da phat thong diep bat login lai.

### Phan D - Logout all sessions

Nut `Logout all sessions`:

1. Goi `POST /auth/logout-all`
2. Clear session local
3. Redirect `/auth/login`

Can them confirm dialog nhe de tranh bam nham.

### Phan E - Logout current session

Account page nen co nut logout thong thuong:

1. Goi `POST /auth/logout` voi body `{ refreshToken }`
2. Dung thanh cong hay fail van clear local session
3. Redirect `/auth/login`

### State can co

- page loading
- sessions loading
- sessions error
- change password submitting
- logout all submitting

Khong can lam page card qua nhieu. Day la trang operational, nen giu layout gon, de scan.

### Definition of Done

- Chua login vao `/account` bi redirect sang `/auth/login`
- Da login thay duoc thong tin user
- Load duoc session list
- Change password thanh cong thi bat login lai
- Logout all thanh cong thi clear session va redirect

### Verify

- Test `/account` khi khong co session
- Test `/account` sau login
- Test change password sai `currentPassword`
- Test change password dung
- Test logout all
- Test logout current session

## Verify tong Phase 1

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- `/auth/login` hoat dong
- `/auth/sign-up` hoat dong
- `/auth/forgot-password` hoat dong
- `/auth/update-password` xu ly duoc token flow
- `/auth/verify-email` xu ly duoc verify va resend
- `/account` la protected route
- session persist qua reload
- request auth protected co `Authorization`
- request auth feature khong phu thuoc `X-Profile-Id`
- refresh token flow khong lap vo han khi `401`

## Deliverable sau Phase 1

Can co it nhat:

```text
AISAM-FE/
  PHASE_1_IMPLEMENTATION.md
  src/
    app/
      auth/
        login/
        sign-up/
        forgot-password/
        update-password/
        verify-email/
      account/
    providers/
      auth-provider.tsx
    hooks/
      use-auth.ts
    lib/
      auth/
        session-storage.ts
        auth-guards.ts
    features/
      auth/
        api/
        components/
        schemas/
    types/
      auth.ts
```

## Risk can tranh trong Phase 1

- Dung `AuthSession.user` cho `/auth/me` response
- Gan logic `X-Profile-Id` vao auth pages
- Goi dong thoi ca `/auth/reset-password` va `/auth/change-password-with-token` cho cung 1 UI flow
- Khong clear session khi refresh fail
- Redirect sau login sang dashboard qua som khi chua co profile flow
- Quen `confirmPassword` trong register va change password
- Hien thong diep forgot/resend verify lam lo email co ton tai hay khong
- Giu session cu sau khi change password thanh cong

## Rule chuyen sang Phase 2

Chi bat dau Phase 2 khi:

- Phase 1 build pass
- Login/register/logout flow chay on dinh
- Forgot/reset/verify email pages hoat dong
- `/account` protected page hoat dong
- Session storage va auth provider san sang de profile module dung lai
