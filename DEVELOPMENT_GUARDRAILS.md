# AISAM Development Guardrails

Tài liệu này là bộ nguyên tắc bắt buộc khi phát triển repo mới của AISAM.

Source code cũ tại:

```text
D:\AISAM\PRN232-AISAM
```

được xem là **baseline**. Mọi thay đổi trong repo mới phải ưu tiên tính ổn định, khả năng kiểm chứng và khả năng rollback.

## 1. Nguyên tắc bắt buộc

### 1.1. Source cũ là baseline

- Source code cũ là điểm tham chiếu chính.
- Nếu module cũ đã chạy ổn, ưu tiên tái sử dụng.
- Không tự tiện viết lại module đã có nếu chưa có lý do rõ ràng.
- Không tự tiện refactor/cải tiến code cũ nếu không cần thiết.
- Mỗi thay đổi phải đủ nhỏ để build/test được ngay sau đó.

### 1.2. Ưu tiên tái sử dụng thay vì viết lại

Khi làm repo mới, thứ tự ưu tiên là:

1. Copy/tái sử dụng module cũ nếu module đã ổn.
2. Chỉnh cấu hình/path/namespace tối thiểu để module chạy được ở repo mới.
3. Chỉ cải tiến khi module cũ có vấn đề rõ ràng hoặc không đáp ứng yêu cầu mới.
4. Nếu cần viết mới, phải giải thích vì sao không thể tái sử dụng source cũ.

### 1.3. Không nhảy task khi task hiện tại chưa kiểm chứng

Không được chuyển sang task mới nếu task hiện tại chưa:

- Build được.
- Test được ở mức tối thiểu.
- Ghi lại kết quả test.
- Ghi lại file đã copy/sửa/tạo.
- Xác định rõ nếu có migration/database thay đổi.

## 2. Quy tắc khi copy code cũ sang repo mới

Mỗi lần copy code cũ phải ghi rõ:

| Nội dung | Bắt buộc ghi |
| --- | --- |
| File/thư mục cũ | Đường dẫn đầy đủ trong `D:\AISAM\PRN232-AISAM` |
| File/thư mục mới | Đường dẫn đầy đủ trong repo mới |
| Lý do giữ nguyên | Vì sao module đủ ổn để tái sử dụng |
| Phạm vi thay đổi | Có sửa namespace/config/import/path không |
| Cách test | Build/test/API/UI test nào chứng minh module chạy ổn |
| Kết quả test | Pass/fail, lỗi còn lại nếu có |

Template ghi chép:

```md
## Copy/Tái sử dụng module

- Module:
- File/thư mục cũ:
- File/thư mục mới:
- Lý do giữ nguyên:
- Thay đổi tối thiểu nếu có:
- Cách test:
- Kết quả test:
- Ghi chú:
```

## 3. Quy tắc khi cải tiến code cũ

Chỉ cải tiến code cũ khi có một trong các lý do sau:

- Code cũ không build được trong repo mới.
- Code cũ sai nghiệp vụ so với yêu cầu hiện tại.
- Code cũ có bug rõ ràng.
- Code cũ phụ thuộc cấu hình/path không còn phù hợp.
- Code cũ gây lỗi bảo mật hoặc lỗi runtime.
- Code cũ không thể test/maintain ở trạng thái hiện tại.

Mỗi lần cải tiến phải ghi rõ:

| Nội dung | Bắt buộc ghi |
| --- | --- |
| Cải tiến cái gì | Mô tả ngắn gọn thay đổi |
| Vì sao cần cải tiến | Lý do kỹ thuật/nghiệp vụ |
| File/thư mục cải tiến | Đường dẫn cụ thể |
| Vấn đề trước cải tiến | Code cũ đang lỗi/thiếu gì |
| Kết quả mong muốn | Sau cải tiến phải đạt gì |
| Ảnh hưởng module khác | Có ảnh hưởng API/UI/database/auth/payment không |
| Cách test | Build/test/API/UI test |
| Cách rollback | Cách quay lại nếu lỗi |

Template ghi chép:

```md
## Cải tiến module

- Module:
- Cải tiến:
- Lý do cần cải tiến:
- File/thư mục sửa:
- Vấn đề trước cải tiến:
- Kết quả mong muốn:
- Module có thể bị ảnh hưởng:
- Cách test:
- Expected result:
- Cách rollback:
- Kết quả thực tế:
```

## 4. Quy tắc làm từng bước nhỏ

Mỗi bước chỉ nên làm một nhóm việc rõ ràng:

- Copy một project.
- Sửa cấu hình build.
- Sửa một module.
- Thêm một endpoint.
- Thêm một màn hình.
- Thêm một migration.
- Thêm một test.

Sau mỗi bước phải kiểm tra được.

Không gom nhiều thay đổi lớn như:

- Copy backend + sửa auth + sửa payment + sửa UI cùng lúc.
- Refactor nhiều service cùng lúc.
- Đổi database schema khi chưa kiểm tra API hiện tại.
- Sửa frontend khi backend chưa build được.

## 5. Format bắt buộc cho mỗi task code

Mỗi task code phải được ghi theo format sau.

```md
# Task: <Tên task>

## 1. Mục tiêu

- 

## 2. Module liên quan

- Backend:
- Frontend user:
- Frontend admin:
- Database:
- External services:

## 3. Source cũ cần kiểm tra

| Loại | Đường dẫn source cũ | Ghi chú |
| --- | --- | --- |
| Controller/API |  |  |
| Service |  |  |
| Repository |  |  |
| Entity/DTO |  |  |
| Frontend page/component |  |  |
| Config/env |  |  |

## 4. File sẽ tạo mới

| File | Lý do tạo |
| --- | --- |
|  |  |

## 5. File sẽ copy/tái sử dụng

| File/thư mục cũ | File/thư mục mới | Lý do giữ nguyên |
| --- | --- | --- |
|  |  |  |

## 6. File sẽ sửa

| File | Nội dung sửa | Lý do sửa |
| --- | --- | --- |
|  |  |  |

## 7. Code cần viết

- 

## 8. Câu lệnh migration/build/test

### Backend

```text
dotnet build
dotnet test
```

### Migration nếu có

```text
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Frontend nếu có

```text
npm install
npm run build
npm run lint
```

## 9. API hoặc UI test

### Swagger/Postman

| API | Method | Input | Expected result |
| --- | --- | --- | --- |
|  |  |  |  |

### UI

| Màn hình | Hành động test | Expected result |
| --- | --- | --- |
|  |  |  |

## 10. Expected result

- 

## 11. Checklist hoàn thành

- [ ] Đã kiểm tra source cũ liên quan.
- [ ] Đã ghi rõ file copy/tái sử dụng.
- [ ] Đã ghi rõ file tạo mới.
- [ ] Đã ghi rõ file sửa.
- [ ] Đã build backend thành công.
- [ ] Đã chạy test backend.
- [ ] Đã kiểm tra migration nếu có.
- [ ] Đã test API bằng Swagger/Postman nếu có API.
- [ ] Đã build/lint frontend nếu có frontend.
- [ ] Đã test UI nếu có UI.
- [ ] Đã ghi lại kết quả test.
- [ ] Đã ghi cách rollback nếu có cải tiến rủi ro.
```

## 6. Kiểm tra bắt buộc sau mỗi phase

Sau mỗi phase phải chạy và ghi lại kết quả.

### 6.1. Backend

```text
dotnet build
dotnet test
```

Bắt buộc ghi:

- Build pass/fail.
- Test pass/fail.
- Số test chạy được.
- Lỗi còn lại nếu có.

### 6.2. API test

Phải test API bằng Swagger hoặc Postman.

Bắt buộc ghi:

| API | Method | Kết quả | Ghi chú |
| --- | --- | --- | --- |
|  |  |  |  |

### 6.3. Database migration

Nếu có thay đổi database:

- Kiểm tra migration được tạo đúng.
- Kiểm tra database update thành công.
- Kiểm tra API liên quan vẫn chạy.
- Ghi rõ rollback migration nếu cần.

Template:

```md
## Database migration check

- Migration name:
- Lý do migration:
- Bảng/cột bị ảnh hưởng:
- Lệnh đã chạy:
- Kết quả:
- Cách rollback:
```

### 6.4. Frontend

Nếu phase có frontend:

```text
npm run build
npm run lint
```

Bắt buộc ghi:

- Build pass/fail.
- Lint pass/fail.
- Màn hình đã test.
- Hành động đã test.
- Kết quả thực tế.

## 7. Quy tắc rollback

Mỗi cải tiến có rủi ro phải có rollback.

Rollback có thể là:

- Xóa file mới tạo.
- Khôi phục file từ source cũ.
- Revert một migration.
- Tắt feature qua config.
- Quay lại endpoint/service cũ.

Template:

```md
## Rollback plan

- Khi nào cần rollback:
- File cần khôi phục:
- Migration cần revert:
- Config cần đổi lại:
- Lệnh rollback:
- Cách test sau rollback:
```

## 8. Thứ tự ưu tiên khi phát triển AISAM

Ưu tiên hiện tại dựa trên source cũ:

| Ưu tiên | Nhóm chức năng | Lý do |
| --- | --- | --- |
| P0 | Auth, profile, brand, product | Source cũ ổn, là nền tảng |
| P0 | AI content generation/refinement | Điểm chính của đề tài |
| P0 | Content management và Facebook publishing | Flow demo quan trọng nhất |
| P1 | Subscription, payment, quota display | Bám sát yêu cầu monetization |
| P1 | Scheduling/content calendar | Source cũ có sẵn, hiệu quả demo cao |
| P1 | Admin user/payment/subscription | Cần cho hệ thống hoàn chỉnh |
| P2 | Analytics cơ bản | Làm nội bộ trước, API thật sau |
| P2 | Team approval nâng cao | Có source cũ nhưng không nên làm đầu tiên |
| P3 | Facebook Ads nâng cao | Rủi ro API/quyền cao |
| P3 | Instagram/TikTok thật | Để phase sau |
| P3 | Mobile App Flutter | Không có baseline rõ trong source cũ |
| P3 | Video AI generation | Phụ thuộc AI service và chi phí |

## 9. Definition of Done cho mỗi task

Một task chỉ được xem là hoàn thành khi:

- Source cũ liên quan đã được kiểm tra.
- Lý do copy/tái sử dụng/cải tiến đã được ghi.
- File tạo mới/copy/sửa đã được liệt kê.
- Backend build được nếu task có backend.
- Backend test được nếu task có backend.
- Migration được kiểm tra nếu task có database.
- API được test bằng Swagger/Postman nếu task có API.
- Frontend build/lint được nếu task có frontend.
- UI được test nếu task có UI.
- Expected result khớp với kết quả thực tế.
- Có rollback plan nếu thay đổi có rủi ro.

## 10. Ghi nhớ ngắn gọn

```text
Source cũ là baseline.
Không viết lại nếu có thể tái sử dụng.
Không refactor nếu không cần.
Làm từng bước nhỏ.
Mỗi bước phải build/test được.
Copy thì ghi rõ nguồn, đích, lý do, cách test.
Cải tiến thì ghi rõ vấn đề, lý do, ảnh hưởng, rollback.
Không chuyển task mới khi task hiện tại chưa test xong.
Sau mỗi phase phải dotnet build, dotnet test, test API, kiểm tra migration và ghi kết quả.
```
