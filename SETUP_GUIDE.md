# AISAM Backend Setup Guide

Tài liệu cấu hình thủ công cho backend `.NET 8` tại:

```text
D:\AISAM\AISAM-FINAL\AISAM-BE
```

Source cũ baseline:

```text
D:\AISAM\PRN232-AISAM\PRN232_Backend
```

## 1. Trạng Thái Backend

Backend đã hoàn thành đến hết **Phase 6 - Social integration và Facebook Page publishing MVP**.

| Nhóm | Trạng thái |
| --- | --- |
| Auth, PostgreSQL, JWT | Hoàn thành |
| Profile, Brand, Product | Hoàn thành |
| Content CRUD, Gemini AI text, Conversation | Hoàn thành |
| Facebook OAuth, Page linking, publish content, Posts query | Hoàn thành code |
| Scheduling, Notification, Dashboard | Chưa làm |
| Payment, quota, Admin | Chưa làm |

Kết quả xác minh ngày `2026-06-01`:

```text
dotnet restore
PASS

dotnet build --no-restore
PASS: 0 errors

dotnet test --no-build
PASS: 75/75 tests

dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
PASS: applied 20260531161937_RemovePostSocialIntegrationShadowFk
```

## 2. Không Commit Secrets Lên Git

<span style="color:red"><strong>REQUIRED NOW</strong></span>

Không commit:

```text
AISAM-BE/AISAM.API/.env
appsettings.Production.json
```

Không gửi secret vào chat hoặc đưa lên Git:

- PostgreSQL password production.
- JWT secret.
- Gemini API key.
- SMTP password.
- Google client secret.
- Facebook App Secret hoặc access token.
- PayOS API key/checksum key.
- Supabase service role key.

Được phép commit:

```text
AISAM-BE/AISAM.API/.env.example
```

## 3. Cấu Hình Bắt Buộc Hiện Tại

Tạo file local:

```text
AISAM-BE/AISAM.API/.env
```

### PostgreSQL

<span style="color:red"><strong>REQUIRED NOW</strong></span>

```env
CONNECTION_STRING=Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password
```

Chạy migration:

```powershell
cd D:\AISAM\AISAM-FINAL\AISAM-BE
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
```

### JWT

<span style="color:red"><strong>REQUIRED NOW</strong></span>

```env
JWT_SECRET_KEY=replace-with-a-long-random-secret-minimum-32-characters
JWT_ISSUER=AISAM.API
JWT_AUDIENCE=AISAM.Client
```

### Frontend URL

```env
FRONTEND_BASE_URL=http://localhost:3000
```

## 4. Active Profile Header

<span style="color:red"><strong>REQUIRED NOW</strong></span>

Các API sau bắt buộc có JWT và active profile:

```text
/api/content
/api/ai
/api/conversations
/api/social-auth
/api/social
/api/posts
```

Trong Postman thêm:

```text
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json
```

`profileId` phải thuộc user trong JWT.

## 5. Gemini AI

### Mục đích

Sinh draft text, cải thiện content và chat AI.

### Trạng thái

<span style="color:red"><strong>REQUIRED FOR REAL AI TEST</strong></span>

### Tạo key

```text
https://aistudio.google.com/
```

### Thêm vào `.env`

```env
GEMINI_API_KEY=your-real-gemini-api-key
GEMINI_MODEL=gemini-2.5-flash
GEMINI_MAX_TOKENS=2048
GEMINI_TEMPERATURE=0.7
```

## 6. SMTP Email

### Mục đích

Gửi verification email và reset password thật.

### Trạng thái

<span style="color:gray"><strong>OPTIONAL</strong></span>

### Thêm vào `.env`

```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-gmail-app-password
FROM_EMAIL=your-email@gmail.com
```

Với Gmail, dùng App Password thay vì password thường.

## 7. Google Login

### Trạng thái

<span style="color:gray"><strong>OPTIONAL</strong></span>

Backend hiện nhận Google `idToken` từ frontend và validate audience bằng Client ID. Không dùng `/signin-google`.

### Thêm vào `.env`

```env
GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=
```

`GOOGLE_CLIENT_SECRET` chưa cần cho flow hiện tại.

## 8. Facebook App Và Graph API

### Mục đích

<span style="color:red"><strong>REQUIRED NOW FOR SOCIAL REAL TEST</strong></span>

Dùng để:

- Tạo Facebook OAuth URL.
- Kết nối Facebook account.
- Lấy danh sách Facebook Pages.
- Link Page vào Brand.
- Publish content lên Facebook Page.

### Tạo App

Mở:

```text
https://developers.facebook.com/
```

Thực hiện:

1. Tạo Meta App.
2. Thêm use case hoặc product hỗ trợ **Facebook Login**.
3. Trong Facebook Login settings, thêm **Valid OAuth Redirect URI**.
4. Thêm tài khoản Facebook test vào Roles nếu app đang ở Development mode.
5. Dùng tài khoản test quản lý ít nhất một Facebook Page.

### Cần Lấy Giá Trị Gì

Từ **App settings > Basic**:

- App ID.
- App Secret.

Redirect URI local đề xuất:

```text
http://localhost:3000/auth/facebook/callback
```

Đây là URL frontend nhận `code` và `state` từ Meta. Frontend sau đó gọi backend:

```http
POST /api/social-auth/facebook/callback
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
Content-Type: application/json

{
  "code": "{code-from-meta}",
  "state": "{state-from-meta}"
}
```

Nếu chưa có frontend, có thể dùng một callback URL local tạm để quan sát `code,state`, nhưng URL đó phải khớp chính xác giữa Meta App và `.env`.

### Quyền Facebook Cần Xin

Backend hiện yêu cầu:

```text
pages_manage_posts
pages_read_engagement
pages_show_list
```

Trong Development mode, dùng app role/test user để test trước. Khi đưa production cho user ngoài app roles, cần Meta App Review nếu Meta yêu cầu.

### Thêm Vào `.env`

```env
FACEBOOK_APP_ID=your-facebook-app-id
FACEBOOK_APP_SECRET=your-facebook-app-secret
FACEBOOK_REDIRECT_URI=http://localhost:3000/auth/facebook/callback
FACEBOOK_GRAPH_API_VERSION=v23.0
FACEBOOK_BASE_URL=https://graph.facebook.com
FACEBOOK_OAUTH_URL=https://www.facebook.com
```

### API Test Theo Thứ Tự

Tất cả request cần:

```text
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

1. Lấy auth URL:

```text
GET /api/social-auth/facebook
```

2. Mở `data.authUrl` trên browser, đăng nhập Meta và lấy `code,state`.

3. Gửi callback:

```text
POST /api/social-auth/facebook/callback
```

4. Xem account:

```text
GET /api/social/accounts/me
```

5. Xem Pages khả dụng:

```text
GET /api/social/accounts/{socialAccountId}/available-targets
```

6. Link Page vào Brand:

```text
POST /api/social/accounts/{socialAccountId}/link-targets
```

7. Publish content:

```text
POST /api/content/{contentId}/publish/{integrationId}
```

8. Xem post:

```text
GET /api/posts
GET /api/posts/{postId}
```

### Lỗi Thường Gặp

- `Facebook integration is not configured`: thiếu App ID, App Secret hoặc Redirect URI.
- Redirect URI mismatch: URL trong Meta App không khớp chính xác `.env`.
- Không thấy Page: tài khoản không quản lý Page hoặc thiếu `pages_show_list`.
- Publish lỗi: thiếu `pages_manage_posts`, token hết hạn hoặc Page token không hợp lệ.
- OAuth state invalid/expired: dùng callback quá muộn hoặc dùng sai `state`.

## 9. Supabase Storage

### Trạng thái

<span style="color:gray"><strong>OPTIONAL / FUTURE</strong></span>

Avatar và product image upload file chưa bật trong MVP.

```env
SUPABASE_URL=
SUPABASE_KEY=
```

## 10. PayOS

### Trạng thái

<span style="color:gray"><strong>OPTIONAL / FUTURE</strong></span>

```env
PAYOS_CLIENT_ID=
PAYOS_API_KEY=
PAYOS_CHECKSUM_KEY=
```

## 11. Chạy Backend Local

```powershell
cd D:\AISAM\AISAM-FINAL\AISAM-BE
dotnet restore
dotnet build
dotnet test
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
dotnet run --project AISAM.API
```

Swagger:

```text
http://localhost:{port}/swagger/index.html
```

## 12. Checklist

### Core Backend

- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> PostgreSQL đang chạy.
- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> Đã thêm `CONNECTION_STRING`.
- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> Đã thêm JWT config.
- [ ] Đã chạy migration.
- [ ] Build pass.
- [ ] Test pass.

### AI Thật

- [ ] Đã thêm `GEMINI_API_KEY`.
- [ ] Test `/api/ai/generate-draft`.
- [ ] Test `/api/ai/chat`.

### Facebook Thật

- [ ] Tạo Meta App.
- [ ] Thêm Facebook Login.
- [ ] Thêm Valid OAuth Redirect URI.
- [ ] Điền `FACEBOOK_APP_ID`.
- [ ] Điền `FACEBOOK_APP_SECRET`.
- [ ] Điền `FACEBOOK_REDIRECT_URI`.
- [ ] Tài khoản test quản lý ít nhất một Facebook Page.
- [ ] Test auth URL, callback, link Page và publish.
