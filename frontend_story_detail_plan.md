# Frontend Story Detail Plan - AISAM

Tai lieu nay la ke hoach viet story detail de implement frontend dua tren codebase hien tai:

- Frontend user app: `SEP490/SEP490_Frontend`
- Frontend admin app: `SEP490/SEP490_FrontendAdmin`
- Backend active: `AISAM-BE`
- Can cu chinh: `user_story_list.md`, `BACKEND_CODE_PLAN.md`, `AISAM-BE/docs/superpowers/CODEBASE.md`, `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`

Muc tieu cua file nay khong phai la task code chi tiet, ma la khung story detail de frontend co the implement theo backend API dang active.

## 1. Ket luan hien tai

Frontend da co san nhieu route, component va context phu hop voi AISAM:

- Auth pages: login, sign-up, forgot password, update password, verify email.
- Dashboard shell: dashboard layout, sidebar, header.
- Profile context va profile switcher.
- Brand/product pages va components.
- Content, AI content, social accounts, posts, notifications, calendar, dashboard pages.
- Payment/subscription/team/approval/ads UI cung co san nhung backend active hien tai chua expose cac module nay.

Backend active hien tai da ho tro:

- Auth/session/email/password/Google login.
- Profile CRUD.
- Brand CRUD.
- Product CRUD.
- Content CRUD/clone/restore/publish.
- Gemini AI generate/improve/approve/chat.
- Conversation history.
- Facebook OAuth/account/target/integration.
- Posts history.
- Notifications.
- Content scheduling.
- Dashboard summary.
- Development-only scheduler trigger.

Backend chua active cho frontend implement that:

- Payment/subscription/quota API.
- Admin user/payment/subscription API.
- Team/approval API.
- Storage upload API.
- Ads API.
- Instagram/TikTok provider.
- AI image/video generation.

## 2. Nguyen tac frontend can bam

Tat ca API call di qua `SEP490/SEP490_Frontend/lib/api.ts`.

Backend response envelope:

```ts
interface ApiResponse<T> {
  success: boolean
  message: string
  statusCode: number
  data: T
  errors?: unknown
}
```

Header chung:

- Public auth endpoints: khong can `Authorization`.
- Protected endpoints: can `Authorization: Bearer <accessToken>`.
- Profile-scoped endpoints: can them `X-Profile-Id: <activeProfileId>`.

Profile-scoped backend prefixes:

```text
/api/content
/api/content-schedules
/api/dashboard
/api/dev/scheduler
/api/ai
/api/conversations
/api/social-auth
/api/social
/api/posts
/api/notifications
```

Frontend `fetchWithAuth` da tu them `Authorization` va `X-Profile-Id` tu localStorage. Story detail can yeu cau:

- User phai co active profile truoc khi vao dashboard feature pages.
- Neu chua co active profile, UI redirect ve onboarding/profile selection.
- Neu API tra `401` do thieu `X-Profile-Id`, UI hien loi "Select a profile first" thay vi chi bao login fail.

## 3. Viec can lam truoc khi implement story

### F0.1 - Dong bo endpoint map voi backend active

Mo ta: Sua `lib/api.ts` de endpoint constants khop voi controller active trong `AISAM-BE`.

Can sua noi bat:

- `profiles()` hien dang la `/profiles`, backend khong co `GET /api/profiles`; dung `/profiles/user/{userId}`.
- `profilesMe()` hien la `/users/profile/me`, backend khong co route nay.
- `socialUnlinkAccount()` hien la `/social/accounts/unlink/{id}`, backend dung `DELETE /social/accounts/{id}`.
- `unlinkTarget()` hien la `/social/accounts/unlink-target/{id}`, backend dung `DELETE /social/integrations/{id}`.
- Notification mark-read hien la `/notifications/{id}/read`, backend dung `POST /notifications/{id}/mark-read`.
- Notification mark-all hien la `/notifications/read/all`, backend dung `POST /notifications/mark-all-read`.
- Unread count hien la `/notifications/unread/count`, backend dung `GET /notifications/unread-count`.
- Content calendar hien dang dung `/content-calendar/*`, backend dung `/content-schedules`.
- Dashboard stats hien la `/dashboard/stats`, backend dung `/dashboard/summary`.
- Payment, team, approval, ads endpoint constants nen danh dau planned, khong goi that khi backend chua active.

Acceptance criteria:

- Tat ca endpoint active khop controller backend.
- Cac endpoint planned duoc gom rieng hoac comment ro "backend not active".
- Khong co page active goi route khong ton tai.

### F0.2 - Chuan hoa type frontend theo DTO backend

Mo ta: Tao/cap nhat type trong `lib/types` cho active DTO.

DTO can co:

- `GenericResponse<T>`
- `PagedResult<T>`
- `AuthSession`, `UserDto`, `SessionDto`
- `ProfileResponseDto`
- `BrandResponseDto`
- `ProductResponseDto`
- `ContentResponseDto`
- `AiGenerationResponse`, `ChatRequest`, `ChatResponse`
- `ConversationResponseDto`, `ConversationDetailDto`, `ChatMessageDto`
- `SocialAccountDto`, `AvailableTargetDto`, `SocialTargetDto`
- `PostListItemDto`
- `NotificationListItemDto`, `NotificationDetailDto`, `UnreadNotificationCountDto`
- `ContentScheduleDto`
- `DashboardSummaryDto`

Acceptance criteria:

- UI component khong dung `any` cho response active.
- Enum numeric backend duoc map sang label UI ro rang.

### F0.3 - Chuan hoa active profile guard

Mo ta: Bao ve dashboard feature pages can `X-Profile-Id`.

Can ap dung cho:

- `/dashboard`
- `/dashboard/contents`
- `/dashboard/brands`
- `/dashboard/brands/[id]`
- `/dashboard/social-accounts`
- `/dashboard/posts`
- `/dashboard/notifications`
- `/dashboard/calendar`

Acceptance criteria:

- Co active profile thi request co `X-Profile-Id`.
- Khong co active profile thi hien empty state/redirect onboarding.
- Khong goi API profile-scoped khi `activeProfileId` null.

## 4. Story detail backlog uu tien frontend

## Story 1 - Login bang email

User goal: Nguoi dung dang nhap de vao AISAM dashboard.

Frontend scope:

- Page: `app/auth/login/page.tsx`
- Component: `components/pages/login/login-form.tsx`
- Context: `lib/contexts/auth-context.tsx`

Backend:

```text
POST /api/Auth/login
```

Request:

```ts
{
  email: string
  password: string
}
```

Response:

```ts
AuthSession
```

UI states:

- Loading khi submit.
- Error khi sai credential.
- Success: save session, set cookies, route den profile selection/onboarding/dashboard.

Acceptance criteria:

- Login thanh cong luu `auth_session`.
- API protected sau login co bearer token.
- Login fail hien message tu backend.

## Story 2 - Register account

User goal: Nguoi dung tao tai khoan moi.

Frontend scope:

- Page: `app/auth/sign-up/page.tsx`
- Context: `AuthProvider.register`

Backend:

```text
POST /api/Auth/register
```

Request:

```ts
{
  email: string
  password: string
  confirmPassword: string
  fullName: string
}
```

UI states:

- Validate email/password/confirm password.
- Loading khi submit.
- Success: save session, route den verify email hoac onboarding.
- Error: duplicate email, password invalid.

Acceptance criteria:

- Register thanh cong tao session.
- Form yeu cau `confirmPassword` vi backend DTO can field nay.

## Story 3 - Forgot/reset password

User goal: Nguoi dung khoi phuc truy cap khi quen mat khau.

Frontend scope:

- `app/auth/forgot-password/page.tsx`
- `app/auth/update-password/page.tsx`

Backend:

```text
POST /api/Auth/forgot-password
POST /api/Auth/change-password-with-token
POST /api/Auth/reset-password
```

UI states:

- Forgot: email input, success message khong tiet lo email co ton tai.
- Reset/update: token input tu URL, new password, confirm password.

Acceptance criteria:

- Forgot password goi dung endpoint.
- Reset fail do token invalid/expired hien loi ro.

## Story 4 - Verify email

User goal: Nguoi dung xac minh email sau khi dang ky.

Frontend scope:

- `app/auth/verify-email/page.tsx`
- `components/pages/verify-email/verify-email-status.tsx`

Backend:

```text
GET  /api/Auth/verify-email?token=...
POST /api/Auth/verify-email/resend
```

Acceptance criteria:

- Co token trong URL thi auto verify.
- Khong co token thi hien CTA resend verification.

## Story 5 - Profile onboarding va selection

User goal: Nguoi dung tao hoac chon business profile de vao workspace.

Frontend scope:

- `app/onboarding/page.tsx`
- `app/overview/profile/new/page.tsx`
- `components/profiles/profile-switcher.tsx`
- `lib/contexts/profile-context.tsx`

Backend:

```text
GET   /api/profiles/user/{userId}?search=&isDeleted=
GET   /api/profiles/{id}
POST  /api/profiles/user/{userId}
PUT   /api/profiles/{id}
DELETE /api/profiles/{id}
PATCH /api/profiles/{id}/restore
```

Notes:

- Backend hien tai reject `AvatarFile`; frontend nen dung `AvatarUrl` hoac an upload file.

Acceptance criteria:

- User moi chua co profile duoc dan den onboarding.
- Tao profile xong set `activeProfileId`.
- Profile switcher cap nhat localStorage de API tu them `X-Profile-Id`.

## Story 6 - Dashboard summary

User goal: Nguoi dung xem tong quan workspace hien tai.

Frontend scope:

- `app/dashboard/page.tsx`
- dashboard overview widgets.

Backend:

```text
GET /api/dashboard/summary
Headers: Authorization, X-Profile-Id
```

Response:

```ts
{
  draftContentCount: number
  publishedContentCount: number
  pendingApprovalContentCount: number
  upcomingScheduleCount: number
  failedScheduleCount: number
  activeSocialIntegrationCount: number
  publishedPostCount: number
  unreadNotificationCount: number
}
```

Acceptance criteria:

- Dashboard dung `/dashboard/summary`, khong dung `/dashboard/stats`.
- Khong co active profile thi hien profile empty state.

## Story 7 - Brand list va CRUD

User goal: Nguoi dung quan ly brand kit trong profile.

Frontend scope:

- `app/dashboard/brands/page.tsx`
- `components/pages/brands/brands-management.tsx`

Backend:

```text
GET    /api/brands?profileId=&page=&pageSize=&searchTerm=&sortBy=&sortDescending=&includeDeleted=
GET    /api/brands/{id}
POST   /api/brands
PUT    /api/brands/{id}
DELETE /api/brands/{id}
POST   /api/brands/{id}/restore
```

Acceptance criteria:

- List brand truyen `profileId=activeProfileId`.
- Create/update/delete/restore refresh list.
- UI hien soft-deleted item neu bat include deleted.

## Story 8 - Brand detail

User goal: Nguoi dung xem chi tiet brand va dieu huong den product/content cua brand.

Frontend scope:

- `app/dashboard/brands/[id]/page.tsx`
- `components/pages/brands/brand-details.tsx`

Backend:

```text
GET /api/brands/{id}
```

Acceptance criteria:

- Brand detail hien metadata brand.
- CTA den Products, Contents, Social integrations.

## Story 9 - Product list va CRUD theo brand

User goal: Nguoi dung quan ly san pham cua brand.

Frontend scope:

- `app/dashboard/brands/[id]/products/page.tsx`
- `components/pages/products/products-management.tsx`
- `components/products/product-form.tsx`

Backend:

```text
GET    /api/products?brandId=&page=&pageSize=&searchTerm=&sortBy=&sortDescending=&includeDeleted=
GET    /api/products/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}
POST   /api/products/{id}/restore
```

Notes:

- Backend reject `ImageFiles`; frontend nen an file upload hoac chi cho nhap image URL neu can.

Acceptance criteria:

- Product list filter dung `brandId`.
- Create/update dung multipart hoac JSON theo controller hien tai, nhung khong gui file.

## Story 10 - Content library

User goal: Nguoi dung xem va quan ly content trong active profile.

Frontend scope:

- `app/dashboard/contents/page.tsx`
- `components/contents/content-list.tsx`
- `components/contents/content-card.tsx`

Backend:

```text
GET /api/content?page=&pageSize=&brandId=&productId=&status=&includeDeleted=
Headers: Authorization, X-Profile-Id
```

Acceptance criteria:

- List content co loading, empty, error.
- Filter theo brand/status neu UI co san.
- Khong hien approval controls nhu feature active neu backend chua co Approval API.

## Story 11 - Create/edit content

User goal: Nguoi dung tao hoac sua draft content.

Frontend scope:

- `app/dashboard/contents/new/page.tsx`
- `app/dashboard/brands/[id]/contents/new/page.tsx`
- `components/contents/content-form-profile.tsx`

Backend:

```text
POST /api/content
PUT  /api/content/{contentId}
```

Request:

```ts
{
  brandId: string
  productId?: string
  adType: number
  title?: string
  textContent: string
  imageUrl?: string
  videoUrl?: string
  styleDescription?: string
  contextDescription?: string
  representativeCharacter?: string
}
```

Acceptance criteria:

- Brand required.
- Product options filtered by selected brand.
- Submit success route den content detail/list.

## Story 12 - Content detail, clone, delete, restore

User goal: Nguoi dung thao tac voi content da tao.

Frontend scope:

- Content modal/detail components.
- `components/contents/content-preview-modal.tsx`

Backend:

```text
GET    /api/content/{contentId}
POST   /api/content/{contentId}/clone
DELETE /api/content/{contentId}
POST   /api/content/{contentId}/restore
```

Acceptance criteria:

- Clone tao content moi va refresh list.
- Delete la soft delete.
- Restore chi hien cho item da deleted neu UI ho tro.

## Story 13 - AI generate draft

User goal: Nguoi dung dung Gemini tao draft content.

Frontend scope:

- `components/pages/contents/ai-content-generator.tsx`
- Content create page.

Backend:

```text
POST /api/ai/generate-draft
```

Request:

```ts
{
  brandId: string
  productId?: string
  adType: number
  title?: string
  prompt: string
}
```

Acceptance criteria:

- Generate success hien generated text va content id.
- Missing `GEMINI_API_KEY`/provider fail hien graceful error tu backend.

## Story 14 - AI improve content

User goal: Nguoi dung cai thien content bang feedback.

Backend:

```text
POST /api/ai/improve/{contentId}
```

Request:

```ts
{ prompt: string }
```

Acceptance criteria:

- Improve tao AI generation moi.
- UI cho phep xem generated output truoc khi approve.

## Story 15 - Approve AI generation

User goal: Nguoi dung chon AI generation de cap nhat vao content.

Backend:

```text
POST /api/ai/approve/{aiGenerationId}
GET  /api/ai/generations/{contentId}
```

Acceptance criteria:

- List generation theo content.
- Approve thanh cong refresh content detail.

## Story 16 - AI chat va conversation history

User goal: Nguoi dung chat voi AI va xem lai lich su.

Frontend scope:

- AI chat component neu co.
- Conversation list/detail UI.

Backend:

```text
POST   /api/ai/chat
GET    /api/conversations
GET    /api/conversations/{id}
DELETE /api/conversations/{id}
```

Request:

```ts
{
  brandId?: string
  productId?: string
  adType: number
  message: string
  conversationId?: string
}
```

Acceptance criteria:

- Chat moi tao conversation.
- Chat tiep gui `conversationId`.
- Delete conversation refresh list.

## Story 17 - Facebook connect flow

User goal: Nguoi dung ket noi Facebook Page vao active profile.

Frontend scope:

- `app/dashboard/social-accounts/page.tsx`
- `components/social/connect-modal.tsx`
- `app/social-callback/[provider]/page.tsx`

Backend:

```text
GET  /api/social-auth/facebook
POST /api/social-auth/facebook/callback
```

Flow:

1. Call `GET /social-auth/facebook`.
2. Redirect browser den `authUrl`.
3. Facebook redirect ve frontend callback page voi `code` va `state`.
4. Frontend POST callback body `{ provider: "facebook", code, state, profileId }` neu DTO can.

Acceptance criteria:

- Missing Facebook config tra `503` thi UI hien "Facebook integration is not configured".
- Callback success refresh social account list.

## Story 18 - Social account target linking

User goal: Nguoi dung link Facebook target/Page voi brand.

Backend:

```text
GET    /api/social/accounts/me
GET    /api/social/accounts/{socialAccountId}/available-targets
GET    /api/social/accounts/{socialAccountId}/linked-targets
POST   /api/social/accounts/{socialAccountId}/link-targets
DELETE /api/social/accounts/{socialAccountId}
DELETE /api/social/integrations/{socialIntegrationId}
GET    /api/social/integrations/brand/{brandId}
```

Acceptance criteria:

- Social account list hien account va active status.
- Link modal chon brand va targets.
- Disconnect account/integration dung route active backend.

## Story 19 - Publish content len Facebook

User goal: Nguoi dung publish content qua integration da link.

Frontend scope:

- `components/contents/content-publish-button.tsx`
- `components/contents/content-preview-modal.tsx`

Backend:

```text
POST /api/content/{contentId}/publish/{integrationId}
```

Acceptance criteria:

- UI chi cho chon integration theo brand cua content.
- Publish success update content status va tao post.
- Publish fail giu content, hien backend error.

## Story 20 - Posts history

User goal: Nguoi dung xem lich su post da publish.

Frontend scope:

- `app/dashboard/posts/page.tsx`
- `components/posts/posts-list.tsx`

Backend:

```text
GET /api/posts?page=&pageSize=&status=&platform=
GET /api/posts/{postId}
```

Acceptance criteria:

- Posts list dung active profile.
- Status/platform filter chi dung query backend ho tro.
- Detail modal/page hien content title, brand, publishedAt, externalPostId.

## Story 21 - Notifications

User goal: Nguoi dung theo doi thong bao noi bo.

Frontend scope:

- `app/dashboard/notifications/page.tsx`
- `components/layout/enhanced-notifications.tsx`

Backend:

```text
GET  /api/notifications?page=&pageSize=&isRead=
GET  /api/notifications/{notificationId}
POST /api/notifications/{notificationId}/mark-read
POST /api/notifications/mark-all-read
GET  /api/notifications/unread-count
```

Acceptance criteria:

- Endpoint map dung `mark-read`, `mark-all-read`, `unread-count`.
- Header notification badge dung unread count.
- Mark read refresh item/count.

## Story 22 - Content scheduling

User goal: Nguoi dung len lich publish content mot lan.

Frontend scope:

- `app/dashboard/calendar/page.tsx`
- `components/content-calendar/*`
- `components/contents/content-schedule-actions.tsx`

Backend:

```text
POST   /api/content-schedules
GET    /api/content-schedules
GET    /api/content-schedules/upcoming
GET    /api/content-schedules/{scheduleId}
PUT    /api/content-schedules/{scheduleId}
DELETE /api/content-schedules/{scheduleId}
```

Request create:

```ts
{
  contentId: string
  integrationId: string
  scheduledAt: string
}
```

Acceptance criteria:

- Calendar khong goi `/content-calendar/*` nua.
- Create/update validate future datetime.
- Delete la cancel/soft delete.
- Upcoming list dung `/content-schedules/upcoming`.

## Story 23 - Development scheduler trigger

User goal: Developer co the test scheduled posting worker tu UI/dev tool neu dang o Development.

Backend:

```text
POST /api/dev/scheduler/run-now
```

Acceptance criteria:

- Chi hien control khi `NODE_ENV` hoac config frontend cho phep dev tools.
- Production khong hien vi backend tra 404 ngoai Development.

## Story 24 - Payment/subscription placeholder

User goal: Nguoi dung thay duoc pricing/subscription UI nhung khong goi API chua active.

Frontend scope:

- `app/dashboard/subscription/page.tsx`
- `app/subscription/*`
- `components/subscription/*`
- `components/payments/*`

Backend status:

- Phase E planned.
- Chua co active `PaymentController`, `PaymentRepository`, `SubscriptionRepository`, `QuotaController`.

Acceptance criteria:

- UI co badge "Backend planned" hoac disable actions can API.
- Khong goi `/payment/*` trong flow active neu backend chua implement.

## Story 25 - Team/approval placeholder

User goal: Nguoi dung khong bi loi khi vao Team/Approval page trong luc backend chua active.

Frontend scope:

- `app/dashboard/teams/*`
- `app/dashboard/approvals/page.tsx`
- `components/teams/*`
- `components/approvals/*`

Backend status:

- Phase H post-MVP.
- Current `TeamMemberRoleEnum` chi co `Copywriter`, `Designer`.
- Chua co active Team/Approval controller trong `AISAM-BE`.

Acceptance criteria:

- Route hien planned/coming soon hoac mock-free empty state.
- Khong goi `/team`, `/team-members`, `/approvals` trong production flow active.

## Story 26 - Ads/campaign placeholder

User goal: Nguoi dung thay module Ads/Campaign la post-MVP neu chua co backend.

Frontend scope:

- `app/dashboard/campaigns/*`
- `components/pages/campaigns/*`
- creatives/ad-set/ad pages.

Backend status:

- Facebook Ads khong nam trong MVP backend hien tai.

Acceptance criteria:

- Disable create/edit ads actions.
- Khong goi `/ad-campaigns`, `/ad-sets`, `/ad-creatives`, `/ads` cho den khi backend active.

## 5. Thu tu implement frontend khuyen nghi

1. F0.1 - Dong bo `lib/api.ts` endpoint map.
2. F0.2 - Chuan hoa active DTO types.
3. F0.3 - Active profile guard.
4. Auth: login/register/forgot/reset/verify.
5. Profile onboarding/switcher.
6. Dashboard summary.
7. Brand CRUD.
8. Product CRUD.
9. Content library/create/edit/detail.
10. AI generate/improve/approve/chat/conversation.
11. Social Facebook connect/link targets.
12. Publish content va posts history.
13. Notifications.
14. Calendar/scheduling.
15. Planned placeholders cho Payment/Team/Approval/Ads.

## 6. Kiem thu sau moi nhom story

Moi nhom story frontend nen test:

- Page load khi chua login: redirect/login guard dung.
- Page load khi da login nhung chua active profile: khong goi profile-scoped API.
- Page load khi da co active profile: request co `Authorization` va `X-Profile-Id`.
- Loading state.
- Empty state.
- API error state.
- Success state va refresh data.
- Route/endpoint khop Swagger backend.

Lenh frontend nen chay sau moi dot implement:

```text
cd SEP490/SEP490_Frontend
pnpm lint
pnpm build
```

Backend smoke nen chay song song:

```text
cd AISAM-BE
dotnet test AISAM.sln
```

## 7. Ghi chu quan trong

- Khong implement frontend dua tren old backend trong `SEP490/SEP490_Backend`; source backend active la `AISAM-BE`.
- Khong coi `user_story_list.md` la API contract; API contract phai lay tu controller/DTO active trong `AISAM-BE`.
- Nhung UI da co san cho Payment, Team, Approval, Ads nen giu lai nhung phai chuyen sang placeholder neu route backend chua active.
- Profile avatar/product image upload hien chua active vi backend reject file upload trong MVP.
