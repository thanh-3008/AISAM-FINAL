# US-59 - Team va phan quyen team

> Superseding target: Workspace Membership thay cho Team governance cu. Role: Owner, Manager, Content Creator, Viewer; Business Plus toi da 10 members, Business Pro toi da 50; Owner transfer chi cho Manager.

## Mo ta

La nguoi quan ly, toi muon tao team, gan thanh vien va phan quyen theo vai tro de to chuc cong viec theo nhom.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `docs/main/requirements.md`: co vai tro Team Leader / Approver, approval workflow, team governance sau MVP.
- `docs/archive/plans/backend-code-plan.md`: Team/approval workflow duoc de sau vi phuc tap; Profile/Brand hien tai chua ho tro shared profile qua team.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase H1 ghi ro Team va Approval permission nang cao la post-MVP optional; can chot nghiep vu leader/member/approval flow.
- Active backend `AISAM-BE`: da co entity/schema `Team`, `TeamMember`, `TeamBrand`, nhung chua co `TeamController`, `TeamMemberController`, `TeamService`, `TeamMemberService`, `TeamRepository`.

## Trang thai backend hien tai

Backend da co:

- `Team` entity.
- `TeamMember` entity.
- `TeamBrand` entity.
- `TeamStatusEnum`:

```ts
Active = 0
Inactive = 1
Archived = 2
```

- `TeamMemberRoleEnum`:

```ts
Copywriter = 0
Designer = 1
Marketer = 2
```

- `TeamMember.Role` dang la string.
- `TeamMember.Permissions` dang la JSON list string.

Backend chua co active:

- `TeamController`
- `TeamMemberController`
- `TeamService`
- `TeamMemberService`
- `TeamRepository`
- `TeamMemberRepository`
- `TeamBrandRepository`
- API tao/sua/xoa team.
- API invite/add/remove member.
- API gan brand cho team.
- API kiem tra permission theo team.
- Shared profile/brand access qua team.

Ket luan: frontend co the chuan bi route, UI, types va planned state. Khong duoc goi API team that trong active production flow cho den khi backend Phase H1 active.

## Muc tieu frontend

Tao UI quan ly team trong workspace/profile:

```text
/dashboard/teams
/dashboard/teams/new
/dashboard/teams/{teamId}
/dashboard/teams/{teamId}/members
/dashboard/teams/{teamId}/brands
```

Nguoi quan ly co the:

- Xem danh sach team trong active profile.
- Tao team moi.
- Sua ten/mo ta/status team.
- Them thanh vien bang email.
- Gan vai tro cho thanh vien.
- Gan brand cho team.
- Kich hoat/vo hieu hoa thanh vien.
- Xem permission cua tung vai tro.

Trong luc backend chua active:

- UI hien planned/backend-not-ready state.
- Cac action create/update/delete/invite phai disabled.
- Khong goi `/api/teams/*`.

## User flows

### Flow 1 - Tao team

1. Manager vao `/dashboard/teams`.
2. Bam `Create team`.
3. Nhap name, description.
4. Chon brands team se phu trach neu backend support.
5. Submit.
6. Backend tao team trong active profile.
7. UI redirect sang team detail.

### Flow 2 - Them thanh vien

1. Manager mo team detail.
2. Vao tab Members.
3. Nhap email thanh vien.
4. Chon role: Copywriter, Designer, Marketer.
5. Chon permissions neu backend support.
6. Submit invite/add member.
7. UI hien member trong list voi status active/pending.

### Flow 3 - Cap nhat role/permission

1. Manager mo member row.
2. Doi role hoac permissions.
3. Confirm.
4. Backend cap nhat member permission.
5. UI refresh list.

### Flow 4 - Gan brand cho team

1. Manager mo tab Brands.
2. Chon brand trong active profile.
3. Gan brand vao team.
4. Team member co quyen thao tac tren brand theo permissions.

## Frontend scope

Pages/components can implement:

```text
/dashboard/teams
/dashboard/teams/new
/dashboard/teams/[teamId]
TeamsPage
TeamCreateForm
TeamDetailPage
TeamMembersTable
TeamMemberInviteDialog
TeamMemberRoleSelect
TeamPermissionsMatrix
TeamBrandsPanel
TeamStatusBadge
BackendNotReadyState
```

Can cap nhat navigation:

```text
Dashboard sidebar -> Teams
```

Neu backend chua active, sidebar link co the hien badge:

```text
Planned
```

## Backend API du kien

Backend hien tai chua expose cac endpoint duoi day. Day la contract de frontend chuan bi cho Phase H1.

### Team list

```http
GET /api/teams?page=1&pageSize=10&status=0
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<PagedResult<TeamListItem>>
```

### Team detail

```http
GET /api/teams/{teamId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<TeamDetail>
```

### Create team

```http
POST /api/teams
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "name": "Marketing Team",
  "description": "Team phu trach noi dung Facebook"
}
```

Response:

```ts
ApiResponse<TeamDetail>
```

### Update team

```http
PUT /api/teams/{teamId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "name": "Marketing Team",
  "description": "Updated description",
  "status": 0
}
```

### Archive/delete team

```http
DELETE /api/teams/{teamId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

### Add/invite member

```http
POST /api/teams/{teamId}/members
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "email": "member@example.com",
  "role": "Copywriter",
  "permissions": ["content:create", "content:update", "approval:submit"]
}
```

### Update member role/permissions

```http
PUT /api/teams/{teamId}/members/{memberId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "role": "Marketer",
  "permissions": ["content:create", "content:update", "content:publish"]
}
```

### Remove/deactivate member

```http
DELETE /api/teams/{teamId}/members/{memberId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

### Assign brands to team

```http
POST /api/teams/{teamId}/brands
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Request:

```json
{
  "brandIds": ["brand-guid-1", "brand-guid-2"]
}
```

### Remove brand from team

```http
DELETE /api/teams/{teamId}/brands/{brandId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

## API response types du kien

```ts
interface TeamListItem {
  id: string
  profileId: string
  name: string
  description?: string
  status: 0 | 1 | 2
  memberCount: number
  brandCount: number
  createdAt: string
  updatedAt?: string
}

interface TeamDetail extends TeamListItem {
  members: TeamMemberItem[]
  brands: TeamBrandItem[]
}

interface TeamMemberItem {
  id: string
  teamId: string
  userId: string
  email: string
  fullName?: string
  role: "Copywriter" | "Designer" | "Marketer" | string
  permissions: string[]
  joinedAt: string
  isActive: boolean
}

interface TeamBrandItem {
  id: string
  teamId: string
  brandId: string
  brandName: string
  assignedAt: string
  isActive: boolean
}
```

## Permission model de frontend chuan bi

Backend chua chot permission model. Frontend nen dung permission string de linh hoat:

```ts
type TeamPermission =
  | "brand:view"
  | "brand:update"
  | "product:view"
  | "product:update"
  | "content:view"
  | "content:create"
  | "content:update"
  | "content:delete"
  | "approval:submit"
  | "approval:approve"
  | "approval:reject"
  | "content:publish"
  | "schedule:create"
  | "schedule:update"
  | "social:manage"
```

Default permission suggestions:

| Role | Permissions |
| --- | --- |
| Copywriter | `content:view`, `content:create`, `content:update`, `approval:submit` |
| Designer | `brand:view`, `product:view`, `content:view`, `content:update` |
| Marketer | `content:view`, `approval:submit`, `content:publish`, `schedule:create`, `schedule:update` |

Can chot sau:

- Co role `Leader` hay `Manager` khong.
- Team co bat buoc 1 leader duy nhat khong.
- Ai duoc approve/reject.
- Ai duoc publish/schedule.

## API status handling

Frontend can xu ly:

- `200`: render/update thanh cong.
- `201`: create team/member thanh cong.
- `400`: validation error.
- `401`: token thieu/het han, redirect login.
- `403`: user khong co quyen quan ly team.
- `404`: endpoint chua active hoac team/member/brand khong ton tai.
- `409`: duplicate member, brand da gan, invalid status transition.
- `500`: loi he thong, retry state.

Khi backend chua active va tra `404`, UI hien:

```text
Team management API chua active trong backend hien tai.
```

## Business rules

- Moi team thuoc mot active profile.
- Team member phai la user hop le trong he thong hoac duoc invite qua email neu backend support.
- Mot user khong nen bi add duplicate vao cung mot team.
- Chi owner/manager co permission team manage moi duoc tao/sua/xoa team.
- Chi manager co permission moi duoc add/remove member.
- Brand gan vao team phai thuoc active profile.
- Team archived khong cho add member/brand moi.
- Team inactive/archived khong nen cap quyen moi cho member.
- Shared profile/brand access qua team chua active trong backend hien tai; frontend khong nen coi team membership la quyen that cho den khi backend enforce.
- Frontend guard chi la UX; backend phai enforce permission.

## UI requirements

### Team list

Cot/card toi thieu:

- Team name
- Description
- Status
- Member count
- Brand count
- Created at
- Actions

Filters:

- Status: Active, Inactive, Archived
- Search by name

### Team detail

Tabs:

- Overview
- Members
- Brands
- Permissions

### Member table

Cot:

- Name/email
- Role
- Permissions summary
- Joined at
- Active status
- Actions

Actions:

- Edit role
- Edit permissions
- Deactivate/remove

### Permissions matrix

Rows:

- Role/member

Columns:

- Brand
- Product
- Content
- Approval
- Publishing
- Scheduling
- Social

Neu backend chua active, matrix read-only hoac planned state.

### Backend not ready state

```text
Team management chua active.
```

Mo ta phu:

```text
Backend can hoan thanh Phase H1 Team/Approval permission truoc khi bat tao team, moi thanh vien va phan quyen.
```

## Acceptance criteria

- `/dashboard/teams` co page rieng.
- Khi chua co active profile, khong goi API team va hien profile guard.
- Khi backend chua active, page hien backend-not-ready state va khong crash.
- Team route/page khong goi `/api/teams/*` trong active production flow neu backend chua active.
- UI co layout cho team list, team detail, members, brands va permissions.
- Role labels map dung `Copywriter`, `Designer`, `Marketer`.
- Team status labels map dung `Active`, `Inactive`, `Archived`.
- Create/edit/add member/remove member actions disabled khi backend chua active.
- Khi backend active, create team gui `X-Profile-Id`.
- Add member validate email required.
- Add member validate role required.
- Duplicate member API `409` hien message ro.
- `401` redirect login.
- `403` hien permission error.
- `404` hien not found/backend-not-ready tuy ngu canh.
- Khong hien team permissions nhu security source of truth neu backend chua enforce.

## Suggested frontend types

```ts
export type TeamStatus = 0 | 1 | 2
export type TeamRole = "Copywriter" | "Designer" | "Marketer" | string

export interface TeamListItem {
  id: string
  profileId: string
  name: string
  description?: string
  status: TeamStatus
  memberCount: number
  brandCount: number
  createdAt: string
  updatedAt?: string
}

export interface TeamMemberItem {
  id: string
  teamId: string
  userId: string
  email: string
  fullName?: string
  role: TeamRole
  permissions: string[]
  joinedAt: string
  isActive: boolean
}

export interface CreateTeamRequest {
  name: string
  description?: string
}

export interface AddTeamMemberRequest {
  email: string
  role: TeamRole
  permissions: string[]
}
```

## Suggested API client methods

```ts
export async function getTeams(query: {
  page: number
  pageSize: number
  status?: TeamStatus
  searchTerm?: string
}) {
  const params = new URLSearchParams()
  params.set("page", String(query.page))
  params.set("pageSize", String(query.pageSize))
  if (query.status !== undefined) params.set("status", String(query.status))
  if (query.searchTerm) params.set("searchTerm", query.searchTerm)

  return fetchWithAuth<ApiResponse<PagedResult<TeamListItem>>>(
    `/teams?${params.toString()}`
  )
}

export async function createTeam(payload: CreateTeamRequest) {
  return fetchWithAuth<ApiResponse<TeamDetail>>("/teams", {
    method: "POST",
    body: JSON.stringify(payload),
  })
}

export async function addTeamMember(teamId: string, payload: AddTeamMemberRequest) {
  return fetchWithAuth<ApiResponse<TeamMemberItem>>(`/teams/${teamId}/members`, {
    method: "POST",
    body: JSON.stringify(payload),
  })
}
```

## Test cases frontend

- Vao `/dashboard/teams` khi chua active profile thi hien profile guard.
- Backend `404` thi hien backend-not-ready state.
- Team list empty hien empty state.
- Role select hien `Copywriter`, `Designer`, `Marketer`.
- Create team disabled khi backend chua active.
- Add member form validate email.
- Add member form validate role.
- API `409` duplicate member hien loi ro.
- API `403` hien permission error.
- Team archived disable add member/brand.
- Permission matrix render read-only neu backend chua active.

## Dependencies / blockers

- Backend can hoan thanh Phase H1 Team/Approval permission.
- Can migrate `TeamController`, `TeamMemberController`, `TeamService`, `TeamMemberService`.
- Can migrate `TeamRepository`, `TeamMemberRepository`, `TeamBrandRepository`.
- Can chot role `Leader/Manager` co can them vao enum/model khong.
- Can chot team co bat buoc mot leader duy nhat khong.
- Can chot permission strings va enforcement policy.
- Can backend enforce shared profile/brand permission truoc khi frontend coi team permission la quyen that.
