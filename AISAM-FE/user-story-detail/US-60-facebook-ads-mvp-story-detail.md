# US-60 - Facebook Ads MVP

## Mo ta

La nguoi dung marketing, toi muon tao campaign, ad set, ad creative va ad tu content de mo rong sang quang cao tra phi.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `requirement.md`: Facebook Ads, dynamic ads, optimization nang cao khong nam trong MVP hien tai; Facebook/Instagram/TikTok duoc dinh huong social, nhung Ads nang cao la pham vi mo rong.
- `BACKEND_CODE_PLAN.md`: Facebook Ads Campaign MVP la Phase 11, chi bat dau sau Phase 9 Workspace Migration va Phase 10 Admin theo Workspace.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: tai lieu cu goi day la Phase H2; backend plan hien tai da chot ten chinh thuc la Phase 11.
- Active backend `AISAM-BE`: da co entity/schema Ads, nhung chua co Ads controllers/services/repositories active.

## Trang thai backend hien tai

Backend da co schema/entity:

- `AdCampaign`
- `AdSet`
- `AdCreative`
- `Ad`
- `Subscription.QuotaAdCampaigns`
- `Subscription.QuotaAdBudgetMonthly`
- `SocialIntegration.AdAccountId`

Backend da co social publishing Facebook MVP:

- Facebook OAuth.
- Facebook page/account linking.
- Publish content len Facebook Page.
- Posts history.

Backend chua co active:

- `AdCampaignsController`
- `AdSetsController`
- `AdCreativesController`
- `AdsController`
- `FacebookMarketingApiService`
- `AdCampaignService`
- `AdSetService`
- `AdCreativeService`
- `AdService`
- `AdQuotaService`
- Repositories cho Ads.
- API list/link Facebook Ad Account.
- API create campaign/ad set/ad creative/ad qua Facebook Marketing API.
- API sync campaign/ad status tu Facebook.

Ket luan: frontend co the chuan bi UI va type cho Facebook Ads MVP, nhung khong goi Ads API trong active production flow cho den khi backend Phase 11 active.

## Muc tieu frontend

Tao UI Facebook Ads MVP cho marketer:

```text
/dashboard/campaigns
/dashboard/campaigns/new
/dashboard/campaigns/{campaignId}
/dashboard/campaigns/{campaignId}/ad-sets
/dashboard/ad-creatives
/dashboard/ads
```

Nguoi dung marketing co the:

- Xem danh sach campaigns theo active profile.
- Tao campaign gan voi brand va Facebook ad account.
- Tao ad set trong campaign.
- Tao ad creative tu content da co.
- Tao ad tu ad set va creative.
- Xem trang thai local/Facebook id cua campaign/ad set/ad creative/ad.
- Chuan bi publish/sync voi Facebook Marketing API khi backend support.

Trong luc backend chua active:

- UI hien planned/backend-not-ready state.
- Create/edit/sync/publish ads actions disabled.
- Khong goi `/api/ad-campaigns`, `/api/ad-sets`, `/api/ad-creatives`, `/api/ads`.

## User flows

### Flow 1 - Tao campaign tu brand

1. User vao `/dashboard/campaigns`.
2. Bam `Create campaign`.
3. Chon brand trong active profile.
4. Chon Facebook ad account neu backend support.
5. Nhap campaign name, objective, budget, start/end date.
6. Submit.
7. Backend tao campaign local va/hoac tao campaign tren Facebook.
8. UI redirect sang campaign detail.

### Flow 2 - Tao ad set

1. User mo campaign detail.
2. Vao tab Ad Sets.
3. Bam `Create ad set`.
4. Nhap name, targeting, daily budget, start/end date.
5. Submit.
6. Backend tao ad set local va/hoac tren Facebook.

### Flow 3 - Tao ad creative tu content

1. User vao content detail hoac Ad Creatives page.
2. Chon content da duoc approve/published neu workflow yeu cau.
3. Chon CTA va link URL.
4. Submit tao creative.
5. Backend tao creative local va/hoac Facebook creative.

### Flow 4 - Tao ad

1. User vao campaign detail.
2. Chon ad set.
3. Chon creative.
4. Nhap ad name/status neu backend support.
5. Submit tao ad.
6. UI hien ad trong list voi status `PAUSED` mac dinh.

## Frontend scope

Pages/components can implement:

```text
/dashboard/campaigns
/dashboard/campaigns/new
/dashboard/campaigns/[campaignId]
/dashboard/campaigns/[campaignId]/ad-sets
/dashboard/ad-creatives
/dashboard/ads
CampaignsPage
CampaignCreateForm
CampaignDetailPage
AdSetsTable
AdSetCreateForm
AdCreativesTable
AdCreativeCreateFromContentDialog
AdsTable
AdCreateForm
AdAccountSelector
AdsBackendNotReadyState
```

Navigation:

```text
Dashboard sidebar -> Campaigns / Ads
```

Neu backend chua active, sidebar co the hien badge:

```text
Post-MVP
```

## Backend API du kien

Backend hien tai chua expose cac endpoint duoi day. Day la contract de frontend chuan bi cho Phase 11.

### List ad accounts

```http
GET /api/social/accounts/{socialAccountId}/ad-accounts
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<FacebookAdAccount[]>
```

### Campaigns

```http
GET /api/ad-campaigns?page=1&pageSize=10&brandId={brandId}&isActive=true
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

```http
POST /api/ad-campaigns
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "brandId": "brand-guid",
  "adAccountId": "act_123456789",
  "name": "Summer Promotion Campaign",
  "objective": "OUTCOME_TRAFFIC",
  "budget": 1000000,
  "startDate": "2026-06-10",
  "endDate": "2026-06-30"
}
```

```http
GET /api/ad-campaigns/{campaignId}
PUT /api/ad-campaigns/{campaignId}
DELETE /api/ad-campaigns/{campaignId}
POST /api/ad-campaigns/{campaignId}/sync
```

### Ad sets

```http
GET /api/ad-campaigns/{campaignId}/ad-sets
POST /api/ad-campaigns/{campaignId}/ad-sets
```

Request:

```json
{
  "name": "HCMC young adults",
  "targeting": {
    "geoLocations": ["VN"],
    "ageMin": 18,
    "ageMax": 35,
    "interests": ["shopping", "social media"]
  },
  "dailyBudget": 200000,
  "startDate": "2026-06-10",
  "endDate": "2026-06-30",
  "status": "PAUSED"
}
```

```http
GET /api/ad-sets/{adSetId}
PUT /api/ad-sets/{adSetId}
DELETE /api/ad-sets/{adSetId}
POST /api/ad-sets/{adSetId}/sync
```

### Ad creatives

```http
GET /api/ad-creatives?page=1&pageSize=10&contentId={contentId}
POST /api/ad-creatives
```

Request:

```json
{
  "contentId": "content-guid",
  "adAccountId": "act_123456789",
  "callToAction": "LEARN_MORE",
  "linkUrl": "https://example.com/product",
  "facebookPostId": "optional-existing-post-id"
}
```

```http
GET /api/ad-creatives/{creativeId}
DELETE /api/ad-creatives/{creativeId}
POST /api/ad-creatives/{creativeId}/sync
```

### Ads

```http
GET /api/ads?page=1&pageSize=10&adSetId={adSetId}&status=PAUSED
POST /api/ads
```

Request:

```json
{
  "adSetId": "ad-set-guid",
  "creativeId": "creative-guid",
  "status": "PAUSED"
}
```

```http
GET /api/ads/{adId}
PUT /api/ads/{adId}
DELETE /api/ads/{adId}
POST /api/ads/{adId}/sync
```

## API response types du kien

```ts
interface FacebookAdAccount {
  id: string
  name: string
  accountStatus?: string
  currency?: string
}

interface AdCampaignDto {
  id: string
  profileId: string
  brandId: string
  brandName?: string
  adAccountId: string
  facebookCampaignId?: string
  name: string
  objective?: string
  budget?: number
  startDate?: string
  endDate?: string
  isActive: boolean
  isDeleted: boolean
  createdAt: string
  updatedAt: string
}

interface AdSetDto {
  id: string
  campaignId: string
  name: string
  facebookAdSetId?: string
  targeting?: unknown
  dailyBudget?: number
  startDate?: string
  endDate?: string
  status?: string
  isDeleted: boolean
  createdAt: string
}

interface AdCreativeDto {
  id: string
  contentId?: string
  adAccountId: string
  creativeId?: string
  callToAction?: string
  linkUrl?: string
  facebookPostId?: string
  isDeleted: boolean
  createdAt: string
}

interface AdDto {
  id: string
  adSetId: string
  creativeId: string
  adId?: string
  status?: string
  isDeleted: boolean
  createdAt: string
}
```

## API status handling

Frontend can xu ly:

- `200/201`: render/update thanh cong.
- `400`: validation error.
- `401`: token thieu/het han, redirect login.
- `403`: user khong co quyen hoac Facebook permission thieu.
- `404`: endpoint chua active hoac resource khong ton tai.
- `409`: invalid state, duplicate entity, quota exceeded, Facebook object already exists.
- `422`: Facebook validation error neu backend map rieng.
- `502/503`: Facebook Marketing API/config unavailable.
- `500`: loi he thong.

Khi endpoint Ads tra `404` do backend chua active:

```text
Facebook Ads API chua active trong backend hien tai.
```

## Business rules

- Moi campaign phai thuoc active profile va brand hop le.
- Brand phai thuoc active profile.
- Ad account phai den tu Facebook account/integration da link hop le.
- Ad set phai thuoc campaign.
- Ad creative co the duoc tao tu content da co.
- Content dung de tao creative nen thuoc cung profile/brand.
- Neu approval workflow duoc bat, chi content `Approved` hoac `Published` moi duoc tao creative.
- Ad phai co ad set va creative hop le.
- Default Facebook ad/ad set status nen la `PAUSED` de tranh chay quang cao ngoai y muon.
- Campaign/ad set budget phai validate so duong.
- Start date phai truoc end date.
- Quota ad campaigns/budget can duoc backend enforce khi Phase Payment/Quota active.
- Frontend khong duoc coi Ads UI la source of truth cho spending; Facebook Marketing API/backend moi la source of truth.

## UI requirements

### Campaign list

Cot/card toi thieu:

- Campaign name
- Brand
- Objective
- Budget
- Date range
- Status/isActive
- Facebook campaign id
- Actions

Filters:

- Brand
- Active/inactive
- Objective
- Search

### Campaign create form

Fields:

- Brand
- Facebook ad account
- Name
- Objective
- Budget
- Start date
- End date

Validation:

- Brand required.
- Ad account required.
- Name required.
- Budget positive.
- End date after start date.

### Ad set form

Fields:

- Name
- Daily budget
- Date range
- Status
- Targeting JSON/simple controls

MVP targeting UI nen bat dau don gian:

- Countries
- Age min/max
- Interests free text/tags

### Ad creative form

Fields:

- Content selector
- CTA
- Link URL
- Facebook post id optional

CTA suggestions:

```text
LEARN_MORE
SHOP_NOW
SIGN_UP
CONTACT_US
GET_OFFER
```

### Ads table

Fields:

- Ad id
- Ad set
- Creative
- Facebook ad id
- Status
- Created at

### Backend not ready state

```text
Facebook Ads chua active.
```

Mo ta phu:

```text
Backend can hoan thanh Phase 11 va co Facebook Marketing API permission truoc khi bat tao campaign, ad set, creative va ad.
```

## Acceptance criteria

- `/dashboard/campaigns` co page rieng.
- Khi chua co active profile, khong goi API Ads va hien profile guard.
- Khi backend chua active, page hien backend-not-ready state va khong crash.
- Sidebar Campaigns/Ads co badge `Post-MVP` hoac action disabled neu backend chua active.
- UI khong goi Ads endpoints trong active production flow neu backend chua active.
- Campaign form co brand selector tu active brands.
- Campaign form validate name, brand, ad account, date range va budget.
- Ad set form validate campaign, name, date range va daily budget.
- Creative form cho chon content trong active profile.
- Ad form cho chon ad set va creative.
- Default status cho ad set/ad la `PAUSED`.
- API `401` redirect login.
- API `403` hien permission/Facebook permission error.
- API `404` hien backend-not-ready hoac not found tuy ngu canh.
- API `503` hien Facebook Marketing API/config unavailable.
- Khong hien claim la Ads da chay that neu backend/Facebook chua sync thanh cong.

## Suggested frontend types

```ts
export interface CreateAdCampaignRequest {
  brandId: string
  adAccountId: string
  name: string
  objective?: string
  budget?: number
  startDate?: string
  endDate?: string
}

export interface CreateAdSetRequest {
  name: string
  targeting?: unknown
  dailyBudget?: number
  startDate?: string
  endDate?: string
  status?: "ACTIVE" | "PAUSED"
}

export interface CreateAdCreativeRequest {
  contentId?: string
  adAccountId: string
  callToAction?: string
  linkUrl?: string
  facebookPostId?: string
}

export interface CreateAdRequest {
  adSetId: string
  creativeId: string
  status?: "ACTIVE" | "PAUSED"
}
```

## Suggested API client methods

```ts
export async function getAdCampaigns(query: {
  page: number
  pageSize: number
  brandId?: string
  isActive?: boolean
}) {
  const params = new URLSearchParams()
  params.set("page", String(query.page))
  params.set("pageSize", String(query.pageSize))
  if (query.brandId) params.set("brandId", query.brandId)
  if (query.isActive !== undefined) params.set("isActive", String(query.isActive))

  return fetchWithAuth<ApiResponse<PagedResult<AdCampaignDto>>>(
    `/ad-campaigns?${params.toString()}`
  )
}

export async function createAdCampaign(payload: CreateAdCampaignRequest) {
  return fetchWithAuth<ApiResponse<AdCampaignDto>>("/ad-campaigns", {
    method: "POST",
    body: JSON.stringify(payload),
  })
}

export async function createAdSet(campaignId: string, payload: CreateAdSetRequest) {
  return fetchWithAuth<ApiResponse<AdSetDto>>(
    `/ad-campaigns/${campaignId}/ad-sets`,
    { method: "POST", body: JSON.stringify(payload) }
  )
}
```

## Test cases frontend

- Vao `/dashboard/campaigns` khi chua active profile thi hien profile guard.
- Backend `404` thi hien backend-not-ready state.
- Campaign create disabled khi backend chua active.
- Campaign form validate budget positive.
- Campaign form validate end date sau start date.
- Ad account missing thi disable submit.
- Ad set default status la `PAUSED`.
- Creative form chi list content trong active profile.
- API `403` hien permission/Facebook permission message.
- API `503` hien Facebook Marketing API unavailable.
- Khong goi `/api/ad-campaigns` neu feature flag Ads disabled.

## Dependencies / blockers

- Backend can hoan thanh Phase 11 Facebook Ads Campaign MVP.
- Can migrate `AdCampaignsController`, `AdSetsController`, `AdCreativesController`, `AdsController`.
- Can migrate Ads services/repositories va `FacebookMarketingApiService`.
- Can Facebook Marketing API permissions va ad account access.
- Can quyet dinh flow link/list ad accounts.
- Can quota/payment/subscription enforcement neu demo SaaS Ads.
- Can compliance voi Meta Ads policy va app review neu chay real ads.
