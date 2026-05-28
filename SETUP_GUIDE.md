# AISAM Backend Setup Guide

Tài liệu này ghi lại toàn bộ cấu hình thủ công cần thiết để chạy backend AISAM `.NET 8`.

Backend mới đang nằm tại:

```text
D:\AISAM\AISAM-FINAL\AISAM-BE
```

Source cũ baseline:

```text
D:\AISAM\PRN232-AISAM\PRN232_Backend
```

> Mục tiêu: developer mới clone repo về có thể biết cần cấu hình gì, lấy key/token ở đâu, thêm vào file nào, config đó dùng để làm gì và lỗi thường gặp nếu thiếu.

## 1. Trạng thái cấu hình hiện tại

### Hiện tại cần ngay trong code mới

Ở tiến độ hiện tại, backend mới mới hoàn thành:

- Solution/project skeleton.
- API host tối thiểu.
- Swagger.
- CORS mặc định.
- Exception middleware.
- Validation filter.
- Health check API.
- Shared response/config/DTO nền tảng.
- Domain entities/enums nền tảng.
- `AisamContext` và migrations cũ từ baseline.
- Đăng ký `DbContext` có điều kiện khi có connection string.

Vì vậy để chạy backend hiện tại ở mức API host + health check, **chưa cần database/API key/OAuth/Storage/Payment**.

Tuy nhiên từ Task 2.3, backend đã có `AisamContext` và migrations. Nếu muốn chạy migration hoặc bắt đầu các module cần database như Auth/Profile/Brand/Product, PostgreSQL connection string sẽ là **REQUIRED**.

REQUIRED hiện tại:

- .NET SDK.
- Restore NuGet packages.
- Không bắt buộc connection string nếu chỉ chạy Swagger/Health API.

REQUIRED khi chạy migration hoặc module dùng DB:

- PostgreSQL database.

Optional/Future feature hiện tại:

- JWT settings.
- SMTP email.
- Google OAuth.
- Facebook OAuth/Graph API.
- Gemini API key.
- PayOS payment.
- Supabase Storage.

Các config optional này sẽ trở thành REQUIRED khi migrate module tương ứng.

## 2. Không commit secrets lên Git

REQUIRED

Tuyệt đối không commit các giá trị thật sau lên Git:

- Database connection string production.
- JWT secret key.
- Google client secret.
- Facebook app secret.
- Facebook access token.
- Gemini API key.
- PayOS API key/checksum key.
- Supabase service role key.
- SMTP password/app password.

Nên dùng:

- `.env` cho local development.
- `appsettings.Development.json` chỉ chứa placeholder hoặc giá trị local không nhạy cảm.
- Secret manager/CI environment variables cho staging/production.

Không nên commit:

```text
.env
appsettings.Production.json
```

Nên commit:

```text
.env.example
appsettings.Development.example.json
```

## 3. .NET SDK

### Mục đích

REQUIRED

Dùng để restore, build, test và chạy backend `.NET 8`.

### Cần tạo tài khoản ở đâu

Không cần tài khoản.

### Cần lấy key/token gì

Không cần key/token.

### Thêm vào file nào

Không cần thêm file config.

### Ví dụ lệnh

```text
dotnet --version
dotnet restore
dotnet build
dotnet test
dotnet run --project AISAM.API
```

### Lỗi thường gặp nếu thiếu config

- `dotnet is not recognized`: chưa cài .NET SDK hoặc PATH lỗi.
- Restore lỗi NuGet: chưa có mạng hoặc sandbox/firewall chặn `api.nuget.org`.

## 4. PostgreSQL Database

### Mục đích

REQUIRED nếu chạy migration hoặc test module có database.

Optional nếu chỉ chạy API host hiện tại, Swagger và `/api/health`.

Dùng để lưu:

- Users.
- Sessions.
- Profiles.
- Brands.
- Products.
- Contents.
- AI generations.
- Social accounts/integrations.
- Payments/subscriptions.
- Notifications.

Source cũ dùng PostgreSQL qua EF Core/Npgsql.

Tiến độ hiện tại:

- Đã copy `AISAM.Repositories/AISAMContext.cs`.
- Đã copy `AISAM.Repositories/Migrations/*`.
- `Program.cs` đã đọc connection string từ `.env` hoặc `appsettings.Development.json`.
- `DbContext` chỉ được đăng ký khi connection string có giá trị.
- Chưa bật auto migration khi app start.

### Cần tạo tài khoản ở đâu

Chọn một trong các cách:

- Local PostgreSQL.
- Docker PostgreSQL.
- Cloud PostgreSQL như Supabase Database, Neon, Azure PostgreSQL, Render, Railway.

### Cần lấy key/token gì

Cần connection string PostgreSQL.

Ví dụ:

```text
Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password
```

### Thêm vào file nào

Ưu tiên thêm vào file:

```text
AISAM-BE/.env
```

```env
CONNECTION_STRING=Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password
```

Hoặc trong `AISAM-BE/AISAM.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password"
  }
}
```

### Config dùng để làm gì

- `ConnectionStrings:DefaultConnection`: EF Core dùng để kết nối database.
- `CONNECTION_STRING`: được repo mới ưu tiên đọc từ environment variable để override appsettings.

### Lỗi thường gặp nếu thiếu config

- `dotnet ef database update` không chạy được vì không có connection string.
- `connection string is not configured`.
- `password authentication failed`.
- `database does not exist`.
- `No connection could be made because the target machine actively refused it`.

Lưu ý hiện tại:

- Nếu connection string rỗng, API vẫn chạy được cho Swagger và `/api/health`.
- Các API cần database sẽ chỉ hoạt động sau khi cấu hình PostgreSQL và chạy migration.

### Lệnh kiểm tra

```text
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
```

Sau khi chạy migration, kiểm tra API host:

```text
dotnet run --project AISAM.API --urls http://localhost:5081
GET http://localhost:5081/api/health
```

## 5. JWT Authentication

### Mục đích

Future feature, sẽ là REQUIRED khi migrate Auth module.

Dùng để:

- Tạo access token.
- Xác thực API protected bằng Bearer token.
- Lưu session/refresh token.

### Cần tạo tài khoản ở đâu

Không cần tài khoản bên ngoài.

### Cần lấy key/token gì

Cần tự tạo JWT secret key đủ dài.

Yêu cầu:

- Tối thiểu 32 ký tự.
- Không dùng secret mặc định.
- Không commit lên Git.

### Thêm vào file nào

Ưu tiên `.env`:

```env
JWT_SECRET_KEY=replace-with-a-long-random-secret-minimum-32-characters
JWT_ISSUER=AISAM.API
JWT_AUDIENCE=AISAM.Client
```

Hoặc `AISAM-BE/AISAM.API/appsettings.Development.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "replace-with-a-long-random-secret-minimum-32-characters",
    "Issuer": "AISAM.API",
    "Audience": "AISAM.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  }
}
```

### Config dùng để làm gì

- `SecretKey`: key ký token.
- `Issuer`: hệ thống phát hành token.
- `Audience`: client hợp lệ nhận token.
- `AccessTokenExpirationMinutes`: thời gian sống access token.
- `RefreshTokenExpirationDays`: thời gian sống refresh token.

### Lỗi thường gặp nếu thiếu config

- API startup lỗi: `JWT SecretKey is not configured`.
- Login tạo token lỗi.
- API protected luôn trả `401 Unauthorized`.
- Token bị reject do issuer/audience không khớp.

## 6. CORS và Frontend Base URL

### Mục đích

Optional hiện tại, REQUIRED khi frontend gọi backend.

Dùng để cho phép frontend gọi API backend từ domain/port khác.

### Cần tạo tài khoản ở đâu

Không cần tài khoản.

### Cần lấy key/token gì

Không cần key/token.

### Thêm vào file nào

`.env`:

```env
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:3001
FRONTEND_BASE_URL=http://localhost:3000
```

Hoặc `appsettings.Development.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  },
  "FrontendSettings": {
    "BaseUrl": "http://localhost:3000"
  }
}
```

### Config dùng để làm gì

- `Cors:AllowedOrigins`: danh sách origin được gọi API.
- `FRONTEND_BASE_URL`: URL frontend dùng cho OAuth callback, payment return/cancel URL, email links.

### Lỗi thường gặp nếu thiếu config

- Browser báo CORS error.
- OAuth redirect sai URL.
- Payment return về sai frontend.
- Link email verify/reset sai domain.

## 7. SMTP Email

### Mục đích

Future feature, REQUIRED khi bật email verification/forgot password.

Dùng để gửi:

- Email xác thực tài khoản.
- Email reset password.
- Email thông báo nếu sau này cần.

Source cũ dùng SMTP qua `EmailSettings`.

### Cần tạo tài khoản ở đâu

Có thể dùng:

- Gmail SMTP với App Password.
- SendGrid.
- Mailgun.
- Brevo.
- SMTP server nội bộ.

### Cần lấy key/token gì

Với Gmail:

- Gmail address.
- App Password.

### Thêm vào file nào

`.env`:

```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@aisam.com
```

Hoặc `appsettings.Development.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@aisam.com",
    "FromName": "AISAM",
    "EnableSsl": true
  }
}
```

### Config dùng để làm gì

- `SmtpHost`: host SMTP.
- `SmtpPort`: port SMTP.
- `SmtpUsername`: username đăng nhập SMTP.
- `SmtpPassword`: password/app password.
- `FromEmail`: email gửi đi.
- `FromName`: tên người gửi.
- `EnableSsl`: bật TLS/SSL.

### Lỗi thường gặp nếu thiếu config

- Register lỗi khi gửi verification email.
- Forgot password không gửi mail.
- Gmail báo authentication failed do dùng password thường thay vì App Password.
- SMTP blocked bởi firewall.

## 8. Google OAuth

### Mục đích

Future feature, REQUIRED khi bật Google login.

Source cũ có `GoogleLoginAsync` trong AuthService và `GoogleSettings`.

### Cần tạo tài khoản ở đâu

Google Cloud Console:

```text
https://console.cloud.google.com/
```

### Cần lấy key/token gì

Cần:

- Google OAuth Client ID.
- Google OAuth Client Secret.

### Thêm vào file nào

`.env`:

```env
GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-google-client-secret
```

Hoặc `appsettings.Development.json`:

```json
{
  "GoogleSettings": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret",
    "RedirectUri": "http://localhost:3000/auth/google/callback",
    "RequiredScopes": [
      "openid",
      "email",
      "profile"
    ]
  }
}
```

### Config dùng để làm gì

- `ClientId`: xác thực Google ID token.
- `ClientSecret`: dùng cho OAuth flow nếu backend exchange code.
- `RedirectUri`: URL callback.
- `RequiredScopes`: quyền cần xin từ Google.

### Lỗi thường gặp nếu thiếu config

- Google login trả `Invalid Google token`.
- Audience không khớp do dùng sai ClientId.
- Redirect URI mismatch.

## 9. Facebook OAuth và Graph API

### Mục đích

Future feature, REQUIRED khi bật:

- Kết nối Facebook account.
- Lấy Facebook Pages.
- Publish bài lên Facebook Page.
- Facebook Ads/Marketing API trong phase sau.

Source cũ ưu tiên Facebook cho MVP social publishing.

### Cần tạo tài khoản ở đâu

Meta for Developers:

```text
https://developers.facebook.com/
```

Cần tạo:

- Meta App.
- Facebook Login product.
- Test user/page hoặc business app nếu dùng thật.

### Cần lấy key/token gì

Cần:

- Facebook App ID.
- Facebook App Secret.
- Page access token hoặc OAuth flow để lấy page token.
- Optional sandbox access token nếu test sandbox.
- Optional ad account id nếu làm Facebook Ads.

### Thêm vào file nào

`.env`:

```env
FACEBOOK_APP_ID=your-facebook-app-id
FACEBOOK_APP_SECRET=your-facebook-app-secret
FACEBOOK_USE_SANDBOX=true
FACEBOOK_SANDBOX_ACCESS_TOKEN=your-sandbox-access-token
```

Hoặc `appsettings.Development.json`:

```json
{
  "FacebookSettings": {
    "AppId": "your-facebook-app-id",
    "AppSecret": "your-facebook-app-secret",
    "RedirectUri": "http://localhost:5283/api/social-auth/facebook/callback",
    "GraphApiVersion": "v24.0",
    "BaseUrl": "https://graph.facebook.com",
    "OAuthUrl": "https://www.facebook.com",
    "UseSandbox": true,
    "Sandbox": {
      "AccessToken": "your-sandbox-access-token",
      "AdAccountId": "your-ad-account-id",
      "PageId": "your-page-id",
      "UserId": "your-user-id"
    },
    "RequiredPermissions": [
      "pages_manage_posts",
      "pages_read_engagement",
      "pages_show_list",
      "pages_manage_metadata",
      "public_profile"
    ]
  }
}
```

### Config dùng để làm gì

- `AppId`: nhận diện app Facebook.
- `AppSecret`: xác thực app.
- `RedirectUri`: callback OAuth.
- `GraphApiVersion`: version Graph API.
- `UseSandbox`: bật sandbox/test mode.
- `Sandbox:AccessToken`: token test.
- `RequiredPermissions`: quyền cần xin.

### Lỗi thường gặp nếu thiếu config

- Không tạo được Facebook auth URL.
- OAuth callback lỗi.
- Không lấy được danh sách Page.
- Publish lỗi do thiếu `pages_manage_posts`.
- Token hết hạn.
- App chưa được cấp quyền qua review.
- Redirect URI mismatch.

## 10. Gemini AI

### Mục đích

Future feature, REQUIRED khi bật AI generation/refinement.

Source cũ dùng Gemini cho:

- Generate draft content.
- Improve content.
- Chat AI.
- Lưu AI generation.

### Cần tạo tài khoản ở đâu

Google AI Studio hoặc Google Cloud:

```text
https://aistudio.google.com/
https://console.cloud.google.com/
```

### Cần lấy key/token gì

Cần:

- Gemini API key.

### Thêm vào file nào

`.env`:

```env
GEMINI_API_KEY=your-gemini-api-key
```

Hoặc `appsettings.Development.json`:

```json
{
  "Gemini": {
    "ApiKey": "your-gemini-api-key",
    "Model": "gemini-2.5-flash",
    "MaxTokens": 8192,
    "Temperature": 0.7
  }
}
```

### Config dùng để làm gì

- `ApiKey`: key gọi Gemini API.
- `Model`: model AI sử dụng.
- `MaxTokens`: giới hạn output token.
- `Temperature`: độ sáng tạo của output.

### Lỗi thường gặp nếu thiếu config

- AI generate trả lỗi thiếu API key.
- API call bị `401/403`.
- Model không tồn tại hoặc không được cấp quyền.
- Quota Google AI hết.

## 11. PayOS Payment

### Mục đích

Future feature, REQUIRED khi bật payment/subscription.

Source cũ dùng PayOS cho:

- Tạo checkout link.
- Confirm payment.
- Webhook.
- Active subscription.

### Cần tạo tài khoản ở đâu

PayOS:

```text
https://payos.vn/
```

### Cần lấy key/token gì

Cần:

- Client ID.
- API Key.
- Checksum Key.

### Thêm vào file nào

`.env`:

```env
PAYOS_CLIENT_ID=your-payos-client-id
PAYOS_API_KEY=your-payos-api-key
PAYOS_CHECKSUM_KEY=your-payos-checksum-key
```

Hoặc `appsettings.Development.json`:

```json
{
  "PayOS": {
    "ClientId": "your-payos-client-id",
    "ApiKey": "your-payos-api-key",
    "ChecksumKey": "your-payos-checksum-key"
  }
}
```

### Config dùng để làm gì

- `ClientId`: định danh merchant/app.
- `ApiKey`: gọi PayOS API.
- `ChecksumKey`: ký request và verify webhook.

### Lỗi thường gặp nếu thiếu config

- Không tạo được checkout link.
- PayOS trả unauthorized.
- Webhook verify fail.
- Payment thành công nhưng subscription không active nếu webhook/confirm lỗi.

## 12. Supabase Storage

### Mục đích

Future feature, REQUIRED khi bật upload/storage.

Source cũ dùng Supabase **chỉ cho Storage**, không dùng Supabase Auth.

Dùng để:

- Upload image/video/assets.
- Lấy public/signed URL.
- Quản lý bucket.

### Cần tạo tài khoản ở đâu

Supabase:

```text
https://supabase.com/
```

### Cần lấy key/token gì

Cần:

- Supabase URL.
- Supabase anon key hoặc service role key tùy cách upload.

Không commit service role key.

### Thêm vào file nào

`.env`:

```env
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_KEY=your-supabase-key
```

### Config dùng để làm gì

- `SUPABASE_URL`: URL project.
- `SUPABASE_KEY`: key để backend gọi Supabase Storage.

### Lỗi thường gặp nếu thiếu config

- Storage service không đăng ký được.
- Upload lỗi unauthorized.
- Bucket không tồn tại.
- Public URL không truy cập được do bucket private.

## 13. Optional/Future Feature Configs

Các config dưới đây chưa bắt buộc cho MVP backend local hiện tại:

| Config | Trạng thái | Khi nào cần |
| --- | --- | --- |
| PostgreSQL connection string | Optional cho Swagger/Health, REQUIRED cho migration/module DB | Khi chạy `dotnet ef database update`, Auth, Profile, Brand, Product |
| Google OAuth | Optional / Future feature | Khi bật Google login |
| Facebook OAuth | Optional hiện tại, REQUIRED ở social phase | Khi bật Facebook connect/publish |
| Facebook Ads sandbox | Optional / Future feature | Khi làm campaign/ad set/ad |
| Gemini | Optional hiện tại, REQUIRED ở AI phase | Khi bật AI generate/refine |
| PayOS | Optional hiện tại, REQUIRED ở payment phase | Khi bật subscription/payment |
| Supabase Storage | Optional / Future feature | Khi bật upload file |
| SMTP | Optional hiện tại, REQUIRED ở auth email phase | Khi bật email verify/reset password |

## 14. Ví dụ `.env`

Tạo file local:

```text
AISAM-BE/.env
```

Ví dụ:

```env
# Database - Optional for Swagger/Health, REQUIRED for migration and DB modules
CONNECTION_STRING=Host=localhost;Port=5432;Database=aisam_dev;Username=postgres;Password=your_password

# JWT - Future REQUIRED from Auth phase
JWT_SECRET_KEY=replace-with-a-long-random-secret-minimum-32-characters
JWT_ISSUER=AISAM.API
JWT_AUDIENCE=AISAM.Client

# Frontend/CORS - REQUIRED when frontend starts calling backend
FRONTEND_BASE_URL=http://localhost:3000
CORS_ALLOWED_ORIGINS=http://localhost:3000

# SMTP - Future feature
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@aisam.com

# Google OAuth - Future feature
GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-google-client-secret

# Facebook - Future social phase
FACEBOOK_APP_ID=your-facebook-app-id
FACEBOOK_APP_SECRET=your-facebook-app-secret
FACEBOOK_USE_SANDBOX=true
FACEBOOK_SANDBOX_ACCESS_TOKEN=your-sandbox-access-token

# Gemini - Future AI phase
GEMINI_API_KEY=your-gemini-api-key

# PayOS - Future payment phase
PAYOS_CLIENT_ID=your-payos-client-id
PAYOS_API_KEY=your-payos-api-key
PAYOS_CHECKSUM_KEY=your-payos-checksum-key

# Supabase Storage - Future upload/storage phase
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_KEY=your-supabase-key
```

## 15. Ví dụ `appsettings.Development.json`

File:

```text
AISAM-BE/AISAM.API/appsettings.Development.json
```

Ví dụ:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  },
  "FrontendSettings": {
    "BaseUrl": "http://localhost:3000"
  },
  "JwtSettings": {
    "SecretKey": "replace-with-a-long-random-secret-minimum-32-characters",
    "Issuer": "AISAM.API",
    "Audience": "AISAM.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@aisam.com",
    "FromName": "AISAM",
    "EnableSsl": true
  },
  "GoogleSettings": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret",
    "RedirectUri": "http://localhost:3000/auth/google/callback",
    "RequiredScopes": [
      "openid",
      "email",
      "profile"
    ]
  },
  "FacebookSettings": {
    "AppId": "your-facebook-app-id",
    "AppSecret": "your-facebook-app-secret",
    "RedirectUri": "http://localhost:5283/api/social-auth/facebook/callback",
    "GraphApiVersion": "v24.0",
    "BaseUrl": "https://graph.facebook.com",
    "OAuthUrl": "https://www.facebook.com",
    "UseSandbox": true,
    "Sandbox": {
      "AccessToken": "your-sandbox-access-token",
      "AdAccountId": "your-ad-account-id",
      "PageId": "your-page-id",
      "UserId": "your-user-id"
    },
    "RequiredPermissions": [
      "pages_manage_posts",
      "pages_read_engagement",
      "pages_show_list",
      "pages_manage_metadata",
      "public_profile"
    ]
  },
  "Gemini": {
    "ApiKey": "your-gemini-api-key",
    "Model": "gemini-2.5-flash",
    "MaxTokens": 8192,
    "Temperature": 0.7
  },
  "PayOS": {
    "ClientId": "your-payos-client-id",
    "ApiKey": "your-payos-api-key",
    "ChecksumKey": "your-payos-checksum-key"
  }
}
```

## 16. Setup checklist

### Chạy backend hiện tại

- [ ] Cài .NET SDK.
- [ ] Clone repo.
- [ ] Vào thư mục `AISAM-BE`.
- [ ] Chạy `dotnet restore`.
- [ ] Chạy `dotnet build`.
- [ ] Chạy `dotnet test`.
- [ ] Chạy `dotnet run --project AISAM.API`.
- [ ] Mở Swagger.

### Khi tới phase database/auth

- [ ] Tạo PostgreSQL database.
- [ ] Thêm `CONNECTION_STRING`.
- [ ] Hoặc thêm `ConnectionStrings:DefaultConnection` trong `AISAM-BE/AISAM.API/appsettings.Development.json`.
- [ ] Chạy `dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API`.
- [ ] Thêm `JwtSettings` hoặc env JWT.
- [ ] Test register/login.

### Khi tới phase email

- [ ] Tạo SMTP account hoặc Gmail App Password.
- [ ] Thêm `EmailSettings`.
- [ ] Test email verification.
- [ ] Test forgot/reset password.

### Khi tới phase AI

- [ ] Tạo Gemini API key.
- [ ] Thêm `GEMINI_API_KEY`.
- [ ] Test AI generate draft.
- [ ] Test AI refine content.

### Khi tới phase social publishing

- [ ] Tạo Meta/Facebook app.
- [ ] Thêm `FACEBOOK_APP_ID`.
- [ ] Thêm `FACEBOOK_APP_SECRET`.
- [ ] Cấu hình redirect URI.
- [ ] Xin quyền Page cần thiết.
- [ ] Test get auth URL.
- [ ] Test link Page.
- [ ] Test publish content.

### Khi tới phase payment

- [ ] Tạo PayOS merchant/app.
- [ ] Thêm `PAYOS_CLIENT_ID`.
- [ ] Thêm `PAYOS_API_KEY`.
- [ ] Thêm `PAYOS_CHECKSUM_KEY`.
- [ ] Test create checkout link.
- [ ] Test confirm payment/webhook.

### Khi tới phase storage

- [ ] Tạo Supabase project.
- [ ] Tạo bucket.
- [ ] Thêm `SUPABASE_URL`.
- [ ] Thêm `SUPABASE_KEY`.
- [ ] Test upload file.

## 17. Quick local smoke test

Hiện tại, sau khi chạy API:

```text
dotnet run --project AISAM.API --urls http://localhost:5081
```

Kiểm tra:

```text
GET http://localhost:5081/swagger/index.html
```

Expected:

```text
HTTP 200
Swagger UI mở được
```

Health check hiện tại:

```text
GET http://localhost:5081/api/health
```

Expected:

```text
HTTP 200
Response có success = true, message = "AISAM backend is ready."
```

Nếu chưa cấu hình database:

```text
Swagger và /api/health vẫn chạy được.
dotnet ef database update sẽ bị skip/fail cho tới khi thêm connection string.
```
