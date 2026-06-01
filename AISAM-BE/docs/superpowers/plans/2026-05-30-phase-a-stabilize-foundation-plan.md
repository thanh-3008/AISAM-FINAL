# Plan Phase A: Ổn định nền tảng hiện tại

Lần review gần nhất: 2026-05-30

## Task A1 - Sửa authorization của Profile

### Mục tiêu

Đảm bảo user chỉ có thể truy cập và thay đổi profile của chính mình.

### File cần sửa

- `AISAM.API/Controllers/ProfileController.cs`
- `AISAM.Services/IServices/IProfileService.cs`
- `AISAM.Services/Service/ProfileService.cs`

### Triển khai

- Thêm logic lấy authenticated user trong `ProfileController`.
- Reject route `userId` không khớp với HTTP 403.
- Truyền acting user ID vào các service call detail/update/delete/restore của profile.
- Thêm ownership check ở service layer để authorization được enforce bên dưới controller.

### Test

- Route `userId` mismatch trả 403.
- Profile đúng owner có thể được read/update/delete/restore.
- Profile không thuộc owner bị reject.

## Task A2 - Giữ behavior upload disabled

### Mục tiêu

Giữ avatar/product upload ở trạng thái tắt trong Phase A và thể hiện rõ điều đó.

### File cần sửa

- `AISAM.Services/Service/ProfileService.cs`
- `AISAM.API/.env.example`

### Triển khai

- Giữ logic reject `AvatarFile`.
- Giữ logic reject product `ImageFiles`.
- Đảm bảo message nói rõ upload chưa được bật trong backend MVP hiện tại.
- Đánh dấu Supabase/storage config là deferred trong `.env.example` nếu có.

### Test

- Profile create/update với `AvatarFile` trả lỗi.

## Task A3 - Sửa text encoding của Email

### Mục tiêu

Làm `EmailService` dễ đọc và an toàn cho Phase A.

### File cần sửa

- `AISAM.Services/Service/EmailService.cs`

### Triển khai

- Thay các subject/plain text/template string bị mojibake bằng text tiếng Việt dễ đọc hoặc ASCII-safe không bị lỗi ký tự.
- Giữ behavior graceful khi chưa cấu hình SMTP.
- Không đổi semantics gửi SMTP.

### Test

- `SendEmailAsync` trả `false` khi thiếu SMTP settings.

## Task A4 - Thay placeholder tests

### Mục tiêu

Tạo test có ý nghĩa để validate behavior Phase A mà không cần migrate module ngoài phạm vi.

### File cần sửa/tạo

- `tests/AISAM.IntegrationTests/UnitTest1.cs`
- File test bổ sung nếu cần.

### Triển khai

- Xóa test rỗng.
- Thêm test tập trung cho profile authorization/service behavior và email no-SMTP behavior.
- Ưu tiên test doubles tối thiểu khi full DB integration không cần thiết.

### Test

- Chạy `dotnet test AISAM.sln`.

## Task A5 - Verify build, test, và smoke checklist

### Mục tiêu

Ghi nhận foundation đã sẵn sàng cho Phase B migration hay chưa.

### Lệnh

```text
dotnet build AISAM.sln
dotnet test AISAM.sln
```

### Smoke test API

- `GET /api/Health`
- Auth register/login/refresh/me
- Profile list/create/detail/update/delete/restore
- Brand list/create
- Product list/create

### Done Criteria

- Kết quả build/test được ghi nhận trong final handoff.
- Nếu có blocker về environment, phải nêu cụ thể.
- Không thêm migration.
- Không copy module Phase B.
