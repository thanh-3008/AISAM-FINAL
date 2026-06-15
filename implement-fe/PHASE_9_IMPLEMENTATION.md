# Phase 9 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task Phase 9 trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu voi `AISAM-BE` hien tai va chot theo target product trong [README.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/README.md>) va [requirement.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/requirement.md>).

Day la phase `User App`.

Ban chat cua Phase 9:

- Day la module target product bat buoc co trong frontend
- Nhung backend repo hien tai chua expose ro user-facing contract day du cho subscription/payment/quota
- Vi vay Phase 9 phai tach ro `backend-ready`, `backend-partial`, `backend-missing`

Pham vi Phase 9:

- Tao pricing, subscription overview, payment history, quota overview pages cho user app
- Chuan bi checkout/upgrade/cancel/renew flow seam theo target product
- Dung lai profile context, dashboard shell, notification pattern da co
- Khong bịa API contract neu backend hien tai chua expose ro

Khong lam trong Phase 9:

- Implement PayOS flow that bang endpoint tu suy dien
- Tu y noi thang den webhook/callback backend ma repo hien tai chua expose ro
- Gop admin subscription/payment pages vao user app
- Xu ly Ads/Reports/Admin

Can cu target product can ton tai:

- `README.md`: profile, subscription, PayOS payment flow, payment history, active subscription, change/cancel plan
- `requirement.md`: pricing, payment, quota, auto-renew policy, AI/publish quota, payment history, block action khi het quota

Can cu backend da doi chieu truc tiep cho Phase 9:

- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/AISAM.API/Controllers/ProfileController.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ProfileResponseDto.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/UserResponseDto.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`
- `AISAM-BE/AISAM.Data/Model/Profile.cs`
- `AISAM-BE/AISAM.Data/Model/Subscription.cs`
- `AISAM-BE/AISAM.Data/Model/Payment.cs`
- `AISAM-BE/AISAM.Data/Enumeration/SubscriptionPlanEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/PaymentStatusEnum.cs`

Can cu backend quan trong can noi thang:

- Repo hien tai co model `Subscription`, `Payment`, `Profile.SubscriptionId`
- Repo hien tai khong cho thay user-facing controller active ro rang cho:
  - pricing list
  - current subscription detail
  - create PayOS checkout link
  - confirm/cancel/renew subscription
  - payment history
  - quota usage summary

Ket luan:

- Phase 9 la `target product, backend-dependent`
- FE van phai co route, state, UX flow, types seam, guard va pending integration points
- FE khong duoc hardcode endpoint `/api/payments/*` hay `/api/subscriptions/*` neu repo hien tai chua co controller contract that

## Tong quan thu tu lam

1. Task 9.1 - Tao subscription/pricing information architecture
2. Task 9.2 - Tao current subscription va billing overview page
3. Task 9.3 - Tao pricing / upgrade / checkout seam
4. Task 9.4 - Tao payment history va billing states
5. Task 9.5 - Tao quota overview va action guards
6. Task 9.6 - Tao backend-dependent callback/result states va docs verify
7. Chay verify tong the Phase 9

## Chot scope nghiep vu truoc khi code

Theo target product, frontend user phai ho tro toi thieu:

- Xem plan hien tai
- Xem thong tin pricing/goi
- Biet profile hien dang co subscription hay khong
- Biet quota con lai/da dung o muc co ban
- Tao flow nang cap / doi goi / huy goi
- Xem lich su giao dich
- Thay ro action nao bi chan khi quota khong du

Theo backend repo hien tai, frontend co the khang dinh truc tiep:

- `ProfileResponseDto` co `subscriptionId`
- `SubscriptionPlanEnum` gom:
  - `Free = 0`
  - `Plus = 1`
  - `Premium = 2`
  - `PlusTrial = 3`
- `PaymentStatusEnum` gom:
  - `Pending = 0`
  - `Success = 1`
  - `Failed = 2`
  - `Refunded = 3`
- Model `Subscription` co cac quota field:
  - `quotaPostsPerMonth`
  - `quotaAIContentPerDay`
  - `quotaAIImagesPerDay`
  - `quotaPlatforms`
  - `quotaAccounts`
  - `quotaAdBudgetMonthly`
  - `quotaAdCampaigns`
- Model `Subscription` co PayOS-related fields:
  - `payOSOrderCode`
  - `payOSPaymentLinkId`

Frontend khong duoc suy dien them:

- exact checkout request body
- exact payment history response
- exact quota usage counters da dung / con lai
- exact cancel/renew endpoint
- exact confirm payment callback contract

## Header va auth context can biet

### Rule hien tai chac chan dung

Tat ca payment/subscription pages deu phai la route da login.

Ly do:

- subscription/payment la user account data
- profile co `subscriptionId`
- pricing page co the mo cho guest, nhung checkout/billing page phai can login

### Rule active profile

Can tach 2 nhom:

1. Route account-level:
   - pricing
   - payment history
   - checkout result
   - billing settings

2. Route profile-context:
   - current workspace subscription overview
   - quota overview
   - quota guards khi generate/publish/connect

Frontend nen thiet ke sao cho:

- account-level billing routes co the chay voi `Authorization` va doc profile state khi can
- workspace quota widgets co the nhin tu `activeProfile`

Do backend user-facing payment contract chua ro, FE Phase 9 chua duoc hardcode rule `X-Profile-Id` cho tat ca payment routes. Rule nay phai de mo va duoc chot lai khi backend expose controller that.

## Types va enum seam nen tao trong FE

File nen tao:

```text
AISAM-FE/src/types/subscription.ts
AISAM-FE/src/types/payment.ts
AISAM-FE/src/types/quota.ts
AISAM-FE/src/constants/subscription.ts
```

### Enum seam exact tu backend

```ts
export const subscriptionPlanValues = {
  Free: 0,
  Plus: 1,
  Premium: 2,
  PlusTrial: 3,
} as const

export const paymentStatusValues = {
  Pending: 0,
  Success: 1,
  Failed: 2,
  Refunded: 3,
} as const
```

### FE types duoc phep chot som

```ts
export type PlanCode = 0 | 1 | 2 | 3

export type PaymentStatusCode = 0 | 1 | 2 | 3

export type SubscriptionContext = {
  subscriptionId?: string | null
  planCode?: PlanCode | null
  isActive?: boolean | null
  startDate?: string | null
  endDate?: string | null
}

export type QuotaSnapshot = {
  postsPerMonth?: { limit: number; used?: number; remaining?: number }
  aiContentPerDay?: { limit: number; used?: number; remaining?: number }
  aiImagesPerDay?: { limit: number; used?: number; remaining?: number }
  platforms?: { limit: number; used?: number; remaining?: number }
  accounts?: { limit: number; used?: number; remaining?: number }
  adBudgetMonthly?: { limit: number; used?: number; remaining?: number }
  adCampaigns?: { limit: number; used?: number; remaining?: number }
}
```

Luu y:

- `used` va `remaining` la target-product shape de FE dung state/UI
- backend hien tai chua xac nhan API tra ra dung shape nay
- FE chi duoc dung shape nay o muc local domain model, khong duoc coi la API DTO cuoi cung

## Task 9.1 - Tao subscription/pricing information architecture

### Muc tieu

- Chot route va feature boundary cho phan pricing/billing cua user app
- Tach ro account billing voi workspace quota context

### Trang thai backend

- `backend-missing` cho pricing/payment API user-facing
- `backend-ready` cho viec doc `profile.subscriptionId`

### File nen tao

```text
AISAM-FE/src/app/(app)/subscription/page.tsx
AISAM-FE/src/app/(app)/subscription/pricing/page.tsx
AISAM-FE/src/app/(app)/subscription/history/page.tsx
AISAM-FE/src/app/(app)/subscription/result/page.tsx
AISAM-FE/src/features/subscription/config/subscription-routes.ts
AISAM-FE/src/features/subscription/config/subscription-navigation.ts
AISAM-FE/src/features/subscription/config/plan-display.ts
```

### Yeu cau implementation

- Route `subscription` la landing page cho billing trong user app
- Route `pricing` hien plan cards va CTA
- Route `history` hien payment history table state
- Route `result` dung cho checkout success/cancel/fail states
- Nav label phai ro: `Subscription & Billing`

### Rule UX

- Khong de route `subscription` chi la 1 badge `backend-dependent` don thuan
- Phai co UI that: plan cards, current plan summary, quota concept summary, locked actions
- Muc tieu la nguoi dung nhin ra san pham co capability gi, phan nao chua noi that duoc

### Definition of Done

- Co route tree ro rang cho billing user app
- Co thong diep phan biet `available now` va `waiting for backend contract`
- Khong co API call fake

## Task 9.2 - Tao current subscription va billing overview page

### Muc tieu

- Hien duoc current workspace subscription context som nhat co the

### Trang thai backend

- `backend-partial`

Ly do:

- Co `profile.subscriptionId`
- Chua co user-facing endpoint ro rang de lay full subscription detail

### File nen tao

```text
AISAM-FE/src/features/subscription/components/current-plan-summary.tsx
AISAM-FE/src/features/subscription/components/billing-overview.tsx
AISAM-FE/src/features/subscription/components/subscription-status-badge.tsx
AISAM-FE/src/features/subscription/hooks/use-subscription-context.ts
```

### Nguon du lieu duoc phep dung ngay

- `activeProfile.subscriptionId`
- profile name
- current user account context

### Khong duoc gia dinh backend tra full detail

Khong duoc tu y hien:

- gia chinh xac cua plan
- renewal date that
- payment method that
- invoice URL that

neu chua co API that.

### UX can co

- Neu `subscriptionId` null:
  - hien `No active subscription linked`
  - CTA sang pricing

- Neu `subscriptionId` co gia tri:
  - hien `Subscription linked`
  - mo ta day la billing context da gan voi profile
  - hien badge `More billing details waiting for backend contract`

### Definition of Done

- User thay duoc billing overview khong crash
- UI dung duoc voi ca profile co va khong co `subscriptionId`
- Khong co field nao bi fake nhu la da active neu backend chua xac nhan

## Task 9.3 - Tao pricing / upgrade / checkout seam

### Muc tieu

- Tao flow UX cho select plan va bat dau checkout
- Khoa ro integration seam cho PayOS

### Trang thai backend

- `backend-missing`

### File nen tao

```text
AISAM-FE/src/features/subscription/components/plan-card.tsx
AISAM-FE/src/features/subscription/components/pricing-grid.tsx
AISAM-FE/src/features/subscription/components/upgrade-plan-button.tsx
AISAM-FE/src/features/subscription/components/checkout-intent-panel.tsx
AISAM-FE/src/features/subscription/components/billing-action-disabled-note.tsx
AISAM-FE/src/features/subscription/api/create-checkout-intent.ts
AISAM-FE/src/features/subscription/api/change-plan.ts
AISAM-FE/src/features/subscription/api/cancel-subscription.ts
AISAM-FE/src/features/subscription/api/renew-subscription.ts
```

### Cach implement dung

Voi 4 file `api/*.ts` ben tren:

- Khong implement HTTP request that neu chua co contract controller
- Export function co the throw `BackendContractMissingError`
- Hoac dung adapter interface de sau nay map sang API that

Vi du:

```ts
export async function createCheckoutIntent(): Promise<never> {
  throw new BackendContractMissingError("User-facing checkout API is not exposed in AISAM-BE yet.")
}
```

### Plan card content nen co

Toi thieu:

- ten plan: Free / Plus / Premium / PlusTrial
- tom tat quota concept
- note ve pricing se duoc lay tu backend or config sau

### CTA states

- `Current plan`
- `Upgrade`
- `Downgrade`
- `Start trial`
- `Contact support`

Nhung:

- CTA co the disabled neu backend contract chua co
- Tooltip/noi giai thich phai noi ro ly do

### Definition of Done

- Pricing page co UI co nghia, khong phai placeholder rong
- Upgrade flow co CTA/state machine ro
- Chua co HTTP call fake den endpoint khong ton tai

## Task 9.4 - Tao payment history va billing states

### Muc tieu

- Co page lich su giao dich va payment state UX dung chuan target product

### Trang thai backend

- `backend-missing`

### File nen tao

```text
AISAM-FE/src/features/payments/components/payment-history-table.tsx
AISAM-FE/src/features/payments/components/payment-status-badge.tsx
AISAM-FE/src/features/payments/components/payment-empty-state.tsx
AISAM-FE/src/features/payments/api/get-payments.ts
AISAM-FE/src/features/payments/api/get-payment-detail.ts
```

### Payment state contract duoc phep chot som

UI phai support:

- Pending
- Success
- Failed
- Refunded

### Table columns nen co

- createdAt
- amount
- currency
- status
- paymentMethod
- transactionId
- invoiceUrl

Luu y:

- Day la field duoc support boi model `Payment`
- Khong dong nghia backend user-facing API se tra dung y het shape nay

### Cach xu ly khi backend chua co

- Page co empty state + thong bao `Payment history API not exposed yet`
- Co skeleton/table shell de team FE hoan thien layout
- Co test ID va state machine san cho luc noi that sau nay

### Definition of Done

- Payment history page co shell hoan chinh
- Payment status badge map dung 4 trang thai
- Khong co promise fake thanh cong

## Task 9.5 - Tao quota overview va action guards

### Muc tieu

- Dua quota vao UX workspace, du backend usage counter chua day du
- Chuan bi guard cho AI generate, publish, connect account, create campaign

### Trang thai backend

- `backend-partial`

Ly do:

- Model `Subscription` co quota limit fields
- Repo hien tai chua expose ro user-facing quota usage API
- Requirement yeu cau chan generate/publish khi het quota

### File nen tao

```text
AISAM-FE/src/features/quota/components/quota-overview-card.tsx
AISAM-FE/src/features/quota/components/quota-meter.tsx
AISAM-FE/src/features/quota/components/quota-limit-banner.tsx
AISAM-FE/src/features/quota/components/quota-locked-dialog.tsx
AISAM-FE/src/features/quota/hooks/use-quota-guards.ts
AISAM-FE/src/features/quota/hooks/use-quota-snapshot.ts
AISAM-FE/src/features/quota/lib/quota-guard-reasons.ts
```

### Guard points phai tinh den

- generate AI draft
- improve AI content
- publish content
- create schedule
- connect social account neu vuot `quotaPlatforms` / `quotaAccounts`
- create ads o phase sau

### Implementation rule

- Khong block bang so gia khi backend chua tra usage that
- Guard hook phai support 3 ket qua:
  - `allowed`
  - `unknown-backend-state`
  - `blocked`

Trong Phase 9, phan lon se la `unknown-backend-state` neu chua co usage API.

### UX can co

- Neu `unknown-backend-state`:
  - hien note `Quota enforcement pending backend integration`
  - khong duoc noi rang user con bao nhieu neu chua co data

- Neu sau nay co quota snapshot:
  - meter hien `used / limit`
  - banner hien ly do bi chan

### Definition of Done

- Co quota overview shell trong user app
- Co guard abstraction de feature phases sau goi lai
- Khong dua ra quota numbers fake

## Task 9.6 - Tao backend-dependent callback/result states va docs verify

### Muc tieu

- Chuan bi checkout result UX va integration seam cho PayOS-related redirects

### Trang thai backend

- `backend-missing`

### File nen tao

```text
AISAM-FE/src/features/subscription/components/checkout-result-state.tsx
AISAM-FE/src/features/subscription/components/checkout-success-state.tsx
AISAM-FE/src/features/subscription/components/checkout-cancelled-state.tsx
AISAM-FE/src/features/subscription/components/checkout-failed-state.tsx
AISAM-FE/src/features/subscription/lib/checkout-result-parser.ts
```

### Yeu cau implementation

- Page `subscription/result` doc query param neu co
- Support toi thieu 3 state:
  - success
  - cancelled
  - failed

### Luu y contract

- Khong duoc gia dinh exact query param cua PayOS callback neu backend chua chot
- Parser phai de mo, chi map cac key thong dung neu xuat hien
- Neu khong parse duoc, hien generic `Unable to confirm payment status yet`

### Verify docs nen them vao Phase 9

Can cap nhat sau nay trong:

- `ENV_SETUP.md`
- `FRONTEND_TEST_CHECKLIST.md`

Noi dung can them:

- subscription routes la target-product routes
- route nao dang `backend-dependent`
- khi nao FE duoc phep bat HTTP calls that

### Definition of Done

- Checkout result page co state machine ro
- Team FE biet ro day la UX seam, chua phai payment integration that

## Verify tong the Phase 9

Sau khi xong Phase 9, can verify:

1. Route `subscription`, `subscription/pricing`, `subscription/history`, `subscription/result` deu render on dinh.
2. User dang nhap vao workspace thay duoc billing information architecture hop ly.
3. Khong co request HTTP nao duoc gui den endpoint payment/subscription khong ton tai.
4. CTA nang cap / doi goi / huy goi / renew deu co state ro rang.
5. Quota guards co the duoc import va goi tu feature khac ma khong buoc team phai fake data.

## Deliverables sau Phase 9

Can co toi thieu:

- `AISAM-FE/src/app/(app)/subscription/page.tsx`
- `AISAM-FE/src/app/(app)/subscription/pricing/page.tsx`
- `AISAM-FE/src/app/(app)/subscription/history/page.tsx`
- `AISAM-FE/src/app/(app)/subscription/result/page.tsx`
- `AISAM-FE/src/features/subscription/*`
- `AISAM-FE/src/features/payments/*`
- `AISAM-FE/src/features/quota/*`
- `AISAM-FE/src/types/subscription.ts`
- `AISAM-FE/src/types/payment.ts`
- `AISAM-FE/src/types/quota.ts`
- `AISAM-FE/src/constants/subscription.ts`

## Rui ro can tranh

- Thay model `Subscription`/`Payment` roi tu y sinh API DTO cuoi cung
- Hardcode `/api/payments`, `/api/subscriptions`, `/api/checkout`, `/api/payos/*` du backend chua expose
- Hien pricing, renewal date, payment method nhu du lieu that khi chi la mock
- Block AI/publish bang quota fake, gay sai workflow user
- Tron user billing pages voi admin billing pages

## Dieu kien de chuyen sang Phase 10

Chi nen xem Phase 9 dat yeu cau khi:

- User app da co billing/subscription/quota information architecture dung target product
- Team FE co component va hook seam de noi backend sau nay ma khong can refactor lon
- Moi task deu ghi ro `backend-ready`, `backend-partial`, hoac `backend-missing`
- Khong co module target product nao bi danh thanh ngoai scope chi vi backend hien tai chua expose API

