# T04 — Permission UX trên Web

**Hoàn thành — 08/09/2026.** Phạm vi: Brand Access, Team, Social, Content, Approvals và ngữ cảnh/cache của dashboard.

## Các thay đổi

- Màn hình `/brands/[id]/access` xem/gán/gỡ Team–Brand, cấp/thu hồi quyền xem, đăng bài và quản lý kênh. Không cho chọn đăng/quản lý khi chưa có quyền xem. Tắt quyền xem đồng thời thu hồi các quyền phụ thuộc. Khi đang lưu, khóa thao tác để tránh gửi song song cùng revision.
- Liên kết từ Brand và Team tới quản lý quyền. Thao tác sửa Brand/sản phẩm được khóa cho tới khi backend xác nhận BrandManage. Social account OAuth/disconnect chỉ dành Owner theo contract T03; quyền Integration được quản lý theo Brand.
- `POST /api/permissions/check` trả quyết định theo đúng thứ tự, tối đa 100 mục/request, chỉ dùng actor từ JWT và workspace đã được middleware xác nhận. Không cấp quyền và không nhận actor do client chỉ định. Endpoint này được phép đọc quyết định khi workspace chỉ đọc; policy vẫn từ chối quyền ghi.
- `GET /api/permissions/context` trả hash ngữ cảnh quyền, không trả chi tiết membership/grant. Hash gồm role, trạng thái workspace, Brand/Team/kênh, quyền ủy nhiệm và grant kênh. FE kiểm tra khi trở lại tab và mỗi 60 giây; hash đổi sẽ bỏ dữ liệu/cache cũ. Đây là thời gian cập nhật UI, không phải thời hạn backend tiếp tục cho phép: backend xác minh từng request.
- Creator mặc định mở “My content”; người dùng có thể chuyển sang toàn bộ nội dung được phép xem. `mine=true` lọc PrimaryCreatorId ở database, trước count/pagination. Quyền xem nội dung người khác không tự cho phép sửa/xóa. Library và queue tải đủ các trang theo scope, không dừng âm thầm ở mục thứ 100.
- Menu nội dung và trang chi tiết lấy quyền sửa/xóa từ BE. Post Now chỉ hiển thị Integration được phép đăng cho chính nội dung đó; quyền xem kênh không đủ. Bulk delete kiểm tra quyền trước khi gửi, backend tiếp tục kiểm tra khi lưu.
- Approvals dùng `/content/review-queue`, chỉ lấy nội dung chờ duyệt trong phạm vi review. Creator có ViewAll nhưng không có Review không xuất hiện trong queue. Các nút duyệt và chọn hàng dựa trên quyết định BE. Sau khi xử lý, mục rời queue; lịch sử vẫn xem ở Content/Posts/Calendar theo scope.

## Lỗi và cache

- 403 giữ phiên đăng nhập, ẩn dữ liệu dashboard cũ và đưa ra nút kiểm tra lại quyền/chọn workspace khác. 404 giải thích tài nguyên không tồn tại hoặc không còn quyền. 409 yêu cầu tải lại quyền; không tự gửi lại grant cũ.
- Đổi workspace hoặc grant thành công sẽ remount cả trang và provider subscription, xóa cache workspace/profile. Sự kiện storage xử lý đổi workspace từ tab khác. Bỏ cache tên Brand không gắn ngữ cảnh.
- API không cache dữ liệu workspace. Phản hồi được kiểm tra ngữ cảnh sau fetch và sau đọc body; bỏ phản hồi của workspace trước. Refresh token không được gửi lại mutation của workspace A sang workspace B.
- Không còn Promise chờ vô hạn trong nhánh chuyển tới đăng nhập. Workspace chưa tải được không được suy thành Owner; membership không còn trong danh sách sẽ xóa workspace/profile đang chọn.
- Bỏ lưu tự động lúc unmount trang chi tiết: rời trang/đổi workspace không được phát sinh thao tác ghi bản nháp trong ngữ cảnh mới. Người dùng lưu bằng nút Save.

## Bằng chứng kiểm tra

- Web: **81/81 test, 25 file đạt**, bao gồm kiểm thử giao diện grant/conflict, ẩn kênh không được publish, reset dữ liệu sau đổi workspace/403, permission response đến muộn, pagination giữ filter và retry không đổi workspace.
- TypeScript và build production Next.js đạt; route Brand Access được sinh trong build.
- Backend: **43/43 test tập trung permission/access đạt**. Toàn bộ backend **469/472 đạt**; ba lỗi PromptEnhancerTests đã có trước task, không thuộc permission UX. Test bổ sung xác nhận actor/workspace từ server và giới hạn batch; test query kiểm tra My content và ViewAll không tự mở review queue.
- Không chạy giao dịch thanh toán, OAuth hoặc publish thật. Kiểm thử E2E với tài khoản/staging và provider thực thuộc T11.

## Điều kiện sử dụng

Chạy FE cùng bản BE có các endpoint T03/T04. Database nguồn vẫn chưa được áp dụng migration trong phiên này; cần các migration T01/T03 trước khi chạy API mới. T04 không thêm migration. Không tự tạo membership hoặc cấp grant cho dữ liệu cũ chưa được xác minh.

Tiếp theo: **T05 — Member Performance API và dashboard**. Composer/media, scheduler và nghiệm thu tích hợp vẫn theo các task riêng trong kế hoạch.
