# US-01 - Đăng ký tài khoản

## 1. User Story

**Là** người dùng mới,  
**tôi muốn** đăng ký tài khoản bằng email và mật khẩu,  
**để** bắt đầu sử dụng hệ thống AISAM.

## 2. Mục tiêu

Cho phép người dùng tạo tài khoản AISAM bằng thông tin hợp lệ. Tài khoản mới được lưu an toàn và sẵn sàng chuyển sang bước xác minh email.

## 3. Phạm vi

### Trong phạm vi

- Hiển thị form đăng ký trên User Web App.
- Nhận email, mật khẩu và xác nhận mật khẩu.
- Kiểm tra dữ liệu đầu vào tại frontend và backend.
- Kiểm tra email chưa được sử dụng.
- Băm mật khẩu trước khi lưu.
- Tạo tài khoản người dùng mới.
- Ghi nhận trạng thái chưa xác minh email.
- Khởi tạo và gửi yêu cầu xác minh email nếu dịch vụ email đã được cấu hình.
- Trả về thông báo thành công hoặc lỗi phù hợp.

### Ngoài phạm vi

- Đăng nhập và phát hành JWT/refresh token sau khi đăng ký: thuộc `US-02`.
- Hoàn tất xác minh email: thuộc `US-03`.
- Đăng nhập bằng Google: thuộc `US-05`.
- Tạo business profile: thuộc `US-07`.
- Đăng ký subscription: thuộc `US-33` và `US-34`.

## 4. Actor

- **Chính:** Người dùng chưa có tài khoản AISAM.
- **Phụ:** User Web App, Backend API, PostgreSQL, dịch vụ gửi email.

## 5. Tiền điều kiện

- Người dùng chưa đăng nhập.
- Email đăng ký chưa tồn tại trong hệ thống.
- Backend API và PostgreSQL đang hoạt động.

## 6. Luồng nghiệp vụ chính

1. Người dùng mở trang đăng ký.
2. Hệ thống hiển thị form gồm email, mật khẩu và xác nhận mật khẩu.
3. Người dùng nhập thông tin và gửi form.
4. Frontend kiểm tra dữ liệu cơ bản trước khi gọi API.
5. Backend chuẩn hóa email và kiểm tra dữ liệu lại.
6. Backend kiểm tra email chưa tồn tại.
7. Backend băm mật khẩu bằng thuật toán phù hợp.
8. Backend tạo tài khoản với trạng thái chưa xác minh email.
9. Backend tạo yêu cầu xác minh email và gửi email nếu dịch vụ email đã được cấu hình.
10. Hệ thống trả thông báo đăng ký thành công và hướng dẫn người dùng kiểm tra email.

## 7. Luồng ngoại lệ

| Mã | Trường hợp | Kết quả mong đợi |
| --- | --- | --- |
| EX-01 | Email sai định dạng | Không tạo tài khoản, hiển thị lỗi validation. |
| EX-02 | Email đã tồn tại | Không tạo tài khoản, trả lỗi xung đột dữ liệu. |
| EX-03 | Mật khẩu không đạt chính sách | Không tạo tài khoản, hiển thị yêu cầu mật khẩu. |
| EX-04 | Xác nhận mật khẩu không khớp | Frontend chặn submit; backend vẫn kiểm tra nếu nhận trường này. |
| EX-05 | Không kết nối được database | Không tạo tài khoản một phần, trả lỗi hệ thống. |
| EX-06 | Gửi email xác minh thất bại | Tài khoản vẫn được tạo ở trạng thái chưa xác minh; ghi log và cho phép gửi lại email xác minh trong `US-03`. |

## 8. Quy tắc nghiệp vụ

| Mã | Quy tắc |
| --- | --- |
| BR-01 | Email là bắt buộc, được trim khoảng trắng và chuyển về chữ thường trước khi so sánh hoặc lưu. |
| BR-02 | Email phải đúng định dạng email hợp lệ. |
| BR-03 | Mỗi email chỉ được gắn với một tài khoản. Database phải có unique constraint trên email chuẩn hóa. |
| BR-04 | Mật khẩu là bắt buộc và không được lưu dưới dạng plain text. |
| BR-05 | Mật khẩu phải có tối thiểu 8 ký tự, gồm ít nhất một chữ hoa, một chữ thường, một chữ số và một ký tự đặc biệt. |
| BR-06 | Thông báo lỗi không được trả về password hash, token, stack trace hoặc dữ liệu nhạy cảm. |
| BR-07 | Tài khoản mới mặc định chưa xác minh email và chưa có quyền admin. |
| BR-08 | API phải xử lý an toàn trường hợp hai request đồng thời đăng ký cùng một email. |

## 9. API Contract Đề Xuất

Tên endpoint và DTO cần được đối chiếu với source hiện tại trước khi implement. Contract dưới đây là baseline đề xuất.

### Request

`POST /api/auth/register`

```json
{
  "email": "user@example.com",
  "password": "StrongPassword@123",
  "confirmPassword": "StrongPassword@123"
}
```

### Response thành công

`201 Created`

```json
{
  "message": "Registration successful. Please verify your email.",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "isEmailVerified": false
  }
}
```

### Response lỗi

| HTTP status | Trường hợp |
| --- | --- |
| `400 Bad Request` | Dữ liệu đầu vào không hợp lệ. |
| `409 Conflict` | Email đã được sử dụng. |
| `500 Internal Server Error` | Lỗi hệ thống không mong đợi. |

Ví dụ lỗi validation:

```json
{
  "message": "Validation failed.",
  "errors": {
    "password": [
      "Password must contain at least 8 characters, uppercase, lowercase, number and special character."
    ]
  }
}
```

## 10. Dữ liệu Cần Lưu

Các tên field cụ thể cần được đối chiếu với entity hiện tại.

| Field | Yêu cầu |
| --- | --- |
| `Id` | UUID của user. |
| `Email` | Email đã chuẩn hóa. |
| `PasswordHash` | Mật khẩu đã băm. |
| `IsEmailVerified` | Mặc định `false`. |
| `Role` | Vai trò user thông thường, không phải admin. |
| `CreatedAt` | Thời điểm tạo tài khoản theo UTC. |
| `UpdatedAt` | Thời điểm cập nhật gần nhất theo UTC. |

Nếu token xác minh email được lưu trong database, chỉ lưu hash của token cùng thời gian hết hạn.

## 11. Acceptance Criteria

### AC-01 - Đăng ký thành công

**Given** email chưa tồn tại và mật khẩu hợp lệ  
**When** người dùng gửi form đăng ký  
**Then** hệ thống tạo một tài khoản mới với email đã chuẩn hóa, mật khẩu đã băm và trạng thái chưa xác minh email  
**And** trả về `201 Created`.

### AC-02 - Không cho phép email trùng

**Given** email đã được gắn với một tài khoản  
**When** người dùng đăng ký lại bằng cùng email, kể cả khác chữ hoa chữ thường hoặc có khoảng trắng thừa  
**Then** hệ thống không tạo thêm tài khoản  
**And** trả về `409 Conflict`.

### AC-03 - Kiểm tra email hợp lệ

**Given** email sai định dạng  
**When** người dùng gửi form đăng ký  
**Then** hệ thống không tạo tài khoản  
**And** trả về `400 Bad Request` kèm lỗi validation.

### AC-04 - Kiểm tra chính sách mật khẩu

**Given** mật khẩu không đạt chính sách bảo mật  
**When** người dùng gửi form đăng ký  
**Then** hệ thống không tạo tài khoản  
**And** trả về `400 Bad Request` kèm mô tả yêu cầu mật khẩu.

### AC-05 - Kiểm tra xác nhận mật khẩu

**Given** mật khẩu và xác nhận mật khẩu không giống nhau  
**When** người dùng gửi form đăng ký  
**Then** hệ thống không tạo tài khoản  
**And** hiển thị lỗi phù hợp.

### AC-06 - Không lưu mật khẩu plain text

**Given** người dùng đăng ký thành công  
**When** kiểm tra dữ liệu đã lưu  
**Then** database chỉ chứa password hash  
**And** response, log và exception không chứa mật khẩu gốc.

### AC-07 - Xử lý đăng ký đồng thời

**Given** hai request đăng ký cùng một email được gửi gần như đồng thời  
**When** backend xử lý request  
**Then** chỉ một tài khoản được tạo  
**And** request còn lại nhận lỗi xung đột dữ liệu.

### AC-08 - Gửi email xác minh thất bại

**Given** dữ liệu đăng ký hợp lệ nhưng dịch vụ email gặp lỗi  
**When** backend tạo tài khoản  
**Then** tài khoản vẫn tồn tại ở trạng thái chưa xác minh  
**And** hệ thống ghi log lỗi không chứa dữ liệu nhạy cảm  
**And** người dùng có thể yêu cầu gửi lại email trong `US-03`.

## 12. Gợi ý Implementation Theo Tech Stack

### Backend - .NET 8 ASP.NET Core Web API

- Thêm hoặc xác nhận endpoint register trong authentication controller.
- Dùng request DTO và FluentValidation cho validation backend.
- Chuẩn hóa email trước khi truy vấn và lưu.
- Dùng password hasher phù hợp của ASP.NET Core Identity hoặc thư viện hashing hiện có trong dự án.
- Đặt unique index cho email chuẩn hóa trong PostgreSQL.
- Xử lý unique constraint violation để trả `409 Conflict`.
- Không ghi log password, password hash hoặc verification token.
- Duy trì kiến trúc Controller-Service-Repository theo baseline dự án.

### Frontend User - Next.js 15, React 19, TypeScript

- Tạo hoặc hoàn thiện trang sign-up.
- Validate email, password và confirm password trước khi submit.
- Hiển thị loading state trong lúc gửi request.
- Hiển thị lỗi theo từng field và lỗi tổng quát từ API.
- Sau khi thành công, chuyển người dùng đến màn hình hướng dẫn xác minh email.

## 13. Test Cases Tối Thiểu

| Mã | Loại test | Nội dung |
| --- | --- | --- |
| TC-01 | Unit | Email được trim và chuyển về lowercase. |
| TC-02 | Unit | Validator từ chối email sai định dạng. |
| TC-03 | Unit | Validator từ chối từng trường hợp mật khẩu không đạt chính sách. |
| TC-04 | Unit | Service băm mật khẩu trước khi lưu. |
| TC-05 | Integration | Đăng ký hợp lệ tạo đúng một user và trả `201`. |
| TC-06 | Integration | Đăng ký email đã tồn tại trả `409`. |
| TC-07 | Integration | Hai request đồng thời cùng email chỉ tạo một user. |
| TC-08 | Integration | Lỗi gửi email không rollback user đã tạo. |
| TC-09 | Frontend | Form chặn submit khi confirm password không khớp. |
| TC-10 | Frontend | Form hiển thị loading, lỗi field và thông báo thành công phù hợp. |

## 14. Definition of Done

- Backend API đăng ký hoạt động theo acceptance criteria.
- Frontend sign-up form gọi API và xử lý đầy đủ trạng thái thành công, loading và lỗi.
- Email có unique constraint trong PostgreSQL.
- Mật khẩu không xuất hiện dưới dạng plain text trong database, response hoặc log.
- Có unit test và integration test cho các luồng chính và luồng lỗi quan trọng.
- Swagger/OpenAPI mô tả endpoint register.
- Code tuân theo kiến trúc Controller-Service-Repository hiện tại.

## 15. Phụ Thuộc

- Dịch vụ gửi email và cơ chế token xác minh email sẽ được hoàn thiện trong `US-03`.
- Cấu hình môi trường phải cung cấp database connection và cấu hình email phù hợp.
- Các chính sách bảo mật chung tiếp tục được harden trong `US-60`.

## 16. Nguồn Tham Chiếu

- `docs/main/requirements.md`: mục `6.1 Authentication and Account Management`, mục `8.1 Security`.
- `README.md`: mục `2.1 Account, Authentication, Profile and Subscription`, mục `3.1 Authentication and Account Flow`, mục `8. Technology Stack`.
