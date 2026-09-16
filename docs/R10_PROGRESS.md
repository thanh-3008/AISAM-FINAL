# R10 — Đồng bộ Mobile với RBAC hai tầng

Ngày cập nhật: 15/09/2026.

Trạng thái: **hoàn thành phạm vi R10 về contract và luồng nội dung Mobile, kiểm thử cục bộ đạt**. Các chức năng chưa hỗ trợ được liệt kê riêng bên dưới; không đồng nghĩa Mobile đã ngang bằng Web hoặc đã nghiệm thu trên thiết bị thật.

## Phạm vi

R10 đồng bộ contract và luồng nội dung đang có trên Flutter. Mobile không có đầy đủ chức năng quản trị như Web; các màn chưa chuyển sang contract v2 được chặn bằng thông báo rõ ràng, không gửi payload vai trò cũ vào API mới.

## Đã triển khai

- Gửi `X-RBAC-Contract-Version: 2` cùng JWT và workspace/profile hiện hành. Giữ cơ chế loại phản hồi đến muộn khi đổi workspace hoặc tài khoản.
- Đọc `/permissions/context`, phân biệt `Owner`, `WorkspaceManager`, `Member` với `Manager`, `ContentCreator`, `Viewer` trong Team. Giá trị số của enum cũ không được dùng để cấp quyền v2. Contract lạ hoặc role không hợp lệ bị từ chối.
- Context được tải lại theo workspace và định kỳ 60 giây khi có màn đang theo dõi. Màn kiểm tra contract tải lại khi ứng dụng trở về foreground. Đây không phải cơ chế đẩy sự kiện thu hồi quyền tức thời; API vẫn kiểm tra quyền mỗi thao tác.
- Bỏ nhánh tự cấp toàn bộ quyền nội dung cho role cũ Owner/Manager trên client. Quyền xem, sửa, xóa, duyệt lấy từ `/permissions/check`; lỗi kiểm tra trả về các quyền `false`. Chi tiết nội dung không hiển thị khi chưa xác nhận quyền xem.
- Tạo bài thủ công và tạo nội dung AI chọn Team thuộc đúng Brand. Member chỉ chọn Team có vai trò Manager/ContentCreator; Owner/WorkspaceManager cũng phải chọn Team thực, không dùng scope Brand chưa gán Team.
- Gửi `teamId` trong DTO tạo nội dung và AI. DTO cập nhật nội dung không có trường đổi Team. Đổi Brand xóa lựa chọn Team cũ; form không được gửi sau khi đổi workspace/tài khoản.
- Tạo AI mở bản nháp mà API đã lưu qua `contentId`, tránh chuyển sang form tạo thêm một bản sao.
- DTO nhận thêm trường workspace role riêng; DTO chat có chỗ truyền Team cho phần tích hợp tiếp theo. Không đổi ý nghĩa các trường enum legacy.
- Khi revision thay đổi, danh sách/chi tiết nội dung và quyết định quyền được nạp lại. Danh sách hội thoại theo dõi workspace/context.
- Chờ cập nhật workspace hiện hành xong trước khi hoàn tất thao tác chọn workspace.
- Lưu `X-HR-Revision` theo tài khoản/workspace để gửi `If-Match` cho thao tác HR. 403/409/428 không tự retry, không xóa đăng nhập. 428 có thông báo xung đột yêu cầu tải lại.

## Chức năng Mobile chưa hỗ trợ trong chế độ v2

| Phần | Cách xử lý hiện tại |
|---|---|
| Tạo/sửa Team, mời thành viên, thay role, gán Brand/kênh | Các route `/settings/team` và `/settings/team/members` hiện thông báo dùng Web; form legacy không được dựng hoặc gọi API |
| Mở/tạo/tiếp tục hội thoại AI theo Team | `/chat/new` và `/chat/:id` hiện thông báo dùng Web. Tạo nội dung AI trên Mobile vẫn được hỗ trợ qua màn riêng có chọn Team |
| KPI thành viên, quản trị nâng cao, automation v2 đầy đủ | Tiếp tục sử dụng Web; R10 không bổ sung các module Mobile mới này |
| Push thu hồi quyền, kiểm thử thiết bị thật, OAuth/đăng bài/thanh toán sandbox | Chưa nghiệm thu trong R10; thuộc kiểm thử tích hợp/vận hành R11/R12 |

Màn legacy vẫn được phép dựng khi server trả context legacy chỉ có `revision`. Backend không được nới quyền để phục vụ client cũ.

## Kiểm chứng cục bộ

- `flutter test --reporter expanded`: 59/59 test đạt, gồm 11 test mới cho giải mã role, scope Team, DTO, header/revision, xử lý từ chối và màn chưa hỗ trợ.
- Phân tích toàn dự án: không có lỗi biên dịch; còn các warning/info, chủ yếu mã giao diện và API deprecated hiện có. Không coi toàn bộ lint đã sạch.
- Log kiểm thử: `.artifacts/r10-mobile-tests.log`; log phân tích: `.artifacts/r10-mobile-analyze.log`.
- `dart analyze` cho context, gate, chọn Team và các test mới: **No issues found**; log `.artifacts/r10-scoped-analyze.log`.
- `flutter build bundle --debug --no-pub`: **exit 0**, có `AISAM-MB/build/flutter_assets/kernel_blob.bin`. Cần đặt `ANDROID_HOME`/`ANDROID_SDK_ROOT` tới `.artifacts/android-sdk`, `JAVA_HOME` tới JDK trong `.artifacts/java` ở máy này. Đây là bundle kiểm tra biên dịch, chưa phải APK đã cài trên thiết bị.
- Sau chỉnh sửa cuối điều kiện quyền, chạy lại riêng 6 test context/DTO: **6/6 đạt**, log `.artifacts/r10-context-final.log`. `git diff --check` phần Mobile đạt.

## Công cụ và giới hạn xác nhận

SDK dùng tại máy này: `.artifacts/flutter-sdk`, Flutter 3.47.3/Dart 3.13.3. `pubspec.lock` cập nhật các dependency do Flutter SDK ghim; không nâng major Freezed/Riverpod.

Bộ generator hiện có dùng analyzer cũ, không chạy được toàn bộ Riverpod generation với SDK này. Các DTO thay đổi đã sinh thành công bằng `build_runner --build-filter` cho từng model; các file sinh mã không liên quan được giữ nguyên. Không còn file generated bị xóa. Cần đồng bộ bộ SDK/generator trước lần tái sinh toàn dự án tiếp theo.

Không chạy migration lên database ứng dụng, không bật `Rbac:UseV2`, không gọi provider hoặc tạo giao dịch bên ngoài trong R10. Test cục bộ không thay thế nghiệm thu app trên thiết bị và backend thật.
