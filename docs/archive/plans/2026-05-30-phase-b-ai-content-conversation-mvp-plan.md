# Phase B AI, Content, Conversation MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hoàn thiện Content CRUD, Gemini text generation và Conversation history theo active profile context đã xác thực.

**Architecture:** Phase B dùng header `X-Profile-Id` cho toàn bộ API Content/AI/Conversation. Middleware resolve active profile từ JWT user một lần mỗi request; service chỉ nhận profile ID đã validate. Gemini được tách thành `IGeminiTextClient` để orchestration service không phụ thuộc trực tiếp HTTP và có thể test bằng fake client.

**Tech Stack:** .NET 8, ASP.NET Core Web API, EF Core, PostgreSQL, JWT Bearer, `HttpClient`, xUnit.

---

## 0. Quy tắc thực thi

- Source cũ tại `docs/code-references/PRN232_Backend` là baseline để tham chiếu.
- Không copy nguyên `ContentService` hoặc `AIService` cũ vì chúng kéo Social, Approval, Team, Notification, Vertex và Supabase.
- Không tự ý commit. Sau mỗi task chỉ ghi checkpoint đề xuất để người dùng tự commit khi cần.
- Không sửa hoặc xóa file generated trong `bin/`, `obj/`.
- Không tạo migration nếu schema hiện tại đã đủ.
- Không chuyển task tiếp theo nếu task hiện tại chưa build/test pass.

## 1. File structure

### File tạo mới

| Nhóm | File | Trách nhiệm |
| --- | --- | --- |
| Profile context | `AISAM.API/Utils/UserClaimsHelper.cs` | Đọc JWT user ID |
| Profile context | `AISAM.API/Utils/ProfileContextHelper.cs` | Đọc active profile đã resolve |
| Profile context | `AISAM.API/Middleware/ActiveProfileMiddleware.cs` | Validate `X-Profile-Id` cho API Phase B |
| Content DTO | `AISAM.Common/Dtos/Request/CreateContentRequest.cs` | Request tạo content nội bộ |
| Content DTO | `AISAM.Common/Dtos/Request/UpdateContentRequest.cs` | Request update content nội bộ |
| Content DTO | `AISAM.Common/Dtos/Response/ContentResponseDto.cs` | Response content |
| Content repo | `AISAM.Repositories/IRepositories/IContentRepository.cs` | Contract persistence content |
| Content repo | `AISAM.Repositories/Repository/ContentRepository.cs` | EF Core persistence content |
| Content service | `AISAM.Services/IServices/IContentService.cs` | Contract Content MVP |
| Content service | `AISAM.Services/Service/ContentService.cs` | Ownership và lifecycle content |
| Content API | `AISAM.API/Controllers/ContentController.cs` | Content CRUD HTTP API |
| AI config/DTO | `AISAM.Common/Models/GeminiModels.cs` | Gemini settings và AI request/response |
| AI repo | `AISAM.Repositories/IRepositories/IAiGenerationRepository.cs` | Contract persistence generation |
| AI repo | `AISAM.Repositories/Repository/AiGenerationRepository.cs` | EF Core persistence generation |
| AI client | `AISAM.Services/IServices/IGeminiTextClient.cs` | Contract Gemini text-only |
| AI client | `AISAM.Services/Service/GeminiTextClient.cs` | HTTP client gọi Gemini |
| AI service | `AISAM.Services/IServices/IAIService.cs` | Contract orchestration AI |
| AI service | `AISAM.Services/Service/AIService.cs` | Generate/improve/approve/list/chat |
| AI API | `AISAM.API/Controllers/GeminiController.cs` | AI HTTP API |
| Conversation DTO | `AISAM.Common/Dtos/Response/ConversationResponseDto.cs` | Conversation list item |
| Conversation DTO | `AISAM.Common/Dtos/Response/ConversationDetailDto.cs` | Conversation detail và messages |
| Conversation repo | `AISAM.Repositories/IRepositories/IConversationRepository.cs` | Contract persistence conversation |
| Conversation repo | `AISAM.Repositories/Repository/ConversationRepository.cs` | EF Core persistence conversation |
| Conversation service | `AISAM.Services/IServices/IConversationService.cs` | Contract conversation history |
| Conversation service | `AISAM.Services/Service/ConversationService.cs` | List/detail/delete theo profile |
| Conversation API | `AISAM.API/Controllers/ConversationController.cs` | Conversation history HTTP API |
| Tests | `tests/AISAM.IntegrationTests/ActiveProfileMiddlewareTests.cs` | Test middleware context |
| Tests | `tests/AISAM.IntegrationTests/ContentServiceTests.cs` | Test lifecycle content |
| Tests | `tests/AISAM.IntegrationTests/AIServiceTests.cs` | Test Gemini orchestration |
| Tests | `tests/AISAM.IntegrationTests/ConversationServiceTests.cs` | Test conversation ownership |

### File sửa

| File | Nội dung |
| --- | --- |
| `AISAM.API/Program.cs` | Env override, options, DI, `HttpClient`, middleware |
| `AISAM.API/.env.example` | Gemini text config |
| `docs/superpowers/CODEBASE.md` | Ghi nhận module Phase B active sau khi verify |
| `docs/superpowers/CODEBASE_UPDATE.md` | Ghi kết quả Phase B sau khi verify |

## 2. Task map

| Task | Deliverable | Checkpoint bắt buộc |
| --- | --- | --- |
| B0 | Rà schema hiện có | Không tạo migration nếu schema đủ |
| B1 | Active profile context | Middleware tests + build pass |
| B2 | Content repository/service | Service tests + build pass |
| B3 | Content controller | Swagger path + smoke CRUD |
| B4 | Gemini config/client/generation | AI tests success/failure + build pass |
| B5 | Conversation persistence và AI chat controller | AI chat tests + missing-config smoke |
| B6 | Conversation history service/controller | Conversation ownership tests + Swagger path |
| B7 | Full verification và docs | Build/test/API smoke + migration note |

---

### Task B0: Rà schema Phase B trước khi code

**Files:**
- Read: `AISAM.Data/Model/Content.cs`
- Read: `AISAM.Data/Model/AiGeneration.cs`
- Read: `AISAM.Data/Model/Conversation.cs`
- Read: `AISAM.Data/Model/ChatMessage.cs`
- Read: `AISAM.Repositories/AISAMContext.cs`
- Read: `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs`

- [ ] **Step 1: Xác nhận entity và DbSet hiện có**

Run:

```powershell
rg "DbSet<.*(Content|AiGeneration|Conversation|ChatMessage)" AISAM.Repositories\AISAMContext.cs
rg "Entity\(\"AISAM.Data.Model.(Content|AiGeneration|Conversation|ChatMessage)\"" AISAM.Repositories\Migrations\AisamContextModelSnapshot.cs
```

Expected:

```text
DbSet<Content>
DbSet<AiGeneration>
DbSet<Conversation>
DbSet<ChatMessage>
```

- [ ] **Step 2: Đối chiếu cột bắt buộc**

Checklist:

```text
contents: id, profile_id, brand_id, product_id, ad_type, title, text_content,
          image_url, video_url, status, is_deleted, created_at, updated_at
ai_generations: id, content_id, ai_prompt, generated_text, generated_image_url,
                generated_video_url, status, error_message, is_deleted, created_at, updated_at
conversations: id, profile_id, brand_id, product_id, ad_type, title,
               is_active, is_deleted, created_at, updated_at
chat_messages: id, conversation_id, sender_type, message, ai_generation_id,
               content_id, is_deleted, created_at
```

Expected: schema hiện tại đủ; không tạo migration.

- [ ] **Step 3: Nếu schema đủ, ghi checkpoint**

Record:

```text
Database impact: none.
Migration: not required because existing schema already contains Phase B tables and columns.
```

Suggested manual commit checkpoint: không cần commit.

---

### Task B1: Thêm active profile context middleware

**Files:**
- Create: `AISAM.API/Utils/UserClaimsHelper.cs`
- Create: `AISAM.API/Utils/ProfileContextHelper.cs`
- Create: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/ActiveProfileMiddlewareTests.cs`

- [ ] **Step 1: Viết failing tests cho middleware**

Create `tests/AISAM.IntegrationTests/ActiveProfileMiddlewareTests.cs` với bốn case:

```csharp
[Fact]
public async Task InvokeAsync_ReturnsUnauthorized_WhenProfileHeaderIsMissing();

[Fact]
public async Task InvokeAsync_ReturnsUnauthorized_WhenProfileHeaderIsInvalid();

[Fact]
public async Task InvokeAsync_ReturnsForbidden_WhenProfileBelongsToAnotherUser();

[Fact]
public async Task InvokeAsync_StoresActiveProfile_WhenProfileBelongsToJwtUser();
```

Test setup:

```csharp
var context = new DefaultHttpContext();
context.Request.Path = "/api/content";
context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
}, "Test"));
context.Request.Headers["X-Profile-Id"] = profileId.ToString();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter ActiveProfileMiddlewareTests
```

Expected: FAIL vì middleware/helper chưa tồn tại.

- [ ] **Step 2: Tạo JWT helper**

Create `AISAM.API/Utils/UserClaimsHelper.cs`:

```csharp
using System.Security.Claims;

namespace AISAM.API.Utils;

public static class UserClaimsHelper
{
    public static Guid GetUserIdOrThrow(ClaimsPrincipal? user)
    {
        var rawId = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user?.FindFirstValue("sub");

        if (!Guid.TryParse(rawId, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user context.");
        }

        return userId;
    }
}
```

- [ ] **Step 3: Tạo active profile helper**

Create `AISAM.API/Utils/ProfileContextHelper.cs`:

```csharp
namespace AISAM.API.Utils;

public static class ProfileContextHelper
{
    public const string ActiveProfileItemKey = "ActiveProfileId";

    public static Guid GetActiveProfileIdOrThrow(HttpContext context)
    {
        if (context.Items.TryGetValue(ActiveProfileItemKey, out var value) &&
            value is Guid profileId)
        {
            return profileId;
        }

        throw new UnauthorizedAccessException("Invalid profile context.");
    }
}
```

- [ ] **Step 4: Tạo middleware validate profile**

Create `AISAM.API/Middleware/ActiveProfileMiddleware.cs`:

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Repositories.IRepositories;
using System.Net;

namespace AISAM.API.Middleware;

public sealed class ActiveProfileMiddleware
{
    private static readonly PathString[] ProtectedPrefixes =
    {
        new("/api/content"),
        new("/api/ai"),
        new("/api/conversations")
    };

    private readonly RequestDelegate _next;

    public ActiveProfileMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IProfileRepository profileRepository)
    {
        if (!ProtectedPrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix)))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "Authentication is required.");
            return;
        }

        if (!Guid.TryParse(context.Request.Headers["X-Profile-Id"], out var profileId))
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "Missing or invalid X-Profile-Id header.");
            return;
        }

        var userId = UserClaimsHelper.GetUserIdOrThrow(context.User);
        var profile = await profileRepository.GetByIdAsync(profileId, context.RequestAborted);
        if (profile == null)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "Profile not found.");
            return;
        }

        if (profile.UserId != userId)
        {
            await WriteErrorAsync(context, HttpStatusCode.Forbidden, "You are not allowed to use this profile.");
            return;
        }

        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profile.Id;
        await _next(context);
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(GenericResponse<object>.CreateError(message, status));
    }
}
```

- [ ] **Step 5: Đăng ký middleware đúng thứ tự**

Modify `AISAM.API/Program.cs`:

```csharp
app.UseAuthentication();
app.UseMiddleware<ActiveProfileMiddleware>();
app.UseAuthorization();
```

- [ ] **Step 6: Chạy tests và build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter ActiveProfileMiddlewareTests
dotnet build AISAM.sln
```

Expected: middleware tests pass; build `0 errors`.

Suggested manual commit checkpoint:

```text
feat(profile-context): validate active profile header for Phase B APIs
```

---

### Task B2: Thêm Content repository và service MVP

**Files:**
- Create: `AISAM.Common/Dtos/Request/CreateContentRequest.cs`
- Create: `AISAM.Common/Dtos/Request/UpdateContentRequest.cs`
- Create: `AISAM.Common/Dtos/Response/ContentResponseDto.cs`
- Create: `AISAM.Repositories/IRepositories/IContentRepository.cs`
- Create: `AISAM.Repositories/Repository/ContentRepository.cs`
- Create: `AISAM.Services/IServices/IContentService.cs`
- Create: `AISAM.Services/Service/ContentService.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/ContentServiceTests.cs`

- [ ] **Step 1: Viết failing service tests**

Create tests:

```csharp
[Fact]
public async Task CreateAsync_UsesActiveProfile_WhenBrandBelongsToProfile();

[Fact]
public async Task CreateAsync_ReturnsNotFound_WhenBrandBelongsToAnotherProfile();

[Fact]
public async Task CreateAsync_ReturnsBadRequest_WhenProductDoesNotBelongToBrand();

[Fact]
public async Task CloneAsync_CreatesNewDraft();

[Fact]
public async Task RestoreAsync_ResetsStatusToDraft();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter ContentServiceTests
```

Expected: FAIL vì `ContentService` chưa tồn tại.

- [ ] **Step 2: Tạo DTO content**

Create `CreateContentRequest` không chứa `ProfileId`, `PublishImmediately`, `IntegrationId`:

```csharp
public sealed class CreateContentRequest
{
    public Guid BrandId { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? StyleDescription { get; set; }
    public string? ContextDescription { get; set; }
    public string? RepresentativeCharacter { get; set; }
}
```

Create `UpdateContentRequest`:

```csharp
public sealed class UpdateContentRequest
{
    public Guid? ProductId { get; set; }
    public AdTypeEnum? AdType { get; set; }
    public string? Title { get; set; }
    public string? TextContent { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? StyleDescription { get; set; }
    public string? ContextDescription { get; set; }
    public string? RepresentativeCharacter { get; set; }
}
```

Create `ContentResponseDto` mapping entity fields:

```csharp
public sealed class ContentResponseDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? StyleDescription { get; set; }
    public string? ContextDescription { get; set; }
    public string? RepresentativeCharacter { get; set; }
    public ContentStatusEnum Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- [ ] **Step 3: Tạo repository contract**

Create `IContentRepository.cs`:

```csharp
public interface IContentRepository
{
    Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Content>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default);
    Task UpdateAsync(Content content, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Tạo EF repository**

Create `ContentRepository.cs` theo pattern `BrandRepository`:

```csharp
var query = _context.Contents
    .Include(content => content.Brand)
    .Include(content => content.Product)
    .Where(content => content.ProfileId == profileId);

if (brandId.HasValue)
{
    query = query.Where(content => content.BrandId == brandId.Value);
}

query = includeDeleted
    ? query
    : query.Where(content => !content.IsDeleted);
```

Phải hỗ trợ:

```text
search title/text_content
filter adType
filter status
sort title/createdAt/updatedAt
page >= 1
pageSize clamp 1..100
```

- [ ] **Step 5: Tạo service contract và implementation**

Create `IContentService.cs`:

```csharp
Task<GenericResponse<ContentResponseDto>> CreateAsync(Guid profileId, CreateContentRequest request, CancellationToken cancellationToken = default);
Task<GenericResponse<PagedResult<ContentResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
Task<GenericResponse<ContentResponseDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
Task<GenericResponse<ContentResponseDto>> UpdateAsync(Guid id, Guid profileId, UpdateContentRequest request, CancellationToken cancellationToken = default);
Task<GenericResponse<ContentResponseDto>> CloneAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
```

Critical validation block:

```csharp
private async Task<GenericResponse<bool>> ValidateBrandAndProductAsync(
    Guid profileId,
    Guid brandId,
    Guid? productId,
    CancellationToken cancellationToken)
{
    var brand = await _brandRepository.GetByIdAsync(brandId, cancellationToken);
    if (brand == null || brand.ProfileId != profileId)
    {
        return GenericResponse<bool>.CreateError("Brand not found.", HttpStatusCode.NotFound);
    }

    if (productId.HasValue)
    {
        var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
        if (product == null)
        {
            return GenericResponse<bool>.CreateError("Product not found.", HttpStatusCode.NotFound);
        }

        if (product.BrandId != brandId)
        {
            return GenericResponse<bool>.CreateError("Product does not belong to the selected brand.", HttpStatusCode.BadRequest);
        }
    }

    return GenericResponse<bool>.CreateSuccess(true);
}
```

Ownership block cho detail/update/delete/restore/clone:

```csharp
if (content == null || content.ProfileId != profileId)
{
    return GenericResponse<ContentResponseDto>.CreateError("Content not found.", HttpStatusCode.NotFound);
}
```

Restore:

```csharp
content.IsDeleted = false;
content.Status = ContentStatusEnum.Draft;
await _contentRepository.UpdateAsync(content, cancellationToken);
```

- [ ] **Step 6: Đăng ký DI**

Modify `Program.cs`:

```csharp
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<IContentService, ContentService>();
```

- [ ] **Step 7: Chạy tests và build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter ContentServiceTests
dotnet build AISAM.sln
```

Expected: Content tests pass; build `0 errors`.

Suggested manual commit checkpoint:

```text
feat(content): add profile-scoped content lifecycle service
```

---

### Task B3: Thêm Content controller và Swagger smoke

**Files:**
- Create: `AISAM.API/Controllers/ContentController.cs`
- Test: `tests/AISAM.IntegrationTests/ContentControllerTests.cs`

- [ ] **Step 1: Viết failing controller test**

Create tests:

```csharp
[Fact]
public async Task Create_ReturnsServiceStatusCode_WhenValidationFails();

[Fact]
public async Task GetPaged_UsesValidatedActiveProfileFromHttpContext();
```

- [ ] **Step 2: Tạo controller**

Controller outline:

```csharp
[ApiController]
[Route("api/content")]
[Authorize]
public sealed class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    private Guid GetProfileId() => ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
}
```

Mapping:

```text
POST   /api/content                 -> CreateAsync
GET    /api/content                 -> GetPagedAsync
GET    /api/content/{contentId}     -> GetByIdAsync
PUT    /api/content/{contentId}     -> UpdateAsync
POST   /api/content/{contentId}/clone   -> CloneAsync
DELETE /api/content/{contentId}     -> SoftDeleteAsync
POST   /api/content/{contentId}/restore -> RestoreAsync
```

Response pattern:

```csharp
var result = await _contentService.GetByIdAsync(contentId, GetProfileId(), cancellationToken);
return StatusCode(result.StatusCode, result);
```

- [ ] **Step 3: Chạy controller tests và build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter ContentControllerTests
dotnet build AISAM.sln
```

Expected: pass.

- [ ] **Step 4: Smoke Swagger path**

Run API:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:JWT_SECRET_KEY='PhaseBDevelopmentSecretKeyMustBeLongEnough12345'
$env:JWT_ISSUER='AISAM.API'
$env:JWT_AUDIENCE='AISAM.Client'
dotnet run --project AISAM.API\AISAM.API.csproj --urls http://localhost:5283
```

Check:

```powershell
$swagger = Invoke-WebRequest http://localhost:5283/swagger/v1/swagger.json -UseBasicParsing
$swagger.Content.Contains('/api/content')
```

Expected: `True`.

Suggested manual commit checkpoint:

```text
feat(content): expose content MVP endpoints
```

---

### Task B4: Thêm Gemini text client và AI generation service

**Files:**
- Create: `AISAM.Common/Models/GeminiModels.cs`
- Create: `AISAM.Repositories/IRepositories/IAiGenerationRepository.cs`
- Create: `AISAM.Repositories/Repository/AiGenerationRepository.cs`
- Create: `AISAM.Services/IServices/IGeminiTextClient.cs`
- Create: `AISAM.Services/Service/GeminiTextClient.cs`
- Create: `AISAM.Services/IServices/IAIService.cs`
- Create: `AISAM.Services/Service/AIService.cs`
- Modify: `AISAM.API/Program.cs`
- Modify: `AISAM.API/.env.example`
- Test: `tests/AISAM.IntegrationTests/AIServiceTests.cs`

- [ ] **Step 1: Viết failing AI tests**

Create tests:

```csharp
[Fact]
public async Task GenerateDraftAsync_ReturnsFailedGeneration_WhenGeminiConfigIsMissing();

[Fact]
public async Task GenerateDraftAsync_ReturnsCompletedGeneration_WhenGeminiReturnsText();

[Fact]
public async Task ApproveGenerationAsync_CopiesTextAndKeepsContentDraft();

[Fact]
public async Task GetGenerationsAsync_ReturnsNotFound_ForAnotherProfileContent();
```

Fake client:

```csharp
private sealed class FakeGeminiTextClient : IGeminiTextClient
{
    private readonly string? _response;
    private readonly Exception? _exception;

    public FakeGeminiTextClient(string response) => _response = response;
    public FakeGeminiTextClient(Exception exception) => _exception = exception;

    public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return _exception != null
            ? Task.FromException<string>(_exception)
            : Task.FromResult(_response!);
    }
}
```

- [ ] **Step 2: Tạo Gemini settings và request/response**

Create `GeminiModels.cs`:

```csharp
public sealed class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
    public int MaxTokens { get; set; } = 2048;
    public double Temperature { get; set; } = 0.7;
}

public sealed class CreateDraftRequest
{
    public Guid BrandId { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public string Prompt { get; set; } = string.Empty;
}

public sealed class ImproveContentRequest
{
    public string Prompt { get; set; } = string.Empty;
}

public sealed class AiGenerationResponse
{
    public Guid AiGenerationId { get; set; }
    public Guid ContentId { get; set; }
    public string? GeneratedText { get; set; }
    public AiStatusEnum Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

- [ ] **Step 3: Tạo generation repository**

Contract:

```csharp
Task<AiGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
Task<IEnumerable<AiGeneration>> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);
Task<AiGeneration> AddAsync(AiGeneration generation, CancellationToken cancellationToken = default);
Task UpdateAsync(AiGeneration generation, CancellationToken cancellationToken = default);
```

Implementation dùng EF Core, filter `!IsDeleted`, include `Content`.

- [ ] **Step 4: Tạo Gemini text client**

Create `IGeminiTextClient.cs`:

```csharp
public interface IGeminiTextClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
```

Create `GeminiTextClient.cs`:

```csharp
public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(_settings.ApiKey))
    {
        throw new InvalidOperationException("Gemini API key is not configured.");
    }

    var model = string.IsNullOrWhiteSpace(_settings.Model) ? "gemini-2.5-flash" : _settings.Model;
    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.ApiKey}";
    var requestBody = new
    {
        contents = new[]
        {
            new { parts = new[] { new { text = prompt } } }
        },
        generationConfig = new
        {
            maxOutputTokens = _settings.MaxTokens,
            temperature = _settings.Temperature
        }
    };

    var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException($"Gemini API returned {(int)response.StatusCode}.");
    }

    using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    var text = document.RootElement
        .GetProperty("candidates")[0]
        .GetProperty("content")
        .GetProperty("parts")[0]
        .GetProperty("text")
        .GetString();

    if (string.IsNullOrWhiteSpace(text))
    {
        throw new InvalidOperationException("Gemini API returned an empty response.");
    }

    return text.Trim();
}
```

Không log URL đầy đủ vì URL chứa API key.

- [ ] **Step 5: Tạo AI service contract**

```csharp
Task<GenericResponse<AiGenerationResponse>> GenerateDraftAsync(Guid profileId, CreateDraftRequest request, CancellationToken cancellationToken = default);
Task<GenericResponse<AiGenerationResponse>> ImproveAsync(Guid contentId, Guid profileId, ImproveContentRequest request, CancellationToken cancellationToken = default);
Task<GenericResponse<ContentResponseDto>> ApproveAsync(Guid generationId, Guid profileId, CancellationToken cancellationToken = default);
Task<GenericResponse<IEnumerable<AiGenerationResponse>>> GetGenerationsAsync(Guid contentId, Guid profileId, CancellationToken cancellationToken = default);
```

- [ ] **Step 6: Implement AI generation orchestration**

Constructor dependency tối thiểu:

```csharp
public AIService(
    IContentRepository contentRepository,
    IAiGenerationRepository generationRepository,
    IBrandRepository brandRepository,
    IProductRepository productRepository,
    IGeminiTextClient geminiTextClient)
{
    _contentRepository = contentRepository;
    _generationRepository = generationRepository;
    _brandRepository = brandRepository;
    _productRepository = productRepository;
    _geminiTextClient = geminiTextClient;
}
```

Validate ownership trước khi generate draft:

```csharp
var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
if (brand == null || brand.ProfileId != profileId)
{
    return GenericResponse<AiGenerationResponse>.CreateError("Brand not found.", HttpStatusCode.NotFound);
}

if (request.ProductId.HasValue)
{
    var product = await _productRepository.GetByIdAsync(request.ProductId.Value, cancellationToken);
    if (product == null || product.ProfileId != profileId)
    {
        return GenericResponse<AiGenerationResponse>.CreateError("Product not found.", HttpStatusCode.NotFound);
    }

    if (product.BrandId != brand.Id)
    {
        return GenericResponse<AiGenerationResponse>.CreateError("Product does not belong to the selected brand.", HttpStatusCode.BadRequest);
    }
}
```

Validate ownership trước khi improve hoặc lấy lịch sử generation:

```csharp
var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
if (content == null || content.ProfileId != profileId)
{
    return GenericResponse<AiGenerationResponse>.CreateError("Content not found.", HttpStatusCode.NotFound);
}
```

Critical generation block:

```csharp
private async Task<AiGenerationResponse> GenerateForContentAsync(
    Content content,
    string prompt,
    CancellationToken cancellationToken)
{
    var generation = await _generationRepository.AddAsync(new AiGeneration
    {
        ContentId = content.Id,
        AiPrompt = prompt,
        Status = AiStatusEnum.Pending
    }, cancellationToken);

    try
    {
        generation.GeneratedText = await _geminiTextClient.GenerateAsync(prompt, cancellationToken);
        generation.Status = AiStatusEnum.Completed;
    }
    catch (Exception ex)
    {
        generation.Status = AiStatusEnum.Failed;
        generation.ErrorMessage = ex.Message;
    }

    await _generationRepository.UpdateAsync(generation, cancellationToken);
    return MapGeneration(generation);
}
```

Approve block:

```csharp
if (generation == null || generation.Content.ProfileId != profileId)
{
    return GenericResponse<ContentResponseDto>.CreateError("AI generation not found.", HttpStatusCode.NotFound);
}

if (generation.Status != AiStatusEnum.Completed || string.IsNullOrWhiteSpace(generation.GeneratedText))
{
    return GenericResponse<ContentResponseDto>.CreateError("AI generation is not completed.", HttpStatusCode.BadRequest);
}

generation.Content.TextContent = generation.GeneratedText;
generation.Content.Status = ContentStatusEnum.Draft;
await _contentRepository.UpdateAsync(generation.Content, cancellationToken);
```

Rules bắt buộc cho public methods:

```text
GenerateDraftAsync:
- Brand phải tồn tại và Brand.ProfileId == profileId.
- Product optional; nếu có thì Product.BrandId == request.BrandId.
- Content mới luôn có ProfileId = profileId và Status = Draft.

ImproveAsync:
- Content phải tồn tại và Content.ProfileId == profileId.

ApproveAsync:
- Generation phải thuộc Content có Content.ProfileId == profileId.

GetGenerationsAsync:
- Content phải tồn tại và Content.ProfileId == profileId trước khi query generations.
```

- [ ] **Step 7: Thêm config và DI**

Modify `.env.example`:

```text
# Optional for API startup, required for AI text endpoints
GEMINI_API_KEY=
GEMINI_MODEL=gemini-2.5-flash
GEMINI_MAX_TOKENS=2048
GEMINI_TEMPERATURE=0.7
```

Modify `Program.cs`:

```csharp
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_API_KEY", "GeminiSettings:ApiKey");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_MODEL", "GeminiSettings:Model");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_MAX_TOKENS", "GeminiSettings:MaxTokens");
ApplyEnvironmentOverride(builder.Configuration, "GEMINI_TEMPERATURE", "GeminiSettings:Temperature");

builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.AddScoped<IAiGenerationRepository, AiGenerationRepository>();
builder.Services.AddHttpClient<IGeminiTextClient, GeminiTextClient>();
builder.Services.AddScoped<IAIService, AIService>();
```

- [ ] **Step 8: Chạy tests và build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter AIServiceTests
dotnet build AISAM.sln
```

Expected: AI tests pass; API startup không yêu cầu Gemini key.

Suggested manual commit checkpoint:

```text
feat(ai): add testable Gemini text generation service
```

---

### Task B5: Thêm Conversation persistence và AI chat controller

**Files:**
- Modify: `AISAM.Common/Models/GeminiModels.cs`
- Create: `AISAM.Repositories/IRepositories/IConversationRepository.cs`
- Create: `AISAM.Repositories/Repository/ConversationRepository.cs`
- Modify: `AISAM.Services/IServices/IAIService.cs`
- Modify: `AISAM.Services/Service/AIService.cs`
- Create: `AISAM.API/Controllers/GeminiController.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/AIControllerTests.cs`

- [ ] **Step 1: Bổ sung chat DTO**

```csharp
public sealed class ChatRequest
{
    public Guid? BrandId { get; set; }
    public Guid? ProductId { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
}

public sealed class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public Guid ConversationId { get; set; }
}
```

- [ ] **Step 2: Viết failing tests cho chat**

Add vào `AIServiceTests.cs`:

```csharp
[Fact]
public async Task ChatAsync_SavesUserAndAiMessages_WhenGeminiSucceeds();

[Fact]
public async Task ChatAsync_ReturnsClearErrorAndStoresAiErrorMessage_WhenGeminiFails();

[Fact]
public async Task ChatAsync_ReturnsNotFound_WhenBrandBelongsToAnotherProfile();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter AIServiceTests
```

Expected: FAIL vì chat orchestration và conversation repository chưa tồn tại.

- [ ] **Step 3: Tạo Conversation repository cho chat**

Create `IConversationRepository.cs`:

```csharp
Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
Task<PagedResult<Conversation>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
Task<Conversation?> GetActiveAsync(Guid profileId, Guid? brandId, Guid? productId, AdTypeEnum adType, CancellationToken cancellationToken = default);
Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
```

Create `ConversationRepository.cs`. Detail query:

```csharp
return await _context.Conversations
    .Include(conversation => conversation.ChatMessages.OrderBy(message => message.CreatedAt))
        .ThenInclude(message => message.AiGeneration)
    .Include(conversation => conversation.Brand)
    .Include(conversation => conversation.Product)
    .FirstOrDefaultAsync(conversation => conversation.Id == id && !conversation.IsDeleted, cancellationToken);
```

Paged query phải filter:

```csharp
conversation.ProfileId == profileId && !conversation.IsDeleted
```

- [ ] **Step 4: Bổ sung ChatAsync vào AI service contract**

Modify `IAIService.cs`:

```csharp
Task<GenericResponse<ChatResponse>> ChatAsync(Guid profileId, ChatRequest request, CancellationToken cancellationToken = default);
```

- [ ] **Step 5: Implement chat orchestration**

Mở rộng constructor của `AIService` để inject `IConversationRepository conversationRepository`.

Rules:

```text
thêm IConversationRepository vào constructor AIService
profileId lấy từ middleware
brand optional nhưng nếu có phải Brand.ProfileId == profileId
product optional nhưng nếu có phải Product.BrandId == brandId
conversationId optional nhưng nếu có phải Conversation.ProfileId == profileId
lưu user ChatMessage trước Gemini call
lưu AI ChatMessage khi success
không tự tạo Content từ chat trong Phase B
```

Critical block:

```csharp
await _conversationRepository.AddMessageAsync(new ChatMessage
{
    ConversationId = conversation.Id,
    SenderType = ChatSenderType.User,
    Message = request.Message
}, cancellationToken);

try
{
    var responseText = await _geminiTextClient.GenerateAsync(prompt, cancellationToken);

    await _conversationRepository.AddMessageAsync(new ChatMessage
    {
        ConversationId = conversation.Id,
        SenderType = ChatSenderType.AI,
        Message = responseText
    }, cancellationToken);

    return GenericResponse<ChatResponse>.CreateSuccess(new ChatResponse
    {
        ConversationId = conversation.Id,
        Response = responseText
    });
}
catch (Exception ex)
{
    const string errorMessage = "AI chat is temporarily unavailable.";
    await _conversationRepository.AddMessageAsync(new ChatMessage
    {
        ConversationId = conversation.Id,
        SenderType = ChatSenderType.AI,
        Message = errorMessage
    }, cancellationToken);

    return GenericResponse<ChatResponse>.CreateError($"{errorMessage} {ex.Message}", HttpStatusCode.ServiceUnavailable);
}
```

- [ ] **Step 6: Đăng ký Conversation repository**

Modify `Program.cs`:

```csharp
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
```

- [ ] **Step 7: Tạo AI controller**

```csharp
[ApiController]
[Route("api/ai")]
[Authorize]
public sealed class GeminiController : ControllerBase
{
    private Guid GetProfileId() => ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
}
```

Mapping:

```text
POST /api/ai/generate-draft
POST /api/ai/improve/{contentId}
POST /api/ai/approve/{aiGenerationId}
GET  /api/ai/generations/{contentId}
POST /api/ai/chat
```

- [ ] **Step 8: Test missing config và Swagger**

Run API không set `GEMINI_API_KEY`, sau đó gửi request generate draft hợp lệ.

Expected:

```text
API startup succeeds.
Response contains generation status Failed.
Error message states Gemini API key is not configured.
```

Swagger:

```powershell
$swagger = Invoke-WebRequest http://localhost:5283/swagger/v1/swagger.json -UseBasicParsing
$swagger.Content.Contains('/api/ai/generate-draft')
```

Expected: `True`.

Suggested manual commit checkpoint:

```text
feat(ai): expose Gemini text and chat endpoints
```

---

### Task B6: Thêm Conversation history API

**Files:**
- Create: `AISAM.Common/Dtos/Response/ConversationResponseDto.cs`
- Create: `AISAM.Common/Dtos/Response/ConversationDetailDto.cs`
- Create: `AISAM.Services/IServices/IConversationService.cs`
- Create: `AISAM.Services/Service/ConversationService.cs`
- Create: `AISAM.API/Controllers/ConversationController.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/ConversationServiceTests.cs`

- [ ] **Step 1: Viết failing ownership tests**

```csharp
[Fact]
public async Task GetByIdAsync_ReturnsNotFound_ForAnotherProfile();

[Fact]
public async Task DeleteAsync_DoesNotDeleteAnotherProfilesConversation();

[Fact]
public async Task GetPagedAsync_ReturnsOnlyActiveProfilesConversations();
```

- [ ] **Step 2: Tạo response DTO**

```csharp
public class ConversationResponseDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public AdTypeEnum AdType { get; set; }
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int MessageCount { get; set; }
}
```

Detail DTO thêm:

```csharp
public sealed class ConversationDetailDto : ConversationResponseDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public sealed class ChatMessageDto
{
    public Guid Id { get; set; }
    public ChatSenderType SenderType { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? AiGenerationId { get; set; }
    public Guid? ContentId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

- [ ] **Step 3: Tạo service contract và implementation**

```csharp
Task<GenericResponse<PagedResult<ConversationResponseDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
Task<GenericResponse<ConversationDetailDto>> GetByIdAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid profileId, CancellationToken cancellationToken = default);
```

Ownership block:

```csharp
if (conversation == null || conversation.ProfileId != profileId)
{
    return GenericResponse<ConversationDetailDto>.CreateError("Conversation not found.", HttpStatusCode.NotFound);
}
```

- [ ] **Step 4: Tạo controller**

```csharp
[ApiController]
[Route("api/conversations")]
[Authorize]
public sealed class ConversationController : ControllerBase
{
    private Guid GetProfileId() => ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
}
```

Mapping:

```text
GET    /api/conversations
GET    /api/conversations/{id}
DELETE /api/conversations/{id}
```

- [ ] **Step 5: Đăng ký DI**

```csharp
builder.Services.AddScoped<IConversationService, ConversationService>();
```

Lưu ý: `IConversationRepository` đã được đăng ký từ Task B5 để `AIService` resolve được.

- [ ] **Step 6: Chạy tests và Swagger smoke**

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter ConversationServiceTests
dotnet build AISAM.sln
```

Swagger:

```powershell
$swagger.Content.Contains('/api/conversations')
```

Expected: `True`.

Suggested manual commit checkpoint:

```text
feat(conversations): add profile-scoped AI chat history
```

---

### Task B7: Full verification, API smoke và docs

**Files:**
- Modify: `docs/superpowers/CODEBASE.md`
- Modify: `docs/superpowers/CODEBASE_UPDATE.md`

- [ ] **Step 1: Chạy full build và test**

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

- [ ] **Step 2: Chạy migration check**

```powershell
dotnet ef migrations list --project AISAM.Repositories --startup-project AISAM.API
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
```

Expected:

```text
No new Phase B migration.
Database is already up to date.
```

- [ ] **Step 3: Chạy Swagger và Health smoke**

```powershell
Invoke-WebRequest http://localhost:5283/swagger/v1/swagger.json -UseBasicParsing
Invoke-WebRequest http://localhost:5283/api/Health -UseBasicParsing
```

Expected: HTTP `200`.

- [ ] **Step 4: Chạy Content CRUD smoke bằng Swagger/Postman**

Headers:

```text
Authorization: Bearer <accessToken>
X-Profile-Id: <profileId>
```

Sequence:

```text
POST   /api/content
GET    /api/content
GET    /api/content/{contentId}
PUT    /api/content/{contentId}
POST   /api/content/{contentId}/clone
DELETE /api/content/{contentId}
POST   /api/content/{contentId}/restore
```

Expected: success response; restore trả content trạng thái `Draft`.

- [ ] **Step 5: Chạy AI missing-config smoke**

Không set `GEMINI_API_KEY`.

```text
POST /api/ai/generate-draft
```

Expected:

```text
API host vẫn chạy.
AiGeneration status = Failed.
ErrorMessage = Gemini API key is not configured.
```

- [ ] **Step 6: Chạy AI success smoke khi có API key hợp lệ**

Set:

```powershell
$env:GEMINI_API_KEY='<local-secret>'
```

Sequence:

```text
POST /api/ai/generate-draft
GET  /api/ai/generations/{contentId}
POST /api/ai/approve/{aiGenerationId}
POST /api/ai/chat
GET  /api/conversations
GET  /api/conversations/{conversationId}
DELETE /api/conversations/{conversationId}
```

Expected:

```text
Generation status = Completed.
Approve copies text into content and content remains Draft.
Chat stores user and AI messages.
Conversation detail returns message history.
```

- [ ] **Step 7: Cập nhật docs**

Update `CODEBASE.md`:

```text
Active modules: Health, Auth, Profile, Brand, Product, Content, AI text, Conversation.
Required Phase B header: X-Profile-Id for Content/AI/Conversation.
Optional startup config: Gemini.
Deferred: image generation, Supabase, social publish, approval, team.
```

Update `CODEBASE_UPDATE.md`:

```text
Phase B status.
Files copied/reused.
Files improved instead of copied verbatim.
Build/test results.
Swagger/API smoke results.
Migration check.
Known external config blocker if Gemini key is unavailable.
Rollback note.
```

Suggested manual commit checkpoint:

```text
docs: record Phase B verification results
```

---

## 3. Definition of Done checklist

- [ ] Schema Phase B đã được rà và ghi rõ migration impact.
- [ ] `X-Profile-Id` được validate với JWT user cho Content/AI/Conversation.
- [ ] Content CRUD/list/detail/clone/delete/restore pass.
- [ ] Content ownership và Brand/Product validation pass.
- [ ] Gemini thiếu config không làm API startup fail.
- [ ] Gemini success/failure đều có test.
- [ ] Approve generation copy text vào content và giữ `Draft`.
- [ ] AI chat lưu message user và AI.
- [ ] Conversation list/detail/delete không truy cập chéo profile.
- [ ] Không kéo Vertex, Supabase, Social, Approval, Team, Notification hoặc quota vào Phase B.
- [ ] `dotnet build AISAM.sln` pass.
- [ ] `dotnet test AISAM.sln` pass.
- [ ] Swagger, Health và API smoke pass.
- [ ] `CODEBASE.md` và `CODEBASE_UPDATE.md` được cập nhật.
- [ ] Không tự ý commit; người dùng quyết định thời điểm commit.
