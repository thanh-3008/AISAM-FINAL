# US-54 - Seed du lieu demo duoc bao ve

## Mo ta

La quan tri vien, toi muon tao du lieu demo bang endpoint duoc gioi han de phuc vu demo va kiem thu.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `docs/main/requirements.md`: admin la doi tuong van hanh he thong, co nhu cau quan ly user/profile/payment/subscription va ho tro tinh huong van hanh.
- `docs/archive/plans/backend-code-plan.md`: Phase 9, Task 9.2 du kien migrate `AdminToolsController` o muc an toan, gom endpoint `seed-demo-user`, seed batch users neu can, va update payment/subscription/profile status neu can.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase Update F ghi ro backend hien tai thieu `AdminToolsController`, admin policy va `SeedDemoUserRequest`; seed endpoint khong nen expose trong production without guard.
- Active backend codebase `AISAM-BE`: da co `UserRoleEnum.Admin`, JWT role claim, `DevelopmentOnlyAttribute` va `EnvironmentAwareControllerFeatureProvider`; chua co `AdminToolsController`.

## Trang thai backend hien tai

Backend da co:

- `UserRoleEnum.Admin`
- JWT authentication va role claim
- `DevelopmentOnlyAttribute`
- `EnvironmentAwareControllerFeatureProvider`
- Pattern controller development-only qua `DevSchedulerController`

Backend chua co active:

- `AdminToolsController`
- `POST /api/admin-tools/seed-demo-user`
- `SeedDemoUserRequest`
- Admin policy ro rang
- Endpoint seed profile/brand/product/content/social demo

Ket luan: frontend co the implement admin demo seed UI dang guarded/planned state. Khong duoc goi endpoint seed trong production flow cho den khi backend Phase F / Task 9.2 active va duoc guard bang admin role hoac development-only.

## Muc tieu frontend

Tao man hinh admin tool de seed du lieu demo:

```text
/admin/tools/demo-seed
```

Trang nay cho phep admin:

- Tao demo user bang email/password.
- Chon loai du lieu demo can tao neu backend support.
- Xem ket qua seed: user, profile, brand, product, content, subscription/payment demo.
- Copy thong tin dang nhap demo sau khi seed thanh cong.
- Nhan biet khi backend seed API chua active.
- Khong hien tool nay cho non-admin.
- Khong hien tool nay trong production neu config frontend tat demo tools.

## User flow

1. Admin dang nhap qua US-51.
2. Admin vao `/admin/tools/demo-seed`.
3. Frontend route guard kiem tra role `Admin`.
4. Frontend kiem tra config cho phep hien demo seed tool.
5. Admin nhap email/password demo hoac dung default generated value.
6. Admin bam `Seed demo user`.
7. UI hien confirmation vi thao tac nay tao du lieu trong database.
8. Frontend goi seed API neu backend active.
9. Neu thanh cong, UI hien thong tin demo da tao va cac quick links de kiem thu.
10. Neu backend chua active, UI hien backend-not-ready state va khong crash.

## Frontend scope

Pages/components can implement:

```text
/admin/tools/demo-seed
AdminDemoSeedPage
DemoSeedForm
DemoSeedResultPanel
DemoSeedHistoryPanel optional
AdminRouteGuard
BackendNotReadyState
```

Neu admin sidebar da co:

```text
AdminSidebar -> Tools -> Demo Seed
```

Can them feature flag frontend:

```text
NEXT_PUBLIC_ENABLE_ADMIN_DEMO_TOOLS=true | false
```

hoac config tuong duong trong frontend hien tai.

## Backend API du kien

Theo `docs/archive/plans/backend-code-plan.md`, Task 9.2 du kien endpoint:

```http
POST /api/admin-tools/seed-demo-user
Authorization: Bearer <adminAccessToken>
```

Request MVP:

```json
{
  "email": "demo@example.com",
  "password": "Password@123"
}
```

Request mo rong nen ho tro neu backend lam:

```json
{
  "email": "demo@example.com",
  "password": "Password@123",
  "fullName": "Demo User",
  "createProfile": true,
  "createBrand": true,
  "createProduct": true,
  "createContent": true,
  "createSubscription": true,
  "subscriptionPlan": 2
}
```

Response envelope:

```ts
interface ApiResponse<T> {
  success: boolean
  message: string
  statusCode: number
  data: T
  errors?: unknown
}
```

Response data du kien:

```ts
interface DemoSeedResult {
  userId: string
  email: string
  password?: string
  profileId?: string
  brandId?: string
  productId?: string
  contentId?: string
  subscriptionId?: string
  paymentId?: string
  createdResources: Array<{
    type: "user" | "profile" | "brand" | "product" | "content" | "subscription" | "payment"
    id: string
    name?: string
  }>
  warnings?: string[]
}
```

## API status handling

Frontend can xu ly:

- `200`: seed thanh cong, hien result panel.
- `400`: request invalid, hien validation message.
- `401`: token thieu/het han, logout admin va redirect `/admin/login`.
- `403`: user khong co role admin hoac tool bi cam, hien forbidden.
- `404`: `AdminToolsController` chua active hoac endpoint bi an o production, hien backend-not-ready/disabled state.
- `409`: email demo da ton tai, hien option dung email khac hoac seed lai voi suffix moi.
- `500`: loi he thong, hien retry state.

Khi API tra `404`, UI hien:

```text
Demo seed API chua active trong backend hien tai.
```

Mo ta phu:

```text
Can hoan thanh backend Phase F / Task 9.2 va bat guard phu hop truoc khi dung tool nay.
```

## Business rules

- Chi role `Admin` duoc truy cap demo seed tool.
- Tool nay phai duoc an neu frontend config khong bat demo/admin tools.
- Khong can `X-Profile-Id` cho endpoint seed.
- Non-admin bi chan truoc khi goi API.
- Moi request seed phai co confirmation dialog.
- UI phai canh bao day la thao tac tao du lieu that trong database hien tai.
- Mat khau demo chi hien trong result neu backend tra ve tai thoi diem seed; khong luu password plaintext vao localStorage.
- Neu email da ton tai, frontend nen de xuat tao email moi bang timestamp.
- Khong cho seed hang loat trong production UI neu khong co guard rieng.
- Seed endpoint backend phai duoc guard bang admin role va/hoac `DevelopmentOnly`.
- Neu backend bat development-only, frontend production phai khong hien tool.

## UI requirements

### Demo seed form

Fields MVP:

- Email
- Password
- Confirm password

Fields optional/mo rong:

- Full name
- Create profile checkbox
- Create brand checkbox
- Create product checkbox
- Create content checkbox
- Create subscription checkbox
- Subscription plan select: Free, Plus, Premium, PlusTrial

Default values de tien demo:

```text
email: demo_<timestamp>@example.com
password: Password@123
```

### Confirmation dialog

Noi dung:

```text
Thao tac nay se tao du lieu demo trong database hien tai. Ban co chac chan muon tiep tuc?
```

Actions:

- Cancel
- Seed demo data

### Result panel

Hien sau khi seed thanh cong:

- Demo email
- Demo password neu backend tra ve
- User id
- Profile id neu co
- Brand id neu co
- Product id neu co
- Content id neu co
- Subscription id neu co
- Payment id neu co
- Quick links:
  - View admin user detail
  - Open user app login
  - View profile operations neu route co

### Backend not ready state

```text
Demo seed API chua active.
```

CTA phu:

```text
Kiem tra backend Phase F / Task 9.2.
```

### Disabled production state

Neu frontend config tat tool:

```text
Demo seed tool dang bi tat trong moi truong nay.
```

## Acceptance criteria

- Route `/admin/tools/demo-seed` chi cho admin.
- Non-admin khong thay form seed va khong goi API.
- Khi feature flag demo tools tat, UI khong hien form seed.
- Form validate email hop le.
- Form validate password va confirm password khop nhau.
- Submit phai hien confirmation dialog.
- Khi backend active va tra `200`, UI hien result panel voi resource da tao.
- Khi backend tra `409`, UI hien loi email da ton tai va cho generate email moi.
- Khi backend tra `401`, frontend redirect `/admin/login`.
- Khi backend tra `403`, frontend hien forbidden hoac clear admin session.
- Khi backend tra `404`, UI hien backend-not-ready state.
- Khi backend tra `500`, UI hien retry state.
- Password demo khong duoc luu vao localStorage/sessionStorage.
- Tool khong can va khong gui `X-Profile-Id`.

## Suggested frontend types

```ts
export interface SeedDemoUserRequest {
  email: string
  password: string
  fullName?: string
  createProfile?: boolean
  createBrand?: boolean
  createProduct?: boolean
  createContent?: boolean
  createSubscription?: boolean
  subscriptionPlan?: 0 | 1 | 2 | 3
}

export interface DemoSeedResource {
  type: "user" | "profile" | "brand" | "product" | "content" | "subscription" | "payment"
  id: string
  name?: string
}

export interface DemoSeedResult {
  userId: string
  email: string
  password?: string
  profileId?: string
  brandId?: string
  productId?: string
  contentId?: string
  subscriptionId?: string
  paymentId?: string
  createdResources: DemoSeedResource[]
  warnings?: string[]
}
```

## Suggested API client

```ts
export async function seedDemoUser(payload: SeedDemoUserRequest) {
  return fetchWithAdminAuth<ApiResponse<DemoSeedResult>>(
    "/admin-tools/seed-demo-user",
    {
      method: "POST",
      body: JSON.stringify(payload),
    }
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

if (process.env.NEXT_PUBLIC_ENABLE_ADMIN_DEMO_TOOLS !== "true") {
  return <DemoToolsDisabledState />
}
```

## Test cases frontend

- Admin vao `/admin/tools/demo-seed` khi feature flag bat thi thay form.
- Admin vao route khi feature flag tat thi thay disabled state.
- Non-admin vao route bi redirect hoac forbidden.
- Email invalid thi khong submit.
- Password va confirm password khong khop thi hien validation error.
- Submit hien confirmation dialog.
- Cancel confirmation khong goi API.
- Confirm goi `POST /api/admin-tools/seed-demo-user`.
- API `200` hien result panel.
- API `404` hien backend-not-ready state.
- API `409` hien loi email da ton tai.
- API `401` redirect `/admin/login`.
- API `403` hien forbidden.
- API `500` hien retry state.
- Password demo khong duoc luu vao browser storage.

## Dependencies / blockers

- Backend can hoan thanh Phase F / Task 9.2.
- Can expose `AdminToolsController` va `POST /api/admin-tools/seed-demo-user`.
- Can tao `SeedDemoUserRequest` trong backend.
- Can protect endpoint bang admin role va/hoac development-only.
- Can backend quyet dinh endpoint co duoc map trong production hay chi Development.
- Neu seed ca subscription/payment demo, backend can Phase E Payment/Subscription active hoac AdminTools tu tao entity co guard ro rang.
