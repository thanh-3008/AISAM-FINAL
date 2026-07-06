# US-57 - Tai lieu hoa setup va smoke test backend

## Mo ta

La nhom phat trien, toi muon co setup guide va API smoke checklist de moi thanh vien co the chay demo on dinh.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `docs/main/requirements.md`: he thong can demo on dinh cac module Auth, Brand/Product, AI Content, Social Integration, Scheduling, Analytics/Dashboard, Notification va Admin.
- `docs/archive/plans/backend-code-plan.md`: Phase 10 yeu cau backend release MVP co runbook, API testing guide, Swagger smoke test va `dotnet build/test` pass.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase G yeu cau API testing guide, local setup guide cap nhat theo modules da migrate, phase completion log, migration rollback notes.
- Active backend codebase `AISAM-BE`: hien da co Auth, Profile, Brand, Product, Content, AI/Gemini, Conversation, Facebook Social, Posts, Notifications, Content Schedules, Dashboard va Dev Scheduler.
- `docs/main/setup-guide.md`: da ton tai nhung can cap nhat de khop codebase active moi nhat va bo sung smoke checklist day du.

## Trang thai backend hien tai

Backend active controllers:

```text
AuthController
BrandController
ContentController
ContentSchedulesController
ConversationController
DashboardController
DevSchedulerController
GeminiController
HealthController
NotificationsController
PostsController
ProductController
ProfileController
SocialAccountsController
SocialAuthController
SocialIntegrationController
```

Backend chua active:

```text
PaymentController
AdminToolsController
UserController admin APIs
Team/Approval APIs
Ads APIs
Storage APIs
Instagram/TikTok APIs
```

Ket luan: docs/smoke checklist phai phan biet ro endpoint active va planned, de frontend khong goi nham route chua co.

## Muc tieu

Tao/cap nhat tai lieu backend setup va smoke test de:

- Developer moi clone repo co the chay backend local.
- Frontend developer biet endpoint nao active, endpoint nao planned.
- Team co checklist test nhanh truoc khi demo.
- Loi thieu config external nhu Gemini/Facebook/PayOS duoc ghi ro.
- Moi lan cap nhat backend co noi de ghi lai smoke result.

## Scope tai lieu can co

Khuyen nghi tao/cap nhat cac file:

```text
docs/main/setup-guide.md
AISAM-BE/docs/BACKEND_RUNBOOK.md
AISAM-BE/docs/API_SMOKE_CHECKLIST.md
AISAM-BE/docs/API_TESTING.md
AISAM-BE/AISAM.API/.env.example
```

Neu muon giu gon, co the tao toi thieu:

```text
AISAM-BE/docs/API_SMOKE_CHECKLIST.md
```

va cap nhat `docs/main/setup-guide.md` de link toi checklist nay.

## Audience

Tai lieu phuc vu:

- Frontend developers can chay backend local.
- Backend developers can verify sau khi sua code.
- Tester/demo operator can chay smoke truoc buoi demo.
- Mentor/reviewer can xem trang thai module active.

## Required documentation content

### 1. Environment setup

Can ghi ro:

- .NET SDK version.
- PostgreSQL local/Docker/cloud.
- Cach tao database `aisam_dev`.
- Cach tao/cap nhat `AISAM-BE/AISAM.API/.env`.
- Cach chay migration.
- Cach run API.
- Swagger URL.
- Health URL.

Lenh toi thieu:

```text
cd AISAM-BE
dotnet restore
dotnet build AISAM.sln
dotnet test AISAM.sln
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
dotnet run --project AISAM.API --urls http://localhost:5081
```

### 2. Required env

Docs phai ghi ro config bat buoc:

```env
CONNECTION_STRING=Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password
JWT_SECRET_KEY=replace-with-a-long-random-secret-minimum-32-characters
JWT_ISSUER=AISAM.API
JWT_AUDIENCE=AISAM.Client
FRONTEND_BASE_URL=http://localhost:3000
```

### 3. Optional external env

Docs phai ghi ro config optional theo module:

```env
GEMINI_API_KEY=your-gemini-api-key
GEMINI_MODEL=gemini-2.5-flash
FACEBOOK_APP_ID=your-facebook-app-id
FACEBOOK_APP_SECRET=your-facebook-app-secret
FACEBOOK_REDIRECT_URI=http://localhost:3000/social-callback/facebook
PAYOS_CLIENT_ID=planned
PAYOS_API_KEY=planned
PAYOS_CHECKSUM_KEY=planned
```

Ghi ro:

- Thieu `GEMINI_API_KEY`: AI endpoint phai fail graceful, API host van start.
- Thieu Facebook config: social auth URL tra loi config error/`503`, API host van start.
- PayOS hien planned, chua co active PaymentController.

### 4. Active endpoint map

Docs phai co bang endpoint active, toi thieu:

| Module | Endpoint |
| --- | --- |
| Health | `GET /api/Health` |
| Auth | `POST /api/Auth/register`, `POST /api/Auth/login`, `GET /api/Auth/me`, `POST /api/Auth/refresh`, `POST /api/Auth/logout` |
| Profile | `GET /api/profiles/user/{userId}`, `POST /api/profiles/user/{userId}`, `GET /api/profiles/{id}`, `PUT /api/profiles/{id}`, `DELETE /api/profiles/{id}`, `PATCH /api/profiles/{id}/restore` |
| Brand | `GET /api/brands`, `POST /api/brands`, `GET /api/brands/{id}`, `PUT /api/brands/{id}`, `DELETE /api/brands/{id}`, `POST /api/brands/{id}/restore` |
| Product | `GET /api/products`, `POST /api/products`, `GET /api/products/{id}`, `PUT /api/products/{id}`, `DELETE /api/products/{id}`, `POST /api/products/{id}/restore` |
| Content | `POST /api/content`, `GET /api/content`, `GET /api/content/{contentId}`, `PUT /api/content/{contentId}`, `POST /api/content/{contentId}/clone`, `POST /api/content/{contentId}/publish/{integrationId}`, `DELETE /api/content/{contentId}`, `POST /api/content/{contentId}/restore` |
| AI | `POST /api/ai/generate-draft`, `POST /api/ai/improve/{contentId}`, `POST /api/ai/approve/{aiGenerationId}`, `GET /api/ai/generations/{contentId}`, `POST /api/ai/chat` |
| Conversation | `GET /api/conversations`, `GET /api/conversations/{id}`, `DELETE /api/conversations/{id}` |
| Social | `GET /api/social-auth/facebook`, `POST /api/social-auth/facebook/callback`, `GET /api/social/accounts/me`, `GET /api/social/accounts/{id}/available-targets`, `GET /api/social/accounts/{id}/linked-targets`, `POST /api/social/accounts/{id}/link-targets`, `DELETE /api/social/accounts/{id}`, `GET /api/social/integrations/brand/{brandId}`, `DELETE /api/social/integrations/{id}` |
| Posts | `GET /api/posts`, `GET /api/posts/{postId}` |
| Notifications | `GET /api/notifications`, `GET /api/notifications/{notificationId}`, `POST /api/notifications/{notificationId}/mark-read`, `POST /api/notifications/mark-all-read`, `GET /api/notifications/unread-count` |
| Scheduling | `POST /api/content-schedules`, `GET /api/content-schedules`, `GET /api/content-schedules/upcoming`, `GET /api/content-schedules/{scheduleId}`, `PUT /api/content-schedules/{scheduleId}`, `DELETE /api/content-schedules/{scheduleId}` |
| Dashboard | `GET /api/dashboard/summary` |
| Dev only | `POST /api/dev/scheduler/run-now` |

### 5. Header rules

Docs phai ghi ro:

Public endpoints:

```text
GET /api/Health
POST /api/Auth/register
POST /api/Auth/login
POST /api/Auth/forgot-password
POST /api/Auth/reset-password
GET /api/Auth/verify-email
POST /api/Auth/verify-email/resend
```

Protected endpoints:

```http
Authorization: Bearer <accessToken>
```

Profile-scoped endpoints can them:

```http
X-Profile-Id: <activeProfileId>
```

Profile-scoped prefixes:

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

### 6. Planned endpoint list

Docs phai ghi ro cac module chua active:

```text
Payment/subscription/quota APIs
Admin user/profile/payment/subscription APIs
AdminTools seed demo API
Team APIs
Approval APIs
Ads APIs
Storage upload APIs
Instagram/TikTok APIs
```

Frontend dev khong duoc goi cac endpoint nay trong active flow.

## API smoke checklist

### Smoke group A - Build and host

- [ ] `dotnet restore` pass.
- [ ] `dotnet build AISAM.sln` pass.
- [ ] `dotnet test AISAM.sln` pass.
- [ ] `dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API` pass hoac blocker DB duoc ghi lai.
- [ ] API run duoc bang `dotnet run --project AISAM.API --urls http://localhost:5081`.
- [ ] Swagger mo duoc: `GET http://localhost:5081/swagger/index.html`.
- [ ] Health pass: `GET http://localhost:5081/api/Health`.

### Smoke group B - Auth

- [ ] `POST /api/Auth/register` tao user moi.
- [ ] `POST /api/Auth/login` tra `accessToken`, `refreshToken`, `user`.
- [ ] `GET /api/Auth/me` voi bearer token tra current user.
- [ ] `POST /api/Auth/refresh` tra token moi.
- [ ] `POST /api/Auth/logout` pass.

Request register mau:

```json
{
  "email": "smoke_user@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "fullName": "Smoke User"
}
```

### Smoke group C - Profile, brand, product

- [ ] `POST /api/profiles/user/{userId}` tao profile.
- [ ] `GET /api/profiles/user/{userId}` lay profile list.
- [ ] `POST /api/brands` tao brand voi `profileId`.
- [ ] `GET /api/brands?profileId={profileId}` lay brand list.
- [ ] `POST /api/products` tao product voi `brandId`.
- [ ] `GET /api/products?brandId={brandId}` lay product list.

Ghi chu:

- Profile/Product create/update hien dung multipart form.
- File upload avatar/product image chua active; khong gui file trong smoke MVP.

### Smoke group D - Content, AI, conversation

- [ ] `POST /api/content` tao content draft voi `Authorization` va `X-Profile-Id`.
- [ ] `GET /api/content` lay content list.
- [ ] `POST /api/content/{contentId}/clone` clone content.
- [ ] `POST /api/ai/generate-draft` fail graceful neu thieu Gemini key hoac success neu co key.
- [ ] `POST /api/ai/chat` fail graceful neu thieu Gemini key hoac success neu co key.
- [ ] `GET /api/conversations` lay conversation list.

### Smoke group E - Social and posts

- [ ] `GET /api/social-auth/facebook` khong JWT tra `401`.
- [ ] `GET /api/social-auth/facebook` co JWT va `X-Profile-Id` tra auth URL neu config du, hoac `503`/config error neu thieu Facebook config.
- [ ] `GET /api/social/accounts/me` protected.
- [ ] `GET /api/posts` protected va profile-scoped.

Ghi chu:

- Real Facebook OAuth/publish can Facebook credentials, redirect URI va Page permission.
- Neu thieu config, smoke pass neu API tra loi ro rang va host khong crash.

### Smoke group F - Notifications, scheduling, dashboard

- [ ] `GET /api/notifications` protected va profile-scoped.
- [ ] `GET /api/notifications/unread-count` protected va profile-scoped.
- [ ] `POST /api/content-schedules` tao schedule neu co content/integration hop le.
- [ ] `GET /api/content-schedules/upcoming` protected va profile-scoped.
- [ ] `GET /api/dashboard/summary` protected va profile-scoped.
- [ ] Development only: `POST /api/dev/scheduler/run-now` chi xuat hien trong Development.

### Smoke group G - Negative/boundary

- [ ] Protected endpoint khong token tra `401`.
- [ ] Profile-scoped endpoint thieu `X-Profile-Id` tra loi ro rang.
- [ ] `X-Profile-Id` khong thuoc user bi chan.
- [ ] Production khong expose `/api/dev/scheduler/run-now`.
- [ ] Payment/Admin/Team/Approval/Ads planned endpoints khong duoc frontend active flow goi.

## Frontend implementation relevance

Story nay khong yeu cau build UI san pham, nhung can output docs de frontend implement dung:

- Frontend endpoint map phai bam `API_SMOKE_CHECKLIST.md`.
- `lib/api.ts` phai phan biet active vs planned endpoints.
- Frontend dev can chay smoke group A/B/C truoc khi debug UI.
- Khi UI loi API, dev can dung checklist de xac dinh loi frontend hay backend/config.
- Docs can ghi ro backend base URL mac dinh va headers bat buoc.

## Acceptance criteria

- Co file setup/runbook backend cap nhat theo active codebase.
- Co file API smoke checklist rieng hoac section smoke checklist ro trong docs.
- Docs co lenh restore/build/test/run/migration.
- Docs co mau `.env` toi thieu cho DB/JWT/frontend base URL.
- Docs co danh sach config optional cho Gemini/Facebook/PayOS/Supabase.
- Docs co active endpoint map khop controllers hien tai.
- Docs co planned endpoint list de frontend khong goi nham.
- Docs co header rules cho `Authorization` va `X-Profile-Id`.
- Docs co smoke checklist Auth/Profile/Brand/Product/Content/AI/Social/Notification/Scheduling/Dashboard.
- Docs co expected behavior khi thieu external config.
- Docs co negative smoke cho `401`, missing profile scope, dev-only route.
- Docs co noi ghi ket qua smoke lan cuoi: date, tester, backend commit, DB state, blockers.
- Link tu `docs/main/setup-guide.md` toi smoke checklist neu tao file moi.

## Suggested file structure

```text
AISAM-BE/docs/
  BACKEND_RUNBOOK.md
  API_SMOKE_CHECKLIST.md
  API_TESTING.md
```

Toi thieu nen co:

```text
AISAM-BE/docs/API_SMOKE_CHECKLIST.md
```

## Suggested smoke result template

```md
## Smoke Result - YYYY-MM-DD

- Tester:
- Backend commit:
- Environment:
- API base URL:
- Database:
- External config:
  - Gemini:
  - Facebook:
  - PayOS:

| Group | Status | Notes |
| --- | --- | --- |
| Build/Host | Pass/Fail/Blocked |  |
| Auth | Pass/Fail/Blocked |  |
| Profile/Brand/Product | Pass/Fail/Blocked |  |
| Content/AI/Conversation | Pass/Fail/Blocked |  |
| Social/Posts | Pass/Fail/Blocked |  |
| Notifications/Scheduling/Dashboard | Pass/Fail/Blocked |  |
| Negative/Boundary | Pass/Fail/Blocked |  |

Known blockers:

-
```

## Test cases cho story docs

- Developer moi doc docs va chay duoc `dotnet build`.
- Developer moi doc docs va chay duoc `dotnet test`.
- Developer moi tao `.env` theo docs va API host start duoc.
- Swagger URL trong docs mo duoc.
- Health endpoint trong docs dung casing route hien tai.
- Auth smoke request co du `confirmPassword`.
- Profile/product notes ghi ro file upload chua active.
- Dashboard endpoint trong docs la `/api/dashboard/summary`, khong phai `/api/dashboard/stats`.
- Notification endpoints trong docs la `mark-read`, `mark-all-read`, `unread-count`.
- Schedule endpoints trong docs la `/api/content-schedules`, khong phai `/api/content-calendar`.
- Payment/Admin/Team/Approval/Ads duoc danh dau planned.

## Dependencies / blockers

- Can backend local co PostgreSQL va JWT config de chay full smoke DB.
- Gemini success smoke can `GEMINI_API_KEY` hop le.
- Facebook OAuth/publish real smoke can Meta app credentials, redirect URI va Page permissions.
- Payment/Admin APIs chua active, chi document planned/blocked.
- Neu backend route thay doi, docs va frontend endpoint map phai cap nhat cung luc.
