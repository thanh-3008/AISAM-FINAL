# US-52 - Xem danh sach nguoi dung trong admin

## Mo ta

La quan tri vien, toi muon xem danh sach nguoi dung de quan ly van hanh he thong.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `requirement.md`: muc `6.11 Admin Management` yeu cau admin quan ly danh sach user, tim kiem/loc user theo trang thai va goi dang ky.
- `BACKEND_CODE_PLAN.md`: Phase 9, Task 9.1 du kien migrate `UserController` admin/user list APIs.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase Update F ghi ro backend hien tai co `UserRepository.GetPagedUsersAsync` va `UserRoleEnum.Admin`, nhung thieu `UserController`, `UserService`, `AdminToolsController`, admin policy.
- Active backend codebase `AISAM-BE`: da co repository list user, chua expose API endpoint admin user list.

## Trang thai backend hien tai

Backend da co:

- `UserRoleEnum.Admin`
- JWT role claim tu `AuthService`
- `IUserRepository.GetPagedUsersAsync(PaginationRequest request)`
- `UserRepository.GetPagedUsersAsync`
- `UserListDto`
- `PaginationRequest`
- `PagedResult<T>`

Backend chua co active:

- `UserController`
- `IUserService`
- `UserService`
- `GET /api/users`
- Admin authorization policy ro rang
- Endpoint filter user theo role/status/subscription

Ket luan: frontend co the implement layout, route guard, state model va API client theo contract du kien, nhung khong duoc goi API that trong production flow cho den khi backend Phase F hoan thanh.

## Muc tieu frontend

Tao trang admin xem danh sach nguoi dung cho quan tri vien:

```text
/admin/users
```

Trang nay cho phep admin:

- Xem danh sach user theo dang bang.
- Tim kiem user theo email.
- Phan trang danh sach user.
- Sap xep theo email hoac ngay tao.
- Xem thong tin tom tat: email, ngay tao, so social accounts dang ket noi.
- Chuan bi dieu huong sang trang chi tiet user khi backend co endpoint detail.

## User flow

1. Admin dang nhap thanh cong qua US-51.
2. Admin vao `/admin/users`.
3. Frontend guard kiem tra session va role `Admin`.
4. Neu backend user list API da active, frontend goi API lay danh sach.
5. UI hien loading trong luc fetch.
6. UI hien bang danh sach user khi thanh cong.
7. Admin co the search, sort, chuyen trang.
8. Neu backend chua active hoac tra 404, UI hien trang thai "Backend admin API chua active" thay vi crash.

## Frontend scope

Pages/components can implement:

```text
/admin/users
admin users table
admin users filters/search bar
pagination controls
admin auth guard
admin API client method
empty/loading/error states
```

Neu co admin layout rieng:

```text
/admin/dashboard
/admin/users
AdminSidebar
AdminHeader
AdminRouteGuard
```

## Backend API du kien

Theo `BACKEND_CODE_PLAN.md`, Task 9.1 du kien endpoint:

```http
GET /api/users
Authorization: Bearer <adminAccessToken>
```

Query parameters du kien theo `PaginationRequest` backend hien co:

```text
page=1
pageSize=10
searchTerm=abc@example.com
sortBy=email | createdAt
sortDescending=true | false
```

Request mau:

```http
GET /api/users?page=1&pageSize=10&searchTerm=admin&sortBy=createdAt&sortDescending=true
Authorization: Bearer <adminAccessToken>
```

Response envelope du kien:

```ts
interface ApiResponse<T> {
  success: boolean
  message: string
  statusCode: number
  data: T
  errors?: unknown
}
```

`data` du kien:

```ts
interface PagedResult<T> {
  data: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}
```

`UserListDto` active trong backend hien tai:

```ts
interface UserListItem {
  id: string
  email: string
  createdAt: string
  socialAccountsCount: number
}
```

## API status handling

Vi backend hien tai chua co `/api/users`, frontend can xu ly cac case:

- `200`: render danh sach user.
- `401`: token thieu/het han, logout admin va redirect `/admin/login`.
- `403`: user khong co role admin, redirect `/admin/login` hoac hien forbidden page.
- `404`: backend admin user list API chua active, hien planned/backend-not-ready state.
- `500`: hien loi he thong va retry action.

## Business rules

- Chi user co role `Admin` moi duoc vao `/admin/users`.
- Non-admin bi chan o frontend route guard, khong goi API admin.
- Neu backend tra `403`, frontend phai xoa admin session va chan truy cap.
- Trang user list khong can `X-Profile-Id`.
- Search chi gui request sau khi user submit hoac debounce hop ly.
- Pagination phai lay theo server, khong phan trang client-side tren du lieu da fetch.
- Khong hien thong tin nhay cam nhu password hash, password salt, reset token, verification token.
- Khi backend chua co endpoint detail, action "view detail" nen disabled hoac route placeholder.

## UI requirements

### Bang danh sach

Cot toi thieu:

- Email
- Created at
- Social accounts count
- Actions

Cot co the bo sung sau khi backend support:

- Full name
- Role
- Email verification status
- Last login
- Subscription plan
- Account status

### Search/filter

MVP theo backend hien co:

- Search by email qua `searchTerm`
- Sort by `email`
- Sort by `createdAt`
- Toggle sort descending
- Page size selector: `10`, `20`, `50`

Planned sau khi backend support:

- Filter by role
- Filter by account status
- Filter by subscription plan
- Filter by email verified

### Empty state

Khi khong co user:

```text
Chua co nguoi dung nao.
```

Khi search khong co ket qua:

```text
Khong tim thay nguoi dung phu hop.
```

### Backend not ready state

Khi API tra `404`:

```text
Admin user API chua active trong backend hien tai.
```

CTA phu hop:

```text
Kiem tra backend Phase F / Task 9.1.
```

### Error state

Khi API loi:

```text
Khong the tai danh sach nguoi dung. Vui long thu lai.
```

Co nut retry.

## Acceptance criteria

- `/admin/users` chi truy cap duoc khi co admin session hop le.
- Non-admin khong thay noi dung danh sach user.
- Frontend khong goi `/api/users` neu user hien tai khong phai admin.
- Khi API thanh cong, bang hien dung `email`, `createdAt`, `socialAccountsCount`.
- Search email gui query `searchTerm`.
- Sort theo email va createdAt gui dung `sortBy`.
- Pagination gui dung `page` va `pageSize`, render `totalCount`, `totalPages`, next/previous state.
- Loading state hien trong luc fetch data.
- Empty state hien khi API tra list rong.
- `401` redirect ve `/admin/login`.
- `403` hien forbidden hoac redirect login va clear admin session.
- `404` hien backend-not-ready state, khong crash page.
- Khong hien cac field bao mat hoac token cua user.

## Suggested frontend types

```ts
export interface AdminUserListItem {
  id: string
  email: string
  createdAt: string
  socialAccountsCount: number
}

export interface PagedResult<T> {
  data: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface AdminUsersQuery {
  page: number
  pageSize: number
  searchTerm?: string
  sortBy?: "email" | "createdAt"
  sortDescending?: boolean
}
```

## Suggested API client

```ts
export async function getAdminUsers(query: AdminUsersQuery) {
  const params = new URLSearchParams()

  params.set("page", String(query.page))
  params.set("pageSize", String(query.pageSize))

  if (query.searchTerm) {
    params.set("searchTerm", query.searchTerm)
  }

  if (query.sortBy) {
    params.set("sortBy", query.sortBy)
  }

  if (typeof query.sortDescending === "boolean") {
    params.set("sortDescending", String(query.sortDescending))
  }

  return fetchWithAdminAuth<ApiResponse<PagedResult<AdminUserListItem>>>(
    `/users?${params.toString()}`
  )
}
```

## Suggested route guard

```ts
function isAdminRole(role: unknown): boolean {
  return role === "Admin" || role === 2
}

if (!session) {
  redirect("/admin/login")
}

if (!isAdminRole(session.user.role)) {
  clearAdminSession()
  redirect("/admin/login")
}
```

## Test cases frontend

- Admin vao `/admin/users` va thay loading roi thay danh sach user.
- Admin search email va URL/API query co `searchTerm`.
- Admin doi sort createdAt/email va request co `sortBy`.
- Admin chuyen page va request co `page`.
- Non-admin vao `/admin/users` bi redirect.
- Token expired tra `401` thi redirect login.
- Backend tra `403` thi clear session.
- Backend tra `404` thi hien backend-not-ready state.
- Empty response render empty state.
- API error render retry state.

## Dependencies / blockers

- Backend can hoan thanh Phase F / Task 9.1 truoc khi frontend goi API that.
- Can expose `GET /api/users` va protect bang admin role.
- Can co admin account de test end-to-end.
- `UserListDto` hien tai chi co `id`, `email`, `createdAt`, `socialAccountsCount`; cac cot khac can backend bo sung DTO neu muon hien thi.
- Filtering theo status/subscription trong requirement chua co active backend, nen frontend chi nen lam placeholder/disabled cho den khi API support.
