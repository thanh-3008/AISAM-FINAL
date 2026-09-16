# R06 — Cách ly nội dung và duyệt

Ngày cập nhật: 14/09/2026. Hoàn thành phần backend của R06 và kiểm thử cục bộ. Chưa bật `Rbac:UseV2` trên ứng dụng; giao diện chọn Team và thao tác thu hồi duyệt thuộc R09.

## Đã thực hiện

- Tạo bài bắt buộc có Team đang hoạt động, liên kết đúng Brand và quyền tạo. Clone giữ Team của bài gốc, kiểm tra quyền tạo và ghi nhận tác giả mới.
- Sửa/xóa chỉ cho Draft/Rejected theo quyền thực tế của Team. Creator chỉ sửa/xóa bài mình. Giá trị role legacy truyền vào service không cấp thêm quyền v2.
- Submit kiểm tra quyền sửa; Approve/Reject kiểm tra quyền review và trạng thái PendingApproval. Không cho dùng update để tự chuyển sang Approved, Published hoặc bỏ qua bước submit.
- List/detail/search/tag và các truy vấn nội dung dùng bộ lọc EF theo workspace, Team và trạng thái. Manager ở Team A không mở rộng quyền Viewer ở Team B dù cùng Brand. Review queue giới hạn Team có quyền quản lý.
- Media/asset gắn với bài phải thuộc phạm vi nội dung có thể đọc. Không được tái sử dụng asset của bài bị ẩn bằng cách gửi ID, kể cả khi người gửi là uploader. Asset chưa gắn bài chỉ được đính kèm bởi uploader; danh sách asset của Member không liệt kê các asset chưa gắn nội dung.
- Viewer chỉ thấy snapshot được duyệt hiện hành, không thấy snapshot bản nháp cũ, lịch sử nhận xét từ chối hoặc prompt AI của bài. Các bản ghi lịch sử vẫn được giữ cho người có quyền.
- Thêm `POST /api/content/{contentId}/withdraw`: chỉ Owner, WorkspaceManager hoặc Manager của Team, chỉ bài Approved. Còn lịch Pending/Processing đang hoạt động trả 409; phải hủy lịch trước. Thu hồi đưa về Draft, xóa tham chiếu approval/submission hiện hành, đổi version và thêm lịch sử thu hồi. Bài phải được submit/duyệt lại.
- Sửa filter chuyển TeamId cho kiểm tra tạo bài và tránh yêu cầu quyền tạo khi chỉ đổi Product trên bài đang sửa.

## Kiểm tra

43 kiểm thử đạt, 0 lỗi, 0 bỏ qua:

```powershell
& C:\Users\thanh\.dotnet\dotnet.exe test AISAM-BE/tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj --filter 'FullyQualifiedName~ContentServiceTests|FullyQualifiedName~RbacV2Tests|FullyQualifiedName~ResourcePermissionFilterTests|FullyQualifiedName~ContentMediaTests|FullyQualifiedName~ContentRepositoryTests|FullyQualifiedName~PermissionScope' --verbosity quiet
```

Bao gồm kiểm tra service, policy/resolver, filter, cách ly asset và snapshot, thu hồi duyệt, và sinh SQL PostgreSQL cho query filters. Các kiểm tra nghiệp vụ dùng database InMemory; sinh SQL không đồng nghĩa đã thực thi truy vấn trên PostgreSQL. `git diff --check` không có lỗi khoảng trắng.

## Ranh giới nghiệm thu

- Chưa nghiệm thu trình duyệt, lưu trữ media thật hoặc triển khai v2; không có thao tác đăng bài/thanh toán bên ngoài trong R06.
- Đồng bộ tranh chấp giữa thu hồi duyệt và worker nhận lịch, kiểm tra quyền tại thời điểm job chạy thuộc R07; chưa chứng nhận an toàn concurrency của toàn luồng publishing.
- R08 kiểm tra analytics/export số liệu; R09 bổ sung UI v2; R11 kiểm thử tích hợp PostgreSQL và hồi quy đầy đủ.
- Restore nội dung đã xóa hiện bị từ chối trong v2; không dùng endpoint này để mở lại nội dung ngoài scope.
- Bộ lọc bảo vệ dữ liệu trả qua API, không thu hồi được URL media công khai mà người dùng đã biết từ trước.
