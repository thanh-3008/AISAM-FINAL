# Phase 6 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `6.1` den `6.5` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu truc tiep voi backend Social Facebook, publish va Posts hien tai trong `AISAM-BE`.

Pham vi Phase 6:

- Hoan thien social accounts page cho active profile
- Hoan thien Facebook connect flow va callback handling
- Hoan thien target linking theo brand
- Hoan thien publish content len Facebook Page da link
- Hoan thien posts history list/detail
- Giu cho publish flow phu hop business rule approval cua target product

Khong lam trong Phase 6:

- Instagram/TikTok providers
- Ads/Campaigns
- Advanced analytics cho posts
- Notifications/Scheduling detail
- Payment/Team/Approval

Luu y target product:

- `requirement.md` cho phep nhieu kenh hon Facebook ve mat muc tieu san pham.
- `README.md` coi Instagram/TikTok la huong mo rong, nen Phase 6 hien tai moi cover phan backend-ready la Facebook.
- Publish button trong FE phai bi chan neu content chua dat business state hop le theo approval workflow, khong duoc coi phase nay la bypass approval.

Can cu backend da doi chieu truc tiep cho Phase 6:

- `AISAM-BE/AISAM.API/Controllers/SocialAuthController.cs`
- `AISAM-BE/AISAM.API/Controllers/SocialAccountsController.cs`
- `AISAM-BE/AISAM.API/Controllers/SocialIntegrationController.cs`
- `AISAM-BE/AISAM.Services/Service/SocialService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/SocialAccountRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/SocialIntegrationRepository.cs`
- `AISAM-BE/AISAM.Common/Models/SocialDtos.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/SocialIntegrationDto.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/SocialCallbackRequest.cs`
- `AISAM-BE/AISAM.API/Controllers/PostsController.cs`
- `AISAM-BE/AISAM.Services/Service/PostService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/PostRepository.cs`
- `AISAM-BE/AISAM.Common/Models/PostListItemDto.cs`
- `AISAM-BE/AISAM.Common/Models/PostDtos.cs`
- `AISAM-BE/AISAM.Services/Service/ContentService.cs` cho route publish
- `AISAM-BE/AISAM.Common/GenericResponse.cs`

## Tong quan thu tu lam

1. Task 6.1 - Tao social accounts page
2. Task 6.2 - Tao Facebook connect flow
3. Task 6.3 - Tao target linking UI
4. Task 6.4 - Tao publish action cho content
5. Task 6.5 - Tao posts history pages
6. Chay verify tong the Phase 6

## Contract backend Social/Posts can chot truoc khi code

### Header rule quan trong

Tat ca route Social va Posts trong Phase 6 deu can:

- `Authorization`
- `X-Profile-Id`

Ly do:

- `/api/social-auth`
- `/api/social/accounts`
- `/api/social/integrations`
- `/api/posts`

deu nam trong `ActiveProfileMiddleware`.

Frontend khong duoc goi API Phase 6 neu chua co `activeProfileId`.

### Middleware behavior can biet

Neu request Phase 6 ma profile context sai, backend tra:

- `401` neu chua login
- `401` neu thieu/invalid `X-Profile-Id`
- `404` neu profile khong ton tai
- `403` neu profile khong thuoc user

Phase 6 can tai su dung shell/profile recovery flow tu Phase 2/3/5.

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

### Route active - Social auth

```text
GET  /api/social-auth/facebook
POST /api/social-auth/facebook/callback
```

### Route active - Social accounts

```text
GET    /api/social/accounts/me
GET    /api/social/accounts/{socialAccountId}/available-targets
GET    /api/social/accounts/{socialAccountId}/linked-targets
POST   /api/social/accounts/{socialAccountId}/link-targets
DELETE /api/social/accounts/{socialAccountId}
```

### Route active - Social integrations

```text
GET    /api/social/integrations/brand/{brandId}
DELETE /api/social/integrations/{socialIntegrationId}
```

### Route active - Publish va posts

```text
POST /api/content/{contentId}/publish/{integrationId}
GET  /api/posts?page=&pageSize=&brandId=&status=
GET  /api/posts/{postId}
```

### Social callback request exact

```ts
type SocialCallbackRequest = {
  code: string
  state: string
}
```

Luu y:

- frontend khong gui `provider`
- frontend khong gui `profileId`
- profile context di qua `X-Profile-Id`

### Social auth URL response exact

```ts
type AuthUrlResponse = {
  authUrl: string
  state: string
}
```

Frontend chu yeu can `authUrl`. `state` co the log/debug neu can.

### Social account response exact

```ts
type SocialAccountDto = {
  id: string
  profileId: string
  provider: string
  providerUserId: string
  isActive: boolean
  expiresAt?: string | null
  createdAt: string
  updatedAt: string
  targets: SocialTargetDto[]
}
```

### Linked target response exact

```ts
type SocialTargetDto = {
  id: string
  providerTargetId: string
  name: string
  type: string
  category?: string | null
  profilePictureUrl?: string | null
  isActive: boolean
}
```

### Available target response exact

```ts
type AvailableTargetDto = {
  providerTargetId: string
  name: string
  type: string
  category?: string | null
  profilePictureUrl?: string | null
  isActive: boolean
}
```

### Link targets request exact

```ts
type LinkSelectedTargetsRequest = {
  profileId: string
  provider: string
  providerTargetIds: string[]
  brandId: string
}
```

Luu y backend:

- controller chi chap nhan `provider = "facebook"`
- service check `brandId` phai thuoc active profile
- selected target phai nam trong danh sach available targets

Frontend khuyen nghi:

- du backend co field `profileId`, frontend van set bang `activeProfileId`
- khong cho user sua tay field nay

### Social integration response exact

```ts
type SocialIntegrationDto = {
  id: string
  socialAccountId: string
  profileId: string
  brandId: string
  externalId: string
  name: string
  platform: string
  isActive: boolean
  createdAt: string
  updatedAt: string
  brandName?: string | null
}
```

### Publish result response exact

```ts
type PublishResultDto = {
  success: boolean
  providerPostId?: string | null
  errorMessage?: string | null
  postedAt?: string | null
  refreshedTargetAccessToken?: string | null
}
```

Frontend chi can su dung:

- `success`
- `providerPostId`
- `errorMessage`
- `postedAt`

Khong can dong vao `refreshedTargetAccessToken`; backend tu xu ly rotate token.

### Post list item response exact

```ts
type PostListItemDto = {
  id: string
  contentId: string
  integrationId: string
  externalPostId?: string | null
  publishedAt: string
  status: string
  contentTitle?: string | null
  brandName?: string | null
}
```

### Filter behavior that backend dang ho tro - Posts

`GET /api/posts` chi ho tro:

- `page`
- `pageSize`
- `brandId`
- `status`

Khong ho tro:

- `platform`
- `searchTerm`
- `sortBy`

Backend sort mac dinh:

- `PublishedAt DESC`

Frontend khong nen expose filter/query ma backend chua ho tro.

### Social service behavior can biet

Facebook connect:

- backend tao OAuth state theo profile
- callback consume state theo `profileId + provider`
- neu state sai/expired -> `InvalidOperationException("OAuth state is invalid or expired.")`

Facebook config missing:

- `GET /social-auth/facebook` va `POST /social-auth/facebook/callback`
- co the tra `503` voi message `Facebook integration is not configured.`

Account linking:

- neu account Facebook da linked truoc do trong cung profile, backend update token va reactivate account

Unlink account:

- soft delete account
- soft delete tat ca integrations cua account do

Unlink integration:

- soft delete 1 target link

### Publish behavior can biet

`POST /api/content/{contentId}/publish/{integrationId}`:

- content phai thuoc active profile
- content khong duoc da `Published`
- integration phai thuoc active profile
- integration phai thuoc cung `brandId` voi content
- provider phai duoc support
- publish thanh cong se:
  - tao `Post`
  - update `content.Status = Published`

Frontend phai refresh:

- content detail
- posts list neu dang xem

## Task 6.1 - Tao social accounts page

### Muc tieu

- Hien danh sach social accounts cua active profile
- Lam entry point cho connect, linked targets, unlink

### File can tao

```text
AISAM-FE/src/app/(app)/social-accounts/page.tsx
AISAM-FE/src/features/social/api/get-accounts.ts
AISAM-FE/src/features/social/components/social-account-list.tsx
AISAM-FE/src/features/social/components/social-account-card.tsx
AISAM-FE/src/features/social/components/social-account-empty-state.tsx
AISAM-FE/src/features/social/components/social-account-error-state.tsx
AISAM-FE/src/features/social/hooks/use-social-accounts-query.ts
AISAM-FE/src/types/social.ts
```

### Route backend

```text
GET /api/social/accounts/me
```

### UI list can co

- provider
- providerUserId
- isActive
- expiresAt
- linked target count
- createdAt

CTA:

- Connect Facebook
- View available targets
- View linked targets
- Unlink account

### Empty/loading/error state

- loading skeleton
- empty: chua co account, CTA `Connect Facebook`
- error: retry button

### Definition of Done

- Page load danh sach account theo active profile
- Linked targets count hien duoc
- Empty state dan user vao connect flow

### Verify

- Test profile chua co social account
- Test profile da co 1 account
- Test reload page van giu active profile context

## Task 6.2 - Tao Facebook connect flow

### Muc tieu

- Bat dau OAuth flow voi Facebook
- Xu ly callback page de link social account vao active profile

### File can tao

```text
AISAM-FE/src/app/social-callback/facebook/page.tsx
AISAM-FE/src/features/social/api/get-facebook-auth-url.ts
AISAM-FE/src/features/social/api/handle-facebook-callback.ts
AISAM-FE/src/features/social/components/connect-facebook-button.tsx
AISAM-FE/src/features/social/components/facebook-connect-status.tsx
```

### Route backend

```text
GET  /api/social-auth/facebook
POST /api/social-auth/facebook/callback
```

### Flow can chot

1. user dang o active profile hop le
2. bam `Connect Facebook`
3. frontend goi `GET /social-auth/facebook`
4. nhan `authUrl`
5. redirect browser den `authUrl`
6. Facebook redirect ve frontend callback route voi query `code`, `state`
7. callback page doc `code`, `state`
8. frontend goi `POST /social-auth/facebook/callback` voi body:

```ts
{
  code,
  state
}
```

9. neu thanh cong:
   - refresh social account list
   - redirect ve `/social-accounts`

### Callback page rule

Callback page phai:

- yeu cau user van dang login
- yeu cau `activeProfileId` van ton tai
- neu thieu `code` hoac `state`, hien error state ro rang

### Error handling can ro

- `503` + `Facebook integration is not configured.` -> hien message cau hinh thieu, khong retry vo han
- `OAuth state is invalid or expired.` -> hien message het han, CTA quay lai `Connect Facebook`
- thieu `code/state` trong query -> hien invalid callback error

### Definition of Done

- Button connect lay duoc OAuth URL
- Callback page doc `code`, `state` va POST dung body
- Thanh cong thi social account xuat hien trong list
- Thieu config/expired state hien loi ro rang

### Verify

- Test connect flow happy path
- Test callback voi `code/state` thieu
- Test backend `503` config missing

## Task 6.3 - Tao target linking UI

### Muc tieu

- Cho user link Facebook Page target vao brand
- Quan ly linked integrations theo brand

### File can tao

```text
AISAM-FE/src/features/social/api/get-available-targets.ts
AISAM-FE/src/features/social/api/get-linked-targets.ts
AISAM-FE/src/features/social/api/link-targets.ts
AISAM-FE/src/features/social/api/delete-account.ts
AISAM-FE/src/features/social/api/delete-integration.ts
AISAM-FE/src/features/social/api/get-integrations-by-brand.ts
AISAM-FE/src/features/social/components/link-target-modal.tsx
AISAM-FE/src/features/social/components/available-target-list.tsx
AISAM-FE/src/features/social/components/linked-target-list.tsx
AISAM-FE/src/features/social/components/integration-list.tsx
AISAM-FE/src/features/social/components/unlink-account-button.tsx
AISAM-FE/src/features/social/components/unlink-integration-button.tsx
```

### Routes backend

```text
GET    /api/social/accounts/{socialAccountId}/available-targets
GET    /api/social/accounts/{socialAccountId}/linked-targets
POST   /api/social/accounts/{socialAccountId}/link-targets
DELETE /api/social/accounts/{socialAccountId}
DELETE /api/social/integrations/{socialIntegrationId}
GET    /api/social/integrations/brand/{brandId}
```

### Phan A - Available targets

Hien:

- name
- type
- category
- profilePictureUrl
- isActive

Du lieu nay den tu Facebook page targets available cho account.

### Phan B - Link target modal

Input can chot:

- socialAccountId
- selected brand
- selected target ids

Payload POST:

```ts
{
  profileId: activeProfileId
  provider: "facebook"
  providerTargetIds: string[]
  brandId: string
}
```

Rule:

- `provider` fixed la `facebook`
- `profileId` fixed theo active profile
- phai chon it nhat 1 target
- phai chon brand

### Phan C - Linked targets/account detail

`GET linked-targets` tra `SocialTargetDto[]` cua account.

UI can hien:

- page/page name
- active state
- unlink integration action neu tim duoc integration mapping

### Phan D - Integrations by brand

`GET /social/integrations/brand/{brandId}` can duoc goi o:

- Brand detail page
- Publish target selector
- Link target modal sau submit de refresh

UI can hien:

- name
- platform
- isActive
- createdAt
- unlink integration button

### Phan E - Unlink account / integration

Unlink account:

- `DELETE /social/accounts/{socialAccountId}`
- soft delete account + tat ca integrations duoi account

Unlink integration:

- `DELETE /social/integrations/{socialIntegrationId}`
- soft delete 1 target link

Frontend:

- confirm truoc delete
- refresh list sau thao tac

### Error handling can ro

- `Brand not found.` -> brand stale hoac khong thuoc active profile
- `Social account not found.` -> account stale/deleted
- `Selected target is not available for this account.` -> target list stale
- `Only Facebook is supported in Phase C.` -> frontend khong duoc allow provider khac

### Definition of Done

- User xem duoc available targets
- Link target vao brand thanh cong
- View integrations theo brand thanh cong
- Unlink account va unlink integration hoat dong

### Verify

- Link 1 target vao 1 brand
- Link nhieu targets neu available
- Unlink 1 integration
- Unlink ca account

## Task 6.4 - Tao publish action cho content

### Muc tieu

- Cho user publish content len Facebook Page da link
- Rang buoc dung integration theo brand cua content

### File can tao

```text
AISAM-FE/src/features/content/api/publish-content.ts
AISAM-FE/src/features/content/components/publish-content-button.tsx
AISAM-FE/src/features/content/components/publish-target-selector.tsx
AISAM-FE/src/features/content/components/publish-result-banner.tsx
AISAM-FE/src/features/social/hooks/use-brand-integrations-query.ts
```

### Route backend

```text
POST /api/content/{contentId}/publish/{integrationId}
```

### Publish preconditions can chot

Frontend truoc khi cho publish can dam bao:

- content ton tai
- content chua `Published`
- brand cua content co it nhat 1 `SocialIntegration`

Integration options phai lay tu:

```text
GET /api/social/integrations/brand/{brandId}
```

Khong cho user nhap tay `integrationId`.

### Publish target selector

UI:

- list integrations cua brand
- chi hien item `isActive = true`
- disable nut publish neu khong co integration active

### Response handling

Success response:

```ts
PublishResultDto
```

Frontend can:

1. hien success state
2. refresh content detail
3. co the navigate sang `/posts`

Khuyen nghi:

- neu dang o detail page, giu user tai cho va hien CTA `View posts`

### Error handling can ro

Backend co the tra:

- `Content not found.`
- `Content has already been published.`
- `Social integration not found.`
- `Publishing provider is not supported.`
- loi provider publish `BadGateway` voi `ErrorMessage`

Frontend phai hien `error.errorMessage` ro rang, khong chet im.

### Definition of Done

- User chi chon duoc integration cua cung brand
- Publish goi dung route
- Thanh cong thi content status refresh ve `Published`
- Co feedback ro khi publish fail

### Verify

- Publish content draft voi integration hop le
- Thu publish lai content da `Published`
- Thu publish voi brand khong co integration active

## Task 6.5 - Tao posts history pages

### Muc tieu

- Hien lich su publish cua active profile
- Cho user loc theo brand va status

### File can tao

```text
AISAM-FE/src/app/(app)/posts/page.tsx
AISAM-FE/src/app/(app)/posts/[id]/page.tsx
AISAM-FE/src/features/posts/api/get-posts.ts
AISAM-FE/src/features/posts/api/get-post-by-id.ts
AISAM-FE/src/features/posts/components/post-list.tsx
AISAM-FE/src/features/posts/components/post-list-item.tsx
AISAM-FE/src/features/posts/components/post-detail.tsx
AISAM-FE/src/features/posts/components/post-filters.tsx
AISAM-FE/src/features/posts/components/post-empty-state.tsx
AISAM-FE/src/features/posts/components/post-error-state.tsx
AISAM-FE/src/features/posts/hooks/use-posts-query.ts
AISAM-FE/src/types/post.ts
```

### Route backend

```text
GET /api/posts?page=&pageSize=&brandId=&status=
GET /api/posts/{postId}
```

### Filter rule can chot

Backend chi ho tro:

- `page`
- `pageSize`
- `brandId`
- `status`

Khong ho tro:

- `platform`
- free text search
- custom sort

Frontend filter UI nen bam dung 2 filter that:

- brand select
- status select

### List page can hien

- contentTitle
- brandName
- status
- publishedAt
- externalPostId

CTA:

- view detail
- open content detail neu team muon link nguoc

### Detail page can hien

- post id
- externalPostId
- publishedAt
- status
- contentTitle
- brandName
- integrationId

Khong co route update/delete post trong MVP, nen detail page chi la read-only.

### Empty/loading/error state

- empty: chua co post nao
- loading: table/list skeleton
- error: retry button

### Definition of Done

- Posts list dung active profile context
- Filter dung `brandId` va `status`
- Sort theo backend mac dinh `PublishedAt DESC`
- Detail page hien dung metadata post

### Verify

- Test profile chua co posts
- Test sau khi publish 1 content, post xuat hien trong list
- Test filter theo brand
- Test filter theo status

## Verify tong Phase 6

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- social/posts requests deu co `Authorization` va `X-Profile-Id`
- connect Facebook flow lay duoc OAuth URL
- callback page POST dung `code` + `state`
- social accounts list hoat dong
- target linking/unlinking hoat dong
- publish content hoat dong va refresh content status
- posts list/detail hoat dong

## Deliverable sau Phase 6

Can co it nhat:

```text
AISAM-FE/
  PHASE_6_IMPLEMENTATION.md
  src/
    app/
      (app)/
        social-accounts/
          page.tsx
        posts/
          page.tsx
          [id]/
            page.tsx
      social-callback/
        facebook/
          page.tsx
    features/
      social/
        api/
        components/
        hooks/
      posts/
        api/
        components/
        hooks/
      content/
        api/
        components/
    types/
      social.ts
      post.ts
```

## Risk can tranh trong Phase 6

- Quen gui `X-Profile-Id` cho social/posts routes
- Gui callback body thua field `provider` hoac `profileId`
- Expose provider khac `facebook` khi backend chua support
- Hieu sai `GET /posts` co support `platform` hay search
- Cho user publish voi integration khac brand cua content
- Khong refresh content detail sau publish, dan den status stale
- Soft delete account ma khong refresh integrations list
- Retry callback/publish vo han khi backend dang tra loi nghiep vu ro rang

## Rule chuyen sang Phase 7

Chi bat dau Phase 7 khi:

- Phase 6 build pass
- connect Facebook flow chay on dinh
- target linking/unlinking chay on dinh
- publish content chay on dinh
- posts history doc duoc du lieu sau publish
