# R09 — Giao diện Web RBAC hai tầng

Ngày cập nhật: 15/09/2026.

**Hoàn thành mã nguồn và kiểm thử cục bộ R09.** Đã nối các luồng Web theo RBAC hai tầng, bao gồm phần AI/automation và thao tác lịch còn mở ở lần trước. Chưa nghiệm thu môi trường thật; R11/R12 giữ phạm vi hồi quy tích hợp và triển khai.

## Các phần đã thực hiện

- Nhận permission context v2 trước khi dựng dashboard; phân biệt workspace role với Team role. Context lỗi/không hỗ trợ không được suy diễn thành quyền legacy.
- API gửi `X-RBAC-Contract-Version: 2`. Các thay đổi nhân sự/Team dùng `If-Match` từ `X-HR-Revision` của đúng workspace; không tự phát lại thao tác khi gặp conflict.
- Owner/WorkspaceManager mời thành viên với workspace role phù hợp, tạo Team thật, thêm thành viên đang hoạt động, chọn/đổi Team role, đổi tên và ngừng hoạt động Team. Owner đổi workspace role; thao tác gỡ thành viên tuân theo tầng workspace.
- Team Manager chỉ quản lý Creator/Viewer trong Team của mình; không có nút tạo Team hoặc mời người vào workspace. Thành viên vẫn xem được danh sách trong phạm vi backend trả về.
- Hai trang quản lý quyền Brand dùng giao diện v2 chung: gán Team rồi cấp từng kênh bằng scope v2; không gửi các cờ CanView/CanPublish/CanManage legacy.
- Tạo nội dung thủ công yêu cầu Team của Brand đã chọn; thay Brand xóa Team cũ. Nút tạo ở danh sách kiểm tra quyền với TeamId. Creator/Manager Team A không dùng quyền A để chọn Team B nơi chỉ là Viewer.
- Màn duyệt dùng quyết định theo từng nội dung, không dùng role workspace cũ để bỏ qua kiểm tra. Màn chi tiết có thu hồi duyệt và quyền đăng/lên lịch theo nội dung/kênh.
- Calendar kiểm tra quyền của nội dung trên từng kênh trước khi gửi tạo lịch; backend tiếp tục là nơi quyết định cuối cùng.
- Đổi workspace hoặc revision quyền làm mất hiệu lực dữ liệu đang hiển thị. Nút kiểm tra lại quyền thực sự tải context mới. Một API nghiệp vụ trả 403 không tự đăng xuất người dùng.
- Member không gọi subscription từ các provider/hook dùng chung khi không có `billing.read`.
- AI chat truyền TeamId; backend kiểm tra quyền tạo nội dung trên Team/Brand trước khi gọi provider hoặc ghi dữ liệu. Các bài AI được gắn TeamId.
- Hội thoại có TeamId riêng; không dùng lại hội thoại của người khác hoặc Team khác. Member chỉ đọc lịch sử do mình tạo trong Team còn quyền viết. Hội thoại cũ thiếu Team không tự được gán sang Team mới.
- Form automation chọn Team, gửi TeamId khi lưu, không cho đổi Team/Brand của dòng đã có Content. CSV v2 có cột TeamId; có thể bổ sung Team qua Edit Request sau khi nhập. Quyền duyệt từng dòng/duyệt cả plan được giới hạn theo Team trong context.
- Sửa/xóa lịch kiểm tra quyền trên Content và kênh; kênh thay thế cũng được kiểm tra. Không sửa lịch đang Processing/Completed. Hoàn tác xóa tạo lại lịch qua API và chỉ thông báo thành công sau khi lưu được; không thêm lại bản ghi giả trên UI.

## Luồng kiểm tra thủ công khi bật v2

1. Owner/WorkspaceManager mở `/team`, mời đúng email; người nhận dùng luồng chấp nhận lời mời hiện có.
2. Tạo Team và thêm thành viên đã chấp nhận, chọn Manager/ContentCreator/Viewer.
3. Mở Brand → quyền truy cập, gán Team và chọn kênh đã kết nối.
4. Creator mở Content → tạo thủ công, chọn Brand/Team, lưu nháp và gửi duyệt.
5. Manager Team mở Approvals để duyệt; mở chi tiết bài để đăng hoặc chuyển Calendar lên lịch.
6. Thử tài khoản Viewer và tài khoản Manager A/Viewer B để kiểm tra nút, dữ liệu và phản hồi API đúng scope.
7. Thu hồi quyền từ phiên khác, quay lại cửa sổ hoặc chờ kiểm tra định kỳ 60 giây; dữ liệu phải được tải lại theo quyền mới.
8. Mở AI Generate, chọn Brand/Team rồi tạo nội dung. Chuyển Team phải tách lịch sử; dùng conversationId Team khác phải bị từ chối.
9. Nhập CSV automation, mở Edit Request để chọn Team cho dòng thiếu scope; xác nhận rồi duyệt bằng Manager đúng Team.
10. Thử sửa kênh/lịch, xóa và hoàn tác; API thất bại không được hiện bản ghi đã lưu thành công.

## Bằng chứng kiểm tra

- `npm.cmd test -- --run`: **133 test đạt / 37 file** (toàn bộ bộ test Web hiện có).
- `npm.cmd run build`: **đạt**, gồm TypeScript và sinh 61 trang.
- Backend: **70 test đạt** với filter `AIServiceTests|RbacV2Tests|ContentScheduleServiceTests|AutomationServiceTests|ResourcePermissionFilterTests|ConversationControllerTests`.
- Kiểm tra mô hình PostgreSQL bằng EF: không có chênh lệch giữa relational model hiện tại và migration snapshot; query hội thoại có điều kiện Team. Kiểm tra này không kết nối database.
- Có kiểm tra conflict không báo thành công, không gửi grant legacy, không dùng HR revision của workspace khác, không mở quyền quản trị workspace cho Team Manager và không cho Viewer chọn Team tạo bài.

## Migration bổ sung và điều kiện chạy

- Thêm `20260915100000_AddConversationTeamScope`: cột nullable `conversations.team_id`. Không suy luận Team từ Brand cho hội thoại cũ. Down migration chỉ bỏ cột mới.
- Đồng bộ snapshot của `TeamMember.Role` về integer theo migration chuyển enum đã có trước đó; không thêm lệnh đổi enum lần nữa.
- Cần áp dụng đầy đủ migration trên database đã chọn trước khi chạy bản backend mới. Chưa thực hiện `database update` trong R09. Khi triển khai, dùng quy trình backup/migrate/rollback của R11/R12.

## Giới hạn xác nhận

- Chưa bật `Rbac:UseV2`, chưa áp dụng migration lên database ứng dụng và chưa gửi lời mời/đăng bài/thanh toán thật.
- Kiểm thử Web dùng component/API-client với mock; backend dùng repository giả/InMemory cùng kiểm tra dịch SQL PostgreSQL. Không thay thế Web E2E với database thật hoặc nghiệm thu OAuth/provider. Các phần đó thuộc R11/R12.
- Bộ build vẫn cảnh báo convention `middleware` của Next.js; build thành công. Không đổi cơ chế middleware trong task RBAC này.

**R09 đã đóng trong phạm vi mã nguồn và kiểm thử cục bộ. Task tiếp theo: R10 — Đồng bộ Mobile.**
