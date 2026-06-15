# AISAM Frontend Code Plan

Tai lieu nay la code plan frontend cho `AISAM-FE`, canh theo `user_story_list.md` hien tai va khong claim vuot qua backend local.

Nguon uu tien:

1. `user_story_list.md`: source of truth ve user story va thu tu scope hien tai.
2. `AISAM-BE/AISAM.API/Controllers`: source of truth ve API route active co the goi that.
3. `README.md`: target product/SRS hop nhat, dung de giu information architecture cho payment, approval, team, ads, analytics, storage va future channels.
4. `BACKEND_CODE_PLAN.md`: dung de biet module backend dang/moi migrate va cac limitation local.

Quy tac quan trong:

- Neu backend co controller/route active: frontend duoc tao adapter HTTP that.
- Neu README noi module thuoc target product nhung backend local chua co controller: frontend chi tao route, UI seam, guard va adapter `BackendContractMissingError`.
- Neu `user_story_list.md` xep module vao `Chua migrate`: frontend khong duoc implement API gia; chi tao shell neu can navigation/product continuity.
- Khong hardcode endpoint suy dien tu model/entity.

## Trang Thai Mapping Theo User Story

### Backend-ready user app

Co controller active trong `AISAM-BE/AISAM.API/Controllers`, frontend duoc noi API that.

- US-01..US-11: Auth, session, Google login, email verify, forgot/reset/change password.
- US-12..US-14: Business profile.
- US-15..US-18: Brand kit, profile scope cho brand, product catalog, search/filter.
- US-19..US-28: Content library, AI draft/improve/approve generation/history, AI chat, conversations.
- US-29..US-35: Facebook OAuth, social accounts, targets, integrations, publish now, posts.
- US-36..US-42: Notifications, schedules, upcoming schedules, dev scheduler trigger.
- US-43: Dashboard summary.

Backend controller basis:

```text
AuthController
ProfileController
BrandController
ProductController
ContentController
GeminiController
ConversationController
SocialAuthController
SocialAccountsController
SocialIntegrationController
PostsController
NotificationsController
ContentSchedulesController
DevSchedulerController
DashboardController
```

### Backend-dependent target product

Co trong `user_story_list.md` nhom da co/can nghiem thu hoac README target product, nhung backend local chua expose controller contract du de goi API that.

- US-44: Tao checkout subscription qua PayOS.
- US-45: Xem lich su thanh toan.
- US-46: Xem subscription hien tai.
- US-47: Xu ly callback/webhook thanh toan.
- US-48: Xem tong quan quota theo profile.
- US-49: Chan AI generation khi vuot quota.
- US-50: Chan publish khi vuot quota.
- US-51..US-55: Admin role gate, users, profile/subscription/payment admin, seed demo, admin policy.

Frontend action:

- Tao route/UI seam va typed local domain model.
- Adapter API phai throw `BackendContractMissingError` cho den khi backend expose controller.
- Khong them `/api/payment`, `/api/subscription`, `/api/quota`, `/api/admin` vao endpoint active khi chua co controller.

### Hardening/docs

- US-56: Kiem thu ownership va boundary chinh.
- US-57: Tai lieu hoa setup va smoke test backend/frontend.

Frontend action:

- Shared loading/empty/error state.
- `ENV_SETUP.md`.
- `FRONTEND_TEST_CHECKLIST.md`.

### Chua migrate / post-MVP shells

Theo `user_story_list.md`, chi tao route shell/backend-missing seam khi can giu IA theo README.

- US-58: Approval workflow nang cao.
- US-59: Team va phan quyen team.
- US-60: Facebook Ads MVP.
- US-61: Upload media qua storage service.
- US-62: Ket noi Instagram Business.
- US-63: Ket noi TikTok Business.
- US-64: Sinh anh AI day du.
- US-65: Sinh video AI.
- US-66: Quan ly plan dong.
- US-67: Ho tro analytics nang cao.
- US-68: Ho tro AI recommendation va optimization.

Frontend action:

- Route shell, guards, local UI seam.
- Reuse backend-ready data neu co: dashboard summary, posts, brands, content, social integrations.
- Khong tao HTTP call den controller chua ton tai.

## Architecture Rules

- Target: Next.js App Router + React + TypeScript trong `AISAM-FE`.
- API call di qua `src/lib/api/client.ts`.
- Runtime config di qua `src/lib/config.ts`.
- Khong doc `process.env` trong component.
- Khong dung fake data nhu production data.
- Backend-dependent adapters phai fail ro rang.

Backend envelope:

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

Prefix can `X-Profile-Id` theo `ActiveProfileMiddleware`:

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

Auth/Profile/Brand/Product can `Authorization` khi protected, nhung khong bat buoc `X-Profile-Id` theo backend local hien tai.

## Phase Order

1. Phase 0 - Scaffold va API foundation
2. Phase 1 - Auth va session
3. Phase 2 - Profile workspace va active profile
4. Phase 3 - App shell va dashboard summary
5. Phase 4 - Brand kit va product catalog
6. Phase 5 - Content library, AI va conversations
7. Phase 6 - Facebook social, publish va posts
8. Phase 7 - Notifications va scheduling
9. Phase 8 - Hardening, docs va backend-missing shell framework
10. Phase 9 - Subscription, payment va quota seam
11. Phase 10 - Post-MVP user modules: approval, teams, storage, ads, analytics, AI media, extra channels
12. Phase 11 - Admin app seam va admin operations plan

## Phase Summary

### Phase 0 - Scaffold va API foundation

Status: `backend-ready`

Cover foundation for US-01..US-68:

- Next.js scaffold, env config, API client, error envelope.
- Session storage, active profile storage.
- Core types for active modules and seam types for target product.

Implementation file: `PHASE_0_IMPLEMENTATION.md`

### Phase 1 - Auth va session

Status: `backend-ready`

Cover US-01..US-11:

- Register, login, Google login, refresh, logout, logout all.
- `/auth/me`, sessions, change password.
- Forgot/reset/change-password-with-token, verify email, resend verification.

Implementation file: `PHASE_1_IMPLEMENTATION.md`

### Phase 2 - Profile workspace

Status: `backend-ready`

Cover US-12..US-14:

- Profile onboarding, list, detail, update, delete, restore.
- Active profile context for scoped APIs.
- Keep `subscriptionId` as backend-partial metadata for Phase 9.

Implementation file: `PHASE_2_IMPLEMENTATION.md`

### Phase 3 - App shell va dashboard

Status: `backend-ready`

Cover US-43:

- Protected `(app)` layout.
- Dashboard summary.
- Active profile stale recovery and workspace shell.

Implementation file: `PHASE_3_IMPLEMENTATION.md`

### Phase 4 - Brand va product

Status: `backend-ready`

Cover US-15..US-18:

- Brand CRUD/restore, profile ownership, search/sort/paging.
- Product CRUD/restore by brand, search/filter.
- Product image upload is disabled/seam unless backend storage upload contract is active.

Implementation file: `PHASE_4_IMPLEMENTATION.md`

### Phase 5 - Content, AI va conversations

Status: `backend-ready`

Cover US-19..US-28:

- Content CRUD/clone/delete/restore.
- AI generate draft, improve, approve generation, generation history.
- AI chat, conversation list/detail/delete.

Implementation file: `PHASE_5_IMPLEMENTATION.md`

### Phase 6 - Social Facebook, publish va posts

Status: `backend-ready`

Cover US-29..US-35:

- Facebook OAuth URL/callback.
- Social accounts, available targets, linked targets.
- Link target to brand, disconnect account/integration.
- Publish now to Facebook Page, posts list/detail.

Implementation file: `PHASE_6_IMPLEMENTATION.md`

### Phase 7 - Notifications va scheduling

Status: `backend-ready`

Cover US-36..US-42:

- Notification list/detail, mark read, mark all read, unread count.
- Schedule create/list/detail/update/delete/upcoming.
- Schedule action from content.
- Dev scheduler trigger panel in development only.

Implementation file: `PHASE_7_IMPLEMENTATION.md`

### Phase 8 - Hardening, docs va backend-missing shell framework

Status: mixed

Cover US-56..US-57 and shared framework for backend-dependent/missing modules:

- Shared loading/empty/error/error-boundary components.
- `ENV_SETUP.md`, `FRONTEND_TEST_CHECKLIST.md`.
- Shared `BackendContractMissingError`.
- Shared backend-missing page/badge/config pattern.

Implementation file: `PHASE_8_IMPLEMENTATION.md`

### Phase 9 - Subscription, payment va quota

Status: `backend-dependent`

Cover US-44..US-50:

- Subscription/billing route tree and current profile subscription context.
- Pricing/plan cards and PayOS checkout seam.
- Payment history shell.
- Checkout result states.
- Quota overview and quota guard abstraction for AI generation/publish/scheduling.

Implementation rule:

- Use `activeProfile.subscriptionId` if available.
- Do not call payment/subscription/quota HTTP endpoints until backend exposes controllers.
- Adapter functions must throw `BackendContractMissingError` or return explicit `backend-dependent` state.

Implementation file: `PHASE_9_IMPLEMENTATION.md`

### Phase 10 - Post-MVP user modules

Status: `backend-missing` or `backend-dependent`

Cover US-58..US-68:

- Approval workflow shell.
- Team/permissions shell.
- Storage/media upload shell.
- Facebook Ads/campaign shell.
- Instagram/TikTok provider shells.
- AI image/video shell.
- Dynamic plans shell.
- Advanced analytics and recommendation shells.

Implementation file: `PHASE_10_IMPLEMENTATION.md`

### Phase 11 - Admin app seam

Status: `backend-dependent`

Cover US-51..US-55:

- Admin route group/app boundary.
- Admin role gate based on auth user role.
- Admin users/profile/subscription/payment/seed demo page seams.
- Non-admin access UX.
- No admin API call until backend exposes admin controller contracts.

Implementation file: `PHASE_11_IMPLEMENTATION.md`

## MVP Frontend Definition of Done

- US-01..US-43 run end-to-end against active backend.
- US-44..US-50 have user-visible route/UX seam and no fake HTTP calls.
- US-51..US-55 have admin route seam and role gate, no fake HTTP calls.
- US-56..US-57 docs/checklists exist and match active backend.
- US-58..US-68 have backend-missing/backend-dependent route shells only where navigation requires them.
- `pnpm lint` and `pnpm build` pass.

## Smoke Checklist

Run at minimum:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Manual smoke:

- Chua login bi chan dung.
- Da login request co `Authorization`.
- Da chon profile request co `X-Profile-Id` cho scoped APIs.
- Auth/Profile/Brand/Product APIs khong phu thuoc `X-Profile-Id`.
- Backend-dependent pages render without network calls to missing endpoints.
- Backend error envelope maps to visible user message.
