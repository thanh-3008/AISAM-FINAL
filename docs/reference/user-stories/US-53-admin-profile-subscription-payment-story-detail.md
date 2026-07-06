# US-53 - Quan ly profile, subscription va payment trong admin

> Migration notice: Admin target se quan ly Workspace subscription/payment, Archived lifecycle va Soft Delete. Profile chi con la thong tin ca nhan/doanh nghiep.

## Mo ta

La quan tri vien, toi muon xem va cap nhat mot so du lieu profile, subscription va payment de ho tro van hanh demo.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `docs/main/requirements.md`: muc `6.11 Admin Management` yeu cau admin xem chi tiet user, profile, subscription va payment; tim kiem/loc user theo trang thai va goi dang ky; kich hoat/vo hieu hoa tai khoan.
- `docs/archive/plans/backend-code-plan.md`: Phase 8 du kien migrate Payment/Subscription/Quota; Phase 9 du kien migrate Admin backend MVP va `AdminToolsController` an toan.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase E ghi ro payment/subscription chua active; Phase F ghi ro admin tools chua active.
- Active backend codebase `AISAM-BE`: da co entity/schema cho `Profile`, `Subscription`, `Payment`; da co user-owned Profile APIs; chua co admin APIs cho profile/subscription/payment.

## Trang thai backend hien tai

Backend da co:

- `Profile` entity
- `Subscription` entity
- `Payment` entity
- `ProfileStatusEnum`
- `SubscriptionPlanEnum`
- `PaymentStatusEnum`
- `ProfileController` cho user-owned profile CRUD
- `ProfileResponseDto`
- DbSet/migration cho `profiles`, `subscriptions`, `payments`

Backend chua co active:

- `SubscriptionValidationService`
- `AdminToolsController`
- Admin endpoint xem/cap nhat profile cua user bat ky
- Admin endpoint xem/cap nhat subscription/payment
- Admin authorization policy ro rang

Backend da co active sau Phase E:
- `PaymentController` (POST /api/payment/checkout, POST /api/payment/callback, POST /api/payment/webhook, GET /api/payment/history, GET /api/payment/subscription/current)
- `PayOSPaymentService`
- `PaymentRepository`, `SubscriptionRepository`
- `Subscription` entity, `Payment` entity, `PaymentStatusEnum`, `SubscriptionPlanEnum`
- User-owned Profile APIs (`ProfileController`)

Ket luan: frontend co the dung story nay de implement khung admin UI, route guard, type, empty/error state va planned/backend-not-ready state. Cac action cap nhat profile/subscription/payment chi duoc bat khi backend Phase F admin endpoints active.

## Muc tieu frontend

Tao man hinh admin ho tro van hanh demo cho profile, subscription va payment:

```text
/admin/users/{userId}
/admin/users/{userId}/profiles
/admin/profiles/{profileId}
```

Man hinh can cho admin:

- Xem danh sach profile cua mot user.
- Xem chi tiet profile.
- Xem subscription gan voi profile.
- Xem payment history lien quan den user/profile/subscription.
- Cap nhat mot so trang thai van hanh demo khi backend support:
  - Profile status.
  - Subscription plan/isActive/endDate.
  - Payment status.
- Khong hien/cap nhat thong tin nhay cam nhu password hash, token, secret, PayOS secret.

## User flow

1. Admin dang nhap qua US-51.
2. Admin vao danh sach user qua US-52.
3. Admin chon mot user de xem chi tiet.
4. Frontend goi admin APIs de lay profile/subscription/payment neu backend da active.
5. UI hien cac tab:
   - Profiles
   - Subscription
   - Payments
6. Admin xem thong tin va chon action cap nhat neu duoc backend support.
7. Sau khi cap nhat thanh cong, UI refresh lai du lieu va hien audit-friendly success message.
8. Neu backend chua active, UI hien backend-not-ready state va disable action ghi du lieu.

## Frontend scope

Pages/components can implement:

```text
/admin/users/[userId]
/admin/users/[userId]/profiles
/admin/profiles/[profileId]
AdminUserDetailPage
AdminProfilesTable
AdminProfileDetailPanel
AdminSubscriptionPanel
AdminPaymentHistoryTable
AdminProfileStatusForm
AdminSubscriptionUpdateForm
AdminPaymentStatusForm
AdminRouteGuard
```

Neu frontend admin chua co nested route, co the bat dau voi mot route tong hop:

```text
/admin/operations
```

nhung can giu type/API client tach rieng de sau nay map sang route chi tiet.

## Data model backend hien co

### Profile

```ts
interface AdminProfile {
  id: string
  userId: string
  name: string
  profileType: number
  subscriptionId?: string
  companyName?: string
  bio?: string
  avatarUrl?: string
  status: 0 | 1 | 2 | 3
  createdAt: string
  updatedAt: string
}
```

`ProfileStatusEnum`:

```ts
Pending = 0
Active = 1
Suspended = 2
Cancelled = 3
```

### Subscription

```ts
interface AdminSubscription {
  id: string
  profileId: string
  plan: 0 | 1 | 2 | 3
  quotaPostsPerMonth: number
  quotaAIContentPerDay: number
  quotaAIImagesPerDay: number
  quotaPlatforms: number
  quotaAccounts: number
  analysisLevel: number
  quotaAdBudgetMonthly: number
  quotaAdCampaigns: number
  startDate: string
  endDate?: string
  isActive: boolean
  isDeleted: boolean
  createdAt: string
  updatedAt: string
  payOSOrderCode?: string
  payOSPaymentLinkId?: string
}
```

`SubscriptionPlanEnum`:

```ts
Free = 0
Plus = 1
Premium = 2
PlusTrial = 3
```

### Payment

```ts
interface AdminPayment {
  id: string
  userId: string
  subscriptionId?: string
  amount: number
  currency: string
  status: 0 | 1 | 2 | 3
  paymentMethod?: string
  transactionId?: string
  invoiceUrl?: string
  isDeleted: boolean
  createdAt: string
}
```

`PaymentStatusEnum`:

```ts
Pending = 0
Success = 1
Failed = 2
Refunded = 3
```

## Backend API du kien

Backend hien tai chua expose cac endpoint duoi day. Day la contract de frontend chuan bi va de backend Phase E/F implement tuong ung.

### Lay profile cua user trong admin

```http
GET /api/admin/users/{userId}/profiles
Authorization: Bearer <adminAccessToken>
```

Response:

```ts
ApiResponse<AdminProfile[]>
```

### Lay chi tiet profile van hanh

```http
GET /api/admin/profiles/{profileId}
Authorization: Bearer <adminAccessToken>
```

Response:

```ts
ApiResponse<{
  profile: AdminProfile
  subscription?: AdminSubscription
  recentPayments: AdminPayment[]
}>
```

### Cap nhat profile status

```http
PATCH /api/admin/profiles/{profileId}/status
Authorization: Bearer <adminAccessToken>
```

Request:

```json
{
  "status": 1,
  "reason": "Activated for demo support"
}
```

Response:

```ts
ApiResponse<AdminProfile>
```

### Lay subscription cua profile

```http
GET /api/admin/profiles/{profileId}/subscription
Authorization: Bearer <adminAccessToken>
```

Response:

```ts
ApiResponse<AdminSubscription | null>
```

### Cap nhat subscription demo

```http
PATCH /api/admin/subscriptions/{subscriptionId}
Authorization: Bearer <adminAccessToken>
```

Request:

```json
{
  "plan": 2,
  "isActive": true,
  "endDate": "2026-07-03",
  "reason": "Demo upgrade"
}
```

Response:

```ts
ApiResponse<AdminSubscription>
```

### Lay payment history

```http
GET /api/admin/users/{userId}/payments?page=1&pageSize=10&status=1
Authorization: Bearer <adminAccessToken>
```

hoac theo profile:

```http
GET /api/admin/profiles/{profileId}/payments?page=1&pageSize=10&status=1
Authorization: Bearer <adminAccessToken>
```

Response:

```ts
ApiResponse<PagedResult<AdminPayment>>
```

### Cap nhat payment status cho demo

```http
PATCH /api/admin/payments/{paymentId}/status
Authorization: Bearer <adminAccessToken>
```

Request:

```json
{
  "status": 1,
  "reason": "Manual confirmation for demo"
}
```

Response:

```ts
ApiResponse<AdminPayment>
```

## API status handling

Frontend can xu ly:

- `200`: render du lieu.
- `400`: request invalid, hien validation message.
- `401`: token thieu/het han, logout admin va redirect `/admin/login`.
- `403`: khong co role admin, clear admin session va hien forbidden/redirect.
- `404`: endpoint admin/payment/subscription chua active hoac resource khong ton tai.
- `409`: conflict trang thai, vi du payment da refunded khong duoc mark success.
- `500`: loi he thong, hien retry state.

Khi backend chua active va tra `404`, UI phai hien:

```text
Admin operations API chua active trong backend hien tai.
```

va disable cac form cap nhat.

## Business rules

- Chi role `Admin` duoc truy cap cac route trong story nay.
- Trang admin operations khong can `X-Profile-Id`.
- Non-admin bi chan truoc khi goi API.
- Cac action cap nhat du lieu phai co confirmation dialog.
- Moi action cap nhat nen co field `reason` de phuc vu demo/audit sau nay.
- Khong cho frontend sua truc tiep amount/transactionId/invoiceUrl neu backend khong support.
- Khong hien field bao mat cua user, token, password hash/salt, reset token, verification token.
- Payment status update chi dung cho van hanh demo; payment production phai dong bo qua PayOS callback/webhook.
- Neu subscription dang inactive hoac het han, UI can hien canh bao ro tren profile detail.
- Neu profile status la `Cancelled`, UI khong nen hien action publish/operation nhu profile active.

## UI requirements

### Admin user detail overview

Thong tin toi thieu:

- User id
- Email
- Created at
- Profiles count
- Payments count neu backend support

### Profiles tab

Cot toi thieu:

- Profile name
- Company name
- Status
- Subscription id
- Created at
- Updated at
- Actions

Action:

- View profile detail
- Change status, disabled neu backend chua active

### Subscription panel

Thong tin toi thieu:

- Plan
- Active/inactive
- Start date
- End date
- Quotas:
  - posts/month
  - AI content/day
  - AI images/day
  - platforms
  - accounts
- PayOS order code/payment link id neu co

Action:

- Change plan, disabled neu backend chua active
- Activate/deactivate, disabled neu backend chua active
- Extend end date, disabled neu backend chua active

### Payments tab

Cot toi thieu:

- Payment id
- Amount
- Currency
- Status
- Payment method
- Transaction id
- Created at
- Invoice link
- Actions

Action:

- View invoice if `invoiceUrl` exists
- Change status for demo, disabled neu backend chua active

### Empty states

Khong co profile:

```text
Nguoi dung nay chua co profile.
```

Khong co subscription:

```text
Profile nay chua co subscription.
```

Khong co payment:

```text
Chua co giao dich thanh toan.
```

### Backend not ready state

```text
Backend admin operations API chua active.
```

Mo ta phu:

```text
Can hoan thanh backend Phase E va Phase F truoc khi bat chuc nang cap nhat profile, subscription va payment.
```

## Acceptance criteria

- Route admin operations chi cho role `Admin`.
- Non-admin khong thay du lieu profile/subscription/payment cua user khac.
- Khi backend API chua active, page hien backend-not-ready state va khong crash.
- UI co tab/panel rieng cho Profiles, Subscription va Payments.
- Profile status duoc map label dung:
  - `Pending`
  - `Active`
  - `Suspended`
  - `Cancelled`
- Subscription plan duoc map label dung:
  - `Free`
  - `Plus`
  - `Premium`
  - `PlusTrial`
- Payment status duoc map label dung:
  - `Pending`
  - `Success`
  - `Failed`
  - `Refunded`
- Cac form update phai disable khi API chua active.
- Cac action update phai co confirmation dialog.
- Sau update thanh cong, UI refresh du lieu moi.
- Loi `401` redirect ve `/admin/login`.
- Loi `403` clear admin session hoac hien forbidden.
- Loi `404` resource not found khac voi backend-not-ready neu API da active nhung item khong ton tai.
- Khong hien field nhay cam lien quan password/token/secret.

## Suggested frontend types

```ts
export type ProfileStatus = 0 | 1 | 2 | 3
export type SubscriptionPlan = 0 | 1 | 2 | 3
export type PaymentStatus = 0 | 1 | 2 | 3

export interface AdminProfile {
  id: string
  userId: string
  name: string
  profileType: number
  subscriptionId?: string
  companyName?: string
  bio?: string
  avatarUrl?: string
  status: ProfileStatus
  createdAt: string
  updatedAt: string
}

export interface AdminSubscription {
  id: string
  profileId: string
  plan: SubscriptionPlan
  quotaPostsPerMonth: number
  quotaAIContentPerDay: number
  quotaAIImagesPerDay: number
  quotaPlatforms: number
  quotaAccounts: number
  analysisLevel: number
  quotaAdBudgetMonthly: number
  quotaAdCampaigns: number
  startDate: string
  endDate?: string
  isActive: boolean
  isDeleted: boolean
  createdAt: string
  updatedAt: string
  payOSOrderCode?: string
  payOSPaymentLinkId?: string
}

export interface AdminPayment {
  id: string
  userId: string
  subscriptionId?: string
  amount: number
  currency: string
  status: PaymentStatus
  paymentMethod?: string
  transactionId?: string
  invoiceUrl?: string
  isDeleted: boolean
  createdAt: string
}
```

## Suggested API client methods

```ts
export async function getAdminUserProfiles(userId: string) {
  return fetchWithAdminAuth<ApiResponse<AdminProfile[]>>(
    `/admin/users/${userId}/profiles`
  )
}

export async function getAdminProfileOperations(profileId: string) {
  return fetchWithAdminAuth<ApiResponse<{
    profile: AdminProfile
    subscription?: AdminSubscription
    recentPayments: AdminPayment[]
  }>>(`/admin/profiles/${profileId}`)
}

export async function updateAdminProfileStatus(
  profileId: string,
  payload: { status: ProfileStatus; reason: string }
) {
  return fetchWithAdminAuth<ApiResponse<AdminProfile>>(
    `/admin/profiles/${profileId}/status`,
    { method: "PATCH", body: JSON.stringify(payload) }
  )
}

export async function updateAdminSubscription(
  subscriptionId: string,
  payload: {
    plan?: SubscriptionPlan
    isActive?: boolean
    endDate?: string
    reason: string
  }
) {
  return fetchWithAdminAuth<ApiResponse<AdminSubscription>>(
    `/admin/subscriptions/${subscriptionId}`,
    { method: "PATCH", body: JSON.stringify(payload) }
  )
}

export async function updateAdminPaymentStatus(
  paymentId: string,
  payload: { status: PaymentStatus; reason: string }
) {
  return fetchWithAdminAuth<ApiResponse<AdminPayment>>(
    `/admin/payments/${paymentId}/status`,
    { method: "PATCH", body: JSON.stringify(payload) }
  )
}
```

## Test cases frontend

- Admin vao user detail va thay tabs Profiles/Subscription/Payments.
- Non-admin vao route bi redirect hoac forbidden.
- API profile operations tra `404` do backend chua active thi hien backend-not-ready state.
- Profile list rong hien empty state.
- Subscription null hien "Profile nay chua co subscription".
- Payment list rong hien "Chua co giao dich thanh toan".
- Enum status/plan/payment render dung label.
- Update action disabled khi backend chua active.
- Update action active thi bat confirmation dialog va yeu cau reason.
- Update thanh cong refresh data.
- `401` redirect `/admin/login`.
- `403` clear admin session.
- `500` hien retry state.

## Dependencies / blockers

- Can backend Phase E hoan thanh Payment/Subscription repositories, service va `PaymentController`.
- Can backend Phase F hoan thanh Admin MVP, admin policy va endpoint admin operations.
- Can backend quyet dinh route chinh thuc cho admin profile/subscription/payment operations.
- Can admin account de test end-to-end.
- Payment status update trong demo can duoc backend guard chat, tranh lam sai luong PayOS production.
- Filtering/search nang cao theo subscription/payment status chua co active API, nen UI chi nen lam placeholder/disabled cho den khi backend support.
