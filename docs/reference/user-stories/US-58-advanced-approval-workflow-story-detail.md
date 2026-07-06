# US-58 - Approval workflow nang cao

## Mo ta

La nguoi duyet noi dung, toi muon co quy trinh pending, approve, reject va feedback chinh thuc truoc khi publish.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `docs/main/requirements.md`: muc `4.2 Team Leader / Approver`, `5.2 Team Leader / Approver Use Cases`, `6.7 Content Review and Approval`, va business rule "Chi content da duoc approve moi duoc len lich hoac dang bai".
- `docs/archive/plans/backend-code-plan.md`: Approval workflow khong nam trong active MVP hien tai, cac task backend tap trung Auth/Profile/Brand/Product/Content/AI/Social/Scheduling truoc.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase H1 ghi ro Approval va Team permission nang cao la post-MVP optional; can chot ai duoc approve/publish va workflow team truoc khi migrate.
- Active backend codebase `AISAM-BE`: co entity `Approval`, enum `ContentStatusEnum`, status count tren dashboard, nhung chua co `ApprovalController`, `ApprovalService`, `ApprovalRepository`.

## Trang thai backend hien tai

Backend da co:

- `Content` entity co `Status`.
- `Approval` entity/schema.
- `ContentStatusEnum`:

```ts
Draft = 0
PendingApproval = 1
Approved = 2
Rejected = 3
Published = 4
```

- `DashboardSummaryDto.PendingApprovalContentCount`.
- Content list filter theo status:

```http
GET /api/content?status=1
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

- AI generation approve endpoint:

```http
POST /api/ai/approve/{aiGenerationId}
```

Luu y quan trong: endpoint AI approve chi ap dung ket qua AI generation vao content, khong phai approval workflow chinh thuc cua nguoi duyet.

Backend chua co active:

- `ApprovalController`
- `ApprovalService`
- `ApprovalRepository`
- Endpoint submit content for approval
- Endpoint approval queue
- Endpoint approve/reject content chinh thuc
- Endpoint feedback/revision history
- Team leader/approver permission model ro rang
- Notification approval-needed tu workflow chinh thuc

Ket luan: frontend co the chuan bi UI/route/type cho approval workflow, nhung cac action submit/approve/reject/feedback phai disabled hoac hien backend-not-ready state cho den khi backend Phase H1 active.

## Muc tieu frontend

Tao workflow UI cho review/approval noi dung:

```text
/dashboard/approvals
/dashboard/contents/{contentId}/review
```

Nguoi duyet co the:

- Xem hang doi noi dung dang cho duyet.
- Xem chi tiet content can duyet.
- Xem brand/product context.
- Approve content de cho phep publish/schedule.
- Reject content kem feedback.
- Xem lich su approval/feedback.

Nguoi tao content co the:

- Gui content draft vao approval.
- Xem trang thai approval.
- Xem feedback khi bi reject.
- Sua content va gui lai.

## User flows

### Flow 1 - Submit for approval

1. User tao hoac sua content draft.
2. User bam `Submit for approval`.
3. UI hien confirmation.
4. Frontend goi API submit approval neu backend active.
5. Content status doi tu `Draft` sang `PendingApproval`.
6. Approver thay content trong approval queue.

### Flow 2 - Approver approve content

1. Approver vao `/dashboard/approvals`.
2. UI list content `PendingApproval`.
3. Approver mo detail.
4. Approver bam `Approve`.
5. UI hien confirmation.
6. Frontend goi API approve.
7. Content status doi sang `Approved`.
8. Content co the publish/schedule.

### Flow 3 - Approver reject content with feedback

1. Approver mo content `PendingApproval`.
2. Approver nhap feedback bat buoc.
3. Approver bam `Reject`.
4. Frontend goi API reject.
5. Content status doi sang `Rejected`.
6. Nguoi tao content thay feedback va sua lai.

### Flow 4 - Resubmit after rejection

1. Creator mo content `Rejected`.
2. Creator sua content theo feedback.
3. Creator bam `Submit again`.
4. Content status quay ve `PendingApproval`.
5. Approval history giu lai lan reject truoc.

## Frontend scope

Pages/components can implement:

```text
/dashboard/approvals
/dashboard/contents/[contentId]/review
ApprovalQueuePage
ApprovalQueueTable
ApprovalStatusBadge
ApprovalDetailPanel
ApprovalActionBar
ApproveDialog
RejectWithFeedbackDialog
ApprovalHistoryTimeline
SubmitForApprovalButton
BackendNotReadyState
```

Can update cac page content hien co:

```text
Content detail
Content card
Content publish button
Content schedule action
Dashboard summary
Notifications panel
```

## Backend API du kien

Backend hien tai chua expose cac endpoint duoi day. Day la contract de frontend chuan bi va de backend Phase H1 implement.

### Submit content for approval

```http
POST /api/approvals/content/{contentId}/submit
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "note": "Please review this campaign copy."
}
```

Response:

```ts
ApiResponse<ApprovalDetail>
```

### Approval queue

```http
GET /api/approvals?page=1&pageSize=10&status=1&brandId={brandId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<PagedResult<ApprovalListItem>>
```

### Approval detail

```http
GET /api/approvals/{approvalId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<ApprovalDetail>
```

### Approve content

```http
POST /api/approvals/{approvalId}/approve
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "note": "Approved for publishing."
}
```

Response:

```ts
ApiResponse<ApprovalDetail>
```

### Reject content

```http
POST /api/approvals/{approvalId}/reject
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "feedback": "Please make the CTA clearer and reduce the promotional tone."
}
```

Response:

```ts
ApiResponse<ApprovalDetail>
```

### Approval history by content

```http
GET /api/approvals/content/{contentId}/history
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<ApprovalHistoryItem[]>
```

## API response types du kien

```ts
interface ApiResponse<T> {
  success: boolean
  message: string
  statusCode: number
  data: T
  errors?: unknown
}

interface ApprovalListItem {
  id: string
  contentId: string
  contentTitle?: string
  brandId: string
  brandName?: string
  approverUserId: string
  approverName?: string
  status: 1 | 2 | 3
  notes?: string
  createdAt: string
  approvedAt?: string
}

interface ApprovalDetail extends ApprovalListItem {
  content: {
    id: string
    title?: string
    textContent: string
    imageUrl?: string
    videoUrl?: string
    status: 0 | 1 | 2 | 3 | 4
    createdAt: string
    updatedAt: string
  }
  feedbackHistory: ApprovalHistoryItem[]
}

interface ApprovalHistoryItem {
  id: string
  approvalId: string
  actorUserId: string
  actorName?: string
  action: "Submitted" | "Approved" | "Rejected" | "Resubmitted"
  note?: string
  createdAt: string
}
```

## API status handling

Frontend can xu ly:

- `200`: render/update approval state thanh cong.
- `400`: validation error, vi du reject thieu feedback.
- `401`: token thieu/het han, redirect login.
- `403`: user khong co quyen approve/reject.
- `404`: endpoint chua active hoac approval/content khong ton tai.
- `409`: invalid transition, vi du approve content da Published hoac reject content khong o `PendingApproval`.
- `500`: loi he thong, hien retry state.

Khi API approval tra `404` do backend chua active, UI hien:

```text
Approval workflow API chua active trong backend hien tai.
```

## Business rules

- Chi content thuoc active profile moi duoc submit/review.
- Chi content `Draft` hoac `Rejected` moi duoc submit for approval.
- Chi content `PendingApproval` moi duoc approve/reject.
- Content `Approved` moi duoc publish/schedule theo requirement.
- Reject phai co feedback bat buoc.
- Approve co note optional.
- Moi lan submit/reject/approve phai duoc ghi vao approval history.
- Non-approver khong duoc approve/reject.
- Creator khong nen approve content cua chinh minh neu team policy yeu cau separation of duties.
- Trong backend hien tai Team/Approver role chua ro, nen frontend phai disable approval action neu khong co permission metadata.
- Khi backend chua co Team/Approval policy, UI chi nen hien planned state hoac read-only queue tu content status filter.

## UI requirements

### Approval queue

Bang/cac card toi thieu:

- Content title
- Brand
- Status
- Submitted at
- Submitter/owner neu backend support
- Approver neu assigned
- Actions

Filters:

- Status: Pending, Approved, Rejected
- Brand
- Search text
- Page/pageSize

### Approval detail

Can hien:

- Content preview
- Brand/product context
- Current status
- Submitted note
- Feedback history
- Approve button
- Reject button

### Submit for approval button

Hien tren content detail/card khi:

- Content status la `Draft` hoac `Rejected`.
- Backend approval API active.
- User co quyen submit.

Disable/hide khi:

- Content status la `PendingApproval`, `Approved`, `Published`.
- Backend approval API chua active.

### Approve/reject actions

Approve:

- Confirmation dialog.
- Note optional.

Reject:

- Feedback textarea required.
- Character limit neu backend quy dinh.

### Backend not ready state

```text
Approval workflow chua active.
```

Mo ta phu:

```text
Backend can hoan thanh Phase H1 Approval/Team permission truoc khi bat submit, approve va reject chinh thuc.
```

## Acceptance criteria

- `/dashboard/approvals` co route/page rieng.
- Khi backend chua active, page hien backend-not-ready state va khong crash.
- UI phan biet ro AI generation approve voi content approval workflow.
- Content status badge map dung `Draft`, `PendingApproval`, `Approved`, `Rejected`, `Published`.
- Approval queue co loading, empty, error va backend-not-ready states.
- Submit for approval button chi enabled cho `Draft`/`Rejected` khi backend active.
- Approve/reject buttons chi enabled cho `PendingApproval` khi user co permission.
- Reject feedback la required.
- Sau approve thanh cong, content status hien `Approved`.
- Sau reject thanh cong, content status hien `Rejected` va feedback hien trong history.
- Publish/schedule actions nen bi disable neu content chua `Approved` khi backend enforce approval workflow.
- `401` redirect login.
- `403` hien permission error.
- `409` hien invalid transition message.
- Khong goi `/api/approvals/*` trong active production flow neu backend chua active.

## Suggested frontend types

```ts
export type ContentStatus = 0 | 1 | 2 | 3 | 4
export type ApprovalStatus = 1 | 2 | 3

export interface SubmitApprovalRequest {
  note?: string
}

export interface ApproveContentRequest {
  note?: string
}

export interface RejectContentRequest {
  feedback: string
}

export interface ApprovalListItem {
  id: string
  contentId: string
  contentTitle?: string
  brandId: string
  brandName?: string
  approverUserId: string
  approverName?: string
  status: ApprovalStatus
  notes?: string
  createdAt: string
  approvedAt?: string
}

export interface ApprovalHistoryItem {
  id: string
  approvalId: string
  actorUserId: string
  actorName?: string
  action: "Submitted" | "Approved" | "Rejected" | "Resubmitted"
  note?: string
  createdAt: string
}
```

## Suggested API client methods

```ts
export async function submitContentForApproval(
  contentId: string,
  payload: SubmitApprovalRequest
) {
  return fetchWithAuth<ApiResponse<ApprovalDetail>>(
    `/approvals/content/${contentId}/submit`,
    { method: "POST", body: JSON.stringify(payload) }
  )
}

export async function getApprovalQueue(query: {
  page: number
  pageSize: number
  status?: ApprovalStatus
  brandId?: string
}) {
  const params = new URLSearchParams()
  params.set("page", String(query.page))
  params.set("pageSize", String(query.pageSize))
  if (query.status) params.set("status", String(query.status))
  if (query.brandId) params.set("brandId", query.brandId)

  return fetchWithAuth<ApiResponse<PagedResult<ApprovalListItem>>>(
    `/approvals?${params.toString()}`
  )
}

export async function approveContent(approvalId: string, payload: ApproveContentRequest) {
  return fetchWithAuth<ApiResponse<ApprovalDetail>>(
    `/approvals/${approvalId}/approve`,
    { method: "POST", body: JSON.stringify(payload) }
  )
}

export async function rejectContent(approvalId: string, payload: RejectContentRequest) {
  return fetchWithAuth<ApiResponse<ApprovalDetail>>(
    `/approvals/${approvalId}/reject`,
    { method: "POST", body: JSON.stringify(payload) }
  )
}
```

## Test cases frontend

- Vao `/dashboard/approvals` khi chua co active profile thi hien/select profile guard.
- Backend `404` thi hien backend-not-ready state.
- Queue empty hien empty state.
- Status filter `PendingApproval` gui query status `1`.
- Content `Draft` hien submit button neu API active.
- Content `PendingApproval` khong cho edit destructive neu workflow lock duoc bat.
- Reject dialog khong submit khi feedback rong.
- Approve action co confirmation.
- API approve `200` refresh content va queue.
- API reject `200` refresh content va history.
- API `403` hien permission error.
- API `409` hien invalid transition error.

## Dependencies / blockers

- Backend can hoan thanh Phase H1 Approval va Team permission nang cao.
- Can migrate `ApprovalController`, `ApprovalService`, `ApprovalRepository`.
- Can chot ai duoc approve/publish.
- Can chot Team co leader/approver role nhu the nao, vi `TeamMemberRoleEnum` hien chi co `Copywriter`, `Designer`, `Marketer`.
- Can chot creator co duoc approve content cua chinh minh khong.
- Can chot approval co SLA/escalation hay khong.
- Can backend enforce rule content phai `Approved` moi duoc publish/schedule neu workflow duoc bat.
