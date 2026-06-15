# Phase 10 Implementation - Post-MVP User Modules

Tai lieu nay mo rong Phase 10 trong `FRONTEND_CODE_PLAN.md`, doi chieu voi `user_story_list.md`, `README.md` va backend local trong `AISAM-BE`.

Day la phase `User App` cho cac module `Chua migrate` hoac `backend-dependent` theo backend local.

Ban chat Phase 10:

- `user_story_list.md` xep US-58..US-68 vao nhom `Chua migrate`.
- `README.md` xem mot so module nhu approval, team, storage, ads, reports/analytics la target product.
- Backend local hien tai khong expose controller active cho approval/team/ads/report/storage/Instagram/TikTok/AI video/dynamic plans/recommendations.
- Vi vay frontend chi tao route shell, UI seam, guard, local domain type va readiness state. Khong goi API gia.

Pham vi Phase 10:

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

Khong lam trong Phase 10:

- Khong hardcode endpoint suy dien tu model/entity.
- Khong tao API request den controller chua ton tai.
- Khong fake success cho create campaign, upload media, approve content, invite team, generate video.
- Khong claim analytics/insight/recommendation la du lieu that neu chi co shell.

## Backend Basis

Backend-ready data co the tai su dung:

```text
DashboardController
PostsController
BrandController
ProductController
ContentController
SocialAccountsController
SocialIntegrationController
GeminiController
```

Backend local chua co controller active ro rang cho:

```text
Approval
Team
Storage/media management
Facebook Ads CRUD/preview/report
Instagram publishing
TikTok publishing
AI video generation
Dynamic subscription plans
Advanced analytics
AI recommendation/optimization
```

## Tong Quan Thu Tu Lam

1. Task 10.1 - Tao post-MVP route registry va navigation states
2. Task 10.2 - Approval workflow shell
3. Task 10.3 - Team va permission shell
4. Task 10.4 - Media/storage va AI media shells
5. Task 10.5 - Facebook Ads shell
6. Task 10.6 - Instagram/TikTok channel shells
7. Task 10.7 - Advanced analytics va recommendation shells
8. Task 10.8 - Dynamic plan management shell
9. Chay verify tong Phase 10

## Task 10.1 - Post-MVP Route Registry

Muc tieu:

- Co mot registry ro rang cho cac module chua migrate.
- Navigation biet module nao active, backend-dependent, backend-missing.

File can tao:

```text
AISAM-FE/src/features/post-mvp/config/post-mvp-modules.ts
AISAM-FE/src/features/post-mvp/components/post-mvp-module-card.tsx
AISAM-FE/src/features/post-mvp/components/post-mvp-status-badge.tsx
```

Config shape:

```ts
type PostMvpModule = {
  userStory: string
  title: string
  route: string
  status: "backend-dependent" | "backend-missing"
  backendController?: string
  summary: string
  blockedActions: string[]
  availableToday: Array<{ label: string; href: string }>
}
```

Definition of Done:

- Tat ca US-58..US-68 co entry trong registry.
- Shell pages dung chung registry, khong duplicate status text lung tung.

## Task 10.2 - Approval Workflow Shell

Cover: US-58.

Route/file can tao:

```text
AISAM-FE/src/app/(app)/approvals/page.tsx
AISAM-FE/src/features/approvals/components/approval-workflow-shell.tsx
AISAM-FE/src/features/approvals/components/approval-status-flow.tsx
AISAM-FE/src/features/approvals/api/submit-approval.ts
AISAM-FE/src/features/approvals/api/approve-content.ts
AISAM-FE/src/features/approvals/api/reject-content.ts
```

Implementation rule:

- API files throw `BackendContractMissingError`.
- UI co status flow: Draft -> PendingApproval -> Approved/Rejected.
- Co CTA ve Content Library de user lam viec voi module active.

Definition of Done:

- User thay ro approval la target workflow nhung backend contract chua expose.
- Khong co request den approval API.

## Task 10.3 - Team Va Permission Shell

Cover: US-59.

Route/file can tao:

```text
AISAM-FE/src/app/(app)/teams/page.tsx
AISAM-FE/src/features/teams/components/team-shell.tsx
AISAM-FE/src/features/teams/components/team-permission-matrix.tsx
AISAM-FE/src/features/teams/api/create-team.ts
AISAM-FE/src/features/teams/api/invite-member.ts
AISAM-FE/src/features/teams/api/update-member-role.ts
```

Implementation rule:

- API files throw `BackendContractMissingError`.
- Matrix hien role/permission concept theo README, nhung gan nhan `backend-missing`.
- Khong enforce leader model tren frontend nhu source of truth.

Definition of Done:

- Route `/teams` render duoc.
- User hieu team/RBAC chua active o backend local.

## Task 10.4 - Media/Storage Va AI Media Shells

Cover: US-61, US-64, US-65.

Route/file can tao:

```text
AISAM-FE/src/app/(app)/media/page.tsx
AISAM-FE/src/app/(app)/ai-media/page.tsx
AISAM-FE/src/features/media/components/media-library-shell.tsx
AISAM-FE/src/features/media/components/upload-dropzone-shell.tsx
AISAM-FE/src/features/ai-media/components/ai-image-shell.tsx
AISAM-FE/src/features/ai-media/components/ai-video-shell.tsx
AISAM-FE/src/features/media/api/upload-media.ts
AISAM-FE/src/features/ai-media/api/generate-image.ts
AISAM-FE/src/features/ai-media/api/generate-video.ts
```

Implementation rule:

- Product/profile upload limitation phai noi ro neu backend local reject file upload.
- AI image full flow va AI video generation khong duoc goi API neu backend contract chua ro.
- Co CTA ve Content AI draft hien active.

Definition of Done:

- `/media` va `/ai-media` render shell co nghia.
- Khong co upload/generation fake success.

## Task 10.5 - Facebook Ads Shell

Cover: US-60.

Route/file can tao:

```text
AISAM-FE/src/app/(app)/campaigns/page.tsx
AISAM-FE/src/app/(app)/campaigns/[id]/page.tsx
AISAM-FE/src/features/ads/components/ads-overview-page.tsx
AISAM-FE/src/features/ads/components/ads-readiness-checklist.tsx
AISAM-FE/src/features/ads/components/campaign-form-shell.tsx
AISAM-FE/src/features/ads/components/creative-preview-card.tsx
AISAM-FE/src/features/ads/hooks/use-ads-readiness.ts
AISAM-FE/src/features/ads/api/create-campaign.ts
AISAM-FE/src/features/ads/api/create-ad-set.ts
AISAM-FE/src/features/ads/api/create-creative.ts
AISAM-FE/src/features/ads/api/create-ad.ts
```

Backend-ready data duoc phep dung:

- brands
- content
- posts
- social integrations
- dashboard summary

Implementation rule:

- Ads CRUD API files throw `BackendContractMissingError`.
- Readiness check duoc dung backend-ready data, nhung quota/approval/ads contract la `unknown-backend-state`.
- Creative preview la local preview, khong claim la Facebook provider preview.

Definition of Done:

- `/campaigns` co checklist va form shell.
- User thay ro can brand, content, social integration, quota/approval va backend ads contract.

## Task 10.6 - Instagram/TikTok Channel Shells

Cover: US-62, US-63.

Route/file can tao:

```text
AISAM-FE/src/app/(app)/channels/instagram/page.tsx
AISAM-FE/src/app/(app)/channels/tiktok/page.tsx
AISAM-FE/src/features/channels/components/channel-shell.tsx
AISAM-FE/src/features/channels/api/connect-instagram.ts
AISAM-FE/src/features/channels/api/connect-tiktok.ts
```

Implementation rule:

- Khong reuse Facebook OAuth endpoint cho Instagram/TikTok.
- CTA ve Facebook social connect la available action hien tai.
- Noi ro platform provider chua migrate.

Definition of Done:

- Routes render on dinh.
- Network tab khong co request den Instagram/TikTok API.

## Task 10.7 - Advanced Analytics Va Recommendations

Cover: US-67, US-68.

Route/file can tao:

```text
AISAM-FE/src/app/(app)/analytics/page.tsx
AISAM-FE/src/app/(app)/recommendations/page.tsx
AISAM-FE/src/features/analytics/components/advanced-analytics-shell.tsx
AISAM-FE/src/features/recommendations/components/recommendation-shell.tsx
AISAM-FE/src/features/analytics/api/get-advanced-analytics.ts
AISAM-FE/src/features/recommendations/api/get-recommendations.ts
```

Backend-ready data duoc phep dung:

- dashboard summary
- posts list

Implementation rule:

- Basic cards co the noi "available from dashboard/posts".
- Advanced metrics/recommendations phai gan nhan backend-missing.
- Khong ve chart bang fake numbers.

Definition of Done:

- `/analytics` phan biet ro basic backend-ready vs advanced backend-missing.
- `/recommendations` la shell, khong claim co AI optimization live.

## Task 10.8 - Dynamic Plan Management Shell

Cover: US-66.

Route/file can tao:

```text
AISAM-FE/src/app/(app)/plans/page.tsx
AISAM-FE/src/features/plans/components/dynamic-plan-shell.tsx
AISAM-FE/src/features/plans/components/plan-version-note.tsx
AISAM-FE/src/features/plans/api/create-plan.ts
AISAM-FE/src/features/plans/api/update-plan.ts
AISAM-FE/src/features/plans/api/delete-plan.ts
```

Implementation rule:

- Dynamic plan management la admin-oriented target product, nhung US-66 nam trong post-MVP.
- Neu route dat trong user app, phai la read-only backend-missing shell hoac redirect/note sang admin seam Phase 11.
- Khong cho user tao/sua plan gia.

Definition of Done:

- Dynamic plans khong bi lan vao billing user flow Phase 9.
- Khong tao endpoint plan CRUD khi backend chua co.

## Verify Tong Phase 10

Chay:

```text
cd AISAM-FE
pnpm lint
pnpm build
```

Manual smoke:

1. `/approvals`, `/teams`, `/media`, `/ai-media`, `/campaigns`, `/channels/instagram`, `/channels/tiktok`, `/analytics`, `/recommendations`, `/plans` render duoc.
2. Khong route nao goi HTTP den controller chua ton tai.
3. Cac CTA tro ve module active dung: Content, Brands, Social, Posts, Dashboard.
4. Trang nao dung backend-ready data phai gan nhan ro phan nao active va phan nao missing.
5. API seam files throw `BackendContractMissingError`.

## Deliverables Sau Phase 10

Can co toi thieu:

- `src/features/post-mvp/*`
- `src/features/approvals/*`
- `src/features/teams/*`
- `src/features/media/*`
- `src/features/ai-media/*`
- `src/features/ads/*`
- `src/features/channels/*`
- `src/features/analytics/*`
- `src/features/recommendations/*`
- `src/features/plans/*`
- Route shells cho US-58..US-68.

## Rui Ro Can Tranh

- Nhin thay entity/model backend roi suy ra REST API contract.
- Dung fake table/chart nhu du lieu production.
- Cho user submit action roi hien success local.
- Tron dynamic plan management vao user billing flow.
- Claim README current feature la frontend-ready khi backend local khong co controller.
