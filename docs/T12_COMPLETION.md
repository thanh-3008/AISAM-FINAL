# T12 — Hoàn thành phát triển mobile tối thiểu

Ngày 10/09/2026. Hoàn thành mã nguồn và kiểm tra cục bộ; nghiệm thu API thật/thiết bị được gom vào đợt cuối theo yêu cầu người dùng. Chưa chứng nhận production.

## Chức năng

- DTO ưu tiên PlainText, tương thích nội dung cũ và ảnh URL/JSON array. Hiển thị văn bản thuần, không thực thi HTML.
- Edit/delete/review lấy quyền từ API; chặn edit/review deep link thiếu quyền. Backend quyết định quyền cuối cùng.
- Danh sách, chi tiết, approval tải lại theo workspace. Bỏ phản hồi khác actor/workspace/profile; giới hạn refresh một lần, không replay multipart hoặc chuyển request sang scope mới.
- Draft văn bản trong phiên tách theo tài khoản/workspace; mở composer sau khi lưu nội dung.
- Thêm nhiều ảnh/video, tối đa 10 media, 50 MiB/file và 200 MiB/lượt chọn. Upload từng file, giữ thành công khi file sau lỗi; sắp xếp, xóa và lưu theo version.
- Import media legacy, gửi duyệt, xem capability theo kênh, chọn kênh publish. Chặn publish khi còn thứ tự chưa lưu.
- Lưu idempotency key trước POST; đọc journal sau lỗi hoặc mở lại màn hình, không tự POST lần nữa. Reload bỏ lựa chọn kênh cũ.
- Approval dùng review queue và endpoint approve/reject chuyên biệt. Màn hiệu quả cá nhân yêu cầu người hiện tại trong workspace, khoảng 30 ngày UTC.

## Kiểm tra cục bộ

Flutter 3.47.3 tại `.artifacts/flutter-sdk`. Chạy từ AISAM-MB:

```powershell
..\.artifacts\flutter-sdk\bin\flutter.bat test --no-pub
..\.artifacts\flutter-sdk\bin\flutter.bat analyze --no-pub --no-fatal-infos --no-fatal-warnings
..\.artifacts\flutter-sdk\bin\flutter.bat build bundle --debug --no-pub --target-platform windows-x64
```

- **15/15 test đạt**: 11 test mới kiểm tra nghiệp vụ và 4 placeholder cũ. Placeholder không chứng minh auth/workspace đã được kiểm thử.
- Test mới: whitelist payload media/version/thứ tự, permission thiếu dữ liệu, scope thay đổi, lỗi 403/404/409, endpoint duyệt, journal không replay sau lỗi/mở lại, readonly/kênh bị từ chối, PlainText và ảnh legacy.
- Analyze: **0 error**, còn 100 diagnostics (6 warning, 94 info). Lệnh cho phép warning/info, không có nghĩa lint sạch. Log: `.artifacts/t12-analyze-final.txt`.
- Build Dart debug bundle exit 0, đầu ra `AISAM-MB/build/flutter_assets/kernel_blob.bin`. Không phải APK hoặc bản cài Windows.

## Giới hạn và nghiệm thu cuối

- Editor mobile là văn bản thuần; có thông báo mất định dạng nâng cao khi lưu. Rich Text editor đầy đủ, phục hồi file/draft bền vững và quản trị nâng cao dùng Web.
- Publish status làm mới thủ công; sau khi ghi journal không tự retry/reset. Phục hồi publishing nâng cao dùng Web.
- Media hiển thị thứ tự/thông tin file; chưa có preview bố cục giống từng mạng xã hội hoặc trình phát video đầy đủ.
- Máy chưa có Android SDK. Cần build bản cài và thử thiết bị Android/iOS trong đợt nghiệm thu cuối.
- Tạo `.env` từ `.env.example`, đặt API_BASE_URL mà thiết bị truy cập được; localhost trên điện thoại không trỏ về máy BE.
- Còn thử API thật: auth/refresh đồng thời, đổi tài khoản/workspace khi tải, deep link trái quyền, thu hồi quyền, approval, nội dung cũ/mới và self-performance.
- Còn thử picker/quyền ảnh, HTTP upload/CDN, partial failure, xung đột version, mất mạng khi publish và mở lại journal trên kênh sandbox được phép. OAuth/PayOS và provider thật chưa được xác nhận.
- Hoàn tất checklist nghiệm thu cuối trong kế hoạch trước phát hành. Test giả lập không thay thế nghiệm thu này.