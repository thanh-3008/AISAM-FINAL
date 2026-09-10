# Nghiệm thu tổng thể — 10/09/2026

Trạng thái: kiểm chứng cục bộ đã chạy lại; **chưa đủ bằng chứng nghiệm thu 100% trên môi trường thật**. Không thay thế điều kiện phát hành bằng tỷ lệ task phát triển.

## Kết quả lượt kiểm tra hiện tại

| Phép kiểm tra | Kết quả | Phạm vi |
|---|---|---|
| Backend | 516/516 đạt, không skip | TRX `.artifacts/final-acceptance/final-backend.trx` |
| Web unit/component | 94/94 đạt, 28 file | Vitest |
| Web build | Đạt | Next production build, có cảnh báo middleware deprecated |
| Web E2E | 21/21 đạt | Edge, production build, API giả lập |
| Mobile | 22/22 đạt | 18 test nghiệp vụ, 4 placeholder cũ; có kiểm tra API URL override |
| Mobile analyze | 0 error, 100 diagnostics còn lại | Không yêu cầu warning/info làm lệnh thất bại |
| Mobile bundle | Đạt | Dart debug bundle windows-x64, không phải APK |
| Database nguồn | Kết nối được, đọc thành công | Read-only, không khởi động worker hoặc sửa nguồn |
| Backup mới | Đã tạo | `.artifacts/permission-backup/source-20260910041955.dump`, không đưa dữ liệu vào git |
| Migration bản restore | Kiểm tra checksum, apply/downgrade/reapply đạt | PostgreSQL cô lập 127.0.0.1:55439 |
| Tải query cục bộ | List 200 lượt/20 worker, p95 28,5 ms; analytics 100 lượt, p95 88,4 ms | 10.009 Content hiển thị trong workspace fixture; chưa gồm HTTP/CDN |

Hash từng file nguồn trong `.artifacts/final-acceptance/source-hashes.json`; baseline Git `e01171531e443e9cf771ff14ee9d5e51b125b842` cùng working tree chưa commit. Đây chưa phải commit phát hành.

## Sửa lỗi trong đợt nghiệm thu

- Bổ sung quyền Internet vào Android main manifest để bản release có thể gọi API. Theo [Flutter networking](https://docs.flutter.dev/data-and-backend/networking).
- Bổ sung mô tả quyền thư viện ảnh iOS cho picker ảnh/video. Theo [image_picker](https://pub.dev/packages/image_picker).
- Kiểm tra lại actor sau khi đọc refresh token; không xóa phiên mới vì phản hồi 401 của phiên cũ. Refresh client kế thừa timeout và có thể kiểm thử bằng transport giả lập.
- Thêm 6 test auth: phản hồi khác workspace, concurrent refresh, đổi tài khoản trong refresh, đổi workspace trước retry, giới hạn retry và multipart không replay.
- Tắt log header/body request/response mobile để luồng đăng nhập không ghi credential vào debug log.
- Thêm công cụ `AISAM-BE/tools/AcceptancePreflight`: kiểm tra nguồn trong chế độ database read-only, chỉ ghi migration/count và loại lỗi, không ghi connection string/token.
- Harness backup hỗ trợ `--fresh-backup` để nghiệm thu từ dữ liệu hiện tại thay vì tái sử dụng backup cũ.

## Điểm chặn xác định từ .env và database thật

Người dùng cho phép đọc `.env`. URL ứng dụng trỏ localhost; không tìm thấy URL staging được chỉ định. Có cấu hình PayOS/social nhưng cấu hình không xác định tài khoản/kênh nào được phép dùng cho sandbox.

Database đang dùng còn **12 migration chưa áp dụng**:

```text
20260907075759_ReconcilePermissionFoundation
20260907162333_AddAutomationCreatorAttribution
20260908003735_CompletePermissionSchema
20260908064221_ConversationCreatorScope
20260908142531_AddApprovalSubmissionTimestamp
20260908222838_AddMediaCollectionsAndSnapshots
20260908232337_AddPublishOperations
20260908232810_AddPublishOperationMediaResults
20260908233207_AddPublishRequestIdempotencyBoundary
20260909034403_AddSnapshotMediaDimensions
20260909115446_AddRichTextDocument
20260910010000_FixPermissionTriggerRecordAccess
```

Số lượng nguồn lúc kiểm tra: 544 Content, 179 Post, 0 Asset, 36 automation plan, 147 social integration. Không suy ra các integration còn token/quyền hợp lệ.

Backup mới vẫn phát hiện 579 vấn đề reconciliation: 542 Content thiếu creator, 36 automation thiếu creator, 1 Post lệch Brand/kênh. Giữ fail-closed khi không có bằng chứng attribution; không tự gán Owner để làm số liệu sạch.

## Còn phải hoàn tất trước khi ghi nhận 100%

- [ ] Xác định database nguồn là môi trường được phép nâng cấp; apply 12 migration sau backup và đối soát nguồn. Hiện chỉ đã kiểm chứng trên bản sao.
- [ ] Chạy API/browser với schema mới và actor thật; kiểm tra permission, approval, Owner billing, OAuth, quota và dữ liệu legacy.
- [ ] Chỉ định kênh sandbox và phạm vi thao tác ngoài hệ thống; kiểm thử publish/revoke/timeout/partial success, upload/CDN và checkout. Không dùng sự hiện diện API key làm bằng chứng sandbox.
- [x] Chuẩn bị Android SDK 36, Build Tools 35, NDK 28.2, JDK 17 và Gradle 8.14 (checksum chính thức khớp).
- [ ] Build APK và thử thiết bị Android/iOS/picker/OAuth/network thực tế; Gradle đang tải dependency, chưa có thiết bị kết nối.
- [ ] Đối chiếu Swagger/ERD, đóng băng phiên bản release và lưu bằng chứng nghiệm thu môi trường triển khai.

Chạy lại kiểm tra nguồn, từ `AISAM-BE`:

```powershell
& 'C:\Users\thanh\.dotnet\dotnet.exe' run --project tools/AcceptancePreflight/AcceptancePreflight.csproj
```

Không cần bật API để chạy công cụ này; tránh làm worker thao tác trên dữ liệu nguồn trong khi chỉ muốn kiểm kê.

## Cập nhật database local — 10/09/2026

- Đã được người dùng cho phép cập nhật `aisam_local` trên `127.0.0.1:5432`, user `postgres`.
- CONNECTION_STRING trong `AISAM-BE/AISAM.API/.env` trỏ về database local; không ghi mật khẩu vào tài liệu.
- Backup trước cập nhật: `.artifacts/final-acceptance/aisam-local-before-20260910144542.dump` (252754 byte).
- Khôi phục hai migration lịch sử `AddWorkspaceBusinessProfile` và `AddProfileWorkspaceOwnership` cùng metadata từ commit `e58a3dc`: local cũ thiếu cột profile workspace, khiến migration phân quyền dừng. Không sửa lịch sử migration bằng cách đánh dấu giả đã chạy.
- EF database update đã đạt; preflight xác nhận **0 migration còn thiếu**.
- Count sau cập nhật giữ nguyên: Content 40, Post 30, Asset 0, automation plan 3, social integration 34. Đây là đối soát số lượng, không thay thế checksum toàn bộ dữ liệu.
- Log: `.artifacts/final-acceptance/local-migration.txt`; kiểm tra: `.artifacts/final-acceptance/database-preflight.json`.
- Không thay đổi database remote cũ. Chưa xác nhận các provider/OAuth/thanh toán thật.