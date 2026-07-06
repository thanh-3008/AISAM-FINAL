# Spec Phase A: Ổn định nền tảng hiện tại

Lần review gần nhất: 2026-05-30

## Mục tiêu

Ổn định backend MVP đang hoạt động trước khi migrate các module mới từ `docs/code-references/PRN232_Backend`.

Phase A chỉ bao phủ API surface hiện tại:

- Health
- Auth
- Profile
- Brand
- Product

Phase A không migrate các module Content, AI, Social, Payment, Admin, Team, Ads, Scheduling, Notification, hoặc Storage.

## Quy tắc nền

- `docs/code-references/PRN232_Backend` vẫn là source tham chiếu chính.
- `docs/main/development-guardrails.md` ràng buộc kỷ luật triển khai.
- `docs/archive/plans/backend-code-plan.md` vẫn là khung phase chính.
- Mỗi thay đổi phải đủ nhỏ để có thể build/test ngay.
- Phase A không dự kiến thêm database migration.

## Phạm vi

### Trong phạm vi

- Sửa authorization của Profile để user chỉ có thể truy cập và thay đổi profile của chính mình.
- Giữ trạng thái tắt upload file avatar/product trong backend MVP.
- Làm cho subject và template email dễ đọc, encoding nhất quán.
- Làm rõ yêu cầu environment cho Phase A.
- Thay test placeholder bằng test tập trung cho các module nền.
- Xác minh bằng build/test và smoke test Swagger.

### Ngoài phạm vi

- Supabase upload.
- Content/AI/Conversation.
- Facebook/Social publishing.
- Scheduling và background services.
- Payment/subscription/quota.
- Admin tools.
- Team/approval governance.
- Facebook Ads.

## Hành vi authorization của Profile

Vấn đề hiện tại: `ProfileController` nhận `userId` và profile ID nhưng chưa luôn đảm bảo chúng thuộc về user đang được xác thực.

Hành vi bắt buộc trong Phase A:

- Acting user luôn được đọc từ JWT `ClaimTypes.NameIdentifier`.
- `GET /api/profiles/user/{userId}` trả 403 khi route `userId` không khớp JWT user ID.
- `POST /api/profiles/user/{userId}` trả 403 khi route `userId` không khớp JWT user ID.
- `GET /api/profiles/{id}` trả 404 khi profile không tồn tại hoặc không thuộc JWT user.
- `PUT /api/profiles/{id}` trả lỗi theo pattern `GenericResponse` hiện có, dạng 400/404, khi profile không tồn tại hoặc không thuộc JWT user.
- `DELETE /api/profiles/{id}` và `PATCH /api/profiles/{id}/restore` chỉ tác động lên profile thuộc JWT user.
- Phase A chưa triển khai admin bypass.

## Hành vi upload

Phase A giữ upload ở trạng thái tắt:

- `CreateProfileRequest.AvatarFile` vẫn được DTO multipart chấp nhận, nhưng `ProfileService` phải reject.
- `UpdateProfileRequest.AvatarFile` vẫn được DTO multipart chấp nhận, nhưng `ProfileService` phải reject.
- Upload file ảnh product vẫn tắt.
- Client nên dùng `AvatarUrl`; product images giữ dạng JSON list rỗng cho đến khi storage được triển khai ở phase sau.

## Hành vi email

`EmailService` cần dùng text UTF-8 dễ đọc cho:

- Email verification.
- Password reset.
- Welcome email.
- Team invitation.
- Notification email.

Nếu thiếu SMTP settings, giữ behavior graceful hiện tại:

- Log warning.
- Return `false`.
- Không throw từ `SendEmailAsync`.

## Yêu cầu test

Thêm test có ý nghĩa cho behavior Phase A. Các mục tiêu test tối thiểu:

- Profile authorization reject khi route user ID không khớp.
- Profile service chặn update/delete/restore với profile không thuộc acting user.
- Behavior upload-disabled trả lỗi rõ ràng cho `AvatarFile`.
- Email service return `false` khi SMTP chưa được cấu hình.

Integration tests có thể dùng test doubles hoặc direct service tests nếu full database setup nặng hơn behavior cần verify.

## Lệnh verification

Backend:

```text
dotnet build AISAM.sln
dotnet test AISAM.sln
```

Smoke test Swagger/API:

```text
GET /api/Health
POST /api/Auth/register
POST /api/Auth/login
POST /api/Auth/refresh
GET /api/Auth/me
GET /api/profiles/user/{userId}
POST /api/profiles/user/{userId}
GET /api/brands
POST /api/brands
GET /api/products
POST /api/products
```

## Definition of Done

- Profile ownership gap đã được fix cho các endpoint profile đang active.
- Upload vẫn được tắt có chủ đích và trả response rõ ràng.
- Text trong email template dễ đọc.
- Placeholder test đã được thay thế.
- Kết quả `dotnet build` được ghi nhận.
- Kết quả `dotnet test` được ghi nhận.
- Không thêm database migration trừ khi phát hiện schema bug thực sự.
- Không migrate module ngoài phạm vi.
