# AISAM Backend Code Plan

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

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
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

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công: không áp dụng.
- [ ] Không phá module đã hoàn thành.
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

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có: không áp dụng.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
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

- [ ] Build thành công.
- [ ] Test pass.
- [ ] Migration chạy được nếu có.
- [ ] API test thành công.
- [ ] Không phá module đã hoàn thành.
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

## Phase 9 - Admin backend MVP

Mục tiêu phase:

- Admin có API quản lý user/payment/subscription.
- Không làm frontend admin ở tài liệu này.

### Task 9.1 - Migrate UserController admin/user list APIs

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

### Task 9.2 - Migrate AdminToolsController ở mức an toàn

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

## Phase 10 - Test hardening và backend release MVP

Mục tiêu phase:

- Backend MVP đủ ổn để frontend bắt đầu dùng.
- Có test tối thiểu.
- Có tài liệu API/env.

### Task 10.1 - Thêm integration tests cho API host và auth

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

### Task 10.2 - Viết backend environment và API testing guide

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
- Facebook Ads end-to-end.
- Video AI generation.
- Mobile app APIs riêng.
- AI cost tracking chi tiết.
- Team approval workflow phức tạp.
- Analytics real-time từ nhiều social platforms.

Các phần này chỉ làm sau khi backend MVP ổn định và đã có frontend sử dụng các API core.
