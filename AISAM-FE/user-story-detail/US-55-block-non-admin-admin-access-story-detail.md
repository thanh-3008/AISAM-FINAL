# US-55 - Chan truy cap admin voi non-admin

## Mo ta

La he thong, toi muon enforce admin policy de nguoi dung thuong khong the dung endpoint quan tri.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `requirement.md`: muc `8.1 Security` yeu cau phan quyen theo role; muc `6.11 Admin Management` yeu cau admin tools rieng cho quan tri vien.
- `BACKEND_CODE_PLAN.md`: Phase 9 yeu cau admin APIs co authorization role; non-admin goi endpoint admin-only phai nhan `401/403`.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase Update F ghi ro backend hien tai co `UserRoleEnum.Admin` nhung thieu admin policy; Definition of Done yeu cau admin policy pass va non-admin blocked.
- Active backend codebase `AISAM-BE`: `AuthService` da dua role vao JWT claim, `AuthController.GetCurrentUser` tra role; `Program.cs` moi co `builder.Services.AddAuthorization()` chung, chua co policy `AdminOnly`.

## Trang thai backend hien tai

Backend da co:

- JWT authentication.
- `UserRoleEnum`:

```ts
User = 0
Vendor = 1
Admin = 2
```

- Role claim trong access token:

```csharp
new Claim(ClaimTypes.Role, user.Role.ToString())
```

- `GET /api/Auth/me` tra role hien tai:

```ts
{
  id: string
  email: string
  fullName?: string
  role: string
}
```

Backend chua co active:

- `AdminOnly` authorization policy.
- `[Authorize(Roles = "Admin")]` tren admin controllers.
- `UserController` admin APIs.
- `AdminToolsController`.
- Test non-admin bi chan khoi admin endpoint.

Ket luan: frontend can enforce admin route guard va API guard ngay. Tuy nhien security khong duoc dua vao frontend duy nhat; backend Phase F phai enforce policy that va tra `403` cho non-admin.

## Muc tieu frontend

Dam bao nguoi dung khong co role `Admin` khong the truy cap admin UI va khong the goi admin API tu frontend app.

Pham vi route admin:

```text
/admin
/admin/dashboard
/admin/users
/admin/users/{userId}
/admin/tools/demo-seed
/admin/profiles/{profileId}
/admin/payments
/admin/subscriptions
```

Frontend can:

- Kiem tra session admin truoc khi render admin pages.
- Kiem tra role `Admin` trong session.
- Verify lai role bang `/api/Auth/me` khi restore session.
- Clear admin session neu role khong hop le.
- Redirect non-admin ve `/admin/login` hoac hien forbidden page.
- Chan API client admin khong gui request neu session khong phai admin.
- Xu ly `401/403` tu backend mot cach nhat quan.

## User flow

### Case 1 - Chua dang nhap

1. User truy cap `/admin/users`.
2. Frontend khong thay admin session.
3. Redirect ve `/admin/login`.

### Case 2 - Dang nhap bang tai khoan user thuong

1. User thuong dang nhap vao admin login.
2. Backend `/api/Auth/login` tra token hop le nhung role la `User`.
3. Frontend kiem tra role.
4. Frontend khong luu admin session hoac clear session vua tao.
5. UI hien:

```text
Tai khoan nay khong co quyen quan tri.
```

### Case 3 - Session cu bi downgrade role

1. Admin session ton tai trong browser.
2. User mo `/admin/dashboard`.
3. Frontend restore session va goi `/api/Auth/me`.
4. Backend tra role khong phai `Admin`.
5. Frontend clear admin session va redirect `/admin/login`.

### Case 4 - Backend chan non-admin

1. Non-admin bang cach nao do goi admin endpoint.
2. Backend tra `403`.
3. Frontend clear admin session lien quan va hien forbidden/redirect.

## Frontend scope

Can implement/cap nhat:

```text
AdminAuthProvider
AdminRouteGuard
AdminLogin role check
fetchWithAdminAuth
Forbidden admin state
Unauthorized admin state
Session restore verification
Admin sidebar/menu visibility by role
```

Neu frontend admin la app rieng, nen dung storage key rieng:

```text
admin_auth_session
admin_access_token
admin_refresh_token
```

Khong dung chung voi user app session neu co the tranh.

## Backend policy contract can co

Backend Phase F nen them policy:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
```

Admin controllers nen dung:

```csharp
[Authorize(Policy = "AdminOnly")]
```

hoac:

```csharp
[Authorize(Roles = "Admin")]
```

Expected backend behavior:

- Thieu token: `401 Unauthorized`.
- Token invalid/expired: `401 Unauthorized`.
- Token hop le nhung role khong phai admin: `403 Forbidden`.
- Token admin hop le: cho phep request.

Admin endpoint du kien can enforce:

```text
GET  /api/users
POST /api/admin-tools/seed-demo-user
GET  /api/admin/users/{userId}/profiles
GET  /api/admin/profiles/{profileId}
PATCH /api/admin/profiles/{profileId}/status
PATCH /api/admin/subscriptions/{subscriptionId}
PATCH /api/admin/payments/{paymentId}/status
```

## Frontend role check

Backend login response co the tra enum numeric trong `user.role`, con `/api/Auth/me` tra claim string. Frontend can support ca hai:

```ts
function isAdminRole(role: unknown): boolean {
  return role === "Admin" || role === 2
}
```

Khong nen chap nhan:

```text
"admin"
"ADMIN"
true
1
```

tru khi backend contract thay doi ro rang.

## Admin API client rules

`fetchWithAdminAuth` can:

- Lay session tu admin storage.
- Kiem tra role bang `isAdminRole`.
- Neu khong phai admin, throw forbidden local error va khong goi network.
- Gan header:

```http
Authorization: Bearer <adminAccessToken>
```

- Khong gan `X-Profile-Id` cho admin endpoints.
- Xu ly response:
  - `401`: clear admin session, redirect `/admin/login`.
  - `403`: clear admin session hoac hien forbidden.
  - `404`: neu endpoint chua active, hien backend-not-ready state theo tung story.

## UI requirements

### Admin route loading state

Khi dang verify session:

```text
Dang kiem tra quyen truy cap...
```

### Unauthorized state

Khi chua login:

```text
Vui long dang nhap admin de tiep tuc.
```

Sau do redirect `/admin/login`.

### Forbidden state

Khi role khong phai admin:

```text
Tai khoan nay khong co quyen truy cap khu vuc quan tri.
```

CTA:

```text
Dang nhap bang tai khoan admin
```

### Admin navigation visibility

- Chi render admin sidebar/menu sau khi role da verify la `Admin`.
- Khong hien link admin trong user app cho non-admin.
- Neu user app co current user role admin, co the hien link "Admin" sau khi verify role.

## Business rules

- Frontend guard chi la UX/security layer phu; backend van bat buoc enforce policy.
- Non-admin khong duoc thay admin pages, admin menu, admin data.
- Non-admin khong duoc goi admin API tu frontend client.
- Admin session phai duoc clear khi `/api/Auth/me` tra role khong phai admin.
- Admin route khong can active profile.
- Admin API request khong gui `X-Profile-Id`.
- Khong trust role chi tu localStorage neu chua verify lai sau khi reload app.
- Neu refresh token tao access token moi voi role moi khong phai admin, phai logout admin.

## Acceptance criteria

- Chua login vao `/admin/*` thi redirect `/admin/login`.
- Login admin thanh cong thi vao duoc `/admin/dashboard`.
- Login non-admin vao admin login bi tu choi va khong luu admin session.
- Refresh admin route se goi `/api/Auth/me` hoac verify session tuong duong.
- Neu `/api/Auth/me` tra role khong phai `Admin`, frontend clear session va redirect.
- Admin menu khong render cho non-admin.
- `fetchWithAdminAuth` khong goi network neu local session khong phai admin.
- Admin API response `401` lam clear session va redirect login.
- Admin API response `403` hien forbidden hoac redirect login theo thiet ke.
- Admin API khong gui `X-Profile-Id`.
- Frontend support role `Admin` va numeric `2`.
- Frontend khong chap nhan role `User`, `Vendor`, `0`, `1` cho admin access.
- Khi backend Phase F active, non-admin goi admin endpoint phai bi chan boi backend voi `403`.

## Suggested frontend types

```ts
export interface AdminSession {
  accessToken: string
  refreshToken: string
  expiresAt: string
  tokenType: "Bearer"
  user: {
    id: string
    email: string
    fullName?: string
    role: 0 | 1 | 2 | "User" | "Vendor" | "Admin"
  }
}

export interface CurrentUserResponse {
  id: string
  email: string
  fullName?: string
  role: string
}
```

## Suggested implementation

### Role helper

```ts
export function isAdminRole(role: unknown): boolean {
  return role === "Admin" || role === 2
}
```

### Admin route guard

```ts
if (!adminSession) {
  redirect("/admin/login")
}

if (!isAdminRole(adminSession.user.role)) {
  clearAdminSession()
  redirect("/admin/login")
}
```

### Session restore verification

```ts
const currentUser = await getCurrentUser()

if (!isAdminRole(currentUser.role)) {
  clearAdminSession()
  redirect("/admin/login")
}
```

### Admin fetch guard

```ts
export async function fetchWithAdminAuth<T>(
  path: string,
  init?: RequestInit
): Promise<T> {
  const session = getAdminSession()

  if (!session || !isAdminRole(session.user.role)) {
    clearAdminSession()
    throw new Error("FORBIDDEN_ADMIN")
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      ...init?.headers,
      Authorization: `Bearer ${session.accessToken}`,
      "Content-Type": "application/json",
    },
  })

  if (response.status === 401) {
    clearAdminSession()
    throw new Error("UNAUTHORIZED")
  }

  if (response.status === 403) {
    clearAdminSession()
    throw new Error("FORBIDDEN_ADMIN")
  }

  return response.json()
}
```

## Test cases frontend

- Chua login vao `/admin/users` thi redirect `/admin/login`.
- User role `User` login qua admin form thi hien loi khong co quyen.
- User role `Vendor` login qua admin form thi hien loi khong co quyen.
- User role `Admin` login qua admin form thi vao dashboard.
- Session localStorage bi sua role thanh `User` thi route guard chan.
- `/api/Auth/me` tra role `User` thi clear admin session.
- Admin API tra `401` thi redirect login.
- Admin API tra `403` thi hien forbidden va clear session.
- `fetchWithAdminAuth` khong gui `X-Profile-Id`.
- Admin sidebar khong render cho non-admin.
- Admin sidebar render cho admin sau khi verify role.

## Dependencies / blockers

- Backend can hoan thanh Phase F de co admin controllers active.
- Backend can them policy `AdminOnly` hoac `[Authorize(Roles = "Admin")]`.
- Backend can test non-admin request bi `403`.
- Can co admin account de test end-to-end.
- Frontend khong the thay the backend authorization; neu backend chua enforce policy, story nay chi dam bao UI guard, chua dam bao security day du.
