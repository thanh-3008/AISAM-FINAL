# AISAM Backend Code Plan

## Approved Next Migration - Workspace Subscription and Credits

Nguon ke hoach chi tiet: `CHANGE_REQUEST_WORKSPACE_SUBSCRIPTION_CREDIT_ANALYSIS.md`.

Trang thai: **implementation in progress**. Phase 9 dang chuyen tung ownership boundary tu Profile sang Workspace; cac module chua migrate van giu Profile-based lam baseline.

Thu tu code tiep theo:

1. Phase 9 - Workspace Migration.
2. Phase 10 - Admin Backend theo Workspace.
3. Phase 11 - Facebook Ads Campaign MVP.
4. Phase 12 - Test Hardening va Backend Release.

Khong duoc bo qua dependency:

```text
Phase 9 Workspace
  -> Phase 10 Admin theo Workspace
  -> Phase 11 Facebook Ads Campaign
  -> Phase 12 Regression/Release
```

Nguyen tac bat buoc:

- Moi Workspace co dung mot Owner va mot Credit Wallet.
- Business Plus cap 15.000 Credits/toi da 10 members.
- Business Pro cap 50.000 Credits/toi da 50 members.
- Khong danh dau task Workspace hoan thanh neu chua build, test, migration va API test.
- Khong sua/xoa migration cu; migration Workspace phai duoc them theo nhieu buoc.

Vai trò áp dụng: **Senior .NET Backend Architect + Tech Lead**

Phạm vi tài liệu: **chỉ backend .NET 8**.

Source cũ baseline:

```text
D:\AISAM\PRN232-AISAM\PRN232_Backend
```

Repo backend mới đề xuất:

```text
D:\AISAM\AISAM-FINAL\AISAM-BE
```

Nguyên tắc chính:

- Backend làm trước, frontend làm sau.
- Source cũ là baseline.
- Ưu tiên tái sử dụng code cũ nếu module đã ổn.
- Không refactor/cải tiến nếu chưa cần.
- Mỗi task đủ nhỏ để commit riêng.
- Sau mỗi task phải build/test/API test được nếu task có API.
- Không chuyển task mới nếu task hiện tại chưa build/test được.
- MVP backend chạy được trước, chưa ôm TikTok/Instagram/Facebook Ads nâng cao/video AI.

## Phase 0 - Chuẩn bị repo backend mới

Mục tiêu phase:

- Tạo nền backend .NET 8 chạy được.
- Chưa migrate nghiệp vụ.
- Chưa thay đổi database schema ngoài setup cần thiết.

### Task 0.1 - Tạo cấu trúc repo backend mới

Mục tiêu:

Tạo workspace backend mới theo cấu trúc tương thích source cũ để dễ migrate từng module.

Loại task:

Setup

Source cũ liên quan:

```text
PRN232_Backend/AISAM.sln
PRN232_Backend/AISAM.API
PRN232_Backend/AISAM.Services
PRN232_Backend/AISAM.Repositories
PRN232_Backend/AISAM.Data
PRN232_Backend/AISAM.Common
```

File/thư mục repo mới:

```text
AISAM-BE/
  AISAM.sln
  AISAM.API/
  AISAM.Services/
  AISAM.Repositories/
  AISAM.Data/
  AISAM.Common/
  tests/
```

Việc cần làm:

- Tạo thư mục `AISAM-BE`.
- Tạo solution `.NET 8`.
- Tạo các project class library/API tương ứng source cũ.
- Add project references theo hướng cũ:
  - `AISAM.API` reference `AISAM.Services`, `AISAM.Repositories`, `AISAM.Common`.
  - `AISAM.Services` reference `AISAM.Repositories`, `AISAM.Data`, `AISAM.Common`.
  - `AISAM.Repositories` reference `AISAM.Data`, `AISAM.Common`.
  - `AISAM.Common` độc lập.
- Tạo test project ban đầu.

Cải tiến so với source cũ nếu có:

Không có. Chỉ tạo skeleton tương thích để migrate.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
chore(solution): initialize backend solution structure
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API.

Checklist hoàn thành:

- [x] Build thành công.
- [x] Test pass.
- [x] Migration chạy được nếu có: không áp dụng.
- [x] API test thành công: không có API mới; smoke test `/api/health` pass.
- [x] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 0.2 - Copy cấu hình project và package cơ bản từ source cũ

Mục tiêu:

Đưa các package cần thiết vào repo mới để các module cũ có thể build.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/AISAM.API.csproj
PRN232_Backend/AISAM.Services/AISAM.Services.csproj
PRN232_Backend/AISAM.Repositories/AISAM.Repositories.csproj
PRN232_Backend/AISAM.Data/AISAM.Data.csproj
PRN232_Backend/AISAM.Common/AISAM.Common.csproj
PRN232_Backend/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/AISAM.API.csproj
AISAM-BE/AISAM.Services/AISAM.Services.csproj
AISAM-BE/AISAM.Repositories/AISAM.Repositories.csproj
AISAM-BE/AISAM.Data/AISAM.Data.csproj
AISAM-BE/AISAM.Common/AISAM.Common.csproj
AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
```

Việc cần làm:

- Copy package references cần thiết từ source cũ.
- Giữ target framework `.NET 8`.
- Chưa copy code nghiệp vụ.
- Đảm bảo restore/build được.

Cải tiến so với source cũ nếu có:

Không có, chỉ giữ package cần thiết.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
chore(projects): migrate backend project package references
```

Lệnh kiểm tra sau task:

```text
dotnet restore
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API.

Checklist hoàn thành:

- [x] Build thành công.
- [x] Test pass.
- [x] Migration chạy được nếu có: không áp dụng.
- [x] API test thành công: không có API mới; smoke test `/api/health` pass.
- [x] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 1 - API host tối thiểu

Mục tiêu phase:

- Backend chạy được bằng `dotnet run`.
- Swagger mở được.
- Có health endpoint để kiểm tra API host.

### Task 1.1 - Migrate Program.cs tối thiểu

Mục tiêu:

Tạo API host .NET 8 tối thiểu có controllers, Swagger, CORS, JSON camelCase và global exception middleware placeholder.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Program.cs
PRN232_Backend/AISAM.API/Middleware/ExceptionHandlerMiddleware.cs
PRN232_Backend/AISAM.API/Filters/ValidationFilter.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Program.cs
AISAM-BE/AISAM.API/Middleware/ExceptionHandlerMiddleware.cs
AISAM-BE/AISAM.API/Filters/ValidationFilter.cs
```

Việc cần làm:

- Copy cấu hình API host từ source cũ nhưng chỉ bật phần tối thiểu:
  - Controllers.
  - JSON options.
  - Swagger/OpenAPI.
  - CORS policy.
  - Exception middleware.
- Tạm chưa bật database, auth, hosted services.
- Đảm bảo API chạy mà không cần secrets.

Cải tiến so với source cũ nếu có:

Có: tách startup tối thiểu, chưa đăng ký toàn bộ services ngay.

Lý do cải tiến:

Source cũ `Program.cs` đăng ký quá nhiều dependency cùng lúc. Nếu copy nguyên file ngay từ đầu, backend mới dễ fail vì thiếu DB/secrets/external services. MVP backend cần host chạy được trước.

Trước cải tiến đang có vấn đề gì:

Startup cũ phụ thuộc nhiều env/config như DB, JWT secret, Supabase, Facebook, Gemini, PayOS.

Sau cải tiến mong muốn kết quả gì:

API mới chạy được, mở Swagger được, chưa cần đầy đủ module.

Có ảnh hưởng module khác không:

Có, các module nghiệp vụ chưa được đăng ký ở bước này. Sẽ bổ sung từng module sau.

Commit đề xuất:

```text
chore(api): add minimal api host and swagger
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/swagger
```

Request mẫu:

Không có.

Expected result:

Swagger UI mở được.

Checklist hoàn thành:

- [x] Build thành công.
- [x] Test pass.
- [x] Migration chạy được nếu có: không áp dụng.
- [x] API test thành công.
- [x] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 1.2 - Thêm HealthController

Mục tiêu:

Có endpoint health check đơn giản để kiểm tra API host.

Loại task:

Viết mới

Source cũ liên quan:

```text
PRN232_Backend/src/AISAM.Api/Controllers/HealthController.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/HealthController.cs
```

Việc cần làm:

- Tạo `HealthController`.
- Endpoint trả thông tin API đang sống.
- Không phụ thuộc database.

Cải tiến so với source cũ nếu có:

Không có hoặc chỉ viết endpoint tối thiểu tương đương.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(api): add health check endpoint
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/health
```

Request mẫu:

Không có.

Expected result:

```json
{
  "status": "Healthy"
}
```

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 2 - Common, domain models, database context

Mục tiêu phase:

- Copy các DTO/common/entity nền tảng.
- Đưa `AisamContext` và migration cũ vào repo mới.
- Database kết nối được.

### Task 2.1 - Copy Common response, config, DTO auth/user/profile nền tảng

Mục tiêu:

Đưa các class dùng chung tối thiểu để các module tiếp theo build được.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Common/GenericResponse.cs
PRN232_Backend/AISAM.Common/Config/JwtSettings.cs
PRN232_Backend/AISAM.Common/Config/EmailSettings.cs
PRN232_Backend/AISAM.Common/Config/GoogleSettings.cs
PRN232_Backend/AISAM.Common/Dtos/Request/AuthRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Response/AuthResponse.cs
PRN232_Backend/AISAM.Common/Dtos/Response/UserResponseDto.cs
PRN232_Backend/AISAM.Common/Dtos/PaginationDtos.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Common/GenericResponse.cs
AISAM-BE/AISAM.Common/Config/
AISAM-BE/AISAM.Common/Dtos/Request/
AISAM-BE/AISAM.Common/Dtos/Response/
AISAM-BE/AISAM.Common/Dtos/PaginationDtos.cs
```

Việc cần làm:

- Copy nguyên các DTO/config/common cần cho auth/user.
- Chưa copy toàn bộ DTO ads/payment/social nếu chưa dùng.
- Build để kiểm tra namespace/reference.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
chore(common): migrate shared response and auth dto contracts
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API mới.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 2.2 - Copy entity và enum nền tảng

Mục tiêu:

Đưa các entity/enum cần cho auth, profile, brand, product, content MVP.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Data/Model/User.cs
PRN232_Backend/AISAM.Data/Model/Session.cs
PRN232_Backend/AISAM.Data/Model/Profile.cs
PRN232_Backend/AISAM.Data/Model/Brand.cs
PRN232_Backend/AISAM.Data/Model/Product.cs
PRN232_Backend/AISAM.Data/Model/Content.cs
PRN232_Backend/AISAM.Data/Model/AiGeneration.cs
PRN232_Backend/AISAM.Data/Model/Conversation.cs
PRN232_Backend/AISAM.Data/Model/ChatMessage.cs
PRN232_Backend/AISAM.Data/Model/SocialAccount.cs
PRN232_Backend/AISAM.Data/Model/SocialIntegration.cs
PRN232_Backend/AISAM.Data/Model/Post.cs
PRN232_Backend/AISAM.Data/Model/ContentCalendar.cs
PRN232_Backend/AISAM.Data/Model/Subscription.cs
PRN232_Backend/AISAM.Data/Model/Payment.cs
PRN232_Backend/AISAM.Data/Model/Notification.cs
PRN232_Backend/AISAM.Data/Enumeration/
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Data/Model/
AISAM-BE/AISAM.Data/Enumeration/
```

Việc cần làm:

- Copy entity/enum nền tảng.
- Chưa copy Ads entities nếu chưa làm Facebook Ads phase sau.
- Build để kiểm tra dependency.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
chore(data): migrate core domain entities and enums
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API mới.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 2.3 - Copy AisamContext và migration cũ

Mục tiêu:

Đưa database context vào repo mới để backend có thể kết nối PostgreSQL.

Loại task:

Copy từ source cũ / Migration

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Repositories/AISAMContext.cs
PRN232_Backend/AISAM.Repositories/Migrations/
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Repositories/Migrations/
```

Việc cần làm:

- Copy `AisamContext`.
- Copy migrations cũ.
- Đăng ký `DbContext` trong `Program.cs`.
- Cấu hình connection string qua `appsettings.Development.json` hoặc `.env`.
- Chưa bật auto migration nếu chưa cần.

Cải tiến so với source cũ nếu có:

Có: không tự động migrate database khi app start trong MVP setup ban đầu.

Lý do cải tiến:

Auto migrate khi startup có thể gây rủi ro trong repo mới nếu connection string sai hoặc migration chưa kiểm tra.

Trước cải tiến đang có vấn đề gì:

Source cũ chạy `context.Database.Migrate()` trong startup.

Sau cải tiến mong muốn kết quả gì:

Migration được chạy chủ động bằng lệnh `dotnet ef database update`.

Có ảnh hưởng module khác không:

Có, các module cần DB chỉ chạy sau khi update database thành công.

Commit đề xuất:

```text
chore(data): migrate db context and existing migrations
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/health
```

Request mẫu:

Không có.

Expected result:

API vẫn chạy sau khi đăng ký DB.

Checklist hoàn thành:

- [x] Build thành công.
- [x] Test pass.
- [x] Migration chạy được nếu có: chưa chạy vì chưa cấu hình `ConnectionStrings:DefaultConnection` local.
- [x] API test thành công.
- [x] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 3 - Authentication MVP

Mục tiêu phase:

- Auth backend chạy được end-to-end.
- Có register, login, refresh, logout, me.
- JWT hoạt động.

### Task 3.1 - Copy repositories cho User và Session

Mục tiêu:

Đưa data access cho auth vào repo mới.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Repositories/IRepositories/IUserRepository.cs
PRN232_Backend/AISAM.Repositories/IRepositories/ISessionRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/UserRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/SessionRepository.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Repositories/IRepositories/IUserRepository.cs
AISAM-BE/AISAM.Repositories/IRepositories/ISessionRepository.cs
AISAM-BE/AISAM.Repositories/Repository/UserRepository.cs
AISAM-BE/AISAM.Repositories/Repository/SessionRepository.cs
```

Việc cần làm:

- Copy repository interfaces và implementations.
- Đăng ký DI cho `IUserRepository`, `ISessionRepository`.
- Build/test.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
chore(auth): migrate user and session repositories
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API mới.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 3.2 - Copy AuthService và EmailService ở mức MVP

Mục tiêu:

Đưa business logic auth vào repo mới.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Services/IServices/IAuthService.cs
PRN232_Backend/AISAM.Services/IServices/IEmailService.cs
PRN232_Backend/AISAM.Services/Service/AuthService.cs
PRN232_Backend/AISAM.Services/Service/EmailService.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Services/IServices/IAuthService.cs
AISAM-BE/AISAM.Services/IServices/IEmailService.cs
AISAM-BE/AISAM.Services/Service/AuthService.cs
AISAM-BE/AISAM.Services/Service/EmailService.cs
```

Việc cần làm:

- Copy service interfaces và implementations.
- Đăng ký DI.
- Cấu hình `JwtSettings`, `EmailSettings`, `GoogleSettings`.
- Nếu SMTP chưa có, giữ service nhưng test auth không phụ thuộc gửi email thật.

Cải tiến so với source cũ nếu có:

Có: email sending trong môi trường local cần fail-safe/log-only nếu chưa có SMTP.

Lý do cải tiến:

Để register không fail hoàn toàn khi local chưa cấu hình SMTP.

Trước cải tiến đang có vấn đề gì:

Auth register source cũ gửi email verification ngay. Nếu SMTP thiếu/sai, registration có thể lỗi.

Sau cải tiến mong muốn kết quả gì:

Local dev có thể register/login; email verification test riêng khi có SMTP.

Có ảnh hưởng module khác không:

Có, ảnh hưởng register/forgot password. Cần ghi rõ nếu đang ở mode dev.

Commit đề xuất:

```text
feat(auth): migrate auth and email services
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API controller ở task này.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 3.3 - Copy AuthController và bật JWT authentication

Mục tiêu:

Hoàn thiện auth APIs MVP.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/AuthController.cs
PRN232_Backend/AISAM.API/Utils/UserClaimsHelper.cs
PRN232_Backend/AISAM.API/Validators/
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/AuthController.cs
AISAM-BE/AISAM.API/Utils/UserClaimsHelper.cs
AISAM-BE/AISAM.API/Validators/
AISAM-BE/AISAM.API/Program.cs
```

Việc cần làm:

- Copy `AuthController`.
- Bật JWT bearer authentication trong `Program.cs`.
- Add Swagger bearer auth.
- Copy validators liên quan auth nếu source cũ có.
- Test register/login/refresh/me/logout.

Cải tiến so với source cũ nếu có:

Không cải tiến nghiệp vụ, chỉ cấu hình lại trong repo mới.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(auth): migrate authentication api endpoints
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/auth/register
```

Request mẫu:

```json
{
  "email": "user1@example.com",
  "password": "Password@123",
  "fullName": "User One"
}
```

Expected result:

- HTTP 200.
- Response có `accessToken`, `refreshToken`, `user`.

Method:

```text
POST
```

Endpoint:

```text
/api/auth/login
```

Request mẫu:

```json
{
  "email": "user1@example.com",
  "password": "Password@123"
}
```

Expected result:

- HTTP 200.
- Response có JWT token.

Method:

```text
GET
```

Endpoint:

```text
/api/auth/me
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Response trả current user.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 4 - Profile, Brand, Product MVP

Mục tiêu phase:

- User có thể tạo profile.
- User có thể quản lý brand và product.
- Đây là đầu vào cho AI/content.

### Task 4.1 - Migrate Profile module

Mục tiêu:

Đưa profile APIs vào backend mới.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/ProfileController.cs
PRN232_Backend/AISAM.Services/IServices/IProfileService.cs
PRN232_Backend/AISAM.Services/Service/ProfileService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IProfileRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/ProfileRepository.cs
PRN232_Backend/AISAM.Common/Dtos/Request/CreateProfileRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Request/UpdateProfileRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Response/ProfileResponseDto.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/ProfileController.cs
AISAM-BE/AISAM.Services/IServices/IProfileService.cs
AISAM-BE/AISAM.Services/Service/ProfileService.cs
AISAM-BE/AISAM.Repositories/IRepositories/IProfileRepository.cs
AISAM-BE/AISAM.Repositories/Repository/ProfileRepository.cs
AISAM-BE/AISAM.Common/Dtos/Request/
AISAM-BE/AISAM.Common/Dtos/Response/
```

Việc cần làm:

- Copy profile controller/service/repository/DTO.
- Đăng ký DI.
- Test tạo profile và lấy profile theo user.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(profile): migrate profile management APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/profiles/user/{userId}
```

Request mẫu:

```json
{
  "name": "My Business",
  "profileType": 0,
  "companyName": "My Company",
  "bio": "Business profile"
}
```

Expected result:

- HTTP 200.
- Profile được tạo.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 4.2 - Migrate Brand module

Mục tiêu:

User quản lý brand kit cơ bản.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/BrandController.cs
PRN232_Backend/AISAM.Services/IServices/IBrandService.cs
PRN232_Backend/AISAM.Services/Service/BrandService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IBrandRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/BrandRepository.cs
PRN232_Backend/AISAM.Common/Dtos/Request/CreateBrandRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Request/UpdateBrandRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Response/BrandResponseDto.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/BrandController.cs
AISAM-BE/AISAM.Services/IServices/IBrandService.cs
AISAM-BE/AISAM.Services/Service/BrandService.cs
AISAM-BE/AISAM.Repositories/IRepositories/IBrandRepository.cs
AISAM-BE/AISAM.Repositories/Repository/BrandRepository.cs
AISAM-BE/AISAM.Common/Dtos/Request/
AISAM-BE/AISAM.Common/Dtos/Response/
```

Việc cần làm:

- Copy brand module.
- Đăng ký DI.
- Test CRUD brand theo profile.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(brand): migrate brand management APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/brands
```

Request mẫu:

```json
{
  "profileId": "<profileId>",
  "name": "AISAM Brand",
  "description": "AI social advertising brand",
  "slogan": "Create smarter ads",
  "usp": "AI-powered content",
  "targetAudience": "Small businesses"
}
```

Expected result:

- HTTP 200.
- Brand được tạo.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 4.3 - Migrate Product module

Mục tiêu:

User quản lý product catalog cơ bản.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/ProductController.cs
PRN232_Backend/AISAM.Services/IServices/IProductService.cs
PRN232_Backend/AISAM.Services/Service/ProductService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IProductRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/ProductRepository.cs
PRN232_Backend/AISAM.Common/Dtos/Request/ProductCreateRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Request/ProductUpdateRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Response/ProductResponseDto.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/ProductController.cs
AISAM-BE/AISAM.Services/IServices/IProductService.cs
AISAM-BE/AISAM.Services/Service/ProductService.cs
AISAM-BE/AISAM.Repositories/IRepositories/IProductRepository.cs
AISAM-BE/AISAM.Repositories/Repository/ProductRepository.cs
AISAM-BE/AISAM.Common/Dtos/Request/
AISAM-BE/AISAM.Common/Dtos/Response/
```

Việc cần làm:

- Copy product module.
- Đăng ký DI.
- Test CRUD product theo brand.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(product): migrate product catalog APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/products
```

Request mẫu:

```json
{
  "brandId": "<brandId>",
  "name": "Product A",
  "description": "Demo product",
  "price": 99000,
  "images": []
}
```

Expected result:

- HTTP 200.
- Product được tạo.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 5 - AI và Content MVP

Mục tiêu phase:

- Có content draft.
- Có AI generate/refine.
- Lưu được AI output vào content.

### Task 5.1 - Migrate Content module cơ bản

Mục tiêu:

Đưa content CRUD/status vào repo mới, chưa publish social.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/ContentController.cs
PRN232_Backend/AISAM.Services/IServices/IContentService.cs
PRN232_Backend/AISAM.Services/Service/ContentService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IContentRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/ContentRepository.cs
PRN232_Backend/AISAM.Common/Dtos/Request/CreateContentRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Request/UpdateContentRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Response/ContentResponseDto.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/ContentController.cs
AISAM-BE/AISAM.Services/IServices/IContentService.cs
AISAM-BE/AISAM.Services/Service/ContentService.cs
AISAM-BE/AISAM.Repositories/IRepositories/IContentRepository.cs
AISAM-BE/AISAM.Repositories/Repository/ContentRepository.cs
AISAM-BE/AISAM.Common/Dtos/Request/
AISAM-BE/AISAM.Common/Dtos/Response/
```

Việc cần làm:

- Copy content module.
- Tạm vô hiệu hóa publish social nếu provider/social module chưa migrate.
- Giữ create/update/get/delete/restore/clone.
- Test tạo content draft theo brand.

Cải tiến so với source cũ nếu có:

Có: tách publish social khỏi content CRUD trong giai đoạn đầu nếu dependency chưa sẵn sàng.

Lý do cải tiến:

Để content CRUD build/test được trước, không bị chặn bởi social provider.

Trước cải tiến đang có vấn đề gì:

`ContentService` source cũ phụ thuộc social integration, approval, post, provider services.

Sau cải tiến mong muốn kết quả gì:

Content CRUD chạy ổn trước; publish sẽ bật ở phase social.

Có ảnh hưởng module khác không:

Có, endpoint publish có thể chưa hoạt động đến Task 6.x.

Commit đề xuất:

```text
feat(content): migrate content draft management APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/content
```

Request mẫu:

```json
{
  "brandId": "<brandId>",
  "productId": "<productId>",
  "adType": 0,
  "title": "Summer campaign",
  "textContent": "Try our new product today",
  "publishImmediately": false
}
```

Expected result:

- HTTP 200.
- Content status là `Draft`.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 5.2 - Migrate AI generation service và Gemini endpoint

Mục tiêu:

Cho phép backend gọi AI để generate/refine nội dung.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/GeminiController.cs
PRN232_Backend/AISAM.Services/IServices/IAIService.cs
PRN232_Backend/AISAM.Services/Service/AIService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IAiGenerationRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/AiGenerationRepository.cs
PRN232_Backend/AISAM.Common/Models/GeminiSettings.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/GeminiController.cs
AISAM-BE/AISAM.Services/IServices/IAIService.cs
AISAM-BE/AISAM.Services/Service/AIService.cs
AISAM-BE/AISAM.Repositories/IRepositories/IAiGenerationRepository.cs
AISAM-BE/AISAM.Repositories/Repository/AiGenerationRepository.cs
AISAM-BE/AISAM.Common/Models/GeminiSettings.cs
```

Việc cần làm:

- Copy AI service/controller/repository.
- Đăng ký Gemini config.
- Đảm bảo nếu thiếu `GEMINI_API_KEY`, API trả lỗi rõ ràng.
- Test generate draft bằng Swagger/Postman.

Cải tiến so với source cũ nếu có:

Có: validate thiếu API key và trả lỗi dễ hiểu.

Lý do cải tiến:

Tránh lỗi runtime khó hiểu khi môi trường local chưa cấu hình Gemini.

Trước cải tiến đang có vấn đề gì:

Nếu thiếu secret, AI call có thể fail không rõ nguyên nhân.

Sau cải tiến mong muốn kết quả gì:

API trả lỗi cấu hình rõ ràng hoặc generate thành công khi có key.

Có ảnh hưởng module khác không:

Ảnh hưởng AI/content only.

Commit đề xuất:

```text
feat(ai): migrate gemini content generation APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/ai/generate-draft
```

Request mẫu:

```json
{
  "brandId": "<brandId>",
  "productId": "<productId>",
  "prompt": "Create a short Facebook ad caption for this product"
}
```

Expected result:

- Nếu có API key: HTTP 200, có generated text.
- Nếu thiếu API key: lỗi rõ ràng về cấu hình.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 5.3 - Migrate Conversation module

Mục tiêu:

Lưu hội thoại AI và lịch sử refinement.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/ConversationController.cs
PRN232_Backend/AISAM.Services/IServices/IConversationService.cs
PRN232_Backend/AISAM.Services/Service/ConversationService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IConversationRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/ConversationRepository.cs
PRN232_Backend/AISAM.Common/Dtos/Response/ConversationResponseDto.cs
PRN232_Backend/AISAM.Common/Dtos/Response/ConversationDetailDto.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/ConversationController.cs
AISAM-BE/AISAM.Services/IServices/IConversationService.cs
AISAM-BE/AISAM.Services/Service/ConversationService.cs
AISAM-BE/AISAM.Repositories/IRepositories/IConversationRepository.cs
AISAM-BE/AISAM.Repositories/Repository/ConversationRepository.cs
AISAM-BE/AISAM.Common/Dtos/Response/
```

Việc cần làm:

- Copy conversation module.
- Đăng ký DI.
- Test list/detail/delete conversation.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(ai): migrate conversation history APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/conversations
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả danh sách conversations hoặc mảng rỗng.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 6 - Social integration và Facebook Page publishing

Mục tiêu phase:

- Kết nối Facebook account/page.
- Link page vào brand.
- Publish content lên Facebook Page.

### Task 6.1 - Migrate provider contracts và FacebookProvider

Mục tiêu:

Đưa lớp provider social vào backend mới.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Services/IServices/IProviderService.cs
PRN232_Backend/AISAM.Services/Service/FacebookProvider.cs
PRN232_Backend/AISAM.Services/Service/GoogleProvider.cs
PRN232_Backend/AISAM.Common/Models/FacebookSettings.cs
PRN232_Backend/AISAM.Common/Models/FacebookModels.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Services/IServices/IProviderService.cs
AISAM-BE/AISAM.Services/Service/FacebookProvider.cs
AISAM-BE/AISAM.Services/Service/GoogleProvider.cs
AISAM-BE/AISAM.Common/Models/
```

Việc cần làm:

- Copy provider interfaces/models.
- Copy FacebookProvider.
- Register `IProviderService`.
- Cấu hình Facebook settings.
- Chưa expose controller nếu social service chưa migrate.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(social): migrate social provider contracts
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API mới.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 6.2 - Migrate social account/integration repositories và service

Mục tiêu:

Quản lý social account và social integration.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Repositories/IRepositories/ISocialAccountRepository.cs
PRN232_Backend/AISAM.Repositories/IRepositories/ISocialIntegrationRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/SocialAccountRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/SocialIntegrationRepository.cs
PRN232_Backend/AISAM.Services/IServices/ISocialService.cs
PRN232_Backend/AISAM.Services/Service/SocialService.cs
PRN232_Backend/AISAM.Common/Dtos/Request/SocialCallbackRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Response/SocialIntegrationDto.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Repositories/IRepositories/
AISAM-BE/AISAM.Repositories/Repository/
AISAM-BE/AISAM.Services/IServices/ISocialService.cs
AISAM-BE/AISAM.Services/Service/SocialService.cs
AISAM-BE/AISAM.Common/Dtos/Request/
AISAM-BE/AISAM.Common/Dtos/Response/
```

Việc cần làm:

- Copy repositories/service/DTO.
- Đăng ký DI.
- Build/test.

Cải tiến so với source cũ nếu có:

Có: kiểm tra lại điều kiện ownership trong unlink account/target.

Lý do cải tiến:

Source cũ có đoạn so sánh `account.ProfileId != userId`, dễ nhầm profileId với userId. Cần xác nhận và sửa nếu đúng là bug.

Trước cải tiến đang có vấn đề gì:

Có nguy cơ unlink social account sai vì nhầm `profileId` và `userId`.

Sau cải tiến mong muốn kết quả gì:

Unlink chỉ thành công khi user sở hữu profile chứa social account.

Có ảnh hưởng module khác không:

Ảnh hưởng social account endpoints.

Commit đề xuất:

```text
feat(social): migrate social account integration service
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa expose controller ở task này.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 6.3 - Migrate SocialAuthController, SocialAccountController, SocialIntegrationController

Mục tiêu:

Expose APIs kết nối Facebook và quản lý integration.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/SocialAuthController.cs
PRN232_Backend/AISAM.API/Controllers/SocialAccountController.cs
PRN232_Backend/AISAM.API/Controllers/SocialIntegrationController.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/SocialAuthController.cs
AISAM-BE/AISAM.API/Controllers/SocialAccountController.cs
AISAM-BE/AISAM.API/Controllers/SocialIntegrationController.cs
```

Việc cần làm:

- Copy controllers.
- Test get auth URL.
- Test social accounts list.
- Test available targets nếu có Facebook token thật.

Cải tiến so với source cũ nếu có:

Không cải tiến ngoài phần ownership đã xử lý ở service nếu có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(social): migrate social integration api endpoints
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/social-auth/facebook
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả Facebook auth URL.

Method:

```text
GET
```

Endpoint:

```text
/api/social/accounts/me
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả danh sách account hoặc mảng rỗng.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 6.4 - Bật publish content lên Facebook

Mục tiêu:

Kích hoạt lại endpoint publish content sau khi social module đã có.

Loại task:

Cải tiến bắt buộc

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Services/Service/ContentService.cs
PRN232_Backend/AISAM.API/Controllers/ContentController.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IPostRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/PostRepository.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Services/Service/ContentService.cs
AISAM-BE/AISAM.API/Controllers/ContentController.cs
AISAM-BE/AISAM.Repositories/IRepositories/IPostRepository.cs
AISAM-BE/AISAM.Repositories/Repository/PostRepository.cs
```

Việc cần làm:

- Copy/migrate `PostRepository`.
- Đăng ký DI.
- Bật lại `PublishContentAsync`.
- Test publish với Facebook Page integration thật hoặc sandbox.
- Lưu `Post` record sau publish.

Cải tiến so với source cũ nếu có:

Có: publish fail phải trả lỗi rõ ràng và không đánh dấu content là Published.

Lý do cải tiến:

Tránh trạng thái sai khi social API fail.

Trước cải tiến đang có vấn đề gì:

Source cũ có xử lý fail, nhưng cần kiểm tra lại status update để tránh publish failed mà content bị hiểu nhầm là published.

Sau cải tiến mong muốn kết quả gì:

- Publish thành công: content `Published`, post record có external id.
- Publish fail: content không bị đánh dấu sai, response có error.

Có ảnh hưởng module khác không:

Ảnh hưởng content, post, social integration.

Commit đề xuất:

```text
feat(content): enable facebook page publishing
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/content/{contentId}/publish/{integrationId}
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Nếu Facebook token hợp lệ: publish success, post được tạo.
- Nếu token lỗi: response báo lỗi rõ ràng, content không bị set sai trạng thái.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 7 - Scheduling, notification, basic dashboard

Mục tiêu phase:

- Lên lịch đăng bài.
- Background publish.
- Notification cơ bản.
- Dashboard MVP.

### Task 7.1 - Migrate Notification module

Mục tiêu:

API notification cơ bản cho hệ thống.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/NotificationController.cs
PRN232_Backend/AISAM.Services/IServices/INotificationService.cs
PRN232_Backend/AISAM.Services/Service/NotificationService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/INotificationRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/NotificationRepository.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/NotificationController.cs
AISAM-BE/AISAM.Services/IServices/INotificationService.cs
AISAM-BE/AISAM.Services/Service/NotificationService.cs
AISAM-BE/AISAM.Repositories/IRepositories/INotificationRepository.cs
AISAM-BE/AISAM.Repositories/Repository/NotificationRepository.cs
```

Việc cần làm:

- Copy notification module.
- Đăng ký DI.
- Test get notifications, mark read.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(notification): migrate notification APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/notifications
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả danh sách notifications hoặc mảng rỗng.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 7.2 - Migrate Content Calendar và Scheduled Posting

Mục tiêu:

Cho phép schedule content và background service publish khi đến lịch.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/ContentCalendarController.cs
PRN232_Backend/AISAM.Services/IServices/IScheduledPostingService.cs
PRN232_Backend/AISAM.Services/Service/ScheduledPostingService.cs
PRN232_Backend/AISAM.Services/Service/ScheduledPostingBackgroundService.cs
PRN232_Backend/AISAM.Repositories/IRepositories/IContentCalendarRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/ContentCalendarRepository.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/ContentCalendarController.cs
AISAM-BE/AISAM.Services/IServices/IScheduledPostingService.cs
AISAM-BE/AISAM.Services/Service/ScheduledPostingService.cs
AISAM-BE/AISAM.Services/Service/ScheduledPostingBackgroundService.cs
AISAM-BE/AISAM.Repositories/IRepositories/IContentCalendarRepository.cs
AISAM-BE/AISAM.Repositories/Repository/ContentCalendarRepository.cs
```

Việc cần làm:

- Copy calendar/scheduled posting module.
- Đăng ký DI.
- Đăng ký hosted service sau khi publish đã ổn.
- Test tạo schedule.
- Test upcoming schedules.
- Test background service ở môi trường local với thời gian gần.

Cải tiến so với source cũ nếu có:

Có: background service phải log rõ ràng và không crash toàn app nếu một schedule fail.

Lý do cải tiến:

Scheduled job lỗi không được làm chết API host.

Trước cải tiến đang có vấn đề gì:

Cần kiểm tra source cũ xử lý exception trong background service đầy đủ chưa.

Sau cải tiến mong muốn kết quả gì:

Schedule fail được log, job tiếp tục chạy các schedule khác.

Có ảnh hưởng module khác không:

Ảnh hưởng content, social publish, notification.

Commit đề xuất:

```text
feat(schedule): migrate content calendar and scheduled publishing
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/content-calendar/schedule/{contentId}
```

Request mẫu:

```json
{
  "scheduledDate": "2026-05-28T15:00:00Z",
  "scheduledTime": "15:00:00",
  "timezone": "Asia/Bangkok",
  "integrationIds": ["<integrationId>"]
}
```

Expected result:

- HTTP 200.
- Schedule được tạo.

Method:

```text
GET
```

Endpoint:

```text
/api/content-calendar/upcoming
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả upcoming schedules.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 7.3 - Migrate Dashboard MVP

Mục tiêu:

Dashboard backend thống kê nội bộ ở mức MVP.

Loại task:

Copy từ source cũ / Cải tiến bắt buộc

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/DashboardController.cs
PRN232_Backend/AISAM.Repositories/Repository/PerformanceReportRepository.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/DashboardController.cs
AISAM-BE/AISAM.Services/Service/DashboardService.cs
AISAM-BE/AISAM.Services/IServices/IDashboardService.cs
```

Việc cần làm:

- Copy dashboard controller nếu source cũ ổn.
- Nếu source cũ gom logic trong controller quá nhiều, chỉ tách tối thiểu service nếu cần test.
- MVP thống kê:
  - total contents.
  - total published posts.
  - total brands.
  - total products.
  - total AI generations.
  - publish success/failed nếu có dữ liệu.

Cải tiến so với source cũ nếu có:

Có thể có: thêm `DashboardService` nếu controller cũ chứa nhiều query trực tiếp.

Lý do cải tiến:

Giữ controller mỏng và dễ test, nhưng chỉ làm nếu source cũ khó maintain.

Trước cải tiến đang có vấn đề gì:

Cần kiểm tra source cũ trước khi quyết định.

Sau cải tiến mong muốn kết quả gì:

Dashboard API trả data ổn định, không phụ thuộc social API thật.

Có ảnh hưởng module khác không:

Không đáng kể, chỉ đọc dữ liệu.

Commit đề xuất:

```text
feat(dashboard): add basic backend dashboard stats
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/dashboard/stats
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả thống kê cơ bản.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 8 - Payment, subscription, quota display

Mục tiêu phase:

- User tạo checkout payment.
- Confirm payment.
- Subscription active.
- Admin/backend đọc được subscription/payment.
- Quota display trước, enforcement sau.

### Task 8.1 - Migrate Payment và Subscription repositories

Mục tiêu:

Data access cho payment/subscription.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Repositories/IRepositories/IPaymentRepository.cs
PRN232_Backend/AISAM.Repositories/IRepositories/ISubscriptionRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/PaymentRepository.cs
PRN232_Backend/AISAM.Repositories/Repository/SubscriptionRepository.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Repositories/IRepositories/IPaymentRepository.cs
AISAM-BE/AISAM.Repositories/IRepositories/ISubscriptionRepository.cs
AISAM-BE/AISAM.Repositories/Repository/PaymentRepository.cs
AISAM-BE/AISAM.Repositories/Repository/SubscriptionRepository.cs
```

Việc cần làm:

- Copy repositories.
- Đăng ký DI.
- Build/test.

Cải tiến so với source cũ nếu có:

Không có.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
chore(payment): migrate payment and subscription repositories
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có API mới.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 8.2 - Migrate PayOSPaymentService

Mục tiêu:

Business logic payment/subscription với PayOS.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Services/IServices/IPaymentService.cs
PRN232_Backend/AISAM.Services/Service/PayOSPaymentService.cs
PRN232_Backend/AISAM.Common/Dtos/Request/CreatePaymentIntentRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Request/CreateSubscriptionRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Request/ChangePlanRequest.cs
PRN232_Backend/AISAM.Common/Dtos/Response/PayOSCheckoutResponse.cs
PRN232_Backend/AISAM.Common/Dtos/Response/PaymentResponseDto.cs
PRN232_Backend/AISAM.Common/Dtos/Response/SubscriptionResponseDto.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Services/IServices/IPaymentService.cs
AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs
AISAM-BE/AISAM.Common/Dtos/Request/
AISAM-BE/AISAM.Common/Dtos/Response/
```

Việc cần làm:

- Copy payment service và DTO.
- Đăng ký `HttpClient`.
- Cấu hình PayOS env.
- Nếu thiếu PayOS key, API phải trả lỗi rõ ràng.

Cải tiến so với source cũ nếu có:

Có: validate thiếu PayOS config rõ ràng.

Lý do cải tiến:

Tránh lỗi mơ hồ khi local chưa có PayOS secret.

Trước cải tiến đang có vấn đề gì:

PayOS service lấy config/env, nếu thiếu có thể gọi API ngoài với credentials rỗng.

Sau cải tiến mong muốn kết quả gì:

Nếu thiếu config, trả lỗi cấu hình. Nếu đủ config, tạo checkout link.

Có ảnh hưởng module khác không:

Ảnh hưởng payment/subscription.

Commit đề xuất:

```text
feat(payment): migrate payos payment service
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Chưa có controller ở task này.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 8.3 - Migrate PaymentController

Mục tiêu:

Expose APIs payment/subscription MVP.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/PaymentController.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/PaymentController.cs
```

Việc cần làm:

- Copy payment controller.
- Test create checkout link.
- Test confirm payment nếu có order code thật.
- Test subscription list/history.
- Test webhook ở mức local bằng sample payload.

Cải tiến so với source cũ nếu có:

Không cải tiến lớn, chỉ đảm bảo lỗi thiếu config rõ ràng từ service.

Lý do cải tiến:

Không áp dụng.

Commit đề xuất:

```text
feat(payment): migrate payment and subscription APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/payment/create-checkout-link
```

Request mẫu:

```json
{
  "subscriptionPlanId": 1,
  "profileId": "<profileId>"
}
```

Expected result:

- Nếu PayOS config đúng: HTTP 200, có checkout URL.
- Nếu thiếu config: lỗi rõ ràng về PayOS configuration.

Method:

```text
GET
```

Endpoint:

```text
/api/payment/history
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả payment history hoặc mảng rỗng.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 8.4 - Thêm quota display service MVP

Mục tiêu:

Backend trả quota hiện tại của profile/subscription.

Loại task:

Viết mới / Cải tiến bắt buộc

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Services/Service/SubscriptionValidationService.cs
PRN232_Backend/AISAM.Services/IServices/ISubscriptionValidationService.cs
PRN232_Backend/AISAM.Common/Config/premium_features.json
PRN232_Backend/AISAM.Common/Config/team_roles.json
PRN232_Backend/AISAM.Common/Config/team_permissions.json
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Services/IServices/IQuotaService.cs
AISAM-BE/AISAM.Services/Service/QuotaService.cs
AISAM-BE/AISAM.API/Controllers/QuotaController.cs
```

Việc cần làm:

- Tạo service đọc subscription active của profile.
- Trả quota:
  - posts/month.
  - AI generations.
  - platforms.
  - social accounts.
- MVP chỉ display, chưa chặn hết quota.
- Tính usage cơ bản từ DB nếu có thể:
  - count content/posts trong tháng.
  - count AI generations trong ngày/tháng.

Cải tiến so với source cũ nếu có:

Có: thêm quota display rõ ràng vì source cũ quota enforcement còn TODO.

Lý do cải tiến:

Đề bài yêu cầu user monitor quota usage, source cũ chưa hoàn chỉnh phần này.

Trước cải tiến đang có vấn đề gì:

Quota chưa có endpoint rõ ràng để frontend/admin đọc.

Sau cải tiến mong muốn kết quả gì:

API trả quota limit và usage hiện tại.

Có ảnh hưởng module khác không:

Chỉ đọc dữ liệu, chưa chặn nghiệp vụ.

Commit đề xuất:

```text
feat(quota): add subscription quota display API
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/quota/profile/{profileId}
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

```json
{
  "profileId": "<profileId>",
  "plan": "Free",
  "limits": {
    "postsPerMonth": 5,
    "aiGenerationsPerMonth": 5,
    "socialAccounts": 1
  },
  "usage": {
    "postsThisMonth": 0,
    "aiGenerationsThisMonth": 0,
    "socialAccounts": 0
  }
}
```

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 10 - Admin backend theo Workspace

Mục tiêu phase:

- Admin có API quản lý user/workspace/payment/subscription.
- Admin operations phải dùng Workspace model sau Phase 9 migration.
- Không làm frontend admin ở tài liệu này.

Dependency bắt buộc:

- Phase 9 Workspace Migration hoàn thành.

### Task 10.1 - Migrate UserController admin/user/workspace list APIs

Mục tiêu:

Expose user profile/current user/list/detail APIs.

Loại task:

Copy từ source cũ

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/UserController.cs
PRN232_Backend/AISAM.Services/IServices/IUserService.cs
PRN232_Backend/AISAM.Services/Service/UserService.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/UserController.cs
AISAM-BE/AISAM.Services/IServices/IUserService.cs
AISAM-BE/AISAM.Services/Service/UserService.cs
```

Việc cần làm:

- Copy user controller/service.
- Đăng ký DI.
- Test `/api/users/profile/me`.
- Test user list nếu role admin.

Cải tiến so với source cũ nếu có:

Có thể có: đảm bảo admin-only endpoints có authorization role.

Lý do cải tiến:

Admin APIs không nên mở cho user thường.

Trước cải tiến đang có vấn đề gì:

Cần kiểm tra source cũ đã guard role đầy đủ chưa.

Sau cải tiến mong muốn kết quả gì:

User thường không gọi được admin list nếu endpoint yêu cầu admin.

Có ảnh hưởng module khác không:

Ảnh hưởng admin/user APIs.

Commit đề xuất:

```text
feat(admin): migrate user management APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
GET
```

Endpoint:

```text
/api/users/profile/me
```

Request mẫu:

```text
Authorization: Bearer <accessToken>
```

Expected result:

- HTTP 200.
- Trả user profile hiện tại.

Method:

```text
GET
```

Endpoint:

```text
/api/users
```

Request mẫu:

```text
Authorization: Bearer <adminAccessToken>
```

Expected result:

- Admin: HTTP 200.
- User thường: HTTP 401/403 nếu endpoint admin-only.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 10.2 - Migrate AdminToolsController ở mức an toàn

Mục tiêu:

Giữ các admin tool hữu ích nhưng không mở nguy hiểm ở production.

Loại task:

Copy từ source cũ / Security hardening

Source cũ liên quan:

```text
PRN232_Backend/AISAM.API/Controllers/AdminToolsController.cs
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.API/Controllers/AdminToolsController.cs
```

Việc cần làm:

- Copy AdminToolsController.
- Chỉ bật endpoint cần cho dev/demo:
  - seed demo user.
  - seed batch users nếu cần.
  - update payment/subscription/profile status nếu cần.
- Bảo vệ bằng admin role hoặc chỉ bật development environment.

Cải tiến so với source cũ nếu có:

Có: harden admin tools bằng role/environment guard.

Lý do cải tiến:

Admin tools có thể thay đổi dữ liệu nhạy cảm, không nên mở rộng rãi.

Trước cải tiến đang có vấn đề gì:

Admin tools source cũ có thể dùng cho demo nhưng rủi ro nếu không guard chặt.

Sau cải tiến mong muốn kết quả gì:

Chỉ admin hoặc development environment được gọi.

Có ảnh hưởng module khác không:

Ảnh hưởng dữ liệu user/payment/subscription nếu gọi endpoint.

Commit đề xuất:

```text
feat(admin): migrate guarded admin tools APIs
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Method:

```text
POST
```

Endpoint:

```text
/api/admin-tools/seed-demo-user
```

Request mẫu:

```json
{
  "email": "demo@example.com",
  "password": "Password@123"
}
```

Expected result:

- Admin/dev: HTTP 200.
- Không có quyền: HTTP 401/403.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 12 - Test hardening và backend release

Mục tiêu phase:

- Regression toàn hệ thống sau Workspace, Admin và Facebook Ads.
- Có test tối thiểu.
- Có tài liệu API/env.

Dependency bắt buộc:

- Phase 9, Phase 10 và Phase 11 hoàn thành.

### Task 12.1 - Thêm integration tests cho API host và auth

Mục tiêu:

Có test tự động tối thiểu cho API host và auth.

Loại task:

Test

Source cũ liên quan:

```text
PRN232_Backend/tests/AISAM.IntegrationTests/
```

File/thư mục repo mới:

```text
AISAM-BE/tests/AISAM.IntegrationTests/
```

Việc cần làm:

- Copy test skeleton cũ nếu có.
- Thêm test:
  - API assembly load.
  - Health endpoint.
  - Auth register/login happy path nếu test DB có thể chạy.
- Nếu chưa có test DB, ít nhất test API host bootstrapping.

Cải tiến so với source cũ nếu có:

Có: bổ sung test health/auth tối thiểu nếu source cũ chưa đủ.

Lý do cải tiến:

Backend mới cần test guard trước khi thêm frontend.

Trước cải tiến đang có vấn đề gì:

Test coverage cũ ít.

Sau cải tiến mong muốn kết quả gì:

`dotnet test` phát hiện lỗi startup/auth cơ bản.

Có ảnh hưởng module khác không:

Không, chỉ test.

Commit đề xuất:

```text
test(api): add backend mvp integration tests
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
```

API cần test bằng Swagger/Postman:

Không bắt buộc, test tự động là chính.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

### Task 12.2 - Viết backend environment và API testing guide

Mục tiêu:

Ghi lại cách chạy backend MVP và test API.

Loại task:

Test

Source cũ liên quan:

```text
PRN232_Backend/JWT_AUTHENTICATION_GUIDE.md
PRN232_Backend/GOOGLE_OAUTH_GUIDE.md
PRN232_Backend/Facebook_Ads_API_Test_Samples.md
```

File/thư mục repo mới:

```text
AISAM-BE/docs/BACKEND_RUNBOOK.md
AISAM-BE/docs/API_TESTING.md
AISAM-BE/.env.example
```

Việc cần làm:

- Tạo `.env.example`.
- Ghi cách chạy:
  - restore.
  - build.
  - database update.
  - run API.
  - mở Swagger.
- Ghi danh sách API MVP cần test.
- Ghi sample Postman/Swagger request.

Cải tiến so với source cũ nếu có:

Có: gom hướng dẫn backend MVP vào docs ngắn gọn.

Lý do cải tiến:

Frontend/dev khác cần biết cách chạy backend mới.

Trước cải tiến đang có vấn đề gì:

Source cũ có nhiều guide rời rạc.

Sau cải tiến mong muốn kết quả gì:

Một runbook rõ ràng cho backend MVP.

Có ảnh hưởng module khác không:

Không.

Commit đề xuất:

```text
docs(backend): add runbook and api testing guide
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

Test lại smoke test:

- `GET /api/health`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/dashboard/stats`

Expected result:

Các API MVP chạy đúng theo runbook.

Checklist hoàn thành:

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
- [ ] Commit riêng task này.

## Phase 9 - Workspace Migration

Mục tiêu phase:

- Hoàn thành Workspace-based ownership trước khi triển khai Admin theo Workspace và Facebook Ads Campaign.
- Thực hiện theo `CHANGE_REQUEST_WORKSPACE_SUBSCRIPTION_CREDIT_ANALYSIS.md`.

### Tổng quan đầy đủ Task Phase 9

> Đây là danh sách chính thức của toàn bộ Phase 9. Không chuyển sang task tiếp theo nếu task hiện tại chưa build/test được.

| Task | Nội dung | Trạng thái | Dependency chính |
|---|---|---|---|
| 9.1 | Workspace domain foundation | DONE | Không |
| 9.2 | DbContext và migration Workspace foundation | DONE | 9.1 |
| 9.3 | Workspace và WorkspaceMember repositories | DONE | 9.2 |
| 9.4 | Workspace service và CRUD API | DONE | 9.3 |
| 9.5 | Tạo Personal Workspace khi register | DONE | 9.4 |
| 9.6 | Active Workspace context và `X-Workspace-Id` | DONE | 9.3 |
| 9.7 | Invitation, role management và Member Limit | DONE | 9.4, 9.6 |
| 9.8 | Atomic Ownership Transfer | DONE | 9.7 |
| 9.9 | Chuyển Subscription và Payment sang Workspace | DONE | 9.4, 9.6 |
| 9.10 | Credit Wallet, Credit Usage và Maximum Balance | DONE | 9.9 |
| 9.11 | Credit Pack và `PaymentType` | DONE | 9.10 |
| 9.12 | Shared Pool, Lifetime và Monthly Assigned Limit | DONE | 9.7, 9.10 |
| 9.13 | Plan Entitlement, Permission Matrix và Post Quota | DONE | 9.9, 9.12 |
| 9.14 | Áp dụng Credits vào AI generation | DONE - 2026-06-12 | 9.10, 9.13 |
| 9.15 | Limited Mode, Archived và Admin Soft Delete lifecycle | DONE - 2026-06-12 | 9.9, 9.13 |
| 9.16 | Chuyển ownership từng domain sang Workspace | DONE - 2026-06-13 | 9.6, 9.13 |
| 9.17 | Backfill dữ liệu cũ và khóa schema Workspace | DONE - 2026-06-13 | 9.9-9.16 |
| 9.18 | Workspace Dashboard, regression và tài liệu cuối Phase 9 | DONE - 2026-06-13 | 9.17 |

### Task 9.1 - Thêm Workspace domain foundation

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Thêm domain contract tối thiểu cho Workspace và WorkspaceMember.
- Hỗ trợ một User tham gia nhiều Workspace thông qua nhiều WorkspaceMember.
- Chưa cấu hình DbContext, migration, repository, service hoặc API.

File đã tạo:

```text
AISAM-BE/AISAM.Data/Model/Workspace.cs
AISAM-BE/AISAM.Data/Model/WorkspaceMember.cs
AISAM-BE/AISAM.Data/Enumeration/WorkspaceTypeEnum.cs
AISAM-BE/AISAM.Data/Enumeration/WorkspaceStatusEnum.cs
AISAM-BE/AISAM.Data/Enumeration/WorkspaceMemberRoleEnum.cs
AISAM-BE/AISAM.Data/Enumeration/MemberQuotaModeEnum.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceDomainFoundationTests.cs
```

Quyết định đã áp dụng:

- `WorkspaceTypeEnum`: Personal = 1, Business = 2.
- Role: Owner, Manager, Content Creator, Viewer.
- Quota mode: Shared Pool, Lifetime Assigned Limit, Monthly Assigned Limit.
- Workspace lifecycle status foundation.
- Owner được biểu diễn bằng WorkspaceMember role; không tạo `OwnerUserId` riêng.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 errors, 2 warnings từ migration cũ verifytoken.

dotnet test --no-build
Passed: 127/127.
```

Migration/API test:

```text
N/A - task này chưa nối DbContext và chưa expose API.
```

Commit đề xuất:

```text
feat(workspace): add workspace domain foundation
```

Task tiếp theo:

```text
Task 9.2 - Cấu hình Workspace/WorkspaceMember trong DbContext và tạo migration foundation.
```

### Task 9.2 - DbContext và migration Workspace foundation

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Thêm DbSet/configuration cho Workspace và WorkspaceMember.
- Tạo migration chỉ thêm foundation; chưa chuyển ownership cũ.

File đã sửa:

```text
AISAM-BE/AISAM.Data/Model/User.cs
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceDomainFoundationTests.cs
```

File migration đã tạo:

```text
AISAM-BE/AISAM.Repositories/Migrations/20260610064359_AddWorkspaceFoundation.cs
AISAM-BE/AISAM.Repositories/Migrations/20260610064359_AddWorkspaceFoundation.Designer.cs
```

Nội dung đã hoàn thành:

- Thêm `DbSet<Workspace>` và `DbSet<WorkspaceMember>`.
- Cấu hình enum, index, default value và quan hệ cascade.
- Thêm unique index `WorkspaceId + UserId` để một User không bị lặp membership trong cùng Workspace.
- Giữ khả năng một User tham gia nhiều Workspace.
- Migration chỉ tạo `workspaces`, `workspace_members`, index và foreign key liên quan; không sửa migration cũ hoặc ownership cũ.
- Quy tắc đúng một Owner chưa được enforce ở database foundation; sẽ xử lý atomic trong Task 9.8.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 errors, 2 warnings từ migration cũ verifytoken.

dotnet test --no-build
Passed: 129/129.

dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
Applied migration: 20260610064359_AddWorkspaceFoundation.

dotnet ef migrations list --project AISAM.Repositories --startup-project AISAM.API --no-build
Migration AddWorkspaceFoundation xuất hiện và không có trạng thái Pending.
```

API test:

```text
N/A - task này chỉ tạo database foundation, chưa expose Workspace API.
```

Commit đề xuất:

```text
feat(workspace): add workspace database foundation
```

Task tiếp theo:

```text
Task 9.3 - Thêm Workspace và WorkspaceMember repositories.
```

### Task 9.3 - Workspace và WorkspaceMember repositories

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Thêm repository đọc/ghi Workspace và membership.
- Hỗ trợ truy vấn tất cả Workspace một User tham gia.

File đã tạo:

```text
AISAM-BE/AISAM.Repositories/IRepositories/IWorkspaceRepository.cs
AISAM-BE/AISAM.Repositories/IRepositories/IWorkspaceMemberRepository.cs
AISAM-BE/AISAM.Repositories/Repository/WorkspaceRepository.cs
AISAM-BE/AISAM.Repositories/Repository/WorkspaceMemberRepository.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceRepositoryTests.cs
```

File đã sửa:

```text
AISAM-BE/AISAM.API/Program.cs
```

Nội dung đã hoàn thành:

- Thêm Workspace repository để đọc theo ID/User, thêm, cập nhật và kiểm tra tồn tại.
- Thêm WorkspaceMember repository để đọc theo Workspace/User, thêm, cập nhật, deactivate và kiểm tra membership.
- Chỉ trả về active membership trong các truy vấn membership thông thường.
- Chặn tạo trùng `WorkspaceId + UserId` tại repository; database unique index vẫn bảo vệ ở tầng schema.
- Đăng ký hai repository với scoped lifetime trong dependency injection.
- Chưa thêm service, controller, API hoặc business rule Owner/role/member limit.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 errors, 2 warnings từ migration cũ verifytoken.

dotnet test --no-build
Passed: 133/133.

Repository tests:
- Một User truy vấn được nhiều Workspace đang tham gia.
- Workspace add/update được lưu.
- Membership trùng trong cùng Workspace bị từ chối.
- Remove membership chuyển IsActive thành false.
```

Migration/API test:

```text
N/A - task này không thay đổi schema và chưa expose Workspace API.
```

Commit đề xuất:

```text
feat(workspace): add workspace repositories
```

Task tiếp theo:

```text
Task 9.4 - Workspace service và CRUD API.
```

### Task 9.4 - Workspace service và CRUD API

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Thêm Workspace service/controller/DTO.
- Tạo Personal/Business Workspace theo rule được phép.

File đã tạo:

```text
AISAM-BE/AISAM.Common/Dtos/Request/WorkspaceRequests.cs
AISAM-BE/AISAM.Common/Dtos/Response/WorkspaceResponseDto.cs
AISAM-BE/AISAM.Services/IServices/IWorkspaceService.cs
AISAM-BE/AISAM.Services/Service/WorkspaceService.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceController.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceControllerTests.cs
```

File đã sửa:

```text
AISAM-BE/AISAM.API/Program.cs
```

Nội dung đã hoàn thành:

- Thêm API list/get/create/update Workspace của user đang đăng nhập.
- Tạo được Personal Workspace hoặc Business Workspace.
- Workspace mới được lưu cùng đúng một Owner membership trong cùng một lần repository save.
- Active member được list/get Workspace đang tham gia.
- Non-member nhận Not Found để không lộ Workspace.
- Chỉ Owner được đổi tên Workspace trong phạm vi CRUD foundation.
- Đăng ký `IWorkspaceService` trong dependency injection.
- Chưa thêm delete, invitation, member role management, member limit hoặc ownership transfer.
- Personal Workspace chưa có API nhận member; rule không nhận member sẽ tiếp tục được giữ khi Task 9.7 thêm invitation.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test --no-build
Passed: 140/140.
```

API runtime smoke test:

```text
API chạy tại http://localhost:5054 trong thời gian smoke test.
Swagger nhận diện:
- /api/workspaces
- /api/workspaces/{id}

Endpoints:
- GET  /api/workspaces
- GET  /api/workspaces/{id}
- POST /api/workspaces
- PUT  /api/workspaces/{id}
```

Migration/config:

```text
Không có migration hoặc config thủ công mới trong task này.
```

Commit đề xuất:

```text
feat(workspace): add workspace management api
```

Task tiếp theo:

```text
Task 9.5 - Tạo Personal Workspace khi register.
```

### Task 9.5 - Tạo Personal Workspace khi register

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Tạo một Personal Workspace và Owner membership khi đăng ký tài khoản.

File đã sửa:

```text
AISAM-BE/AISAM.Services/Service/AuthService.cs
```

File đã tạo:

```text
AISAM-BE/tests/AISAM.IntegrationTests/AuthRegistrationWorkspaceTests.cs
```

Nội dung đã hoàn thành:

- Luồng register email/password tạo Personal Workspace mặc định.
- Tên mặc định dùng `{FullName}'s Workspace`; nếu không có FullName dùng `Personal Workspace`.
- User mới là Owner duy nhất của Personal Workspace.
- User, Workspace và Owner membership được gắn thành một EF graph và lưu bằng cùng một `SaveChanges`.
- Duplicate email bị từ chối trước khi tạo thêm Workspace/membership.
- Không thay đổi Google login, email delivery hoặc session semantics hiện tại ngoài phạm vi task.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 errors, 2 warnings từ migration cũ verifytoken.

dotnet test --no-build
Passed: 143/143.

Registration workspace tests:
- Register tạo đúng một Personal Workspace.
- Personal Workspace có đúng một Owner là User vừa đăng ký.
- Duplicate register không tạo thêm User, Workspace hoặc membership.
```

Migration/config/API:

```text
Không có migration hoặc config thủ công mới.
Endpoint hiện tại tiếp tục dùng: POST /api/Auth/register.
```

Commit đề xuất:

```text
feat(auth): create personal workspace on registration
```

Task tiếp theo:

```text
Task 9.6 - Active Workspace context và X-Workspace-Id.
```

### Regression fixes trước Task 9.6 - Security, quota và account consistency

Trạng thái:

```text
DONE - 2026-06-10
```

Lý do:

- Rà soát Phase 0-8 và Task 9.1-9.5 phát hiện các blocker cần sửa trước khi chuyển active context sang Workspace.

Nội dung đã sửa:

- PayOS callback/webhook bắt buộc có signature hợp lệ trước khi đồng bộ payment/subscription.
- Subscription có `EndDate` đã hết hạn hoặc chưa đến `StartDate` không còn được xem là active.
- Prompt quota theo ngày chỉ đếm AI generation thành công trong ngày UTC hiện tại.
- AI Improve/Regenerate kiểm tra prompt quota trước khi gọi provider.
- Google user mới được tạo Personal Workspace và Owner membership giống register email/password.

File đã sửa:

```text
AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs
AISAM-BE/AISAM.Repositories/Repository/SubscriptionRepository.cs
AISAM-BE/AISAM.Services/Service/QuotaService.cs
AISAM-BE/AISAM.Services/Service/AIService.cs
AISAM-BE/AISAM.Services/Service/AuthService.cs
AISAM-BE/tests/AISAM.IntegrationTests/PaymentServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/QuotaServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/AIServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/SubscriptionRepositoryTests.cs
```

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 errors, 2 warnings từ migration cũ verifytoken.

dotnet test --no-build
Passed: 148/148.

dotnet ef migrations has-pending-model-changes
No changes have been made to the model since the last migration.
```

Migration/config:

```text
Không có migration hoặc config thủ công mới.
PayOS callback/webhook thiếu signature hiện trả PAYOS_SIGNATURE_REQUIRED.
```

Commit đề xuất:

```text
fix(regression): secure payment and workspace account flows
```

### Task 9.6 - Active Workspace context và X-Workspace-Id

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Thêm middleware/helper đọc `X-Workspace-Id` và kiểm tra membership.
- Chưa xóa Active Profile middleware trong task này.

Rà soát trước khi code:

- Xác nhận Workspace repository/membership foundation và registration flow đã pass regression.
- Phát hiện active membership vẫn có thể trỏ tới Workspace đã Soft Delete.
- Fix trong middleware: Workspace có status `Deleted` trả `404`; các lifecycle status khác được giữ context để Task 9.15 áp quyền.
- Không áp `X-Workspace-Id` lên route Profile-based hiện tại vì ownership chưa được migrate.

File đã tạo:

```text
AISAM-BE/AISAM.API/Utils/WorkspaceContextHelper.cs
AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs
AISAM-BE/tests/AISAM.IntegrationTests/ActiveWorkspaceMiddlewareTests.cs
```

File đã sửa:

```text
AISAM-BE/AISAM.API/Program.cs
```

Nội dung đã hoàn thành:

- Middleware đọc và validate `X-Workspace-Id` cho các route Workspace-scoped.
- Kiểm tra authentication và active membership theo JWT user.
- Lưu Active Workspace ID và WorkspaceMember vào `HttpContext.Items`.
- Helper cung cấp Active Workspace ID và membership/role cho controller/service sau.
- Non-member nhận `403`; Workspace đã Deleted nhận `404`.
- Active Profile middleware tiếp tục hoạt động độc lập cho module cũ.
- Các prefix foundation hiện được bảo vệ: `/api/workspace-context`, `/api/workspace-members`, `/api/workspace-invitations`, `/api/workspace-dashboard`.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 errors, 2 warnings từ migration cũ verifytoken.

dotnet test --no-build
Passed: 154/154.
```

Runtime smoke test:

```text
GET /api/workspace-members không có authentication
Result: 401 Authentication is required.
```

Migration/config:

```text
Không có migration hoặc config thủ công mới.
Client phải gửi X-Workspace-Id khi gọi API Workspace-scoped.
```

Test đã xác nhận:

- Thiếu/sai header.
- Non-member và Workspace A không dùng được context Workspace B.
- Member hợp lệ nhận đúng Workspace ID và membership.
- Workspace Deleted bị chặn.
- Route Profile-based không bị middleware mới yêu cầu Workspace header.

Commit đề xuất:

```text
feat(workspace): add active workspace context
```

Task tiếp theo:

```text
Task 9.7 - Invitation, role management và Member Limit.
```

### Regression fixes trước Task 9.7 - Workspace membership và Owner safety

Trạng thái:

```text
DONE - 2026-06-10
```

Vấn đề phát hiện:

- Membership đã inactive không thể tham gia lại vì unique index `(WorkspaceId, UserId)`.
- Generic repository cho phép remove Owner, làm Workspace không còn Owner.
- Generic repository cho phép thêm/nâng member khác thành Owner hoặc hạ Owner, có thể phá rule mỗi Workspace đúng một Owner.

File đã sửa:

```text
AISAM-BE/AISAM.Repositories/Repository/WorkspaceMemberRepository.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceRepositoryTests.cs
```

Nội dung đã hoàn thành:

- Tái kích hoạt membership inactive thay vì tạo record trùng.
- Vẫn từ chối thêm membership đang active lần thứ hai.
- Chặn thêm membership Owner ngoài workspace creation/ownership transfer.
- Chặn remove Owner nếu chưa ownership transfer.
- Chặn đổi role Owner và chặn nâng member thành Owner qua generic update.
- Giữ atomic ownership transfer cho Task 9.8.

Kết quả kiểm tra:

```text
WorkspaceRepositoryTests: Passed 9/9.
dotnet build AISAM-BE/AISAM.sln --no-restore: Build succeeded. 0 warnings, 0 errors.
dotnet test AISAM-BE/AISAM.sln --no-build --no-restore: Passed 159/159.
git diff --check: không có whitespace error.
```

Migration/config:

```text
Không có migration hoặc config thủ công mới.
```

Commit đề xuất:

```text
fix(workspace): protect owner membership invariants
```

### Task 9.7 - Invitation, role management và Member Limit

Mục tiêu:

- Invite/accept/list/remove/update role member.
- Business Plus tối đa 10 members; Business Pro tối đa 50 members.

Chia task nhỏ:

| Task | Nội dung | Trạng thái |
|---|---|---|
| 9.7.1 | Workspace Invitation entity, repository, migration và tests | DONE - 2026-06-10 |
| 9.7.2 | Invite/accept invitation service và API | DONE - 2026-06-10 |
| 9.7.3 | List/remove/update role member và permission tests | DONE - 2026-06-11 |
| 9.7.4 | Member limit integration và hoàn tất Task 9.7 | DONE - 2026-06-11 |

### Task 9.7.1 - Workspace Invitation foundation

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Tạo nền tảng lưu invitation độc lập trước khi expose API.
- Lưu email chuẩn hóa, role được mời, token, người mời, thời hạn, trạng thái accepted/revoked.

File đã tạo:

```text
AISAM-BE/AISAM.Data/Model/WorkspaceInvitation.cs
AISAM-BE/AISAM.Repositories/IRepositories/IWorkspaceInvitationRepository.cs
AISAM-BE/AISAM.Repositories/Repository/WorkspaceInvitationRepository.cs
AISAM-BE/AISAM.Repositories/Migrations/20260610160919_AddWorkspaceInvitationFoundation.cs
AISAM-BE/AISAM.Repositories/Migrations/20260610160919_AddWorkspaceInvitationFoundation.Designer.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceInvitationRepositoryTests.cs
```

File đã sửa:

```text
AISAM-BE/AISAM.Data/Model/User.cs
AISAM-BE/AISAM.Data/Model/Workspace.cs
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Repositories/Migrations/AisamContextModelSnapshot.cs
AISAM-BE/AISAM.API/Program.cs
```

Nội dung đã hoàn thành:

- Thêm bảng `workspace_invitations` và navigation tới Workspace/người mời.
- Token invitation có unique index.
- Repository hỗ trợ lấy invitation theo token và lấy/đếm pending invitation.
- Pending invitation tự loại accepted, revoked và expired.
- Email được chuẩn hóa lowercase khi lưu và tìm kiếm.

Kết quả kiểm tra:

```text
WorkspaceInvitationRepositoryTests: Passed 4/4.
dotnet build AISAM-BE/AISAM.sln --no-restore: Build succeeded. 0 errors, 2 warnings migration cũ verifytoken.
dotnet test AISAM-BE/AISAM.sln --no-build --no-restore: Passed 163/163.
dotnet ef database update: Applied 20260610160919_AddWorkspaceInvitationFoundation.
dotnet ef migrations has-pending-model-changes: No changes have been made to the model since the last migration.
```

Giới hạn đã xác nhận:

- Task này chưa expose invitation API và chưa gửi email.
- Task 9.7.4 lưu và enforce `MemberLimit` ngay trên Workspace; Task 9.9 sẽ tự động gán limit `10/50` khi Subscription Business Plus/Business Pro được chuyển sang Workspace.

Commit đề xuất:

```text
feat(workspace): add invitation persistence foundation
```

### Task 9.7.2 - Invite/Accept Invitation service và API

Trạng thái:

```text
DONE - 2026-06-10
```

Mục tiêu:

- Owner tạo invitation cho Business Workspace.
- User đã đăng nhập accept invitation bằng token và email đúng tài khoản.
- Người nhận chưa phải member vẫn gọi được endpoint accept.

File đã tạo:

```text
AISAM-BE/AISAM.Common/Dtos/Request/WorkspaceInvitationRequests.cs
AISAM-BE/AISAM.Common/Dtos/Response/WorkspaceInvitationResponseDto.cs
AISAM-BE/AISAM.Services/IServices/IWorkspaceInvitationService.cs
AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceInvitationController.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceInvitationServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceInvitationControllerTests.cs
```

File đã sửa:

```text
AISAM-BE/AISAM.Repositories/IRepositories/IWorkspaceInvitationRepository.cs
AISAM-BE/AISAM.Repositories/Repository/WorkspaceInvitationRepository.cs
AISAM-BE/AISAM.Repositories/Repository/UserRepository.cs
AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs
AISAM-BE/AISAM.API/Program.cs
AISAM-BE/tests/AISAM.IntegrationTests/ActiveWorkspaceMiddlewareTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceInvitationRepositoryTests.cs
```

Nội dung đã hoàn thành:

- `POST /api/workspace-invitations` yêu cầu JWT và `X-Workspace-Id`.
- Chỉ Owner của Business Workspace đang Active được invite.
- Chặn Personal Workspace, non-owner, role Owner, member đã tồn tại và pending invitation trùng.
- User lookup theo email không phân biệt chữ hoa/thường để tránh mời trùng member cũ.
- Token invitation được sinh bằng random bytes và hết hạn sau 7 ngày.
- Gửi invitation link bằng EmailService hiện có; thiếu SMTP không làm hỏng API local.
- `POST /api/workspace-invitations/accept` yêu cầu JWT nhưng không yêu cầu `X-Workspace-Id`.
- Accept chỉ thành công khi email invitation khớp email tài khoản đăng nhập.
- Tạo hoặc reactivate membership và đánh dấu invitation accepted trong cùng `SaveChanges`.

API test:

```text
POST /api/workspace-invitations
Headers: Authorization: Bearer <owner-token>, X-Workspace-Id: <business-workspace-id>
Body: { "email": "member@example.com", "role": 3 }

POST /api/workspace-invitations/accept
Headers: Authorization: Bearer <invited-user-token>
Body: { "token": "<token-from-invitation-email>" }
```

Kết quả kiểm tra:

```text
Focused Workspace Invitation + middleware tests: Passed 22/22.
dotnet build AISAM-BE/AISAM.sln --no-restore: Build succeeded. 0 warnings, 0 errors.
dotnet test AISAM-BE/AISAM.sln --no-build --no-restore: Passed 175/175.
dotnet ef migrations has-pending-model-changes: No changes have been made to the model since the last migration.
Runtime Swagger smoke test: invite path = true, accept path = true.
Runtime unauthenticated accept: 401.
```

Config:

```text
Không có config bắt buộc mới.
SMTP chỉ cần khi muốn gửi invitation email thật.
FRONTEND_BASE_URL được dùng để tạo link accept invitation.
```

Commit đề xuất:

```text
feat(workspace): add invite and accept invitation APIs
```

### Task 9.7.3 - List/Remove/Update Role Member

Trạng thái:

```text
DONE - 2026-06-11
```

Nội dung đã hoàn thành:

- `GET /api/workspace-members`: mọi active member được xem danh sách team.
- `PUT /api/workspace-members/{memberId}/role`: chỉ Owner được đổi role non-owner.
- `DELETE /api/workspace-members/{memberId}`: chỉ Owner được remove non-owner.
- Không cho gán role Owner hoặc sửa/remove Owner; ownership transfer giữ cho Task 9.8.
- Limited/Archived Workspace vẫn xem team nhưng bị chặn role management/remove.

File chính:

```text
AISAM-BE/AISAM.Common/Dtos/Request/WorkspaceMemberRequests.cs
AISAM-BE/AISAM.Common/Dtos/Response/WorkspaceMemberResponseDto.cs
AISAM-BE/AISAM.Services/IServices/IWorkspaceMemberService.cs
AISAM-BE/AISAM.Services/Service/WorkspaceMemberService.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceMemberController.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceMemberServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceMemberControllerTests.cs
```

### Task 9.7.4 - Member Limit integration và hoàn tất Task 9.7

Trạng thái:

```text
DONE - 2026-06-11
```

Nội dung đã hoàn thành:

- Thêm `Workspace.MemberLimit`.
- Personal Workspace mặc định `1`; Business Workspace mặc định `10`.
- Hỗ trợ Workspace limit `50` để dùng cho Business Pro.
- Invite kiểm tra tổng active members + pending invitations.
- Accept kiểm tra lại active member count để tránh vượt limit sau khi invitation đã tạo.
- Invitation không thể accept sau khi Workspace rời trạng thái Active.
- Migration cập nhật Business Workspace cũ thành limit `10`; Personal giữ `1`.

Migration:

```text
AISAM-BE/AISAM.Repositories/Migrations/20260610172441_AddWorkspaceMemberLimit.cs
Applied successfully to PostgreSQL local.
No pending model changes.
```

Kết quả hoàn tất Task 9.7:

```text
Focused Workspace/member/invitation tests: Passed 41/41.
dotnet build AISAM-BE/AISAM.sln --no-restore: Build succeeded. 0 warnings, 0 errors.
dotnet test AISAM-BE/AISAM.sln --no-build --no-restore: Passed 186/186.
Runtime Swagger smoke: invite, accept, member list, role update và remove routes đều tồn tại.
Runtime thiếu JWT: member list và invitation accept đều trả 401.
```

Lưu ý dependency:

- Task 9.7 đã enforce đúng giá trị `MemberLimit` của Workspace.
- Task 9.9 sẽ tự động đặt `MemberLimit = 10` cho Business Plus và `MemberLimit = 50` cho Business Pro khi chuyển Subscription sang Workspace.

Commit đề xuất:

```text
feat(workspace): complete member roles and limits
```

Cách test:

- Chặn member thứ 11/51.
- Personal Workspace không invite member.
- Enforce Owner/Manager/Content Creator/Viewer permissions.

Commit đề xuất:

```text
feat(workspace): add invitations roles and member limits
```

### Task 9.8 - Atomic Ownership Transfer

Trạng thái:

```text
DONE - 2026-06-11
```

Mục tiêu:

- Transfer ownership từ Owner sang Manager trong cùng transaction.

Nội dung đã hoàn thành:

- Thêm `POST /api/workspace-members/ownership-transfer`.
- Chỉ active Owner của Active Workspace được transfer ownership.
- Target bắt buộc là active Manager trong cùng Workspace.
- Repository tái kiểm tra Workspace có đúng một current Owner trước khi đổi role.
- PostgreSQL dùng transaction isolation `Serializable`.
- Owner cũ được hạ thành Manager và Manager được nâng thành Owner trong cùng transaction.
- Validation failure xảy ra trước mutation; transaction rollback toàn bộ khi lỗi trong quá trình lưu.
- Generic update/add/remove tiếp tục chặn mọi đường tạo zero/multiple Owner ngoài ownership transfer.
- Database partial unique index chặn nhiều active Owner trong cùng Workspace.

File đã sửa:

```text
AISAM-BE/AISAM.Common/Dtos/Request/WorkspaceMemberRequests.cs
AISAM-BE/AISAM.Repositories/IRepositories/IWorkspaceMemberRepository.cs
AISAM-BE/AISAM.Repositories/Repository/WorkspaceMemberRepository.cs
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Services/IServices/IWorkspaceMemberService.cs
AISAM-BE/AISAM.Services/Service/WorkspaceMemberService.cs
AISAM-BE/AISAM.API/Controllers/WorkspaceMemberController.cs
```

Migration:

```text
AISAM-BE/AISAM.Repositories/Migrations/20260611085418_EnforceSingleActiveWorkspaceOwner.cs
Applied successfully to PostgreSQL local.
No pending model changes.
```

API test:

```text
POST /api/workspace-members/ownership-transfer
Headers:
Authorization: Bearer <owner-token>
X-Workspace-Id: <workspace-id>

Body:
{
  "targetMemberId": "<active-manager-member-id>"
}
```

Kết quả kiểm tra:

```text
Focused Workspace ownership/member tests: Passed 24/24.
dotnet build AISAM-BE/AISAM.sln --no-restore: Build succeeded. 0 warnings, 0 errors.
dotnet test AISAM-BE/AISAM.sln --no-build --no-restore: Passed 190/190.
dotnet ef database update: Applied EnforceSingleActiveWorkspaceOwner.
dotnet ef migrations has-pending-model-changes: No changes have been made to the model since the last migration.
Runtime Swagger smoke: ownership-transfer path = true.
Runtime thiếu JWT: 401.
```

Cách test:

- Manager mới thành Owner, Owner cũ thành Manager.
- Rollback toàn bộ khi lỗi.
- Owner không thể tự remove trước khi transfer.
- Workspace luôn có đúng một Owner.

Commit đề xuất:

```text
feat(workspace): add atomic ownership transfer
```

### Task 9.9 - Chuyển Subscription và Payment sang Workspace

Mục tiêu:

- Checkout, webhook, current subscription và history dùng Workspace.

Trạng thái:

```text
DONE - 2026-06-11
Task tiếp theo: 9.10 Credit Wallet, Credit Usage và Maximum Balance.
```

#### Task 9.9.1 - Schema và repository tương thích Workspace

Trạng thái:

```text
DONE - 2026-06-11
```

Mục tiêu:

- Thêm ownership Workspace nullable cho `Subscription` và `Payment`.
- Giữ tương thích tạm thời với dữ liệu và API Profile cũ.
- Thêm repository query subscription/payment cô lập theo Workspace.

File đã sửa:

```text
AISAM-BE/AISAM.Data/Model/Payment.cs
AISAM-BE/AISAM.Data/Model/Subscription.cs
AISAM-BE/AISAM.Data/Model/Workspace.cs
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Repositories/IRepositories/IPaymentRepository.cs
AISAM-BE/AISAM.Repositories/IRepositories/ISubscriptionRepository.cs
AISAM-BE/AISAM.Repositories/Repository/PaymentRepository.cs
AISAM-BE/AISAM.Repositories/Repository/SubscriptionRepository.cs
AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs
AISAM-BE/tests/AISAM.IntegrationTests/PaymentRepositoryTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/PaymentServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/QuotaServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/SubscriptionRepositoryTests.cs
```

Migration:

```text
20260611092549_AddWorkspacePaymentSubscriptionOwnership
```

Kết quả:

- `subscriptions.profile_id` chuyển nullable để hỗ trợ giai đoạn chuyển tiếp.
- Thêm `subscriptions.workspace_id` và `payments.workspace_id` nullable, có index và foreign key.
- Không backfill dữ liệu cũ trong task này vì chưa có quan hệ Profile -> Workspace đủ tin cậy; backfill thuộc Task 9.17.
- Payment API, PayOS flow và quota usage hiện vẫn Profile-based; chuyển sang Workspace thuộc Task 9.9.2 và các task ownership sau.

Kiểm tra đã chạy:

```text
dotnet build AISAM-BE/AISAM.sln --no-restore
Build succeeded. 0 warnings, 0 errors.

dotnet test AISAM-BE/AISAM.sln --no-build --no-restore
Passed. 193/193 tests passed.

Focused repository/payment/quota tests
Passed. 21/21 tests passed.

dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
Database is already up to date; migration exists in migration history.

dotnet ef migrations has-pending-model-changes --project AISAM.Repositories --startup-project AISAM.API
No changes have been made to the model since the last migration.
```

Commit đề xuất:

```text
feat(payment): prepare workspace subscription payment ownership
```

#### Task 9.9.2 - Chuyển payment API và PayOS flow sang Active Workspace

Trạng thái:

```text
DONE - 2026-06-11
```

Mục tiêu:

- Checkout, callback, webhook, current subscription và history dùng Active Workspace.
- Giữ validation để payment không thể kích hoạt nhầm Workspace.

Kết quả:

- `POST /api/payment/checkout`, `GET /api/payment/history` và `GET /api/payment/subscription/current` bắt buộc Active Workspace hợp lệ.
- Checkout tạo `Subscription.WorkspaceId` và `Payment.WorkspaceId`; `Payment.UserId` lưu người thực hiện checkout.
- Payment history và current subscription được cô lập theo Workspace.
- `POST /api/payment/callback` và `POST /api/payment/webhook` vẫn anonymous cho PayOS, nhưng bỏ qua Workspace middleware đúng hai route provider này.
- Các payment route Workspace khác yêu cầu JWT và header `X-Workspace-Id`.

#### Task 9.9.3 - Renewal, Member Limit và regression

Trạng thái:

```text
DONE - 2026-06-11
```

Mục tiêu:

- Áp dụng renewal và Member Limit theo plan Workspace.
- Chạy regression và cập nhật tài liệu hoàn tất Task 9.9.

Kết quả:

- Thanh toán thành công kích hoạt đúng Workspace Subscription.
- Renewal dùng ngày hết hạn hiện tại làm mốc nếu gói cũ vẫn còn hạn, sau đó cộng thêm 30 ngày.
- Subscription Workspace cũ được deactivate khi renewal thành công.
- Webhook lặp lại không cộng thêm thời hạn lần thứ hai.
- `Workspace.SubscriptionExpiredAt` được đồng bộ theo subscription mới.
- Với enum plan hiện tại: Business `Plus` đặt Member Limit `10`, Business `Premium` đặt Member Limit `50`; Personal luôn là `1`.
- Cộng Credits khi renewal chưa áp dụng vì Credit Wallet chưa tồn tại; thuộc Task 9.10.
- Payment/subscription cũ đã được backfill `WorkspaceId` trong Task 9.17.

Kiểm tra hoàn tất Task 9.9:

```text
Focused payment/workspace tests
Passed. 32/32 tests passed.

dotnet build AISAM-BE/AISAM.sln --no-restore
Build succeeded. 0 warnings, 0 errors.

dotnet test AISAM-BE/AISAM.sln --no-build --no-restore
Passed. 198/198 tests passed.

dotnet ef migrations has-pending-model-changes --project AISAM.Repositories --startup-project AISAM.API
No changes have been made to the model since the last migration.

Runtime smoke:
GET /swagger/index.html -> 200
GET /api/payment/history without authentication -> 401
POST /api/payment/webhook with empty anonymous payload -> 400
```

Cách test:

- PayOS payment kích hoạt đúng Workspace Subscription.
- Gia hạn cộng thời gian; Credits được bổ sung sau khi Task 9.10 tạo Credit Wallet.
- Payment history cô lập theo Workspace.

Commit đề xuất:

```text
feat(payment): move subscriptions and payments to workspace
```

### Task 9.10 - Credit Wallet, Credit Usage và Maximum Balance

Trạng thái:

```text
DONE - 2026-06-11
Task tiếp theo: 9.11 Credit Pack và PaymentType.
```

Mục tiêu:

- Mỗi Workspace có đúng một Credit Wallet.
- Lưu Credit Usage metadata, không lưu full prompt.

File đã sửa:

```text
AISAM-BE/AISAM.API/Program.cs
AISAM-BE/AISAM.Data/Enumeration/CreditActionEnum.cs
AISAM-BE/AISAM.Data/Enumeration/CreditUsageStatusEnum.cs
AISAM-BE/AISAM.Data/Model/CreditUsageRecord.cs
AISAM-BE/AISAM.Data/Model/CreditWallet.cs
AISAM-BE/AISAM.Data/Model/Workspace.cs
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Repositories/IRepositories/ICreditUsageRecordRepository.cs
AISAM-BE/AISAM.Repositories/IRepositories/ICreditWalletRepository.cs
AISAM-BE/AISAM.Repositories/Repository/CreditUsageRecordRepository.cs
AISAM-BE/AISAM.Repositories/Repository/CreditWalletRepository.cs
AISAM-BE/AISAM.Services/IServices/ICreditService.cs
AISAM-BE/AISAM.Services/Service/AuthService.cs
AISAM-BE/AISAM.Services/Service/CreditService.cs
AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs
AISAM-BE/AISAM.Services/Service/WorkspaceService.cs
AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
AISAM-BE/tests/AISAM.IntegrationTests/AuthRegistrationWorkspaceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/CreditServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/CreditWalletRepositoryTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/PaymentServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceServiceTests.cs
```

Migration:

```text
20260611115818_AddCreditWalletAndUsageTracking
```

Kết quả:

- Thêm `CreditWallet` one-to-one với `Workspace` và unique index trên `WorkspaceId`.
- Thêm `CreditUsageRecord` để lưu metadata usage gồm `WorkspaceId`, `UserId`, `Action`, `Credits`, `Status`, `AiGenerationId`; không lưu full prompt.
- Thêm `ICreditService` và `CreditService` để:
  - tạo wallet mặc định cho Workspace,
  - grant credits theo `WorkspaceTypeEnum` + `SubscriptionPlanEnum`,
  - chặn toàn bộ giao dịch nếu số dư mới vượt maximum balance.
- `AuthService.RegisterAsync` và `WorkspaceService.CreateAsync` tự tạo wallet cho Personal/Business Workspace mới.
- `PayOSPaymentService` cộng credits khi payment subscription thành công và fail toàn bộ nếu vượt maximum balance.
- Mapping credits hiện dùng enum/plan đang có:
  - Personal `Free` -> `50`
  - Personal `Plus` -> `500`
  - Personal `Premium` -> `2_000`
  - Business `Plus` -> `15_000`
  - Business `Premium` -> `50_000`
- Maximum balance đã enforce:
  - Personal -> `15_000`
  - Business -> `500_000`
- Task này chỉ hoàn thiện wallet/usage/max-balance foundation cho Workspace.
- Chưa triển khai `PaymentType`/Credit Pack, shared-lifetime-monthly member quota, AI debit flow hay post quota theo Workspace; các phần đó vẫn thuộc Task 9.11-9.14.

Cách test:

- Personal không vượt 15.000; Business không vượt 500.000.
- Giao dịch vượt maximum bị từ chối toàn bộ.
- Unique Wallet constraint hoạt động.

Kiểm tra đã chạy:

```text
dotnet build AISAM-BE/AISAM.sln
Build succeeded. 0 warnings, 0 errors.

dotnet test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
Passed. 203/203 tests passed.

Focused Task 9.10 tests
Passed. 22/22 tests passed.
```

Commit đề xuất:

```text
feat(credits): add workspace wallet and usage tracking
```

### Task 9.11 - Credit Pack và PaymentType

Trạng thái:

```text
DONE - 2026-06-11
Task tiếp theo: 9.12 Shared Pool, Lifetime và Monthly Assigned Limit.
```

Mục tiêu:

- Mua Credit Pack qua PayOS và phân biệt `Subscription`/`CreditPack`.

File đã sửa:

```text
AISAM-BE/AISAM.Common/Models/PaymentDtos.cs
AISAM-BE/AISAM.Data/Enumeration/CreditActionEnum.cs
AISAM-BE/AISAM.Data/Enumeration/CreditPackCodeEnum.cs
AISAM-BE/AISAM.Data/Enumeration/PaymentTypeEnum.cs
AISAM-BE/AISAM.Data/Model/Payment.cs
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Services/IServices/ICreditService.cs
AISAM-BE/AISAM.Services/Service/CreditService.cs
AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs
AISAM-BE/tests/AISAM.IntegrationTests/PaymentControllerTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/PaymentServiceTests.cs
```

Migration:

```text
20260611123701_AddCreditPackPaymentType
```

Kết quả:

- Thêm `PaymentTypeEnum` để phân biệt `Subscription` và `CreditPack`.
- Thêm `CreditPackCodeEnum` theo catalog đã chốt:
  - `Starter` -> `100` credits / `29.000`
  - `Standard` -> `500` credits / `99.000`
  - `Growth` -> `1.500` credits / `249.000`
  - `Business` -> `5.000` credits / `699.000`
- Mở rộng `CreateCheckoutRequest` để checkout được cả subscription và credit pack qua cùng payment API.
- `payments` lưu thêm `payment_type`, `credit_pack_code`, `credit_amount` và index theo `payment_type`.
- `PayOSPaymentService`:
  - tạo payment `Subscription` như cũ,
  - tạo payment `CreditPack` không cần `SubscriptionId`,
  - webhook/callback xử lý riêng credit pack để cộng credits vào wallet,
  - không đổi `Subscription.EndDate`, `Workspace.SubscriptionExpiredAt` hay feature/plan khi credit pack thành công.
- `CreditService` có thêm `GrantCreditPackCreditsAsync` và dùng lại maximum balance rule của Task 9.10.
- Credit Pack bị từ chối toàn bộ nếu cộng vào làm vượt maximum balance của Workspace.
- Task này chỉ hoàn thiện `PaymentType` và Credit Pack purchase flow.
- Chưa triển khai shared pool, lifetime/monthly assigned quota, AI debit flow hay post quota theo Workspace; các phần đó vẫn thuộc Task 9.12-9.14.

Cách test:

- Credit Pack cộng Credits, không đổi subscription expiry/feature.
- Credit Pack không hết hạn.
- Vượt maximum balance bị từ chối.

Kiểm tra đã chạy:

```text
dotnet build AISAM-BE/AISAM.sln
Build succeeded. 0 warnings, 0 errors.

dotnet test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
Passed. 207/207 tests passed.

Focused Task 9.11 payment tests
Passed. 17/17 tests passed.
```

Commit đề xuất:

```text
feat(credits): add workspace credit pack payments
```

### Task 9.12 - Shared Pool, Lifetime và Monthly Assigned Limit

Trạng thái:

```text
DONE - 2026-06-11
Task tiếp theo: 9.13 Plan Entitlement, Permission Matrix và Post Quota.
```

Mục tiêu:

- Business Plus dùng Shared Pool.
- Business Pro hỗ trợ Shared Pool, Lifetime và Monthly Assigned Limit.

File đã sửa:

```text
AISAM-BE/AISAM.API/Controllers/WorkspaceMemberController.cs
AISAM-BE/AISAM.Common/Dtos/Request/WorkspaceInvitationRequests.cs
AISAM-BE/AISAM.Common/Dtos/Request/WorkspaceMemberRequests.cs
AISAM-BE/AISAM.Common/Dtos/Response/WorkspaceInvitationResponseDto.cs
AISAM-BE/AISAM.Common/Dtos/Response/WorkspaceMemberResponseDto.cs
AISAM-BE/AISAM.Data/Model/WorkspaceInvitation.cs
AISAM-BE/AISAM.Repositories/AISAMContext.cs
AISAM-BE/AISAM.Repositories/Repository/WorkspaceInvitationRepository.cs
AISAM-BE/AISAM.Services/IServices/ICreditService.cs
AISAM-BE/AISAM.Services/IServices/IWorkspaceMemberService.cs
AISAM-BE/AISAM.Services/Service/CreditService.cs
AISAM-BE/AISAM.Services/Service/WorkspaceInvitationService.cs
AISAM-BE/AISAM.Services/Service/WorkspaceMemberService.cs
AISAM-BE/tests/AISAM.IntegrationTests/CreditServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceInvitationServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceMemberControllerTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/WorkspaceMemberServiceTests.cs
```

Migration:

```text
20260611131708_AddWorkspaceInvitationQuotaModes
```

Kết quả:

- Mở rộng `WorkspaceInvitation` và invitation DTO để owner có thể cấu hình `QuotaMode` và `CreditLimit` ngay từ lúc invite member.
- `WorkspaceInvitationService` validate plan/quota rule:
  - `Business Plus` chỉ được `SharedPool`.
  - `Business Pro` mới được dùng `LifetimeAssignedLimit` và `MonthlyAssignedLimit`.
  - Assigned quota bắt buộc `CreditLimit > 0`.
- `WorkspaceInvitationRepository.AcceptAsync` copy quota config từ invitation sang `WorkspaceMember`.
- Thêm endpoint `PUT /api/workspace-members/{memberId}/quota` để owner cập nhật quota mode cho member sau khi join.
- `WorkspaceMemberService` hỗ trợ chuyển mode giữa `SharedPool`, `LifetimeAssignedLimit` và `MonthlyAssignedLimit`, đồng thời reset usage phù hợp khi đổi mode.
- `WorkspaceMemberResponseDto` trả thêm `QuotaMode`, `CreditLimit`, `CreditUsed`, `CreditPeriodStart` để frontend/admin theo dõi quota member.
- `CreditService` có thêm `ConsumeCreditsAsync`:
  - Shared Pool chỉ trừ `CreditWallet` của Workspace.
  - Assigned member phải đồng thời còn workspace credits và chưa vượt member limit.
  - `MonthlyAssignedLimit` reset `CreditUsed` theo calendar month, vào ngày 01 của tháng mới khi phát sinh usage tiếp theo.
  - Khi member quota bị vượt, wallet không bị trừ dù workspace vẫn còn credits.
- Task này chỉ hoàn thiện quota mode foundation, invitation/member management flow và credit consume enforcement primitive.
- Chưa nối AI generation endpoints sang `ConsumeCreditsAsync`; phần cắm AI flow vẫn thuộc Task 9.14.

Cách test:

- Assigned member hết quota bị chặn dù Workspace còn Credits.
- Monthly usage reset ngày 01.
- Workspace Credit balance không bị reset.

Kiểm tra đã chạy:

```text
dotnet build AISAM-BE/AISAM.sln
Build succeeded. 0 warnings, 0 errors.

dotnet test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
Passed. 214/214 tests passed.

Focused Task 9.12 tests
Passed. 30/30 tests passed.
```

Commit đề xuất:

```text
feat(credits): add workspace member quota modes
```

### Task 9.13 - Plan Entitlement, Permission Matrix và Post Quota

Trạng thái:

```text
DONE - 2026-06-11
Task tiếp theo: 9.14 Áp dụng Credits vào AI generation.
```

Mục tiêu:

- Áp dụng feature inheritance, role permissions và Post Quota đã chốt.

File đã sửa:

```text
AISAM-BE/AISAM.API/Controllers/ContentController.cs
AISAM-BE/AISAM.API/Controllers/QuotaController.cs
AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs
AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs
AISAM-BE/AISAM.Data/Enumeration/WorkspaceFeatureEnum.cs
AISAM-BE/AISAM.Data/Enumeration/WorkspacePermissionEnum.cs
AISAM-BE/AISAM.Services/IServices/IContentService.cs
AISAM-BE/AISAM.Services/IServices/IQuotaService.cs
AISAM-BE/AISAM.Services/Service/ContentService.cs
AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs
AISAM-BE/AISAM.Services/Service/QuotaService.cs
AISAM-BE/tests/AISAM.IntegrationTests/AIServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/ActiveWorkspaceMiddlewareTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/ContentControllerPublishTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/ContentControllerTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/ContentServicePublishTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/ContentServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/PhaseEQuotaIntegrationTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/QuotaControllerTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/QuotaServiceTests.cs
AISAM-BE/tests/AISAM.IntegrationTests/ScheduledPostingServiceTests.cs
```

Kết quả đã có:

- Thêm `WorkspaceFeatureEnum` và `WorkspacePermissionEnum` để gom entitlement/permission theo Workspace.
- `QuotaService` có `GetWorkspaceSummaryAsync` và `EnsureWorkspacePostQuotaAsync`; Post Quota theo plan đã map đúng:
  - Free -> `20/tuần`
  - Personal Plus -> `300/tháng`
  - Personal Pro -> `1.000/tháng`
  - Business Plus -> `5.000/tháng`
  - Business Pro -> `20.000/tháng`
- `QuotaController` chuyển sang `GET /api/quota/workspace/current` để trả quota summary theo Active Workspace.
- `ContentController` publish truyền `workspaceId`; `ContentService` có overload publish theo Workspace và publish chỉ kiểm tra Post Quota, không trừ Credits.
- `ActiveWorkspaceMiddleware` bảo vệ thêm `/api/ai`, `/api/brands`, `/api/content`, `/api/content-schedules`, `/api/dashboard`, `/api/products`, `/api/quota`, đồng thời áp dụng permission gate cơ bản cho billing/content/brand/product/AI/schedule.
- `ActiveWorkspaceMiddleware` đã gate feature cho `SchedulePost` và `MultiPlatformPublish`.
- `ActiveWorkspaceMiddleware` tiếp tục được mở rộng để gate entitlement theo route/feature cho:
  - `GenerateText`
  - `AI Image`
  - `AI Video`
  - `Trend Analysis`
  - `Holiday Suggestion`
  - `Campaign Recommendation`
  - `Basic Analytics`
  - `Workspace Dashboard`
- `PayOSPaymentService` đồng bộ lại plan definition để Post Quota trong subscription data khớp matrix đã chốt.
- `ScheduledPostingService` đã resolve `workspaceId` từ `Profile -> User -> WorkspaceMember` và gọi overload publish theo Workspace, nên scheduled publish cũng đi qua `EnsureWorkspacePostQuotaAsync`; khoảng hở bypass Post Quota theo Workspace đã được đóng.
- Thêm regression tests để khóa 2 behavior quan trọng:
  - scheduled publish phải dùng workspace-aware publish path
  - feature gate phải chặn đúng plan cho `AI Image` và `Workspace Dashboard`

Kiểm tra đã chạy:

```text
dotnet build AISAM-BE/AISAM.sln
Build succeeded. 0 warnings, 0 errors.

dotnet test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
Passed. 222/222 tests passed.

Focused Task 9.13 tests
Passed. 20/20 tests passed.
```

Cách test:

- Feature/permission đúng theo matrix.
- Publish không trừ Credits.
- Post Quota đúng theo từng plan.

Commit đề xuất:

```text
feat(subscription): enforce workspace plan entitlements
```

### Task 9.14 - Áp dụng Credits vào AI generation

Trạng thái:

```text
DONE - 2026-06-12
Task tiếp theo: 9.16 Chuyển ownership từng domain sang Workspace.
```

Mục tiêu:

- Trừ Credits chỉ sau AI generate/regenerate/refine thành công.

Cách test:

- AI thành công trừ đúng Credits.
- AI/provider thất bại không trừ Credits.
- AI Chat không trừ Credits trong MVP.

Kết quả đã có:

- `GeminiController` truyền Active Workspace membership vào generate/refine.
- Generate text và refine kiểm tra Workspace/member Credits trước khi gọi provider.
- Chỉ generation thành công mới trừ `1` Credit và lưu `CreditUsageRecord`.
- Provider thất bại và AI Chat không trừ Credits.
- Generate Text/Basic Analytics là feature Free/basic, không bắt buộc active subscription nếu Workspace vẫn còn Credits.
- Loại bỏ dependency AutoMapper không sử dụng có cảnh báo lỗ hổng mức High.

Kiểm tra đã chạy:

```text
dotnet build AISAM-BE/AISAM.sln
Build succeeded. 2 legacy migration naming warnings, 0 errors.

dotnet test AISAM-BE/AISAM.sln
Passed. 226/226 tests passed.

dotnet ef migrations has-pending-model-changes
No changes have been made to the model since the last migration.

dotnet ef database update
Applied pending Workspace credit migrations and `20260612020911_FixEfModelConfigurationWarnings` successfully.

Runtime smoke
GET /api/health -> 200
GET /swagger/index.html -> 200

dotnet list AISAM.sln package --vulnerable --include-transitive
No vulnerable packages found.
```

Commit đề xuất:

```text
feat(ai): enforce workspace credit usage
```

### Task 9.15 - Limited Mode, Archived và Admin Soft Delete lifecycle

Trạng thái:

```text
DONE - 2026-06-12
Task tiếp theo: 9.16 Chuyển ownership từng domain sang Workspace.
```

Mục tiêu:

- Áp dụng lifecycle Workspace hết hạn.

Cách test:

- Dưới 90 ngày Limited Mode.
- 90-180 ngày Archived: Owner View/Export/Renew, Member View Only.
- Trên 180 ngày chỉ Admin được Soft Delete.

Kết quả đã có:

- Business Workspace tự đồng bộ `Limited`, `Archived`, `EligibleForDeletion` theo `SubscriptionExpiredAt`.
- Limited/Archived/EligibleForDeletion chặn thao tác ghi; member vẫn được View; Owner vẫn được Billing/Renew và Export.
- `DELETE /api/workspaces/{id}` chỉ cho Admin và chỉ Soft Delete Workspace đã quá 180 ngày.
- PayOS renewal khôi phục Workspace về `Active`.
- Scheduled posting không chạy qua Workspace đã hết hạn.
- Automated tests xác minh mốc 90/180 ngày, read-only, renew và Admin Soft Delete.

Kiểm tra đã chạy:

```text
dotnet build AISAM-BE/AISAM.sln
Build succeeded.

dotnet test AISAM-BE/AISAM.sln
Passed. 242/242 tests passed.
```

Commit đề xuất:

```text
feat(workspace): add expiration lifecycle
```

### Task 9.16 - Chuyển ownership từng domain sang Workspace

Trạng thái:

```text
DONE - 2026-06-13
Task 9.17 và Task 9.18 đã hoàn thành; Phase 9 đã chốt.
```

Mục tiêu:

- Chuyển ownership Brand, Product, Content/Post, Social, Calendar, Conversation, Notification và Campaign.

Quy tắc commit:

- Mỗi domain là một commit riêng; không gom toàn bộ domain vào một commit.

Cách test:

- CRUD và isolation theo Workspace sau từng domain.
- Không phá API/module đã migrate trước đó.

Kết quả đã có:

- Brand có nullable `WorkspaceId`; Brand mới thuộc Active Workspace và CRUD/list được isolation theo Workspace.
- `ProfileId` của Brand được giữ làm metadata/audit compatibility; ownership bắt buộc dùng `WorkspaceId`.
- Product kế thừa ownership qua Brand; CRUD/list và pagination được lọc tại database theo `Brand.WorkspaceId`.
- Migration `AddBrandWorkspaceOwnership` đã được áp dụng thành công.
- Automated tests xác minh Brand/Product không đọc hoặc tạo dữ liệu xuyên Workspace.
- Content/Post, Social Account/Integration, Calendar, Conversation, Notification và Campaign đã có Workspace ownership.
- Các controller chuẩn đọc Active Workspace context và repository query isolation theo Workspace.
- AI draft/chat, schedule và scheduled posting tạo dữ liệu mới có `WorkspaceId`.
- Task 9.17 đã backfill dữ liệu cũ và khóa `WorkspaceId` ownership bắt buộc; `ProfileId` chỉ còn metadata/audit compatibility.
- Migration `AddRemainingDomainWorkspaceOwnership` đã được áp dụng thành công.
- Full automated tests xác minh isolation cho toàn bộ domain ownership.

Commit đề xuất:

```text
refactor(<domain>): move ownership to workspace
```

### Task 9.17 - Backfill dữ liệu cũ và khóa schema Workspace

Trạng thái:

```text
DONE - 2026-06-13
Task 9.18 đã hoàn thành; Phase 9 đã chốt.
```

Mục tiêu:

- Mỗi Profile cũ tạo một Personal Workspace.
- Backfill Subscription/resources/Credits rồi mới khóa schema mới.

Cách test:

- Chạy trên database test có dữ liệu cũ.
- Không mất dữ liệu.
- Migration rollback được trên database test.

Kết quả đã có:

- Migration `BackfillLegacyWorkspaceDataAndLockOwnership` tạo hoặc tái sử dụng Personal Workspace cho Profile cũ.
- Owner membership và Credit Wallet được tạo cho Workspace backfill còn thiếu.
- Subscription, Payment, Brand, Content, Social Account/Integration, Calendar, Conversation, Notification và Campaign được backfill sang Workspace.
- `workspace_id` của 10 bảng ownership đã khóa `NOT NULL`; migration dừng an toàn nếu còn dòng không ánh xạ được.
- Workspace runtime không còn suy luận ownership từ Profile trong scheduled posting hoặc billing.
- Migration đã được apply, rollback và apply lại thành công trên database dev/test.

Commit đề xuất:

```text
migration(workspace): backfill legacy profile data
```

### Task 9.18 - Workspace Dashboard, regression và tài liệu cuối Phase 9

Trạng thái: **DONE - 2026-06-13**

Kết quả:

- Đã thêm `GET /api/workspace-dashboard/summary`, dùng Active Workspace từ `X-Workspace-Id`.
- Dashboard tổng hợp đúng Credits Remaining, Posts Remaining, Published Posts, Total AI Usage và Top Members By Usage.
- Feature gate chỉ cho Business Plus/Premium sử dụng Workspace Dashboard; middleware tiếp tục xác minh membership.
- Regression test khóa việc lọc dữ liệu theo Workspace, chỉ tính giao dịch thành công, bỏ qua credit grant và member inactive.
- Audit cuối Phase 9 đã sửa Post Quota và Basic Dashboard bị trộn dữ liệu giữa các Workspace của cùng user/profile.
- Personal Workspace mới được provision atomically với Free subscription, Credit Wallet 50 và Free Credits reset theo chu kỳ 7 ngày.
- Migration `20260613130339_ProvisionMissingPersonalFreePlan` provision idempotent Free subscription/50 Credits cho Personal Workspace cũ còn thiếu; rollback và apply lại đã pass.
- Tạo Workspace luôn lưu Owner + Credit Wallet trong cùng EF graph; credit consume/grant và PayOS status application dùng transaction trên relational database.
- Invitation/accept yêu cầu active Business Plus hoặc Business Pro, không còn cho Business Workspace chưa có plan sử dụng Team Management.
- `dotnet build`, 268 automated tests, EF pending-model check/database update, package vulnerability scan và Swagger runtime smoke-test đều pass.

Mục tiêu:

- Hoàn thiện Workspace Dashboard và xác minh toàn bộ Phase 9.

Cách test:

- Credits/Posts/AI Usage/Top Members đúng.
- `dotnet build`, `dotnet test`, migration và Swagger/Postman regression pass.
- Cập nhật `BACKEND_CODE_PLAN.md`, `SETUP_GUIDE.md` và Change Request.

Commit đề xuất:

```text
test(workspace): complete workspace migration regression
```

Điều kiện hoàn thành:

- `X-Workspace-Id` hoạt động và kiểm tra membership.
- Brand, Content và Social Integration dùng Workspace ownership.
- Mỗi Workspace có đúng một Owner và một Credit Wallet.
- Subscription, Credits, Post Quota, Feature Gate và Permission Matrix hoạt động theo Workspace.
- Business Plus/Business Pro member limit hoạt động.
- Build, test, migration và API regression pass.

Không bắt đầu Phase 10 hoặc Phase 11 nếu Phase 9 chưa hoàn thành.

## Phase 11 - Facebook Ads Campaign MVP

Mục tiêu phase:

- Cung cấp luồng Campaign -> Ad Set -> Ad Creative -> Ad theo Active Workspace.
- Ưu tiên tái sử dụng Ads entities/schema hiện có.
- Chỉ triển khai Facebook Marketing API; chưa làm multi-platform Ads hoặc tự động tối ưu ngân sách.

Dependency bắt buộc:

- Phase 9 Workspace Migration hoàn thành.
- Phase 10 Admin Backend theo Workspace hoàn thành.
- Facebook App có Marketing API permissions phù hợp.
- Workspace đã liên kết Facebook Ad Account hợp lệ.
- Brand và Content dùng Workspace ownership.

### Task 11.1 - Kích hoạt Campaign repository và CRUD API local

Mục tiêu:

- Người dùng có quyền có thể tạo, xem, cập nhật và soft delete Campaign trong Workspace.

Loại task:

Copy từ source cũ / Cải tiến bắt buộc

Source cũ liên quan:

```text
PRN232_Backend/AISAM.Data/Model/AdCampaign.cs
PRN232_Backend/AISAM.Repositories/
PRN232_Backend/AISAM.Services/
PRN232_Backend/AISAM.API/Controllers/
```

File/thư mục repo mới:

```text
AISAM-BE/AISAM.Repositories/
AISAM-BE/AISAM.Services/
AISAM-BE/AISAM.API/Controllers/AdCampaignController.cs
AISAM-BE/tests/
```

Việc cần làm:

- Giữ entity/schema Ads hiện có nếu phù hợp.
- Chuyển Campaign ownership từ Profile sang Workspace.
- Thêm repository/service/controller CRUD.
- Kiểm tra Brand thuộc Active Workspace.
- Chưa gọi Facebook Marketing API trong task này.

Cải tiến so với source cũ nếu có:

Có: bắt buộc dùng Workspace ownership và Permission Matrix mới.

Commit đề xuất:

```text
feat(ads): add workspace campaign crud
```

Lệnh kiểm tra sau task:

```text
dotnet build
dotnet test
dotnet ef database update
dotnet run --project AISAM.API
```

API cần test bằng Swagger/Postman:

```text
POST   /api/ad-campaigns
GET    /api/ad-campaigns
GET    /api/ad-campaigns/{id}
PUT    /api/ad-campaigns/{id}
DELETE /api/ad-campaigns/{id}
```

Expected result:

- Workspace A không truy cập được Campaign của Workspace B.
- Viewer không tạo/sửa/xóa Campaign.
- Manager và Owner quản lý Campaign theo Permission Matrix.

### Task 11.2 - Kích hoạt Ad Set CRUD local

Mục tiêu:

- Quản lý Ad Set thuộc Campaign trong Active Workspace.

Việc cần làm:

- Thêm repository/service/controller cho Ad Set.
- Kiểm tra Campaign thuộc Active Workspace.
- Validate budget, schedule và targeting cơ bản.
- Chưa gọi Facebook Marketing API.

Commit đề xuất:

```text
feat(ads): add workspace ad set crud
```

API cần test:

```text
POST   /api/ad-sets
GET    /api/ad-sets/campaign/{campaignId}
GET    /api/ad-sets/{id}
PUT    /api/ad-sets/{id}
DELETE /api/ad-sets/{id}
```

Sau task phải chạy `dotnet build`, `dotnet test` và API test.

### Task 11.3 - Kích hoạt Ad Creative CRUD từ Content

Mục tiêu:

- Tạo Ad Creative local từ Content đã có trong cùng Workspace.

Việc cần làm:

- Thêm repository/service/controller cho Ad Creative.
- Kiểm tra Content, Brand và Campaign cùng Workspace.
- Validate text/media URL cần thiết.
- Chưa gọi Facebook Marketing API.

Commit đề xuất:

```text
feat(ads): add ad creative from workspace content
```

Sau task phải build/test/API test riêng.

### Task 11.4 - Kích hoạt Ad CRUD local

Mục tiêu:

- Tạo Ad liên kết Ad Set và Ad Creative trong cùng Workspace.

Việc cần làm:

- Thêm repository/service/controller cho Ad.
- Kiểm tra Ad Set và Ad Creative cùng Workspace.
- Quản lý trạng thái local Draft/Ready/Paused theo enum hiện có nếu phù hợp.

Commit đề xuất:

```text
feat(ads): add workspace ad crud
```

Sau task phải build/test/API test riêng.

### Task 11.5 - Liên kết Facebook Ad Account và Marketing API client

Mục tiêu:

- Xác minh Ad Account đã liên kết và cung cấp client gọi Facebook Marketing API.

Loại task:

Security hardening / Viết mới

Việc cần làm:

- Dùng Facebook config/provider hiện có.
- Không log access token.
- Kiểm tra Ad Account thuộc Social Integration của Active Workspace.
- Trả lỗi rõ khi thiếu permission/token/config.

Commit đề xuất:

```text
feat(ads): add facebook marketing api client
```

API test:

- List/validate linked Ad Accounts.
- Invalid token/permission trả lỗi rõ.

### Task 11.6 - Publish Campaign structure lên Facebook

Mục tiêu:

- Tạo Campaign, Ad Set, Ad Creative và Ad trên Facebook theo thứ tự an toàn.

Việc cần làm:

- Sync từng resource và lưu Facebook ID.
- Không tạo resource con nếu parent chưa sync thành công.
- Lưu trạng thái lỗi để retry thủ công.
- Không tự động bật chạy Ads nếu chưa được người dùng xác nhận.

Commit đề xuất:

```text
feat(ads): publish campaign structure to facebook
```

API cần test:

```text
POST /api/ad-campaigns/{id}/sync
POST /api/ad-sets/{id}/sync
POST /api/ad-creatives/{id}/sync
POST /api/ads/{id}/sync
```

### Task 11.7 - Sync status và basic insights

Mục tiêu:

- Đồng bộ trạng thái và chỉ số cơ bản của Ads đã publish.

Việc cần làm:

- Sync status Campaign/Ad Set/Ad/Creative.
- Đọc basic insights từ Facebook khi permission cho phép.
- Không triển khai auto-optimization.

Commit đề xuất:

```text
feat(ads): sync facebook ad status and basic insights
```

### Task 11.8 - Facebook Ads regression, security và documentation

Mục tiêu:

- Xác minh luồng Campaign end-to-end không phá module đã hoàn thành.

Việc cần làm:

- Unit/integration tests cho ownership, permission và validation.
- Swagger/Postman test local CRUD.
- Test Marketing API bằng Facebook test account/ad account.
- Cập nhật `SETUP_GUIDE.md`, API docs và progress log.

Commit đề xuất:

```text
test(ads): harden facebook campaign workflow
```

Phase 11 chỉ hoàn thành khi:

- [ ] Campaign/Ad Set/Ad Creative/Ad CRUD local pass.
- [ ] Workspace isolation và Permission Matrix pass.
- [ ] Facebook Ad Account validation pass.
- [ ] Sync Facebook IDs/status pass hoặc trả lỗi provider rõ ràng.
- [ ] Không tự động bật Ads ngoài xác nhận người dùng.
- [ ] `dotnet build` pass.
- [ ] `dotnet test` pass.
- [ ] Migration pass.
- [ ] Swagger/Postman test được ghi lại.

## Backend MVP Definition of Done

Backend MVP chỉ được xem là xong khi:

- [ ] `dotnet build` pass.
- [ ] `dotnet test` pass.
- [ ] Database migration chạy được.
- [ ] Swagger mở được.
- [ ] Auth register/login/refresh/me chạy được.
- [ ] Profile/brand/product CRUD cơ bản chạy được.
- [ ] AI generate/refine API chạy được hoặc trả lỗi config rõ ràng.
- [ ] Content draft CRUD chạy được.
- [ ] Facebook auth URL/social account APIs chạy được.
- [ ] Publish content lên Facebook chạy được với token hợp lệ hoặc fail rõ ràng.
- [ ] Schedule API chạy được.
- [ ] Payment/subscription API chạy được hoặc trả lỗi config PayOS rõ ràng.
- [ ] Quota display API chạy được.
- [ ] Admin user/payment/subscription APIs chạy được.
- [ ] API test bằng Swagger/Postman đã được ghi lại.
- [ ] Không có task nào gom nhiều module vào một commit.

## Những phần chưa đưa vào MVP backend

Không làm trong MVP backend đầu tiên:

- TikTok integration thật.
- Instagram Business integration đầy đủ.
- Facebook Ads end-to-end chưa thuộc baseline hiện tại; đã được đưa vào Phase 11 sau Workspace và Admin.
- Video AI generation.
- Mobile app APIs riêng.
- AI cost tracking chi tiết.
- Team approval workflow phức tạp.
- Analytics real-time từ nhiều social platforms.

Các phần này chỉ làm sau khi backend MVP ổn định và đã có frontend sử dụng các API core.

## Progress Log

> Từ thời điểm này, sau mỗi task hoàn thành phải cập nhật progress log này trước khi chuyển task tiếp theo.

| Task | Trạng thái | Commit đề xuất | Build | Test | Migration | API/Swagger test | Ghi chú |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Task 0.1 - Tạo cấu trúc repo backend mới | Done | `chore(solution): initialize backend solution structure` | Pass: `dotnet build` | Pass: `dotnet test` - 1/1 | N/A | N/A | Đã tạo `AISAM.sln`, `AISAM.API`, `AISAM.Services`, `AISAM.Repositories`, `AISAM.Data`, `AISAM.Common`, `tests/AISAM.IntegrationTests`; đã nối project references theo baseline. |
| Task 0.2 - Copy cấu hình project và package cơ bản từ source cũ | Done | `chore(projects): migrate backend project package references` | Pass: `dotnet build` | Pass: `dotnet test` - 1/1 | N/A | N/A | Đã migrate package references/root namespace từ các `.csproj` cũ. Chưa migrate code nghiệp vụ. |
| Task 1.1 - Migrate Program.cs tối thiểu | Done | `chore(api): add minimal api host and swagger` | Pass: `dotnet build` | Pass: `dotnet test` - 1/1 | N/A | Pass: `GET /swagger/index.html` status 200 on `http://localhost:5081` | Đã thay WeatherForecast template bằng API host tối thiểu; thêm `ExceptionHandlerMiddleware`, `ValidationFilter`, `GenericResponse`. `GenericResponse` được copy sớm vì middleware/filter phụ thuộc. |
| Task 1.2 - Thêm HealthController | Done | `feat(api): add health check endpoint` | Pass: `dotnet build` | Pass: `dotnet test` - 1/1 | N/A | Pass: `GET /api/health` status 200 on `http://localhost:5082` | Đã thêm health endpoint không phụ thuộc DB/secrets. Tạm tắt `UseHttpsRedirection()` trong local minimal host để tránh lỗi Windows Event Log khi chưa cấu hình HTTPS port. |
| Task 2.1 - Copy Common response, config, DTO auth/user/profile nền tảng | Done | `chore(common): migrate shared response and auth dto contracts` | Pass: `dotnet build` | Pass: `dotnet test` - 1/1 | N/A | N/A | Đã copy config/DTO nền tảng từ source cũ. `GenericResponse` đã có từ Task 1.1. Copy thêm dependency tối thiểu `SocialDtos` và `UserRoleEnum` để DTO build được. |
| Task 2.2 - Copy entity và enum nền tảng | Done | `chore(data): migrate core domain entities and enums` | Pass: `dotnet build` | Pass: `dotnet test` - 1/1 | N/A | N/A | Đã copy toàn bộ `AISAM.Data/Model` và `AISAM.Data/Enumeration` từ source cũ để giữ nguyên quan hệ entity. Ads entities chỉ được copy như dependency model, chưa bật Ads module/API/service. |
| Task 2.3 - Copy AisamContext và migration cũ | Done | `chore(data): migrate db context and existing migrations` | Pass: `dotnet build` - 0 warnings | Pass: `dotnet test` - 1/1 | Skipped: chưa có local connection string | Pass: `GET /api/health` status 200 on `http://localhost:5083` | Đã copy `AisamContext` và migrations cũ; đăng ký DbContext có điều kiện khi có connection string. Pin `Microsoft.EntityFrameworkCore.Relational 9.0.9` ở `AISAM.Services` để build sạch do dependency Supabase/Npgsql kéo version thấp hơn. |
| DB Setup - Kết nối PostgreSQL local | Done | `chore(data): add design time db context factory` | Pass: `dotnet build` - còn 2 warning migration cũ `verifytoken` | Pass: `dotnet test --no-build` - 1/1 | Pass: applied 5 migrations to `aisam_dev` | Pass: `GET /api/health` status 200 on `http://localhost:5084` | Đã thêm `.gitignore`, `AISAM.API/.env.example`, `AisamContextFactory`; `.env` local chứa connection string thật và đã được ignore. |
| Setup Guide - Manual backend configuration | Done | `docs(backend): add setup guide for manual configuration` | N/A | N/A | N/A | N/A | Đã tạo `SETUP_GUIDE.md`, ghi rõ REQUIRED hiện tại và Optional/Future configs cho PostgreSQL, JWT, CORS, SMTP, Google, Facebook, Gemini, PayOS, Supabase. |
| Task 3.1 - Copy repositories cho User và Session | Done | `chore(auth): migrate user and session repositories` | Pass: `dotnet build` - còn 2 warning migration cũ `verifytoken` | Pass: `dotnet test --no-build` - 1/1 | N/A | Pass: `GET /api/health` status 200 on `http://localhost:5085` | Đã copy `IUserRepository`, `ISessionRepository`, `UserRepository`, `SessionRepository`; copy thêm dependency `UserListDto`; đăng ký DI trong `Program.cs`. |
| Task 3.2 - Copy AuthService và EmailService ở mức MVP | Done | `feat(auth): migrate auth and email services` | Pass: `dotnet build` - còn 2 warning migration cũ `verifytoken` | Pass: `dotnet test --no-build` - 1/1 | N/A | Pass: `GET /api/health` status 200 on `http://localhost:5086` | Đã copy `IAuthService`, `IEmailService`, `AuthService`, `EmailService`; copy thêm `EmailRequest`, `FrontendSettings`; đăng ký DI/options và env overrides trong `Program.cs`; `.env.example` có JWT/SMTP/Google placeholders. |
| Task 3.3 - Copy AuthController và bật JWT authentication | Done | `feat(auth): migrate authentication api endpoints` | Pass: `dotnet build` - 0 warnings | Pass: `dotnet test --no-build` - 1/1 | N/A | Pass: register/login/me/refresh/logout on `http://localhost:5088` | Đã copy `AuthController`, bật JWT Bearer, Swagger Bearer auth, `UseAuthentication/UseAuthorization`; chỉnh logging sang Console để tránh Windows Event Log crash; chưa copy `UserClaimsHelper` vì phụ thuộc `IUserService` ngoài phạm vi task. |

### Progress Detail - Task 0.1

Ngày hoàn thành: 2026-05-28

File/thư mục tạo mới:

- `AISAM-BE/AISAM.sln`
- `AISAM-BE/AISAM.API/`
- `AISAM-BE/AISAM.Services/`
- `AISAM-BE/AISAM.Repositories/`
- `AISAM-BE/AISAM.Data/`
- `AISAM-BE/AISAM.Common/`
- `AISAM-BE/tests/AISAM.IntegrationTests/`

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test
Passed. 1/1 tests passed.
```

### Progress Detail - Task 0.2

Ngày hoàn thành: 2026-05-28

Source cũ đã đối chiếu:

- `PRN232_Backend/AISAM.API/AISAM.API.csproj`
- `PRN232_Backend/AISAM.Services/AISAM.Services.csproj`
- `PRN232_Backend/AISAM.Repositories/AISAM.Repositories.csproj`
- `PRN232_Backend/AISAM.Data/AISAM.Data.csproj`
- `PRN232_Backend/AISAM.Common/AISAM.Common.csproj`
- `PRN232_Backend/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj`

File đã sửa:

- `AISAM-BE/AISAM.API/AISAM.API.csproj`
- `AISAM-BE/AISAM.Services/AISAM.Services.csproj`
- `AISAM-BE/AISAM.Repositories/AISAM.Repositories.csproj`
- `AISAM-BE/AISAM.Data/AISAM.Data.csproj`
- `AISAM-BE/AISAM.Common/AISAM.Common.csproj`

Kết quả kiểm tra:

```text
dotnet restore
OK

dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test
Passed. 1/1 tests passed.
```

Ghi chú:

- Task này chỉ migrate package/project config, chưa migrate code nghiệp vụ.
- Sau build/test có nhiều file `bin/obj` phát sinh; chưa xử lý `.gitignore` trong task này.

### Progress Detail - Task 1.1

Ngày hoàn thành: 2026-05-28

Source cũ đã dùng:

- `PRN232_Backend/AISAM.API/Program.cs`
- `PRN232_Backend/AISAM.API/Middleware/ExceptionHandlerMiddleware.cs`
- `PRN232_Backend/AISAM.API/Filters/ValidationFilter.cs`
- `PRN232_Backend/AISAM.Common/GenericResponse.cs`

File đã tạo/sửa:

- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/AISAM.API/Middleware/ExceptionHandlerMiddleware.cs`
- `AISAM-BE/AISAM.API/Filters/ValidationFilter.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`

Cải tiến/điều chỉnh:

- Không copy nguyên `Program.cs` cũ vì startup cũ phụ thuộc nhiều module chưa migrate như DB, auth, hosted services, Facebook, Gemini, PayOS, Supabase.
- Tạo API host tối thiểu trước để Swagger chạy được.
- Copy `GenericResponse` sớm hơn plan vì middleware/filter cần để build.
- Sửa nullable warning trong middleware/filter để build sạch.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test
Passed. 1/1 tests passed.
```

API/Swagger test:

```text
GET http://localhost:5081/swagger/index.html
STATUS=200
```

Ghi chú:

- API chỉ chạy tạm để test Swagger và đã được dừng lại.
- Chưa bật database, authentication, DI nghiệp vụ hoặc hosted services.

### Progress Detail - Task 1.2

Ngày hoàn thành: 2026-05-28

Source cũ đã dùng:

- `PRN232_Backend/src/AISAM.Api/Controllers/HealthController.cs`

File đã tạo/sửa:

- `AISAM-BE/AISAM.API/Controllers/HealthController.cs`
- `AISAM-BE/AISAM.API/Program.cs`

Cải tiến/điều chỉnh:

- HealthController source cũ ở nhánh `src` dùng `ApiResponse` của kiến trúc thử nghiệm; repo mới đang dùng `GenericResponse`, nên endpoint mới trả `GenericResponse<object>` để đồng nhất API hiện tại.
- Tạm tắt `UseHttpsRedirection()` trong `Program.cs` vì local runtime chưa có HTTPS port. Khi bật, middleware cố log warning vào Windows Event Log và gây lỗi `Cannot open log for source '.NET Runtime'` trong môi trường hiện tại.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test
Passed. 1/1 tests passed.
```

API test:

```text
GET http://localhost:5082/api/health
STATUS=200
```

Response mẫu:

```json
{
  "success": true,
  "message": "AISAM backend is ready.",
  "statusCode": 200,
  "data": {
    "status": "Healthy",
    "service": "AISAM Backend"
  }
}
```

Ghi chú:

- API chỉ chạy tạm để test health endpoint và đã được dừng lại.
- Chưa bật database/auth/module nghiệp vụ.

### Progress Detail - Task 2.1

Ngày hoàn thành: 2026-05-28

Source cũ đã dùng:

- `PRN232_Backend/AISAM.Common/GenericResponse.cs`
- `PRN232_Backend/AISAM.Common/Config/JwtSettings.cs`
- `PRN232_Backend/AISAM.Common/Config/EmailSettings.cs`
- `PRN232_Backend/AISAM.Common/Config/GoogleSettings.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/AuthRequest.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Response/AuthResponse.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Response/UserResponseDto.cs`
- `PRN232_Backend/AISAM.Common/Dtos/PaginationDtos.cs`
- `PRN232_Backend/AISAM.Common/Models/SocialDtos.cs`
- `PRN232_Backend/AISAM.Data/Enumeration/UserRoleEnum.cs`

File đã tạo/sửa:

- `AISAM-BE/AISAM.Common/Config/JwtSettings.cs`
- `AISAM-BE/AISAM.Common/Config/EmailSettings.cs`
- `AISAM-BE/AISAM.Common/Config/GoogleSettings.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/AuthRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/AuthResponse.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/UserResponseDto.cs`
- `AISAM-BE/AISAM.Common/Dtos/PaginationDtos.cs`
- `AISAM-BE/AISAM.Common/Models/SocialDtos.cs`
- `AISAM-BE/AISAM.Data/Enumeration/UserRoleEnum.cs`

Cải tiến/điều chỉnh:

- Không cải tiến nghiệp vụ.
- `GenericResponse.cs` đã được copy sớm ở Task 1.1 vì middleware/filter cần để build.
- Copy thêm `SocialDtos.cs` vì `UserResponseDto` phụ thuộc `SocialAccountDto`.
- Copy thêm `UserRoleEnum.cs` vì `AuthResponse` phụ thuộc `UserRoleEnum`; phần enum còn lại vẫn để Task 2.2.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test
Passed. 1/1 tests passed.
```

API test:

```text
Không áp dụng, task này chưa thêm endpoint mới.
```

### Progress Detail - Task 2.2

Ngày hoàn thành: 2026-05-28

Source cũ đã dùng:

- `PRN232_Backend/AISAM.Data/Model/*`
- `PRN232_Backend/AISAM.Data/Enumeration/*`

File/thư mục đã tạo/sửa:

- `AISAM-BE/AISAM.Data/Model/*`
- `AISAM-BE/AISAM.Data/Enumeration/*`
- Xóa template rỗng:
  - `AISAM-BE/AISAM.Data/Class1.cs`
  - `AISAM-BE/AISAM.Common/Class1.cs`
  - `AISAM-BE/AISAM.Repositories/Class1.cs`
  - `AISAM-BE/AISAM.Services/Class1.cs`

Cải tiến/điều chỉnh:

- Không cải tiến nghiệp vụ.
- Kế hoạch ban đầu muốn tránh Ads entities ở MVP, nhưng các entity core như `Brand`, `Profile`, `Content`, `Post` có navigation property tới `AdCampaign`, `AdCreative`, `PerformanceReport`.
- Để không refactor model baseline và tránh phá quan hệ entity, task này copy toàn bộ `Model` và `Enumeration`.
- Việc copy Ads entities ở đây chỉ là dependency domain model; chưa triển khai controller/service/repository Ads.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test
Passed. 1/1 tests passed.
```

API test:

```text
Không áp dụng, task này chưa thêm endpoint mới.
```

### Progress Detail - Task 2.3

Ngày hoàn thành: 2026-05-28

Source cũ đã dùng:

- `PRN232_Backend/AISAM.Repositories/AISAMContext.cs`
- `PRN232_Backend/AISAM.Repositories/Migrations/*`

File/thư mục đã tạo/sửa:

- `AISAM-BE/AISAM.Repositories/AISAMContext.cs`
- `AISAM-BE/AISAM.Repositories/Migrations/*`
- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/AISAM.API/appsettings.Development.json`
- `AISAM-BE/AISAM.Services/AISAM.Services.csproj`

Cải tiến/điều chỉnh:

- Không bật auto migration khi app start. Migration sẽ chạy thủ công bằng `dotnet ef database update` sau khi có connection string local.
- `DbContext` chỉ được đăng ký khi có `CONNECTION_STRING` hoặc `ConnectionStrings:DefaultConnection`, giúp API host và health endpoint vẫn chạy được khi developer mới clone repo nhưng chưa setup database.
- Pin `Microsoft.EntityFrameworkCore.Relational` version `9.0.9` trong `AISAM.Services` để tránh conflict giữa EF Core `9.0.9` và dependency transitive `9.0.1` từ Supabase/Npgsql.
- Không đổi tên migration class cũ để giữ nguyên migration history baseline.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test
Passed. 1/1 tests passed.
```

Migration:

```text
dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API
Skipped: chưa có local connection string trong appsettings.Development.json hoặc .env.
```

API test:

```text
GET http://localhost:5083/api/health
STATUS=200
```

Response mẫu:

```json
{
  "success": true,
  "message": "AISAM backend is ready.",
  "statusCode": 200,
  "data": {
    "status": "Healthy",
    "service": "AISAM Backend"
  }
}
```

Ghi chú:

- API chỉ chạy tạm để test health endpoint và đã được dừng lại.
- Các module cần database chỉ nên bật/test sau khi cấu hình PostgreSQL và chạy migration thành công.

### Progress Detail - DB Setup PostgreSQL local

Ngày hoàn thành: 2026-05-28

Mục tiêu:

- Kết nối backend repo mới tới PostgreSQL local của developer.
- Chạy migration cũ vào database `aisam_dev`.

File đã tạo/sửa:

- `.gitignore`
- `AISAM-BE/AISAM.API/.env.example`
- `AISAM-BE/AISAM.API/.env` local, không commit
- `AISAM-BE/AISAM.Repositories/AisamContextFactory.cs`
- `SETUP_GUIDE.md`

Cải tiến/điều chỉnh:

- Thêm `IDesignTimeDbContextFactory<AisamContext>` để `dotnet ef database update` tạo DbContext trực tiếp, không cần dựng API host/logger.
- `.env` thật được ignore để tránh commit secret.
- `SETUP_GUIDE.md` đã đổi đúng đường dẫn `.env` sang `AISAM-BE/AISAM.API/.env`.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 2 warnings từ migration cũ `verifytoken`, 0 errors.

dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
Applied migrations:
- 20251102025736_Initial
- 20260124120929_AddCustomAuthenticationTables
- 20260124133308_verifytoken
- 20260124135926_UpdatePasswordSaltLength
- 20260127160619_UpdateSubscriptionPayOS

dotnet test --no-build
Passed. 1/1 tests passed.

GET http://localhost:5084/api/health
STATUS=200
```

Ghi chú:

- Cảnh báo EF tools version `8.0.10` thấp hơn runtime `9.0.9` chưa chặn migration, nhưng nên update dotnet-ef sau.
- Không ghi password DB thật vào tài liệu hoặc file tracked.

### Progress Detail - Task 3.1

Ngày hoàn thành: 2026-05-29

Source cũ đã dùng:

- `PRN232_Backend/AISAM.Repositories/IRepositories/IUserRepository.cs`
- `PRN232_Backend/AISAM.Repositories/IRepositories/ISessionRepository.cs`
- `PRN232_Backend/AISAM.Repositories/Repository/UserRepository.cs`
- `PRN232_Backend/AISAM.Repositories/Repository/SessionRepository.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Response/UserListDto.cs`

File đã tạo/sửa:

- `AISAM-BE/AISAM.Repositories/IRepositories/IUserRepository.cs`
- `AISAM-BE/AISAM.Repositories/IRepositories/ISessionRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/UserRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/SessionRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/UserListDto.cs`
- `AISAM-BE/AISAM.API/Program.cs`

Cải tiến/điều chỉnh:

- Không cải tiến nghiệp vụ repository; giữ nguyên baseline.
- Copy thêm `UserListDto` vì `IUserRepository.GetPagedUsersAsync` và `UserRepository` phụ thuộc DTO này.
- Đăng ký DI cho `IUserRepository` và `ISessionRepository` trong `Program.cs`.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 2 warnings từ migration cũ `verifytoken`, 0 errors.

dotnet test --no-build
Passed. 1/1 tests passed.

GET http://localhost:5085/api/health
STATUS=200
```

Migration:

```text
Không áp dụng, task này không đổi schema database.
```

API test:

```text
Không có API mới. Smoke test `/api/health` pass để chứng minh API host không bị phá.
```

### Progress Detail - Task 3.2

Ngày hoàn thành: 2026-05-29

Source cũ đã dùng:

- `PRN232_Backend/AISAM.Services/IServices/IAuthService.cs`
- `PRN232_Backend/AISAM.Services/IServices/IEmailService.cs`
- `PRN232_Backend/AISAM.Services/Service/AuthService.cs`
- `PRN232_Backend/AISAM.Services/Service/EmailService.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/EmailRequest.cs`
- `PRN232_Backend/AISAM.Common/Models/FrontendSettings.cs`
- `PRN232_Backend/AISAM.API/Program.cs` để đối chiếu options/env override pattern

File đã tạo/sửa:

- `AISAM-BE/AISAM.Services/IServices/IAuthService.cs`
- `AISAM-BE/AISAM.Services/IServices/IEmailService.cs`
- `AISAM-BE/AISAM.Services/Service/AuthService.cs`
- `AISAM-BE/AISAM.Services/Service/EmailService.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/EmailRequest.cs`
- `AISAM-BE/AISAM.Common/Models/FrontendSettings.cs`
- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/AISAM.API/.env.example`
- `AISAM-BE/AISAM.API/.env` local, không commit

Cải tiến/điều chỉnh:

- Không cải tiến nghiệp vụ auth core; giữ baseline register/login/refresh/logout/password/email verification.
- Giữ behavior fail-safe của `EmailService`: nếu thiếu SMTP config thì log warning và trả `false`, không throw làm hỏng register local.
- Bổ sung env override tối thiểu cho `JwtSettings`, `EmailSettings`, `GoogleSettings`, `FrontendSettings`.
- Bổ sung JWT dev config vào `.env` local để Task 3.3 có thể test register/login; `.env` đã được ignore.
- Chưa bật JWT authentication middleware trong task này; phần đó thuộc Task 3.3.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 2 warnings từ migration cũ `verifytoken`, 0 errors.

dotnet test --no-build
Passed. 1/1 tests passed.

GET http://localhost:5086/api/health
STATUS=200
```

Migration:

```text
Không áp dụng, task này không đổi schema database.
```

API test:

```text
Không có API mới. Smoke test `/api/health` pass để chứng minh API host không bị phá.
```

### Progress Detail - Task 3.3

Ngày hoàn thành: 2026-05-29

Source cũ đã dùng:

- `PRN232_Backend/AISAM.API/Controllers/AuthController.cs`
- `PRN232_Backend/AISAM.API/Program.cs` để đối chiếu JWT Bearer và Swagger Bearer setup
- `PRN232_Backend/AISAM.API/Utils/UserClaimsHelper.cs` đã kiểm tra nhưng chưa copy

File đã tạo/sửa:

- `AISAM-BE/AISAM.API/Controllers/AuthController.cs`
- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/AISAM.Common/GenericResponse.cs`

Cải tiến/điều chỉnh:

- Bật JWT Bearer authentication và authorization trong repo mới.
- Bật Swagger Bearer auth để test API protected bằng token.
- Cấu hình logging chỉ dùng Console trong local API host để tránh lỗi Windows Event Log `Cannot open log for source '.NET Runtime'`.
- Sửa `GenericResponse<T>.CreateSuccess` nhận `T?` vì nhiều endpoint success hợp lệ có `data = null`; không đổi JSON response contract.
- Chưa copy `UserClaimsHelper` trong task này vì file cũ phụ thuộc `IUserService`, nếu copy sẽ kéo thêm user service ngoài scope Auth MVP task. `AuthController` hiện không cần helper này.
- Không copy validators vì source cũ không có validator auth riêng; DTO auth đang dùng DataAnnotations.

Kết quả kiểm tra:

```text
dotnet build
Build succeeded. 0 warnings, 0 errors.

dotnet test --no-build
Passed. 1/1 tests passed.
```

Migration:

```text
Không áp dụng, task này không đổi schema database.
```

API test:

```text
POST http://localhost:5088/api/auth/register
REGISTER_SUCCESS=True
REGISTER_HAS_ACCESS_TOKEN=True
REGISTER_HAS_REFRESH_TOKEN=True

POST http://localhost:5088/api/auth/login
LOGIN_SUCCESS=True
LOGIN_HAS_ACCESS_TOKEN=True

GET http://localhost:5088/api/auth/me
ME_SUCCESS=True
ME_EMAIL=task33_20260529193317@example.com

POST http://localhost:5088/api/auth/refresh
REFRESH_SUCCESS=True

POST http://localhost:5088/api/auth/logout
LOGOUT_SUCCESS=True
```

Ghi chú:

- Request register thực tế cần `confirmPassword` vì `RegisterRequest` có `[Required]` và `[Compare]`.
- SMTP chưa cấu hình nên email verification được log/skip fail-safe theo `EmailService`, không làm hỏng register local.

### Progress Detail - Task 4.1

Ngay hoan thanh: 2026-05-29

Source cu da dung:

- `PRN232_Backend/AISAM.API/Controllers/ProfileController.cs`
- `PRN232_Backend/AISAM.Services/IServices/IProfileService.cs`
- `PRN232_Backend/AISAM.Services/Service/ProfileService.cs`
- `PRN232_Backend/AISAM.Repositories/IRepositories/IProfileRepository.cs`
- `PRN232_Backend/AISAM.Repositories/Repository/ProfileRepository.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/CreateProfileRequest.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/UpdateProfileRequest.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Response/ProfileResponseDto.cs`

File da tao/sua:

- `AISAM-BE/AISAM.API/Controllers/ProfileController.cs`
- `AISAM-BE/AISAM.Services/IServices/IProfileService.cs`
- `AISAM-BE/AISAM.Services/Service/ProfileService.cs`
- `AISAM-BE/AISAM.Repositories/IRepositories/IProfileRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ProfileRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateProfileRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateProfileRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ProfileResponseDto.cs`
- `AISAM-BE/AISAM.API/Program.cs`

Cai tien/dieu chinh so voi source cu:

- Giu route va DTO profile tu source cu de API contract quen thuoc.
- Khong copy nguyen `ProfileService` cu vi no keo them `SupabaseStorageService`, `ITeamService`, `ITeamMemberRepository`; cac module nay chua thuoc Task 4.1.
- `AvatarFile` chua upload that trong MVP hien tai; neu gui file se tra loi loi ro rang va developer co the dung `AvatarUrl`.
- `SearchUserProfilesAsync` tam thoi chi lay profiles so huu boi user, chua lay shared profiles qua TeamMembers vi Team module chua migrate.
- Sua text log/error trong `ProfileController` bi loi encoding tu source cu sang ASCII ro rang, khong doi route/behavior chinh.

Ket qua kiem tra:

```text
dotnet build
Build succeeded. 2 warnings tu migration cu `verifytoken`, 0 errors.

dotnet test --no-build
Passed. 1/1 tests passed.

dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
No migrations were applied. The database is already up to date.
```

API test:

```text
POST /api/Auth/register
Lay accessToken va userId cho profile test.

POST /api/profiles/user/{userId}
Content-Type: multipart/form-data
Authorization: Bearer {accessToken}
Fields: Name, ProfileType, CompanyName, Bio
CREATE_SUCCESS=True

PUT /api/profiles/{profileId}
Content-Type: multipart/form-data
Authorization: Bearer {accessToken}
Fields: Name, Bio
UPDATE_SUCCESS=True

GET /api/profiles/user/{userId}
Authorization: Bearer {accessToken}
GET_SUCCESS=True

DELETE /api/profiles/{profileId}
Authorization: Bearer {accessToken}
DELETE_SUCCESS=True

PATCH /api/profiles/{profileId}/restore
Authorization: Bearer {accessToken}
RESTORE_SUCCESS=True
```

Checklist:

- [x] Build thanh cong.
- [x] Test pass.
- [x] Migration kiem tra: database already up to date, khong co migration moi.
- [x] API test thanh cong.
- [x] Khong pha module Auth/Health da hoan thanh.
- [ ] Commit rieng task nay.

### Progress Detail - Task 4.2

Ngay hoan thanh: 2026-05-29

Source cu da dung:

- `PRN232_Backend/AISAM.API/Controllers/BrandController.cs`
- `PRN232_Backend/AISAM.Services/IServices/IBrandService.cs`
- `PRN232_Backend/AISAM.Services/Service/BrandService.cs`
- `PRN232_Backend/AISAM.Repositories/IRepositories/IBrandRepository.cs`
- `PRN232_Backend/AISAM.Repositories/Repository/BrandRepository.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/CreateBrandRequest.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/UpdateBrandRequest.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Response/BrandResponseDto.cs`

File da tao/sua:

- `AISAM-BE/AISAM.API/Controllers/BrandController.cs`
- `AISAM-BE/AISAM.Services/IServices/IBrandService.cs`
- `AISAM-BE/AISAM.Services/Service/BrandService.cs`
- `AISAM-BE/AISAM.Repositories/IRepositories/IBrandRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/BrandRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateBrandRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateBrandRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/BrandResponseDto.cs`
- `AISAM-BE/AISAM.API/Program.cs`

Cai tien/dieu chinh so voi source cu:

- Giu DTO va entity mapping tu source cu.
- Khong copy nguyen `BrandService` cu vi no keo them `ITeamMemberRepository`, `ITeamRepository`, `IProductRepository`, `IContentRepository`, `RolePermissionConfig`.
- Brand MVP hien tai chi ho tro CRUD brand theo profile owner; shared brand qua Team se lam sau khi Team module duoc migrate.
- `GET /api/brands` dung `profileId` query thay vi `ProfileContextHelper`/active profile context vi FE va profile context middleware chua co.
- Chua soft delete/restore cascade products vi Product module chua migrate trong task nay.
- Sua controller ve text ASCII ro rang, lay userId truc tiep tu JWT claim `ClaimTypes.NameIdentifier`.

Ket qua kiem tra:

```text
dotnet build
Build succeeded. 2 warnings tu migration cu `verifytoken`, 0 errors.

dotnet test --no-build
Passed. 1/1 tests passed.

dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
No migrations were applied. The database is already up to date.
```

API test:

```text
POST /api/Auth/register
POST /api/profiles/user/{userId}

POST /api/brands
CREATE_BRAND_SUCCESS=True

GET /api/brands?profileId={profileId}&page=1&pageSize=10
LIST_BRAND_SUCCESS=True
LIST_TOTAL=1

GET /api/brands/{brandId}
GET_BRAND_SUCCESS=True

PUT /api/brands/{brandId}
UPDATE_BRAND_SUCCESS=True
UPDATED_NAME=AISAM Updated Brand

DELETE /api/brands/{brandId}
DELETE_BRAND_SUCCESS=True

POST /api/brands/{brandId}/restore
RESTORE_BRAND_SUCCESS=True
```

Checklist:

- [x] Build thanh cong.
- [x] Test pass.
- [x] Migration kiem tra: database already up to date, khong co migration moi.
- [x] API test thanh cong.
- [x] Khong pha module Auth/Profile/Health da hoan thanh.
- [ ] Commit rieng task nay.

### Progress Detail - Task 4.3

Ngay hoan thanh: 2026-05-29

Source cu da dung:

- `PRN232_Backend/AISAM.API/Controllers/ProductController.cs`
- `PRN232_Backend/AISAM.Services/IServices/IProductService.cs`
- `PRN232_Backend/AISAM.Services/Service/ProductService.cs`
- `PRN232_Backend/AISAM.Repositories/IRepositories/IProductRepository.cs`
- `PRN232_Backend/AISAM.Repositories/Repository/ProductRepository.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/ProductCreateRequest.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Request/ProductUpdateRequest.cs`
- `PRN232_Backend/AISAM.Common/Dtos/Response/ProductResponseDto.cs`

File da tao/sua:

- `AISAM-BE/AISAM.API/Controllers/ProductController.cs`
- `AISAM-BE/AISAM.Services/IServices/IProductService.cs`
- `AISAM-BE/AISAM.Services/Service/ProductService.cs`
- `AISAM-BE/AISAM.Repositories/IRepositories/IProductRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ProductRepository.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/ProductCreateRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/ProductUpdateRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ProductResponseDto.cs`
- `AISAM-BE/AISAM.API/Program.cs`

Cai tien/dieu chinh so voi source cu:

- Giu DTO, entity mapping va route product tu source cu.
- Khong copy nguyen `ProductService` cu vi no phu thuoc `SupabaseStorageService` de upload anh; Supabase chua bat buoc trong MVP hien tai.
- Bo `[Required]` khoi `ProductCreateRequest.ImageFiles` de co the tao product khong can upload anh trong local MVP.
- Neu gui `ImageFiles`, service tra loi loi ro rang: product image upload chua bat trong MVP.
- Tat ca endpoint product duoc bao ve bang JWT va kiem tra user la owner cua Brand/Profile.
- `GET /api/products` ho tro `brandId` query de test product theo brand da tao.

Ket qua kiem tra:

```text
dotnet build
Build succeeded. 2 warnings tu migration cu `verifytoken`, 0 errors.

dotnet test --no-build
Passed. 1/1 tests passed.

dotnet ef database update --project AISAM.Repositories --startup-project AISAM.API --no-build
No migrations were applied. The database is already up to date.
```

API test:

```text
POST /api/Auth/register
POST /api/profiles/user/{userId}
POST /api/brands

POST /api/products
Content-Type: multipart/form-data
Fields: BrandId, Name, Description, Price
CREATE_PRODUCT_SUCCESS=True

GET /api/products?brandId={brandId}&page=1&pageSize=10
LIST_PRODUCT_SUCCESS=True
LIST_TOTAL=1

GET /api/products/{productId}
GET_PRODUCT_SUCCESS=True

PUT /api/products/{productId}
Content-Type: multipart/form-data
Fields: Name, Price
UPDATE_PRODUCT_SUCCESS=True
UPDATED_NAME=Product A Updated

DELETE /api/products/{productId}
DELETE_PRODUCT_SUCCESS=True

POST /api/products/{productId}/restore
RESTORE_PRODUCT_SUCCESS=True
```

Checklist:

- [x] Build thanh cong.
- [x] Test pass.
- [x] Migration kiem tra: database already up to date, khong co migration moi.
- [x] API test thanh cong.
- [x] Khong pha module Auth/Profile/Brand/Health da hoan thanh.
- [ ] Commit rieng task nay.

## Current Backend Status - Updated 2026-06-11

Backend source hien tai tren nhanh `Thanhk3` da vuot moc ghi chu cu trong plan. Code thuc te da co den **Phase 8 - Payment, subscription, quota display** o muc MVP hoan thien hon. Thu tu tiep theo da duoc chot: Phase 9 Workspace -> Phase 10 Admin theo Workspace -> Phase 11 Facebook Ads -> Phase 12 Release.

| Phase | Trang thai hien tai | Ghi chu |
| --- | --- | --- |
| Phase 0 - Chuan bi repo backend moi | DONE | Repo/backend solution da co. |
| Phase 1 - API host toi thieu | DONE | API host, Swagger, Health API da co. |
| Phase 2 - Common, domain models, database context | DONE | Common response, entities, DbContext, migrations da co. |
| Phase 3 - Authentication MVP | DONE | Register/login/JWT/refresh/logout/email/google endpoints da co. |
| Phase 4 - Profile, Brand, Product MVP | DONE | CRUD MVP cho Profile/Brand/Product da co. |
| Phase 5 - AI va Content MVP | DONE | Content CRUD, Gemini text, conversation history da co. |
| Phase 6 - Social integration va Facebook Page publishing | DONE/BASIC | Facebook OAuth, social account, linked targets, publish content, post history da co. |
| Phase 7 - Scheduling, notification, basic dashboard | DONE/BASIC | Content schedules, scheduler service/dev endpoint, notifications, dashboard summary da co. |
| Phase 8 - Payment, subscription, quota display | DONE/MVP | Payment checkout goi PayOS Merchant API, callback/webhook sync payment/subscription, history/current subscription va quota display da co. |
| Phase 9 - Workspace Migration | DONE | Task 9.1-9.18 da hoan thanh; Workspace Dashboard va regression cuoi Phase 9 da pass. |
| Phase 10 - Admin backend theo Workspace | TODO | Chi bat dau sau Phase 9. |
| Phase 11 - Facebook Ads Campaign MVP | TODO | Chi bat dau sau Phase 9 va Phase 10. |
| Phase 12 - Test hardening va backend release | IN PROGRESS/PARTIAL | Automated tests hien co pass, nhung regression cuoi chi hoan thanh sau Phase 9-11. |

Ket qua kiem tra gan nhat ngay 2026-06-11:

```text
dotnet build --no-restore
Build succeeded. 0 warnings, 0 errors.

dotnet test --no-build --no-restore
Passed. 198/198 tests passed.
```

Test files hien tai:

```text
ActiveProfileMiddlewareTests.cs
AIControllerTests.cs
AIServiceTests.cs
ContentCalendarRepositoryTests.cs
ContentControllerPublishTests.cs
ContentControllerTests.cs
ContentSchedulesControllerTests.cs
ContentScheduleServiceTests.cs
ContentServicePublishTests.cs
ContentServiceTests.cs
ConversationControllerTests.cs
ConversationServiceTests.cs
DashboardControllerTests.cs
DashboardServiceTests.cs
DevSchedulerControllerTests.cs
FacebookProviderTests.cs
FoundationTests.cs
GeminiTextClientTests.cs
NotificationRepositoryTests.cs
NotificationsControllerTests.cs
NotificationServiceTests.cs
OAuthStateStoreTests.cs
PaymentControllerTests.cs
PaymentRepositoryTests.cs
PaymentServiceTests.cs
PhaseEQuotaIntegrationTests.cs
PostRepositoryTests.cs
PostsControllerTests.cs
PostServiceTests.cs
QuotaControllerTests.cs
QuotaServiceTests.cs
ScheduledPostingServiceTests.cs
SocialControllerTests.cs
SocialRepositoryTests.cs
SocialServiceTests.cs
SocialTokenProtectorTests.cs
```

Controller/API surface hien tai:

```text
AuthController
BrandController
ContentController
ContentSchedulesController
ConversationController
DashboardController
DevSchedulerController
GeminiController
HealthController
NotificationsController
PaymentController
PostsController
ProductController
ProfileController
QuotaController
SocialAccountsController
SocialAuthController
SocialIntegrationController
```

Luu y thu cong:

- Database PostgreSQL va JWT config la bat buoc de chay/test backend local voi database that.
- `.env` local bat buoc neu muon test database/Gemini/SMTP/Google/Facebook that; file nay khong commit len Git.
- `GEMINI_API_KEY` chi bat buoc khi test AI voi Gemini that. Automated tests co the dung fake/mock.
- Facebook publish that can Meta App permissions va Page quan ly hop le.
- PayOS payment that can PayOS config, `PAYOS_BASE_URL`, return/cancel URL va webhook/callback URL hop le.
- Cac API theo active profile nhu `/api/content`, `/api/ai`, `/api/conversations`, `/api/social-*`, `/api/posts`, `/api/content-schedules`, `/api/notifications`, `/api/dashboard`, `/api/quota` can gui `Authorization: Bearer {accessToken}` va header `X-Profile-Id: {profileId}` neu middleware yeu cau.

Next recommended tasks:

1. Cap nhat chi tiet Progress Detail cho Phase 6 va Phase 7 theo source code hien tai.
2. Hoan thanh Phase 9 Workspace Migration.
3. Cap nhat va hoan thanh Phase 10 Admin theo Workspace.
4. Trien khai Phase 11 Facebook Ads Campaign MVP.
5. Chay Phase 12 regression/release cuoi.
3. Hardening Phase 6-8: Facebook error handling, scheduler retry/failed-state, PayOS idempotency/retry, quota edge cases.
4. Dong bo `SETUP_GUIDE.md` voi config thuc te cua Phase 6-8.

### Progress Detail - Phase 8 Payment, Subscription, Quota

Ngay cap nhat: 2026-06-04

Muc tieu:

- Hoan thien Phase 8 o muc MVP backend co the test duoc.
- Checkout khong con la placeholder tra `ReturnUrl`; backend tao pending subscription/payment va goi PayOS Merchant API de lay checkout URL.
- Callback/webhook PayOS dong bo payment status va active subscription cho profile.
- Quota display va quota enforcement basic tiep tuc dung subscription hien tai.

File da tao/sua:

- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/AISAM.API/.env.example`
- `AISAM-BE/AISAM.Common/Models/PayOSSettings.cs`
- `AISAM-BE/AISAM.Repositories/Repository/PaymentRepository.cs`
- `AISAM-BE/AISAM.Services/Service/PayOSPaymentService.cs`
- `AISAM-BE/tests/AISAM.IntegrationTests/PaymentServiceTests.cs`

Behavior:

```text
POST /api/payment/checkout
- Required: Authorization Bearer token + X-Profile-Id
- Required PayOS config: PAYOS_CLIENT_ID, PAYOS_API_KEY, PAYOS_CHECKSUM_KEY, PAYOS_BASE_URL, PAYOS_RETURN_URL, PAYOS_CANCEL_URL
- Plus/Premium tao subscription inactive + payment pending
- Goi PayOS: POST /v2/payment-requests
- Tra ve checkoutUrl, paymentLinkId, orderCode

POST /api/payment/callback
- AllowAnonymous
- Nhan query params tu PayOS
- Bat buoc co signature HMAC SHA256 hop le; thieu signature tra PAYOS_SIGNATURE_REQUIRED
- Status paid/success/00 => payment Success, subscription active
- Status cancelled/failed/expired => payment Failed

POST /api/payment/webhook
- AllowAnonymous
- Nhan JSON payload tu PayOS
- Bat buoc co signature HMAC SHA256 hop le theo data primitives
- Dong bo payment/subscription nhu callback

GET /api/payment/history
GET /api/payment/subscription/current
GET /api/quota/profile/{profileId}
```

Cai tien so voi source/trang thai cu:

- Truoc: `PayOSPaymentService.CreateCheckoutAsync` chi tra `_settings.ReturnUrl`, khong tao payment link that.
- Sau: tao pending subscription/payment, ky request bang checksum key, goi PayOS Merchant API, luu checkout URL/paymentLinkId/orderCode.
- Truoc: callback/webhook chi return accepted.
- Sau: callback/webhook sync payment status va active subscription.

Luu y:

- Plan/pricing/quota van hardcoded theo enum `Free/Plus/Premium/PlusTrial`, dung cho MVP.
- Chua lam dynamic plan CRUD admin, refund/cancel/expiry job, proration, webhook retry/idempotency nang cao.

Kiem tra:

```text
dotnet build --no-restore
Build succeeded. 0 warnings, 0 errors.

dotnet test --no-build --filter "Payment|Quota"
Passed. 23/23 tests passed.

dotnet test --no-build
Passed. 121/121 tests passed.
```

Checklist Phase 8:

- [x] Payment checkout tao checkout request that qua PayOS Merchant API.
- [x] Payment pending duoc luu truoc khi redirect PayOS.
- [x] Subscription inactive duoc tao khi checkout.
- [x] Callback/webhook co logic dong bo payment status.
- [x] Payment success active subscription va gan `Profile.SubscriptionId`.
- [x] Payment history doc theo profile.
- [x] Current subscription API doc active subscription.
- [x] Quota display API doc active subscription va usage.
- [x] Quota enforcement basic cho AI prompt va publish post.
- [x] Build thanh cong.
- [x] Payment/Quota tests pass.
- [x] Full tests pass.
- [x] PayOS webhook verification payload duoc acknowledge HTTP 200 khi orderCode mau khong ton tai trong database.
- [x] Quota queries chuan hoa PostgreSQL `date`/DateTimeKind.Unspecified sang UTC truoc khi loc cac cot `timestamp with time zone`.

### Progress Detail - Active Profile Context Middleware

Ngay hoan thanh: 2026-05-30

Muc tieu:

- Xac thuc active profile truoc khi cho phep user truy cap Content, AI va Conversation.
- Khong tin `profileId` tuy y tu request body.

File da tao/sua:

- `AISAM-BE/AISAM.API/Middleware/ActiveProfileMiddleware.cs`
- `AISAM-BE/AISAM.API/Utils/ProfileContextHelper.cs`
- `AISAM-BE/AISAM.API/Utils/UserClaimsHelper.cs`
- `AISAM-BE/AISAM.API/Program.cs`
- `AISAM-BE/tests/AISAM.IntegrationTests/ActiveProfileMiddlewareTests.cs`

Behavior:

```text
Protected prefixes:
/api/content
/api/ai
/api/conversations

Required headers:
Authorization: Bearer {accessToken}
X-Profile-Id: {profileId}
```

Middleware tra ve:

- `401` neu token thieu/khong hop le.
- `401` neu `X-Profile-Id` thieu/khong hop le.
- `404` neu profile khong ton tai.
- `403` neu profile khong thuoc JWT user.

Commit:

```text
9adf22a Them middleware kiem tra X-Profile-Id thuoc JWT user
```

### Progress Detail - Task 5.1

Ngay hoan thanh: 2026-05-30

Muc tieu:

- Hoan thanh Content CRUD MVP theo active profile da xac thuc.
- Chua publish social trong task nay.

File da tao/sua:

- `AISAM-BE/AISAM.API/Controllers/ContentController.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/CreateContentRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Request/UpdateContentRequest.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ContentResponseDto.cs`
- `AISAM-BE/AISAM.Repositories/IRepositories/IContentRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ContentRepository.cs`
- `AISAM-BE/AISAM.Services/IServices/IContentService.cs`
- `AISAM-BE/AISAM.Services/Service/ContentService.cs`
- `AISAM-BE/AISAM.API/Program.cs`

API da co:

```text
POST   /api/content
GET    /api/content
GET    /api/content/{contentId}
PUT    /api/content/{contentId}
POST   /api/content/{contentId}/clone
DELETE /api/content/{contentId}
POST   /api/content/{contentId}/restore
```

Dieu chinh MVP:

- Tat ca Content API dung active profile tu middleware.
- Chua publish Facebook/TikTok.
- Chua bat upload media storage.

### Progress Detail - Task 5.2

Ngay hoan thanh: 2026-05-30

Muc tieu:

- Them Gemini text generation MVP: generate draft, improve, approve generation, xem history va chat.

File da tao/sua:

- `AISAM-BE/AISAM.API/Controllers/GeminiController.cs`
- `AISAM-BE/AISAM.Common/Models/GeminiModels.cs`
- `AISAM-BE/AISAM.Repositories/IRepositories/IAiGenerationRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/AiGenerationRepository.cs`
- `AISAM-BE/AISAM.Services/IServices/IAIService.cs`
- `AISAM-BE/AISAM.Services/IServices/IGeminiTextClient.cs`
- `AISAM-BE/AISAM.Services/Service/AIService.cs`
- `AISAM-BE/AISAM.Services/Service/GeminiTextClient.cs`
- `AISAM-BE/AISAM.API/.env.example`
- `AISAM-BE/AISAM.API/Program.cs`

API da co:

```text
POST /api/ai/generate-draft
POST /api/ai/improve/{contentId}
POST /api/ai/approve/{aiGenerationId}
GET  /api/ai/generations/{contentId}
POST /api/ai/chat
```

Config thu cong:

```env
GEMINI_API_KEY=your-real-api-key
GEMINI_MODEL=gemini-2.5-flash
GEMINI_MAX_TOKENS=2048
GEMINI_TEMPERATURE=0.7
```

Dieu chinh MVP:

- Chi AI text. Chua lam AI image/video.
- Dung Gemini official client qua HTTP.
- Automated tests dung fake client, khong can key that.

### Progress Detail - Task 5.3

Ngay hoan thanh: 2026-05-30

Muc tieu:

- Luu va truy van conversation history cho AI chat theo active profile.

File da tao/sua:

- `AISAM-BE/AISAM.API/Controllers/ConversationController.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ConversationDetailDto.cs`
- `AISAM-BE/AISAM.Common/Dtos/Response/ConversationResponseDto.cs`
- `AISAM-BE/AISAM.Repositories/IRepositories/IConversationRepository.cs`
- `AISAM-BE/AISAM.Repositories/Repository/ConversationRepository.cs`
- `AISAM-BE/AISAM.Services/IServices/IConversationService.cs`
- `AISAM-BE/AISAM.Services/Service/ConversationService.cs`
- `AISAM-BE/AISAM.API/Program.cs`

API da co:

```text
GET    /api/conversations
GET    /api/conversations/{id}
DELETE /api/conversations/{id}
```

Kiem tra Phase 5:

```text
dotnet build --no-restore
Build succeeded. 0 warnings, 0 errors.

dotnet test --no-build
Passed. 36/36 tests passed.
```

Commit gom Phase 5:

```text
6055fdd Hoan thien Content CRUD, Gemini text generation va Conversation history theo active profile context da xac thuc.
```

Checklist Phase 5:

- [x] Content CRUD/status MVP.
- [x] Content clone, soft delete, restore.
- [x] Gemini generate draft.
- [x] Gemini improve content.
- [x] Approve AI generation.
- [x] AI generation history.
- [x] AI chat va conversation history.
- [x] Active profile ownership middleware.
- [x] Build thanh cong.
- [x] 36/36 automated tests pass.
- [ ] Test Gemini API that sau khi developer them `GEMINI_API_KEY`.

### Progress Detail - Setup Guide

Ngày hoàn thành: 2026-05-28

File đã tạo:

- `SETUP_GUIDE.md`

Nội dung chính:

- Ghi rõ cấu hình hiện tại cần ngay trong code mới: .NET SDK/NuGet restore, chưa cần secret để chạy API host tối thiểu.
- Ghi rõ các config future/optional sẽ cần khi migrate module:
  - PostgreSQL.
  - JWT.
  - CORS/Frontend base URL.
  - SMTP email.
  - Google OAuth.
  - Facebook OAuth/Graph API.
  - Gemini AI.
  - PayOS.
  - Supabase Storage.
- Có ví dụ `.env`.
- Có ví dụ `appsettings.Development.json`.
- Có checklist setup cuối file.
- Có phần không commit secrets lên Git.

Kết quả kiểm tra:

```text
Không chạy build/test vì task này chỉ thêm tài liệu Markdown.
```
