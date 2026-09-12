# Quyết định triển khai Permission & Publishing

Ngày 07/09/2026. Người dùng đã ủy quyền quyết định nghiệp vụ trong phiên làm việc. Các quyết định dưới đây thay thế trạng thái chờ xác nhận trong T00; không cần hỏi lại về cùng policy.

| ID | Quyết định |
|---|---|
| D01 | Owner có toàn bộ Brand trong workspace có membership hoạt động; không vượt tenant. Workspace Deleted bị chặn. |
| D02 | Manager chỉ quản lý Brand/Team được gán, không tự mở rộng phạm vi bằng assignment. |
| D03 | Creator xem lịch sử của mình trong Brand được cấp. content.view_all_creators chỉ thêm quyền đọc, không cấp sửa/xóa/publish nội dung người khác. |
| D04 | Approver là permission, không thêm workspace role. Queue review là quyền riêng; Viewer chỉ xem Brand/kênh được cấp, không xem lịch sử Creator hoặc KPI thành viên. |
| D05 | Chưa thêm direct user override. Quyền bổ sung của Creator phải do người quản lý có quyền cấp trong Team/Brand hợp lệ, không tin permissions JSON cũ chưa kiểm chứng. |
| D06 | SocialIntegration là resource kênh. TeamSocialChannel dùng IntegrationId. |
| D07 | Thu hồi quyền dừng job chưa publish và thông báo. Manager được quyền có thể tiếp quản chủ động; phải duyệt/lên lịch lại với actor và snapshot mới. Không rollback bài đã đăng lên mạng xã hội tự động. |
| D08 | Worker lưu và kiểm tra người schedule/automation owner; kiểm tra lại cả entitlement, channel và workspace trước thao tác ngoài hệ thống. |
| D09 | Capability không hỗ trợ thì chặn và giải thích; không tự tách thành nhiều bài hoặc bỏ media. |
| D10 | Snapshot bất biến gồm text/media/formatter version; sửa draft không đổi lịch cũ. Recurring có idempotency theo occurrence+integration. |
| D11 | Thời gian lưu UTC, filter theo khoảng [from,to). On-time tolerance 5 phút cấu hình. Engagement rate dùng impressions khi platform có dữ liệu; reach được biểu diễn metric riêng, không trộn mẫu số. Không có mẫu số trả null thay vì 0 giả. |
| D12 | Resource ngoài tenant/không được nhìn trả 404; action không được phép trên resource đã được nhìn trả 403. Query filter ngoài scope không được trả dữ liệu. |
| D13 | Job NeedsAttention cho quyền/OAuth cần xử lý; lỗi payload terminal Failed; kết quả từng integration độc lập. Post hiện tại vẫn là bản ghi publish, không ép PublishedAt giả cho job queued. |
| D14 | Giai đoạn đầu không cache quyền. Khi cần tối ưu phải bổ sung invalidation và thời hạn hiệu lực có test trước khi bật. |

## Chính sách bổ sung

- Creator được self-performance trong Brand còn được cấp; không lấy số liệu aggregate của người khác.
- Owner billing vẫn được phép trong Limited/Archived/EligibleForDeletion; các thao tác ghi nội dung bị chặn theo lifecycle.
- Attribution cũ không xác định được giữ null và báo cáo. Không dùng Profile.UserId để cấp quyền Creator giả.
- Không đánh dấu exactly-once chỉ nhờ khóa nội bộ: timeout sau khi provider nhận bài cần reconcile; không retry mù khi kết quả chưa xác định.
- Capability thực tế từng provider chỉ được bật sau kiểm chứng API và tài khoản; mặc định không quảng bá tính năng chưa xác nhận.

## Trạng thái triển khai

Đã có policy thuần `AISAM.Services/Access/ResourcePermissionPolicy.cs` và kiểm thử ma trận. Policy nhận facts tin cậy từ backend; không dùng làm DTO nhận từ client.

Chưa nối policy vào request pipeline. Cần schema/attribution, resolver từ database, scope query và API assignment trước khi bật runtime. T01–T03 chưa hoàn thành; không coi policy thuần là hệ thống phân quyền đã triển khai đầy đủ.
