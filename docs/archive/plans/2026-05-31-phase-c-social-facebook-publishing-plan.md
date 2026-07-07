# Phase C Social Integration va Facebook Page Publishing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hoan thien Facebook OAuth, Page integration, content publishing va post history theo active profile context, dong thoi cleanup quan he `Post` - `SocialIntegration` bi map du.

**Architecture:** Phase C bo sung provider layer (`IProviderService`, `FacebookProvider`, `GoogleProvider`) va social orchestration layer tach biet khoi controller. OAuth state duoc quan ly bang `IMemoryCache`, token social duoc ma hoa bang ASP.NET Core Data Protection, va publish flow chi cap nhat `Content` sang `Published` sau khi Facebook tra ket qua thanh cong. Posts API giu toi gian, chi doc theo active profile, khong keo notification, team permission hoac Facebook Ads.

**Tech Stack:** .NET 8, ASP.NET Core Web API, EF Core, PostgreSQL, JWT Bearer, Data Protection, `IMemoryCache`, `HttpClient`, xUnit.

---

## 0. Quy tac thuc thi

- Source cu tai `docs/code-references/PRN232_Backend` chi dung lam baseline tham chieu.
- Khong copy nguyen `SocialService`, `PostService` hoac controller cu vi chung keo Facebook Ads, scheduling, notification va team permission.
- Khong tu y commit. Sau moi task chi ghi checkpoint de nguoi dung tu commit neu can.
- Khong sua hoac xoa file generated trong `bin/`, `obj/`.
- OAuth Google khong duoc expose ra API public trong Phase C.
- Facebook Ads, `ad-accounts`, `link-ad-account`, YouTube publish va remote delete post la ngoai pham vi.
- Khong chuyen task tiep theo neu task hien tai chua build/test pass.

## 1. File structure

### File tao moi

| Nhom | File | Trach nhiem |
| --- | --- | --- |
| Config/provider | `AISAM.Common/Models/FacebookSettings.cs` | Facebook config va required permissions |
| Config/provider | `AISAM.Common/Models/FacebookModels.cs` | Request/response model cho Graph API |
| Config/provider | `AISAM.Common/Models/GoogleModels.cs` | Baseline model cho `GoogleProvider` noi bo |
| Provider | `AISAM.Services/IServices/IProviderService.cs` | Contract chung cho provider social |
| Provider | `AISAM.Services/Service/FacebookProvider.cs` | Facebook OAuth, list Page, publish Page |
| Provider | `AISAM.Services/Service/GoogleProvider.cs` | Provider noi bo de giu contract, khong expose public flow |
| OAuth state | `AISAM.Services/IServices/IOAuthStateStore.cs` | Contract luu/truy xuat OAuth state |
| OAuth state | `AISAM.Services/Service/MemoryOAuthStateStore.cs` | OAuth state store bang `IMemoryCache` |
| Token protection | `AISAM.Services/IServices/ISocialTokenProtector.cs` | Contract ma hoa/giai ma token social |
| Token protection | `AISAM.Services/Service/SocialTokenProtector.cs` | Data Protection wrapper cho token social |
| Social DTO | `AISAM.Common/Models/SocialDtos.cs` | DTO account, target, request link target, auth URL |
| Social DTO | `AISAM.Common/Models/PostDtos.cs` | DTO publish noi bo cho provider |
| Social DTO | `AISAM.Common/Dtos/Request/SocialCallbackRequest.cs` | Request callback OAuth |
| Social DTO | `AISAM.Common/Dtos/Response/SocialIntegrationDto.cs` | Response integration theo brand |
| Repository | `AISAM.Repositories/IRepositories/ISocialAccountRepository.cs` | Contract persistence social account |
| Repository | `AISAM.Repositories/Repository/SocialAccountRepository.cs` | EF Core repository social account |
| Repository | `AISAM.Repositories/IRepositories/ISocialIntegrationRepository.cs` | Contract persistence Page integration |
| Repository | `AISAM.Repositories/Repository/SocialIntegrationRepository.cs` | EF Core repository Page integration |
| Repository | `AISAM.Repositories/IRepositories/IPostRepository.cs` | Contract persistence post history |
| Repository | `AISAM.Repositories/Repository/PostRepository.cs` | EF Core repository post history |
| Service | `AISAM.Services/IServices/ISocialService.cs` | Contract social orchestration |
| Service | `AISAM.Services/Service/SocialService.cs` | Link account, list/link/unlink Page, brand ownership |
| Service | `AISAM.Services/IServices/IPostService.cs` | Contract read-only post history |
| Service | `AISAM.Services/Service/PostService.cs` | List/detail post theo active profile |
| Controller | `AISAM.API/Controllers/SocialAuthController.cs` | Facebook auth URL va callback API |
| Controller | `AISAM.API/Controllers/SocialAccountsController.cs` | Account/Page management API |
| Controller | `AISAM.API/Controllers/SocialIntegrationController.cs` | Integration by brand va unlink integration |
| Controller | `AISAM.API/Controllers/PostsController.cs` | Read-only posts API |
| DTO | `AISAM.Common/Models/PostListItemDto.cs` | DTO list/detail cho post history |
| Test | `tests/AISAM.IntegrationTests/OAuthStateStoreTests.cs` | Test state one-time va expiry |
| Test | `tests/AISAM.IntegrationTests/SocialTokenProtectorTests.cs` | Test encrypt/decrypt token |
| Test | `tests/AISAM.IntegrationTests/FacebookProviderTests.cs` | Test provider qua fake HTTP handler |
| Test | `tests/AISAM.IntegrationTests/SocialRepositoryTests.cs` | Test repository social soft delete |
| Test | `tests/AISAM.IntegrationTests/PostRepositoryTests.cs` | Test repository post history |
| Test | `tests/AISAM.IntegrationTests/SocialServiceTests.cs` | Test account/integration ownership |
| Test | `tests/AISAM.IntegrationTests/SocialControllerTests.cs` | Test auth/social endpoints |
| Test | `tests/AISAM.IntegrationTests/ContentServicePublishTests.cs` | Test publish orchestration |
| Test | `tests/AISAM.IntegrationTests/ContentControllerPublishTests.cs` | Test publish API |
| Test | `tests/AISAM.IntegrationTests/PostServiceTests.cs` | Test posts list/detail theo profile |
| Test | `tests/AISAM.IntegrationTests/PostsControllerTests.cs` | Test posts HTTP API |

### File sua

| File | Noi dung |
| --- | --- |
| `AISAM.API/Program.cs` | Facebook env overrides, options, DI, `IMemoryCache`, `HttpClient`, middleware routes |
| `AISAM.API/.env.example` | Facebook OAuth/Graph config |
| `AISAM.API/Middleware/ActiveProfileMiddleware.cs` | Bao ve them `/api/social-auth`, `/api/social`, `/api/posts` |
| `AISAM.API/Controllers/ContentController.cs` | Them endpoint `POST /api/content/{contentId}/publish/{integrationId}` |
| `AISAM.Services/IServices/IContentService.cs` | Them `PublishAsync` cho content |
| `AISAM.Services/Service/ContentService.cs` | Publish flow Facebook va luu `Post` |
| `AISAM.Repositories/AISAMContext.cs` | Sua quan he `Post` - `SocialIntegration` chi con mot FK dung |
| `AISAM.Repositories/Migrations/*_RemovePostSocialIntegrationShadowFk.cs` | Migration cleanup shadow FK |
| `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs` | Snapshot sau migration cleanup |
| `docs/superpowers/CODEBASE.md` | Ghi nhan module social/publish active sau khi verify |
| `docs/superpowers/CODEBASE_UPDATE.md` | Ghi ket qua trien khai va verify Phase C |

## 2. Task map

| Task | Deliverable | Checkpoint bat buoc |
| --- | --- | --- |
| C0 | Ra schema social hien tai va shadow FK | Chot migration cleanup can tao |
| C1 | Provider/config/state/token infrastructure | Tests state/protector/provider pass |
| C2 | Repository va migration cleanup | Build pass, repo tests pass |
| C3 | Social service MVP | Ownership tests pass |
| C4 | Social controllers va active profile routes | Swagger paths + auth smoke |
| C5 | Content publish Facebook | Publish tests pass, content status dung |
| C6 | Posts API chi doc | Post list/detail tests pass |
| C7 | Full verification va docs | Build/test/API smoke + blocker note |

---

### Task C0: Ra schema social va chot migration cleanup

**Files:**
- Read: `AISAM.Data/Model/SocialAccount.cs`
- Read: `AISAM.Data/Model/SocialIntegration.cs`
- Read: `AISAM.Data/Model/Post.cs`
- Modify: `AISAM.Repositories/AISAMContext.cs`
- Read: `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs`

- [ ] **Step 1: Xac nhan entity, DbSet va shadow FK hien tai**

Run:

```powershell
rg -n "DbSet<.*(SocialAccount|SocialIntegration|Post)" AISAM.Repositories\AISAMContext.cs
rg -n "SocialIntegrationId|IntegrationId|WithMany\(\"Posts\"\)|WithMany\(\)" AISAM.Repositories\Migrations\AisamContextModelSnapshot.cs AISAM.Repositories\AISAMContext.cs
```

Expected:

```text
DbSet<SocialAccount>
DbSet<SocialIntegration>
DbSet<Post>
Snapshot co dau vet cua SocialIntegrationId shadow FK ben canh integration_id
```

- [ ] **Step 2: Viet failing migration-shape test hoac checklist doi chieu**

Checklist:

```text
posts phai chi con FK duy nhat qua integration_id -> social_integrations.id
Khong con cot shadow SocialIntegrationId trong snapshot moi
Khong con HasOne(...).WithMany() map lap lai cho Post/Integration
```

Expected: checklist hien tai FAIL vi snapshot cu con shadow FK.

- [ ] **Step 3: Sua mapping trong DbContext de chi con mot quan he dung**

Modify block `Post` trong `AISAM.Repositories/AISAMContext.cs` thanh:

```csharp
modelBuilder.Entity<Post>(entity =>
{
    entity.HasKey(p => p.Id);
    entity.Property(p => p.Status).HasConversion<int>().HasDefaultValue(ContentStatusEnum.Published);
    entity.HasIndex(p => p.ContentId);
    entity.HasIndex(p => p.IntegrationId);
    entity.HasIndex(p => p.PublishedAt);
    entity.HasIndex(p => p.ExternalPostId);
    entity.HasOne(p => p.Content)
          .WithMany(c => c.Posts)
          .HasForeignKey(p => p.ContentId)
          .OnDelete(DeleteBehavior.Cascade);
    entity.HasOne(p => p.Integration)
          .WithMany(i => i.Posts)
          .HasForeignKey(p => p.IntegrationId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

- [ ] **Step 4: Tao migration cleanup**

Run:

```powershell
dotnet ef migrations add RemovePostSocialIntegrationShadowFk --project AISAM.Repositories --startup-project AISAM.API
```

Expected:

```text
Migration moi duoc tao duoi AISAM.Repositories\Migrations
```

Migration phai xoa:

```csharp
migrationBuilder.DropForeignKey(
    name: "FK_posts_social_integrations_SocialIntegrationId",
    table: "posts");

migrationBuilder.DropIndex(
    name: "IX_posts_SocialIntegrationId",
    table: "posts");

migrationBuilder.DropColumn(
    name: "SocialIntegrationId",
    table: "posts");
```

- [ ] **Step 5: Chay build de xac nhan schema shape**

Run:

```powershell
dotnet build AISAM.sln
```

Expected: build `0 errors`.

Suggested manual commit checkpoint:

```text
chore(db): remove duplicate post social integration shadow fk
```

---

### Task C1: Them provider, config, OAuth state va token protection infrastructure

**Files:**
- Create: `AISAM.Common/Models/FacebookSettings.cs`
- Create: `AISAM.Common/Models/FacebookModels.cs`
- Create: `AISAM.Common/Models/GoogleModels.cs`
- Create: `AISAM.Services/IServices/IProviderService.cs`
- Create: `AISAM.Services/Service/FacebookProvider.cs`
- Create: `AISAM.Services/Service/GoogleProvider.cs`
- Create: `AISAM.Services/IServices/IOAuthStateStore.cs`
- Create: `AISAM.Services/Service/MemoryOAuthStateStore.cs`
- Create: `AISAM.Services/IServices/ISocialTokenProtector.cs`
- Create: `AISAM.Services/Service/SocialTokenProtector.cs`
- Modify: `AISAM.API/Program.cs`
- Modify: `AISAM.API/.env.example`
- Test: `tests/AISAM.IntegrationTests/OAuthStateStoreTests.cs`
- Test: `tests/AISAM.IntegrationTests/SocialTokenProtectorTests.cs`
- Test: `tests/AISAM.IntegrationTests/FacebookProviderTests.cs`

- [ ] **Step 1: Viet failing tests cho OAuth state store**

Create tests:

```csharp
[Fact]
public async Task CreateAsync_ThenConsumeAsync_ReturnsStoredProfileAndProvider_OnceOnly();

[Fact]
public async Task ConsumeAsync_ReturnsNull_WhenStateExpired();

[Fact]
public async Task ConsumeAsync_ReturnsNull_WhenProfileIdDoesNotMatch();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter OAuthStateStoreTests
```

Expected: FAIL vi state store chua ton tai.

- [ ] **Step 2: Viet failing tests cho token protector**

Create tests:

```csharp
[Fact]
public void Protect_RoundTripsOriginalToken();

[Fact]
public void Protect_ReturnsCiphertextDifferentFromPlaintext();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter SocialTokenProtectorTests
```

Expected: FAIL vi protector chua ton tai.

- [ ] **Step 3: Viet failing provider tests qua fake HTTP handler**

Create tests:

```csharp
[Fact]
public async Task GetAuthUrlAsync_BuildsFacebookOAuthUrlWithConfiguredPermissions();

[Fact]
public async Task ExchangeCodeAsync_ReturnsSocialAccountDto_WhenFacebookReturnsTokenAndProfile();

[Fact]
public async Task GetTargetsAsync_ReturnsAvailablePages();

[Fact]
public async Task PublishAsync_TextPost_SucceedsAgainstFeedEndpoint();

[Fact]
public async Task PublishAsync_SingleImage_SucceedsAgainstPhotosEndpoint();

[Fact]
public async Task PublishAsync_MultiImage_UploadsUnpublishedMediaThenPublishesFeed();

[Fact]
public async Task PublishAsync_Video_SucceedsAgainstVideosEndpoint();

[Fact]
public async Task PublishAsync_RetriesWithFreshPageToken_WhenInitialPageTokenFails();

[Fact]
public async Task GetAuthUrlAsync_ThrowsClearError_WhenFacebookConfigMissing();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter FacebookProviderTests
```

Expected: FAIL vi provider/model chua ton tai.

- [ ] **Step 4: Tao settings va model cho Facebook/Google**

Create `AISAM.Common/Models/FacebookSettings.cs`:

```csharp
namespace AISAM.Common.Models;

public sealed class FacebookSettings
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string GraphApiVersion { get; set; } = "v23.0";
    public string BaseUrl { get; set; } = "https://graph.facebook.com";
    public string OAuthUrl { get; set; } = "https://www.facebook.com";
    public List<string> RequiredPermissions { get; set; } = new()
    {
        "pages_manage_posts",
        "pages_read_engagement",
        "pages_show_list"
    };
}
```

Create `AISAM.Common/Models/FacebookModels.cs` toi thieu:

```csharp
namespace AISAM.Common.Models;

public sealed class FacebookTokenResponse
{
    public string? AccessToken { get; set; }
    public int? ExpiresIn { get; set; }
}

public sealed class FacebookUserResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public sealed class FacebookPageResponse
{
    public List<FacebookPageData>? Data { get; set; }
}

public sealed class FacebookPageData
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? AccessToken { get; set; }
}

public sealed class FacebookPostResponse
{
    public string? Id { get; set; }
}

public sealed class FacebookErrorResponse
{
    public FacebookError? Error { get; set; }
}

public sealed class FacebookError
{
    public int Code { get; set; }
    public int? ErrorSubcode { get; set; }
    public string? Message { get; set; }
}
```

Create `AISAM.Common/Models/GoogleModels.cs` toi thieu de `GoogleProvider` compile:

```csharp
namespace AISAM.Common.Models;

public sealed class GoogleTokenResponse
{
    public string? access_token { get; set; }
    public int expires_in { get; set; }
    public string? refresh_token { get; set; }
}
```

- [ ] **Step 5: Tao provider contract va DTO noi bo**

Create `AISAM.Services/IServices/IProviderService.cs`:

```csharp
using AISAM.Common.Models;
using AISAM.Data.Model;

namespace AISAM.Services.IServices;

public interface IProviderService
{
    string ProviderName { get; }
    Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default);
    Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default);
}
```

Create `AISAM.Common/Models/SocialDtos.cs`:

```csharp
namespace AISAM.Common.Models;

public sealed class AuthUrlResponse
{
    public string AuthUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public sealed class SocialAccountDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SocialTargetDto> Targets { get; set; } = new();
}

public sealed class SocialTargetDto
{
    public Guid Id { get; set; }
    public string ProviderTargetId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}
```

Create `AISAM.Common/Models/PostDtos.cs`:

```csharp
namespace AISAM.Common.Models;

public sealed class PostDto
{
    public string Message { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<string>? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
}

public sealed class PublishResultDto
{
    public bool Success { get; set; }
    public string? ProviderPostId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PostedAt { get; set; }
    public string? RefreshedTargetAccessToken { get; set; }
}
```

- [ ] **Step 6: Implement OAuth state store va token protector**

Create `IOAuthStateStore.cs`:

```csharp
namespace AISAM.Services.IServices;

public interface IOAuthStateStore
{
    Task<string> CreateAsync(Guid profileId, string provider, CancellationToken cancellationToken = default);
    Task<OAuthStatePayload?> ConsumeAsync(string state, Guid profileId, string provider, CancellationToken cancellationToken = default);
}

public sealed class OAuthStatePayload
{
    public string State { get; init; } = string.Empty;
    public Guid ProfileId { get; init; }
    public string Provider { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
```

Create `MemoryOAuthStateStore.cs`:

```csharp
using Microsoft.Extensions.Caching.Memory;

namespace AISAM.Services.Service;

public sealed class MemoryOAuthStateStore : IOAuthStateStore
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);
    private readonly IMemoryCache _cache;

    public MemoryOAuthStateStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> CreateAsync(Guid profileId, string provider, CancellationToken cancellationToken = default)
    {
        var state = Guid.NewGuid().ToString("N");
        var payload = new OAuthStatePayload
        {
            State = state,
            ProfileId = profileId,
            Provider = provider,
            ExpiresAtUtc = DateTime.UtcNow.Add(Expiration)
        };

        _cache.Set(GetKey(state), payload, payload.ExpiresAtUtc);
        return Task.FromResult(state);
    }

    public Task<OAuthStatePayload?> ConsumeAsync(string state, Guid profileId, string provider, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetKey(state), out OAuthStatePayload? payload))
        {
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        _cache.Remove(GetKey(state));
        if (payload == null || payload.ExpiresAtUtc <= DateTime.UtcNow || payload.ProfileId != profileId || !string.Equals(payload.Provider, provider, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<OAuthStatePayload?>(null);
        }

        return Task.FromResult<OAuthStatePayload?>(payload);
    }

    private static string GetKey(string state) => $"oauth-state:{state}";
}
```

Create `ISocialTokenProtector.cs` and `SocialTokenProtector.cs`:

```csharp
namespace AISAM.Services.IServices;

public interface ISocialTokenProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}
```

```csharp
using Microsoft.AspNetCore.DataProtection;

namespace AISAM.Services.Service;

public sealed class SocialTokenProtector : ISocialTokenProtector
{
    private readonly IDataProtector _protector;

    public SocialTokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("AISAM.SocialTokens");
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);
    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}
```

- [ ] **Step 7: Implement FacebookProvider va GoogleProvider noi bo**

`FacebookProvider` phai co guard config:

```csharp
private void EnsureConfigured()
{
    if (string.IsNullOrWhiteSpace(_settings.AppId) ||
        string.IsNullOrWhiteSpace(_settings.AppSecret) ||
        string.IsNullOrWhiteSpace(_settings.RedirectUri))
    {
        throw new InvalidOperationException("Facebook integration is not configured.");
    }
}
```

`GetAuthUrlAsync`:

```csharp
public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
{
    EnsureConfigured();
    var permissions = string.Join(",", _settings.RequiredPermissions.Distinct());
    var authUrl = $"{_settings.OAuthUrl}/{_settings.GraphApiVersion}/dialog/oauth" +
                  $"?client_id={_settings.AppId}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&scope={Uri.EscapeDataString(permissions)}" +
                  $"&response_type=code" +
                  $"&state={Uri.EscapeDataString(state)}";
    return Task.FromResult(authUrl);
}
```

`GoogleProvider` chi can compile, khong expose flow public:

```csharp
public sealed class GoogleProvider : IProviderService
{
    public string ProviderName => "google";

    public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Google OAuth is not available in Phase C.");

    public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Google OAuth is not available in Phase C.");

    public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<AvailableTargetDto>>(Array.Empty<AvailableTargetDto>());

    public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default)
        => Task.FromResult(new Dictionary<string, string>());

    public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Google publishing is not available in Phase C.");
}
```

- [ ] **Step 8: Them env/config va DI**

Modify `AISAM.API/.env.example`:

```text
# Optional for API startup, required for Facebook OAuth and Page publishing
FACEBOOK_APP_ID=
FACEBOOK_APP_SECRET=
FACEBOOK_REDIRECT_URI=
FACEBOOK_GRAPH_API_VERSION=v23.0
FACEBOOK_BASE_URL=https://graph.facebook.com
FACEBOOK_OAUTH_URL=https://www.facebook.com
```

Modify `AISAM.API/Program.cs`:

```csharp
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_APP_ID", "FacebookSettings:AppId");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_APP_SECRET", "FacebookSettings:AppSecret");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_REDIRECT_URI", "FacebookSettings:RedirectUri");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_GRAPH_API_VERSION", "FacebookSettings:GraphApiVersion");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_BASE_URL", "FacebookSettings:BaseUrl");
ApplyEnvironmentOverride(builder.Configuration, "FACEBOOK_OAUTH_URL", "FacebookSettings:OAuthUrl");

builder.Services.Configure<FacebookSettings>(builder.Configuration.GetSection("FacebookSettings"));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IOAuthStateStore, MemoryOAuthStateStore>();
builder.Services.AddScoped<ISocialTokenProtector, SocialTokenProtector>();
builder.Services.AddHttpClient<FacebookProvider>();
builder.Services.AddHttpClient<GoogleProvider>();
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<FacebookProvider>());
builder.Services.AddScoped<IProviderService>(sp => sp.GetRequiredService<GoogleProvider>());
```

- [ ] **Step 9: Chay tests va build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "OAuthStateStoreTests|SocialTokenProtectorTests|FacebookProviderTests"
dotnet build AISAM.sln
```

Expected: tests pass; build pass.

Suggested manual commit checkpoint:

```text
feat(social): add provider, oauth state and token protection infrastructure
```

---

### Task C2: Them repositories social/post va migration cleanup shape

**Files:**
- Create: `AISAM.Repositories/IRepositories/ISocialAccountRepository.cs`
- Create: `AISAM.Repositories/Repository/SocialAccountRepository.cs`
- Create: `AISAM.Repositories/IRepositories/ISocialIntegrationRepository.cs`
- Create: `AISAM.Repositories/Repository/SocialIntegrationRepository.cs`
- Create: `AISAM.Repositories/IRepositories/IPostRepository.cs`
- Create: `AISAM.Repositories/Repository/PostRepository.cs`
- Create: `tests/AISAM.IntegrationTests/SocialRepositoryTests.cs`
- Create: `tests/AISAM.IntegrationTests/PostRepositoryTests.cs`
- Modify: `AISAM.Repositories/AISAMContext.cs`
- Modify: `AISAM.Repositories/Migrations/*_RemovePostSocialIntegrationShadowFk.cs`
- Modify: `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs`

- [ ] **Step 1: Viet failing repository tests cho soft delete va query active**

Create tests:

```csharp
[Fact]
public async Task GetByProfileIdAsync_ExcludesSoftDeletedAccountsAndIntegrations();

[Fact]
public async Task SoftDeleteAccountAsync_MarksAccountAndRelatedIntegrationsDeleted_ButKeepsPosts();

[Fact]
public async Task GetPagedAsync_ReturnsOnlyPostsForActiveProfile();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "SocialRepositoryTests|PostRepositoryTests"
```

Expected: FAIL vi repositories chua ton tai.

- [ ] **Step 2: Tao social account repository contract va implementation**

Create `ISocialAccountRepository.cs`:

```csharp
public interface ISocialAccountRepository
{
    Task<SocialAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SocialAccount?> GetByIdWithIntegrationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SocialAccount?> GetByProfileIdPlatformAndAccountIdAsync(Guid profileId, SocialPlatformEnum platform, string accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccount>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<SocialAccount> AddAsync(SocialAccount account, CancellationToken cancellationToken = default);
    Task UpdateAsync(SocialAccount account, CancellationToken cancellationToken = default);
}
```

Implementation query active:

```csharp
return await _context.SocialAccounts
    .Include(sa => sa.SocialIntegrations.Where(si => !si.IsDeleted))
    .Where(sa => sa.ProfileId == profileId && !sa.IsDeleted)
    .ToListAsync(cancellationToken);
```

- [ ] **Step 3: Tao social integration repository contract va implementation**

Create `ISocialIntegrationRepository.cs`:

```csharp
public interface ISocialIntegrationRepository
{
    Task<SocialIntegration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SocialIntegration?> GetByExternalIdAsync(Guid socialAccountId, string externalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegration>> GetBySocialAccountIdAsync(Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegration>> GetByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default);
    Task<SocialIntegration> AddAsync(SocialIntegration integration, CancellationToken cancellationToken = default);
    Task UpdateAsync(SocialIntegration integration, CancellationToken cancellationToken = default);
}
```

Implementation query active:

```csharp
return await _context.SocialIntegrations
    .Include(si => si.SocialAccount)
    .Include(si => si.Brand)
    .Where(si => si.BrandId == brandId && !si.IsDeleted)
    .ToListAsync(cancellationToken);
```

- [ ] **Step 4: Tao post repository contract va implementation**

Create `IPostRepository.cs`:

```csharp
public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default);
    Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
}
```

Implementation query:

```csharp
var query = _context.Posts
    .Include(p => p.Content)
        .ThenInclude(c => c.Brand)
    .Include(p => p.Integration)
    .Where(p => !p.IsDeleted && p.Content.ProfileId == profileId);

if (brandId.HasValue)
{
    query = query.Where(p => p.Content.BrandId == brandId.Value);
}

if (status.HasValue)
{
    query = query.Where(p => p.Status == status.Value);
}
```

- [ ] **Step 5: Dang ky repositories trong Program**

Modify `AISAM.API/Program.cs`:

```csharp
builder.Services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
builder.Services.AddScoped<ISocialIntegrationRepository, SocialIntegrationRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
```

- [ ] **Step 6: Chay tests va build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "SocialRepositoryTests|PostRepositoryTests"
dotnet build AISAM.sln
```

Expected: repository tests pass; build pass.

Suggested manual commit checkpoint:

```text
feat(social): add repositories and soft-delete aware persistence
```

---

### Task C3: Implement social service MVP

**Files:**
- Create: `AISAM.Common/Dtos/Request/SocialCallbackRequest.cs`
- Create: `AISAM.Common/Dtos/Response/SocialIntegrationDto.cs`
- Create: `AISAM.Services/IServices/ISocialService.cs`
- Create: `AISAM.Services/Service/SocialService.cs`
- Modify: `AISAM.Common/Models/SocialDtos.cs`
- Test: `tests/AISAM.IntegrationTests/SocialServiceTests.cs`

- [ ] **Step 1: Viet failing tests cho ownership va re-auth flow**

Create tests:

```csharp
[Fact]
public async Task GetAuthUrlAsync_CreatesStateForActiveProfile();

[Fact]
public async Task LinkAccountAsync_UpdatesExistingAccountToken_WhenFacebookAccountAlreadyLinked();

[Fact]
public async Task LinkSelectedTargetsForAccountAsync_ReturnsBadRequest_WhenBrandBelongsToAnotherProfile();

[Fact]
public async Task LinkSelectedTargetsForAccountAsync_CreatesIntegrationWithProtectedPageToken();

[Fact]
public async Task UnlinkAccountAsync_SoftDeletesAccountAndIntegrations();

[Fact]
public async Task UnlinkTargetAsync_SoftDeletesOnlyRequestedIntegration();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter SocialServiceTests
```

Expected: FAIL vi service/DTO chua ton tai.

- [ ] **Step 2: Bo sung request/response DTO cho social flow**

Create `SocialCallbackRequest.cs`:

```csharp
namespace AISAM.Common.Dtos.Request;

public sealed class SocialCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
```

Extend `SocialDtos.cs`:

```csharp
public sealed class LinkSelectedTargetsRequest
{
    public Guid BrandId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public List<string> ProviderTargetIds { get; set; } = new();
}

public sealed class AvailableTargetDto
{
    public string ProviderTargetId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "page";
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}
```

Create `SocialIntegrationDto.cs`:

```csharp
namespace AISAM.Common.Dtos.Response;

public sealed class SocialIntegrationDto
{
    public Guid Id { get; set; }
    public Guid SocialAccountId { get; set; }
    public Guid ProfileId { get; set; }
    public Guid BrandId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? BrandName { get; set; }
}
```

- [ ] **Step 3: Tao social service contract**

Create `ISocialService.cs`:

```csharp
public interface ISocialService
{
    Task<AuthUrlResponse> GetAuthUrlAsync(string provider, Guid profileId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> LinkAccountAsync(string provider, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialAccountDto>> GetProfileAccountsAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsForAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto> LinkSelectedTargetsForAccountAsync(Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<bool> UnlinkAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
    Task<bool> UnlinkTargetAsync(Guid profileId, Guid socialIntegrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandAsync(Guid profileId, Guid brandId, CancellationToken cancellationToken = default);
    Task<SocialAccountDto?> GetSocialAccountByIdAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Implement auth URL va callback one-time state**

Critical blocks trong `SocialService.cs`:

```csharp
public async Task<AuthUrlResponse> GetAuthUrlAsync(string provider, Guid profileId, CancellationToken cancellationToken = default)
{
    EnsureFacebookProvider(provider);
    var state = await _oauthStateStore.CreateAsync(profileId, provider, cancellationToken);
    var authUrl = await _facebookProvider.GetAuthUrlAsync(state, _facebookSettings.RedirectUri, cancellationToken);
    return new AuthUrlResponse { AuthUrl = authUrl, State = state };
}
```

```csharp
var statePayload = await _oauthStateStore.ConsumeAsync(request.State, profileId, provider, cancellationToken);
if (statePayload == null)
{
    throw new InvalidOperationException("OAuth state is invalid or expired.");
}
```

- [ ] **Step 5: Implement link account, re-auth va token protection**

Re-auth branch:

```csharp
var existing = await _socialAccountRepository.GetByProfileIdPlatformAndAccountIdAsync(profileId, platform, providerAccount.ProviderUserId, cancellationToken);
if (existing != null)
{
    existing.UserAccessToken = _tokenProtector.Protect(providerAccount.AccessToken);
    existing.ExpiresAt = providerAccount.ExpiresAt;
    existing.IsDeleted = false;
    existing.IsActive = true;
    existing.UpdatedAt = DateTime.UtcNow;
    await _socialAccountRepository.UpdateAsync(existing, cancellationToken);
    return MapAccount(existing);
}
```

Create branch:

```csharp
var entity = new SocialAccount
{
    ProfileId = profileId,
    Platform = platform,
    AccountId = providerAccount.ProviderUserId,
    UserAccessToken = _tokenProtector.Protect(providerAccount.AccessToken),
    ExpiresAt = providerAccount.ExpiresAt,
    IsActive = true,
    IsDeleted = false
};
```

- [ ] **Step 6: Implement list/link/unlink Page with strict ownership**

Brand ownership gate:

```csharp
var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
if (brand == null || brand.ProfileId != profileId)
{
    throw new ArgumentException("Brand not found.");
}
```

Account ownership gate:

```csharp
var account = await _socialAccountRepository.GetByIdWithIntegrationsAsync(socialAccountId, cancellationToken);
if (account == null || account.ProfileId != profileId || account.IsDeleted)
{
    throw new ArgumentException("Social account not found.");
}
```

Persist page token encrypted:

```csharp
var targetTokens = await _facebookProvider.GetTargetAccessTokensAsync(userAccessToken, request.ProviderTargetIds, cancellationToken);
var integration = new SocialIntegration
{
    ProfileId = profileId,
    BrandId = brand.Id,
    SocialAccountId = account.Id,
    Platform = SocialPlatformEnum.Facebook,
    ExternalId = target.ProviderTargetId,
    AccessToken = _tokenProtector.Protect(targetTokens[target.ProviderTargetId]),
    IsActive = true,
    IsDeleted = false
};
```

Soft delete unlink:

```csharp
account.IsDeleted = true;
account.IsActive = false;
foreach (var integration in account.SocialIntegrations.Where(i => !i.IsDeleted))
{
    integration.IsDeleted = true;
    integration.IsActive = false;
    integration.UpdatedAt = DateTime.UtcNow;
}
```

- [ ] **Step 7: Chay tests va build**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter SocialServiceTests
dotnet build AISAM.sln
```

Expected: ownership tests pass; build pass.

Suggested manual commit checkpoint:

```text
feat(social): add facebook account and page integration service
```

---

### Task C4: Expose social controllers va mo rong active profile middleware routes

**Files:**
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Create: `AISAM.API/Controllers/SocialAuthController.cs`
- Create: `AISAM.API/Controllers/SocialAccountsController.cs`
- Create: `AISAM.API/Controllers/SocialIntegrationController.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/SocialControllerTests.cs`

- [ ] **Step 1: Viet failing controller tests**

Create tests:

```csharp
[Fact]
public async Task GetFacebookAuthUrl_ReturnsUnauthorized_WhenProfileHeaderMissing();

[Fact]
public async Task Callback_ReturnsBadRequest_WhenStateInvalid();

[Fact]
public async Task GetAccounts_ReturnsOnlyActiveProfilesAccounts();

[Fact]
public async Task LinkTargets_ReturnsNotFound_WhenAccountBelongsToAnotherProfile();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter SocialControllerTests
```

Expected: FAIL vi controllers chua ton tai.

- [ ] **Step 2: Mo rong active profile middleware prefixes**

Modify `AISAM.API/Middleware/ActiveProfileMiddleware.cs`:

```csharp
private static readonly PathString[] ProtectedPrefixes =
{
    new("/api/content"),
    new("/api/ai"),
    new("/api/conversations"),
    new("/api/social-auth"),
    new("/api/social"),
    new("/api/posts")
};
```

- [ ] **Step 3: Tao `SocialAuthController`**

Create:

```csharp
[ApiController]
[Route("api/social-auth")]
[Authorize]
public sealed class SocialAuthController : ControllerBase
{
    private readonly ISocialService _socialService;

    public SocialAuthController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet("facebook")]
    public async Task<ActionResult<GenericResponse<AuthUrlResponse>>> GetFacebookAuthUrl(CancellationToken cancellationToken)
    {
        var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
        var result = await _socialService.GetAuthUrlAsync("facebook", profileId, cancellationToken);
        return Ok(GenericResponse<AuthUrlResponse>.CreateSuccess(result));
    }

    [HttpPost("facebook/callback")]
    public async Task<ActionResult<GenericResponse<SocialAccountDto>>> HandleFacebookCallback([FromBody] SocialCallbackRequest request, CancellationToken cancellationToken)
    {
        var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
        var result = await _socialService.LinkAccountAsync("facebook", profileId, request, cancellationToken);
        return Ok(GenericResponse<SocialAccountDto>.CreateSuccess(result));
    }
}
```

- [ ] **Step 4: Tao `SocialAccountsController` va `SocialIntegrationController`**

Routes:

```text
GET    /api/social/accounts/me
GET    /api/social/accounts/{socialAccountId}/available-targets
GET    /api/social/accounts/{socialAccountId}/linked-targets
POST   /api/social/accounts/{socialAccountId}/link-targets
DELETE /api/social/accounts/{socialAccountId}
DELETE /api/social/integrations/{socialIntegrationId}
GET    /api/social/integrations/brand/{brandId}
```

Critical ownership call:

```csharp
var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
var result = await _socialService.LinkSelectedTargetsForAccountAsync(profileId, socialAccountId, request, cancellationToken);
```

Provider gate:

```csharp
if (!string.Equals(request.Provider, "facebook", StringComparison.OrdinalIgnoreCase))
{
    return BadRequest(GenericResponse<object>.CreateError("Only Facebook is supported in Phase C."));
}
```

- [ ] **Step 5: Dang ky service DI**

Modify `AISAM.API/Program.cs`:

```csharp
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<IPostService, PostService>();
```

- [ ] **Step 6: Chay controller tests va Swagger smoke**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter SocialControllerTests
dotnet build AISAM.sln
```

Swagger checks:

```powershell
$swagger = Invoke-WebRequest http://localhost:5283/swagger/v1/swagger.json -UseBasicParsing
$swagger.Content.Contains('/api/social-auth/facebook')
$swagger.Content.Contains('/api/social/accounts/me')
$swagger.Content.Contains('/api/social/integrations/brand/{brandId}')
```

Expected:

```text
True
True
True
```

Suggested manual commit checkpoint:

```text
feat(social): expose facebook auth and page integration endpoints
```

---

### Task C5: Bat publish content len Facebook Page

**Files:**
- Modify: `AISAM.Services/IServices/IContentService.cs`
- Modify: `AISAM.Services/Service/ContentService.cs`
- Modify: `AISAM.API/Controllers/ContentController.cs`
- Modify: `AISAM.Common/Models/PostDtos.cs`
- Create: `tests/AISAM.IntegrationTests/ContentServicePublishTests.cs`
- Create: `tests/AISAM.IntegrationTests/ContentControllerPublishTests.cs`
- Test: `tests/AISAM.IntegrationTests/ContentServicePublishTests.cs`
- Test: `tests/AISAM.IntegrationTests/ContentControllerPublishTests.cs`

- [ ] **Step 1: Viet failing tests cho publish flow**

Create tests:

```csharp
[Fact]
public async Task PublishAsync_SetsContentPublishedAndCreatesPost_WhenFacebookReturnsSuccess();

[Fact]
public async Task PublishAsync_KeepsContentStatusUnchanged_WhenProviderFails();

[Fact]
public async Task PublishAsync_ReturnsBadRequest_WhenContentAlreadyPublished();

[Fact]
public async Task PublishAsync_ReturnsNotFound_WhenIntegrationBelongsToAnotherProfile();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "ContentServicePublishTests|ContentControllerPublishTests"
```

Expected: FAIL vi publish API chua ton tai.

- [ ] **Step 2: Mo rong content service contract**

Modify `AISAM.Services/IServices/IContentService.cs`:

```csharp
Task<GenericResponse<PublishResultDto>> PublishAsync(Guid contentId, Guid integrationId, Guid profileId, CancellationToken cancellationToken = default);
```

- [ ] **Step 3: Implement publish orchestration trong `ContentService`**

Them dependencies:

```csharp
private readonly ISocialIntegrationRepository _socialIntegrationRepository;
private readonly ISocialAccountRepository _socialAccountRepository;
private readonly IPostRepository _postRepository;
private readonly IEnumerable<IProviderService> _providers;
private readonly ISocialTokenProtector _tokenProtector;
```

Guard blocks:

```csharp
var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken);
if (content == null || content.ProfileId != profileId || content.IsDeleted)
{
    return GenericResponse<PublishResultDto>.CreateError("Content not found.", HttpStatusCode.NotFound);
}

if (content.Status == ContentStatusEnum.Published)
{
    return GenericResponse<PublishResultDto>.CreateError("Content has already been published.", HttpStatusCode.BadRequest);
}

var integration = await _socialIntegrationRepository.GetByIdAsync(integrationId, cancellationToken);
if (integration == null || integration.ProfileId != profileId || integration.IsDeleted || integration.BrandId != content.BrandId)
{
    return GenericResponse<PublishResultDto>.CreateError("Social integration not found.", HttpStatusCode.NotFound);
}
```

Build `PostDto`:

```csharp
var postDto = new PostDto { Message = content.TextContent };
if (content.AdType == AdTypeEnum.ImageText && !string.IsNullOrWhiteSpace(content.ImageUrl))
{
    var raw = content.ImageUrl.Trim();
    if (raw.StartsWith("["))
    {
        var urls = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
        var validUrls = urls.Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
        if (validUrls.Count == 1) postDto.ImageUrl = validUrls[0];
        else if (validUrls.Count > 1) postDto.ImageUrls = validUrls;
    }
    else
    {
        postDto.ImageUrl = content.ImageUrl;
    }
}
else if (content.AdType == AdTypeEnum.VideoText)
{
    postDto.VideoUrl = content.VideoUrl;
}
```

- [ ] **Step 4: Giai ma token, goi provider va persist ket qua**

Critical call:

```csharp
var provider = _providers.Single(p => p.ProviderName == integration.Platform.ToString().ToLowerInvariant());
var decryptedAccount = integration.SocialAccount;
decryptedAccount.UserAccessToken = _tokenProtector.Unprotect(decryptedAccount.UserAccessToken);
integration.AccessToken = _tokenProtector.Unprotect(integration.AccessToken);

var publishResult = await provider.PublishAsync(decryptedAccount, integration, postDto, cancellationToken);
if (!publishResult.Success)
{
    return GenericResponse<PublishResultDto>.CreateError(publishResult.ErrorMessage ?? "Publishing failed.", HttpStatusCode.BadGateway);
}

await _postRepository.AddAsync(new Post
{
    ContentId = content.Id,
    IntegrationId = integration.Id,
    ExternalPostId = publishResult.ProviderPostId,
    PublishedAt = publishResult.PostedAt ?? DateTime.UtcNow,
    Status = ContentStatusEnum.Published
}, cancellationToken);

content.Status = ContentStatusEnum.Published;
await _contentRepository.UpdateAsync(content, cancellationToken);
```

Neu provider tra `RefreshedTargetAccessToken`:

```csharp
if (!string.IsNullOrWhiteSpace(publishResult.RefreshedTargetAccessToken))
{
    integration.AccessToken = _tokenProtector.Protect(publishResult.RefreshedTargetAccessToken);
    await _socialIntegrationRepository.UpdateAsync(integration, cancellationToken);
}
```

- [ ] **Step 5: Expose publish endpoint trong `ContentController`**

Modify `AISAM.API/Controllers/ContentController.cs`:

```csharp
[HttpPost("{contentId:guid}/publish/{integrationId:guid}")]
public async Task<ActionResult<GenericResponse<PublishResultDto>>> Publish(
    Guid contentId,
    Guid integrationId,
    CancellationToken cancellationToken = default)
{
    var result = await _contentService.PublishAsync(contentId, integrationId, GetProfileId(), cancellationToken);
    return StatusCode(result.StatusCode, result);
}
```

- [ ] **Step 6: Chay tests va Swagger smoke**

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "ContentServicePublishTests|ContentControllerPublishTests"
dotnet build AISAM.sln
```

Swagger:

```powershell
$swagger.Content.Contains('/api/content/{contentId}/publish/{integrationId}')
```

Expected: `True`.

Suggested manual commit checkpoint:

```text
feat(content): enable facebook page publishing
```

---

### Task C6: Them Posts API chi doc theo active profile

**Files:**
- Create: `AISAM.Common/Models/PostListItemDto.cs`
- Create: `AISAM.Services/IServices/IPostService.cs`
- Create: `AISAM.Services/Service/PostService.cs`
- Create: `AISAM.API/Controllers/PostsController.cs`
- Create: `tests/AISAM.IntegrationTests/PostServiceTests.cs`
- Create: `tests/AISAM.IntegrationTests/PostsControllerTests.cs`
- Test: `tests/AISAM.IntegrationTests/PostServiceTests.cs`
- Test: `tests/AISAM.IntegrationTests/PostsControllerTests.cs`

- [ ] **Step 1: Viet failing tests cho list/detail post**

Create tests:

```csharp
[Fact]
public async Task GetPagedAsync_ReturnsOnlyPostsForActiveProfile();

[Fact]
public async Task GetByIdAsync_ReturnsNotFound_ForAnotherProfilesPost();

[Fact]
public async Task GetPagedAsync_AppliesOptionalBrandIdAndStatusFilters();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "PostServiceTests|PostsControllerTests"
```

Expected: FAIL vi post service/controller chua ton tai.

- [ ] **Step 2: Tao DTO va post service contract**

Create `AISAM.Common/Models/PostListItemDto.cs`:

```csharp
namespace AISAM.Common.Models;

public sealed class PostListItemDto
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public Guid IntegrationId { get; set; }
    public string? ExternalPostId { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ContentTitle { get; set; }
    public string? BrandName { get; set; }
}
```

Create `AISAM.Services/IServices/IPostService.cs`:

```csharp
public interface IPostService
{
    Task<GenericResponse<PagedResult<PostListItemDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<PostListItemDto>> GetByIdAsync(Guid profileId, Guid postId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Implement read-only post service**

Core query:

```csharp
var posts = await _postRepository.GetPagedByProfileIdAsync(profileId, request, brandId, status, cancellationToken);
return GenericResponse<PagedResult<PostListItemDto>>.CreateSuccess(new PagedResult<PostListItemDto>
{
    Data = posts.Data.Select(post => new PostListItemDto
    {
        Id = post.Id,
        ContentId = post.ContentId,
        IntegrationId = post.IntegrationId,
        ExternalPostId = post.ExternalPostId,
        PublishedAt = post.PublishedAt,
        Status = post.Status.ToString(),
        ContentTitle = post.Content.Title,
        BrandName = post.Content.Brand.Name
    }).ToList(),
    TotalCount = posts.TotalCount,
    Page = posts.Page,
    PageSize = posts.PageSize
});
```

Detail guard:

```csharp
var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
if (post == null || post.Content.ProfileId != profileId || post.IsDeleted)
{
    return GenericResponse<PostListItemDto>.CreateError("Post not found.", HttpStatusCode.NotFound);
}
```

- [ ] **Step 4: Expose `PostsController`**

Create:

```csharp
[ApiController]
[Route("api/posts")]
[Authorize]
public sealed class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<PostListItemDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? brandId = null,
        [FromQuery] ContentStatusEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
        var result = await _postService.GetPagedAsync(profileId, new PaginationRequest { Page = page, PageSize = pageSize }, brandId, status, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{postId:guid}")]
    public async Task<ActionResult<GenericResponse<PostListItemDto>>> GetById(Guid postId, CancellationToken cancellationToken = default)
    {
        var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
        var result = await _postService.GetByIdAsync(profileId, postId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
```

- [ ] **Step 5: Dang ky DI va chay tests**

Modify `AISAM.API/Program.cs`:

```csharp
builder.Services.AddScoped<IPostService, PostService>();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "PostServiceTests|PostsControllerTests"
dotnet build AISAM.sln
```

Expected: post tests pass; build pass.

Suggested manual commit checkpoint:

```text
feat(posts): add profile-scoped post history endpoints
```

---

### Task C7: Full verification va docs

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

- [ ] **Step 2: Verify migration cleanup**

Run:

```powershell
dotnet ef migrations list --project AISAM.Repositories --startup-project AISAM.API
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
```

Expected:

```text
Migration RemovePostSocialIntegrationShadowFk xuat hien trong list.
Database update thanh cong neu PostgreSQL san sang.
```

Neu DB khong san sang, ghi ro blocker ket noi thay vi khang dinh da apply.

- [ ] **Step 3: Chay Swagger smoke khi thieu Facebook credentials**

Start API khong set `FACEBOOK_APP_ID`/`FACEBOOK_APP_SECRET`.

Checks:

```powershell
$swagger = Invoke-WebRequest http://localhost:5283/swagger/v1/swagger.json -UseBasicParsing
$swagger.Content.Contains('/api/social-auth/facebook')
$swagger.Content.Contains('/api/posts')
```

Expected:

```text
Swagger HTTP 200
True
True
```

- [ ] **Step 4: Chay auth/config smoke cho social routes**

Cases:

```text
GET /api/social-auth/facebook khong JWT -> 401
GET /api/social-auth/facebook co JWT + X-Profile-Id nhung thieu config -> 503
GET /api/social/accounts/me khong JWT -> 401
GET /api/posts khong JWT -> 401
```

Expected: boundary auth va config ro rang, khong crash host.

- [ ] **Step 5: Chay publish smoke bang fake provider/test harness**

Sequence:

```text
tao content draft
tao social account/page integration test fixture
goi POST /api/content/{contentId}/publish/{integrationId}
verify post duoc tao va content = Published
```

Expected:

```text
Publish success path pass trong automated tests.
Publish fail path giu content status nguyen ven.
```

- [ ] **Step 6: Cap nhat docs**

Update `docs/superpowers/CODEBASE.md`:

```text
Active modules: Health, Auth, Profile, Brand, Product, Content, Gemini text, Conversation, Social/Facebook publish, Posts history.
Required header: X-Profile-Id for Content/AI/Conversation/Social/Posts.
External blockers: real Facebook OAuth/publish can App credentials va Page permissions.
```

Update `docs/superpowers/CODEBASE_UPDATE.md`:

```text
Phase C task execution record.
Files moi/chinh sua chinh.
Migration cleanup note.
Build/test/API smoke ket qua.
Blocker smoke test that do real Facebook OAuth/publish.
```

Suggested manual commit checkpoint:

```text
docs: record phase c social publishing verification
```

---

## 3. Definition of Done checklist

- [ ] Shadow FK `Post.SocialIntegrationId` da duoc xoa khoi mapping va migration snapshot.
- [ ] Migration cleanup duoc tao va verify.
- [ ] Facebook config, provider contract, OAuth state store va token protector hoat dong.
- [ ] OAuth state one-time, expiry 10 phut va khop active profile.
- [ ] Social token duoc ma hoa trong persistence va khong lo qua DTO/log.
- [ ] Facebook account link/re-auth pass.
- [ ] Facebook Page list/link/unlink pass voi ownership theo active profile va brand.
- [ ] Social controllers duoc protect boi `X-Profile-Id`.
- [ ] Publish text, single image, multi-image va video duoc test bang fake HTTP handler.
- [ ] Publish success tao `Post` va set `Content.Status = Published`.
- [ ] Publish fail khong tao du lieu thanh cong gia va khong doi status content sai.
- [ ] Posts API chi doc theo active profile hoat dong voi pagination va filter `brandId`/`status`.
- [ ] Google provider chi ton tai de giu contract, khong expose OAuth public.
- [ ] Facebook Ads, scheduling, notification, team permission va remote delete post khong bi keo vao Phase C.
- [ ] `dotnet build AISAM.sln` pass.
- [ ] `dotnet test AISAM.sln` pass.
- [ ] Swagger smoke va auth/config smoke pass.
- [ ] `CODEBASE.md` va `CODEBASE_UPDATE.md` duoc cap nhat.
- [ ] Khong tu y commit; nguoi dung quyet dinh thoi diem commit.
