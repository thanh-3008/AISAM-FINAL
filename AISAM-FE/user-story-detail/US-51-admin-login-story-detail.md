# US-51 - Dang nhap voi vai tro admin

## Mo ta

La quan tri vien, toi muon dang nhap bang tai khoan admin de truy cap cac chuc nang quan tri rieng.

## Trang thai backend hien tai

Backend da co:

- `AuthController`
- `AuthService`
- JWT authentication
- Role claim trong access token
- `GET /api/Auth/me` de lay user hien tai

Backend chua co:

- AdminController active
- Admin policy rieng
- Admin seed endpoint active

Role enum hien tai:

```ts
User = 0
Vendor = 1
Admin = 2
```

JWT claim role tra dang string, vi du: `"Admin"`.

## Muc tieu frontend

Frontend can tao flow dang nhap rieng cho admin, co the nam o admin app hoac route admin rieng:

```text
/admin/login
```

Sau khi dang nhap thanh cong:

- Neu tai khoan co role `Admin`, luu session va dieu huong vao admin dashboard.
- Neu tai khoan khong phai admin, khong cho vao admin system, xoa session vua nhan va hien loi phu hop.
- Neu sai email/password, hien loi tu backend.
- Neu token het han, dung refresh token neu app da co auth refresh flow; neu refresh fail thi quay lai admin login.

## Backend API su dung

### Login

```http
POST /api/Auth/login
```

Request:

```json
{
  "email": "admin@example.com",
  "password": "Password@123"
}
```

Response success envelope:

```ts
interface ApiResponse<T> {
  success: boolean
  message: string
  statusCode: number
  data: T
}
```

`data`:

```ts
interface AuthSession {
  accessToken: string
  refreshToken: string
  expiresAt: string
  tokenType: "Bearer"
  user: {
    id: string
    email: string
    fullName?: string
    role: 0 | 1 | 2 | "User" | "Vendor" | "Admin"
    isEmailVerified: boolean
    createdAt: string
    lastLoginAt?: string
  }
}
```

### Verify current user sau khi restore session

```http
GET /api/Auth/me
Authorization: Bearer <accessToken>
```

Response `data`:

```ts
{
  id: string
  email: string
  fullName?: string
  role: string
}
```

## Frontend scope

Pages/components de implement:

```text
/admin/login
/admin/dashboard
admin auth guard / route guard
admin auth context hoac reuse auth context hien co
api client login function
```

Neu frontend hien tai da co `AuthProvider`, nen reuse login API nhung tach rule redirect/role check cho admin.

## Business rules

- Admin login dung cung backend endpoint `/api/Auth/login`.
- Chi user co role `Admin` moi duoc vao admin routes.
- User role `User` hoac `Vendor` dang nhap thanh cong ve mat credential nhung phai bi tu choi vao admin.
- Admin session nen luu tach key voi user app neu frontend admin la app rieng, vi du:
  - `admin_auth_session`
  - `admin_access_token`
  - `admin_refresh_token`
- Khong can `X-Profile-Id` cho admin login.
- Khong hien admin dashboard truoc khi verify role thanh cong.

## UI states

### Default

- Email input.
- Password input.
- Submit button: `Dang nhap admin`.
- Link quay ve user app login neu can.

### Loading

- Disable form.
- Button hien loading state.

### Login failed

Backend co the tra `401` voi message:

```text
Invalid email or password
```

UI hien:

```text
Email hoac mat khau khong dung.
```

### Non-admin login

Neu login thanh cong nhung role khong phai admin:

- Xoa token/session vua luu.
- Hien loi:

```text
Tai khoan nay khong co quyen quan tri.
```

### Success

Neu role la admin:

- Luu session.
- Redirect:

```text
/admin/dashboard
```

## Acceptance criteria

- Admin co the dang nhap bang email/password hop le.
- Sau login, frontend kiem tra role trong `response.data.user.role`.
- Role `Admin` hoac numeric `2` duoc xem la admin.
- Non-admin khong duoc redirect vao admin dashboard.
- Non-admin session khong duoc giu lai trong admin app.
- Refresh page trong admin dashboard van giu dang nhap neu token con hop le.
- Khi goi `/api/Auth/me` tra role khong phai `Admin`, frontend logout va redirect ve `/admin/login`.
- Sai credential hien error ro rang, khong crash UI.
- Form validate email/password required truoc khi submit.

## Suggested implementation notes

Tao helper check role:

```ts
function isAdminRole(role: unknown): boolean {
  return role === "Admin" || role === 2
}
```

Login flow:

```ts
const response = await api.post<ApiResponse<AuthSession>>("/Auth/login", {
  email,
  password,
})

const session = response.data.data

if (!isAdminRole(session.user.role)) {
  clearAdminSession()
  throw new Error("Tai khoan nay khong co quyen quan tri.")
}

saveAdminSession(session)
router.push("/admin/dashboard")
```

Admin route guard:

```ts
if (!session) redirect("/admin/login")
if (!isAdminRole(session.user.role)) {
  clearAdminSession()
  redirect("/admin/login")
}
```

## Dependencies / blockers

- Backend chua co endpoint admin rieng, nen US-51 chi implement login va role gate tren frontend.
- Cac story admin tiep theo nhu xem danh sach user, quan ly payment/subscription can backend Phase F/Admin MVP.
- Can co san tai khoan admin trong database. Backend register hien tao role mac dinh `User`, nen admin account can duoc seed/update DB rieng cho den khi co Admin seed endpoint.
