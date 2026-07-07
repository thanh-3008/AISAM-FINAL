# Báo cáo tiến độ backend AISAM so với SRS

## Change Request Impact Notice - Dang Trien Khai

Nguon: `docs/reference/workspace-subscription-credit-analysis.md`.

Policy update 2026-06-24: `docs/main/workspace-subscription-expiry-policy.md`.

Implementation follow-up dang mo:

- Audit de Free/basic fallback chi ap dung cho Personal Workspace.
- Chan Business PendingPayment/Limited/Archived consume Credits.
- Xac minh Business creation grant 0 Credits va payment/renewal grant idempotent.
- Bo sung regression tests chong tao nhieu Business Workspace de farm Free Credits.

Bao cao chi tiet ben duoi duoc lap tu snapshot cu ngay 2026-05-26. Trang thai Workspace hien tai da tien xa hon snapshot do:

- Task 9.1-9.18 da hoan thanh va automated tests pass.
- Workspace/subscription/payment/credits/member quota/feature gate/Post Quota/AI Credit charging da co.
- Tat ca domain trong Task 9.16 da migrate sang Workspace ownership; Task 9.17 da backfill du lieu cu va khoa `WorkspaceId` ownership bat buoc.
- Lifecycle Limited/Archived/Admin Soft Delete va Workspace Dashboard da co; regression va tai lieu cuoi Phase 9 da hoan thanh.
- Audit cuoi Phase 9 da xac minh va sua Workspace quota/dashboard isolation, Free Credits reset 7 ngay, atomic credit/payment transaction va Team Management feature gate.

Do do, cac ty le tien do ben duoi khong duoc hieu la da hoan thanh Workspace Change Request. Tien do Change Request hien tai:

| Hang muc | Trang thai |
|---|---|
| Business decisions va impact analysis | DONE |
| Tai lieu lien quan | IN PROGRESS |
| Workspace entities/migration/API | DONE - Task 9.1-9.18 |
| Subscription/Credits migration | DONE cho payment/subscription/wallet/AI text charging |
| Ownership backfill va regression | DONE |
| Personal-vs-Business expiry policy audit | PLANNED - policy approved 2026-06-24 |

Ngày đánh giá: 2026-05-26  
Tài liệu yêu cầu: `D:\final\AISAM-FINAL\README.md`, `D:\final\AISAM-FINAL\docs/reference/specification-answers.md`  
Source được rà: `D:\NEWCODE\PRN232_Backend`  
Phạm vi: backend .NET trong `PRN232_Backend`, không đánh giá frontend.

## Kết luận tổng quan

Source backend hiện đã có phần lớn các module lõi mà SRS mô tả là "Currently Implemented": auth, profile/subscription, Facebook integration, brand/product/content, AI text/image, approval, publishing/scheduling, Facebook Ads, payment, notification, storage và dashboard cơ bản.

Tuy nhiên, nếu tính cả nhóm Planned/Future Features và 19 câu hỏi cần làm rõ trong SRS, backend vẫn còn nhiều khoảng trống quan trọng: quota/usage thật, security/RBAC hardening, audit log thực thi, dynamic plan, Instagram/TikTok, AI video, retry/monitoring cho background job, content revision, approval SLA, test coverage.

Ước lượng tiến độ:

| Cách tính | Tiến độ ước lượng | Nhận xét |
|---|---:|---|
| So với 14 chức năng hiện tại trong README | 72% | Đủ module chính, nhưng một số phần còn basic hoặc thiếu hardening. |
| So với toàn bộ SRS gồm current + planned + 19 requirement questions | 58% | Hoàn thành nền tảng MVP, chưa đủ production/roadmap đầy đủ. |
| Mức sẵn sàng production backend | 50-55% | Cần xử lý bảo mật, quota, retry, audit, tests trước khi gọi là production-ready. |

## Bảng đánh giá theo nhóm chức năng

| Nhóm chức năng | Yêu cầu trong SRS | Trạng thái trong source | Tiến độ |
|---|---|---|---:|
| Auth & account | Register, login, Google login, JWT, refresh token, session, password reset, verify email | Có controller/service đầy đủ ở `AuthController`, `AuthService`; JWT cấu hình trong `Program.cs`. | 85% |
| Profile & subscription | Profile context, Free/Plus/Premium/PlusTrial, active subscription, PayOS | Có model/API/service; plan còn enum/hardcode, chưa dynamic. | 75% |
| Payment PayOS | Checkout, confirm, webhook, history, admin views | Có `PaymentController`, `PayOSPaymentService`; cần rà verify webhook/signature, proration/refund. | 75% |
| Facebook social integration | OAuth, page/target linking, ad account linking | Có `SocialAuthController`, `SocialAccountController`, `SocialIntegrationController`, `FacebookProvider`. | 75% |
| Instagram/TikTok/Twitter | Planned platform support | Chỉ có enum/comment; chưa có provider thật. | 10% |
| Brand kit | CRUD brand, assign/unassign team, brand context cho AI | Có `BrandController`, `BrandService`, team-brand mapping. | 85% |
| Product management | CRUD product, filter, upload ảnh, link brand/content | Có `ProductController`, `ProductService`; một số `[Authorize]` bị comment ở list/create. | 75% |
| Content library | CRUD, clone, restore, status, TextOnly/ImageText/VideoText | Có `ContentController`, `ContentService`; chưa có revision/version/diff. | 70% |
| AI text generation | Gemini draft, chat, improve | Có `GeminiController`, `AIService`; cần authorize và quota/cost tracking. | 75% |
| AI image generation | Vertex AI Imagen cho ImageText, upload Supabase | Có logic trong `AIService` và storage. | 70% |
| AI video generation | Proposed advanced feature | Chỉ có `VideoUrl`/`GeneratedVideoUrl` field và publish video URL; chưa có pipeline sinh video. | 15% |
| Prompting strategy | Prompt theo brand/product, template/versioning | Có prompt context cơ bản; chưa có prompt template/admin/versioning. | 45% |
| Approval workflow | Submit, pending, approve/reject, notification, team permission | Có `ApprovalController`, `ApprovalService`; chưa có SLA/escalation/delegation/single Leader enforcement rõ. | 65% |
| Team permission/governance | Team CRUD, role/permission, 1 Leader/team rule | Có team/member/permissions JSON; chưa enforce rõ 1 Leader per team bằng DB/service constraint. | 65% |
| Publishing | Publish ngay qua provider, lưu Post record | Có publish content qua provider, Facebook là nền tảng chính. | 70% |
| Scheduled posts | One-time, recurring, background service | Có `ScheduledPostingService` và background service; thiếu retry policy, DLQ, monitoring, membership check cho team schedules. | 60% |
| Facebook Ads | Campaign, ad set, creative, ad, preview, reports | Có controllers/services và Facebook Marketing API service; quota validation bị comment một phần. | 70% |
| Budget optimization | Auto optimize budget, recommendations | Chưa có service/controller tương ứng. | 5% |
| Analytics/reports | Dashboard stats, Facebook insights, reports | Có dashboard và performance reports cơ bản; nhiều số liệu còn 0/basic, chưa caching/latency strategy. | 45% |
| Notification | List/detail/read/read-all/unread count | Có `NotificationController`, `NotificationService`, cleanup background. | 75% |
| Conversation | AI chat history/list/detail/delete | Có model/service/controller. | 75% |
| Storage | Upload/download/list/delete/signed/public URL, validate file | Có `StorageController`, `SupabaseStorageService`; controller chưa có `[Authorize]`. | 70% |
| Admin tools | User/payment/subscription/profile tools, seed data | Có `AdminToolsController` và admin payment endpoints; `AdminToolsController` chưa authorize/admin guard. | 45% |
| Dynamic subscription plans | Admin CRUD plan/pricing/quota | Chưa có entity/API CRUD plan riêng, đang enum/hardcode. | 20% |
| Audit log | Audit trail, retention, security/RBAC | Có model/DbSet `AuditLog`, nhưng chưa thấy ghi log/expose API đầy đủ. | 20% |
| Provider architecture | Social/payment/AI abstraction | Có `IProviderService` cho social, PayOS service riêng; AI chưa tách provider rõ như `IAIProvider`. | 55% |
| Tests | Unit/integration tests cho payment, approval, scheduling, AI | Không thấy test project trong repo. | 5% |

## Những thứ đã có

### 1. Kiến trúc backend

- Solution .NET theo tầng: `AISAM.API`, `AISAM.Common`, `AISAM.Data`, `AISAM.Repositories`, `AISAM.Services`.
- EF Core context có DbSet cho hầu hết domain chính: users, profiles, teams, brands, products, contents, posts, social accounts/integrations, subscriptions, payments, approvals, ads, reports, notifications, audit logs, conversations.
- Có DI cho repository/service, JWT auth, Swagger, CORS, FluentValidation, background services.

### 2. Authentication và account

- Đăng ký, đăng nhập, Google login.
- JWT Bearer auth, refresh token, session management.
- Logout, logout all, list sessions.
- Change password, forgot/reset password, change password with token.
- Verify email và resend verification email.

### 3. Profile, subscription và payment

- CRUD profile, profile theo user, soft delete/restore.
- Subscription Free/Plus/Premium/PlusTrial theo enum.
- PayOS checkout link, confirm payment, webhook, payment history.
- Admin payment endpoints để xem payments/subscriptions toàn hệ thống hoặc theo user.

### 4. Social/Facebook integration

- OAuth URL/callback theo provider.
- Lưu social account/token.
- Lấy available targets, link/unlink targets, linked targets.
- Lấy accounts-with-targets và ad accounts.
- Link Facebook ad account vào social integration/brand.
- Facebook provider hỗ trợ publish text/image/video URL và ad account discovery.

### 5. Brand và product

- CRUD brand, list theo profile/team, assign/unassign brand to team, restore.
- Brand có các trường context như description, slogan, USP, target audience.
- CRUD product, list/filter/search, upload ảnh Supabase, lưu images dạng JSON.
- Product được dùng làm context cho AI.

### 6. Content lifecycle

- Create/update/delete/restore/clone content.
- List/detail content.
- Status flow cơ bản: Draft, PendingApproval, Approved, Published, Rejected.
- Hỗ trợ `TextOnly`, `ImageText`, `VideoText`.
- Content liên kết với brand, product, approvals, calendars, posts, ad creatives.

### 7. AI content

- Generate draft bằng Gemini.
- Improve content.
- Approve AI generation để cập nhật content.
- Get generations theo content.
- Chat AI với brand/product context.
- ImageText có luồng tạo visual prompt và gọi Vertex AI Imagen, sau đó upload ảnh lên Supabase.

### 8. Approval và team permission

- Team CRUD, get team by vendor/user, get members, permissions, team stats.
- Team member có role/permissions/is active.
- Approval CRUD/list/pending/count/approve/reject/delete/restore.
- Có notification liên quan approval ở mức cơ bản.

### 9. Publishing và scheduled posts

- Publish ngay qua `api/content/{contentId}/publish/{integrationId}`.
- Lưu `Post` record sau khi publish.
- Schedule one-time và recurring.
- Update/cancel schedule, upcoming schedules, team schedules.
- Background service xử lý lịch đến hạn.

### 10. Facebook Ads

- Ad campaign, ad set, ad creative, ad.
- Tạo creative từ content hoặc Facebook post.
- Preview, update status, delete.
- Pull reports/insights từ Facebook Marketing API.
- Có model `PerformanceReport`.

### 11. Dashboard, reports, notification, conversation, storage

- Dashboard stats cơ bản.
- Notification list/detail/read/read-all/unread-count.
- Conversation list/detail/delete.
- Supabase storage upload/download/list/delete/signed/public URL.
- Validate ảnh/video và giới hạn size trong storage service.

## Những thứ cần cải tiến hoặc còn thiếu

### Ưu tiên cao

1. **Khóa bảo mật các endpoint nhạy cảm**
   - `AdminToolsController` chưa có `[Authorize]` và chưa check admin role.
   - `GeminiController` chưa có `[Authorize]`, request còn nhận `UserId` từ body.
   - `StorageController` chưa có `[Authorize]`.
   - `ContentCalendarController` chưa gắn `[Authorize]` ở controller/action.
   - `ProductController` có list/create bị comment `[Authorize]`.

2. **Enforce quota/usage thật**
   - `ContentService.CheckSubscriptionAndQuotaForProfile` đang TODO và `return true`.
   - `SubscriptionValidationService` trả quota theo plan nhưng chưa trừ usage.
   - Chưa có bảng usage counters cho AI, content, posts, storage, social accounts, ads.
   - AI quota trong SRS nói "basic", nhưng source hiện chưa đủ để xem là quota tracking thật.

3. **Team Leader governance**
   - Chưa thấy constraint rõ "mỗi team chỉ có một Leader".
   - Cần validate ở create/update team member.
   - Cần làm rõ quyền Leader/Manager/Member với approve, publish, manage members.

4. **Approval SLA và escalation**
   - Chưa có `ApprovalSLA`, delegation, auto-escalation, overdue notification.
   - Chưa có workflow xử lý Leader vắng mặt.

5. **Scheduled posting reliability**
   - Thiếu retry policy có số lần retry.
   - Thiếu dead-letter/failed schedule status.
   - Thiếu monitoring/admin view cho job lỗi.
   - `GetTeamSchedules` có TODO membership verification và đang trust frontend.

6. **Audit log thực thi**
   - Có `AuditLog` model/DbSet nhưng chưa thấy ghi audit nhất quán.
   - Cần audit cho auth, payment, subscription, content, approval, social token, ads, admin actions.

7. **Token security**
   - Social token đang lưu trong model dạng string, comment có nhắc encrypted nhưng cần xác nhận mã hóa thật.
   - Cần encrypt/rotate/revoke token, tránh log token.

8. **Test coverage**
   - Không thấy test project.
   - Cần unit/integration test cho payment, approval, scheduled posting, AI generation, social publish, quota, permission.

### Ưu tiên trung bình

1. **Dynamic subscription plans**
   - Hiện plan là enum và quota/price hardcode.
   - Cần entity `SubscriptionPlan`, `PlanPrice`, `PlanFeature`, `PlanQuota`, admin CRUD và migration strategy.

2. **Content revision/versioning**
   - Chưa có `ContentRevision`, revision history, compare diff, restore revision.
   - Cần audit ai sửa nội dung, sửa lúc nào, lý do sửa.

3. **Prompt template system**
   - Prompt hiện build trong service, chưa có template entity/API/admin.
   - Cần version prompt, phân quyền sửa prompt, history và rollback.

4. **Analytics/reporting**
   - Dashboard còn basic, một số số liệu để 0 hoặc tính chưa đầy đủ.
   - Cần analytics theo profile/team/brand/content/campaign.
   - Cần cache, rate-limit strategy, data latency policy.

5. **Facebook Ads quota**
   - `AdQuotaService` có tồn tại, nhưng validate quota khi create campaign đang bị comment.
   - Cần enforce campaign quota và budget quota thật.

6. **Payment/subscription policy**
   - Cần làm rõ calendar month hay 30 days.
   - Cần proration khi đổi plan, refund/cancel policy, renewal/expiry job.
   - Cần xác thực webhook/signature chắc chắn trước production.

7. **Provider abstraction**
   - Social provider có `IProviderService`, nhưng AI provider chưa tách rõ thành `IAIProvider`.
   - Payment provider đang PayOS riêng, chưa có abstraction nếu muốn thêm Stripe/VNPay.

### Roadmap/future

1. **Instagram/TikTok/Twitter**
   - Cần provider, OAuth scopes, account discovery, publishing, media validation, insights mapping.

2. **AI video generation**
   - Cần video provider, async job queue, progress tracking, quota/cost tracking, storage, moderation.

3. **Sentiment analysis và trend prediction**
   - Chưa có service/controller/model.
   - Cần nguồn dữ liệu, API strategy, cache, cost control.

4. **AI strategy recommendation và real-time optimization**
   - Chưa có budget recommendation, best-time-to-post, campaign optimization.
   - Nếu triển khai cần manual approval và audit trail để tránh tự động thay đổi campaign ngoài kiểm soát.

5. **Budget auto-optimization**
   - Chưa có automation rules/suggestions.
   - Cần rule engine hoặc recommendation service, approval trước khi apply.

## Nhận xét theo 19 requirement questions trong `docs/reference/specification-answers.md`

| # | Requirement | Đánh giá lại theo source | Cần làm |
|---|---|---|---|
| 1 | Team Permission | Partial | Enforce single Leader, chuẩn hóa role/permission, test permission matrix. |
| 2 | Subscription Plans | Enum only | Thêm dynamic plan CRUD hoặc ít nhất config hóa quota/price. |
| 3 | Instagram | None/very low | Implement Instagram Business provider nếu là yêu cầu bắt buộc. |
| 4 | Background Job | Basic | Retry, failure state, monitoring, idempotency. |
| 5 | AI Video | None pipeline | Implement async video generation pipeline. |
| 6 | Budget Auto | None | Recommendation/automation rules, manual approval. |
| 7 | Provider Arch | Partial-good | Tách AI/payment provider abstraction rõ hơn. |
| 8 | Test Coverage | Very low | Thêm test projects và coverage cho luồng chính. |
| 9 | AI Quota | Weak/basic | Thêm usage counters và enforce quota trước generation. |
| 10 | Leader Approval | Basic | SLA, delegation, escalation, single Leader rule. |
| 11 | Prompting | Basic | Prompt templates, versioning, admin CRUD. |
| 12 | Content Library | Basic | Revision history, diff, restore revision, permissions. |
| 13 | Meta OAuth | Security issue | Encrypt tokens, refresh strategy, scope minimization, revoke handling. |
| 14 | Scheduled Posts | Basic | Retry, DLQ/failed status, monitoring, membership verification. |
| 15 | Ads Automation | Basic | Validate mapping UI -> Meta params, edit flow, quota enforcement. |
| 16 | Analytics | Basic | Reports by scope, caching, rate-limit handling. |
| 17 | Payment | Basic | Webhook verification, proration, refund/cancel/expiry policy. |
| 18 | Data Model | Good baseline | Bổ sung usage/revision/SLA/dynamic plan/token audit models. |
| 19 | Security/RBAC | Partial | Authorize missing controllers, audit log, token encryption, rate limiting. |

## Rủi ro chính nếu demo/production ngay

- Endpoint admin/storage/AI có thể bị gọi không đúng quyền nếu deploy public mà chưa harden.
- Quota subscription gần như chưa enforce thật, dễ vượt giới hạn plan.
- Scheduled posting thiếu retry/failed-state nên lỗi provider có thể lặp hoặc khó truy vết.
- Social token và payment webhook cần rà bảo mật kỹ.
- Không có test tự động cho các luồng rủi ro cao.
- Dashboard/analytics chưa phản ánh đầy đủ số liệu thật.

## Đề xuất thứ tự làm tiếp

Thu tu phase chinh thuc da duoc cap nhat trong `docs/archive/plans/backend-code-plan.md`:

```text
Phase 9  - Workspace Migration
Phase 10 - Admin Backend theo Workspace
Phase 11 - Facebook Ads Campaign MVP
Phase 12 - Test Hardening va Backend Release
```

Khong bat dau Facebook Ads truoc khi Workspace va Admin theo Workspace hoan thanh.

1. Sprint 0: khóa security ngay
   - Thêm `[Authorize]` cho controller/action thiếu.
   - Check admin role cho `AdminToolsController`.
   - Không nhận `UserId` từ body ở AI API, lấy từ JWT/profile context.
   - Verify webhook PayOS và che giấu token/secret trong log.

2. Sprint 1: quota và governance
   - Thêm usage counters.
   - Enforce quota ở content creation, AI generation, image generation, posting, social accounts, ads.
   - Enforce one Leader per team và permission matrix.

3. Sprint 2: reliability và audit
   - Retry policy cho scheduled posting.
   - Failed schedule status + admin monitoring.
   - Audit log cho các action quan trọng.
   - Idempotency cho publish/payment/schedule.

4. Sprint 3: content và approval nâng cao
   - Content revision history.
   - Prompt templates.
   - Approval SLA, delegation, escalation.

5. Sprint 4: analytics, tests, dynamic plans
   - Dashboard/report đủ số liệu.
   - Unit/integration tests.
   - Dynamic plan/pricing/quota CRUD nếu business yêu cầu.

6. Roadmap sau MVP
   - Instagram/TikTok.
   - AI video.
   - Sentiment/trend prediction.
   - Budget optimization và AI strategy recommendation.

## Ghi chú về độ tin cậy của đánh giá

Đánh giá này dựa trên rà soát static source code, file SRS và các dấu hiệu implementation/TODO trong repo. Chưa chạy build/test vì yêu cầu hiện tại là phân tích và tạo báo cáo, không phải xác minh runtime. Nếu cần số phần trăm chính xác hơn, bước tiếp theo nên là chạy API smoke test theo từng endpoint và đối chiếu database migration/runtime configuration.
