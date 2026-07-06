# US-66 - Quan ly plan dong

## 1. Thong tin user story

**Ma story:** US-66  
**Ten story:** Quan ly plan dong  
**Vai tro:** Quan tri vien  
**Muc tieu:** CRUD subscription plans dong de thay doi pricing va quota ma khong can sua code.  
**Mo ta goc:** La quan tri vien, toi muon CRUD subscription plans dong de thay doi pricing va quota ma khong can sua code.

## 2. Boi canh tu requirement va backend hien tai

Requirement co yeu cau quan ly subscription, pricing, payment va quota co ban. Administrator co use case cau hinh goi dang ky/pricing trong pham vi duoc phep. Requirement cung ghi dynamic subscription plans va pricing config linh hoat la phan mo rong.

Trang thai backend hien tai:

- Entity `Subscription` da ton tai va luu plan theo `SubscriptionPlanEnum`.
- `SubscriptionPlanEnum` hien hard-code:
  - `Free = 0`
  - `Plus = 1`
  - `Premium = 2`
  - `PlusTrial = 3`
- Quota hien nam truc tiep tren `Subscription`:
  - `QuotaPostsPerMonth`
  - `QuotaAIContentPerDay`
  - `QuotaAIImagesPerDay`
  - `QuotaPlatforms`
  - `QuotaAccounts`
  - `QuotaAdBudgetMonthly`
  - `QuotaAdCampaigns`
  - `AnalysisLevel`
- `Payment` entity da co amount, currency, status, payment method, transaction id, invoice url.
- `Profile` co `SubscriptionId`.
- Migration `UpdateSubscriptionPayOS` da bo sung mot so field quota/PayOS.

Han che backend hien tai:

- Chua co entity/bang rieng cho dynamic plan, vi du `SubscriptionPlan`.
- Chua co `SubscriptionController` (khong ton tai, subscription doc qua `PaymentController`).
- Chua co admin controller/policy active cho CRUD plan.
- Chua co admin controller/policy active cho CRUD plan.
- Chua co endpoint public/admin de list pricing plans.
- Chua co versioning/snapshot plan de bao ve subscription da mua truoc khi admin sua gia/quota.
- CODEBASE_UPDATE xep dynamic subscription plan CRUD vao Phase H5 / post-MVP.

Vi vay frontend cua US-66 can duoc thiet ke theo huong admin feature-ready, nhung can blocker backend ro rang.

## 3. Pham vi frontend

### In scope

- Tao man hinh admin quan ly subscription plans.
- Hien thi danh sach plans, pricing, currency, cycle, status va quota.
- Tao plan moi.
- Cap nhat ten plan, mo ta, pricing, currency, billing cycle, quota va feature flags.
- Archive/deactivate plan thay vi hard delete neu plan da co user/subscription dung.
- Hien confirm dialog cho thay doi nhay cam.
- Hien validation loi tu backend.
- Hien audit-friendly metadata: created at, updated at, created by/updated by neu backend tra ve.

### Out of scope

- Implement payment gateway/PayOS.
- Tu dong migrate subscription hien co sang plan moi neu backend chua ho tro.
- Tinh prorate, refund, invoice adjustment.
- Quan ly campaign quota usage chi tiet.
- Cho non-admin truy cap plan management.
- Sua enum backend tu frontend.

## 4. API hien tai va khoang trong

Backend hien tai khong co endpoint active cho dynamic plan CRUD. FE khong nen hard-code thao tac ghi vao `SubscriptionPlanEnum`. Neu can hien pricing public truoc khi backend xong, chi nen dung mock/config tam thoi tach rieng va danh dau la fallback.

Admin plan management can backend cung cap API moi va admin policy.

## 5. API/DTO de xuat cho US-66

### 5.1. List plans cho admin

```http
GET /api/admin/subscription-plans?page=1&pageSize=20&searchTerm=&status=&includeArchived=false
Authorization: Bearer {adminAccessToken}
```

Response:

```json
{
  "items": [
    {
      "id": "plan-id",
      "code": "plus",
      "name": "Plus",
      "description": "For growing brands",
      "price": 199000,
      "currency": "VND",
      "billingCycle": "monthly",
      "status": "active",
      "isPublic": true,
      "quotas": {
        "postsPerMonth": 100,
        "aiContentPerDay": 50,
        "aiImagesPerDay": 20,
        "aiVideosPerDay": 0,
        "platforms": 3,
        "accounts": 5,
        "adBudgetMonthly": 5000000,
        "adCampaigns": 10,
        "storageGb": 10
      },
      "features": {
        "approvalWorkflow": true,
        "teamManagement": true,
        "facebookAds": false,
        "aiImageGeneration": true,
        "aiVideoGeneration": false
      },
      "activeSubscriptionsCount": 12,
      "createdAt": "2026-06-03T10:00:00Z",
      "updatedAt": "2026-06-03T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1
}
```

### 5.2. Get plan detail

```http
GET /api/admin/subscription-plans/{planId}
Authorization: Bearer {adminAccessToken}
```

### 5.3. Create plan

```http
POST /api/admin/subscription-plans
Authorization: Bearer {adminAccessToken}
Content-Type: application/json
```

Request:

```json
{
  "code": "growth",
  "name": "Growth",
  "description": "For teams scaling content operations",
  "price": 299000,
  "currency": "VND",
  "billingCycle": "monthly",
  "isPublic": true,
  "quotas": {
    "postsPerMonth": 200,
    "aiContentPerDay": 100,
    "aiImagesPerDay": 50,
    "aiVideosPerDay": 3,
    "platforms": 5,
    "accounts": 10,
    "adBudgetMonthly": 10000000,
    "adCampaigns": 20,
    "storageGb": 20
  },
  "features": {
    "approvalWorkflow": true,
    "teamManagement": true,
    "facebookAds": true,
    "aiImageGeneration": true,
    "aiVideoGeneration": false
  }
}
```

### 5.4. Update plan

```http
PUT /api/admin/subscription-plans/{planId}
Authorization: Bearer {adminAccessToken}
Content-Type: application/json
```

Request tuong tu create. Backend nen validate khong cho sua `code` neu plan da co subscription active, hoac tao version moi.

### 5.5. Archive/deactivate plan

```http
POST /api/admin/subscription-plans/{planId}/archive
Authorization: Bearer {adminAccessToken}
```

Response:

```json
{
  "id": "plan-id",
  "status": "archived",
  "isPublic": false
}
```

### 5.6. Activate plan

```http
POST /api/admin/subscription-plans/{planId}/activate
Authorization: Bearer {adminAccessToken}
```

### 5.7. Public pricing plans

```http
GET /api/subscription-plans/public
Authorization: Bearer {accessToken}
```

Endpoint nay dung cho user pricing/subscription page, chi tra plans `active` va `isPublic = true`.

## 6. UX/UI detail

### 6.1. Admin navigation

Them item trong admin sidebar:

- Label: `Subscription Plans`
- Route de xuat: `/admin/subscription-plans`
- Chi hien voi user role `Admin`.

### 6.2. Plan list page

List page can co:

- Search theo code/name.
- Filter theo status:
  - `Active`
  - `Draft`
  - `Archived`
- Filter public/private.
- Table columns:
  - Plan name/code
  - Price
  - Billing cycle
  - Public
  - Status
  - Key quotas
  - Active subscriptions
  - Updated at
  - Actions
- Actions:
  - View/Edit
  - Duplicate
  - Archive
  - Activate

### 6.3. Create/Edit plan form

Form nen chia thanh sections:

- Basic info:
  - Code
  - Name
  - Description
  - Status
  - Public visibility
- Pricing:
  - Price
  - Currency
  - Billing cycle
- Quotas:
  - Posts per month
  - AI content per day
  - AI images per day
  - AI videos per day
  - Platforms
  - Accounts
  - Ad budget monthly
  - Ad campaigns
  - Storage GB
- Features:
  - Approval workflow
  - Team management
  - Facebook Ads
  - AI image generation
  - AI video generation
- Review summary before save for high impact changes.

### 6.4. Confirm sensitive changes

FE can hien confirm modal khi:

- Gia thay doi.
- Quota giam.
- Plan active chuyen archived.
- Public plan chuyen private.
- Feature dang enabled bi disable.

Modal can hien anh huong:

- Plan name.
- So subscription active neu backend tra.
- Noi dung thay doi chinh.

### 6.5. Versioning/snapshot indicator

Neu backend ho tro versioning, UI nen hien:

- Current version.
- Effective date.
- Badge `New subscriptions only` hoac `Affects existing subscriptions`.

Neu backend chua ho tro versioning, UI can canh bao khi sua plan dang co active subscriptions.

## 7. Business rules

- Chi admin moi duoc truy cap CRUD dynamic plans.
- Non-admin truy cap route admin phai bi redirect/forbidden.
- `code` phai unique, lowercase/kebab-case hoac format backend quy dinh.
- Price khong am.
- Currency mac dinh `VND`, chi cho currency backend support.
- Quota khong am.
- Billing cycle chi dung gia tri backend support, vi du `monthly`, `yearly`.
- Plan archived khong hien tren public pricing va khong cho user subscribe moi.
- Plan co active subscriptions khong nen hard delete.
- Neu sua quota/gia cua plan dang active, backend can quyet dinh apply ngay, effective date, hoac tao version moi. FE phai hien policy ro rang.

## 8. Data model frontend de xuat

```ts
type PlanStatus = "draft" | "active" | "archived";
type BillingCycle = "monthly" | "yearly";

type PlanQuotas = {
  postsPerMonth: number;
  aiContentPerDay: number;
  aiImagesPerDay: number;
  aiVideosPerDay?: number;
  platforms: number;
  accounts: number;
  adBudgetMonthly: number;
  adCampaigns: number;
  storageGb?: number;
};

type PlanFeatures = {
  approvalWorkflow: boolean;
  teamManagement: boolean;
  facebookAds: boolean;
  aiImageGeneration: boolean;
  aiVideoGeneration: boolean;
};

type SubscriptionPlan = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  price: number;
  currency: "VND" | "USD" | string;
  billingCycle: BillingCycle;
  status: PlanStatus;
  isPublic: boolean;
  quotas: PlanQuotas;
  features: PlanFeatures;
  activeSubscriptionsCount?: number;
  version?: number;
  effectiveFrom?: string | null;
  createdAt: string;
  updatedAt: string;
};
```

## 9. Acceptance criteria

### AC1 - Admin xem danh sach plans

Given user dang nhap voi role `Admin`  
When user mo `/admin/subscription-plans`  
Then FE goi API list plans  
And hien danh sach plan voi pricing, quota, status va actions.

### AC2 - Non-admin bi chan

Given user khong co role `Admin`  
When user truy cap route plan management  
Then FE khong hien man hinh admin  
And dieu huong ve forbidden/login tuy auth state.

### AC3 - Tao plan moi

Given admin mo form create plan  
When admin nhap day du thong tin hop le va bam Save  
Then FE goi `POST /api/admin/subscription-plans`  
And sau thanh cong quay ve list/detail voi plan moi.

### AC4 - Validation form

Given admin nhap price am hoac quota am  
When submit form  
Then FE chan submit va hien validation error tai field.

### AC5 - Cap nhat plan

Given admin dang edit plan ton tai  
When admin thay doi pricing/quota va save  
Then FE hien confirm neu thay doi nhay cam  
And goi update API sau khi admin confirm.

### AC6 - Archive plan

Given plan dang active  
When admin bam Archive  
Then FE hien confirm modal  
And goi archive API neu admin xac nhan  
And plan khong con public active tren list.

### AC7 - Backend chua ho tro dynamic plan

Given backend tra `404`, `501` hoac chua co endpoint  
When admin mo plan management  
Then FE hien empty/error state ro rang `Dynamic plan management is not enabled`  
And khong render form nhu tinh nang da active.

### AC8 - Public pricing khong hien archived/private plans

Given public pricing page goi plans public  
When backend tra danh sach plans  
Then FE chi hien plan active va public theo response.

## 10. Error handling

| Truong hop | Xu ly frontend |
| --- | --- |
| `401 Unauthorized` | Dua user ve login hoac refresh token |
| `403 Forbidden` | Hien forbidden va an admin nav |
| `404 Plan not found` | Hien not found va CTA quay lai list |
| `409 Duplicate code` | Hien loi tai field `code` |
| `409 Plan has active subscriptions` | Hien conflict va de xuat archive/versioning |
| `422 Validation error` | Map loi vao tung field |
| `501 Not implemented` | Hien backend chua bat dynamic plan |
| Network error | Hien retry, giu form data chua submit |

## 11. Test cases frontend

- Admin route chi render voi role `Admin`.
- Non-admin bi redirect/forbidden.
- List plans render table, filters va pagination.
- Create form validate required fields, price/quota non-negative.
- Submit create goi dung payload.
- Edit plan preload data dung.
- Thay doi price/quota giam hien confirm modal.
- Archive active plan hien confirm va goi dung endpoint.
- Duplicate code backend error map vao field `code`.
- Backend `501` hien feature unavailable state.
- Public pricing page chi hien plans active/public theo API response.

## 12. Dependency va blocker

- Backend can co entity/bang dynamic subscription plan rieng.
- Backend can co admin policy active voi `UserRoleEnum.Admin`.
- Backend can co CRUD endpoints cho admin plan management.
- Backend can co public endpoint list active public plans.
- Backend can quyet dinh strategy cho subscription da mua:
  - snapshot quota/price tai thoi diem mua,
  - plan versioning,
  - hay apply thay doi truc tiep.
- Backend can tich hop PayOS/payment voi plan id dong thay vi enum.
- Backend can quota service doc limit tu dynamic plan thay vi hard-code tren enum/subscription.

## 13. Definition of Done

- Co admin UI list/create/edit/archive/activate subscription plans.
- FE chi cho admin truy cap man hinh nay.
- Form validate pricing/quota/features truoc khi submit.
- UI graceful khi backend chua ho tro dynamic plan CRUD.
- Public pricing/subscription UI co the doc plans tu endpoint public khi backend san sang.
- Co test cho role guard, list, create, edit, archive, validation va backend-not-enabled state.
