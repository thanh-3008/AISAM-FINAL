# T11 — Hoàn thành phát triển

Ngày chốt: 10/09/2026.

Người dùng yêu cầu hoàn thiện các task phát triển trước, sau đó kiểm thử tổng thể một đợt. Theo quyết định này, T11 được đóng ở mức **hoàn thành phát triển và kiểm chứng cục bộ**. Các tiêu chí môi trường thực và hồ sơ release chuyển sang đợt nghiệm thu cuối sau T12; không bị xóa hoặc coi là đã đạt.

## Kết quả bàn giao

- Backend 516/516 test đạt; sửa regression PromptEnhancer, bổ sung kiểm tra giới hạn upload và xử lý đủ payload 200 MiB tại service với storage giả lập.
- Web E2E 21/21 đạt với API giả lập; fixture permissions, route và video được cập nhật.
- Harness PostgreSQL restore đối soát bảy bảng gồm Content/Post/Asset, migration/rollback/reapply, query scope, snapshot, publish journal và cạnh tranh worker.
- Migration bổ sung sửa lỗi trigger khi INSERT Brand/Team, đã kiểm tra trên restore cô lập.
- Seed 10.000 Content, hai Brand/Team và đủ năm actor; phân trang và scope Creator đạt.
- Đo tải 200 lượt list và 100 lượt analytics/20 worker với 200 Post/400 insights tổng hợp; đối soát báo cáo mới nhất và chỉ số đúng theo Creator.
- Đã lập ma trận truy vết, phân nhóm vấn đề legacy và runbook nghiệm thu.

Bằng chứng và giới hạn từng phép đo: [T11_VALIDATION.md](T11_VALIDATION.md). Bản sửa trigger: [T11_TRIGGER_FIX.md](T11_TRIGGER_FIX.md).

## Chuyển sang đợt nghiệm thu cuối

- Ma trận permission/publishing và regression xuyên suốt BE/Web/Mobile trên bản code cuối.
- Upload qua HTTP/CDN thật, hiệu năng HTTP/index và kiểm tra security trên môi trường triển khai.
- Migration/backup/rollback staging; quyết định xử lý 542 Content, 36 automation thiếu creator và một Post lệch Brand/kênh.
- Google/social/checkout sandbox và demo thực tế.
- Swagger/ERD đối chiếu bản release, commit phát hành, release notes và bằng chứng nghiệm thu.

Checklist vẫn mở tại [kế hoạch](../KE_HOACH_TASK_PERMISSION_PUBLISHING.md) và [release gates](T11_RELEASE_GATES.md). Chưa áp dụng migration lên database nguồn; chưa đăng bài/thanh toán thật; chưa chứng nhận production. T12 có thể bắt đầu ngay, không cần đợi staging.
