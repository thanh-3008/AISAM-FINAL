# Phase 4 Implementation - AISAM Frontend

Tai lieu nay mo rong chi tiet cho cac task `4.1` den `4.4` trong [FRONTEND_CODE_PLAN.md](</c:/Users/Kietv/Downloads/To do list/AISAM-FINAL/AISAM-FE/FRONTEND_CODE_PLAN.md>), doi chieu truc tiep voi backend Brand/Product hien tai trong `AISAM-BE`.

Pham vi Phase 4:

- Hoan thien brand list, brand detail, brand create/update/delete/restore
- Hoan thien product list theo brand, product detail, product create/update/delete/restore
- Bám active profile context da co tu Phase 2 va app shell da co tu Phase 3
- Tao duoc flow quan ly Brand -> Product de Phase 5 dung tiep cho Content va AI
- Chuan bi san cho target product co media upload, du backend hien tai con gioi han contract upload that

Khong lam trong Phase 4:

- Content library
- AI generate/improve/chat
- Social integrations that
- Publish flow
- Notifications/Scheduling
- Payment/Team/Approval/Ads

Luu y target product:

- `requirement.md` xem product image la mot phan MVP.
- `README.md` xem storage/media upload da nam trong he thong.
- Vi vay Phase 4 khong duoc coi upload media la ngoai scope; no chi la `backend-partial` trong repo hien tai va phai duoc danh dau ro trong UX.

Can cu backend da doi chieu truc tiep cho Phase 4:

- `AISAM-BE/AISAM.API/Controllers/BrandController.cs`
- `AISAM-BE/AISAM.Services/Service/BrandService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/BrandRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateBrandRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateBrandRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/BrandResponseDto.cs`
- `AISAM-BE/AISAM.API/Controllers/ProductController.cs`
- `AISAM-BE/AISAM.Services/Service/ProductService.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ProductRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/ProductCreateRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/ProductUpdateRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ProductResponseDto.cs`
- `AISAM-BE/AISAM.Common/Dtos/PaginationDtos.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`

## Tong quan thu tu lam

1. Task 4.1 - Tao brand list page
2. Task 4.2 - Tao brand create/edit/detail
3. Task 4.3 - Tao product list page theo brand
4. Task 4.4 - Tao product create/edit/detail
5. Chay verify tong the Phase 4

## Contract backend Brand/Product can chot truoc khi code

### Route active - Brand

```text
GET    /api/brands?profileId=&page=&pageSize=&searchTerm=&sortBy=&sortDescending=&includeDeleted=
GET    /api/brands/{id}
POST   /api/brands
PUT    /api/brands/{id}
DELETE /api/brands/{id}
POST   /api/brands/{id}/restore
```

### Route active - Product

```text
GET    /api/products?brandId=&page=&pageSize=&searchTerm=&sortBy=&sortDescending=&includeDeleted=
GET    /api/products/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}
POST   /api/products/{id}/restore
```

### Header rule quan trong

Tat ca Brand/Product routes:

- can `Authorization`
- khong can `X-Profile-Id`

Brand/Product khong nam trong `ActiveProfileMiddleware`.

Frontend van phai dung `activeProfileId` de truyen vao query/body dung nghiep vu:

- Brand list can `profileId=activeProfileId`
- Brand create can `profileId=activeProfileId`
- Product list thuong can `brandId=<brandId>`
- Product create can `brandId=<selectedBrandId>`

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

Backend dung field `error`, khong phai `errors`.

### Paged result exact

```ts
type PagedResult<T> = {
  data: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}
```

### Brand response exact

```ts
type BrandResponseDto = {
  id: string
  userId: string
  name: string
  description?: string | null
  logoUrl?: string | null
  slogan?: string | null
  usp?: string | null
  targetAudience?: string | null
  profileId?: string | null
  createdAt: string
  updatedAt: string
  productsCount: number
  contentsCount: number
}
```

### Product response exact

```ts
type ProductResponseDto = {
  id: string
  brandId: string
  name: string
  description?: string | null
  price?: number | null
  images?: string[] | null
  createdAt: string
  updatedAt: string
}
```

Luu y:

- `images` la danh sach string URL, nhung backend MVP product create/update hien khong upload file
- `images` co the rong `[]`

### Brand request exact

Create:

```ts
type CreateBrandRequest = {
  name: string
  description?: string
  logoUrl?: string
  slogan?: string
  usp?: string
  targetAudience?: string
  profileId?: string
}
```

Update:

```ts
type UpdateBrandRequest = {
  name?: string
  description?: string
  logoUrl?: string
  slogan?: string
  usp?: string
  targetAudience?: string
  profileId?: string
}
```

Brand requests la JSON, khong phai multipart.

### Product request exact

Create:

```ts
type ProductCreateRequest = {
  brandId: string
  name: string
  description?: string
  price?: number
  imageFiles?: File[]
}
```

Update:

```ts
type ProductUpdateRequest = {
  brandId?: string
  name?: string
  description?: string
  price?: number
  imageFiles?: File[]
}
```

Product requests la `multipart/form-data`.

### Validation frontend nen bam

Brand:

- `name`: required khi create, max 255
- `description`: max 2000
- `logoUrl`: max 500
- `slogan`: max 255
- `profileId`: frontend phai tu cap, user khong nhap tay

Product create:

- `brandId`: required
- `name`: required, max 255
- `description`: max 2000
- `price`: optional, neu co thi phai la number hop le

Product update:

- partial update, nhung validate field neu co gia tri

### Soft delete behavior can biet

Brand:

- delete set `IsDeleted = true`
- restore set `IsDeleted = false`
- `GET /brands` mac dinh bo qua deleted
- `includeDeleted=true` tra ca active va deleted vi repository chi bo filter `!IsDeleted`

Product:

- delete set `IsDeleted = true`
- restore set `IsDeleted = false`
- `GET /products` mac dinh bo qua deleted
- `includeDeleted=true` tra ca active va deleted

Luu y quan trong:

- `includeDeleted=true` khong co nghia chi lay deleted items
- no co nghia la khong loc deleted ra nua

Frontend tab/filter phai ro diem nay, tranh mong doi behavior giong `profiles?isDeleted=true`.

### Sort behavior backend that

Brand sort support:

- `name`
- `createdAt`
- mac dinh `createdAt DESC`

Product sort support:

- `name`
- `price`
- `createdAt`
- mac dinh `createdAt DESC`

Frontend khong nen dua option sort ma backend khong ho tro.

### Search behavior backend that

Brand search tren:

- `name`
- `description`

Product search tren:

- `name`
- `description`

### Ownership behavior can biet

Brand list/create:

- backend check `profileId` thuoc user dang login

Brand detail/update/delete/restore:

- backend check brand -> profile -> user ownership

Product list:

- neu co `brandId`, backend check ownership cua brand truoc
- neu khong co `brandId`, backend fetch danh sach roi chi giu product ma `product.Brand.Profile.UserId == currentUser`

Product detail/update/delete/restore:

- backend check `product.Brand.Profile.UserId == currentUser`

Frontend van nen restrict UI theo active profile de user khong bi nham workspace.

### MVP upload rule can chot

Product backend accept DTO co `ImageFiles`, nhung `ProductService` hien tai reject neu gui file:

```text
Product image upload is not enabled in the current MVP backend.
```

Phase 4 can:

- gui `multipart/form-data`
- nhung khong submit `imageFiles`
- co the an uploader file hoac hien disabled control `Image upload planned`

## Task 4.1 - Tao brand list page

### Muc tieu

- Hien danh sach brand theo active profile
- Cho search, paging, sort co ban, include deleted toggle

### File can tao

```text
AISAM-FE/src/app/(app)/brands/page.tsx
AISAM-FE/src/features/brands/api/get-brands.ts
AISAM-FE/src/features/brands/components/brand-list.tsx
AISAM-FE/src/features/brands/components/brand-filters.tsx
AISAM-FE/src/features/brands/components/brand-list-item.tsx
AISAM-FE/src/features/brands/components/brand-list-toolbar.tsx
AISAM-FE/src/features/brands/components/brand-empty-state.tsx
AISAM-FE/src/features/brands/components/brand-error-state.tsx
AISAM-FE/src/features/brands/hooks/use-brands-query.ts
AISAM-FE/src/types/brand.ts
```

### API helper can co

```ts
type GetBrandsParams = {
  profileId: string
  page?: number
  pageSize?: number
  searchTerm?: string
  sortBy?: "name" | "createdAt"
  sortDescending?: boolean
  includeDeleted?: boolean
}
```

`get-brands.ts`

```ts
export async function getBrands(params: GetBrandsParams) {
  return api.get<PagedResult<BrandResponseDto>>(
    endpoints.brands.list(params),
    {
      requireAuth: true,
      skipProfileHeader: true,
    },
  )
}
```

### Query rule can chot

Brand list bat buoc truyen:

```text
profileId = activeProfileId
```

Neu `activeProfileId` null:

- khong goi API
- redirect/empty state da duoc shell guard xu ly

### UI table/list can co

It nhat nen hien:

- name
- slogan
- productsCount
- contentsCount
- updatedAt
- deleted state neu `includeDeleted=true`

CTA:

- view detail
- edit
- delete
- restore neu item deleted

### Filter UX khuyen nghi

- search input
- sort select:
  - Newest
  - Oldest
  - Name A-Z
  - Name Z-A
- toggle `Show deleted`

Do `includeDeleted=true` tra ca active va deleted, frontend nen:

- van hien 1 list chung
- item deleted co badge ro rang
- restore action chi xuat hien tren item deleted

### Loading/empty/error state

- loading: table/list skeleton
- empty khong co brand: CTA `Create brand`
- empty sau search/filter: thong bao khong co ket qua
- error: retry button

### Definition of Done

- Query dung `profileId=activeProfileId`
- Search, paging, sort dung contract backend
- Toggle `includeDeleted` hoat dong dung nghia
- Khong goi API khi chua co active profile

### Verify

- Test profile moi chua co brand
- Test tao nhieu brand va paging
- Test search theo name/description
- Test `includeDeleted=false` va `includeDeleted=true`

## Task 4.2 - Tao brand create/edit/detail

### Muc tieu

- Hoan thien CRUD brand kit
- Dat duoc trang detail lam diem di tiep sang products, contents, social integrations

### File can tao

```text
AISAM-FE/src/app/(app)/brands/[id]/page.tsx
AISAM-FE/src/app/(app)/brands/new/page.tsx
AISAM-FE/src/features/brands/api/get-brand-by-id.ts
AISAM-FE/src/features/brands/api/create-brand.ts
AISAM-FE/src/features/brands/api/update-brand.ts
AISAM-FE/src/features/brands/api/delete-brand.ts
AISAM-FE/src/features/brands/api/restore-brand.ts
AISAM-FE/src/features/brands/components/brand-form.tsx
AISAM-FE/src/features/brands/components/brand-detail.tsx
AISAM-FE/src/features/brands/components/brand-actions.tsx
AISAM-FE/src/features/brands/schemas/brand-create-schema.ts
AISAM-FE/src/features/brands/schemas/brand-update-schema.ts
```

### Phan A - Create brand

Route backend:

```text
POST /api/brands
Content-Type: application/json
```

Payload can submit:

```ts
{
  name: string
  description?: string
  logoUrl?: string
  slogan?: string
  usp?: string
  targetAudience?: string
  profileId: activeProfileId
}
```

Rule quan trong:

- frontend phai tu inject `profileId = activeProfileId`
- khong cho user doi profileId trong form

Validation:

- `name` required, max 255
- `description` max 2000
- `logoUrl` max 500
- `slogan` max 255

Success flow:

1. tao brand thanh cong
2. refresh brand list cache neu co
3. redirect ve `/brands/[id]` hoac detail panel

Khuyen nghi:

```text
/brands/[id]
```

### Phan B - Brand detail

Route backend:

```text
GET /api/brands/{id}
```

Detail page nen hien:

- name
- description
- logoUrl
- slogan
- usp
- targetAudience
- productsCount
- contentsCount
- createdAt
- updatedAt

CTA lien quan:

- Edit brand
- View products
- View contents
- View social integrations placeholder/path
- Delete/Restore

### Phan C - Update brand

Route backend:

```text
PUT /api/brands/{id}
Content-Type: application/json
```

Payload:

```ts
{
  name?: string
  description?: string
  logoUrl?: string
  slogan?: string
  usp?: string
  targetAudience?: string
  profileId?: string
}
```

Luu y backend service:

- service hien tai khong su dung `request.ProfileId` de doi profile
- update chu yeu cap nhat metadata

Frontend khuyen nghi:

- khong dua field `profileId` vao form edit
- neu co can di kem theo payload, giu nguyen active profile id hoac bo qua

Rule update:

- neu field string can xoa, frontend co the gui chuoi rong `""`
- backend set field khi request field `!= null`

### Phan D - Delete/restore brand

Delete:

```text
DELETE /api/brands/{id}
```

Restore:

```text
POST /api/brands/{id}/restore
```

Behavior can biet:

- delete la soft delete
- restore fail neu brand chua bi xoa

Frontend:

- confirm truoc khi delete
- neu dang o detail page cua brand vua delete:
  - co the redirect ve `/brands?includeDeleted=true`
  - hoac ve `/brands` va show toast

Khuyen nghi don gian:

```text
/brands
```

Sau restore:

- refetch detail/list
- co the navigate lai detail item

### Definition of Done

- Create/update dung JSON contract
- Detail page hien dung metadata va counts
- Delete/restore dung route
- Detail page co CTA sang products va contents

### Verify

- Tao 1 brand moi
- Sua slogan/usp/targetAudience
- Xoa brand va xem lai qua `includeDeleted=true`
- Restore brand

## Task 4.3 - Tao product list page theo brand

### Muc tieu

- Hien va loc product cua 1 brand
- Dat context san sang cho content creation ve sau

### File can tao

```text
AISAM-FE/src/app/(app)/brands/[id]/products/page.tsx
AISAM-FE/src/features/products/api/get-products.ts
AISAM-FE/src/features/products/components/product-list.tsx
AISAM-FE/src/features/products/components/product-filters.tsx
AISAM-FE/src/features/products/components/product-list-item.tsx
AISAM-FE/src/features/products/components/product-empty-state.tsx
AISAM-FE/src/features/products/components/product-error-state.tsx
AISAM-FE/src/features/products/hooks/use-products-query.ts
AISAM-FE/src/types/product.ts
```

### API helper can co

```ts
type GetProductsParams = {
  brandId?: string
  page?: number
  pageSize?: number
  searchTerm?: string
  sortBy?: "name" | "price" | "createdAt"
  sortDescending?: boolean
  includeDeleted?: boolean
}
```

Brand detail context:

- route `/brands/[id]/products` bat buoc filter `brandId = params.id`

### Query rule can chot

Frontend nen uu tien list theo brand detail:

```text
GET /api/products?brandId=<brandId>
```

Ly do:

- user dang o context cua 1 brand
- backend check ownership cua brand ro rang hon

Khong can build all-products-global page trong Phase 4 neu roadmap chua yeu cau.

### UI list can co

- name
- price
- updatedAt
- deleted state neu `includeDeleted=true`
- image count hoac first image placeholder neu co

CTA:

- view detail
- edit
- delete
- restore neu deleted

### Filter UX

- search input
- sort select:
  - Newest
  - Oldest
  - Name A-Z
  - Name Z-A
  - Price low-high
  - Price high-low
- toggle `Show deleted`

### Empty state

- brand chua co product: CTA `Create product`
- search/filter khong co ket qua: thong bao khong tim thay

### Definition of Done

- Filter dung `brandId`
- Search/sort/paging dung contract backend
- `includeDeleted=true` hien ca active va deleted
- Error state ro rang neu brand khong thuoc user hoac khong ton tai

### Verify

- Test 1 brand chua co product
- Test 1 brand co nhieu product
- Test sort `price`
- Test search theo name/description

## Task 4.4 - Tao product create/edit/detail

### Muc tieu

- Hoan thien CRUD product
- Giu contract multipart nhung khong lam upload image that o MVP

### File can tao

```text
AISAM-FE/src/app/(app)/products/[id]/page.tsx
AISAM-FE/src/app/(app)/brands/[id]/products/new/page.tsx
AISAM-FE/src/features/products/api/get-product-by-id.ts
AISAM-FE/src/features/products/api/create-product.ts
AISAM-FE/src/features/products/api/update-product.ts
AISAM-FE/src/features/products/api/delete-product.ts
AISAM-FE/src/features/products/api/restore-product.ts
AISAM-FE/src/features/products/components/product-form.tsx
AISAM-FE/src/features/products/components/product-detail.tsx
AISAM-FE/src/features/products/components/product-actions.tsx
AISAM-FE/src/features/products/schemas/product-create-schema.ts
AISAM-FE/src/features/products/schemas/product-update-schema.ts
AISAM-FE/src/features/products/lib/product-form-data.ts
```

### Phan A - Create product

Route backend:

```text
POST /api/products
Content-Type: multipart/form-data
```

Payload frontend nghiep vu:

```ts
{
  brandId: string
  name: string
  description?: string
  price?: number
}
```

Khong submit:

```text
imageFiles
```

Ly do:

- backend reject product image upload trong MVP

Can tao helper:

```ts
export function toCreateProductFormData(input: ProductCreateFormValues): FormData
```

Rule:

- `brandId` append bat buoc
- `name` append bat buoc
- `description` append neu co
- `price` append neu co
- khong append `imageFiles`

### UI form can co

- name
- description
- price
- brand context readonly neu tao trong `/brands/[id]/products/new`
- image upload control disabled hoac hidden

Khuyen nghi:

- neu route da nam trong context brand, khong can cho doi brand
- brand field chi hien thong tin brand hien tai

### Validation frontend

- `name` required, max 255
- `description` max 2000
- `price` optional, neu co thi `>= 0` hoac rule team chot, nhung khong gui chuoi khong parse duoc

### Phan B - Product detail

Route backend:

```text
GET /api/products/{id}
```

Detail page nen hien:

- name
- description
- price
- image list neu co
- createdAt
- updatedAt
- brand context

Luu y:

- `images` hien tai co the la `[]`
- khong can fake gallery neu backend chua co image upload

CTA:

- Edit
- Delete/Restore
- Tao content cho product nay o Phase 5

### Phan C - Update product

Route backend:

```text
PUT /api/products/{id}
Content-Type: multipart/form-data
```

Payload:

```ts
{
  brandId?: string
  name?: string
  description?: string
  price?: number
}
```

Service behavior can biet:

- cho phep doi `brandId` neu user so huu brand moi
- reject `imageFiles` neu co file

Frontend khuyen nghi cho MVP:

- neu product edit dang o context 1 brand, giu brand do
- neu muon cho doi brand, chi cho chon trong danh sach brands cua active profile

Neu team chua can move product sang brand khac, co the:

- khong expose field doi brand trong Phase 4

### Phan D - Delete/restore product

Delete:

```text
DELETE /api/products/{id}
```

Restore:

```text
POST /api/products/{id}/restore
```

Behavior:

- delete la soft delete
- restore fail neu product chua bi xoa

Frontend:

- confirm truoc delete
- sau delete tu detail:
  - redirect ve `/brands/[brandId]/products`
- sau restore:
  - refetch detail/list

### Phan E - Error handling can ro

- `Brand not found`: co the xay ra khi tao product voi brand stale
- `You are not allowed to access this brand`: user sai ownership hoac stale route
- `Product not found`: detail stale hoac item da deleted va dang xem bang route active
- `Product image upload is not enabled in the current MVP backend.`: hien ro, khong swallow

### Definition of Done

- Create/update submit dung multipart contract
- Khong gui `imageFiles` trong MVP
- Detail page hien dung data product
- Delete/restore hoat dong
- Route sau action giu dung context brand

### Verify

- Tao product moi cho 1 brand
- Sua name/description/price
- Xoa product va xem lai qua `includeDeleted=true`
- Restore product
- Thu bat buoc image upload UI khong duoc goi file upload that

## Verify tong Phase 4

Sau khi xong tat ca task, chay:

```text
cd AISAM-FE
pnpm install
pnpm lint
pnpm build
```

Smoke can dat:

- `/brands` la protected route trong app shell
- brand list truyen `profileId=activeProfileId`
- create/update brand dung JSON
- `/brands/[id]/products` truyen `brandId=<id>`
- create/update product dung `multipart/form-data`
- frontend khong gui `X-Profile-Id` cho brand/product APIs
- frontend khong gui `imageFiles` that
- soft delete/restore brand/product hoat dong

## Deliverable sau Phase 4

Can co it nhat:

```text
AISAM-FE/
  PHASE_4_IMPLEMENTATION.md
  src/
    app/
      (app)/
        brands/
          page.tsx
          new/
          [id]/
            page.tsx
            products/
              page.tsx
              new/
        products/
          [id]/
            page.tsx
    features/
      brands/
        api/
        components/
        hooks/
        schemas/
      products/
        api/
        components/
        hooks/
        schemas/
        lib/
    types/
      brand.ts
      product.ts
```

## Risk can tranh trong Phase 4

- Quen truyen `profileId=activeProfileId` cho brand list/create
- Gia dinh brand/product APIs can `X-Profile-Id`
- Gui JSON thay vi `multipart/form-data` cho product create/update
- Hieu sai `includeDeleted=true` la chi lay deleted items
- Expose option sort ma backend khong support
- Render uploader image that va gui `imageFiles`, dan den backend tra loi
- Cho user sua `profileId`/`brandId` tuy y ma khong rang buoc theo context
- Redirect sau delete lam mat context brand hien tai

## Rule chuyen sang Phase 5

Chi bat dau Phase 5 khi:

- Phase 4 build pass
- brand CRUD chay on dinh
- product CRUD chay on dinh
- flow Brand -> Product ro rang trong app shell
- active profile context va brand context san sang de content form dung lai
