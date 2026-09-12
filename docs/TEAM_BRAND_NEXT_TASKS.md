# Việc tiếp theo — Team, Brand và quyền Manager

Cập nhật 10/09/2026 theo trao đổi với người dùng. Đây là backlog cần thực hiện, chưa phải chức năng đã hoàn thành. Ưu tiên hoàn thiện BE/Web trước, sau đó đồng bộ mobile.

## 1. Nguyên tắc nghiệp vụ

- Owner quản lý toàn workspace, thấy mọi Brand, quản lý billing, vai trò và quyền sở hữu.
- Manager quản lý Brand được giao hoặc tự tạo hợp lệ; không có quyền xem Brand khác, billing hay chuyển Owner.
- Brand Manager tạo thuộc workspace, không phải tài sản riêng; Owner vẫn quản lý và có thể bàn giao khi Manager rời workspace.
- Đề xuất mặc định tắt hai quyền Tạo Team và Tạo Brand của Manager; Owner cấp riêng khi cần. Cần thể hiện quyết định này trong contract và decision log trước khi triển khai.
- Manager chỉ gán Brand/Team và cấp quyền trong phạm vi được phép phân quyền; quyền sử dụng không tự đồng nghĩa quyền cấp lại.
- Mời vào workspace không tự cấp quyền Brand. Quyền duyệt, đăng và quản lý kênh là các quyền độc lập.
- Giữ Brand ngoài scope không hiển thị theo policy hiện tại. Danh mục Brand công khai và yêu cầu truy cập mới là đề xuất tùy chọn, chưa đưa vào phạm vi triển khai này.

## 2. Task A — Chốt contract và quyền quản lý

- [ ] Đối chiếu policy hiện tại với quyền Tạo Team/Tạo Brand và quyền phân công; ghi rõ phần thay đổi.
- [ ] Xác định cách lưu quyền cấp bởi Owner, kiểm tra quota gói và scope trên backend.
- [ ] Thiết kế API/DTO/error cho Team CRUD, membership Team, gán Brand và quyền kênh.
- [ ] Phân biệt thành viên workspace, thành viên Team và liên kết Team–Brand; không dùng vai trò hiển thị trên UI để cấp quyền.

Hoàn thành khi: contract và ma trận Owner/Manager/Creator/Viewer rõ ràng, không có đường tự nâng quyền.

## 3. Task B — Tạo và quản lý Team thật

- [ ] Bổ sung API tạo, sửa, xem và lưu trữ/xóa mềm Team với kiểm tra quyền.
- [ ] Thay hàm createTeam giả lập bằng API lưu database; rà lại các hàm update/delete giả lập liên quan.
- [ ] Gắn form Tạo Team vào Team Management; tên, mô tả, người quản lý, thành viên và Brand phụ trách.
- [ ] Chọn người đã có membership hoạt động trong workspace; lời mời chưa chấp nhận hiển thị riêng, chưa cấp quyền.
- [ ] Thêm/gỡ thành viên Team; kiểm tra phạm vi Manager và chống thêm người khác workspace.
- [ ] Cho phép xem thông tin đồng đội cơ bản trong phạm vi được cấp; không mở toàn bộ danh sách workspace hoặc KPI cho mọi thành viên.
- [ ] Lưu cấu hình có tính nguyên tử; không báo thành công nếu chỉ một phần được ghi.

Hoàn thành khi: tạo Team trên UI, reload vẫn tồn tại; thành viên/Brand đúng trong database, lỗi lưu không để lại cấu hình dở dang.

## 4. Task C — Gán Brand và quyền kênh

- [ ] Gán một hoặc nhiều Brand cho Team; chỉ cho chọn Brand được phép phân công.
- [ ] Cấu hình quyền xem/đăng/quản lý từng kênh; không tự cấp mọi kênh khi gán Brand.
- [ ] Quyền duyệt và xem lịch sử Creator khác phải thể hiện rõ, không đi kèm ngầm với quyền đăng.
- [ ] Trang Team và trang quyền Brand dùng chung API/dữ liệu phân quyền.
- [ ] Hiển thị bản tóm tắt quyền trước khi lưu và thông báo thành công/lỗi rõ ràng.
- [ ] Thu hồi quyền làm mới cache và kiểm tra job chưa thực thi; quyền qua Team khác vẫn được tính hợp lệ.
- [ ] Hiển thị trạng thái “Bạn chưa được phân công Brand. Hãy liên hệ Owner hoặc người quản lý.” khi chưa có scope.

Hoàn thành khi: thành viên được gán thấy đúng Brand/kênh; đổi URL hoặc gọi API trực tiếp không vượt scope.

## 5. Task D — Manager tạo Brand

- [ ] Owner có thể cấp/thu hồi quyền Tạo Brand và Tạo Team riêng cho Manager.
- [ ] Tạo Brand kiểm tra membership, quyền tạo và quota workspace.
- [ ] Lưu Brand cùng liên kết quyền quản lý ban đầu cho Manager trong một transaction; ghi actor tạo/audit.
- [ ] Manager có thể phân công Brand mới theo quyền được cấp; không tự mở quyền dùng social account toàn workspace.
- [ ] Khi Manager rời workspace/đổi vai trò, tài nguyên vẫn giữ; Owner có thể bàn giao trách nhiệm.
- [ ] Kiểm tra lại quyền trên mỗi thao tác, kể cả khi Owner vừa thu hồi quyền tạo/phân công.

Hoàn thành khi: Manager được cấp tạo Brand và quản lý Brand mới; Manager khác không tự nhìn thấy; Owner luôn quản lý được.

## 6. Task E — Kiểm thử và cập nhật tiến độ thật

- [ ] E2E: Owner mời → thành viên chấp nhận → tạo Team → thêm người → gán Brand/kênh → thành viên truy cập đúng phạm vi.
- [ ] E2E: Manager được cấp quyền tạo Brand/Team; Manager không được cấp bị chặn ở cả UI và API.
- [ ] Test chéo workspace/Brand, gửi ID giả, tự nâng quyền, thu hồi quyền và quyền qua nhiều Team.
- [ ] Test rollback khi một bước cấu hình lỗi; xung đột khi hai người cùng sửa quyền.
- [ ] Test Creator/Viewer chỉ thấy dữ liệu và đồng đội được phép; kiểm tra scope thống kê thành viên.
- [ ] Đồng bộ mobile theo subset sản phẩm, hoặc ghi rõ tác vụ quản trị chỉ có trên Web.
- [ ] Cập nhật checklist chính theo bằng chứng thực tế, không đánh dấu hoàn thành chỉ vì có entity/form/hàm service.

## 7. Nghiệm thu đang còn mở ngoài nhóm trên

- [ ] Xác nhận kết quả build APK và nghiệm thu thiết bị.
- [ ] Kiểm thử API thật, OAuth, upload/CDN, publish và checkout trong môi trường/kênh được phép.
- [ ] Đối chiếu schema với model ngoài kiểm tra migration pending; bảo đảm lỗi thiếu cột đã được bao phủ.
- [ ] Chốt bản code/database nghiệm thu, tài liệu và các giới hạn còn lại trước bàn giao.

Thứ tự đề xuất: A → B → C → D → E. Chỉ đánh dấu từng task lớn hoàn thành khi đạt tiêu chí và kiểm thử tương ứng.
