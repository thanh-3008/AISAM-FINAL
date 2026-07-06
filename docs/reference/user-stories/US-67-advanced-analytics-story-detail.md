# US-67 - Ho tro analytics nang cao

## 1. Thong tin user story

**Ma story:** US-67  
**Ten story:** Ho tro analytics nang cao  
**Vai tro:** Nguoi dung marketing / marketer / brand owner  
**Muc tieu:** Xem analytics chi tiet theo kenh va chien dich de toi uu hieu qua marketing.  
**Mo ta goc:** La nguoi dung, toi muon xem analytics chi tiet hon theo kenh va chien dich de toi uu hieu qua marketing.

## 2. Boi canh tu requirement va backend hien tai

Requirement co yeu cau dashboard va analytics co ban trong MVP:

- Xem chi so co ban theo bai dang: reach, impressions, engagement, click-through khi co du lieu.
- Dashboard va danh sach du lieu can ho tro pagination va filter.
- Social platform APIs duoc dung cho OAuth, publishing va analytics.
- Analytics nang cao, AI recommendations va optimization nam trong pham vi mo rong.

Trang thai backend hien tai:

- `DashboardController` chi co `GET /api/dashboard/summary`.
- `DashboardSummaryDto` chi tra cac count noi bo:
  - draft content,
  - published content,
  - pending approval,
  - upcoming/failed schedules,
  - active social integrations,
  - published posts,
  - unread notifications.
- `PerformanceReport` entity da co metric co ban:
  - `PostId`,
  - `AdId`,
  - `Impressions`,
  - `Engagement`,
  - `Ctr`,
  - `EstimatedRevenue`,
  - `ReportDate`,
  - `RawData`.
- `PerformanceReportRepository` hien chi co `CountByProfileIdAsync`, chua co query series/breakdown.
- `Post` da lien ket `Content` va `SocialIntegration`, co `ExternalPostId`, `PublishedAt`, `Status`.
- `AdCampaign`, `AdSet`, `Ad`, `AdCreative` entities da ton tai, nhung Ads API chua active.
- Facebook publishing flow active, nhung analytics sync tu Facebook/Instagram/TikTok/Ads provider chua active.
- CODEBASE_UPDATE ghi ro dashboard hien chi la summary MVP, chua keo performance analytics nang cao.

Vi vay frontend cho US-67 can thiet ke dashboard analytics nang cao theo contract moi, dong thoi graceful khi backend moi chi co summary.

## 3. Pham vi frontend

### In scope

- Tao man hinh analytics nang cao cho user theo active profile.
- Hien metric tong quan trong khoang thoi gian.
- Filter theo:
  - date range,
  - brand,
  - platform/channel,
  - campaign,
  - content type,
  - post/ad status.
- Hien breakdown theo kenh social.
- Hien breakdown theo campaign khi backend co campaign/ad data.
- Hien time series chart cho impressions, reach, engagement, CTR, clicks, spend/estimated revenue neu co.
- Hien top/bottom performing posts hoac campaigns.
- Hien empty state khi chua co data hoac provider chua sync analytics.
- Export CSV neu backend ho tro hoac FE co the export tu data dang hien.

### Out of scope

- Tu dong toi uu budget/campaign.
- AI recommendation nang cao neu backend chua co endpoint.
- Real-time streaming analytics.
- Fetch social insights truc tiep tu frontend.
- Quan ly Facebook Ads campaign CRUD. Phan nay thuoc US-60.
- Ket noi Instagram/TikTok analytics neu provider chua active.

## 4. API hien tai co the dung

### 4.1. Dashboard summary hien tai

```http
GET /api/dashboard/summary
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response hien tai:

```json
{
  "success": true,
  "message": "Dashboard summary retrieved successfully.",
  "data": {
    "draftContentCount": 10,
    "publishedContentCount": 20,
    "pendingApprovalContentCount": 3,
    "upcomingScheduleCount": 5,
    "failedScheduleCount": 1,
    "activeSocialIntegrationCount": 2,
    "publishedPostCount": 20,
    "unreadNotificationCount": 4
  }
}
```

FE co the dung endpoint nay cho summary cards co ban, nhung khong du cho analytics theo kenh/campaign.

### 4.2. Posts list hien tai

```http
GET /api/posts?page=1&pageSize=10&brandId={brandId}&status={status}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response hien tai chi co post metadata, chua co metric:

```json
{
  "id": "post-id",
  "contentId": "content-id",
  "integrationId": "integration-id",
  "externalPostId": "facebook-post-id",
  "publishedAt": "2026-06-03T10:00:00Z",
  "status": "Published",
  "contentTitle": "Summer Campaign",
  "brandName": "AISAM"
}
```

## 5. API/DTO de xuat cho US-67

### 5.1. Analytics overview

```http
GET /api/analytics/overview?from=2026-06-01&to=2026-06-30&brandId=&platform=&campaignId=
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "dateRange": {
    "from": "2026-06-01",
    "to": "2026-06-30"
  },
  "totals": {
    "impressions": 120000,
    "reach": 85000,
    "engagement": 5400,
    "clicks": 1800,
    "ctr": 0.015,
    "estimatedRevenue": 0,
    "spend": 2500000,
    "publishedPosts": 35,
    "activeCampaigns": 4
  },
  "changes": {
    "impressionsPct": 12.5,
    "engagementPct": -3.2,
    "ctrPct": 1.1,
    "spendPct": 8.0
  },
  "dataFreshness": {
    "lastSyncedAt": "2026-06-03T10:00:00Z",
    "isPartial": false
  }
}
```

### 5.2. Time series

```http
GET /api/analytics/time-series?from=2026-06-01&to=2026-06-30&granularity=day&metrics=impressions,engagement,ctr&brandId=&platform=&campaignId=
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "granularity": "day",
  "points": [
    {
      "date": "2026-06-01",
      "impressions": 5000,
      "reach": 4200,
      "engagement": 240,
      "clicks": 75,
      "ctr": 0.015,
      "spend": 100000,
      "estimatedRevenue": 0
    }
  ]
}
```

### 5.3. Breakdown theo kenh

```http
GET /api/analytics/channel-breakdown?from=2026-06-01&to=2026-06-30&brandId=&campaignId=
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
[
  {
    "platform": "facebook",
    "integrationId": "integration-id",
    "displayName": "AISAM Facebook Page",
    "impressions": 80000,
    "reach": 60000,
    "engagement": 4200,
    "clicks": 1200,
    "ctr": 0.015,
    "spend": 1500000,
    "publishedPosts": 22,
    "lastSyncedAt": "2026-06-03T10:00:00Z"
  }
]
```

### 5.4. Breakdown theo campaign

```http
GET /api/analytics/campaign-breakdown?from=2026-06-01&to=2026-06-30&brandId=&platform=&page=1&pageSize=20&sortBy=impressions&sortDescending=true
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "items": [
    {
      "campaignId": "campaign-id",
      "name": "Summer Launch",
      "platform": "facebook",
      "objective": "engagement",
      "status": "ACTIVE",
      "budget": 5000000,
      "impressions": 45000,
      "reach": 32000,
      "engagement": 2100,
      "clicks": 700,
      "ctr": 0.0156,
      "spend": 1200000,
      "estimatedRevenue": 0
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1
}
```

### 5.5. Top content/posts

```http
GET /api/analytics/top-posts?from=2026-06-01&to=2026-06-30&brandId=&platform=&metric=engagement&page=1&pageSize=10
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "items": [
    {
      "postId": "post-id",
      "contentId": "content-id",
      "contentTitle": "Summer Product Post",
      "brandName": "AISAM",
      "platform": "facebook",
      "publishedAt": "2026-06-03T10:00:00Z",
      "externalPostId": "facebook-post-id",
      "impressions": 10000,
      "reach": 8200,
      "engagement": 700,
      "clicks": 150,
      "ctr": 0.015
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1
}
```

### 5.6. Sync status

```http
GET /api/analytics/sync-status
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "providers": [
    {
      "platform": "facebook",
      "enabled": true,
      "lastSyncedAt": "2026-06-03T10:00:00Z",
      "status": "healthy",
      "message": null
    },
    {
      "platform": "instagram",
      "enabled": false,
      "lastSyncedAt": null,
      "status": "not_configured",
      "message": "Instagram analytics is not enabled yet."
    }
  ]
}
```

## 6. UX/UI detail

### 6.1. Analytics route

Route de xuat:

- `/analytics`
- `/analytics/campaigns`
- `/analytics/posts`

Man hinh nen dung layout dashboard work-focused, uu tien scan nhanh va filter ro rang.

### 6.2. Global filters

Filter bar can co:

- Date range picker:
  - last 7 days,
  - last 30 days,
  - this month,
  - custom.
- Brand selector.
- Channel/platform multi-select:
  - Facebook,
  - Instagram,
  - TikTok,
  - others khi backend ho tro.
- Campaign selector.
- Metric selector cho chart.

Moi API analytics phai gui filter dong nhat va `X-Profile-Id`.

### 6.3. Overview cards

Cards nen hien:

- Impressions.
- Reach.
- Engagement.
- Clicks.
- CTR.
- Spend hoac Estimated revenue neu backend co.
- Published posts.
- Active campaigns.

Moi card can hien:

- Current value.
- Delta so voi previous period neu backend tra `changes`.
- Empty/unknown state neu metric khong co.

### 6.4. Charts

Charts can co:

- Time series line chart.
- Channel breakdown bar chart.
- Campaign comparison table/bar.
- Top posts table.

Metric khong co data thi hien placeholder `No data for this metric`.

### 6.5. Channel breakdown

Table/channel section can hien:

- Platform icon/name.
- Connected target/page/account name.
- Impressions.
- Reach.
- Engagement.
- Clicks.
- CTR.
- Spend.
- Last sync time.
- Status badge:
  - healthy,
  - partial,
  - not configured,
  - permission issue.

### 6.6. Campaign analytics

Campaign table can co:

- Campaign name.
- Platform.
- Objective.
- Status.
- Budget.
- Spend.
- Impressions.
- Engagement.
- CTR.
- Estimated revenue.
- Actions:
  - View details,
  - Open ads campaign neu US-60 da implement.

Neu Ads API chua active, FE hien empty state: `Campaign analytics will appear after ads campaign data is synced.`

## 7. Business rules

- User phai authenticated va co active profile.
- Tat ca analytics API phai gui `Authorization` va `X-Profile-Id`.
- User chi duoc xem analytics cua profile hien tai.
- Brand/campaign/post filter phai thuoc profile hien tai.
- Metric tu social provider co the partial hoac unavailable do app review/permission.
- FE khong tinh lai metric nhay cam neu backend da aggregate; FE chi format va hien thi.
- Date range qua lon nen backend enforce limit; FE nen gioi han option mac dinh 30/90 ngay tuy capability.
- Neu provider sync loi, FE hien sync status thay vi coi la zero performance.

## 8. Data model frontend de xuat

```ts
type AnalyticsPlatform = "facebook" | "instagram" | "tiktok" | "twitter" | "google" | "youtube";

type AnalyticsFilters = {
  from: string;
  to: string;
  brandId?: string;
  platform?: AnalyticsPlatform;
  campaignId?: string;
  contentType?: "TextOnly" | "ImageText" | "VideoText";
};

type AnalyticsTotals = {
  impressions?: number;
  reach?: number;
  engagement?: number;
  clicks?: number;
  ctr?: number;
  spend?: number;
  estimatedRevenue?: number;
  publishedPosts?: number;
  activeCampaigns?: number;
};

type AnalyticsPoint = AnalyticsTotals & {
  date: string;
};

type ChannelBreakdownItem = AnalyticsTotals & {
  platform: AnalyticsPlatform;
  integrationId?: string;
  displayName?: string;
  lastSyncedAt?: string | null;
  status?: "healthy" | "partial" | "not_configured" | "permission_issue" | "error";
};

type CampaignAnalyticsItem = AnalyticsTotals & {
  campaignId: string;
  name: string;
  platform?: AnalyticsPlatform;
  objective?: string | null;
  status?: string | null;
  budget?: number | null;
};
```

## 9. Acceptance criteria

### AC1 - Xem analytics overview

Given user da dang nhap va co active profile  
When user mo man hinh analytics  
Then FE goi analytics overview voi `X-Profile-Id`  
And hien cac metric tong quan trong date range mac dinh.

### AC2 - Filter theo date range

Given analytics da load  
When user doi date range  
Then FE reload overview, time series, channel breakdown va campaign breakdown theo range moi.

### AC3 - Filter theo kenh

Given backend co du lieu nhieu platform  
When user chon platform filter  
Then FE chi hien metric cua platform duoc chon  
And chart/table cap nhat dong nhat.

### AC4 - Hien breakdown theo campaign

Given backend tra campaign analytics  
When user vao tab Campaigns  
Then FE hien bang campaign voi impressions, engagement, CTR, spend va status.

### AC5 - Hien top posts

Given backend tra top posts  
When user chon metric `engagement`  
Then FE hien danh sach posts sap xep theo engagement.

### AC6 - Backend chi co summary MVP

Given backend chua co `/api/analytics/*`  
When user mo analytics nang cao  
Then FE hien dashboard summary co ban neu `/api/dashboard/summary` thanh cong  
And hien message `Advanced analytics is not enabled yet` cho cac section nang cao.

### AC7 - Provider partial/unavailable

Given sync status cua provider la `permission_issue` hoac `not_configured`  
When FE render channel breakdown  
Then FE hien status badge va thong diep phu hop  
And khong coi metric unavailable la zero.

### AC8 - Export CSV

Given analytics table da co data  
When user bam Export  
Then FE export dung du lieu dang filter neu backend/FE support  
And file chi gom rows user dang co quyen xem.

## 10. Error handling

| Truong hop | Xu ly frontend |
| --- | --- |
| `401 Unauthorized` | Chuyen user ve login hoac refresh token |
| `403 Forbidden` | Hien forbidden cho profile hien tai |
| `404 Brand/Campaign not found` | Clear filter khong hop le va hien message |
| `400 Invalid date range` | Hien validation error tai date picker |
| `501 Analytics not implemented` | Fallback dashboard summary va hien advanced unavailable |
| `503 Provider sync unavailable` | Hien sync status partial/error, giu data cu neu co |
| Empty data | Hien empty state, khong hien zero misleading |
| Network error | Hien retry va giu filters |

## 11. Test cases frontend

- Load `/analytics` goi overview/time-series/channel APIs voi `X-Profile-Id`.
- Date range change reload tat ca sections lien quan.
- Platform filter cap nhat query params/API params.
- Campaign tab render paged campaign data.
- Top posts tab render sort theo metric.
- Backend `501` hien advanced unavailable va fallback summary.
- Provider `permission_issue` hien status badge.
- Empty response hien empty state dung.
- Invalid date range khong goi API va hien validation.
- Export CSV chi gom rows hien tai/dang filter.

## 12. Dependency va blocker

- Backend can implement analytics controller/service/repository queries.
- Backend can mo rong `PerformanceReportRepository` de aggregate theo date, channel, post, ad/campaign.
- Backend can co job sync metrics tu social APIs hoac ingest metrics tu provider.
- Backend can expose mapping social integration -> channel display name.
- Backend can expose campaign/ad analytics neu US-60 Ads MVP active.
- Backend can quyet dinh metric canonical:
  - reach,
  - impressions,
  - engagement,
  - clicks,
  - CTR,
  - spend,
  - revenue/conversions.
- Backend can phan biet metric zero va metric unavailable.
- Can permission/app review cho social analytics scopes, dac biet ngoai Facebook.

## 13. Definition of Done

- FE co man hinh analytics nang cao voi overview, time series, channel breakdown, campaign breakdown va top posts.
- FE filter theo date range, brand, channel va campaign.
- FE graceful khi backend chi co dashboard summary MVP.
- FE hien sync status/partial data ro rang.
- FE co test cho filters, empty state, backend not implemented, provider unavailable va export.
- Khong fetch social provider analytics truc tiep tu frontend; tat ca di qua backend API.
