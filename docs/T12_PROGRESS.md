# T12 — Tiến độ mobile

Ngày 10/09/2026: hoàn thành phát triển mobile tối thiểu. Xem [biên bản T12](T12_COMPLETION.md) cho bằng chứng và giới hạn.

## Nghiệm thu đang thực hiện

- [x] Flutter tại `.artifacts/flutter-sdk`; test và Dart bundle đã đạt.
- [x] Bổ sung 6 test auth/refresh/scope; tổng 22/22 test đạt (18 nghiệp vụ, 4 placeholder; gồm API URL override).
- [x] Bổ sung quyền Internet Android release và mô tả quyền thư viện ảnh iOS.
- [x] Cài Android SDK 36, Build Tools 35, NDK 28.2 và JDK 17 vào `.artifacts`.
- [x] Tải Gradle 8.14 và đối chiếu SHA256 với nguồn chính thức.
- [ ] Build APK debug: đang thực hiện, chưa xác nhận thành công.
- [ ] Nghiệm thu thiết bị: chưa có thiết bị Android kết nối; iOS cần môi trường phù hợp.
- [x] Cập nhật database local aisam_local sau backup; 0 migration còn thiếu.
- [ ] Nghiệm thu API/provider và thiết bị thật.

Chạy lại build bằng `AISAM-MB/build-local-apk.ps1`. Xem [báo cáo nghiệm thu](FINAL_ACCEPTANCE_20260910.md) để đối chiếu bằng chứng và các mục còn mở.

Build cho API cụ thể: `.\AISAM-MB\build-local-apk.ps1 -ApiBaseUrl https://staging.example/api`. Dùng URL thật có thể truy cập từ thiết bị; ví dụ trên chỉ minh họa.

## Cập nhật database local — 10/09/2026

- Đã được người dùng cho phép cập nhật `aisam_local` trên `127.0.0.1:5432`, user `postgres`.
- CONNECTION_STRING trong `AISAM-BE/AISAM.API/.env` trỏ về database local; không ghi mật khẩu vào tài liệu.
- Backup trước cập nhật: `.artifacts/final-acceptance/aisam-local-before-20260910144542.dump` (252754 byte).
- Khôi phục hai migration lịch sử `AddWorkspaceBusinessProfile` và `AddProfileWorkspaceOwnership` cùng metadata từ commit `e58a3dc`: local cũ thiếu cột profile workspace, khiến migration phân quyền dừng. Không sửa lịch sử migration bằng cách đánh dấu giả đã chạy.
- EF database update đã đạt; preflight xác nhận **0 migration còn thiếu**.
- Count sau cập nhật giữ nguyên: Content 40, Post 30, Asset 0, automation plan 3, social integration 34. Đây là đối soát số lượng, không thay thế checksum toàn bộ dữ liệu.
- Log: `.artifacts/final-acceptance/local-migration.txt`; kiểm tra: `.artifacts/final-acceptance/database-preflight.json`.
- Không thay đổi database remote cũ. Chưa xác nhận các provider/OAuth/thanh toán thật.
## Backlog bổ sung BE/Web

Luồng Team/Brand và quyền Manager còn thiếu được theo dõi riêng tại [TEAM_BRAND_NEXT_TASKS.md](TEAM_BRAND_NEXT_TASKS.md). Đây là phần cần hoàn thiện trước khi xác nhận toàn bộ nghiệp vụ phân quyền; không nằm trong các mục mobile đã kiểm chứng.
