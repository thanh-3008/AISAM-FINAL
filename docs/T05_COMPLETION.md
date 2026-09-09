# T05 — Member Performance API và dashboard

**Hoàn thành — 08/09/2026.** API `GET /api/team/member-performance`; web `/team/performance`, có liên kết từ Team.

## Quyền và bộ lọc

Actor lấy từ JWT, workspace từ middleware. Service tự xác minh membership hoạt động, user hoạt động, workspace chưa Deleted và vai trò hợp lệ. Owner xem các thành viên đang hoạt động; Manager chỉ dữ liệu thuộc các cặp member/Brand được resolver AnalyticsMember cho phép; Creator chỉ bản thân trong Brand còn được cấp; Viewer bị từ chối.

Mỗi cặp member/Brand được kiểm tra riêng: cùng Team ở Alpha không mở dữ liệu người đó ở Beta. `brandId`, `teamId`, `memberId` ngoài phạm vi trả 404; không âm thầm chuyển sang toàn workspace. Thu hồi TeamBrand có hiệu lực ở request tiếp theo. Bộ lọc và tenant predicate áp trước đếm/tổng hợp. Service dùng query riêng để self-performance không bị filter analytics dành riêng Manager chặn; mọi query bỏ global filter đều có scope tường minh.

Filter Team dùng **TeamId ghi trên Content**, không suy Team lịch sử từ membership hiện tại. Thành viên hiển thị là membership đang hoạt động; đây không phải báo cáo nhân sự đã rời workspace. Tên hiển thị không kèm email hoặc dữ liệu tài khoản riêng. Danh sách tùy chọn chỉ có phạm vi được phép.

## Công thức

Khoảng thời gian UTC `[from,to)`, tối đa 366 ngày. FE chọn ngày UTC; ngày kết thúc không tính. API nhận DateTimeOffset và chuyển về UTC. Phân trang thành viên 1–100 mục/trang; thứ tự tên rồi ID.

| KPI | Quy tắc |
|---|---|
| Content created | PrimaryCreatorId và CreatedAt trong kỳ; không đếm lại theo platform |
| Creator published posts | Bài thành công có PublishedAt trong kỳ, nội dung do thành viên tạo |
| Publisher published posts | Bài thành công có PublishedByUserId là thành viên; tách khỏi Creator |
| Approval rate | Quyết định Approved / tổng Approved + Rejected của các submission có timestamp, quyết định trong kỳ; nhóm theo ContentId + SubmittedAt |
| Turnaround | Trung bình giờ từ SubmittedAt đến thời điểm quyết định; chỉ timestamp hợp lệ |
| On-time rate | Lịch đơn Completed có ExecutedAt trong ±5 phút so với ScheduledAt / lịch đơn Completed có đủ timestamp; chọn kỳ theo ScheduledAt |
| Failed publish rate | Lịch đơn Failed / (Completed + Failed) theo kết quả cuối; retry không tăng mẫu số |
| Engagement / impressions / reach | Snapshot cumulative mới nhất của mỗi đích đăng cho các bài xuất bản trong kỳ; không cộng nhiều snapshot ngày |
| Engagement rate | 100 × tổng engagement / tổng impressions của các bài có insights; reach báo riêng |

Bài đăng được khử trùng theo IntegrationId + ExternalPostId; khi thiếu ID ngoài dùng Post.Id và không đoán hai bài là một. Insights của các Post trùng cùng đích được chọn chung theo ReportDate, CreatedAt và ID xác định. Bản ghi chỉ có trackedClicks không phải snapshot insights. Trường metric provider thiếu/null không được biến thành 0; dữ liệu legacy thiếu raw JSON giữ giá trị cột đã lưu.

Không có mẫu số, insights hoặc timestamp trả null và UI ghi “Chưa đủ dữ liệu”. Engagement đang dùng ngữ nghĩa provider lưu trong hệ thống (engaged users hoặc tổng reactions/comments/shares), không khẳng định tất cả provider có cùng định nghĩa. Reach là tổng reach từng bài, không phải người duy nhất toàn workspace. Insights lấy snapshot mới nhất tại thời điểm truy vấn của cohort bài xuất bản trong kỳ, không phải tương tác phát sinh riêng trong kỳ. Thời điểm đồng bộ hiển thị riêng từng thành viên.

Owner thấy số Content chưa có PrimaryCreatorId; không đưa các nội dung này vào thành tích cá nhân. Không gán Publisher từ Creator hoặc chủ Profile khi attribution trống.

## Bổ sung dữ liệu duyệt

Migration `20260908142531_AddApprovalSubmissionTimestamp` thêm `approvals.submitted_at` nullable. Submit tạo bản ghi Pending có mốc gửi; Approve/Reject ghi cùng mốc vào quyết định. Dữ liệu cũ không backfill bằng CreatedAt của Content. Quyết định legacy chưa xác định submission bị loại khỏi approval rate và turnaround, tránh độ chính xác giả. Migration có Down xóa cột; rollback sẽ mất timestamp mới.

## Giới hạn đã chốt

- Lịch lặp bị ghi đè occurrence trong model hiện tại, nên loại khỏi on-time và failed publish rate. T08 xử lý lịch sử occurrence/retry; không suy kết quả các lần chạy cũ.
- Lỗi đăng ngay không được lưu đầy đủ thành attempt bền vững. Vì vậy failed publish rate hiện là tỷ lệ lỗi lịch đơn cuối cùng, được ghi rõ ở API/UI, không phải tỷ lệ mọi lần gọi provider.
- Report đọc theo quyền hiện tại; member đã rời workspace không nằm trong danh sách. Các metric liên quan lịch sử không có timestamp sẽ thiếu dữ liệu cho tới khi có hoạt động mới.
- Giao diện có bảng KPI, bảng insights, biểu đồ thanh bằng meter có nhãn truy cập, bộ lọc và phân trang; có trạng thái trống/lỗi và loại phản hồi cũ khi filter thay đổi.
- Không chạy đồng bộ provider hoặc tác vụ publish thật trong nghiệm thu; E2E staging thuộc T11. Database nguồn chưa migrate.

## Kiểm chứng

- Fixture tính tay: approval 50%, turnaround 3 giờ, on-time 50%, failed schedule 33,33%, engagement 30 / impressions 200 = 15%, reach 120. Kiểm tra snapshot cũ, tracked-click-only, Post retry trùng, boundary `to`, lịch lặp, legacy attribution và thiếu mẫu số.
- Test scope: Creator self, Manager không thấy Brand khác, Viewer 403, member/team ngoài phạm vi 404, thu hồi assignment mất quyền.
- PostgreSQL: backup restore riêng, EF migrator áp dụng thành công; truy vấn Member Performance chạy trên bản sao dữ liệu thật. Database nguồn chỉ đọc, không áp dụng migration.
- Web: 83/83 test, 26 file đạt; TypeScript và production build đạt, có route `/team/performance`.
- Backend đầy đủ: 470/473 đạt; ba lỗi PromptEnhancerTests hiện hữu trước task. Kiểm thử tập trung Member Performance/ContentService và phân quyền: **62/62 đạt** sau chỉnh sửa cuối.

Task tiếp theo: T06 — Media collection, upload và snapshot.
