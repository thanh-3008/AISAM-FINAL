# Phase D - Notification, Scheduling, Dashboard Design

Last updated: 2026-06-01

## 1. Muc tieu

Phase D tuong ung Phase 7 trong `BACKEND_CODE_PLAN.md`.

Muc tieu cua phase nay la hoan thien lop van hanh sau khi codebase da co:

- content CRUD va AI flow o Phase B
- social integration va Facebook publishing o Phase C

Scope Phase D:

- them notification noi bo trong DB va API doc/danh dau da doc
- them scheduling dang bai mot lan cho content
- them background worker quet lich den han va publish that qua `ContentService.PublishAsync`
- them dashboard tong quan MVP theo active profile
- them manual trigger dev-only de chay scheduler ngay trong local/dev smoke

Ngoai scope:

- repeat scheduling (`Daily`, `Weekly`, `Monthly`)
- push/realtime/email notification
- dashboard performance metric phuc tap
- public scheduler trigger cho end user
- retry/backoff strategy phuc tap nhieu cap

## 2. Nguyen tac thiet ke

Phase D phai bam cac nguyen tac sau:

1. Tai su dung `ContentService.PublishAsync` cua Phase C, khong nhan doi publish logic.
2. Moi API moi phai scope theo active profile bang JWT + `X-Profile-Id`.
3. Background worker phai fail safe: khong lam crash host khi external config thieu hoac publish loi.
4. Scheduling chi ho tro one-time publish de giam rui ro publish lap.
5. Notification giai quyet nhu lop persistence + read API, khong keo them transport layer.
6. Dashboard MVP chi tong hop du lieu hien co trong DB, khong dua performance analytics nang.

## 3. Kien truc tong quan

Phase D duoc chia thanh 4 cum:

- Notification module
- Scheduling module
- Dashboard module
- Scheduler execution module

Luong tong quat:

1. User tao schedule cho `contentId`, `integrationId`, `scheduledAt`
2. Schedule duoc luu trong DB voi ownership theo active profile
3. Background worker hoac dev trigger quet cac schedule den han chua xu ly
4. Moi schedule due goi `ContentService.PublishAsync(contentId, integrationId, profileId)`
5. Ket qua publish duoc phan anh vao:
   - schedule status
   - notification records
   - post/content state da co tu Phase C
6. Dashboard doc du lieu tong hop tu content, schedule, social integration, post, notification

## 4. Pham vi module

### 4.1 Notification

Can active:

- repository
- service
- controller
- DTO list/detail/count/update

Behavior MVP:

- list notifications theo active profile
- xem chi tiet notification
- mark mot notification da doc
- mark all da doc
- lay unread count

Nguon tao notification trong Phase D:

- schedule created
- schedule updated
- schedule deleted
- scheduled publish succeeded
- scheduled publish failed

Chua lam:

- realtime stream
- websocket/signalr
- email notification tuong ung event
- notification preferences

### 4.2 Scheduling

Can active:

- repository cho `ContentCalendar`
- service CRUD/list/upcoming
- controller
- worker/service execute due schedules

Behavior MVP:

- tao schedule mot lan
- update schedule khi chua completed
- xoa schedule
- lay danh sach schedule
- lay upcoming schedules
- execute due schedules qua worker hoac dev trigger

### 4.3 Dashboard

Can active:

- service tong hop
- controller summary
- DTO summary

Behavior MVP:

- tong so content theo trang thai
- tong so social integration active
- tong so post da publish
- tong so schedule upcoming
- tong so notification unread
- tong so schedule failed

Khong lam:

- chart phuc tap
- breakdown performance theo kenh
- engagement metrics
- ad performance dashboard

### 4.4 Scheduler execution

Can active:

- scheduled posting service
- hosted background service
- dev-only trigger endpoint

Behavior MVP:

- scan due schedules theo timer
- lock/mark status phu hop de tranh xu ly lap trong mot luot
- publish that qua `ContentService.PublishAsync`
- cap nhat schedule va tao notification theo ket qua
- swallow/log exceptions de host khong chet

## 5. API de xuat

Tat ca API duoi day can:

- `[Authorize]`
- `X-Profile-Id`
- active profile ownership

### 5.1 Notification APIs

```text
GET    /api/notifications
GET    /api/notifications/{notificationId}
POST   /api/notifications/{notificationId}/mark-read
POST   /api/notifications/mark-all-read
GET    /api/notifications/unread-count
```

### 5.2 Scheduling APIs

```text
POST   /api/content-schedules
GET    /api/content-schedules
GET    /api/content-schedules/{scheduleId}
PUT    /api/content-schedules/{scheduleId}
DELETE /api/content-schedules/{scheduleId}
GET    /api/content-schedules/upcoming
```

### 5.3 Dashboard API

```text
GET    /api/dashboard/summary
```

### 5.4 Dev-only trigger API

```text
POST   /api/dev/scheduler/run-now
```

Route nay chi duoc map trong `Development`.

## 6. Ownership va validation rules

### 6.1 Notification ownership

- notification phai thuoc `ProfileId` cua active profile
- user khong duoc doc/mark notification cua profile khac

### 6.2 Schedule ownership

Khi tao/cap nhat/xoa schedule:

- `content.ProfileId` phai khop active profile
- `integration.ProfileId` phai khop active profile
- `integration.BrandId` phai khop `content.BrandId`
- `content` khong duoc da `Published`
- `scheduledAt` phai la gia tri hop le

### 6.3 Scheduler execution ownership

Background worker khong nhan profile tu request. No phai doc du lieu da luu trong DB va chi xu ly cac schedule:

- chua completed
- den han
- khong bi xoa
- co `content` va `integration` hop le

Neu resource lien quan khong con hop le, schedule phai chuyen sang `Failed` va tao notification loi.

### 6.4 Dashboard ownership

Moi metric trong dashboard phai duoc tinh tu active profile, khong tong hop cheo profile.

## 7. Mo hinh du lieu va trang thai

Phase D uu tien tai su dung `ContentCalendar` lam schedule store.

Canh bao quan trong: entity hien tai co the chua du cot de the hien execution state cua scheduler MVP. Phase D duoc phep tao migration nho neu sau khi doi chieu source cu va schema active can bo sung metadata toi thieu.

### 7.1 Scheduling fields toi thieu

Neu schema hien tai chua co du, Phase D can bo sung cac truong phuc vu runtime:

- `IntegrationId`
- `ScheduledAt`
- `ExecutedAt`
- `Status`
- `AttemptCount`
- `LastError`

Trang thai schedule de xuat:

- `Pending`
- `Processing`
- `Completed`
- `Failed`

Trong MVP:

- khong ho tro repeat
- khong auto-reschedule
- khong retry policy nhieu cap

### 7.2 Notification fields

`Notification` can duoc dung nhat quan cho:

- type
- title/message
- profile ownership
- read flag
- created time

Neu source cu co them metadata JSON hoac link field huu ich ma schema active chua co, chi bo sung neu thuc su can cho API Phase D.

### 7.3 Dashboard DTO

Dashboard summary DTO can it nhat co:

- `draftContentCount`
- `publishedContentCount`
- `pendingApprovalContentCount` neu data dang dung enum nay
- `upcomingScheduleCount`
- `failedScheduleCount`
- `activeSocialIntegrationCount`
- `publishedPostCount`
- `unreadNotificationCount`

Neu enum/status nao khong co data active thi van de field trong DTO, nhung service chi map theo enum that cua codebase hien tai.

## 8. Luong xu ly chinh

### 8.1 Tao schedule

1. Lay active profile tu `ProfileContextHelper`
2. Validate `contentId`, `integrationId`, `scheduledAt`
3. Validate ownership cua content/integration
4. Validate content chua published
5. Luu schedule
6. Tao notification `schedule created`

### 8.2 Cap nhat schedule

1. Validate schedule thuoc active profile
2. Khong cho sua schedule da `Completed`
3. Validate lai integration/content neu request cho phep doi
4. Save
5. Tao notification `schedule updated`

### 8.3 Xoa schedule

1. Validate ownership
2. Soft delete hoac hard delete theo pattern source cu/schema active
3. Tao notification `schedule deleted`

Spec nay uu tien soft delete neu entity da co field xoa mem; neu khong co, giu implementation sat schema active va ghi ro trong plan.

### 8.4 Background execution

1. Worker theo chu ky goi service scan due schedules
2. Moi due schedule duoc mark `Processing`
3. Goi `ContentService.PublishAsync(contentId, integrationId, profileId)`
4. Neu success:
   - mark `Completed`
   - set `ExecutedAt`
   - tao notification success
5. Neu fail:
   - mark `Failed`
   - tang `AttemptCount`
   - save `LastError`
   - tao notification fail
6. Bat moi exception o boundary worker va log lai

### 8.5 Dev trigger

1. Chi map trong `Development`
2. Endpoint goi cung service scan due schedules
3. Tra ve summary don gian: so schedule duoc scan, success, fail

## 9. Xu ly loi va operational behavior

### 9.1 API errors

Can giu pattern `GenericResponse<T>` hien tai.

Cases chinh:

- resource ngoai profile -> `404`
- request invalid -> `400`
- host thieu social config khi scheduler publish -> khong crash host; schedule `Failed`

### 9.2 Worker resilience

Worker phai:

- log loi tung schedule
- tiep tuc xu ly schedule khac trong cung luot
- khong throw ra ngoai `BackgroundService` lam dung host

### 9.3 Publish dependency

Scheduler phu thuoc vao Phase C:

- `ContentService.PublishAsync`
- social integration/profile ownership
- provider configuration

Neu Facebook config thieu:

- worker khong duoc crash
- schedule duoc danh dau `Failed`
- notification fail duoc tao

## 10. Testing strategy

Phase D can mo rong test theo 4 lop:

### 10.1 Service tests

- tao/cap nhat/xoa schedule dung ownership
- khong cho schedule content da published
- notification list/unread count dung scope
- dashboard summary tra dung so lieu theo profile

### 10.2 Worker tests

- due schedule goi `ContentService.PublishAsync`
- success path mark completed va tao notification
- fail path mark failed, luu loi, tao notification
- schedule completed khong bi xu ly lai

### 10.3 Controller tests

- propagate `X-Profile-Id` dung vao service
- tra `404/400` dung theo service result
- dev trigger khong map/khong cho dung ngoai `Development`

### 10.4 Verification

- `dotnet build AISAM.sln`
- `dotnet test AISAM.sln`
- Swagger smoke cho notifications, schedules, dashboard
- local smoke cho dev trigger

## 11. Rollout strategy

Phase D nen trien khai theo thu tu:

1. repository + DTO foundation
2. notification API
3. schedule CRUD
4. dashboard summary
5. worker + dev trigger
6. full verification + docs

Ly do:

- notification va schedule CRUD it rui ro hon worker
- dashboard phu thuoc read-only, lam sau khi data contracts ro
- worker dat cuoi cung de giam blast radius

## 12. Risks va blocker

### 12.1 Risks

- schedule publish lap neu state transition khong chat
- worker xu ly song song neu host restart/scheduler overlap
- publish fail do Facebook config/token/resource thay doi
- schema `ContentCalendar`/`Notification` active co the khong khop source cu 100%

### 12.2 Blockers da biet

- real Facebook publish van can credentials/quyen o moi truong local
- local database migration history hien dang lech schema thuc te, can can nhac khi tao/apply migration moi

## 13. Definition of Done

Phase D duoc xem la hoan tat khi:

- Notification API hoat dong theo active profile
- Scheduling CRUD hoat dong theo active profile
- Background worker publish that qua `ContentService.PublishAsync`
- Dev trigger chi hoat dong trong `Development`
- Dashboard summary MVP hoat dong
- `dotnet build AISAM.sln` pass
- `dotnet test AISAM.sln` pass
- Swagger smoke pass
- Cac blocker external duoc ghi ro neu chua chay publish that thanh cong
