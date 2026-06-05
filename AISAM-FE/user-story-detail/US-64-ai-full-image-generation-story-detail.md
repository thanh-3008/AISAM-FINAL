# US-64 - Sinh anh AI day du

## 1. Thong tin user story

**Ma story:** US-64  
**Ten story:** Sinh anh AI day du  
**Vai tro:** Nguoi dung marketing / content creator  
**Muc tieu:** Sinh anh quang cao hoan chinh bang AI cho content dang `ImageText` khi Vertex Imagen va storage service da san sang.  
**Mo ta goc:** La nguoi dung, toi muon AI sinh anh quang cao hoan chinh cho content dang ImageText khi da san sang Vertex va storage.

## 2. Boi canh tu requirement va backend hien tai

Requirement co dinh huong ho tro content type:

- `TextOnly`: sinh text.
- `ImageText`: sinh text va anh AI.
- `VideoText`: sinh text va video khi co provider phu hop.

Requirement cung neu he thong co the dung Gemini, Vertex AI Imagen va storage service, nhung khong tu huan luyen model rieng.

Trang thai backend hien tai:

- Endpoint AI active: `POST /api/ai/generate-draft`, `POST /api/ai/improve/{contentId}`, `POST /api/ai/approve/{aiGenerationId}`, `GET /api/ai/generations/{contentId}`, `POST /api/ai/chat`.
- `CreateDraftRequest` da co `AdType`, `BrandId`, `ProductId`, `Title`, `Prompt`.
- `AdTypeEnum` da co `ImageText = 1`.
- `AiGeneration` entity da co `GeneratedImageUrl` va `GeneratedVideoUrl`.
- `Content` entity da co `ImageUrl` dang JSONB string.
- `Asset` entity da co metadata cho storage file va `AssetTypeEnum.Image`.
- `DefaultBucketEnum` da co `AiGenerated`.
- `Subscription` schema co `quota_ai_images_per_day`.

Han che backend hien tai:

- `AIService` moi goi `IGeminiTextClient` de sinh text.
- `AiGenerationResponse` hien chua tra `GeneratedImageUrl`, `GeneratedVideoUrl`, `AssetId`, image metadata.
- Chua co Vertex Imagen client/service.
- Chua co storage upload service active cho AI generated media.
- CODEBASE_UPDATE ghi ro Phase B khong keo Vertex image generation va Supabase/storage upload vao MVP.
- Neu user tao `ImageText` bang AI endpoint hien tai, backend van chi sinh text va tao content draft, chua sinh anh.

Vi vay US-64 la story frontend can chuan bi cho giai doan backend da co Vertex va storage. FE phai graceful khi backend chua enabled image generation.

## 3. Pham vi frontend

### In scope

- Cho phep user chon content type `ImageText` trong AI content creation flow.
- Hien thi input prompt sinh anh va cac tuy chon anh co ban khi backend capability cho phep.
- Goi API sinh draft co `AdType = ImageText`.
- Hien thi trang thai sinh text va sinh anh:
  - pending,
  - completed,
  - failed,
  - partial success.
- Hien thi preview anh AI tu URL/storage URL backend tra ve.
- Cho phep approve generation de apply text va image vao content draft.
- Hien thi loi ro rang neu Vertex/storage chua cau hinh hoac quota anh AI da het.
- Khong upload/lua chon file thay cho AI generation trong story nay. Upload media thuoc US-61.

### Out of scope

- Implement Vertex Imagen call truc tiep tren frontend.
- Luu base64 image lon tren browser state dai han.
- Quan ly bucket/storage credential tren frontend.
- Sinh video AI.
- Edit anh nang cao nhu inpaint/outpaint neu backend chua co.
- Payment/quota enforcement chi tiet ngoai viec hien thi loi/quota tu backend.

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
  "adType": 1,
  "title": "Summer campaign image",
  "prompt": "Generate Vietnamese ad copy and a product image concept for..."
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

Luu y cho FE: response hien tai chua co image URL. Neu `adType = ImageText` nhung response khong co image field, FE phai hien thi generation la `Text generated only` hoac `Image generation unavailable`.

### 4.2. Approve generation hien tai

```http
POST /api/ai/approve/{aiGenerationId}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Backend hien tai chi apply `GeneratedText` vao `Content.TextContent`. Khi backend mo rong US-64, approve can apply ca `GeneratedImageUrl` vao `Content.ImageUrl`.

### 4.3. Lay lich su generation hien tai

```http
GET /api/ai/generations/{contentId}
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

FE can dung endpoint nay de reload generation history sau khi tao draft, approve, retry hoac quay lai editor.

## 5. API/DTO can mo rong cho US-64

Backend nen mo rong response thay vi tao flow rieng neu co the, de FE co mot AI generation flow thong nhat cho `TextOnly`, `ImageText`, `VideoText`.

### 5.1. De xuat request mo rong

```json
{
  "brandId": "brand-id",
  "productId": "optional-product-id",
  "adType": 1,
  "title": "Summer campaign image",
  "prompt": "Create a complete ad image for...",
  "imageOptions": {
    "aspectRatio": "1:1",
    "style": "product_ad",
    "size": "1024x1024",
    "count": 1,
    "referenceAssetIds": ["optional-asset-id"]
  }
}
```

### 5.2. De xuat response mo rong

```json
{
  "aiGenerationId": "generation-id",
  "contentId": "content-id",
  "generatedText": "Generated ad copy",
  "generatedImageUrl": "https://storage/.../image.png",
  "generatedImageAssetId": "asset-id",
  "imageMimeType": "image/png",
  "imageWidth": 1024,
  "imageHeight": 1024,
  "status": 1,
  "errorMessage": null,
  "createdAt": "2026-06-03T10:00:00Z"
}
```

### 5.3. Capability endpoint de xuat

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
  "videoGeneration": false,
  "imageProvider": "vertex-imagen",
  "storageEnabled": true,
  "limits": {
    "aiImagesRemainingToday": 10,
    "maxImageCount": 4,
    "supportedAspectRatios": ["1:1", "4:5", "9:16", "16:9"]
  }
}
```

Neu backend khong co endpoint capability, FE co the fallback bang cach an advanced image controls va chi hien thong diep khi API tra `503`, `501` hoac message tu backend.

## 6. UX/UI detail

### 6.1. AI content creation form

Khi user chon `ImageText`, form can hien thi:

- Brand selector bat buoc.
- Product selector tuy chon, chi hien product thuoc brand.
- Title input.
- Prompt textarea.
- Image prompt helper hoac section `Image direction`.
- Aspect ratio selector:
  - `1:1`,
  - `4:5`,
  - `9:16`,
  - `16:9`.
- Style selector neu backend ho tro:
  - `Product ad`,
  - `Lifestyle`,
  - `Minimal`,
  - `Social post`,
  - `Custom`.
- Generate button.

Neu image generation chua enabled:

- Van cho user tao `ImageText` neu backend cho phep, nhung hien thong bao: `Image generation is not enabled. AI will generate text only.`
- Disable image-specific controls.

### 6.2. Generation result panel

Sau khi user bam Generate, FE hien:

- Loading state trong khi request dang chay.
- Generated text preview.
- Generated image preview neu co `generatedImageUrl`.
- Placeholder `Image not generated` neu response khong co image.
- Error block neu `status = Failed`.
- CTA:
  - `Approve`
  - `Regenerate`
  - `Edit prompt`
  - `Open content draft`

### 6.3. Image preview

Image preview can co:

- Stable aspect ratio container theo option user da chon.
- Loading skeleton khi image URL dang tai.
- Broken image state neu URL het han/khong truy cap duoc.
- Link mo anh full size neu policy san pham cho phep.
- Badge `AI generated`.

FE khong nen render anh bang base64 neu backend tra URL storage. Neu backend tra signed URL co expiry, FE can reload generation/content detail khi URL het han thay vi cache dai han.

### 6.4. Approve behavior

Khi user approve generation:

1. FE goi `POST /api/ai/approve/{aiGenerationId}`.
2. Backend update content draft.
3. FE dieu huong sang content editor/detail.
4. Editor hien:
   - `TextContent` da apply.
   - `ImageUrl` da co anh AI neu backend support.
   - `AdType = ImageText`.

Neu backend hien tai chi apply text, FE phai hien warning nhe: `Image was generated but was not applied to content. Please refresh or update backend contract.`

## 7. Business rules

- User phai authenticated va co active profile.
- Moi request AI phai gui `Authorization` va `X-Profile-Id`.
- Brand phai thuoc profile hien tai.
- Product neu chon phai thuoc brand.
- `ImageText` yeu cau text va image khi backend capability `imageGeneration = true`.
- Neu Vertex hoac storage chua cau hinh, FE phai hien loi cau hinh/coming soon thay vi retry lien tuc.
- Anh AI phai duoc backend upload vao storage truoc khi tra ve FE.
- FE chi dung URL/asset id backend tra ve, khong luu binary image dai han.
- Quota AI image theo subscription do backend enforce; FE chi hien remaining/limit neu backend tra ve.
- Content sau approve van o `Draft`, khong tu dong publish.

## 8. Data model frontend de xuat

```ts
type AdType = "TextOnly" | "ImageText" | "VideoText";

type AiGenerationStatus = "Pending" | "Completed" | "Failed";

type AiImageOptions = {
  aspectRatio?: "1:1" | "4:5" | "9:16" | "16:9";
  style?: "product_ad" | "lifestyle" | "minimal" | "social_post" | "custom";
  size?: "1024x1024" | "1024x1792" | "1792x1024";
  count?: number;
  referenceAssetIds?: string[];
};

type AiGenerationResult = {
  aiGenerationId: string;
  contentId: string;
  generatedText?: string | null;
  generatedImageUrl?: string | null;
  generatedImageAssetId?: string | null;
  imageMimeType?: string | null;
  imageWidth?: number | null;
  imageHeight?: number | null;
  status: AiGenerationStatus;
  errorMessage?: string | null;
  createdAt: string;
};

type AiCapabilities = {
  textGeneration: boolean;
  imageGeneration: boolean;
  videoGeneration: boolean;
  imageProvider?: "vertex-imagen" | string | null;
  storageEnabled: boolean;
  limits?: {
    aiImagesRemainingToday?: number;
    maxImageCount?: number;
    supportedAspectRatios?: string[];
  };
};
```

## 9. Acceptance criteria

### AC1 - Chon ImageText trong AI generation form

Given user da dang nhap va co active profile  
When user mo AI content creation form  
Then user co the chon content type `ImageText`  
And form hien cac field prompt/text/image phu hop.

### AC2 - Backend chua bat image generation

Given backend capability bao `imageGeneration = false` hoac API tra provider unavailable  
When user chon `ImageText`  
Then FE disable image-specific controls  
And hien thong bao AI hien chi sinh text.

### AC3 - Goi generate draft voi ImageText

Given user da nhap prompt, chon brand va chon `ImageText`  
When user bam Generate  
Then FE goi `POST /api/ai/generate-draft` voi `adType = 1`  
And gui bearer token cung `X-Profile-Id`.

### AC4 - Hien thi anh AI khi backend tra ve image URL

Given backend tra generation completed co `generatedImageUrl`  
When generation result duoc render  
Then FE hien preview anh AI trong container dung aspect ratio  
And hien generated text neu co.

### AC5 - Xu ly partial success

Given backend sinh text thanh cong nhung image generation that bai  
When FE nhan response co text va error image  
Then FE van hien text preview  
And hien loi anh ro rang  
And cho phep retry/regenerate.

### AC6 - Approve generation ImageText

Given generation `ImageText` da completed co text va image  
When user bam Approve  
Then FE goi `POST /api/ai/approve/{aiGenerationId}`  
And sau thanh cong dieu huong den content draft  
And content draft hien text va image neu backend da apply.

### AC7 - Quota het

Given backend tra loi quota AI image da het  
When user generate `ImageText`  
Then FE hien thong bao quota het  
And khong lap lai request tu dong.

### AC8 - Khong luu binary/token storage

Given backend tra image URL/storage URL  
When FE cap nhat state  
Then FE chi luu URL/asset id metadata  
And khong luu base64 image lon hoac storage credential trong browser storage.

## 10. Error handling

| Truong hop | Xu ly frontend |
| --- | --- |
| `401 Unauthorized` | Chuyen user ve login hoac refresh token theo auth flow |
| `403 Forbidden` | Hien thong bao khong co quyen tren profile/brand |
| `404 Brand/Product not found` | Yeu cau user chon lai brand/product |
| `400 Product does not belong to brand` | Hien validation error tai product selector |
| `409 Quota exceeded` | Hien quota message va disable generate neu can |
| `501 Image generation not implemented` | Hien Image generation coming soon |
| `503 Vertex not configured` | Hien loi cau hinh provider, cho phep generate text-only neu backend support |
| `503 Storage not configured` | Hien loi storage, khong hien image preview |
| Broken image URL | Hien fallback preview va CTA refresh |
| Generation `Failed` | Hien `errorMessage`, cho phep edit prompt/regenerate |

## 11. Test cases frontend

- Render AI form co option `ImageText`.
- Khi capability image off, image controls disabled va message dung.
- Generate `ImageText` goi endpoint voi `adType = 1`.
- Response hien tai khong co `generatedImageUrl` duoc render thanh text-only/unsupported image state.
- Response mo rong co `generatedImageUrl` render image preview dung aspect ratio.
- Broken image URL hien fallback.
- Approve generation goi dung endpoint va redirect den content detail/editor.
- Loi brand/product ownership hien dung validation message.
- Quota exceeded khong auto retry.
- Khong ghi base64 image/storage credential vao localStorage/sessionStorage.

## 12. Dependency va blocker

- Backend can implement Vertex Imagen client/service.
- Backend can implement storage service va bucket `AiGenerated`.
- Backend can upload anh sinh ra vao storage truoc khi tra response.
- Backend can mo rong `AiGenerationResponse` de tra `GeneratedImageUrl`, `GeneratedImageAssetId` va metadata anh.
- Backend can mo rong approve flow de apply image vao `Content.ImageUrl`.
- Backend can expose AI capability/quota neu FE can render disabled state chuan.
- Can thong nhat image URL format: public URL, signed URL, hay asset id + download endpoint.
- Can thong nhat quota AI image theo subscription.

## 13. Definition of Done

- FE co UI tao AI content `ImageText` voi image prompt/options.
- FE goi dung AI generation endpoint voi active profile.
- FE graceful khi backend chua bat Vertex/storage.
- FE hien preview anh AI khi backend tra image URL.
- FE support approve generation va mo content draft sau approve.
- FE co test cho enabled/disabled image generation, success, failed, partial success va quota exceeded.
- Khong co token, credential storage hoac binary image lon bi luu dai han tren frontend.
