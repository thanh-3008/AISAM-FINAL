# HỢP ĐỒNG PHÁT TRIỂN AI (AI DEVELOPMENT CONTRACT)

Tài liệu này đóng vai trò là **Quy định Kỹ thuật (Technical Standard) và Ranh giới (Boundaries)** bắt buộc đối với mọi AI Agent (Developer) khi tham gia lập trình tính năng (Feature) cho dự án Flutter `AISAM-MB`. 

Bất kỳ AI Agent nào khi được giao nhiệm vụ **BẮT BUỘC (MUST)** đọc, hiểu và tuân thủ 100% hợp đồng này trước khi bắt đầu sửa đổi mã nguồn.

---

## 1. Vai Trò Của AI (AI Role Definition)
- AI **BẮT BUỘC (MUST)** đóng vai trò là một **Developer** thực thi nhiệm vụ được giao.
- AI **KHÔNG ĐƯỢC (MUST NOT)** đóng vai trò Architect trong Giai đoạn Phát triển Feature.
- AI **KHÔNG ĐƯỢC (MUST NOT)** tự ý thay đổi thiết kế hệ thống tổng thể.
- AI **KHÔNG ĐƯỢC (MUST NOT)** tự ý bổ sung thêm tính năng ngoài Đặc tả Module (Module Specification).
- AI **KHÔNG ĐƯỢC (MUST NOT)** tự ý thay đổi Business Logic của Backend quy định.

## 2. Phạm Vi Làm Việc (Scope of Work)
- AI **CHỈ ĐƯỢC PHÉP (MUST ONLY)** làm việc trong phạm vi Module được chỉ định (Ví dụ: `Authentication`, `Content`, `Calendar`).
- AI **KHÔNG ĐƯỢC (MUST NOT)** can thiệp, sửa chữa, hay phân tích các Module không được giao.

## 3. Thư Mục Được Phép Sửa (Allowed Directories)
AI chỉ được phép tạo mới và chỉnh sửa tệp tin bên trong thư mục Feature của mình:
- `lib/features/<assigned_module>/**` (Ví dụ: `lib/features/auth/**`, `lib/features/content/**`)

## 4. Thư Mục Cấm Sửa (Forbidden Directories)
AI **TUYỆT ĐỐI KHÔNG ĐƯỢC (MUST NOT)** sửa các thư mục/tệp sau trừ khi có yêu cầu đặc biệt từ Architect/Tech Lead:
- `lib/core/**` (Core Layer, Error Handling, Network)
- `lib/shared/**` (Shared Widgets)
- `lib/app/router.dart`
- `lib/app/theme.dart`
- `lib/config/**`
- `lib/main.dart`
- `pubspec.yaml`
- `analysis_options.yaml`
- Thư mục nền tảng: `android/`, `ios/`, `linux/`, `macos/`, `windows/`, `web/`

## 5. Quy Định Dependency (Package Management)
- AI **KHÔNG ĐƯỢC (MUST NOT)** thêm package mới.
- AI **KHÔNG ĐƯỢC (MUST NOT)** đổi version package.
- AI **KHÔNG ĐƯỢC (MUST NOT)** xóa package hiện tại.
- Nếu Feature bắt buộc cần một package mới, AI **PHẢI (MUST)** dừng lại và ghi yêu cầu vào mục `Recommendations` để chờ phê duyệt.

## 6. Quy Định Kiến Trúc (Architecture Enforcement)
Bắt buộc duy trì nguyên trạng:
- Clean Architecture (Data Layer / Presentation Layer phân tách rõ ràng).
- Feature-First Folder Structure.
- Repository Pattern (Kết nối API Client).
- State Management (Riverpod).
- Dùng chung Core Layer.
- AI **KHÔNG ĐƯỢC (MUST NOT)** đổi sang mô hình MVC, MVVM (với Provider/Bloc) khác với Riverpod.

## 7. Quy Định API & Network
- AI **KHÔNG ĐƯỢC (MUST NOT)** đổi Endpoint.
- AI **KHÔNG ĐƯỢC (MUST NOT)** đổi Request Payload hoặc Response format.
- AI **KHÔNG ĐƯỢC (MUST NOT)** Hardcode URL (Bắt buộc dùng `EnvConfig.apiBaseUrl`).
- AI **KHÔNG ĐƯỢC (MUST NOT)** Hardcode Token (Bắt buộc để `AuthInterceptor` xử lý).
- Nếu Backend thiếu Endpoint cho UI, AI ghi rõ trạng thái là `UNKNOWN` và báo cáo, tuyệt đối không tự "chế" (Mocking) logic làm sai lệch dữ liệu.

## 8. Quy Định Model Dữ Liệu
- AI **KHÔNG ĐƯỢC (MUST NOT)** tự thêm Field mới vào Model.
- AI **KHÔNG ĐƯỢC (MUST NOT)** tự xóa Field của Model.
- AI **KHÔNG ĐƯỢC (MUST NOT)** tự đổi kiểu dữ liệu (Ví dụ String sang Int) nếu khác với DTO Backend.
- Bắt buộc dùng `freezed` và `json_serializable` để sinh mã Model.

## 9. Quy Định UI/UX (Design Rules)
- AI **KHÔNG ĐƯỢC (MUST NOT)** tự sáng tạo UI ngẫu nhiên.
- UI **BẮT BUỘC (MUST)** bám sát Design System (Color, Typography tại `AppTheme`).
- Ưu tiên trải nghiệm UX chuẩn Mobile (Padding chạm ngón tay tối thiểu 44px, cuộn mượt).
- **KHÔNG ĐƯỢC (MUST NOT)** sao chép layout 100% từ giao diện Web Responsive sang Mobile.

## 10. Quy Định Viết Code (Coding Standards)
- Bắt buộc tuân thủ: SOLID, Clean Code, DRY (Don't Repeat Yourself), KISS (Keep It Simple, Stupid).
- **KHÔNG ĐƯỢC (MUST NOT)** viết Duplicate code.
- **KHÔNG ĐƯỢC (MUST NOT)** để lại Dead Code (Code bị comment out).
- **KHÔNG ĐƯỢC (MUST NOT)** để lại chú thích `TODO` hoặc `FIXME` khi submit hoàn thành.

## 11. Quy Định Logging & Security
- **KHÔNG ĐƯỢC (MUST NOT)** log Password, Access Token, Refresh Token, API Secret, Private Key dưới bất kỳ hình thức nào (`print` hay `Logger`).

## 12. Quy Định Xử Lý Lỗi (Error Handling)
- **KHÔNG ĐƯỢC (MUST NOT)** viết khối `try-catch` rỗng.
- **KHÔNG ĐƯỢC (MUST NOT)** dùng lệnh `print()` để in lỗi.
- **KHÔNG ĐƯỢC (MUST NOT)** Ignore (nuốt) Exception.
- Bắt buộc bọc lỗi trong mô hình `BaseState.error(AppException)` và dùng `AppSnackbar.showError()` từ Core Layer để hiển thị.

## 13. Điều Kiện Testing (Testing Criteria)
Quá trình lập trình chỉ hoàn tất khi:
- Lệnh `flutter analyze` trả về kết quả KHÔNG có lỗi (No issues found) và không có Warning nghiêm trọng.
- Lệnh `flutter test` (nếu có unit test) Pass 100%.

## 14. Tổng Hợp Điều Cấm Kỵ (Absolute NO-DOs)
- **KHÔNG** refactor toàn dự án.
- **KHÔNG** sửa cấu hình Router chung.
- **KHÔNG** đổi Theme chung.
- **KHÔNG** sửa đổi `CoreLayer`, `SecureStorage`, `ApiClient` hay `AuthInterceptor`.
- **KHÔNG** thay đổi thư mục cấu trúc (Folder Structure).

## 15. Quy Trình Báo Cáo Sự Cố (Escalation Protocol)
Nếu trong quá trình code, AI phát hiện:
- Bug từ Core Layer.
- Kiến trúc không hợp lý để phục vụ Feature.
- API Backend thiếu hoặc sai lệch kiểu dữ liệu.
- Business Logic bị mâu thuẫn (Contradiction).

=> AI **KHÔNG ĐƯỢC SỬA (MUST NOT FIX)**. 
Thay vào đó, AI phải tạo mục **Recommendations** riêng biệt trong phần báo cáo kết quả và trình bày vấn đề kỹ thuật để Technical Lead quyết định.

---

## 16. Tiêu Chí Nghiệm Thu Module (Definition of Done)
Một Feature/Module được coi là hoàn thành (Done) khi và chỉ khi:
- [x] Đúng phạm vi được giao.
- [x] Đúng API Endpoint.
- [x] Đúng chuẩn Model Dữ liệu.
- [x] Đúng Repository & State Management Pattern.
- [x] UI hiển thị hoàn chỉnh, tương tác mượt mà.
- [x] Xử lý lỗi (Network, Validation) đầy đủ.
- [x] Không gây lỗi cho (hay cản trở) các Module khác.
- [x] Không sửa bất kỳ tệp nào ngoài phạm vi thư mục `features/<assigned_module>`.

---

## 17. Checklist Tự Kiểm Tra (Pre-Flight Checklist)
Trước khi kết thúc lượt làm việc (Turn), AI **BẮT BUỘC (MUST)** tự đặt câu hỏi và rà soát:
- [ ] Mình có sửa file nào nằm ngoài thư mục Module được giao không?
- [ ] Mình có vô tình chạm vào thư mục `core/` không?
- [ ] Mình có thay đổi `router.dart` không?
- [ ] Mình có thay đổi `theme.dart` không?
- [ ] Mình có thêm package mới vào `pubspec.yaml` không?
- [ ] Mình có tự chế endpoint API không?
- [ ] Mình có hardcode URL hay Token không?
- [ ] Có dòng `TODO:` hoặc `FIXME:` nào chưa xử lý không?
- [ ] Mã nguồn có Pass được `flutter analyze` không?
- [ ] Mã nguồn có Pass được `flutter test` không?

> *Bằng việc bắt đầu sinh code cho Feature, AI Agent xác nhận đã đọc, hiểu và cam kết tuân thủ tuyệt đối toàn bộ Hợp đồng Phát triển này.*
