# US-61 - Upload media qua storage service

## Mo ta

La nguoi dung, toi muon upload va quan ly file media de dung trong product va content thay vi chi tham chieu URL thu cong.

## Can cu tai lieu va codebase

Tai lieu da doi chieu:

- `docs/main/requirements.md`: he thong co external storage service de luu media va tai nguyen upload; media generation/publishing co the xu ly bat dong bo.
- `docs/archive/plans/backend-code-plan.md`: storage/Supabase khong nam trong MVP backend dau, chi cau hinh optional/future.
- `AISAM-BE/docs/superpowers/CODEBASE_UPDATE.md`: Phase H3 ghi ro Storage/Supabase upload la optional post-MVP, source cu gom `StorageController`, `SupabaseStorageService`, `BucketInitializerService`, `FileDto`.
- `AISAM-BE/docs/superpowers/CODEBASE.md`: profile avatar va product image DTO co file fields, nhung service hien reject upload; client nen dung URL fields cho den khi storage active.
- Active backend `AISAM-BE`: da co `Asset` entity/DbSet, nhung chua co `StorageController`/storage service active.

## Trang thai backend hien tai

Backend da co:

- `Asset` entity.
- `AssetTypeEnum`:

```ts
Video = 0
Image = 1
Audio = 2
Document = 3
Other = 4
```

- `DbSet<Asset>`.
- `CreateProfileRequest.AvatarFile`.
- `ProductCreateRequest.ImageFiles`.
- `CreateContentRequest.ImageUrl`.
- `CreateContentRequest.VideoUrl`.
- `Content.ImageUrl`, `Content.VideoUrl`.
- `Product.Images`.
- `Profile.AvatarUrl`.

Backend hien tai reject upload:

- `ProfileService.CreateProfileAsync` / `UpdateProfileAsync` tra loi:

```text
Avatar file upload is not enabled in the current MVP backend. Use AvatarUrl instead.
```

- `ProductService.CreateAsync` / `UpdateAsync` tra loi:

```text
Product image upload is not enabled in the current MVP backend.
```

Backend chua co active:

- `StorageController`
- `SupabaseStorageService`
- `BucketInitializerService`
- `IStorageService`
- Asset repository/service.
- API upload file.
- API list/delete media assets.
- Supabase config wired vao DI.
- Content/product/profile integration voi uploaded asset.

Ket luan: frontend co the chuan bi media library/upload UI, nhung upload action phai disabled hoac hien backend-not-ready state cho den khi backend Phase H3 active. Trong active flow hien tai, frontend phai tiep tuc dung `AvatarUrl`, `ImageUrl`, `VideoUrl` hoac URL list thay vi gui file.

## Muc tieu frontend

Tao UI upload va quan ly media de dung lai trong profile/product/content:

```text
/dashboard/media
/dashboard/media/upload
```

Nguoi dung co the:

- Upload image/video/document/audio neu backend support.
- Xem media library theo active profile/user.
- Chon media de gan vao content.
- Chon media image de gan vao product.
- Chon media image lam profile avatar.
- Xoa asset khong con dung.
- Copy public URL cua asset.

Trong luc backend chua active:

- Hien backend-not-ready state tren `/dashboard/media`.
- File input/upload button disabled.
- Profile/product/content forms khong gui `AvatarFile`/`ImageFiles`.
- Forms tiep tuc cho nhap URL thu cong.

## User flows

### Flow 1 - Upload image vao media library

1. User vao `/dashboard/media`.
2. Bam `Upload`.
3. Chon file image.
4. Frontend validate file type/size.
5. Frontend goi storage upload API neu backend active.
6. Backend upload file len storage provider, tao `Asset` record.
7. UI hien asset moi trong media library.

### Flow 2 - Chon media cho content

1. User tao/sua content.
2. Bam `Choose media`.
3. Media picker mo danh sach uploaded assets.
4. User chon image/video.
5. UI set `imageUrl` hoac `videoUrl` trong content form.
6. Submit content voi URL da chon.

### Flow 3 - Upload product images

1. User tao/sua product.
2. Bam `Choose/upload images`.
3. User chon mot hoac nhieu image assets.
4. UI gan asset URLs vao product images neu backend product image URL support.
5. Neu backend product upload chua active, UI khong gui `ImageFiles`.

### Flow 4 - Avatar upload

1. User tao/sua profile.
2. Bam `Upload avatar`.
3. User chon image.
4. Backend storage active thi upload va tra URL.
5. UI set `AvatarUrl`.
6. Profile submit voi `AvatarUrl`.

## Frontend scope

Pages/components can implement:

```text
/dashboard/media
/dashboard/media/upload
MediaLibraryPage
MediaUploadDropzone
MediaGrid
MediaListTable
MediaAssetCard
MediaPreviewDialog
MediaPickerDialog
MediaTypeFilter
MediaDeleteConfirmDialog
StorageBackendNotReadyState
```

Can update:

```text
Profile form
Product form
Content form
AI/content generator result UI
```

## Backend API du kien

Backend hien tai chua expose cac endpoint duoi day. Day la contract de frontend chuan bi cho Phase H3.

### Upload media

```http
POST /api/storage/upload
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
Content-Type: multipart/form-data
```

Form data:

```text
file: File
assetType: Image | Video | Audio | Document | Other
folder?: profile | product | content | media
metadata?: JSON string
```

Response:

```ts
ApiResponse<MediaAssetDto>
```

### Upload multiple media

```http
POST /api/storage/upload-multiple
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
Content-Type: multipart/form-data
```

Form data:

```text
files: File[]
assetType: Image
folder?: product | content | media
```

Response:

```ts
ApiResponse<MediaAssetDto[]>
```

### List media assets

```http
GET /api/storage/assets?page=1&pageSize=24&type=Image&searchTerm=abc
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<PagedResult<MediaAssetDto>>
```

### Get media asset detail

```http
GET /api/storage/assets/{assetId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<MediaAssetDto>
```

### Delete media asset

```http
DELETE /api/storage/assets/{assetId}
Authorization: Bearer <accessToken>
X-Profile-Id: <activeProfileId>
```

Response:

```ts
ApiResponse<boolean>
```

## API response types du kien

```ts
interface MediaAssetDto {
  id: string
  uploadedBy?: string
  assetType: 0 | 1 | 2 | 3 | 4
  storagePath: string
  publicUrl: string
  mimeType?: string
  sizeBytes?: number
  width?: number
  height?: number
  durationSeconds?: number
  metadata?: Record<string, unknown>
  createdAt: string
}
```

## API status handling

Frontend can xu ly:

- `200/201`: upload/list/delete thanh cong.
- `400`: file invalid, missing file, metadata invalid.
- `401`: token thieu/het han, redirect login.
- `403`: asset/profile khong thuoc user hoac storage permission bi cam.
- `404`: storage endpoint chua active hoac asset khong ton tai.
- `409`: duplicate path/name neu backend enforce.
- `413`: file qua lon.
- `415`: file type khong duoc ho tro.
- `503`: storage provider chua config hoac unavailable.
- `500`: loi he thong.

Khi endpoint storage tra `404` do backend chua active:

```text
Media upload API chua active trong backend hien tai.
```

Khi storage provider thieu config:

```text
Storage service chua duoc cau hinh.
```

## Business rules

- Upload media phai yeu cau JWT.
- Media phai scope theo active profile hoac uploaded user, tuy backend chot.
- File upload phai validate type va size o frontend va backend.
- Frontend khong gui `AvatarFile` hoac `ImageFiles` vao active backend hien tai.
- Khi storage active, upload xong frontend nen dung URL tra ve de set:
  - `Profile.AvatarUrl`
  - `Content.ImageUrl`
  - `Content.VideoUrl`
  - Product image URLs neu backend product DTO support.
- Delete asset can confirmation dialog.
- Khong cho delete asset dang duoc content/product/profile dung neu backend tra conflict.
- Khong luu secret storage key o frontend.
- Public URL/signed URL do backend tra ve; frontend khong tu build Supabase URL.

## UI requirements

### Media library page

Can co:

- Upload button/dropzone.
- Grid/list toggle.
- Filter by type: All, Image, Video, Audio, Document, Other.
- Search.
- Empty state.
- Loading state.
- Error/backend-not-ready state.

### Upload component

Can co:

- Drag and drop.
- File picker.
- Preview truoc khi upload.
- Progress state neu backend support.
- Per-file error.
- Supported file types label.

Suggested MVP limits de hien UI:

```text
Images: .jpg, .jpeg, .png, .webp
Videos: .mp4, .mov
Documents: .pdf
Max image size: 5 MB
Max video size: 50 MB
```

Backend se la source of truth cho limit thuc te.

### Media picker dialog

Dung trong:

- Profile avatar.
- Product images.
- Content image/video.

Can co:

- Type filter.
- Select one/multiple tuy context.
- Preview.
- Confirm selection.

### Backend not ready state

```text
Media upload chua active.
```

Mo ta phu:

```text
Backend can hoan thanh Phase H3 Storage/Supabase upload truoc khi bat upload file. Hien tai hay dung URL thu cong.
```

## Current active workaround

Cho den khi storage active:

- Profile form dung `AvatarUrl`.
- Content form dung `ImageUrl` va `VideoUrl`.
- Product form khong upload `ImageFiles`; neu UI can anh san pham, chi hien URL input neu backend product DTO support.
- Khong gui file multipart neu service hien reject upload.

## Acceptance criteria

- `/dashboard/media` co page rieng.
- Khi backend storage chua active, page hien backend-not-ready state va khong crash.
- Upload button disabled khi backend chua active.
- Profile form khong gui `AvatarFile` vao backend hien tai.
- Product form khong gui `ImageFiles` vao backend hien tai.
- Content form van cho dung `ImageUrl`/`VideoUrl`.
- Media picker co the render planned/disabled state trong Profile/Product/Content forms.
- Khi backend active, upload request dung `multipart/form-data`.
- Upload request co `Authorization` va `X-Profile-Id`.
- File type/size invalid duoc chan truoc khi upload.
- API `413` hien file too large.
- API `415` hien unsupported file type.
- API `503` hien storage config unavailable.
- Delete asset co confirmation.
- Chon asset trong media picker set dung URL vao form tuong ung.
- Khong luu Supabase key/secret o frontend.

## Suggested frontend types

```ts
export type AssetType = 0 | 1 | 2 | 3 | 4

export interface MediaAssetDto {
  id: string
  uploadedBy?: string
  assetType: AssetType
  storagePath: string
  publicUrl: string
  mimeType?: string
  sizeBytes?: number
  width?: number
  height?: number
  durationSeconds?: number
  metadata?: Record<string, unknown>
  createdAt: string
}

export interface UploadMediaRequest {
  file: File
  assetType: AssetType
  folder?: "profile" | "product" | "content" | "media"
  metadata?: Record<string, unknown>
}
```

## Suggested API client methods

```ts
export async function uploadMedia(payload: UploadMediaRequest) {
  const formData = new FormData()
  formData.append("file", payload.file)
  formData.append("assetType", String(payload.assetType))

  if (payload.folder) {
    formData.append("folder", payload.folder)
  }

  if (payload.metadata) {
    formData.append("metadata", JSON.stringify(payload.metadata))
  }

  return fetchWithAuth<ApiResponse<MediaAssetDto>>("/storage/upload", {
    method: "POST",
    body: formData,
  })
}

export async function getMediaAssets(query: {
  page: number
  pageSize: number
  type?: AssetType
  searchTerm?: string
}) {
  const params = new URLSearchParams()
  params.set("page", String(query.page))
  params.set("pageSize", String(query.pageSize))
  if (query.type !== undefined) params.set("type", String(query.type))
  if (query.searchTerm) params.set("searchTerm", query.searchTerm)

  return fetchWithAuth<ApiResponse<PagedResult<MediaAssetDto>>>(
    `/storage/assets?${params.toString()}`
  )
}
```

## Test cases frontend

- Vao `/dashboard/media` khi chua active profile thi hien profile guard.
- Backend `404` thi hien backend-not-ready state.
- Upload button disabled khi feature flag storage disabled.
- Chon file qua lon thi hien validation error va khong goi API.
- Chon file type khong support thi hien validation error va khong goi API.
- API `413` hien file too large.
- API `415` hien unsupported media type.
- API `503` hien storage unavailable.
- Upload success hien asset moi trong grid.
- Chon image asset cho content set `ImageUrl`.
- Chon video asset cho content set `VideoUrl`.
- Profile avatar form set `AvatarUrl`, khong gui `AvatarFile`.
- Product form khong gui `ImageFiles` khi backend storage chua active.

## Dependencies / blockers

- Backend can hoan thanh Phase H3 Storage/Supabase upload.
- Can migrate `StorageController`, `SupabaseStorageService`, `BucketInitializerService`, `FileDto`.
- Can cau hinh `SUPABASE_URL`, `SUPABASE_KEY` va bucket.
- Can chot asset ownership theo user hay profile.
- Can chot public URL hay signed URL.
- Can backend cap nhat Profile/Product/Content services de chap nhan uploaded asset URL hoac asset id.
- Can chot file size/type limits.
