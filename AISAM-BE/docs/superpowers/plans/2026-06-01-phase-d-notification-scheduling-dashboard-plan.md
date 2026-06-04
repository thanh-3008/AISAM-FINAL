# Phase D Notification, Scheduling, Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hoan thien notification noi bo, scheduling dang bai mot lan, background worker publish that qua `ContentService.PublishAsync`, dashboard summary MVP va dev-only scheduler trigger theo active profile.

**Architecture:** Phase D duoc chia thanh 4 cum doc lap nhung noi mach: persistence/repository cho `Notification`, `ContentCalendar`, `PerformanceReport`; service/controller cho notification va schedule CRUD; dashboard service tong hop read-only; va scheduler execution layer gom worker + dev trigger. Toan bo publish theo lich tai su dung publish flow cua Phase C thay vi nhan doi logic dang bai.

**Tech Stack:** .NET 8, ASP.NET Core Web API, EF Core, PostgreSQL, JWT Bearer, `BackgroundService`, `HttpClient`, xUnit.

---

## 0. Quy tac thuc thi

- Khong tu y commit. Sau moi task chi de xuat checkpoint de nguoi dung tu commit neu can.
- Khong keo repeat scheduling, realtime notification, email event notification, Ads metrics hoac dashboard nang cao vao Phase D.
- Background worker phai fail safe: bat exception, log lai, tiep tuc host.
- Publish theo lich phai goi `IContentService.PublishAsync`, khong viet publish logic moi.
- Moi API moi phai dung `ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext)` va duoc bao ve boi `ActiveProfileMiddleware`.
- Khong sua/xoa file `bin/`, `obj/`.

## 1. File structure

### File tao moi

| Nhom | File | Trach nhiem |
| --- | --- | --- |
| DTO | `AISAM.Common/Models/NotificationDtos.cs` | DTO notification list/detail/unread count |
| DTO | `AISAM.Common/Models/ScheduleDtos.cs` | DTO request/response cho content schedules |
| DTO | `AISAM.Common/Models/DashboardSummaryDto.cs` | DTO summary dashboard MVP |
| DTO | `AISAM.Common/Models/SchedulerRunResultDto.cs` | DTO ket qua manual trigger scheduler |
| Repository | `AISAM.Repositories/IRepositories/INotificationRepository.cs` | Contract persistence notification |
| Repository | `AISAM.Repositories/Repository/NotificationRepository.cs` | EF repository notification |
| Repository | `AISAM.Repositories/IRepositories/IContentCalendarRepository.cs` | Contract persistence schedule |
| Repository | `AISAM.Repositories/Repository/ContentCalendarRepository.cs` | EF repository schedule |
| Repository | `AISAM.Repositories/IRepositories/IPerformanceReportRepository.cs` | Contract performance report read support |
| Repository | `AISAM.Repositories/Repository/PerformanceReportRepository.cs` | EF repository performance report |
| Service | `AISAM.Services/IServices/INotificationService.cs` | Contract notification APIs |
| Service | `AISAM.Services/Service/NotificationService.cs` | List/detail/mark-read/mark-all/unread-count |
| Service | `AISAM.Services/IServices/IContentScheduleService.cs` | Contract CRUD/list/upcoming schedules |
| Service | `AISAM.Services/Service/ContentScheduleService.cs` | Ownership/validation cho schedules |
| Service | `AISAM.Services/IServices/IDashboardService.cs` | Contract dashboard summary |
| Service | `AISAM.Services/Service/DashboardService.cs` | Tong hop content/post/schedule/social/notification counts |
| Service | `AISAM.Services/IServices/IScheduledPostingService.cs` | Contract scan due schedules va trigger publish |
| Service | `AISAM.Services/Service/ScheduledPostingService.cs` | Execute due schedules va tao notifications |
| Service | `AISAM.Services/Service/ScheduledPostingBackgroundService.cs` | Hosted worker scan due schedules theo interval |
| Controller | `AISAM.API/Controllers/NotificationsController.cs` | Notification read/update APIs |
| Controller | `AISAM.API/Controllers/ContentSchedulesController.cs` | Schedule CRUD/upcoming APIs |
| Controller | `AISAM.API/Controllers/DashboardController.cs` | Dashboard summary API |
| Controller | `AISAM.API/Controllers/DevSchedulerController.cs` | Dev-only run-now endpoint |
| Test | `tests/AISAM.IntegrationTests/NotificationServiceTests.cs` | Notification service scope/read state tests |
| Test | `tests/AISAM.IntegrationTests/NotificationsControllerTests.cs` | Notification controller tests |
| Test | `tests/AISAM.IntegrationTests/ContentScheduleServiceTests.cs` | Schedule CRUD/ownership tests |
| Test | `tests/AISAM.IntegrationTests/ContentSchedulesControllerTests.cs` | Schedule controller tests |
| Test | `tests/AISAM.IntegrationTests/DashboardServiceTests.cs` | Dashboard summary aggregation tests |
| Test | `tests/AISAM.IntegrationTests/DashboardControllerTests.cs` | Dashboard controller tests |
| Test | `tests/AISAM.IntegrationTests/ScheduledPostingServiceTests.cs` | Worker success/fail/idempotency tests |
| Test | `tests/AISAM.IntegrationTests/DevSchedulerControllerTests.cs` | Dev trigger tests |

### File sua

| File | Noi dung |
| --- | --- |
| `AISAM.Data/Model/ContentCalendar.cs` | Chuan hoa fields can cho schedule MVP neu schema active chua du |
| `AISAM.Data/Model/Notification.cs` | Chuan hoa fields can cho notification read state neu can |
| `AISAM.Repositories/AISAMContext.cs` | Mapping cho schedule/notification fields moi neu co migration |
| `AISAM.Repositories/Migrations/*` | Migration nho bo sung schedule runtime fields neu can |
| `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs` | Snapshot sau migration moi |
| `AISAM.API/Middleware/ActiveProfileMiddleware.cs` | Bao ve them `/api/notifications`, `/api/content-schedules`, `/api/dashboard`, `/api/dev/scheduler` |
| `AISAM.API/Program.cs` | Dang ky repository/service moi, hosted service, env gate cho dev trigger |
| `AISAM.API/appsettings.json` hoac `.env.example` | Scheduler interval config neu can |
| `docs/superpowers/CODEBASE.md` | Ghi lai module active sau Phase D |
| `docs/superpowers/CODEBASE_UPDATE.md` | Ghi lai execution va verification Phase D |

## 2. Task map

| Task | Deliverable | Checkpoint bat buoc |
| --- | --- | --- |
| D0 | Ra schema schedule/notification hien tai va chot migration can thiet | Chot co/khong migration Phase D |
| D1 | Repository + DTO foundation | Repo tests/build pass |
| D2 | Notification service/controller | Notification tests pass |
| D3 | Schedule CRUD service/controller | Schedule tests pass |
| D4 | Dashboard summary service/controller | Dashboard tests pass |
| D5 | Scheduled posting service + worker + dev trigger | Worker/dev tests pass |
| D6 | Runtime wiring, middleware prefixes, Swagger smoke | Build/API smoke pass |
| D7 | Full verification va docs | Full build/test/docs cap nhat |

---

### Task D0: Ra schema schedule va notification hien tai

**Files:**
- Read: `AISAM.Data/Model/ContentCalendar.cs`
- Read: `AISAM.Data/Model/Notification.cs`
- Read: `AISAM.Repositories/AISAMContext.cs`
- Read: `AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs`

- [ ] **Step 1: Doi chieu entity schedule hien tai voi spec**

Run:

```powershell
Get-Content -Encoding utf8 'AISAM.Data\Model\ContentCalendar.cs'
Get-Content -Encoding utf8 'AISAM.Repositories\AISAMContext.cs' | Select-String -Pattern 'ContentCalendar|Notification' -Context 0,40
```

Expected:

```text
Xac dinh duoc ContentCalendar hien tai da co hoac chua co IntegrationId, ScheduledAt, ExecutedAt, Status, AttemptCount, LastError
```

- [ ] **Step 2: Ghi checklist migration**

Checklist:

```text
Neu ContentCalendar chua co du runtime fields cua spec thi tao migration Phase D nho.
Neu Notification da co read flag/type/message/profile ownership thi khong tao migration cho notification.
Khong bo sung repeat scheduling fields.
```

Expected: checklist duoc chot truoc khi viet repository/service.

- [ ] **Step 3: Neu can, tao migration shape note trong plan thuc thi**

Run:

```powershell
rg -n "IntegrationId|ScheduledAt|ExecutedAt|AttemptCount|LastError|Status" AISAM.Data AISAM.Repositories -g "*.cs"
```

Expected:

```text
Xac dinh ro co phai tao migration Phase D hay khong
```

Suggested manual commit checkpoint:

```text
chore(schedule): assess phase d schema requirements
```

---

### Task D1: Tao DTO va repository foundation

**Files:**
- Create: `AISAM.Common/Models/NotificationDtos.cs`
- Create: `AISAM.Common/Models/ScheduleDtos.cs`
- Create: `AISAM.Common/Models/DashboardSummaryDto.cs`
- Create: `AISAM.Common/Models/SchedulerRunResultDto.cs`
- Create: `AISAM.Repositories/IRepositories/INotificationRepository.cs`
- Create: `AISAM.Repositories/Repository/NotificationRepository.cs`
- Create: `AISAM.Repositories/IRepositories/IContentCalendarRepository.cs`
- Create: `AISAM.Repositories/Repository/ContentCalendarRepository.cs`
- Create: `AISAM.Repositories/IRepositories/IPerformanceReportRepository.cs`
- Create: `AISAM.Repositories/Repository/PerformanceReportRepository.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/NotificationRepositoryTests.cs`
- Test: `tests/AISAM.IntegrationTests/ContentCalendarRepositoryTests.cs`

- [ ] **Step 1: Viet failing repository tests**

Create tests:

```csharp
[Fact]
public async Task GetUnreadCountAsync_ReturnsOnlyActiveProfilesUnreadNotifications();

[Fact]
public async Task GetDueSchedulesAsync_ReturnsOnlyPendingSchedulesWhoseScheduledAtIsPast();

[Fact]
public async Task GetUpcomingByProfileIdAsync_SortsAscendingAndSkipsDeletedSchedules();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "NotificationRepositoryTests|ContentCalendarRepositoryTests"
```

Expected: FAIL vi repository contracts/implementations chua ton tai.

- [ ] **Step 2: Tao DTO models**

Add `NotificationDtos.cs` voi it nhat:

```csharp
namespace AISAM.Common.Models;

public sealed class NotificationListItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class NotificationDetailDto : NotificationListItemDto
{
    public Guid ProfileId { get; set; }
}

public sealed class UnreadNotificationCountDto
{
    public int Count { get; set; }
}
```

Add `ScheduleDtos.cs` voi it nhat:

```csharp
namespace AISAM.Common.Models;

public sealed class CreateContentScheduleRequest
{
    public Guid ContentId { get; set; }
    public Guid IntegrationId { get; set; }
    public DateTime ScheduledAt { get; set; }
}

public sealed class UpdateContentScheduleRequest
{
    public Guid? IntegrationId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public sealed class ContentScheduleDto
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public Guid ContentId { get; set; }
    public Guid IntegrationId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
```

- [ ] **Step 3: Tao repository contracts**

Add `INotificationRepository.cs`:

```csharp
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Notification>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default);
}
```

Add `IContentCalendarRepository.cs`:

```csharp
public interface IContentCalendarRepository
{
    Task<ContentCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentCalendar>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentCalendar>> GetUpcomingByProfileIdAsync(Guid profileId, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentCalendar>> GetDueSchedulesAsync(DateTime utcNow, int limit, CancellationToken cancellationToken = default);
    Task<ContentCalendar> AddAsync(ContentCalendar schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(ContentCalendar schedule, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Implement EF repositories**

`NotificationRepository` phai:

```csharp
var query = _context.Notifications
    .Where(x => x.ProfileId == profileId && !x.IsDeleted)
    .OrderByDescending(x => x.CreatedAt);
```

`ContentCalendarRepository` phai:

```csharp
var query = _context.ContentCalendars
    .Include(x => x.Content)
    .Where(x => x.ProfileId == profileId && !x.IsDeleted);
```

`GetDueSchedulesAsync` phai loc:

```csharp
.Where(x => !x.IsDeleted &&
            x.Status == ScheduleStatusEnum.Pending &&
            x.ScheduledAt <= utcNow)
```

- [ ] **Step 5: Dang ky DI va rerun tests**

Add vao `Program.cs`:

```csharp
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IContentCalendarRepository, ContentCalendarRepository>();
builder.Services.AddScoped<IPerformanceReportRepository, PerformanceReportRepository>();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "NotificationRepositoryTests|ContentCalendarRepositoryTests"
dotnet build AISAM.sln
```

Expected: repository tests PASS, build PASS.

Suggested manual commit checkpoint:

```text
feat(schedule): add notification and content calendar repositories
```

---

### Task D2: Implement notification service va controller

**Files:**
- Create: `AISAM.Services/IServices/INotificationService.cs`
- Create: `AISAM.Services/Service/NotificationService.cs`
- Create: `AISAM.API/Controllers/NotificationsController.cs`
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/NotificationServiceTests.cs`
- Test: `tests/AISAM.IntegrationTests/NotificationsControllerTests.cs`

- [ ] **Step 1: Viet failing notification tests**

Create tests:

```csharp
[Fact]
public async Task GetPagedAsync_ReturnsOnlyActiveProfilesNotifications();

[Fact]
public async Task MarkReadAsync_ReturnsNotFound_ForAnotherProfilesNotification();

[Fact]
public async Task MarkAllReadAsync_OnlyMarksCurrentProfilesNotifications();

[Fact]
public async Task GetUnreadCountAsync_ReturnsProfilesUnreadCount();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "NotificationServiceTests|NotificationsControllerTests"
```

Expected: FAIL vi service/controller chua ton tai.

- [ ] **Step 2: Tao notification service contract**

Add:

```csharp
public interface INotificationService
{
    Task<GenericResponse<PagedResult<NotificationListItemDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<NotificationDetailDto>> GetByIdAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> MarkReadAsync(Guid profileId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> MarkAllReadAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<GenericResponse<UnreadNotificationCountDto>> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Implement notification service**

`GetByIdAsync` guard:

```csharp
var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
if (notification == null || notification.ProfileId != profileId || notification.IsDeleted)
{
    return GenericResponse<NotificationDetailDto>.CreateError("Notification not found.", HttpStatusCode.NotFound);
}
```

`MarkReadAsync`:

```csharp
notification.IsRead = true;
await _notificationRepository.UpdateAsync(notification, cancellationToken);
return GenericResponse<bool>.CreateSuccess(true, "Notification marked as read.");
```

- [ ] **Step 4: Expose notifications controller**

Create routes:

```text
GET  /api/notifications
GET  /api/notifications/{notificationId}
POST /api/notifications/{notificationId}/mark-read
POST /api/notifications/mark-all-read
GET  /api/notifications/unread-count
```

Controller pattern:

```csharp
var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
var result = await _notificationService.GetUnreadCountAsync(profileId, cancellationToken);
return StatusCode(result.StatusCode, result);
```

- [ ] **Step 5: Bao ve route va rerun tests**

Them prefix moi vao `ActiveProfileMiddleware`:

```csharp
new("/api/notifications"),
```

Dang ky DI:

```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "NotificationServiceTests|NotificationsControllerTests"
dotnet build AISAM.sln
```

Expected: PASS.

Suggested manual commit checkpoint:

```text
feat(notification): add profile-scoped notification apis
```

---

### Task D3: Implement schedule CRUD service va controller

**Files:**
- Create: `AISAM.Services/IServices/IContentScheduleService.cs`
- Create: `AISAM.Services/Service/ContentScheduleService.cs`
- Create: `AISAM.API/Controllers/ContentSchedulesController.cs`
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/ContentScheduleServiceTests.cs`
- Test: `tests/AISAM.IntegrationTests/ContentSchedulesControllerTests.cs`

- [ ] **Step 1: Viet failing schedule tests**

Create tests:

```csharp
[Fact]
public async Task CreateAsync_CreatesPendingSchedule_WhenContentAndIntegrationBelongToProfile();

[Fact]
public async Task CreateAsync_ReturnsNotFound_WhenIntegrationBelongsToAnotherProfile();

[Fact]
public async Task CreateAsync_ReturnsBadRequest_WhenContentAlreadyPublished();

[Fact]
public async Task UpdateAsync_ReturnsBadRequest_WhenScheduleAlreadyCompleted();

[Fact]
public async Task GetUpcomingAsync_ReturnsOnlyFutureSchedulesForProfile();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "ContentScheduleServiceTests|ContentSchedulesControllerTests"
```

Expected: FAIL vi service/controller chua ton tai.

- [ ] **Step 2: Tao schedule service contract**

Add:

```csharp
public interface IContentScheduleService
{
    Task<GenericResponse<ContentScheduleDto>> CreateAsync(Guid profileId, CreateContentScheduleRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<PagedResult<ContentScheduleDto>>> GetPagedAsync(Guid profileId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentScheduleDto>> GetByIdAsync(Guid profileId, Guid scheduleId, CancellationToken cancellationToken = default);
    Task<GenericResponse<ContentScheduleDto>> UpdateAsync(Guid profileId, Guid scheduleId, UpdateContentScheduleRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<bool>> DeleteAsync(Guid profileId, Guid scheduleId, CancellationToken cancellationToken = default);
    Task<GenericResponse<IReadOnlyList<ContentScheduleDto>>> GetUpcomingAsync(Guid profileId, int limit, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Implement ownership va schedule validation**

Core guards:

```csharp
var content = await _contentRepository.GetByIdAsync(request.ContentId, cancellationToken);
if (content == null || content.ProfileId != profileId || content.IsDeleted)
{
    return GenericResponse<ContentScheduleDto>.CreateError("Content not found.", HttpStatusCode.NotFound);
}

if (content.Status == ContentStatusEnum.Published)
{
    return GenericResponse<ContentScheduleDto>.CreateError("Published content cannot be scheduled again.", HttpStatusCode.BadRequest);
}

var integration = await _socialIntegrationRepository.GetByIdAsync(request.IntegrationId, cancellationToken);
if (integration == null || integration.ProfileId != profileId || integration.IsDeleted || integration.BrandId != content.BrandId)
{
    return GenericResponse<ContentScheduleDto>.CreateError("Social integration not found.", HttpStatusCode.NotFound);
}
```

- [ ] **Step 4: Tao notification side effect trong create/update/delete**

Sau moi hanh dong:

```csharp
await _notificationRepository.AddAsync(new Notification
{
    ProfileId = profileId,
    Type = NotificationTypeEnum.SystemUpdate,
    Title = "Schedule updated",
    Message = "...",
    IsRead = false
}, cancellationToken);
```

Ghi chu: neu enum/type cu co gia tri phu hop hon trong source cu, uu tien tai su dung gia tri do, nhung khong tao enum moi neu chua can.

- [ ] **Step 5: Expose schedule controller**

Routes:

```text
POST   /api/content-schedules
GET    /api/content-schedules
GET    /api/content-schedules/{scheduleId}
PUT    /api/content-schedules/{scheduleId}
DELETE /api/content-schedules/{scheduleId}
GET    /api/content-schedules/upcoming
```

- [ ] **Step 6: Wire middleware/DI va rerun tests**

Them prefix:

```csharp
new("/api/content-schedules"),
```

Dang ky:

```csharp
builder.Services.AddScoped<IContentScheduleService, ContentScheduleService>();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "ContentScheduleServiceTests|ContentSchedulesControllerTests"
dotnet build AISAM.sln
```

Expected: PASS.

Suggested manual commit checkpoint:

```text
feat(schedule): add content scheduling crud apis
```

---

### Task D4: Implement dashboard summary service va controller

**Files:**
- Create: `AISAM.Services/IServices/IDashboardService.cs`
- Create: `AISAM.Services/Service/DashboardService.cs`
- Create: `AISAM.API/Controllers/DashboardController.cs`
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Modify: `AISAM.API/Program.cs`
- Test: `tests/AISAM.IntegrationTests/DashboardServiceTests.cs`
- Test: `tests/AISAM.IntegrationTests/DashboardControllerTests.cs`

- [ ] **Step 1: Viet failing dashboard tests**

Create tests:

```csharp
[Fact]
public async Task GetSummaryAsync_ReturnsCountsScopedToActiveProfile();

[Fact]
public async Task GetSummaryAsync_CountsUpcomingSchedulesAndUnreadNotifications();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "DashboardServiceTests|DashboardControllerTests"
```

Expected: FAIL vi service/controller chua ton tai.

- [ ] **Step 2: Tao dashboard contract**

Add:

```csharp
public interface IDashboardService
{
    Task<GenericResponse<DashboardSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Implement summary aggregation**

Service phai tinh it nhat:

```csharp
var draftContentCount = ...
var publishedContentCount = ...
var upcomingScheduleCount = ...
var failedScheduleCount = ...
var activeSocialIntegrationCount = ...
var publishedPostCount = ...
var unreadNotificationCount = ...
```

Return:

```csharp
return GenericResponse<DashboardSummaryDto>.CreateSuccess(new DashboardSummaryDto
{
    DraftContentCount = draftContentCount,
    PublishedContentCount = publishedContentCount,
    UpcomingScheduleCount = upcomingScheduleCount,
    FailedScheduleCount = failedScheduleCount,
    ActiveSocialIntegrationCount = activeSocialIntegrationCount,
    PublishedPostCount = publishedPostCount,
    UnreadNotificationCount = unreadNotificationCount
});
```

- [ ] **Step 4: Expose dashboard controller**

Route:

```text
GET /api/dashboard/summary
```

Pattern:

```csharp
var profileId = ProfileContextHelper.GetActiveProfileIdOrThrow(HttpContext);
var result = await _dashboardService.GetSummaryAsync(profileId, cancellationToken);
return StatusCode(result.StatusCode, result);
```

- [ ] **Step 5: Wire middleware/DI va rerun tests**

Them prefix:

```csharp
new("/api/dashboard"),
```

Dang ky:

```csharp
builder.Services.AddScoped<IDashboardService, DashboardService>();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "DashboardServiceTests|DashboardControllerTests"
dotnet build AISAM.sln
```

Expected: PASS.

Suggested manual commit checkpoint:

```text
feat(dashboard): add profile summary endpoint
```

---

### Task D5: Implement scheduled posting service, worker va dev trigger

**Files:**
- Create: `AISAM.Services/IServices/IScheduledPostingService.cs`
- Create: `AISAM.Services/Service/ScheduledPostingService.cs`
- Create: `AISAM.Services/Service/ScheduledPostingBackgroundService.cs`
- Create: `AISAM.API/Controllers/DevSchedulerController.cs`
- Modify: `AISAM.API/Program.cs`
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Test: `tests/AISAM.IntegrationTests/ScheduledPostingServiceTests.cs`
- Test: `tests/AISAM.IntegrationTests/DevSchedulerControllerTests.cs`

- [ ] **Step 1: Viet failing worker tests**

Create tests:

```csharp
[Fact]
public async Task RunDueSchedulesAsync_PublishesDueScheduleAndMarksCompleted_WhenPublishSucceeds();

[Fact]
public async Task RunDueSchedulesAsync_MarksFailedAndCreatesNotification_WhenPublishFails();

[Fact]
public async Task RunDueSchedulesAsync_DoesNotReprocessCompletedSchedules();
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "ScheduledPostingServiceTests|DevSchedulerControllerTests"
```

Expected: FAIL vi service/worker/controller chua ton tai.

- [ ] **Step 2: Tao scheduler contract**

Add:

```csharp
public interface IScheduledPostingService
{
    Task<SchedulerRunResultDto> RunDueSchedulesAsync(int batchSize, CancellationToken cancellationToken = default);
}
```

`SchedulerRunResultDto` phai co:

```csharp
public sealed class SchedulerRunResultDto
{
    public int ScannedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}
```

- [ ] **Step 3: Implement due schedule execution**

Core loop:

```csharp
var schedules = await _contentCalendarRepository.GetDueSchedulesAsync(DateTime.UtcNow, batchSize, cancellationToken);
foreach (var schedule in schedules)
{
    schedule.Status = ScheduleStatusEnum.Processing;
    await _contentCalendarRepository.UpdateAsync(schedule, cancellationToken);

    var publishResult = await _contentService.PublishAsync(schedule.ContentId, schedule.IntegrationId, schedule.ProfileId, cancellationToken);

    if (publishResult.Success)
    {
        schedule.Status = ScheduleStatusEnum.Completed;
        schedule.ExecutedAt = DateTime.UtcNow;
        ...
    }
    else
    {
        schedule.Status = ScheduleStatusEnum.Failed;
        schedule.AttemptCount += 1;
        schedule.LastError = publishResult.Message;
        ...
    }
}
```

- [ ] **Step 4: Tao notification side effect cho worker**

Success notification:

```csharp
new Notification
{
    ProfileId = schedule.ProfileId,
    Type = NotificationTypeEnum.SystemUpdate,
    Title = "Scheduled publish succeeded",
    Message = $"Content {schedule.ContentId} was published successfully.",
    IsRead = false
}
```

Fail notification:

```csharp
new Notification
{
    ProfileId = schedule.ProfileId,
    Type = NotificationTypeEnum.SystemUpdate,
    Title = "Scheduled publish failed",
    Message = schedule.LastError ?? "Publishing failed.",
    IsRead = false
}
```

- [ ] **Step 5: Implement background worker**

Worker loop:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await _scheduledPostingService.RunDueSchedulesAsync(20, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled posting worker iteration failed.");
        }

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
    }
}
```

- [ ] **Step 6: Expose dev-only controller**

Route:

```text
POST /api/dev/scheduler/run-now
```

Controller action:

```csharp
[HttpPost("scheduler/run-now")]
public async Task<ActionResult<GenericResponse<SchedulerRunResultDto>>> RunNow(CancellationToken cancellationToken = default)
{
    var result = await _scheduledPostingService.RunDueSchedulesAsync(20, cancellationToken);
    return Ok(GenericResponse<SchedulerRunResultDto>.CreateSuccess(result));
}
```

Chi map route khi `app.Environment.IsDevelopment()`.

- [ ] **Step 7: Wire DI/hosted service va rerun tests**

Dang ky:

```csharp
builder.Services.AddScoped<IScheduledPostingService, ScheduledPostingService>();
builder.Services.AddHostedService<ScheduledPostingBackgroundService>();
```

Them prefix:

```csharp
new("/api/dev/scheduler"),
```

Run:

```powershell
dotnet test tests\AISAM.IntegrationTests\AISAM.IntegrationTests.csproj --filter "ScheduledPostingServiceTests|DevSchedulerControllerTests"
dotnet build AISAM.sln
```

Expected: PASS.

Suggested manual commit checkpoint:

```text
feat(schedule): add scheduled posting worker and dev trigger
```

---

### Task D6: Runtime wiring, middleware prefixes va Swagger smoke

**Files:**
- Modify: `AISAM.API/Program.cs`
- Modify: `AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- Test: local Swagger smoke

- [ ] **Step 1: Chot protected prefixes**

`ActiveProfileMiddleware` phai chua it nhat:

```csharp
new("/api/content"),
new("/api/ai"),
new("/api/conversations"),
new("/api/social-auth"),
new("/api/social"),
new("/api/posts"),
new("/api/notifications"),
new("/api/content-schedules"),
new("/api/dashboard"),
new("/api/dev/scheduler")
```

- [ ] **Step 2: Chot Program.cs registrations**

`Program.cs` phai dang ky day du repository/service/worker moi cua Phase D va khong duplicate registration cu.

Run:

```powershell
rg -n "NotificationRepository|ContentCalendarRepository|DashboardService|ScheduledPostingBackgroundService|DevSchedulerController" AISAM.API\Program.cs AISAM.API\Controllers -g "*.cs"
```

Expected: thay du cac registrations va controller files.

- [ ] **Step 3: Chay Swagger smoke**

Run:

```powershell
$env:ASPNETCORE_URLS='http://localhost:5283'
$env:ASPNETCORE_ENVIRONMENT='Development'
$p = Start-Process dotnet -ArgumentList @('bin\Debug\net8.0\AISAM.API.dll') -WorkingDirectory 'D:\final\AISAM-FINAL\AISAM-BE\AISAM.API' -WindowStyle Hidden -PassThru
Start-Sleep -Seconds 3
$swagger = Invoke-WebRequest 'http://localhost:5283/swagger/v1/swagger.json' -UseBasicParsing
$swagger.Content.Contains('/api/notifications')
$swagger.Content.Contains('/api/content-schedules')
$swagger.Content.Contains('/api/dashboard/summary')
$swagger.Content.Contains('/api/dev/scheduler/run-now')
Stop-Process -Id $p.Id
```

Expected:

```text
True
True
True
True
```

Suggested manual commit checkpoint:

```text
chore(api): wire phase d routes and middleware protection
```

---

### Task D7: Full verification va docs

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

- [ ] **Step 2: Chay runtime smoke boundary**

Cases:

```text
GET /api/notifications khong JWT -> 401
GET /api/content-schedules khong JWT -> 401
GET /api/dashboard/summary khong JWT -> 401
POST /api/dev/scheduler/run-now ngoai Development -> route khong duoc map
```

Expected: boundary ro rang, host khong crash.

- [ ] **Step 3: Chay worker smoke local**

Sequence:

```text
tao content draft
tao social integration fixture
tao due schedule
goi POST /api/dev/scheduler/run-now trong Development
verify content = Published
verify post duoc tao
verify schedule = Completed
verify notification success duoc tao
```

Expected:

```text
Scheduler success path pass.
```

- [ ] **Step 4: Cap nhat docs**

`docs/superpowers/CODEBASE.md` can duoc cap nhat:

```text
Active modules: Notification, Scheduling, Dashboard.
Required header X-Profile-Id mo rong cho notifications, content-schedules, dashboard, dev scheduler.
Background worker su dung ContentService.PublishAsync.
```

`docs/superpowers/CODEBASE_UPDATE.md` can ghi:

```text
Phase D task execution record.
Migration note neu co.
Build/test/swagger/runtime smoke ket qua.
Blocker external neu real Facebook publish hoac DB state can ghi ro.
```

- [ ] **Step 5: Chot blocker note**

Neu local DB migration history van lech hoac real Facebook credentials van thieu, ghi ro:

```text
Khong khang dinh real Facebook scheduled publish da pass neu chua co credentials.
Khong khang dinh migration apply thanh cong neu DB local van lech migration history.
```

Suggested manual commit checkpoint:

```text
docs: record phase d scheduling verification
```

---

## 3. Definition of Done checklist

- [ ] Notification repository/service/controller active theo active profile
- [ ] Schedule repository/service/controller active theo active profile
- [ ] Dashboard summary endpoint active
- [ ] Background worker publish that qua `ContentService.PublishAsync`
- [ ] Dev-only scheduler trigger chi hoat dong trong `Development`
- [ ] `dotnet build AISAM.sln` pass
- [ ] `dotnet test AISAM.sln` pass
- [ ] Swagger smoke pass
- [ ] Runtime auth/config smoke pass
- [ ] Blocker external duoc ghi ro trong docs neu con ton tai
