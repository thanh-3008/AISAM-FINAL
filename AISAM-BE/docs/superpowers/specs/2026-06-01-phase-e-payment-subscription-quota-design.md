# Phase E - Payment, Subscription, Quota Design

Last updated: 2026-06-01

## 1. Muc tieu

Phase E tuong ung Phase 8 trong `BACKEND_CODE_PLAN.md`.

Muc tieu cua phase nay la bo sung lop monetization/SaaS MVP sau khi codebase da co:

- content CRUD va AI flow o Phase B
- social integration va Facebook publishing o Phase C
- notification, scheduling va dashboard co ban o Phase D

Scope Phase E:

- them payment/subscription repositories va APIs MVP
- them PayOS checkout/callback/webhook flow voi fail-safe config handling
- them quota summary API theo active profile
- enforce quota toi thieu o hai luong ton tai nguyen nhat:
  - AI generation voi `PromptQuota`
  - publish now va scheduled publish voi `PostQuota`
- giu quota usage o dang derived usage, khong tao usage counter table rieng

Ngoai scope:

- dynamic subscription plan CRUD
- quota enforcement cho CRUD brand/product/content draft
- point system, rollover quota, top-up quota
- usage ledger/persisted counters
- refund/proration policy phuc tap
- payment provider abstraction nhieu cong thanh toan

## 2. Nguyen tac thiet ke

Phase E phai bam cac nguyen tac sau:

1. Repository chi lam persistence/query, khong chua quota policy.
2. `QuotaService` la nguon su that duy nhat cho quota policy, usage summary va enforcement.
3. Quota chi duoc consume sau khi tac vu thanh cong:
   - AI generation thanh cong moi tinh vao `PromptUsage`
   - publish thanh cong moi tinh vao `PostUsage`
4. Neu thieu PayOS config, checkout/payment intent APIs phai tra loi an toan, nhung khong duoc lam hong quota/current subscription/payment history APIs.
5. Quota enforcement trong Phase E phai duoc tai su dung giua HTTP flow va background flow; khong viet hai bo logic rieng cho publish now va scheduled publish.
6. Phase E uu tien derived usage de giam migration va giu scope MVP nho.

## 3. Kien truc tong quan

Phase E duoc chia thanh 3 cum:

- Payment/Subscription persistence va API module
- Quota summary/enforcement module
- Integration hooks vao AI va publishing flows

Luong tong quat:

1. User xem subscription/payment/quota theo active profile
2. User tao checkout request neu can nang cap/gia han plan
3. Payment callback/webhook cap nhat payment va subscription state
4. `QuotaService` doc active subscription, suy ra usage trong chu ky hien tai
5. AI generation va publish flows goi `QuotaService` truoc khi thuc thi
6. Neu quota con, flow tiep tuc; neu het quota, API tra `403` voi `errorCode` ro rang
7. Sau khi AI/publish thanh cong, usage tang len mot cach tu nhien vi derived usage se dem them record thanh cong moi

## 4. Pham vi module

### 4.1 Payment va Subscription

Can active:

- `IPaymentRepository` / `PaymentRepository`
- `ISubscriptionRepository` / `SubscriptionRepository`
- `IPaymentService` / `PayOSPaymentService`
- `PaymentController`
- DTO request/response cho checkout, payment history, current subscription

Behavior MVP:

- tao checkout/payment intent qua PayOS
- xu ly callback/webhook co ban
- doc lich su payment theo profile/user
- doc current subscription
- expose thong tin plan dang active va chu ky su dung

Chua lam:

- Stripe/VNPay
- dynamic plan CRUD
- refund workflow day du
- billing portal day du

### 4.2 Quota Summary

Can active:

- `IQuotaService` / `QuotaService`
- `QuotaController`
- DTO summary cho quota

Behavior MVP:

- tra `PromptQuotaLimit`, `PromptUsage`, `PromptRemaining`
- tra `PostQuotaLimit`, `PostUsage`, `PostRemaining`
- tra subscription window hien tai
- neu can, tra them thong tin plan name/status de frontend hien thi goi nang cap

### 4.3 Quota Enforcement

Can active:

- hook trong `AIService`
- hook trong `ContentService.PublishAsync`
- tai su dung enforcement trong `ScheduledPostingService` thong qua `ContentService.PublishAsync`

Behavior MVP:

- chan AI generation khi het `PromptQuota`
- chan publish now khi het `PostQuota`
- chan scheduled publish khi het `PostQuota`
- tra `403` voi error contract ro rang

Khong lam:

- quota cho draft content
- quota cho brand/product CRUD
- quota cho social account linking
- quota cho ads/create campaign

## 5. API de xuat

Tat ca API user-side duoi day can:

- `[Authorize]`
- `X-Profile-Id` neu route thuoc profile-scoped workflow
- active profile ownership

### 5.1 Payment APIs

```text
POST   /api/payment/checkout
POST   /api/payment/callback
POST   /api/payment/webhook
GET    /api/payment/history
GET    /api/payment/subscription/current
```

Neu source cu dang co route shape khac nho hon, implementation duoc phep bam sat route hien co, nhung phai bao toan 5 nang luc tren.

### 5.2 Quota API

```text
GET    /api/quota/profile/{profileId}
```

Neu codebase hien tai uu tien active profile header thay vi `profileId` route param, implementation co the doi ve route theo active profile, nhung spec yeu cau nang luc doc quota theo profile dang active.

## 6. Ownership va validation rules

### 6.1 Payment ownership

- user chi duoc doc payment history cua profile/user ma minh so huu
- current subscription phai duoc scope theo active profile
- webhook/callback khong dua vao JWT user de validate business state; no dua vao payment reference/order data

### 6.2 Quota ownership

- quota summary chi duoc doc cho active profile
- profile khac khong duoc xem quota cua nhau

### 6.3 Quota enforcement rules

#### Prompt quota

- `AIService` phai goi `QuotaService.EnsurePromptQuotaAsync(profileId)` truoc khi goi AI provider
- neu vuot quota:
  - HTTP `403`
  - `errorCode = PROMPT_QUOTA_EXCEEDED`
- neu AI provider fail hoac generation khong thanh cong, khong duoc consume quota

#### Post quota

- `ContentService.PublishAsync` phai goi `QuotaService.EnsurePostQuotaAsync(profileId)` truoc khi publish
- neu vuot quota:
  - HTTP `403`
  - `errorCode = POST_QUOTA_EXCEEDED`
- neu publish fail, khong duoc consume quota
- `ScheduledPostingService` khong viet enforcement rieng; no phai tai su dung `ContentService.PublishAsync`

### 6.4 Safe config handling

- neu thieu PayOS config:
  - checkout/payment intent APIs tra loi an toan va ro rang
  - callback/webhook co the reject request theo config state
  - current subscription, payment history, quota APIs van phai chay duoc neu khong can outbound call toi PayOS

## 7. Mo hinh du lieu va usage model

Phase E uu tien tai su dung entities va migrations hien co:

- `Subscription`
- `Payment`
- `AiGeneration`
- `Post`

### 7.1 Derived usage

Phase E su dung derived usage.

`QuotaService` tinh:

- `PromptUsage` tu so `AiGeneration` thanh cong trong chu ky subscription hien tai
- `PostUsage` tu so `Post` publish thanh cong trong chu ky subscription hien tai

Phase E khong tao:

- bang usage counter rieng
- usage ledger
- persisted counters

Ly do:

- giam migration
- giam blast radius
- giu MVP nho

Ghi chu post-MVP:

- neu sau nay can audit manh hon, xu ly concurrency cao, top-up quota hoac rollback usage, he thong co the nang cap sang `UsageLedger` hoac `PersistedCounters`

### 7.2 Subscription window

Quota phai duoc tinh trong chu ky subscription hien tai.

`QuotaService` phai:

1. tim active subscription cua profile
2. xac dinh `windowStart` va `windowEnd`
3. chi dem `AiGeneration` va `Post` thanh cong nam trong window do

Neu codebase hien tai co convention fallback plan `Free` khi profile chua co active subscription, implementation duoc phep tai su dung convention do, nhung plan phai duoc chot ro trong plan va tests.

### 7.3 Quota DTO

Quota summary DTO can it nhat co:

- `planName`
- `subscriptionStatus`
- `windowStart`
- `windowEnd`
- `promptQuotaLimit`
- `promptUsage`
- `promptRemaining`
- `postQuotaLimit`
- `postUsage`
- `postRemaining`

Neu frontend can UI nang cap goi, DTO co the bo sung:

- `isQuotaExceeded`
- `upgradeMessage`

## 8. Luong xu ly chinh

### 8.1 Tao checkout/payment intent

1. User gui request checkout cho plan/renew action hop le
2. `PayOSPaymentService` validate config
3. Neu thieu config:
   - tra loi an toan
   - khong ghi du lieu payment mo ho
4. Neu config hop le:
   - tao payment intent/checkout request
   - luu payment record phu hop
   - tra checkout response cho frontend

### 8.2 Callback/Webhook

1. Nhan callback/webhook tu PayOS
2. Validate signature/reference theo muc MVP
3. Tim payment record lien quan
4. Cap nhat payment status
5. Neu thanh toan thanh cong:
   - cap nhat hoac tao active subscription state
6. Tra response xac nhan

### 8.3 Doc quota summary

1. Lay active profile
2. `QuotaService` tim active subscription
3. Suy ra quota limit theo subscription plan hien tai
4. Dem derived usage trong subscription window
5. Tra quota summary DTO

### 8.4 AI generation

1. `AIService` lay `profileId`
2. Goi `QuotaService.EnsurePromptQuotaAsync(profileId)`
3. Neu het quota -> `403` + `PROMPT_QUOTA_EXCEEDED`
4. Neu con quota -> goi AI provider
5. Chi khi generation thanh cong, usage moi tang mot cach tu nhien do record thanh cong vua duoc tao

### 8.5 Publish now

1. `ContentService.PublishAsync` lay `profileId`
2. Goi `QuotaService.EnsurePostQuotaAsync(profileId)`
3. Neu het quota -> `403` + `POST_QUOTA_EXCEEDED`
4. Neu con quota -> thuc hien publish
5. Chi khi publish thanh cong va `Post` duoc persist thanh cong, usage moi tang

### 8.6 Scheduled publish

1. `ScheduledPostingService` quet schedule den han
2. Goi lai `ContentService.PublishAsync(contentId, integrationId, profileId)`
3. Publish flow duoc enforce quota boi cung mot logic nhu publish now
4. Neu het quota:
   - schedule fail theo business state phu hop
   - khong tao `Post` moi

## 9. Xu ly loi va error contract

### 9.1 API errors

Can giu pattern `GenericResponse<T>` hien tai.

Cases chinh:

- thieu PayOS config o checkout -> loi config an toan, khong throw mo ho
- resource ngoai profile -> `404`
- request invalid -> `400`
- vuot prompt quota -> `403` + `PROMPT_QUOTA_EXCEEDED`
- vuot post quota -> `403` + `POST_QUOTA_EXCEEDED`

### 9.2 Error contract de xuat

Ngoai `message`, response loi quota phai co:

- `errorCode`

Gia tri toi thieu:

- `PROMPT_QUOTA_EXCEEDED`
- `POST_QUOTA_EXCEEDED`

Frontend dung `errorCode` de:

- hien thi thong bao phu hop
- goi y nang cap goi

## 10. Testing strategy

Phase E can mo rong test theo 4 lop:

### 10.1 Repository tests

- doc active subscription dung
- doc payment history dung scope
- derived usage query dem dung `AiGeneration` thanh cong trong subscription window
- derived usage query dem dung `Post` publish thanh cong trong subscription window

### 10.2 Quota service tests

- con prompt quota -> pass
- het prompt quota -> `403` + `PROMPT_QUOTA_EXCEEDED`
- con post quota -> pass
- het post quota -> `403` + `POST_QUOTA_EXCEEDED`
- AI fail khong lam tang usage
- publish fail khong lam tang usage
- scheduled publish reuse dung publish enforcement

### 10.3 Payment/controller tests

- thieu PayOS config -> checkout tra loi an toan
- current subscription API van doc duoc khi PayOS config thieu
- quota API van doc duoc khi PayOS config thieu
- payment history API van doc duoc khi PayOS config thieu
- callback/webhook success/fail path co verification toi thieu

### 10.4 Verification

- `dotnet build AISAM.sln`
- `dotnet test AISAM.sln`
- Swagger smoke cho payment va quota APIs
- AI generate smoke khi con quota va het quota
- publish now smoke khi con quota va het quota
- scheduled publish smoke khi het quota

## 11. Rollout strategy

Phase E nen trien khai theo thu tu:

1. repository + DTO foundation
2. payment service
3. payment controller
4. quota service + quota controller
5. hook quota vao `AIService`
6. hook quota vao `ContentService.PublishAsync`
7. verify scheduled publish reuse logic
8. full verification + docs

Ly do:

- payment/subscription la nen doc policy cho quota
- quota service can co read model on dinh truoc khi chen enforcement vao flow dang chay
- chen enforcement vao publish flow sau cung de giam blast radius

## 12. Risks va blocker

### 12.1 Risks

- schema `Subscription`/`Payment` active co the chua khop 100% voi nhu cau callback/webhook runtime
- derived usage co the ton query hon persisted counter, nhung chap nhan duoc cho MVP
- fallback plan policy neu khong co active subscription can ro rang de tranh frontend va backend hieu khac nhau
- payment callback/webhook de mo sai co the gay lech state neu validation khong du chat

### 12.2 Blockers da biet

- local sandbox/credentials PayOS co the chua san sang
- local database migration history co the van can dong bo truoc khi khang dinh end-to-end payment flow

## 13. Definition of Done

Phase E duoc xem la hoan tat khi:

- Payment/subscription repositories hoat dong
- Checkout/payment intent APIs hoat dong hoac tra loi config PayOS ro rang
- Current subscription API hoat dong
- Payment history API hoat dong
- Quota summary API hoat dong
- AI generation bi chan dung khi het `PromptQuota`
- Publish now va scheduled publish bi chan dung khi het `PostQuota`
- Repository khong chua quota policy
- `QuotaService` la noi duy nhat chua quota policy
- Chi consume quota sau khi AI/publish thanh cong
- `dotnet build AISAM.sln` pass
- `dotnet test AISAM.sln` pass
- Swagger smoke pass
- Cac blocker external duoc ghi ro neu chua chay PayOS that
