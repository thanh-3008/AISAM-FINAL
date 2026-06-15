# Phase 7 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `7.1` den `7.4` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu truc tiep voi backend Notifications, Scheduling va Dev Scheduler hien tai trong `AISAM-BE`.

Pham vi Phase 7:

- Hoan thien notification center, unread badge, mark read, mark all read
- Hoan thien scheduling CRUD cho content publish mot lan
- Hoan thien schedule action tu content detail/list
- Hoan thien dev scheduler trigger panel chi cho moi truong development
- Dat scheduling UX theo target product, nghia la content hop le moi duoc schedule

Khong lam trong Phase 7:

- Subscription/payment
- Team/Approval
- Ads/Campaigns
- Instagram/TikTok
- Analytics nang cao

Luu y target product:

- `requirement.md` yeu cau schedule cho content da approve.
- Phase 7 vi vay phai goi lai dung publish guard/business guard tu phase approval, khong cho UI tao schedule cho content sai lifecycle.

Can cu backend da doi chieu truc tiep cho Phase 7:

- `AISAM-BE/AISAM.API/Controllers/NotificationsController.cs`
- `AISAM-BE/AISAM.Services/Service/NotificationService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/NotificationRepository.cs`
- `AISAM-BE/AISAM.Common/Models/NotificationDtos.cs`
- `AISAM-BE/AISAM.API/Controllers/ContentSchedulesController.cs`
- `AISAM-BE/AISAM.Services/Service/ContentScheduleService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ContentCalendarRepository.cs`
- `AISAM-BE/AISAM.Common/Models/ScheduleDtos.cs`
- `AISAM-BE/AISAM.API/Controllers/DevSchedulerController.cs`
- `AISAM-BE/AISAM.Services/Service/ScheduledPostingService.cs`
- `AISAM-BE/AISAM.Common/Models/SchedulerRunResultDto.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`

## Tong quan thu tu lam

1. Task 7.1 - Tao notification center
2. Task 7.2 - Tao scheduling pages
3. Task 7.3 - Tao schedule action tu content detail
4. Task 7.4 - Tao dev scheduler trigger panel
5. Chay verify tong the Phase 7

## Contract backend Notifications/Scheduling can chot truoc khi code

### Header rule quan trong

Tat ca route Phase 7 deu can:

- `Authorization`
- `X-Profile-Id`

Ly do:

- `/api/notifications`
- `/api/content-schedules`
- `/api/dev/scheduler`

deu nam trong `ActiveProfileMiddleware`.

Frontend khong duoc goi API Phase 7 neu chua co `activeProfileId`.

### Middleware behavior can biet

Neu context profile sai, backend tra:

- `401` neu chua login
- `401` neu thieu/invalid `X-Profile-Id`
- `404` neu profile khong ton tai
- `403` neu profile khong thuoc user

Phase 7 tai su dung shell/profile recovery flow da co tu Phase 2/3.

### Envelope response

```ts
type ApiResponse<T> = {
  success: boolean
  message?: string
  statusCode: number
  data?: T | null
  error?: {
    errorCode?: string
    errorMessage?: string
    stackTrace?: string
    validationErrors?: Record<string, string[]>
  }
  timestamp: string
}
```

## Notifications contract

### Route active

```text
GET  /api/notifications?page=&pageSize=
GET  /api/notifications/{notificationId}
POST /api/notifications/{notificationId}/mark-read
POST /api/notifications/mark-all-read
GET  /api/notifications/unread-count
```

### Notification list item exact

```ts
type NotificationListItemDto = {
  id: string
  type: string
  title: string
  message: string
  isRead: boolean
  createdAt: string
}
```

### Notification detail exact

```ts
type NotificationDetailDto = NotificationListItemDto & {
  profileId: string
}
```

### Unread count exact

```ts
type UnreadNotificationCountDto = {
  count: number
}
```

### Notifications behavior that backend dang ho tro

`GET /notifications`:

- chi ho tro `page`, `pageSize`
- sort mac dinh `CreatedAt DESC`
- khong ho tro `isRead` filter o backend hien tai

`POST /notifications/{id}/mark-read`:

- tra `404` neu notification khong thuoc profile hoac da deleted

`POST /notifications/mark-all-read`:

- mark toan bo unread notifications cua profile hien tai

`GET /notifications/unread-count`:

- tra tong unread count cua profile hien tai

Frontend khong nen gia dinh co API search/filter by type/isRead neu backend chua support.

## Scheduling contract

### Route active

```text
POST   /api/content-schedules
GET    /api/content-schedules?page=&pageSize=
GET    /api/content-schedules/upcoming?limit=
GET    /api/content-schedules/{scheduleId}
PUT    /api/content-schedules/{scheduleId}
DELETE /api/content-schedules/{scheduleId}
```

### Schedule create request exact

```ts
type CreateContentScheduleRequest = {
  contentId: string
  integrationId: string
  scheduledAt: string
}
```

### Schedule update request exact

```ts
type UpdateContentScheduleRequest = {
  integrationId?: string
  scheduledAt?: string
}
```

### Schedule response exact

```ts
type ContentScheduleDto = {
  id: string
  profileId: string
  contentId: string
  integrationId: string
  scheduledAt: string
  executedAt?: string | null
  status: string
  attemptCount: number
  lastError?: string | null
}
```

### Schedule behavior that backend dang ho tro

Create schedule:

- content phai thuoc profile hien tai
- content khong duoc da `Published`
- integration phai thuoc profile hien tai
- integration phai thuoc cung brand voi content
- `scheduledAt` se duoc normalize ve UTC
- create thanh cong tao them 1 notification `Schedule created`

Update schedule:

- khong update duoc neu `Status = Completed`
- khong update duoc neu content da `Published`
- co the update `integrationId`, `scheduledAt`, hoac ca hai
- update thanh cong tao notification `Schedule updated`

Delete schedule:

- la soft delete: `IsDeleted = true`, `IsActive = false`
- delete thanh cong tao notification `Schedule deleted`

Upcoming:

- tra schedule tuong lai cua profile hien tai
- order tang dan theo `ScheduledAt`
- `limit` bi clamp 1..100 o repository

Paged schedules:

- order tang dan theo `ScheduledAt`
- chi ho tro `page`, `pageSize`
- chua ho tro filter by status/content/brand o backend hien tai

### Status behavior can biet

`ContentScheduleDto.status` la string backend map tu enum `ScheduleStatusEnum`.
Frontend khong nen hardcode qua chat ma nen map linh hoat theo string.

Nhung workflow that hien tai bao gom it nhat:

- `Pending`
- `Processing`
- `Completed`
- `Failed`

`Failed` co the di kem `lastError`.

## Dev scheduler contract

### Route active

```text
POST /api/dev/scheduler/run-now
```

### Response exact

```ts
type SchedulerRunResultDto = {
  scannedCount: number
  successCount: number
  failedCount: number
}
```

### Development-only behavior can biet

Controller co `[DevelopmentOnly]` va check `_environment.IsDevelopment()`:

- o Development: route available
- ngoai Development: tra `404 Not found.`

Frontend phai:

- chi hien panel khi config frontend dang o development mode
- khong render button nay trong production

## Task 7.1 - Tao notification center

### Muc tieu

- Hien danh sach notifications cua active profile
- Hoan thien unread badge, mark read, mark all read

### File can tao

```text
AISAM-FE/src/app/(app)/notifications/page.tsx
AISAM-FE/src/features/notifications/api/get-notifications.ts
AISAM-FE/src/features/notifications/api/get-notification-by-id.ts
AISAM-FE/src/features/notifications/api/get-unread-count.ts
AISAM-FE/src/features/notifications/api/mark-read.ts
AISAM-FE/src/features/notifications/api/mark-all-read.ts
AISAM-FE/src/features/notifications/components/notification-list.tsx
AISAM-FE/src/features/notifications/components/notification-list-item.tsx
AISAM-FE/src/features/notifications/components/notification-detail.tsx
AISAM-FE/src/features/notifications/components/notification-badge.tsx
AISAM-FE/src/features/notifications/components/notification-empty-state.tsx
AISAM-FE/src/features/notifications/components/notification-error-state.tsx
AISAM-FE/src/features/notifications/hooks/use-notifications-query.ts
AISAM-FE/src/types/notification.ts
```

### API helpers can co

```ts
type GetNotificationsParams = {
  page?: number
  pageSize?: number
}
```

Routes:

```text
GET  /api/notifications
GET  /api/notifications/{notificationId}
POST /api/notifications/{notificationId}/mark-read
POST /api/notifications/mark-all-read
GET  /api/notifications/unread-count
```

### UI list can co

- title
- message
- type
- createdAt
- read/unread visual state

CTA:

- mo detail drawer/modal hoac detail inline
- mark read cho item unread
- mark all read

### Badge behavior

`notification-badge.tsx` nen:

- goi `GET /notifications/unread-count`
- hien `count`
- an badge neu `count = 0` hoac hien `0` theo design team chot

Khuyen nghi operational UI:

- an badge neu `0`

### State update strategy

Sau `mark-read`:

- cap nhat item local thanh `isRead = true`
- giam unread count ngay
- co the refetch nhe sau do

Sau `mark-all-read`:

- update toan bo items page hien tai thanh read
- set unread count = 0
- refetch list neu can

### Detail view

Route backend co `GET /notifications/{id}`, nhung page route rieng khong bat buoc.
Co the dung:

- modal
- side panel

de giu UX nhanh hon.

### Definition of Done

- List notifications load duoc
- Badge unread count load duoc
- Mark read cap nhat item va badge
- Mark all read cap nhat toan bo UI

### Verify

- Test unread count > 0
- Mark read 1 item
- Mark all read
- Reload page va count van dung

## Task 7.2 - Tao scheduling pages

### Muc tieu

- Hien danh sach schedules cua active profile
- Cho user tao, sua, xoa schedule
- Hien upcoming schedules rieng

### File can tao

```text
AISAM-FE/src/app/(app)/calendar/page.tsx
AISAM-FE/src/features/schedules/api/create-schedule.ts
AISAM-FE/src/features/schedules/api/get-schedules.ts
AISAM-FE/src/features/schedules/api/get-upcoming-schedules.ts
AISAM-FE/src/features/schedules/api/get-schedule.ts
AISAM-FE/src/features/schedules/api/update-schedule.ts
AISAM-FE/src/features/schedules/api/delete-schedule.ts
AISAM-FE/src/features/schedules/components/schedule-list.tsx
AISAM-FE/src/features/schedules/components/schedule-list-item.tsx
AISAM-FE/src/features/schedules/components/schedule-form.tsx
AISAM-FE/src/features/schedules/components/upcoming-schedules.tsx
AISAM-FE/src/features/schedules/components/schedule-empty-state.tsx
AISAM-FE/src/features/schedules/components/schedule-error-state.tsx
AISAM-FE/src/features/schedules/hooks/use-schedules-query.ts
AISAM-FE/src/types/schedule.ts
```

### API routes can co

```text
POST   /api/content-schedules
GET    /api/content-schedules
GET    /api/content-schedules/upcoming
GET    /api/content-schedules/{scheduleId}
PUT    /api/content-schedules/{scheduleId}
DELETE /api/content-schedules/{scheduleId}
```

### List page can co

Section 1:

- upcoming schedules

Section 2:

- paged full schedule list

List item nen hien:

- contentId hoac metadata content neu team co san local context
- integrationId hoac target label neu team co san local mapping
- scheduledAt
- executedAt
- status
- attemptCount
- lastError

### Create schedule form

Payload:

```ts
{
  contentId: string
  integrationId: string
  scheduledAt: string
}
```

Validation frontend:

- contentId required
- integrationId required
- scheduledAt required
- scheduledAt phai la future datetime hop le

Du backend khong explicitly block past time o service, frontend nen chan o UI de tranh user schedule vo nghia.

### Update schedule form

Payload:

```ts
{
  integrationId?: string
  scheduledAt?: string
}
```

Rule:

- neu schedule `Completed`, disable edit tren UI
- neu schedule `Failed` hoac `Pending`, cho phep update
- co the cho update chi datetime, chi integration, hoac ca hai

### Delete schedule

Delete la soft delete.

Frontend:

- confirm truoc xoa
- sau xoa refresh list va upcoming section

### Upcoming section

`GET /content-schedules/upcoming?limit=10`

Khuyen nghi UI:

- 5 hoac 10 item upcoming trong panel rieng
- refresh sau create/update/delete

### Definition of Done

- Calendar page load duoc paged schedules
- Upcoming section load duoc
- Tao/sua/xoa schedule goi dung route
- Completed schedules khong cho edit trong UI

### Verify

- Tao 1 schedule moi
- Sua `scheduledAt`
- Sua `integrationId`
- Xoa schedule
- Kiem upcoming section refresh dung

## Task 7.3 - Tao schedule action tu content detail

### Muc tieu

- Cho user tao schedule ngay tu context content
- Khong bat user tu di qua page Calendar de tao moi schedule

### File can tao

```text
AISAM-FE/src/features/content/components/schedule-content-button.tsx
AISAM-FE/src/features/content/components/schedule-content-modal.tsx
AISAM-FE/src/features/schedules/hooks/use-content-schedule-action.ts
```

### Input context can co

Từ content detail/list item:

- `contentId`
- `brandId`

Frontend phai lay integrations theo brand:

```text
GET /api/social/integrations/brand/{brandId}
```

Chi hien integrations active cho user chon.

### Modal flow

1. mo modal tu content detail/list
2. load integrations cua brand
3. chon integration
4. chon datetime
5. submit `POST /content-schedules`

Payload:

```ts
{
  contentId,
  integrationId,
  scheduledAt
}
```

### UX guard can co

- neu content da `Published`, disable schedule button
- neu brand khong co integration active, disable schedule button va hien CTA sang Social Accounts

### Success flow

- dong modal
- refresh content detail neu can
- refresh upcoming schedules widget/page neu dang mount
- hien thong bao schedule created

### Definition of Done

- User tao schedule ngay tu content context duoc
- Integration list dung theo brand cua content
- Schedule button bi disable khi content da published hoac chua co integration active

### Verify

- Tao schedule tu content detail
- Tao schedule tu content list item neu co action
- Test content da published thi nut bi khoa

## Task 7.4 - Tao dev scheduler trigger panel

### Muc tieu

- Cho developer test worker publish due schedules trong moi truong development
- Khong lo panel nay xuat hien o production

### File can tao

```text
AISAM-FE/src/features/schedules/api/run-dev-scheduler.ts
AISAM-FE/src/features/schedules/components/dev-scheduler-panel.tsx
AISAM-FE/src/features/schedules/components/dev-scheduler-result.tsx
```

### Route backend

```text
POST /api/dev/scheduler/run-now
```

### Frontend visibility rule

Chi render panel neu:

- `appConfig.appEnv === "development"` hoac
- `appConfig.enableDevTools === true`

Khuyen nghi ket hop ca 2:

- production build khong render
- local/dev co the bat ro rang

### Response hien thi

`SchedulerRunResultDto`:

```ts
{
  scannedCount: number
  successCount: number
  failedCount: number
}
```

UI can hien:

- scanned count
- success count
- failed count

### Error handling

- neu backend tra `404 Not found.` do khong phai Development -> panel nen khong xuat hien ngay tu dau
- neu van goi nham va tra `404`, hien thong bao route khong available

### Definition of Done

- Dev panel chi hien trong development mode
- Run-now goi dung route
- Ket qua scanned/success/failed hien duoc
- Production khong render panel

### Verify

- Test local development mode
- Test turn off dev tools flag
- Test route 404 handling neu panel bi force render sai

## Verify tong Phase 7

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- notifications/schedules/dev-scheduler requests deu co `Authorization` va `X-Profile-Id`
- notification badge count dung
- mark-read va mark-all-read hoat dong
- schedule create/update/delete/upcoming hoat dong
- schedule action tu content detail hoat dong
- dev scheduler panel chi hien trong development

## Deliverable sau Phase 7

Can co it nhat:

```text
AISAM-FE/
  PHASE_7_IMPLEMENTATION.md
  src/
    app/
      (app)/
        notifications/
          page.tsx
        calendar/
          page.tsx
    features/
      notifications/
        api/
        components/
        hooks/
      schedules/
        api/
        components/
        hooks/
      content/
        components/
    types/
      notification.ts
      schedule.ts
```

## Risk can tranh trong Phase 7

- Quen gui `X-Profile-Id` cho notifications/schedules routes
- Gia dinh notifications API co support filter `isRead`, trong khi backend chua co
- Cho edit schedule da `Completed`
- Khong validate future datetime tren frontend khi tao schedule
- Khong refresh unread count sau mark-read/mark-all-read
- Khong refresh upcoming schedules sau create/update/delete
- Render dev scheduler panel o production
- Nhap tay integrationId ma khong rang buoc theo brand cua content

## Rule chuyen sang Phase 8

Chi bat dau Phase 8 khi:

- Phase 7 build pass
- notification center chay on dinh
- scheduling CRUD chay on dinh
- schedule action tu content context chay on dinh
- dev scheduler panel chi xuat hien o development
