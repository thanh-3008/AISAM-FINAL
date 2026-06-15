# Phase 2 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `2.1` den `2.3` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu truc tiep voi backend profile hien tai trong `AISAM-BE`.

Pham vi Phase 2:

- Hoan thien profile workspace flow cho user da dang nhap
- Co active profile context de cap `X-Profile-Id` cho cac phase sau
- Dung duoc onboarding, create profile, profile switcher, profile detail, edit, soft delete, restore
- Chot redirect sau login theo trang thai profile cua user
- Mang theo subscription context co san trong profile state de phuc vu target product

Khong lam trong Phase 2:

- Dashboard summary widgets
- Brand/Product/Content/Social/Notifications/Scheduling pages
- Payment/Subscription UI that
- Team/Approval/Ads

Luu y target product:

- `README.md` va `requirement.md` dat subscription/quota la module core.
- Vi vay Phase 2 khong duoc bo qua `subscriptionId` trong profile state; can giu no de phuc vu pricing/payment/quota phases sau.

Can cu backend da doi chieu truc tiep cho Phase 2:

- `AISAM-BE/AISAM.API/Controllers/ProfileController.cs`
- `AISAM-BE/AISAM.Services/Service/ProfileService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ProfileRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateProfileRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateProfileRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ProfileResponseDto.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`
- `AISAM-BE/AISAM.Data/Enumeration/ProfileTypeEnum.cs`
- `AISAM-BE/AISAM.Data/Enumeration/ProfileStatusEnum.cs`
- `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`

## Tong quan thu tu lam

1. Task 2.1 - Tao profile store va active profile context
2. Task 2.2 - Tao onboarding va create profile page
3. Task 2.3 - Tao profile switcher va profile detail
4. Chay verify tong the Phase 2

## Contract backend profile can chot truoc khi code

### Route active

Tat ca route profile deu can `Authorization`, nhung khong can `X-Profile-Id`:

```text
GET    /api/profiles/user/{userId}?search=&isDeleted=
GET    /api/profiles/{id}
POST   /api/profiles/user/{userId}
PUT    /api/profiles/{id}
DELETE /api/profiles/{id}
PATCH  /api/profiles/{id}/restore
```

### Rule access quan trong

Backend tu kiem tra user dang login co phai owner cua profile hay khong:

- `GET /profiles/user/{userId}` tra `403` neu `userId` khac user trong token
- `POST /profiles/user/{userId}` tra `403` neu co gang tao profile cho user khac
- `GET`, `PUT`, `DELETE`, `PATCH restore` tra `404` neu profile khong ton tai hoac khong thuoc user

Frontend khong duoc cho user nhap tu do `userId` de tao/sua profile. Luon lay `userId` tu auth session hoac `/auth/me`.

### Envelope response

Tat ca route profile deu bam `GenericResponse<T>`:

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

Luu y:

- field loi la `error`
- delete/restore tra `GenericResponse<boolean>`
- list profile khong tra paging object, ma tra `IEnumerable<ProfileResponseDto>`

### Profile response exact

```ts
type ProfileResponseDto = {
  id: string
  userId: string
  name: string
  profileType: 0 | 1 | 2
  subscriptionId?: string | null
  companyName?: string | null
  bio?: string | null
  avatarUrl?: string | null
  status: 0 | 1 | 2 | 3
  createdAt: string
  updatedAt: string
  isOwner: boolean
  memberRole?: string | null
}
```

### Enum can map

`ProfileTypeEnum`:

```ts
const profileTypeValues = {
  Free: 0,
  Basic: 1,
  Pro: 2,
} as const
```

`ProfileStatusEnum`:

```ts
const profileStatusValues = {
  Pending: 0,
  Active: 1,
  Suspended: 2,
  Cancelled: 3,
} as const
```

Rule quan trong:

- frontend co the hien label user-friendly
- nhung payload gui len backend van phai la enum number

### Request DTO exact theo backend

Create:

```ts
type CreateProfileRequest = {
  name: string
  profileType: 0 | 1 | 2
  companyName?: string
  bio?: string
  avatarUrl?: string
  avatarFile?: File
}
```

Update:

```ts
type UpdateProfileRequest = {
  name?: string
  profileType?: 0 | 1 | 2
  companyName?: string
  bio?: string
  avatarUrl?: string
  avatarFile?: File
}
```

### Validation frontend nen bam

Theo DataAnnotations hien tai:

- `name`: required khi create, max 255
- `profileType`: required khi create
- `companyName`: max 255
- `bio`: max 1000
- `avatarUrl`: max 500

Update cho phep partial, nhung frontend van nen validate neu field co gia tri.

### Multipart rule can chot

`POST /profiles/user/{userId}` va `PUT /profiles/{id}` deu:

```text
Consumes: multipart/form-data
```

Frontend phai gui `FormData`, ke ca khi khong upload file.

Tuy nhien backend `ProfileService` hien tai reject `AvatarFile`:

```text
Avatar file upload is not enabled in the current MVP backend. Use AvatarUrl instead.
```

Vi vay Phase 2 can:

- dung `multipart/form-data`
- chi submit `AvatarUrl`
- khong render input upload file active trong UI MVP

### Soft delete behavior can biet

Delete profile la xoa mem:

- repository set `Status = Cancelled`
- restore set `Status = Pending`

`GET /profiles/user/{userId}` behavior:

- mac dinh chi tra profile `Status != Cancelled`
- `?isDeleted=true` chi tra profile `Cancelled`
- `?isDeleted=false` chi tra profile chua bi xoa mem

Khong co route tra ca hai nhom cung luc trong mot call.

### Search behavior can biet

`search` duoc tim tren:

- `Name`
- `CompanyName`
- `Bio`

Ket qua order:

- `CreatedAt DESC`

### Rule active profile va middleware

Profile endpoints khong can `X-Profile-Id`, nhung Phase 2 phai tao duoc active profile storage cho cac phase sau.

Protected prefixes can `X-Profile-Id` tu `ActiveProfileMiddleware`:

```text
/api/content
/api/content-schedules
/api/dashboard
/api/dev/scheduler
/api/ai
/api/conversations
/api/social-auth
/api/social
/api/posts
/api/notifications
```

Can chot ngay:

- active profile duoc luu rieng trong browser storage
- request chi them `X-Profile-Id` cho nhom route can context
- profile APIs khong phu thuoc active profile de tranh deadlock onboarding

## Task 2.1 - Tao profile store va active profile context

### Muc tieu

- Tao state trung tam cho danh sach profile cua user
- Persist `activeProfileId`
- Cap API cho onboarding, profile switcher, route guard va cac feature phase sau

### File can tao

```text
AISAM-FE/src/providers/profile-provider.tsx
AISAM-FE/src/hooks/use-profile.ts
AISAM-FE/src/lib/profile/active-profile-storage.ts
AISAM-FE/src/lib/profile/profile-guards.ts
AISAM-FE/src/features/profile/api/get-user-profiles.ts
AISAM-FE/src/features/profile/api/get-profile-by-id.ts
AISAM-FE/src/features/profile/api/create-profile.ts
AISAM-FE/src/features/profile/api/update-profile.ts
AISAM-FE/src/features/profile/api/delete-profile.ts
AISAM-FE/src/features/profile/api/restore-profile.ts
AISAM-FE/src/types/profile.ts
AISAM-FE/src/constants/profile-enums.ts
```

Neu Phase 0 da co `active-profile-storage.ts`, task nay bo sung implementation that.

### API layer can co

`get-user-profiles.ts`

```ts
export async function getUserProfiles(params: {
  userId: string
  search?: string
  isDeleted?: boolean
}) {
  return api.get<ProfileResponseDto[]>(
    endpoints.profiles.byUser(params.userId, params.search, params.isDeleted),
    {
      requireAuth: true,
      skipProfileHeader: true,
    },
  )
}
```

`create-profile.ts`

```ts
export async function createProfile(userId: string, input: CreateProfileFormValues) {
  const formData = toCreateProfileFormData(input)
  return api.post<ProfileResponseDto>(endpoints.profiles.create(userId), formData, {
    requireAuth: true,
    skipProfileHeader: true,
  })
}
```

`update-profile.ts`

```ts
export async function updateProfile(id: string, input: UpdateProfileFormValues) {
  const formData = toUpdateProfileFormData(input)
  return api.put<ProfileResponseDto>(endpoints.profiles.update(id), formData, {
    requireAuth: true,
    skipProfileHeader: true,
  })
}
```

### Profile context contract

`profile-provider.tsx` nen expose it nhat:

```ts
type ProfileContextValue = {
  profiles: ProfileResponseDto[]
  deletedProfiles: ProfileResponseDto[]
  activeProfileId: string | null
  activeProfile: ProfileResponseDto | null
  isBootstrapping: boolean
  isLoadingProfiles: boolean
  hasProfiles: boolean
  setActiveProfile: (profileId: string | null) => void
  refreshProfiles: (options?: { includeDeleted?: boolean }) => Promise<void>
  reloadActiveProfile: () => Promise<ProfileResponseDto | null>
  createProfile: (input: CreateProfileFormValues) => Promise<ProfileResponseDto>
  updateProfile: (id: string, input: UpdateProfileFormValues) => Promise<ProfileResponseDto>
  deleteProfile: (id: string) => Promise<void>
  restoreProfile: (id: string) => Promise<void>
  clearActiveProfile: () => void
}
```

### State rule can chot

- `profiles`: danh sach profile active, mac dinh `isDeleted=false`
- `deletedProfiles`: chi fetch khi can, khong bat buoc load ngay khi app mount
- `activeProfileId`: doc/ghi browser storage
- `activeProfile`: find tu `profiles`, neu khong co thi fallback fetch `GET /profiles/{id}` neu phu hop

### Active profile bootstrapping flow

Khi provider mount:

1. Dam bao auth da bootstrap xong
2. Neu chua login:
   - clear state profile
   - clear active profile storage
3. Neu da login:
   - goi `GET /profiles/user/{currentUserId}`
   - doc `activeProfileId` trong storage
   - neu `activeProfileId` ton tai va nam trong list profile active, giu lai
   - neu `activeProfileId` khong ton tai hoac da bi deleted, auto chon profile dau tien trong list
   - neu list rong, de `activeProfileId = null`

### Quy tac auto-chon active profile

Khuyen nghi don gian cho MVP:

- neu user chi co 1 profile active: auto set profile do
- neu user co nhieu profile active:
  - neu storage co id hop le thi giu
  - neu khong, auto set profile dau tien theo `createdAt DESC` do backend tra ve

Khong can bat user chon tay neu da co 1 profile ro rang.

### Storage helper can co

```ts
const ACTIVE_PROFILE_STORAGE_KEY = "aisam.active-profile-id"

export function getActiveProfileId(): string | null
export function setActiveProfileId(profileId: string): void
export function clearActiveProfileId(): void
export function hasActiveProfile(): boolean
```

Rule implementation:

- SSR-safe
- neu key la chuoi rong hoac parse bat thuong thi clear

### Route guard can tao

`profile-guards.ts` nen co:

```ts
export function requireActiveProfile(options?: {
  isAuthenticated: boolean
  activeProfileId: string | null
  redirectTo?: string
}): void
```

Hoac helper:

```ts
export function getPostLoginRoute(hasProfiles: boolean, hasActiveProfile: boolean): string
```

Phase 2 can doi redirect sau login dang tam dung o `/account`:

- neu user chua co profile: vao `/onboarding`
- neu user da co profile: vao `/dashboard` o Phase 3
- trong luc Phase 3 chua xong, co the redirect tam ve `/onboarding` hoac `/profiles/{id}`

Tai lieu nay chot logic nghiep vu, implementation route cu the co the follow roadmap:

```text
Phase 2 xong, post-login redirect uu tien:
1. /onboarding neu khong co profile
2. /dashboard neu da co active profile va Phase 3 da san sang
3. /profiles/{id} neu team chua mo dashboard shell
```

### FormData mapper can co

Can tao helper:

```ts
export function toCreateProfileFormData(input: CreateProfileFormValues): FormData
export function toUpdateProfileFormData(input: UpdateProfileFormValues): FormData
```

Rule:

- append field chi khi co gia tri
- `profileType` append bang `String(number)`
- khong append `avatarFile` trong MVP
- neu user xoa `avatarUrl`, can quyet convention:
  - don gian nhat la cho update bang chuoi rong `""`

### Definition of Done

- Provider wrap duoc app sau auth provider
- `useProfile()` doc duoc profiles va active profile
- Active profile persist qua reload
- Profile APIs khong gui `X-Profile-Id`
- Co helper guard san sang cho route can active profile

### Verify

- Login xong reload van doc duoc `activeProfileId`
- Clear key active profile khong lam app crash
- Active profile da deleted thi provider tu clear/chon lai profile hop le

## Task 2.2 - Tao onboarding va create profile page

### Muc tieu

- Cho user moi tao profile dau tien
- Chot redirect sau login khi user chua co workspace

### File can tao

```text
AISAM-FE/src/app/onboarding/page.tsx
AISAM-FE/src/app/profiles/new/page.tsx
AISAM-FE/src/features/profile/components/profile-create-form.tsx
AISAM-FE/src/features/profile/components/profile-empty-state.tsx
AISAM-FE/src/features/profile/schemas/profile-create-schema.ts
AISAM-FE/src/features/profile/lib/profile-form-data.ts
```

### Route access

- `/onboarding`: protected route
- `/profiles/new`: protected route

Khong can active profile moi vao duoc 2 route nay.

### UX flow can chot

#### Flow 1 - User moi sau login chua co profile

1. Auth xac nhan da login
2. Profile provider load `GET /profiles/user/{userId}`
3. Neu list rong:
   - redirect den `/onboarding`
4. `/onboarding` hien ly do phai tao profile
5. CTA den `/profiles/new`

#### Flow 2 - User da co profile roi ma vao `/onboarding`

- redirect ra khoi onboarding
- dich den:
  - `/dashboard` neu Phase 3 da xong
  - hoac `/profiles/{activeProfileId}`

### Request can bam

Route:

```text
POST /api/profiles/user/{userId}
Content-Type: multipart/form-data
```

Field can submit:

```ts
{
  name: string
  profileType: 0 | 1 | 2
  companyName?: string
  bio?: string
  avatarUrl?: string
}
```

Khong submit:

```text
avatarFile
```

Ly do: backend MVP reject file upload.

### UI form can co

- `name`
- `profileType`
- `companyName`
- `bio`
- `avatarUrl`
- submit button

Khong render uploader file trong MVP. Neu can, chi hien note `Avatar upload planned` va disable control.

### Validation frontend

- `name` required, max 255
- `profileType` required
- `companyName` max 255
- `bio` max 1000
- `avatarUrl` max 500
- `avatarUrl` neu co thi nen validate format URL hop le o client

### Profile type UX

Backend chi can number enum. Frontend nen hien label:

- `Free`
- `Basic`
- `Pro`

Khong can build logic payment/subscription theo tung type trong Phase 2. Day moi la metadata cho profile.

### Hanh vi submit

1. Submit form
2. Goi `profile.createProfile(values)`
3. Neu thanh cong:
   - refresh profile list
   - auto set `activeProfileId = createdProfile.id`
   - redirect:
     - `/dashboard` neu Phase 3 da san sang
     - tam thoi `/profiles/{id}`

4. Neu that bai:
   - neu server tra loi upload disabled do gui nham `AvatarFile`, hien ro message backend
   - neu validation/server loi, hien `error.errorMessage`

### Definition of Done

- User chua co profile vao duoc onboarding
- Create profile submit dung multipart contract
- Tao profile xong auto set active profile
- User da co profile khong bi mac ket o onboarding

### Verify

- Test account moi chua co profile
- Tao profile `Free`
- Tao profile voi `avatarUrl`
- Thu submit voi field vuot max length o client

## Task 2.3 - Tao profile switcher va profile detail

### Muc tieu

- Cho user chuyen workspace
- Quan ly profile da tao: xem, sua, delete, restore

### File can tao

```text
AISAM-FE/src/features/profile/components/profile-switcher.tsx
AISAM-FE/src/app/profiles/[id]/page.tsx
AISAM-FE/src/features/profile/components/profile-detail.tsx
AISAM-FE/src/features/profile/components/profile-form.tsx
AISAM-FE/src/features/profile/components/profile-actions.tsx
AISAM-FE/src/features/profile/components/deleted-profile-list.tsx
AISAM-FE/src/features/profile/schemas/profile-update-schema.ts
```

Neu team muon co route list rieng, co the them sau:

```text
AISAM-FE/src/app/profiles/page.tsx
```

Nhung khong bat buoc trong Phase 2 neu switcher + detail la du.

### Phan A - Profile switcher

`profile-switcher.tsx` nen hien:

- danh sach profile active
- ten profile
- company name neu co
- profile type label
- CTA tao profile moi

Hanh vi:

1. User chon profile
2. Goi `setActiveProfile(profile.id)`
3. Update storage ngay
4. Tu phase sau, request `dashboard/content/...` se co `X-Profile-Id` moi

Khong can goi API rieng khi doi active profile neu profile da co trong list local.

### Phan B - Profile detail

Route:

```text
GET /api/profiles/{id}
```

Page detail nen hien:

- name
- companyName
- profileType
- status
- bio
- avatarUrl
- createdAt
- updatedAt
- owner/member info co ban

Luu y:

- `memberRole` hien tai co the `null`
- `isOwner` tu backend profile service luon `true` cho owner flow hien tai

### Phan C - Update profile

Route:

```text
PUT /api/profiles/{id}
Content-Type: multipart/form-data
```

Payload partial:

```ts
{
  name?: string
  profileType?: 0 | 1 | 2
  companyName?: string
  bio?: string
  avatarUrl?: string
}
```

Rule update can chot:

- form prefill tu `ProfileResponseDto`
- submit bang `FormData`
- chi append field user da sua neu team muon toi uu patch-like behavior
- hoac append day du field editable cung duoc, backend van chap nhan

Khong submit `avatarFile`.

### Phan D - Delete profile

Route:

```text
DELETE /api/profiles/{id}
```

Behavior backend:

- soft delete, `status = Cancelled`
- profile mac dinh se bien mat khoi list active

Frontend rule:

- bat buoc confirm truoc khi xoa
- neu xoa profile dang active:
  - refresh active profile list
  - neu con profile khac thi auto switch sang profile dau tien
  - neu khong con profile nao thi clear `activeProfileId` va redirect `/onboarding`

### Phan E - Restore profile

Route:

```text
PATCH /api/profiles/{id}/restore
```

Behavior backend:

- restore tra `true`
- status profile tro ve `Pending`

Frontend flow:

1. Co view danh sach deleted profiles, fetch bang `GET /profiles/user/{userId}?isDeleted=true`
2. User bam restore
3. Refresh `deletedProfiles`
4. Refresh `profiles`
5. Co the dua vao active profile neu hien tai dang null

### Phan F - Search va deleted filters

Can co it nhat cho profile management:

- search theo `name/companyName/bio`
- tab hoac toggle:
  - `Active`
  - `Deleted`

Do backend khong tra ca hai nhom trong 1 call, frontend nen fetch theo tab hien tai, khong merge ad hoc.

### Error handling can ro

- `403` o `GET /profiles/user/{userId}`: frontend nen coi la bug flow, force refetch current user/session thay vi cho user nhap lai
- `404` o `GET /profiles/{id}`: hien "Profile not found or no longer available"
- loi upload disabled: hien nguyen van thong diep backend

### Definition of Done

- Chuyen active profile hoat dong va persist
- Detail page load duoc profile cua owner
- Update profile submit dung multipart contract
- Delete la soft delete
- Restore profile hoat dong qua `isDeleted=true`

### Verify

- Tao 2 profile, doi qua lai bang switcher
- Sua profile name/bio/avatarUrl
- Xoa active profile
- Khoi phuc profile da xoa
- Test mo detail voi id khong ton tai

## Verify tong Phase 2

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- login xong, user khong co profile thi vao `/onboarding`
- tao profile xong co `activeProfileId`
- reload page van giu duoc active profile hop le
- profile APIs chi gui `Authorization`, khong phu thuoc `X-Profile-Id`
- deleted profile khong xuat hien trong list mac dinh
- view deleted profile qua `isDeleted=true` hoat dong
- restore xong profile quay lai list active

## Deliverable sau Phase 2

Can co it nhat:

```text
AISAM-FE/
  PHASE_2_IMPLEMENTATION.md
  src/
    app/
      onboarding/
      profiles/
        new/
        [id]/
    providers/
      profile-provider.tsx
    hooks/
      use-profile.ts
    lib/
      profile/
        active-profile-storage.ts
        profile-guards.ts
    features/
      profile/
        api/
        components/
        schemas/
        lib/
    types/
      profile.ts
    constants/
      profile-enums.ts
```

## Risk can tranh trong Phase 2

- Goi profile API ma lai ep can `X-Profile-Id`
- Gui JSON thay vi `multipart/form-data` cho create/update profile
- Render uploader file that va submit `AvatarFile`, dan den backend tra loi
- Tin rang delete la hard delete
- Giu `activeProfileId` tro den profile da bi xoa mem
- Merge active va deleted profiles trong 1 state ma khong ro filter nguon
- Dung string union cho `profileType` payload thay vi enum number
- Redirect sau login van co dinh o `/account`, bo qua onboarding flow

## Rule chuyen sang Phase 3

Chi bat dau Phase 3 khi:

- Phase 2 build pass
- onboarding/create profile flow chay on dinh
- profile switcher hoat dong
- active profile persist qua reload
- delete/restore profile chay dung
- route guard cho feature can active profile da san sang de dashboard dung lai
