# R04 — Team CRUD và thành viên

Hoàn thành backend v2 và kiểm thử cục bộ ngày 14/09/2026. Kế thừa TeamService/TeamController hiện có, không tạo bộ API trùng.

- [x] Owner/WorkspaceManager tạo, sửa, deactivate Team. TeamManager không tự tạo/sửa thông tin/xóa Team dù có delegation legacy.
- [x] TeamManager chỉ thêm/gỡ/đổi Creator/Viewer trong Team mình quản lý; không bổ nhiệm, hạ hoặc gỡ Manager.
- [x] Xác minh workspace membership và Team hoạt động; workspace chỉ đọc không được ghi. List/detail của Member giới hạn Team mình tham gia.
- [x] V2 chỉ nhận UserId thực, không dùng shim WorkspaceMember.Id. Kiểm tra role chính xác, tên/mô tả giới hạn và thành viên trùng/ngoài workspace trước tạo Team.
- [x] Tạo Team không tự thêm người tạo vào Team ở v2; danh sách thành viên là danh sách được chỉ định. O/W vẫn đọc được Team không có membership riêng.
- [x] Deactivate dùng Inactive=1, giữ bản ghi Team/Content; vô hiệu TeamBrand, membership và ScopeEnabledV2 của kênh cùng lần lưu, có audit.
- [x] Controller Team dùng HR concurrency filter, revision tính cả Team và TeamMember; các mutation cần If-Match ở v2.
- [x] TeamManager được qua cổng middleware cho route thành viên, service vẫn kiểm tra Team và giới hạn vai trò.
- [x] Không đưa email đồng đội vào response v2 cho Member; role đầu vào sai trả lỗi thay vì mặc định Creator.

## API hiện có được sử dụng

GET /api/teams/manage (danh sách đầy đủ), GET /api/teams/{id}, POST /api/teams, PUT /api/teams/{id}; DELETE /api/teams/{id} và POST /api/teams/{id}/deactivate cùng vô hiệu hóa trong v2.

POST /api/teams/{id}/members thêm người; PUT /api/teams/{id}/members/{userId} đổi role; DELETE route đó gỡ người. Giữ PUT cập nhật hiện có thay vì thêm PATCH với ý nghĩa khác. GET /api/teams nhẹ ở ResourceAssignmentsController vẫn tồn tại, không đăng ký thêm route trùng. Client lấy X-HR-Revision từ GET /manage hoặc detail trước ghi.

Gán Brand/kênh vẫn dùng assignment APIs và được hoàn thiện ở R05; tạo Team và gán Brand là các thao tác riêng của API hiện tại. Không tuyên bố wizard Web đã xong (R09).

## Kiểm tra

Filter TeamV2Tests, WorkspaceHrV2, ActiveWorkspaceMiddlewareTests, RbacV2Tests: **61 passed, 0 failed**. Build Services thành công. git diff --check đạt (cảnh báo LF/CRLF của Windows).

Ca mới kiểm tra tạo Team hợp lệ, từ chối người ngoài workspace trước ghi, không tự thêm Owner, chặn Manager đổi tên Team/nâng role/gỡ Manager, cho đổi Creator→Viewer, deactivate thu hồi scope và giữ Content, Member không còn thấy Team inactive. Các ca dùng EF InMemory; transaction và tải đồng thời PostgreSQL cần nghiệm thu R11.

Không bật Rbac:UseV2, không thay .env/database ứng dụng. Luồng UI và tích hợp staging chưa được nghiệm thu.
