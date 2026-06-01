# AISAM Backend Codebase Update Plan

Last reviewed: 2026-05-30

Tai lieu nay tra loi cau hoi: voi yeu cau do an hien tai, so voi active codebase trong repo moi thi can update nhung gi, theo phase nao, va can bam source cu / guardrails / backend code plan nhu the nao.

## 1. Ket luan ngan gon

Huong di da chot: bam theo `BACKEND_CODE_PLAN.md` lam khung phase chinh.

Trang thai active codebase hien tai:

- Phase 0 - repo structure: co ban da co.
- Phase 1 - API host toi thieu: da co Swagger, CORS, middleware/filter, health.
- Phase 2 - Common/domain/database context: da co mot phan lon model/DbContext/migration.
- Phase 3 - Authentication MVP: da co AuthController, AuthService, EmailService, UserRepository, SessionRepository, JWT.
- Phase 4 - Profile/Brand/Product MVP: da co controller/service/repository cho 3 module.
- Phase 5 tro di: chua migrate vao active codebase, du entity/DbSet nhung thieu controller/service/repository/DTO/DI/validation/test cho nhieu module.

Source cu trong `docs/code-references/PRN232_Backend` la baseline chinh. Neu module cu da on, uu tien copy/tai su dung va chi sua toi thieu theo guardrails.

Muc tieu ngan han cho do an:

1. Lam chac foundation hien tai truoc khi copy module lon.
2. Migrate Content + AI + Conversation vi day la diem chinh cua de tai AISAM.
3. Migrate Social/Facebook publishing de co flow demo tu tao noi dung den dang bai.
4. Migrate Scheduling/Notification/Dashboard de demo van hanh.
5. Migrate Payment/Subscription/Quota va Admin MVP neu can demo day du SaaS.
6. Chi dua Facebook Ads nang cao, Instagram/TikTok, AI video vao post-MVP neu con thoi gian.

## 2. Tai lieu va source dung lam can cu

Can cu bat buoc:

- `DEVELOPMENT_GUARDRAILS.md`: quy tac lam tung buoc nho, source cu la baseline, khong refactor khong can thiet, moi phase phai build/test/API test.
- `BACKEND_CODE_PLAN.md`: khung phase 0-10 cho backend .NET 8.
- `docs/superpowers/CODEBASE.md`: mo ta active codebase hien tai.
- `docs/code-references/PRN232_Backend`: source cu dung de copy/tai su dung.

Khong duoc hieu nham:

- `README.md` va `AISAM_BACKEND_PROGRESS_VS_SRS.md` mo ta source cu/vision day du hon active codebase hien tai.
- Active root codebase hien tai moi expose Auth, Profile, Brand, Product, Health.
- Nhieu entity da co trong `AisamContext`, nhung neu thieu controller/service/repository/DI thi chua tinh la module active.

## 3. So sanh tong quan active codebase voi source cu

### 3.1 Controllers

Active root controllers:

- `AuthController`
- `BrandController`
- `HealthController`
- `ProductController`
- `ProfileController`

Source cu co them:

- `AdCampaignsController`
- `AdCreativesController`
- `AdminToolsController`
- `AdsController`
- `AdSetsController`
- `ApprovalController`
- `ContentCalendarController`
- `ContentController`
- `ConversationController`
- `DashboardController`
- `GeminiController`
- `NotificationController`
- `PaymentController`
- `PostsController`
- `SocialAccountController`
- `SocialAuthController`
- `SocialIntegrationController`
- `StorageController`
- `TeamController`
- `TeamMemberController`
- `UserController`

Y nghia update:

- Current API thieu gan nhu toan bo flow AI/social/content/SaaS cua do an.
- Can migrate theo phase, khong copy tat ca controller cung luc.

### 3.2 Services

Active root services:

- `AuthService`
- `BrandService`
- `EmailService`
- `ProductService`
- `ProfileService`

Source cu co them:

- `AIService`
- `ApprovalService`
- `ContentService`
- `ConversationService`
- `FacebookProvider`
- `FacebookMarketingApiService`
- `GoogleProvider`
- `NotificationService`
- `NotificationCleanupService`
- `PayOSPaymentService`
- `PostService`
- `ScheduledPostingService`
- `ScheduledPostingBackgroundService`
- `SocialService`
- `SubscriptionValidationService`
- `SupabaseStorageService`
- `TeamService`
- `TeamMemberService`
- `UserService`
- Ads services: `AdCampaignService`, `AdSetService`, `AdCreativeService`, `AdService`, `AdQuotaService`
- `BucketInitializerService`

Y nghia update:

- Phase 5 can copy `ContentService`, `AIService`, `ConversationService`.
- Phase 6 can copy `SocialService`, provider contracts, `FacebookProvider`, `PostService`.
- Phase 7 can copy notification/scheduling/dashboard-related services.
- Phase 8 can copy payment/subscription/quota services.
- Phase 9 can copy admin/user/team-related services if needed.

### 3.3 Repositories

Active root repositories:

- `BrandRepository`
- `ProductRepository`
- `ProfileRepository`
- `SessionRepository`
- `UserRepository`

Source cu co them:

- `AdCampaignRepository`
- `AdCreativeRepository`
- `AdRepository`
- `AdSetRepository`
- `AiGenerationRepository`
- `ApprovalRepository`
- `ContentCalendarRepository`
- `ContentRepository`
- `ConversationRepository`
- `NotificationRepository`
- `PaymentRepository`
- `PerformanceReportRepository`
- `PostRepository`
- `SocialAccountRepository`
- `SocialIntegrationRepository`
- `SubscriptionRepository`
- `TeamBrandRepository`
- `TeamMemberRepository`
- `TeamRepository`

Y nghia update:

- Active `AisamContext` da co DbSet cho nhieu entity nay, nen repository migration co the uu tien copy tu source cu.
- Moi repository copy xong phai dang ky DI va co test/API smoke lien quan.

### 3.4 DTO/Validators/Middleware/Utils

Active root DTO chi co auth/profile/brand/product/basic social DTO. Source cu co them DTO cho:

- Content
- Conversation
- Approval
- Notification
- Payment
- Subscription
- Team
- Social integration
- Ad campaign/ad set/ad creative/ad
- File/storage
- Facebook response models
- Gemini models

Source cu co validators cho nhieu request. Active root chi co global validation filter, chua thay validator registration/validators active.

Source cu co middleware/utils them:

- `UserProvisioningMiddleware`
- `UserClaimsHelper`
- `ProfileContextHelper`

Y nghia update:

- Khi migrate controller nao, phai copy dung DTO + validator + helper lien quan.
- Khong nen copy validators tat ca cung luc neu module chua migrate.

## 4. Ranh gioi MVP va post-MVP

### Nen lam cho MVP do an

- Auth/profile/brand/product foundation on dinh.
- Content CRUD va lifecycle.
- AI generate/improve/chat bang Gemini theo source cu.
- Conversation history.
- Facebook social connection va page publishing.
- Scheduling/content calendar.
- Notification co ban.
- Dashboard co ban.
- Payment/subscription/quota display neu can demo SaaS.
- Admin user/payment/subscription tools neu can cho demo/quan tri.

### Khong nen lam voi truoc khi MVP pass

- Facebook Ads nang cao neu chua can demo.
- Instagram/TikTok/Twitter provider that.
- AI video generation.
- Dynamic subscription plan CRUD.
- Budget auto-optimization.
- Sentiment analysis/trend prediction.
- Mobile app Flutter.

Ly do:

- Guardrails yeu cau tung buoc nho, build/test duoc.
- Cac module post-MVP phu thuoc external API/quyen/cost cao.
- Current codebase chua co test nen nen giam blast radius.

## 5. Phase Update A - Stabilize foundation hien tai

Map voi `BACKEND_CODE_PLAN.md`: hoan tat/kiem tra lai Phase 0-4.

### Muc tieu

Lam chac codebase hien tai truoc khi migrate module moi.

### Trang thai hien tai

Da co:

- Solution .NET 8 5 project + test project.
- Swagger/Health.
- JWT auth.
- EF Core PostgreSQL context/migrations.
- Auth/Profile/Brand/Product API.

Can update/fix:

- Xac thuc build clean va test clean trong moi truong co NuGet restore.
- Them meaningful tests thay vi `UnitTest1` rong.
- Fix authorization gap cua Profile endpoints: route `userId` can khop JWT user hoac co admin policy.
- Quyet dinh xu ly file upload: tiep tuc reject trong MVP hay bat SupabaseStorageService som.
- Fix mojibake/encoding trong `EmailService`.
- Kiem tra `.env.example` va setup guide khop config thuc te.
- Ghi lai Swagger smoke test cho Auth/Profile/Brand/Product.

### Source cu can tham chieu

- `docs/code-references/PRN232_Backend/AISAM.API/Program.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Filters/ValidationFilter.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Middleware/ExceptionHandlerMiddleware.cs`
- Current copied modules Auth/Profile/Brand/Product trong source cu.

### File can tao/copy/sua

Sua:

- `AISAM.API/Controllers/ProfileController.cs`
- `AISAM.Services/Service/ProfileService.cs`
- `AISAM.Services/Service/EmailService.cs`
- `AISAM.API/.env.example`
- `tests/AISAM.IntegrationTests/*`

Co the tao:

- Test helpers/factory cho integration tests.
- Auth/Profile/Brand/Product integration test files.

### Database impact

Khong nen co migration trong phase nay, tru khi fix bug schema bat buoc.

### API can test

- `GET /api/Health`
- `POST /api/Auth/register`
- `POST /api/Auth/login`
- `POST /api/Auth/refresh`
- `GET /api/Auth/me`
- `GET /api/profiles/user/{userId}`
- `POST /api/profiles/user/{userId}`
- `GET /api/brands`
- `POST /api/brands`
- `GET /api/products`
- `POST /api/products`

### Definition of Done

- `dotnet build` pass.
- `dotnet test` pass.
- Swagger opens.
- Health/Auth/Profile/Brand/Product smoke test pass.
- Authorization gap documented or fixed.
- No unrelated refactor.

## 6. Phase Update B - Complete Phase 5: AI, Content, Conversation MVP

Map voi `BACKEND_CODE_PLAN.md`: Phase 5.

### Muc tieu

Them flow chinh cua de tai: tao/quan ly noi dung va sinh/cai thien noi dung bang AI.

### Trang thai hien tai

Da co entity/DbSet:

- `Content`
- `AiGeneration`
- `Conversation`
- `ChatMessage`

Thieu active API/service/repository:

- `ContentController`
- `GeminiController` hoac AI endpoints tu source cu.
- `ConversationController`
- `ContentService`, `AIService`, `ConversationService`
- `ContentRepository`, `AiGenerationRepository`, `ConversationRepository`
- DTO request/response cho content, AI, conversation.
- Validators lien quan.
- DI registration.

### Source cu can copy/tai su dung

Controllers:

- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/ContentController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/GeminiController.cs`
- `docs/code-references/PRN232_Backend/AISAM.API/Controllers/ConversationController.cs`

Services:

- `AISAM.Services/Service/ContentService.cs`
- `AISAM.Services/Service/AIService.cs`
- `AISAM.Services/Service/ConversationService.cs`

Interfaces:

- `IContentService.cs`
- `IAIService.cs`
- `IConversationService.cs`

Repositories:

- `ContentRepository.cs`
- `AiGenerationRepository.cs`
- `ConversationRepository.cs`

DTO/Models:

- `CreateContentRequest.cs`
- `UpdateContentRequest.cs`
- `ContentResponseDto.cs`
- `ConversationResponseDto.cs`
- `ConversationDetailDto.cs`
- `GeminiModels.cs`

Validators:

- `CreateContentRequestValidator.cs`
- `UpdateContentRequestValidator.cs`

### File can sua

- `AISAM.API/Program.cs`: DI va config AI.
- `AISAM.Common/*`: copy DTO/models.
- `AISAM.Services/*`: copy services/interfaces.
- `AISAM.Repositories/*`: copy repositories/interfaces.
- `AISAM.API/Controllers/*`: copy controllers.

### Database impact

Kiem tra current migrations da co day du bang/cot cho `Contents`, `AiGenerations`, `Conversations`, `ChatMessages`. Neu schema khop source cu thi khong tao migration. Neu thieu field theo DTO/service source cu, tao migration nho rieng.

### External config

Can config Gemini/AI:

- Gemini API key/model settings theo source cu/setup guide.
- Neu Image generation qua Vertex/Supabase chua can, co the disable image flow cho MVP text first.

### API can test

- Content CRUD/list/detail.
- AI generate draft.
- AI improve content.
- AI generations by content.
- Conversation list/detail/delete.

### Risk

- AI external API co the fail do key/quota/network.
- Service cu co the phu thuoc Supabase/Vertex neu copy nguyen.

### Rollback

- Remove DI registrations.
- Remove copied controllers/services/repositories/DTO.
- Revert migration neu co.
- Tat AI feature bang config neu can.

### Definition of Done

- Build/test pass.
- Content CRUD pass voi DB local.
- AI endpoint co test thanh cong hoac graceful error khi thieu config.
- Conversation flow pass neu AI/chat duoc bat.

### Ket qua trien khai Phase B - 2026-05-31

Da hoan tat cac task:

- `B0`: ra schema Phase B.
- `B1`: them `ActiveProfileMiddleware` validate `X-Profile-Id` thuoc JWT user.
- `B2`: them Content repository/service MVP.
- `B3`: expose Content controller va Swagger paths.
- `B4`: them Gemini text client va AI generation service.
- `B5`: them Conversation persistence, AI chat va Gemini controller.
- `B6`: them Conversation history service/controller.
- `B7`: chay verification va ghi lai blocker external.

Active API moi:

```text
POST   /api/content
GET    /api/content
GET    /api/content/{contentId}
PUT    /api/content/{contentId}
POST   /api/content/{contentId}/clone
DELETE /api/content/{contentId}
POST   /api/content/{contentId}/restore

POST   /api/ai/generate-draft
POST   /api/ai/improve/{contentId}
POST   /api/ai/approve/{aiGenerationId}
GET    /api/ai/generations/{contentId}
POST   /api/ai/chat

GET    /api/conversations
GET    /api/conversations/{id}
DELETE /api/conversations/{id}
```

Header bat buoc cho Content/AI/Conversation:

```text
Authorization: Bearer <access-token>
X-Profile-Id: <owned-profile-guid>
```

#### Source cu da kiem tra

| Loai | Duong dan source cu | Cach su dung |
| --- | --- | --- |
| Content | `docs/code-references/PRN232_Backend/AISAM.Services/Service/ContentService.cs` | Tai su dung lifecycle CRUD/clone/restore co chon loc; bo publish, approval, team, quota |
| Content repository | `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/ContentRepository.cs` | Tai su dung query pattern; them profile scope, cancellation token, paging clamp |
| AI | `docs/code-references/PRN232_Backend/AISAM.Services/Service/AIService.cs` | Tach text-only client; khong copy Vertex/Supabase/chat monolith |
| Conversation | `docs/code-references/PRN232_Backend/AISAM.Repositories/Repository/ConversationRepository.cs` | Tai su dung include pattern; them `PagedResult`, cancellation token, message persistence |
| Controllers | `docs/code-references/PRN232_Backend/AISAM.API/Controllers` | Viet lai boundary voi `[Authorize]`, active profile middleware va service status code |

#### Cai tien thay vi copy nguyen

- Khong tin `ProfileId` tu body request.
- Validate `X-Profile-Id` thuoc JWT user cho `/api/content`, `/api/ai`, `/api/conversations`.
- Enforce ownership theo profile cho content, generation va conversation.
- `Product` khong co `ProfileId`; validate qua brand ownership va `Product.BrandId`.
- Gemini key optional khi startup. Thieu key thi generation luu `Failed`, chat tra graceful error.
- Khong keo Vertex image generation, Supabase upload, social publish, approval, team, notification hoac quota vao Phase B.

#### Database/migration

- Da doi chieu entity, `DbSet` va `AisamContextModelSnapshot`.
- Cac bang `contents`, `ai_generations`, `conversations`, `chat_messages` va cot bat buoc da ton tai.
- Khong tao migration Phase B moi.
- `dotnet ef migrations list` liet ke 5 migration cu.
- Chua xac minh applied status va `database update` vi PostgreSQL local khong lang nghe tai `127.0.0.1:5432`.

#### Verification thuc te

```text
dotnet build AISAM.sln
Build succeeded. 0 warnings, 0 errors.

dotnet test AISAM.sln
Passed: 36, Failed: 0, Skipped: 0.

Swagger JSON: HTTP 200.
Health: HTTP 200.
Swagger co Content, AI va Conversation paths.
Content/AI/Conversation request khong JWT: HTTP 401.
```

#### Blocker external va smoke con lai

- Docker daemon/PostgreSQL local dang tat, nen chua chay Content CRUD HTTP smoke co persistence.
- Chua chay AI success HTTP smoke vi khong co `GEMINI_API_KEY` hop le.
- Missing-key behavior da co automated test: API host van start; generation luu `Failed`; chat tra graceful error.

Can rerun sau khi bat PostgreSQL va co credential:

```text
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
Content CRUD smoke
AI generate/improve/approve/chat success smoke
Conversation list/detail/delete persistence smoke
```

#### Rollback

- Remove DI Phase B trong `AISAM.API/Program.cs`.
- Remove `ActiveProfileMiddleware`, Content/AI/Conversation controllers.
- Remove Content/AI/Conversation service va repository implementations moi.
- Remove DTO/models Phase B moi.
- Khong can rollback migration vi Phase B khong tao migration.

## 7. Phase Update C - Complete Phase 6: Social integration va Facebook publishing

Map voi `BACKEND_CODE_PLAN.md`: Phase 6.

### Muc tieu

Cho phep ket noi Facebook va publish content len Facebook Page theo source cu.

### Trang thai hien tai

Da co entity/DbSet:

- `SocialAccount`
- `SocialIntegration`
- `Post`

Thieu active API/service/repository:

- `SocialAuthController`
- `SocialAccountController`
- `SocialIntegrationController`
- `PostsController`
- `SocialService`
- `FacebookProvider`
- `GoogleProvider` neu dung provider contract.
- `PostService`
- Social/post repositories.
- Facebook settings/models.

### Source cu can copy/tai su dung

Controllers:

- `SocialAuthController.cs`
- `SocialAccountController.cs`
- `SocialIntegrationController.cs`
- `PostsController.cs`

Services:

- `SocialService.cs`
- `FacebookProvider.cs`
- `GoogleProvider.cs`
- `PostService.cs`

Interfaces:

- `ISocialService.cs`
- `IProviderService.cs`
- `IPostService.cs`

Repositories:

- `SocialAccountRepository.cs`
- `SocialIntegrationRepository.cs`
- `PostRepository.cs`

DTO/models:

- `SocialDtos.cs`
- `FacebookSettings.cs`
- `FacebookModels.cs`
- `GoogleModels.cs`
- `PostDtos.cs`
- `PostListIemDto.cs`
- `PublishContentRequest.cs`
- `PublishPostRequest.cs`
- `SocialCallbackRequest.cs`

Validators:

- `PublishRequestValidator.cs`
- `LinkSelectedTargetsRequestValidator.cs`

### File can sua

- `Program.cs`: DI social providers/repositories/services, config settings, HttpClient neu source cu can.
- `appsettings`/`.env.example`: Facebook OAuth/Graph config.
- Common/config/models DTO.

### Database impact

Kiem tra schema social/post co khop source cu. Neu source cu co token fields/callback fields chua co trong current migration, tao migration rieng.

### API can test

- Generate Facebook auth URL.
- Callback/manual token flow neu co.
- List social accounts.
- List available targets.
- Link selected targets.
- Link ad account neu can for future ads.
- Publish content to Facebook Page.
- List posts by content/profile.

### Risk

- Can Facebook app credentials, redirect URI, permissions, page access token.
- Facebook API permissions co the khong du cho demo.

### Rollback

- Remove social/publishing DI.
- Remove controllers.
- Revert migration neu co.
- Disable social feature through config.

### Definition of Done

- Build/test pass.
- API returns clear config error if Facebook settings missing.
- With valid token, publish smoke test pass or documented blocker from Facebook permission.

## 8. Phase Update D - Complete Phase 7: Notification, Scheduling, Dashboard

Map voi `BACKEND_CODE_PLAN.md`: Phase 7.

### Muc tieu

Hoan thien flow van hanh sau content/publishing: thong bao, dat lich dang bai, dashboard co ban.

### Trang thai hien tai

Da co entity/DbSet:

- `Notification`
- `ContentCalendar`
- `PerformanceReport`

Thieu active API/service/repository:

- `NotificationController`
- `ContentCalendarController`
- `DashboardController`
- `NotificationService`
- `NotificationCleanupService`
- `ScheduledPostingService`
- `ScheduledPostingBackgroundService`
- Repositories lien quan.

### Source cu can copy/tai su dung

Controllers:

- `NotificationController.cs`
- `ContentCalendarController.cs`
- `DashboardController.cs`

Services:

- `NotificationService.cs`
- `NotificationCleanupService.cs`
- `ScheduledPostingService.cs`
- `ScheduledPostingBackgroundService.cs`

Repositories:

- `NotificationRepository.cs`
- `ContentCalendarRepository.cs`
- `PerformanceReportRepository.cs`

DTO:

- `NotificationResponseDto.cs`
- `NotificationListDto.cs`
- `CreateNotificationRequest.cs`
- `UpdateNotificationRequest.cs`
- `ContentCalendarResponseDto.cs`
- schedule/publish request DTOs.

### File can sua

- `Program.cs`: DI va hosted services.
- `appsettings`: background service interval neu source cu co.

### Database impact

Kiem tra ContentCalendar/Notification schema. Background job khong nen tao schema moi tru khi can retry metadata.

### API can test

- Notification list/detail/mark-read/unread-count.
- Schedule content.
- Update/delete schedule.
- Upcoming schedules.
- Dashboard stats.
- Background service smoke: schedule due item in test/local DB.

### Risk

- Background service co the publish lap lai neu idempotency khong ro.
- Can logging va retry behavior ro.

### Rollback

- Remove hosted service registration first.
- Remove controllers/services if needed.
- Revert migration neu co.

### Definition of Done

- Build/test pass.
- Scheduling API pass.
- Background service khong crash khi DB/external social config missing.
- Notification API pass.

## 9. Phase Update E - Complete Phase 8: Payment, Subscription, Quota

Map voi `BACKEND_CODE_PLAN.md`: Phase 8.

### Muc tieu

Ho tro monetization/SaaS demo: subscription, PayOS payment, quota display.

### Trang thai hien tai

Da co entity/DbSet:

- `Subscription`
- `Payment`

Thieu active API/service/repository:

- `PaymentController`
- `PayOSPaymentService`
- `SubscriptionValidationService`
- `PaymentRepository`
- `SubscriptionRepository`
- quota service/display endpoints.

### Source cu can copy/tai su dung

Controller:

- `PaymentController.cs`

Services:

- `PayOSPaymentService.cs`
- `SubscriptionValidationService.cs`
- `AdQuotaService.cs` neu dung quota lien quan ads.

Repositories:

- `PaymentRepository.cs`
- `SubscriptionRepository.cs`

DTO:

- `CreatePaymentIntentRequest.cs`
- `CreateSubscriptionRequest.cs`
- `ChangePlanRequest.cs`
- `PaymentResponseDto.cs`
- `PayOSCheckoutResponse.cs`
- `SubscriptionResponseDto.cs`

### File can sua

- `Program.cs`: DI payment/subscription services/repositories.
- `.env.example`/setup docs: PayOS config.
- `appsettings`: payment settings neu source cu dung.

### Database impact

Kiem tra migration `UpdateSubscriptionPayOS` da du voi source cu chua. Neu chua, tao migration nho rieng.

### API can test

- Create checkout/payment intent.
- PayOS webhook/callback.
- Get payment history.
- Get current subscription.
- Change/cancel plan neu source cu co.
- Quota display endpoint neu implement.

### Risk

- Payment webhook can signature validation dung.
- Demo co the can sandbox PayOS credentials.

### Rollback

- Disable payment endpoints/DI.
- Revert migration neu co.
- Keep existing subscription enum fallback.

### Definition of Done

- Build/test pass.
- Payment endpoints return safe config error if PayOS missing.
- Sandbox checkout/webhook test documented.

## 10. Phase Update F - Complete Phase 9: Admin backend MVP

Map voi `BACKEND_CODE_PLAN.md`: Phase 9.

### Muc tieu

Them admin/user tools can cho demo va quan tri du lieu.

### Trang thai hien tai

Da co:

- `UserRepository.GetPagedUsersAsync`
- `UserRoleEnum.Admin`

Thieu:

- `UserController`
- `AdminToolsController`
- `UserService`
- Admin authorization/policies ro rang.
- Admin DTO/request types can thiet.

### Source cu can copy/tai su dung

Controllers:

- `UserController.cs`
- `AdminToolsController.cs`

Services:

- `UserService.cs`

Interfaces:

- `IUserService.cs`

DTO:

- `UserListDto.cs` da co mot phan.
- `SeedDemoUserRequest.cs`
- Admin/payment/profile/subscription request DTO tu source cu neu endpoint can.

### File can sua

- `Program.cs`: DI `IUserService`.
- Authorization policy cho Admin.
- `AuthService`/JWT claims neu role policy can them.

### Database impact

Khong nen co migration tru khi AdminTools can field thieu.

### API can test

- Admin user list.
- Admin seed demo user.
- Admin update profile/payment/subscription status neu migrate.
- Non-admin request must be 403.

### Risk

- Admin tools co the thay doi du lieu that.
- Seed endpoint khong nen expose trong production without guard.

### Rollback

- Disable/remove AdminToolsController.
- Keep UserController read-only neu can.

### Definition of Done

- Build/test pass.
- Admin policy pass.
- Non-admin blocked.
- Seed/demo endpoints documented and protected.

## 11. Phase Update G - Complete Phase 10: Test hardening va backend release MVP

Map voi `BACKEND_CODE_PLAN.md`: Phase 10.

### Muc tieu

Bien codebase thanh backend MVP co the demo/release co kiem chung.

### Trang thai hien tai

Test project co nhung test rong.

### Can update

Them test cho:

- API host/health.
- Auth register/login/refresh/me.
- Profile ownership.
- Brand ownership and soft delete/restore.
- Product ownership and soft delete/restore.
- Content CRUD.
- AI endpoints with mocked/fake provider where possible.
- Social publish behavior with mocked provider where possible.
- Scheduling service logic.
- Payment webhook signature/handler if migrate.

Them docs/checklists:

- API testing guide.
- Local setup guide updated theo modules da migrate.
- Phase completion log.
- Migration rollback notes.

### File can tao/sua

- `tests/AISAM.IntegrationTests/*`
- `SETUP_GUIDE.md`
- `docs/api/api-spec.md`
- `docs/database/db-spec.md`
- `docs/superpowers/CODEBASE.md`
- `docs/superpowers/CODEBASE_UPDATE.md`

### Definition of Done

- `dotnet build` pass.
- `dotnet test` pass with meaningful tests.
- Swagger smoke test pass.
- Required env documented.
- Known blockers documented.

## 12. Phase Update H - Post-MVP optional modules

Chi thuc hien sau khi Phase A-G pass.

### H1 - Approval va Team permission nang cao

Source cu:

- `ApprovalController`
- `TeamController`
- `TeamMemberController`
- `ApprovalService`
- `TeamService`
- `TeamMemberService`
- repositories/DTO/validators lien quan.

Ly do de sau:

- Can chot nghiep vu leader/member/approval flow.
- `TeamMemberRoleEnum` current chi co `Copywriter`, `Designer`, chua ro leader.

Can hoi truoc khi lam:

- Team co bat buoc 1 leader duy nhat khong?
- Ai duoc approve/publish?
- Approval co SLA/escalation khong?

### H2 - Facebook Ads nang cao

Source cu:

- `AdCampaignsController`
- `AdSetsController`
- `AdCreativesController`
- `AdsController`
- `FacebookMarketingApiService`
- ads services/repositories/DTO/validators.

Ly do de sau:

- Can Facebook Marketing API permission.
- Risk cao ve token/ad account/budget.
- Demo core content/publishing nen xong truoc.

### H3 - Storage/Supabase upload

Source cu:

- `StorageController`
- `SupabaseStorageService`
- `BucketInitializerService`
- `FileDto`
- storage config.

Ly do co the dua som hon neu can upload avatar/product/content media.

### H4 - Instagram/TikTok/Twitter

Chua nen lam trong MVP neu khong co business/demo requirement bat buoc.

Can hoi truoc khi lam:

- Nen support nen tang nao truoc?
- OAuth/publishing/analytics scope nao?
- Co app credentials/permission chua?

### H5 - AI video, dynamic subscription plans, auto optimization

De long-term. Khong nen dua vao current backend migration neu muc tieu la do an demo on dinh.

## 13. Thu tu uu tien de thuc hien

Khuyen nghi thuc hien:

1. Phase A - Stabilize foundation hien tai.
2. Phase B - Content + AI + Conversation.
3. Phase C - Social/Facebook publishing.
4. Phase D - Notification + Scheduling + Dashboard.
5. Phase E - Payment + Subscription + Quota.
6. Phase F - Admin MVP.
7. Phase G - Test hardening/release.
8. Phase H - Optional post-MVP.

Neu thoi gian rat han che:

1. Phase A.
2. Phase B voi AI text only.
3. Phase C voi Facebook publishing toi thieu.
4. Phase G voi tests/smoke docs.

Neu demo can SaaS/payment:

1. Phase A.
2. Phase B.
3. Phase E.
4. Phase F.
5. Phase G.

## 14. Template bat buoc cho tung task trong moi phase

Moi task thuc thi tu tai lieu nay phai ghi theo format sau trong commit/PR/task note:

```md
# Task: <ten task>

## Muc tieu

-

## Source cu da kiem tra

| Loai | Duong dan source cu | Ghi chu |
| --- | --- | --- |
| Controller |  |  |
| Service |  |  |
| Repository |  |  |
| DTO/Model |  |  |
| Validator |  |  |
| Config |  |  |

## File copy/tai su dung

| Source cu | File moi | Ly do giu nguyen |
| --- | --- | --- |
|  |  |  |

## File sua

| File | Noi dung sua | Ly do |
| --- | --- | --- |
|  |  |  |

## Database/migration

- Migration name:
- Bang/cot anh huong:
- Rollback:

## Test

- Build:
- Unit/integration test:
- Swagger/Postman:
- External service:

## Ket qua thuc te

-

## Rollback plan

-
```

## 15. Checklist bat buoc sau moi phase

- [ ] Da kiem tra source cu lien quan trong `docs/code-references/PRN232_Backend`.
- [ ] Da ghi ro file copy/tai su dung.
- [ ] Da ghi ro file tao moi/sua.
- [ ] Da dang ky DI day du trong `Program.cs`.
- [ ] Da kiem tra config/env can thiet.
- [ ] Da kiem tra migration neu co DB change.
- [ ] `dotnet build` pass.
- [ ] `dotnet test` pass.
- [ ] Swagger/Postman API smoke test pass.
- [ ] External service missing config duoc handle ro rang.
- [ ] Rollback plan ro.
- [ ] `CODEBASE.md` va `CODEBASE_UPDATE.md` duoc cap nhat neu phase thay doi kien truc/module.

## 16. Cac cau hoi can chot truoc cac phase lon

Can hoi truoc Phase B:

- AI MVP chi can text generation/improve/chat, hay bat buoc image generation bang Vertex/Supabase?

Can hoi truoc Phase C:

- Demo Facebook publishing dung token/page that hay mock/sandbox?
- Co san Facebook App ID/Secret/redirect URI/page permission chua?

Can hoi truoc Phase D:

- Scheduled posting co can background service that trong demo hay chi can API schedule/list?
- Retry policy can muc nao?

Can hoi truoc Phase E:

- Payment demo co dung PayOS sandbox/real credential khong?
- Subscription quota can enforce hard limit hay chi display?

Can hoi truoc Phase H Approval/Team:

- Team co bat buoc mot Leader duy nhat khong?
- Role hien tai nen mo rong enum hay dung permission JSON/config?

## 17. Ghi chu ve tinh trung thuc cua tai lieu

Tai lieu nay khong noi cac module trong source cu la da co trong active codebase. Mot module chi duoc tinh la active khi co it nhat:

- Controller/API trong `AISAM.API/Controllers`.
- Service interface + implementation trong `AISAM.Services`.
- Repository interface + implementation neu can DB query.
- DTO/request/response trong `AISAM.Common`.
- DI registration trong `Program.cs`.
- API smoke test hoac integration test.

Neu chi co entity/DbSet trong `AisamContext`, module do moi la schema/model-ready, chua phai feature hoan thanh.
