# US-65 - Sinh video AI

## 1. Thong tin user story

**Ma story:** US-65  
**Ten story:** Sinh video AI  
**Vai tro:** Nguoi dung marketing / content creator  
**Muc tieu:** Tao video asset bang AI de ho tro content dang `VideoText` trong giai doan mo rong.  
**Mo ta goc:** La nguoi dung, toi muon AI tao video asset de ho tro content dang VideoText trong giai doan mo rong.

## 2. Boi canh tu requirement va backend hien tai

Requirement co dinh huong ho tro 3 loai content:

- `TextOnly`: sinh text.
- `ImageText`: sinh text va anh AI.
- `VideoText`: sinh text va mo ta/video asset theo kha nang tung giai doan.

Requirement cung ghi ro AI video generation service la phan giai doan mo rong, chat luong anh/video phu thuoc provider ben thu ba, va he thong khong ho tro chinh sua video hau ky nang cao.

Trang thai backend hien tai:

- `AdTypeEnum` da co `VideoText = 2`.
- `Content` entity da co `VideoUrl`.
- `AiGeneration` entity da co `GeneratedVideoUrl`.
- `Asset` entity da co `AssetTypeEnum.Video`, `StoragePath`, `MimeType`, `SizeBytes`, `Width`, `Height`, `DurationSeconds`, `Metadata`.
- `DefaultBucketEnum` da co `AiGenerated`.
- `ContentController` va publish flow da co notion `VideoText`, Facebook provider co test publish video bang `VideoUrl`.
- AI endpoint hien tai gom:
  - `POST /api/ai/generate-draft`
  - `POST /api/ai/improve/{contentId}`
  - `POST /api/ai/approve/{aiGenerationId}`
  - `GET /api/ai/generations/{contentId}`
  - `POST /api/ai/chat`

Han che backend hien tai:

- `AIService` chi goi Gemini text client, chua co AI video provider.
- `AiGenerationResponse` hien chua tra `GeneratedVideoUrl`, `GeneratedVideoAssetId`, duration hoac video metadata.
- Chua co storage upload service active cho AI generated video.
- Chua co job/polling endpoint cho video generation chay lau.
- CODEBASE_UPDATE xep AI video vao post-MVP / Phase H5, khong nam trong MVP active.

Vi vay frontend cho US-65 can thiet ke feature-ready, graceful khi backend chua enabled video generation, va san sang cho async job flow.

## 3. Pham vi frontend

### In scope

- Cho phep user chon content type `VideoText` trong AI content creation flow.
- Hien thi video prompt va option co ban khi backend capability cho phep.
- Bat dau video generation thong qua backend API.
- Hien thi trang thai video generation:
  - queued,
  - processing,
  - completed,
  - failed,
  - cancelled neu backend ho tro.
- Poll generation/job status neu backend tra async job.
- Hien thi video preview tu URL/storage URL backend tra ve.
- Cho phep approve generation de apply text va video vao content draft.
- Hien thi loi khi video provider/storage/quota chua san sang.

### Out of scope

- Goi AI video provider truc tiep tu frontend.
- Luu video binary/base64 tren browser storage.
- Chinh sua video hau ky nang cao.
- Cat/ghep timeline, subtitle editor, audio mixing.
- Upload media thu cong. Phan nay thuoc US-61.
- Publish video len social neu chua co social integration/publish target phu hop.

## 4. API hien tai co the dung

### 4.1. Sinh draft AI hien tai

```http
POST /api/ai/generate-draft
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request hien tai:

```json
{
  "brandId": "brand-id",
  "productId": "optional-product-id",
  "adType": 2,
  "title": "Product launch video",
  "prompt": "Generate Vietnamese ad copy and a short video concept for..."
}
```

Response hien tai:

```json
{
  "success": true,
  "message": "AI generation processed.",
  "data": {
    "aiGenerationId": "generation-id",
    "contentId": "content-id",
    "generatedText": "Generated ad copy",
    "status": 1,
    "errorMessage": null,
    "createdAt": "2026-06-03T10:00:00Z"
  }
}
```

Luu y cho FE: backend hien tai chua sinh video. Neu `adType = VideoText` nhung response khong co video field, FE phai hien `Text generated only` hoac `Video generation unavailable`.

### 4.2. Approve generation hien tai

```http
POST /api/ai/approve/{aiGenerationId}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Backend hien tai chi apply generated text vao content. Khi US-65 backend hoan tat, approve can apply ca `GeneratedVideoUrl` vao `Content.VideoUrl`.

### 4.3. Lay lich su generation hien tai

```http
GET /api/ai/generations/{contentId}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

FE can dung endpoint nay de reload generation history va cap nhat preview neu backend mo rong response co video URL.

## 5. API/DTO can mo rong cho US-65

Do video generation co the chay lau, backend nen ho tro async job/polling thay vi request synchronous duy nhat.

### 5.1. Capability endpoint de xuat

```http
GET /api/ai/capabilities
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response:

```json
{
  "textGeneration": true,
  "imageGeneration": true,
  "videoGeneration": true,
  "videoProvider": "third-party-video-provider",
  "storageEnabled": true,
  "limits": {
    "aiVideosRemainingToday": 3,
    "maxVideoDurationSeconds": 15,
    "supportedAspectRatios": ["9:16", "1:1", "16:9"],
    "supportedFormats": ["mp4"]
  }
}
```

### 5.2. Request sinh VideoText de xuat

```http
POST /api/ai/generate-draft
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

Request mo rong:

```json
{
  "brandId": "brand-id",
  "productId": "optional-product-id",
  "adType": 2,
  "title": "Product launch video",
  "prompt": "Create a short product ad video for...",
  "videoOptions": {
    "aspectRatio": "9:16",
    "durationSeconds": 10,
    "style": "product_ad",
    "format": "mp4",
    "referenceAssetIds": ["optional-image-or-video-asset-id"]
  }
}
```

Response khi async:

```json
{
  "aiGenerationId": "generation-id",
  "contentId": "content-id",
  "generatedText": "Generated ad copy",
  "status": "Queued",
  "jobId": "video-job-id",
  "pollAfterSeconds": 5,
  "createdAt": "2026-06-03T10:00:00Z"
}
```

### 5.3. Poll video generation status de xuat

```http
GET /api/ai/generations/{aiGenerationId}/status
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Response dang xu ly:

```json
{
  "aiGenerationId": "generation-id",
  "contentId": "content-id",
  "status": "Processing",
  "progressPercent": 45,
  "estimatedSecondsRemaining": 30,
  "generatedText": "Generated ad copy",
  "generatedVideoUrl": null,
  "errorMessage": null
}
```

Response hoan tat:

```json
{
  "aiGenerationId": "generation-id",
  "contentId": "content-id",
  "status": "Completed",
  "progressPercent": 100,
  "generatedText": "Generated ad copy",
  "generatedVideoUrl": "https://storage/.../video.mp4",
  "generatedVideoAssetId": "asset-id",
  "videoMimeType": "video/mp4",
  "videoWidth": 1080,
  "videoHeight": 1920,
  "durationSeconds": 10,
  "errorMessage": null
}
```

### 5.4. Cancel video generation neu backend ho tro

```http
POST /api/ai/generations/{aiGenerationId}/cancel
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

FE chi hien nut cancel khi capability/response cho biet job co the cancel.

## 6. UX/UI detail

### 6.1. AI content creation form

Khi user chon `VideoText`, form can hien:

- Brand selector bat buoc.
- Product selector tuy chon.
- Title input.
- Prompt textarea.
- Video direction section.
- Aspect ratio selector:
  - `9:16` cho short-form,
  - `1:1`,
  - `16:9`.
- Duration selector theo backend limit:
  - 5s,
  - 10s,
  - 15s.
- Style selector neu backend ho tro:
  - `Product ad`,
  - `Lifestyle`,
  - `Explainer`,
  - `Social short`,
  - `Custom`.
- Reference assets selector neu US-61 da hoan tat.

Neu `videoGeneration = false`:

- Disable video-specific controls.
- Hien thong bao `AI video generation is not enabled yet. You can still create VideoText content manually if backend allows VideoUrl.`

### 6.2. Generation progress

Video generation can lau, nen FE can co progress panel:

- `Queued`: dang cho provider.
- `Processing`: dang render video.
- `Completed`: co video preview.
- `Failed`: co error message.
- `Cancelled`: user da huy hoac backend huy.

Polling:

- Bat dau poll khi response co `jobId` hoac status `Queued/Processing`.
- Ton trong `pollAfterSeconds` neu backend tra ve.
- Dung exponential backoff nhe neu network loi.
- Dung polling khi user roi khoi trang hoac generation completed/failed/cancelled.

### 6.3. Video preview

Khi co `generatedVideoUrl`, FE hien:

- HTML5 video player voi controls.
- Poster/thumbnail neu backend tra ve.
- Duration va format.
- Badge `AI generated`.
- CTA:
  - `Approve`,
  - `Regenerate`,
  - `Download` neu policy cho phep,
  - `Open content draft`.

Preview container can co aspect ratio on dinh de tranh layout shift.

### 6.4. Approve behavior

Khi user approve generation:

1. FE goi `POST /api/ai/approve/{aiGenerationId}`.
2. Backend update content draft.
3. FE dieu huong sang content editor/detail.
4. Editor hien:
   - `TextContent` da apply.
   - `VideoUrl` da co video AI neu backend support.
   - `AdType = VideoText`.

Neu backend chi apply text, FE can reload content va hien warning nhe: `Video was generated but was not applied to content. Backend approve contract may need update.`

## 7. Business rules

- User phai authenticated va co active profile.
- Moi AI request phai gui `Authorization` va `X-Profile-Id`.
- Brand phai thuoc profile hien tai.
- Product neu chon phai thuoc brand.
- Video generation chi enabled khi backend capability `videoGeneration = true` va `storageEnabled = true`.
- Backend phai upload video vao storage truoc khi tra `generatedVideoUrl`.
- FE chi luu metadata URL/asset id, khong luu binary video dai han.
- Quota video AI do backend enforce; FE chi hien quota/remaining neu backend tra.
- Content sau approve van la `Draft`, khong tu dong publish.
- Neu video provider gioi han duration/aspect ratio/format, FE phai chi hien option backend support.

## 8. Data model frontend de xuat

```ts
type AdType = "TextOnly" | "ImageText" | "VideoText";

type AiGenerationStatus =
  | "Pending"
  | "Queued"
  | "Processing"
  | "Completed"
  | "Failed"
  | "Cancelled";

type AiVideoOptions = {
  aspectRatio?: "9:16" | "1:1" | "16:9";
  durationSeconds?: 5 | 10 | 15;
  style?: "product_ad" | "lifestyle" | "explainer" | "social_short" | "custom";
  format?: "mp4";
  referenceAssetIds?: string[];
};

type AiVideoGenerationResult = {
  aiGenerationId: string;
  contentId: string;
  jobId?: string | null;
  generatedText?: string | null;
  generatedVideoUrl?: string | null;
  generatedVideoAssetId?: string | null;
  videoMimeType?: string | null;
  videoWidth?: number | null;
  videoHeight?: number | null;
  durationSeconds?: number | null;
  status: AiGenerationStatus;
  progressPercent?: number | null;
  estimatedSecondsRemaining?: number | null;
  errorMessage?: string | null;
  createdAt: string;
};

type AiCapabilities = {
  textGeneration: boolean;
  imageGeneration: boolean;
  videoGeneration: boolean;
  videoProvider?: string | null;
  storageEnabled: boolean;
  limits?: {
    aiVideosRemainingToday?: number;
    maxVideoDurationSeconds?: number;
    supportedAspectRatios?: string[];
    supportedFormats?: string[];
  };
};
```

## 9. Acceptance criteria

### AC1 - Chon VideoText trong AI form

Given user da dang nhap va co active profile  
When user mo AI content creation form  
Then user co the chon `VideoText`  
And form hien video prompt/options neu backend enabled.

### AC2 - Backend chua bat video generation

Given backend capability bao `videoGeneration = false` hoac API tra not implemented  
When user chon `VideoText`  
Then FE disable video-specific controls  
And hien thong bao video AI chua san sang.

### AC3 - Bat dau sinh video

Given backend da enabled video generation va user nhap day du thong tin  
When user bam Generate  
Then FE goi AI generation endpoint voi `adType = 2`  
And gui bearer token cung `X-Profile-Id`.

### AC4 - Poll trang thai video

Given backend tra `Queued` hoac `Processing`  
When generation dang chay  
Then FE poll status theo `pollAfterSeconds` hoac interval cau hinh  
And cap nhat progress tren UI.

### AC5 - Hien thi video khi completed

Given backend tra `Completed` voi `generatedVideoUrl`  
When result panel render  
Then FE hien video player co controls  
And hien duration/format neu backend tra metadata.

### AC6 - Xu ly failed generation

Given backend tra `Failed` voi `errorMessage`  
When FE render result  
Then FE hien loi ro rang  
And cho phep user sua prompt hoac regenerate.

### AC7 - Approve VideoText generation

Given video generation completed  
When user bam Approve  
Then FE goi `POST /api/ai/approve/{aiGenerationId}`  
And dieu huong den content draft  
And content draft hien `VideoUrl` neu backend da apply.

### AC8 - Khong luu video binary

Given backend tra video storage URL  
When FE cap nhat state  
Then FE chi luu URL/asset id metadata  
And khong luu video binary/base64 trong localStorage/sessionStorage.

## 10. Error handling

| Truong hop | Xu ly frontend |
| --- | --- |
| `401 Unauthorized` | Chuyen user ve login hoac refresh token |
| `403 Forbidden` | Hien khong co quyen tren profile/brand |
| `404 Brand/Product not found` | Yeu cau chon lai brand/product |
| `400 Product does not belong to brand` | Hien validation error tai product selector |
| `409 Quota exceeded` | Hien quota het va disable generate neu can |
| `501 Video generation not implemented` | Hien coming soon cho AI video |
| `503 Video provider not configured` | Hien provider unavailable |
| `503 Storage not configured` | Hien storage unavailable, khong render preview |
| Poll timeout | Hien trang thai dang xu ly lau va CTA refresh |
| Broken video URL | Hien fallback va CTA reload generation |
| Generation `Failed` | Hien error message va CTA regenerate |

## 11. Test cases frontend

- Render AI form co option `VideoText`.
- Capability video off thi controls disabled va hien message dung.
- Generate `VideoText` goi endpoint voi `adType = 2`.
- Response hien tai khong co `generatedVideoUrl` duoc render thanh text-only/video unavailable state.
- Response `Queued/Processing` bat dau polling.
- Poll completed co `generatedVideoUrl` render HTML5 video player.
- Poll failed hien `errorMessage` va CTA regenerate.
- Approve generation goi dung endpoint va redirect den content detail/editor.
- Quota exceeded khong auto retry.
- Khong ghi video binary/storage credential vao browser storage.

## 12. Dependency va blocker

- Backend can implement AI video provider client/service.
- Backend can implement storage service cho generated video.
- Backend can upload video vao bucket `AiGenerated` va tao asset `AssetTypeEnum.Video`.
- Backend can mo rong `AiGenerationResponse` de tra `GeneratedVideoUrl`, asset id va metadata.
- Backend can mo rong approve flow de apply video vao `Content.VideoUrl`.
- Backend can ho tro async job/polling cho video generation.
- Backend can expose capability/quota de FE render controls dung.
- Can thong nhat gioi han duration, aspect ratio, format, file size va signed URL/public URL.

## 13. Definition of Done

- FE co UI tao AI content `VideoText` voi video prompt/options.
- FE graceful khi backend chua bat AI video.
- FE support async polling/job status cho video generation.
- FE hien video preview khi backend tra `generatedVideoUrl`.
- FE approve generation va mo content draft sau approve.
- FE co test cho disabled state, queued/processing, completed, failed, quota exceeded va approve flow.
- FE khong luu binary video, provider token hoac storage credential tren browser.
