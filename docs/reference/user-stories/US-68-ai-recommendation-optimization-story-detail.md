# US-68 - Ho tro AI recommendation va optimization

## 1. Thong tin user story

**Ma story:** US-68  
**Ten story:** Ho tro AI recommendation va optimization  
**Vai tro:** Nguoi dung marketing / marketer / brand owner  
**Muc tieu:** Nhan de xuat chien luoc hoac toi uu tu dong sau khi MVP da on dinh.  
**Mo ta goc:** La nguoi dung, toi muon nhan de xuat chien luoc hoac toi uu tu dong sau khi MVP da on dinh.

## 2. Boi canh tu requirement va backend hien tai

Requirement xep analytics nang cao, du doan xu huong, toi uu chien luoc tu dong, Dynamic Ads, Pixel optimization va A/B testing tu dong vao pham vi mo rong, khong phai MVP can co ngay. Requirement cung neu AI co the dung provider co san nhu Gemini va cac social/analytics API, nhung khong tu huan luyen model rieng.

Trang thai backend hien tai:

- `GeminiController` chi expose:
  - `POST /api/ai/generate-draft`
  - `POST /api/ai/improve/{contentId}`
  - `POST /api/ai/approve/{aiGenerationId}`
  - `GET /api/ai/generations/{contentId}`
  - `POST /api/ai/chat`
- `AIService` chi dung `IGeminiTextClient` de sinh text, improve content va chat.
- `DashboardController` chi co `GET /api/dashboard/summary`.
- `PerformanceReport` entity da co metric co ban: impressions, engagement, CTR, estimated revenue, report date, raw data.
- `PerformanceReportRepository` hien chi co count theo profile, chua co aggregate analytics.
- Ads entities (`AdCampaign`, `AdSet`, `Ad`, `AdCreative`) da ton tai nhung Ads API/optimization service chua active.
- CODEBASE_UPDATE ghi ro dashboard hien la summary MVP, chua co performance analytics nang cao; auto-optimization nam Phase H5/post-MVP.

Vi vay frontend cua US-68 can duoc thiet ke de san sang nhan recommendation tu backend sau nay, dong thoi graceful khi backend chua enabled.

## 3. Pham vi frontend

### In scope

- Tao UI `Recommendations` hoac section trong Analytics/Dashboard.
- Hien danh sach AI recommendations theo profile.
- Filter recommendation theo:
  - brand,
  - channel/platform,
  - campaign,
  - priority,
  - type,
  - status.
- Hien chi tiet recommendation gom ly do, metric evidence, expected impact va action de xuat.
- Cho phep user accept/dismiss/save recommendation.
- Cho phep apply recommendation khi backend ho tro va action an toan.
- Hien guard/confirm truoc moi action co the thay doi campaign, budget, schedule hoac content.
- Hien fallback `AI recommendations are not enabled yet` neu backend chua co endpoint.

### Out of scope

- Tu dong sua campaign/budget ma khong co xac nhan user.
- Goi social/ads provider truc tiep tu frontend.
- Tu tinh recommendation bang logic phuc tap tren client.
- Real-time optimization loop.
- A/B testing tu dong neu backend chua co experiment service.
- Pixel/conversion optimization neu tracking backend chua san sang.

## 4. API hien tai co the dung

### 4.1. AI chat hien tai

```http
POST /api/ai/chat
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request:

```json
{
  "brandId": "optional-brand-id",
  "productId": "optional-product-id",
  "adType": 0,
  "message": "Suggest how to improve this campaign",
  "conversationId": null
}
```

Endpoint nay co the dung cho manual advisory chat, nhung khong phai recommendation engine vi:

- Khong lay analytics/campaign evidence co cau truc.
- Khong tra recommendation type/priority/status/action.
- Khong co apply/dismiss workflow.

### 4.2. Dashboard summary hien tai

```http
GET /api/dashboard/summary
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Chi du cho count co ban, khong du de sinh optimization recommendation.

## 5. API/DTO de xuat cho US-68

### 5.1. Lay danh sach recommendations

```http
GET /api/recommendations?brandId=&platform=&campaignId=&type=&priority=&status=open&page=1&pageSize=20
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "items": [
    {
      "id": "recommendation-id",
      "type": "budget_optimization",
      "priority": "high",
      "status": "open",
      "title": "Shift budget to higher CTR ad set",
      "summary": "Ad set A has 42% higher CTR than Ad set B in the last 7 days.",
      "brandId": "brand-id",
      "brandName": "AISAM",
      "platform": "facebook",
      "campaignId": "campaign-id",
      "campaignName": "Summer Launch",
      "confidence": 0.82,
      "expectedImpact": {
        "metric": "ctr",
        "direction": "increase",
        "estimatedChangePct": 12.5
      },
      "createdAt": "2026-06-03T10:00:00Z",
      "expiresAt": "2026-06-10T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1
}
```

### 5.2. Recommendation detail

```http
GET /api/recommendations/{recommendationId}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "id": "recommendation-id",
  "type": "content_optimization",
  "priority": "medium",
  "status": "open",
  "title": "Rewrite CTA for lower click-through posts",
  "summary": "Posts with direct offer CTA are outperforming educational CTA.",
  "rationale": "Based on the last 30 days of Facebook page posts, offer-led copy has higher engagement and CTR.",
  "evidence": [
    {
      "label": "Offer CTA CTR",
      "value": 0.021,
      "comparisonValue": 0.014,
      "unit": "ratio"
    }
  ],
  "suggestedActions": [
    {
      "id": "action-id",
      "type": "create_content_variant",
      "label": "Create a CTA-focused variant",
      "riskLevel": "low",
      "requiresConfirmation": true,
      "payloadPreview": {
        "contentId": "content-id",
        "prompt": "Rewrite this post with a clearer offer-led CTA."
      }
    }
  ],
  "relatedEntities": {
    "brandId": "brand-id",
    "campaignId": "campaign-id",
    "contentIds": ["content-id"],
    "postIds": ["post-id"]
  },
  "createdAt": "2026-06-03T10:00:00Z",
  "updatedAt": "2026-06-03T10:00:00Z"
}
```

### 5.3. Generate recommendations on demand

```http
POST /api/recommendations/generate
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request:

```json
{
  "brandId": "optional-brand-id",
  "campaignId": "optional-campaign-id",
  "from": "2026-05-04",
  "to": "2026-06-03",
  "scope": "profile",
  "types": ["content_optimization", "budget_optimization", "schedule_optimization"]
}
```

Response:

```json
{
  "jobId": "recommendation-job-id",
  "status": "queued",
  "pollAfterSeconds": 5
}
```

### 5.4. Poll generation job

```http
GET /api/recommendations/jobs/{jobId}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "jobId": "recommendation-job-id",
  "status": "completed",
  "createdRecommendationsCount": 3,
  "errorMessage": null
}
```

### 5.5. Dismiss/save recommendation

```http
POST /api/recommendations/{recommendationId}/dismiss
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request:

```json
{
  "reason": "not_relevant",
  "note": "Campaign already ended."
}
```

```http
POST /api/recommendations/{recommendationId}/save
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

### 5.6. Apply suggested action

```http
POST /api/recommendations/{recommendationId}/actions/{actionId}/apply
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request:

```json
{
  "confirmed": true,
  "clientReviewNote": "Approved by marketer after review."
}
```

Response:

```json
{
  "applied": true,
  "actionType": "create_content_variant",
  "resultEntityId": "new-content-id",
  "message": "Content variant created."
}
```

## 6. UX/UI detail

### 6.1. Recommendations page

Route de xuat:

- `/recommendations`
- hoac tab trong `/analytics/recommendations`

Layout gom:

- Header voi date range/brand filter.
- Button `Generate recommendations`.
- Summary counters:
  - Open,
  - High priority,
  - Saved,
  - Applied.
- Recommendation list/table.
- Detail drawer hoac detail page.

### 6.2. Recommendation card/table

Moi item can hien:

- Priority badge.
- Type badge:
  - Content,
  - Schedule,
  - Channel,
  - Budget,
  - Audience,
  - Creative,
  - Campaign.
- Title.
- Short summary.
- Confidence.
- Expected impact.
- Related brand/campaign/platform.
- Created/expiry date.
- Actions:
  - View detail,
  - Save,
  - Dismiss,
  - Apply neu co supported action.

### 6.3. Detail drawer

Detail drawer can hien:

- Rationale.
- Evidence metrics.
- Related posts/campaigns.
- Suggested actions.
- Risk level.
- Backend data freshness.
- CTA:
  - Apply,
  - Create draft from recommendation,
  - Open related campaign/content,
  - Dismiss.

### 6.4. Apply flow

Moi action thay doi du lieu phai co confirm:

1. User bam Apply.
2. FE hien modal review action.
3. Modal hien payload preview va risk level.
4. User confirm.
5. FE goi apply endpoint.
6. FE hien result va link den entity duoc tao/cap nhat.

Khong auto apply neu action co `riskLevel = medium/high` hoac `requiresConfirmation = true`.

### 6.5. Backend not enabled state

Neu backend tra `404`/`501` cho recommendations endpoints:

- Hien empty state: `AI recommendations are not enabled yet.`
- Co CTA phu: `Use AI chat` neu `/api/ai/chat` active.
- Khong hien UI apply optimization.

## 7. Business rules

- User phai authenticated va co active profile.
- Moi request phai gui `Authorization` va `X-Profile-Id`.
- Recommendation chi duoc hien neu lien quan profile hien tai.
- Recommendation dua tren analytics partial phai hien data freshness/partial warning.
- FE khong auto apply optimization khi chua co confirmation.
- Action budget/campaign/status can duoc backend validate quyen va quota.
- Recommendation het han nen duoc an khoi default open list hoac hien badge expired.
- Dismiss/save/apply phai cap nhat status local va refetch neu can.
- Neu AI provider unavailable, FE hien loi ro rang va giu danh sach cu.

## 8. Data model frontend de xuat

```ts
type RecommendationType =
  | "content_optimization"
  | "schedule_optimization"
  | "channel_optimization"
  | "budget_optimization"
  | "audience_optimization"
  | "creative_optimization"
  | "campaign_strategy";

type RecommendationPriority = "low" | "medium" | "high";
type RecommendationStatus = "open" | "saved" | "dismissed" | "applied" | "expired";
type RiskLevel = "low" | "medium" | "high";

type Recommendation = {
  id: string;
  type: RecommendationType;
  priority: RecommendationPriority;
  status: RecommendationStatus;
  title: string;
  summary: string;
  brandId?: string | null;
  brandName?: string | null;
  platform?: string | null;
  campaignId?: string | null;
  campaignName?: string | null;
  confidence?: number | null;
  expectedImpact?: {
    metric: string;
    direction: "increase" | "decrease";
    estimatedChangePct?: number | null;
  };
  createdAt: string;
  expiresAt?: string | null;
};

type SuggestedAction = {
  id: string;
  type:
    | "create_content_variant"
    | "adjust_schedule"
    | "adjust_budget"
    | "pause_campaign"
    | "create_experiment"
    | "manual_review";
  label: string;
  riskLevel: RiskLevel;
  requiresConfirmation: boolean;
  payloadPreview?: Record<string, unknown>;
};
```

## 9. Acceptance criteria

### AC1 - Xem danh sach recommendations

Given user da dang nhap va co active profile  
When user mo `/recommendations`  
Then FE goi API recommendations voi `X-Profile-Id`  
And hien danh sach recommendations theo status mac dinh `open`.

### AC2 - Backend chua enabled

Given backend chua co recommendations endpoint  
When user mo recommendations page  
Then FE hien `AI recommendations are not enabled yet`  
And co the hien CTA dung AI chat neu endpoint chat active.

### AC3 - Filter recommendations

Given danh sach recommendations da load  
When user doi filter brand/platform/type/priority/status  
Then FE reload danh sach voi query params tuong ung.

### AC4 - Xem detail recommendation

Given user click mot recommendation  
When detail drawer mo  
Then FE hien rationale, evidence, expected impact va suggested actions.

### AC5 - Generate recommendations on demand

Given backend ho tro generate job  
When user bam Generate recommendations  
Then FE goi generate endpoint  
And poll job status den khi completed/failed.

### AC6 - Dismiss recommendation

Given recommendation dang open  
When user dismiss voi reason  
Then FE goi dismiss endpoint  
And item chuyen sang status `dismissed`.

### AC7 - Apply action co confirmation

Given recommendation co suggested action `requiresConfirmation = true`  
When user bam Apply  
Then FE hien confirm modal truoc  
And chi goi apply endpoint sau khi user confirm.

### AC8 - Apply success

Given backend apply action thanh cong  
When FE nhan response  
Then FE hien success message  
And cung cap link den entity duoc tao/cap nhat neu co.

## 10. Error handling

| Truong hop | Xu ly frontend |
| --- | --- |
| `401 Unauthorized` | Chuyen user ve login hoac refresh token |
| `403 Forbidden` | Hien forbidden cho profile hien tai |
| `404 Recommendation not found` | Hien not found va quay lai list |
| `409 Recommendation expired` | Hien expired badge va disable apply |
| `409 Action already applied` | Refetch detail va cap nhat status |
| `422 Unsafe action` | Hien validation/risk message tu backend |
| `501 Not implemented` | Hien recommendations not enabled |
| `503 AI provider unavailable` | Hien provider unavailable, giu data cu |
| Job timeout | Hien job dang xu ly lau va CTA refresh |

## 11. Test cases frontend

- Recommendations route goi API voi `X-Profile-Id`.
- Backend `501` hien not-enabled state va CTA AI chat.
- Filter brand/platform/type/priority/status cap nhat API params.
- Detail drawer render rationale/evidence/actions.
- Generate recommendations start job va poll status.
- Poll completed refetch recommendations.
- Dismiss action goi dung endpoint va cap nhat status.
- Apply action co confirm modal.
- Apply success hien link result entity.
- Expired recommendation disable apply.
- Provider unavailable hien error nhung khong xoa data cu.

## 12. Dependency va blocker

- Backend can implement recommendation controller/service/repository.
- Backend can co analytics aggregation du de lam evidence.
- Backend can co AI orchestration prompt dua tren metrics, content, campaign va brand context.
- Backend can luu recommendation status: open/saved/dismissed/applied/expired.
- Backend can co suggested action schema va apply handlers an toan.
- Backend can ho tro job async neu recommendation generation lau.
- Backend can co quota/enforcement cho AI recommendation generation.
- Backend can ho tro Ads/campaign APIs neu action lien quan budget/campaign.
- Can policy ro rang: de xuat chi la advisory hay co the auto-apply sau confirmation.

## 13. Definition of Done

- FE co recommendations page/list/detail.
- FE graceful khi backend chua enabled recommendations.
- FE filter, save, dismiss va apply recommendation theo contract.
- FE co confirm guard cho moi optimization action co rui ro.
- FE co test cho not-enabled state, filters, detail, generate job, dismiss, apply va provider unavailable.
- FE khong fetch social/ads provider truc tiep; moi recommendation va optimization action di qua backend.
