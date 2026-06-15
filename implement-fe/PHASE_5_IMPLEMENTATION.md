# Phase 5 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `5.1` den `5.6` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu truc tiep voi backend Content, AI va Conversation hien tai trong `AISAM-BE`.

Pham vi Phase 5:

- Hoan thien content list, create, edit, detail, clone, soft delete, restore
- Hoan thien AI generate draft, improve content, approve generation
- Hoan thien AI chat va conversation list/detail/delete
- Dat context dung cho active profile, brand, product de Phase 6 publish/social tiep tuc dung lai
- Chot ro day la phase tao/noi dung truoc approval workflow day du cua target product

Khong lam trong Phase 5:

- Facebook connect va linking
- Publish content len social
- Posts history
- Notifications, Scheduling
- Payment/Team/Approval/Ads

Luu y target product:

- `requirement.md` yeu cau content phai qua approval truoc khi publish/schedule.
- Vi vay Phase 5 khong duoc de nguoi implement hieu rang AI approve generation = business approve content.
- `approve generation` o phase nay chi la chap nhan output AI de ghi vao content, khong thay the approval workflow cua team/approver.

Can cu backend da doi chieu truc tiep cho Phase 5:

- `AISAM-BE/AISAM.API/Controllers/ContentController.cs`
- `AISAM-BE/AISAM.Services/Service/ContentService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ContentRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateContentRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateContentRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ContentResponseDto.cs`
- `AISAM-BE/AISAM.API/Controllers/GeminiController.cs`
- `AISAM-BE/AISAM.Services/Service/AIService.cs`
- `AISAM-BE/AISAM.Common/Models/GeminiModels.cs`
- `AISAM-BE/AISAM.API/Controllers/ConversationController.cs`
- `AISAM-BE/AISAM.Services/Service/ConversationService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ConversationRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ConversationResponseDto.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ConversationDetailDto.cs`
- `AISAM-BE/AISAM.Data/Enumeration/AdTypeEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/ContentStatusEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/AiStatusEnum.cs`
- `AISAM-BE/AISAM.Common/Dtos/PaginationDtos.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`

## Tong quan thu tu lam

1. Task 5.1 - Tao content list page
2. Task 5.2 - Tao content create/edit forms
3. Task 5.3 - Tao content detail, clone, delete, restore
4. Task 5.4 - Tao AI draft generation UI
5. Task 5.5 - Tao AI improve va approve UI
6. Task 5.6 - Tao AI chat va conversation pages
7. Chay verify tong the Phase 5

## Contract backend Content/AI/Conversation can chot truoc khi code

### Header rule quan trong

Tat ca route trong Phase 5 deu can:

- `Authorization`
- `X-Profile-Id`

Ly do:

- `ContentController` nam duoi `/api/content`
- `GeminiController` nam duoi `/api/ai`
- `ConversationController` nam duoi `/api/conversations`
- ca 3 prefix nay deu nam trong `ActiveProfileMiddleware`

Frontend khong duoc goi bat ky API Phase 5 nao neu chua co `activeProfileId`.

### Middleware behavior can biet

Neu request content/ai/conversation ma active profile context loi, backend tra:

- `401` neu thieu JWT
- `401` neu thieu hoac invalid `X-Profile-Id`
- `404` neu profile khong ton tai
- `403` neu profile khong thuoc user

Phase 5 can tai su dung auto-recovery va guard da dat tu Phase 2/3.

### Envelope response

```ts
type ApiResponse<T> = {
  success: boolean
  message?: string
  statusCode: number
  data?: T | null
  error?: {
    errorCode?: string
    errorMessage?: string
    stackTrace?: string
    validationErrors?: Record<string, string[]>
  }
  timestamp: string
}
```

### Enum exact

`AdTypeEnum`:

```ts
const adTypeValues = {
  TextOnly: 0,
  ImageText: 1,
  VideoText: 2,
} as const
```

`ContentStatusEnum`:

```ts
const contentStatusValues = {
  Draft: 0,
  PendingApproval: 1,
  Approved: 2,
  Rejected: 3,
  Published: 4,
} as const
```

`AiStatusEnum`:

```ts
const aiStatusValues = {
  Pending: 0,
  Completed: 1,
  Failed: 2,
} as const
```

Frontend phai gui payload enum bang number, khong gui string label.

### Content response exact

```ts
type ContentResponseDto = {
  id: string
  profileId: string
  brandId: string
  brandName?: string | null
  productId?: string | null
  adType: 0 | 1 | 2
  title?: string | null
  textContent: string
  imageUrl?: string | null
  videoUrl?: string | null
  styleDescription?: string | null
  contextDescription?: string | null
  representativeCharacter?: string | null
  status: 0 | 1 | 2 | 3 | 4
  createdAt: string
  updatedAt: string
}
```

### Content create request exact

```ts
type CreateContentRequest = {
  brandId: string
  productId?: string
  adType: 0 | 1 | 2
  title?: string
  textContent: string
  imageUrl?: string
  videoUrl?: string
  styleDescription?: string
  contextDescription?: string
  representativeCharacter?: string
}
```

### Content update request exact

```ts
type UpdateContentRequest = {
  productId?: string
  adType?: 0 | 1 | 2
  title?: string
  textContent?: string
  imageUrl?: string
  videoUrl?: string
  styleDescription?: string
  contextDescription?: string
  representativeCharacter?: string
}
```

Luu y quan trong:

- update request hien tai khong co `brandId`
- user khong doi brand tren edit content MVP

### AI request/response exact

Generate draft:

```ts
type CreateDraftRequest = {
  brandId: string
  productId?: string
  adType: 0 | 1 | 2
  title?: string
  prompt: string
}
```

Improve:

```ts
type ImproveContentRequest = {
  prompt: string
}
```

Generation response:

```ts
type AiGenerationResponse = {
  aiGenerationId: string
  contentId: string
  generatedText?: string | null
  status: 0 | 1 | 2
  errorMessage?: string | null
  createdAt: string
}
```

Chat request:

```ts
type ChatRequest = {
  brandId?: string
  productId?: string
  adType: 0 | 1 | 2
  message: string
  conversationId?: string
}
```

Chat response:

```ts
type ChatResponse = {
  response: string
  conversationId: string
}
```

### Conversation response exact

List item:

```ts
type ConversationResponseDto = {
  id: string
  profileId: string
  brandId?: string | null
  brandName?: string | null
  productId?: string | null
  productName?: string | null
  adType: 0 | 1 | 2
  title?: string | null
  isActive: boolean
  lastMessage?: string | null
  lastMessageAt?: string | null
  messageCount: number
}
```

Detail:

```ts
type ConversationDetailDto = ConversationResponseDto & {
  messages: ChatMessageDto[]
}

type ChatMessageDto = {
  id: string
  senderType: number
  message: string
  aiGenerationId?: string | null
  contentId?: string | null
  createdAt: string
}
```

### Query/filter behavior that backend dang ho tro

Content list query:

```text
GET /api/content?page=&pageSize=&searchTerm=&sortBy=&sortDescending=&brandId=&adType=&includeDeleted=&status=
```

Ho tro:

- `brandId`
- `adType`
- `includeDeleted`
- `status`
- search tren `title`, `textContent`
- sort:
  - `title`
  - `updatedAt`
  - `createdAt`
  - mac dinh `createdAt DESC`

Conversation list query:

```text
GET /api/conversations?page=&pageSize=&searchTerm=&sortBy=&sortDescending=
```

Ho tro:

- search tren `title`
- sort:
  - `title`
  - `createdAt`
  - mac dinh `updatedAt DESC`

### Brand/product ownership validation that backend dang lam

Content create/generate:

- `brandId` phai thuoc active profile
- `productId` neu co phai ton tai va thuoc dung `brandId`

Content update:

- service validate `content.BrandId` hien tai + `request.ProductId`
- frontend khong nen cho chon product khac brand

AI chat:

- neu co `productId` ma khong co `brandId` -> `400`
- neu `conversationId` co gia tri nhung conversation khong thuoc profile -> `404`

### ImageUrl behavior can biet

ContentService khi save `ImageUrl`:

- neu la chuoi thuong, backend convert thanh JSON string array dang `["url"]`
- neu da la chuoi JSON array thi backend giu nguyen

Frontend MVP khuyen nghi:

- cho user nhap 1 URL trong form
- gui len chuoi URL don
- khi render detail, neu field `imageUrl` nhin giong JSON array thi parse de hien preview

Khong can build multi-image editor phuc tap trong Phase 5.

### Restore behavior can biet

Content restore:

- chi restore duoc item deleted
- sau restore, backend set `status = Draft`

Frontend phai cap nhat badge dung theo rule nay.

## Task 5.1 - Tao content list page

### Muc tieu

- Hien thu vien content theo active profile
- Co filter co ban cho brand, ad type, status, include deleted

### File can tao

```text
AISAM-FE/src/app/(app)/contents/page.tsx
AISAM-FE/src/features/content/api/get-contents.ts
AISAM-FE/src/features/content/components/content-list.tsx
AISAM-FE/src/features/content/components/content-filters.tsx
AISAM-FE/src/features/content/components/content-status-badge.tsx
AISAM-FE/src/features/content/components/content-list-item.tsx
AISAM-FE/src/features/content/components/content-empty-state.tsx
AISAM-FE/src/features/content/components/content-error-state.tsx
AISAM-FE/src/features/content/hooks/use-contents-query.ts
AISAM-FE/src/types/content.ts
```

### API helper can co

```ts
type GetContentsParams = {
  page?: number
  pageSize?: number
  searchTerm?: string
  sortBy?: "title" | "updatedAt" | "createdAt"
  sortDescending?: boolean
  brandId?: string
  adType?: 0 | 1 | 2
  includeDeleted?: boolean
  status?: 0 | 1 | 2 | 3 | 4
}
```

`get-contents.ts`

```ts
export async function getContents(params: GetContentsParams) {
  return api.get<PagedResult<ContentResponseDto>>(endpoints.content.list(params), {
    requireAuth: true,
  })
}
```

Khong can tu truyen `profileId`; backend lay tu `X-Profile-Id`.

### UI list can co

It nhat nen hien:

- title
- brandName
- adType
- status
- updatedAt
- deleted state neu `includeDeleted=true`

CTA:

- view detail
- edit
- clone
- delete
- restore neu deleted

### Filter UX khuyen nghi

- search input
- brand select
- ad type select
- status select
- sort select:
  - Newest
  - Oldest
  - Recently updated
  - Title A-Z
  - Title Z-A
- toggle `Show deleted`

### Empty state

- chua co content: CTA `Create content`
- filter khong co ket qua: thong bao `No matching contents`

### Definition of Done

- List dung active profile context
- Search/filter/sort dung contract backend
- `includeDeleted=true` hoat dong
- Khong goi API khi active profile chua san sang

### Verify

- Test workspace chua co content
- Test filter theo brand, adType, status
- Test search theo title/text content
- Test `includeDeleted=false` va `includeDeleted=true`

## Task 5.2 - Tao content create/edit forms

### Muc tieu

- Cho user tao draft content thu cong
- Cho user sua draft content ma khong doi brand

### File can tao

```text
AISAM-FE/src/app/(app)/contents/new/page.tsx
AISAM-FE/src/app/(app)/contents/[id]/edit/page.tsx
AISAM-FE/src/features/content/api/create-content.ts
AISAM-FE/src/features/content/api/update-content.ts
AISAM-FE/src/features/content/api/get-content-by-id.ts
AISAM-FE/src/features/content/components/content-form.tsx
AISAM-FE/src/features/content/schemas/content-create-schema.ts
AISAM-FE/src/features/content/schemas/content-update-schema.ts
```

### Phan A - Create content

Route backend:

```text
POST /api/content
Content-Type: application/json
```

Payload:

```ts
{
  brandId: string
  productId?: string
  adType: 0 | 1 | 2
  title?: string
  textContent: string
  imageUrl?: string
  videoUrl?: string
  styleDescription?: string
  contextDescription?: string
  representativeCharacter?: string
}
```

Validation frontend khuyen nghi:

- `brandId` required
- `adType` required
- `textContent` required cho flow tao thu cong
- `productId` optional
- neu `productId` co gia tri, phai nam trong products cua selected brand

### Form UX can chot

Field co:

- brand select
- product select phu thuoc brand
- ad type select
- title
- textContent
- imageUrl
- videoUrl
- styleDescription
- contextDescription
- representativeCharacter

Conditional hint:

- `TextOnly`: co the an/bot nhan manh field media
- `ImageText`: uu tien `imageUrl`
- `VideoText`: uu tien `videoUrl`

Nhung frontend khong can chot validation cứng qua muc backend khong yeu cau.

### Brand/product dependency rule

Khi user doi `brandId`:

- reset `productId`
- reload products theo brand moi

Khong giu `productId` cu vi backend se tra `Product does not belong to the selected brand.`

### Phan B - Edit content

Route backend:

```text
GET /api/content/{contentId}
PUT /api/content/{contentId}
```

Rule quan trong:

- update request khong co `brandId`
- frontend edit form khong cho doi brand
- product select chi duoc load theo brand hien tai cua content

Payload update:

```ts
{
  productId?: string
  adType?: 0 | 1 | 2
  title?: string
  textContent?: string
  imageUrl?: string
  videoUrl?: string
  styleDescription?: string
  contextDescription?: string
  representativeCharacter?: string
}
```

### Definition of Done

- Create content submit dung JSON contract
- Edit content khong cho doi brand
- Product options filter theo brand
- Loi product-brand mismatch duoc tranh o UI

### Verify

- Tao content thu cong voi brand only
- Tao content voi brand + product hop le
- Sua title/text/adType
- Thu chon product sai brand o UI phai bi reset/chan

## Task 5.3 - Tao content detail, clone, delete, restore

### Muc tieu

- Hoan thien lifecycle co ban cua content truoc khi them AI/publish

### File can tao

```text
AISAM-FE/src/app/(app)/contents/[id]/page.tsx
AISAM-FE/src/features/content/api/clone-content.ts
AISAM-FE/src/features/content/api/delete-content.ts
AISAM-FE/src/features/content/api/restore-content.ts
AISAM-FE/src/features/content/components/content-detail.tsx
AISAM-FE/src/features/content/components/content-actions.tsx
AISAM-FE/src/features/content/components/content-media-preview.tsx
```

### Route backend

```text
GET    /api/content/{contentId}
POST   /api/content/{contentId}/clone
DELETE /api/content/{contentId}
POST   /api/content/{contentId}/restore
```

### Detail page can hien

- title
- brandName
- productId hoac product info neu co the map
- adType
- status
- textContent
- imageUrl preview neu parse duoc
- videoUrl
- styleDescription
- contextDescription
- representativeCharacter
- createdAt
- updatedAt

### ImageUrl render rule

Vì backend co the tra:

- chuoi URL don duoc convert thanh JSON array string
- hoac null

Frontend can co helper:

```ts
function extractImageUrls(raw?: string | null): string[]
```

Rule:

- neu `raw` null/rong -> `[]`
- neu `raw` la JSON array -> parse
- neu `raw` la URL thuong -> `[raw]`

### Clone flow

`POST /clone`:

- tao content moi
- backend set `status = Draft`

Frontend khuyen nghi:

- sau clone thanh cong, redirect den detail item moi

### Delete/restore flow

Delete:

- soft delete
- sau delete tu detail, redirect ve `/contents?includeDeleted=true` hoac `/contents`

Khuyen nghi:

```text
/contents
```

Restore:

- chi apply cho item deleted
- sau restore, status se tro ve `Draft`
- refetch detail/list va cap nhat badge

### Definition of Done

- Detail page hien dung metadata va media
- Clone tao item moi
- Delete la soft delete
- Restore cap nhat status ve `Draft`

### Verify

- Clone 1 content
- Xoa content
- Restore content
- Test preview `imageUrl` dang URL va dang JSON array string

## Task 5.4 - Tao AI draft generation UI

### Muc tieu

- Cho user tao draft bang Gemini dua tren prompt
- Dung backend tao content moi + generation dau tien

### File can tao

```text
AISAM-FE/src/features/ai/api/generate-draft.ts
AISAM-FE/src/features/ai/components/ai-draft-panel.tsx
AISAM-FE/src/features/ai/components/prompt-form.tsx
AISAM-FE/src/features/ai/components/generation-result-card.tsx
AISAM-FE/src/features/ai/schemas/generate-draft-schema.ts
AISAM-FE/src/types/ai.ts
```

### Route backend

```text
POST /api/ai/generate-draft
```

Payload:

```ts
{
  brandId: string
  productId?: string
  adType: 0 | 1 | 2
  title?: string
  prompt: string
}
```

Behavior backend can biet:

- backend tu tao `Content` moi voi `status = Draft`
- sau do tao 1 `AiGeneration`
- co the tra `Completed` hoac `Failed`

### UI flow khuyen nghi

1. user chon brand/product/adType
2. nhap title optional
3. nhap prompt
4. submit
5. hien generation result
6. CTA:
   - view content detail
   - open generation history
   - approve neu generation completed

### Validation

- `brandId` required
- `adType` required
- `prompt` required
- `productId` optional nhung phai thuoc brand da chon

### Result state can co

- `Pending`: co the hien spinner/timepoint, nhung AIService hien tai xu ly sync; state nay co the chi xuat hien rat ngan
- `Completed`: hien `generatedText`
- `Failed`: hien `errorMessage`

### Definition of Done

- Generate draft goi dung route
- Nhap prompt va context dung contract
- Hien ro completed/failed state
- Co link den content moi qua `contentId`

### Verify

- Generate draft voi brand only
- Generate draft voi brand + product hop le
- Test provider fail/thieu config va UI hien loi backend

## Task 5.5 - Tao AI improve va approve UI

### Muc tieu

- Cho user cai thien content hien co bang prompt
- Cho user xem lich su generation va approve ket qua ve content

### File can tao

```text
AISAM-FE/src/features/ai/api/improve-content.ts
AISAM-FE/src/features/ai/api/get-generations.ts
AISAM-FE/src/features/ai/api/approve-generation.ts
AISAM-FE/src/features/ai/components/generation-history.tsx
AISAM-FE/src/features/ai/components/improve-form.tsx
AISAM-FE/src/features/ai/components/generation-history-item.tsx
AISAM-FE/src/features/ai/schemas/improve-content-schema.ts
```

### Routes backend

```text
POST /api/ai/improve/{contentId}
GET  /api/ai/generations/{contentId}
POST /api/ai/approve/{aiGenerationId}
```

### Improve flow

Payload:

```ts
{ prompt: string }
```

Behavior backend:

- content phai thuoc active profile
- tao generation moi cho content do

Frontend:

- mo improve panel tu content detail
- submit prompt
- refresh generation history sau khi thanh cong

### Generation history can hien

Moi item:

- createdAt
- status
- generatedText preview
- errorMessage neu failed
- action `Approve` neu status = Completed

Sort:

- co the dung thu tu backend tra ve
- neu can, sap xep moi nhat len tren o UI

### Approve flow

Behavior backend:

- approve chi thanh cong neu generation `Completed` va co `GeneratedText`
- approve se cap nhat `content.TextContent = generatedText`
- content status tro ve `Draft`

Frontend:

1. click approve
2. disable nut trong luc submit
3. neu thanh cong:
   - refresh content detail
   - refresh generation history neu can
   - hien thong bao text content da duoc cap nhat

### Definition of Done

- Improve tao generation moi
- History list hien du completed/failed items
- Approve chi bat cho generation completed
- Approve xong content detail refresh dung

### Verify

- Improve content voi prompt moi
- Test generation failed state
- Approve generation completed
- Sau approve, content textContent thay doi dung

## Task 5.6 - Tao AI chat va conversation pages

### Muc tieu

- Cho user chat voi AI
- Xem lai lich su conversation theo active profile
- Soft delete conversation

### File can tao

```text
AISAM-FE/src/app/(app)/conversations/page.tsx
AISAM-FE/src/app/(app)/conversations/[id]/page.tsx
AISAM-FE/src/features/ai/components/chat-panel.tsx
AISAM-FE/src/features/conversation/api/chat.ts
AISAM-FE/src/features/conversation/api/get-conversations.ts
AISAM-FE/src/features/conversation/api/get-conversation-by-id.ts
AISAM-FE/src/features/conversation/api/delete-conversation.ts
AISAM-FE/src/features/conversation/components/conversation-list.tsx
AISAM-FE/src/features/conversation/components/conversation-list-item.tsx
AISAM-FE/src/features/conversation/components/conversation-detail.tsx
AISAM-FE/src/features/conversation/components/chat-message-list.tsx
AISAM-FE/src/features/conversation/components/chat-message-bubble.tsx
AISAM-FE/src/features/conversation/hooks/use-conversations-query.ts
AISAM-FE/src/features/conversation/schemas/chat-schema.ts
AISAM-FE/src/types/conversation.ts
```

### Chat route backend

```text
POST /api/ai/chat
```

Payload:

```ts
{
  brandId?: string
  productId?: string
  adType: 0 | 1 | 2
  message: string
  conversationId?: string
}
```

### Chat behavior can biet

- neu co `productId` ma khong co `brandId` -> backend tra `400`
- neu `conversationId` khong ton tai/khong thuoc profile -> `404`
- neu khong co `conversationId`:
  - backend tim active conversation cung `brandId/productId/adType`
  - neu khong co thi tao moi
- backend se luu ca user message va AI message

### Chat panel UX

It nhat can co:

- brand select optional
- product select optional, chi mo khi da chon brand
- ad type select
- message input
- send button

Flow:

1. neu dang o conversation detail, dung `conversationId` hien tai
2. neu bat dau chat moi, bo `conversationId`
3. sau response:
   - route sang `/conversations/[conversationId]`
   - hoac cap nhat state tai cho neu dang o detail

### Conversation list

Route backend:

```text
GET /api/conversations?page=&pageSize=&searchTerm=&sortBy=&sortDescending=
```

List nen hien:

- title
- brandName
- productName
- adType
- lastMessage
- lastMessageAt
- messageCount
- active state

Sort/filter:

- search title
- sort newest activity mac dinh
- optional title A-Z

### Conversation detail

Route backend:

```text
GET /api/conversations/{id}
```

Detail nen hien:

- metadata conversation
- danh sach messages theo thu tu tang dan
- sender type user/AI
- createdAt

`senderType` la enum tu backend model; frontend co the map don gian:

- user -> canh phai
- AI -> canh trai

### Delete conversation

Route backend:

```text
DELETE /api/conversations/{id}
```

Behavior:

- soft delete
- set `IsDeleted = true`
- set `IsActive = false`

Frontend:

- confirm truoc khi xoa
- sau delete detail -> redirect ve `/conversations`
- list refresh de item bien mat

### Error handling can ro

- `Conversation not found.` -> stale route sau delete
- `Brand is required when product is selected.` -> validation UI phai chan truoc
- `AI chat is temporarily unavailable.` -> render message loi ro, nhung backend van co the da luu AI error message vao conversation

### Definition of Done

- Chat moi tao duoc conversation
- Chat tiep dung `conversationId`
- Conversation list/detail hien dung message history
- Delete conversation hoat dong va refresh list

### Verify

- Bat dau 1 chat moi
- Gui them message trong conversation cu
- Test product selected khong co brand bi chan o UI
- Xoa conversation tu detail va tu list

## Verify tong Phase 5

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- content/ai/conversation requests deu co `Authorization` va `X-Profile-Id`
- content list/filter/sort hoat dong
- create/edit/detail/clone/delete/restore content hoat dong
- AI generate/improve/approve hoat dong
- conversation list/detail/delete hoat dong
- chat moi va chat tiep conversation cu deu hoat dong
- stale active profile duoc shell/provider xu ly, khong crash page

## Deliverable sau Phase 5

Can co it nhat:

```text
AISAM-FE/
  PHASE_5_IMPLEMENTATION.md
  src/
    app/
      (app)/
        contents/
          page.tsx
          new/
          [id]/
            page.tsx
            edit/
        conversations/
          page.tsx
          [id]/
            page.tsx
    features/
      content/
        api/
        components/
        hooks/
        schemas/
      ai/
        api/
        components/
        schemas/
      conversation/
        api/
        components/
        hooks/
        schemas/
    types/
      content.ts
      ai.ts
      conversation.ts
```

## Risk can tranh trong Phase 5

- Quen gui `X-Profile-Id` cho content/ai/conversation APIs
- Cho doi brand trong edit content, trong khi backend update khong support
- Giu `productId` cu khi user doi brand trong form
- Hieu sai `imageUrl` la URL thuan, khong xu ly truong hop backend tra JSON array string
- Approve generation xong khong refresh content detail
- Gui `productId` ma khong co `brandId` cho AI chat
- Retry chat/generation vo han khi provider dang fail
- Hien controls publish/social ngay trong Phase 5 du backend flow publish nam o Phase 6

## Rule chuyen sang Phase 6

Chi bat dau Phase 6 khi:

- Phase 5 build pass
- content CRUD/clone/restore chay on dinh
- AI generate/improve/approve chay on dinh
- chat va conversations chay on dinh
- brand/product/content context san sang de social publish gan vao
