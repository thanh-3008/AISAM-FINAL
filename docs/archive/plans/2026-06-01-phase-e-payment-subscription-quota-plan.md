# Phase E Payment, Subscription, Quota Implementation Plan

> Legacy phase notice: this active-profile plan predates Workspace Phase 9. Current policy is `docs/main/workspace-subscription-expiry-policy.md`: entitlement is workspace-based, Personal may fall back to Free, Business has no Free tier, and payment/renewal Credit grants must be idempotent.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hoan thien payment/subscription APIs MVP, quota summary API va basic quota enforcement cho AI generation va publish flows theo active profile.

**Architecture:** Phase E duoc chia thanh 3 cum noi mach: persistence/repository cho `Payment` va `Subscription`; service/controller cho PayOS checkout, callback/webhook, payment history va current subscription; va `QuotaService` la single source of truth cho quota summary va enforcement duoc tai su dung boi `AIService`, `ContentService.PublishAsync` va scheduled publish flow. Usage duoc tinh theo derived usage trong subscription window, khong tao usage counter table.

**Tech Stack:** .NET 8, ASP.NET Core Web API, EF Core, PostgreSQL, JWT Bearer, `HttpClient`, xUnit.

---

## 0. Quy tac thuc thi

- Khong tu y commit. Sau moi task chi de xuat checkpoint de nguoi dung tu commit neu can.
- Khong keo dynamic subscription plan CRUD, top-up quota, rollover quota, refund/proration phuc tap vao Phase E.
- Repository khong duoc chua quota policy.
- `QuotaService` la noi duy nhat chua quota policy va error code quota.
- Quota chi duoc consume sau khi AI/publish thanh cong.
- Thieu PayOS config chi duoc lam fail checkout/payment intent flow; khong duoc lam hong current subscription, payment history hay quota summary APIs.
- Scheduled publish phai tai su dung `ContentService.PublishAsync`, khong viet quota enforcement rieng trong scheduler.
- Khong sua/xoa file `bin/`, `obj/`.

## 1. File structure

### File tao moi

| Nhom | File | Trach nhiem |
| --- | --- | --- |
| DTO | `AISAM.Common/Models/PaymentDtos.cs` | DTO request/response cho checkout, history, current subscription |
| DTO | `AISAM.Common/Models/QuotaDtos.cs` | DTO summary cho prompt/post quota |
| Repository | `AISAM.Repositories/IRepositories/IPaymentRepository.cs` | Contract persistence payment |
| Repository | `AISAM.Repositories/Repository/PaymentRepository.cs` | EF repository payment history/detail lookup |
| Repository | `AISAM.Repositories/IRepositories/ISubscriptionRepository.cs` | Contract persistence subscription |
| Repository | `AISAM.Repositories/Repository/SubscriptionRepository.cs` | EF repository active subscription/window lookup |
| Service | `AISAM.Services/IServices/IPaymentService.cs` | Contract payment/subscription APIs |
| Service | `AISAM.Services/Service/PayOSPaymentService.cs` | Checkout/callback/webhook/history/current subscription |
| Service | `AISAM.Services/IServices/IQuotaService.cs` | Contract quota summary/enforcement |
| Service | `AISAM.Services/Service/QuotaService.cs` | Single source of truth cho quota policy va derived usage |
| Controller | `AISAM.API/Controllers/PaymentController.cs` | Payment/subscription MVP APIs |
| Controller | `AISAM.API/Controllers/QuotaController.cs` | Quota summary API |
| Test | `tests/AISAM.IntegrationTests/PaymentRepositoryTests.cs` | Payment/subscription repository tests |
| Test | `tests/AISAM.IntegrationTests/QuotaServiceTests.cs` | Quota summary/enforcement tests |
| Test | `tests/AISAM.IntegrationTests/PaymentControllerTests.cs` | Payment API tests |
| Test | `tests/AISAM.IntegrationTests/QuotaControllerTests.cs` | Quota API tests |
| Test | `tests/AISAM.IntegrationTests/PhaseEQuotaIntegrationTests.cs` | AI publish/scheduled publish enforcement tests |

### File sua

| File | Noi dung |
| --- | --- |
| `AISAM.API/Program.cs` | Dang ky repositories/services moi, `HttpClient`, PayOS config binding |
| `AISAM.API/.env.example` | Them PayOS config placeholders |
| `AISAM.API/appsettings.json` hoac `appsettings.Development.json` | Neu can bo sung section PayOS settings |
| `AISAM.Services/Service/AIService.cs` | Hook `EnsurePromptQuotaAsync` truoc generation |
| `AISAM.Services/Service/ContentService.cs` | Hook `EnsurePostQuotaAsync` truoc publish |
| `AISAM.Services/Service/ScheduledPostingService.cs` | Xac nhan tai su dung `ContentService.PublishAsync` va map fail state khi quota exceeded |
| `AISAM.Common/GenericResponse.cs` | Neu can bo sung `ErrorCode` ma khong vo contract hien tai |
| `AISAM.Repositories/AISAMContext.cs` | Mapping bo sung neu schema `Payment`/`Subscription` thieu field can thiet |
| `AISAM.Repositories/Migrations/*` | Chi tao migration additive neu schema thieu runtime fields cho PayOS/subscription |
| `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs` | Snapshot sau migration moi neu co |
| `docs/superpowers/CODEBASE.md` | Ghi lai module active sau Phase E |
| `docs/superpowers/CODEBASE_UPDATE.md` | Ghi lai execution va verification Phase E |

## 2. Task map

| Task | Deliverable | Checkpoint bat buoc |
| --- | --- | --- |
| E0 | Ra schema Payment/Subscription va route shape hien tai | Chot co/khong migration Phase E |
| E1 | DTO + Payment/Subscription repositories | Repository tests/build pass |
| E2 | PayOS payment service | Service tests/build pass |
| E3 | Payment controller APIs | Payment controller tests/Swagger smoke pass |
| E4 | Quota service + quota controller | Quota tests pass |
| E5 | Hook quota vao AIService | Prompt quota enforcement tests pass |
| E6 | Hook quota vao publish/scheduled publish | Post quota enforcement tests pass |
| E7 | Full verification va docs | Full build/test/docs cap nhat |

---

### Task E0: Ra schema payment/subscription va boundary Phase E

**Files:**
- Read: `AISAM.Data/Model/Payment.cs`
- Read: `AISAM.Data/Model/Subscription.cs`
- Read: `AISAM.Repositories/AISAMContext.cs`
- Read: `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs`
- Read: `AISAM.Services/Service/ScheduledPostingService.cs`
- Read: `AISAM.Services/Service/ContentService.cs`
- Read: `AISAM.Services/Service/AIService.cs`

- [ ] **Step 1: Doi chieu entity `Payment` va `Subscription` voi scope spec**

Run:

```powershell
Get-Content -Encoding utf8 'AISAM.Data\Model\Payment.cs'
Get-Content -Encoding utf8 'AISAM.Data\Model\Subscription.cs'
Get-Content -Encoding utf8 'AISAM.Repositories\AISAMContext.cs' | Select-String -Pattern 'Payment|Subscription' -Context 0,40
```

Expected:

```text
Xac dinh duoc field can cho payment history, current subscription, PayOS reference va subscription window.
```

- [ ] **Step 2: Xac nhan scheduler da tai su dung `ContentService.PublishAsync`**

Run:

```powershell
rg -n "PublishAsync\\(" AISAM.Services\Service\ScheduledPostingService.cs AISAM.Services\Service\ContentService.cs
```

Expected:

```text
ScheduledPostingService dang goi ContentService.PublishAsync thay vi publish logic rieng.
```

- [ ] **Step 3: Ghi checklist migration**

Checklist:

```text
Neu Payment/Subscription schema da du cho current subscription, payment history va PayOS reference thi khong tao migration.
Neu thieu field bat buoc cho callback/webhook runtime thi chi tao migration additive nho.
Khong tao usage table, usage ledger hoac persisted counters trong Phase E.
```

Suggested manual commit checkpoint:

```text
chore(payment): assess phase e schema requirements
```

---

### Task E1: Tao DTO va repositories cho Payment/Subscription

**Files:**
- Create: `AISAM.Common/Models/PaymentDtos.cs`
- Create: `AISAM.Common/Models/QuotaDtos.cs`
- Create: `AISAM.Repositories/IRepositories/IPaymentRepository.cs`
- Create: `AISAM.Repositories/Repository/PaymentRepository.cs`
- Create: `AISAM.Repositories/IRepositories/ISubscriptionRepository.cs`
- Create: `AISAM.Repositories/Repository/SubscriptionRepository.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/PaymentRepositoryTests.cs`

- [ ] **Step 1: Viet failing repository tests**

Create tests:

```csharp
[Fact]
public async Task GetCurrentActiveByProfileIdAsync_ReturnsOnlyCurrentProfilesActiveSubscription();

[Fact]
public async Task GetHistoryByProfileIdAsync_ReturnsPaymentsSortedNewestFirst();

[Fact]
public async Task CountSuccessfulPromptUsageAsync_CountsOnlyCompletedGenerationsInsideSubscriptionWindow();

[Fact]
public async Task CountSuccessfulPostUsageAsync_CountsOnlyPublishedPostsInsideSubscriptionWindow();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PaymentRepositoryTests
```

Expected: FAIL vi repository contracts/implementations chua ton tai.

- [ ] **Step 2: Tao DTO models**

Add `PaymentDtos.cs` voi it nhat:

```csharp
namespace AISAM.Common.Models;

public sealed class CreateCheckoutRequest
{
    public string PlanCode { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public sealed class PaymentHistoryItemDto
{
    public Guid Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CurrentSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
```

Add `QuotaDtos.cs` voi it nhat:

```csharp
namespace AISAM.Common.Models;

public sealed class QuotaSummaryDto
{
    public string PlanName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime WindowStart { get; set; }
    public DateTime? WindowEnd { get; set; }
    public int PromptQuotaLimit { get; set; }
    public int PromptUsage { get; set; }
    public int PromptRemaining { get; set; }
    public int PostQuotaLimit { get; set; }
    public int PostUsage { get; set; }
    public int PostRemaining { get; set; }
}
```

- [ ] **Step 3: Tao repository contracts**

Add `IPaymentRepository.cs`:

```csharp
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task<PagedResult<Payment>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
```

Add `ISubscriptionRepository.cs`:

```csharp
public interface ISubscriptionRepository
{
    Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default);
    Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Implement EF repositories**

`PaymentRepository` phai:

```csharp
var query = _context.Payments
    .Where(x => x.ProfileId == profileId)
    .OrderByDescending(x => x.CreatedAt);
```

`SubscriptionRepository.GetCurrentActiveByProfileIdAsync` phai:

```csharp
return await _context.Subscriptions
    .Where(x => x.ProfileId == profileId && !x.IsDeleted && x.Status == SubscriptionStatusEnum.Active)
    .OrderByDescending(x => x.StartDate)
    .FirstOrDefaultAsync(cancellationToken);
```

Derived usage queries phai loc:

```csharp
AiGeneration: Status == AiStatusEnum.Completed
Post: publish success/persisted post records
CreatedAt >= windowStart
windowEnd == null || CreatedAt <= windowEnd
```

- [ ] **Step 5: Dang ky DI va rerun tests**

Add vao `Program.cs`:

```csharp
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PaymentRepositoryTests
dotnet build AISAM.sln
```

Expected: repository tests PASS, build PASS.

Suggested manual commit checkpoint:

```text
feat(payment): add payment and subscription repositories
```

---

### Task E2: Implement PayOS payment service

**Files:**
- Create: `AISAM.Services/IServices/IPaymentService.cs`
- Create: `AISAM.Services/Service/PayOSPaymentService.cs`
- Modify: `AISAM.API/Program.cs`
- Modify: `AISAM.API/.env.example`
- Test: `tests/AISAM.IntegrationTests/PaymentServiceTests.cs`

- [ ] **Step 1: Viet failing payment service tests**

Create tests:

```csharp
[Fact]
public async Task CreateCheckoutAsync_ReturnsSafeError_WhenPayOsConfigMissing();

[Fact]
public async Task GetCurrentSubscriptionAsync_DoesNotRequirePayOsOutboundConfig();

[Fact]
public async Task GetPaymentHistoryAsync_ReturnsProfilesPaymentsWithoutPayOsConfig();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PaymentServiceTests
```

Expected: FAIL vi payment service chua ton tai.

- [ ] **Step 2: Tao payment service contract**

Add:

```csharp
public interface IPaymentService
{
    Task<GenericResponse<PayOSCheckoutResponse>> CreateCheckoutAsync(Guid profileId, CreateCheckoutRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> HandleCallbackAsync(IQueryCollection query, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> HandleWebhookAsync(string rawPayload, CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<CurrentSubscriptionDto>> GetCurrentSubscriptionAsync(Guid profileId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Implement safe config validation**

Trong `PayOSPaymentService`, tao helper:

```csharp
private bool HasPayOsConfig()
{
    return !string.IsNullOrWhiteSpace(_settings.ClientId)
        && !string.IsNullOrWhiteSpace(_settings.ApiKey)
        && !string.IsNullOrWhiteSpace(_settings.ChecksumKey);
}
```

`CreateCheckoutAsync` guard:

```csharp
if (!HasPayOsConfig())
{
    return GenericResponse<PayOSCheckoutResponse>.CreateError(
        "PayOS is not configured.",
        HttpStatusCode.ServiceUnavailable,
        "PAYOS_NOT_CONFIGURED");
}
```

- [ ] **Step 4: Implement read-only methods khong phu thuoc outbound PayOS**

`GetCurrentSubscriptionAsync` va `GetPaymentHistoryAsync` chi duoc doc DB:

```csharp
var subscription = await _subscriptionRepository.GetCurrentActiveByProfileIdAsync(profileId, cancellationToken);
...
var payments = await _paymentRepository.GetPagedByProfileIdAsync(profileId, request, cancellationToken);
```

Expected behavior:

```text
Thiếu PayOS config khong anh huong 2 API doc nay.
```

- [ ] **Step 5: Them config placeholders va DI**

Them vao `.env.example`:

```text
PAYOS_CLIENT_ID=
PAYOS_API_KEY=
PAYOS_CHECKSUM_KEY=
PAYOS_RETURN_URL=
PAYOS_CANCEL_URL=
```

Dang ky vao `Program.cs`:

```csharp
builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOSSettings"));
builder.Services.AddHttpClient<IPaymentService, PayOSPaymentService>();
```

- [ ] **Step 6: Rerun tests va build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PaymentServiceTests
dotnet build AISAM.sln
```

Expected: PASS.

Suggested manual commit checkpoint:

```text
feat(payment): add payos payment service with safe config handling
```

---

### Task E3: Expose payment and current subscription APIs

**Files:**
- Create: `AISAM.API/Controllers/PaymentController.cs`
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/PaymentControllerTests.cs`

- [ ] **Step 1: Viet failing payment controller tests**

Create tests:

```csharp
[Fact]
public async Task GetCurrentSubscription_ReturnsOnlyActiveProfilesSubscription();

[Fact]
public async Task GetPaymentHistory_ReturnsProfilesPayments();

[Fact]
public async Task CreateCheckout_ReturnsServiceUnavailable_WhenPayOsConfigMissing();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PaymentControllerTests
```

Expected: FAIL vi controller chua ton tai.

- [ ] **Step 2: Tao payment controller**

Routes:

```text
POST   /api/payment/checkout
POST   /api/payment/callback
POST   /api/payment/webhook
GET    /api/payment/history
GET    /api/payment/subscription/current
```

Controller pattern:

```csharp
[ApiController]
[Route("api/payment")]
[Authorize]
public sealed class PaymentController : ControllerBase
{
    [HttpGet("subscription/current")]
    public async Task<IActionResult> GetCurrentSubscription(CancellationToken cancellationToken)
    {
        var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
        var result = await _paymentService.GetCurrentSubscriptionAsync(profileId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
```

- [ ] **Step 3: Bao ve route va DI**

Them prefix vao `ActiveProfileMiddleware`:

```csharp
new("/api/payment"),
```

Dang ky:

```csharp
builder.Services.AddScoped<IPaymentService, PayOSPaymentService>();
```

- [ ] **Step 4: Chay tests va Swagger smoke**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PaymentControllerTests
dotnet build AISAM.sln
```

Swagger smoke:

```powershell
$env:ASPNETCORE_URLS='http://localhost:5283'
$env:ASPNETCORE_ENVIRONMENT='Development'
$p = Start-Process dotnet -ArgumentList @('bin\Debug\net8.0\AISAM.API.dll') -WorkingDirectory 'D:\final\AISAM-FINAL\AISAM-BE\AISAM.API' -WindowStyle Hidden -PassThru
Start-Sleep -Seconds 3
$swagger = Invoke-WebRequest 'http://localhost:5283/swagger/v1/swagger.json' -UseBasicParsing
$swagger.Content.Contains('/api/payment/checkout')
$swagger.Content.Contains('/api/payment/subscription/current')
Stop-Process -Id $p.Id
```

Expected:

```text
True
True
```

Suggested manual commit checkpoint:

```text
feat(payment): expose payment and current subscription apis
```

---

### Task E4: Implement QuotaService va QuotaController

**Files:**
- Create: `AISAM.Services/IServices/IQuotaService.cs`
- Create: `AISAM.Services/Service/QuotaService.cs`
- Create: `AISAM.API/Controllers/QuotaController.cs`
- Modify: `AISAM.Common/GenericResponse.cs`
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/QuotaServiceTests.cs`
- Test: `tests/AISAM.IntegrationTests/QuotaControllerTests.cs`

- [ ] **Step 1: Viet failing quota tests**

Create tests:

```csharp
[Fact]
public async Task GetSummaryAsync_ReturnsDerivedUsageInsideCurrentSubscriptionWindow();

[Fact]
public async Task EnsurePromptQuotaAsync_ReturnsForbiddenWithPromptErrorCode_WhenQuotaExceeded();

[Fact]
public async Task EnsurePostQuotaAsync_ReturnsForbiddenWithPostErrorCode_WhenQuotaExceeded();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "QuotaServiceTests|QuotaControllerTests"
```

Expected: FAIL vi quota service/controller chua ton tai.

- [ ] **Step 2: Bo sung error code support neu GenericResponse chua co**

Neu `GenericResponse<T>` chua ho tro `ErrorCode`, them property:

```csharp
public string? ErrorCode { get; set; }
```

va overload:

```csharp
public static GenericResponse<T> CreateError(string message, HttpStatusCode statusCode, string? errorCode = null)
{
    return new GenericResponse<T>
    {
        Success = false,
        Message = message,
        StatusCode = (int)statusCode,
        ErrorCode = errorCode
    };
}
```

- [ ] **Step 3: Tao quota service contract**

Add:

```csharp
public interface IQuotaService
{
    Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Implement QuotaService la single source of truth**

`GetSummaryAsync` shape:

```csharp
var subscription = await _subscriptionRepository.GetCurrentActiveByProfileIdAsync(profileId, cancellationToken);
if (subscription == null)
{
    return GenericResponse<QuotaSummaryDto>.CreateError("Active subscription not found.", HttpStatusCode.NotFound);
}

var windowStart = subscription.StartDate;
var windowEnd = subscription.EndDate;
var promptUsage = await _subscriptionRepository.CountSuccessfulPromptUsageAsync(profileId, windowStart, windowEnd, cancellationToken);
var postUsage = await _subscriptionRepository.CountSuccessfulPostUsageAsync(profileId, windowStart, windowEnd, cancellationToken);
```

`EnsurePromptQuotaAsync`:

```csharp
if (summary.Data.PromptRemaining <= 0)
{
    return GenericResponse<bool>.CreateError(
        "Prompt quota has been exceeded for the current subscription.",
        HttpStatusCode.Forbidden,
        "PROMPT_QUOTA_EXCEEDED");
}
```

`EnsurePostQuotaAsync`:

```csharp
if (summary.Data.PostRemaining <= 0)
{
    return GenericResponse<bool>.CreateError(
        "Post quota has been exceeded for the current subscription.",
        HttpStatusCode.Forbidden,
        "POST_QUOTA_EXCEEDED");
}
```

- [ ] **Step 5: Tao QuotaController**

Route:

```text
GET /api/quota/profile/{profileId}
```

Ownership guard:

```csharp
var activeProfileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
if (activeProfileId != profileId)
{
    return StatusCode(404, GenericResponse<QuotaSummaryDto>.CreateError("Profile not found.", HttpStatusCode.NotFound));
}
```

- [ ] **Step 6: Wire middleware/DI va rerun tests**

Them prefix:

```csharp
new("/api/quota"),
```

Dang ky:

```csharp
builder.Services.AddScoped<IQuotaService, QuotaService>();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "QuotaServiceTests|QuotaControllerTests"
dotnet build AISAM.sln
```

Expected: PASS.

Suggested manual commit checkpoint:

```text
feat(quota): add derived subscription quota summary and enforcement service
```

---

### Task E5: Hook prompt quota vao AIService

**Files:**
- Modify: `AISAM.Services/Service/AIService.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/PhaseEQuotaIntegrationTests.cs`

- [ ] **Step 1: Viet failing AI quota tests**

Create tests:

```csharp
[Fact]
public async Task GenerateDraftAsync_ReturnsForbiddenWithPromptQuotaError_WhenPromptQuotaExceeded();

[Fact]
public async Task GenerateDraftAsync_DoesNotIncreaseUsage_WhenGeminiFails();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PhaseEQuotaIntegrationTests
```

Expected: FAIL vi AIService chua enforce quota.

- [ ] **Step 2: Inject IQuotaService vao AIService**

Constructor bo sung:

```csharp
public AIService(
    IContentRepository contentRepository,
    IAiGenerationRepository generationRepository,
    IBrandRepository brandRepository,
    IProductRepository productRepository,
    IGeminiTextClient geminiTextClient,
    IQuotaService quotaService)
{
    _quotaService = quotaService;
    ...
}
```

- [ ] **Step 3: Enforce quota truoc generation**

Trong `GenerateDraftAsync` va flow generate tuong tu:

```csharp
var quotaCheck = await _quotaService.EnsurePromptQuotaAsync(profileId, cancellationToken);
if (!quotaCheck.Success)
{
    return GenericResponse<AiGenerationResponse>.CreateError(
        quotaCheck.Message,
        HttpStatusCode.Forbidden,
        quotaCheck.ErrorCode);
}
```

- [ ] **Step 4: Xac nhan khong consume quota khi AI fail**

Implementation note:

```text
Khong tao counter rieng.
Derived usage chi tang khi generation status = Completed duoc persist thanh cong.
Neu Gemini fail va generation status = Failed, query usage khong dem ban ghi nay.
```

- [ ] **Step 5: Rerun tests va build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PhaseEQuotaIntegrationTests
dotnet build AISAM.sln
```

Expected: prompt quota tests PASS.

Suggested manual commit checkpoint:

```text
feat(quota): enforce prompt quota in ai generation flow
```

---

### Task E6: Hook post quota vao publish va scheduled publish

**Files:**
- Modify: `AISAM.Services/Service/ContentService.cs`
- Modify: `AISAM.Services/Service/ScheduledPostingService.cs`
- Test: `tests/AISAM.IntegrationTests/PhaseEQuotaIntegrationTests.cs`

- [ ] **Step 1: Viet failing post quota tests**

Add tests:

```csharp
[Fact]
public async Task PublishAsync_ReturnsForbiddenWithPostQuotaError_WhenPostQuotaExceeded();

[Fact]
public async Task PublishAsync_DoesNotIncreaseUsage_WhenProviderPublishFails();

[Fact]
public async Task RunDueSchedulesAsync_MarksScheduleFailed_WhenPostQuotaExceeded();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PhaseEQuotaIntegrationTests
```

Expected: FAIL vi publish flow chua enforce quota.

- [ ] **Step 2: Inject IQuotaService vao ContentService**

Constructor bo sung:

```csharp
public ContentService(
    ...,
    IQuotaService quotaService)
{
    _quotaService = quotaService;
}
```

- [ ] **Step 3: Enforce post quota truoc publish**

Trong `PublishAsync`:

```csharp
var quotaCheck = await _quotaService.EnsurePostQuotaAsync(profileId, cancellationToken);
if (!quotaCheck.Success)
{
    return GenericResponse<ContentResponseDto>.CreateError(
        quotaCheck.Message,
        HttpStatusCode.Forbidden,
        quotaCheck.ErrorCode);
}
```

- [ ] **Step 4: Giu scheduler tai su dung PublishAsync**

Trong `ScheduledPostingService`, fail path phai nhan duoc message quota exceeded tu publish result:

```csharp
if (!publishResult.Success)
{
    schedule.Status = ScheduleStatusEnum.Failed;
    schedule.AttemptCount += 1;
    schedule.LastError = publishResult.Message;
    await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);
}
```

Expected:

```text
Khong viet EnsurePostQuotaAsync rieng trong ScheduledPostingService.
```

- [ ] **Step 5: Rerun tests va build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter PhaseEQuotaIntegrationTests
dotnet build AISAM.sln
```

Expected: post quota tests PASS.

Suggested manual commit checkpoint:

```text
feat(quota): enforce post quota in publish and scheduled publish flows
```

---

### Task E7: Full verification va docs

**Files:**
- Modify: `docs/superpowers/CODEBASE.md`
- Modify: `docs/superpowers/CODEBASE_UPDATE.md`

- [ ] **Step 1: Chay full build va test**

Run:

```powershell
dotnet build AISAM.sln
dotnet test AISAM.sln
```

Expected:

```text
Build succeeded.
0 errors.
All tests passed.
```

- [ ] **Step 2: Chay Swagger smoke**

Run:

```powershell
$env:ASPNETCORE_URLS='http://localhost:5283'
$env:ASPNETCORE_ENVIRONMENT='Development'
$p = Start-Process dotnet -ArgumentList @('bin\Debug\net8.0\AISAM.API.dll') -WorkingDirectory 'D:\final\AISAM-FINAL\AISAM-BE\AISAM.API' -WindowStyle Hidden -PassThru
Start-Sleep -Seconds 3
$swagger = Invoke-WebRequest 'http://localhost:5283/swagger/v1/swagger.json' -UseBasicParsing
$swagger.Content.Contains('/api/payment/checkout')
$swagger.Content.Contains('/api/payment/history')
$swagger.Content.Contains('/api/payment/subscription/current')
$swagger.Content.Contains('/api/quota/profile/{profileId}')
Stop-Process -Id $p.Id
```

Expected:

```text
True
True
True
True
```

- [ ] **Step 3: Chay runtime boundary smoke**

Cases:

```text
GET /api/payment/history khong JWT -> 401
GET /api/payment/subscription/current khong JWT -> 401
GET /api/quota/profile/{profileId} khong JWT -> 401
POST /api/payment/checkout thieu PayOS config -> 503 + PAYOS_NOT_CONFIGURED
```

Expected: host khong crash, read APIs van mount binh thuong.

- [ ] **Step 4: Chay quota behavior smoke**

Cases:

```text
AI generate khi prompt quota = 0 -> 403 + PROMPT_QUOTA_EXCEEDED
Publish now khi post quota = 0 -> 403 + POST_QUOTA_EXCEEDED
Scheduled publish khi post quota = 0 -> schedule Failed, khong tao Post moi
```

Expected: enforcement dung scope, khong anh huong CRUD khac.

- [ ] **Step 5: Cap nhat docs**

`docs/superpowers/CODEBASE.md` can duoc cap nhat:

```text
Active modules: Payment, Subscription summary, Quota summary.
QuotaService la single source of truth.
Derived usage tinh tu completed AI generations va published posts trong subscription window.
PayOS config chi bat buoc cho checkout/payment intent, khong bat buoc cho current subscription/payment history/quota summary.
```

`docs/superpowers/CODEBASE_UPDATE.md` can ghi:

```text
Phase E task execution record.
Migration note neu co.
Build/test/swagger/runtime smoke ket qua.
Known blocker external neu chua co PayOS sandbox credentials.
```

- [ ] **Step 6: Chot blocker note**

Neu local PayOS credentials chua co hoac DB local migration history van lech:

```text
Khong khang dinh real PayOS checkout/webhook da pass neu chua co sandbox credentials.
Khong khang dinh migration apply thanh cong neu DB local van lech migration history.
```

Suggested manual commit checkpoint:

```text
docs: record phase e payment and quota verification
```

---

## 3. Definition of Done checklist

- [ ] Payment/subscription repositories active
- [ ] PayOS payment service active voi safe config handling
- [ ] Payment controller APIs active
- [ ] QuotaService active va la single source of truth
- [ ] Quota summary API active
- [ ] AI generation bi chan dung khi het `PromptQuota`
- [ ] Publish now va scheduled publish bi chan dung khi het `PostQuota`
- [ ] Repository khong chua quota policy
- [ ] Chi consume quota sau khi AI/publish thanh cong
- [ ] `dotnet build AISAM.sln` pass
- [ ] `dotnet test AISAM.sln` pass
- [ ] Swagger smoke pass
- [ ] Runtime auth/config smoke pass
- [ ] Blocker external duoc ghi ro trong docs neu con ton tai
