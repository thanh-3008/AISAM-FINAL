# AISAM Backend Setup Guide

## Workspace Migration Status

Tai lieu chi tiet: `CHANGE_REQUEST_WORKSPACE_SUBSCRIPTION_CREDIT_ANALYSIS.md`.

Backend hien tai dang o trang thai chuyen tiep:

```text
X-Workspace-Id: Workspace membership, payment, subscription, credits, feature gate, permission va Post Quota
X-Profile-Id: cac domain chua migrate ownership nhu Brand, Product, Content, Social
```

Phase 9 da hoan thanh Task 9.1-9.15. Khong bo `X-Profile-Id` khoi cac route Profile-based cho den khi Task 9.16 va backfill Task 9.17 hoan thanh.

Tài liệu này ghi lại các cấu hình thủ công để chạy backend AISAM `.NET 8`.

Backend hiện tại:

```text
D:\AISAM\AISAM-FINAL\AISAM-BE
```

Source cũ baseline:

```text
D:\AISAM\PRN232-AISAM\PRN232_Backend
```

## 1. Quy Ước Trạng Thái

- <span style="color:red"><strong>REQUIRED NOW</strong></span>: bắt buộc để chạy và test backend hiện tại.
- <span style="color:red"><strong>REQUIRED FOR REAL AI TEST</strong></span>: bắt buộc khi gọi Gemini thật.
- <span style="color:red"><strong>REQUIRED IN NEXT PHASE</strong></span>: chưa cần ngay, nhưng sẽ cần ở phase tiếp theo.
- <span style="color:gray"><strong>OPTIONAL / FUTURE</strong></span>: chưa cần cho MVP hiện tại.

## 2. Tiến Độ Hiện Tại

Backend da hoan thanh Phase 0-8 va **Phase 9 Task 9.1-9.15**:

- API host, Swagger, Health.
- PostgreSQL, EF Core và migrations.
- Auth JWT: register, login, refresh, logout, sessions, password.
- Profile CRUD.
- Brand CRUD.
- Product CRUD.
- Content CRUD, clone, soft delete, restore.
- Gemini text generation: generate draft, improve, approve, history, chat.
- Conversation history.
- Active profile context middleware.
- Workspace CRUD, invitation/member role, ownership transfer.
- Workspace subscription/payment, Credit Wallet va member quota.
- Workspace feature/permission gate, Post Quota va AI Credit charging.

Trong giai doan chuyen tiep, cac API `/api/content`, `/api/ai` va route Workspace-protected lien quan bat buoc co:

```text
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
X-Workspace-Id: {workspaceId}
```

`X-Profile-Id` phai la profile thuoc user trong JWT va `X-Workspace-Id` phai la Workspace ma user dang la active member.

## 3. Không Commit Secrets Lên Git

<span style="color:red"><strong>REQUIRED NOW</strong></span>

Không commit:

```text
AISAM-BE/AISAM.API/.env
appsettings.Production.json
```

Không đưa các giá trị thật sau lên Git:

- PostgreSQL password hoặc connection string production.
- JWT secret key.
- Gemini API key.
- SMTP password.
- Google client secret.
- Facebook app secret hoặc access token.
- PayOS API key/checksum key.
- Supabase service role key.

Được phép commit:

```text
AISAM-BE/AISAM.API/.env.example
```

Lưu ý: file `AISAM-BE/AISAM.API/appsettings.Development.json` hiện có connection string local. Chỉ dùng giá trị local và không đưa secret production vào file này.

## 4. Yêu Cầu Local Bắt Buộc

### Mục đích

<span style="color:red"><strong>REQUIRED NOW</strong></span>

Cần để chạy toàn bộ backend hiện tại, ngoại trừ gọi Gemini thật.

### Cần cài đặt

- .NET SDK 8.
- PostgreSQL.
- `dotnet-ef` nếu chạy migration thủ công.

### Lệnh kiểm tra

```powershell
dotnet --version
dotnet restore
dotnet build
dotnet test
```

Ket qua gan nhat ngay `2026-06-12`:

```text
dotnet build --no-restore
Build succeeded. 2 legacy migration naming warnings, 0 errors.

dotnet test --no-build
Passed. 226/226 tests passed.
```

## 5. PostgreSQL Database

### Mục đích

<span style="color:red"><strong>REQUIRED NOW</strong></span>

Lưu users, sessions, profiles, brands, products, contents, AI generations và conversations.

### Cần tạo tài khoản ở đâu

Không cần tài khoản cloud nếu dùng PostgreSQL local.

Có thể dùng PostgreSQL local, Docker hoặc dịch vụ cloud như Supabase Database, Neon, Azure PostgreSQL.

### Cần lấy key/token gì

Connection string PostgreSQL.

### Thêm vào file nào

Ưu tiên tạo file local:

```text
AISAM-BE/AISAM.API/.env
```

Ví dụ:

```env
CONNECTION_STRING=Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password
```

Hoặc thêm vào:

```text
AISAM-BE/AISAM.API/appsettings.Development.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password"
  }
}
```

### Lệnh migration

```powershell
cd D:\AISAM\AISAM-FINAL\AISAM-BE
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
```

### Lỗi thường gặp

- `password authentication failed`: sai username/password.
- `database does not exist`: chưa tạo database.
- `connection refused`: PostgreSQL service chưa chạy hoặc sai port.
- Thiếu connection string: API dùng database không hoạt động.

## 6. JWT Authentication

### Mục đích

<span style="color:red"><strong>REQUIRED NOW</strong></span>

Dùng để tạo access token, bảo vệ API và xác định user hiện tại.

### Cần tạo tài khoản ở đâu

Không cần tài khoản bên ngoài.

### Cần lấy key/token gì

Tự tạo JWT secret key dài tối thiểu 32 ký tự.

### Thêm vào file nào

```text
AISAM-BE/AISAM.API/.env
```

```env
JWT_SECRET_KEY=replace-with-a-long-random-secret-minimum-32-characters
JWT_ISSUER=AISAM.API
JWT_AUDIENCE=AISAM.Client
```

### Lỗi thường gặp

- Startup lỗi `JWT SecretKey is not configured`.
- API protected trả `401 Unauthorized`.
- Token bị reject do issuer/audience không khớp.

## 7. Gemini AI

### Mục đích

Sinh nội dung quảng cáo dạng text, cải thiện content và chat AI.

### Trạng thái

- <span style="color:gray"><strong>OPTIONAL</strong></span> để startup backend, chạy Content CRUD và chạy automated tests.
- <span style="color:red"><strong>REQUIRED FOR REAL AI TEST</strong></span> để gọi `/api/ai/*` với Gemini thật.

### Cần tạo tài khoản ở đâu

Google AI Studio:

```text
https://aistudio.google.com/
```

### Cần lấy key/token gì

- Gemini API key.

### Thêm vào file nào

```text
AISAM-BE/AISAM.API/.env
```

```env
GEMINI_API_KEY=your-real-gemini-api-key
GEMINI_MODEL=gemini-2.5-flash
GEMINI_MAX_TOKENS=2048
GEMINI_TEMPERATURE=0.7
```

Hoặc trong `appsettings.Development.json`:

```json
{
  "GeminiSettings": {
    "ApiKey": "your-real-gemini-api-key",
    "Model": "gemini-2.5-flash",
    "MaxTokens": 2048,
    "Temperature": 0.7
  }
}
```

### Config dùng để làm gì

- `GEMINI_API_KEY`: key gọi Gemini API.
- `GEMINI_MODEL`: model text generation.
- `GEMINI_MAX_TOKENS`: giới hạn token output.
- `GEMINI_TEMPERATURE`: mức sáng tạo của output.

### Lỗi thường gặp

- Thiếu API key: AI endpoint không gọi Gemini thật được.
- `401/403`: API key sai hoặc chưa được cấp quyền.
- Model không tồn tại hoặc không khả dụng cho tài khoản.
- Quota Google AI đã hết.

## 8. Active Profile Header

### Mục đích

<span style="color:red"><strong>REQUIRED NOW</strong></span> khi gọi Content, AI hoặc Conversation APIs.

Ngăn user thao tác dữ liệu của profile không thuộc tài khoản của họ.

### Thêm vào đâu

Trong Swagger/Postman, thêm hai header:

```text
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

### Áp dụng cho API nào

```text
/api/content
/api/ai
/api/conversations
```

### Lỗi thường gặp

- `401 Missing or invalid X-Profile-Id header`: thiếu header hoặc GUID sai.
- `404 Profile not found`: profile không tồn tại.
- `403 You are not allowed to use this profile`: profile không thuộc JWT user.

## 9. SMTP Email

### Mục đích

Gửi email verify và reset password thật.

### Trạng thái

<span style="color:gray"><strong>OPTIONAL / FUTURE</strong></span>

Auth local vẫn chạy khi chưa cấu hình SMTP; email service log warning và không gửi mail.

### Cần tạo tài khoản ở đâu

Gmail App Password, SendGrid, Brevo hoặc SMTP provider khác.

### Thêm vào file nào

```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@aisam.com
```

### Lỗi thường gặp

- Forgot password không gửi email thật.
- Gmail authentication failed nếu dùng password thường thay vì App Password.

## 10. Google OAuth

### Mục đích

Đăng nhập bằng Google.

### Trạng thái

<span style="color:gray"><strong>OPTIONAL / FUTURE</strong></span>

### Cần tạo tài khoản ở đâu

```text
https://console.cloud.google.com/
```

### Thêm vào file nào

```env
GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-google-client-secret
```

## 11. Facebook OAuth Và Graph API

### Mục đích

Kết nối Facebook Page và publish content.

### Trạng thái

<span style="color:red"><strong>REQUIRED IN NEXT PHASE</strong></span> khi bắt đầu Phase 6.

### Cần tạo tài khoản ở đâu

Meta for Developers:

```text
https://developers.facebook.com/
```

### Cần lấy key/token gì

- Facebook App ID.
- Facebook App Secret.
- OAuth redirect URI.
- Page access token hoặc OAuth flow để lấy page token.

### Thêm vào file nào

```env
FACEBOOK_APP_ID=your-facebook-app-id
FACEBOOK_APP_SECRET=your-facebook-app-secret
FACEBOOK_USE_SANDBOX=true
FACEBOOK_SANDBOX_ACCESS_TOKEN=your-sandbox-access-token
```

### Lỗi thường gặp

- OAuth redirect URI mismatch.
- Token hết hạn.
- Thiếu quyền `pages_manage_posts`.
- App chưa được cấp quyền cần thiết.

## 12. Supabase Storage

### Mục đích

Upload avatar, product image và media asset.

### Trạng thái

<span style="color:gray"><strong>OPTIONAL / FUTURE</strong></span>

Profile avatar file và product image upload hiện chưa bật trong MVP. Có thể dùng URL hoặc bỏ trống ảnh.

### Cần tạo tài khoản ở đâu

```text
https://supabase.com/
```

### Thêm vào file nào

```env
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_KEY=your-supabase-key
```

## 13. PayOS Payment
## 13. PayOS Payment

### Muc dich

Tao checkout subscription qua PayOS, nhan callback/webhook, cap nhat payment status va kich hoat subscription cho profile.

### Trang thai

<span style="color:red"><strong>REQUIRED ONLY FOR REAL PHASE 8 PAYMENT TEST</strong></span>

Khong bat buoc neu chi chay backend, auth, profile, content, AI mock/automated tests. Bat buoc neu muon test `POST /api/payment/checkout` voi checkout URL that tu PayOS.

### Can tao tai khoan o dau

```text
https://payos.vn/
```

### Can lay key/token gi

```text
Client ID
API Key
Checksum Key
Return URL
Cancel URL
Webhook URL neu PayOS dashboard yeu cau
```

### Them vao file nao

Them vao:

```text
AISAM-BE/AISAM.API/.env
```

```env
PAYOS_CLIENT_ID=your-payos-client-id
PAYOS_API_KEY=your-payos-api-key
PAYOS_CHECKSUM_KEY=your-payos-checksum-key
PAYOS_BASE_URL=https://api-merchant.payos.vn
PAYOS_RETURN_URL=http://localhost:3000/payment/success
PAYOS_CANCEL_URL=http://localhost:3000/payment/cancel
```

### API lien quan

```text
POST /api/payment/checkout
POST /api/payment/callback
POST /api/payment/webhook
GET  /api/payment/history
GET  /api/payment/subscription/current
GET  /api/quota/profile/{profileId}
```

### Bao mat callback/webhook

```text
Callback va webhook PayOS bat buoc phai co signature hop le.
Request thieu signature se bi tu choi voi PAYOS_SIGNATURE_REQUIRED.
Khong tu tao request PAID thu cong neu khong tao dung HMAC bang PAYOS_CHECKSUM_KEY.
```

### Loi thuong gap neu thieu config

```text
503 PAYOS_NOT_CONFIGURED
```

Nghia la backend da chay dung, nhung chua co `PAYOS_CLIENT_ID`, `PAYOS_API_KEY`, hoac `PAYOS_CHECKSUM_KEY`.

```text
503 PAYOS_URL_NOT_CONFIGURED
```

Nghia la thieu `PAYOS_RETURN_URL` hoac `PAYOS_CANCEL_URL`.

```text
502 PAYOS_CHECKOUT_FAILED
```

Nghia la backend da goi PayOS nhung PayOS tra loi loi. Kiem tra key, amount, return/cancel URL va merchant status tren PayOS.

```text
400 PAYOS_SIGNATURE_REQUIRED
```

Nghia la callback/webhook khong co signature. Kiem tra Webhook URL tren PayOS dashboard va khong dung payload PAID tu tao thu cong.

## Active Workspace Header

### Muc dich

Xac dinh Workspace dang hoat dong cho cac API Workspace-scoped va kiem tra user la active member.

### Trang thai

<span style="color:red"><strong>REQUIRED FOR WORKSPACE-SCOPED APIs</strong></span>

### Header can them

```http
Authorization: Bearer your-access-token
X-Workspace-Id: your-workspace-guid
```

Lay Workspace ID bang:

```text
GET /api/workspaces
```

### Loi thuong gap

```text
401 Missing or invalid X-Workspace-Id header.
403 You are not a member of this workspace.
404 Workspace not found.
```

## 14. Ví Dụ `.env`

Tạo file local:

```text
AISAM-BE/AISAM.API/.env
```

```env
# REQUIRED NOW
CONNECTION_STRING=Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password
JWT_SECRET_KEY=replace-with-a-long-random-secret-minimum-32-characters
JWT_ISSUER=AISAM.API
JWT_AUDIENCE=AISAM.Client

# REQUIRED FOR REAL AI TEST
GEMINI_API_KEY=your-real-gemini-api-key
GEMINI_MODEL=gemini-2.5-flash
GEMINI_MAX_TOKENS=2048
GEMINI_TEMPERATURE=0.7

# OPTIONAL / FUTURE
FRONTEND_BASE_URL=http://localhost:3000
SMTP_HOST=
SMTP_PORT=587
SMTP_USERNAME=
SMTP_PASSWORD=
FROM_EMAIL=
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=

# REQUIRED IN NEXT PHASE: Facebook integration
FACEBOOK_APP_ID=
FACEBOOK_APP_SECRET=
FACEBOOK_USE_SANDBOX=true
FACEBOOK_SANDBOX_ACCESS_TOKEN=

# OPTIONAL / FUTURE
SUPABASE_URL=
SUPABASE_KEY=
PAYOS_CLIENT_ID=
PAYOS_API_KEY=
PAYOS_CHECKSUM_KEY=
PAYOS_BASE_URL=https://api-merchant.payos.vn
PAYOS_RETURN_URL=http://localhost:3000/payment/success
PAYOS_CANCEL_URL=http://localhost:3000/payment/cancel
```

## 15. Chạy Backend Local

```powershell
cd D:\AISAM\AISAM-FINAL\AISAM-BE
dotnet restore
dotnet build
dotnet test
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
dotnet run --project AISAM.API
```

Swagger thường mở tại URL in ra trong terminal:

```text
http://localhost:{port}/swagger/index.html
```

Health check:

```text
GET http://localhost:{port}/api/health
```

## 16. Checklist Setup

### Chạy backend hiện tại

- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> Cài .NET SDK 8.
- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> Cài và chạy PostgreSQL.
- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> Tạo database local.
- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> Thêm `CONNECTION_STRING`.
- [ ] <span style="color:red"><strong>REQUIRED NOW</strong></span> Thêm `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`.
- [ ] Chạy `dotnet restore`.
- [ ] Chạy `dotnet build`.
- [ ] Chạy `dotnet test`.
- [ ] Chạy migration.
- [ ] Chạy API và mở Swagger.
- [ ] Test register/login.
- [ ] Test Profile, Brand, Product.
- [ ] Test Content với `Authorization` và `X-Profile-Id`.
- [ ] <span style="color:red"><strong>REQUIRED FOR REAL PHASE 8 PAYMENT TEST</strong></span> Them `PAYOS_CLIENT_ID`, `PAYOS_API_KEY`, `PAYOS_CHECKSUM_KEY`, `PAYOS_BASE_URL`, `PAYOS_RETURN_URL`, `PAYOS_CANCEL_URL` neu test PayOS that.

### Test Gemini thật

- [ ] <span style="color:red"><strong>REQUIRED FOR REAL AI TEST</strong></span> Tạo Gemini API key.
- [ ] <span style="color:red"><strong>REQUIRED FOR REAL AI TEST</strong></span> Thêm `GEMINI_API_KEY`.
- [ ] Test `/api/ai/generate-draft`.
- [ ] Test `/api/ai/improve/{contentId}`.
- [ ] Test `/api/ai/chat`.

### Chuẩn bị Phase 6

- [ ] <span style="color:red"><strong>REQUIRED IN NEXT PHASE</strong></span> Tạo Meta/Facebook app.
- [ ] <span style="color:red"><strong>REQUIRED IN NEXT PHASE</strong></span> Thêm Facebook App ID và App Secret.
- [ ] <span style="color:red"><strong>REQUIRED IN NEXT PHASE</strong></span> Cấu hình OAuth redirect URI.
- [ ] Chuẩn bị Page test và quyền publish cần thiết.
