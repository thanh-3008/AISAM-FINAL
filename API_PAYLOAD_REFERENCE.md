# 📦 AISAM — API Payload Reference (Sinh từ Backend Source Code)

> Tài liệu này được sinh trực tiếp từ source code `AISAM-BE`. Tất cả các field, type, và enum đều chính xác 100% từ backend.

---

## 🔑 CƠ CHẾ AUTH CHUNG

### Headers bắt buộc

```
Authorization: Bearer <accessToken>     // Tất cả API cần auth
X-Profile-Id: <profileId (GUID)>        // Bắt buộc cho: content, ai, social, posts, notifications, payment, quota, dashboard, content-schedules, conversations
```

> **LƯU Ý QUAN TRỌNG:** Header `X-Profile-Id` được xác thực bởi `ActiveProfileMiddleware`. Nếu thiếu hoặc sai profile → `401/403`.

### Cấu trúc Response chung

```json
{
  "success": true | false,
  "message": "...",
  "statusCode": 200 | 400 | 401 | 403 | 404 | 500,
  "data": { ... },
  "error": {
    "errorCode": null,
    "errorMessage": "...",
    "validationErrors": {}
  },
  "timestamp": "2026-06-04T10:00:00Z"
}
```

---

## 1️⃣ AUTH (Xác thực)

### [US-01] Đăng ký
`POST /api/auth/register`

**Request Body (JSON):**
```json
{
  "email": "user@example.com",       // required, valid email
  "password": "Password123!",        // required, minLength: 8
  "confirmPassword": "Password123!", // required, phải khớp password
  "fullName": "Nguyen Van A"         // optional, maxLength: 255
}
```

**Response Data (`TokenResponse`):**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "abc123...",
  "expiresAt": "2026-06-04T11:00:00Z",
  "tokenType": "Bearer",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "fullName": "Nguyen Van A",
    "role": 0,                      // UserRoleEnum: 0=User, 1=Admin
    "isEmailVerified": false,
    "createdAt": "2026-06-04T10:00:00Z",
    "lastLoginAt": null
  }
}
```

---

### [US-02] Đăng nhập
`POST /api/auth/login`

**Request Body (JSON):**
```json
{
  "email": "user@example.com",   // required
  "password": "Password123!"     // required
}
```

**Response Data:** Same `TokenResponse` as above.

---

### [US-11] Đăng nhập Google
`POST /api/auth/google`

**Request Body (JSON):**
```json
{
  "idToken": "google-id-token-string"  // required — lấy từ Google OAuth flow phía FE
}
```

**Response Data:** Same `TokenResponse` as above.

---

### [US-03] Refresh Token
`POST /api/auth/refresh`

**Request Body (JSON):**
```json
{
  "refreshToken": "abc123..."  // required
}
```

**Response Data:** Same `TokenResponse` as above.

---

### [US-05] Logout (thiết bị hiện tại)
`POST /api/auth/logout` *(Auth required)*

**Request Body (JSON):**
```json
{
  "refreshToken": "abc123..."  // optional — nếu gửi, sẽ revoke đúng token này
}
```

**Response Data:** `null`

---

### [US-06] Logout All Devices
`POST /api/auth/logout-all` *(Auth required)*

**Request Body:** Không có body.

**Response Data:** `null`

---

### [US-04] Lấy thông tin User hiện tại
`GET /api/auth/me` *(Auth required)*

**Response Data:**
```json
{
  "id": "guid",
  "email": "user@example.com",
  "fullName": "Nguyen Van A",
  "role": "User"   // string: "User" | "Admin"
}
```

---

### [US-07] Xác minh Email
`GET /api/auth/verify-email?token=<verificationToken>`

**Query Params:**
| Param | Type | Bắt buộc | Mô tả |
|-------|------|----------|-------|
| `token` | string | ✅ | Token gửi trong email xác minh |

**Response Data:** `null`

---

### [US-08] Gửi lại Email xác minh
`POST /api/auth/verify-email/resend`

**Request Body (JSON):**
```json
{
  "email": "user@example.com"  // required
}
```

---

### [US-09] Quên mật khẩu
`POST /api/auth/forgot-password`

**Request Body (JSON):**
```json
{
  "email": "user@example.com"  // required
}
```

**Response Data:** `null` *(không lộ thông tin email có tồn tại không)*

---

### [US-10] Đặt lại mật khẩu
`POST /api/auth/reset-password`

**Request Body (JSON):**
```json
{
  "email": "user@example.com",
  "token": "reset-token-from-email",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

**Response Data:** `null`

---

### Đổi mật khẩu (đã đăng nhập)
`POST /api/auth/change-password` *(Auth required)*

**Request Body (JSON):**
```json
{
  "currentPassword": "OldPassword123!",  // required
  "newPassword": "NewPassword123!",      // required, minLength: 8
  "confirmPassword": "NewPassword123!"   // required, phải khớp newPassword
}
```

---

## 2️⃣ PROFILE (Business Profile / Onboarding)

> ⚠️ Tất cả Profile API dùng `multipart/form-data` (có file upload). Không phải JSON.

### [US-12, US-13] Lấy danh sách Profile của User
`GET /api/profiles/user/{userId}`

**Path Params:** `userId` (GUID của user hiện tại — phải khớp với token)

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `search` | string | null | Tìm kiếm theo tên |
| `isDeleted` | bool | null | Lọc xóa mềm |

**Response Data (`ProfileResponseDto[]`):**
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "name": "My Business",
    "profileType": 0,             // ProfileTypeEnum: 0=Free, 1=Basic, 2=Pro
    "subscriptionId": "guid|null",
    "companyName": "ACME Corp",
    "bio": "Mô tả business",
    "avatarUrl": "https://...",
    "status": 0,                  // ProfileStatusEnum: 0=Active, 1=Inactive, 2=Suspended
    "createdAt": "...",
    "updatedAt": "...",
    "isOwner": true,
    "memberRole": null
  }
]
```

---

### [US-12] Tạo Profile mới (Onboarding)
`POST /api/profiles/user/{userId}` — **Content-Type: multipart/form-data**

**Form Fields:**
| Field | Type | Bắt buộc | Mô tả |
|-------|------|----------|-------|
| `Name` | string | ✅ | Tên profile, maxLength: 255 |
| `ProfileType` | int | ✅ | `0`=Free, `1`=Basic, `2`=Pro |
| `CompanyName` | string | ❌ | maxLength: 255 |
| `Bio` | string | ❌ | maxLength: 1000 |
| `AvatarUrl` | string | ❌ | URL ảnh, maxLength: 500 |
| `AvatarFile` | file | ❌ | File ảnh upload (thay thế AvatarUrl) |

---

### [US-14] Cập nhật Profile
`PUT /api/profiles/{id}` — **Content-Type: multipart/form-data**

**Form Fields:**
| Field | Type | Bắt buộc | Mô tả |
|-------|------|----------|-------|
| `Name` | string | ❌ | maxLength: 255 |
| `ProfileType` | int | ❌ | `0|1|2` |
| `CompanyName` | string | ❌ | maxLength: 255 |
| `Bio` | string | ❌ | maxLength: 1000 |
| `AvatarUrl` | string | ❌ | maxLength: 500 |
| `AvatarFile` | file | ❌ | File upload thay thế AvatarUrl |

**Response Data:** `ProfileResponseDto` (như trên)

---

### Xóa mềm Profile
`DELETE /api/profiles/{id}` *(Auth required)*

**Response Data:** `true | false`

---

### Khôi phục Profile
`PATCH /api/profiles/{id}/restore` *(Auth required)*

**Response Data:** `true | false`

---

## 3️⃣ BRAND (Brand Kit)

> Header `X-Profile-Id` **KHÔNG bắt buộc** cho Brand. Dùng `userId` từ JWT.

### [US-15] Lấy danh sách Brand (phân trang)
`GET /api/brands`

**Query Params:**
| Param | Type | Default | Bắt buộc | Mô tả |
|-------|------|---------|----------|-------|
| `profileId` | GUID | — | ✅ | ID profile đang active |
| `page` | int | `1` | ❌ | Trang |
| `pageSize` | int | `10` | ❌ | Số item/trang |
| `searchTerm` | string | null | ❌ | Tìm kiếm |
| `sortBy` | string | null | ❌ | Field sắp xếp |
| `sortDescending` | bool | `true` | ❌ | Giảm dần |
| `includeDeleted` | bool | `false` | ❌ | Bao gồm đã xóa |

**Response Data (Paged):**
```json
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "name": "My Brand",
      "description": "Mô tả brand",
      "logoUrl": "https://...",
      "slogan": "Just do it",
      "usp": "Unique Selling Point",
      "targetAudience": "18-35 tuổi, thích thể thao",
      "profileId": "guid",
      "createdAt": "...",
      "updatedAt": "...",
      "productsCount": 5,
      "contentsCount": 12
    }
  ],
  "totalCount": 20,
  "page": 1,
  "pageSize": 10,
  "totalPages": 2
}
```

---

### Lấy Brand theo ID
`GET /api/brands/{id}` *(Auth required)*

**Response Data:** `BrandResponseDto` (một item như trên)

---

### [US-15] Tạo Brand mới
`POST /api/brands` — **Content-Type: application/json**

**Request Body:**
```json
{
  "name": "My Brand",            // required, maxLength: 255
  "description": "Mô tả",       // optional, maxLength: 2000
  "logoUrl": "https://...",      // optional, maxLength: 500
  "slogan": "Just do it",        // optional, maxLength: 255
  "usp": "Điểm nổi bật duy nhất", // optional
  "targetAudience": "18-35 tuổi", // optional
  "profileId": "guid"             // optional — ID profile liên kết
}
```

**Response Data:** `BrandResponseDto`

---

### [US-15] Cập nhật Brand
`PUT /api/brands/{id}` — **Content-Type: application/json**

**Request Body:**
```json
{
  "name": "Updated Brand",       // optional, maxLength: 255
  "description": "Mô tả mới",   // optional, maxLength: 2000
  "logoUrl": "https://...",      // optional, maxLength: 500
  "slogan": "New slogan",        // optional, maxLength: 255
  "usp": "USP mới",              // optional
  "targetAudience": "Đối tượng mới", // optional
  "profileId": "guid"            // optional
}
```

**Response Data:** `BrandResponseDto`

---

### [US-15] Xóa mềm Brand
`DELETE /api/brands/{id}` *(Auth required)*

**Response Data:** `true | false`

---

### [US-15] Khôi phục Brand
`POST /api/brands/{id}/restore` *(Auth required)*

**Response Data:** `true | false`

---

## 4️⃣ PRODUCT (Sản phẩm)

> ⚠️ Create/Update dùng `multipart/form-data` (có file ảnh). Header `X-Profile-Id` **KHÔNG bắt buộc**.

### [US-17, US-18] Lấy danh sách Product (phân trang)
`GET /api/products`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `brandId` | GUID | null | Lọc theo Brand |
| `page` | int | `1` | Trang |
| `pageSize` | int | `10` | Số item/trang |
| `searchTerm` | string | null | Tìm kiếm |
| `sortBy` | string | null | Field sắp xếp |
| `sortDescending` | bool | `true` | Giảm dần |
| `includeDeleted` | bool | `false` | Bao gồm đã xóa |

**Response Data (Paged):**
```json
{
  "items": [
    {
      "id": "guid",
      "brandId": "guid",
      "name": "Sản phẩm A",
      "description": "Mô tả sản phẩm",
      "price": 299000,
      "images": ["https://url1.jpg", "https://url2.jpg"],
      "createdAt": "...",
      "updatedAt": "..."
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

### [US-17] Tạo Product
`POST /api/products` — **Content-Type: multipart/form-data**

**Form Fields:**
| Field | Type | Bắt buộc | Mô tả |
|-------|------|----------|-------|
| `BrandId` | GUID | ✅ | Brand sở hữu sản phẩm |
| `Name` | string | ✅ | maxLength: 255 |
| `Description` | string | ❌ | maxLength: 2000 |
| `Price` | decimal | ❌ | Giá sản phẩm |
| `ImageFiles` | file[] | ❌ | Danh sách file ảnh upload |

**Response Data:** `ProductResponseDto` (một item như trên)

---

### [US-17] Cập nhật Product
`PUT /api/products/{id}` — **Content-Type: multipart/form-data**

**Form Fields:**
| Field | Type | Bắt buộc | Mô tả |
|-------|------|----------|-------|
| `BrandId` | GUID | ❌ | Đổi brand |
| `Name` | string | ❌ | Tên mới |
| `Description` | string | ❌ | Mô tả mới |
| `Price` | decimal | ❌ | Giá mới |
| `ImageFiles` | file[] | ❌ | Ảnh mới |

**Response Data:** `ProductResponseDto`

---

### [US-17] Xóa mềm / Khôi phục Product
- `DELETE /api/products/{id}` → Response: `true | false`
- `POST /api/products/{id}/restore` → Response: `true | false`

---

## 5️⃣ CONTENT HUB (Nội dung & AI)

> ⚠️ **Tất cả** Content API yêu cầu header `X-Profile-Id`.

### [US-20] Lấy danh sách Content (phân trang)
`GET /api/content`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | `1` | Trang |
| `pageSize` | int | `10` | Số item/trang |
| `searchTerm` | string | null | Tìm kiếm |
| `sortBy` | string | null | Field sắp xếp |
| `sortDescending` | bool | `true` | Giảm dần |
| `brandId` | GUID | null | Lọc theo brand |
| `adType` | int | null | `0`=TextOnly, `1`=ImageText, `2`=VideoText |
| `status` | int | null | `0`=Draft, `1`=PendingApproval, `2`=Approved, `3`=Rejected, `4`=Published |
| `includeDeleted` | bool | `false` | Bao gồm đã xóa |

**Response Data (Paged):**
```json
{
  "items": [
    {
      "id": "guid",
      "profileId": "guid",
      "brandId": "guid",
      "brandName": "My Brand",
      "productId": "guid|null",
      "adType": 0,
      "title": "Tiêu đề bài viết",
      "textContent": "Nội dung quảng cáo...",
      "imageUrl": "https://...",
      "videoUrl": null,
      "styleDescription": "Phong cách viết trang trọng",
      "contextDescription": "Dùng cho dịp Tết",
      "representativeCharacter": "Người phát ngôn thương hiệu",
      "status": 0,
      "createdAt": "...",
      "updatedAt": "..."
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

---

### [US-19] Tạo Content thủ công
`POST /api/content` — **Content-Type: application/json**

**Request Body:**
```json
{
  "brandId": "guid",                    // required
  "productId": "guid",                  // optional
  "adType": 0,                          // required: 0=TextOnly, 1=ImageText, 2=VideoText
  "title": "Tiêu đề",                   // optional
  "textContent": "Nội dung bài viết",   // required (có thể để string rỗng)
  "imageUrl": null,                     // optional
  "videoUrl": null,                     // optional
  "styleDescription": "Trang trọng",    // optional
  "contextDescription": "Dịp lễ Tết",  // optional
  "representativeCharacter": "Brand Ambassador" // optional
}
```

**Response Data:** `ContentResponseDto` (một item như trên)

---

### [US-19] Cập nhật Content
`PUT /api/content/{contentId}` — **Content-Type: application/json**

**Request Body:**
```json
{
  "productId": "guid|null",            // optional
  "adType": 1,                         // optional: 0|1|2
  "title": "Tiêu đề mới",             // optional
  "textContent": "Nội dung cập nhật", // optional
  "imageUrl": "https://...",           // optional
  "videoUrl": null,                    // optional
  "styleDescription": "...",           // optional
  "contextDescription": "...",         // optional
  "representativeCharacter": "..."     // optional
}
```

**Response Data:** `ContentResponseDto`

---

### [US-21] Clone Content
`POST /api/content/{contentId}/clone`

**Request Body:** Không có body.

**Response Data:** `ContentResponseDto` (bản clone mới)

---

### [US-34] Publish Content ngay lập tức lên Facebook
`POST /api/content/{contentId}/publish/{integrationId}`

**Path Params:**
- `contentId`: GUID của content muốn đăng
- `integrationId`: GUID của Social Integration (Fanpage đã được link)

**Request Body:** Không có body.

**Response Data (`PublishResultDto`):**
```json
{
  // Xem PostListItemDto cho cấu trúc
  "externalPostId": "fb-post-id-123"
}
```

---

### Xóa mềm / Khôi phục Content
- `DELETE /api/content/{contentId}` → Response: `true | false`
- `POST /api/content/{contentId}/restore` → Response: `true | false`

---

## 6️⃣ AI (Gemini)

> ⚠️ Tất cả AI API yêu cầu header `X-Profile-Id`.

### [US-22] Generate Draft (Sinh bản nháp từ AI)
`POST /api/ai/generate-draft`

**Request Body (JSON):**
```json
{
  "brandId": "guid",      // required
  "productId": "guid",    // optional
  "adType": 0,            // required: 0=TextOnly, 1=ImageText, 2=VideoText
  "title": "Tiêu đề",    // optional
  "prompt": "Viết bài quảng cáo về sản phẩm này cho dịp Tết..." // required
}
```

**Response Data (`AiGenerationResponse`):**
```json
{
  "aiGenerationId": "guid",
  "contentId": "guid",
  "generatedText": "Nội dung AI sinh ra...",
  "status": 1,             // AiStatusEnum: 0=Pending, 1=Completed, 2=Failed
  "errorMessage": null,
  "createdAt": "..."
}
```

---

### [US-23] Improve Content (Cải thiện đoạn văn bằng AI)
`POST /api/ai/improve/{contentId}`

**Request Body (JSON):**
```json
{
  "prompt": "Viết lại đoạn này ngắn gọn hơn, thu hút hơn..."  // required
}
```

**Response Data:** `AiGenerationResponse` (như trên)

---

### [US-24] Approve AI Generation (Áp dụng bản nháp AI vào content)
`POST /api/ai/approve/{aiGenerationId}`

**Request Body:** Không có body.

**Response Data:** `ContentResponseDto` (content đã được cập nhật với text mới)

---

### [US-25] Xem lịch sử AI Generations của 1 Content
`GET /api/ai/generations/{contentId}`

**Response Data:** `AiGenerationResponse[]` (danh sách tất cả phiên bản AI đã sinh)

---

### [US-26] Chat với AI
`POST /api/ai/chat`

**Request Body (JSON):**
```json
{
  "brandId": "guid",         // optional — cho AI context
  "productId": "guid",       // optional
  "adType": 0,               // required: 0|1|2
  "message": "Hãy cho tôi ý tưởng quảng cáo Tết...", // required
  "conversationId": "guid"   // optional — nếu có, tiếp tục conversation cũ
}
```

**Response Data (`ChatResponse`):**
```json
{
  "response": "Đây là ý tưởng của tôi...",
  "conversationId": "guid"   // GUID conversation (mới hoặc cũ)
}
```

---

## 7️⃣ CONVERSATIONS (Lịch sử AI Chat)

> ⚠️ Tất cả yêu cầu header `X-Profile-Id`.

### [US-27] Lấy danh sách Conversations
`GET /api/conversations`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | `1` | Trang |
| `pageSize` | int | `10` | Số item/trang |
| `searchTerm` | string | null | Tìm kiếm |
| `sortBy` | string | null | Sắp xếp |
| `sortDescending` | bool | `true` | Giảm dần |

**Response Data (`ConversationResponseDto[]` paged):**
```json
{
  "items": [
    {
      "id": "guid",
      "profileId": "guid",
      "brandId": "guid|null",
      "brandName": "My Brand",
      "productId": "guid|null",
      "productName": "Sản phẩm A",
      "adType": 0,
      "title": "Hội thoại về Tết",
      "isActive": true,
      "lastMessage": "Ý tưởng cuối cùng...",
      "lastMessageAt": "...",
      "messageCount": 8
    }
  ]
}
```

---

### [US-28] Xóa Conversation
`DELETE /api/conversations/{id}`

**Response Data:** `true | false`

---

## 8️⃣ SOCIAL INTEGRATIONS (Kết nối MXH)

> ⚠️ Tất cả Social API yêu cầu header `X-Profile-Id`.

### [US-29] Lấy URL OAuth Facebook
`GET /api/social-auth/facebook`

**Response Data (`AuthUrlResponse`):**
```json
{
  "authUrl": "https://www.facebook.com/dialog/oauth?...",
  "state": "random-state-string"
}
```

> FE mở URL này để user cho phép. Facebook redirect về callback URL của FE.

---

### [US-29] Xử lý Facebook OAuth Callback
`POST /api/social-auth/facebook/callback`

**Request Body (JSON):**
```json
{
  "code": "facebook-auth-code-from-oauth",  // required
  "state": "random-state-string"             // required — phải khớp với state lưu trước đó
}
```

**Response Data:** `SocialAccountDto`
```json
{
  "id": "guid",
  "profileId": "guid",
  "provider": "facebook",
  "providerUserId": "fb-user-id",
  "isActive": true,
  "expiresAt": "...",
  "createdAt": "...",
  "updatedAt": "...",
  "targets": []
}
```

---

### [US-30] Lấy danh sách Social Accounts đã kết nối
`GET /api/social/accounts/me`

**Response Data:** `SocialAccountDto[]`

---

### [US-31] Lấy danh sách Fanpage/Targets khả dụng
`GET /api/social/accounts/{socialAccountId}/available-targets`

**Response Data:** `AvailableTargetDto[]`
```json
[
  {
    "providerTargetId": "fb-page-id-123",
    "name": "Fanpage Thương Hiệu",
    "type": "page",
    "category": "Brand",
    "profilePictureUrl": "https://...",
    "isActive": true
  }
]
```

---

### [US-32] Link Fanpage vào Brand
`POST /api/social/accounts/{socialAccountId}/link-targets`

**Request Body (JSON):**
```json
{
  "profileId": "guid",                       // required
  "provider": "facebook",                    // required — chỉ "facebook" được support
  "providerTargetIds": ["fb-page-id-123"],   // required — list page ID muốn link
  "brandId": "guid"                          // required — brand sẽ sử dụng các page này
}
```

**Response Data:** `SocialAccountDto` (updated với targets mới)

---

### Xem Linked Targets của 1 Social Account
`GET /api/social/accounts/{socialAccountId}/linked-targets`

**Response Data:** `SocialTargetDto[]`
```json
[
  {
    "id": "guid",
    "providerTargetId": "fb-page-id-123",
    "name": "Fanpage Thương Hiệu",
    "type": "page",
    "category": "Brand",
    "profilePictureUrl": "https://...",
    "isActive": true
  }
]
```

---

### [US-33] Ngắt kết nối Social Account
`DELETE /api/social/accounts/{socialAccountId}`

**Response Data:** `true | false`

---

### [US-33] Unlink Fanpage cụ thể (Integration)
`DELETE /api/social/integrations/{socialIntegrationId}`

**Response Data:** `true | false`

---

### Lấy Integrations theo Brand
`GET /api/social/integrations/brand/{brandId}`

**Response Data:** `SocialIntegrationDto[]`
```json
[
  {
    "id": "guid",
    "socialAccountId": "guid",
    "profileId": "guid",
    "brandId": "guid",
    "externalId": "fb-page-id",
    "name": "Fanpage AISAM",
    "platform": "facebook",
    "isActive": true,
    "createdAt": "...",
    "updatedAt": "...",
    "brandName": "My Brand"
  }
]
```

---

## 9️⃣ CONTENT SCHEDULES (Lên lịch đăng bài)

> ⚠️ Tất cả yêu cầu header `X-Profile-Id`.

### [US-40] Tạo lịch đăng
`POST /api/content-schedules`

**Request Body (JSON):**
```json
{
  "contentId": "guid",         // required — content muốn lên lịch
  "integrationId": "guid",     // required — GUID social integration (Fanpage)
  "scheduledAt": "2026-06-10T09:00:00Z" // required — thời điểm đăng (UTC)
}
```

**Response Data (`ContentScheduleDto`):**
```json
{
  "id": "guid",
  "profileId": "guid",
  "contentId": "guid",
  "integrationId": "guid",
  "scheduledAt": "2026-06-10T09:00:00Z",
  "executedAt": null,
  "status": "Pending",         // "Pending" | "Completed" | "Failed" | "Cancelled"
  "attemptCount": 0,
  "lastError": null
}
```

---

### [US-41] Lấy danh sách lịch đăng (phân trang)
`GET /api/content-schedules`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | `1` | Trang |
| `pageSize` | int | `10` | Số item/trang |

**Response Data (Paged `ContentScheduleDto[]`)**

---

### Upcoming Schedules (Calendar)
`GET /api/content-schedules/upcoming`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `limit` | int | `10` | Số lịch sắp tới trả về |

**Response Data:** `ContentScheduleDto[]`

---

### [US-41] Cập nhật lịch đăng
`PUT /api/content-schedules/{scheduleId}`

**Request Body (JSON):**
```json
{
  "integrationId": "guid",              // optional — đổi fanpage
  "scheduledAt": "2026-06-15T10:00:00Z" // optional — đổi thời gian
}
```

**Response Data:** `ContentScheduleDto`

---

### [US-41] Hủy lịch đăng
`DELETE /api/content-schedules/{scheduleId}`

**Response Data:** `true | false`

---

## 🔟 POSTS (Lịch sử bài đã đăng)

> ⚠️ Tất cả yêu cầu header `X-Profile-Id`.

### [US-35] Lấy danh sách Posts đã đăng
`GET /api/posts`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | `1` | Trang |
| `pageSize` | int | `10` | Số item/trang |
| `brandId` | GUID | null | Lọc theo brand |
| `status` | int | null | ContentStatusEnum: 0-4 |

**Response Data (Paged `PostListItemDto[]`):**
```json
{
  "items": [
    {
      "id": "guid",
      "contentId": "guid",
      "integrationId": "guid",
      "externalPostId": "fb-123456",
      "publishedAt": "...",
      "status": "Published",       // string status
      "contentTitle": "Tiêu đề bài",
      "brandName": "My Brand"
    }
  ]
}
```

---

## 1️⃣1️⃣ NOTIFICATIONS

> ⚠️ Tất cả yêu cầu header `X-Profile-Id`.

### [US-37] Lấy danh sách thông báo
`GET /api/notifications`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | `1` | Trang |
| `pageSize` | int | `10` | Số item/trang |

**Response Data (Paged `NotificationListItemDto[]`):**
```json
{
  "items": [
    {
      "id": "guid",
      "type": "PostPublished",   // NotificationType string
      "title": "Bài viết đã đăng thành công",
      "message": "Bài 'Tiêu đề' đã đăng lên Fanpage...",
      "isRead": false,
      "createdAt": "..."
    }
  ]
}
```

---

### [US-36] Lấy số thông báo chưa đọc (Unread Badge)
`GET /api/notifications/unread-count`

**Response Data:**
```json
{
  "count": 5
}
```

---

### [US-38] Mark as read (1 thông báo)
`POST /api/notifications/{notificationId}/mark-read`

**Response Data:** `true | false`

---

### Mark All Read
`POST /api/notifications/mark-all-read`

**Response Data:** `true | false`

---

## 1️⃣2️⃣ QUOTA

> ⚠️ Yêu cầu header `X-Profile-Id`.

### [US-48] Lấy Quota hiện tại
`GET /api/quota/profile/{profileId}`

> `profileId` trong URL phải khớp với `X-Profile-Id` header.

**Response Data (`QuotaSummaryDto`):**
```json
{
  "planName": "Free",
  "subscriptionStatus": "Active",
  "windowStart": "2026-06-01T00:00:00Z",
  "windowEnd": "2026-06-30T23:59:59Z",
  "promptQuotaLimit": 50,
  "promptUsage": 20,
  "promptRemaining": 30,
  "postQuotaLimit": 10,
  "postUsage": 3,
  "postRemaining": 7
}
```

---

## 1️⃣3️⃣ PAYMENT & SUBSCRIPTION

> ⚠️ Yêu cầu header `X-Profile-Id`.

### [US-46] Xem Subscription hiện tại
`GET /api/payment/subscription/current`

**Response Data (`CurrentSubscriptionDto`):**
```json
{
  "subscriptionId": "guid",
  "planName": "Pro",
  "status": "Active",
  "startDate": "2026-06-01T00:00:00Z",
  "endDate": "2026-07-01T00:00:00Z"
}
```

---

### [US-44] Tạo checkout (Upgrade Plan)
`POST /api/payment/checkout`

**Request Body (JSON):**
```json
{
  "planCode": "PRO_MONTHLY",    // required — mã gói cước
  "returnUrl": "https://app.aisam.vn/payment/success", // optional
  "cancelUrl": "https://app.aisam.vn/payment/cancel"   // optional
}
```

**Response Data (`PayOSCheckoutResponse`):**
```json
{
  "checkoutUrl": "https://pay.payos.vn/...",  // URL QR thanh toán
  "paymentLinkId": "payos-link-id",
  "orderCode": "123456"
}
```

---

### [US-45] Lịch sử thanh toán
`GET /api/payment/history`

**Query Params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | `1` | Trang |
| `pageSize` | int | `10` | Số item/trang |

**Response Data (Paged `PaymentHistoryItemDto[]`):**
```json
{
  "items": [
    {
      "id": "guid",
      "paymentMethod": "QR",
      "amount": 299000,
      "status": "Paid",
      "createdAt": "..."
    }
  ]
}
```

---

## 1️⃣4️⃣ DASHBOARD

> ⚠️ Yêu cầu header `X-Profile-Id`.

### [US-43] Lấy Dashboard Summary
`GET /api/dashboard/summary`

**Response Data (`DashboardSummaryDto`):**
```json
{
  "draftContentCount": 5,
  "publishedContentCount": 12,
  "pendingApprovalContentCount": 2,
  "upcomingScheduleCount": 3,
  "failedScheduleCount": 1,
  "activeSocialIntegrationCount": 2,
  "publishedPostCount": 24,
  "unreadNotificationCount": 4
}
```

---

## ⚠️ DANH SÁCH NHỮNG GÌ **CHƯA CÓ** TRONG BACKEND (Cần tránh khi build UI)

| US | Tính năng | Trạng thái | Ghi chú |
|----|-----------|------------|---------|
| US-58 | Gửi bài cho sếp duyệt (Pending → Approve/Reject/Feedback) | ❌ Chưa migrate | Enum `ContentStatusEnum.PendingApproval` có sẵn nhưng flow approval chưa có endpoint |
| US-61 | Upload media (ảnh/video) riêng lên Storage | ❌ Chưa migrate | Chỉ có URL string, chưa có endpoint upload file cho Content |
| US-62, 63 | Kết nối Instagram Business & TikTok Business | ❌ Chưa migrate | Chỉ Facebook được support, code check hard-code "facebook" |
| US-64, 65 | AI sinh ảnh/video quảng cáo | ❌ Chưa migrate | Chỉ có generate text (Gemini), chưa có image/video AI |
| US-59 | Quản lý Team (Member, Role) | ❌ Chưa migrate | Model có nhưng không có API Controller |
| US-60 | Chạy Facebook Ads trực tiếp | ❌ Chưa migrate | — |
| US-66 | Admin quản lý Plans động | ❌ Chưa migrate | — |
| US-67, 68 | Analytics chi tiết & AI gợi ý tối ưu | ❌ Chưa migrate | — |
| US-54 | Admin Seed Demo Data | ❓ Không tìm thấy endpoint rõ ràng | Có thể là internal tool |

---

## 🔢 ENUM REFERENCE

### AdTypeEnum
| Value | Name | Mô tả |
|-------|------|-------|
| `0` | TextOnly | Chỉ văn bản |
| `1` | ImageText | Ảnh + văn bản |
| `2` | VideoText | Video + văn bản |

### ContentStatusEnum
| Value | Name | Mô tả |
|-------|------|-------|
| `0` | Draft | Bản nháp |
| `1` | PendingApproval | Chờ duyệt |
| `2` | Approved | Đã duyệt |
| `3` | Rejected | Bị từ chối |
| `4` | Published | Đã đăng |

### ProfileTypeEnum
| Value | Name | Mô tả |
|-------|------|-------|
| `0` | Free | Gói miễn phí |
| `1` | Basic | Gói cơ bản |
| `2` | Pro | Gói chuyên nghiệp |

### UserRoleEnum
| Value | Name | Mô tả |
|-------|------|-------|
| `0` | User | Người dùng thường |
| `1` | Admin | Quản trị viên |

### AiStatusEnum (trong AiGenerationResponse)
| Value | Name |
|-------|------|
| `0` | Pending |
| `1` | Completed |
| `2` | Failed |

---

## 🗺️ API ENDPOINT SUMMARY TABLE

| Route | Method | Header | Auth | US |
|-------|--------|--------|------|----|
| `/api/auth/register` | POST | — | ❌ | US-01 |
| `/api/auth/login` | POST | — | ❌ | US-02 |
| `/api/auth/google` | POST | — | ❌ | US-11 |
| `/api/auth/refresh` | POST | — | ❌ | US-03 |
| `/api/auth/logout` | POST | Bearer | ✅ | US-05 |
| `/api/auth/logout-all` | POST | Bearer | ✅ | US-06 |
| `/api/auth/me` | GET | Bearer | ✅ | US-04 |
| `/api/auth/forgot-password` | POST | — | ❌ | US-09 |
| `/api/auth/reset-password` | POST | — | ❌ | US-10 |
| `/api/auth/verify-email` | GET | — | ❌ | US-07 |
| `/api/auth/verify-email/resend` | POST | — | ❌ | US-08 |
| `/api/auth/change-password` | POST | Bearer | ✅ | — |
| `/api/profiles/user/{userId}` | GET | Bearer | ✅ | US-13 |
| `/api/profiles/user/{userId}` | POST | Bearer | ✅ | US-12 |
| `/api/profiles/{id}` | GET | Bearer | ✅ | US-13 |
| `/api/profiles/{id}` | PUT | Bearer | ✅ | US-14 |
| `/api/profiles/{id}` | DELETE | Bearer | ✅ | US-15 |
| `/api/profiles/{id}/restore` | PATCH | Bearer | ✅ | US-15 |
| `/api/brands` | GET | Bearer | ✅ | US-15 |
| `/api/brands` | POST | Bearer | ✅ | US-15 |
| `/api/brands/{id}` | GET | Bearer | ✅ | US-15 |
| `/api/brands/{id}` | PUT | Bearer | ✅ | US-15 |
| `/api/brands/{id}` | DELETE | Bearer | ✅ | US-15 |
| `/api/brands/{id}/restore` | POST | Bearer | ✅ | US-15 |
| `/api/products` | GET | Bearer | ✅ | US-17,18 |
| `/api/products` | POST | Bearer | ✅ | US-17 |
| `/api/products/{id}` | GET | Bearer | ✅ | US-17 |
| `/api/products/{id}` | PUT | Bearer | ✅ | US-17 |
| `/api/products/{id}` | DELETE | Bearer | ✅ | US-17 |
| `/api/products/{id}/restore` | POST | Bearer | ✅ | US-17 |
| `/api/social-auth/facebook` | GET | Bearer + X-Profile-Id | ✅ | US-29 |
| `/api/social-auth/facebook/callback` | POST | Bearer + X-Profile-Id | ✅ | US-29 |
| `/api/social/accounts/me` | GET | Bearer + X-Profile-Id | ✅ | US-30 |
| `/api/social/accounts/{id}/available-targets` | GET | Bearer + X-Profile-Id | ✅ | US-31 |
| `/api/social/accounts/{id}/linked-targets` | GET | Bearer + X-Profile-Id | ✅ | US-32 |
| `/api/social/accounts/{id}/link-targets` | POST | Bearer + X-Profile-Id | ✅ | US-32 |
| `/api/social/accounts/{id}` | DELETE | Bearer + X-Profile-Id | ✅ | US-33 |
| `/api/social/integrations/{id}` | DELETE | Bearer + X-Profile-Id | ✅ | US-33 |
| `/api/social/integrations/brand/{brandId}` | GET | Bearer + X-Profile-Id | ✅ | — |
| `/api/content` | GET | Bearer + X-Profile-Id | ✅ | US-20 |
| `/api/content` | POST | Bearer + X-Profile-Id | ✅ | US-19 |
| `/api/content/{id}` | GET | Bearer + X-Profile-Id | ✅ | US-20 |
| `/api/content/{id}` | PUT | Bearer + X-Profile-Id | ✅ | US-19 |
| `/api/content/{id}/clone` | POST | Bearer + X-Profile-Id | ✅ | US-21 |
| `/api/content/{id}/publish/{integrationId}` | POST | Bearer + X-Profile-Id | ✅ | US-34 |
| `/api/content/{id}` | DELETE | Bearer + X-Profile-Id | ✅ | US-19 |
| `/api/content/{id}/restore` | POST | Bearer + X-Profile-Id | ✅ | US-19 |
| `/api/ai/generate-draft` | POST | Bearer + X-Profile-Id | ✅ | US-22 |
| `/api/ai/improve/{contentId}` | POST | Bearer + X-Profile-Id | ✅ | US-23 |
| `/api/ai/approve/{aiGenerationId}` | POST | Bearer + X-Profile-Id | ✅ | US-24 |
| `/api/ai/generations/{contentId}` | GET | Bearer + X-Profile-Id | ✅ | US-25 |
| `/api/ai/chat` | POST | Bearer + X-Profile-Id | ✅ | US-26 |
| `/api/conversations` | GET | Bearer + X-Profile-Id | ✅ | US-27 |
| `/api/conversations/{id}` | GET | Bearer + X-Profile-Id | ✅ | US-27 |
| `/api/conversations/{id}` | DELETE | Bearer + X-Profile-Id | ✅ | US-28 |
| `/api/content-schedules` | POST | Bearer + X-Profile-Id | ✅ | US-40 |
| `/api/content-schedules` | GET | Bearer + X-Profile-Id | ✅ | US-41 |
| `/api/content-schedules/upcoming` | GET | Bearer + X-Profile-Id | ✅ | US-41 |
| `/api/content-schedules/{id}` | GET | Bearer + X-Profile-Id | ✅ | US-41 |
| `/api/content-schedules/{id}` | PUT | Bearer + X-Profile-Id | ✅ | US-41 |
| `/api/content-schedules/{id}` | DELETE | Bearer + X-Profile-Id | ✅ | US-41 |
| `/api/posts` | GET | Bearer + X-Profile-Id | ✅ | US-35 |
| `/api/posts/{postId}` | GET | Bearer + X-Profile-Id | ✅ | US-35 |
| `/api/notifications` | GET | Bearer + X-Profile-Id | ✅ | US-37 |
| `/api/notifications/{id}` | GET | Bearer + X-Profile-Id | ✅ | US-37 |
| `/api/notifications/unread-count` | GET | Bearer + X-Profile-Id | ✅ | US-36 |
| `/api/notifications/{id}/mark-read` | POST | Bearer + X-Profile-Id | ✅ | US-38 |
| `/api/notifications/mark-all-read` | POST | Bearer + X-Profile-Id | ✅ | US-38 |
| `/api/quota/profile/{profileId}` | GET | Bearer + X-Profile-Id | ✅ | US-48 |
| `/api/payment/checkout` | POST | Bearer + X-Profile-Id | ✅ | US-44 |
| `/api/payment/history` | GET | Bearer + X-Profile-Id | ✅ | US-45 |
| `/api/payment/subscription/current` | GET | Bearer + X-Profile-Id | ✅ | US-46 |
| `/api/payment/callback` | POST | — | ❌ | US-47 (webhook) |
| `/api/payment/webhook` | POST | — | ❌ | US-47 (webhook) |
| `/api/dashboard/summary` | GET | Bearer + X-Profile-Id | ✅ | US-43 |
